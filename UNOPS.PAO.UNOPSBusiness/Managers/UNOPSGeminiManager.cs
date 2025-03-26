using Google.Apis.Auth.OAuth2;
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
using System.Net.Http;
using System.Net.Http.Headers;
using Google.Cloud.Vision.V1;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Google.Cloud.TextToSpeech.V1;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSGeminiManager : IGeminiManager
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly GoogleCredential _credentials;
    private readonly DataRepository<AiScreenMapping> _screenMappingRepository;
    private readonly DataRepository<AiPrompt> _promptRepository;
    private readonly UNOPSAppDbContext _context;
    private readonly string _connectionString;
    private readonly GoogleTextToSpeechService _ttsService;
    private readonly TextExtractionService _textExtractionService;
    private readonly GoogleCloudStorageService _gcsService;
    private readonly GeminiSessionService _sessionService;

    public UNOPSGeminiManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        _mapper = mapper;
        _screenMappingRepository = new DataRepository<AiScreenMapping>(context);
        _promptRepository = new DataRepository<AiPrompt>(context);
        _configuration = configuration;
        _credentials = GetCredentials();
        _connectionString = configuration.GetValue<string>("ConnectionStrings:DbSchema");
        _textExtractionService = new TextExtractionService();
        _gcsService = new GoogleCloudStorageService(configuration);
        _sessionService = new GeminiSessionService(context);
        _ttsService = new GoogleTextToSpeechService();
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

    // Get screen mappings by type and sort the response
    public async Task<IEnumerable<AiScreenMapping>> GetScreenMappingsByType(string type)
    {
        return await _screenMappingRepository
            .GetAll()
            .Where(x => x.Type == type)
            .OrderBy(x => x.Order) // Sort by TableName or any other property
            .ToListAsync();
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
        textJson = textJson.Replace("```json", "").Replace("```", "").Replace("\\n", "").Trim();
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
    private async Task<dynamic> ChatWithGemini(AiChatSession session, GeminiAssistantRequest req, string promptType, IEnumerable<dynamic> formattedChatHistory, string fileUrl, string fileType) {
        var chatHistoryList = formattedChatHistory?.ToList() ?? new List<dynamic>();
        Guid sessionId = req.sessionId;
        string message = req.Message;
        string extractedText = req.ExtractedText ?? "";
        string accessToken = await GetAccessTokenAsync();
        string finalPrompt = extractedText ?? message;
        var promptData = GetPromptData(promptType).FirstOrDefault();
        if (promptData == null)
        {
            //throw new Exception("Prompt data not found for the given type.");
            promptType = "general_information";
            promptData = GetPromptData(promptType).FirstOrDefault();
        }
        dynamic generationConfig = string.IsNullOrEmpty(promptData.GenerationConfig)
                        ? new ExpandoObject() : JsonConvert.DeserializeObject<ExpandoObject>(promptData.GenerationConfig);
        dynamic toolsConfig = string.IsNullOrEmpty(promptData.ToolsConfig)
                        ? new List<ExpandoObject>() : JsonConvert.DeserializeObject<List<ExpandoObject>>(promptData.ToolsConfig);
        dynamic safetySettings = string.IsNullOrEmpty(promptData.SafetySettings)
                        ? new List<ExpandoObject>() : JsonConvert.DeserializeObject<List<ExpandoObject>>(promptData.SafetySettings);
        string url = await GetURL(promptData);

        if (chatHistoryList.Count() == 0)
        {
            string promptTemplate = promptData.Prompt;
            finalPrompt = promptTemplate.Replace("{promptData}", message);
        }
        
        chatHistoryList.Add(new
        {
            role = "user",
            parts = new[] { new { text = finalPrompt } }
        });

        var requestBody = new
        {
            contents = new[] { chatHistoryList },
            generationConfig = generationConfig,
            tools = new[] { toolsConfig },
            safetySettings = new[] { safetySettings }
        };

        string jsonRequest = JsonConvert.SerializeObject(requestBody);
        string response = await CallGeminiApiAsync(url, jsonRequest, accessToken);
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
            fileType = "audio/mpeg";
        } else {
            fileUrl = null;
            fileType = null;
        }
        if (parsedResponse["Forward"]?.ToString() == "No")
        {
            _sessionService.UpdateChatHistoryTable(sessionId, "model", message, responseInString, entity, intent, promptType, fileUrl, fileType);
        }
        var finalResponse = new {Entity = entity
                            , Intent = intent
                            , Message = parsedResponse["Message"]?.ToString() ?? ""
                            , Type = parsedResponse["Type"]?.ToString() ?? ""
                            , Summary = parsedResponse["Summary"]?.ToString() ?? ""
                            , Forward = parsedResponse["Forward"]?.ToString() ?? "No"
                            , RawMessage = responseInString
                            , MediaUrl = fileUrl
                            , MediaType = fileType
                            , Files = new[] { new { MediaUrl = fileUrl, MediaType = fileType } }
                            };
        return finalResponse;
    }

    // Fetch result from Gemini
    public async Task<string> FetchResultFromGemini(AiPromptModel promptData, string relatedJsonData) {
        string promptTemplate = promptData.Prompt;
        string finalPrompt = promptTemplate.Replace("{promptData}", relatedJsonData);
        string geminiResponse = await callGemini(finalPrompt, promptData);
        return geminiResponse;
    }

    // Call Gemini API
    public async Task<string> callGemini(string prompt, AiPromptModel promptData)
    {
        string accessToken = await GetAccessTokenAsync();
        var requestBody = await GetRequestBody(prompt, promptData);
        string url = await GetURL(promptData);
        string jsonRequest = JsonConvert.SerializeObject(requestBody);
        string response = await CallGeminiApiAsync(url, jsonRequest, accessToken);
        return response;
    }

    // Get request body for Gemini API
    public async Task<dynamic> GetRequestBody(string prompt, AiPromptModel promptData)
    {
        dynamic contentConfig = JsonConvert.DeserializeObject<ExpandoObject>(promptData.ContentConfig);
        dynamic generationConfig = JsonConvert.DeserializeObject<ExpandoObject>(promptData.GenerationConfig);
        dynamic toolsConfig = string.IsNullOrEmpty(promptData.ToolsConfig)
                        ? new List<ExpandoObject>() : JsonConvert.DeserializeObject<List<ExpandoObject>>(promptData.ToolsConfig);
        dynamic safetySettings = string.IsNullOrEmpty(promptData.SafetySettings)
                        ? new List<ExpandoObject>() : JsonConvert.DeserializeObject<List<ExpandoObject>>(promptData.SafetySettings);

        contentConfig.parts[0].text = prompt;

        var requestBody = new
        {
            contents = new[] { contentConfig },
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
        var credentialParams = _configuration.GetSection("GoogleDriveSettings")
            .Get<JsonCredentialParameters>();
        if (credentialParams == null)
            throw new Exception("GoogleDriveSettings configuration is missing.");

        var secretName = _configuration.GetValue<string>("GoogleDriveSettings:GoogleDriveConnectionKeySecretId");
        var secretManagerProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId, secretName);
        var secretValue = secretManagerProvider.GetSecretVersion(secretName, "latest");

        return GoogleCredential.FromJson(secretValue);
    }

    // Get data based on screen mappings
    public async Task<string> GetDataBasedOnScreenMapping(string type, int recordId, AiScreenMapping[] mappings)
    {
        // To add custom logic, uncomment the following code-block and edit it (example)
        /*
        if (type == 'contacts_summary') {
            // custom logic
            // ensure to return a string (json)
        }
        */
        if (mappings == null || mappings.Length == 0)
        {
            throw new InvalidOperationException("Screen mappings are missing.");
        }

        var dbProperties = typeof(UNOPSAppDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        var selectColumns = BuildSelectColumns(mappings);
        var joinClauses = BuildJoinClauses(mappings);
        string baseTableName = mappings[0].Name;
        string baseTable = $"{_connectionString}.\"{baseTableName}\"";
        string columnWithQuotes = "\"Id\"";
        string baseTableKeyCheck = $"{baseTable}.{columnWithQuotes}";

        string sqlQuery = $@"
            SELECT ROW_TO_JSON(t)
            FROM (
                SELECT {string.Join(", ", selectColumns)}
                FROM {baseTable}
                {string.Join(" ", joinClauses)}
                WHERE {baseTableKeyCheck} = @RecordId
            ) t;";

        var result = await ExecuteSqlQuery(sqlQuery, recordId);
        return JsonConvert.SerializeObject(result[0]?["row_to_json"], Formatting.Indented);
    }

    // Build select columns for SQL query
    private List<string> BuildSelectColumns(AiScreenMapping[] mappings)
    {
        var selectColumns = new List<string>();
        var aggregates = new List<string>();
        foreach (var mapping in mappings)
        {
            var tableRecord = _context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName().Equals(mapping.TableName, StringComparison.OrdinalIgnoreCase));
            var foreignKeys = tableRecord?.GetForeignKeys()?.ToList();
            var primaryKey = tableRecord?.FindPrimaryKey();

            string tableWithSchema = $"{_connectionString}.\"{mapping.TableName}\"";
            var tableProperties = tableRecord?.GetProperties();
            var tablePropetiesAsList = tableProperties?.ToList();
            string aggregation = $"JSON_AGG(DISTINCT JSONB_BUILD_OBJECT(";

            if (!selectColumns.Any(col => col.Contains(tableWithSchema)) && (tableRecord?.ClrType != null && tablePropetiesAsList.Count != 2 && foreignKeys.Count != 2))
            {
                foreach (var property in tableProperties)
                {
                    var commaSeparation = string.Empty;
                    if (tableProperties.First().Name != property.Name)
                    {
                        commaSeparation = ",";
                    }
                    aggregation = string.Concat(aggregation, $"{commaSeparation} '{property.Name}', ", $"{tableWithSchema}.\"{property.Name}\"");
                }
                if (!string.IsNullOrEmpty(aggregation))
                {
                    aggregation = string.Concat(aggregation, $")) AS {mapping.TableName}");
                    selectColumns.Add(aggregation);
                }
            }


            if (!string.IsNullOrEmpty(mapping.RelatedEntity) && !string.IsNullOrEmpty(mapping.RelatedEntityKey))
            {
                aggregation = $"JSON_AGG(DISTINCT JSONB_BUILD_OBJECT(";
                string relatedTableWithSchema = $"{_connectionString}.\"{mapping.RelatedEntity}\"";
                tableRecord = _context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName().Equals(mapping.RelatedEntity, StringComparison.OrdinalIgnoreCase));
                tableProperties = tableRecord?.GetProperties();
                tablePropetiesAsList = tableProperties?.ToList();
                foreignKeys = tableRecord?.GetForeignKeys()?.ToList();
                primaryKey = tableRecord?.FindPrimaryKey();

                if (!selectColumns.Any(col => col.Contains(relatedTableWithSchema)) && (tableRecord?.ClrType != null && tablePropetiesAsList.Count != 2 && foreignKeys.Count != 2))
                {
                    foreach (var property in tableProperties)
                    {
                        var commaSeparation = string.Empty;
                        if (tableProperties.First().Name != property.Name)
                        {
                            commaSeparation = ",";
                        }
                        aggregation = string.Concat(aggregation, $"{commaSeparation} '{property.Name}', ", $"{relatedTableWithSchema}.\"{property.Name}\"");
                    }
                    if (!string.IsNullOrEmpty(aggregation))
                    {
                        aggregation = string.Concat(aggregation, $")) AS {mapping.RelatedEntity}");
                        selectColumns.Add(aggregation);
                    }
                }
            }
        }
        return selectColumns;
    }

    // Build join clauses for SQL query
    private List<string> BuildJoinClauses(AiScreenMapping[] mappings)
    {
        var joinClauses = new List<string>();
        foreach (var mapping in mappings)
        {
            if (!string.IsNullOrEmpty(mapping.RelatedEntity) && !string.IsNullOrEmpty(mapping.RelatedEntityKey))
            {
                string tableWithSchema = $"{_connectionString}.\"{mapping.TableName}\"";
                string relatedTableWithSchema = $"{_connectionString}.\"{mapping.RelatedEntity}\"";
                joinClauses.Add($"LEFT JOIN {relatedTableWithSchema} ON {tableWithSchema}.\"{mapping.ComparisonKey}\" = {relatedTableWithSchema}.\"{mapping.RelatedEntityKey}\"");
            }
        }
        return joinClauses;
    }

    // Execute SQL query
    private async Task<List<Dictionary<string, object>>> ExecuteSqlQuery(string sqlQuery, int recordId)
    {
        var result = new List<Dictionary<string, object>>();
        using (var connection = _context.Database.GetDbConnection())
        {
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sqlQuery;
                command.CommandType = System.Data.CommandType.Text;
                var param = command.CreateParameter();
                param.ParameterName = "@RecordId";
                param.Value = recordId;
                command.Parameters.Add(param);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        result.Add(row);
                    }
                }
            }
        }
        return result;
    }

    // Build nested JSON from result and mappings
    private object BuildNestedJson(List<Dictionary<string, object>> result, AiScreenMapping[] mappings)
    {
        // Implement the logic to build nested JSON from the result and mappings
        // This is a placeholder implementation
        return result;
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
        var screenMappings = (await GetScreenMappingsByType(promptData.Type)).ToArray();
        relatedMessage = await GetDataBasedOnScreenMapping(promptData.Type, req.Id, screenMappings);

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

        // Entity detection and intent classification to be done
        var entityResponse = await EntityDetectionThroughGemini(session, formattedChatHistory, req, fileUrl, fileType);
        //var entityResponse = GetDetailsFromGeminiResponse(entityDetectionResponse);
        var forward = entityResponse.Forward.ToString();
        if (forward == string.Empty || forward == "No") {
            return entityResponse;
        }

        var promptType = entityResponse.Type.ToString();
        var summary = entityResponse.Summary.ToString();

        chatHistory = await GetChatHistory(req.sessionId, promptType);

        formattedChatHistory = chatHistory.Select(x => new {
            role = x.Sender,
            parts = new[] { new { text = x.RawMessage } }
        }).ToList();

        req.Message = "Summary: " + summary;

        var detailedResponse = await FetchDetailedResponseFromGemini(session, formattedChatHistory, req, promptType, fileUrl, fileType);

        _sessionService.UpdateChatHistoryTable(req.sessionId, "model", detailedResponse.Message.ToString(), detailedResponse.RawMessage
                                        , detailedResponse.Entity, detailedResponse.Intent, "entity_intent_detection", detailedResponse.MediaUrl, detailedResponse.MediaType);
        
        return detailedResponse;

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
}