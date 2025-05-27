using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.Identity.Entities;
using Microsoft.Extensions.Configuration;

namespace UNOPS.PAO.UNOPSIdentity.Authentication
{
    public class DevIdentityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DevIdentityMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public DevIdentityMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<DevIdentityMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Skip for non-development environments
                if (!_configuration.GetValue<bool>("Development:IAPSimulation:Enabled", true))
                {
                    await _next(context);
                    return;
                }

                // Get the development email from various sources
                string? devEmail = null;

                // 1. Check for dev-user query parameter
                if (context.Request.Query.TryGetValue("dev-user", out var queryEmail))
                {
                    devEmail = queryEmail.ToString();
                }

                // 2. Check for dev-user-email cookie
                if (string.IsNullOrEmpty(devEmail) && context.Request.Cookies.TryGetValue("dev-user-email", out var cookieEmail))
                {
                    devEmail = cookieEmail;
                }

                // 3. Use configured default email
                if (string.IsNullOrEmpty(devEmail))
                {
                    devEmail = _configuration["Development:IAPSimulation:UserEmail"] ?? "dev.user@example.com";
                }

                // Ensure we handle emails with account provider prefix
                if (devEmail.Contains(':'))
                {
                    devEmail = devEmail.Split(':').Last();
                }

                // Get required services
                var userManager = context.RequestServices.GetService<UserManager<PAOIdentityUser>>();
                var roleManager = context.RequestServices.GetService<RoleManager<PAOIdentityRole>>();

                if (userManager == null || roleManager == null)
                {
                    _logger.LogError("Required services (UserManager or RoleManager) not available");
                    await _next(context);
                    return;
                }

                // Create claims list
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, devEmail),
                    new Claim(ClaimTypes.Email, devEmail),
                    new Claim("iap-jwt-verified", "true"),
                    new Claim("IAPAuthenticated", "true"),
                    new Claim("IsInternal", devEmail.EndsWith("@unops.org").ToString()),
                    new Claim("hd", devEmail.Split('@')[1])
                };

                try
                {
                    // Ensure required roles exist
                    var requiredRoles = new[] { "UNOPS_GEN_USER", "PARTNER_GLOB_ADMIN", "PARTNER_USER", "ORG_UNIT_ADMIN" };
                    foreach (var roleName in requiredRoles)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            await roleManager.CreateAsync(new PAOIdentityRole { Name = roleName });
                        }
                    }

                    // Find or create user
                    var user = await userManager.FindByEmailAsync(devEmail);
                    if (user == null)
                    {
                        user = new PAOIdentityUser
                        {
                            UserName = devEmail,
                            Email = devEmail,
                            EmailConfirmed = true
                        };

                        var result = await userManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            _logger.LogError("Failed to create test user: {Errors}", string.Join(", ", result.Errors));
                            throw new InvalidOperationException("Failed to create test user");
                        }
                    }

                    // Add NameIdentifier claim
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.Id.ToString()));
                    await userManager.AddToRoleAsync(user, "UNOPS_GEN_USER");
                    claims.Add(new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting up dev user: {Email}", devEmail);
                    // Fallback to numeric ID if there's an error
                    var numericId = Math.Abs(devEmail.GetHashCode()).ToString();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, numericId));
                    claims.Add(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", numericId));
                    
                    // Add default roles
                    claims.Add(new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"));
                }

                // Create and set the identity
                var identity = new ClaimsIdentity(claims, "Development-IAP", ClaimTypes.Name, ClaimTypes.Role);
                context.User = new ClaimsPrincipal(identity);

                // Store the development authentication in a cookie with more permissive settings for development
                context.Response.Cookies.Append("dev-user-email", devEmail, new CookieOptions
                {
                    HttpOnly = false, // Allow JavaScript access in development
                    Secure = false, // Allow non-HTTPS in development
                    SameSite = SameSiteMode.None, // Allow cross-site requests in development
                    Expires = DateTimeOffset.Now.AddHours(8),
                    Path = "/" // Ensure cookie is available for all paths
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DevIdentityMiddleware");
                // Continue with the request even if there's an error
            }

            await _next(context);
        }
    }
} 