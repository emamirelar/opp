using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace UNOPS.PAO.Server.Infrastructure;

public class DevelopmentLoginPageMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DevelopmentLoginPageMiddleware> _logger;

    public DevelopmentLoginPageMiddleware(
        RequestDelegate next, 
        IWebHostEnvironment environment,
        ILogger<DevelopmentLoginPageMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_environment.IsDevelopment() || !context.Request.Path.StartsWithSegments("/dev-login"))
        {
            await _next(context);
            return;
        }

        _logger.LogInformation("Serving development login page");
        context.Response.ContentType = "text/html";

        // Check if a user email was provided in the query string (for direct login)
        if (context.Request.Query.TryGetValue("user", out var email))
        {
            _logger.LogInformation("Direct login requested for: {Email}", email);
            
            // Clear any existing cookies before setting new ones
            foreach (var cookie in context.Request.Cookies.Keys)
            {
                context.Response.Cookies.Delete(cookie);
            }
            
            // Set the cookie directly
            context.Response.Cookies.Append("dev-user-email", email, new CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(7)
            });
            
            // Set the DevIAPAuth cookie that our IAP handler will use
            context.Response.Cookies.Append("DevIAPAuth", email, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(7)
            });
            
            // Instead of calling an API, directly set the content with client-side redirect
            var redirectHtml = new StringBuilder();
            redirectHtml.AppendLine("<!DOCTYPE html>");
            redirectHtml.AppendLine("<html><head><title>Development Login</title>");
            redirectHtml.AppendLine("<script>");
            redirectHtml.AppendLine("// Clear all storage to start fresh");
            redirectHtml.AppendLine("localStorage.clear();");
            redirectHtml.AppendLine("sessionStorage.clear();");
            redirectHtml.AppendLine("// Set redirect flag");
            redirectHtml.AppendLine("localStorage.setItem('iap_redirect_handled', 'true');");
            
            // Add cookie verification
            redirectHtml.AppendLine("// Verify cookie was set properly");
            redirectHtml.AppendLine("function checkCookie() {");
            redirectHtml.AppendLine("  const hasDevUserEmail = document.cookie.split(';').some(c => c.trim().startsWith('dev-user-email='));");
            redirectHtml.AppendLine($"  console.log('Checking for dev-user-email cookie for {email}:', hasDevUserEmail);");
            redirectHtml.AppendLine("  console.log('All cookies:', document.cookie);");
            redirectHtml.AppendLine("  return hasDevUserEmail;");
            redirectHtml.AppendLine("}");
            
            // Try to set the cookie directly in JavaScript as a fallback
            redirectHtml.AppendLine("// Backup method to set cookie if not found");
            redirectHtml.AppendLine("function setCookieIfNeeded() {");
            redirectHtml.AppendLine("  if (!checkCookie()) {");
            redirectHtml.AppendLine($"    console.log('Cookie not found, setting manually');");
            redirectHtml.AppendLine($"    document.cookie = 'dev-user-email={email};path=/;max-age=604800;';");
            redirectHtml.AppendLine($"    document.cookie = 'DevIAPAuth={email};path=/;max-age=604800;';");
            redirectHtml.AppendLine("    return checkCookie();");
            redirectHtml.AppendLine("  }");
            redirectHtml.AppendLine("  return true;");
            redirectHtml.AppendLine("}");
            
            // Redirect with verification
            redirectHtml.AppendLine("// Redirect with verification");
            redirectHtml.AppendLine("function redirectToHome() {");
            redirectHtml.AppendLine("  const cookieSet = setCookieIfNeeded();");
            redirectHtml.AppendLine("  if (cookieSet) {");
            redirectHtml.AppendLine("    console.log('Cookie confirmed, redirecting to home page...');");
            redirectHtml.AppendLine("    window.location.href = '/?ts=' + new Date().getTime();");
            redirectHtml.AppendLine("  } else {");
            redirectHtml.AppendLine("    console.error('Cookie could not be set! Authentication will fail.');");
            redirectHtml.AppendLine("    document.getElementById('error-message').style.display = 'block';");
            redirectHtml.AppendLine("  }");
            redirectHtml.AppendLine("}");
            
            // Log info and redirect
            redirectHtml.AppendLine($"console.log('Development login successful for: {email}');");
            redirectHtml.AppendLine("console.log('Cookies:', document.cookie);");
            redirectHtml.AppendLine("// Redirect after a delay to ensure cookies take effect");
            redirectHtml.AppendLine("setTimeout(redirectToHome, 1000);");
            
            redirectHtml.AppendLine("</script>");
            redirectHtml.AppendLine("<style>");
            redirectHtml.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; text-align: center; }");
            redirectHtml.AppendLine(".spinner { margin: 20px auto; width: 50px; height: 50px; border: 3px solid #f3f3f3; border-top: 3px solid #3498db; border-radius: 50%; animation: spin 1s linear infinite; }");
            redirectHtml.AppendLine("@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }");
            redirectHtml.AppendLine("#error-message { background: #f8d7da; color: #721c24; padding: 10px; border-radius: 4px; margin: 10px auto; max-width: 80%; display: none; }");
            redirectHtml.AppendLine("</style>");
            redirectHtml.AppendLine("</head><body>");
            redirectHtml.AppendLine("<h1>Development Login Successful</h1>");
            redirectHtml.AppendLine($"<p>Logged in as: <strong>{email}</strong></p>");
            redirectHtml.AppendLine("<p>Redirecting to home page...</p>");
            redirectHtml.AppendLine("<div class='spinner'></div>");
            redirectHtml.AppendLine("<div id='error-message'>Error: Cookie could not be set. Try disabling any browser privacy features or manually go to the homepage.</div>");
            redirectHtml.AppendLine("</body></html>");
            
            await context.Response.WriteAsync(redirectHtml.ToString());
            return;
        }

        // Standard login page HTML with hardcoded test users
        await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <title>Development Login</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; max-width: 800px; margin: 0 auto; padding: 20px; }
        .user-box { border: 1px solid #ddd; padding: 15px; margin: 15px 0; cursor: pointer; border-radius: 4px; }
        .user-box:hover { background-color: #f5f5f5; }
        h1 { color: #333; }
        h3 { margin: 5px 0; color: #0066cc; }
        .role { display: inline-block; background: #e1ecf4; color: #39739d; padding: 2px 8px; margin: 3px; border-radius: 3px; }
        button { background: #0095ff; color: white; border: none; padding: 10px 15px; border-radius: 4px; cursor: pointer; }
        button:hover { background: #0077cc; }
        .info { background: #e1ecf4; padding: 15px; border-radius: 4px; margin: 20px 0; }
        #error-message { color: red; display: none; }
    </style>
    <script>
        // Function to directly set cookie and redirect
        function loginAs(email) {
            console.log('Logging in as:', email);
            document.getElementById('error-message').style.display = 'none';
            
            try {
                // First clear browser storage
                localStorage.clear();
                sessionStorage.clear();
                
                // Clear all cookies
                document.cookie.split(';').forEach(function(c) {
                    document.cookie = c.trim().split('=')[0] + '=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;';
                });
                
                // Wait a moment to ensure everything is cleared
                setTimeout(() => {
                    // Redirect to login with the selected user
                    window.location.href = `/dev-login?user=${encodeURIComponent(email)}`;
                }, 100);
            } catch (error) {
                console.error('Login failed:', error);
                document.getElementById('error-message').style.display = 'block';
                document.getElementById('error-message').innerText = `Login error: ${error.message}`;
            }
        }
        
        window.onload = function() {
            // Clear storages on page load
            localStorage.clear();
            sessionStorage.clear();
            
            // Show the predefined users
            renderPredefinedUsers();
        };
        
        function renderPredefinedUsers() {
            const users = [
                { email: 'anushas@unops.org', roles: ['Internal'] },
                { email: 'admin@unops.org', roles: ['Administrator'] },
                { email: 'partner@partner.org', roles: ['Partner'] },
                { email: 'external@unops.org', roles: ['External'] },
                { email: 'partnerexternal@unops.org', roles: ['Partner', 'External'] }
            ];
            
            const container = document.getElementById('users-container');
            container.innerHTML = '';
            
            users.forEach(user => {
                const div = document.createElement('div');
                div.className = 'user-box';
                div.onclick = () => loginAs(user.email);
                
                let rolesHtml = '';
                if (user.roles && user.roles.length) {
                    rolesHtml = user.roles.map(role => 
                        `<span class='role'>${role}</span>`).join('');
                }
                
                div.innerHTML = `
                    <h3>${user.email}</h3>
                    <div>${rolesHtml}</div>
                `;
                container.appendChild(div);
            });
        }
    </script>
</head>
<body>
    <h1>Development Login</h1>
    <div class='info'>
        <p>Select a test user to login. Each user has specific roles for testing permissions.</p>
    </div>
    
    <div id='error-message'></div>
    
    <div id='users-container'>
        <!-- Test users will be rendered here -->
    </div>
    
    <div class='info'>
        <h3>Available Test Users:</h3>
        <p><strong>admin@unops.org</strong> - Has Administrator role</p>
        <p><strong>anushas@unops.org</strong> - Has Internal role</p>
        <p><strong>partner@partner.org</strong> - Has Partner role</p>
        <p><strong>external@unops.org</strong> - Has External role</p>
        <p><strong>partnerexternal@unops.org</strong> - Has both Partner and External roles</p>
    </div>
</body>
</html>
        ");
    }
} 