using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;

/// <summary>
/// Mock implementations for cache services used in testing
/// </summary>

public class MockUserProfileCacheService : IUserProfileCacheService
{
    public Task<object?> GetCachedUserProfileAsync(string userId) => Task.FromResult<object?>(null);
    public Task SetCachedUserProfileAsync(string userId, object userProfile) => Task.CompletedTask;
    public void InvalidateUserProfileCache(string userId) { }
    public string GetCacheKey(string userId) => $"test_user_{userId}";
    public Task<Dictionary<int, string>> GetCachedUserNamesBatchAsync(IEnumerable<int> userIds) 
        => Task.FromResult(new Dictionary<int, string>());
    public Task SetCachedUserNamesBatchAsync(Dictionary<int, string> userNames) => Task.CompletedTask;
}

public class MockScreenContextCacheService : IScreenContextCacheService
{
    public Task<object?> GetScreenContextAsync(string screenUrl, string userFocusContext, string userId) 
        => Task.FromResult<object?>(null);
    public void InvalidateScreenContextCache(string screenUrl, string userFocusContext) { }
}

public class MockGeoTimeCacheService : IGeoTimeCacheService
{
    public Task<object?> GetGeoTimeDataAsync(string? userIpAddress = null) => Task.FromResult<object?>(null);
    public void InvalidateGeoTimeCache(string? userIpAddress = null) { }
}
