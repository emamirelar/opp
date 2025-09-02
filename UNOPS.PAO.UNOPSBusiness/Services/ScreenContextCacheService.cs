using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public interface IScreenContextCacheService
{
    Task<object?> GetScreenContextAsync(string screenUrl, string userFocusContext, string userId);
    void InvalidateScreenContextCache(string screenUrl, string userFocusContext);
}

public class ScreenContextCacheService : IScreenContextCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ScreenContextCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly UNOPSAppDbContext _context;
    private readonly HttpClient _httpClient;
    
    private const string CACHE_KEY_PREFIX = "screen_context_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15); // Cache for 15 minutes
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(5); // Refresh if accessed within 5 minutes

    public ScreenContextCacheService(
        IMemoryCache cache, 
        ILogger<ScreenContextCacheService> logger, 
        IConfiguration configuration,
        UNOPSAppDbContext context,
        HttpClient httpClient)
    {
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
        _context = context;
        _httpClient = httpClient;
    }

    public async Task<object?> GetScreenContextAsync(string screenUrl, string userFocusContext, string userId)
    {
        try
        {
            var cacheKey = GetCacheKey(screenUrl, userFocusContext);
            
            if (_cache.TryGetValue(cacheKey, out var cachedContext))
            {
                _logger.LogDebug("Retrieved screen context from cache for URL: {ScreenUrl}", screenUrl);
                return cachedContext;
            }

            _logger.LogDebug("Screen context not in cache, generating for URL: {ScreenUrl}", screenUrl);
            
            // Generate screen context
            var screenContext = await GenerateScreenContextAsync(screenUrl, userFocusContext, userId);
            
            // Cache the result
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                SlidingExpiration = SlidingExpiration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, screenContext, cacheOptions);
            _logger.LogDebug("Cached screen context for URL: {ScreenUrl}", screenUrl);
            
            return screenContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screen context for URL: {ScreenUrl}", screenUrl);
            return null;
        }
    }

    public void InvalidateScreenContextCache(string screenUrl, string userFocusContext)
    {
        try
        {
            var cacheKey = GetCacheKey(screenUrl, userFocusContext);
            _cache.Remove(cacheKey);
            _logger.LogDebug("Invalidated screen context cache for URL: {ScreenUrl}", screenUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating screen context cache for URL: {ScreenUrl}", screenUrl);
        }
    }

    private string GetCacheKey(string screenUrl, string userFocusContext)
    {
        var normalizedScreenUrl = screenUrl?.Trim() ?? "";
        var normalizedUserFocus = userFocusContext?.Trim() ?? "";
        return $"{CACHE_KEY_PREFIX}{normalizedScreenUrl}_{normalizedUserFocus}".Replace("/", "_").Replace(":", "_");
    }

    private async Task<object> GenerateScreenContextAsync(string screenUrl, string userFocusContext, string userId)
    {
        try
        {
            // Apply the same logic as the Python agent
            var originalUserFocus = userFocusContext;
            if (screenUrl != userFocusContext && string.IsNullOrEmpty(userFocusContext) && !string.IsNullOrEmpty(screenUrl))
            {
                userFocusContext = screenUrl;
                _logger.LogDebug("Applied condition: user_focus_context set to '{UserFocusContext}' (was empty)", userFocusContext);
            }

            var focusRelationship = screenUrl == userFocusContext ? "same" : "different";
            var urlToAnalyze = !string.IsNullOrEmpty(userFocusContext) ? userFocusContext : screenUrl;

            _logger.LogDebug("Analyzing URL: {UrlToAnalyze}", urlToAnalyze);

            // Parse URL to extract entity information
            var urlAnalysis = AnalyzeUrl(urlToAnalyze);
            
            // If we have an entity and ID, fetch the entity details
            object? entityDetails = null;
            if (!string.IsNullOrEmpty(urlAnalysis.EntityType) && !string.IsNullOrEmpty(urlAnalysis.EntityId))
            {
                entityDetails = await FetchEntityDetailsAsync(urlAnalysis.EntityType, urlAnalysis.EntityId);
            }

            // Build screen context response
            var screenContext = new
            {
                original_screen_url = screenUrl,
                original_user_focus_context = originalUserFocus,
                resolved_user_focus_context = userFocusContext,
                focus_relationship = focusRelationship,
                screen_type = urlAnalysis.ScreenType,
                entity_in_focus = urlAnalysis.EntityType,
                entity_id_in_focus = urlAnalysis.EntityId,
                entity_details = entityDetails,
                screen_name = GetScreenName(urlAnalysis),
                screen_url = urlToAnalyze,
                screen_data = new
                {
                    entity = urlAnalysis.EntityType,
                    entity_id = urlAnalysis.EntityId,
                    view_type = urlAnalysis.ViewType,
                    intelligent_context = true,
                    parsed_from_state = urlToAnalyze,
                    detected_entity = urlAnalysis.EntityType
                },
                recommendations = GetRecommendations(focusRelationship)
            };

            return screenContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating screen context");
            return new
            {
                screen_name = "Error Page",
                screen_url = screenUrl ?? "unknown",
                screen_type = "error",
                screen_data = new { error = ex.Message }
            };
        }
    }

    private UrlAnalysis AnalyzeUrl(string url)
    {
        var analysis = new UrlAnalysis
        {
            OriginalUrl = url,
            ScreenType = "unknown",
            EntityType = null,
            EntityId = null,
            ViewType = "unknown"
        };

        if (string.IsNullOrEmpty(url) || url == "/")
        {
            analysis.ScreenType = "homepage";
            analysis.ViewType = "homepage";
            return analysis;
        }

        // Handle AI assistant mode
        if (url.ToLower().Contains("/ai"))
        {
            analysis.ScreenType = "ai_assistant_mode";
            analysis.ViewType = "ai_fullscreen";
            return analysis;
        }

        // Parse URL path
        var pathParts = url.Trim('/').Split('/').Where(part => !string.IsNullOrEmpty(part)).ToArray();
        
        if (pathParts.Length == 0)
        {
            analysis.ScreenType = "homepage";
            analysis.ViewType = "homepage";
            return analysis;
        }

        // Entity patterns mapping
        var entityPatterns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "partners", "Partner" },
            { "contacts", "Contact" },
            { "interactions", "Interaction" },
            { "aiprompts", "AiPrompt" },
            { "partnerships", "Partner" },
            { "partner-tree", "PartnerTree" },
            { "partnertree", "PartnerTree" }
        };

        // Look for entity patterns
        for (int i = 0; i < pathParts.Length; i++)
        {
            if (entityPatterns.TryGetValue(pathParts[i], out var entityType))
            {
                analysis.EntityType = entityType;
                
                // Look for ID in the next part
                if (i + 1 < pathParts.Length)
                {
                    var nextPart = pathParts[i + 1];
                    if (IsLikelyId(nextPart))
                    {
                        analysis.EntityId = nextPart;
                        analysis.ScreenType = "entity_detail_page";
                        analysis.ViewType = "detail";
                        return analysis;
                    }
                }
                
                analysis.ScreenType = "entity_list_page";
                analysis.ViewType = "list";
                return analysis;
            }
        }

        // Check for form pages
        if (pathParts.Any(part => new[] { "create", "edit", "new", "form" }.Contains(part.ToLower())))
        {
            analysis.ScreenType = "form_page";
            analysis.ViewType = "form";
            return analysis;
        }

        // Dashboard pages
        if (pathParts.Any(part => part.ToLower().Contains("dashboard")))
        {
            analysis.ScreenType = "dashboard_overview";
            analysis.ViewType = "dashboard";
            return analysis;
        }

        analysis.ScreenType = "specific_page";
        analysis.ViewType = "generic_page";
        return analysis;
    }

    private bool IsLikelyId(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // Numeric ID
        if (int.TryParse(value, out _))
            return true;

        // GUID pattern
        if (Guid.TryParse(value, out _))
            return true;

        // Long alphanumeric (likely ID)
        if (value.Length > 10 && Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
            return true;

        return false;
    }

    private async Task<object?> FetchEntityDetailsAsync(string entityType, string entityId)
    {
        try
        {
            // Map entity types to database tables and fetch basic info
            return entityType.ToLower() switch
            {
                "partner" => await GetPartnerDetailsAsync(entityId),
                "contact" => await GetContactDetailsAsync(entityId),
                "interaction" => await GetInteractionDetailsAsync(entityId),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching entity details for {EntityType} {EntityId}", entityType, entityId);
            return new { error = $"Failed to fetch {entityType} details: {ex.Message}" };
        }
    }

    private async Task<object?> GetPartnerDetailsAsync(string partnerId)
    {
        if (!int.TryParse(partnerId, out var id))
            return null;

        var partner = await _context.Partners
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Status,
                p.PartnerLongDescription,
                EntityType = "Partner"
            })
            .FirstOrDefaultAsync();

        return partner;
    }

    private async Task<object?> GetContactDetailsAsync(string contactId)
    {
        if (!int.TryParse(contactId, out var id))
            return null;

        var contact = await _context.Contacts
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                Name = c.FirstName + " " + c.LastName,
                c.Email,
                c.Title,
                c.Department,
                EntityType = "Contact"
            })
            .FirstOrDefaultAsync();

        return contact;
    }

    private async Task<object?> GetInteractionDetailsAsync(string interactionId)
    {
        if (!int.TryParse(interactionId, out var id))
            return null;

        var interaction = await _context.Interactions
            .Where(i => i.Id == id)
            .Select(i => new
            {
                i.Id,
                Name = i.Subject,
                i.Subject,
                i.Type,
                i.CreatedDate,
                EntityType = "Interaction"
            })
            .FirstOrDefaultAsync();

        return interaction;
    }

    private string GetScreenName(UrlAnalysis analysis)
    {
        if (!string.IsNullOrEmpty(analysis.EntityType))
        {
            return analysis.ViewType == "detail" 
                ? $"{analysis.EntityType} Details"
                : $"{analysis.EntityType} List";
        }

        return analysis.ScreenType switch
        {
            "homepage" => "Dashboard",
            "ai_assistant_mode" => "AI Assistant",
            "dashboard_overview" => "Dashboard Overview",
            "form_page" => "Form Page",
            _ => "Application Page"
        };
    }

    private string[] GetRecommendations(string focusRelationship)
    {
        return focusRelationship == "same"
            ? new[] { "User is focusing on this context - prioritize this entity in responses" }
            : new[] { "User has separate focus - ask about current focus entity when relevant" };
    }

    private class UrlAnalysis
    {
        public string OriginalUrl { get; set; } = "";
        public string ScreenType { get; set; } = "";
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string ViewType { get; set; } = "";
    }
}
