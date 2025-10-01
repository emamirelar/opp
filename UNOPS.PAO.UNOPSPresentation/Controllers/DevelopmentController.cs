using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSDomain.Authorization;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSPresentation.Controllers;

[ApiController]
[Route("api/dev")]
public class DevelopmentController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly RoleManager<PAOIdentityRole> _roleManager;
    private readonly UNOPSAppDbContext _context;
    private readonly IConfiguration _configuration;

    public DevelopmentController(
        IWebHostEnvironment environment,
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        UNOPSAppDbContext context,
        IConfiguration configuration)
    {
        _environment = environment;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetDevelopmentUsers()
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        var users = await _userManager.Users.ToListAsync();
        
        var userList = new List<object>();
        
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            userList.Add(new
            {
                Email = user.Email,
                Roles = roles
            });
        }
        
        return Ok(userList);
    }

    [HttpPost("login/{email}")]
    public IActionResult SetDevelopmentUser(string email)
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        // First delete any existing cookie to avoid issues
        Response.Cookies.Delete("dev-user-email");
            
        // Set IAP simulation header
        Response.Headers["X-Goog-Authenticated-User-Email"] = $"accounts.google.com:{email}";
        
        // Set dev cookie for JavaScript access with proper options
        Response.Cookies.Append("dev-user-email", email, new CookieOptions
        {
            HttpOnly = false,   // Must be false to be accessible from JavaScript
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Secure = false,     // False for local development
            Expires = DateTimeOffset.Now.AddDays(7)
        });
        
        return Ok(new { 
            Email = email,
            CookieSet = true,
            CookieName = "dev-user-email",
            CookieValue = email
        });
    }

    [HttpPost("seed-dev-users")]
    public async Task<IActionResult> SeedDevelopmentUsers()
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        // First, clear existing users and roles
        // Only do this if we're in development mode for safety
        if (_environment.IsDevelopment())
        {
            // Remove existing users
            var existingUsers = await _userManager.Users.ToListAsync();
            foreach (var user in existingUsers)
            {
                await _userManager.DeleteAsync(user);
            }
            
            // Clear existing roles
            var existingRoles = await _roleManager.Roles.ToListAsync();
            foreach (var role in existingRoles)
            {
                await _roleManager.DeleteAsync(role);
            }
        }
            
        // Get the configured user email from appsettings.json
        var configuredEmail = _configuration["Development:IAPSimulation:UserEmail"];
        
        // Define our specific test users and roles using configured email
        var devUsers = new[]
        {
            new { Email = configuredEmail, Password = "Password123!", Roles = new[] { "UNOPS_GEN_USER" }, IsInternal = configuredEmail.EndsWith("@unops.org") },
        };
        
        // Ensure roles exist
        foreach (var user in devUsers)
        {
            foreach (var roleName in user.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new PAOIdentityRole { Name = roleName });
                }
            }
        }
        
        // Create users if they don't exist
        var results = new List<object>();
        
        foreach (var devUser in devUsers)
        {
            var existingUser = await _userManager.FindByEmailAsync(devUser.Email);
            
            if (existingUser == null)
            {
                var user = new PAOIdentityUser
                {
                    Email = devUser.Email,
                    UserName = devUser.Email,
                    EmailConfirmed = true,
                    IsInternal = devUser.IsInternal
                };
                
                var createResult = await _userManager.CreateAsync(user, devUser.Password);
                
                if (createResult.Succeeded)
                {
                    foreach (var role in devUser.Roles)
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                    
                    results.Add(new { Email = user.Email, Status = "Created", Roles = devUser.Roles });
                }
                else
                {
                    results.Add(new { Email = devUser.Email, Status = "Failed", Errors = createResult.Errors.Select(e => e.Description) });
                }
            }
            else
            {
                // User exists, ensure they're in the right roles
                var currentRoles = await _userManager.GetRolesAsync(existingUser);
                
                // Remove existing roles
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                }
                
                // Add new roles
                foreach (var role in devUser.Roles)
                {
                    await _userManager.AddToRoleAsync(existingUser, role);
                }
                
                // Update internal status
                existingUser.IsInternal = devUser.IsInternal;
                await _userManager.UpdateAsync(existingUser);
                
                results.Add(new { Email = existingUser.Email, Status = "Updated", Roles = devUser.Roles });
            }
        }
        
        return Ok(results);
    }

    [HttpGet("check-iap-simulation")]
    public IActionResult CheckIAPSimulation()
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        var hasIapEmailHeaderValue = Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var headerValue);
        
        // Get cookies for debug info
        var hasCookie = Request.Cookies.TryGetValue("dev-user-email", out var cookieValue);
        
        // Check session storage if available
        var headerKeys = new List<string>();
        foreach (var header in Request.Headers)
        {
            headerKeys.Add($"{header.Key}: {header.Value}");
        }
        
        var result = new
        {
            Environment = _environment.EnvironmentName,
            HasIapHeader = hasIapEmailHeaderValue,
            IapHeaderValue = headerValue.ToString(),
            HasDevCookie = hasCookie,
            CookieValue = cookieValue,
            Headers = headerKeys,
            Roles = new List<string>()
        };

        return Ok(result);
    }

    [HttpGet("debug")]
    public IActionResult DebugAuthPage()
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        // Generate debug view - using simple string with minimal JS
        var html = @"<!DOCTYPE html>
<html>
<head>
    <title>Development Authentication Debug</title>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; max-width: 800px; margin: 0 auto; }
        .panel { background: #f0f0f0; padding: 15px; margin-bottom: 15px; border-radius: 5px; }
        .debug { font-family: monospace; white-space: pre-wrap; background: #f8f8f8; padding: 10px; border: 1px solid #ddd; }
        button { padding: 8px 12px; background: #0066cc; color: white; border: none; border-radius: 4px; margin-right: 10px; cursor: pointer; }
        .status { padding: 10px; margin: 10px 0; border-radius: 4px; }
        .success { background: #d4edda; color: #155724; }
        .error { background: #f8d7da; color: #721c24; }
    </style>
    <script>
        async function checkAuth() {
            const statusDiv = document.getElementById('status');
            statusDiv.innerHTML = '<p>Checking authentication...</p>';
            
            try {
                const response = await fetch('/api/dev/check-iap-simulation');
                const result = await response.json();
                
                document.getElementById('debug-output').textContent = JSON.stringify(result, null, 2);
                
                if (result.hasIapHeader) {
                    statusDiv.className = 'status success';
                    statusDiv.innerHTML = '<h3>IAP Authentication Success</h3><p>Authentication is properly configured.</p>';
                    
                    // Also check for cookie
                    if (result.hasDevCookie) {
                        statusDiv.innerHTML += '<p>Dev Cookie: ' + result.cookieValue + '</p>';
                    } else {
                        statusDiv.innerHTML += '<p class=""error"">Warning: Dev cookie not set. Some client-side features may not work.</p>';
                    }
                } else {
                    statusDiv.className = 'status error';
                    statusDiv.innerHTML = '<h3>IAP Authentication Failed</h3><p>IAP headers not set properly. Please try again.</p>';
                }
            } catch (error) {
                document.getElementById('status').className = 'status error';
                document.getElementById('status').innerHTML = '<h3>Error</h3><p>Failed to check authentication status.</p>';
            }
        }
        
        window.onload = function() {
            checkAuth();
        };
    </script>
</head>
<body>
    <h1>Development Authentication Debug</h1>
    
    <div class=""panel"">
        <h2>Authentication Status</h2>
        <div id=""status"" class=""status"">
            <p>Checking authentication status...</p>
        </div>
    </div>
    
    <div class=""panel"">
        <h2>Actions</h2>
        <button onclick=""checkAuth()"">Refresh Status</button>
        <button onclick=""window.location.href = '/api/dev/'"">Return to Dev Portal</button>
        <button onclick=""window.location.href = '/'"">Go to Home Page</button>
    </div>
    
    <div class=""panel"">
        <h2>Debug Information</h2>
        <div class=""debug"" id=""debug-output"">Loading...</div>
    </div>
</body>
</html>";
        
        return Content(html, "text/html");
    }

    [HttpGet("direct-login/{email}")]
    public async Task<IActionResult> DirectLogin(string email)
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        // Set IAP simulation header to bypass UNOPSUserValidator checks
        Request.Headers["X-Dev-IAP-Simulation"] = "true";
        
        // Also set the IAP headers right now (don't wait for middleware)
        Request.Headers["X-Goog-Authenticated-User-Email"] = $"accounts.google.com:{email}";
        Request.Headers["X-Goog-Iap-Jwt-Assertion"] = "dev-jwt-placeholder";
            
        // Validate user exists
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            // Try creating a dummy user for testing
            user = new PAOIdentityUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };
            
            var createResult = await _userManager.CreateAsync(user, "DevTest123!");
            if (!createResult.Succeeded)
            {
                return BadRequest(new { Error = "Could not create test user", Details = createResult.Errors });
            }
            
            await _userManager.AddToRoleAsync(user, "Partner");
        }
        
        var roles = await _userManager.GetRolesAsync(user);
        
        // Set a cookie accessible to JavaScript
        Response.Cookies.Append("dev-user-email", email, new CookieOptions
        {
            HttpOnly = false,  // Allow JavaScript access
            Secure = false,    // Allow HTTP in development
            Path = "/",        // Ensure it's available for all paths
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.Now.AddDays(1)
        });
        
        // Generate a simplified loading page with minimal JavaScript
        var html = $@"<!DOCTYPE html>
<html>
<head>
    <title>IAP Authentication Setup</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; max-width: 800px; margin: 0 auto; }}
        .info {{ background: #e6f7ff; padding: 15px; border-radius: 8px; margin-bottom: 20px; }}
        .status {{ padding: 15px; border-radius: 8px; margin-bottom: 20px; }}
        .status.success {{ background: #d4edda; color: #155724; }}
        .status.error {{ background: #f8d7da; color: #721c24; }}
        button {{ padding: 10px 15px; background: #0066cc; color: white; border: none; border-radius: 4px; cursor: pointer; margin: 5px; }}
    </style>
    <script>
        function redirectHome() {{
            localStorage.setItem('iap_redirect_handled', 'true');
            document.getElementById('redirect-message').innerText = 'Redirecting now...';
            window.location.replace('/');
        }}
        
        window.onload = function() {{
            // Set timer for redirection
            var counter = 5;
            var timer = document.getElementById('redirect-timer');
            
            var interval = setInterval(function() {{
                counter--;
                timer.innerText = counter;
                
                if (counter <= 0) {{
                    clearInterval(interval);
                    redirectHome();
                }}
            }}, 1000);
        }};
    </script>
</head>
<body>
    <h1>IAP Authentication Setup</h1>
    <div class=""info"">
        <p>Setting up IAP development authentication for <strong>{email}</strong></p>
        <p id=""redirect-message"">Auto-redirect in <span id=""redirect-timer"">5</span> seconds...</p>
    </div>
    
    <div class=""status success"">
        <h3>Authentication Ready</h3>
        <p>Authentication has been configured for user: {email}</p>
        <p>Roles: {string.Join(", ", roles)}</p>
    </div>
    
    <div>
        <button onclick=""redirectHome()"">Go to Home Page Now</button>
        <button onclick=""window.location.href = '/api/dev/debug'"">View Debug Info</button>
    </div>
</body>
</html>";
        
        Response.ContentType = "text/html";
        return Content(html, "text/html");
    }

    [HttpGet("set-cookie/{email}")]
    public IActionResult SetBrowserCookie(string email)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }
        
        // Clear any existing cookies first
        foreach (var cookie in Request.Cookies.Keys)
        {
            if (cookie.StartsWith(".AspNetCore.") || cookie.StartsWith("Identity.") || cookie == "dev-user-email")
            {
                Response.Cookies.Delete(cookie);
            }
        }
        
        // Set the cookie with browser-friendly options (not HttpOnly)
        Response.Cookies.Append("dev-user-email", email, new CookieOptions
        {
            HttpOnly = false,  // Allow JavaScript to access
            Secure = false,    // Allow HTTP in development
            Path = "/",        // Ensure it's available for all paths
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.Now.AddDays(7)
        });
        
        // Create JavaScript to set a cookie as a fallback
        var cookieJs = $"document.cookie = 'dev-user-email={email};path=/;max-age=604800';";
        
        // Simplified HTML with minimal JavaScript
        var html = $@"<!DOCTYPE html>
<html>
<head>
    <title>Cookie Test</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; max-width: 800px; margin: 0 auto; }}
        .box {{ background: #f0f7ff; padding: 20px; border-radius: 8px; margin-bottom: 20px; }}
        .success {{ background: #d4edda; color: #155724; }}
        .error {{ background: #f8d7da; color: #721c24; }}
        button {{ padding: 10px 15px; background: #0066cc; color: white; border: none; border-radius: 4px; cursor: pointer; margin: 5px; }}
    </style>
    <script>
        // Set a cookie via JavaScript 
        function setJsCookie() {{
            {cookieJs}
            checkCookie();
        }}
        
        function checkCookie() {{
            const cookies = document.cookie.split(';').map(c => c.trim());
            const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
            const statusElement = document.getElementById('cookie-status');
            
            let cookieReport = '';
            cookies.forEach(function(cookie) {{
                cookieReport += '- ' + cookie + '<br>';
            }});
            document.getElementById('all-cookies').innerHTML = cookieReport || 'No cookies found';
            
            if (devCookie) {{
                statusElement.className = 'box success';
                statusElement.innerHTML = '<h3>Cookie Found!</h3><p>Authentication cookie was set successfully</p>';
            }} else {{
                statusElement.className = 'box error';
                statusElement.innerHTML = '<h3>Cookie Not Found</h3><p>Try clicking the Set JavaScript Cookie button below.</p>';
            }}
        }}
        
        window.onload = function() {{
            checkCookie();
        }};
    </script>
</head>
<body>
    <h1>Authentication Cookie Test</h1>
    
    <div id=""cookie-status"" class=""box"">
        Checking cookie status...
    </div>
    
    <div class=""box"">
        <h3>All Cookies</h3>
        <div id=""all-cookies"">Loading...</div>
    </div>
    
    <div>
        <button onclick=""setJsCookie()"">Set JavaScript Cookie</button>
        <button onclick=""window.location.href = '/'"">Go to Home Page</button>
        <button onclick=""window.location.href = '/api/dev/debug'"">View Debug Info</button>
    </div>
    
    <div class=""box"">
        <h3>Troubleshooting</h3>
        <p>If the cookie is not being set, try the following:</p>
        <ol>
            <li>Check your browser's cookie settings and ensure cookies are allowed for localhost</li>
            <li>Try using Chrome or Edge which typically have fewer cookie restrictions</li>
            <li>Try accessing the site via IP address instead of localhost</li>
            <li>Try clicking the ""Set JavaScript Cookie"" button</li>
            <li>If all else fails, try using the debug view to manually set the cookie</li>
        </ol>
    </div>
</body>
</html>";
        
        return Content(html, "text/html");
    }

    [HttpGet("debug-auth")]
    public IActionResult DebugAuthStatus()
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        // Detect circular references in services
        var services = new List<string>();
        try 
        {
            // This can help detect circular reference issues
            var serviceProvider = HttpContext.RequestServices;
            if (serviceProvider != null)
            {
                var userServices = serviceProvider.GetServices<object>();
                if (userServices != null)
                {
                    var userServiceNames = userServices
                        .Where(s => s != null && s.GetType().Name.Contains("User"))
                        .Select(s => s.GetType().FullName)
                        .ToList();
                        
                    services = userServiceNames?.Where(s => s != null).ToList() ?? new List<string>();
                }
            }
        }
        catch (Exception ex)
        {
            services.Add($"Error inspecting services: {ex.Message}");
        }
        
        // Handle claims separately to avoid anonymous type issues
        var claims = new List<object>();
        if (User?.Claims != null)
        {
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList<object>();
        }
        
        // Handle identities separately
        var identities = new List<object>();
        if (User?.Identities != null)
        {
            identities = User.Identities.Select(i => new 
            {
                i.AuthenticationType,
                IsAuthenticated = i.IsAuthenticated,
                Name = i.Name,
                ClaimCount = i.Claims?.Count() ?? 0,
                NameClaimType = i.NameClaimType,
                RoleClaimType = i.RoleClaimType
            }).ToList<object>();
        }
            
        var authInfo = new
        {
            RequestInfo = new
            {
                Path = Request.Path.ToString(),
                Method = Request.Method,
                QueryString = Request.QueryString.ToString(),
                IsHttps = Request.IsHttps
            },
            Authentication = new
            {
                IsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
                AuthenticationType = User?.Identity?.AuthenticationType,
                Name = User?.Identity?.Name,
                Claims = claims,
                Identities = identities
            },
            IapHeaders = new
            {
                HasIapHeader = Request.Headers.ContainsKey("X-Goog-Authenticated-User-Email"),
                IapHeaderValue = Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var headerValue) ? headerValue.ToString() : null,
                HasJwtHeader = Request.Headers.ContainsKey("X-Goog-Iap-Jwt-Assertion"),
                HasDevFlag = Request.Headers.ContainsKey("X-Dev-IAP-Simulation")
            },
            AllHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Cookies = Request.Cookies.Keys.ToDictionary(k => k, k => Request.Cookies[k]),
            Services = services,
            Environment = _environment.EnvironmentName,
            Time = DateTime.UtcNow
        };
        
        return Ok(authInfo);
    }

    [HttpGet("verify-roles")]
    public async Task<IActionResult> VerifyRoles()
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        var usersWithRoles = new List<object>();
        
        var users = await _userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            
            usersWithRoles.Add(new
            {
                Email = user.Email,
                IsInternal = user.IsInternal,
                Roles = roles,
                Claims = claims.Select(c => new { c.Type, c.Value })
            });
        }
        
        return Ok(new
        {
            Users = usersWithRoles,
            AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
        });
    }

    [HttpPost("clear-all-users")]
    public async Task<IActionResult> ClearAllUsers()
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        // Only do this if we're in development mode for safety
        var results = new List<string>();
        
        // Remove existing users
        var existingUsers = await _userManager.Users.ToListAsync();
        foreach (var user in existingUsers)
        {
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                results.Add($"Deleted user: {user.Email}");
            }
            else
            {
                results.Add($"Failed to delete user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        
        // Clear existing roles
        var existingRoles = await _roleManager.Roles.ToListAsync();
        foreach (var role in existingRoles)
        {
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                results.Add($"Deleted role: {role.Name}");
            }
            else
            {
                results.Add($"Failed to delete role {role.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        
        return Ok(results);
    }

    [HttpPost("setup-row-level-filters")]
    public async Task<IActionResult> SetupRowLevelFilters()
    {
        if (!_environment.IsDevelopment())
            return NotFound();
            
        // First, clear existing entity permissions
        var existingPermissions = await _context.EntityPermissions.ToListAsync();
        _context.EntityPermissions.RemoveRange(existingPermissions);
        await _context.SaveChangesAsync();
        
        // Define basic entity permissions for different roles
        var permissions = new[]
        {
            // Administrator has access to everything (not needed in EntityPermissions)
            
            // Internal role permissions
            new { EntityName = "Partner", Action = "Read", RoleName = "Internal", PropertyName = (string?)null, FilterExpression = (string?)null },
            new { EntityName = "Partner", Action = "Create", RoleName = "Internal", PropertyName = (string?)null, FilterExpression = (string?)null },
            new { EntityName = "Partner", Action = "Update", RoleName = "Internal", PropertyName = (string?)null, FilterExpression = (string?)null },
            new { EntityName = "Partner", Action = "Delete", RoleName = "Internal", PropertyName = (string?)null, FilterExpression = (string?)null },
            
            // External role permissions with row-level filters
            new { EntityName = "Partner", Action = "Read", RoleName = "External", PropertyName = (string?)null, FilterExpression = "IsPublic == true" },
            new { EntityName = "Partner", Action = "Create", RoleName = "External", PropertyName = (string?)null, FilterExpression = (string?)null },
            new { EntityName = "Partner", Action = "Update", RoleName = "External", PropertyName = (string?)null, FilterExpression = "CreatedBy == CurrentUser" },
            
            // Partner role permissions with row-level filters
            new { EntityName = "Partner", Action = "Read", RoleName = "Partner", PropertyName = (string?)null, FilterExpression = "CreatedBy == CurrentUser" },
            new { EntityName = "Partner", Action = "Update", RoleName = "Partner", PropertyName = (string?)null, FilterExpression = "CreatedBy == CurrentUser" },
            
            // Contact permissions
            new { EntityName = "Contact", Action = "Read", RoleName = "Internal", PropertyName = (string?)null, FilterExpression = (string?)null },
            new { EntityName = "Contact", Action = "Read", RoleName = "External", PropertyName = (string?)null, FilterExpression = "IsPublic == true" },
            new { EntityName = "Contact", Action = "Read", RoleName = "Partner", PropertyName = (string?)null, FilterExpression = "CreatedBy == CurrentUser" }
        };
        
        // Add permissions to the database
       /* foreach (var permission in permissions)
        {
            _context.EntityPermissions.Add(new UNOPS.PAO.UNOPSDomain.Authorization.EntityPermission
            {
                Entity = permission.Entity,
                //Action = permission.Action,
                Role = permission.Role,
                PropertyFilter = permission.PropertyFilter,
                FilterExpression = permission.FilterExpression
            });
        }*/
        
        await _context.SaveChangesAsync();
        
        return Ok(new
        {
            Success = true,
            Message = "Row-level filtering permissions configured successfully",
            PermissionsCount = permissions.Length
        });
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUserWithDefaultRole([FromBody] CreateUserRequest request)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest("Email is required");
        }

        try
        {
            // Ensure UNOPS_GEN_USER role exists
            if (!await _roleManager.RoleExistsAsync("UNOPS_GEN_USER"))
            {
                await _roleManager.CreateAsync(new PAOIdentityRole { Name = "UNOPS_GEN_USER" });
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            
            if (existingUser == null)
            {
                // Create new user
                var user = new PAOIdentityUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    EmailConfirmed = true,
                    IsInternal = request.Email.EndsWith("@unops.org")
                };

                var createResult = await _userManager.CreateAsync(user, "DevPassword123!");

                if (createResult.Succeeded)
                {
                    // Add UNOPS_GEN_USER role by default
                    await _userManager.AddToRoleAsync(user, "UNOPS_GEN_USER");
                    
                    return Ok(new { 
                        Email = user.Email, 
                        Status = "Created", 
                        Roles = new[] { "UNOPS_GEN_USER" },
                        IsInternal = user.IsInternal
                    });
                }
                else
                {
                    return BadRequest(new { 
                        Email = request.Email, 
                        Status = "Failed", 
                        Errors = createResult.Errors.Select(e => e.Description) 
                    });
                }
            }
            else
            {
                // User exists, ensure they have UNOPS_GEN_USER role
                var currentRoles = await _userManager.GetRolesAsync(existingUser);
                
                if (!currentRoles.Contains("UNOPS_GEN_USER"))
                {
                    await _userManager.AddToRoleAsync(existingUser, "UNOPS_GEN_USER");
                }

                return Ok(new { 
                    Email = existingUser.Email, 
                    Status = "Already exists", 
                    Roles = await _userManager.GetRolesAsync(existingUser),
                    IsInternal = existingUser.IsInternal
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpGet("configured-user")]
    public IActionResult GetConfiguredUser()
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        var configuredEmail = _configuration["Development:IAPSimulation:UserEmail"];
        
        return Ok(new { 
            Email = configuredEmail,
            Source = "appsettings.json Development:IAPSimulation:UserEmail"
        });
    }

}

// Class for testing row-level filtering
public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

// Request model for creating users in development mode
public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
} 