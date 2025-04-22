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

namespace UNOPS.PAO.UNOPSBusiness.Managers
{
    public class AiContextualService
    {
        private readonly PredictionServiceClient _predictionClient;
        private readonly string _endpoint;
        private readonly IConfiguration _configuration;
        public readonly UNOPSAppDbContext _context;
        private readonly DataRepository<AiScreenMapping> _screenMappingRepository;
        private readonly string _connectionString;
        private readonly DataRepository<AiPrompt> _promptRepository;
        private readonly GoogleCredential _credentials;
        protected readonly PubSubPublisher _pubSubPublisher;

        public AiContextualService(IConfiguration configuration, UNOPSAppDbContext context, GoogleCredential credentials)
        {
            _configuration = configuration;
            _screenMappingRepository = new DataRepository<AiScreenMapping>(context);
            var projectId = _configuration.GetValue<string>("AISettings:ProjectId");
            var location = _configuration.GetValue<string>("AISettings:Location");
            var model = _configuration.GetValue<string>("AISettings:EmbeddingModelName");
            _endpoint = $"projects/{projectId}/locations/{location}/publishers/google/models/{model}";
            _predictionClient = PredictionServiceClient.Create(); // gRPC Client
            _context = context;
            _connectionString = configuration.GetValue<string>("ConnectionStrings:DbSchema");
            _credentials = credentials;
            _promptRepository = new DataRepository<AiPrompt>(context);
            _pubSubPublisher = new PubSubPublisher(configuration);
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

        public async Task PersistEmbedding(string entityName, int entityId, string vectorString)
        {
            var sql = "CALL public.\"InsertEntityEmbedding\"(@entityName, @entityId, @embedding)";

            var parameters = new[] 
            {
                new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
                new NpgsqlParameter("@entityId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = entityId },
                new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = vectorString }
            };

            // Execute the stored procedure using ExecuteSqlRaw
            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public async Task GenerateEmbeddingAsync(string entityName, int entityId, string text)
        {
            var vectorString = await CreateEmbeddingForText(text);
            await PersistEmbedding(entityName, entityId, vectorString);
        }

        public async Task<string> RetrieveContent(string promptType, dynamic entityId)
        {
            var content = "No content found";

            if (entityId is not int)
                return content;

            // Query the AiScreenMapping table based on Type
            var screenMappings = (await GetScreenMappingsByType(promptType)).ToArray();
            content = await GetDataBasedOnScreenMapping(promptType, (int)entityId, screenMappings);

            return content;
        }

        public async Task<dynamic> RetrieveEntityId(string entityName, string vectorEmbedding)
        {
            var sql = "SELECT public.RetrieveSimilarityId(@entityName, @embedding)";

            entityName = entityName.Pluralize();

            var parameters = new[] 
            {
                new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
                new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = vectorEmbedding }
            };

            // Execute the stored procedure using ExecuteSqlRaw
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();

            return result;
        }

        public async Task<dynamic> RetrieveSimilarityIds(string entityName, string similarityCriteria, string vectorEmbedding=null, int limitCount=5, string whereCondition=null)
        {
            var sql = "SELECT public.Retrieve_Similarity_Results(@entityName, @text, @embedding, @limit, @where)";

            entityName = entityName.Pluralize();

            var parameters = new[] 
            {
                new NpgsqlParameter("@entityName", NpgsqlTypes.NpgsqlDbType.Text) { Value = entityName },
                new NpgsqlParameter("@embedding", NpgsqlTypes.NpgsqlDbType.Text) { Value = vectorEmbedding },
                new NpgsqlParameter("@text", NpgsqlTypes.NpgsqlDbType.Text) { Value = similarityCriteria },
                new NpgsqlParameter("@limit", NpgsqlTypes.NpgsqlDbType.Integer) { Value = limitCount },
                new NpgsqlParameter("@where", NpgsqlTypes.NpgsqlDbType.Text) { Value = whereCondition }
            };

            // Execute the stored procedure using ExecuteSqlRaw
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();

            return result;
        }

        public async Task<IEnumerable<AiScreenMapping>> GetScreenMappingsByType(string type)
        {
            return await _screenMappingRepository
                .GetAll()
                .Where(x => x.Type == type)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<string> GetDataBasedOnScreenMapping(string type, object recordId, AiScreenMapping[] mappings)
        {
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

            List<int> recordIds = new List<int>();

            if (recordId is int id)
            {
                recordIds = new List<int> { id };
            }
            else if (recordId is IEnumerable<int> idList)
            {
                recordIds = idList.ToList();
            }

            string sqlQuery = $@"
                SELECT ROW_TO_JSON(t)
                FROM (
                    SELECT {string.Join(", ", selectColumns)}
                    FROM {baseTable}
                    {string.Join(" ", joinClauses)}
                    WHERE {baseTableKeyCheck} IN ({string.Join(", ", recordIds)})
                ) t;";

            var result = await ExecuteSqlQuery(sqlQuery);
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

            if (!selectColumns.Any(col => col.Contains(tableWithSchema)) && (tableRecord?.ClrType != null && !(tablePropetiesAsList.Count == 2 && foreignKeys.Count == 2)))
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

        private async Task<List<Dictionary<string, object>>> ExecuteSqlQuery(string sqlQuery)
        {
            var result = new List<Dictionary<string, object>>();
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sqlQuery;
                command.CommandType = CommandType.Text;

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
            return result;
        }

    // Build nested JSON from result and mappings
    private object BuildNestedJson(List<Dictionary<string, object>> result, AiScreenMapping[] mappings)
    {
        // Implement the logic to build nested JSON from the result and mappings
        // This is a placeholder implementation
        return result;
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

        public async Task<IEnumerable<AiPromptModel>> GetPromptData(string type)
        {
            var prompts = await _promptRepository
                .GetAll()
                .Where(x => x.Type == type)
                .ToListAsync();

        return prompts.Select(entity => new AiPromptModel
        {
            Type = entity.Type,
            Prompt = entity.Prompt ?? string.Empty, // Ensure null safety
            ContentConfig = entity.ContentConfig,
            GenerationConfig = entity.GenerationConfig,
            ToolsConfig = entity.ToolsConfig,
            SafetySettings = entity.SafetySettings,
            Location = entity.Location,
            Project = entity.Project,
            Model = entity.Model
        }).ToList();
    }

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

    // Common function to handle Gemini API calls
    public async Task<string> CallGeminiApi(dynamic prompt, AiPromptModel promptData)
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

        private async Task<string> GetURL(AiPromptModel promptData)
        {
            return $"https://{promptData.Location}-aiplatform.googleapis.com/v1/projects/{promptData.Project}/locations/{promptData.Location}/publishers/google/models/{promptData.Model}:generateContent";
        }

        public async Task PublishMessageToPubSub(MyPubSubMessage message)
        {
            // Publishing the message
            await _pubSubPublisher.PublishMessageAsync(new List<MyPubSubMessage> { message });
        }

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

        public async Task<List<dynamic>> ProcessBulkImport(string stringifiedBatchRecords, AiPromptModel promptData, int userId, string entityName, bool isAsync = false)
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
                        dynamic updatedResponse = await GetDependentDropdownValues(dependents, record);
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
            AiPromptModel promptData, 
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
                        dynamic updatedResponse = await GetDependentDropdownValues(dependents, record);
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

        public async Task<dynamic> GetDependentDropdownValues(dynamic dependents, dynamic responseObject)
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
                            }
                            var embeddingString = await CreateEmbeddingForText(text);
                            string entityName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dependent.Replace("Id", ""));
                            entityName = entityName.Pluralize();
                            entityId = await RetrieveEntityId(entityName, embeddingString);

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
    }
}