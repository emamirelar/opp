using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Services;

/// <summary>
/// Service for generating URLs for entities and getting current host information
/// </summary>
public class UrlService : IUrlService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UrlService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _fallbackBaseUrl;

    public UrlService(IConfiguration configuration, ILogger<UrlService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _fallbackBaseUrl = configuration["AppConfig:BaseUrl"] ?? "https://test-opportunityplus.unops.org";
    }

    /// <summary>
    /// Gets the current host URL from the HTTP context, with fallback to configuration
    /// </summary>
    public string GetCurrentHostUrl()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request != null)
            {
                var request = httpContext.Request;
                var scheme = request.Scheme; // http or https
                var host = request.Host.Value; // domain.com:port
                var hostUrl = $"{scheme}://{host}";
                var normalizedHostUrl = hostUrl.TrimEnd('/');
                
                _logger.LogDebug("Generated host URL from request context: {HostUrl}", normalizedHostUrl);
                return normalizedHostUrl;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get host URL from HTTP context, using fallback configuration");
        }

        var normalizedFallbackUrl = _fallbackBaseUrl.TrimEnd('/');
        _logger.LogDebug("Using fallback host URL from configuration: {HostUrl}", normalizedFallbackUrl);
        return normalizedFallbackUrl;
    }

    /// <summary>
    /// Build URL to a specific entity page
    /// </summary>
    public string BuildEntityUrl(string entityType, int entityId)
    {
        var entityPath = entityType.ToLower() switch
        {
            "partner" => $"/partnerships/partners/{entityId}",
            "contact" => $"/partnerships/contacts/{entityId}",
            "interaction" => $"/partnerships/interaction/{entityId}",
            "opportunity" => $"/partnerships/opportunities/{entityId}",
            _ => $"/partnerships/{entityType.ToLower()}s/{entityId}" // Generic fallback
        };
        
        return BuildUrl(entityPath);
    }

    /// <summary>
    /// Builds a URL to a specific page/route
    /// </summary>
    private string BuildUrl(string relativePath)
    {
        var hostUrl = GetCurrentHostUrl();
        var trimmedPath = relativePath.TrimStart('/');
        return $"{hostUrl}/{trimmedPath}";
    }
}
