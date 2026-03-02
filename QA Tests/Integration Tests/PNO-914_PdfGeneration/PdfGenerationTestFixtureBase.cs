/**
 * @fileoverview PNO-914 PDF Generation test fixture base.
 * Provides shared setup for DocumentController PDF generation endpoint tests.
 * Tests are SKIPPED due to DEF-021/DEF-024; fully implemented for un-skip when fixed.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO914.PdfGeneration;

/// <summary>
/// Request model for CreatePdfFromMarkdown endpoint.
/// Mirrors expected production CreatePdfFromMarkdownRequest (PNO-914).
/// </summary>
public class CreatePdfFromMarkdownRequest
{
    public string Content { get; set; } = string.Empty;
    public string? Filename { get; set; }
}

/// <summary>
/// Base fixture for PNO-914 PDF generation integration tests.
/// Uses Integration Tests collection for shared PAOWebApplicationFactory.
/// All tests hit DocumentController PDF endpoint; currently skipped due to DEF-021/DEF-024.
/// </summary>
[CollectionDefinition("PNO914_PdfGeneration")]
public class PdfGenerationCollectionDefinition : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
}

public abstract class PdfGenerationTestFixtureBase
{
    protected readonly PAOWebApplicationFactory<Program> Factory;
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// PDF generation endpoint route. Add to APIDictionary when implementing:
    /// DocumentGeneratePdf = Document + "/generate-pdf"
    /// </summary>
    protected const string PdfEndpoint = "/api/document/generate-pdf";

    protected PdfGenerationTestFixtureBase(PAOWebApplicationFactory<Program> factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Creates an authenticated HTTP client for DocumentController requests.
    /// </summary>
    protected HttpClient CreateAuthenticatedClient()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    /// <summary>
    /// Creates an unauthenticated client (no auth headers).
    /// </summary>
    protected HttpClient CreateUnauthenticatedClient()
    {
        return Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    /// <summary>
    /// POSTs CreatePdfFromMarkdownRequest to the PDF endpoint.
    /// </summary>
    protected async Task<HttpResponseMessage> PostPdfRequestAsync(HttpClient client, CreatePdfFromMarkdownRequest request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(PdfEndpoint, content);
    }

    /// <summary>
    /// POSTs raw JSON to the PDF endpoint (for boundary/negative tests).
    /// </summary>
    protected async Task<HttpResponseMessage> PostPdfRawAsync(HttpClient client, string json, string? contentType = "application/json")
    {
        var content = new StringContent(json, Encoding.UTF8, contentType);
        return await client.PostAsync(PdfEndpoint, content);
    }
}
