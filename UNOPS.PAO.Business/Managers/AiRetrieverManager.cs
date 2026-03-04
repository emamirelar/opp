/**
 * @fileoverview AI Retriever Manager for calling external AI retriever API endpoints with IAP authentication
 * @author UNOPS Opportunity+ System Development Team
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Configuration;

namespace UNOPS.PAO.Business.Managers;

/// <summary>
/// Manager for calling external AI retriever API endpoints
/// Provides shared authentication logic for all 100+ endpoints
/// Python equivalent: Combines tool functions with build_request_headers()
/// </summary>
public class AiRetrieverManager : IAiRetrieverManager
{
    private readonly IAPAuthHelper _iapAuthHelper;
    private readonly ExternalApiSettings _settings;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiRetrieverManager> _logger;
    private readonly HttpClient _httpClient;

    #region Endpoint Constants

    // Vector Store endpoints
    private const string VECTOR_STORE_SEARCH = "/v1/tools/vector-store/search";

    // Document conversion endpoints
    private const string CONVERT_URL = "/v1/convert/url";
    private const string CONVERT_MARKDOWN_TO_GOOGLE_DOC = "/v1/convert/markdown-to-google-doc";

    // Google Drive endpoints
    private const string GOOGLE_DRIVE_UPLOAD = "/v1/google-drive/upload";
    private const string GOOGLE_DRIVE_DOWNLOAD = "/v1/google-drive/download";

    // Add more endpoint constants as needed (100+ endpoints)
    // private const string ANOTHER_ENDPOINT = "/v1/another/endpoint";

    #endregion

    public AiRetrieverManager(
        IAPAuthHelper iapAuthHelper,
        IOptions<ExternalApiSettings> settings,
        IConfiguration configuration,
        ILogger<AiRetrieverManager> logger)
    {
        _iapAuthHelper = iapAuthHelper;
        _settings = settings.Value;
        _configuration = configuration;
        _logger = logger;

        // Create HTTP client with base address and timeout
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_settings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(_settings.Timeout)
        };

        _logger.LogInformation("AiRetrieverManager initialized with base URL: {BaseUrl}", _settings.BaseUrl);
    }

    #region Public Endpoint Methods

    /// <summary>
    /// Search corporate vector store
    /// Python equivalent: search_corp_vector_store_tool.py
    /// </summary>
    public async Task<VectorStoreSearchResponse> SearchVectorStoreAsync(
        VectorStoreSearchRequest request,
        string? userEmail = null)
    {
        _logger.LogInformation("🔍 Searching vector store with query: {Query}", request.Query);

        return await PostAsync<VectorStoreSearchRequest, VectorStoreSearchResponse>(
            VECTOR_STORE_SEARCH,
            request,
            userEmail
        );
    }

    /// <summary>
    /// Convert URL to document
    /// </summary>
    public async Task<ConvertedDocument> ConvertUrlAsync(
        string url,
        string? userEmail = null)
    {
        _logger.LogInformation("🔗 Converting URL to document: {Url}", url);

        var request = new { url };
        return await PostAsync<object, ConvertedDocument>(
            CONVERT_URL,
            request,
            userEmail
        );
    }

    /// <summary>
    /// Convert markdown to Google Doc.
    /// API expects multipart/form-data with "file" (markdown content) and "data" (JSON metadata).
    /// Handles both JSON response (with pdfBase64) and raw PDF response (when API returns application/pdf).
    /// </summary>
    public async Task<GoogleDocResponse> ConvertMarkdownToGoogleDocAsync(
        string markdown,
        string? userEmail = null,
        string? fileName = null)
    {
        _logger.LogInformation("📄 Converting markdown to Google Doc (length: {Length})", markdown?.Length ?? 0);

        var headers = await BuildAuthenticatedHeadersAsync(CONVERT_MARKDOWN_TO_GOOGLE_DOC, userEmail, additionalHeaders: null);

        using var content = new MultipartFormDataContent();
        var fileBytes = Encoding.UTF8.GetBytes(markdown);
        var fileContentPart = new ByteArrayContent(fileBytes);
        fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("text/markdown");
        content.Add(fileContentPart, "file", fileName ?? "document.md");

        var dataJson = JsonSerializer.Serialize(new
        {
            name = Path.GetFileNameWithoutExtension(fileName ?? "document.md"),
            downloadPDF = true
        });
        content.Add(new StringContent(dataJson, Encoding.UTF8, "application/json"), "data");

        var request = new HttpRequestMessage(HttpMethod.Post, CONVERT_MARKDOWN_TO_GOOGLE_DOC);
        foreach (var header in headers)
        {
            if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                continue;
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

        _logger.LogInformation("📥 Response status: {StatusCode}, Content-Type: {ContentType}", response.StatusCode, contentType);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("❌ API call failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            throw new HttpRequestException($"API call failed: {response.StatusCode} - {errorContent}");
        }

        // Read response as bytes first - needed to handle both binary PDF and JSON
        var responseBytes = await response.Content.ReadAsByteArrayAsync();

        // Handle raw PDF: Content-Type application/pdf or body starts with %PDF (0x25 0x50 0x44 0x46)
        var isPdf = contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase)
            || (responseBytes.Length >= 4 && responseBytes[0] == 0x25 && responseBytes[1] == 0x50 && responseBytes[2] == 0x44 && responseBytes[3] == 0x46);

        if (isPdf)
        {
            _logger.LogInformation("✅ Received raw PDF response ({Length} bytes)", responseBytes.Length);
            return new GoogleDocResponse
            {
                Status = "success",
                PdfBase64 = Convert.ToBase64String(responseBytes)
            };
        }

        var responseContent = Encoding.UTF8.GetString(responseBytes);

        // Validate JSON before deserializing (avoids JsonException when API returns HTML/error page)
        var trimmed = responseContent.TrimStart();
        if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
        {
            _logger.LogError("❌ Unexpected response format. First 200 chars: {Preview}", responseContent.Length > 200 ? responseContent[..200] : responseContent);
            throw new InvalidOperationException($"API returned non-JSON response. Content-Type: {contentType}. Response starts with: {(responseContent.Length > 50 ? responseContent[..50] + "..." : responseContent)}");
        }

        try
        {
            var result = JsonSerializer.Deserialize<GoogleDocResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                throw new InvalidOperationException("Failed to deserialize response");

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "❌ JSON deserialization failed. First 200 chars: {Preview}", responseContent.Length > 200 ? responseContent[..200] : responseContent);
            throw;
        }
    }

    // Add more public methods for other endpoints as needed...
    // Each method is a simple one-liner calling PostAsync or GetAsync

    #endregion

    #region Private Shared Methods

    /// <summary>
    /// Generic POST method - all endpoint methods use this
    /// Python equivalent: requests.post() with build_request_headers()
    /// </summary>
    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest payload,
        string? userEmail = null,
        Dictionary<string, string>? additionalHeaders = null)
    {
        try
        {
            _logger.LogInformation("📤 Making POST request to {Endpoint}", endpoint);

            // Build authenticated headers (Python: build_request_headers)
            var headers = await BuildAuthenticatedHeadersAsync(endpoint, userEmail, additionalHeaders);

            // Create request
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            // Add headers
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Add JSON body
            var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("📋 Request payload: {Payload}", jsonContent);

            // Make request
            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 Response status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("📄 Raw response content (first 500 chars): {Content}", 
                responseContent.Length > 500 ? responseContent.Substring(0, 500) + "..." : responseContent);

            // Handle response
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("📊 Full response content length: {Length} bytes", responseContent.Length);
                
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    _logger.LogError("❌ Deserialization returned null for response: {Response}", responseContent);
                    throw new InvalidOperationException("Failed to deserialize response");
                }

                _logger.LogInformation("✅ Successfully completed POST request to {Endpoint}", endpoint);
                return result;
            }
            else
            {
                _logger.LogError("❌ API call failed: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);

                throw new HttpRequestException(
                    $"API call failed: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error calling endpoint {Endpoint}: {Message}", endpoint, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// POST multipart/form-data with file and data fields (API expects "file" and "data").
    /// </summary>
    private async Task<TResponse> PostMultipartFormDataAsync<TResponse>(
        string endpoint,
        string fileContent,
        string fileName,
        string? userEmail = null)
    {
        try
        {
            _logger.LogInformation("📤 Making POST multipart request to {Endpoint}", endpoint);

            var headers = await BuildAuthenticatedHeadersAsync(endpoint, userEmail, additionalHeaders: null);

            using var content = new MultipartFormDataContent();
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);
            var fileContentPart = new ByteArrayContent(fileBytes);
            fileContentPart.Headers.ContentType = new MediaTypeHeaderValue("text/markdown");
            content.Add(fileContentPart, "file", fileName);

            var dataJson = JsonSerializer.Serialize(new
            {
                name = Path.GetFileNameWithoutExtension(fileName),
                downloadPDF = true
            });
            content.Add(new StringContent(dataJson, Encoding.UTF8, "application/json"), "data");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            foreach (var header in headers)
            {
                if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                    continue;
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 Response status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                    throw new InvalidOperationException("Failed to deserialize response");

                return result;
            }

            _logger.LogError("❌ API call failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
            throw new HttpRequestException($"API call failed: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error calling endpoint {Endpoint}: {Message}", endpoint, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Generic GET method
    /// </summary>
    private async Task<TResponse> GetAsync<TResponse>(
        string endpoint,
        string? userEmail = null,
        Dictionary<string, string>? additionalHeaders = null)
    {
        try
        {
            _logger.LogInformation("📤 Making GET request to {Endpoint}", endpoint);

            var headers = await BuildAuthenticatedHeadersAsync(endpoint, userEmail, additionalHeaders);

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 Response status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    throw new InvalidOperationException("Failed to deserialize response");
                }

                _logger.LogInformation("✅ Successfully completed GET request to {Endpoint}", endpoint);
                return result;
            }
            else
            {
                _logger.LogError("❌ API call failed: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);

                throw new HttpRequestException(
                    $"API call failed: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error calling endpoint {Endpoint}: {Message}", endpoint, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Build authenticated request headers
    /// Python equivalent: build_request_headers()
    /// </summary>
    private async Task<Dictionary<string, string>> BuildAuthenticatedHeadersAsync(
        string endpoint,
        string? userEmail = null,
        Dictionary<string, string>? additionalHeaders = null)
    {
        _logger.LogInformation("=======================START: BUILD REQUEST HEADERS======================================");

        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json",
            ["Accept"] = "application/json"
        };

        // Add additional headers
        if (additionalHeaders != null)
        {
            foreach (var header in additionalHeaders)
            {
                headers[header.Key] = header.Value;
            }
        }

        // Check if we're in development mode
        var isDevelopment = _configuration.GetValue<bool>("Development:IAPSimulation:Enabled");
        var devEmail = _configuration.GetValue<string>("Development:IAPSimulation:UserEmail");

        _logger.LogInformation("🔍 Environment: IsDevelopment={IsDevelopment}, DevEmail={DevEmail}",
            isDevelopment, devEmail ?? "null");

        // Development IAP simulation headers (Python equivalent)
        if (isDevelopment && !string.IsNullOrEmpty(devEmail))
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            headers["x-goog-authenticated-user-email"] = $"accounts.google.com:{devEmail}";
            headers["x-goog-authenticated-user-id"] = $"accounts.google.com:dev-user-id-{timestamp}";
            headers["x-forwarded-user"] = devEmail;
            headers["x-forwarded-email"] = devEmail;
            headers["X-Dev-IAP-Simulation"] = "true";
            headers["X-Dev-Auth-Timestamp"] = timestamp;

            _logger.LogInformation("✅ [AUTH-HEADERS] Added development IAP headers for email: {Email}", devEmail);
        }

        // Determine if endpoint needs Identity Platform token exchange
        bool isGoogleApi = IsGoogleApiEndpoint(endpoint);
        bool useIdentityPlatform = !isGoogleApi; // Use IDP for IAP, not for Google APIs

        _logger.LogInformation("🔍 [AUTH-HEADERS] Endpoint: {Endpoint}, IsGoogleApi: {IsGoogleApi}, UseIDP: {UseIDP}",
            endpoint, isGoogleApi, useIdentityPlatform);

        // Get OIDC token (ALWAYS - required for IAP authentication)
        try
        {
            _logger.LogInformation("🔐 [AUTH-HEADERS] Requesting OIDC token with audience: {Audience}",
                _settings.OAuthClientId);

            string oidcToken = await _iapAuthHelper.GetOidcTokenAsync(
                _settings.OAuthClientId,
                useIdentityPlatform
            );

            if (!string.IsNullOrEmpty(oidcToken))
            {
                headers["Authorization"] = $"Bearer {oidcToken}";
                _logger.LogInformation("✅ [AUTH-HEADERS] Added Authorization header. Token length: {Length}",
                    oidcToken.Length);

                // Log token details for debugging (first 20 chars)
                _logger.LogInformation("🔍 [AUTH-HEADERS-TOKEN] Token prefix: {Prefix}...",
                    oidcToken.Length > 20 ? oidcToken.Substring(0, 20) : oidcToken);
            }
            else
            {
                _logger.LogWarning("❌ [AUTH-HEADERS] Failed to get OIDC token - token is empty or null!");
                _logger.LogWarning("⚠️ [AUTH-HEADERS] This will likely cause the API to return 0 results due to authorization failure");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [AUTH-HEADERS] Error getting OIDC token: {Message}", ex.Message);
            _logger.LogError("⚠️ [AUTH-HEADERS] Request will proceed without Authorization header - API call will likely fail");
            // Continue without auth token - let the API return an error
        }

        // Add impersonation header (Python: x-unops-impersonated-user)
        var effectiveUserEmail = userEmail ?? (isDevelopment ? devEmail : null);
        if (!string.IsNullOrEmpty(effectiveUserEmail))
        {
            // Extract email if it contains a colon (e.g., "securetoken.google.com/project/uid:email@domain.com")
            // Split by colon and take the last part if it looks like an email
            var emailParts = effectiveUserEmail.Split(':');
            if (emailParts.Length > 1)
            {
                var lastPart = emailParts[emailParts.Length - 1].Trim();
                // Check if the last part contains @ (basic email validation)
                if (lastPart.Contains("@"))
                {
                    effectiveUserEmail = lastPart;
                    _logger.LogInformation("🔍 [AUTH-HEADERS] Extracted email from claim: {Email}", effectiveUserEmail);
                }
            }
            
            headers["x-unops-impersonated-user"] = effectiveUserEmail;
            _logger.LogInformation("✅ [AUTH-HEADERS] Added impersonation header for: {Email}", effectiveUserEmail);
        }
        else
        {
            _logger.LogInformation("⚠️ [AUTH-HEADERS] No user email available for impersonation header");
        }

        // Log final headers
        _logger.LogInformation("🔐 [AUTH-HEADERS] Final request headers prepared:");
        _logger.LogInformation("📋 Total headers: {Count}", headers.Count);
        _logger.LogInformation("📋 Header keys: {Keys}", string.Join(", ", headers.Keys));

        // Log headers safely (mask sensitive ones)
        foreach (var header in headers)
        {
            if (header.Key.Contains("authorization", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Contains("token", StringComparison.OrdinalIgnoreCase))
            {
                var maskedValue = header.Value.Length > 10 ? header.Value.Substring(0, 10) + "..." : "***";
                _logger.LogInformation("   {Key}: {Value} (masked)", header.Key, maskedValue);
            }
            else if (header.Key.Contains("email", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("   {Key}: {Value}", header.Key, header.Value);
            }
            else if (header.Value.Length > 100)
            {
                _logger.LogInformation("   {Key}: {Value}... (truncated)", header.Key, header.Value.Substring(0, 50));
            }
            else
            {
                _logger.LogInformation("   {Key}: {Value}", header.Key, header.Value);
            }
        }

        _logger.LogInformation("=======================END: BUILD REQUEST HEADERS======================================");

        return headers;
    }

    /// <summary>
    /// Determine if endpoint is a Google API (no IDP token exchange needed)
    /// Python equivalent: is_google_api check in build_request_headers
    /// </summary>
    private bool IsGoogleApiEndpoint(string endpoint)
    {
        var googleApiPaths = new[]
        {
            "/google-drive/",
            "/vector-store/",
            "/convert/url",
            "/convert/markdown-to-google-doc"
        };

        return googleApiPaths.Any(path => endpoint.Contains(path, StringComparison.OrdinalIgnoreCase));
    }

    #endregion
}

