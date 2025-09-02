using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Google.Apis.Auth.OAuth2;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Humanizer;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.WellKnownTypes;
using Google.Api.Gax.ResourceNames;
using Value = Google.Protobuf.WellKnownTypes.Value;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using AutoMapper;
using System.Text;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Models;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

public class SearchResult
{
    public int EntityId { get; set; }
    public float Score { get; set; }
    public string SearchType { get; set; }
}

namespace UNOPS.PAO.UNOPSBusiness.Managers
{
    public class AiContextualService
    {
        private readonly PredictionServiceClient _predictionClient;
        private readonly string _endpoint;
        private readonly IConfiguration _configuration;
        public readonly UNOPSAppDbContext _context;
        private readonly DataRepository<AiPrompt> _promptRepository;
        private readonly GoogleCredential _credentials;
        protected readonly PubSubPublisher _pubSubPublisher;
        private readonly string _connectionString;

        public AiContextualService(IConfiguration configuration, UNOPSAppDbContext context, GoogleCredential credentials)
        {
            _configuration = configuration;
            _context = context;
            _connectionString = configuration.GetValue<string>("ConnectionStrings:DbSchema");
            _credentials = credentials;
            _promptRepository = new DataRepository<AiPrompt>(context);
            _pubSubPublisher = new PubSubPublisher(configuration);
            var projectId = _configuration.GetValue<string>("AISettings:ProjectId");
            var location = _configuration.GetValue<string>("AISettings:Location");
            var model = _configuration.GetValue<string>("AISettings:EmbeddingModelName");
            _endpoint = $"projects/{projectId}/locations/{location}/publishers/google/models/{model}";
            _predictionClient = PredictionServiceClient.Create(); // gRPC Client
        }

        public async Task<string> CreateEmbeddingForText(string text)
        {
            // Reuse the batch embedding function for single text
            var embeddings = await CreateBatchEmbeddingsAsync(new List<string> { text });
            return embeddings.FirstOrDefault() ?? string.Empty;
        }

        public async Task PersistEmbedding(string entityName, int entityId, string text, string vectorString)
        {
            var sql = "CALL public.\"InsertEntityEmbedding\"(@entityName, @entityId, @text, @embedding)";

            var parameters = new[] 
            {
                new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
                new NpgsqlParameter("@entityId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = entityId },
                new NpgsqlParameter("@text", NpgsqlTypes.NpgsqlDbType.Text) { Value = text },
                new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = vectorString }
            };

            // Execute the stored procedure using ExecuteSqlRaw
            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public async Task GenerateEmbeddingAsync(string entityName, int entityId, string text)
        {
            var vectorString = await CreateEmbeddingForText(text);
            await PersistEmbedding(entityName, entityId, text, vectorString);
        }

        public async Task<string> RetrieveContent(string promptType, dynamic entityId)
        {
            // Since screen mappings are no longer used, return a default response
            return "No content found - screen mappings functionality has been removed";
        }

        public async Task<dynamic> RetrieveEntityId(string entityName, string? vectorEmbedding, string? searchText=null, float similarityThreshold=0.3f, float embeddingThreshold=0.7f, string? where=null)
        {
            entityName = entityName.Pluralize();

            // Step 1: Try similarity search first (faster) if we have search text
            if (!string.IsNullOrEmpty(searchText))
            {
                var similarityResult = await ExecuteSimilaritySearch(entityName, searchText, similarityThreshold, where);
                
                if (similarityResult != null && !(similarityResult is DBNull))
                {
                    return similarityResult; // Found via similarity - return immediately
                }
            }
            
            // Step 2: If similarity fails or we only have embedding, use embedding search
            if (!string.IsNullOrEmpty(vectorEmbedding))
            {
                var embeddingResult = await ExecuteEmbeddingSearch(entityName, vectorEmbedding, embeddingThreshold, "1=1");
                return embeddingResult;
            }
            
            // Step 3: If we have search text but no embedding, generate embedding and search
            if (!string.IsNullOrEmpty(searchText))
            {
                var generatedEmbedding = await CreateEmbeddingForText(searchText);
                if (!string.IsNullOrEmpty(generatedEmbedding))
                {
                    var embeddingResult = await ExecuteEmbeddingSearch(entityName, generatedEmbedding, embeddingThreshold, "1=1");
                    
                    // If embedding search also fails but we have a vector, log for future searches
                    if ((embeddingResult == null || embeddingResult is DBNull))
                    {
                        Console.WriteLine($"No match found for '{searchText}' in '{entityName}', but embedding created for future searches.");
                    }
                    
                    return embeddingResult;
                }
            }
            
            return null;
        }

        private async Task<dynamic> ExecuteSimilaritySearch(string entityName, string searchText, float similarityThreshold, string whereCondition)
        {
            var sql = "SELECT entityId, score, search_type FROM public.retrieve_similarity_search(@entityName, @searchText, @similarityThreshold, @where) LIMIT 1";
            
            var parameters = new[] 
            {
                new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
                new NpgsqlParameter("@searchText", NpgsqlTypes.NpgsqlDbType.Text) { Value = searchText },
                new NpgsqlParameter("@similarityThreshold", NpgsqlTypes.NpgsqlDbType.Real) { Value = similarityThreshold },
                new NpgsqlParameter("@where", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object?)whereCondition ?? DBNull.Value }
            };

            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result;
        }

        private async Task<dynamic> ExecuteEmbeddingSearch(string entityName, string embeddingVector, float embeddingThreshold, string whereCondition)
        {
            var sql = "SELECT entityId, score, search_type FROM public.retrieve_embedding_search(@entityName, @embedding, @embeddingThreshold, @where) LIMIT 1";
            
            var parameters = new[] 
            {
                new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
                new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = embeddingVector },
                new NpgsqlParameter("@embeddingThreshold", NpgsqlTypes.NpgsqlDbType.Real) { Value = embeddingThreshold },
                new NpgsqlParameter("@where", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object?)whereCondition ?? DBNull.Value }
            };

            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result;
        }

        public async Task<List<SearchResult>> ExecuteEmbeddingSearchMultiple(string entityName, string embeddingVector, float embeddingThreshold = 0.7f, int resultLimit = 10, string whereCondition = null)
        {
            var sql = "SELECT entityId, score, search_type FROM public.retrieve_embedding_search_multiple(@entityName, @embedding, @embeddingThreshold, @resultLimit, @where)";
            
            var parameters = new[] 
            {
                new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
                new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = embeddingVector },
                new NpgsqlParameter("@embeddingThreshold", NpgsqlTypes.NpgsqlDbType.Real) { Value = embeddingThreshold },
                new NpgsqlParameter("@resultLimit", NpgsqlTypes.NpgsqlDbType.Integer) { Value = resultLimit },
                new NpgsqlParameter("@where", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object?)whereCondition ?? DBNull.Value }
            };

            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);

            var results = new List<SearchResult>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new SearchResult
                {
                    EntityId = reader.GetInt32("entityId"),
                    Score = reader.GetFloat("score"),
                    SearchType = reader.GetString("search_type")
                });
            }

            return results;
        }

        public async Task<List<SearchResult>> RetrieveSimilarityIds(string entityName, string similarityCriteria, string vectorEmbedding=null, float similarityThreshold=0.3f, float embeddingThreshold=0.7f, string whereCondition=null)
        {
            var sql = "SELECT entityId, score, search_type FROM public.retrieve_similarity_results(@entityName, @text, @embedding, @similarityThreshold, @embeddingThreshold, @where)";

            entityName = entityName.Pluralize();

            var parameters = new[] 
            {
                new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
                new NpgsqlParameter("@text", NpgsqlTypes.NpgsqlDbType.Text) { Value = similarityCriteria },
                new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object?)vectorEmbedding ?? DBNull.Value },
                new NpgsqlParameter("@similarityThreshold", NpgsqlTypes.NpgsqlDbType.Real) { Value = similarityThreshold },
                new NpgsqlParameter("@embeddingThreshold", NpgsqlTypes.NpgsqlDbType.Real) { Value = embeddingThreshold },
                new NpgsqlParameter("@where", NpgsqlTypes.NpgsqlDbType.Text) { Value = (object?)whereCondition ?? DBNull.Value }
            };

            // Execute the query and return all matching results
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);

            var results = new List<SearchResult>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new SearchResult
                {
                    EntityId = reader.GetInt32("entityId"),
                    Score = reader.GetFloat("score"),
                    SearchType = reader.GetString("search_type")
                });
            }

            return results;
        }

        public async Task<string> ReadFileData(string fileId)
        {
            try
            {
                var service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = _credentials,
                    ApplicationName = "GoogleSheetsReader",
                });
                var spreadsheet = service.Spreadsheets.Get(fileId).Execute();
                var firstSheetName = spreadsheet.Sheets[0].Properties.Title;

                // Read values
                var request = service.Spreadsheets.Values.Get(fileId, firstSheetName);
                ValueRange response = await request.ExecuteAsync();
                var data = string.Empty;

                if (response.Values != null && response.Values.Count > 0)
                {
                    // Convert response.Values to a stringified array
                    data = JsonConvert.SerializeObject(response.Values);
                }

                return data;
            }
            catch (Exception ex)
            {   
                // Throw a more descriptive error
                throw new Exception($"Failed to read Google Sheet data. FileId: {fileId}. Error: {ex.Message}", ex);
            }
        }

        private static AiPromptModel MapEntityToAiPromptModel(AiPrompt entity, IMapper mapper)
        {
            var result = mapper.Map<AiPrompt, AiPromptModel>(entity);
            return result;
        }

        public JObject GetDetailsFromGeminiResponse(string modelResponse)
        {
            JObject json = JObject.Parse(modelResponse);
            var candidates = json["candidates"];
            var parts = candidates[0]?["content"]["parts"];
            var textJson = parts[0]["text"].ToString();
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

        public async Task<IEnumerable<AiPrompt>> GetPromptData(string type)
        {
            var prompts = await _promptRepository
                .GetAll()
                .Where(x => x.Type == type)
                .ToListAsync();

        return prompts.Select(entity => new AiPrompt
        {
            Type = entity.Type,
            Prompt = entity.Prompt ?? string.Empty, // Ensure null safety
            ContentConfig = entity.ContentConfig,
            GenerationConfig = entity.GenerationConfig,
            ToolsConfig = entity.ToolsConfig,
            SafetySettings = entity.SafetySettings,
            Location = entity.Location,
            Project = entity.Project,
            Model = entity.Model,
            PromptFunction = entity.PromptFunction,
            Name = entity.Name
        }).ToList();
    }

    public async Task<string> FetchResultFromGemini(AiPrompt promptData, string relatedJsonData)
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

    // Common function to handle Gemini API calls
    public async Task<string> CallGeminiApi(dynamic prompt, AiPrompt promptData)
    {
        string accessToken = await GetAccessTokenAsync();
        var requestBody = await GetRequestBody(prompt, promptData);
        string url = await GetURL(promptData);
        string jsonRequest = JsonConvert.SerializeObject(requestBody);
        return await CallGeminiApiAsync(url, jsonRequest, accessToken);
        }

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

        private static async Task<string> GetAccessTokenAsync()
        {
            GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
            credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        }

        private async Task<string> GetURL(AiPrompt promptData)
        {
            return $"https://{promptData.Location}-aiplatform.googleapis.com/v1/projects/{promptData.Project}/locations/{promptData.Location}/publishers/google/models/{promptData.Model}:generateContent";
        }

        public async Task PublishMessageToPubSub(MyPubSubMessage message)
        {
            // Publishing the message
            await _pubSubPublisher.PublishMessageAsync(new List<MyPubSubMessage> { message });
        }

        public async Task<dynamic> GetRequestBody(dynamic prompt, AiPrompt promptData)
        {
            dynamic contentConfig = JsonConvert.DeserializeObject<ExpandoObject>(promptData.ContentConfig);
            dynamic generationConfig = JsonConvert.DeserializeObject<ExpandoObject>(promptData.GenerationConfig);
            
            // Handle toolsConfig - support both old object format and new array format
            dynamic toolsConfig;
            if (string.IsNullOrEmpty(promptData.ToolsConfig))
            {
                toolsConfig = new List<ExpandoObject>();
            }
            else
            {
                try
                {
                    // First try to parse as array (new format)
                    toolsConfig = JsonConvert.DeserializeObject<List<ExpandoObject>>(promptData.ToolsConfig);
                }
                catch (JsonException)
                {
                    try
                    {
                        // If that fails, try to parse as object (old format) and convert to array
                        var toolConfigObject = JsonConvert.DeserializeObject<ExpandoObject>(promptData.ToolsConfig);
                        toolsConfig = new List<ExpandoObject> { toolConfigObject };
                    }
                    catch (JsonException)
                    {
                        // If both fail, use empty list
                        toolsConfig = new List<ExpandoObject>();
                    }
                }
            }
            
            dynamic safetySettings = string.IsNullOrEmpty(promptData.SafetySettings)
                            ? new List<ExpandoObject>() : JsonConvert.DeserializeObject<List<ExpandoObject>>(promptData.SafetySettings);

            if (prompt is string)
            {
                contentConfig.parts[0].text = prompt.ToString();
            }
            else
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

        public async Task<List<dynamic>> ProcessBulkImport(string stringifiedBatchRecords, AiPrompt promptData, int userId, string entityName, bool isAsync = false)
        {
            var finalResponse = new List<dynamic>();

            List<object> batchData;

            try
            {
                // Unescape the deeply escaped JSON string
                string unescapedJson = stringifiedBatchRecords.Replace("\\\"", "\"").Trim('"');
                
                // Deserialize the unescaped JSON into a list of objects
                batchData = JsonConvert.DeserializeObject<List<object>>(unescapedJson);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize batch records. Ensure the input is a valid JSON array.", ex);
            }

            // Define batch size based on entity type
            // Increased token limit allows larger batches for Partner
            int batchSize = 25; // Increased from 5 to 25 for Partner
            
            // Log the batch size for debugging
            if (entityName.Equals("Partner", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Using batch size (25) for Partner entity with increased token limit");
            }
            
            var headerRow = batchData[0];

            for (int i = 1; i < batchData.Count; i += batchSize)
            {
                var batch = new JArray
                {
                    headerRow
                };
                for (int j = i; j < i + batchSize && j < batchData.Count; j++)
                {
                    batch.Add(batchData[j]);
                }

                // Process the entire batch at once
                var content = JsonConvert.SerializeObject(batch, Formatting.Indented);

                // Process the content using Gemini
                string response = await FetchResultFromGemini(promptData, content);
                var parsedResponse = GetDetailsFromGeminiResponse(response);

                var records = parsedResponse["records"];
                if (records != null)
                {
                    foreach (var record in records)
                    {
                        var dependents = record["dependents"]?.ToString();
                        dynamic updatedResponse = await GetDependentDropdownValues(dependents, record, promptData);
                        finalResponse.Add(updatedResponse);
                    }
                }
            }

            if (isAsync)
            {
                // Create a single notification for the entire batch
                var notification = new Notification
                {
                    UserId = userId,
                    Message = "Batch processed successfully",
                    Category = promptData.Type,
                    ResponseType = "Success",
                    RecordData = JsonConvert.SerializeObject(finalResponse),
                    IsRead = false,
                    Status = NotificationStatus.Done,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
            }

            return finalResponse;
        }
        
        // New method with progress tracking
        public async Task<List<dynamic>> ProcessBulkImportWithProgress(
            string stringifiedBatchRecords, 
            AiPrompt promptData, 
            int userId, 
            string entityName, 
            bool isAsync = false,
            Func<int, int, List<dynamic>, Task<bool>> progressCallback = null,
            string fileId = null)
        {
            var finalResponse = new List<dynamic>();

            List<object> batchData;

            try
            {
                // Unescape the deeply escaped JSON string
                string unescapedJson = stringifiedBatchRecords.Replace("\\\"", "\"").Trim('"');
                
                // Deserialize the unescaped JSON into a list of objects
                batchData = JsonConvert.DeserializeObject<List<object>>(unescapedJson);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize batch records. Ensure the input is a valid JSON array.", ex);
            }

            // Define batch size based on entity type
            // Increased token limit allows larger batches for Partner
            int batchSize = 25; // Increased from 5 to 25 for Partner
            var headerRow = batchData[0];
            int totalRecords = batchData.Count - 1; // Excluding header
            int processedRecords = 0;

            // Initial progress update
            if (progressCallback != null)
            {
                await progressCallback(processedRecords, totalRecords, finalResponse);
            }

            for (int i = 1; i < batchData.Count; i += batchSize)
            {
                var batch = new JArray
                {
                    headerRow
                };
                
                int recordsInBatch = 0;
                for (int j = i; j < i + batchSize && j < batchData.Count; j++)
                {
                    batch.Add(batchData[j]);
                    recordsInBatch++;
                }

                // Process the entire batch at once
                var content = JsonConvert.SerializeObject(batch, Formatting.Indented);

                // Process the content using Gemini
                string response = await FetchResultFromGemini(promptData, content);
                var parsedResponse = GetDetailsFromGeminiResponse(response);

                var records = parsedResponse["records"];
                if (records != null)
                {
                    foreach (var record in records)
                    {
                        var dependents = record["dependents"]?.ToString();
                        dynamic updatedResponse = await GetDependentDropdownValues(dependents, record, promptData);
                        finalResponse.Add(updatedResponse);
                    }
                }
                
                // Update processed count
                processedRecords += recordsInBatch;
                
                // Call progress callback
                if (progressCallback != null)
                {
                    bool shouldContinue = await progressCallback(processedRecords, totalRecords, finalResponse);
                    if (!shouldContinue)
                    {
                        break; // Allow for cancellation
                    }
                }
            }

            // Apply duplicate detection for async processing (same as sync)
            if (isAsync && finalResponse != null && finalResponse.Count > 0)
            {
                // Convert records to dynamic list for duplicate detection
                var recordsList = finalResponse.Select(r => (dynamic)r).ToList();
                
                // Check for internal duplicates within the file first
                var internalDuplicateResult = await DetectInternalDuplicatesAsync(entityName, recordsList, 0.8);
                
                // If internal duplicates are found, create error notification
                if (internalDuplicateResult.HasInternalDuplicates)
                {
                    var errorNotification = new Notification
                    {
                        UserId = userId,
                        Message = !string.IsNullOrEmpty(fileId) 
                            ? $"Internal duplicates found in the uploaded file (Sheet ID: {fileId}). Please fix the duplicates before proceeding."
                            : "Internal duplicates found in the uploaded file. Please fix the duplicates before proceeding.",
                        Category = promptData.Type,
                        ResponseType = "InternalDuplicatesFound",
                        RecordData = JsonConvert.SerializeObject(new
                        {
                            intent = "InternalDuplicatesFound",
                            fileId = fileId, // Include sheet ID in the data
                            internalDuplicates = new
                            {
                                totalGroups = internalDuplicateResult.TotalDuplicateGroups,
                                totalDuplicateRecords = internalDuplicateResult.TotalDuplicateRecords,
                                totalRecords = internalDuplicateResult.TotalRecords,
                                cleanRecords = internalDuplicateResult.CleanRecords,
                                duplicateGroups = internalDuplicateResult.DuplicateGroups.Select(group => new
                                {
                                    masterRowNumber = group.MasterIndex + 2, // +2 because: +1 for 0-based index, +1 for header row
                                    duplicateRowNumbers = group.DuplicateIndices.Select(idx => idx + 2).ToList(),
                                    matchReasons = group.MatchReasons
                                }).ToList()
                            }
                        }),
                        IsRead = false,
                        Status = NotificationStatus.Done,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Notifications.AddAsync(errorNotification);
                    await _context.SaveChangesAsync();
                    return finalResponse; // Return without database duplicate detection
                }
                
                // If no internal duplicates, proceed with database duplicate detection
                var recordsWithDuplicates = await DetectDuplicatesAsync(entityName, recordsList, 0.65);
                finalResponse = recordsWithDuplicates.Select(r => (object)r).ToList();
            }

            if (isAsync)
            {
                // Create a single notification for the entire batch
                var notification = new Notification
                {
                    UserId = userId,
                    Message = !string.IsNullOrEmpty(fileId) 
                        ? $"Batch processed successfully with duplicate detection (Sheet ID: {fileId})"
                        : "Batch processed successfully with duplicate detection",
                    Category = promptData.Type,
                    ResponseType = "Success",
                    RecordData = JsonConvert.SerializeObject(finalResponse),
                    IsRead = false,
                    Status = NotificationStatus.Done,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
            }

            // Publish entity processing messages to PubSub after the bulk import is completed
            await PublishEntityProcessingMessages(entityName, finalResponse);

            return finalResponse;
        }

        public async Task<dynamic> GetDependentDropdownValues(dynamic dependents, dynamic responseObject, AiPrompt promptData)
        {
            var interactionType = false;
            if (!string.IsNullOrWhiteSpace(dependents))
            {
                var dependentsList = JsonConvert.DeserializeObject<List<string>>(dependents);

                if (dependentsList.Count > 0)
                {
                    if (promptData?.Type?.Contains("interaction", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        interactionType = true;
                    }
                    
                    foreach (var dependent in dependentsList)
                    {
                        var text = responseObject[dependent];
                        if (text != null)
                        {
                            // Special case: OrganizationUnitRelationships (many-to-many)
                            if (dependent == "organizationUnitRelationships")
                            {
                                await HandleOrganizationUnitRelationships(responseObject, text);
                                continue;
                            }
                            
                            // Check if the dependent field is already an array of text values
                            if (text is JArray textArray)
                            {
                                // Handle array of text values - convert each to ID
                                var idsArray = new JArray();
                                
                                foreach (var textItem in textArray)
                                {
                                    var textValue = textItem?.ToString();
                                    if (!string.IsNullOrEmpty(textValue))
                                    {
                                        int id;
                                        // Check if it's already a numeric value
                                        if (int.TryParse(textValue, out id))
                                        {
                                            idsArray.Add(id);
                                            
                                            // For interactions, handle special logic for existing IDs
                                            if (interactionType && dependent == "contactIds")
                                            {
                                                await HandleInteractionContactLogic(responseObject, id);
                                            }
                                        }
                                        else
                                        {
                                            // Convert text to entity ID
                                            var entityId = await GetEntityIdFromText(textValue, dependent);
                                            if (entityId != null && !(entityId is DBNull))
                                            {
                                                idsArray.Add(entityId);
                                                
                                                // Special handling for interactions
                                                if (interactionType)
                                                {
                                                    if (dependent == "contactIds")
                                                    {
                                                        await HandleInteractionContactLogic(responseObject, entityId);
                                                    }
                                                    else if (dependent == "userIds")
                                                    {
                                                        await AddEmailToResponse(responseObject, entityId);
                                                    }
                                                    else if (dependent == "organizationHierarchyIds")
                                                    {
                                                        // Handle org unit logic if needed
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                
                                responseObject[dependent] = idsArray;
                            }
                            else
                            {
                                // Handle single text value (existing behavior)
                                dynamic entityId;
                                int id;
                                if (text?.Value != null)
                                {
                                    text = text.Value;
                                }
                                
                                // Check if 'text' is already a numeric value (long/int)
                                if (text is long longValue)
                                {
                                    // It is already an ID, just continue
                                    if (interactionType && dependent == "contactIds")
                                    {
                                        await HandleInteractionContactLogic(responseObject, longValue);
                                    }
                                    continue;
                                } else if (text is int intValue)
                                {
                                    // It is already an ID, just continue
                                    if (interactionType && dependent == "contactIds")
                                    {
                                        await HandleInteractionContactLogic(responseObject, intValue);
                                    }
                                    continue;
                                }
                                else if (int.TryParse(text?.ToString(), out id))
                                {
                                    if (interactionType && dependent == "contactIds")
                                    {
                                        await HandleInteractionContactLogic(responseObject, id);
                                    }
                                    continue;
                                }
                                
                                // Convert text to entity ID
                                entityId = await GetEntityIdFromText(text?.ToString(), dependent);
                                if (entityId == null || entityId is DBNull)
                                {
                                    continue;
                                }
                                else
                                {
                                    // Check if the dependent field is already an array
                                    if (responseObject[dependent] is JArray existingArray)
                                    {
                                        // Handle as array - append if not already present
                                        if (!existingArray.Any(e => e.ToString() == entityId.ToString()))
                                        {
                                            existingArray.Add(entityId);
                                        }
                                    }
                                    else
                                    {
                                        // Handle as single value
                                        responseObject[dependent] = entityId;
                                    }
                                }
                                
                                // Special handling for interactions
                                if (interactionType)
                                {
                                    if (dependent == "contactIds")
                                    {
                                        await HandleInteractionContactLogic(responseObject, entityId);
                                    }
                                    else if (dependent == "userIds")
                                    {
                                        await AddEmailToResponse(responseObject, entityId);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return responseObject;
        }
        
        // Helper method to publish entity processing messages to PubSub
        public async Task PublishEntityProcessingMessages(string entityName, List<dynamic> processedEntities)
        {
            try
            {
                // Create a list to hold batches of messages (max 50 per batch)
                var messages = new List<MyPubSubMessage>();
                
                foreach (dynamic entity in processedEntities)
                {
                    // Extract the ID from the entity
                    if (entity["id"] != null || entity["Id"] != null)
                    {
                        int entityId;
                        var idValue = entity["id"] ?? entity["Id"];
                        
                        // Handle different ID formats
                        if (idValue is int id)
                        {
                            entityId = id;
                        }
                        else if (int.TryParse(idValue?.ToString(), out int parsedId))
                        {
                            entityId = parsedId;
                        }
                        else
                        {
                            // Skip if we can't get a valid ID
                            continue;
                        }
                        
                        messages.Add(new MyPubSubMessage
                        {
                            MessageType = "EntityProcessing",
                            EntityName = entityName,
                            EntityId = entityId
                        });
                        
                        // Publish in batches of 50 to avoid overwhelming the service
                        if (messages.Count >= 50)
                        {
                            await _pubSubPublisher.PublishMessageAsync(messages);
                            messages.Clear();
                        }
                    }
                }
                
                // Publish any remaining messages
                if (messages.Count > 0)
                {
                    await _pubSubPublisher.PublishMessageAsync(messages);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the operation
                Console.WriteLine($"Error publishing entity processing messages to PubSub: {ex.Message}");
            }
        }

        private async Task<dynamic> GetEntityIdFromText(string text, string dependent)
        {
            // Convert dependent to entity name - remove "Id"/"Ids" and capitalize first letter only
            string baseEntityName = dependent.EndsWith("Ids", StringComparison.OrdinalIgnoreCase) ? 
                dependent.Substring(0, dependent.Length - 3) : 
                dependent.Replace("Id", "", StringComparison.OrdinalIgnoreCase);
            
            // Capitalize only the first letter, preserving existing capitalization
            string entityName = string.IsNullOrEmpty(baseEntityName) ? 
                baseEntityName : 
                char.ToUpper(baseEntityName[0]) + baseEntityName.Substring(1);
            
            string whereCondition = "1=1"; // Default WHERE condition
            
            // Special case for OrganizationUnitRelationships - should look at OrganizationHierarchies table
            if (dependent.Equals("organizationUnitRelationships", StringComparison.OrdinalIgnoreCase)
                    // Special case for organizationHierarchyIds - should look at OrganizationHierarchies table
                    || dependent.Equals("organizationHierarchyIds", StringComparison.OrdinalIgnoreCase)
                    || entityName.Equals("Orgunit", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "OrganizationHierarchies";
                whereCondition = "\"Type\" = 'OrgUnit'";
            }
            // Special case for User/UserIds - should look at UserProfile table (which has searchable Name field)
            else if (entityName.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "UserProfile";
                // UserProfile can be searched by Name field directly
                whereCondition = "1=1"; // Allow all UserProfiles to be searched
            }
            // Special case for Contact/ContactIds - should look at Contacts table
            else if (entityName.Equals("Contact", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "Contacts";
                whereCondition = "1=1"; // Allow all Contacts to be searched
            }
            // Special case for partnerGroupId - should look at PartnerTrees table
            else if (dependent.Equals("partnerGroupId", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "PartnerTrees";
                whereCondition = "1=1";
            }
            // Special case for partnerCategoryId - should look at PartnerTrees table  
            else if (dependent.Equals("partnerCategoryId", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "PartnerTrees";
                whereCondition = "1=1";
            }
            else
            {
                entityName = entityName.Pluralize();
            }
            
            return await RetrieveEntityId(entityName, null, text, 0.3f, 0.7f, whereCondition);
        }
        
        private async Task HandleOrganizationUnitRelationships(dynamic responseObject, dynamic orgUnitText)
        {
            var orgUnitIds = new JArray();
            
            if (orgUnitText is JArray orgUnitArray)
            {
                // Handle array of org unit names
                foreach (var orgUnitName in orgUnitArray)
                {
                    var orgUnitId = await GetEntityIdFromText(orgUnitName?.ToString(), "organizationUnitRelationships");
                    if (orgUnitId != null && !(orgUnitId is DBNull))
                        orgUnitIds.Add(orgUnitId);
                }
            }
            else if (orgUnitText != null)
            {
                // Handle single org unit name
                var orgUnitId = await GetEntityIdFromText(orgUnitText.ToString(), "organizationUnitRelationships");
                if (orgUnitId != null && !(orgUnitId is DBNull))
                    orgUnitIds.Add(orgUnitId);
            }
            
            responseObject["organizationUnitRelationships"] = orgUnitIds;
        }
        
        private async Task AddEmailToResponse(dynamic responseObject, dynamic entityId)
        {
            // Cast entityId to int to avoid dynamic operation in expression tree
            int idToSearch = Convert.ToInt32(entityId);
            string emailId = null;
            
            // First, try to find email in Contacts table
            emailId = await _context.Contacts
                .Where(c => c.Id == idToSearch)
                .Select(c => c.Email)
                .FirstOrDefaultAsync();
            
            // If not found in Contacts, try UserProfile table
            if (string.IsNullOrEmpty(emailId))
            {
                emailId = await _context.UserProfile
                    .Where(u => u.UserId == idToSearch)
                    .Select(u => u.UserEmail)
                    .FirstOrDefaultAsync();
            }
            
            if (!string.IsNullOrEmpty(emailId))
            {
                // Handle emailAddresses as an array
                if (responseObject["emailAddresses"] == null)
                {
                    responseObject["emailAddresses"] = new JArray();
                }
                
                var emailArray = (JArray)responseObject["emailAddresses"];
                if (!emailArray.Any(e => e.ToString() == emailId))
                {
                    emailArray.Add(emailId);
                }
            }
        }

        /// <summary>
        /// Special handling for interaction contacts - adds email and partner information
        /// </summary>
        private async Task HandleInteractionContactLogic(dynamic responseObject, dynamic contactId)
        {
            int idToSearch = Convert.ToInt32(contactId);
            
            // Get contact details including email and partner
            var contact = await _context.Contacts
                .Where(c => c.Id == idToSearch)
                .Select(c => new { c.Email, c.PartnerId })
                .FirstOrDefaultAsync();
            
            if (contact != null)
            {
                // Add email to emailAddresses array
                if (!string.IsNullOrEmpty(contact.Email))
                {
                    if (responseObject["emailAddresses"] == null)
                    {
                        responseObject["emailAddresses"] = new JArray();
                    }
                    
                    var emailArray = (JArray)responseObject["emailAddresses"];
                    if (!emailArray.Any(e => e.ToString() == contact.Email))
                    {
                        emailArray.Add(contact.Email);
                    }
                }
                
                // Add partner ID to partnerIds array
                if (contact.PartnerId != null)
                {
                    if (responseObject["partnerIds"] == null)
                    {
                        responseObject["partnerIds"] = new JArray();
                    }
                    
                    var partnerArray = (JArray)responseObject["partnerIds"];
                    if (!partnerArray.Any(p => p.ToString() == contact.PartnerId.ToString()))
                    {
                        partnerArray.Add(contact.PartnerId);
                    }
                }
            }
        }

        /// <summary>
        /// Creates batch embeddings using Gemini Embedding API
        /// </summary>
        /// <param name="texts">List of texts to create embeddings for</param>
        /// <returns>List of embedding vectors as strings</returns>
        public async Task<List<string>> CreateBatchEmbeddingsAsync(List<string> texts)
        {
            if (texts == null || !texts.Any())
                return new List<string>();

            var embeddings = new List<string>();
            var batchSize = 30; // Process in batches of 30

            for (int i = 0; i < texts.Count; i += batchSize)
            {
                var batch = texts.Skip(i).Take(batchSize).ToList();
                var batchEmbeddings = await CreateEmbeddingsBatchAsync(batch);
                embeddings.AddRange(batchEmbeddings);
            }

            return embeddings;
        }

        /// <summary>
        /// Creates embeddings for a batch of texts using Vertex AI Embedding API
        /// </summary>
        /// <param name="texts">Batch of texts to create embeddings for</param>
        /// <returns>List of embedding vectors as strings</returns>
        private async Task<List<string>> CreateEmbeddingsBatchAsync(List<string> texts)
        {
            try
            {
                var projectId = _configuration.GetValue<string>("AISettings:ProjectId");
                var location = _configuration.GetValue<string>("AISettings:Location");
                
                if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(location))
                {
                    throw new InvalidOperationException("Project ID or Location not configured in AISettings");
                }

                // Get access token using Google Cloud credentials
                var accessToken = await GetAccessTokenAsync();

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var embeddings = new List<string>();

                // Create batch request with all texts
                 var instances = new List<object>();
                 foreach (var text in texts)
                 {
                     instances.Add(new
                     {
                         task_type = "SEMANTIC_SIMILARITY",
                         content = text
                     });
                 }

                var requestBody = new
                  {
                      instances = instances,
                      parameters = new
                      {
                          outputDimensionality = 768
                      }
                  };

                 var jsonContent = JsonConvert.SerializeObject(requestBody);
                 var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                 var url = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/gemini-embedding-001:predict";
                 
                 var response = await httpClient.PostAsync(url, content);

                 if (!response.IsSuccessStatusCode)
                 {
                     var errorContent = await response.Content.ReadAsStringAsync();
                     throw new HttpRequestException($"Vertex AI API error: {response.StatusCode} - {errorContent}");
                 }

                 var responseContent = await response.Content.ReadAsStringAsync();
                 var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                 // Process each prediction in the response
                 if (responseObject?.predictions != null)
                 {
                     foreach (var prediction in responseObject.predictions)
                     {
                         if (prediction?.embeddings?.values != null)
                         {
                             var values = prediction.embeddings.values.ToObject<float[]>();
                             var valueStrings = new List<string>();
                             foreach (var v in values)
                             {
                                 valueStrings.Add(v.ToString(CultureInfo.InvariantCulture));
                             }
                             var vectorString = "[" + string.Join(",", valueStrings) + "]";
                             embeddings.Add(vectorString);
                         }
                     }
                 }

                return embeddings;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating batch embeddings: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts entity data to readable string format for embedding generation
        /// </summary>
        /// <param name="entityData">The entity data object</param>
        /// <returns>Readable string representation</returns>
        private string ConvertEntityDataToReadableString(object entityData)
        {
            if (entityData == null) return string.Empty;

            var readableLines = new List<string>();

            // Handle dynamic objects (JObject, ExpandoObject, etc.)
            if (entityData is IDictionary<string, object> dynamicDict)
            {
                foreach (var kvp in dynamicDict)
                {
                    try
                    {
                        var value = kvp.Value;
                        
                        // Skip null values and complex objects
                        if (value == null) continue;
                        
                        string formattedValue = FormatValueForReadableString(value);
                        
                        // Add to readable format if we have a meaningful value
                        if (!string.IsNullOrWhiteSpace(formattedValue))
                        {
                            // Convert property name from PascalCase to readable format
                            var readablePropertyName = System.Text.RegularExpressions.Regex.Replace(kvp.Key, "([a-z])([A-Z])", "$1 $2");
                            readableLines.Add($"{readablePropertyName}: {formattedValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log warning but continue processing other properties
                        System.Diagnostics.Debug.WriteLine($"Error processing property {kvp.Key}: {ex.Message}");
                    }
                }
            }
            else
            {
                // Handle regular objects using reflection
                var properties = entityData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var property in properties)
                {
                    try
                    {
                        var value = property.GetValue(entityData);
                        
                        // Skip null values, empty collections, and complex navigation properties
                        if (value == null) continue;
                        
                        string formattedValue = FormatValueForReadableString(value);

                        // Add to readable format if we have a meaningful value
                        if (!string.IsNullOrWhiteSpace(formattedValue))
                        {
                            // Convert property name from PascalCase to readable format
                            var readablePropertyName = System.Text.RegularExpressions.Regex.Replace(property.Name, "([a-z])([A-Z])", "$1 $2");
                            readableLines.Add($"{readablePropertyName}: {formattedValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log warning but continue processing other properties
                        System.Diagnostics.Debug.WriteLine($"Error processing property {property.Name}: {ex.Message}");
                    }
                }
            }

            return string.Join("\n", readableLines);
        }

        /// <summary>
        /// Formats a value for readable string representation
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>Formatted string or null if should be skipped</returns>
        private string FormatValueForReadableString(object value)
        {
            if (value is string str)
            {
                return string.IsNullOrWhiteSpace(str) ? null : str;
            }
            else if (value is DateTime dateTime)
            {
                return dateTime == DateTime.MinValue ? null : dateTime.ToString("yyyy-MM-dd");
            }
            else if (value is bool boolean)
            {
                return boolean.ToString();
            }
            else if (value is int number)
            {
                return number == 0 ? null : number.ToString();
            }
            else if (value is decimal dec)
            {
                return dec == 0 ? null : dec.ToString("0.##");
            }
            else if (value is System.Enum enumValue)
            {
                return enumValue.ToString();
            }
            else if (value is System.Collections.IEnumerable)
            {
                return null; // Skip complex objects and collections
            }
            else if (value.GetType().IsClass && value.GetType() != typeof(string))
            {
                return null; // Skip complex objects
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Detects duplicates for a list of records using field-specific matching only
        /// </summary>
        /// <param name="entityName">Name of the entity type (e.g., "Contact", "Partner", "Interaction")</param>
        /// <param name="records">List of records to check for duplicates</param>
        /// <param name="fieldMatchThreshold">Field matching threshold for duplicate detection (default: 0.5)</param>
        /// <returns>List of records with duplicate information added</returns>
        public async Task<List<dynamic>> DetectDuplicatesAsync(string entityName, List<dynamic> records, 
            double fieldMatchThreshold = 0.5)
        {
            if (records == null || !records.Any())
                return records;

            try
            {
                // Ensure entity name is pluralized for consistency with the database
                var pluralizedEntityName = entityName.Pluralize();

                // Check for duplicates using the simplified field-based detection function
                for (int i = 0; i < records.Count; i++)
                {
                    var record = records[i];

                    // Use the simplified detect_duplicate_records function (field-based only)
                    var duplicateResult = await DetectDuplicateForRecordAsync(
                        pluralizedEntityName, 
                        record, 
                        (float)fieldMatchThreshold
                    );
                    
                    // Convert record to JObject for safe property assignment
                    JObject recordObj;
                    if (record is JObject jObj)
                    {
                        recordObj = jObj;
                    }
                    else
                    {
                        // Convert dynamic record to JObject
                        var recordJson = JsonConvert.SerializeObject(record, new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore
                        });
                        recordObj = JObject.Parse(recordJson);
                        records[i] = recordObj; // Replace the original record with JObject
                    }

                    // Add duplicate information to the record
                    if (duplicateResult != null && duplicateResult.HasDuplicates)
                    {
                        // Create TopDuplicate object
                        JObject topDuplicateObj = null;
                        if (duplicateResult.TopDuplicate != null)
                        {
                            topDuplicateObj = new JObject
                            {
                                ["entityId"] = duplicateResult.TopDuplicate.EntityId,
                                ["entityType"] = duplicateResult.TopDuplicate.EntityType,
                                ["score"] = duplicateResult.TopDuplicate.Score,
                                ["matchReason"] = duplicateResult.TopDuplicate.MatchReason,
                                ["searchType"] = duplicateResult.TopDuplicate.SearchType,
                                ["matchedData"] = duplicateResult.TopDuplicate.MatchedData != null ? 
                                    JToken.FromObject(duplicateResult.TopDuplicate.MatchedData) : null
                            };
                        }

                        recordObj["duplicateDetection"] = new JObject
                        {
                            ["hasDuplicates"] = true,
                            ["totalDuplicates"] = duplicateResult.TotalDuplicates,
                            ["highConfidence"] = duplicateResult.HighConfidence,
                            ["mediumConfidence"] = duplicateResult.MediumConfidence,
                            ["lowConfidence"] = duplicateResult.LowConfidence,
                            ["topDuplicate"] = topDuplicateObj
                        };
                    }
                    else
                    {
                        recordObj["duplicateDetection"] = new JObject
                        {
                            ["hasDuplicates"] = false,
                            ["totalDuplicates"] = 0,
                            ["highConfidence"] = 0,
                            ["mediumConfidence"] = 0,
                            ["lowConfidence"] = 0,
                            ["topDuplicate"] = null
                        };
                    }
                }

                return records;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error detecting duplicates: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Detects duplicates for a single record using the simplified field-based detection function
        /// </summary>
        /// <param name="entityName">Name of the entity type (pluralized)</param>
        /// <param name="recordData">The record data to check for duplicates</param>
        /// <param name="fieldMatchThreshold">Field matching threshold</param>
        /// <returns>Comprehensive duplicate detection result</returns>
        private async Task<ComprehensiveDuplicateResult> DetectDuplicateForRecordAsync(
            string entityName, 
            dynamic recordData, 
            float fieldMatchThreshold = 0.5f)
         {
             try
             {
                // Ensure entity name is singular for the SQL function
                var singularEntityName = entityName.Singularize();
                
                // Serialize the record data directly to JSON text
                string jsonData;
                try
                {
                    jsonData = JsonConvert.SerializeObject(recordData, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                }
                catch (JsonSerializationException ex)
                {
                    throw new Exception($"Failed to serialize record data to JSON: {ex.Message}. Record type: {recordData?.GetType()?.Name ?? "null"}", ex);
                }
                
                 var connection = _context.Database.GetDbConnection();
                 if (connection.State != ConnectionState.Open)
                     await connection.OpenAsync();

                 using var command = connection.CreateCommand();
                command.CommandText = "SELECT public.detect_duplicate_records(@entityType, @entityData, @fieldMatchThreshold, @debugMode)";

                // Create parameters for the simplified function call (entity_data as TEXT)
                 var parameters = new[] 
                 {
                    new NpgsqlParameter("@entityType", NpgsqlTypes.NpgsqlDbType.Text) { Value = singularEntityName },
                    new NpgsqlParameter("@entityData", NpgsqlTypes.NpgsqlDbType.Text) { Value = jsonData },
                    new NpgsqlParameter("@fieldMatchThreshold", NpgsqlTypes.NpgsqlDbType.Real) { Value = fieldMatchThreshold },
                    new NpgsqlParameter("@debugMode", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = false }
                 };

                 command.Parameters.AddRange(parameters);

                var result = await command.ExecuteScalarAsync();
                
                if (result != null && result != DBNull.Value)
                {
                    var jsonResult = result.ToString();
                    var parsedResult = JsonConvert.DeserializeObject<dynamic>(jsonResult);
                    
                    return new ComprehensiveDuplicateResult
                    {
                        HasDuplicates = parsedResult.duplicates != null && ((JArray)parsedResult.duplicates).Count > 0,
                        TotalDuplicates = parsedResult.summary?.totalDuplicates ?? 0,
                        HighConfidence = parsedResult.summary?.highConfidence ?? 0,
                        MediumConfidence = parsedResult.summary?.mediumConfidence ?? 0,
                        LowConfidence = parsedResult.summary?.lowConfidence ?? 0,
                        TopDuplicate = ((JArray)parsedResult.duplicates)?.Count > 0 ? 
                            JsonConvert.DeserializeObject<DuplicateMatch>(((JArray)parsedResult.duplicates)[0].ToString()) : null,
                        AllDuplicates = parsedResult.duplicates
                    };
                }

                return new ComprehensiveDuplicateResult
                {
                    HasDuplicates = false,
                    TotalDuplicates = 0,
                    HighConfidence = 0,
                    MediumConfidence = 0,
                    LowConfidence = 0,
                    TopDuplicate = null,
                    AllDuplicates = null
                };
             }
             catch (Exception ex)
             {
                throw new Exception($"Error detecting duplicates for record: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts a request object to the format expected by duplicate detection (camelCase, simplified)
        /// </summary>
        /// <param name="requestObject">The request object to convert</param>
        /// <returns>Simplified object with camelCase properties</returns>
        private object ConvertRequestObjectForDuplicateDetection(object requestObject)
        {
            if (requestObject == null) return null;

            try
            {
                // First serialize with camelCase naming policy to convert PascalCase to camelCase
                var camelCaseJson = JsonConvert.SerializeObject(requestObject, new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });

                // Deserialize to JObject for manipulation
                var jObject = JObject.Parse(camelCaseJson);

                // Remove complex nested objects that aren't needed for duplicate detection
                jObject.Remove("extensions");
                jObject.Remove("confirmDuplicateCreation");
                
                // Convert back to a simple object
                return jObject.ToObject<Dictionary<string, object>>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert request object for duplicate detection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Detects duplicates for a single record using field-based similarity matching
        /// </summary>
        /// <param name="entityName">Name of the entity type (e.g., "Contact", "Partner", "Interaction")</param>
        /// <param name="recordData">The record data to check for duplicates</param>
        /// <param name="fieldMatchThreshold">Field matching threshold (default: 0.5)</param>
        /// <returns>Comprehensive duplicate detection result</returns>
        public async Task<ComprehensiveDuplicateResult> DetectDuplicateForSingleRecordAsync(
            string entityName, 
            dynamic recordData, 
            double fieldMatchThreshold = 0.5)
        {
            try
            {
                // Ensure entity name is pluralized for consistency
                var pluralizedEntityName = entityName.Pluralize();
                
                // Convert the request object to the format expected by duplicate detection
                var convertedData = ConvertRequestObjectForDuplicateDetection(recordData);
                
                return await DetectDuplicateForRecordAsync(
                    pluralizedEntityName, 
                    convertedData, 
                    (float)fieldMatchThreshold
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error detecting duplicate for single record: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Detects duplicate records within the uploaded file data itself (before checking database)
        /// </summary>
        /// <param name="entityName">Name of the entity type</param>
        /// <param name="records">List of records from the uploaded file</param>
        /// <param name="fieldMatchThreshold">Threshold for field matching (0.0 to 1.0)</param>
        /// <returns>Internal duplicate detection result</returns>
        public async Task<InternalDuplicateResult> DetectInternalDuplicatesAsync(
            string entityName, 
            List<dynamic> records, 
            double fieldMatchThreshold = 0.8)
        {
            try
            {
                var duplicateGroups = new List<InternalDuplicateGroup>();
                var processedIndices = new HashSet<int>();

                // Compare each record with every other record
                for (int i = 0; i < records.Count; i++)
                {
                    if (processedIndices.Contains(i)) continue;

                    var currentRecord = records[i];
                    var duplicateGroup = new InternalDuplicateGroup
                    {
                        MasterIndex = i,
                        MasterRecord = currentRecord,
                        DuplicateIndices = new List<int>(),
                        DuplicateRecords = new List<dynamic>(),
                        MatchReasons = new List<string>()
                    };

                    // Compare with remaining records
                    for (int j = i + 1; j < records.Count; j++)
                    {
                        if (processedIndices.Contains(j)) continue;

                        var compareRecord = records[j];
                        var matchResult = CompareRecordsForInternalDuplicates(entityName, currentRecord, compareRecord, fieldMatchThreshold);

                        if (matchResult.IsMatch)
                        {
                            duplicateGroup.DuplicateIndices.Add(j);
                            duplicateGroup.DuplicateRecords.Add(compareRecord);
                            duplicateGroup.MatchReasons.Add(matchResult.MatchReason);
                            processedIndices.Add(j);
                        }
                    }

                    // Only add to duplicateGroups if we found duplicates
                    if (duplicateGroup.DuplicateIndices.Count > 0)
                    {
                        processedIndices.Add(i);
                        duplicateGroups.Add(duplicateGroup);
                    }
                }

                return new InternalDuplicateResult
                {
                    HasInternalDuplicates = duplicateGroups.Count > 0,
                    TotalDuplicateGroups = duplicateGroups.Count,
                    TotalDuplicateRecords = duplicateGroups.Sum(g => g.DuplicateIndices.Count),
                    DuplicateGroups = duplicateGroups,
                    TotalRecords = records.Count,
                    CleanRecords = records.Count - duplicateGroups.Sum(g => g.DuplicateIndices.Count + 1) // +1 for master record
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error detecting internal duplicates for {entityName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Compares two records to determine if they are internal duplicates
        /// </summary>
        private InternalMatchResult CompareRecordsForInternalDuplicates(string entityName, dynamic record1, dynamic record2, double threshold)
        {
            try
            {
                var matchReasons = new List<string>();
                var matchScore = 0.0;
                var totalFields = 0;

                // Convert to JObjects for easier property access
                var obj1 = JObject.FromObject(record1);
                var obj2 = JObject.FromObject(record2);

                // Define key fields to compare based on entity type
                var keyFields = GetKeyFieldsForEntity(entityName);

                foreach (var field in keyFields)
                {
                    var value1 = obj1[field]?.ToString()?.Trim();
                    var value2 = obj2[field]?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(value1) || string.IsNullOrEmpty(value2))
                        continue;

                    totalFields++;

                    // Exact match
                    if (string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase))
                    {
                        matchScore += 1.0;
                        matchReasons.Add($"Exact {field} match");
                    }
                    // Fuzzy match for text fields
                    else if (field.ToLower().Contains("name") || field.ToLower().Contains("title") || field.ToLower().Contains("subject"))
                    {
                        var similarity = CalculateStringSimilarity(value1, value2);
                        if (similarity >= 0.85) // High similarity threshold for internal duplicates
                        {
                            matchScore += similarity;
                            matchReasons.Add($"Similar {field} ({(similarity * 100):F0}% match)");
                        }
                    }
                }

                if (totalFields == 0)
                {
                    return new InternalMatchResult { IsMatch = false, MatchReason = "No comparable fields found" };
                }

                var finalScore = matchScore / totalFields;
                var isMatch = finalScore >= threshold;

                return new InternalMatchResult
                {
                    IsMatch = isMatch,
                    Score = finalScore,
                    MatchReason = isMatch ? string.Join(", ", matchReasons) : "No significant matches"
                };
            }
            catch (Exception ex)
            {
                return new InternalMatchResult { IsMatch = false, MatchReason = $"Error comparing records: {ex.Message}" };
            }
        }

        /// <summary>
        /// Gets key fields to compare for internal duplicate detection based on entity type
        /// </summary>
        private List<string> GetKeyFieldsForEntity(string entityName)
        {
            return entityName.ToLower() switch
            {
                "contact" or "contacts" => new List<string> { "email", "firstName", "lastName", "phone", "mobile" },
                "partner" or "partners" => new List<string> { "name", "partnerShortDescription", "erpDimValue" },
                "interaction" or "interactions" => new List<string> { "type", "subject", "date", "description" },
                _ => new List<string> { "name", "title", "email" } // Default fields
            };
        }

        /// <summary>
        /// Calculates string similarity using a simple algorithm
        /// </summary>
        private double CalculateStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0.0;

            str1 = str1.ToLowerInvariant();
            str2 = str2.ToLowerInvariant();

            if (str1 == str2) return 1.0;

            // Simple Levenshtein distance-based similarity
            var maxLen = Math.Max(str1.Length, str2.Length);
            if (maxLen == 0) return 1.0;

            var distance = LevenshteinDistance(str1, str2);
            return 1.0 - (double)distance / maxLen;
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings
        /// </summary>
        private int LevenshteinDistance(string str1, string str2)
        {
            var matrix = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= str2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[str1.Length, str2.Length];
        }
    }

    /// <summary>
    /// Comprehensive result of duplicate detection
    /// </summary>
    public class ComprehensiveDuplicateResult
    {
        public bool HasDuplicates { get; set; }
        public int TotalDuplicates { get; set; }
        public int HighConfidence { get; set; }
        public int MediumConfidence { get; set; }
        public int LowConfidence { get; set; }
        public DuplicateMatch TopDuplicate { get; set; }
        public dynamic AllDuplicates { get; set; }
    }

    /// <summary>
    /// Individual duplicate match details
    /// </summary>
    public class DuplicateMatch
    {
        public int EntityId { get; set; }
        public string EntityType { get; set; }
        public double Score { get; set; }
        public string MatchReason { get; set; }
        public dynamic MatchedData { get; set; }
        public string SearchType { get; set; }
    }

    /// <summary>
    /// Result of internal duplicate detection within uploaded file
    /// </summary>
    public class InternalDuplicateResult
    {
        public bool HasInternalDuplicates { get; set; }
        public int TotalDuplicateGroups { get; set; }
        public int TotalDuplicateRecords { get; set; }
        public int TotalRecords { get; set; }
        public int CleanRecords { get; set; }
        public List<InternalDuplicateGroup> DuplicateGroups { get; set; } = new List<InternalDuplicateGroup>();
    }

    /// <summary>
    /// Represents a group of duplicate records within the file
    /// </summary>
    public class InternalDuplicateGroup
    {
        public int MasterIndex { get; set; }
        public dynamic MasterRecord { get; set; }
        public List<int> DuplicateIndices { get; set; } = new List<int>();
        public List<dynamic> DuplicateRecords { get; set; } = new List<dynamic>();
        public List<string> MatchReasons { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result of comparing two records for internal duplicates
    /// </summary>
    public class InternalMatchResult
    {
        public bool IsMatch { get; set; }
        public double Score { get; set; }
        public string MatchReason { get; set; } = string.Empty;
    }
}