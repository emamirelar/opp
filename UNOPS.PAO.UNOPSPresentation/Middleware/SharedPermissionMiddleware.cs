using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Authorization;

namespace UNOPS.PAO.UNOPSPresentation.Middleware
{
    /// <summary>
    /// Middleware that enforces permissions from our shared JSON configuration
    /// </summary>
    public class SharedPermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SharedPermissionMiddleware> _logger;

        // Define paths that should bypass permission checks
        private static readonly string[] BypassPaths = new[] 
        {
            "/dev-login",
            "/api/dev",
            "/api/debug",
            "/api/permissions",
            "/api/configuration",
            "/api/user",
            "/api/secureresource",
            "/api/secureresource/authtest",
            "/user/login",
            "/user/logout",
            "/user/register",
            "/user/claims",
            "/health",
            "/swagger",
            "/api/values",
            "/api/process-data",
            "/api/links",
            "/api/ai-assistant",
            "/api/document",
            "/api/current-user-data"
        };

        public SharedPermissionMiddleware(RequestDelegate next, ILogger<SharedPermissionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, SharedPermissionService permissionService)
        {
            string path = context.Request.Path.Value ?? string.Empty;
            
            // Skip permission checks for certain paths
            foreach (var bypassPath in BypassPaths)
            {
                if (path.StartsWith(bypassPath, System.StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }
            }
            
            // Skip options requests and non-API endpoints
            if (context.Request.Method == "OPTIONS" || !path.StartsWith("/api"))
            {
                await _next(context);
                return;
            }

            // Skip if user is not authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Check if the endpoint is authorized
            if (!permissionService.IsAuthorizedEndpoint())
            {
                _logger.LogWarning("User {User} attempted to access unauthorized endpoint {Path} with method {Method}",
                    context.User.Identity?.Name,
                    context.Request.Path,
                    context.Request.Method);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { 
                    message = "You do not have permission to access this resource",
                    path = context.Request.Path.Value,
                    method = context.Request.Method,
                    user = context.User.Identity?.Name ?? "Unknown" 
                });
                return;
            }

            await _next(context);
        }
    }
} 