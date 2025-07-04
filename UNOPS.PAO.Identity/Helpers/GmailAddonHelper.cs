using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;

namespace UNOPS.PAO.Identity.Helpers;

public class GmailAddonHelper
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GmailAddonHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string GetValidAudienceForCurrentHost()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new ApplicationException("HttpContext is not available. This method can only be called in the context of an HTTP request.");
        }

        string hostUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        string normalizedHostUrl = hostUrl.TrimEnd('/');

        return normalizedHostUrl;
    }
}
