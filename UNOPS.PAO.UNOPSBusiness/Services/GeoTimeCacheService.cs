using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public interface IGeoTimeCacheService
{
    Task<object?> GetGeoTimeDataAsync(string? userIpAddress = null);
    void InvalidateGeoTimeCache(string? userIpAddress = null);
}

public class GeoTimeCacheService : IGeoTimeCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeoTimeCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    
    private const string CACHE_KEY_PREFIX = "geo_time_";
    private const string DEFAULT_CACHE_KEY = "geo_time_default";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(60); // Cache for 1 hour
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(30); // Refresh if accessed within 30 minutes

    public GeoTimeCacheService(
        IMemoryCache cache, 
        ILogger<GeoTimeCacheService> logger, 
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<object?> GetGeoTimeDataAsync(string? userIpAddress = null)
    {
        try
        {
            var cacheKey = GetCacheKey(userIpAddress);
            
            if (_cache.TryGetValue(cacheKey, out var cachedData))
            {
                _logger.LogDebug("Retrieved geo-time data from cache");
                return cachedData;
            }

            _logger.LogDebug("Geo-time data not in cache, generating new data");
            
            // Generate geo-time data
            var geoTimeData = await GenerateGeoTimeDataAsync(userIpAddress);
            
            // Cache the result
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration,
                SlidingExpiration = SlidingExpiration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, geoTimeData, cacheOptions);
            _logger.LogDebug("Cached geo-time data");
            
            return geoTimeData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting geo-time data");
            return null;
        }
    }

    public void InvalidateGeoTimeCache(string? userIpAddress = null)
    {
        try
        {
            var cacheKey = GetCacheKey(userIpAddress);
            _cache.Remove(cacheKey);
            _logger.LogDebug("Invalidated geo-time cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating geo-time cache");
        }
    }

    private string GetCacheKey(string? userIpAddress)
    {
        if (string.IsNullOrEmpty(userIpAddress))
        {
            return DEFAULT_CACHE_KEY;
        }
        
        return $"{CACHE_KEY_PREFIX}{userIpAddress.Replace(".", "_").Replace(":", "_")}";
    }

    private async Task<object> GenerateGeoTimeDataAsync(string? userIpAddress)
    {
        try
        {
            var now = DateTime.UtcNow;
            
            // Get location information
            var locationInfo = await GetLocationInfoAsync(userIpAddress);

            var geoTimeData = new
            {
                current_datetime = now.ToString("O"), // ISO 8601 format
                current_timestamp_utc = ((DateTimeOffset)now).ToUnixTimeSeconds(),
                location = locationInfo,
                generated_at = now,
                cache_info = new
                {
                    cached = false,
                    generated_for_ip = userIpAddress ?? "server_default"
                }
            };

            return geoTimeData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating geo-time data");
            
            var now = DateTime.UtcNow;
            return new
            {
                current_datetime = now.ToString("O"),
                current_timestamp_utc = ((DateTimeOffset)now).ToUnixTimeSeconds(),
                location = new
                {
                    country = "Unknown",
                    city = "Unknown",
                    timezone = "Unknown",
                    status = "error",
                    error = ex.Message
                },
                generated_at = now,
                cache_info = new
                {
                    cached = false,
                    error = true
                }
            };
        }
    }

    private async Task<object> GetLocationInfoAsync(string? userIpAddress)
    {
        try
        {
            // Get API timeout from configuration, default to 10 seconds
            var apiTimeout = _configuration.GetValue<int>("APITimeout", 10);
            var geoTimeout = Math.Max(5, apiTimeout / 2); // Use half of API timeout, minimum 5 seconds

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(geoTimeout);

            string apiUrl;
            if (!string.IsNullOrEmpty(userIpAddress) && userIpAddress != "::1" && userIpAddress != "127.0.0.1")
            {
                // Use specific IP if provided and it's not localhost
                apiUrl = $"http://ip-api.com/json/{userIpAddress}";
            }
            else
            {
                // Use default endpoint to get server's public IP location
                apiUrl = "http://ip-api.com/json/";
            }

            _logger.LogDebug("Fetching location data from: {ApiUrl}", apiUrl);

            var response = await httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                
                if (data != null)
                {
                    return new
                    {
                        country = data.country?.ToString() ?? "Unknown",
                        country_code = data.countryCode?.ToString() ?? "",
                        region = data.regionName?.ToString() ?? "Unknown",
                        city = data.city?.ToString() ?? "Unknown",
                        timezone = data.timezone?.ToString() ?? "Unknown",
                        latitude = Convert.ToDouble(data.lat ?? 0),
                        longitude = Convert.ToDouble(data.lon ?? 0),
                        isp = data.isp?.ToString() ?? "Unknown",
                        status = data.status?.ToString() ?? "unknown",
                        query_ip = data.query?.ToString() ?? userIpAddress
                    };
                }
            }
            else
            {
                _logger.LogWarning("Failed to get location (HTTP {StatusCode})", response.StatusCode);
                return new
                {
                    country = "Unknown",
                    city = "Unknown", 
                    timezone = "Unknown",
                    status = "error",
                    error = $"HTTP {response.StatusCode}"
                };
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("Location API request timed out");
            return new
            {
                country = "Unknown",
                city = "Unknown",
                timezone = "Unknown", 
                status = "timeout",
                error = "Request timed out"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting location info");
            return new
            {
                country = "Unknown",
                city = "Unknown",
                timezone = "Unknown",
                status = "error", 
                error = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting location info");
            return new
            {
                country = "Unknown",
                city = "Unknown", 
                timezone = "Unknown",
                status = "error",
                error = ex.Message
            };
        }

        return new
        {
            country = "Unknown",
            city = "Unknown",
            timezone = "Unknown",
            status = "error",
            error = "Failed to retrieve location data"
        };
    }
}
