using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public interface IAiPromptCacheService
{
    Task<string?> GetCachedResultAsync(string promptType, string entityId);
    Task<AiPromptCacheEntry?> GetCachedEntryAsync(string promptType, string entityId);
    Task SetCachedResultAsync(string promptType, string entityId, string fullyFormedSystemInstructions, 
        string fullyFormedUserPrompt, string geminiResult, int cacheInvalidationMinutes);
    Task InvalidateCache(string promptType, string entityId);
    void InvalidateAllForPrompt(string promptType);
}

public class AiPromptCacheService : IAiPromptCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AiPromptCacheService> _logger;
    private const string CACHE_KEY_PREFIX = "ai_prompt_";

    public AiPromptCacheService(IMemoryCache cache, ILogger<AiPromptCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<string?> GetCachedResultAsync(string promptType, string entityId)
    {
        try
        {
            var cacheKey = GetCacheKey(promptType, entityId);
            
            if (_cache.TryGetValue(cacheKey, out var cachedData))
            {
                var cacheEntry = cachedData as AiPromptCacheEntry;
                _logger.LogDebug("Retrieved AI prompt result from cache for prompt {PromptType}, entity {EntityId}", 
                    promptType, entityId);
                return Task.FromResult(cacheEntry?.GeminiResult);
            }
            
            _logger.LogDebug("AI prompt result not found in cache for prompt {PromptType}, entity {EntityId}", 
                promptType, entityId);
            return Task.FromResult<string?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI prompt result from cache for prompt {PromptType}, entity {EntityId}", 
                promptType, entityId);
            return Task.FromResult<string?>(null);
        }
    }

    public Task<AiPromptCacheEntry?> GetCachedEntryAsync(string promptType, string entityId)
    {
        try
        {
            var cacheKey = GetCacheKey(promptType, entityId);
            
            if (_cache.TryGetValue(cacheKey, out var cachedData))
            {
                var cacheEntry = cachedData as AiPromptCacheEntry;
                _logger.LogDebug("Retrieved AI prompt entry from cache for prompt {PromptType}, entity {EntityId}", 
                    promptType, entityId);
                return Task.FromResult(cacheEntry);
            }
            
            _logger.LogDebug("AI prompt entry not found in cache for prompt {PromptType}, entity {EntityId}", 
                promptType, entityId);
            return Task.FromResult<AiPromptCacheEntry?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI prompt entry from cache for prompt {PromptType}, entity {EntityId}", 
                promptType, entityId);
            return Task.FromResult<AiPromptCacheEntry?>(null);
        }
    }

    public Task SetCachedResultAsync(string promptType, string entityId, string fullyFormedSystemInstructions, 
        string fullyFormedUserPrompt, string geminiResult, int cacheInvalidationMinutes)
    {
        try
        {
            var cacheKey = GetCacheKey(promptType, entityId);
            var cacheEntry = new AiPromptCacheEntry
            {
                PromptType = promptType,
                EntityId = entityId,
                SystemInstructions = fullyFormedSystemInstructions,
                UserPrompt = fullyFormedUserPrompt,
                Result = geminiResult,
                CreatedAt = DateTime.UtcNow
            };

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheInvalidationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(Math.Min(cacheInvalidationMinutes / 2, 30)), // Half of absolute or 30 min max
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, cacheEntry, cacheOptions);
            _logger.LogDebug("Cached AI prompt result for prompt {PromptType}, entity {EntityId} (expires in {Minutes} minutes)", 
                promptType, entityId, cacheInvalidationMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching AI prompt result for prompt {PromptType}, entity {EntityId}", 
                promptType, entityId);
        }
        return Task.CompletedTask;
    }

    public Task InvalidateCache(string promptType, string entityId)
    {
        try
        {
            var cacheKey = GetCacheKey(promptType, entityId);
            _cache.Remove(cacheKey);
            _logger.LogDebug("Invalidated AI prompt cache for prompt {PromptType}, entity {EntityId}", promptType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating AI prompt cache for prompt {PromptType}, entity {EntityId}", 
                promptType, entityId);
        }
        return Task.CompletedTask;
    }

    public void InvalidateAllForPrompt(string promptType)
    {
        // Note: MemoryCache doesn't have built-in pattern-based removal
        // For more advanced scenarios, consider using a distributed cache like Redis
        _logger.LogWarning("InvalidateAllForPrompt not fully supported with MemoryCache for prompt {PromptType}. Consider individual invalidation.", promptType);
    }

    private string GetCacheKey(string promptType, string entityId)
    {
        return $"{CACHE_KEY_PREFIX}{promptType}_{entityId}";
    }
}

/// <summary>
/// Cache entry model for AI prompt results
/// </summary>
public class AiPromptCacheEntry
{
    public string PromptType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string SystemInstructions { get; set; } = string.Empty;
    public string? UserPrompt { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string FullyFormedSystemInstructions 
    { 
        get => SystemInstructions; 
        set => SystemInstructions = value; 
    }
    public string? FullyFormedUserPrompt 
    { 
        get => UserPrompt; 
        set => UserPrompt = value; 
    }
    public string GeminiResult 
    { 
        get => Result; 
        set => Result = value; 
    }
}
