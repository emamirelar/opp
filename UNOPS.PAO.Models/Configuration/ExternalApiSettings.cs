/**
 * @fileoverview Configuration settings for external AI retriever API authentication and endpoints
 * @author UNOPS Opportunity+ System Development Team
 */

namespace UNOPS.PAO.Models.Configuration;

/// <summary>
/// Configuration settings for external AI retriever API calls
/// Used for IAP authentication and API communication
/// </summary>
public class ExternalApiSettings
{
    /// <summary>
    /// Base URL for AI retriever API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.ai.unops.org";

    /// <summary>
    /// OAuth 2.0 Client ID for IAP authentication (audience)
    /// </summary>
    public string OAuthClientId { get; set; } = string.Empty;

    /// <summary>
    /// Service account email to impersonate for API calls
    /// </summary>
    public string ServiceAccount { get; set; } = string.Empty;

    /// <summary>
    /// Identity Platform tenant ID for GCIP token exchange
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Secret name for Identity Toolkit API key
    /// </summary>
    public string IdentityToolkitApiKeySecret { get; set; } = string.Empty;

    /// <summary>
    /// HTTP request timeout in seconds
    /// </summary>
    public int Timeout { get; set; } = 60;

    /// <summary>
    /// Skip authentication in development mode (for local testing)
    /// </summary>
    public bool SkipAuthenticationInDevelopment { get; set; } = false;
}

