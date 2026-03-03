using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace UNOPS.PAO.Server.Infrastructure;

public class DevelopmentLoginPageMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DevelopmentLoginPageMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public DevelopmentLoginPageMiddleware(
        RequestDelegate next, 
        IWebHostEnvironment environment,
        ILogger<DevelopmentLoginPageMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
        _configuration = configuration;
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
            _logger.LogInformation("Direct login requested for: {Email}", email!);
            
            // Clear any existing cookies before setting new ones
            foreach (var cookie in context.Request.Cookies.Keys)
            {
                context.Response.Cookies.Delete(cookie);
            }
            
            // Set the cookie directly
            context.Response.Cookies.Append("dev-user-email", email!, new CookieOptions
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

        // Get the configured user email from appsettings.json
        var configuredUserEmail = _configuration["Development:IAPSimulation:UserEmail"];

        // Standard login page HTML with textbox for email input
        var loginPageHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Development Login</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .login-form {{ border: 1px solid #ddd; padding: 20px; margin: 20px 0; border-radius: 8px; background: #f9f9f9; }}
        .form-group {{ margin-bottom: 15px; }}
        label {{ display: block; margin-bottom: 5px; font-weight: bold; }}
        input[type='email'] {{ width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 4px; font-size: 16px; }}
        button {{ background: #0095ff; color: white; border: none; padding: 12px 20px; border-radius: 4px; cursor: pointer; font-size: 16px; width: 100%; }}
        button:hover {{ background: #0077cc; }}
        button:disabled {{ background: #ccc; cursor: not-allowed; }}
        .quick-login {{ margin-top: 20px; padding: 15px; background: #e1ecf4; border-radius: 4px; }}
        .quick-login button {{ background: #28a745; margin-top: 10px; }}
        .quick-login button:hover {{ background: #218838; }}
        h1 {{ color: #333; text-align: center; }}
        .info {{ background: #fff3cd; padding: 15px; border-radius: 4px; margin: 20px 0; border-left: 4px solid #ffc107; }}
        #error-message {{ color: red; display: none; margin-top: 10px; }}
        #success-message {{ color: green; display: none; margin-top: 10px; }}
        .loading {{ display: none; margin-top: 10px; }}
    </style>
    <script>
        async function loginWithEmail() {{
            const emailInput = document.getElementById('email-input');
            const email = emailInput.value.trim();
            const loginButton = document.getElementById('login-button');
            const errorDiv = document.getElementById('error-message');
            const successDiv = document.getElementById('success-message');
            const loadingDiv = document.getElementById('loading');
            
            // Hide previous messages
            errorDiv.style.display = 'none';
            successDiv.style.display = 'none';
            
            if (!email) {{
                errorDiv.innerText = 'Please enter an email address';
                errorDiv.style.display = 'block';
                return;
            }}
            
            if (!email.includes('@')) {{
                errorDiv.innerText = 'Please enter a valid email address';
                errorDiv.style.display = 'block';
                return;
            }}
            
            // Disable button and show loading
            loginButton.disabled = true;
            loadingDiv.style.display = 'block';
            
            try {{
                // First, create/ensure user exists with UNOPS_GEN_USER role
                const createUserResponse = await fetch('/api/dev/create-user', {{
                    method: 'POST',
                    headers: {{
                        'Content-Type': 'application/json'
                    }},
                    body: JSON.stringify({{ email: email }})
                }});
                
                if (createUserResponse.ok) {{
                    const userData = await createUserResponse.json();
                    successDiv.innerText = `User ${{userData.status.toLowerCase()}}: ${{email}} with roles: ${{userData.roles.join(', ')}}`;
                    successDiv.style.display = 'block';
                    
                    // Wait a moment to show the success message
                    setTimeout(() => {{
                        loginAs(email);
                    }}, 1000);
                }} else {{
                    const errorData = await createUserResponse.json();
                    errorDiv.innerText = `Failed to create user: ${{errorData.error || 'Unknown error'}}`;
                    errorDiv.style.display = 'block';
                    loginButton.disabled = false;
                    loadingDiv.style.display = 'none';
                }}
            }} catch (error) {{
                console.error('Error creating user:', error);
                errorDiv.innerText = `Error: ${{error.message}}`;
                errorDiv.style.display = 'block';
                loginButton.disabled = false;
                loadingDiv.style.display = 'none';
            }}
        }}

        function loginWithConfiguredUser() {{
            loginAs('{configuredUserEmail}');
        }}

        function loginAs(email) {{
            console.log('Logging in as:', email);
            
            try {{
                // First clear browser storage
                localStorage.clear();
                sessionStorage.clear();
                
                // Clear all cookies
                document.cookie.split(';').forEach(function(c) {{
                    document.cookie = c.trim().split('=')[0] + '=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;';
                }});
                
                // Wait a moment to ensure everything is cleared
                setTimeout(() => {{
                    // Redirect to login with the selected user
                    window.location.href = `/dev-login?user=${{encodeURIComponent(email)}}`;
                }}, 100);
            }} catch (error) {{
                console.error('Login failed:', error);
                document.getElementById('error-message').style.display = 'block';
                document.getElementById('error-message').innerText = `Login error: ${{error.message}}`;
            }}
        }}
        
        window.onload = function() {{
            // Clear storages on page load
            localStorage.clear();
            sessionStorage.clear();
            
            // Set the configured email as default
            document.getElementById('email-input').value = '{configuredUserEmail}';
        }};

        // Allow Enter key to submit
        function handleKeyPress(event) {{
            if (event.key === 'Enter') {{
                loginWithEmail();
            }}
        }}
    </script>
</head>
<body>
    <h1>Development Login</h1>
    
    <div class='info'>
        <p><strong>Development Mode:</strong> Enter any valid email address to simulate login. The user will be automatically created with UNOPS_GEN_USER role if they don't exist.</p>
    </div>
    
    <div class='login-form'>
        <div class='form-group'>
            <label for='email-input'>Email Address:</label>
            <input type='email' id='email-input' placeholder='Enter email address' onkeypress='handleKeyPress(event)' />
        </div>
        <button id='login-button' onclick='loginWithEmail()'>Login with Email</button>
        <div class='loading' id='loading'>Creating user and logging in...</div>
        <div id='success-message'></div>
        <div id='error-message'></div>
    </div>
    
    <div class='quick-login'>
        <p><strong>Quick Login:</strong> Use the configured default user from appsettings.json</p>
        <button onclick='loginWithConfiguredUser()'>Login as {configuredUserEmail}</button>
    </div>
</body>
</html>";

        await context.Response.WriteAsync(loginPageHtml);
    }
} 