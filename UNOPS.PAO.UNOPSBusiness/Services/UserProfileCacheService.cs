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

    public async Task<object?> GetCachedUserProfileAsync(string userId)
    {
        try
        {
            var cacheKey = GetCacheKey(userId);
            if (_cache.TryGetValue(cacheKey, out var cachedProfile))
            {
                _logger.LogDebug("Retrieved user profile from cache for user: {UserId}", userId);
                return cachedProfile;
            }
            
            _logger.LogDebug("User profile not found in cache for user: {UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile from cache for user: {UserId}", userId);
            return null;
        }
    }

    public async Task SetCachedUserProfileAsync(string userId, object userProfile)
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
}
