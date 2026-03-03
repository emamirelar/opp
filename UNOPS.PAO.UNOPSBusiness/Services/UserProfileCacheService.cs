using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public interface IUserProfileCacheService
{
    Task<object?> GetCachedUserProfileAsync(string userId);
    Task SetCachedUserProfileAsync(string userId, object userProfile);
    void InvalidateUserProfileCache(string userId);
    string GetCacheKey(string userId);
    Task<Dictionary<int, string>> GetCachedUserNamesBatchAsync(IEnumerable<int> userIds);
    Task SetCachedUserNamesBatchAsync(Dictionary<int, string> userNames);
}

public class UserProfileCacheService : IUserProfileCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserProfileCacheService> _logger;
    private const string CACHE_KEY_PREFIX = "user_profile_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30); // Cache for 30 minutes

    public UserProfileCacheService(IMemoryCache cache, ILogger<UserProfileCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public string GetCacheKey(string userId)
    {
        return $"{CACHE_KEY_PREFIX}{userId}";
    }

    public Task<object?> GetCachedUserProfileAsync(string userId)
    {
        try
        {
            var cacheKey = GetCacheKey(userId);
            if (_cache.TryGetValue(cacheKey, out var cachedProfile))
            {
                _logger.LogDebug("Retrieved user profile from cache for user: {UserId}", userId);
                return Task.FromResult<object?>(cachedProfile);
            }
            
            _logger.LogDebug("User profile not found in cache for user: {UserId}", userId);
            return Task.FromResult<object?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile from cache for user: {UserId}", userId);
            return Task.FromResult<object?>(null);
        }
    }

    public Task SetCachedUserProfileAsync(string userId, object userProfile)
    {
        try
        {
            var cacheKey = GetCacheKey(userId);
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(15), // Refresh if accessed within 15 minutes
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, userProfile, cacheOptions);
            _logger.LogDebug("Cached user profile for user: {UserId}, expires in: {Expiration}", userId, CacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching user profile for user: {UserId}", userId);
        }
        return Task.CompletedTask;
    }

    public void InvalidateUserProfileCache(string userId)
    {
        try
        {
            var cacheKey = GetCacheKey(userId);
            _cache.Remove(cacheKey);
            _logger.LogDebug("Invalidated user profile cache for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating user profile cache for user: {UserId}", userId);
        }
    }

    public Task<Dictionary<int, string>> GetCachedUserNamesBatchAsync(IEnumerable<int> userIds)
    {
        var result = new Dictionary<int, string>();
        var uncachedUserIds = new List<int>();

        try
        {
            foreach (var userId in userIds.Where(id => id > 0))
            {
                var cacheKey = GetCacheKey(userId.ToString());
                if (_cache.TryGetValue(cacheKey, out var cachedName) && cachedName is string userName)
                {
                    result[userId] = userName;
                }
                else
                {
                    uncachedUserIds.Add(userId);
                }
            }

            _logger.LogDebug("Retrieved {CachedCount} user names from cache, {UncachedCount} need to be loaded", 
                result.Count, uncachedUserIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user names from cache");
            // Return empty result and let caller handle all users as uncached
            return Task.FromResult(new Dictionary<int, string>());
        }

        return Task.FromResult(result);
    }

    public Task SetCachedUserNamesBatchAsync(Dictionary<int, string> userNames)
    {
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(15),
                Priority = CacheItemPriority.Normal
            };

            foreach (var kvp in userNames)
            {
                var cacheKey = GetCacheKey(kvp.Key.ToString());
                _cache.Set(cacheKey, kvp.Value, cacheOptions);
            }

            _logger.LogDebug("Cached {Count} user names, expires in: {Expiration}", 
                userNames.Count, CacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching user names batch");
        }
        return Task.CompletedTask;
    }
}
