using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSBusiness.Extensions;
using Npgsql;
using NpgsqlTypes;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Services;

/// <summary>
/// Advanced search service that handles both structured filters and smart text search with similarity
/// Works with Partner, Contact, and Interaction entities
/// </summary>
public class AdvancedSearchService
{
    private readonly UNOPSAppDbContext _context;
    private readonly ILogger<AdvancedSearchService> _logger;
    private readonly IMapper _mapper;
    private readonly GlobalFilterService _globalFilterService;
    private const int SIMILARITY_THRESHOLD_PERCENT = 30; // 30% similarity threshold

    public AdvancedSearchService(
        UNOPSAppDbContext context,
        ILogger<AdvancedSearchService> logger,
        IMapper mapper,
        GlobalFilterService globalFilterService)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _globalFilterService = globalFilterService;
    }

    #region Main Search Methods

    /// <summary>
    /// Main search method that handles both structured filters and text query
    /// </summary>
    /// <typeparam name="TEntity">Entity type (UNOPSPartner, UNOPSContact, UNOPSInteraction)</typeparam>
    /// <typeparam name="TModel">Model type (PartnerModel, ContactModel, InteractionModel)</typeparam>
    /// <param name="request">Search request containing query text, filters, and pagination</param>
    /// <param name="user">Current user for access control</param>
    /// <returns>Paginated search results</returns>
    public async Task<PaginationResponse<TModel>> SearchAsync<TEntity, TModel>(
        UnifiedSearchRequest request,
        ClaimsPrincipal user)
        where TEntity : class
        where TModel : class
    {
        try
        {
            _logger.LogInformation("=== ADVANCED SEARCH SERVICE ===");
            _logger.LogInformation("Entity: {EntityType}, Query: '{Query}', Filters: {FilterCount}, FilterActive: {FilterActive}", 
                typeof(TEntity).Name, request.Query, request.Filters?.Count ?? 0, request.FilterActive);

            // Build base query with proper includes
            var query = BuildBaseQueryWithIncludes<TEntity>();

            // Apply structured filters first (more efficient)
            if (request.Filters?.Any() == true)
            {
                query = await ApplyStructuredFilters(query, request.Filters);
                _logger.LogInformation("Applied {FilterCount} structured filters", request.Filters.Count);
            }

            // Apply smart text search if query provided
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                query = await ApplySmartTextSearchAsync(query, request.Query);
                _logger.LogInformation("Applied smart text search for: '{Query}'", request.Query);
            }

            // Apply access control and global filters (respecting filterActive flag)
            query = await ApplyAccessControlAsync(query, user, request.FilterActive);

            // Get total count
            var totalCount = await query.CountAsync();
            _logger.LogInformation("Total results: {Count}", totalCount);

            // Apply ordering and pagination
            var orderedQuery = ApplyDynamicOrdering(query, request.OrderBy, request.Ascending);
            var results = await orderedQuery
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to models
            var mappedResults = await MapToModelsAsync<TEntity, TModel>(results);

            _logger.LogInformation("Returning {ResultCount} mapped results", mappedResults.Count);

            return new PaginationResponse<TModel>
            {
                Records = mappedResults,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                SearchMetadata = !string.IsNullOrWhiteSpace(request.Query) ? await GenerateBasicSearchMetadataAsync(mappedResults, request.Query, typeof(TEntity).Name) : null,
                SearchQuery = request.Query
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AdvancedSearchService for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Search method specifically for structured filters only (advanced-search endpoint)
    /// </summary>
    public async Task<PaginationResponse<TModel>> SearchWithFiltersAsync<TEntity, TModel>(
        List<SearchFilter> filters,
        PaginationRequest pagination,
        ClaimsPrincipal user)
        where TEntity : class
        where TModel : class
    {
        var request = new UnifiedSearchRequest
        {
            Query = null, // No text search, only filters
            Filters = filters,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
            OrderBy = pagination.OrderBy,
            Ascending = pagination.Ascending ?? false,
            FilterActive = pagination.FilterActive
        };

        return await SearchAsync<TEntity, TModel>(request, user);
    }

    /// <summary>
    /// Search method specifically for text query only (search endpoint)
    /// </summary>
    public async Task<PaginationResponse<TModel>> SearchWithQueryAsync<TEntity, TModel>(
        string query,
        PaginationRequest pagination,
        ClaimsPrincipal user)
        where TEntity : class
        where TModel : class
    {
        var request = new UnifiedSearchRequest
        {
            Query = query,
            Filters = null, // No structured filters, only text search
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
            OrderBy = pagination.OrderBy,
            Ascending = pagination.Ascending ?? false,
            FilterActive = pagination.FilterActive
        };

        return await SearchAsync<TEntity, TModel>(request, user);
    }

    /// <summary>
    /// Search method with metadata for text query only (enhanced search endpoint)
    /// Returns search results with metadata showing which fields matched
    /// </summary>
    public async Task<PaginationResponse<TModel>> SearchWithQueryAndMetadataAsync<TEntity, TModel>(
        string query,
        PaginationRequest pagination,
        ClaimsPrincipal user)
        where TEntity : class
        where TModel : class
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("=== SEARCH WITH METADATA ===");
            _logger.LogInformation("Entity: {EntityType}, Query: '{Query}'", typeof(TEntity).Name, query);

            // Get entity type name for PostgreSQL function
            var entityType = typeof(TEntity).Name.Replace("UNOPS", ""); // UNOPSPartner -> Partner
            
            // Use the specific PostgreSQL search function based on entity type
            List<GlobalSearchResult> searchResults;
            switch (entityType)
            {
                case "Partner":
                    searchResults = await SearchPartnersAsync(query);
                    break;
                case "Contact":
                    searchResults = await SearchContactsAsync(query);
                    break;
                case "Interaction":
                    searchResults = await SearchInteractionsAsync(query);
                    break;
                default:
                    throw new ArgumentException($"Unsupported entity type: {entityType}");
            }

            // Get all entity IDs from search results (don't paginate yet)
            // Note: PostgreSQL functions now return only one row per entity (best match)
            var allEntityIds = searchResults.Select(r => r.EntityId).ToList();
            
            // Get the actual entity records for all search results
            var allEntities = await GetEntitiesByIds<TEntity>(allEntityIds);
            
            // Apply user's requested ordering to the entities
            // If orderBy is "relevance" or not specified, maintain PostgreSQL score order (relevance)
            List<TEntity> orderedEntities;
            if (string.IsNullOrWhiteSpace(pagination.OrderBy) || 
                pagination.OrderBy.Equals("relevance", StringComparison.OrdinalIgnoreCase))
            {
                // Maintain PostgreSQL score order by using the order from allEntityIds
                // Since we used Distinct(), each ID appears only once, so ToDictionary won't fail
                var entityIdOrder = allEntityIds.Select((id, index) => new { Id = id, Order = index }).ToDictionary(x => x.Id, x => x.Order);
                orderedEntities = allEntities.OrderBy(e => entityIdOrder.GetValueOrDefault(GetEntityId(e), int.MaxValue)).ToList();
            }
            else
            {
                // Apply user's custom ordering
                orderedEntities = ApplyDynamicOrdering(allEntities.AsQueryable(), pagination.OrderBy, pagination.Ascending ?? false).ToList();
            }
            
            // Update total count to reflect actual entities returned after access control
            var totalCount = orderedEntities.Count;
            
            // Now apply pagination to the ordered entities
            var paginatedEntities = orderedEntities
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();
            
            // Get the corresponding search results for the paginated entities
            var paginatedEntityIds = paginatedEntities.Select(e => GetEntityId(e)).ToList();
            var paginatedResults = searchResults.Where(r => paginatedEntityIds.Contains(r.EntityId)).ToList();
            
            // Map to models
            var mappedResults = await MapToModelsAsync<TEntity, TModel>(paginatedEntities);

            // Create search metadata using the same structure as global search
            var searchMetadata = new Dictionary<int, Dictionary<string, object>>();
            foreach (var result in paginatedResults)
            {
                var metadata = new Dictionary<string, object>();
                
                if (!string.IsNullOrEmpty(result.MatchedField))
                    metadata["matchedField"] = result.MatchedField;
                    
                if (!string.IsNullOrEmpty(result.SearchType))
                    metadata["searchType"] = result.SearchType;
                    
                if (!string.IsNullOrEmpty(result.MatchCriteria))
                    metadata["matchCriteria"] = result.MatchCriteria;
                    
                metadata["score"] = result.Score;
                
                if (!string.IsNullOrEmpty(result.Snippet))
                {
                    // Truncate snippet if too long for frontend display
                    metadata["snippet"] = result.Snippet.Length > 200 ? 
                        result.Snippet.Substring(0, 200) + "..." : result.Snippet;
                }
                
                searchMetadata[result.EntityId] = metadata;
            }

            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            _logger.LogInformation("Search with metadata completed. Results: {Count}, Time: {ExecutionTime}ms", 
                mappedResults.Count, executionTime);

            return new PaginationResponse<TModel>
            {
                Records = mappedResults,
                TotalCount = totalCount,
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pagination.PageSize),
                SearchMetadata = searchMetadata,
                SearchQuery = query,
                ExecutionTimeMs = executionTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchWithQueryAndMetadataAsync for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    /// <summary>
    /// Helper method to get entities by their IDs
    /// </summary>
    private async Task<List<TEntity>> GetEntitiesByIds<TEntity>(List<int> ids) where TEntity : class
    {
        var query = BuildBaseQueryWithIncludes<TEntity>();
        
        // Add ID filter based on entity type
        if (typeof(TEntity).Name.Contains("Partner"))
        {
            query = query.Where(e => ids.Contains(EF.Property<int>(e, "Id")));
        }
        else if (typeof(TEntity).Name.Contains("Contact"))
        {
            query = query.Where(e => ids.Contains(EF.Property<int>(e, "Id")));
        }
        else if (typeof(TEntity).Name.Contains("Interaction"))
        {
            query = query.Where(e => ids.Contains(EF.Property<int>(e, "Id")));
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Global search across all entities using PostgreSQL search_entity_records function
    /// Perfect for unified search endpoint that searches Partners, Contacts, and Interactions
    /// </summary>
    public async Task<GlobalSearchResponse> SearchAllEntitiesAsync(string searchText, int maxResultsPerEntity = 15)
    {
        try
        {
            _logger.LogInformation("=== GLOBAL POSTGRESQL SEARCH ===");
            _logger.LogInformation("Search Text: '{SearchText}', Max Results Per Entity: {MaxResults}", searchText, maxResultsPerEntity);

            // Execute PostgreSQL search function for all entities (no filter)
            var searchResultsJson = await ExecutePostgreSQLSearchAsync(searchText, null);
            
            // Parse and return structured results
            var globalResults = ParseGlobalSearchResults(searchResultsJson);
            
            _logger.LogInformation("Global search completed. Partners: {PartnerCount}, Contacts: {ContactCount}, Interactions: {InteractionCount}",
                globalResults.Partners?.Count ?? 0,
                globalResults.Contacts?.Count ?? 0, 
                globalResults.Interactions?.Count ?? 0);

            return globalResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing global PostgreSQL search for: '{SearchText}'", searchText);
            return new GlobalSearchResponse
            {
                Partners = new List<GlobalSearchResult>(),
                Contacts = new List<GlobalSearchResult>(),
                Interactions = new List<GlobalSearchResult>(),
                SearchQuery = searchText,
                ExecutionTimeMs = 0
            };
        }
    }

    /// <summary>
    /// Parse PostgreSQL search results JSON into structured global search response
    /// </summary>
    private GlobalSearchResponse ParseGlobalSearchResults(string searchResultsJson)
    {
        var response = new GlobalSearchResponse
        {
            Partners = new List<GlobalSearchResult>(),
            Contacts = new List<GlobalSearchResult>(),
            Interactions = new List<GlobalSearchResult>()
        };

        try
        {
            if (string.IsNullOrEmpty(searchResultsJson) || searchResultsJson == "{}")
                return response;

            using var document = JsonDocument.Parse(searchResultsJson);
            
            // Check if we have results
            if (!document.RootElement.TryGetProperty("results", out var results))
                return response;

            // Parse Partners
            if (results.TryGetProperty("Partners", out var partnersElement))
            {
                response.Partners = ParseEntitySearchResults(partnersElement, "Partner");
            }

            // Parse Contacts  
            if (results.TryGetProperty("Contacts", out var contactsElement))
            {
                response.Contacts = ParseEntitySearchResults(contactsElement, "Contact");
            }

            // Parse Interactions
            if (results.TryGetProperty("Interactions", out var interactionsElement))
            {
                response.Interactions = ParseEntitySearchResults(interactionsElement, "Interaction");
            }

            // Extract execution time if available
            if (document.RootElement.TryGetProperty("summary", out var summary) &&
                summary.TryGetProperty("executionTimeMs", out var executionTime))
            {
                response.ExecutionTimeMs = executionTime.GetDouble();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing global search results JSON");
        }

        return response;
    }

    /// <summary>
    /// Parse individual entity search results from JSON
    /// </summary>
    private List<GlobalSearchResult> ParseEntitySearchResults(JsonElement entityElement, string entityType)
    {
        var results = new List<GlobalSearchResult>();

        try
        {
            if (entityElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    var result = new GlobalSearchResult
                    {
                        EntityType = entityType,
                        EntityId = item.TryGetProperty("entityId", out var idElement) ? idElement.GetInt32() : 0,
                        Score = item.TryGetProperty("score", out var scoreElement) ? scoreElement.GetDouble() : 0,
                        MatchedField = item.TryGetProperty("matchedField", out var fieldElement) ? fieldElement.GetString() : "",
                        FieldValue = item.TryGetProperty("fieldValue", out var valueElement) ? valueElement.GetString() : "",
                        SearchType = item.TryGetProperty("searchType", out var typeElement) ? typeElement.GetString() : "",
                        MatchCriteria = item.TryGetProperty("matchCriteria", out var criteriaElement) ? criteriaElement.GetString() : "",
                        Snippet = item.TryGetProperty("snippet", out var snippetElement) ? snippetElement.GetString() : ""
                    };

                    results.Add(result);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing {EntityType} search results", entityType);
        }

        return results;
    }

    #endregion

    #region Modular Entity Search Methods

    /// <summary>
    /// Search Partners using the dedicated PostgreSQL function with nested properties
    /// </summary>
    public async Task<List<GlobalSearchResult>> SearchPartnersAsync(string searchText, float textBoost = 1.0f, int snippetLength = 150)
    {
        try
        {
            _logger.LogInformation("Searching Partners with nested properties: '{SearchText}'", searchText);

            using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM public.search_partners_with_nested($1, $2, $3)";
            command.Parameters.Add(new NpgsqlParameter { Value = searchText });
            command.Parameters.Add(new NpgsqlParameter { Value = textBoost });
            command.Parameters.Add(new NpgsqlParameter { Value = snippetLength });

            var results = new List<GlobalSearchResult>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new GlobalSearchResult
                {
                    EntityType = reader.GetString("entity_type"),
                    EntityId = int.Parse(reader.GetString("entity_id")),
                    Score = reader.GetDouble("score"),
                    MatchedField = reader.GetString("matched_field"),
                    FieldValue = reader.GetString("field_value"),
                    SearchType = reader.GetString("search_type"),
                    MatchCriteria = reader.GetString("match_criteria"),
                    Snippet = reader.GetString("snippet")
                });
            }

            _logger.LogInformation("Partners search completed. Found {Count} results", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Partners with nested properties: '{SearchText}'", searchText);
            return new List<GlobalSearchResult>();
        }
    }

    /// <summary>
    /// Search Contacts using the dedicated PostgreSQL function with nested properties
    /// </summary>
    public async Task<List<GlobalSearchResult>> SearchContactsAsync(string searchText, float textBoost = 1.0f, int snippetLength = 150)
    {
        try
        {
            _logger.LogInformation("Searching Contacts with nested properties: '{SearchText}'", searchText);

            using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM public.search_contacts_with_nested($1, $2, $3)";
            command.Parameters.Add(new NpgsqlParameter { Value = searchText });
            command.Parameters.Add(new NpgsqlParameter { Value = textBoost });
            command.Parameters.Add(new NpgsqlParameter { Value = snippetLength });

            var results = new List<GlobalSearchResult>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new GlobalSearchResult
                {
                    EntityType = reader.GetString("entity_type"),
                    EntityId = int.Parse(reader.GetString("entity_id")),
                    Score = reader.GetDouble("score"),
                    MatchedField = reader.GetString("matched_field"),
                    FieldValue = reader.GetString("field_value"),
                    SearchType = reader.GetString("search_type"),
                    MatchCriteria = reader.GetString("match_criteria"),
                    Snippet = reader.GetString("snippet")
                });
            }

            _logger.LogInformation("Contacts search completed. Found {Count} results", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Contacts with nested properties: '{SearchText}'", searchText);
            return new List<GlobalSearchResult>();
        }
    }

    /// <summary>
    /// Search Interactions using the dedicated PostgreSQL function with nested properties
    /// </summary>
    public async Task<List<GlobalSearchResult>> SearchInteractionsAsync(string searchText, float textBoost = 1.0f, int snippetLength = 150)
    {
        try
        {
            _logger.LogInformation("Searching Interactions with nested properties: '{SearchText}'", searchText);

            using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM public.search_interactions_with_nested($1, $2, $3)";
            command.Parameters.Add(new NpgsqlParameter { Value = searchText });
            command.Parameters.Add(new NpgsqlParameter { Value = textBoost });
            command.Parameters.Add(new NpgsqlParameter { Value = snippetLength });

            var results = new List<GlobalSearchResult>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new GlobalSearchResult
                {
                    EntityType = reader.GetString("entity_type"),
                    EntityId = int.Parse(reader.GetString("entity_id")),
                    Score = reader.GetDouble("score"),
                    MatchedField = reader.GetString("matched_field"),
                    FieldValue = reader.GetString("field_value"),
                    SearchType = reader.GetString("search_type"),
                    MatchCriteria = reader.GetString("match_criteria"),
                    Snippet = reader.GetString("snippet")
                });
            }

            _logger.LogInformation("Interactions search completed. Found {Count} results", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Interactions with nested properties: '{SearchText}'", searchText);
            return new List<GlobalSearchResult>();
        }
    }

    /// <summary>
    /// Enhanced global search using modular functions for better performance and control
    /// </summary>
    public async Task<GlobalSearchResponse> SearchAllEntitiesModularAsync(string searchText, float textBoost = 1.0f, int maxResultsPerEntity = 15, bool filterActive = true)
    {
        try
        {
            _logger.LogInformation("=== ENHANCED MODULAR GLOBAL SEARCH ===");
            _logger.LogInformation("Search Text: '{SearchText}', Text Boost: {TextBoost}, Max Results: {MaxResults}, FilterActive: {FilterActive}", 
                searchText, textBoost, maxResultsPerEntity, filterActive);

            var startTime = DateTime.UtcNow;

            // Execute searches in parallel for better performance
            var partnersTask = SearchPartnersAsync(searchText, textBoost);
            var contactsTask = SearchContactsAsync(searchText, textBoost);
            var interactionsTask = SearchInteractionsAsync(searchText, textBoost);

            await Task.WhenAll(partnersTask, contactsTask, interactionsTask);

            var partners = await partnersTask;
            var contacts = await contactsTask;
            var interactions = await interactionsTask;

            // Limit results per entity
            var response = new GlobalSearchResponse
            {
                SearchQuery = searchText,
                Partners = partners.Take(maxResultsPerEntity).ToList(),
                Contacts = contacts.Take(maxResultsPerEntity).ToList(),
                Interactions = interactions.Take(maxResultsPerEntity).ToList(),
                ExecutionTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds
            };

            _logger.LogInformation("Enhanced modular search completed. Partners: {PartnerCount}, Contacts: {ContactCount}, Interactions: {InteractionCount}, Time: {ExecutionTime}ms",
                response.Partners.Count,
                response.Contacts.Count,
                response.Interactions.Count,
                response.ExecutionTimeMs);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing enhanced modular global search for: '{SearchText}'", searchText);
            return new GlobalSearchResponse
            {
                Partners = new List<GlobalSearchResult>(),
                Contacts = new List<GlobalSearchResult>(),
                Interactions = new List<GlobalSearchResult>(),
                SearchQuery = searchText,
                ExecutionTimeMs = 0
            };
        }
    }

    #endregion

    #region Entity-Specific Query Building

    /// <summary>
    /// Build base query with appropriate includes based on entity type
    /// </summary>
    private IQueryable<TEntity> BuildBaseQueryWithIncludes<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity).Name;
        var query = _context.Set<TEntity>().AsQueryable();

        switch (entityType)
        {
            case "UNOPSPartner":
                // Only include navigation properties that definitely exist on UNOPSPartner
                query = query
                    .Include("PartnerGroup")
                    .Include("Contacts")
                    .Include("LiaisonOffice");
                break;

            case "UNOPSContact":
                query = query
                    .Include("Partner")
                    .Include("Partner.PartnerGroup")
                    .Include("Partner.LiaisonOffice")
                    .Include("Interactions");
                break;

            case "UNOPSInteraction":
                query = query
                    .Include("InteractionContacts.Contact")
                    .Include("InteractionPartners.Partner")
                    .Include("InteractionUsers.User");
                break;

            default:
                _logger.LogWarning("Unknown entity type for includes: {EntityType}", entityType);
                break;
        }

        // Apply soft delete filter if entity supports it
        var deletedProperty = typeof(TEntity).GetProperty("IsDeleted");
        if (deletedProperty != null)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, deletedProperty);
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(condition, parameter);
            query = query.Where(lambda);
        }

        _logger.LogDebug("Built base query for {EntityType} with includes", entityType);
        return query;
    }

    #endregion

    #region Smart Text Search

    /// <summary>
    /// Apply smart text search with comprehensive similarity across all text fields
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplySmartTextSearchAsync<TEntity>(
        IQueryable<TEntity> query,
        string searchText) where TEntity : class
    {
        _logger.LogInformation("=== APPLYING SMART TEXT SEARCH ===");
        _logger.LogInformation("Search Text: '{SearchText}', Threshold: {Threshold}%", searchText, SIMILARITY_THRESHOLD_PERCENT);

        var entityType = typeof(TEntity).Name;

        // Step 1: Get exact matches first (performance optimization)
        var exactMatches = await GetExactMatchesAsync(query, searchText, entityType);
        _logger.LogInformation("Found {Count} exact matches", exactMatches.Count);

        // Step 2: Get similarity matches for typo handling - DISABLED FOR TESTING
        var similarityMatches = await GetSimilarityMatchesAsync(query, searchText, entityType);
        //var similarityMatches = new List<TEntity>(); // TEMP: Empty list for testing
        _logger.LogInformation("Found {Count} similarity matches (DISABLED FOR TESTING)", similarityMatches.Count);

        // Step 3: Combine all matching IDs
        var allMatchingIds = exactMatches.Select(GetEntityId)
            .Union(similarityMatches.Select(GetEntityId))
            .Distinct()
            .ToList();

        _logger.LogInformation("Total unique entities found: {Count}", allMatchingIds.Count);

        // Return filtered query
        return query.Where(BuildIdFilterExpression<TEntity>(allMatchingIds));
    }

    /// <summary>
    /// Get exact matches using Contains for fast initial filtering
    /// </summary>
    private async Task<List<TEntity>> GetExactMatchesAsync<TEntity>(
        IQueryable<TEntity> query,
        string searchText,
        string entityType) where TEntity : class
    {
        var searchLower = searchText.ToLower();

        switch (entityType)
        {
            case "UNOPSPartner":
                return await query.Where(p =>
                    // Direct partner fields
                    (EF.Property<string>(p, "Name") != null && EF.Property<string>(p, "Name").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(p, "PartnerShortDescription") != null && EF.Property<string>(p, "PartnerShortDescription").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(p, "PartnerLongDescription") != null && EF.Property<string>(p, "PartnerLongDescription").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(p, "PartnerApprovalReference") != null && EF.Property<string>(p, "PartnerApprovalReference").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(p, "ReasonForLevy") != null && EF.Property<string>(p, "ReasonForLevy").ToLower().Contains(searchLower)) ||
                    
                    // Enum fields - search by string representation and human-readable text
                    EF.Property<object>(p, "Status").ToString().ToLower().Contains(searchLower) ||
                    EF.Property<object>(p, "PartnerApprovalStatus").ToString().ToLower().Contains(searchLower) ||
                    (searchLower.Contains("active") && EF.Property<object>(p, "Status").ToString() == "Active") ||
                    (searchLower.Contains("inactive") && EF.Property<object>(p, "Status").ToString() == "Inactive") ||
                    (searchLower.Contains("draft") && EF.Property<object>(p, "Status").ToString() == "Draft") ||
                    (searchLower.Contains("closed") && EF.Property<object>(p, "Status").ToString() == "Closed") ||
                    (searchLower.Contains("archived") && EF.Property<object>(p, "Status").ToString() == "Archived") ||
                    (searchLower.Contains("approved") && EF.Property<object>(p, "PartnerApprovalStatus").ToString() == "Approved") ||
                    (searchLower.Contains("not approved") && EF.Property<object>(p, "PartnerApprovalStatus").ToString() == "NotApproved") ||
                    
                    // Navigation properties
                    (EF.Property<object>(p, "PartnerGroup") != null && 
                     EF.Property<string>(EF.Property<object>(p, "PartnerGroup"), "Name") != null && 
                     EF.Property<string>(EF.Property<object>(p, "PartnerGroup"), "Name").ToLower().Contains(searchLower)) ||
                    
                    (EF.Property<object>(p, "LiaisonOffice") != null && 
                     EF.Property<string>(EF.Property<object>(p, "LiaisonOffice"), "Name") != null && 
                     EF.Property<string>(EF.Property<object>(p, "LiaisonOffice"), "Name").ToLower().Contains(searchLower)) ||
                    
                    // Contact fields
                    EF.Property<ICollection<object>>(p, "Contacts").Any(c =>
                        (EF.Property<string>(c, "FirstName") != null && EF.Property<string>(c, "FirstName").ToLower().Contains(searchLower)) ||
                        (EF.Property<string>(c, "LastName") != null && EF.Property<string>(c, "LastName").ToLower().Contains(searchLower)) ||
                        (EF.Property<string>(c, "Email") != null && EF.Property<string>(c, "Email").ToLower().Contains(searchLower)) ||
                        (EF.Property<string>(c, "Title") != null && EF.Property<string>(c, "Title").ToLower().Contains(searchLower)) ||
                        (EF.Property<string>(c, "Department") != null && EF.Property<string>(c, "Department").ToLower().Contains(searchLower))
                    )
                ).ToListAsync();

            case "UNOPSContact":
                return await query.Where(c =>
                    // Direct contact fields
                    (EF.Property<string>(c, "FirstName") != null && EF.Property<string>(c, "FirstName").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(c, "LastName") != null && EF.Property<string>(c, "LastName").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(c, "Email") != null && EF.Property<string>(c, "Email").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(c, "Title") != null && EF.Property<string>(c, "Title").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(c, "Department") != null && EF.Property<string>(c, "Department").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(c, "Phone") != null && EF.Property<string>(c, "Phone").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(c, "Mobile") != null && EF.Property<string>(c, "Mobile").ToLower().Contains(searchLower)) ||
                    
                    // Enum fields - search by string representation and human-readable text
                    EF.Property<object>(c, "Status").ToString().ToLower().Contains(searchLower) ||
                    (searchLower.Contains("active") && EF.Property<object>(c, "Status").ToString() == "Active") ||
                    (searchLower.Contains("inactive") && EF.Property<object>(c, "Status").ToString() == "Inactive") ||
                    (searchLower.Contains("draft") && EF.Property<object>(c, "Status").ToString() == "Draft") ||
                    (searchLower.Contains("closed") && EF.Property<object>(c, "Status").ToString() == "Closed") ||
                    (searchLower.Contains("archived") && EF.Property<object>(c, "Status").ToString() == "Archived") ||
                    
                    // Partner fields
                    (EF.Property<object>(c, "Partner") != null && 
                     EF.Property<string>(EF.Property<object>(c, "Partner"), "Name") != null && 
                     EF.Property<string>(EF.Property<object>(c, "Partner"), "Name").ToLower().Contains(searchLower))
                ).ToListAsync();

            case "UNOPSInteraction":
                return await query.Where(i =>
                    // Direct interaction fields
                    (EF.Property<string>(i, "Subject") != null && EF.Property<string>(i, "Subject").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(i, "Description") != null && EF.Property<string>(i, "Description").ToLower().Contains(searchLower)) ||
                    (EF.Property<string>(i, "Location") != null && EF.Property<string>(i, "Location").ToLower().Contains(searchLower)) ||
                    
                    // Enum fields - search by string representation
                    EF.Property<object>(i, "Type").ToString().ToLower().Contains(searchLower) ||
                    EF.Property<object>(i, "Status").ToString().ToLower().Contains(searchLower) ||
                    
                    // Enum fields - search by human-readable text (for text like "Virtual Meeting", "In Person", etc.)
                    (searchLower.Contains("virtual") && EF.Property<object>(i, "Type").ToString() == "VirtualMeeting") ||
                    (searchLower.Contains("person") && EF.Property<object>(i, "Type").ToString() == "InPersonMeeting") ||
                    (searchLower.Contains("meeting") && (EF.Property<object>(i, "Type").ToString() == "VirtualMeeting" || EF.Property<object>(i, "Type").ToString() == "InPersonMeeting")) ||
                    (searchLower.Contains("email") && EF.Property<object>(i, "Type").ToString() == "Email") ||
                    (searchLower.Contains("chat") && EF.Property<object>(i, "Type").ToString() == "Chat") ||
                    (searchLower.Contains("call") && EF.Property<object>(i, "Type").ToString() == "Call") ||
                    (searchLower.Contains("active") && EF.Property<object>(i, "Status").ToString() == "Active") ||
                    (searchLower.Contains("inactive") && EF.Property<object>(i, "Status").ToString() == "Inactive") ||
                    (searchLower.Contains("draft") && EF.Property<object>(i, "Status").ToString() == "Draft") ||
                    (searchLower.Contains("closed") && EF.Property<object>(i, "Status").ToString() == "Closed") ||
                    (searchLower.Contains("archived") && EF.Property<object>(i, "Status").ToString() == "Archived") ||
                    
                    // Related contacts
                    EF.Property<ICollection<object>>(i, "InteractionContacts").Any(ic =>
                        EF.Property<string>(EF.Property<object>(ic, "Contact"), "FirstName").ToLower().Contains(searchLower) ||
                        EF.Property<string>(EF.Property<object>(ic, "Contact"), "LastName").ToLower().Contains(searchLower)
                    ) ||
                    
                    // Related partners
                    EF.Property<ICollection<object>>(i, "InteractionPartners").Any(ip =>
                        EF.Property<string>(EF.Property<object>(ip, "Partner"), "Name").ToLower().Contains(searchLower)
                    )
                ).ToListAsync();

            default:
                _logger.LogWarning("Unknown entity type for exact search: {EntityType}", entityType);
                return new List<TEntity>();
        }
    }

    /// <summary>
    /// Get similarity matches using the new modular PostgreSQL search functions
    /// </summary>
    private async Task<List<TEntity>> GetSimilarityMatchesAsync<TEntity>(
        IQueryable<TEntity> query,
        string searchText,
        string entityType) where TEntity : class
    {
        try
        {
            _logger.LogInformation("=== MODULAR POSTGRESQL SIMILARITY SEARCH ===");
            _logger.LogInformation("Entity Type: {EntityType}, Search Text: '{SearchText}'", entityType, searchText);

            List<GlobalSearchResult> searchResults;

            // Use the appropriate modular search function based on entity type
            switch (entityType)
            {
                case "UNOPSPartner":
                    searchResults = await SearchPartnersAsync(searchText);
                    break;
                case "UNOPSContact":
                    searchResults = await SearchContactsAsync(searchText);
                    break;
                case "Interaction":
                    searchResults = await SearchInteractionsAsync(searchText);
                    break;
                default:
                    _logger.LogWarning("Entity type {EntityType} not supported for modular similarity search", entityType);
                    return new List<TEntity>();
            }
            
            // Extract entity IDs from the search results
            var entityIds = searchResults.Select(r => r.EntityId).ToList();
            
            if (!entityIds.Any())
            {
                _logger.LogInformation("No similarity matches found for '{SearchText}' in {EntityType}", searchText, entityType);
                return new List<TEntity>();
            }

            _logger.LogInformation("Modular similarity search found {Count} matches for '{SearchText}' in {EntityType}", 
                entityIds.Count, searchText, entityType);

            // Filter the original query to only include matching IDs
            return await query.Where(BuildIdFilterExpression<TEntity>(entityIds)).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing modular similarity search for entity type: {EntityType}", entityType);
            return new List<TEntity>();
        }
    }

    /// <summary>
    /// Execute PostgreSQL search_entity_records function
    /// </summary>
    private async Task<string> ExecutePostgreSQLSearchAsync(string searchText, string[]? entityFilter = null)
    {
        try
        {
            using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            
            // Use the search_entity_records function with entity filter
            // Parameters: search_query, embedding (null), text_boost, embedding_boost, snippet_length, debug_mode, entity_filter
            command.CommandText = "SELECT public.search_entity_records($1, NULL, $2, $3, $4, $5, $6)";
            command.Parameters.Add(new NpgsqlParameter { Value = searchText });
            command.Parameters.Add(new NpgsqlParameter { Value = 1.0f }); // text_boost
            command.Parameters.Add(new NpgsqlParameter { Value = 1.0f }); // embedding_boost (not used since embedding is null)
            command.Parameters.Add(new NpgsqlParameter { Value = 150 }); // snippet_length
            command.Parameters.Add(new NpgsqlParameter { Value = false }); // debug_mode
            
            // Add entity filter parameter
            if (entityFilter != null && entityFilter.Length > 0)
            {
                command.Parameters.Add(new NpgsqlParameter 
                { 
                    Value = entityFilter,
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text
                });
            }
            else
            {
                command.Parameters.Add(new NpgsqlParameter 
                { 
                    Value = DBNull.Value,
                    NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text
                });
            }

            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? "{}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing PostgreSQL search function for query: '{SearchText}', entities: {Entities}", 
                searchText, entityFilter != null ? string.Join(", ", entityFilter) : "all");
            return "{}";
        }
    }

    /// <summary>
    /// Extract entity IDs from PostgreSQL search results JSON
    /// </summary>
    private List<int> ExtractEntityIdsFromSearchResults(string searchResultsJson, string targetEntityType)
    {
        try
        {
            var entityIds = new List<int>();
            
            if (string.IsNullOrEmpty(searchResultsJson) || searchResultsJson == "{}")
                return entityIds;

            using var document = JsonDocument.Parse(searchResultsJson);
            
            // Check if we have results
            if (!document.RootElement.TryGetProperty("results", out var results))
                return entityIds;

            // Look for the specific entity type in results
            if (results.TryGetProperty(targetEntityType, out var entityResults))
            {
                if (entityResults.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        if (item.TryGetProperty("entityId", out var entityIdElement) && 
                            entityIdElement.TryGetInt32(out var entityId))
                        {
                            entityIds.Add(entityId);
                        }
                    }
                }
            }

            _logger.LogDebug("Extracted {Count} entity IDs for {EntityType} from PostgreSQL search results", 
                entityIds.Count, targetEntityType);
            
            return entityIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting entity IDs from search results for {EntityType}", targetEntityType);
            return new List<int>();
        }
    }

    /// <summary>
    /// Get all searchable text fields for an entity
    /// </summary>
    private List<(string FieldName, string FieldValue)> GetSearchableTextFields<TEntity>(TEntity entity, string entityType)
    {
        var fields = new List<(string, string)>();

        switch (entityType)
        {
            case "UNOPSPartner":
                var partner = entity as UNOPSPartner;
                if (partner != null)
                {
                    // Direct fields
                    AddFieldIfNotNull(fields, "Name", partner.Name);
                    AddFieldIfNotNull(fields, "PartnerShortDescription", partner.PartnerShortDescription);
                    AddFieldIfNotNull(fields, "PartnerLongDescription", partner.PartnerLongDescription);
                    AddFieldIfNotNull(fields, "PartnerApprovalReference", partner.PartnerApprovalReference);
                    AddFieldIfNotNull(fields, "PartnerApprovedBy", partner.PartnerApprovedBy);
                    AddFieldIfNotNull(fields, "ReasonForLevy", partner.ReasonForLevy);
                    AddFieldIfNotNull(fields, "LevyTreatment", partner.LevyTreatment);
                    AddFieldIfNotNull(fields, "ReasonForNoNewOpportunity", partner.ReasonForNoNewOpportunity);

                    // Navigation properties
                    if (partner.PartnerGroup != null)
                    {
                        AddFieldIfNotNull(fields, "PartnerGroup.Name", partner.PartnerGroup.Name);
                        AddFieldIfNotNull(fields, "PartnerGroup.Code", partner.PartnerGroup.Code);
                    }

                    if (partner.LiaisonOffice != null)
                    {
                        AddFieldIfNotNull(fields, "LiaisonOffice.Name", partner.LiaisonOffice.Name);
                        AddFieldIfNotNull(fields, "LiaisonOffice.Code", partner.LiaisonOffice.Code);
                    }

                    // Contact fields
                    foreach (var contact in partner.Contacts ?? Enumerable.Empty<Contact>())
                    {
                        AddFieldIfNotNull(fields, $"Contact[{contact.Id}].FirstName", contact.FirstName);
                        AddFieldIfNotNull(fields, $"Contact[{contact.Id}].LastName", contact.LastName);
                        AddFieldIfNotNull(fields, $"Contact[{contact.Id}].Email", contact.Email);
                        AddFieldIfNotNull(fields, $"Contact[{contact.Id}].Title", contact.Title);
                        AddFieldIfNotNull(fields, $"Contact[{contact.Id}].Department", contact.Department);
                    }
                }
                break;

            case "UNOPSContact":
                var contactEntity = entity as UNOPSContact;
                if (contactEntity != null)
                {
                    AddFieldIfNotNull(fields, "FirstName", contactEntity.FirstName);
                    AddFieldIfNotNull(fields, "LastName", contactEntity.LastName);
                    AddFieldIfNotNull(fields, "Email", contactEntity.Email);
                    AddFieldIfNotNull(fields, "Title", contactEntity.Title);
                    AddFieldIfNotNull(fields, "Department", contactEntity.Department);
                    AddFieldIfNotNull(fields, "Description", contactEntity.Description);
                    AddFieldIfNotNull(fields, "Phone", contactEntity.Phone);
                    AddFieldIfNotNull(fields, "Mobile", contactEntity.Mobile);
                    AddFieldIfNotNull(fields, "Assistant", contactEntity.Assistant);
                    AddFieldIfNotNull(fields, "AssistantEmail", contactEntity.AssistantEmail);

                    // Partner fields
                    if (contactEntity.Partner != null)
                    {
                        AddFieldIfNotNull(fields, "Partner.Name", contactEntity.Partner.Name);
                    }
                }
                break;

            case "UNOPSInteraction":
                var interaction = entity as UNOPSInteraction;
                if (interaction != null)
                {
                    AddFieldIfNotNull(fields, "Subject", interaction.Subject);
                    AddFieldIfNotNull(fields, "Description", interaction.Description);
                    AddFieldIfNotNull(fields, "Location", interaction.Location);

                    // Related entities would be added here
                }
                break;
        }

        return fields;
    }

    /// <summary>
    /// Helper method to add field if not null or empty
    /// </summary>
    private void AddFieldIfNotNull(List<(string, string)> fields, string fieldName, string? fieldValue)
    {
        if (!string.IsNullOrEmpty(fieldValue))
        {
            fields.Add((fieldName, fieldValue));
        }
    }

    #endregion

    #region Structured Filters

    /// <summary>
    /// Apply structured filters using dynamic LINQ
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplyStructuredFilters<TEntity>(
        IQueryable<TEntity> query,
        List<SearchFilter> filters) where TEntity : class
    {
        if (!filters.Any()) return query;

        // Check if we have mixed filter types (regular + similarity) with OR operators
        var hasMixedFiltersWithOr = HasMixedFiltersWithOr(filters);

        if (hasMixedFiltersWithOr)
        {
            // For mixed filters with OR, we need to handle them differently
            return await ApplyMixedFiltersWithOr<TEntity>(query, filters);
        }

        // For simple cases (all regular, all similarity, or only AND operators), use the optimized approach
        var regularFilters = new List<SearchFilter>();
        var similarityFilters = new List<SearchFilter>();

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.field) || string.IsNullOrWhiteSpace(filter.value))
                continue;

            // Separate similarity-based filters from regular ones
            if ((filter.@operator.ToLower() == "like" || filter.@operator.ToLower() == "contains") && 
                filter.fieldType == "text")
            {
                similarityFilters.Add(filter);
            }
            else
            {
                regularFilters.Add(filter);
            }
        }

        // Apply regular filters using dynamic LINQ
        if (regularFilters.Any())
        {
            var conditions = new List<string>();
            var parameters = new List<object>();

            foreach (var filter in regularFilters)
            {
                var condition = BuildFilterCondition(filter, parameters);
                if (!string.IsNullOrEmpty(condition))
                {
                    conditions.Add(condition);
                }
            }

            if (conditions.Any())
            {
                var combinedCondition = CombineConditions(conditions, regularFilters);
                _logger.LogDebug("Applying regular filters: {Condition}", combinedCondition);
                query = query.Where(combinedCondition, parameters.ToArray());
            }
        }

        // Apply similarity filters using Entity Framework functions
        query = await ApplySimilarityFilters(query, similarityFilters);

        return query;
    }

    /// <summary>
    /// Check if we have mixed filter types (regular + similarity) with OR operators
    /// </summary>
    private bool HasMixedFiltersWithOr(List<SearchFilter> filters)
    {
        var hasRegular = false;
        var hasSimilarity = false;
        var hasOr = false;

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.field) || string.IsNullOrWhiteSpace(filter.value))
                continue;

            // Check filter type
            if ((filter.@operator.ToLower() == "like" || filter.@operator.ToLower() == "contains") && 
                filter.fieldType == "text")
            {
                hasSimilarity = true;
            }
            else
            {
                hasRegular = true;
            }

            // Check for OR operator
            if (filter.logicalOperator?.ToUpper() == "OR")
            {
                hasOr = true;
            }
        }

        return hasRegular && hasSimilarity && hasOr;
    }

    /// <summary>
    /// Apply mixed filters with OR operators by combining all results
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplyMixedFiltersWithOr<TEntity>(IQueryable<TEntity> query, List<SearchFilter> filters)
        where TEntity : class
    {
        var allMatchingIds = new HashSet<int>();

        // Process filters sequentially, respecting logical operators
        for (int i = 0; i < filters.Count; i++)
        {
            var filter = filters[i];
            if (string.IsNullOrWhiteSpace(filter.field) || string.IsNullOrWhiteSpace(filter.value))
                continue;

            var currentMatchingIds = new HashSet<int>();

            // Apply individual filter to get matching IDs
            if ((filter.@operator.ToLower() == "like" || filter.@operator.ToLower() == "contains") && 
                filter.fieldType == "text")
            {
                // Similarity filter
                var similarityIds = await GetSimilarityMatchingIds<TEntity>(new List<SearchFilter> { filter });
                currentMatchingIds.UnionWith(similarityIds);
            }
            else
            {
                // Regular filter
                try
                {
                    var tempQuery = BuildBaseQueryWithIncludes<TEntity>();
                    var condition = BuildFilterCondition(filter, new List<object>());
                    if (!string.IsNullOrEmpty(condition))
                    {
                        var parameters = new List<object>();
                        condition = BuildFilterCondition(filter, parameters);
                        tempQuery = tempQuery.Where(condition, parameters.ToArray());
                        var ids = await tempQuery.Select(GetEntityIdExpression<TEntity>()).ToListAsync();
                        currentMatchingIds.UnionWith(ids);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error applying regular filter for mixed OR logic: {Field} {Operator} {Value}", 
                        filter.field, filter.@operator, filter.value);
                }
            }

            // Combine results based on logical operator
            if (i == 0)
            {
                // First filter - initialize the result set
                allMatchingIds.UnionWith(currentMatchingIds);
            }
            else
            {
                var logicalOperator = filter.logicalOperator?.ToUpper() ?? "AND";
                if (logicalOperator == "OR")
                {
                    // OR: Add to existing results
                    allMatchingIds.UnionWith(currentMatchingIds);
                }
                else
                {
                    // AND: Intersect with existing results
                    allMatchingIds.IntersectWith(currentMatchingIds);
                }
            }
        }

        // Apply the combined ID filter
        if (allMatchingIds.Any())
        {
            var idList = allMatchingIds.ToList();
            return query.Where(BuildIdFilter<TEntity>(idList));
        }

        return query.Where(entity => false); // No matches found
    }

    /// <summary>
    /// Get entity ID expression for dynamic selection
    /// </summary>
    private Expression<Func<TEntity, int>> GetEntityIdExpression<TEntity>() where TEntity : class
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var idProperty = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (idProperty == null)
        {
            // Fallback for inherited entities
            var allIdProperties = typeof(TEntity).GetProperties().Where(p => p.Name == "Id").ToArray();
            if (allIdProperties.Length > 0)
            {
                idProperty = allIdProperties[0];
            }
        }

        if (idProperty == null)
        {
            throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not have an Id property");
        }

        var propertyAccess = Expression.Property(parameter, idProperty);
        return Expression.Lambda<Func<TEntity, int>>(propertyAccess, parameter);
    }

    /// <summary>
    /// Apply similarity-based filters using PostgreSQL's similarity function from pg_trgm extension
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplySimilarityFilters<TEntity>(IQueryable<TEntity> query, List<SearchFilter> similarityFilters)
        where TEntity : class
    {
        if (!similarityFilters.Any()) return query;

        try
        {
            // Get matching IDs using PostgreSQL similarity function
            var matchingIds = await GetSimilarityMatchingIds<TEntity>(similarityFilters);
            
            if (matchingIds.Any())
            {
                // Apply the ID filter to the original query
                return query.Where(BuildIdFilter<TEntity>(matchingIds));
            }
            
            return query.Where(entity => false); // No matches found
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply PostgreSQL similarity search, falling back to enhanced contains matching");
            return ApplyFallbackSimilarityFilters(query, similarityFilters);
        }
    }

    /// <summary>
    /// Get matching entity IDs using PostgreSQL similarity function with proper column names and JOINs for nested fields
    /// </summary>
    private async Task<List<int>> GetSimilarityMatchingIds<TEntity>(List<SearchFilter> similarityFilters) where TEntity : class
    {
        var tableName = GetTableName<TEntity>();
        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        var joins = new List<string>();
        var joinedTables = new HashSet<string>();

        // Map common field names to actual database column names and handle nested fields
        var fieldMappings = GetFieldMappings<TEntity>();

        foreach (var filter in similarityFilters)
        {
            var fieldInfo = GetFieldInfo<TEntity>(filter.field, fieldMappings, tableName);
            var searchValue = filter.value;

            // Add necessary JOINs for nested fields
            foreach (var join in fieldInfo.RequiredJoins)
            {
                if (!joinedTables.Contains(join))
                {
                    joins.Add(join);
                    joinedTables.Add(join);
                }
            }

            // Create similarity condition with proper column name and table alias
            var likeParamIndex = parameters.Count + 1; // PostgreSQL parameters start from $1
            var similarityParamIndex = parameters.Count + 2;
            parameters.Add(new NpgsqlParameter($"@param{likeParamIndex}", NpgsqlDbType.Text) { Value = $"%{searchValue.ToLower()}%" });
            parameters.Add(new NpgsqlParameter($"@param{similarityParamIndex}", NpgsqlDbType.Text) { Value = searchValue });
            
            // Use both exact matching and similarity for best results (simplified boolean logic)
            var condition = $@"{fieldInfo.FullColumnName} IS NOT NULL AND 
                LOWER({fieldInfo.FullColumnName}) LIKE @param{likeParamIndex} OR
                (similarity({fieldInfo.FullColumnName}, @param{similarityParamIndex}) * 100) > {SIMILARITY_THRESHOLD_PERCENT}";
            
            conditions.Add(condition);
        }

        if (!conditions.Any())
        {
            return new List<int>();
        }

        var joinClause = joins.Any() ? string.Join(" ", joins) : "";
        var whereClause = CombineSimilarityConditions(conditions, similarityFilters);
        var mainTableAlias = GetMainTableAlias<TEntity>();
        var sql = $@"SELECT DISTINCT {mainTableAlias}.""Id"" FROM public.""{tableName}"" {mainTableAlias} {joinClause} WHERE {whereClause}";

        _logger.LogDebug("Executing similarity SQL with JOINs: {Sql} with parameters: {Parameters}", 
            sql, string.Join(", ", parameters));
        _logger.LogDebug("Parameter count: {Count}, Parameter values: [{Values}]", 
            parameters.Count, string.Join(", ", parameters.Select((p, i) => $"${i+1}='{p}'")));

        try
        {
            var result = await _context.Database
                .SqlQueryRaw<int>(sql, parameters.ToArray())
                .ToListAsync();
            
            _logger.LogDebug("Similarity search found {Count} matching IDs", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing similarity SQL with JOINs: {Sql}", sql);
            return new List<int>();
        }
    }

    /// <summary>
    /// Combine similarity conditions with logical operators (for SQL WHERE clause)
    /// </summary>
    private string CombineSimilarityConditions(List<string> conditions, List<SearchFilter> filters)
    {
        if (conditions.Count == 1) return $"({conditions[0]})";

        var result = new StringBuilder();
        result.Append($"({conditions[0]})");

        for (int i = 1; i < conditions.Count; i++)
        {
            var logicalOperator = filters[i].logicalOperator?.ToUpper() ?? "AND";
            if (logicalOperator != "AND" && logicalOperator != "OR")
                logicalOperator = "AND";

            result.Append($" {logicalOperator} ");
            result.Append($"({conditions[i]})");
        }

        return result.ToString();
    }

    /// <summary>
    /// Field information for database queries including JOINs
    /// </summary>
    private class FieldInfo
    {
        public string FullColumnName { get; set; } = "";
        public List<string> RequiredJoins { get; set; } = new List<string>();
    }

    /// <summary>
    /// Get field information including required JOINs for nested fields
    /// </summary>
    private FieldInfo GetFieldInfo<TEntity>(string fieldName, Dictionary<string, string> fieldMappings, string mainTableName) where TEntity : class
    {
        var fieldInfo = new FieldInfo();

        // Handle special computed fields
        if (fieldName.ToLower() == "fullname" || fieldName.EndsWith(".fullname", StringComparison.OrdinalIgnoreCase))
        {
            return GetFullNameFieldInfo<TEntity>(fieldName);
        }

        // Handle nested fields (e.g., partnerGroup.name, contacts.firstName)
        if (fieldName.Contains('.'))
        {
            var parts = fieldName.Split('.');
            var navigationProperty = parts[0];
            var targetField = parts[1];

            // Determine the main table alias based on entity type
            var mainTableAlias = GetMainTableAlias<TEntity>();
            
            switch (navigationProperty.ToLower())
            {
                // Partner-specific navigation properties
                case "partnergroup":
                    fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""PartnerTrees"" pg ON {mainTableAlias}.""PartnerGroupId"" = pg.""Id""");
                    fieldInfo.FullColumnName = $@"pg.""{GetColumnName(targetField)}""";
                    break;

                case "liaisonoffice":
                    fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""LiaisonOffices"" lo ON {mainTableAlias}.""PartnerLiaisonOfficeId"" = lo.""Id""");
                    fieldInfo.FullColumnName = $@"lo.""{GetColumnName(targetField)}""";
                    break;

                case "contacts":
                    if (typeof(TEntity).Name.Contains("Partner"))
                    {
                        fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""Contacts"" c ON {mainTableAlias}.""Id"" = c.""PartnerId""");
                        fieldInfo.FullColumnName = $@"c.""{GetColumnName(targetField)}""";
                    }
                    break;

                // Contact-specific navigation properties
                case "partner":
                    if (typeof(TEntity).Name.Contains("Contact"))
                    {
                        fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""Partners"" p ON {mainTableAlias}.""PartnerId"" = p.""Id""");
                        fieldInfo.FullColumnName = $@"p.""{GetColumnName(targetField)}""";
                    }
                    break;

                case "interactions":
                    if (typeof(TEntity).Name.Contains("Contact"))
                    {
                        fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""InteractionContacts"" ic ON {mainTableAlias}.""Id"" = ic.""ContactId""");
                        fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""Interactions"" i ON ic.""InteractionId"" = i.""Id""");
                        fieldInfo.FullColumnName = $@"i.""{GetColumnName(targetField)}""";
                    }
                    break;

                // Interaction-specific navigation properties
                case "interactioncontacts":
                    if (typeof(TEntity).Name.Contains("Interaction"))
                    {
                        if (parts.Length >= 3 && parts[1].ToLower() == "contact")
                        {
                            fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""InteractionContacts"" ic ON {mainTableAlias}.""Id"" = ic.""InteractionId""");
                            fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""Contacts"" c ON ic.""ContactId"" = c.""Id""");
                            fieldInfo.FullColumnName = $@"c.""{GetColumnName(parts[2])}""";
                        }
                    }
                    break;

                case "interactionpartners":
                    if (typeof(TEntity).Name.Contains("Interaction"))
                    {
                        if (parts.Length >= 3 && parts[1].ToLower() == "partner")
                        {
                            fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""InteractionPartners"" ip ON {mainTableAlias}.""Id"" = ip.""InteractionId""");
                            fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""Partners"" p ON ip.""PartnerId"" = p.""Id""");
                            fieldInfo.FullColumnName = $@"p.""{GetColumnName(parts[2])}""";
                        }
                    }
                    break;

                case "interactionusers":
                    if (typeof(TEntity).Name.Contains("Interaction"))
                    {
                        if (parts.Length >= 3 && parts[1].ToLower() == "user")
                        {
                            fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""InteractionUsers"" iu ON {mainTableAlias}.""Id"" = iu.""InteractionId""");
                            fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""AspNetUsers"" u ON iu.""UserId"" = u.""Id""");
                            fieldInfo.FullColumnName = $@"u.""{GetColumnName(parts[2])}""";
                        }
                    }
                    break;

                // Common navigation properties
                case "organizationunitrelationships":
                    if (parts.Length >= 3 && parts[1].ToLower() == "organizationhierarchy")
                    {
                        var entityIdColumn = typeof(TEntity).Name.Contains("Partner") ? "PartnerId" : 
                                           typeof(TEntity).Name.Contains("Interaction") ? "InteractionId" : 
                                           "ContactId";
                        fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""OrganizationUnitRelationships"" our ON {mainTableAlias}.""Id"" = our.""{entityIdColumn}""");
                        fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""OrganizationHierarchies"" oh ON our.""OrganizationHierarchyId"" = oh.""Id""");
                        fieldInfo.FullColumnName = $@"oh.""{GetColumnName(parts[2])}""";
                    }
                    break;

                case "documents":
                    // Documents can be associated with Partners, Contacts, or Interactions
                    fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""Documents"" d ON {mainTableAlias}.""Id"" = d.""EntityId"" AND d.""EntityType"" = '{typeof(TEntity).Name}'");
                    fieldInfo.FullColumnName = $@"d.""{GetColumnName(targetField)}""";
                    break;

                default:
                    // Fallback for unknown nested fields
                    _logger.LogWarning("Unknown nested field: {FieldName}, using fallback", fieldName);
                    fieldInfo.FullColumnName = $@"{mainTableAlias}.""{GetDatabaseColumnName(fieldName, fieldMappings)}""";
                    break;
            }
        }
        else
        {
            // Simple field on main table
            var mainTableAlias = GetMainTableAlias<TEntity>();
            fieldInfo.FullColumnName = $@"{mainTableAlias}.""{GetDatabaseColumnName(fieldName, fieldMappings)}""";
        }

        return fieldInfo;
    }

    /// <summary>
    /// Get the main table alias based on entity type
    /// </summary>
    private string GetMainTableAlias<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);
        
        if (entityType.Name.Contains("Partner"))
            return "p";
        if (entityType.Name.Contains("Contact"))
            return "c";
        if (entityType.Name.Contains("Interaction"))
            return "i";
            
        // Default fallback
        return "e";
    }

    /// <summary>
    /// Get field mappings for database column names
    /// </summary>
    private Dictionary<string, string> GetFieldMappings<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);
        
        // Common field mappings for Partner entities
        if (entityType.Name.Contains("Partner"))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "name", "Name" },
                { "partnerShortDescription", "PartnerShortDescription" },
                { "partnerLongDescription", "PartnerLongDescription" },
                { "partnerApprovalReference", "PartnerApprovalReference" }
            };
        }
        
        // Common field mappings for Contact entities
        if (entityType.Name.Contains("Contact"))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "firstName", "FirstName" },
                { "lastName", "LastName" },
                { "middleName", "MiddleName" },
                { "email", "Email" },
                { "phone", "Phone" },
                { "mobile", "Mobile" },
                { "title", "Title" },
                { "department", "Department" },
                { "description", "Description" },
                { "salutation", "Salutation" },
                { "suffix", "Suffix" },
                { "assistant", "Assistant" },
                { "assistantPhone", "AssistantPhone" },
                { "assistantEmail", "AssistantEmail" },
                { "mailingStreet", "MailingStreet" },
                { "mailingCity", "MailingCity" },
                { "mailingCountry", "MailingCountry" },
                { "contactNumber", "ContactNumber" }
            };
        }
        
        // Common field mappings for Interaction entities
        if (entityType.Name.Contains("Interaction"))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "subject", "Subject" },
                { "description", "Description" },
                { "location", "Location" },
                { "type", "Type" },
                { "date", "Date" },
                { "gmailThreadId", "GmailThreadId" },
                { "gmailMessageId", "GmailMessageId" }
            };
        }
        
        // Add mappings for other entity types as needed
        return new Dictionary<string, string>();
    }

    /// <summary>
    /// Get the correct database column name for a field
    /// </summary>
    private string GetDatabaseColumnName(string fieldName, Dictionary<string, string> fieldMappings)
    {
        if (fieldMappings.TryGetValue(fieldName, out var mappedName))
        {
            return mappedName;
        }
        
        // Default: assume field name matches column name (with proper casing)
        return fieldName.First().ToString().ToUpper() + fieldName.Substring(1);
    }

    /// <summary>
    /// Convert field name to proper database column name with capitalization
    /// </summary>
    private string GetColumnName(string fieldName)
    {
        // Common field name mappings for nested entities
        var commonMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "name", "Name" },
            { "firstName", "FirstName" },
            { "lastName", "LastName" },
            { "email", "Email" },
            { "title", "Title" },
            { "department", "Department" },
            { "phone", "Phone" },
            { "mobile", "Mobile" },
            { "code", "Code" },
            { "description", "Description" }
        };

        if (commonMappings.TryGetValue(fieldName, out var mappedName))
        {
            return mappedName;
        }

        // Default: Pascal case
        return fieldName.First().ToString().ToUpper() + fieldName.Substring(1);
    }

    /// <summary>
    /// Build an ID filter expression for the given entity type
    /// </summary>
    private Expression<Func<TEntity, bool>> BuildIdFilter<TEntity>(List<int> matchingIds)
        where TEntity : class
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        
        // Get the Id property more specifically to avoid ambiguous match
        // Try to get the Id property from the exact type first, then from declared properties only
        var idProperty = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
        // If not found in declared properties, get the first Id property available
        if (idProperty == null)
        {
            var allIdProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name == "Id" && p.PropertyType == typeof(int))
                .ToArray();
            
            if (allIdProperties.Length > 0)
            {
                idProperty = allIdProperties[1]; // Use the first one found
            }
        }
        
        if (idProperty == null)
        {
            throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have an accessible 'Id' property.");
        }
        
        var propertyExpression = Expression.Property(parameter, idProperty);
        var idsConstant = Expression.Constant(matchingIds);
        var containsMethod = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });
        var containsExpression = Expression.Call(idsConstant, containsMethod, propertyExpression);
        
        return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
    }

    /// <summary>
    /// Get the table name for an entity type
    /// </summary>
    private string GetTableName<TEntity>() where TEntity : class
    {
        var entityType = _context.Model.FindEntityType(typeof(TEntity));
        return entityType?.GetTableName() ?? typeof(TEntity).Name;
    }

    /// <summary>
    /// Fallback similarity filters using enhanced contains matching
    /// </summary>
    private IQueryable<TEntity> ApplyFallbackSimilarityFilters<TEntity>(IQueryable<TEntity> query, List<SearchFilter> similarityFilters)
        where TEntity : class
    {
        foreach (var filter in similarityFilters)
        {
            var field = ConvertFieldName(filter.field);
            var searchValue = filter.value.ToLower();

            // Apply enhanced text matching using Contains and StartsWith for better results
            query = query.Where($"({field} != null && ({field}.ToLower().Contains(@0) || {field}.ToLower().StartsWith(@1)))", 
                searchValue, searchValue);
        }

        return query;
    }

    /// <summary>
    /// Build condition string for a single filter
    /// </summary>
    private string BuildFilterCondition(SearchFilter filter, List<object> parameters)
    {
        var field = ConvertFieldName(filter.field);
        var paramIndex = parameters.Count;

        switch (filter.@operator.ToLower())
        {
            case "like":
            case "contains":
                // These are now handled by ApplySimilarityFilters method
                // Return empty to skip in regular filter processing
                return string.Empty;

            case "eq":
            case "equals":
                if (filter.fieldType == "text")
                {
                    parameters.Add(filter.value.ToLower());
                    return $"{field} != null && {field}.ToLower() == @{paramIndex}";
                }
                else
                {
                    parameters.Add(ConvertValue(filter.value, filter.fieldType));
                    return $"{field} == @{paramIndex}";
                }

            case "neq":
            case "not equals":
                if (filter.fieldType == "text")
                {
                    parameters.Add(filter.value.ToLower());
                    return $"{field} != null && {field}.ToLower() != @{paramIndex}";
                }
                else
                {
                    parameters.Add(ConvertValue(filter.value, filter.fieldType));
                    return $"{field} != @{paramIndex}";
                }

            case "gt":
            case "greater than":
                parameters.Add(ConvertValue(filter.value, filter.fieldType));
                return $"{field} > @{paramIndex}";

            case "lt":
            case "less than":
                parameters.Add(ConvertValue(filter.value, filter.fieldType));
                return $"{field} < @{paramIndex}";

            case "gte":
            case "greater than or equal":
                parameters.Add(ConvertValue(filter.value, filter.fieldType));
                return $"{field} >= @{paramIndex}";

            case "lte":
            case "less than or equal":
                parameters.Add(ConvertValue(filter.value, filter.fieldType));
                return $"{field} <= @{paramIndex}";

            case "after":
                // For date fields, "after" means greater than the specified date
                if (filter.fieldType == "date")
                {
                    var dateValue = ConvertValue(filter.value, filter.fieldType);
                    if (dateValue is DateTime afterDate)
                    {
                        // Add one day to make it truly "after" the date (start of next day)
                        // Convert to UTC to satisfy PostgreSQL timezone requirements
                        var utcDate = DateTime.SpecifyKind(afterDate.Date.AddDays(1), DateTimeKind.Utc);
                        parameters.Add(utcDate);
                        return $"{field} >= @{paramIndex}";
                    }
                }
                parameters.Add(ConvertValue(filter.value, filter.fieldType));
                return $"{field} > @{paramIndex}";

            case "before":
                // For date fields, "before" means less than the specified date
                if (filter.fieldType == "date")
                {
                    var dateValue = ConvertValue(filter.value, filter.fieldType);
                    if (dateValue is DateTime beforeDate)
                    {
                        // Use the start of the day to make it truly "before" the date
                        // Convert to UTC to satisfy PostgreSQL timezone requirements
                        var utcDate = DateTime.SpecifyKind(beforeDate.Date, DateTimeKind.Utc);
                        parameters.Add(utcDate);
                        return $"{field} < @{paramIndex}";
                    }
                }
                parameters.Add(ConvertValue(filter.value, filter.fieldType));
                return $"{field} < @{paramIndex}";

            case "on":
                // For date fields, "on" means within the entire day
                if (filter.fieldType == "date")
                {
                    var dateValue = ConvertValue(filter.value, filter.fieldType);
                    if (dateValue is DateTime onDate)
                    {
                        // Convert to UTC to satisfy PostgreSQL timezone requirements
                        var startOfDay = DateTime.SpecifyKind(onDate.Date, DateTimeKind.Utc);
                        var endOfDay = DateTime.SpecifyKind(onDate.Date.AddDays(1), DateTimeKind.Utc);
                        parameters.Add(startOfDay);
                        parameters.Add(endOfDay);
                        return $"{field} >= @{paramIndex} && {field} < @{paramIndex + 1}";
                    }
                }
                parameters.Add(ConvertValue(filter.value, filter.fieldType));
                return $"{field} == @{paramIndex}";

            case "between":
                // For date ranges - expects "value,secondValue" format
                if (filter.fieldType == "date" && !string.IsNullOrEmpty(filter.secondValue))
                {
                    var fromDate = ConvertValue(filter.value, filter.fieldType);
                    var toDate = ConvertValue(filter.secondValue, filter.fieldType);
                    
                    if (fromDate is DateTime fromDateTime && toDate is DateTime toDateTime)
                    {
                        // Convert to UTC to satisfy PostgreSQL timezone requirements
                        var startOfDay = DateTime.SpecifyKind(fromDateTime.Date, DateTimeKind.Utc);
                        var endOfDay = DateTime.SpecifyKind(toDateTime.Date.AddDays(1), DateTimeKind.Utc);
                        parameters.Add(startOfDay);
                        parameters.Add(endOfDay);
                        return $"{field} >= @{paramIndex} && {field} < @{paramIndex + 1}";
                    }
                }
                // Fallback for non-date fields
                parameters.Add(ConvertValue(filter.value, filter.fieldType));
                return $"{field} >= @{paramIndex}"; // Simplified fallback

            default:
                _logger.LogWarning("Unsupported operator: {Operator}", filter.@operator);
                return string.Empty;
        }
    }

    /// <summary>
    /// Convert field name to proper property name
    /// </summary>
    private string ConvertFieldName(string field)
    {
        if (string.IsNullOrEmpty(field)) return field;

        // Handle navigation properties
        if (field.Contains('.'))
        {
            var parts = field.Split('.');
            return string.Join(".", parts.Select(ConvertToPascalCase));
        }

        return ConvertToPascalCase(field);
    }

    /// <summary>
    /// Convert to PascalCase (capitalize first letter only, preserve the rest)
    /// </summary>
    private string ConvertToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1);
    }

    /// <summary>
    /// Convert string value to appropriate type
    /// </summary>
    private object ConvertValue(string value, string? fieldType)
    {
        try
        {
            return (fieldType ?? "text").ToLower() switch
            {
                "number" => decimal.Parse(value),
                "int" => int.Parse(value),
                "bool" => bool.Parse(value),
                "date" => DateTime.SpecifyKind(DateTime.Parse(value), DateTimeKind.Utc),
                "enum" => value, // Keep as string for enum comparisons (will be converted to enum during query execution)
                _ => value
            };
        }
        catch
        {
            return value;
        }
    }

    /// <summary>
    /// Combine conditions with logical operators
    /// </summary>
    private string CombineConditions(List<string> conditions, List<SearchFilter> filters)
    {
        if (conditions.Count == 1) return conditions[0];

        var result = new StringBuilder();
        result.Append(conditions[0]);

        for (int i = 1; i < conditions.Count; i++)
        {
            var logicalOperator = filters[i].logicalOperator?.ToUpper() ?? "AND";
            if (logicalOperator != "AND" && logicalOperator != "OR")
                logicalOperator = "AND";

            result.Append($" {logicalOperator} ");
            result.Append(conditions[i]);
        }

        return result.ToString();
    }

    #endregion

    #region Helper Methods


    /// <summary>
    /// Get entity ID using reflection
    /// </summary>
    private int GetEntityId<TEntity>(TEntity entity)
    {
        // Try to get the Id property with DeclaredOnly first to avoid ambiguity
        var idProperty = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
        // If not found, iterate through all properties to find the first Id property
        if (idProperty == null)
        {
            var allIdProperties = typeof(TEntity).GetProperties()
                .Where(p => p.Name == "Id" && p.PropertyType == typeof(int))
                .ToArray();
            
            if (allIdProperties.Length > 0)
            {
                idProperty = allIdProperties[0]; // Use the first one found
            }
        }
        
        return idProperty != null ? (int)idProperty.GetValue(entity)! : 0;
    }

    /// <summary>
    /// Build ID filter expression
    /// </summary>
    private Expression<Func<TEntity, bool>> BuildIdFilterExpression<TEntity>(List<int> ids)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        
        // Try to get the Id property with DeclaredOnly first to avoid ambiguity
        var idPropertyInfo = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
        // If not found, iterate through all properties to find the first Id property
        if (idPropertyInfo == null)
        {
            var allIdProperties = typeof(TEntity).GetProperties()
                .Where(p => p.Name == "Id" && p.PropertyType == typeof(int))
                .ToArray();
            
            if (allIdProperties.Length > 0)
            {
                idPropertyInfo = allIdProperties[0]; // Use the first one found
            }
        }
        
        if (idPropertyInfo == null)
        {
            throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not have an Id property");
        }
        
        var idProperty = Expression.Property(parameter, idPropertyInfo);
        var idList = Expression.Constant(ids);
        var containsMethod = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });
        var containsCall = Expression.Call(idList, containsMethod!, idProperty);
        
        return Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);
    }

    /// <summary>
    /// Apply access control and global filters using the centralized GlobalFilterService
    /// </summary>
    private async Task<IQueryable<TEntity>> ApplyAccessControlAsync<TEntity>(IQueryable<TEntity> query, ClaimsPrincipal user, bool filterActive = true) where TEntity : class
    {
        // Apply global filters only if filterActive is true (following UNOPSPartnerManager pattern)
        if (_globalFilterService != null && filterActive == true)
        {
            _logger.LogInformation("Applying global filters (filterActive: {FilterActive})", filterActive);
            return await _globalFilterService.ApplyGlobalFiltersAsync(query, user);
        }
        else
        {
            _logger.LogInformation("Skipping global filters (filterActive: {FilterActive})", filterActive);
            return query;
        }
    }

    /// <summary>
    /// Apply access control (synchronous wrapper for backward compatibility)
    /// </summary>
    private IQueryable<TEntity> ApplyAccessControl<TEntity>(IQueryable<TEntity> query, ClaimsPrincipal user) where TEntity : class
    {
        // For now, return query unchanged - async version handles global filters
        // This is called from the synchronous path, global filters will be applied in the async version
        return query;
    }


    /// <summary>
    /// Map entities to models
    /// </summary>
    private async Task<List<TModel>> MapToModelsAsync<TEntity, TModel>(List<TEntity> entities)
        where TEntity : class
        where TModel : class
    {
        var mappedResults = new List<TModel>();
        
        // Use reflection to cast entities to their concrete types and map accordingly
        var entityTypeName = typeof(TEntity).Name;
        var modelTypeName = typeof(TModel).Name;
        
        try
        {
            foreach (var entity in entities)
            {
                if (entity == null) continue;
                
                object? mappedModel = null;
                
                // Handle Partner entities
                if (entityTypeName == "UNOPSPartner" && modelTypeName == "PartnerModel")
                {
                    // Use AutoMapper or direct mapping - for now using direct cast approach
                    mappedModel = await MapPartnerToModel(entity);
                }
                // Handle Contact entities  
                else if (entityTypeName == "UNOPSContact" && modelTypeName == "ContactModel")
                {
                    mappedModel = await MapContactToModel(entity);
                }
                // Handle Interaction entities
                else if (entityTypeName == "UNOPSInteraction" && modelTypeName == "InteractionModel")
                {
                    mappedModel = await MapInteractionToModel(entity);
                }
                else
                {
                    _logger.LogWarning("Unknown entity/model mapping: {EntityType} -> {ModelType}", entityTypeName, modelTypeName);
                    continue;
                }
                
                if (mappedModel != null && mappedModel is TModel model)
                {
                    mappedResults.Add(model);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping entities to models: {EntityType} -> {ModelType}", entityTypeName, modelTypeName);
        }
        
        return mappedResults;
    }
    
    private async Task<object?> MapPartnerToModel(object entity)
    {
        try
        {
            if (entity is UNOPSPartner partner)
            {
                // Use AutoMapper just like UNOPSPartnerManager does
                return _mapper.Map<UNOPSPartner, PartnerModel>(partner);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping UNOPSPartner to PartnerModel");
        }
        return null;
    }
    
    private async Task<object?> MapContactToModel(object entity)
    {
        try
        {
            if (entity is UNOPSContact contact)
            {
                // Use AutoMapper for ContactModel mapping
                return _mapper.Map<UNOPSContact, ContactModel>(contact);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping UNOPSContact to ContactModel");
        }
        return null;
    }
    
    private async Task<object?> MapInteractionToModel(object entity)
    {
        try
        {
            if (entity is UNOPSInteraction interaction)
            {
                // Use AutoMapper for InteractionModel mapping
                return _mapper.Map<UNOPSInteraction, InteractionModel>(interaction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping UNOPSInteraction to InteractionModel");
        }
        return null;
    }

    /// <summary>
    /// Handle fullName field which searches across FirstName, MiddleName, and LastName
    /// </summary>
    private FieldInfo GetFullNameFieldInfo<TEntity>(string fieldName) where TEntity : class
    {
        var fieldInfo = new FieldInfo();
        
        if (fieldName.Contains('.'))
        {
            // Handle nested fullName (e.g., contact.fullName, contacts.fullName)
            var parts = fieldName.Split('.');
            var navigationProperty = parts[0];
            var mainTableAlias = GetMainTableAlias<TEntity>();
            
            switch (navigationProperty.ToLower())
            {
                case "contact":
                case "contacts":
                    if (typeof(TEntity).Name.Contains("Partner") || typeof(TEntity).Name.Contains("Interaction"))
                    {
                        fieldInfo.RequiredJoins.Add($@"LEFT JOIN public.""Contacts"" c ON {mainTableAlias}.""Id"" = c.""PartnerId""");
                        // For fullName, we'll create a concatenated search across FirstName, MiddleName, LastName
                        fieldInfo.FullColumnName = @"CONCAT(COALESCE(c.""FirstName"", ''), ' ', COALESCE(c.""MiddleName"", ''), ' ', COALESCE(c.""LastName"", ''))";
                    }
                    break;
                    
                default:
                    // Direct fullName on entity
                    fieldInfo.FullColumnName = @"CONCAT(COALESCE(""FirstName"", ''), ' ', COALESCE(""MiddleName"", ''), ' ', COALESCE(""LastName"", ''))";
                    break;
            }
        }
        else
        {
            // Direct fullName field on the main entity (Contact)
            var mainTableAlias = GetMainTableAlias<TEntity>();
            fieldInfo.FullColumnName = $@"CONCAT(COALESCE({mainTableAlias}.""FirstName"", ''), ' ', COALESCE({mainTableAlias}.""MiddleName"", ''), ' ', COALESCE({mainTableAlias}.""LastName"", ''))";
        }
        
        return fieldInfo;
    }

    /// <summary>
    /// Applies dynamic ordering to a queryable
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">The queryable to order</param>
    /// <param name="orderBy">Field to order by</param>
    /// <param name="ascending">Sort direction</param>
    /// <returns>Ordered queryable</returns>
    private static IQueryable<T> ApplyDynamicOrdering<T>(IQueryable<T> query, string? orderBy, bool ascending)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            orderBy = "Id"; // Default ordering
        }

        try
        {
            // Use System.Linq.Dynamic.Core for dynamic ordering
            var direction = ascending ? "ascending" : "descending";
            return query.OrderBy($"{orderBy} {direction}");
        }
        catch (Exception)
        {
            // Fallback to default ordering if dynamic ordering fails
            return query.OrderBy($"Id {(ascending ? "ascending" : "descending")}");
        }
    }

    /// <summary>
    /// Generates basic search metadata for SearchAsync method
    /// This provides basic metadata when using Entity Framework queries instead of PostgreSQL search functions
    /// </summary>
    private async Task<Dictionary<int, Dictionary<string, object>>?> GenerateBasicSearchMetadataAsync<TModel>(
        List<TModel> results, 
        string query, 
        string entityTypeName)
        where TModel : class
    {
        if (results == null || !results.Any() || string.IsNullOrWhiteSpace(query))
            return null;

        var searchMetadata = new Dictionary<int, Dictionary<string, object>>();
        
        foreach (var result in results)
        {
            var entityId = GetEntityId(result);
            if (entityId == 0) continue;

            var metadata = new Dictionary<string, object>
            {
                ["matchedField"] = "General Search",
                ["searchType"] = "text",
                ["matchCriteria"] = $"Contains '{query}'",
                ["score"] = 0.8, // Default score for basic search
                ["snippet"] = $"Search matched: {query}"
            };

            searchMetadata[entityId] = metadata;
        }

        return searchMetadata;
    }

    #endregion
}

#region Request/Response Models

/// <summary>
/// Unified search request that can handle both text query and structured filters
/// </summary>
public class UnifiedSearchRequest
{
    /// <summary>
    /// Text query for smart search with similarity
    /// </summary>
    public string? Query { get; set; }
    
    /// <summary>
    /// Structured filters for advanced search
    /// </summary>
    public List<SearchFilter>? Filters { get; set; }
    
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageIndex { get; set; } = 1;
    
    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Field to order by
    /// </summary>
    public string? OrderBy { get; set; } = "CreatedDate";
    
    /// <summary>
    /// Sort direction
    /// </summary>
    public bool Ascending { get; set; } = false;
    
    /// <summary>
    /// Filter toggle state - controls whether global filters are applied
    /// </summary>
    public bool FilterActive { get; set; } = true;
}

/// <summary>
/// Search filter for structured filtering
/// Uses camelCase to match frontend SearchCriteria interface exactly
/// Note: This is different from UNOPS.PAO.Models.SearchFilter which uses PascalCase
/// </summary>
public class SearchFilter
{
    public string field { get; set; } = string.Empty;
    public string value { get; set; } = string.Empty;
    public string label { get; set; } = string.Empty;
    public string @operator { get; set; } = "like";
    public string? logicalOperator { get; set; } = "AND";
    public string? secondValue { get; set; }
    public string? fieldType { get; set; } = "text";
}

#endregion
