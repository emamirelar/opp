using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                    _logger.LogInformation("IAP Email Header: {Header}", emailHeader);
                }

                if (context.Request.Headers.TryGetValue("X-Dev-IAP-Simulation", out var devHeader))
                {
                    _logger.LogInformation("Dev IAP Simulation: {Header}", devHeader);
                }
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