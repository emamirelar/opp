// ============================================================================
// INTELLIGENT GLOBAL SEARCH CONTROLLER
// ============================================================================
// This controller provides advanced hybrid search capabilities that intelligently
// combines field-specific text search with semantic embedding search.
//
// SMART EMBEDDING DECISION ENGINE:
// - Analyzes query characteristics (length, complexity, intent)
// - Detects question words and semantic patterns
// - Identifies conceptual terms and relationship context
// - Automatically chooses optimal search strategy
//
// HYBRID SEARCH ARCHITECTURE:
// - Field-Specific Search: Searches actual database columns with intelligent scoring
// - Semantic Search: Uses AI embeddings for meaning-based matching
// - Dynamic Entity Discovery: Automatically adapts to new entity types
// - Performance Optimized: Only generates embeddings when semantically beneficial
//
// SEARCH STRATEGIES:
// - Short/Exact Queries: Fast text-only search for names, codes, IDs
// - Complex Queries: Hybrid approach for natural language and conceptual searches
// - Question-based: Semantic search for "what", "where", "how", "find" queries
// - Conceptual Terms: Embedding search for "project manager", "procurement expert"
//
// FUTURE-PROOF DESIGN:
// - No hardcoded entity types - discovers from EntityEmbeddings table
// - Extensible scoring system for new field types
// - Configurable boost factors for different search modes
// - Rich response format with detailed scoring and match criteria
//
// PERFORMANCE CONSIDERATIONS:
// - Embedding generation only when semantically necessary
// - Graceful fallback to text search if embedding fails
// - Efficient database queries with proper indexing
// - Comprehensive logging for monitoring and optimization
//
// GLOBAL FILTER INTEGRATION:
// - Search results now respect user's global filter preferences
// - Applies org unit, date range, "related to me", and other global filters
// - Filters applied at retrieval time for consistency with list views
// - Graceful fallback if global filtering fails (returns unfiltered results)
// - Maintains search performance while ensuring data consistency
// ============================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Presentation.Helpers;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Enums;
using System.Reflection;
using System.Security.Claims;
using System.Dynamic;
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("api/global")]
[Authorize(AuthenticationSchemes = "IAP")]
public class GlobalController : BaseController
{
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly AiContextualService _aiContextualService;
    private readonly IManagerWrapper _managerWrapper;
    private readonly GlobalFilterService _globalFilterService;
    private readonly AdvancedSearchService _advancedSearchService;

    public GlobalController(
        IUserPreferenceService userPreferenceService,
        UserManager<PAOIdentityUser> userManager,
        AiContextualService aiContextualService,
        IManagerWrapper managerWrapper,
        GlobalFilterService globalFilterService,
        AdvancedSearchService advancedSearchService,
        UserResolverService<int> userResolverService, 
        IAuthorizationService authorizationService,
        ILogger<GlobalController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _userPreferenceService = userPreferenceService;
        _userManager = userManager;
        _aiContextualService = aiContextualService;
        _managerWrapper = managerWrapper;
        _globalFilterService = globalFilterService;
        _advancedSearchService = advancedSearchService;
    }

    /// <summary>
    /// Retrieves user-specific preferences and settings for interface customization and default behaviors.
    /// </summary>
    /// <param name="id">User ID to get preferences for</param>
    /// <example_uses>
    /// Get my user preferences and settings
    /// Show user interface customizations
    /// Get default language and timezone settings
    /// Retrieve user's saved preferences
    /// Show personalization settings
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for their preferences, settings, customizations, or when loading user interface defaults.</when_to_use>
    /// <returns>User preference object with all customization settings</returns>
    [HttpGet(APIDictionary.GlobalUserPreferences)]
    public async Task<ActionResult> GetUserPreferences([FromQuery] string id)
    {
        return await HandleOperationAsync(async () =>
        {
            var userPreferences = await _userPreferenceService.GetUserPreferencesAsync(id);
            return userPreferences;
        });
    }

    /// <summary>
    /// Updates user-specific preferences and settings for interface customization and default behaviors.
    /// </summary>
    /// <param name="id">User ID to update preferences for</param>
    /// <param name="userPreferences">Updated user preference object containing new settings</param>
    /// <param name="userPreferences.language">Preferred language setting</param>
    /// <param name="userPreferences.timezone">Preferred timezone</param>
    /// <param name="userPreferences.theme">UI theme preference</param>
    /// <param name="userPreferences.defaultView">Default dashboard view</param>
    /// <param name="userPreferences.notifications">Notification preferences</param>
    /// <example_uses>
    /// Update my language preference to French
    /// Change my timezone to EST
    /// Set theme to dark mode
    /// Update notification settings
    /// Save my dashboard preferences
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to update their preferences, settings, or interface customizations.</when_to_use>
    /// <returns>Success confirmation</returns>
    [HttpPut(APIDictionary.GlobalUserPreferences)]
    public async Task<ActionResult> UpdateUserPreferences([FromQuery] string id, [FromBody] UserPreference userPreferences)
    {
        return await HandleOperationAsync(async () =>
        {
            await _userPreferenceService.UpdateUserPreferencesAsync(id, userPreferences);
            return Ok();
        });
    }

    /// <summary>
    /// Retrieves user's global filter settings and preferences for consistent filtering across all views.
    /// </summary>
    /// <param name="id">User ID to get global filters for</param>
    /// <example_uses>
    /// Get my global filter settings
    /// Show saved filter preferences
    /// Retrieve default filters for all pages
    /// Get user's persistent filter configuration
    /// Show global search and filter defaults
    /// </example_uses>
    /// <when_to_use>Use this when loading user's saved filter settings or when applying consistent filters across multiple views.</when_to_use>
    /// <returns>Global filter object with user's saved filter preferences</returns>
    [HttpGet(APIDictionary.GlobalFilters)]
    public async Task<ActionResult> GetGlobalFilters([FromQuery] string id)
    {
        return await HandleOperationAsync(async () =>
        {
            return await _userPreferenceService.GetGlobalFiltersAsync(id);
        });
    }

    /// <summary>
    /// Updates user's global filter settings and preferences for consistent filtering across all views.
    /// </summary>
    /// <param name="id">User ID to update global filters for</param>
    /// <param name="globalFilters">Updated global filter configuration</param>
    /// <param name="globalFilters.orgUnitId">Default organizational unit filter</param>
    /// <param name="globalFilters.defaultDateRange">Default date range for filtering</param>
    /// <param name="globalFilters.statusFilters">Default status filters</param>
    /// <param name="globalFilters.searchPreferences">Search behavior preferences</param>
    /// <example_uses>
    /// Save my default organizational unit filter
    /// Update global date range preferences
    /// Set default status filters for all views
    /// Save search behavior preferences
    /// Update persistent filter settings
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to save filter preferences that apply across all views and pages.</when_to_use>
    /// <returns>Success confirmation</returns>
    [HttpPut(APIDictionary.GlobalFilters)]
    public async Task<ActionResult> UpdateGlobalFilters([FromQuery] string id, [FromBody] GlobalFilters globalFilters)
    {
        return await HandleOperationAsync(async () =>
        {
            await _userPreferenceService.UpdateGlobalFiltersAsync(id, globalFilters);
            return Ok();
        });
    }

    /// <summary>
    /// Resets user's global filter settings to system defaults, clearing all custom filter preferences.
    /// </summary>
    /// <param name="id">User ID to reset global filters for</param>
    /// <example_uses>
    /// Reset my filter settings to defaults
    /// Clear all saved filter preferences
    /// Restore default filtering behavior
    /// Reset global search preferences
    /// Clear personalized filter settings
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to clear their saved filter preferences and return to system defaults.</when_to_use>
    /// <returns>Success confirmation</returns>
    [HttpPost(APIDictionary.GlobalFiltersReset)]
    public async Task<ActionResult> ResetGlobalFilters([FromQuery] string id)
    {
        return await HandleOperationAsync(async () =>
        {
            await _userPreferenceService.ResetGlobalFiltersAsync(id);
            return Ok();
        });
    }

    /// <summary>
    /// Retrieves the current user's preferred language setting for interface localization.
    /// </summary>
    /// <example_uses>
    /// Get my preferred language setting
    /// Show current language preference
    /// What language is my interface set to?
    /// Get user's localization setting
    /// Check current language configuration
    /// </example_uses>
    /// <when_to_use>Use this when loading interface language settings or when determining localization preferences.</when_to_use>
    /// <returns>User's preferred language code and settings</returns>
    [HttpGet(APIDictionary.PreferredLanguage)]
    public async Task<ActionResult> GetPreferredLanguage()
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            
            if (userId == null)
            {
                return BadRequest("User ID not found");
            }

            var userPreferences = await _userPreferenceService.GetUserPreferencesAsync(userId);
            var preferredLanguage = userPreferences?.GlobalFilters?.PreferredLanguage ?? "en";
            
            return Ok(new { language = preferredLanguage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting preferred language");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Updates the current user's preferred language setting for interface localization.
    /// </summary>
    /// <param name="language">Language code to set as preferred (e.g., 'en', 'fr', 'es')</param>
    /// <example_uses>
    /// Change my language to French
    /// Set interface language to Spanish
    /// Update my language preference to English
    /// Switch to Arabic interface
    /// Change localization to Portuguese
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to change their interface language or localization preferences.</when_to_use>
    /// <returns>Success confirmation</returns>
    [HttpPut(APIDictionary.PreferredLanguage)]
    public async Task<ActionResult> UpdatePreferredLanguage([FromBody] string language)
    {
        try
        {
            var userId = _userManager.GetUserId(User);
            
            if (userId == null)
            {
                return BadRequest("User ID not found");
            }

            var userPreferences = await _userPreferenceService.GetUserPreferencesAsync(userId);
            if (userPreferences == null)
            {
                // Convert userId string to int for UserPreference
                if (!int.TryParse(userId, out int userIdInt))
                {
                    return BadRequest("Invalid user ID format");
                }
                
                userPreferences = new UserPreference
                {
                    UserId = userIdInt,
                    Name = $"UserPreferences_{userId}",
                    GlobalFilters = new GlobalFilters()
                };
            }

            if (userPreferences.GlobalFilters == null)
            {
                userPreferences.GlobalFilters = new GlobalFilters();
            }

            // Get the current GlobalFilters, modify it, then set it back to trigger the setter
            var globalFilters = userPreferences.GlobalFilters;
            globalFilters.PreferredLanguage = language;
            userPreferences.GlobalFilters = globalFilters;
            
            await _userPreferenceService.UpdateUserPreferencesAsync(userId, userPreferences);
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating preferred language");
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Performs intelligent global search across all entities using hybrid text and semantic search capabilities.
    /// Automatically determines optimal search strategy based on query characteristics and complexity.
    /// </summary>
    /// <param name="q">Search query text</param>
    /// <param name="debug">Enable debug mode to show search strategy and scoring details (default: false)</param>
    /// <param name="fullResults">Return full detailed results instead of summary (default: false)</param>
    /// <example_uses>
    /// Search for "UNICEF project manager"
    /// Find "meetings about climate change"
    /// Look for "john.doe@example.com"
    /// Search "partners in Bangladesh"
    /// Find "documents related to health"
    /// Search for "WHO collaboration agreements"
    /// Look for "procurement experts"
    /// Find "project coordinators in Africa"
    /// </example_uses>
    /// <when_to_use>Use this when the user performs any search operation across the system - it automatically chooses between text search for exact matches and semantic search for conceptual queries.</when_to_use>
    /// <returns>Comprehensive search results with relevance scoring and entity details</returns>
    [HttpGet(APIDictionary.GlobalSearch)]
    public async Task<ActionResult> IntelligentGlobalSearch([FromQuery] string q, [FromQuery] bool debug = false, [FromQuery] bool fullResults = false, [FromQuery] bool filterActive = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search query cannot be empty");
            }

            var cleanedQuery = q.Trim();
            _logger.LogInformation("Processing search query: {Query}", cleanedQuery);

            // Determine if we need semantic search using enhanced heuristics
            bool needsSemanticSearch = ShouldUseSemanticSearch(cleanedQuery);
            
            string? embedding = null;
            if (needsSemanticSearch)
            {
                try
                {
                    _logger.LogInformation("Generating embedding for semantic search query: {Query}", cleanedQuery);
                    // Create embedding for semantic search
                    embedding = await _aiContextualService.CreateEmbeddingForText(cleanedQuery);
                    _logger.LogInformation("Successfully generated embedding for query: {Query}", cleanedQuery);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create embedding for query: {Query}. Falling back to text-only search.", cleanedQuery);
                    // Continue with text search only - this is expected behavior
                }
            }
            else
            {
                _logger.LogInformation("Using text-only search for query: {Query}", cleanedQuery);
            }

            // Call the PostgreSQL hybrid search function
            var searchResults = await CallSearchFunction(cleanedQuery, embedding, debug, filterActive);

            // Process results to get actual entity data
            var consolidatedResults = await ProcessAndConsolidateResults(searchResults, User, fullResults, filterActive);
            
            return Ok(consolidatedResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing search query: {Query}", q);
            return StatusCode(500, new { error = "An error occurred while processing your search request" });
        }
    }

    /// <summary>
    /// Determines if the search query should use semantic search (embeddings) using enhanced heuristics
    /// </summary>
    private bool ShouldUseSemanticSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        var cleanedQuery = query.Trim();
        var words = cleanedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var wordCount = words.Length;
        var queryLength = cleanedQuery.Length;

        // Skip semantic search for very short queries (likely exact matches)
        if (queryLength < 5 || wordCount == 1)
        {
            return false;
        }

        // Always use semantic search for longer, complex queries
        if (wordCount > 4 || queryLength > 30)
        {
            return true;
        }

        // Check for question words (semantic intent)
        var questionWords = new[] { 
            "what", "where", "how", "when", "why", "who", "which", "whose",
            "describe", "explain", "find", "show", "get", "search", "lookup", "locate"
        };
        if (questionWords.Any(qw => cleanedQuery.ToLower().Contains(qw)))
        {
            return true;
        }

        // Check for natural language patterns (semantic context)
        var semanticPatterns = new[] { 
            "similar to", "like", "related to", "about", "regarding", "concerning",
            "experience in", "expertise in", "working on", "involved in", "responsible for",
            "specializes in", "focused on", "deals with", "handles", "manages"
        };
        if (semanticPatterns.Any(pattern => cleanedQuery.ToLower().Contains(pattern)))
        {
            return true;
        }

        // Check for conceptual terms (benefit from semantic understanding)
        var conceptualTerms = new[] {
            "project", "manager", "development", "procurement", "partnership", "cooperation",
            "collaboration", "implementation", "coordination", "administration", "governance",
            "sustainability", "humanitarian", "emergency", "capacity", "training", "technical"
        };
        if (conceptualTerms.Any(term => cleanedQuery.ToLower().Contains(term)))
        {
            return true;
        }

        // Check for multiple entities or complex relationships
        if (Regex.IsMatch(cleanedQuery, @"\b(and|or|with|in|at|for|from|to)\b", RegexOptions.IgnoreCase))
        {
            return true;
        }

        // Check for descriptive adjectives (semantic nuance)
        var descriptiveWords = new[] {
            "experienced", "senior", "junior", "skilled", "expert", "specialized", "qualified",
            "international", "regional", "local", "remote", "onsite", "temporary", "permanent",
            "urgent", "priority", "strategic", "operational", "technical", "administrative"
        };
        if (descriptiveWords.Any(word => cleanedQuery.ToLower().Contains(word)))
        {
            return true;
        }

        // For medium-length queries (2-4 words, 5-30 chars), use text search
        // These are likely specific names, codes, or exact terms
        return false;
    }

    /// <summary>
    /// Calls the modular AdvancedSearchService to perform global search across all entities
    /// </summary>
    private async Task<object> CallSearchFunction(string query, string? embedding = null, bool debug = false, bool filterActive = true)
    {
        try
        {
            // Log the search strategy being used
            var strategy = embedding != null ? "hybrid (field + semantic)" : "modular-field-search";
            _logger.LogInformation("Executing {Strategy} search for query: {Query}, Debug: {Debug}", strategy, query, debug);
            
            // Use the enhanced modular search from AdvancedSearchService for better performance
            var searchResults = await _advancedSearchService.SearchAllEntitiesModularAsync(query, 1.0f, 15, filterActive);
            
            // Convert the GlobalSearchResponse to the expected format for ProcessAndConsolidateResults
            var formattedResults = new
            {
                availableEntities = new[] { "Partners", "Contacts", "Interactions" },
                results = new
                {
                    Partners = new
                    {
                        items = searchResults.Partners?.Select(r => new
                        {
                            entityId = r.EntityId,
                            score = r.Score,
                            matchedField = r.MatchedField,
                            fieldValue = r.FieldValue,
                            searchType = r.SearchType,
                            matchCriteria = r.MatchCriteria,
                            snippet = r.Snippet
                        }).ToArray() ?? new object[0]
                    },
                    Contacts = new
                    {
                        items = searchResults.Contacts?.Select(r => new
                        {
                            entityId = r.EntityId,
                            score = r.Score,
                            matchedField = r.MatchedField,
                            fieldValue = r.FieldValue,
                            searchType = r.SearchType,
                            matchCriteria = r.MatchCriteria,
                            snippet = r.Snippet
                        }).ToArray() ?? new object[0]
                    },
                    Interactions = new
                    {
                        items = searchResults.Interactions?.Select(r => new
                        {
                            entityId = r.EntityId,
                            score = r.Score,
                            matchedField = r.MatchedField,
                            fieldValue = r.FieldValue,
                            searchType = r.SearchType,
                            matchCriteria = r.MatchCriteria,
                            snippet = r.Snippet
                        }).ToArray() ?? new object[0]
                    }
                }
            };
            
            _logger.LogInformation("Modular search completed. Partners: {PartnerCount}, Contacts: {ContactCount}, Interactions: {InteractionCount}, ExecutionTime: {ExecutionTime}ms",
                searchResults.Partners?.Count ?? 0,
                searchResults.Contacts?.Count ?? 0,
                searchResults.Interactions?.Count ?? 0,
                searchResults.ExecutionTimeMs);
            
            return formattedResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during modular search for query: {Query}", query);
            return new { 
                searchQuery = query,
                hasEmbedding = embedding != null,
                strategy = "modular-field-search",
                error = "Error during modular search",
                message = "An error occurred while searching using modular functions",
                availableEntities = new string[0],
                results = new object()
            };
        }
    }

    /// <summary>
    /// Processes search results and retrieves actual entity data using reflection,
    /// preserving search metadata like matchedField for frontend transparency
    /// </summary>
    private async Task<object> ProcessAndConsolidateResults(object searchResults, ClaimsPrincipal user, bool fullResults = false, bool filterActive = true)
    {
        try
        {
            // Convert to JObject for processing
            var jsonString = JsonConvert.SerializeObject(searchResults);
            var resultObj = JObject.Parse(jsonString);

            // Extract available entities and convert to camelCase to match the results keys
            var availableEntitiesRaw = resultObj["availableEntities"]?.ToObject<string[]>() ?? new string[0];
            var availableEntities = availableEntitiesRaw.Select(entity => char.ToLowerInvariant(entity[0]) + entity.Substring(1)).ToArray();

            var consolidatedResults = new Dictionary<string, object>();

            // Process results section
            var resultsSection = resultObj["results"];
            if (resultsSection != null)
            {
                foreach (var entityProperty in resultsSection.Children<JProperty>())
                {
                    var entityType = entityProperty.Name;
                    var entityResults = entityProperty.Value;
                    var items = entityResults["items"];

                    if (items != null && items.HasValues)
                    {
                        // Extract entity IDs and preserve search metadata
                        var searchMetadata = new Dictionary<int, object>();
                        var entityIds = new List<int>();

                        foreach (var item in items.Children())
                        {
                            var entityId = item["entityId"]?.Value<int>() ?? 0;
                            if (entityId > 0)
                            {
                                entityIds.Add(entityId);
                                
                                // Preserve most useful search metadata for frontend display
                                var metadata = new Dictionary<string, object>();
                                
                                // Which field matched the search (most important for user understanding)
                                var matchedField = item["matchedField"]?.Value<string>();
                                if (!string.IsNullOrEmpty(matchedField))
                                {
                                    metadata["matchedField"] = matchedField;
                                }
                                
                                // Type of search performed
                                var searchType = item["searchType"]?.Value<string>();
                                if (!string.IsNullOrEmpty(searchType))
                                {
                                    metadata["searchType"] = searchType;
                                }
                                
                                // Match quality indicator
                                var matchCriteria = item["matchCriteria"]?.Value<string>();
                                if (!string.IsNullOrEmpty(matchCriteria))
                                {
                                    metadata["matchCriteria"] = matchCriteria;
                                }
                                
                                // Relevance score for sorting
                                var score = item["score"]?.Value<double?>();
                                if (score.HasValue)
                                {
                                    metadata["score"] = score.Value;
                                }
                                
                                // Preview snippet of matched content
                                var snippet = item["snippet"]?.Value<string>();
                                if (!string.IsNullOrEmpty(snippet))
                                {
                                    // Truncate snippet if too long for frontend display
                                    metadata["snippet"] = snippet.Length > 200 ? 
                                        snippet.Substring(0, 200) + "..." : snippet;
                                }
                                
                                searchMetadata[entityId] = metadata;
                            }
                        }

                        if (entityIds.Count > 0)
                        {
                            // Limit to top 3 results for dropdown display, unless fullResults is requested
                            var entityIdsToProcess = fullResults ? entityIds.ToArray() : entityIds.Take(3).ToArray();
                            
                            // Use reflection to get the appropriate manager and call GetByIdsAsync
                            var entityData = await GetEntityDataByIds(entityType, entityIdsToProcess, user, filterActive);
                            if (entityData != null)
                            {
                                // Enhance original entities with search metadata (no transformation needed)
                                var enhancedResults = EnhanceEntitiesWithSearchMetadata(entityData, searchMetadata);
                                // Convert entityType to camelCase to match JSON naming policy
                                var camelCaseKey = char.ToLowerInvariant(entityType[0]) + entityType.Substring(1);
                                consolidatedResults[camelCaseKey] = enhancedResults;
                            }
                        }
                    }
                }
            }

            return new
            {
                availableEntities = availableEntities,
                results = consolidatedResults
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing and consolidating search results");
            return new
            {
                availableEntities = new string[0],
                results = new Dictionary<string, object>(),
                error = "Error processing search results"
            };
        }
    }

    /// <summary>
    /// Enhances entity data with search metadata like matchedField for frontend transparency
    /// </summary>
    private object EnhanceEntitiesWithSearchMetadata(object entityData, Dictionary<int, object> searchMetadata)
    {
        try
        {
            if (entityData is IEnumerable<object> entities)
            {
                var enhancedEntities = new List<object>();
                
                foreach (var entity in entities)
                {
                    // Find the entity ID for metadata lookup
                    int? entityId = null;
                    var entityType = entity.GetType();
                    var idProperty = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    
                    if (idProperty != null)
                    {
                        var idValue = idProperty.GetValue(entity);
                        if (idValue != null && int.TryParse(idValue.ToString(), out int id))
                        {
                            entityId = id;
                        }
                    }
                    
                    // If we have search metadata for this entity, create an enhanced version
                    if (entityId.HasValue && searchMetadata.TryGetValue(entityId.Value, out var metadata))
                    {
                        // Create a dynamic object that flattens entity properties with search metadata
                        var flattened = new ExpandoObject() as IDictionary<string, object>;
                        
                        // Copy all properties from the original entity
                        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var property in properties)
                        {
                            try
                            {
                                var value = property.GetValue(entity);
                                flattened[property.Name] = value ?? string.Empty;
                            }
                            catch (Exception propEx)
                            {
                                _logger.LogWarning(propEx, "Failed to get property {PropertyName} from entity", property.Name);
                            }
                        }
                        
                        // Add search metadata - ASP.NET Core will serialize this as camelCase
                        flattened["_searchMetadata"] = metadata;
                        
                        enhancedEntities.Add(flattened);
                    }
                    else
                    {
                        // No metadata for this entity, return as-is - ASP.NET Core will serialize as camelCase
                        enhancedEntities.Add(entity);
                    }
                }
                
                return enhancedEntities;
            }
            
            return entityData;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enhance entities with search metadata, returning original data");
            return entityData;
        }
    }

    /// <summary>
    /// Uses reflection to access manager from UNOPSManagerWrapper and get entity data by IDs
    /// </summary>
    private async Task<object?> GetEntityDataByIds(string entityType, int[] entityIds, ClaimsPrincipal user, bool filterActive = true)
    {
        try
        {
            _logger.LogInformation("GetEntityDataByIds called for {EntityType} with filterActive={FilterActive}, EntityIds=[{EntityIds}]", 
                entityType, filterActive, string.Join(", ", entityIds));
            
            // Apply global filters to entity IDs before retrieving data
            if (user != null && filterActive)
            {
                var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    try
                    {
                        var globalFilters = await _userPreferenceService.GetGlobalFiltersAsync(currentUserId);
                        if (globalFilters != null)
                        {
                            _logger.LogInformation("Applying global filters to {EntityType} entities. Original count: {OriginalCount}", 
                                entityType, entityIds.Length);
                            
                            // Filter entity IDs based on global filters
                            entityIds = await FilterEntityIdsByGlobalFilters(entityIds, entityType, globalFilters, user);
                            
                            _logger.LogInformation("After global filtering: {EntityType} count reduced to {FilteredCount}", 
                                entityType, entityIds.Length);
                            
                            if (entityIds.Length == 0)
                            {
                                _logger.LogInformation("Global filters excluded all {EntityType} entities from search results", entityType);
                                return new List<object>(); // Return empty list if all entities are filtered out
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error applying global filters to {EntityType} search results, proceeding without global filters", entityType);
                        // Continue with original entity IDs if global filtering fails
                    }
                }
            }
            else
            {
                _logger.LogInformation("Skipping global filters for {EntityType} - filterActive={FilterActive}, user={HasUser}", 
                    entityType, filterActive, user != null);
            }

            // Get manager field name from entity type
            string managerFieldName = GetManagerFieldName(entityType);
            
            _logger.LogInformation("Attempting to access manager field: {ManagerFieldName} for entity type: {EntityType} with IDs: [{EntityIds}]", 
                managerFieldName, entityType, string.Join(", ", entityIds));

            // Use reflection to get the private manager field from UNOPSManagerWrapper
            var wrapperType = _managerWrapper.GetType();
            var managerField = wrapperType.GetField(managerFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (managerField == null)
            {
                _logger.LogWarning("Manager field not found: {ManagerFieldName}", managerFieldName);
                return null;
            }

            var manager = managerField.GetValue(_managerWrapper);
            if (manager == null)
            {
                _logger.LogWarning("Manager instance is null for field: {ManagerFieldName}", managerFieldName);
                return null;
            }

            // Get and invoke the GetByIdsAsync method
            var managerType = manager.GetType();
            var method = managerType.GetMethod("GetByIdsAsync");
            if (method == null)
            {
                _logger.LogWarning("GetByIdsAsync method not found on manager type: {ManagerType}", managerType.Name);
                return null;
            }

            // Call the method with parameters
            var task = (Task?)method.Invoke(manager, new object[] { entityIds, user });
            if (task != null)
            {
                await task;

                // Get the result from the completed task
                var resultProperty = task.GetType().GetProperty("Result");
                var result = resultProperty?.GetValue(task);
                
                if (result is IEnumerable<object> resultList)
                {
                    var count = resultList.Count();
                    _logger.LogInformation("Successfully retrieved {ActualCount} out of {RequestedCount} {EntityType} entities using {ManagerFieldName}", 
                        count, entityIds.Length, entityType, managerFieldName);
                    
                    if (count == 0)
                    {
                        _logger.LogWarning("No {EntityType} entities returned after RBAC filtering for IDs: [{EntityIds}]", 
                            entityType, string.Join(", ", entityIds));
                    }
                }
                else
                {
                    _logger.LogWarning("Unexpected result type from GetByIdsAsync: {ResultType}", result?.GetType().Name ?? "null");
                }

                return result;
            }
            return (object?)null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity data for type: {EntityType} with IDs: [{EntityIds}]", 
                entityType, string.Join(", ", entityIds));
            return null;
        }
    }

    /// <summary>
    /// Filters entity IDs based on user's global filter preferences
    /// </summary>
    private async Task<int[]> FilterEntityIdsByGlobalFilters(int[] entityIds, string entityType, dynamic globalFilters, ClaimsPrincipal user)
    {
        try
        {
            if (entityIds == null || entityIds.Length == 0)
                return entityIds;

            _logger.LogInformation("Applying global filters to {Count} {EntityType} entities", entityIds.Length, entityType);

            // Apply global filters based on entity type
            var filteredIds = await ApplyGlobalFiltersForEntityType(entityIds, entityType, user);
            var filteredArray = filteredIds.Where(id => entityIds.Contains(id)).ToArray();
            
            _logger.LogInformation("Global filters reduced {OriginalCount} {EntityType} entities to {FilteredCount}", 
                entityIds.Length, entityType, filteredArray.Length);
            
            return filteredArray;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying global filters to {EntityType} entities, returning original IDs", entityType);
            return entityIds; // Return original IDs if filtering fails
        }
    }

    /// <summary>
    /// Builds a queryable for the specified entity type with the given IDs
    /// </summary>
    private async Task<List<int>> ApplyGlobalFiltersForEntityType(int[] entityIds, string entityType, ClaimsPrincipal user)
    {
        try
        {
            var context = _aiContextualService._context;
            
            return entityType.ToLower() switch
            {
                "partners" => await ApplyGlobalFiltersToPartners(entityIds, user, context),
                "contacts" => await ApplyGlobalFiltersToContacts(entityIds, user, context),
                "interactions" => await ApplyGlobalFiltersToInteractions(entityIds, user, context),
                "baseengagements" => await ApplyGlobalFiltersToBaseEngagements(entityIds, user, context),
                _ => entityIds.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying global filters for entity type: {EntityType}", entityType);
            return entityIds.ToList();
        }
    }

    private async Task<List<int>> ApplyGlobalFiltersToPartners(int[] entityIds, ClaimsPrincipal user, UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext context)
    {
        var query = context.Set<UNOPS.PAO.UNOPSDomain.Entities.UNOPSPartner>()
            .Where(p => entityIds.Contains(p.Id) && !p.IsDeleted);
        
        var filteredQuery = await _globalFilterService.ApplyGlobalFiltersAsync(query, user);
        return await filteredQuery.Select(p => p.Id).ToListAsync();
    }

    private async Task<List<int>> ApplyGlobalFiltersToContacts(int[] entityIds, ClaimsPrincipal user, UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext context)
    {
        var query = context.Set<UNOPS.PAO.UNOPSDomain.Entities.UNOPSContact>()
            .Where(c => entityIds.Contains(c.Id) && !c.IsDeleted);
        
        var filteredQuery = await _globalFilterService.ApplyGlobalFiltersAsync(query, user);
        return await filteredQuery.Select(c => c.Id).ToListAsync();
    }

    private async Task<List<int>> ApplyGlobalFiltersToInteractions(int[] entityIds, ClaimsPrincipal user, UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext context)
    {
        var query = context.Set<UNOPS.PAO.Domain.Entities.Interaction>()
            .Where(i => entityIds.Contains(i.Id) && !i.IsDeleted);
        
        var filteredQuery = await _globalFilterService.ApplyGlobalFiltersAsync(query, user);
        return await filteredQuery.Select(i => i.Id).ToListAsync();
    }

    private async Task<List<int>> ApplyGlobalFiltersToBaseEngagements(int[] entityIds, ClaimsPrincipal user, UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext context)
    {
        var query = context.Set<UNOPS.PAO.UNOPSDomain.Entities.BaseEngagement>()
            .Where(e => entityIds.Contains(e.Id) && !e.IsDeleted);
        
        var filteredQuery = await _globalFilterService.ApplyGlobalFiltersAsync(query, user);
        return await filteredQuery.Select(e => e.Id).ToListAsync();
    }

    /// <summary>
    /// Maps entity type to the corresponding private field name in UNOPSManagerWrapper
    /// </summary>
    private string GetManagerFieldName(string entityType)
    {
        return entityType.ToLower() switch
        {
            "contacts" => "contactManager",
            "interactions" => "interactionManager", 
            "partners" => "partnerManager",
            _ => $"{entityType.ToLower().TrimEnd('s')}Manager" // Fallback pattern
        };
    }
} 



