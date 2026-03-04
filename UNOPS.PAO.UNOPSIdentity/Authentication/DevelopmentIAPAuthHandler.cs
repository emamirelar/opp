using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

namespace UNOPS.PAO.UNOPSIdentity.Authentication;

public class DevelopmentIAPAuthHandler : IMiddleware
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DevelopmentIAPAuthHandler> _logger;
    private string _lastProcessedEmail = string.Empty;

    public DevelopmentIAPAuthHandler(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<DevelopmentIAPAuthHandler> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Only apply in Development environment
        if (_environment.IsDevelopment())
        {
            _logger.LogInformation("DevelopmentIAPAuthHandler running for path: {Path}", context.Request.Path);
            
            // For ALL API calls, make sure to set IAP headers if we have a dev cookie
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                _logger.LogInformation("Processing API request: {Path}", context.Request.Path);
                
                // If we have a cookie, set headers for ALL API calls
                if (context.Request.Cookies.TryGetValue("dev-user-email", out var apiCookieEmail) || 
                    context.Request.Cookies.TryGetValue("DevIAPAuth", out apiCookieEmail))
                {
                    // Check if user has changed (previously processed a different email)
                    bool isUserChanged = !string.IsNullOrEmpty(_lastProcessedEmail) && 
                                        !string.Equals(_lastProcessedEmail, apiCookieEmail, StringComparison.OrdinalIgnoreCase);
                    
                    if (isUserChanged)
                    {
                        _logger.LogWarning("User change detected! From {OldEmail} to {NewEmail}", 
                            _lastProcessedEmail, apiCookieEmail);
                    }
                    
                    // Remember the current user
                    _lastProcessedEmail = apiCookieEmail;
                    
                    _logger.LogInformation("Setting IAP headers for API call: {Path} with dev user: {Email}", 
                        context.Request.Path, apiCookieEmail);
                    
                    // Clear any existing headers to avoid conflicts
                    context.Request.Headers.Remove("X-Goog-Authenticated-User-Email");
                    context.Request.Headers.Remove("X-Goog-Iap-Jwt-Assertion");
                    context.Request.Headers.Remove("X-Dev-IAP-Simulation");
                    context.Request.Headers.Remove("X-Dev-Auth-Timestamp");
                    
                    // Add the headers with new values
                    context.Request.Headers.Append("X-Goog-Authenticated-User-Email", $"accounts.google.com:{apiCookieEmail}");
                    context.Request.Headers.Append("X-Goog-Iap-Jwt-Assertion", "dev-jwt-placeholder");
                    context.Request.Headers.Append("X-Dev-IAP-Simulation", "true");
                    
                    // Add a timestamp to prevent caching of authentication
                    context.Request.Headers.Append("X-Dev-Auth-Timestamp", DateTime.UtcNow.Ticks.ToString());
                }
                else
                {
                    _logger.LogWarning("No dev cookie found for API call: {Path}", context.Request.Path);
                    _lastProcessedEmail = string.Empty;
                }
                
                await next(context);
                return;
            }
            
            // Skip processing for static resources
            if (context.Request.Path.Value?.EndsWith(".js") == true ||
                context.Request.Path.Value?.EndsWith(".css") == true ||
                context.Request.Path.Value?.EndsWith(".png") == true ||
                context.Request.Path.Value?.EndsWith(".jpg") == true ||
                context.Request.Path.Value?.EndsWith(".jpeg") == true ||
                context.Request.Path.Value?.EndsWith(".gif") == true ||
                context.Request.Path.Value?.EndsWith(".svg") == true ||
                context.Request.Path.Value?.EndsWith(".ico") == true ||
                context.Request.Path.Value?.EndsWith(".webmanifest") == true ||
                context.Request.Path.Value?.EndsWith(".woff") == true ||
                context.Request.Path.Value?.EndsWith(".woff2") == true ||
                context.Request.Path.Value?.EndsWith(".ttf") == true ||
                context.Request.Path.Value?.EndsWith(".eot") == true ||
                context.Request.Path.Value?.EndsWith(".map") == true ||
                context.Request.Path.Value?.StartsWith("/assets/") == true ||
                context.Request.Path.Value?.StartsWith("/favicon") == true)
            {
                await next(context);
                return;
            }
            
            // Always check if we have a dev cookie first
            if (context.Request.Cookies.TryGetValue("dev-user-email", out var cookieEmail) ||
                context.Request.Cookies.TryGetValue("DevIAPAuth", out cookieEmail))
            {
                _logger.LogInformation("Found dev cookie: {Email}", cookieEmail);
                
                // Check if user has changed
                bool isUserChanged = !string.IsNullOrEmpty(_lastProcessedEmail) && 
                                    !string.Equals(_lastProcessedEmail, cookieEmail, StringComparison.OrdinalIgnoreCase);
                
                if (isUserChanged)
                {
                    _logger.LogWarning("User change detected! From {OldEmail} to {NewEmail}", 
                        _lastProcessedEmail, cookieEmail);
                }
                
                // Update the last processed email
                _lastProcessedEmail = cookieEmail;
                
                // Always set the IAP headers when we have a dev cookie
                string devEmail = cookieEmail;
                
                // Ensure we also have a DevIAPAuth cookie
                if (!context.Request.Cookies.ContainsKey("DevIAPAuth"))
                {
                    _logger.LogInformation("Setting DevIAPAuth cookie for user: {Email}", devEmail);
                    context.Response.Cookies.Append("DevIAPAuth", devEmail, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = context.Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                        Expires = DateTimeOffset.Now.AddDays(7)
                    });
                }
                
                // Clear any existing headers first
                context.Request.Headers.Remove("X-Goog-Authenticated-User-Email");
                context.Request.Headers.Remove("X-Goog-Iap-Jwt-Assertion");
                context.Request.Headers.Remove("X-Dev-IAP-Simulation");
                context.Request.Headers.Remove("X-Dev-Auth-Timestamp");
                
                // Directly set the IAP headers that the IAPAuthenticationHandler expects
                context.Request.Headers["X-Goog-Authenticated-User-Email"] = $"accounts.google.com:{devEmail}";
                context.Request.Headers["X-Goog-Iap-Jwt-Assertion"] = "dev-jwt-placeholder";
                context.Request.Headers["X-Dev-IAP-Simulation"] = "true";
                
                // Add a timestamp to prevent caching of authentication
                context.Request.Headers["X-Dev-Auth-Timestamp"] = DateTime.UtcNow.Ticks.ToString();
                
                _logger.LogInformation("Development IAP headers set for user: {Email}", devEmail);
                
                // Handle login page redirects
                if (context.Request.Path.Value?.EndsWith("/login") == true && 
                    !context.Request.Path.Value.Contains("/dev-login"))
                {
                    // In development, always redirect to home when on login page and have dev cookie
                    _logger.LogInformation("Redirecting from login page to home page for dev user: {Email}", devEmail);
                    context.Response.Redirect("/");
                    return;
                }
            }
            else
            {
                _logger.LogWarning("No dev cookies (dev-user-email or DevIAPAuth) found in request");
                
                // Reset last processed email if we no longer have cookies
                if (!string.IsNullOrEmpty(_lastProcessedEmail))
                {
                    _logger.LogWarning("Clearing last processed user email: {Email}", _lastProcessedEmail);
                    _lastProcessedEmail = string.Empty;
                }
                
                // If no dev cookie and trying to access a page that needs auth, 
                // redirect to dev login instead of regular login
                if (context.Request.Path.Value?.EndsWith("/login") == false && 
                    !context.Request.Path.Value?.Contains("/dev-login") == true &&
                    !context.Request.Path.Value?.StartsWith("/api/dev/") == true &&
                    !context.Request.Path.Value?.StartsWith("/api/user/") == true &&
                    !context.Request.Path.Value?.StartsWith("/assets/") == true)
                {
                    // Try to check if this is an authenticated request first
                    if (context.User?.Identity?.IsAuthenticated != true)
                    {
                        _logger.LogInformation("Redirecting to dev-login for unauthenticated request to: {Path}", 
                            context.Request.Path);
                        context.Response.Redirect("/dev-login");
                        return;
                    }
                }
            }
        }
        
        await next(context);
    }

    private string GetDevelopmentUserEmail(HttpContext context)
    {
        // Option 1: First check for a cookie for persistent development identity
        if (context.Request.Cookies.TryGetValue("dev-user-email", out var cookieEmail))
        {
            _logger.LogInformation("Using dev-user-email cookie for development user: {Email}", cookieEmail);
            return cookieEmail;
        }
        
        // Check for DevIAPAuth cookie as another option
        if (context.Request.Cookies.TryGetValue("DevIAPAuth", out var iapAuthCookie))
        {
            _logger.LogInformation("Using DevIAPAuth cookie for development user: {Email}", iapAuthCookie);
            return iapAuthCookie;
        }
        
        // Option 2: Check for a query parameter for testing different users
        if (context.Request.Query.TryGetValue("dev-user", out var queryEmail))
        {
            var email = queryEmail.ToString();
            _logger.LogInformation("Using query parameter development user email: {Email}", email ?? "");
            return email ?? "";
        }
        
        // Option 3: Fall back to configured value
        var configuredEmail = _configuration["Development:IAPSimulation:UserEmail"];
        if (!string.IsNullOrEmpty(configuredEmail))
        {
            _logger.LogInformation("Using configured development user email: {Email}", configuredEmail);
            return configuredEmail;
        }
        
        // Absolute fallback to default dev user
        _logger.LogInformation("Using default development user email: dev.user@unops.org");
        return "dev.user@unops.org";
    }
} 