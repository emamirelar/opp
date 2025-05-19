namespace UNOPS.PAO.UNOPSIdentity.Authentication;

using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
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

    public IAPAuthenticationHandler(
        IOptionsMonitor<IAPAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager) 
        : base(options, logger, encoder, clock)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger.CreateLogger<IAPAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Skip authentication on the dev-login page
        if (Request.Path.StartsWithSegments("/dev-login"))
        {
            _logger.LogDebug("Skipping authentication on dev-login page");
            return AuthenticateResult.NoResult();
        }
        
        // Check if we already have a verified JWT from the middleware
        if (Context.User?.Identity?.IsAuthenticated == true && 
            Context.User.HasClaim(c => c.Type == "iap-jwt-verified" && c.Value == "true"))
        {
            _logger.LogDebug("Using existing JWT-verified identity from middleware");
            return AuthenticateResult.Success(new AuthenticationTicket(Context.User, Scheme.Name));
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
                        _logger.LogInformation("Using JWT-verified email: {Email}", userEmail);
                        goto ProcessUser; // Skip the header check
                    }
                }
            }
            catch
            {
                // JWT verification failed, continue to header-based auth
                _logger.LogDebug("JWT validation failed, falling back to header-based auth");
            }
        }
        
        // Extract IAP headers if JWT verification failed or was skipped
        if (!Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var userEmailValues))
        {
            _logger.LogDebug("No IAP email header found");
            
            // Check if we're in development mode and should use cookie auth as fallback
            var env = Context.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
            var config = Context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            
            if (env?.IsDevelopment() == true && 
                config?.GetValue<bool>("Development:IAPSimulation:Enabled", false) == true)
            {
                // Look for dev auth cookie
                if (Request.Cookies.TryGetValue("DevIAPAuth", out var emailFromCookie) && !string.IsNullOrEmpty(emailFromCookie))
                {
                    _logger.LogInformation("Using dev auth cookie for authentication: {Email}", emailFromCookie);
                    userEmailValues = new Microsoft.Extensions.Primitives.StringValues(emailFromCookie);
                }
                else
                {
                    return AuthenticateResult.NoResult();
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
        _logger.LogInformation("IAP email header found: {Email}", userEmail);
        
    ProcessUser:
        // Find or create user based on Google identity
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            // Auto-provision user if enabled
            if (Options.AutoProvisionUsers)
            {
                _logger.LogInformation("Auto-provisioning new user: {Email}", userEmail);
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

        // Process IAP groups if available
        await ProcessGroupsAsync(user);

        // Get user roles and claims
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        
        // Create identity with explicit authentication type - ensure it's not null or empty
        var identity = new ClaimsIdentity(claims, "IAP", ClaimTypes.Name, ClaimTypes.Role);
        
        // Make sure all essential claims are present
        if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        
        if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
        
        if (!identity.HasClaim(c => c.Type == ClaimTypes.Email))
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        
        if (!identity.HasClaim(c => c.Type == "IsInternal"))
            identity.AddClaim(new Claim("IsInternal", user.IsInternal.ToString()));
        
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
        
        // Store authentication in cookie for development mode
        var hostEnv = Context.RequestServices.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
        var appConfig = Context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        
        if (hostEnv?.IsDevelopment() == true && 
            appConfig?.GetValue<bool>("Development:IAPSimulation:Enabled", false) == true)
        {
            // Set a dev auth cookie to persist authentication
            Response.Cookies.Append("DevIAPAuth", userEmail, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = DateTimeOffset.Now.AddHours(8)
            });
        }
        
        // Log the authentication state
        _logger.LogInformation("IAP Authentication successful: {Email}, IsAuthenticated={IsAuthenticated}", 
            user.Email, identity.IsAuthenticated);
        
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
        bool jwtVerified = false;
        ClaimsPrincipal? jwtPrincipal = null;
        string? verifiedEmail = null;

        if (Request.Headers.TryGetValue("X-Goog-IAP-JWT-Assertion", out var jwtHeaderValues))
        {
            var jwt = jwtHeaderValues.ToString();
            try
            {
                jwtPrincipal = await VerifyIapJwtAndGetPrincipalAsync(jwt);
                if (jwtPrincipal != null)
                {
                    jwtVerified = true;
                    verifiedEmail = jwtPrincipal.FindFirstValue(ClaimTypes.Email);
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
} 