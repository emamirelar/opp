using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace UNOPS.PAO.GoogleServices;

public class CloudRunHelper
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CloudRunHelper> _logger;
    private readonly GoogleCredential _credential;

    public CloudRunHelper(ILogger<CloudRunHelper> logger, GoogleCredential? credential = null)
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = logger;
        var defaultCredential = GoogleCredential.GetApplicationDefault();
        _credential = credential ?? defaultCredential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    }

    // Custom Cloud Run service client that inherits from BaseClientService
    public class CloudRunServiceClient : BaseClientService
    {
        private readonly string _baseUri;
        private readonly GoogleCredential? _credential;
        private readonly ILogger<CloudRunHelper>? _logger;

        public CloudRunServiceClient(BaseClientService.Initializer initializer) : base(initializer) {
          _baseUri = initializer.BaseUri;
        }

        public override string Name => "CloudRunService";
        public override string BaseUri => _baseUri;
        public override string BasePath => "";
        public override IList<string> Features => new string[0];

        public CloudRunServiceClient(string baseUri, GoogleCredential credential, ILogger<CloudRunHelper> logger) : base(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "UNOPS-PAO-CloudRun-Client"
        })
        {
            _baseUri = baseUri;
            _credential = credential;
            _logger = logger;
        }

        // Create HttpClient with proper ID token authentication for Cloud Run service-to-service calls
        public async Task<HttpClient> CreateAuthenticatedHttpClient()
        {
            var httpClient = new HttpClient();
            
            // Set the base address to the service URL
            httpClient.BaseAddress = new Uri(_baseUri);
            
            // Get ID token with audience set to the service URL (required for Cloud Run auth)
            var idToken = await GetIdTokenAsync(_baseUri);
            
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", idToken);
            
            return httpClient;
        }

        private async Task<string> GetIdTokenAsync(string audience)
        {
            try
            {
                if (_credential == null)
                {
                    throw new InvalidOperationException("Google credential is not configured");
                }
                
                // Get OIDC token with the target audience (the service URL)
                var oidcToken = await _credential.GetOidcTokenAsync(OidcTokenOptions.FromTargetAudience(audience));
                
                // Note: Despite the confusing name, GetAccessTokenAsync() on an OidcToken 
                // actually returns the ID token string, not an access token
                var idToken = await oidcToken.GetAccessTokenAsync();
                
                _logger?.LogInformation("Generated ID token for audience {Audience}: {IdToken}", audience, idToken);
                
                return idToken;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get ID token for audience {audience}", ex);
            }
        }
    }

    // Get Cloud Run service URL using Cloud Run REST API with caching
    public async Task<string> GetCloudRunServiceUrl(string projectId, string location, string serviceName, string? serviceUrl = null)
    {
        // If serviceUrl is provided, return it directly
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            _logger.LogInformation("Using provided service URL: {ServiceUrl}", serviceUrl);
            return serviceUrl;
        }

        var cacheKey = $"{projectId}:{location}:{serviceName}";
        
        // Check memory cache first
        if (_cache.TryGetValue(cacheKey, out string? cachedUrl) && !string.IsNullOrEmpty(cachedUrl))
        {
            _logger.LogInformation("Retrieved Cloud Run service URL from cache for {ServiceName}", serviceName);
            return cachedUrl;
        }

        try
        {
            using var httpClient = new HttpClient();
            
            // Get access token for Cloud Run API (this is different from service-to-service calls)
            var accessToken = await _credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var apiUrl = $"https://run.googleapis.com/v2/projects/{projectId}/locations/{location}/services/{serviceName}";
            var response = await httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Cloud Run API request failed: {response.StatusCode} - {response.ReasonPhrase}");
            }
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            var serviceData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
            
            if (!serviceData.TryGetProperty("status", out var status) || 
                !status.TryGetProperty("uri", out var uriElement))
            {
                throw new InvalidOperationException($"Cloud Run service URL not found for {serviceName} in {location}");
            }

            var resolvedServiceUrl = uriElement.GetString();
            
            if (string.IsNullOrEmpty(resolvedServiceUrl))
            {
                throw new InvalidOperationException($"Cloud Run service URL is empty for {serviceName} in {location}");
            }
            
            // Cache the URL in both caches
            _cache.Set(cacheKey, resolvedServiceUrl, TimeSpan.FromHours(24));
            
            _logger.LogInformation("Retrieved and cached Cloud Run service URL for {ServiceName}: {ServiceUrl}", serviceName, resolvedServiceUrl);
            return resolvedServiceUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Cloud Run service URL for {ServiceName}: {ErrorMessage}", serviceName, ex.Message);
            throw new InvalidOperationException($"Failed to get Cloud Run service URL for {serviceName}", ex);
        }
    }

    // Create an authenticated HttpClient for Cloud Run service communication
    public async Task<HttpClient> CreateAuthenticatedHttpClient(string projectId, string location, string serviceName)
    {
        try
        {
            var resolvedServiceUrl = await GetCloudRunServiceUrl(projectId, location, serviceName);
            var serviceClient = new CloudRunServiceClient(resolvedServiceUrl, _credential, _logger);
            return await serviceClient.CreateAuthenticatedHttpClient();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create authenticated HttpClient: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException("Failed to create authenticated HttpClient", ex);
        }
    }

    // Alternative method that uses the full Cloud Run service URL directly
    public async Task<HttpClient> CreateAuthenticatedHttpClientForUrl(string serviceUrl)
    {
        try
        {
            var serviceClient = new CloudRunServiceClient(serviceUrl, _credential, _logger);
            return await serviceClient.CreateAuthenticatedHttpClient();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create authenticated HttpClient from URL: {ErrorMessage}", ex.Message);
            throw new InvalidOperationException("Failed to create authenticated HttpClient", ex);
        }
    }

    // Clear cache for a specific service (useful for testing or when service is updated)
    public void ClearServiceCache(string projectId, string location, string serviceName)
    {
        var cacheKey = $"{projectId}:{location}:{serviceName}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Cleared cache for Cloud Run service {ServiceName}", serviceName);
    }
} 