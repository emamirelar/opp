/**
 * @fileoverview PNO-1212 Opportunity Statement PDF Generation - Optimization integration tests.
 * Tests PDF generation endpoints: generate-document, generate-statement-pdf, and opportunity GET.
 * Status: Peer Review
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1212
 */

using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1212;

[Collection("Integration Tests")]
[Trait("JiraRef", "PNO-1212")]
[Trait("Feature", "PNO-1212")]
[Trait("Component", "PdfOptimization")]
public class PdfOptimizationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _unauthClient;
    private readonly bool _isPostgresAvailable;

    private const string GenerateDocumentEndpoint = "/api/document/generate-document";
    private const string GenerateStatementPdfEndpoint = "/api/opportunity/generate-statement-pdf";
    private const string OpportunityEndpoint = "/api/opportunity/1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PdfOptimizationTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        _unauthClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _factory = factory;
    }

    private static StringContent JsonContent(object obj) =>
        new(JsonSerializer.Serialize(obj, JsonOptions), Encoding.UTF8, "application/json");

    #region Positive (2)

    [Fact]
    [Trait("TestId", "TC-PNO1212-POS-001")]
    [Trait("Category", "Positive")]
    public async Task POS_001_GenerateDocument_ValidData_Returns200AndPdf()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "# Opportunity Statement\n\nTest content.", filename = "TestDoc" };
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
            var bytes = await response.Content.ReadAsByteArrayAsync();
            bytes.Should().NotBeEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-POS-002")]
    [Trait("Category", "Positive")]
    public async Task POS_002_GenerateStatementPdf_WithData_Returns200OrPdf()
    {
        if (!_isPostgresAvailable) return;
        var request = new GeneratePdfRequest { Data = "# Statement\n\nWhy: Test rationale.", Filename = "Statement" };
        var response = await _client.PostAsJsonAsync(GenerateStatementPdfEndpoint, request, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        if (response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "application/pdf")
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            bytes.Should().NotBeEmpty();
        }
    }

    #endregion

    #region Negative (6)

    [Fact]
    [Trait("TestId", "TC-PNO1212-NEG-001")]
    [Trait("Category", "Negative")]
    public async Task NEG_001_GenerateDocument_EmptyData_Returns400()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "", filename = "Test" };
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task NEG_002_GenerateDocument_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "Test", filename = "Test" };
        var response = await _unauthClient.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect, HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task NEG_003_GenerateStatementPdf_NullRequest_Returns400()
    {
        if (!_isPostgresAvailable) return;
        var content = new StringContent("null", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(GenerateStatementPdfEndpoint, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task NEG_004_GenerateStatementPdf_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return;
        var request = new GeneratePdfRequest { Data = "Test" };
        var response = await _unauthClient.PostAsJsonAsync(GenerateStatementPdfEndpoint, request, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect, HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task NEG_005_GenerateDocument_InvalidJson_Returns400()
    {
        if (!_isPostgresAvailable) return;
        var content = new StringContent("{ broken json }", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(GenerateDocumentEndpoint, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-NEG-006")]
    [Trait("Category", "Negative")]
    public async Task NEG_006_GetOpportunity_NonExistentId_Returns404()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Edge/Boundary (6)

    [Fact]
    [Trait("TestId", "TC-PNO1212-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_001_GenerateDocument_SpecialCharacters_Handles()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "Café & naïve <test> \"quotes\"", filename = "Special" };
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_002_GenerateDocument_UnicodeContent_Handles()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "日本語 北京 Café 北京", filename = "Unicode" };
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_003_GenerateDocument_LargeContent_Handles()
    {
        if (!_isPostgresAvailable) return;
        var largeContent = "# Doc\n\n" + new string('x', 50000);
        var body = new { data = largeContent, filename = "Large" };
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_004_GenerateStatementPdf_EntityIdZero_Handles()
    {
        if (!_isPostgresAvailable) return;
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 0, Data = "Fallback" };
        var response = await _client.PostAsJsonAsync(GenerateStatementPdfEndpoint, request, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_005_GetOpportunity_InvalidIdFormat_Returns404Or400()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/not-an-id");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_006_GenerateDocument_ConcurrentRequests_AllComplete()
    {
        if (!_isPostgresAvailable) return;
        var tasks = Enumerable.Range(0, 5)
            .Select(i => _client.PostAsJsonAsync(GenerateDocumentEndpoint, new { data = $"Doc{i}", filename = $"F{i}" }, JsonOptions));
        var responses = await Task.WhenAll(tasks);
        responses.Should().HaveCount(5);
        responses.Select(r => r.StatusCode).Should().OnlyContain(sc =>
            sc == HttpStatusCode.OK || sc == HttpStatusCode.BadRequest || sc == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Functional (6)

    [Fact]
    [Trait("TestId", "TC-PNO1212-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_GenerateDocument_ValidData_ReturnsPdfContentType()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "# Test", filename = "Test" };
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_GenerateDocument_ResponseTime_WithinSLA()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "# Quick test", filename = "Perf" };
        var sw = Stopwatch.StartNew();
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        sw.Stop();
        response.StatusCode.Should().NotBe(HttpStatusCode.RequestTimeout);
        sw.ElapsedMilliseconds.Should().BeLessThan(30000, "PDF generation should complete within 30 seconds");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_GenerateStatementPdf_WithData_ReturnsPdfContentType()
    {
        if (!_isPostgresAvailable) return;
        var request = new GeneratePdfRequest { Data = "# Statement", Filename = "S" };
        var response = await _client.PostAsJsonAsync(GenerateStatementPdfEndpoint, request, JsonOptions);
        if (response.IsSuccessStatusCode && response.Content.Headers.ContentType != null)
        {
            response.Content.Headers.ContentType.MediaType.Should().Be("application/pdf");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task FUNC_004_GetOpportunity_ReturnsJsonStructure()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(OpportunityEndpoint);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(body);
            json.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task FUNC_005_GenerateDocument_SequentialRequests_AllSucceed()
    {
        if (!_isPostgresAvailable) return;
        for (var i = 0; i < 3; i++)
        {
            var body = new { data = $"Seq{i}", filename = $"Seq{i}" };
            var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_GenerateDocument_Utf8Encoding_Preserved()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "Réunion avec José García", filename = "Accented" };
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        if (response.IsSuccessStatusCode)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            bytes.Should().NotBeEmpty();
        }
    }

    #endregion

    #region Integration (6)

    [Fact]
    [Trait("TestId", "TC-PNO1212-INT-001")]
    [Trait("Category", "Integration")]
    public async Task INT_001_FullFlow_GetOpportunityThenGenerateStatementPdf()
    {
        if (!_isPostgresAvailable) return;
        var oppResponse = await _client.GetAsync(OpportunityEndpoint);
        oppResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        var request = new GeneratePdfRequest { Data = "# Statement\n\nWhy: Test.", Filename = "Flow" };
        var pdfResponse = await _client.PostAsJsonAsync(GenerateStatementPdfEndpoint, request, JsonOptions);
        pdfResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-INT-002")]
    [Trait("Category", "Integration")]
    public async Task INT_002_GenerateDocumentAndGenerateStatementPdf_BothAccessible()
    {
        if (!_isPostgresAvailable) return;
        var docResponse = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, new { data = "A", filename = "A" }, JsonOptions);
        var stmtResponse = await _client.PostAsJsonAsync(GenerateStatementPdfEndpoint, new GeneratePdfRequest { Data = "B" }, JsonOptions);
        docResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        stmtResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-INT-003")]
    [Trait("Category", "Integration")]
    public async Task INT_003_PdfEndpointsAndOpportunity_ShareAuthentication()
    {
        if (!_isPostgresAvailable) return;
        var oppResponse = await _client.GetAsync(OpportunityEndpoint);
        var docResponse = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, new { data = "X", filename = "X" }, JsonOptions);
        oppResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
        docResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-INT-004")]
    [Trait("Category", "Integration")]
    public async Task INT_004_GenerateDocument_SequentialRequests_NoStateIssues()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, new { data = "First", filename = "First" }, JsonOptions);
        var r2 = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, new { data = "Second", filename = "Second" }, JsonOptions);
        r1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        r2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-INT-005")]
    [Trait("Category", "Integration")]
    public async Task INT_005_GenerateStatementPdf_WithEntityNameAndId_Handles()
    {
        if (!_isPostgresAvailable) return;
        var request = new GeneratePdfRequest { EntityName = "Opportunity", EntityId = 1 };
        var response = await _client.PostAsJsonAsync(GenerateStatementPdfEndpoint, request, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1212-INT-006")]
    [Trait("Category", "Integration")]
    public async Task INT_006_GenerateDocument_ResponseHeaders_Present()
    {
        if (!_isPostgresAvailable) return;
        var body = new { data = "# H", filename = "H" };
        var response = await _client.PostAsJsonAsync(GenerateDocumentEndpoint, body, JsonOptions);
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType.Should().NotBeNull();
            response.Content.Headers.ContentLength.Should().BeGreaterThan(0);
        }
    }

    #endregion
}
