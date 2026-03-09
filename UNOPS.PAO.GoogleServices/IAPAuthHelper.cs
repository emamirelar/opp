/**
 * @fileoverview Helper class for Identity Aware Proxy (IAP) authentication with service account impersonation
 * @author UNOPS Opportunity+ System Development Team
 */

using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UNOPS.PAO.Models.Configuration;

namespace UNOPS.PAO.GoogleServices;

/// <summary>
/// Helper class for generating OIDC tokens for IAP authentication with service account impersonation
/// Python equivalent: auth_helpers.py
/// </summary>
public class IAPAuthHelper
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<IAPAuthHelper> _logger;
    private readonly GoogleCredential _credential;
    private readonly ExternalApiSettings _settings;

    public IAPAuthHelper(
        ILogger<IAPAuthHelper> logger,
        IOptions<ExternalApiSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _cache = new MemoryCache(new MemoryCacheOptions());

        _logger.LogInformation("IAPAuthHelper: Initializing with service account: {ServiceAccount}", 
            _settings.ServiceAccount);

        // Get application default credentials
        var defaultCredential = GoogleCredential.GetApplicationDefault();
        _logger.LogInformation("IAPAuthHelper: Retrieved default credential type: {CredentialType}",
            defaultCredential?.GetType()?.Name ?? "null");

        // Create scoped credential for cloud platform access
        _credential = (defaultCredential ?? throw new InvalidOperationException("Application default credentials are not available"))
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        _logger.LogInformation("IAPAuthHelper: Credential initialized successfully");
    }

    /// <summary>
    /// Get OIDC token for IAP authentication with service account impersonation
    /// Python equivalent: get_service_account_oidc_token()
    /// </summary>
    /// <param name="audience">The OAuth client ID (IAP audience)</param>
    /// <param name="useIdentityPlatform">Whether to exchange for GCIP token (true for IAP with Identity Platform)</param>
    /// <returns>OIDC token string</returns>
    public async Task<string> GetOidcTokenAsync(string audience, bool useIdentityPlatform = false)
    {
        var cacheKey = $"OIDC_TOKEN_{audience}_{useIdentityPlatform}";

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.LogInformation("🔐 Retrieved OIDC token from cache for audience: {Audience}", audience);
            return cachedToken;
        }

        try
        {
            _logger.LogInformation("🔐 Attempting to get OIDC token for audience: {Audience}", audience);
            _logger.LogInformation("🔐 Method 1: Trying impersonation of service account: {ServiceAccount}", 
                _settings.ServiceAccount);

            string idToken;

            try
            {
                // Step 1: Try to get impersonated credentials
                var targetScopes = new[]
                {
                    "openid",
                    "https://www.googleapis.com/auth/userinfo.email",
                    "https://www.googleapis.com/auth/userinfo.profile"
                };

                var impersonatedCred = GetImpersonatedCredential(
                    _settings.ServiceAccount,
                    targetScopes
                );

                _logger.LogInformation("✅ Successfully created impersonated credentials");

                // Step 2: Get ID token for the impersonated service account
                _logger.LogInformation("🔐 Requesting ID token for audience: {Audience}", audience);

                var oidcTokenOptions = OidcTokenOptions.FromTargetAudience(audience)
                    .WithTokenFormat(OidcTokenFormat.Standard);

                var oidcToken = await impersonatedCred.GetOidcTokenAsync(oidcTokenOptions);
                idToken = await oidcToken.GetAccessTokenAsync();

                _logger.LogInformation("✅ Successfully got ID token via impersonation");
            }
            catch (Exception impersonationEx)
            {
                _logger.LogWarning(impersonationEx, 
                    "⚠️ Impersonation failed: {Message}. Trying direct credential approach...", 
                    impersonationEx.Message);

                // Fallback: Try to get token directly from application default credentials
                _logger.LogInformation("🔐 Method 2: Trying direct OIDC token from application default credentials");

                var oidcTokenOptions = OidcTokenOptions.FromTargetAudience(audience)
                    .WithTokenFormat(OidcTokenFormat.Standard);

                var oidcToken = await _credential.GetOidcTokenAsync(oidcTokenOptions);
                idToken = await oidcToken.GetAccessTokenAsync();

                _logger.LogInformation("✅ Successfully got ID token directly (without impersonation)");
            }

            _logger.LogInformation("🔐 Retrieved ID token. Length: {Length}, Prefix: {Prefix}",
                idToken?.Length ?? 0,
                !string.IsNullOrEmpty(idToken) && idToken.Length > 20 ? idToken.Substring(0, 20) + "..." : idToken ?? "null");

            // Step 3: Exchange for GCIP token if needed (for IAP with Identity Platform)
            string finalToken = idToken ?? string.Empty;
            if (useIdentityPlatform && !string.IsNullOrEmpty(idToken))
            {
                _logger.LogInformation("🔐 Exchanging Google ID token for GCIP token");
                finalToken = await ExchangeForGcipTokenAsync(idToken);
            }

            // Cache the token (expires in 55 minutes, tokens are valid for 1 hour)
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(55)
            };
            _cache.Set(cacheKey, finalToken, cacheOptions);

            _logger.LogInformation("✅ Successfully generated and cached OIDC token");
            return finalToken ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting OIDC token for audience {Audience}: {Message}",
                audience, ex.Message);
            throw new InvalidOperationException($"Failed to get OIDC token for audience {audience}", ex);
        }
    }

    /// <summary>
    /// Get impersonated credentials for target service account
    /// Python equivalent: get_impersonated_credentials()
    /// </summary>
    private GoogleCredential GetImpersonatedCredential(
        string targetPrincipal,
        IEnumerable<string> scopes)
    {
        try
        {
            _logger.LogInformation("🔐 Creating impersonated credentials for: {Principal}", targetPrincipal);

            // Create impersonated credential with the target service account
            var impersonatedCred = _credential.Impersonate(new ImpersonatedCredential.Initializer(targetPrincipal)
            {
                DelegateAccounts = Array.Empty<string>(),
                Scopes = scopes,
                Lifetime = TimeSpan.FromHours(1) // Maximum allowed by GCP
            });

            _logger.LogInformation("✅ Impersonated credentials created successfully");
            return impersonatedCred;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating impersonated credentials for {Principal}: {Message}",
                targetPrincipal, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Exchange Google ID token for GCIP (Identity Platform) token
    /// Python equivalent: exchange_google_id_token_for_gcip_id_token()
    /// </summary>
    private async Task<string> ExchangeForGcipTokenAsync(string googleIdToken)
    {
        try
        {
            _logger.LogInformation("🔐 Exchanging Google ID token for GCIP token");

            // Get the Identity Toolkit API key from Secret Manager
            var apiKey = await GetIdentityToolkitApiKeyAsync();
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Identity Toolkit API key is empty or not configured");
            }

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key={apiKey}";

            using var httpClient = new HttpClient();
            var requestData = new
            {
                requestUri = "http://localhost",
                postBody = $"id_token={googleIdToken}&providerId=google.com",
                returnSecureToken = true,
                returnIdpCredential = true,
                tenantId = _settings.TenantId
            };

            _logger.LogInformation("🔐 Calling Identity Toolkit API with tenant: {TenantId}", _settings.TenantId);

            var response = await httpClient.PostAsJsonAsync(url, requestData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ Identity Toolkit API request failed: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
                throw new InvalidOperationException(
                    $"Identity Toolkit API request failed with status {response.StatusCode}: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Check for error in response
            if (result.TryGetProperty("error", out var error))
            {
                var errorMessage = error.GetProperty("message").GetString() ?? "Unknown error";
                throw new InvalidOperationException($"Identity Toolkit API error: {errorMessage}");
            }

            // Extract ID token
            if (!result.TryGetProperty("idToken", out var idTokenElement))
            {
                throw new InvalidOperationException("No idToken returned from Identity Toolkit API");
            }

            var gcipToken = idTokenElement.GetString();
            if (string.IsNullOrEmpty(gcipToken))
            {
                throw new InvalidOperationException("GCIP token is empty");
            }

            // Validate JWT format (should have 3 parts)
            var parts = gcipToken.Split('.');
            if (parts.Length != 3)
            {
                throw new InvalidOperationException(
                    $"Invalid JWT format: Expected 3 parts separated by '.' but got {parts.Length} parts");
            }

            _logger.LogInformation("✅ Successfully exchanged for GCIP token");
            return gcipToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error exchanging for GCIP token: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Get Identity Toolkit API key from Google Secret Manager
    /// </summary>
    private async Task<string> GetIdentityToolkitApiKeyAsync()
    {
        try
        {
            // Use Google Secret Manager to retrieve the API key
            var client = SecretManagerServiceClient.Create();
            // Extract project from service account email (e.g. pno-ai-service@unops-opportunityplus-dev.iam.gserviceaccount.com -> unops-opportunityplus-dev)
            var projectId = _credential.QuotaProject ?? ExtractProjectFromServiceAccount(_settings.ServiceAccount)
                ?? throw new InvalidOperationException("Cannot determine GCP project for Secret Manager. Set QuotaProject on credentials or use a service account email in format name@project.iam.gserviceaccount.com");
            var secretName = new SecretVersionName(projectId, 
                _settings.IdentityToolkitApiKeySecret, "latest");
            
            var response = await client.AccessSecretVersionAsync(secretName);
            var apiKey = response.Payload.Data.ToStringUtf8();
            
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("⚠️ Identity Toolkit API key is empty");
            }
            
            return apiKey ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving Identity Toolkit API key from Secret Manager");
            throw;
        }
    }

    /// <summary>
    /// Extract GCP project ID from service account email (e.g. name@project.iam.gserviceaccount.com -> project)
    /// </summary>
    private static string? ExtractProjectFromServiceAccount(string? serviceAccount)
    {
        if (string.IsNullOrEmpty(serviceAccount) || !serviceAccount.Contains('@'))
            return null;
        var atIndex = serviceAccount.IndexOf('@');
        var suffix = ".iam.gserviceaccount.com";
        var endIndex = serviceAccount.IndexOf(suffix, StringComparison.OrdinalIgnoreCase);
        if (endIndex < 0)
            return null;
        return serviceAccount.Substring(atIndex + 1, endIndex - atIndex - 1);
    }

    /// <summary>
    /// Clear cached token for a specific audience
    /// </summary>
    public void ClearTokenCache(string audience, bool useIdentityPlatform = false)
    {
        var cacheKey = $"OIDC_TOKEN_{audience}_{useIdentityPlatform}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("🗑️ Cleared token cache for audience: {Audience}", audience);
    }

    /// <summary>
    /// Clear all cached tokens
    /// </summary>
    public void ClearAllTokenCaches()
    {
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0); // Remove all entries
            _logger.LogInformation("🗑️ Cleared all cached tokens");
        }
    }
}

