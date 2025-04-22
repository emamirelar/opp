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
using UNOPS.PAO.Utilities.Helpers;

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

    // Map AiPromptModel to AiPrompt entity
    private AiPrompt MapModelToEntity(AiPromptModel model)
    {
        var entity = _mapper.Map(model, new AiPrompt());
        return entity;
    }

    // Get prompt data by type
    public async Task<IEnumerable<AiPromptModel>> GetPromptData(string type)
    {
        return await _aiService.GetPromptData(type);
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
        return _aiService.GetDetailsFromGeminiResponse(modelResponse);
    }

    // Chat with Gemini
    private async Task<dynamic> ChatWithGemini(AiChatSession session, GeminiAssistantRequest req, string promptType, IEnumerable<dynamic> formattedChatHistory, string fileUrl, string fileType)
    {
        var chatHistoryList = formattedChatHistory?.ToList() ?? new List<dynamic>();
        Guid sessionId = req.sessionId;
        string message = req.Message;
        string extractedText = req.ExtractedText ?? "";
        string finalPrompt = (string.IsNullOrEmpty(extractedText) ? message : extractedText);
        var promptData = (await GetPromptData(promptType)).FirstOrDefault();

        if (promptData == null)
        {
            promptType = "general_information";
            promptData = (await GetPromptData(promptType)).FirstOrDefault();
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

        string response = await _aiService.CallGeminiApi(chatHistoryList, promptData);
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

    // Updated FetchResultFromGemini to use CallGeminiApi
    public async Task<string> FetchResultFromGemini(AiPromptModel promptData, string relatedJsonData)
    {
        return await _aiService.FetchResultFromGemini((AiPromptModel)promptData, relatedJsonData);
    }

    // Updated callGemini to use CallGeminiApi
    public async Task<string> callGemini(string prompt, AiPromptModel promptData)
    {
        var promptList = new
        {
            role = "user",
            parts = new[] { new { text = prompt } }
        };
        return await _aiService.CallGeminiApi(promptList, promptData);
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
        var promptData = (await GetPromptData(promptModel.Type)).FirstOrDefault();

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
            var promptData = (await GetPromptData(type)).FirstOrDefault();

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

        // Entity detection and intent classification to be done
        var entityResponse = await EntityDetectionThroughGemini(session, formattedChatHistory, req, fileUrl, fileType);
        var forward = entityResponse.Forward.ToString();
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
        var updatedMessage = await _aiService.GetDependentDropdownValues(detailedResponse?.Dependents, JsonConvert.DeserializeObject(detailedResponse.RawMessage));
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

    public async Task<dynamic> ExtractDataAfterAnalysis(AnalyseFileRequest req, int currentUserId)
    {
        var promptData = (await GetPromptData(req.Type)).FirstOrDefault();
        if (promptData == null)
        {
            return null;
        }

        var fileData = await _aiService.ReadFileData(req.FileId);
        var fileDataArray = JArray.Parse(fileData);

        // Check if we should process asynchronously
        if (fileDataArray.Count > 100)
        {
            var message = new MyPubSubMessage
            {
                MessageType = "BulkImport",
                EntityName = req.Type,
                PromptType = promptData.Type,
                BatchData = JsonConvert.SerializeObject(fileDataArray.ToObject<List<object>>()), // Convert to JSON string
                UserId = currentUserId
            };

            var pubSubPublisher = new PubSubPublisher(_configuration);

            await pubSubPublisher.PublishMessageAsync(new List<MyPubSubMessage> { message });

            return new
            {
                Message = "Bulk import processing started. You will be notified when complete.",
                Entity = req.Type,
                Intent = "Processing"
            };
        }
        else
        {
            var headerRow = fileDataArray[0];
            var finalResponse = new List<dynamic>();
            // Process synchronously
            var batch = new JArray
            {
                headerRow
            };
            for (int i = 1; i < fileDataArray.Count; i++)
            {
                batch.Add(fileDataArray[i]);
            }

            finalResponse = await _aiService.ProcessBulkImport(
                JsonConvert.SerializeObject(batch),
                promptData,
                currentUserId,
                req.Type,
                false
            );

            return new
            {
                Message = "Processing completed successfully",
                Entity = req.Type,
                Intent = "Success",
                Records = JsonConvert.SerializeObject(finalResponse)
            };
        }
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

        var assembly = typeof(UNOPSContact).Assembly;
        var modelType = assembly.GetType($"UNOPS.PAO.UNOPSDomain.Entities.UNOPS{camelCaseType}", throwOnError: false, ignoreCase: true);

        if (modelType == null)
            throw new InvalidOperationException($"Unsupported type: {type}");

        var recordsArray = request.Records.Select(record =>
        {
            if (record is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonElement.GetRawText());
                return JObject.FromObject(dictionary);
            }
            throw new InvalidOperationException("Unsupported record format. Expected JSON object.");
        }).ToList();

        var convertedRecords = recordsArray.Select(r => r.ToObject(modelType)).Cast<object>().ToList();

        // Create a typed array of the correct model type
        var typedArray = Array.CreateInstance(modelType, convertedRecords.Count);

        for (int i = 0; i < convertedRecords.Count; i++)
        {
            typedArray.SetValue(convertedRecords[i], i);
        }

        var tableName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(request.Type).Pluralize();
        var dbSetProperty = _context.GetType().GetProperty(tableName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (dbSetProperty == null)
            throw new InvalidOperationException($"Table '{tableName}' not found in the context.");

        var dbSet = dbSetProperty.GetValue(_context) as dynamic;
        if (dbSet == null)
            throw new InvalidOperationException($"Unable to retrieve DbSet for table '{tableName}'.");

        // Add all at once using AddRange if available
        var addRangeMethod = ((IEnumerable<MethodInfo>)dbSet.GetType().GetMethods())
                                .FirstOrDefault(m => m.Name == "AddRange" && m.GetParameters().Length == 1);

        addRangeMethod?.Invoke(dbSet, new[] { typedArray });

        var successList = new List<object>();
        var errorMessages = new List<string>();
        var isSuccess = true;

        try
        {
            await _context.SaveChangesAsync();

            foreach (var record in convertedRecords)
            {
                try
                {
                    var idProperty = record.GetType()
                                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                    .FirstOrDefault();

                    var idValue = record.GetType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.Name == "Id" && p.PropertyType == typeof(int))
                                .Select(p => (int?)p.GetValue(record))
                                .FirstOrDefault(v => v.HasValue && v.Value != 0);

                    if (idValue != null)
                    {
                        successList.Add(new { Id = idValue, Entity = record });
                    }
                    else
                    {
                        successList.Add(new { Id = "Unknown", Entity = record });
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Success record parsed but ID fetch failed: {ex.Message}");
                }
            }
        }
        catch (DbUpdateException dbEx)
        {
            isSuccess = false;
            foreach (var entry in dbEx.Entries)
            {
                var entityJson = JsonConvert.SerializeObject(entry.Entity);
                var errorMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                errorMessages.Add($"Error saving entity {entry.Entity.GetType().Name}: {entityJson} - {errorMsg}");
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errorMessages.Add($"Unexpected error during SaveChangesAsync: {ex.Message}");
            
            // Add inner exception details if available
            if (ex.InnerException != null)
            {
                errorMessages.Add($"Inner exception: {ex.InnerException.Message}");
            }
        }

        var result = new
        {
            IsSuccess = isSuccess,
            SuccessCount = successList.Count,
            SuccessRecords = successList.Select(s => new { Id = ((dynamic)s).Id }),
            ErrorCount = errorMessages.Count,
            Errors = errorMessages
        };

        return JsonConvert.SerializeObject(result, Formatting.Indented);
    }


    IEnumerable<AiPromptModel> IGeminiManager.GetPromptData(string type)
    {
        throw new NotImplementedException();
    }
}