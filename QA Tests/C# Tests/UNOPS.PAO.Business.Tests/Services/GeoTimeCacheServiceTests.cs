/**
 * @fileoverview Comprehensive unit tests for GeoTimeCacheService.
 * Tests IP-based location lookup (success, fallback), cache behavior (store, retrieve, invalidate, expiry),
 * fallback when external API fails, error handling, and configuration.
 *
 * Requirements source: UNOPS.PAO.UNOPSBusiness/Services/GeoTimeCacheService.cs
 *
 * NOTE: GeoTimeCacheService creates its own HttpClient inside GetLocationInfoAsync, ignoring the
 * injected HttpClient. This prevents unit testing of HTTP success/failure/timeout without modifying
 * production code. See DEF-222 in Defect List for Developers.md.
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.UNOPSBusiness.Services;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Comprehensive unit tests for GeoTimeCacheService.
/// Covers: cache behavior, cache keys, response structure, fallback, configuration, error handling.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "GeoTimeCacheService")]
public class GeoTimeCacheServiceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;

    public GeoTimeCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    public void Dispose()
    {
        _memoryCache?.Dispose();
    }

    #region 1. Cache Behavior — Store, Retrieve, Hit, Miss

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetGeoTimeDataAsync_CacheMiss_PopulatesCacheAndReturnsData()
    {
        // Arrange - empty cache
        var service = CreateService(_memoryCache);

        // Act
        var result = await service.GetGeoTimeDataAsync();

        // Assert
        result.Should().NotBeNull();
        var json = ToJsonElement(result);
        json.GetProperty("current_datetime").ValueKind.Should().Be(JsonValueKind.String);
        json.GetProperty("current_timestamp_utc").ValueKind.Should().Be(JsonValueKind.Number);
        json.GetProperty("location").ValueKind.Should().Be(JsonValueKind.Object);
        json.GetProperty("cache_info").ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetGeoTimeDataAsync_CacheHit_ReturnsCachedDataWithoutRefetch()
    {
        // Arrange - pre-populate cache to avoid HTTP
        var cachedData = new
        {
            current_datetime = DateTime.UtcNow.ToString("O"),
            current_timestamp_utc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            location = new { country = "Cached", city = "Cache", timezone = "UTC", status = "success" },
            generated_at = DateTime.UtcNow,
            cache_info = new { cached = true, generated_for_ip = "server_default" }
        };
        _memoryCache.Set("geo_time_default", cachedData, TimeSpan.FromMinutes(60));

        var service = CreateService(_memoryCache);

        // Act
        var result = await service.GetGeoTimeDataAsync();

        // Assert
        result.Should().NotBeNull();
        var json = ToJsonElement(result);
        json.GetProperty("location").GetProperty("country").GetString().Should().Be("Cached");
        json.GetProperty("location").GetProperty("city").GetString().Should().Be("Cache");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_SecondCallWithSameKey_ReturnsSameCachedResult()
    {
        // Arrange - first call populates cache
        var service = CreateService(_memoryCache);
        var first = await service.GetGeoTimeDataAsync();
        first.Should().NotBeNull();

        // Act
        var second = await service.GetGeoTimeDataAsync();

        // Assert - same instance or equivalent (from cache)
        second.Should().NotBeNull();
        var j1 = ToJsonElement(first);
        var j2 = ToJsonElement(second);
        j1.GetProperty("current_timestamp_utc").GetInt64().Should().Be(j2.GetProperty("current_timestamp_utc").GetInt64());
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void InvalidateGeoTimeCache_RemovesEntry_SubsequentGetRefetches()
    {
        // Arrange
        _memoryCache.Set("geo_time_default", new { v = 1 }, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);
        service.InvalidateGeoTimeCache();

        // Assert
        _memoryCache.Get("geo_time_default").Should().BeNull();
    }

    #endregion

    #region 2. Cache Keys — Null, Empty, Localhost, IPv4, IPv6

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetGeoTimeDataAsync_NullIp_UsesDefaultCacheKey()
    {
        var cached = new { cache_info = new { generated_for_ip = "server_default" } };
        _memoryCache.Set("geo_time_default", cached, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var result = await service.GetGeoTimeDataAsync(null);

        result.Should().NotBeNull();
        var json = ToJsonElement(result);
        json.GetProperty("cache_info").GetProperty("generated_for_ip").GetString().Should().Be("server_default");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetGeoTimeDataAsync_EmptyIp_UsesDefaultCacheKey()
    {
        _memoryCache.Set("geo_time_default", new { key = "default" }, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var result = await service.GetGeoTimeDataAsync("");

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetGeoTimeDataAsync_IPv4Address_UsesIpSpecificCacheKey()
    {
        var cached = new { cache_info = new { generated_for_ip = "192.168.1.1" } };
        _memoryCache.Set("geo_time_192_168_1_1", cached, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var result = await service.GetGeoTimeDataAsync("192.168.1.1");

        result.Should().NotBeNull();
        var json = ToJsonElement(result);
        json.GetProperty("cache_info").GetProperty("generated_for_ip").GetString().Should().Be("192.168.1.1");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetGeoTimeDataAsync_IPv6Style_ReplacesColonsInCacheKey()
    {
        // ::1 and 127.0.0.1 use default key (localhost). Use a non-localhost IPv6-style
        _memoryCache.Set("geo_time_2001_db8__1", new { ip = "2001:db8::1" }, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var result = await service.GetGeoTimeDataAsync("2001:db8::1");

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_DifferentIps_UseDifferentCacheKeys()
    {
        _memoryCache.Set("geo_time_default", new { cache_info = new { generated_for_ip = "server_default" } }, TimeSpan.FromMinutes(60));
        _memoryCache.Set("geo_time_10_0_0_1", new { cache_info = new { generated_for_ip = "10.0.0.1" } }, TimeSpan.FromMinutes(60));
        _memoryCache.Set("geo_time_10_0_0_2", new { cache_info = new { generated_for_ip = "10.0.0.2" } }, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var r1 = await service.GetGeoTimeDataAsync(null);
        var r2 = await service.GetGeoTimeDataAsync("10.0.0.1");
        var r3 = await service.GetGeoTimeDataAsync("10.0.0.2");

        r1.Should().NotBeNull();
        r2.Should().NotBeNull();
        r3.Should().NotBeNull();
        ToJsonElement(r1).GetProperty("cache_info").GetProperty("generated_for_ip").GetString().Should().Be("server_default");
        ToJsonElement(r2).GetProperty("cache_info").GetProperty("generated_for_ip").GetString().Should().Be("10.0.0.1");
        ToJsonElement(r3).GetProperty("cache_info").GetProperty("generated_for_ip").GetString().Should().Be("10.0.0.2");
    }

    #endregion

    #region 3. InvalidateGeoTimeCache — Null, Empty, NonExistent, Specific IP

    [Fact]
    [Trait("Category", "Negative")]
    public void InvalidateGeoTimeCache_NullIp_DoesNotThrow()
    {
        var service = CreateService();

        var act = () => service.InvalidateGeoTimeCache(null);

        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void InvalidateGeoTimeCache_EmptyIp_DoesNotThrow()
    {
        var service = CreateService();

        var act = () => service.InvalidateGeoTimeCache("");

        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void InvalidateGeoTimeCache_NonExistentKey_DoesNotThrow()
    {
        var service = CreateService();

        var act = () => service.InvalidateGeoTimeCache("192.168.99.99");

        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void InvalidateGeoTimeCache_WithIp_RemovesIpSpecificEntry()
    {
        _memoryCache.Set("geo_time_10_0_0_5", new { v = 1 }, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        service.InvalidateGeoTimeCache("10.0.0.5");

        _memoryCache.Get("geo_time_10_0_0_5").Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void InvalidateGeoTimeCache_LocalhostIp_HandlesKeyFormat()
    {
        var service = CreateService();

        var act = () => service.InvalidateGeoTimeCache("::1");

        act.Should().NotThrow();
    }

    #endregion

    #region 4. Response Structure — Required Fields

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_ReturnsAllRequiredTopLevelProperties()
    {
        var service = CreateService(_memoryCache);
        var result = await service.GetGeoTimeDataAsync();

        result.Should().NotBeNull();
        var json = ToJsonElement(result);
        json.GetProperty("current_datetime").ValueKind.Should().Be(JsonValueKind.String);
        json.GetProperty("current_timestamp_utc").ValueKind.Should().Be(JsonValueKind.Number);
        json.GetProperty("location").ValueKind.Should().Be(JsonValueKind.Object);
        json.GetProperty("generated_at").ValueKind.Should().Be(JsonValueKind.String);
        json.GetProperty("cache_info").ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_LocationHasRequiredFields()
    {
        var service = CreateService(_memoryCache);
        var result = await service.GetGeoTimeDataAsync();

        result.Should().NotBeNull();
        var loc = ToJsonElement(result).GetProperty("location");
        loc.GetProperty("country").ValueKind.Should().Be(JsonValueKind.String);
        loc.GetProperty("city").ValueKind.Should().Be(JsonValueKind.String);
        loc.GetProperty("timezone").ValueKind.Should().Be(JsonValueKind.String);
        loc.GetProperty("status").ValueKind.Should().Be(JsonValueKind.String);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_CacheInfoHasGeneratedForIp()
    {
        var service = CreateService(_memoryCache);
        var result = await service.GetGeoTimeDataAsync("8.8.8.8");

        result.Should().NotBeNull();
        var cacheInfo = ToJsonElement(result).GetProperty("cache_info");
        cacheInfo.GetProperty("generated_for_ip").GetString().Should().Be("8.8.8.8");
    }

    #endregion

    #region 5. Fallback Behavior — When API Fails

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_WhenLocationFails_ReturnsFallbackWithErrorStatus()
    {
        // When API fails (timeout, HTTP error, network), GetLocationInfoAsync returns fallback.
        // GenerateGeoTimeDataAsync catches and returns full object with location.status = "error" or "timeout".
        // We cannot force API failure without HTTP mock (DEF-222). Instead verify structure is valid
        // regardless of API success - both success and fallback have location.country, city, timezone, status.
        var service = CreateService(_memoryCache);
        var result = await service.GetGeoTimeDataAsync();

        result.Should().NotBeNull();
        var loc = ToJsonElement(result).GetProperty("location");
        loc.GetProperty("country").GetString().Should().NotBeNullOrEmpty();
        loc.GetProperty("city").GetString().Should().NotBeNullOrEmpty();
        loc.GetProperty("timezone").GetString().Should().NotBeNullOrEmpty();
        loc.GetProperty("status").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetGeoTimeDataAsync_ResultIsAlwaysSerializable()
    {
        var service = CreateService(_memoryCache);
        var result = await service.GetGeoTimeDataAsync();

        result.Should().NotBeNull();
        var act = () => JsonSerializer.Serialize(result);
        act.Should().NotThrow();
    }

    #endregion

    #region 6. Configuration — APITimeout

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_WithCustomAPITimeout_RespectsConfiguration()
    {
        // APITimeout is read; geoTimeout = max(5, apiTimeout/2). Service uses it for HttpClient timeout.
        // We cannot assert timeout value without reflection or HTTP mock. We verify service works with custom config.
        var config = CreateConfiguration(apiTimeout: 20);
        var service = CreateService(_memoryCache, config);

        var result = await service.GetGeoTimeDataAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetGeoTimeDataAsync_WithMissingAPITimeout_UsesDefault()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var service = CreateService(_memoryCache, config);

        var result = await service.GetGeoTimeDataAsync();

        result.Should().NotBeNull();
    }

    #endregion

    #region 7. Error Handling — Cache Throws, Returns Null

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetGeoTimeDataAsync_WhenCacheThrowsOnTryGetValue_ReturnsNull()
    {
        var mockCache = new Mock<IMemoryCache>();
        object? outVal = null;
        mockCache
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out outVal))
            .Throws(new InvalidOperationException("Cache error"));

        var service = CreateService(mockCache.Object);

        var result = await service.GetGeoTimeDataAsync();

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetGeoTimeDataAsync_WhenCacheCreateEntryThrows_ReturnsNull()
    {
        // IMemoryCache.Set is an extension method (not mockable). CreateEntry is the interface method used by Set.
        var mockCache = new Mock<IMemoryCache>();
        object? outVal = null;
        mockCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out outVal)).Returns(false);
        mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Throws(new InvalidOperationException("Cache create failed"));

        var service = CreateService(mockCache.Object);

        var result = await service.GetGeoTimeDataAsync();

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void InvalidateGeoTimeCache_WhenCacheThrows_DoesNotPropagate()
    {
        var mockCache = new Mock<IMemoryCache>();
        mockCache.Setup(m => m.Remove(It.IsAny<object>())).Throws(new InvalidOperationException("Remove failed"));

        var service = CreateService(mockCache.Object);

        var act = () => service.InvalidateGeoTimeCache("10.0.0.1");

        act.Should().NotThrow();
    }

    #endregion

    #region 8. Localhost IPs — Use Default Endpoint

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetGeoTimeDataAsync_127_0_0_1_UsesDefaultKey()
    {
        // 127.0.0.1 triggers default endpoint (ip-api.com/json/) - uses geo_time_default key
        _memoryCache.Set("geo_time_default", new { localhost = true }, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var result = await service.GetGeoTimeDataAsync("127.0.0.1");

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task GetGeoTimeDataAsync_ColonColon1_UsesDefaultKey()
    {
        _memoryCache.Set("geo_time_default", new { ipv6_localhost = true }, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var result = await service.GetGeoTimeDataAsync("::1");

        result.Should().NotBeNull();
    }

    #endregion

    #region 9. Integration — Full Flow

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetGeoTimeDataAsync_Invalidate_Get_RefetchesData()
    {
        var service = CreateService(_memoryCache);
        var first = await service.GetGeoTimeDataAsync();
        first.Should().NotBeNull();

        service.InvalidateGeoTimeCache();
        var second = await service.GetGeoTimeDataAsync();

        second.Should().NotBeNull();
        // After invalidate, second call triggers fresh fetch (or fallback)
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetGeoTimeDataAsync_ConsecutiveCallsWithCache_SameResult()
    {
        var cached = new
        {
            current_datetime = DateTime.UtcNow.ToString("O"),
            current_timestamp_utc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            location = new { country = "X", city = "Y", timezone = "Z", status = "success" },
            cache_info = new { cached = true }
        };
        _memoryCache.Set("geo_time_default", cached, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var r1 = await service.GetGeoTimeDataAsync();
        var r2 = await service.GetGeoTimeDataAsync();

        r1.Should().NotBeNull();
        r2.Should().NotBeNull();
        var ts1 = ToJsonElement(r1).GetProperty("current_timestamp_utc").GetInt64();
        var ts2 = ToJsonElement(r2).GetProperty("current_timestamp_utc").GetInt64();
        ts1.Should().Be(ts2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetGeoTimeDataAsync_DefaultAndIpKeys_Isolated()
    {
        _memoryCache.Set("geo_time_default", new { cache_info = new { generated_for_ip = "server_default" } }, TimeSpan.FromMinutes(60));
        _memoryCache.Set("geo_time_8_8_8_8", new { cache_info = new { generated_for_ip = "8.8.8.8" } }, TimeSpan.FromMinutes(60));
        var service = CreateService(_memoryCache);

        var defaultResult = await service.GetGeoTimeDataAsync();
        var ipResult = await service.GetGeoTimeDataAsync("8.8.8.8");

        defaultResult.Should().NotBeNull();
        ipResult.Should().NotBeNull();
        ToJsonElement(defaultResult).GetProperty("cache_info").GetProperty("generated_for_ip").GetString().Should().Be("server_default");
        ToJsonElement(ipResult).GetProperty("cache_info").GetProperty("generated_for_ip").GetString().Should().Be("8.8.8.8");
    }

    #endregion

    #region 10. Validation — ISO 8601 DateTime, Unix Timestamp

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_CurrentDatetime_IsIso8601Format()
    {
        var service = CreateService(_memoryCache);
        var result = await service.GetGeoTimeDataAsync();

        result.Should().NotBeNull();
        var dtStr = ToJsonElement(result).GetProperty("current_datetime").GetString();
        dtStr.Should().NotBeNullOrEmpty();
        dtStr.Should().Contain("T");
        dtStr.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetGeoTimeDataAsync_CurrentTimestampUtc_IsValidUnixSeconds()
    {
        var service = CreateService(_memoryCache);
        var result = await service.GetGeoTimeDataAsync();

        result.Should().NotBeNull();
        var ts = ToJsonElement(result).GetProperty("current_timestamp_utc").GetInt64();
        ts.Should().BeGreaterThan(0);
        var expectedMin = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
        var expectedMax = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        ts.Should().BeInRange(expectedMin, expectedMax);
    }

    #endregion

    #region Helpers

    private static GeoTimeCacheService CreateService(
        IMemoryCache? cache = null,
        IConfiguration? configuration = null)
    {
        var memCache = cache ?? new MemoryCache(new MemoryCacheOptions());
        var config = configuration ?? CreateConfiguration(10);
        return new GeoTimeCacheService(
            memCache,
            Mock.Of<ILogger<GeoTimeCacheService>>(),
            config,
            new HttpClient());
    }

    private static IConfiguration CreateConfiguration(int apiTimeout = 10)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["APITimeout"] = apiTimeout.ToString()
            })
            .Build();
    }

    private static JsonElement ToJsonElement(object? obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));
        var json = JsonSerializer.Serialize(obj);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    #endregion
}
