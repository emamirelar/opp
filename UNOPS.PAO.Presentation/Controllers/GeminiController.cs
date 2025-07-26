namespace UNOPS.PAO.Presentation.Controllers;

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.Domain.Entities;
using Google.Cloud.Vision.V1;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using Humanizer;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

public class SearchResult
{
    public int EntityId { get; set; }
    public float Score { get; set; }
    public string SearchType { get; set; }
}

[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class GeminiController : BaseController
{
    private readonly IGeminiManager _manager;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _agenticAiServiceUrl;

    public GeminiController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<GeminiController> logger,
        UNOPSAppDbContext context,
        AiContextualService aiService,
        IPermissionService permissionService,
        HttpClient httpClient,
        IConfiguration configuration)
        : base(logger, authorizationService, userResolverService, permissionService, context, aiService)
    {
        _manager = manager.GeminiManager;
        _httpClient = httpClient;
        _configuration = configuration;
        _agenticAiServiceUrl = _configuration.GetValue<string>("AgenticAi:ServiceURL");
    }

    [HttpPost(APIDictionary.AiAssistantGetUserSessions)]
    public async Task<ActionResult> GetUserSessions() 
    {
        return await HandleOperationAsync(async () => 
        {
            var sessionData = (await _manager.GetUserSessions(CurrentUserId)).ToList();
            return sessionData;
        });
    }

    [HttpPost(APIDictionary.AiAssistantGetSession)]
    public async Task<ActionResult> GetSessionDetails([FromBody] GeminiSessionRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            var sessionData = await _manager.GetSessionDataWithChats(req.sessionId, CurrentUserId);
            return sessionData;
        });
    }

    [HttpPost(APIDictionary.AiAssistantChat)]
    public async Task<ActionResult> ChatWithGemini([FromForm] GeminiAssistantRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            // Debug logging for file upload
            if (req.Files != null && req.Files.Any())
            {
                Console.WriteLine($"📎 [CONTROLLER] Received {req.Files.Count()} files:");
                foreach (var file in req.Files)
                {
                    Console.WriteLine($"   - {file.FileName} ({file.ContentType}, {file.Length} bytes)");
                }
            }
            else
            {
                Console.WriteLine("📎 [CONTROLLER] No files received in request");
            }
            
            return await _manager.ChatWithGemini(req, User, Request.Headers);
        });
    }

    [HttpPost(APIDictionary.GeminiFileScan)]
    public async Task<ActionResult> ScanFile([FromForm] GeminiFileRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req?.File == null || req?.File.Length == 0)
            {
                throw new BusinessException("No valid file detected.");
            }

            string fileType = _manager.FindFileType(req.File);

            if (string.IsNullOrEmpty(fileType)) 
            {
                throw new BusinessException("File type not compatible");
            }

            string response = await _manager.ScanFileForGeminiProcessing(req);

            if (string.IsNullOrEmpty(response))
            {
                throw new BusinessException("Prompt configuration for the screen is not found.");
            }

            return response.Trim();
        });
    }

    [HttpPost(APIDictionary.GeminiProcessDataSummary)]
    // Internal call: Process Data Related Summary
    public async Task<ActionResult> ProcessDataRelatedSummaryDetails([FromBody] GeminiProcessDataRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || req?.Id == null)
            {
                throw new BusinessException("Invalid request");
            }

            var response = await _manager.ProcessDataRelatedSummaryDetails(req);
            
            if (string.IsNullOrEmpty(response))
            {
                throw new BusinessException("Prompt configuration for the screen is not found.");
            }

            return response.Trim();
        });
    }

    [HttpPost(APIDictionary.AiAssistantAccessibility)]
    public async Task<ActionResult> UpdateAiAssistantAccessibility([FromBody] GeminiAccessibilityRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            var success = await _manager.UpdateAiAssistantAccessibility(req);
            return new { success = success };
        });
    }

    [HttpPost(APIDictionary.AiAssistantUpdateStar)]
    public async Task<ActionResult> UpdateSessionStar([FromBody] SessionStarRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.SessionId))
            {
                throw new BusinessException("Invalid request");
            }

            var success = await _manager.UpdateSessionStar(req.SessionId, req.Starred);
            return new { success = success };
        });
    }

    [HttpPost(APIDictionary.AiAssistantUpdateArchive)]
    public async Task<ActionResult> UpdateSessionArchive([FromBody] SessionArchiveRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.SessionId))
            {
                throw new BusinessException("Invalid request");
            }

            var success = await _manager.UpdateSessionArchive(req.SessionId, req.Archived);
            return new { success = success };
        });
    }

    [HttpPost(APIDictionary.AiAssistantUpdateTitle)]
    public async Task<ActionResult> UpdateSessionTitle([FromBody] SessionTitleRequest req)
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.SessionId) || string.IsNullOrEmpty(req.Title))
            {
                throw new BusinessException("Invalid request");
            }

            var success = await _manager.UpdateSessionTitle(req.SessionId, req.Title.Trim());
            return new { success = success };
        });
    }

    [HttpGet(APIDictionary.GenerateEmbeddings)]
    public async Task<ActionResult> GenerateAndStoreEmbeddings(string? entityName)
    {
        return await HandleOperationAsync(async () => 
        {
            await _manager.GenerateEmbeddings(entityName);
            return new { message = $"Embeddings generated and published'." };
        });
    }

    [HttpPost(APIDictionary.BulkUpload)]
    public async Task<ActionResult> BulkUpload([FromBody] BulkUploadRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.Type))
            {
                throw new BusinessException("Invalid request.");
            }

            // Call the updated BulkInsertRecordsAsync method
            string response = await _manager.BulkInsertRecordsAsync(req);
            return new { message = response };
        });
    }

    [HttpPost(APIDictionary.AnalyseFile)]
    public async Task<ActionResult> AnalyseFile([FromBody]AnalyseFileRequest request)
    {
        return await HandleOperationAsync(async () => 
        {
            if (request == null)
            {
                throw new BusinessException("Invalid request.");
            }

            return await _manager.ExtractDataAfterAnalysis(request, CurrentUserId);
        });
    }

    /// <summary>
    /// Performs semantic/similarity search with RBAC filtering
    /// </summary>
    /// <param name="entityName">Name of the entity to search (e.g., "Partners", "Contacts")</param>
    /// <param name="searchText">Text to search for</param>
    /// <param name="vectorEmbedding">Optional vector embedding for semantic search</param>
    /// <param name="similarityThreshold">Threshold for similarity search (default: 0.3)</param>
    /// <param name="embeddingThreshold">Threshold for embedding search (default: 0.7)</param>
    /// <param name="whereCondition">Optional additional WHERE condition</param>
    /// <returns>Search results filtered by RBAC permissions</returns>
    [HttpGet("get-similarity-result")]
    public async Task<ActionResult> GetSimilarityResultAsync(
        [FromQuery] string entityName,
        [FromQuery] string searchText,
        [FromQuery] string vectorEmbedding = null,
        [FromQuery] float similarityThreshold = 0.3f,
        [FromQuery] float embeddingThreshold = 0.7f,
        [FromQuery] string whereCondition = null)
    {
        return await HandleOperationAsync(async () =>
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(entityName))
            {
                throw new BusinessException("EntityName is required");
            }
        });

            /*if (string.IsNullOrEmpty(searchText) && string.IsNullOrEmpty(vectorEmbedding))
            {
                return BadRequest(new { error = "Either searchText or vectorEmbedding is required" });
            }

            // Check if required services are available
            if (_aiService == null)
            {
                return StatusCode(500, new { error = "AI search service is not available" });
            }

            if (_context == null)
            {
                return StatusCode(500, new { error = "Database context is not available" });
            }

            if (_permissionService == null)
            {
                return StatusCode(500, new { error = "Permission service is not available" });
            }

            try
            {
                // Step 1: Get similarity search results
                // Ensure entityName is pluralized and capitalized for the retrieve function
                var pluralizedEntityName = entityName.Pluralize().Transform(To.TitleCase);
                
                var searchResults = await _aiService.RetrieveSimilarityIds(
                    pluralizedEntityName, 
                    searchText, 
                    vectorEmbedding, 
                    similarityThreshold, 
                    embeddingThreshold, 
                    whereCondition);

                if (searchResults == null || !searchResults.Any())
                {
                    return Ok(new { 
                        message = "No results found matching the search criteria",
                        results = new List<object>(),
                        totalCount = 0
                    });
                }

                // Step 2: Extract entity IDs from search results
                var entityIds = searchResults.Select(r => r.EntityId).ToList();

                if (!entityIds.Any())
                {
                    return Ok(new { 
                        message = "No valid entity IDs found in search results",
                        results = new List<object>(),
                        totalCount = 0
                    });
                }

                // Step 3: Use reflection to get the appropriate DbSet
                var dbSetEntityName = entityName.EndsWith("s") ? entityName : entityName + "s";
                var dbSetProperty = _context.GetType()
                    .GetProperty(dbSetEntityName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (dbSetProperty == null)
                {
                    // Try with original entity name if pluralized doesn't work
                    dbSetProperty = _context.GetType()
                        .GetProperty(entityName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }

                if (dbSetProperty == null)
                {
                    return BadRequest(new { error = $"Entity type '{entityName}' not found" });
                }

                var dbSet = dbSetProperty.GetValue(_context);
                if (dbSet == null)
                {
                    return StatusCode(500, new { error = $"Unable to retrieve DbSet for entity '{entityName}'" });
                }

                // Step 4: Get the entity type for RBAC
                var entityType = dbSetProperty.PropertyType.GetGenericArguments().FirstOrDefault();
                if (entityType == null)
                {
                    return StatusCode(500, new { error = $"Unable to determine entity type for '{entityName}'" });
                }

                // Step 5-7: Use reflection to build and execute the complete query pipeline
                var queryBuilderMethod = typeof(GeminiController)
                    .GetMethod(nameof(BuildAndExecuteQuery), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(entityType);

                var singularizedEntityName = entityName.Singularize().Transform(To.TitleCase);
                
                var authorizedResults = await (Task<IEnumerable<object>>)queryBuilderMethod.Invoke(
                    this, 
                    new object[] { dbSet, entityIds, singularizedEntityName });

                // Step 8: Combine with search scores and metadata
                var scoreLookup = searchResults.ToDictionary(
                    r => r.EntityId, 
                    r => new { 
                        Score = r.Score, 
                        SearchType = r.SearchType 
                    });

                var finalResults = authorizedResults.Select(entity =>
                {

                    var idProperties = entity.GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                        .Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var entityId = idProperties
                        .Select(p => (int)p.GetValue(entity))
                        .FirstOrDefault(value => value != 0); // Take the first non-zero Id
                        
                    var searchMetadata = scoreLookup.TryGetValue(entityId, out var metadata) ? metadata : null;

                    return new
                    {
                        Entity = entity,
                        SearchScore = searchMetadata?.Score,
                        SearchType = searchMetadata?.SearchType,
                        EntityId = entityId
                    };
                })
                .OrderByDescending(r => r.SearchScore)
                .ToList();

                return Ok(new
                {
                    message = $"Found {finalResults.Count} authorized results out of {searchResults.Count} total matches",
                    results = finalResults,
                    totalCount = finalResults.Count,
                    totalSearchMatches = searchResults.Count,
                    searchMetadata = new
                    {
                        entityName,
                        searchText,
                        hasEmbedding = !string.IsNullOrEmpty(vectorEmbedding),
                        similarityThreshold,
                        embeddingThreshold,
                        whereCondition
                    }
                });*/
            throw new BusinessException("Not implemented");
    }

    [HttpGet(APIDictionary.AiAssistantGenerateTitle)]
    public async Task<ActionResult> GenerateTitle([FromQuery] string sessionId)
    {
        return await HandleOperationAsync(async () =>
        {
            var title = await _manager.GenerateTitle(sessionId, CurrentUserId);
            return new { title };
        });
    }

    public class GenerateTitleResponse
    {
        public string session_id { get; set; }
        public string formatted_conversation { get; set; }
        public string title { get; set; }
    }

    /// <summary>
    /// Helper method to build and execute the query with proper generic typing
    /// </summary>
    private async Task<IEnumerable<object>> BuildAndExecuteQuery<T>(object dbSet, List<int> entityIds, string singularizedEntityName) where T : class
    {
        var typedDbSet = (IQueryable<T>)dbSet;
        
        // Find the correct Id property to avoid ambiguity
        var idProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && p.PropertyType == typeof(int))
            .ToList();
        
        PropertyInfo idPropertyInfo;
        if (idProperties.Count == 1)
        {
            idPropertyInfo = idProperties.First();
        }
        else if (idProperties.Count > 1)
        {
            // Prefer properties declared on the actual type over inherited ones
            idPropertyInfo = idProperties
                .OrderBy(p => p.DeclaringType == typeof(T) ? 0 : 1)
                .First();
        }
        else
        {
            throw new InvalidOperationException($"No 'Id' property found on entity type {typeof(T).Name}");
        }

        // Build the query with proper typing
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var property = System.Linq.Expressions.Expression.Property(parameter, idPropertyInfo);
        var containsMethod = typeof(List<int>).GetMethod(nameof(List<int>.Contains));
        var containsCall = System.Linq.Expressions.Expression.Call(
            System.Linq.Expressions.Expression.Constant(entityIds), 
            containsMethod, 
            property);
        var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(containsCall, parameter);

        // Apply the where condition with proper typing
        var filteredQuery = typedDbSet.Where(lambda);

        // Apply RBAC filtering with proper typing
        var rbacFilteredQueryResult = await _permissionService.ApplyAccessControlFiltersAsync<T>(
            filteredQuery, 
            User, 
            "read", 
            singularizedEntityName);

        // Handle both cases: IQueryable<T> or List<T>
        List<T> results;
        if (rbacFilteredQueryResult is IQueryable<T> queryable)
        {
            // If it's still a queryable, execute it
            results = await queryable.ToListAsync();
        }
        else if (rbacFilteredQueryResult is List<T> list)
        {
            // If it's already materialized as a list, use it directly
            results = list;
        }
        else
        {
            // Try to cast it as IEnumerable<T> and convert to list
            results = ((IEnumerable<T>)rbacFilteredQueryResult).ToList();
        }

        return results.Cast<object>();
    }
}
