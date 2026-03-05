using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Server;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace UNOPS.PAO.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
{
    protected readonly PAOWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected IntegrationTestBase(PAOWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        
        // Set up authentication headers for Testing environment
        // The IAP handler will check for these headers when cookies are not available
        Client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        Client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        
        // Also add a cookie header for the development IAP simulation
        Client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Call at the start of tests that require a real PostgreSQL database.
    /// When Postgres is unavailable (IAM auth failure, proxy not running, etc.),
    /// this outputs a clear diagnostic message to the test runner instead of
    /// silently returning. Returns true if Postgres is available, false if not.
    /// </summary>
    protected bool RequirePostgres(ITestOutputHelper? output = null)
    {
        if (Factory.IsUsingPostgres)
            return true;

        var message = "[SKIPPED — QA-102] PostgreSQL not available. " +
                      "Possible causes: (1) Cloud SQL proxy not running on port 5432, " +
                      "(2) gcloud IAM token expired — run: gcloud auth print-access-token > %TEMP%\\gcloud_token.txt, " +
                      "(3) IAM user lacks table GRANTs, " +
                      "(4) ADC expired — run: gcloud auth application-default login";
        output?.WriteLine(message);
        Console.WriteLine(message);
        return false;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    protected async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await Client.GetAsync(url);
    }

    protected async Task<T?> PostAsync<T>(string url, object data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await Client.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
    }

    protected async Task<HttpResponseMessage> PostAsync(string url, object data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        return await Client.PostAsync(url, content);
    }

    protected async Task<T?> PutAsync<T>(string url, object data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await Client.PutAsync(url, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await Client.DeleteAsync(url);
    }

    protected T GetService<T>() where T : notnull
    {
        using var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    protected async Task<UNOPSAppDbContext> GetDbContextAsync()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
    }

    protected async Task<AppDbContext> GetCoreDbContextAsync()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    protected async Task ResetDatabaseAsync()
    {
        // NOTE: With a real PostgreSQL test database we do NOT drop and recreate —
        // that would destroy the entire schema between test runs.  Instead we rely
        // on idempotent seeding (PAOWebApplicationFactory.SeedTestData) and test
        // isolation via unique identifiers in test data.
        await Task.CompletedTask;
    }
}