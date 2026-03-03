using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using UNOPS.PAO.Models.AI;

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
        private readonly PredictionServiceClient? _predictionClient;
        private readonly string _endpoint;
        private readonly IConfiguration _configuration;
        public readonly UNOPSAppDbContext _context;
        private readonly DataRepository<AiPrompt> _promptRepository;
        private readonly GoogleCredential _credentials;
        protected readonly PubSubPublisher _pubSubPublisher;
        private readonly string _connectionString;
        private readonly IAiPromptCacheService _aiPromptCacheService;
        private readonly ILogger _logger;
        private readonly bool _disableExternalCalls;

        public AiContextualService(
            IConfiguration configuration,
            UNOPSAppDbContext context,
            GoogleCredential credentials,
            IAiPromptCacheService aiPromptCacheService = null,
            ILogger logger = null)
        {
            _configuration = configuration;
            _context = context;
            _connectionString = configuration.GetValue<string>("ConnectionStrings:DbSchema");
            _credentials = credentials;
            _promptRepository = new DataRepository<AiPrompt>(context);
            _pubSubPublisher = new PubSubPublisher(configuration);
            _aiPromptCacheService = aiPromptCacheService; // Optional dependency for backward compatibility
            _logger = logger; // Optional logger for keyword generation
            _disableExternalCalls = configuration.GetValue<bool>("AISettings:DisableExternalCalls") ||
                string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Testing", StringComparison.OrdinalIgnoreCase);
            var projectId = _configuration.GetValue<string>("AISettings:ProjectId");
            var location = _configuration.GetValue<string>("AISettings:Location");
            var model = _configuration.GetValue<string>("AISettings:EmbeddingModelName");
            _endpoint = $"projects/{projectId}/locations/{location}/publishers/google/models/{model}";
            _predictionClient = _disableExternalCalls ? null : PredictionServiceClient.Create(); // gRPC Client
        }

        /// <summary>
        /// Replaces placeholders in text with actual values from JSON data
        /// </summary>
        /// <param name="text">Text containing placeholders like {partnerName}, {userInfo}</param>
        /// <param name="jsonData">JSON data containing the replacement values</param>
        /// <returns>Text with placeholders replaced</returns>
        public string ProcessPlaceholders(string text, string jsonData)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(jsonData))
                return text ?? string.Empty;

            try
            {
                var dataObject = JsonConvert.DeserializeObject<JObject>(jsonData);
                var result = text;

                if (result == "{promptData}")
                {
                    return jsonData;
                }
                
                // Find all placeholders in format {propertyName} or {object.property}
                // Only replace when the inner part looks like a property path (alphanumeric, dots, underscores)
                // so that JSON examples in prompts (e.g. { "isAligned": true }) are not corrupted
                var placeholderPattern = @"\{([^}]+)\}";
                var matches = Regex.Matches(text, placeholderPattern);
                var simplePathPattern = new Regex(@"^[a-zA-Z0-9_.]+$");

                foreach (Match match in matches)
                {
                    var placeholder = match.Value; // e.g., "{partner.name}"
                    var propertyPath = match.Groups[1].Value.Trim();
                    if (!simplePathPattern.IsMatch(propertyPath))
                        continue; // Skip JSON-like content (e.g. " \"isAligned\": true")

                    var value = GetNestedPropertyValue(dataObject, propertyPath);

                    if (value != null)
                    {
                        result = result.Replace(placeholder, value);
                    }
                    else
                    {
                        // Log warning for unresolved placeholders but don't fail
                        Console.WriteLine($"[WARNING] Placeholder '{placeholder}' not found in JSON data");
                        // Replace with empty string to avoid showing placeholder in output
                        result = result.Replace(placeholder, "");
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing placeholders: {ex.Message}");
                return text; // Return original text if processing fails
            }
        }

        /// <summary>
        /// Gets nested property value from JObject using dot notation (e.g., "partner.name")
        /// </summary>
        private string GetNestedPropertyValue(JObject dataObject, string propertyPath)
        {
            try
            {
                var pathParts = propertyPath.Split('.');
                JToken current = dataObject;
                
                foreach (var part in pathParts)
                {
                    if (current == null) return null;
                    
                    // Handle arrays - if current is an array, try to get first element
                    if (current is JArray array && array.Count > 0)
                    {
                        current = array[0];
                    }
                    
                    // Look for property (case-insensitive)
                    if (current is JObject obj)
                    {
                        var property = obj.Properties()
                            .FirstOrDefault(p => string.Equals(p.Name, part, StringComparison.OrdinalIgnoreCase));
                        
                        if (property != null)
                        {
                            current = property.Value;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                
                // Handle the final value
                if (current != null)
                {
                    // If it's an object or array, serialize it as JSON
                    if (current is JObject || current is JArray)
                    {
                        return current.ToString(Formatting.None);
                    }
                    else
                    {
                        return current.ToString();
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting nested property '{propertyPath}': {ex.Message}");
                return null;
            }
        }

        public async Task<string> CreateEmbeddingForText(string text)
        {
            if (_disableExternalCalls)
            {
                return string.Empty;
            }

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
            if (entityName != "UserProfile")
            {
                entityName = entityName.Pluralize();
            }

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

        public async Task<string> ReadFileData(string fileId, string sheetName = null)
        {
            try
            {
                var service = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = _credentials,
                    ApplicationName = "GoogleSheetsReader",
                });
                var spreadsheet = service.Spreadsheets.Get(fileId).Execute();
                var firstSheetName = sheetName ?? spreadsheet.Sheets[0].Properties.Title;

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
            SystemInstructions = entity.SystemInstructions ?? string.Empty, 
            UserPrompt = entity.UserPrompt,
            ContentConfig = entity.ContentConfig,
            GenerationConfig = entity.GenerationConfig,
            ToolsConfig = entity.ToolsConfig,
            SafetySettings = entity.SafetySettings,
            Location = entity.Location,
            Project = entity.Project,
            Model = entity.Model,
            DataRetrievalMethod = entity.DataRetrievalMethod, 
            Name = entity.Name,
            Feature = entity.Feature,
            UseCache = entity.UseCache,
            CacheInvalidationMinutes = entity.CacheInvalidationMinutes
        }).ToList();
    }

    public async Task<string> FetchResultFromGemini(AiPrompt promptData, string relatedJsonData, string entityId = null, bool bypassCache = false)
    {
        try
        {
            // Step 1: Process placeholders to create fully formed instructions/prompts
            // Use new SystemInstructions field
            var systemInstructionsTemplate = promptData.SystemInstructions ?? string.Empty;
                
            var fullyFormedSystemInstructions = ProcessPlaceholders(systemInstructionsTemplate, relatedJsonData);
            
            var fullyFormedUserPrompt = !string.IsNullOrEmpty(promptData.UserPrompt) 
                ? ProcessPlaceholders(promptData.UserPrompt, relatedJsonData)
                : relatedJsonData; // Default to raw data if no user prompt specified
            
            // Step 2: Check cache if enabled and not bypassed
            if (!bypassCache && promptData.UseCache && !string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(promptData.Type) && _aiPromptCacheService != null)
            {
                var cachedEntry = await _aiPromptCacheService.GetCachedEntryAsync(promptData.Type, entityId);
                if (cachedEntry != null)
                {
                    // Check if the current fully formed instructions/prompts match the cached ones
                    if (cachedEntry.SystemInstructions == fullyFormedSystemInstructions && 
                        cachedEntry.UserPrompt == fullyFormedUserPrompt)
                    {
                        Console.WriteLine($"[CACHE HIT] Returning cached result for prompt {promptData.Type}, entity {entityId}");
                        return cachedEntry.Result;
                    }
                    else
                    {
                        Console.WriteLine($"[CACHE MISS] Instructions/prompts changed for prompt {promptData.Type}, entity {entityId}");
                        // Instructions/prompts have changed, invalidate cache entry
                        await _aiPromptCacheService.InvalidateCache(promptData.Type, entityId);
                    }
                }
            }
            
            // Step 3: Call Gemini API with fully formed content
            var userContent = new
            {
                role = "user",
                parts = new[] { new { text = fullyFormedUserPrompt } }
            };
            
            var result = await CallGeminiApi(userContent, promptData, fullyFormedSystemInstructions);
            
            // Step 4: Cache the result if caching is enabled and not bypassed
            if (!bypassCache && promptData.UseCache && !string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(promptData.Type) && _aiPromptCacheService != null)
            {
                await _aiPromptCacheService.SetCachedResultAsync(
                    promptData.Type, 
                    entityId, 
                    fullyFormedSystemInstructions,
                    fullyFormedUserPrompt, 
                    result, 
                    promptData.CacheInvalidationMinutes);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in FetchResultFromGemini: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Fetch result from Gemini with document file (using gs:// URI)
    /// </summary>
    public async Task<string> FetchResultFromGeminiWithDocument(
        AiPrompt promptData, 
        string relatedJsonData, 
        string documentStoragePath, 
        string documentMimeType,
        string entityId = null, 
        bool bypassCache = false)
    {
        try
        {
            // Step 1: Process placeholders to create fully formed instructions/prompts
            var systemInstructionsTemplate = promptData.SystemInstructions ?? string.Empty;
            var fullyFormedSystemInstructions = ProcessPlaceholders(systemInstructionsTemplate, relatedJsonData);
            
            var fullyFormedUserPrompt = !string.IsNullOrEmpty(promptData.UserPrompt) 
                ? ProcessPlaceholders(promptData.UserPrompt, relatedJsonData)
                : "Please analyze this document and extract the requested information.";
            
            // Step 2: Check cache if enabled and not bypassed
            if (!bypassCache && promptData.UseCache && !string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(promptData.Type) && _aiPromptCacheService != null)
            {
                var cachedEntry = await _aiPromptCacheService.GetCachedEntryAsync(promptData.Type, entityId);
                if (cachedEntry != null)
                {
                    Console.WriteLine($"[CACHE HIT] Returning cached result for document prompt {promptData.Type}, entity {entityId}");
                    return cachedEntry.Result;
                }
            }
            
            // Step 3: Call Gemini API with document URI
            // Build parts array with both text and fileData
            var parts = new List<object>
            {
                new { text = fullyFormedUserPrompt }
            };

            // Add document URI if provided (using Gemini REST API format)
            if (!string.IsNullOrEmpty(documentStoragePath) && documentStoragePath.StartsWith("gs://"))
            {
                parts.Add(new 
                { 
                    fileData = new
                    {
                        fileUri = documentStoragePath,
                        mimeType = documentMimeType
                    }
                });
            }

            var userContent = new
            {
                role = "user",
                parts = parts.ToArray()
            };
            
            var result = await CallGeminiApi(userContent, promptData, fullyFormedSystemInstructions);
            
            // Step 4: Cache the result if caching is enabled and not bypassed
            if (!bypassCache && promptData.UseCache && !string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(promptData.Type) && _aiPromptCacheService != null)
            {
                await _aiPromptCacheService.SetCachedResultAsync(
                    promptData.Type, 
                    entityId, 
                    fullyFormedSystemInstructions,
                    fullyFormedUserPrompt + " [with document: " + documentStoragePath + "]", 
                    result, 
                    promptData.CacheInvalidationMinutes);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in FetchResultFromGeminiWithDocument: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Fetches AI result from Gemini with MULTIPLE documents attached as file URIs.
    /// Used for extracting products/services from multiple Partner Results Framework documents.
    /// </summary>
    public async Task<string> FetchResultFromGeminiWithMultipleDocuments(
        AiPrompt promptData, 
        string relatedJsonData, 
        List<(string storagePath, string mimeType)> documents,
        string entityId = null, 
        bool bypassCache = false)
    {
        try
        {
            // Step 1: Process placeholders to create fully formed instructions/prompts
            var systemInstructionsTemplate = promptData.SystemInstructions ?? string.Empty;
            var fullyFormedSystemInstructions = ProcessPlaceholders(systemInstructionsTemplate, relatedJsonData);
            
            var fullyFormedUserPrompt = !string.IsNullOrEmpty(promptData.UserPrompt) 
                ? ProcessPlaceholders(promptData.UserPrompt, relatedJsonData)
                : "Please analyze these documents and extract the requested information.";
            
            // Step 2: Check cache if enabled and not bypassed
            if (!bypassCache && promptData.UseCache && !string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(promptData.Type) && _aiPromptCacheService != null)
            {
                var cachedEntry = await _aiPromptCacheService.GetCachedEntryAsync(promptData.Type, entityId);
                if (cachedEntry != null)
                {
                    Console.WriteLine($"[CACHE HIT] Returning cached result for multi-document prompt {promptData.Type}, entity {entityId}");
                    return cachedEntry.Result;
                }
            }
            
            // Step 3: Build parts array with text prompt and ALL document URIs
            var parts = new List<object>
            {
                new { text = fullyFormedUserPrompt }
            };

            // Add ALL documents as fileData objects
            int validDocCount = 0;
            foreach (var (storagePath, mimeType) in documents)
            {
                if (!string.IsNullOrEmpty(storagePath) && storagePath.StartsWith("gs://"))
                {
                    parts.Add(new 
                    { 
                        fileData = new
                        {
                            fileUri = storagePath,
                            mimeType = mimeType
                        }
                    });
                    validDocCount++;
                }
            }

            Console.WriteLine($"[AI CALL] Calling Gemini with {validDocCount} documents attached");

            var userContent = new
            {
                role = "user",
                parts = parts.ToArray()
            };
            
            var result = await CallGeminiApi(userContent, promptData, fullyFormedSystemInstructions);
            
            // Step 4: Cache the result if caching is enabled and not bypassed
            if (!bypassCache && promptData.UseCache && !string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(promptData.Type) && _aiPromptCacheService != null)
            {
                var documentList = string.Join(", ", documents.Select(d => d.storagePath));
                await _aiPromptCacheService.SetCachedResultAsync(
                    promptData.Type, 
                    entityId, 
                    fullyFormedSystemInstructions,
                    fullyFormedUserPrompt + $" [with {validDocCount} documents]", 
                    result, 
                    promptData.CacheInvalidationMinutes);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in FetchResultFromGeminiWithMultipleDocuments: {ex.Message}", ex);
        }
    }

    // Common function to handle Gemini API calls
    public async Task<string> CallGeminiApi(dynamic prompt, AiPrompt promptData, string systemInstructions = null)
    {
        if (_disableExternalCalls)
        {
            return string.Empty;
        }

        string accessToken = await GetAccessTokenAsync();
        var requestBody = await GetRequestBody(prompt, promptData, systemInstructions);
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

        private async Task<string> GetAccessTokenAsync()
        {
            if (_disableExternalCalls)
            {
                return string.Empty;
            }

            GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
            credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
            return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        }

        private async Task<string> GetURL(AiPrompt promptData)
        {
            // Use project ID from environment configuration instead of database
            // This allows different environments (Dev, QA, Prod) to use their respective Google Cloud projects
            // without requiring database changes during deployment
            // Note: The Project property still exists in AiPrompt entity for backward compatibility and potential future use
            var projectId = _configuration.GetValue<string>("AISettings:ProjectId");
            return $"https://{promptData.Location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{promptData.Location}/publishers/google/models/{promptData.Model}:generateContent";
        }

        public async Task PublishMessageToPubSub(MyPubSubMessage message)
        {
            // Publishing the message
            await _pubSubPublisher.PublishMessageAsync(new List<MyPubSubMessage> { message });
        }

        public async Task<dynamic> GetRequestBody(dynamic prompt, AiPrompt promptData, string systemInstructions = null)
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

            // Handle user content
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
                contents = new[] { contentConfig }, // Wrap in array for proper format
                system_instruction = !string.IsNullOrEmpty(systemInstructions) 
                    ? new { parts = new[] { new { text = systemInstructions } } }
                    : null, // Add system instructions if provided
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
                    Message = "File Analysis Complete",
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
            string fileId = null,
            int? notificationId = null)
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
                
                // Add internal duplicate warning to each record if found
                if (internalDuplicateResult.HasInternalDuplicates)
                {
                    // Add internal duplicate information to affected records
                    foreach (var group in internalDuplicateResult.DuplicateGroups)
                    {
                        // Mark master record
                        if (group.MasterIndex < recordsList.Count)
                        {
                            var masterRecord = recordsList[group.MasterIndex];
                            if (masterRecord is JObject masterObj)
                            {
                                masterObj["internalDuplicateWarning"] = new JObject
                                {
                                    ["isMaster"] = true,
                                    ["duplicateCount"] = group.DuplicateIndices.Count,
                                    ["duplicateRows"] = JArray.FromObject(group.DuplicateIndices.Select(idx => idx + 2).ToList()),
                                    ["message"] = $"This record has {group.DuplicateIndices.Count} duplicate(s) in rows {string.Join(", ", group.DuplicateIndices.Select(idx => idx + 2))}"
                                };
                            }
                        }
                        
                        // Mark duplicate records
                        foreach (var duplicateIndex in group.DuplicateIndices)
                        {
                            if (duplicateIndex < recordsList.Count)
                            {
                                var duplicateRecord = recordsList[duplicateIndex];
                                if (duplicateRecord is JObject duplicateObj)
                                {
                                    duplicateObj["internalDuplicateWarning"] = new JObject
                                    {
                                        ["isMaster"] = false,
                                        ["masterRow"] = group.MasterIndex + 2,
                                        ["matchReasons"] = JArray.FromObject(group.MatchReasons),
                                        ["message"] = $"This record is a duplicate of the record in row {group.MasterIndex + 2}"
                                    };
                                }
                            }
                        }
                    }
                }
                
                // Always proceed with database duplicate detection
                var recordsWithDuplicates = await DetectDuplicatesAsync(entityName, recordsList, 0.65);
                finalResponse = recordsWithDuplicates.Select(r => (object)r).ToList();
            }

            if (isAsync)
            {
                // Create a single notification for the entire batch
                var hasInternalDuplicates = finalResponse.Any(r => r is JObject obj && obj["internalDuplicateWarning"] != null);
                
                // Check if any database duplicates were found
                var hasDatabaseDuplicates = finalResponse.Any(r => 
                    r is JObject obj && 
                    obj["duplicateDetection"] is JObject dupDetection &&
                    dupDetection["hasDuplicates"]?.Value<bool>() == true &&
                    dupDetection["totalDuplicates"]?.Value<int>() > 0
                );
                
                // Determine appropriate message based on duplicate detection results
                string successMessage;
                if (!string.IsNullOrEmpty(fileId))
                {
                    if (hasInternalDuplicates)
                    {
                        successMessage = $"File Analysis Complete with warnings - Internal duplicates found in file (Sheet ID: {fileId}). Please review and fix duplicates.";
                    }
                    else if (hasDatabaseDuplicates)
                    {
                        successMessage = $"File Analysis Complete with duplicate detection (Sheet ID: {fileId})";
                    }
                    else
                    {
                        successMessage = $"File Analysis Complete (Sheet ID: {fileId})";
                    }
                }
                else
                {
                    if (hasInternalDuplicates)
                    {
                        successMessage = "File Analysis Complete with warnings - Internal duplicates found in file. Please review and fix duplicates.";
                    }
                    else if (hasDatabaseDuplicates)
                    {
                        successMessage = "File Analysis Complete with duplicate detection";
                    }
                    else
                    {
                        successMessage = "File Analysis Complete";
                    }
                }
                
                // Update existing notification if ID provided, otherwise create new one
                if (notificationId.HasValue)
                {
                    var existingNotification = await _context.Notifications.FindAsync(notificationId.Value);
                    if (existingNotification != null)
                    {
                        existingNotification.Message = successMessage;
                        existingNotification.ResponseType = hasInternalDuplicates ? "SuccessWithWarnings" : "Success";
                        existingNotification.RecordData = JsonConvert.SerializeObject(finalResponse);
                        existingNotification.Status = NotificationStatus.Done;
                        _context.Notifications.Update(existingNotification);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Fallback: create new notification if existing one not found
                        var notification = new Notification
                        {
                            UserId = userId,
                            Message = successMessage,
                            Category = promptData.Type,
                            ResponseType = hasInternalDuplicates ? "SuccessWithWarnings" : "Success",
                            RecordData = JsonConvert.SerializeObject(finalResponse),
                            IsRead = false,
                            Status = NotificationStatus.Done,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Notifications.AddAsync(notification);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // No notification ID provided - create new notification
                    var notification = new Notification
                    {
                        UserId = userId,
                        Message = successMessage,
                        Category = promptData.Type,
                        ResponseType = hasInternalDuplicates ? "SuccessWithWarnings" : "Success",
                        RecordData = JsonConvert.SerializeObject(finalResponse),
                        IsRead = false,
                        Status = NotificationStatus.Done,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Notifications.AddAsync(notification);
                    await _context.SaveChangesAsync();
                }
            }

            // Publish entity processing messages to PubSub after the bulk import is completed
            await PublishEntityProcessingMessages(entityName, finalResponse);

            return finalResponse;
        }

        public async Task<dynamic> GetDependentDropdownValues(dynamic dependents, dynamic responseObject, AiPrompt promptData)
        {
            var interactionType = false;
            if (promptData?.Type?.Contains("interaction", StringComparison.OrdinalIgnoreCase) == true)
            {
                interactionType = true;
            }
            bool partnerType = false;
            if (promptData?.Type?.Contains("partner", StringComparison.OrdinalIgnoreCase) == true)
            {
                partnerType = true;
            }
            bool opportunityType = false;
            if (promptData?.Type?.Contains("opportunity", StringComparison.OrdinalIgnoreCase) == true)
            {
                opportunityType = true;
            }
            // Ensure date is set for interactions if missing or empty
            if (interactionType)
            {
                var dateValue = responseObject["date"];
                    if (dateValue == null || 
                    string.IsNullOrWhiteSpace(dateValue?.ToString()) ||
                    (dateValue is JValue jValue && (jValue.Value == null || string.IsNullOrWhiteSpace(jValue.Value?.ToString()))))
                {
                    responseObject["date"] = DateTime.UtcNow.ToString("yyyy-MM-dd");
                }
            }
            if (!string.IsNullOrWhiteSpace(dependents))
            {
                var dependentsList = JsonConvert.DeserializeObject<List<string>>(dependents);

                if (dependentsList.Count > 0)
                {
                    foreach (var dependent in dependentsList)
                    {
                        var text = responseObject[dependent];
                        // When ID field is null but corresponding Name field has text, use it for resolution (AI returns name-only for dependents like proposedInitiativeTypeId)
                        if (text == null)
                        {
                            var nameField = dependent.Replace("Id", "Name").Replace("Ids", "Names");
                            if (!string.IsNullOrEmpty(nameField))
                            {
                                var nameValue = responseObject[nameField];
                                if (nameValue != null)
                                {
                                    var nameStr = nameValue is string s ? s : nameValue.ToString();
                                    if (!string.IsNullOrWhiteSpace(nameStr))
                                        text = nameStr;
                                }
                            }
                        }
                        if (text != null)
                        {
                            // Special case: OrganizationUnitRelationships (many-to-many)
                            if (dependent == "organizationUnitRelationships")
                            {
                                string entityType = "Partner"; // Default to Partner
                                if (interactionType)
                                    entityType = "Interaction";
                                    
                                await HandleOrganizationUnitRelationships(responseObject, text, entityType, dependent);
                                continue;
                            }
                            
                            // Check if the dependent field is already an array of text values
                            if (text is JArray textArray)
                            {
                                // Handle opportunity-specific arrays that need full object structures
                                if (opportunityType && IsOpportunityCollectionField(dependent))
                                {
                                    // Pass partnerBudgets array if available (for funding partner budget associations)
                                    JArray partnerBudgets = null;
                                    if (dependent.Equals("fundingPartners", StringComparison.OrdinalIgnoreCase))
                                    {
                                        partnerBudgets = responseObject["partnerBudgets"] as JArray;
                                    }
                                    var objectsArray = await BuildOpportunityCollectionObjects(textArray, dependent, partnerBudgets);
                                    responseObject[dependent] = objectsArray;
                                    continue;
                                }
                                
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
                                            else
                                            {
                                                // Log warning for array items that couldn't be resolved
                                                Console.WriteLine($"[WARNING] Could not find ID for '{dependent}' array item with value '{textValue}'. Skipping this item.");
                                                // Don't add anything to the array for unresolved items
                                            }
                                        }
                                    }
                                }
                                
                                responseObject[dependent] = idsArray;
                                string nameField = dependent.Replace("Id", "Name");
                                responseObject[nameField] = await GetEntityNameFromId(idsArray, dependent);
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
                                    // It is already an ID, set name and continue
                                    string nameField = dependent.Replace("Id", "Name");
                                    responseObject[nameField] = await GetEntityNameFromId((int)longValue, dependent);
                                    if (interactionType && dependent == "contactIds")
                                    {
                                        await HandleInteractionContactLogic(responseObject, longValue);
                                    }
                                    continue;
                                } else if (text is int intValue)
                                {
                                    // It is already an ID, set name and continue
                                    string nameField = dependent.Replace("Id", "Name");
                                    responseObject[nameField] = await GetEntityNameFromId(intValue, dependent);
                                    if (interactionType && dependent == "contactIds")
                                    {
                                        await HandleInteractionContactLogic(responseObject, intValue);
                                    }
                                    continue;
                                }
                                else if (int.TryParse(text?.ToString(), out id))
                                {
                                    // It is already an ID, set name and continue
                                    string nameField = dependent.Replace("Id", "Name");
                                    responseObject[nameField] = await GetEntityNameFromId(id, dependent);
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
                                    // Set the field to null if no ID was found
                                    Console.WriteLine($"[WARNING] Could not find ID for '{dependent}' with value '{text}'. Setting field to null.");
                                    responseObject[dependent] = null;
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
                                    // Convert entityId to int for name lookup
                                    string nameField = dependent.Replace("Id", "Name");
                                    if (int.TryParse(entityId.ToString(), out int idForNameLookup))
                                    {
                                        responseObject[nameField] = await GetEntityNameFromId(idForNameLookup, dependent);
                                    }
                                    else
                                    {
                                        responseObject[nameField] = null;
                                    }
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

            // Remove _confidence and dependents objects from response for opportunity documents
            if (opportunityType && responseObject is JObject jobject)
            {
                jobject.Remove("_confidence");
                jobject.Remove("dependents");
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
            Console.WriteLine($"[DEBUG] GetEntityIdFromText called with text='{text}', dependent='{dependent}'");
            
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
                    // Special case for selectedOrgUnitId (Contact specific) - should look at OrganizationHierarchies table
                    || dependent.Equals("selectedOrgUnitId", StringComparison.OrdinalIgnoreCase)
                    || entityName.Equals("Orgunit", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "OrganizationHierarchies";
                whereCondition = "\"Type\" = 'OrgUnit'";
            }
            // Special case for responsibleOrgUnitId (Opportunity specific) - should look at OrganizationHierarchies table
            else if (dependent.Equals("responsibleOrgUnitId", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "OrganizationHierarchies";
                whereCondition = "\"Type\" = 'OrgUnit'";
            }
            // Special case for proposedInitiativeTypeId (Opportunity specific) - should look at ProposedInitiativeTypes table
            else if (dependent.Equals("proposedInitiativeTypeId", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "ProposedInitiativeTypes";
                whereCondition = "1=1";
            }
            // Special case for fundingPartners and clientPartners (Opportunity specific) - should look at Partners table
            else if (dependent.Equals("fundingPartners", StringComparison.OrdinalIgnoreCase) 
                     || dependent.Equals("clientPartners", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "Partners";
                whereCondition = "1=1";
            }
            // Special case for stakeholders (Opportunity specific) - handled by BuildStakeholderObject
            // Stakeholders are resolved in BuildStakeholderObject using userName and roleName from AI
            else if (dependent.Equals("stakeholders", StringComparison.OrdinalIgnoreCase))
            {
                // Stakeholder user resolution uses UserProfile table (same as User/PartnerFocalPointUser)
                entityName = "UserProfile";
                whereCondition = "1=1";
            }
            // Special case for countries (Opportunity specific) - should look at Countries table
            else if (dependent.Equals("countries", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "Countries";
                whereCondition = "1=1";
            }
            // Special case for sdGs (Opportunity specific) - should look at SDGs table
            else if (dependent.Equals("sdGs", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "SDGs";
                whereCondition = "1=1";
            }
            // Special case for deliverables (Opportunity specific) - should look at Outputs table
            else if (dependent.Equals("deliverables", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "Outputs";
                whereCondition = "1=1";
            }
            // Special case for unopsMissions (Opportunity specific) - should look at UNOPSMissions table
            else if (dependent.Equals("unopsMissions", StringComparison.OrdinalIgnoreCase))
            {
                entityName = "UNOPSMissions";
                whereCondition = "1=1";
            }
            // Special case for User/UserIds - should look at UserProfile table (which has searchable Name field)
            else if (entityName.Equals("User", StringComparison.OrdinalIgnoreCase)
                || (entityName.Equals("PartnerFocalPointUser", StringComparison.OrdinalIgnoreCase)))
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
            } else if (dependent.Equals("roleIds", StringComparison.OrdinalIgnoreCase)) {
                entityName = "AspNetRoles";
                whereCondition = "1=1";
            }
            else
            {
                entityName = entityName.Pluralize();
            }
            
            Console.WriteLine($"[DEBUG] Looking up '{text}' in entity '{entityName}' with where condition: '{whereCondition}'");
            var result = await RetrieveEntityId(entityName, null, text, 0.3f, 0.7f, whereCondition);
            Console.WriteLine($"[DEBUG] GetEntityIdFromText result: {(result != null && !(result is DBNull) ? result.ToString() : "NOT FOUND")}");
            
            return result;
        }

        /// <summary>
        /// Gets entity name from ID using direct DbSet queries
        /// </summary>
        /// <param name="id">The entity ID to lookup</param>
        /// <param name="dependent">The dependent field name (e.g., "partnerGroupId", "partnerCategoryId")</param>
        /// <returns>The entity name if found, null otherwise</returns>
        public async Task<string> GetEntityNameFromId(int id, string dependent)
        {
            try
            {
                string name = null;
                
                // Convert dependent to entity name using the same logic as GetEntityIdFromText
                string baseEntityName = dependent.EndsWith("Ids", StringComparison.OrdinalIgnoreCase) ? 
                    dependent.Substring(0, dependent.Length - 3) : 
                    dependent.Replace("Id", "", StringComparison.OrdinalIgnoreCase);
                
                // Capitalize only the first letter, preserving existing capitalization
                string entityName = string.IsNullOrEmpty(baseEntityName) ? 
                    baseEntityName : 
                    char.ToUpper(baseEntityName[0]) + baseEntityName.Substring(1);
                
                // Apply the same mapping logic as GetEntityIdFromText
                if (dependent.Equals("organizationUnitRelationships", StringComparison.OrdinalIgnoreCase)
                        || dependent.Equals("organizationHierarchyIds", StringComparison.OrdinalIgnoreCase)
                        // Special case for selectedOrgUnitId (Contact specific) - should look at OrganizationHierarchies table
                        || dependent.Equals("selectedOrgUnitId", StringComparison.OrdinalIgnoreCase)
                        || entityName.Equals("Orgunit", StringComparison.OrdinalIgnoreCase))
                {
                    name = await _context.OrganizationHierarchies
                        .Where(x => x.Id == id && x.Type == OrganizationUnitType.OrgUnit)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();
                }
                else if (entityName.Equals("User", StringComparison.OrdinalIgnoreCase) 
                         || dependent.Equals("partnerfocalpointuserid", StringComparison.OrdinalIgnoreCase)
                         || dependent.Equals("createdby", StringComparison.OrdinalIgnoreCase)
                         || dependent.Equals("lastmodifiedby", StringComparison.OrdinalIgnoreCase))
                {
                    name = await _context.UserProfile
                        .Where(x => x.UserId == id && !x.IsDeleted)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();
                }
                else if (entityName.Equals("Contact", StringComparison.OrdinalIgnoreCase)
                         || dependent.Equals("contactIds", StringComparison.OrdinalIgnoreCase))
                {
                    name = await _context.Contacts
                        .Where(x => x.Id == id && !x.IsDeleted)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();
                }
                else if (entityName.Equals("Partner", StringComparison.OrdinalIgnoreCase)
                         || dependent.Equals("partnerIds", StringComparison.OrdinalIgnoreCase))
                {
                    name = await _context.Partners
                        .Where(x => x.Id == id && !x.IsDeleted)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();
                }
                else if (entityName.Equals("Interaction", StringComparison.OrdinalIgnoreCase)
                         || dependent.Equals("interactionIds", StringComparison.OrdinalIgnoreCase))
                {
                    name = await _context.Interactions
                        .Where(x => x.Id == id && !x.IsDeleted)
                        .Select(x => x.Subject)
                        .FirstOrDefaultAsync();
                }
                else if (dependent.Equals("partnerGroupId", StringComparison.OrdinalIgnoreCase)
                    || dependent.Equals("partnerCategoryId", StringComparison.OrdinalIgnoreCase))
                {
                    name = await _context.PartnerTrees
                        .Where(x => x.Id == id)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();
                }
                else if (dependent.Equals("liaisonofficeid", StringComparison.OrdinalIgnoreCase)
                         || entityName.Equals("Office", StringComparison.OrdinalIgnoreCase))
                {
                    name = await _context.LiaisonOffices
                        .Where(x => x.Id == id)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();
                }
                else if (dependent.Equals("proposedInitiativeTypeId", StringComparison.OrdinalIgnoreCase))
                {
                    name = await _context.ProposedInitiativeTypes
                        .Where(x => x.Id == id)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    // For other entities, try to find by convention using the pluralized entity name
                    // This is a fallback that might work for entities following standard naming conventions
                    Console.WriteLine($"[DEBUG] No specific mapping found for dependent '{dependent}', entityName '{entityName}'");
                }

                return name;
            }
            catch (Exception ex)
            {
                // Log the error but return null instead of throwing
                Console.WriteLine($"Error getting entity name for {dependent} ID {id}: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
                return null;
            }
        }


        /// <summary>
        /// Gets entity names from array of IDs using the same mapping strategy as GetEntityIdFromText but in reverse
        /// </summary>
        /// <param name="ids">The entity IDs to lookup (can be JArray or int[])</param>
        /// <param name="dependent">The dependent field name (e.g., "partnerGroupId", "partnerCategoryId")</param>
        /// <returns>Comma-separated string of entity names if found, null otherwise</returns>
        public async Task<string> GetEntityNameFromId(dynamic ids, string dependent)
        {
            try
            {
                // Handle different input types
                int[] idArray = null;
                
                if (ids is JArray jArray)
                {
                    // Convert JArray to int array
                    idArray = jArray.Select(token => 
                    {
                        if (int.TryParse(token.ToString(), out int id))
                            return id;
                        return 0; // Default for invalid values
                    }).Where(id => id > 0).ToArray();

                    var names = new List<string>();
                    for (int i = 0; i < idArray.Length; i++)
                    {
                        var entityName = await GetEntityNameFromId(idArray[i], dependent);
                        if (!string.IsNullOrEmpty(entityName))
                        {
                            names.Add(entityName);
                        }
                    }

                    return names.Count > 0 ? string.Join(", ", names) : null;
                }
                else if (ids is int[] intArray)
                {
                    idArray = intArray;
                }
                else if (ids is List<int> intList)
                {
                    var names = new List<string>();
                    for (int i = 0; i < intList.Count; i++)
                    {
                        var entityName = await GetEntityNameFromId(intList[i], dependent);
                        if (!string.IsNullOrEmpty(entityName))
                        {
                            names.Add(entityName);
                        }
                    }

                    return names.Count > 0 ? string.Join(", ", names) : null;
                }
                else if (ids is int singleId)
                {
                    // Single ID case - use existing method
                    return await GetEntityNameFromId(singleId, dependent);
                }
                else if (ids != null)
                {
                    // Try to parse as single ID
                    if (int.TryParse(ids.ToString(), out int parsedId))
                    {
                        return await GetEntityNameFromId(parsedId, dependent);
                    }
                }

                if (idArray == null || idArray.Length == 0)
                {
                    Console.WriteLine($"[DEBUG] GetEntityNameFromId: No valid IDs found in input for dependent='{dependent}'");
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the error but return null instead of throwing
                Console.WriteLine($"Error getting entity names for {dependent} IDs: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Executes the database query to lookup entity name by ID
        /// </summary>
        /// <param name="tableName">The table to query</param>
        /// <param name="nameField">The field containing the name (can be a computed field)</param>
        /// <param name="id">The ID to lookup</param>
        /// <param name="whereCondition">Additional WHERE conditions</param>
        /// <returns>The entity name if found, null otherwise</returns>
        private async Task<string> ExecuteNameLookupQuery(string tableName, string nameField, int id, string whereCondition)
        {
            try
            {
                var sql = $"SELECT {nameField} as EntityName FROM \"{tableName}\" WHERE \"Id\" = @id AND ({whereCondition}) LIMIT 1";
                
                Console.WriteLine($"[DEBUG] Executing query: {sql}");
                Console.WriteLine($"[DEBUG] Parameters: id={id}");
                
                var parameters = new[] 
                {
                    new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Integer) { Value = id }
                };

                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddRange(parameters);

                var result = await command.ExecuteScalarAsync();
                Console.WriteLine($"[DEBUG] Query result: '{result}'");
                return result?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing name lookup query for table {tableName}, ID {id}: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Executes the database query to lookup entity names by array of IDs
        /// </summary>
        /// <param name="tableName">The table to query</param>
        /// <param name="nameField">The field containing the name (can be a computed field)</param>
        /// <param name="ids">The array of IDs to lookup</param>
        /// <param name="whereCondition">Additional WHERE conditions</param>
        /// <returns>Comma-separated string of entity names if found, null otherwise</returns>
        private async Task<string> ExecuteNameLookupQuery(string tableName, string nameField, int[] ids, string whereCondition)
        {
            try
            {
                if (ids == null || ids.Length == 0)
                    return null;

                // Handle single ID case by calling the single ID method
                if (ids.Length == 1)
                {
                    return await ExecuteNameLookupQuery(tableName, nameField, ids[0], whereCondition);
                }

                // Create parameterized query for multiple IDs
                var parameterPlaceholders = string.Join(",", ids.Select((id, index) => $"@id{index}"));
                var sql = $"SELECT {nameField} as EntityName FROM \"{tableName}\" WHERE \"Id\" IN ({parameterPlaceholders}) AND ({whereCondition}) ORDER BY \"Id\"";
                
                Console.WriteLine($"[DEBUG] Executing multi-ID query: {sql}");
                Console.WriteLine($"[DEBUG] Parameters: ids=[{string.Join(",", ids)}]");
                
                var parameters = ids.Select((id, index) => 
                    new NpgsqlParameter($"@id{index}", NpgsqlTypes.NpgsqlDbType.Integer) { Value = id })
                    .ToArray();

                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddRange(parameters);

                var names = new List<string>();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var name = reader["EntityName"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        names.Add(name);
                    }
                }

                var result = names.Count > 0 ? string.Join(", ", names) : null;
                Console.WriteLine($"[DEBUG] Multi-ID query result: '{result}'");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing name lookup query for table {tableName}, IDs [{string.Join(",", ids)}]: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
                return null;
            }
        }
        
        private async Task HandleOrganizationUnitRelationships(dynamic responseObject, dynamic orgUnitText, string entityType = "Partner", string dependent = "organizationUnitRelationships")
        {
            var orgUnitRelationships = new JArray();
            var orgUnitCodes = new List<string>(); // For collecting codes for the Name field
            
            // Handle different types of orgUnitText input
            string[] orgUnitNames = null;
            
            if (orgUnitText is JArray orgUnitArray)
            {
                // Handle array of org unit names
                orgUnitNames = orgUnitArray.Select(item => ExtractOrgUnitName(item)).Where(name => !string.IsNullOrEmpty(name)).ToArray();
            }
            else if (orgUnitText is JValue jValue)
            {
                // Handle JValue (like {ITG}) - extract the actual value
                var extractedName = ExtractOrgUnitName(jValue);
                if (!string.IsNullOrEmpty(extractedName))
                {
                    orgUnitNames = new[] { extractedName };
                }
            }
            else if (orgUnitText != null)
            {
                // Handle single org unit name as string or other types
                var extractedName = ExtractOrgUnitName(orgUnitText);
                if (!string.IsNullOrEmpty(extractedName))
                {
                    orgUnitNames = (string[]?)(new[] { extractedName });
                }
            }
            
            var detectedOrgUnitNamesList = new List<string>();
            // Process each org unit name
            if (orgUnitNames != null && orgUnitNames.Length > 0)
            {
                foreach (var orgUnitTextValue in orgUnitNames)
                {
                    Console.WriteLine($"[DEBUG] Processing org unit text: '{orgUnitTextValue}'");
                    
                    try
                    {
                        // First resolve the text to an ID
                        var orgUnitId = await GetEntityIdFromText(orgUnitTextValue, "organizationHierarchyIds");
                        
                        if (orgUnitId != null && !(orgUnitId is DBNull))
                        {
                            Console.WriteLine($"[DEBUG] Resolved '{orgUnitTextValue}' to org unit ID: {orgUnitId}");
                            
                            // Then get the full object by ID
                            var orgUnitData = await GetOrganizationUnitRelationshipDataById(Convert.ToInt32(orgUnitId), entityType);
                            if (orgUnitData != null)
                            {
                                orgUnitRelationships.Add(orgUnitData);
                                
                                // Extract the code for the Name field
                                var orgHierarchy = orgUnitData["organizationHierarchy"];
                                if (orgHierarchy != null && orgHierarchy["code"] != null)
                                {
                                    var code = orgHierarchy["code"].ToString();
                                    if (!string.IsNullOrEmpty(code))
                                    {
                                        orgUnitCodes.Add(code);
                                    }
                                    var orgUnitName = orgHierarchy["name"]?.ToString();
                                    if (!string.IsNullOrEmpty(orgUnitName))
                                    {
                                        detectedOrgUnitNamesList.Add(orgUnitName);
                                    }
                                }
                                
                                Console.WriteLine($"[DEBUG] Successfully added org unit relationship for ID {orgUnitId} ('{orgUnitTextValue}')");
                            }
                            else
                            {
                                Console.WriteLine($"[WARNING] No org unit object found for resolved ID {orgUnitId} ('{orgUnitTextValue}')");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[WARNING] Could not resolve org unit text '{orgUnitTextValue}' to an ID");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Error processing org unit '{orgUnitTextValue}': {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"[WARNING] No valid org unit names found in: '{orgUnitText}'");
            }
            
            // Set the relationships array
            responseObject[dependent] = orgUnitRelationships;
            
            // Set the Name field with comma-separated names
            var nameField = dependent.Replace("Id", "Name");
            responseObject[nameField] = detectedOrgUnitNamesList.Count > 0 ? string.Join(", ", detectedOrgUnitNamesList) : null;
        }
        
        /// <summary>
        /// Extract org unit name from various input types (JValue, string, etc.)
        /// </summary>
        private string ExtractOrgUnitName(dynamic input)
        {
            try
            {
                if (input == null) return null;
                
                if (input is JValue jValue)
                {
                    // For JValue, get the actual value
                    var value = jValue.Value?.ToString();
                    Console.WriteLine($"[DEBUG] Extracted from JValue: '{value}'");
                    return value;
                }
                else if (input is JToken jToken)
                {
                    // For other JToken types
                    var value = jToken.ToString();
                    Console.WriteLine($"[DEBUG] Extracted from JToken: '{value}'");
                    return value;
                }
                else
                {
                    // For other types, convert to string
                    var value = input.ToString();
                    Console.WriteLine($"[DEBUG] Extracted from other type: '{value}'");
                    return value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to extract org unit name from '{input}': {ex.Message}");
                return null;
            }
        }
        
        private async Task<JObject> GetOrganizationUnitRelationshipDataById(int orgUnitId, string entityType = "Partner")
        {
            try
            {
                // Get the organization hierarchy data by exact ID
                var orgHierarchy = await _context.OrganizationHierarchies
                    .Where(oh => oh.Id == orgUnitId)
                    .Select(oh => new
                    {
                        id = oh.Id,
                        code = oh.Code,
                        name = oh.Name,
                        type = oh.Type,
                        description = oh.Description,
                        parentId = oh.ParentId
                    })
                    .FirstOrDefaultAsync();
                    
                if (orgHierarchy == null)
                {
                    Console.WriteLine($"[WARNING] No organization hierarchy found for ID {orgUnitId}");
                    return null;
                }
                    
                var relationshipData = new JObject
                {
                    ["organizationHierarchyId"] = orgHierarchy.id,
                    ["organizationHierarchy"] = JObject.FromObject(orgHierarchy),
                    ["entityId"] = 0,
                    ["entityType"] = entityType
                };
                
                Console.WriteLine($"[DEBUG] Created relationship data for org unit ID {orgUnitId} ('{orgHierarchy.name}')");
                return relationshipData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error getting organization unit relationship data by ID {orgUnitId}: {ex.Message}");
                return null;
            }
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
                        
                        // Get existing partner names or create new list
                        var partnerNamesList = new List<string>();
                        var existingPartnerNames = responseObject["partnerNames"]?.ToString();
                        if (!string.IsNullOrEmpty(existingPartnerNames))
                        {
                            partnerNamesList.AddRange(existingPartnerNames.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries));
                        }
                        
                        // Add new partner name
                        var newPartnerName = await GetEntityNameFromId(contact.PartnerId, "partnerIds");
                        if (!string.IsNullOrEmpty(newPartnerName))
                        {
                            partnerNamesList.Add(newPartnerName);
                        }
                        
                        responseObject["partnerNames"] = partnerNamesList.Count > 0 ? string.Join(", ", partnerNamesList) : null;
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
            if (_disableExternalCalls)
            {
                return texts.Select(_ => string.Empty).ToList();
            }

            var embeddings = new List<string>();
            var batchSize = 100; // Gemini Embedding API supports up to 100 requests per batch

            for (int i = 0; i < texts.Count; i += batchSize)
            {
                var batch = texts.Skip(i).Take(batchSize).ToList();
                var batchEmbeddings = await CreateEmbeddingsBatchAsync(batch);
                embeddings.AddRange(batchEmbeddings);
                
                _logger?.LogInformation("📊 Generated embeddings for batch {Current}/{Total} texts", 
                    Math.Min(i + batchSize, texts.Count), texts.Count);
            }

            return embeddings;
        }

        /// <summary>
        /// Generates keywords for a list of texts using Gemini AI for hybrid search
        /// </summary>
        /// <param name="texts">List of texts to generate keywords for</param>
        /// <returns>Dictionary mapping text to comma-separated keywords</returns>
        public async Task<Dictionary<string, string>> GenerateKeywordsAsync(List<string> texts)
        {
            if (texts == null || !texts.Any())
                return new Dictionary<string, string>();
            if (_disableExternalCalls)
            {
                return texts.ToDictionary(text => text, _ => string.Empty);
            }

            var keywords = new Dictionary<string, string>();
            var batchSize = 10; // Process 10 at a time for keyword generation

            for (int i = 0; i < texts.Count; i += batchSize)
            {
                var batch = texts.Skip(i).Take(batchSize).ToList();
                
                foreach (var text in batch)
                {
                    try
                    {
                        var generatedKeywords = await GenerateKeywordsForTextAsync(text);
                        keywords[text] = generatedKeywords;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning("⚠️ Failed to generate keywords for text: {Text}. Error: {Error}", 
                            text.Substring(0, Math.Min(50, text.Length)), ex.Message);
                        keywords[text] = string.Empty;
                    }
                }
                
                _logger?.LogInformation("🔑 Generated keywords for {Current}/{Total} texts", 
                    Math.Min(i + batchSize, texts.Count), texts.Count);
                
                // Rate limiting: avoid overwhelming the API
                await Task.Delay(100);
            }

            return keywords;
        }

        /// <summary>
        /// Generates keywords for a single text using Gemini AI
        /// </summary>
        private async Task<string> GenerateKeywordsForTextAsync(string text)
        {
            if (_disableExternalCalls)
            {
                return string.Empty;
            }

            var projectId = _configuration.GetValue<string>("AISettings:ProjectId");
            var location = _configuration.GetValue<string>("AISettings:Location");
            var model = _configuration.GetValue<string>("AISettings:Model") ?? "gemini-2.0-flash-exp";

            if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(location))
            {
                throw new InvalidOperationException("Project ID or Location not configured in AISettings");
            }

            var accessToken = await GetAccessTokenAsync();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var prompt = $@"Generate 5-10 relevant keywords for the following service/product description. 
Return ONLY the keywords as a comma-separated list, no explanations.

Text: {text}

Keywords:";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    maxOutputTokens = 100,
                    topP = 0.8
                }
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/{model}:generateContent";

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

            if (responseObject?.candidates?[0]?.content?.parts?[0]?.text != null)
            {
                var keywordsText = responseObject.candidates[0].content.parts[0].text.ToString();
                // Clean up the response (remove "Keywords:", newlines, extra spaces)
                keywordsText = keywordsText.Replace("Keywords:", "").Replace("\n", "").Trim();
                return keywordsText;
            }

            return string.Empty;
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
                if (_disableExternalCalls)
                {
                    return texts.Select(_ => string.Empty).ToList();
                }

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

                    // Extract record ID if it exists to exclude from duplicate detection
                    int? recordId = null;
                    if (record is JObject recordJObj && recordJObj.ContainsKey("id"))
                    {
                        if (int.TryParse(recordJObj["id"]?.ToString(), out int recordId1) && recordId1 > 0)
                        {
                            recordId = recordId1;
                        }
                    }
                    else if (record is ExpandoObject expObj)
                    {
                        var dict = (IDictionary<string, object>)expObj;
                        if (dict.ContainsKey("id") && int.TryParse(dict["id"]?.ToString(), out int recordId2) && recordId2 > 0)
                        {
                            recordId = recordId2;
                        }
                    }
                    else
                    {
                        // Try reflection for strongly typed objects
                        var idProperty = record.GetType().GetProperty("Id") ?? record.GetType().GetProperty("id");
                        if (idProperty != null)
                        {
                            var idValue = idProperty.GetValue(record);
                            if (idValue != null)
                            {
                                if (int.TryParse(idValue.ToString(), out int recordId3) && recordId3 > 0)
                                {
                                    recordId = recordId3;
                                }
                            }
                        }
                    }

                    // Use the simplified detect_duplicate_records function (field-based only)
                    var duplicateResult = await DetectDuplicateForRecordAsync(
                        pluralizedEntityName, 
                        record, 
                        (float)fieldMatchThreshold,
                        recordId
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
            float fieldMatchThreshold = 0.5f,
            int? excludeRecordId = null)
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
                command.CommandText = "SELECT public.detect_duplicate_records(@entityType, @entityData, @fieldMatchThreshold, @debugMode, @excludeRecordId)";

                // Create parameters for the simplified function call (entity_data as TEXT)
                 var parameters = new[] 
                 {
                    new NpgsqlParameter("@entityType", NpgsqlTypes.NpgsqlDbType.Text) { Value = singularEntityName },
                    new NpgsqlParameter("@entityData", NpgsqlTypes.NpgsqlDbType.Text) { Value = jsonData },
                    new NpgsqlParameter("@fieldMatchThreshold", NpgsqlTypes.NpgsqlDbType.Real) { Value = fieldMatchThreshold },
                    new NpgsqlParameter("@debugMode", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = false },
                    new NpgsqlParameter("@excludeRecordId", NpgsqlTypes.NpgsqlDbType.Integer) { Value = excludeRecordId.HasValue ? (object)excludeRecordId.Value : DBNull.Value }
                 };

                 command.Parameters.AddRange(parameters);

                var result = await command.ExecuteScalarAsync();
                
                if (result != null && result != DBNull.Value)
                {
                    var jsonResult = result.ToString();
                    var parsedResult = JsonConvert.DeserializeObject<dynamic>(jsonResult);
                    
                    var duplicatesArray = (JArray)parsedResult.duplicates;
                    DuplicateMatch topDuplicate = null;
                    
                    if (duplicatesArray?.Count > 0)
                    {
                        var firstDuplicate = duplicatesArray[0];
                        topDuplicate = new DuplicateMatch
                        {
                            EntityId = (int)(firstDuplicate["entityId"] ?? 0),
                            EntityType = (string)(firstDuplicate["entityType"] ?? ""),
                            Score = (double)(firstDuplicate["score"] ?? 0.0),
                            MatchReason = (string)(firstDuplicate["matchReason"] ?? ""),
                            SearchType = (string)(firstDuplicate["searchType"] ?? ""),
                            MatchedData = firstDuplicate["matchedData"]
                        };
                    }
                    
                    return new ComprehensiveDuplicateResult
                    {
                        HasDuplicates = duplicatesArray != null && duplicatesArray.Count > 0,
                        TotalDuplicates = parsedResult.summary?.totalDuplicates ?? 0,
                        HighConfidence = parsedResult.summary?.highConfidence ?? 0,
                        MediumConfidence = parsedResult.summary?.mediumConfidence ?? 0,
                        LowConfidence = parsedResult.summary?.lowConfidence ?? 0,
                        TopDuplicate = topDuplicate,
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
                
                // Extract record ID if it exists to exclude from duplicate detection
                int? recordId = null;
                if (recordData is JObject recordDataJObj && recordDataJObj.ContainsKey("id"))
                {
                    if (int.TryParse(recordDataJObj["id"]?.ToString(), out int recordDataId1) && recordDataId1 > 0)
                    {
                        recordId = recordDataId1;
                    }
                }
                else
                {
                    // Try reflection for strongly typed objects
                        var idProperty = recordData.GetType().GetProperty("Id") ?? recordData.GetType().GetProperty("id");
                        if (idProperty != null)
                        {
                            var idValue = idProperty.GetValue(recordData);
                            if (idValue != null)
                            {
                                if (int.TryParse(idValue.ToString(), out int recordDataId2) && recordDataId2 > 0)
                                {
                                    recordId = recordDataId2;
                                }
                            }
                        }
                }

                return await DetectDuplicateForRecordAsync(
                    pluralizedEntityName, 
                    convertedData, 
                    (float)fieldMatchThreshold,
                    recordId
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
                "role" or "roles" => new List<string> { "name", "description" },
                "user_role" => new List<string> { "userId", "roleIds" },
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
        
        /// <summary>
        /// Extracts semantic search keywords from opportunity context using Gemini AI
        /// </summary>
        /// <param name="opportunityContext">Complete opportunity context JSON</param>
        /// <param name="promptData">AI prompt configuration for keyword extraction</param>
        /// <returns>List of extracted keywords for semantic search</returns>
        public async Task<List<string>> ExtractKeywordsForSemanticSearchAsync(string opportunityContext, AiPrompt promptData)
        {
            try
            {
                Console.WriteLine($"[DEBUG] ExtractKeywordsForSemanticSearchAsync: Extracting keywords from opportunity context");
                
                // Call Gemini API to extract keywords
                var geminiResponse = await FetchResultFromGemini(promptData, opportunityContext);
                
                // Parse the response to extract keywords
                var responseJson = GetDetailsFromGeminiResponse(geminiResponse);
                
                var keywords = new List<string>();
                
                if (responseJson["keywords"] != null && responseJson["keywords"] is JArray keywordsArray)
                {
                    keywords = keywordsArray.Select(k => k.ToString()).ToList();
                }
                else if (responseJson["query"] != null)
                {
                    // Fallback: if the response contains a single query string, split it
                    keywords = responseJson["query"].ToString().Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(k => k.Trim())
                        .ToList();
                }
                
                Console.WriteLine($"[DEBUG] Extracted {keywords.Count} keywords: {string.Join(", ", keywords)}");
                
                return keywords;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error extracting keywords: {ex.Message}");
                throw new Exception($"Failed to extract keywords for semantic search: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Checks if a field is an opportunity collection field that needs object structure
        /// </summary>
        private bool IsOpportunityCollectionField(string dependent)
        {
            return dependent.Equals("fundingPartners", StringComparison.OrdinalIgnoreCase) ||
                   dependent.Equals("clientPartners", StringComparison.OrdinalIgnoreCase) ||
                   dependent.Equals("stakeholders", StringComparison.OrdinalIgnoreCase) ||
                   dependent.Equals("teamMembers", StringComparison.OrdinalIgnoreCase) ||
                   dependent.Equals("deliverables", StringComparison.OrdinalIgnoreCase) ||
                   dependent.Equals("countries", StringComparison.OrdinalIgnoreCase) ||
                   dependent.Equals("sdGs", StringComparison.OrdinalIgnoreCase) ||
                   dependent.Equals("unopsMissions", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Builds opportunity collection objects from text arrays
        /// </summary>
        /// <param name="textArray">Array of text values to convert to objects</param>
        /// <param name="dependent">The dependent field name being processed</param>
        /// <param name="partnerBudgets">Optional array of partner budget allocations (used for funding partners)</param>
        private async Task<JArray> BuildOpportunityCollectionObjects(JArray textArray, string dependent, JArray partnerBudgets = null)
        {
            var objectsArray = new JArray();
            
            foreach (var textItem in textArray)
            {
                var textValue = textItem?.ToString();
                if (string.IsNullOrEmpty(textValue)) continue;
                
                try
                {
                    if (dependent.Equals("countries", StringComparison.OrdinalIgnoreCase))
                    {
                        var countryObj = await BuildCountryObject(textValue);
                        if (countryObj != null) objectsArray.Add(countryObj);
                    }
                    else if (dependent.Equals("sdGs", StringComparison.OrdinalIgnoreCase))
                    {
                        var sdgObj = await BuildSDGObject(textValue);
                        if (sdgObj != null) objectsArray.Add(sdgObj);
                    }
                    else if (dependent.Equals("fundingPartners", StringComparison.OrdinalIgnoreCase))
                    {
                        // Look up budget for this partner from partnerBudgets array
                        decimal? amount = null;
                        string currency = "USD";
                        
                        if (partnerBudgets != null && partnerBudgets.Count > 0)
                        {
                            var budgetEntry = FindPartnerBudget(textValue, partnerBudgets);
                            if (budgetEntry != null)
                            {
                                amount = budgetEntry["amount"]?.Value<decimal?>();
                                currency = budgetEntry["currency"]?.ToString() ?? "USD";
                                Console.WriteLine($"[INFO] Found budget for partner '{textValue}': {amount} {currency}");
                            }
                        }
                        
                        var partnerObj = await BuildPartnerObject(textValue, "Funding", amount, currency);
                        if (partnerObj != null) objectsArray.Add(partnerObj);
                    }
                    else if (dependent.Equals("clientPartners", StringComparison.OrdinalIgnoreCase))
                    {
                        var partnerObj = await BuildPartnerObject(textValue, "Client", null, null);
                        if (partnerObj != null) objectsArray.Add(partnerObj);
                    }
                    else if (dependent.Equals("stakeholders", StringComparison.OrdinalIgnoreCase))
                    {
                        // Stakeholders now come as JSON objects with userName and roleName
                        // Handle both object format and legacy string format
                        var stakeholderObj = await BuildStakeholderObject(textItem);
                        if (stakeholderObj != null) objectsArray.Add(stakeholderObj);
                    }
                    else if (dependent.Equals("teamMembers", StringComparison.OrdinalIgnoreCase))
                    {
                        var teamMemberObj = await BuildTeamMemberObject(textValue);
                        if (teamMemberObj != null) objectsArray.Add(teamMemberObj);
                    }
                    else if (dependent.Equals("deliverables", StringComparison.OrdinalIgnoreCase))
                    {
                        var deliverableObj = await BuildDeliverableObject(textValue);
                        if (deliverableObj != null) objectsArray.Add(deliverableObj);
                    }
                    else if (dependent.Equals("unopsMissions", StringComparison.OrdinalIgnoreCase))
                    {
                        var missionObj = await BuildUNOPSMissionObject(textValue);
                        if (missionObj != null) objectsArray.Add(missionObj);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to build object for {dependent} with value '{textValue}': {ex.Message}");
                }
            }
            
            return objectsArray;
        }
        
        /// <summary>
        /// Finds a partner's budget entry from the partnerBudgets array using fuzzy matching
        /// </summary>
        private JObject FindPartnerBudget(string partnerName, JArray partnerBudgets)
        {
            if (string.IsNullOrEmpty(partnerName) || partnerBudgets == null) return null;
            
            var normalizedPartnerName = partnerName.ToLowerInvariant().Trim();
            
            foreach (var budget in partnerBudgets)
            {
                var budgetPartnerName = budget["partnerName"]?.ToString();
                if (string.IsNullOrEmpty(budgetPartnerName)) continue;
                
                var normalizedBudgetPartnerName = budgetPartnerName.ToLowerInvariant().Trim();
                
                // Exact match
                if (normalizedPartnerName == normalizedBudgetPartnerName)
                {
                    return budget as JObject;
                }
                
                // Partial match (one contains the other)
                if (normalizedPartnerName.Contains(normalizedBudgetPartnerName) || 
                    normalizedBudgetPartnerName.Contains(normalizedPartnerName))
                {
                    return budget as JObject;
                }
                
                // Common abbreviation handling (e.g., "AfDB" matches "African Development Bank")
                var partnerWords = normalizedPartnerName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                var budgetWords = normalizedBudgetPartnerName.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Check if budget name is an abbreviation of partner name (first letters match)
                if (budgetWords.Length == 1 && partnerWords.Length > 1)
                {
                    var abbreviation = string.Concat(partnerWords.Select(w => w[0]));
                    if (abbreviation == normalizedBudgetPartnerName)
                    {
                        return budget as JObject;
                    }
                }
                
                // Vice versa
                if (partnerWords.Length == 1 && budgetWords.Length > 1)
                {
                    var abbreviation = string.Concat(budgetWords.Select(w => w[0]));
                    if (abbreviation == normalizedPartnerName)
                    {
                        return budget as JObject;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Builds a country object from text value
        /// Returns format matching OpportunityCountryModel: { countryId, country: {...} }
        /// </summary>
        private async Task<JObject> BuildCountryObject(string countryText)
        {
            try
            {
                var countryId = await GetEntityIdFromText(countryText, "countries");
                if (countryId == null || countryId is DBNull)
                {
                    Console.WriteLine($"[WARNING] Country not found: '{countryText}'");
                    return null;
                }
                
                // Cast to int for database query
                int countryIdInt = Convert.ToInt32(countryId);
                
                // Get full country details from database
                var country = await _context.Countries
                    .Where(c => c.Id == countryIdInt)
                    .Select(c => new { c.Id, c.Name, c.Iso2Code })
                    .FirstOrDefaultAsync();
                
                if (country == null) return null;
                
                // Return object matching OpportunityCountryModel structure
                // countryId at root level for comparison, nested country for display
                return new JObject
                {
                    ["countryId"] = country.Id,
                    ["country"] = new JObject
                    {
                        ["id"] = country.Id,
                        ["name"] = country.Name,
                        ["iso2Code"] = country.Iso2Code
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error building country object for '{countryText}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Builds an SDG object from text value
        /// </summary>
        private async Task<JObject> BuildSDGObject(string sdgText)
        {
            try
            {
                var sdgId = await GetEntityIdFromText(sdgText, "sdGs");
                if (sdgId == null || sdgId is DBNull)
                {
                    Console.WriteLine($"[WARNING] SDG not found: '{sdgText}'");
                    return null;
                }
                
                // Cast to int for database query
                int sdgIdInt = Convert.ToInt32(sdgId);
                
                // Get full SDG details from database
                var sdg = await _context.SDGs
                    .Where(s => s.Id == sdgIdInt)
                    .Select(s => new { s.Id, s.SDGNumber, s.Name })
                    .FirstOrDefaultAsync();
                
                if (sdg == null) return null;
                
                // Generate SDG logo URL
                string sdgLogoUrl = $"https://sdgs.un.org/sites/default/files/goals/E_SDG_Icons-{sdg.SDGNumber.ToString().PadLeft(2, '0')}.jpg";
                
                return new JObject
                {
                    ["sdgId"] = sdg.Id,
                    ["sdgNumber"] = sdg.SDGNumber,
                    ["sdgName"] = sdg.Name,
                    ["sdgLogoUrl"] = sdgLogoUrl,
                    ["isPrimary"] = false // Default to false, can be updated later
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error building SDG object for '{sdgText}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Builds a UNOPS Mission object from text value
        /// </summary>
        private async Task<JObject?> BuildUNOPSMissionObject(string missionText)
        {
            try
            {
                var missionId = await GetEntityIdFromText(missionText, "unopsMissions");
                if (missionId == null || missionId is DBNull)
                {
                    Console.WriteLine($"[WARNING] UNOPS Mission not found: '{missionText}'");
                    return null;
                }
                
                int missionIdInt = Convert.ToInt32(missionId);
                
                var mission = await _context.UNOPSMissions
                    .Where(m => m.Id == missionIdInt && !m.IsDeleted)
                    .Select(m => new { m.Id, m.Name, m.Code })
                    .FirstOrDefaultAsync();
                
                var result = new JObject { ["unopsMissionId"] = missionIdInt };
                if (mission != null)
                {
                    result["name"] = mission.Name;
                    result["code"] = mission.Code;
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error building UNOPS Mission object for '{missionText}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Builds a partner object from text value
        /// </summary>
        /// <param name="partnerText">The partner name text to search for</param>
        /// <param name="partnerType">"Funding" or "Client"</param>
        /// <param name="budgetAmount">Optional budget amount (for funding partners)</param>
        /// <param name="budgetCurrency">Optional budget currency code (for funding partners, defaults to USD)</param>
        private async Task<JObject> BuildPartnerObject(string partnerText, string partnerType, decimal? budgetAmount = null, string budgetCurrency = null)
        {
            try
            {
                var partnerId = await GetEntityIdFromText(partnerText, partnerType == "Funding" ? "fundingPartners" : "clientPartners");
                if (partnerId == null || partnerId is DBNull)
                {
                    Console.WriteLine($"[WARNING] Partner not found: '{partnerText}'");
                    return null;
                }
                
                // Cast to int for database query
                int partnerIdInt = Convert.ToInt32(partnerId);
                
                // Get full partner details from database including logo
                var partner = await _context.Partners
                    .Where(p => p.Id == partnerIdInt)
                    .Select(p => new { p.Id, p.Name, p.LogoUrl })
                    .FirstOrDefaultAsync();
                
                if (partner == null) return null;
                
                var partnerObj = new JObject
                {
                    ["partnerId"] = partner.Id,
                    ["partnerName"] = partner.Name,
                    ["partnerLogoUrl"] = !string.IsNullOrEmpty(partner.LogoUrl) ? partner.LogoUrl : "assets/images/Partner.png"
                };
                
                // Add budget fields for funding partners
                if (partnerType == "Funding")
                {
                    // Resolve currency code to ID
                    var currencyCode = !string.IsNullOrEmpty(budgetCurrency) ? budgetCurrency.ToUpperInvariant() : "USD";
                    int? currencyId = await GetCurrencyIdFromCode(currencyCode);
                    
                    // Use provided budget amount/currency if available
                    partnerObj["amount"] = budgetAmount.HasValue ? (JToken)budgetAmount.Value : JValue.CreateNull();
                    partnerObj["currencyId"] = currencyId.HasValue ? (JToken)currencyId.Value : JValue.CreateNull();
                    partnerObj["currencyCode"] = currencyCode;
                    
                    if (budgetAmount.HasValue)
                    {
                        Console.WriteLine($"[INFO] Partner '{partner.Name}' budget set to {budgetAmount.Value} {currencyCode} (CurrencyId: {currencyId})");
                    }
                }
                
                return partnerObj;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error building partner object for '{partnerText}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the currency ID from a currency code (e.g., "USD" → 1, "EUR" → 2)
        /// </summary>
        private async Task<int?> GetCurrencyIdFromCode(string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode)) return null;
            
            try
            {
                var currency = await _context.Currencies
                    .Where(c => c.Code.ToUpper() == currencyCode.ToUpper() && !c.IsDeleted)
                    .Select(c => new { c.Id })
                    .FirstOrDefaultAsync();
                
                if (currency == null)
                {
                    Console.WriteLine($"[WARNING] Currency not found for code: '{currencyCode}', defaulting to USD lookup");
                    // Try to get USD as fallback
                    currency = await _context.Currencies
                        .Where(c => c.Code.ToUpper() == "USD" && !c.IsDeleted)
                        .Select(c => new { c.Id })
                        .FirstOrDefaultAsync();
                }
                
                return currency?.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error looking up currency '{currencyCode}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Builds a team member object from text value (UNOPS internal staff)
        /// Expected format: "Name - Role" (e.g., "Jane Smith - Project Manager")
        /// </summary>
        private async Task<JObject> BuildTeamMemberObject(string teamMemberText)
        {
            try
            {
                // Extract name from text (before the " - " if present)
                string nameToSearch = teamMemberText;
                string roleHint = null;
                
                if (teamMemberText.Contains(" - "))
                {
                    var parts = teamMemberText.Split(new[] { " - " }, 2, StringSplitOptions.None);
                    nameToSearch = parts[0].Trim();
                    if (parts.Length > 1)
                    {
                        roleHint = parts[1].Trim();
                    }
                }
                
                // Try to find the user by name (supports fuzzy matching via GetEntityIdFromText)
                var userId = await GetEntityIdFromText(nameToSearch, "userIds");
                
                if (userId == null || userId is DBNull)
                {
                    Console.WriteLine($"[WARNING] Team member user not found: '{nameToSearch}' (original: '{teamMemberText}')");
                    return null;
                }
                
                // Cast to int for database query
                int userIdInt = Convert.ToInt32(userId);
                
                // Get full user details from database - PAOUser has UserProfile for name details
                var user = await _context.PAOUsers
                    .Include(u => u.UserProfile)
                    .Where(u => u.Id == userIdInt)
                    .Select(u => new 
                    { 
                        u.Id, 
                        FirstName = u.UserProfile != null ? u.UserProfile.FirstName : null, 
                        LastName = u.UserProfile != null ? u.UserProfile.LastName : null, 
                        u.Email,
                        ProfilePosition = u.UserProfile != null ? u.UserProfile.Position : null
                    })
                    .FirstOrDefaultAsync();
                
                if (user == null)
                {
                    Console.WriteLine($"[WARNING] User with ID {userIdInt} not found in database");
                    return null;
                }
                
                // Build team member object
                // Note: EntityRoleId will need to be set by the frontend or backend based on role selection
                string userName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(userName))
                {
                    userName = user.Email?.Split('@').FirstOrDefault() ?? "Unknown";
                }
                
                return new JObject
                {
                    ["userId"] = user.Id,
                    ["userName"] = userName,
                    ["userEmail"] = user.Email,
                    ["userTitle"] = user.ProfilePosition ?? roleHint ?? "Team Member",
                    ["entityRoleId"] = null, // To be filled by frontend/backend
                    ["entityRoleName"] = roleHint ?? "Team Member" // Suggested role from AI extraction
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error building team member object for '{teamMemberText}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Builds a stakeholder object from AI-extracted data
        /// AI now returns stakeholders as objects with userName and roleName
        /// </summary>
        /// <param name="stakeholderData">JToken containing either a JSON object {userName, roleName} or a legacy string</param>
        private async Task<JObject> BuildStakeholderObject(JToken stakeholderData)
        {
            try
            {
                string userName = null;
                string roleName = null;
                
                // Handle new JSON object format: { "userName": "John Doe", "roleName": "Opportunity Manager" }
                if (stakeholderData is JObject stakeholderObj)
                {
                    userName = stakeholderObj["userName"]?.ToString();
                    roleName = stakeholderObj["roleName"]?.ToString();
                }
                // Handle legacy string format: "John Doe - Project Manager"
                else if (stakeholderData is JValue stakeholderValue && stakeholderValue.Type == JTokenType.String)
                {
                    var textValue = stakeholderValue.ToString();
                    if (textValue.Contains(" - "))
                    {
                        var parts = textValue.Split(new[] { " - " }, 2, StringSplitOptions.None);
                        userName = parts[0].Trim();
                        if (parts.Length > 1)
                        {
                            roleName = parts[1].Trim();
                        }
                    }
                    else
                    {
                        userName = textValue;
                    }
                }
                
                if (string.IsNullOrWhiteSpace(userName))
                {
                    Console.WriteLine($"[WARNING] Stakeholder userName is empty. Skipping.");
                    return null;
                }
                
                // Try to find the user by name using similarity search
                var userId = await GetEntityIdFromText(userName, "userIds");
                
                if (userId == null || userId is DBNull)
                {
                    Console.WriteLine($"[WARNING] Stakeholder user not found: '{userName}'. Skipping.");
                    return null;
                }
                
                int userIdInt = Convert.ToInt32(userId);
                
                // Get full user details from database
                var user = await _context.PAOUsers
                    .Include(u => u.UserProfile)
                    .Where(u => u.Id == userIdInt)
                    .Select(u => new 
                    { 
                        u.Id, 
                        FirstName = u.UserProfile != null ? u.UserProfile.FirstName : null, 
                        LastName = u.UserProfile != null ? u.UserProfile.LastName : null, 
                        u.Email
                    })
                    .FirstOrDefaultAsync();
                
                if (user == null)
                {
                    Console.WriteLine($"[WARNING] User with ID {userIdInt} not found in database");
                    return null;
                }
                
                // Try to resolve the entity role ID from the role name
                int? entityRoleId = null;
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    entityRoleId = await GetEntityRoleIdForOpportunity(roleName);
                }
                
                // If no role matched, default to "Internal Stakeholder"
                if (!entityRoleId.HasValue)
                {
                    Console.WriteLine($"[INFO] Role '{roleName}' not found. Using 'Internal Stakeholder' as default.");
                    entityRoleId = await GetEntityRoleIdForOpportunity("Internal Stakeholder");
                    roleName = "Internal Stakeholder";
                }
                
                string resolvedUserName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(resolvedUserName))
                {
                    resolvedUserName = user.Email?.Split('@').FirstOrDefault() ?? userName;
                }
                
                return new JObject
                {
                    ["userId"] = user.Id,
                    ["userName"] = resolvedUserName,
                    ["userEmail"] = user.Email,
                    ["entityRoleId"] = entityRoleId,
                    ["entityRoleName"] = roleName
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error building stakeholder object: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets the EntityRole ID for an Opportunity role by name using similarity search
        /// </summary>
        private async Task<int?> GetEntityRoleIdForOpportunity(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return null;
            
            try
            {
                // Use PostgreSQL similarity search to find matching EntityRole
                var roleId = await _context.EntityRoles
                    .Where(er => er.EntityType == "Opportunity" && !er.IsDeleted)
                    .OrderByDescending(er => EF.Functions.TrigramsSimilarity(er.Name.ToLower(), roleName.ToLower()))
                    .Select(er => (int?)er.Id)
                    .FirstOrDefaultAsync();
                
                if (roleId.HasValue)
                {
                    Console.WriteLine($"[INFO] Resolved role '{roleName}' to EntityRoleId: {roleId}");
                }
                else
                {
                    Console.WriteLine($"[WARNING] No EntityRole found for role name: '{roleName}'");
                }
                
                return roleId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error resolving EntityRole for '{roleName}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Builds a deliverable object from text value
        /// </summary>
        private async Task<JObject> BuildDeliverableObject(string deliverableText)
        {
            try
            {
                // Try to find the deliverable/output in the database
                var outputId = await GetEntityIdFromText(deliverableText, "deliverables");
                
                // Only include deliverable if outputId was found
                if (outputId == null || outputId is DBNull)
                {
                    Console.WriteLine($"[WARNING] Deliverable/Output not found in database: '{deliverableText}'. Skipping.");
                    return null;
                }
                
                // Cast to int for database query
                int outputIdInt = Convert.ToInt32(outputId);
                
                // Get full output details from database (include both Level names and Definitions)
                var output = await _context.Outputs
                    .Where(o => o.Id == outputIdInt)
                    .Select(o => new { 
                        o.Id, 
                        o.Name, 
                        o.Level0, 
                        o.Level1, 
                        o.Level2, 
                        o.Level3, 
                        o.Level4,
                        o.DefinitionLevel1, 
                        o.DefinitionLevel2, 
                        o.DefinitionLevel3, 
                        o.DefinitionLevel4,
                        o.ServiceLine
                    })
                    .FirstOrDefaultAsync();
                
                if (output == null)
                {
                    Console.WriteLine($"[WARNING] Output with ID {outputIdInt} not found in database");
                    return null;
                }
                
                // Return deliverable object with all level information for frontend display
                return new JObject
                {
                    ["outputId"] = output.Id,
                    ["outputName"] = output.Name,
                    ["level0"] = output.Level0 ?? "",
                    ["level1"] = output.Level1 ?? "",
                    ["level2"] = output.Level2 ?? "",
                    ["level3"] = output.Level3 ?? "",
                    ["level4"] = output.Level4 ?? "",
                    ["definitionLevel1"] = output.DefinitionLevel1 ?? "",
                    ["definitionLevel2"] = output.DefinitionLevel2 ?? "",
                    ["definitionLevel3"] = output.DefinitionLevel3 ?? "",
                    ["definitionLevel4"] = output.DefinitionLevel4 ?? "",
                    ["serviceLine"] = output.ServiceLine ?? "",
                    ["quantity"] = null
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error building deliverable object for '{deliverableText}': {ex.Message}");
                return null;
            }
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