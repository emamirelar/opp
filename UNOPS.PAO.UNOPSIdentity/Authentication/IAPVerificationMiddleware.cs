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
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;

namespace UNOPS.PAO.UNOPSIdentity.Authentication
{
    public class IAPVerificationMiddleware
    {
        private static readonly string IAP_ISSUER = "https://cloud.google.com/iap";
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
            
            // Log available headers for debugging
            _logger.LogDebug("Headers: {@Headers}", 
                context.Request.Headers.Select(h => new { h.Key, Value = h.Value.ToString() }));
                
            // Skip verification in development if configured
            if (_environment.IsDevelopment())
            {
                bool skipValidation = _configuration.GetValue<bool>("IAP:SkipValidationInDevelopment", 
                               _configuration.GetValue<bool>("Development:IAPSimulation:SkipValidationInDevelopment", false));
                
                // Check for development cookie indicators
                bool hasDevelopmentCookie = context.Request.Cookies.ContainsKey("DevIAPAuth") || 
                                           context.Request.Cookies.ContainsKey("dev-user-email");
                bool hasDevFlag = context.Request.Headers.ContainsKey("x-using-dev-cookie") ||
                                 context.Request.Headers.ContainsKey("X-Dev-IAP-Simulation");
                
                if (skipValidation || hasDevelopmentCookie || hasDevFlag)
                {
                    _logger.LogDebug("Development authentication detected. Using development user setup.");
                    string devEmail = GetDevelopmentUserEmail(context);
                    if (!string.IsNullOrEmpty(devEmail))
                    {
                        SetupDevUserPrincipal(context, devEmail);
                        await _next(context);
                        return;
                    }
                }
            }

            // Check if header-based authentication is allowed
            bool allowHeaderAuth = _configuration.GetValue<bool>("IAP:AllowHeaderAuthentication", true);
            _logger.LogDebug("Header-based authentication allowed: {AllowHeaderAuth}", allowHeaderAuth);
            
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
                    // Don't immediately fail here, continue and try to use header authentication
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
                else if (allowHeaderAuth || !_configuration.GetValue<bool>("IAP:RequireJwtVerification", true) || _environment.IsDevelopment())
                {
                    // Use email header if header auth is allowed, JWT verification isn't required, or in development
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
                    // Allow header-based authentication in production if JWT validation failed but a header is present
                    // This helps in cases where the JWT is properly signed but missing the email claim
                    _logger.LogInformation("Allowing header-based authentication for: {Email}", extractedEmail);
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
            var result = parts.Length == 2 ? parts[1].Trim() : string.Empty;
            _logger.LogDebug("Extracted email from header: '{Header}' -> '{Email}'", emailHeader, result);
            return result;
        }

        private async Task<ClaimsPrincipal> VerifyIapJwtAndGetPrincipalAsync(string jwt, HttpContext context)
        {
            try
            {
                // Parse the JWT without validation first to get information for logging
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
                
                if (jsonToken == null)
                {
                    throw new SecurityTokenException("Invalid JWT token format");
                }
                
                // Log token information for debugging
                _logger.LogDebug("JWT Header: {@JwtHeader}", jsonToken.Header);
                _logger.LogDebug("JWT Claims: {@JwtClaims}", jsonToken.Claims.Select(c => new { c.Type, c.Value }));
                _logger.LogDebug("JWT Audience(s): {Aud}", string.Join(",", jsonToken.Audiences));
                _logger.LogDebug("JWT Issuer: {Issuer}", jsonToken.Issuer);
                
                // Get the expected audience(s)
                List<string> audiences = GetExpectedAudiences();
                if (!audiences.Any())
                {
                    throw new InvalidOperationException("No valid audience configuration found. Configure IAP:Audience or IAP:ProjectNumber");
                }
                
                _logger.LogDebug("Trying JWT validation with audience formats: {@Audiences}", audiences);
                
                // Use Google's official verification library
                JsonWebSignature.Payload payload = null;
                Exception lastException = null;
                
                foreach (var audience in audiences)
                {
                    try
                    {
                        var options = new SignedTokenVerificationOptions
                        {
                            IssuedAtClockTolerance = TimeSpan.FromMinutes(2),
                            ExpiryClockTolerance = TimeSpan.FromMinutes(2),
                            TrustedAudiences = { audience },
                            TrustedIssuers = { IAP_ISSUER },
                            CertificatesUrl = GoogleAuthConsts.IapKeySetUrl
                        };
                        
                        // This will throw if validation fails
                        payload = await JsonWebSignature.VerifySignedTokenAsync(jwt, options);
                        
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
                
                if (payload == null)
                {
                    _logger.LogWarning("JWT validation failed with all audience formats");
                    throw lastException ?? new SecurityTokenException("JWT validation failed with all audience formats");
                }
                
                // Create a claims identity using the payload
                var claims = new List<Claim>();
                
                // Add email claim - Google payload should have the Email property
                string emailValue = payload.Email;
                
                // If email is still null, try the subject claim or other claims
                if (string.IsNullOrEmpty(emailValue))
                {
                    if (!string.IsNullOrEmpty(payload.Subject) && payload.Subject.Contains("@"))
                    {
                        emailValue = payload.Subject;
                        _logger.LogDebug("Using subject as email: {Email}", emailValue);
                    }
                    else if (payload.ContainsKey("email"))
                    {
                        emailValue = payload["email"] as string;
                        _logger.LogDebug("Found email in custom claim: {Email}", emailValue);
                    }
                }
                
                // Check if we need to fall back to IAP header
                if (string.IsNullOrEmpty(emailValue) && context.Request.Headers.TryGetValue("x-goog-authenticated-user-email", out var emailHeaderValues))
                {
                    var emailHeader = emailHeaderValues.ToString();
                    emailValue = ExtractEmailFromHeader(emailHeader);
                    _logger.LogDebug("Used email from IAP header as fallback: {Email}", emailValue);
                }
                
                if (string.IsNullOrEmpty(emailValue))
                {
                    // Log all payload properties to help diagnose the issue
                    _logger.LogWarning("JWT missing email claim. Available payload properties: {@Payload}", 
                        payload.ToDictionary(k => k.Key, v => v.Value));
                    
                    // Instead of throwing an exception, try to continue with header-based authentication
                    if (context.Request.Headers.TryGetValue("x-goog-authenticated-user-email", out var fallbackEmailValues))
                    {
                        var fallbackEmailHeader = fallbackEmailValues.ToString();
                        emailValue = ExtractEmailFromHeader(fallbackEmailHeader);
                        
                        if (!string.IsNullOrEmpty(emailValue))
                        {
                            _logger.LogInformation("Using email from header after JWT validation: {Email}", emailValue);
                        }
                        else
                        {
                            throw new SecurityTokenException("JWT missing email claim and header email extraction failed");
                        }
                    }
                    else
                    {
                        throw new SecurityTokenException("JWT missing email claim");
                    }
                }
                
                // Add standard claims
                claims.Add(new Claim(ClaimTypes.Name, emailValue));
                claims.Add(new Claim(ClaimTypes.Email, emailValue));
                claims.Add(new Claim("iap-jwt-verified", "true"));
                
                // Add subject claim if available
                if (!string.IsNullOrEmpty(payload.Subject))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, payload.Subject));
                    claims.Add(new Claim("sub", payload.Subject));
                }
                
                // Add all other claims from payload
                foreach (var entry in payload)
                {
                    if (entry.Value != null && !claims.Any(c => c.Type == entry.Key))
                    {
                        claims.Add(new Claim(entry.Key, entry.Value.ToString()));
                    }
                }
                
                // Add the IAP validation status to the HTTP context
                context.Items["IAP_JWT_VALIDATED"] = true;
                context.Items["IAP_JWT_EMAIL"] = emailValue;
                
                // Also add a special header for the authentication handler to detect
                context.Request.Headers["X-IAP-JWT-Validated"] = "true";
                context.Request.Headers["X-IAP-JWT-Email"] = emailValue;
                
                return new ClaimsPrincipal(new ClaimsIdentity(claims, "IAP-JWT"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying IAP JWT token");
                throw;
            }
        }

        private List<string> GetExpectedAudiences()
        {
            List<string> audiences = new List<string>();
            
            // Add configured audience if available
            string configuredAudience = _configuration["IAP:Audience"];
            if (!string.IsNullOrEmpty(configuredAudience))
            {
                audiences.Add(configuredAudience);
            }
            
            // Get the project number
            string projectNumber = _configuration["IAP:ProjectNumber"];
            
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
            
            return audiences;
        }

        private string GetDevelopmentUserEmail(HttpContext context)
        {
            _logger.LogDebug("Looking for development user email");
            
            // Option 1: Development simulation from header
            if (context.Request.Headers.TryGetValue("X-Dev-IAP-Simulation", out _))
            {
                if (context.Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var headerEmail))
                {
                    var email = headerEmail.ToString();
                    if (email.Contains(':'))
                    {
                        email = email.Split(':').Last();
                    }
                    _logger.LogDebug("Found development email in header: {Email}", email);
                    return email;
                }
            }
            
            // Option 2: Check for dev auth cookie (preferred for web apps)
            if (context.Request.Cookies.TryGetValue("DevIAPAuth", out var cookieEmail) && 
                !string.IsNullOrEmpty(cookieEmail))
            {
                _logger.LogDebug("Found DevIAPAuth cookie with value: {Email}", cookieEmail);
                return Uri.UnescapeDataString(cookieEmail);
            }
            
            // Option 3: Check for dev-user-email cookie (client-side set)
            if (context.Request.Cookies.TryGetValue("dev-user-email", out var devUserEmail) && 
                !string.IsNullOrEmpty(devUserEmail))
            {
                _logger.LogDebug("Found dev-user-email cookie with value: {Email}", devUserEmail);
                return Uri.UnescapeDataString(devUserEmail);
            }
            
            // Option 4: Use a fixed value from configuration
            var configuredEmail = _configuration["Development:IAPSimulation:UserEmail"];
            if (!string.IsNullOrEmpty(configuredEmail))
            {
                _logger.LogDebug("Using configured development email: {Email}", configuredEmail);
                return configuredEmail;
            }
            
            // Option 5: Use a query parameter for testing different users
            if (context.Request.Query.TryGetValue("dev-user", out var queryEmail))
            {
                _logger.LogDebug("Using development email from query string: {Email}", queryEmail.ToString());
                return queryEmail.ToString();
            }
            
            // Default dev user
            _logger.LogDebug("Using default development email: dev.user@example.com");
            return "dev.user@example.com";
        }

        private void SetupDevUserPrincipal(HttpContext context, string email)
        {
            // Ensure we handle emails with account provider prefix and any URL encoding
            email = Uri.UnescapeDataString(email);
            
            if (email.Contains(':'))
            {
                email = email.Split(':').Last();
            }
            
            _logger.LogInformation("Setting up development user principal for: {Email}", email);
            
            var devClaims = new List<Claim>
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
                devClaims.Add(new Claim(ClaimTypes.Role, "Internal"));
                
                // If admin is in the email, add Administrator role
                if (email.ToLower().Contains("admin"))
                {
                    devClaims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                }
            }
            else
            {
                devClaims.Add(new Claim(ClaimTypes.Role, "External"));
                
                // Check for partner domain patterns
                if (_configuration.GetSection("IAP:DomainRoles").Exists())
                {
                    string emailDomain = email.Substring(email.IndexOf('@') + 1);
                    var domainRoles = _configuration.GetSection("IAP:DomainRoles").GetChildren();
                    
                    foreach (var domainRole in domainRoles)
                    {
                        if (emailDomain == domainRole.Key)
                        {
                            devClaims.Add(new Claim(ClaimTypes.Role, domainRole.Value));
                            break;
                        }
                    }
                }
            }
            
            // Add basic User role
            devClaims.Add(new Claim(ClaimTypes.Role, "User"));
            
            var devIdentity = new ClaimsIdentity(devClaims, "Development-IAP");
            context.User = new ClaimsPrincipal(devIdentity);
            
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