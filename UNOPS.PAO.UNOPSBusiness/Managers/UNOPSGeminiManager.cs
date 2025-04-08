using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.DataAccess.Context;
using AutoMapper;
using UNOPS.PAO.Business.Repositories.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Dynamic;
using Humanizer;
using System.Net.Http;
using System.Net.Http.Headers;
using Google.Cloud.Vision.V1;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Google.Cloud.TextToSpeech.V1;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Globalization;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Reflection.Metadata.Ecma335;
using Google.Cloud.AIPlatform.V1;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Services;
using Z.EntityFramework.Plus;
using System.Text.Json;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSGeminiManager : IGeminiManager
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly GoogleCredential _credentials;
    private readonly DataRepository<AiPrompt> _promptRepository;
    private readonly UNOPSAppDbContext _context;
    private readonly string _connectionString;
    private readonly GoogleTextToSpeechService _ttsService;
    private readonly TextExtractionService _textExtractionService;
    private readonly GoogleCloudStorageService _gcsService;
    private readonly GeminiSessionService _sessionService;
    private readonly AiContextualService _aiService;

    public UNOPSGeminiManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        _mapper = mapper;
        _context = context;
        _promptRepository = new DataRepository<AiPrompt>(context);
        _configuration = configuration;
        _credentials = GetCredentials()
                        .CreateScoped("https://www.googleapis.com/auth/spreadsheets.readonly");//.CreateWithUser("anushas@unops.org");;
        _connectionString = configuration.GetValue<string>("ConnectionStrings:DbSchema");
        _textExtractionService = new TextExtractionService();
        _gcsService = new GoogleCloudStorageService(configuration);
        _sessionService = new GeminiSessionService(context);
        _ttsService = new GoogleTextToSpeechService();
        _aiService = new AiContextualService(configuration, _context, _credentials);
    }

    // Map AiPrompt entity to AiPromptModel
    private static AiPromptModel MapEntityToAiPromptModel(AiPrompt entity, IMapper mapper)
    {
        var result = mapper.Map<AiPrompt, AiPromptModel>(entity);
        return result;
    }

    // Map AiPromptModel to AiPrompt entity
    private AiPrompt MapModelToEntity(AiPromptModel model)
    {
        var entity = _mapper.Map(model, new AiPrompt());
        return entity;
    }

    // Get prompt data by type
    public IEnumerable<AiPromptModel> GetPromptData(string type)
    {
        return _promptRepository
            .GetAll()
            .Where(x => x.Type == type)
            .Select(x => MapEntityToAiPromptModel(x, _mapper));
    }

    // Fetch detailed response from Gemini
    public async Task<dynamic> FetchDetailedResponseFromGemini(AiChatSession session, IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request, string promptType
                                                                , string fileUrl, string fileType) {
        var geminiResponse = await ChatWithGemini(session, request, promptType, formattedChatHistory, fileUrl, fileType);
        return geminiResponse;
    }

    // Entity detection through Gemini
    public async Task<dynamic> EntityDetectionThroughGemini(AiChatSession session, IEnumerable<dynamic> formattedChatHistory, GeminiAssistantRequest request
                                                                , string fileUrl, string fileType) {
        var geminiResponse = await ChatWithGemini(session, request, "entity_intent_detection", formattedChatHistory, fileUrl, fileType);
        return geminiResponse;
    }
    
    // Get details from Gemini response
    public JObject GetDetailsFromGeminiResponse(string modelResponse) {
        JObject json = JObject.Parse(modelResponse);
        var candidates = json["candidates"];
        var parts = candidates[0]?["content"]["parts"];
        var textJson = parts[0]["text"].ToString(); ;
        textJson = textJson.Replace("```json", "").Replace("```", "").Trim();
        var entityResponse = new JObject();

        try
        {
            entityResponse = JObject.Parse(textJson); // Try parsing as JSON
        }
        catch (JsonReaderException)
        {
            entityResponse = new JObject { { "Message", textJson } }; // Wrap in JSON
        }

        return entityResponse;
    }

    // Chat with Gemini
    private async Task<dynamic> ChatWithGemini(AiChatSession session, GeminiAssistantRequest req, string promptType, IEnumerable<dynamic> formattedChatHistory, string fileUrl, string fileType)
    {
        var chatHistoryList = formattedChatHistory?.ToList() ?? new List<dynamic>();
        Guid sessionId = req.sessionId;
        string message = req.Message;
        string extractedText = req.ExtractedText ?? "";
        string finalPrompt = (string.IsNullOrEmpty(extractedText) ? message : extractedText);
        var promptData = GetPromptData(promptType).FirstOrDefault();

        if (promptData == null)
        {
            promptType = "general_information";
            promptData = GetPromptData(promptType).FirstOrDefault();
        }

        if (chatHistoryList.Count == 0)
        {
            string promptTemplate = promptData.Prompt;
            finalPrompt = promptTemplate.Replace("{promptData}", message);
        }

        chatHistoryList.Add(new
        {
            role = "user",
            parts = new[] { new { text = finalPrompt } }
        });

        string response = await CallGeminiApi(chatHistoryList, promptData);
        var parsedResponse = GetDetailsFromGeminiResponse(response);
        var entity = parsedResponse["Entity"]?.ToString() ?? parsedResponse["Category"]?.ToString();
        var intent = parsedResponse["Intent"]?.ToString() ?? parsedResponse["ResponseType"]?.ToString();
        var forward = parsedResponse["Forward"]?.ToString() ?? "No";

        if (intent == "Action" && forward == "No" && promptType == "entity_intent_detection")
        {
            intent = "Information";
        }

        string responseInString = JsonConvert.SerializeObject(parsedResponse);
        _sessionService.UpdateChatHistoryTable(sessionId, "user", message, finalPrompt, entity, intent, promptType, fileUrl, fileType);
        message = parsedResponse["Message"]?.ToString();

        if (session.TextToSpeech == true)
        {
            byte[] audioBytes = await _ttsService.ConvertTextToAudio(message);
            fileUrl = await _gcsService.UploadAudioToGCS(audioBytes);
            fileType = "audio";
        }
        else
        {
            fileUrl = null;
            fileType = null;
        }

        if (parsedResponse["Forward"]?.ToString() == "No")
        {
            _sessionService.UpdateChatHistoryTable(sessionId, "model", message, responseInString, entity, intent, promptType, fileUrl, fileType);
        }

        var finalResponse = new
        {
            Entity = entity,
            Intent = intent,
            Message = parsedResponse["Message"]?.ToString() ?? "",
            Type = parsedResponse["Type"]?.ToString() ?? "",
            Summary = parsedResponse["Summary"]?.ToString() ?? "",
            Forward = parsedResponse["Forward"]?.ToString() ?? "No",
            RawMessage = responseInString,
            MediaUrl = fileUrl,
            MediaType = fileType,
            ShortSummary = parsedResponse["ShortSummary"]?.ToString() ?? "",
            Dependents = parsedResponse["dependents"]?.ToString() ?? "",
            Url = parsedResponse["URL"]?.ToString() ?? "",
            Files = new[] { new { MediaUrl = fileUrl, MediaType = fileType } }
        };

        return finalResponse;
    }

    // Common function to handle Gemini API calls
    private async Task<string> CallGeminiApi(dynamic prompt, AiPromptModel promptData)
    {
        string accessToken = await GetAccessTokenAsync();
        var requestBody = await GetRequestBody(prompt, promptData);
        string url = await GetURL(promptData);
        string jsonRequest = JsonConvert.SerializeObject(requestBody);
        return await CallGeminiApiAsync(url, jsonRequest, accessToken);
    }

    // Updated FetchResultFromGemini to use CallGeminiApi
    public async Task<string> FetchResultFromGemini(AiPromptModel promptData, string relatedJsonData)
    {
        string promptTemplate = promptData.Prompt;
        string finalPrompt = promptTemplate.Replace("{promptData}", relatedJsonData);
        var promptList = new
        {
            role = "user",
            parts = new[] { new { text = finalPrompt } }
        };
        return await CallGeminiApi(promptList, promptData);
    }

    // Updated callGemini to use CallGeminiApi
    public async Task<string> callGemini(string prompt, AiPromptModel promptData)
    {
        var promptList = new
        {
            role = "user",
            parts = new[] { new { text = prompt } }
        };
        return await CallGeminiApi(promptList, promptData);
    }

    // Get request body for Gemini API
    public async Task<dynamic> GetRequestBody(dynamic prompt, AiPromptModel promptData)
    {
        dynamic contentConfig = JsonConvert.DeserializeObject<ExpandoObject>(promptData.ContentConfig);
        dynamic generationConfig = JsonConvert.DeserializeObject<ExpandoObject>(promptData.GenerationConfig);
        dynamic toolsConfig = string.IsNullOrEmpty(promptData.ToolsConfig)
                        ? new List<ExpandoObject>() : JsonConvert.DeserializeObject<List<ExpandoObject>>(promptData.ToolsConfig);
        dynamic safetySettings = string.IsNullOrEmpty(promptData.SafetySettings)
                        ? new List<ExpandoObject>() : JsonConvert.DeserializeObject<List<ExpandoObject>>(promptData.SafetySettings);

        if (prompt is string)
        {
            contentConfig.parts[0].text = prompt.ToString();
        } else
        {
            contentConfig = prompt;
        }

            var requestBody = new
            {
                contents = contentConfig,
                generationConfig = generationConfig,
                tools = new[] { toolsConfig },
                safetySettings = new[] { safetySettings }
            };

        return requestBody;
    }

    // Get URL for Gemini API
    private async Task<string> GetURL(AiPromptModel promptData)
    {
        return $"https://{promptData.Location}-aiplatform.googleapis.com/v1/projects/{promptData.Project}/locations/{promptData.Location}/publishers/google/models/{promptData.Model}:generateContent";
    } 

    // Map GeminiProcessDataRequest to AiPrompt entity
    private AiPrompt MapModelToEntity(GeminiProcessDataRequest model)
    {
        var entity = _mapper.Map<AiPrompt>(model);
        return entity;
    }

    AiPrompt IGeminiManager.MapModelToEntity(GeminiProcessDataRequest req)
    {
        return MapModelToEntity(req);
    }

    // Get access token for Gemini API
    private static async Task<string> GetAccessTokenAsync()
    {
        GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }

    // Call Gemini API with the request
    private static async Task<string> CallGeminiApiAsync(string url, string jsonRequest, string accessToken, int maxRetries = 5)
    {
        HttpResponseMessage response = new HttpResponseMessage();
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    //retry the prompt after a delay incase of an error response
                    TimeSpan waitTime = TimeSpan.FromSeconds(Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(new Random().Next(0, 1000));  //jitter up to 1 second.
                    Console.WriteLine($"Rate limit exceeded. Retrying in {waitTime.TotalSeconds:F2} seconds (Attempt {attempt + 1}/{maxRetries})");
                    await Task.Delay(waitTime);
                }
            }
        }
        //respond with the most recent error after max retries are reached
        return await response.Content.ReadAsStringAsync();
    }

    // Get Google credentials from configuration
    private GoogleCredential GetCredentials()
    {
        var credentialParams = _configuration.GetSection("AISettings")
            .Get<JsonCredentialParameters>();
        if (credentialParams == null)
            throw new Exception("AISettings configuration is missing.");
    
        var secretName = _configuration.GetValue<string>("AISettings:AIServiceAccountJSONSecretName");
        
        var basicProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId);
        var secretValue = basicProvider.GetSecretVersion(secretName, "latest");
        return GoogleCredential.FromJson(secretValue);
    }

    public async Task<string> ProcessDataRelatedSummaryDetails(GeminiProcessDataRequest req)
    {
        string relatedMessage = "";

        AiPrompt promptModel = MapModelToEntity(req);

        // Call the GetPromptData method and get the first prompt
        var promptData = GetPromptData(promptModel.Type).FirstOrDefault();

        if (promptData == null)
        {
            return "";
        }

        // Query the AiScreenMapping table based on Type
        var screenMappings = (await _aiService.GetScreenMappingsByType(promptData.Type)).ToArray();
        relatedMessage = await _aiService.GetDataBasedOnScreenMapping(promptData.Type, req.Id, screenMappings);

        // Fetch result from Gemini
        return await FetchResultFromGemini(promptData, relatedMessage);
    }

    public async Task<string> ScanFileForGeminiProcessing(GeminiFileRequest req)
    {
        string extractedText = await ExtractDataFromFile(req.File);
        string type = req?.Type;

        if (!string.IsNullOrEmpty(type)) {
            var promptData = GetPromptData(type).FirstOrDefault();

            if (promptData == null)
            {
                return "";
            }

            // Fetch result from Gemini
            return await FetchResultFromGemini(promptData, extractedText);
        }

        return extractedText;
    }

    public async Task<dynamic> ProcessChatWithGemini(GeminiAssistantRequest req, int currentUserId)
    {
        string extractedText = "";
        string fileUrl = "";
        string fileType = "";

        if (string.IsNullOrEmpty(req?.Message)) {
            req.Message = "";
        }

        // If any other session is active, mark it as inactive and activate this session (if required)
        var session = await UpdateCurrentSessionIfInactive(currentUserId, req.sessionId);

        if (req.File != null) {
            fileType = FindFileType(req.File);
            extractedText = await ExtractDataFromFile(req.File);
            fileUrl = await UploadFileToGCS(req.File);

        }

        var chatHistory = await GetChatHistory(req.sessionId, "entity_intent_detection");

        if (!string.IsNullOrEmpty(extractedText)) 
        {
            if (!string.IsNullOrEmpty(req.Message))
            {
                req.ExtractedText = req.Message + "\\n";
            }
            req.ExtractedText = req.ExtractedText + extractedText + ".\\n"; 
        }

        var formattedChatHistory = chatHistory.Select(x => new {
            role = x.Sender,
            parts = new[] { new { text = x.RawMessage } }
        }).ToList();

       // var dataResult = await _aiService.GetAllEntityDataAsync("Contacts");

        // Entity detection and intent classification to be done
        var entityResponse = await EntityDetectionThroughGemini(session, formattedChatHistory, req, fileUrl, fileType);
        //var entityResponse = GetDetailsFromGeminiResponse(entityDetectionResponse);
        var forward = entityResponse.Forward.ToString();
       // await _aiService.GenerateEmbeddingAsync("Contact", 1, "Name: Anusha Swaminathan, Country: Denmark, Address: Else Alfelts Vej 52N, 1.tv, PartnerName: UNOPS Partner A, Status: Active");
        //await _aiService.GenerateEmbeddingAsync("Contact", 2, "Name: Raghavendar Murali, Country: Denmark, Address: Else Alfelts Vej 52N, 1.tv, PartnerName: UNOPS Partner X, Status: Active");
        if (forward == string.Empty || forward == "No") {
            return entityResponse;
        }

        var promptType = entityResponse.Type.ToString();
        var summary = entityResponse.Summary.ToString();
        var shortSummary = entityResponse.ShortSummary.ToString();

        var content = "";

        chatHistory = await GetChatHistory(req.sessionId, promptType);

        if (forward == "Yes" && promptType.StartsWith("retrieve"))
        {
            var embeddingString = await _aiService.CreateEmbeddingForText(shortSummary);
            var entityId = await _aiService.RetrieveEntityId(entityResponse.Entity.ToString(), embeddingString);
            content = await _aiService.RetrieveContent(promptType, entityId);
            req.Message = "Summary of the conversation with the user: " + summary + ". Content: " + content;
        } else {
            req.Message = "Summary: " + summary;
        }

        formattedChatHistory = chatHistory.Select(x => new {
            role = x.Sender,
            parts = new[] { new { text = x.RawMessage } }
        }).ToList();

        var detailedResponse = await FetchDetailedResponseFromGemini(session, formattedChatHistory, req, promptType, fileUrl, fileType);
        var updatedMessage = await GetDependentDropdownValues(detailedResponse?.Dependents, JsonConvert.DeserializeObject(detailedResponse.RawMessage));
        var updatedDetailedResponse = new
        {
            detailedResponse.Entity,
            detailedResponse.Intent,
            detailedResponse.Message,
            detailedResponse.Type,
            detailedResponse.Summary,
            detailedResponse.Forward,
            RawMessage = JsonConvert.SerializeObject(updatedMessage),
            detailedResponse.MediaUrl,
            detailedResponse.MediaType,
            detailedResponse.ShortSummary,
            detailedResponse.Dependents,
            detailedResponse.Url,
            detailedResponse.Files
        };

        _sessionService.UpdateChatHistoryTable(req.sessionId, "model", updatedDetailedResponse.Message.ToString(), updatedDetailedResponse.RawMessage
                                        , updatedDetailedResponse.Entity, updatedDetailedResponse.Intent, "entity_intent_detection", updatedDetailedResponse.MediaUrl, updatedDetailedResponse.MediaType);
        
        return updatedDetailedResponse;

    }

    private async Task<dynamic> GetDependentDropdownValues(dynamic dependents, dynamic responseObject)
    {
        if (!string.IsNullOrWhiteSpace(dependents))
        {
            var dependentsList = JsonConvert.DeserializeObject<List<string>>(dependents);

            if (dependentsList.Count > 0)
            {
                //var detailedRawMessage = JsonConvert.DeserializeObject(detailedResponse.RawMessage);
                foreach (var dependent in dependentsList)
                {
                    var text = responseObject[dependent];
                    if (text != null)
                    { 
                        dynamic entityId;
                        int id;
                        if (text != null)
                        {
                            if (text?.Value != null)
                            {
                                text = text.Value;
                            }
                            if (int.TryParse(text, out id))
                            {
                                continue;
                            }
                        }
                        var embeddingString = await _aiService.CreateEmbeddingForText(text);
                        string entityName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dependent.Replace("Id", ""));
                        entityName = entityName.Pluralize();
                        entityId = await _aiService.RetrieveEntityId(entityName, embeddingString);

                        if (entityId == null || entityId is DBNull)
                        {
                            continue;
                        }
                        else
                        {
                            responseObject[dependent] = entityId;
                        }
                    }
                }
            }

        }

        return responseObject;
    }

    public IEnumerable<AiChatSession> GetSessionDataWithChats(Guid sessionId, int userId) 
    {
        return _sessionService.GetSessionDataWithChats(sessionId, userId);
    }

    public async Task<IEnumerable<AiChatSession>> GetSessionData(Guid sessionId, int userId) 
    {
        return await _sessionService.GetSessionData(sessionId, userId);
    }

    public IEnumerable<AiChatSession> GetUserSessions(int userId) 
    {
        return _sessionService.GetUserSessions(userId);
    }

    public Guid CreateNewSession(int userId) 
    {
        return _sessionService.CreateNewSession(userId);
    }

    public bool EndSession(Guid sessionId) 
    {
        return _sessionService.EndSession(sessionId);
    }

    public async Task<IEnumerable<AiChatHistory>> GetChatHistory(Guid sessionId, string type) 
    {
        return await _sessionService.GetChatHistory(sessionId, type);
    }

    public async Task<AiChatSession> UpdateCurrentSessionIfInactive(int userId, Guid sessionId)
    {
        return await _sessionService.UpdateCurrentSessionIfInactive(userId, sessionId);
    }

    public async Task<string> ExtractDataFromFile(IFormFile file) {
        return await _textExtractionService.ExtractDataFromFile(file);
    }

    public string FindFileType(IFormFile file) 
    {
        return _textExtractionService.FindFileType(file);
    }

    // Overload for IFormFile
    public async Task<string> UploadFileToGCS(IFormFile file)
    {
        return await _gcsService.UploadFileToGCS(file);
    }

    public async Task<bool> UpdateAiAssistantAccessibility(GeminiAccessibilityRequest req)
    {
        return await _sessionService.UpdateAiAssistantAccessibility(req);
    }

    public async Task<dynamic> ExtractDataAfterAnalysis(AnalyseFileRequest req)
    {
        var promptData = GetPromptData(req.Type).FirstOrDefault();
        if (promptData == null)
        {
            return null;
        }

        var fileData = await _aiService.ReadFileData(req.FileId);
        var fileDataArray = JArray.Parse(fileData);
        var headerRow = fileDataArray[0];
        // Define batch size
        int batchSize = 25;
        string entity = "";
        string intent = "";
        string message = "";
        var finalResponse = new List<dynamic>();

        // TODO

        for (int i = 0; i < fileDataArray.Count; i += batchSize)
        {
            var batch = new JArray
            {
                headerRow
            };
            for (int j = i; j < i + batchSize && j < fileDataArray.Count; j++)
            {
                batch.Add(fileDataArray[j]);
            }
            // Convert the batch to a string
            string batchJson = batch.ToString(Newtonsoft.Json.Formatting.None);

            var finalPrompt = promptData.Prompt.Replace("{promptData}", batchJson);
            var promptList = new
            {
                role = "user",
                parts = new[] { new { text = finalPrompt } }
            };
            var response = await CallGeminiApi(promptList, promptData);
            var parsedResponse = GetDetailsFromGeminiResponse(response);
            message = parsedResponse["Message"]?.ToString();
            var records = parsedResponse["records"];
            entity = parsedResponse["Category"]?.ToString();
            intent = parsedResponse["ResponseType"]?.ToString();
            if (records != null)
            {
                foreach (var record in records)
                {
                    var dependents = record["dependents"]?.ToString();
                    dynamic updatedResponse = await GetDependentDropdownValues(dependents, record);
                    finalResponse.Add(updatedResponse);
                }
            }
        }

        var finalResponseString = JsonConvert.SerializeObject(finalResponse);

        return new
        {
            Message = message,
            Entity = entity,
            Intent = intent,
            Records = finalResponseString
        };
    }

    private static string ToConcatenatedString(object model)
    {
        if (model == null) return string.Empty;

        var properties = model.GetType().GetProperties();
        string result = "";

        foreach (var property in properties)
        {
            var value = property.GetValue(model, null);

            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                // Handle collections by concatenating their elements
                foreach (var item in enumerable)
                {
                    result += ToConcatenatedString(item) + ", ";
                }
            }
            else if (value != null && !value.GetType().IsValueType && value.GetType() != typeof(string))
            {
                // Handle nested objects by recursively concatenating their properties
                var nestedProperties = value.GetType().GetProperties();
                foreach (var nestedProperty in nestedProperties)
                {
                    var nestedValue = nestedProperty.GetValue(value, null);
                    result += $"{property.Name}.{nestedProperty.Name}: {nestedValue}, ";
                }
            }
            else
            {
                result += $"{property.Name}: {value}, ";
            }
        }

        // Remove trailing comma and space
        return result.TrimEnd(',', ' ');
    }

    public async Task<dynamic> GenerateEmbeddings(string entityName)
    {
        var tableNames = _context.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.Name)
            .Where(name => name != "EntityEmbeddings" && name != "AiChatSession")
            .ToArray();

        if (entityName != null)
        {
            tableNames = new[] { entityName };
        }

        var result = new List<MyPubSubMessage>();
        var pubSubPublisher = new PubSubPublisher(_configuration);

        foreach (var tableName in tableNames)
        {
            var dbSetProperty = _context.GetType()
                .GetProperty(tableName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (dbSetProperty == null)
            {
                Console.WriteLine($"DbSet for table '{tableName}' not found.");
                continue;
            }

            var dbSet = dbSetProperty.GetValue(_context) as IQueryable<object>;
            if (dbSet == null)
            {
                Console.WriteLine($"Unable to retrieve DbSet for table '{tableName}'.");
                continue;
            }

            // Dynamically include all navigation properties
            var navigationProperties = dbSetProperty.PropertyType
                .GenericTypeArguments[0]
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(IEnumerable<object>).IsAssignableFrom(p.PropertyType) || !p.PropertyType.IsValueType && p.PropertyType != typeof(string))
                .Select(p => p.Name);

            foreach (var navigationProperty in navigationProperties)
            {
                dbSet = dbSet.Include(navigationProperty);
            }

            var records = await dbSet.ToListAsync();
            foreach (var record in records)
            {
                var idProperties = record.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                    .Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var entityId = idProperties
                    .Select(p => (int)p.GetValue(record))
                    .FirstOrDefault(value => value != 0); // Take the first non-zero Id

                if (entityId == 0)
                {
                    Console.WriteLine($"No valid Id found for record in table '{tableName}'.");
                    continue;
                }

                // Check if embedding already exists
                var exists = await _context.EntityEmbeddings
                    .AnyAsync(e => e.EntityName == tableName && e.EntityId == entityId);

                if (exists)
                {
                    Console.WriteLine($"Embedding already exists for Entity '{tableName}' with Id '{entityId}'. Skipping...");
                    continue;
                }

                var content = JsonConvert.SerializeObject(record, Formatting.Indented);

                result.Add(new MyPubSubMessage
                {
                    EntityName = tableName,
                    EntityId = entityId,
                    Content = content
                });

                if (result.Count == 30)
                {
                    // Publish the result array to Pub/Sub
                    await pubSubPublisher.PublishMessageAsync(result);
                    result.Clear(); // Clear the list after publishing
                }
            }

            // Publish any remaining messages
            if (result.Count > 0)
            {
                await pubSubPublisher.PublishMessageAsync(result);
            }
        }

        return null;
    }

    public async Task<string> BulkInsertRecordsAsync(BulkUploadRequest request)
    {
        var type = request.Type;
        var camelCaseType = char.ToUpper(type[0]) + type.Substring(1).ToLower();

        // Get the assembly where BulkUploadRequest is defined
        var assembly = typeof(UNOPSContact).Assembly;

        // Resolve the model type dynamically
        var modelType = assembly.GetType($"UNOPS.PAO.UNOPSDomain.Entities.UNOPS{camelCaseType}", throwOnError: false, ignoreCase: true);
        if (modelType == null)
        {
            throw new InvalidOperationException($"Unsupported type: {type}");
        }

        var recordsArray = request.Records
            .Select(record =>
            {
                if (record is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    // Convert JsonElement to a dictionary
                    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonElement.GetRawText());
                    return JObject.FromObject(dictionary);
                }
                throw new InvalidOperationException("Unsupported record format. Expected JSON object.");
            })
            .ToList();

        var convertedRecords = recordsArray
            .Select(r => r.ToObject(modelType))
            .Cast<object>() // Explicitly cast to object to match DbSet<T>.Add(T entity)
            .ToList();

        var tableName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(request.Type);
        tableName = tableName.Pluralize();

        // Get the DbSet property for the specified table name
        var dbSetProperty = _context.GetType()
            .GetProperty(tableName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (dbSetProperty == null)
        {
            throw new InvalidOperationException($"Table '{tableName}' not found in the context.");
        }

        var dbSet = dbSetProperty.GetValue(_context) as dynamic;
        if (dbSet == null)
        {
            throw new InvalidOperationException($"Unable to retrieve DbSet for table '{tableName}'.");
        }

        var errorMessages = new List<string>();

        // Add all records to the DbSet using reflection
        var addMethod = dbSet.GetType().GetMethod("Add");
        foreach (var record in convertedRecords)
        {
            try
            {
                addMethod.Invoke(dbSet, new[] { record }); // Use reflection to invoke the Add method
                
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Error adding record: {JsonConvert.SerializeObject(record)} - {ex.Message}");
            }
        }

        // Save all records in one operation
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            // Log detailed error information
            foreach (var entry in dbEx.Entries)
            {
                Console.WriteLine($"Entity of type {entry.Entity.GetType().Name} in state {entry.State} caused the error.");
                Console.WriteLine($"Entity data: {JsonConvert.SerializeObject(entry.Entity)}");
            }

            if (dbEx.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
            }

            throw new InvalidOperationException("An error occurred while saving changes to the database. See logs for details.", dbEx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Exception: {ex.Message}");
            throw new InvalidOperationException("An unexpected error occurred while saving changes to the database.", ex);
        }

        // Return consolidated error messages
        return errorMessages.Count > 0 ? string.Join("\n", errorMessages) : "All records processed successfully.";
    }
}