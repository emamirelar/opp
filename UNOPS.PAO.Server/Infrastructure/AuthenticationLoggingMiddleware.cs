using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace UNOPS.PAO.Server.Infrastructure
{
    public class AuthenticationLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationLoggingMiddleware> _logger;

        public AuthenticationLoggingMiddleware(RequestDelegate next, ILogger<AuthenticationLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                _logger.LogInformation("Before authentication: IsAuthenticated={IsAuthenticated}, User={UserName}, Path={Path}",
                    context.User?.Identity?.IsAuthenticated,
                    context.User?.Identity?.Name,
                    context.Request.Path);

                // Log IAP headers
                if (context.Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var emailHeader))
                {
                    _logger.LogInformation("IAP Email Header: {Header}", emailHeader!);
                    
                    // Extract and log the actual email part
                    var emailParts = emailHeader.ToString().Split(':', 2);
                    if (emailParts.Length == 2)
                    {
                        _logger.LogInformation("IAP Email (extracted): {Email}", emailParts[1]);
                    }
                }
                else
                {
                    _logger.LogWarning("IAP Email Header: Missing");
                }

                if (context.Request.Headers.TryGetValue("X-Dev-IAP-Simulation", out var devHeader))
                {
                    _logger.LogInformation("Dev IAP Simulation: {Header}", devHeader);
                }
                
                // Log IAP JWT header presence (not the full token for security)
                if (context.Request.Headers.TryGetValue("X-Goog-IAP-JWT-Assertion", out var jwtHeader))
                {
                    _logger.LogInformation("IAP JWT Header: {Present}, Length: {Length}", 
                        "Present", jwtHeader.ToString().Length);
                }
                else
                {
                    _logger.LogWarning("IAP JWT Header: Missing");
                }
                
                // Print all headers for troubleshooting
                var headerLog = new StringBuilder("AuthLoggingMiddleware - All request headers:\n");
                foreach (var header in context.Request.Headers)
                {
                    var headerValue = header.Key.Contains("JWT", StringComparison.OrdinalIgnoreCase) ? 
                        $"[REDACTED - Length: {header.Value.ToString().Length}]" : 
                        header.Value.ToString();
                    
                    headerLog.AppendLine($"  {header.Key}: {headerValue}");
                }
                _logger.LogInformation(headerLog.ToString());
            }

            // Call the next middleware in the pipeline
            await _next(context);

            // Log after authentication
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                _logger.LogInformation("After authentication: IsAuthenticated={IsAuthenticated}, User={UserName}, Path={Path}",
                    context.User?.Identity?.IsAuthenticated,
                    context.User?.Identity?.Name,
                    context.Request.Path);
            }
        }
    }
} 