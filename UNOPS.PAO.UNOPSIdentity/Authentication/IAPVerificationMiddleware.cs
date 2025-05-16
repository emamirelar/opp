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
            // Health check path exception
            string healthCheckPath = _configuration["IAP:HealthCheckPath"] ?? "/health";
            if (context.Request.Path.StartsWithSegments(healthCheckPath))
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
                    SetupDevUserPrincipal(context, devEmail);
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
                try
                {
                    jwtPrincipal = await VerifyIapJwtAndGetPrincipalAsync(jwt, context);
                    if (jwtPrincipal != null)
                    {
                        jwtVerified = true;
                        verifiedEmail = jwtPrincipal.FindFirstValue(ClaimTypes.Email);
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
                _logger.LogDebug("No JWT header found, checking for email header");
            }

            // Secondary Authentication: Email Header Check (fallback or verification)
            if (context.Request.Headers.TryGetValue("x-goog-authenticated-user-email", out var emailHeaderValues))
            {
                var emailHeader = emailHeaderValues.ToString();
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
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "IAP-Header"));
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
                context.User = jwtPrincipal;
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
            // Header format: "accounts.google.com:john.doe@example.com"
            var parts = emailHeader.Split(':', 2);
            return parts.Length == 2 ? parts[1].Trim() : string.Empty;
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
            string projectNumber = _configuration["IAP:ProjectNumber"];
            
            // Try multiple audience formats
            List<string> audiences = new List<string>();
            
            // Add configured audience if available
            string configuredAudience = _configuration["IAP:Audience"];
            if (!string.IsNullOrEmpty(configuredAudience))
            {
                audiences.Add(configuredAudience);
            }
            
            // Cloud Run format
            string region = _configuration["IAP:Region"];
            string serviceName = _configuration["IAP:ServiceName"];
            if (!string.IsNullOrEmpty(projectNumber) && 
                !string.IsNullOrEmpty(region) && 
                !string.IsNullOrEmpty(serviceName))
            {
                audiences.Add($"/projects/{projectNumber}/locations/{region}/services/{serviceName}");
            }
            
            // Backend service format
            string backendServiceId = _configuration["IAP:BackendServiceId"];
            if (!string.IsNullOrEmpty(projectNumber) && 
                !string.IsNullOrEmpty(backendServiceId))
            {
                audiences.Add($"/projects/{projectNumber}/global/backendServices/{backendServiceId}");
            }
            
            // Simplest format
            if (!string.IsNullOrEmpty(projectNumber))
            {
                audiences.Add($"/projects/{projectNumber}");
            }
            
            // Project ID/app format
            string projectId = _configuration["IAP:ProjectId"];
            if (!string.IsNullOrEmpty(projectNumber) && !string.IsNullOrEmpty(projectId))
            {
                audiences.Add($"/projects/{projectNumber}/apps/{projectId}");
            }
            
            if (!audiences.Any())
            {
                throw new InvalidOperationException("No valid audience configuration found. Configure IAP:Audience or IAP:ProjectNumber");
            }
            
            _logger.LogDebug("Trying JWT validation with audience formats: {@Audiences}", audiences);
            
            // Try each audience until one works
            SecurityToken validatedToken = null;
            ClaimsPrincipal validatedPrincipal = null;
            Exception lastException = null;
            
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
            string email = null;
            
            // Common claim types for email in IAP tokens
            var emailClaimTypes = new[] { 
                "email", 
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                "preferred_username",
                "unique_name",
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
                "sub" // Sometimes the subject claim contains the email
            };
            
            // Check all possible email claim types
            foreach (var claimType in emailClaimTypes)
            {
                email = jsonToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    _logger.LogDebug("Found email claim in claim type: {ClaimType}", claimType);
                    break;
                }
            }
            
            // If still no email, check for the subject claim which might have the email
            if (string.IsNullOrEmpty(email))
            {
                var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                if (!string.IsNullOrEmpty(subClaim) && subClaim.Contains("@"))
                {
                    email = subClaim;
                    _logger.LogDebug("Using subject claim as email: {Email}", email);
                }
            }
            
            // For external identities, the email might be in the gcip claim
            if (string.IsNullOrEmpty(email))
            {
                var gcipClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "gcip")?.Value;
                if (!string.IsNullOrEmpty(gcipClaim))
                {
                    try
                    {
                        var gcipJson = JsonDocument.Parse(gcipClaim);
                        if (gcipJson.RootElement.TryGetProperty("email", out var emailElement))
                        {
                            email = emailElement.GetString();
                            _logger.LogDebug("Found email in gcip claim: {Email}", email);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse gcip claim for email");
                    }
                }
            }
            
            // Last resort: try to extract from any claim that looks like an email
            if (string.IsNullOrEmpty(email))
            {
                foreach (var claim in jsonToken.Claims)
                {
                    if (claim.Value.Contains("@") && claim.Value.Contains("."))
                    {
                        email = claim.Value;
                        _logger.LogDebug("Found potential email in claim {ClaimType}: {Email}", claim.Type, email);
                        break;
                    }
                }
            }
            
            // Check if we need to fall back to IAP header
            if (string.IsNullOrEmpty(email) && context.Request.Headers.TryGetValue("x-goog-authenticated-user-email", out var emailHeaderValues))
            {
                var emailHeader = emailHeaderValues.ToString();
                if (emailHeader.Contains(':'))
                {
                    email = emailHeader.Split(':').Last();
                    _logger.LogDebug("Used email from IAP header as fallback: {Email}", email);
                }
                else
                {
                    email = emailHeader;
                }
            }
            
            if (string.IsNullOrEmpty(email))
            {
                // Log all claims to help diagnose the issue
                _logger.LogWarning("JWT missing email claim. Available claims: {@Claims}", 
                    jsonToken.Claims.Select(c => new { c.Type, c.Value }));
                throw new SecurityTokenException("JWT missing email claim");
            }
            
            // Add user identity claims if not already present
            var identity = validatedPrincipal.Identity as ClaimsIdentity;
            if (!validatedPrincipal.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, email));
            }
            if (!validatedPrincipal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email));
            }
            
            // Add a special claim to indicate this is a verified IAP JWT (used for security checks)
            identity.AddClaim(new Claim("iap-jwt-verified", "true"));
            
            // Add all original JWT claims for potential use in authorization
            foreach (var claim in jsonToken.Claims)
            {
                if (!validatedPrincipal.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value));
                }
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

        private void SetupDevUserPrincipal(HttpContext context, string email)
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
            
            // Add role claims based on domain
            if (email.EndsWith("@unops.org"))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Internal"));
                
                // If admin is in the email, add Administrator role
                if (email.ToLower().Contains("admin"))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "External"));
                
                // Check for partner domain patterns
                if (_configuration.GetSection("IAP:DomainRoles").Exists())
                {
                    string domain = email.Substring(email.IndexOf('@') + 1);
                    var domainRoles = _configuration.GetSection("IAP:DomainRoles").GetChildren();
                    
                    foreach (var domainRole in domainRoles)
                    {
                        if (domain == domainRole.Key)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, domainRole.Value));
                            break;
                        }
                    }
                }
            }
            
            // Add basic User role
            claims.Add(new Claim(ClaimTypes.Role, "User"));
            
            var identity = new ClaimsIdentity(claims, "Development-IAP");
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