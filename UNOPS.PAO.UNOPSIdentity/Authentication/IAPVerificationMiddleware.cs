using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Identity.Entities;
using System.Security.Cryptography;
using System.IO;

namespace UNOPS.PAO.UNOPSIdentity.Authentication
{
    public class IAPVerificationMiddleware
    {
        private static readonly string PUBLIC_KEY_URL = "https://www.gstatic.com/iap/verify/public_key-jwk";
        private static readonly string IAP_ISSUER = "https://cloud.google.com/iap";
        private static readonly Dictionary<string, JsonWebKey> _cachedKeys = new Dictionary<string, JsonWebKey>();
        private static DateTime _keysLastRefreshed = DateTime.MinValue;
        private static readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IAPVerificationMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;

        public IAPVerificationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<IAPVerificationMiddleware> logger,
            IWebHostEnvironment environment,
            IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log all headers at the beginning for troubleshooting
            var headerLog = new StringBuilder("IAPVerificationMiddleware - All Request Headers:\n");
            
            // Check for any IAP-related headers with different casing
            bool foundIapEmailHeader = false;
            bool foundIapJwtHeader = false;
            
            foreach (var header in context.Request.Headers)
            {
                var headerValue = header.Key.Contains("jwt", StringComparison.OrdinalIgnoreCase) ? 
                    $"[REDACTED - Length: {header.Value.ToString().Length}]" : 
                    header.Value.ToString();
                
                headerLog.AppendLine($"  {header.Key}: {headerValue}");
                
                // Check for IAP headers with different casing
                if (header.Key.Contains("authenticated-user-email", StringComparison.OrdinalIgnoreCase))
                {
                    foundIapEmailHeader = true;
                }
                
                if (header.Key.Contains("iap-jwt", StringComparison.OrdinalIgnoreCase))
                {
                    foundIapJwtHeader = true;
                }
            }
            
            if (!foundIapEmailHeader)
            {
                _logger.LogWarning("No IAP email header found in any case variation");
            }
            
            if (!foundIapJwtHeader)
            {
                _logger.LogWarning("No IAP JWT header found in any case variation");
            }
            
            _logger.LogInformation(headerLog.ToString());
            
            // Health check path exception
            string healthCheckPath = _configuration["IAP:HealthCheckPath"] ?? "/health";
            if (context.Request.Path.StartsWithSegments(healthCheckPath))
            {
                await _next(context);
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
                await _next(context);
                return;
            }
            
            // Skip verification in development if configured
            if (_environment.IsDevelopment() && _configuration.GetValue<bool>("IAP:SkipValidationInDevelopment", _configuration.GetValue<bool>("Development:IAPSimulation:SkipValidationInDevelopment", false)))
            {
                string devEmail = GetDevelopmentUserEmail(context);
                if (!string.IsNullOrEmpty(devEmail))
                {
                    await SetupDevUserPrincipal(context, devEmail);
                    await _next(context);
                    return;
                }
            }

            // Primary Authentication: JWT Verification
            bool jwtVerified = false;
            ClaimsPrincipal? jwtPrincipal = null;
            string? verifiedEmail = null;

            if (context.Request.Headers.TryGetValue("x-goog-iap-jwt-assertion", out var jwtHeaderValues))
            {
                var jwt = jwtHeaderValues.ToString();
                _logger.LogInformation("Found JWT header with length: {Length}", jwt.Length);
                try
                {
                    jwtPrincipal = await VerifyIapJwtAndGetPrincipalAsync(jwt, context);
                    if (jwtPrincipal != null)
                    {
                        jwtVerified = true;
                        verifiedEmail = jwtPrincipal.FindFirstValue(ClaimTypes.Email);
                        if (verifiedEmail != null && verifiedEmail.Contains(":")) {
                            verifiedEmail = verifiedEmail.Split(':').Last();
                        }
                        _logger.LogDebug("Successfully verified JWT for user: {Email}", verifiedEmail);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "JWT verification failed, falling back to header-based authentication");
                }
            }
            else
            {
                _logger.LogWarning("No JWT header found, checking for email header");
            }

            // Secondary Authentication: Email Header Check (fallback or verification)
            if (context.Request.Headers.TryGetValue("x-goog-authenticated-user-email", out var emailHeaderValues))
            {
                var emailHeader = emailHeaderValues.ToString();
                _logger.LogInformation("Found IAP email header: {Header}", emailHeader);
                string extractedEmail = ExtractEmailFromHeader(emailHeader);

                if (string.IsNullOrEmpty(extractedEmail))
                {
                    _logger.LogWarning("Invalid email format in x-goog-authenticated-user-email header");
                }
                else if (jwtVerified)
                {
                    // If JWT was verified, verify that the emails match
                    if (!string.Equals(verifiedEmail, extractedEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Email mismatch between JWT ({JwtEmail}) and header ({HeaderEmail})", 
                            verifiedEmail, extractedEmail);
                        
                        // In production, this would be suspicious and might indicate tampering
                        if (!_environment.IsDevelopment())
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Unauthorized: Identity mismatch");
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Email in JWT and header match: {Email}", extractedEmail);
                    }
                }
                else if (!_configuration.GetValue<bool>("IAP:RequireJwtVerification", true) || _environment.IsDevelopment())
                {
                    // Only use email header if JWT verification isn't required or in development
                    _logger.LogDebug("Using email from header: {Email}", extractedEmail);
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, extractedEmail),
                        new Claim(ClaimTypes.Email, extractedEmail),
                        new Claim("iap-header-verified", "true")
                    };

                    // Check for user ID header
                    if (context.Request.Headers.TryGetValue("x-goog-authenticated-user-id", out var userIdHeaderValues))
                    {
                        var userIdHeader = userIdHeaderValues.ToString();
                        _logger.LogInformation("IAPVerificationMiddleware - Found user ID header: {Header}", userIdHeader);
                        
                        // Get the user manager
                        var userManager = context.RequestServices.GetService<UserManager<PAOIdentityUser>>();
                        var roleManager = context.RequestServices.GetService<RoleManager<PAOIdentityRole>>();
                        if (userManager != null && roleManager != null)
                        {
                            try
                            {
                                // Extract email from the header
                                extractedEmail = context.Request.Headers["x-goog-authenticated-user-email"].ToString().Split(':').Last();
                                _logger.LogInformation("IAPVerificationMiddleware - Looking up user by email: {Email}", extractedEmail);

                                // Check if the extracted value is a numeric ID instead of an email
                                PAOIdentityUser? user = null;
                                if (long.TryParse(extractedEmail, out _))
                                {
                                    // This is a numeric user ID, not an email
                                    _logger.LogInformation("IAPVerificationMiddleware - Detected numeric user ID: {UserId}", extractedEmail);
                                    
                                    // For numeric IDs, we need to create a user with a placeholder email
                                    // since we don't have the actual email from the header
                                    var placeholderEmail = $"user{extractedEmail}@iap.google.com";
                                    user = await userManager.FindByEmailAsync(placeholderEmail);
                                    
                                    if (user == null)
                                    {
                                        // Create new user with placeholder email
                                        _logger.LogInformation("IAPVerificationMiddleware - Creating new user for numeric ID: {UserId}", extractedEmail);
                                        
                                        var newUser = new PAOIdentityUser
                                        {
                                            UserName = placeholderEmail,
                                            Email = placeholderEmail,
                                            EmailConfirmed = true // Since this comes from IAP, we trust it
                                        };

                                        var createResult = await userManager.CreateAsync(newUser);
                                        if (createResult.Succeeded)
                                        {
                                            user = newUser;
                                            _logger.LogInformation("IAPVerificationMiddleware - Successfully created new user for numeric ID: {UserId} with email: {Email} and database ID: {Id}", 
                                                extractedEmail, placeholderEmail, newUser.Id);
                                        }
                                        else
                                        {
                                            _logger.LogError("IAPVerificationMiddleware - Failed to create user for numeric ID: {UserId}. Errors: {Errors}", 
                                                extractedEmail, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogInformation("IAPVerificationMiddleware - Found existing user for numeric ID: {UserId} with database ID: {Id}", 
                                            extractedEmail, user.Id);
                                    }
                                }
                                else
                                {
                                    // This is an email address
                                    user = await userManager.FindByEmailAsync(extractedEmail);
                                }

                                if (user != null)
                                {
                                    // Use the database ID as NameIdentifier
                                    claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                                    _logger.LogInformation("IAPVerificationMiddleware - Added database ID as NameIdentifier: {Id}", user.Id);

                                    // Check if user has any roles
                                    var roles = await userManager.GetRolesAsync(user);
                                    _logger.LogInformation("IAPVerificationMiddleware - User {Email} has {RoleCount} roles: {Roles}", extractedEmail, roles.Count, string.Join(", ", roles));
                                    
                                    if (!roles.Any())
                                    {
                                        // Ensure UNOPS_GEN_USER role exists
                                        if (!await roleManager.RoleExistsAsync("UNOPS_GEN_USER"))
                                        {
                                            await roleManager.CreateAsync(new PAOIdentityRole { Name = "UNOPS_GEN_USER" });
                                            _logger.LogInformation("IAPVerificationMiddleware - Created UNOPS_GEN_USER role");
                                        }

                                        // Add UNOPS_GEN_USER role
                                        await userManager.AddToRoleAsync(user, "UNOPS_GEN_USER");
                                        claims.Add(new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"));
                                        _logger.LogInformation("IAPVerificationMiddleware - Added UNOPS_GEN_USER role to user");
                                    }
                                    else
                                    {
                                        // Add existing roles as claims
                                        foreach (var role in roles)
                                        {
                                            claims.Add(new Claim(ClaimTypes.Role, role));
                                            _logger.LogInformation("IAPVerificationMiddleware - Added existing role claim: {Role}", role);
                                        }
                                    }
                                }
                                else if (!long.TryParse(extractedEmail, out _))
                                {
                                    // Only try to create a user if it's an email (not a numeric ID that failed to create)
                                    // User not found - create new user
                                    _logger.LogInformation("IAPVerificationMiddleware - User not found in database for email: {Email}. Creating new user.", extractedEmail);
                                    
                                    var newUser = new PAOIdentityUser
                                    {
                                        UserName = extractedEmail,
                                        Email = extractedEmail,
                                        EmailConfirmed = true // Since this comes from IAP, we trust the email is verified
                                    };

                                    var createResult = await userManager.CreateAsync(newUser);
                                    if (createResult.Succeeded)
                                    {
                                        _logger.LogInformation("IAPVerificationMiddleware - Successfully created new user: {Email} with ID: {Id}", extractedEmail, newUser.Id);
                                        
                                        // Use the new database ID as NameIdentifier
                                        claims.Add(new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()));
                                        
                                        // Ensure UNOPS_GEN_USER role exists
                                        if (!await roleManager.RoleExistsAsync("UNOPS_GEN_USER"))
                                        {
                                            await roleManager.CreateAsync(new PAOIdentityRole { Name = "UNOPS_GEN_USER" });
                                            _logger.LogInformation("IAPVerificationMiddleware - Created UNOPS_GEN_USER role");
                                        }

                                        // Add UNOPS_GEN_USER role to new user
                                        await userManager.AddToRoleAsync(newUser, "UNOPS_GEN_USER");
                                        claims.Add(new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"));
                                        _logger.LogInformation("IAPVerificationMiddleware - Added UNOPS_GEN_USER role to new user");
                                    }
                                    else
                                    {
                                        _logger.LogError("IAPVerificationMiddleware - Failed to create user for email: {Email}. Errors: {Errors}", 
                                            extractedEmail, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                                        // Continue without database ID - will use the original IAP ID
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "IAPVerificationMiddleware - Error processing user ID header");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("IAPVerificationMiddleware - No user ID header found");
                    }

                    var identity = new ClaimsIdentity(claims, "IAP", ClaimTypes.Name, ClaimTypes.Role);
                    context.User = new ClaimsPrincipal(identity);
                    
                    // DEBUG: Log all final claims
                    _logger.LogInformation("IAPVerificationMiddleware - Final claims for user {Email}:", extractedEmail);
                    foreach (var claim in claims)
                    {
                        _logger.LogInformation("  Claim: {Type} = {Value}", claim.Type, claim.Value);
                    }

                    _logger.LogInformation("IAPVerificationMiddleware - Successfully authenticated user via header: {Email}", extractedEmail);
                    await _next(context);
                    return;
                }
                else
                {
                    _logger.LogWarning("JWT verification required but failed. Denying access.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: JWT verification required");
                    return;
                }
            }
            else if (!jwtVerified)
            {
                _logger.LogWarning("No IAP authentication found (neither JWT nor email header)");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: No IAP authentication found");
                return;
            }

            // If we got here with jwtVerified true, use the JWT principal
            if (jwtVerified && jwtPrincipal != null)
            {
                // Set the validated principal as the current user
                context.User = jwtPrincipal;
                
                // DEBUG: Log all final claims from JWT
                _logger.LogInformation("IAPVerificationMiddleware - Final claims from JWT for user {Email}:", verifiedEmail);
                foreach (var claim in jwtPrincipal.Claims)
                {
                    _logger.LogInformation("  Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
                _logger.LogInformation("IAPVerificationMiddleware - Successfully authenticated user via JWT: {Email}", verifiedEmail);
                await _next(context);
                return;
            }

            // If we somehow got here without setting a principal, deny access
            _logger.LogWarning("Authentication failed - no valid identity established");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Authentication failed");
        }

        private string ExtractEmailFromHeader(string emailHeader)
        {
            // Header format: "accounts.google.com:user@example.com" or "accounts.google.com:123456789"
            var parts = emailHeader.Split(':', 2);
            if (parts.Length != 2)
            {
                return string.Empty;
            }

            var value = parts[1].Trim();
            
            // If the value is numeric, it's a user ID - find or create user with this ID
            if (long.TryParse(value, out _))
            {
                // Use the numeric ID as is - it will be converted to int when used as NameIdentifier
                return value;
            }
            
            return value;
        }

        private async Task<ClaimsPrincipal> VerifyIapJwtAndGetPrincipalAsync(string jwt, HttpContext context)
        {
            // Parse the JWT without validation first to get the kid (key ID)
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
            
            if (jsonToken == null)
            {
                throw new SecurityTokenException("Invalid JWT token format");
            }
            
            var kid = jsonToken.Header["kid"]?.ToString();
            if (string.IsNullOrEmpty(kid))
            {
                throw new SecurityTokenException("JWT missing kid (key ID) header");
            }
            
            // Log token information for debugging
            _logger.LogDebug("JWT Header: {@JwtHeader}", jsonToken.Header);
            _logger.LogDebug("JWT Claims: {@JwtClaims}", jsonToken.Claims.Select(c => new { c.Type, c.Value }));
            
            // Get the public key for this kid
            var publicKey = await GetPublicKeyAsync(kid);
            
            // Get the expected audience
            string? projectNumber = _configuration["IAP:ProjectNumber"];
            
            // Try multiple audience formats
            List<string> audiences = new List<string>();
            
            // Add configured audience if available
            string? configuredAudience = _configuration["IAP:Audience"];
            if (!string.IsNullOrEmpty(configuredAudience))
            {
                audiences.Add(configuredAudience);
            }
            
            // Cloud Run format
            string? region = _configuration["IAP:Region"];
            string? serviceName = _configuration["IAP:ServiceName"];
            
            // Backend service format
            string? backendServiceId = _configuration["IAP:BackendServiceId"];
            if (!string.IsNullOrEmpty(projectNumber) && 
                !string.IsNullOrEmpty(backendServiceId))
            {
                audiences.Add($"/projects/{projectNumber}/global/backendServices/{backendServiceId}");
            }
            
            _logger.LogDebug("Trying JWT validation with audience formats: {@Audiences}", audiences);
            
            // Try each audience until one works
            SecurityToken? validatedToken = null;
            ClaimsPrincipal? validatedPrincipal = null;
            Exception? lastException = null;
            
            foreach (var audience in audiences)
            {
                try
                {
                    // Set up the parameters for JWT validation
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = IAP_ISSUER,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = publicKey,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                    
                    // Validate the JWT
                    validatedPrincipal = handler.ValidateToken(jwt, validationParameters, out validatedToken);
                    _logger.LogInformation("JWT validation successful with audience: {Audience}", audience);
                    break; // Success, exit the loop
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogDebug("JWT validation failed with audience {Audience}: {ErrorMessage}", 
                        audience, ex.Message);
                    // Continue to try next audience
                }
            }
            
            if (validatedPrincipal == null)
            {
                _logger.LogWarning("JWT validation failed with all audience formats");
                throw lastException ?? new SecurityTokenException("JWT validation failed with all audience formats");
            }
            
            // Extract the email claim from the validated token - try multiple possible claim types
            string? userEmail = null;
            
            // Common claim types for email in IAP tokens
            var emailClaimTypes = new[] { 
                "email", 
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                "preferred_username",
                "unique_name",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
            };
            
            // Check all possible email claim types
            foreach (var claimType in emailClaimTypes)
            {
                userEmail = jsonToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogDebug("Found email claim in claim type: {ClaimType}", claimType);
                    break;
                }
            }
            
            // Check for subject claim which might contain either email or numeric ID
            var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            _logger.LogInformation("IAPVerificationMiddleware - Processing subject claim: {SubClaim}", subClaim);

            // Add user identity claims if not already present
            var identity = validatedPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
            {
                throw new SecurityTokenException("Validated principal has no ClaimsIdentity");
            }

            if (!string.IsNullOrEmpty(subClaim))
            {
                if (subClaim.Contains("@"))
                {
                    // If subject contains @, it's an email
                    userEmail = subClaim.Contains(":") ? subClaim.Split(':').Last() : subClaim;
                    _logger.LogInformation("IAPVerificationMiddleware - Using subject claim as email: {Email}", userEmail);
                }
            }
            
            // For external identities, the email might be in the gcip claim
            if (string.IsNullOrEmpty(userEmail))
            {
                var gcipClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "gcip")?.Value;
                if (!string.IsNullOrEmpty(gcipClaim))
                {
                    try
                    {
                        var gcipJson = JsonDocument.Parse(gcipClaim);
                        if (gcipJson.RootElement.TryGetProperty("email", out var emailElement))
                        {
                            userEmail = emailElement.GetString();
                            userEmail = !string.IsNullOrEmpty(userEmail) && userEmail.Contains(":") ? userEmail.Split(':').Last() : userEmail;
                            _logger.LogDebug("Found email in gcip claim: {Email}", userEmail);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse gcip claim for email");
                    }
                }
            }
            
            // Last resort: try to extract from any claim that looks like an email
            if (string.IsNullOrEmpty(userEmail))
            {
                foreach (var claim in jsonToken.Claims)
                {
                    if (claim.Value.Contains("@") && claim.Value.Contains("."))
                    {
                        userEmail = claim.Value;
                        userEmail = userEmail.Contains(":") ? userEmail.Split(':').Last() : userEmail;
                        _logger.LogDebug("Found potential email in claim {ClaimType}: {Email}", claim.Type, userEmail);
                        break;
                    }
                }
            }
            
            // Check if we need to fall back to IAP header
            if (string.IsNullOrEmpty(userEmail) && context.Request.Headers.TryGetValue("x-goog-authenticated-user-email", out var emailHeaderValues))
            {
                var emailHeader = emailHeaderValues.ToString();
                if (emailHeader.Contains(':'))
                {
                    userEmail = emailHeader.Split(':').Last();
                    _logger.LogDebug("Used email from IAP header as fallback: {Email}", userEmail);
                }
                else
                {
                    userEmail = emailHeader;
                }
            }
            
            if (string.IsNullOrEmpty(userEmail))
            {
                // Log all claims to help diagnose the issue
                _logger.LogWarning("JWT missing email claim. Available claims: {@Claims}", 
                    jsonToken.Claims.Select(c => new { c.Type, c.Value }));
                throw new SecurityTokenException("JWT missing email claim");
            }
            // Remove the account provider prefix if it exists. Should be handled above, but just in case.
            userEmail = userEmail.Contains(":") ? userEmail.Split(':').Last() : userEmail;
            
            if (!validatedPrincipal.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, userEmail));
            }
            if (!validatedPrincipal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, userEmail));
            }
            
            // Add a special claim to indicate this is a verified IAP JWT (used for security checks)
            identity.AddClaim(new Claim("iap-jwt-verified", "true"));
            
            // Add all original JWT claims for potential use in authorization
            foreach (var claim in jsonToken.Claims)
            {
                if (!validatedPrincipal.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value ?? ""));
                }
            }
            
            // Get the user manager
            var userManager = context.RequestServices.GetService<UserManager<PAOIdentityUser>>();
            var roleManager = context.RequestServices.GetService<RoleManager<PAOIdentityRole>>();
            if (userManager != null && roleManager != null)
            {
                try
                {
                    // Find the user in the database
                    var user = await userManager.FindByEmailAsync(userEmail);
                    if (user != null)
                    {
                        // Remove any existing NameIdentifier claim
                        var existingNameId = identity.FindFirst(ClaimTypes.NameIdentifier);
                        if (existingNameId != null)
                        {
                            identity.RemoveClaim(existingNameId);
                        }

                        // Use the database ID as NameIdentifier
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        _logger.LogInformation("IAPVerificationMiddleware - Added database ID as NameIdentifier from JWT: {Id}", user.Id);

                        // Check if user has any roles
                        var roles = await userManager.GetRolesAsync(user);
                        _logger.LogInformation("IAPVerificationMiddleware - User {Email} has {RoleCount} roles: {Roles}", userEmail, roles.Count, string.Join(", ", roles));
                        
                        if (!roles.Any())
                        {
                            // Ensure UNOPS_GEN_USER role exists
                            if (!await roleManager.RoleExistsAsync("UNOPS_GEN_USER"))
                            {
                                await roleManager.CreateAsync(new PAOIdentityRole { Name = "UNOPS_GEN_USER" });
                                _logger.LogInformation("IAPVerificationMiddleware - Created UNOPS_GEN_USER role");
                            }

                            // Add UNOPS_GEN_USER role
                            await userManager.AddToRoleAsync(user, "UNOPS_GEN_USER");
                            identity.AddClaim(new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"));
                            _logger.LogInformation("IAPVerificationMiddleware - Added UNOPS_GEN_USER role to user");
                        }
                        else
                        {
                            // Add existing roles as claims
                            foreach (var role in roles)
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                                _logger.LogInformation("IAPVerificationMiddleware - Added existing role claim: {Role}", role);
                            }
                        }
                    }
                    else
                    {
                        // User not found - create new user
                        _logger.LogInformation("IAPVerificationMiddleware - User not found in database for JWT email: {Email}. Creating new user.", userEmail);
                        
                        var newUser = new PAOIdentityUser
                        {
                            UserName = userEmail,
                            Email = userEmail,
                            EmailConfirmed = true // Since this comes from IAP, we trust the email is verified
                        };

                        var createResult = await userManager.CreateAsync(newUser);
                        if (createResult.Succeeded)
                        {
                            _logger.LogInformation("IAPVerificationMiddleware - Successfully created new user from JWT: {Email} with ID: {Id}", userEmail, newUser.Id);
                            
                            // Remove any existing NameIdentifier claim
                            var existingNameId = identity.FindFirst(ClaimTypes.NameIdentifier);
                            if (existingNameId != null)
                            {
                                identity.RemoveClaim(existingNameId);
                            }
                            
                            // Use the new database ID as NameIdentifier
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()));
                            
                            // Ensure UNOPS_GEN_USER role exists
                            if (!await roleManager.RoleExistsAsync("UNOPS_GEN_USER"))
                            {
                                await roleManager.CreateAsync(new PAOIdentityRole { Name = "UNOPS_GEN_USER" });
                                _logger.LogInformation("IAPVerificationMiddleware - Created UNOPS_GEN_USER role");
                            }

                            // Add UNOPS_GEN_USER role to new user
                            await userManager.AddToRoleAsync(newUser, "UNOPS_GEN_USER");
                            identity.AddClaim(new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"));
                            _logger.LogInformation("IAPVerificationMiddleware - Added UNOPS_GEN_USER role to new user from JWT");
                        }
                        else
                        {
                            _logger.LogError("IAPVerificationMiddleware - Failed to create user from JWT for email: {Email}. Errors: {Errors}", 
                                userEmail, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                            // Continue without database ID - will use the original JWT subject
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "IAPVerificationMiddleware - Error handling user lookup/creation from JWT");
                }
            }
            else
            {
                _logger.LogWarning("IAPVerificationMiddleware - UserManager or RoleManager not available for JWT processing");
            }
            
            return validatedPrincipal;
        }

        private async Task<JsonWebKey> GetPublicKeyAsync(string kid)
        {
            // Refresh keys if they're more than 1 hour old
            if (_keysLastRefreshed.AddHours(1) < DateTime.UtcNow)
            {
                await RefreshPublicKeysAsync();
            }
            
            // Try to get key from cache
            if (_cachedKeys.TryGetValue(kid, out var key))
            {
                return key;
            }
            
            // If key not in cache, refresh and try again
            await RefreshPublicKeysAsync();
            
            if (_cachedKeys.TryGetValue(kid, out key))
            {
                return key;
            }
            
            throw new SecurityTokenException($"No public key found for kid: {kid}");
        }

        private async Task RefreshPublicKeysAsync()
        {
            // Use a lock to prevent multiple simultaneous refreshes
            await _refreshLock.WaitAsync();
            try
            {
                // Check again in case another thread already refreshed while waiting
                if (_keysLastRefreshed.AddHours(1) > DateTime.UtcNow)
                {
                    return;
                }
                
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync(PUBLIC_KEY_URL);
                
                var jwkSet = JsonWebKeySet.Create(response);
                var newKeys = new Dictionary<string, JsonWebKey>();
                
                foreach (var jwk in jwkSet.Keys)
                {
                    if (jwk.Kid != null)
                    {
                        newKeys[jwk.Kid] = jwk;
                    }
                }
                
                // Update the cache atomically
                _cachedKeys.Clear();
                foreach (var entry in newKeys)
                {
                    _cachedKeys[entry.Key] = entry.Value;
                }
                
                _keysLastRefreshed = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh IAP public keys");
                throw;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private string GetDevelopmentUserEmail(HttpContext context)
        {
            // Development simulation from header
            if (context.Request.Headers.TryGetValue("X-Dev-IAP-Simulation", out _))
            {
                if (context.Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var headerEmail))
                {
                    var email = headerEmail.ToString();
                    if (email.Contains(':'))
                    {
                        email = email.Split(':').Last();
                    }
                    return email;
                }
            }
            
            // Check for dev auth cookie
            if (context.Request.Cookies.TryGetValue("DevIAPAuth", out var cookieEmail) && 
                !string.IsNullOrEmpty(cookieEmail))
            {
                return cookieEmail;
            }
            
            // Option 1: Use a fixed value from configuration
            var configuredEmail = _configuration["Development:IAPSimulation:UserEmail"];
            if (!string.IsNullOrEmpty(configuredEmail))
            {
                return configuredEmail;
            }
            
            // Option 2: Use a query parameter for testing different users
            if (context.Request.Query.TryGetValue("dev-user", out var queryEmail))
            {
                return queryEmail.ToString();
            }
            
            // Option 3: Use a cookie for persistent development identity
            if (context.Request.Cookies.TryGetValue("dev-user-email", out var devCookieEmail))
            {
                return devCookieEmail;
            }
            
            // Default dev user
            return "dev.user@example.com";
        }

        private async Task SetupDevUserPrincipal(HttpContext context, string email)
        {
            // Ensure we handle emails with account provider prefix
            if (email.Contains(':'))
            {
                email = email.Split(':').Last();
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim("iap-jwt-verified", "true"), // Development simulation
                new Claim("IAPAuthenticated", "true"),
                new Claim("IsInternal", email.EndsWith("@unops.org").ToString()),
                new Claim("hd", email.Split('@')[1]) // Domain claim for testing domain-based policies
            };

            try
            {
                // Get user from database to get their ID
                var userManager = context.RequestServices.GetRequiredService<UserManager<PAOIdentityUser>>();
                var roleManager = context.RequestServices.GetRequiredService<RoleManager<PAOIdentityRole>>();
                var user = await userManager.FindByEmailAsync(email);
                
                // Ensure all required roles exist
                var requiredRoles = new[] { "UNOPS_GEN_USER", "PARTNER_GLOB_ADMIN", "PARTNER_USER", "ORG_UNIT_ADMIN" };
                foreach (var roleName in requiredRoles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new PAOIdentityRole { Name = roleName });
                    }
                }
                
                if (user != null)
                {
                    // Use the actual user ID from the database
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    _logger.LogInformation("IAPVerificationMiddleware - Using database NameIdentifier claim: {Id}", user.Id);
                    
                    // Add user's actual roles
                    var roles = await userManager.GetRolesAsync(user);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
                else
                {
                    // If user doesn't exist, create a test user
                    var testUser = new PAOIdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    
                    var result = await userManager.CreateAsync(testUser);
                    if (result.Succeeded)
                    {
                        // Use the newly created user's ID
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, testUser.Id.ToString()));
                        _logger.LogInformation("IAPVerificationMiddleware - Using new test user NameIdentifier claim: {Id}", testUser.Id);
                        
                        // Add roles based on email
                        if (!string.IsNullOrEmpty(email))
                        {
                            await userManager.AddToRoleAsync(testUser, "UNOPS_GEN_USER");
                            claims.Add(new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up dev user principal for email: {Email}", email);
                // Fallback to default numeric ID if there's an error
                var numericId = Math.Abs(email.GetHashCode()).ToString();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, numericId));
                _logger.LogInformation("IAPVerificationMiddleware - Using fallback numeric NameIdentifier claim due to error: {Id}", numericId);
                claims.Add(new Claim(ClaimTypes.Role, "UNOPS_GEN_USER"));
            }
            
            // Ensure IsInternal is set correctly based on email domain
            var isInternal = email.EndsWith("@unops.org");
            claims.RemoveAll(c => c.Type == "IsInternal");
            claims.Add(new Claim("IsInternal", isInternal.ToString()));
            
            var identity = new ClaimsIdentity(claims, "Development-IAP", ClaimTypes.Name, ClaimTypes.Role);
            context.User = new ClaimsPrincipal(identity);
            
            // Store the development authentication in a cookie for session persistence
            if (_configuration.GetValue<bool>("Development:IAPSimulation:Enabled", true))
            {
                context.Response.Cookies.Append("DevIAPAuth", email, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddHours(8)
                });
            }
        }
    }
} 