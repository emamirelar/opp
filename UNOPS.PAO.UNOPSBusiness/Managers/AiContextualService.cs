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
            // Create the request instance
            // Create structured instance in JSON format
            var instance = new Value
            {
                StructValue = new Struct
                {
                    Fields = { { "content", Value.ForString(text) } }
                }
            };
            var request = new PredictRequest
            {
                Endpoint = _endpoint,
                Instances = { instance }
            };

            // Call Vertex AI using gRPC
            PredictResponse response = await _predictionClient.PredictAsync(request);

            var structValue = response.Predictions[0].StructValue;

            // Navigate to the "values" inside "embeddings"
            var embeddingValues = structValue.Fields["embeddings"].StructValue.Fields["values"].ListValue.Values;

            // Convert to float array
            float[] embedding = embeddingValues.Select(v => (float)v.NumberValue).ToArray();

            string vectorString = "[" + string.Join(",", embedding.Select(f => f.ToString(CultureInfo.InvariantCulture))) + "]";

            return vectorString;
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
                var embeddingResult = await ExecuteEmbeddingSearch(entityName, vectorEmbedding, embeddingThreshold, where);
                return embeddingResult;
            }
            
            // Step 3: If we have search text but no embedding, generate embedding and search
            if (!string.IsNullOrEmpty(searchText))
            {
                var generatedEmbedding = await CreateEmbeddingForText(searchText);
                if (!string.IsNullOrEmpty(generatedEmbedding))
                {
                    var embeddingResult = await ExecuteEmbeddingSearch(entityName, generatedEmbedding, embeddingThreshold, where);
                    
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
            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credentials,
                ApplicationName = "GoogleSheetsReader",
            });

            // Read values
            var request = service.Spreadsheets.Values.Get(fileId, "Sheet1");
            ValueRange response = await request.ExecuteAsync();
            var data = string.Empty;

            if (response.Values != null && response.Values.Count > 0)
            {
                // Convert response.Values to a stringified array
                data = JsonConvert.SerializeObject(response.Values);
            }

            return data;
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

            // Define batch size
            int batchSize = 25;
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
            Func<int, int, List<dynamic>, Task<bool>> progressCallback = null)
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

            // Define batch size
            int batchSize = 25;
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
                    //var detailedRawMessage = JsonConvert.DeserializeObject(detailedResponse.RawMessage);
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
                                        }
                                        else
                                        {
                                            // Convert text to entity ID
                                            var entityId = await GetEntityIdFromText(textValue, dependent);
                                            if (entityId != null && !(entityId is DBNull))
                                            {
                                                idsArray.Add(entityId);
                                                
                                                // Handle email lookup for interaction types
                                                if (interactionType && (dependent == "contactIds" || dependent == "userIds"))
                                                {
                                                    await AddEmailToResponse(responseObject, entityId);
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
                                if (text is long longValue || text is int intValue)
                                {
                                    // It is already an ID, just continue
                                    continue;
                                }
                                else if (int.TryParse(text?.ToString(), out id))
                                {
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
                                
                                // Handle email lookup for interaction types
                                if (interactionType && (dependent == "contactId" || dependent == "userId"))
                                {
                                    await AddEmailToResponse(responseObject, entityId);
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
            if (dependent.Equals("organizationUnitRelationships", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "OrganizationHierarchies";
                whereCondition = "\"Type\" = 'OrgUnit'"; // OrgUnit enum value stored as string
            }
            // Special case for Orgunit - should look at OrganizationHierarchies table
            else if (entityName.Equals("Orgunit", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "OrganizationHierarchies";
                whereCondition = "\"Type\" = 'OrgUnit'"; // OrgUnit enum value stored as string
            }
            // Special case for User/UserIds - should look at UserProfile table (which has searchable Name field)
            else if (entityName.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "UserProfile";
                // UserProfile can be searched by Name field directly
                whereCondition = "1=1"; // Allow all UserProfiles to be searched
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
            
            // If not found in Contacts, try UserInfos table
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
    }
}