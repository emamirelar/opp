using System.Net;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using ServerProgram = UNOPS.PAO.Server.Program;

/// <summary>
/// Lightweight reverse-proxy server that forwards HTTP requests to
/// <see cref="PAOWebApplicationFactory{TStartup}"/>'s in-process TestServer.
///
/// The factory runs the backend in "Testing" environment (bypasses Secret Manager)
/// but configures real PostgreSQL via the gcloud IAM token. This proxy exposes
/// the factory on a real TCP port so Angular (ng serve) and Playwright tests
/// can connect to it like a normal API server.
///
/// Usage:
///   dotnet run --project "QA Tests/TestApiServer"
///
/// The server listens on http://localhost:5159 by default.
/// Override with: --urls http://localhost:5200
/// </summary>
// Resolve repo root — works whether run from repo root, QA Tests/, or bin/Debug/
var REPO_ROOT = FindRepoRoot(AppContext.BaseDirectory)
    ?? FindRepoRoot(Directory.GetCurrentDirectory())
    ?? throw new DirectoryNotFoundException("Cannot find repository root (looking for UNOPS.PAO.Server/)");

var port = 5159;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--urls" && i + 1 < args.Length)
    {
        var url = args[i + 1];
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            port = uri.Port;
    }
}

static string? FindRepoRoot(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "UNOPS.PAO.Server")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return null;
}

Console.WriteLine("=== UNOPS Opportunity+ Test API Server ===");
Console.WriteLine();

// Refresh gcloud token for database connectivity
RefreshGcloudToken();

// Create the WebApplicationFactory — triggers real PostgreSQL probe
// The WebApplicationFactory resolves content root from the Server project.
// It needs to find appsettings.json in the Server project directory.
var serverProjectDir = Path.Combine(REPO_ROOT, "UNOPS.PAO.Server");
if (Directory.Exists(serverProjectDir))
    Directory.SetCurrentDirectory(serverProjectDir);

Console.Write("Initializing PAOWebApplicationFactory... ");
HttpClient factoryClient;
PAOWebApplicationFactory<ServerProgram> factory;
try
{
    factory = new PAOWebApplicationFactory<ServerProgram>();
    factoryClient = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true,
        BaseAddress = new Uri($"http://localhost:{port}")
    });
    Console.WriteLine("done.");
    Console.WriteLine($"  Using PostgreSQL: {factory.IsUsingPostgres}");
}
catch (Exception ex)
{
    Console.WriteLine($"FAILED: {ex.GetType().Name}");
    Console.WriteLine($"  {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
    Console.WriteLine();
    Console.WriteLine("The factory could not initialize. Check:");
    Console.WriteLine("  1. Cloud SQL proxy is running on port 5432");
    Console.WriteLine("  2. gcloud token is valid: gcloud auth print-access-token > %TEMP%\\gcloud_token.txt");
    Environment.Exit(1);
    return; // unreachable but makes compiler happy
}
Console.WriteLine();

// Build a Kestrel host that reverse-proxies to the factory's TestServer
var builder = WebApplication.CreateSlimBuilder();
builder.WebHost.UseUrls($"http://localhost:{port}");
builder.Logging.SetMinimumLevel(LogLevel.Warning);
var app = builder.Build();

app.Map("/{**path}", async (HttpContext ctx) =>
{
    var request = new HttpRequestMessage
    {
        Method = new HttpMethod(ctx.Request.Method),
        RequestUri = new Uri($"http://localhost/{ctx.Request.Path}{ctx.Request.QueryString}"),
    };

    foreach (var header in ctx.Request.Headers)
    {
        if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
            continue;
        request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
    }

    // IAPVerificationMiddleware runs before UseAuthentication() and returns 401
    // if neither x-goog-iap-jwt-assertion nor x-goog-authenticated-user-email is
    // present. Inject the email header so the middleware sets up the user principal
    // from the seeded test identity (testuser@unops.org, Id=123).
    if (!request.Headers.Contains("x-goog-authenticated-user-email"))
    {
        request.Headers.TryAddWithoutValidation(
            "x-goog-authenticated-user-email",
            "accounts.google.com:testuser@unops.org");
    }

    if (ctx.Request.ContentLength is > 0 || ctx.Request.ContentType != null)
    {
        request.Content = new StreamContent(ctx.Request.Body);
        if (ctx.Request.ContentType != null)
            request.Content.Headers.ContentType =
                System.Net.Http.Headers.MediaTypeHeaderValue.Parse(ctx.Request.ContentType);
        if (ctx.Request.ContentLength.HasValue)
            request.Content.Headers.ContentLength = ctx.Request.ContentLength;
    }

    HttpResponseMessage response;
    try
    {
        response = await factoryClient.SendAsync(request);
    }
    catch (Exception ex)
    {
        ctx.Response.StatusCode = 502;
        await ctx.Response.WriteAsync($"Proxy error: {ex.Message}");
        return;
    }

    ctx.Response.StatusCode = (int)response.StatusCode;

    foreach (var header in response.Headers)
        ctx.Response.Headers[header.Key] = header.Value.ToArray();
    foreach (var header in response.Content.Headers)
        ctx.Response.Headers[header.Key] = header.Value.ToArray();

    ctx.Response.Headers.Remove("transfer-encoding");

    await response.Content.CopyToAsync(ctx.Response.Body);
});

Console.WriteLine($"Test API Server listening on http://localhost:{port}");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

await app.RunAsync();

static void RefreshGcloudToken()
{
    var tokenFile = Path.Combine(Path.GetTempPath(), "gcloud_token.txt");
    try
    {
        var psi = new System.Diagnostics.ProcessStartInfo("gcloud", "auth print-access-token")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var proc = System.Diagnostics.Process.Start(psi);
        if (proc == null) return;
        var token = proc.StandardOutput.ReadToEnd().Trim();
        proc.WaitForExit(15_000);
        if (token.Length > 50)
        {
            File.WriteAllText(tokenFile, token);
            Console.WriteLine($"[gcloud] Token refreshed ({token.Length} chars)");
        }
    }
    catch
    {
        Console.WriteLine("[gcloud] Token refresh failed (cached token may still work)");
    }
}
