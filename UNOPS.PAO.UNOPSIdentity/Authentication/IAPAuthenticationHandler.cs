namespace UNOPS.PAO.UNOPSIdentity.Authentication;

using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using UNOPS.PAO.Identity.Entities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

public class IAPAuthenticationHandler : AuthenticationHandler<IAPAuthenticationOptions>
{
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly RoleManager<PAOIdentityRole> _roleManager;
    private readonly ILogger<IAPAuthenticationHandler> _logger;
    private readonly IConfiguration _configuration;

    public IAPAuthenticationHandler(
        IOptionsMonitor<IAPAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger.CreateLogger<IAPAuthenticationHandler>();
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Skip authentication on the dev-login page
        if (Request.Path.StartsWithSegments("/dev-login"))
        {
            return AuthenticateResult.NoResult();
        }

        // First check for Bearer token
        var bearerToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        ClaimsPrincipal? bearerPrincipal = null;

        if (!string.IsNullOrEmpty(bearerToken))
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecret = _configuration["JWTSettings:Secret"];
                if (string.IsNullOrEmpty(jwtSecret))
                {
                    _logger.LogWarning("JWTSettings:Secret not configured, skipping Bearer token validation");
                }
                else
                {
                    var key = Encoding.ASCII.GetBytes(jwtSecret);

                    var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JWTSettings:validIssuer"],
                    ValidAudience = _configuration["JWTSettings:validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                    };

                    bearerPrincipal = tokenHandler.ValidateToken(bearerToken, validationParameters, out _);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Bearer token validation failed");
            }
        }

        // Check if we already have a verified JWT from the middleware
        if (Context.User?.Identity?.IsAuthenticated == true && 
            Context.User.HasClaim(c => c.Type == "iap-jwt-verified" && c.Value == "true"))
        {
            _logger.LogInformation("🔍 [MIDDLEWARE-AUTH] User already authenticated by middleware: {Email}", 
                Context.User.FindFirstValue(ClaimTypes.Email));
            
            // Handle impersonation for middleware-authenticated users
            var middlewarePrincipal = Context.User;
            var authenticatedEmail = Context.User.FindFirstValue(ClaimTypes.Email);
            
            // Strip identity provider prefix if present (e.g., "securetoken.google.com/project/tenant:email@domain.com" -> "email@domain.com")
            if (authenticatedEmail?.Contains(':') == true)
            {
                authenticatedEmail = authenticatedEmail.Split(':').Last();
                _logger.LogDebug("🔍 [MIDDLEWARE-AUTH] Stripped email prefix, using: {Email}", authenticatedEmail);
            }
            
            if (Options.EnableImpersonation && 
                Request.Headers.TryGetValue(Options.ImpersonationHeaderName, out var middlewareImpersonatedEmailValues))
            {
                var middlewareImpersonatedEmail = middlewareImpersonatedEmailValues.ToString()?.Trim();
                _logger.LogInformation("🔍 [MIDDLEWARE-IMPERSONATION] Impersonation header found: {ImpersonatedEmail}", middlewareImpersonatedEmail);
                
                if (!string.IsNullOrEmpty(middlewareImpersonatedEmail) && middlewareImpersonatedEmail != authenticatedEmail)
                {
                    // Check if authenticated user is trusted
                    bool isTrusted = Options.TrustedServiceAccounts?.Any(sa => 
                        sa.Equals(authenticatedEmail, StringComparison.OrdinalIgnoreCase)) == true;
                    
                    if (isTrusted)
                    {
                        _logger.LogInformation("🔄 [MIDDLEWARE-IMPERSONATION] Trusted service account {AuthUser} requesting impersonation of {TargetUser}", 
                            authenticatedEmail, middlewareImpersonatedEmail);
                        
                        // Look up the impersonated user (must be active)
                        var middlewareNormalizedEmail = _userManager.NormalizeEmail(middlewareImpersonatedEmail);
                        var middlewareImpersonatedUser = await _userManager.Users
                            .FirstOrDefaultAsync(u => u.NormalizedEmail == middlewareNormalizedEmail && u.ActiveUser);
                        
                        if (middlewareImpersonatedUser != null)
                        {
                            // Get impersonated user's roles and claims
                            var middlewareUserRoles = await _userManager.GetRolesAsync(middlewareImpersonatedUser);
                            var middlewareUserClaims = await _userManager.GetClaimsAsync(middlewareImpersonatedUser);
                            
                            _logger.LogInformation("✅ [MIDDLEWARE-IMPERSONATION] Successfully impersonating {ImpersonatedUser}. Roles: {Roles}",
                                middlewareImpersonatedEmail, string.Join(", ", middlewareUserRoles));
                            
                            // Create new identity with impersonated user's information
                            var middlewareImpersonatedIdentity = new ClaimsIdentity(middlewareUserClaims, "IAP", ClaimTypes.Name, ClaimTypes.Role);
                            
                            // Add essential claims
                            if (!middlewareImpersonatedIdentity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                                middlewareImpersonatedIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, middlewareImpersonatedUser.Id.ToString()));
                            
                            if (!middlewareImpersonatedIdentity.HasClaim(c => c.Type == ClaimTypes.Name))
                                middlewareImpersonatedIdentity.AddClaim(new Claim(ClaimTypes.Name, middlewareImpersonatedUser.UserName ?? middlewareImpersonatedUser.Email ?? ""));
                            
                            if (!middlewareImpersonatedIdentity.HasClaim(c => c.Type == ClaimTypes.Email))
                                middlewareImpersonatedIdentity.AddClaim(new Claim(ClaimTypes.Email, middlewareImpersonatedUser.Email ?? ""));
                            
                            if (!middlewareImpersonatedIdentity.HasClaim(c => c.Type == "IsInternal"))
                                middlewareImpersonatedIdentity.AddClaim(new Claim("IsInternal", middlewareImpersonatedUser.IsInternal.ToString()));
                            
                            // Add role claims
                            foreach (var role in middlewareUserRoles)
                            {
                                if (!middlewareImpersonatedIdentity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role))
                                    middlewareImpersonatedIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                            
                            // Add impersonation audit claims
                            middlewareImpersonatedIdentity.AddClaim(new Claim("IsImpersonating", "true"));
                            middlewareImpersonatedIdentity.AddClaim(new Claim("AuthenticatedServiceAccount", authenticatedEmail ?? ""));
                            middlewareImpersonatedIdentity.AddClaim(new Claim("ImpersonatedUser", middlewareImpersonatedEmail));
                            middlewareImpersonatedIdentity.AddClaim(new Claim("iap-jwt-verified", "true"));
                            
                            middlewarePrincipal = new ClaimsPrincipal(middlewareImpersonatedIdentity);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ [MIDDLEWARE-IMPERSONATION] Impersonated user not found: {ImpersonatedEmail} (normalized: {NormalizedEmail})", 
                                middlewareImpersonatedEmail, middlewareNormalizedEmail);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("🚫 [MIDDLEWARE-IMPERSONATION] User {UserEmail} is not in trusted service accounts list. Impersonation denied.",
                            authenticatedEmail);
                    }
                }
            }
            
            // If we have a valid Bearer token, merge its claims
            if (bearerPrincipal != null)
            {
                var userIdentity = middlewarePrincipal.Identity as ClaimsIdentity;
                if (userIdentity != null)
                {
                    foreach (var claim in bearerPrincipal.Claims)
                    {
                        if (!userIdentity.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                        {
                            userIdentity.AddClaim(claim);
                        }
                    }
                }
            }
            
            return AuthenticateResult.Success(new AuthenticationTicket(middlewarePrincipal, Scheme.Name));
        }

        // Validate IAP JWT if required
        if (Options.RequireJwtVerification && !await ValidateIapJwtAsync())
        {
            _logger.LogWarning("IAP JWT validation failed");
            return AuthenticateResult.Fail("Invalid IAP JWT token");
        }
        
        // Extract email from IAP headers or from JWT validation
        string userEmail;
        
        // Check if we have a verified email from JWT
        if (Request.Headers.TryGetValue("X-Goog-IAP-JWT-Assertion", out var jwtValues))
        {
            var jwt = jwtValues.ToString();
            try 
            {
                var principle = await VerifyIapJwtAndGetPrincipalAsync(jwt);
                if (principle != null)
                {
                    var email = principle.FindFirstValue(ClaimTypes.Email);
                    if (!string.IsNullOrEmpty(email))
                    {
                        userEmail = email;
                        goto ProcessUser; // Skip the header check
                    }
                }
            }
            catch
            {
                // JWT verification failed, continue to header-based auth
            }
        }
        
        // Extract IAP headers if JWT verification failed or was skipped
        if (!Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var userEmailValues))
        {
            // Check if we're in development mode and should use cookie auth as fallback
            var env = Context.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
            var config = Context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            
            if (env?.IsDevelopment() == true && 
                config?.GetValue<bool>("Development:IAPSimulation:Enabled", false) == true)
            {
                // Look for dev auth cookie first
                if (Request.Cookies.TryGetValue("DevIAPAuth", out var emailFromCookie) && !string.IsNullOrEmpty(emailFromCookie))
                {
                    userEmailValues = new Microsoft.Extensions.Primitives.StringValues(emailFromCookie);
                    _logger.LogDebug("Using email from DevIAPAuth cookie: {Email}", emailFromCookie);
                }
                else
                {
                    // Fall back to configured UserEmail for dev simulation
                    var configuredDevEmail = config?.GetValue<string>("Development:IAPSimulation:UserEmail");
                    if (!string.IsNullOrEmpty(configuredDevEmail))
                    {
                        userEmailValues = new Microsoft.Extensions.Primitives.StringValues(configuredDevEmail);
                        _logger.LogInformation("🔧 [DEV-MODE] Using configured development user: {Email}", configuredDevEmail);
                    }
                    else
                    {
                        _logger.LogWarning("Development IAP simulation enabled but no UserEmail configured and no cookie found");
                        return AuthenticateResult.NoResult();
                    }
                }
            }
            else
            {
                return AuthenticateResult.NoResult();
            }
        }

        // The email header is in the format "accounts.google.com:user@example.com"
        userEmail = userEmailValues.ToString();
        if (userEmail.Contains(':'))
        {
            userEmail = userEmail.Split(':').Last();
        }
        
    ProcessUser:
        // Find or create user based on Google identity
        _logger.LogInformation("🔍 [AUTH] Looking up user by email: {Email}", userEmail);
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user != null && !user.ActiveUser)
        {
            _logger.LogWarning("User account is inactive: {Email}", userEmail);
            return AuthenticateResult.Fail("User account is inactive");
        }
        if (user == null)
        {
            // Auto-provision user if enabled
            if (Options.AutoProvisionUsers)
            {
                user = new PAOIdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    IsInternal = userEmail.EndsWith("@unops.org"),
                    GoogleSignIn = true
                };
                
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create user account: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return AuthenticateResult.Fail("Failed to create user account");
                }
                
                // Ensure UNOPS_GEN_USER role exists
                if (!await _roleManager.RoleExistsAsync("UNOPS_GEN_USER"))
                {
                    await _roleManager.CreateAsync(new PAOIdentityRole { Name = "UNOPS_GEN_USER" });
                }
                
                // Assign UNOPS_GEN_USER role
                await _userManager.AddToRoleAsync(user, "UNOPS_GEN_USER");
                                
                // Assign default role if needed
                if (!string.IsNullOrEmpty(Options.DefaultRole))
                {
                    if (!await _roleManager.RoleExistsAsync(Options.DefaultRole))
                    {
                        await _roleManager.CreateAsync(new PAOIdentityRole { Name = Options.DefaultRole });
                    }
                    
                    await _userManager.AddToRoleAsync(user, Options.DefaultRole);
                }
                
                // Assign domain-specific roles
                await AssignDomainSpecificRolesAsync(user);
            }
            else
            {
                _logger.LogWarning("User not found and auto-provisioning is disabled: {Email}", userEmail);
                return AuthenticateResult.Fail("User not found");
            }
        }
        else
        {
            // For existing users, ensure they have UNOPS_GEN_USER role
            if (!await _roleManager.RoleExistsAsync("UNOPS_GEN_USER"))
            {
                await _roleManager.CreateAsync(new PAOIdentityRole { Name = "UNOPS_GEN_USER" });
            }
            
            if (!await _userManager.IsInRoleAsync(user, "UNOPS_GEN_USER"))
            {
                await _userManager.AddToRoleAsync(user, "UNOPS_GEN_USER");
            }
        }

        // Process IAP groups if available
        await ProcessGroupsAsync(user);

        // Log authenticated user details before impersonation
        var authenticatedUserRoles = await _userManager.GetRolesAsync(user);
        _logger.LogInformation("🔍 [AUTH] Authenticated user: {Email}, Roles: {Roles}",
            user.Email ?? "", string.Join(", ", authenticatedUserRoles));

        // Handle user impersonation if enabled and requested
        PAOIdentityUser effectiveUser = user;
        string authenticatedUserEmail = user.Email ?? "";
        bool isImpersonating = false;
        
        // Diagnostic logging for impersonation setup
        _logger.LogInformation("🔍 [IMPERSONATION-CHECK] EnableImpersonation={EnableImpersonation}, HeaderName={HeaderName}, AuthenticatedUser={AuthUser}", 
            Options.EnableImpersonation, Options.ImpersonationHeaderName, user.Email);
        _logger.LogInformation("🔍 [IMPERSONATION-CHECK] Request headers: {Headers}", 
            string.Join(", ", Request.Headers.Select(h => $"{h.Key}={h.Value.ToString().Substring(0, Math.Min(50, h.Value.ToString().Length))}")));
        
        if (Options.EnableImpersonation && 
            Request.Headers.TryGetValue(Options.ImpersonationHeaderName, out var impersonatedEmailValues))
        {
            _logger.LogInformation("🔍 [IMPERSONATION-CHECK] Found impersonation header: {HeaderValue}", impersonatedEmailValues.ToString());
            var impersonatedEmail = impersonatedEmailValues.ToString()?.Trim();
            
            if (!string.IsNullOrEmpty(impersonatedEmail) && impersonatedEmail != user.Email)
            {
                // Check if user is trusted to impersonate
                bool isTrusted = Options.TrustedServiceAccounts?.Any(sa => sa.Equals(user.Email, StringComparison.OrdinalIgnoreCase)) == true;
                
                // In development mode, also trust the configured dev user for testing
                var devEnv = Context.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
                var devConfig = Context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
                if (devEnv?.IsDevelopment() == true)
                {
                    var configuredDevEmail = devConfig?.GetValue<string>("Development:IAPSimulation:UserEmail");
                    if (!string.IsNullOrEmpty(configuredDevEmail) && 
                        configuredDevEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        isTrusted = true;
                        _logger.LogDebug("🔧 [DEV-MODE] Allowing impersonation for configured dev user: {Email}", configuredDevEmail);
                    }
                }
                
                if (isTrusted)
                {
                    _logger.LogInformation("🔄 [IMPERSONATION] {AuthUser} requesting impersonation of {TargetUser}", 
                        user.Email, impersonatedEmail);
                    
                    // Look up the impersonated user (normalize email to ensure case-insensitive lookup)
                    var normalizedEmail = _userManager.NormalizeEmail(impersonatedEmail);
                    _logger.LogDebug("🔍 [IMPERSONATION-DEBUG] Looking up user: Original={OriginalEmail}, Normalized={NormalizedEmail}", 
                        impersonatedEmail, normalizedEmail);
                    
                    var impersonatedUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.ActiveUser);
                    
                    if (impersonatedUser != null)
                    {
                        effectiveUser = impersonatedUser;
                        isImpersonating = true;
                        var impersonatedUserRoles = await _userManager.GetRolesAsync(impersonatedUser);
                        _logger.LogInformation("✅ [IMPERSONATION] Successfully impersonating {ImpersonatedUser} (authenticated as {AuthUser}). Impersonated user has roles: {Roles}",
                            impersonatedEmail, authenticatedUserEmail, string.Join(", ", impersonatedUserRoles));
                    }
                    else
                    {
                        var serviceAccountRoles = await _userManager.GetRolesAsync(user);
                        _logger.LogWarning("⚠️ [IMPERSONATION] Impersonated user not found: {ImpersonatedEmail} (normalized: {NormalizedEmail}). Proceeding with service account '{ServiceAccount}' which has roles: {Roles}", 
                            impersonatedEmail, normalizedEmail, user.Email, string.Join(", ", serviceAccountRoles));
                    }
                }
                else
                {
                    _logger.LogWarning("🚫 [IMPERSONATION] User {UserEmail} is not in trusted service accounts list. Impersonation denied.",
                        user.Email);
                }
            }
            else
            {
                _logger.LogInformation("🔍 [IMPERSONATION-CHECK] Impersonation header present but empty or same as authenticated user: {ImpersonatedEmail} vs {AuthUser}", 
                    impersonatedEmail, user.Email);
            }
        }
        else
        {
            _logger.LogInformation("🔍 [IMPERSONATION-CHECK] Impersonation not attempted. EnableImpersonation={EnableImpersonation}, HeaderPresent={HeaderPresent}", 
                Options.EnableImpersonation, Request.Headers.ContainsKey(Options.ImpersonationHeaderName ?? ""));
        }

        // Get user roles and claims from the effective user (impersonated or original)
        var roles = await _userManager.GetRolesAsync(effectiveUser);
        var claims = await _userManager.GetClaimsAsync(effectiveUser);
        
        // Create identity with explicit authentication type
        var identity = new ClaimsIdentity(claims, "IAP", ClaimTypes.Name, ClaimTypes.Role);
        
        // Make sure all essential claims are present (use effectiveUser for permissions)
        if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, effectiveUser.Id.ToString()));
        
        if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
            identity.AddClaim(new Claim(ClaimTypes.Name, effectiveUser.UserName ?? ""));
        
        if (!identity.HasClaim(c => c.Type == ClaimTypes.Email))
            identity.AddClaim(new Claim(ClaimTypes.Email, effectiveUser.Email ?? ""));
        
        if (!identity.HasClaim(c => c.Type == "IsInternal"))
            identity.AddClaim(new Claim("IsInternal", effectiveUser.IsInternal.ToString()));
        
        // Add impersonation audit claims if applicable
        if (isImpersonating)
        {
            identity.AddClaim(new Claim("IsImpersonating", "true"));
            identity.AddClaim(new Claim("AuthenticatedServiceAccount", authenticatedUserEmail ?? ""));
            identity.AddClaim(new Claim("ImpersonatedUser", effectiveUser.Email ?? ""));
            _logger.LogInformation("🔐 [IMPERSONATION-AUDIT] Request authenticated as {ServiceAccount}, acting as {ImpersonatedUser}",
                authenticatedUserEmail, effectiveUser.Email);
        }
        
        // Add IAPAuthenticated claim if not present
        if (!claims.Any(c => c.Type == "IAPAuthenticated"))
        {
            var iapAuthClaim = new Claim("IAPAuthenticated", "true");
            await _userManager.AddClaimAsync(user, iapAuthClaim);
            identity.AddClaim(iapAuthClaim);
        }
        else
        {
            identity.AddClaim(new Claim("IAPAuthenticated", "true"));
        }
        
        // Add role claims
        foreach (var role in roles)
        {
            if (!identity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == role))
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        
        // If we have a valid Bearer token, merge its claims
        if (bearerPrincipal != null)
        {
            foreach (var claim in bearerPrincipal.Claims)
            {
                if (!identity.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    identity.AddClaim(claim);
                }
            }
        }
        
        // Store authentication in cookie for development mode
        var hostEnv = Context.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
        var appConfig = Context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        
        if (hostEnv?.IsDevelopment() == true && 
            appConfig?.GetValue<bool>("Development:IAPSimulation:Enabled", false) == true)
        {
            // Set a dev auth cookie to persist authentication (use effective user for consistency)
            Response.Cookies.Append("DevIAPAuth", effectiveUser.Email ?? "", new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = DateTimeOffset.Now.AddHours(8)
            });
        }
        
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
    
    private async Task<bool> ValidateIapJwtAsync()
    {
        // Check for development simulation flag first
        if (Request.Headers.TryGetValue("X-Dev-IAP-Simulation", out _))
        {
            _logger.LogInformation("Development IAP simulation flag found, skipping JWT validation");
            return true;
        }
        
        // Check for dev auth cookie in development
        var hostingEnv = Context.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
        var appConfiguration = Context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        
        if (hostingEnv?.IsDevelopment() == true)
        {
            // Skip validation in development if configured
            if (appConfiguration?.GetValue<bool>("Development:IAPSimulation:SkipValidationInDevelopment", false) == true)
            {
                _logger.LogInformation("Skipping IAP JWT validation in development environment");
                return true;
            }
            
            // Check for dev auth cookie - if present, we're in dev mode
            if (appConfiguration?.GetValue<bool>("Development:IAPSimulation:Enabled", false) == true &&
                Request.Cookies.ContainsKey("DevIAPAuth"))
            {
                _logger.LogInformation("Dev auth cookie found, skipping JWT validation");
                return true;
            }
        }
        
        // Allow header fallback if configured
        if (Options.AllowHeaderFallback && !Request.Headers.TryGetValue("X-Goog-IAP-JWT-Assertion", out var _))
        {
            _logger.LogWarning("No IAP JWT header found, but AllowHeaderFallback is enabled");
            return true;
        }
        
        // Primary Authentication: JWT Verification
        ClaimsPrincipal? jwtPrincipal = null;

        if (Request.Headers.TryGetValue("X-Goog-IAP-JWT-Assertion", out var jwtHeaderValues))
        {
            var jwt = jwtHeaderValues.ToString();
            try
            {
                jwtPrincipal = await VerifyIapJwtAndGetPrincipalAsync(jwt);
                if (jwtPrincipal != null)
                {
                    var verifiedEmail = jwtPrincipal.FindFirstValue(ClaimTypes.Email);
                    _logger.LogDebug("Successfully verified JWT for user: {Email}", verifiedEmail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JWT verification failed");
            }
        }
        else
        {
            _logger.LogDebug("No JWT header found");
        }
        
        // If we reached here, JWT verification failed or no JWT was present
        return false;
    }
    
    private async Task<ClaimsPrincipal?> VerifyIapJwtAndGetPrincipalAsync(string jwt)
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
        
        // Generate all possible audience strings based on configuration
        var audiences = new List<string>();
        
        // For backend services
        if (!string.IsNullOrEmpty(Options.ProjectNumber) && 
            !string.IsNullOrEmpty(Options.BackendServiceId))
        {
            audiences.Add($"/projects/{Options.ProjectNumber}/global/backendServices/{Options.BackendServiceId}");
        }
        
        _logger.LogDebug("Will try JWT validation with audiences: {Audiences}", string.Join(", ", audiences));
        
        // Try each audience format
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
                    ValidIssuer = "https://cloud.google.com/iap",
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
        string? email = null;
        
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
            var subValue = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (!string.IsNullOrEmpty(subValue) && subValue.Contains("@"))
            {
                email = subValue;
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
                var claimValue = claim.Value;
                if (!string.IsNullOrEmpty(claimValue) && claimValue.Contains("@") && claimValue.Contains("."))
                {
                    email = claimValue;
                    _logger.LogDebug("Found potential email in claim {ClaimType}: {Email}", claim.Type, email);
                    break;
                }
            }
        }
        
        // Check if we need to fall back to IAP header
        if (string.IsNullOrEmpty(email) && Request.Headers.TryGetValue("x-goog-authenticated-user-email", out var emailHeaderValues))
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
        if (identity == null)
        {
            throw new SecurityTokenException("Validated principal has no ClaimsIdentity");
        }

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
                identity.AddClaim(new Claim(claim.Type, claim.Value ?? ""));
            }
        }
        
        return validatedPrincipal;
    }
    
    private async Task<JsonWebKey> GetPublicKeyAsync(string kid)
    {
        // Check if we have a cached key
        if (_cachedKeys.TryGetValue(kid, out var cachedKey))
        {
            return cachedKey;
        }
        
        // If keys are old, refresh them
        if (_keysLastRefreshed.AddHours(1) < DateTime.UtcNow)
        {
            await RefreshPublicKeysAsync();
            
            // Check again after refresh
            if (_cachedKeys.TryGetValue(kid, out cachedKey))
            {
                return cachedKey;
            }
        }
        
        throw new SecurityTokenException($"No public key found for kid: {kid}");
    }

    private static readonly Dictionary<string, JsonWebKey> _cachedKeys = new Dictionary<string, JsonWebKey>();
    private static DateTime _keysLastRefreshed = DateTime.MinValue;
    private static readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
    private static readonly string PUBLIC_KEY_URL = "https://www.gstatic.com/iap/verify/public_key-jwk";

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
            
            var client = new HttpClient();
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
    
    private async Task AssignDomainSpecificRolesAsync(PAOIdentityUser user)
    {
        if (string.IsNullOrEmpty(user.Email))
        {
            return;
        }
        
        // Get domain from email
        string domain = user.Email.Substring(user.Email.IndexOf('@') + 1);
        
        // Check if we have a mapping for this domain
        if (Options.DomainRoles != null && Options.DomainRoles.TryGetValue(domain, out var domainRole))
        {
            if (!await _roleManager.RoleExistsAsync(domainRole))
            {
                await _roleManager.CreateAsync(new PAOIdentityRole { Name = domainRole });
            }
            await _userManager.AddToRoleAsync(user, domainRole);
            
            // Record internal status based on domain
            user.IsInternal = domainRole == "Internal";
            await _userManager.UpdateAsync(user);
        }
        
        // Check for special indicators in email (for admin, etc.)
        if (Options.ExternalRoleMappings != null)
        {
            foreach (var mapping in Options.ExternalRoleMappings)
            {
                string indicator = mapping.Key.ToLower();
                string role = mapping.Value;
                
                if (user.Email.ToLower().Contains(indicator))
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new PAOIdentityRole { Name = role });
                    }
                    
                    if (!await _userManager.IsInRoleAsync(user, role))
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                }
            }
        }
        
        // Default to External role if no domain match (and not already Internal)
        if (!user.IsInternal && !await _userManager.IsInRoleAsync(user, "External"))
        {
            const string externalRole = "External";
            if (!await _roleManager.RoleExistsAsync(externalRole))
            {
                await _roleManager.CreateAsync(new PAOIdentityRole { Name = externalRole });
            }
            await _userManager.AddToRoleAsync(user, externalRole);
        }
    }
    
    private async Task ProcessGroupsAsync(PAOIdentityUser user)
    {
        // Process IAP groups if provided
        if (Request.Headers.TryGetValue("X-Goog-Authenticated-User-Groups", out var groupValues))
        {
            var groups = groupValues.ToString()
                .Split(',')
                .Select(g => g.Split(':').Last())
                .ToList();
                
            // Add groups as claims
            foreach (var group in groups)
            {
                var claim = new Claim("Group", group);
                if (!(await _userManager.GetClaimsAsync(user)).Any(c => c.Type == "Group" && c.Value == group))
                {
                    await _userManager.AddClaimAsync(user, claim);
                }
            }
            
            // Map groups to roles based on configuration
            if (Options.ExternalGroupMappings != null)
            {
                foreach (var group in groups)
                {
                    if (Options.ExternalGroupMappings.TryGetValue(group, out var role))
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                        {
                            await _roleManager.CreateAsync(new PAOIdentityRole { Name = role });
                        }
                        
                        if (!await _userManager.IsInRoleAsync(user, role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }
            }
        }
    }
}

public class IAPAuthenticationOptions : AuthenticationSchemeOptions
{
    public bool AutoProvisionUsers { get; set; } = true;
    public string DefaultRole { get; set; } = "User";
    public bool RequireJwtVerification { get; set; } = true;
    public bool AllowHeaderFallback { get; set; } = false;
    public string ProjectNumber { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string BackendServiceId { get; set; } = string.Empty;
    public string HealthCheckPath { get; set; } = "/health";
    public string Region { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    
    // Domain-specific role mappings (e.g., unops.org -> Internal)
    public Dictionary<string, string> DomainRoles { get; set; } = new();
    
    // Map IAP user attribute values to roles (e.g., admin -> Administrator)
    public Dictionary<string, string> ExternalRoleMappings { get; set; } = new();
    
    // Map IAP group names to roles (e.g., unops-admins -> Administrator)
    public Dictionary<string, string> ExternalGroupMappings { get; set; } = new();
    
    // User impersonation settings
    public bool EnableImpersonation { get; set; } = false;
    public List<string> TrustedServiceAccounts { get; set; } = new();
    public string ImpersonationHeaderName { get; set; } = "x-unops-impersonated-user";
} 