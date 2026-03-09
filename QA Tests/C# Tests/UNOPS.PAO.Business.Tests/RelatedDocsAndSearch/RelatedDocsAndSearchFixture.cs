/**
 * @fileoverview Related Docs & Search test fixture base.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.RelatedDocsAndSearch;

/// <summary>
/// Base fixture for Related Section, Documents &amp; Search integration tests.
/// </summary>
public abstract class RelatedDocsAndSearchFixtureBase
{
    protected readonly PAOWebApplicationFactory<Program> Factory;
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    protected RelatedDocsAndSearchFixtureBase(PAOWebApplicationFactory<Program> factory)
    {
        Factory = factory;
    }

    protected HttpClient CreateAuthenticatedClient()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    protected HttpClient CreateUnauthenticatedClient()
    {
        return Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    protected async Task<HttpResponseMessage> PostCreateOpportunityAsync(HttpClient client, object request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(RelatedDocsAndSearchSpec.OpportunityBase, content);
    }

    /// <summary>Creates a mock IFormFile for testing document upload.</summary>
    protected static IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    /// <summary>POST document link (PNO-1216, DEF-199) - link from Google Drive.</summary>
    protected async Task<HttpResponseMessage> PostLinkDocumentAsync(HttpClient client, object request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(RelatedDocsAndSearchSpec.DocumentLinkUrl, content);
    }
}
