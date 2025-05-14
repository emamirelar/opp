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

namespace UNOPS.PAO.UNOPSIdentity.Authentication
{
    public class DevIdentityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DevIdentityMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public DevIdentityMiddleware(
            RequestDelegate next,
            ILogger<DevIdentityMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply in development and for API calls
            if (_environment.IsDevelopment() && context.Request.Path.StartsWithSegments("/api"))
            {
                // Check if we have IAP headers - now we apply this to all requests, not just unauthenticated ones
                if (context.Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var emailHeader))
                {
                    // Even if the user is already authenticated, ensure we have proper IAP identity
                    // This prevents cases where the user is authenticated but with wrong scheme
                    string email = emailHeader.ToString().Split(':').Last();
                    
                    _logger.LogInformation("Setting dev identity for API call with email: {Email}, path: {Path}, currentAuth: {IsAuthenticated}", 
                        email, 
                        context.Request.Path, 
                        context.User?.Identity?.IsAuthenticated);
                    
                    // Create a scope to ensure proper DbContext lifetime
                    using (var scope = context.RequestServices.CreateScope())
                    {
                        // Get a new UserManager with its own DbContext instance
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PAOIdentityUser>>();
                        
                        // Find or create the user
                        var user = await userManager.FindByEmailAsync(email);
                        if (user == null)
                        {
                            _logger.LogWarning("User not found in database: {Email}, creating development user", email);
                            
                            // For development, always create the user if not found
                            user = new PAOIdentityUser
                            {
                                Id = GenerateUniqueIntId(),
                                UserName = email,
                                Email = email,
                                EmailConfirmed = true,
                                IsInternal = email.EndsWith("@unops.org"),
                                GoogleSignIn = true
                            };
                            
                            var result = await userManager.CreateAsync(user);
                            if (!result.Succeeded)
                            {
                                _logger.LogError("Failed to create development user: {Errors}", 
                                    string.Join(", ", result.Errors.Select(e => e.Description)));
                            }
                            else
                            {
                                _logger.LogInformation("Successfully created development user: {Email}", email);
                                
                                // Assign roles based on email
                                if (email.EndsWith("@unops.org"))
                                {
                                    await EnsureRoleExists(scope, "Internal");
                                    await userManager.AddToRoleAsync(user, "Internal");
                                    
                                    if (email.ToLower().Contains("admin"))
                                    {
                                        await EnsureRoleExists(scope, "Administrator");
                                        await userManager.AddToRoleAsync(user, "Administrator");
                                    }
                                }
                                else
                                {
                                    await EnsureRoleExists(scope, "Partner");
                                    await userManager.AddToRoleAsync(user, "Partner");
                                }
                                
                                // Always add basic user role
                                await EnsureRoleExists(scope, "User");
                                await userManager.AddToRoleAsync(user, "User");
                            }
                        }
                        
                        if (user != null)
                        {
                            // Get all user claims and roles
                            var userClaims = await userManager.GetClaimsAsync(user);
                            var roles = await userManager.GetRolesAsync(user);
                            
                            // Log what we found
                            _logger.LogInformation("User {Email} has {RoleCount} roles: {Roles}", 
                                email, roles.Count, string.Join(", ", roles));
                                
                            // Validate roles based on email domain - fix if necessary
                            if (email.EndsWith("@unops.org") && !roles.Contains("Internal"))
                            {
                                _logger.LogWarning("UNOPS user {Email} missing Internal role, adding it", email);
                                await EnsureRoleExists(scope, "Internal");
                                await userManager.AddToRoleAsync(user, "Internal");
                                roles = await userManager.GetRolesAsync(user);
                            }
                            
                            if (email.ToLower().Contains("admin") && !roles.Contains("Administrator"))
                            {
                                _logger.LogWarning("Admin user {Email} missing Administrator role, adding it", email);
                                await EnsureRoleExists(scope, "Administrator");
                                await userManager.AddToRoleAsync(user, "Administrator");
                                roles = await userManager.GetRolesAsync(user);
                            }
                            
                            if (!email.EndsWith("@unops.org") && !roles.Contains("Partner"))
                            {
                                _logger.LogWarning("External user {Email} missing Partner role, adding it", email);
                                await EnsureRoleExists(scope, "Partner");
                                await userManager.AddToRoleAsync(user, "Partner");
                                roles = await userManager.GetRolesAsync(user);
                            }
                            
                            // Build a comprehensive claims list
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new Claim(ClaimTypes.Name, user.UserName),
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim("IsInternal", user.IsInternal.ToString()),
                                new Claim("IAPAuthenticated", "true")
                            };
                            
                            // Add existing user claims 
                            foreach (var claim in userClaims)
                            {
                                if (!claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                                {
                                    claims.Add(claim);
                                }
                            }
                            
                            // Add roles
                            foreach (var role in roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                            
                            // Create the identity with "IAP" authentication type to match the IAP authentication handler
                            var identity = new ClaimsIdentity(claims, "IAP", ClaimTypes.Name, ClaimTypes.Role);
                            
                            // Always set the user
                            context.User = new ClaimsPrincipal(identity);
                            
                            _logger.LogInformation("Dev identity set for user: {Email} with {RoleCount} roles, IsAuthenticated: {IsAuthenticated}", 
                                email, roles.Count, identity.IsAuthenticated);
                                
                            // Log all the roles and claims for debugging
                            _logger.LogInformation("User roles: {Roles}", string.Join(", ", roles));
                            _logger.LogInformation("User has {ClaimCount} claims", claims.Count);
                        }
                    }
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
        
        private async Task EnsureRoleExists(IServiceScope scope, string roleName)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<PAOIdentityRole>>();
            
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogInformation("Creating missing role: {Role}", roleName);
                var role = new PAOIdentityRole { Name = roleName };
                await roleManager.CreateAsync(role);
            }
        }

        private int GenerateUniqueIntId()
        {
            // Generate a more unique ID based on current timestamp
            // Using timestamp ensures increasing values and minimizes collision risk
            // Take the last 9 digits to fit within int range
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var uniqueId = (int)(timestamp % 1_000_000_000); 
            
            // Add small random number to further reduce collision possibility
            uniqueId = uniqueId * 10 + new Random().Next(0, 9);
            
            // Ensure positive value within int range
            return Math.Abs(uniqueId % int.MaxValue);
        }
    }
} 