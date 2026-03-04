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
    private readonly GoogleCredential _credential = null!;

    public CloudRunHelper(ILogger<CloudRunHelper> logger, GoogleCredential? credential = null)
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = logger;

        _logger.LogInformation("CloudRunHelper: Initializing with credential parameter: {HasCredential}", credential != null);

        var defaultCredential = GoogleCredential.GetApplicationDefault();
        _logger.LogInformation("CloudRunHelper: Retrieved default credential. Type: {CredentialType}",
            defaultCredential?.GetType()?.Name ?? "null");

        if (credential != null)
        {
            _logger.LogInformation("CloudRunHelper: Using provided credential. Type: {CredentialType}",
                credential.GetType().Name);
            _credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        }
        else
        {
            _logger.LogInformation("CloudRunHelper: Using default credential with cloud-platform scope");
            _credential = (defaultCredential ?? throw new InvalidOperationException("Application default credentials are not available"))
                .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        }
        
        _logger.LogInformation("CloudRunHelper: Final credential type: {CredentialType}, IsCreateScoped: {IsScoped}", 
            _credential?.GetType()?.Name ?? "null", 
            _credential != null ? "true" : "false");
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
            
            _logger?.LogInformation("CloudRunServiceClient: Initialized with baseUri: {BaseUri}, credential type: {CredentialType}", 
                baseUri, credential?.GetType()?.Name ?? "null");
        }

        // Create HttpClient with proper ID token authentication for Cloud Run service-to-service calls
        public async Task<HttpClient> CreateAuthenticatedHttpClient()
        {
            _logger?.LogInformation("CloudRunServiceClient: Creating authenticated HttpClient for baseUri: {BaseUri}", _baseUri);
            
            var httpClient = new HttpClient();
            
            // Set the base address to the service URL
            httpClient.BaseAddress = new Uri(_baseUri);
            _logger?.LogInformation("CloudRunServiceClient: Set HttpClient BaseAddress to: {BaseAddress}", httpClient.BaseAddress);
            
            // Get ID token with audience set to the service URL (required for Cloud Run auth)
            _logger?.LogInformation("CloudRunServiceClient: Requesting ID token for audience: {Audience}", _baseUri);
            var idToken = await GetIdTokenAsync(_baseUri);
            _logger?.LogInformation("CloudRunServiceClient: Received ID token. Length: {TokenLength}, First 20 chars: {TokenPrefix}", 
                idToken?.Length ?? 0, 
                !string.IsNullOrEmpty(idToken) && idToken.Length > 20 ? idToken.Substring(0, 20) + "..." : idToken ?? "null");
            
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", idToken);
            _logger?.LogInformation("CloudRunServiceClient: Set Authorization header with Bearer token");
            
            return httpClient;
        }

        private async Task<string> GetIdTokenAsync(string audience)
        {
            try
            {
                _logger?.LogInformation("GetIdTokenAsync: Starting ID token generation for audience: {Audience}", audience);
                
                if (_credential == null)
                {
                    _logger?.LogError("GetIdTokenAsync: Google credential is null");
                    throw new InvalidOperationException("Google credential is not configured");
                }
                
                _logger?.LogInformation("GetIdTokenAsync: Using credential type: {CredentialType}", _credential.GetType().Name);
                
                // Log credential details if possible
                try
                {
                    var underlyingCredential = _credential.UnderlyingCredential;
                    _logger?.LogInformation("GetIdTokenAsync: Underlying credential type: {UnderlyingType}", 
                        underlyingCredential?.GetType()?.Name ?? "null");
                }
                catch (Exception credEx)
                {
                    _logger?.LogWarning("GetIdTokenAsync: Could not access underlying credential: {Error}", credEx.Message);
                }
                
                // Get OIDC token with the target audience (the service URL)
                _logger?.LogInformation("GetIdTokenAsync: Creating OIDC token options for audience: {Audience}", audience);
                var oidcTokenOptions = OidcTokenOptions.FromTargetAudience(audience);
                _logger?.LogInformation("GetIdTokenAsync: OIDC token options created. Target audience: {TargetAudience}", 
                    oidcTokenOptions?.TargetAudience ?? "null");
                
                _logger?.LogInformation("GetIdTokenAsync: Calling GetOidcTokenAsync...");
                var oidcToken = await _credential!.GetOidcTokenAsync(oidcTokenOptions);
                _logger?.LogInformation("GetIdTokenAsync: OIDC token received. Type: {TokenType}", 
                    oidcToken?.GetType()?.Name ?? "null");
                
                // Note: Despite the confusing name, GetAccessTokenAsync() on an OidcToken 
                // actually returns the ID token string, not an access token
                _logger?.LogInformation("GetIdTokenAsync: Calling GetAccessTokenAsync on OIDC token...");
                var idToken = await (oidcToken ?? throw new InvalidOperationException("OIDC token is null")).GetAccessTokenAsync();
                
                _logger?.LogInformation("GetIdTokenAsync: ID token generated successfully. Length: {TokenLength}, Starts with: {TokenPrefix}", 
                    idToken?.Length ?? 0, 
                    !string.IsNullOrEmpty(idToken) && idToken.Length > 10 ? idToken.Substring(0, 10) + "..." : idToken ?? "null");
                
                return idToken!;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetIdTokenAsync: Failed to get ID token for audience {Audience}. Error: {ErrorMessage}", 
                    audience, ex.Message);
                _logger?.LogError("GetIdTokenAsync: Exception type: {ExceptionType}, Stack trace: {StackTrace}", 
                    ex.GetType().Name, ex.StackTrace);
                    
                if (ex.InnerException != null)
                {
                    _logger?.LogError("GetIdTokenAsync: Inner exception: {InnerExceptionType} - {InnerMessage}", 
                        ex.InnerException.GetType().Name, ex.InnerException.Message);
                }
                
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
            if (resolvedServiceUrl == null)
            {
                throw new InvalidOperationException($"Cloud Run service URL is null for {serviceName} in {location}");
            }

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
            var serviceClient = new CloudRunServiceClient(resolvedServiceUrl, _credential!, _logger);
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
            _logger.LogInformation("CloudRunHelper: CreateAuthenticatedHttpClientForUrl called with serviceUrl: {ServiceUrl}", serviceUrl);
            _logger.LogInformation("CloudRunHelper: Using credential type: {CredentialType}", _credential?.GetType()?.Name ?? "null");
            
            var serviceClient = new CloudRunServiceClient(serviceUrl, _credential!, _logger);
            _logger.LogInformation("CloudRunHelper: Created CloudRunServiceClient, calling CreateAuthenticatedHttpClient...");
            
            var httpClient = await serviceClient.CreateAuthenticatedHttpClient();
            _logger.LogInformation("CloudRunHelper: Successfully created authenticated HttpClient");
            
            return httpClient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CloudRunHelper: Failed to create authenticated HttpClient from URL: {ServiceUrl}. Error: {ErrorMessage}", serviceUrl, ex.Message);
            _logger.LogError("CloudRunHelper: Exception type: {ExceptionType}, Stack trace: {StackTrace}", ex.GetType().Name, ex.StackTrace);
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