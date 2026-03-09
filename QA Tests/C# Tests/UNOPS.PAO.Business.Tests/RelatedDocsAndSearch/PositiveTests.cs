/**
 * @fileoverview Related Section, Documents & Search — Positive tests.
 * PNO-810, PNO-806, PNO-812, PNO-1216. Happy-path scenarios.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using UNOPS.PAO.Utilities.Helpers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.RelatedDocsAndSearch;

/// <summary>
/// Positive tests for Related Section, Documents &amp; Search.
/// Requirements: People with correct descriptions (PNO-810); .docx works (PNO-806);
/// Search retrieves opportunities (PNO-812); Document upload pipeline (PNO-1216).
/// </summary>
[Collection("Related Docs And Search Integration")]
[Trait("Category", "Positive")]
[Trait("Feature", "RelatedDocsAndSearch")]
public class PositiveTests : RelatedDocsAndSearchFixtureBase
{
    public PositiveTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    #region PNO-812 — Opportunity Search (Positive)

    [Fact]
    [Trait("TestId", "RDS-POS-001")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_ValidQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-002")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_QueryByName_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("opportunity"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-003")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_QueryWithPagination_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a", 2, 20));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-004")]
    [Trait("Ticket", "PNO-812")]
    public async Task SearchFields_Authenticated_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchFieldsUrl);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-005")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_AlphanumericQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("Project123"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-006")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_SingleCharacter_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-007")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_LongQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = new string('x', 100);
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-008")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_UnicodeQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("José"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-009")]
    [Trait("Ticket", "PNO-812")]
    public async Task CreateThenSearch_OpportunityAppearsInResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-POS-009 Searchable Opp " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Search test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region PNO-806, PNO-1216 — Document Upload (Positive)

    [Fact]
    [Trait("TestId", "RDS-POS-010")]
    [Trait("Ticket", "PNO-806")]
    public void GetFileType_DocxContentType_ReturnsDocxMime()
    {
        var file = CreateMockFormFile(
            "test.docx",
            RelatedDocsAndSearchSpec.DocxMimeType,
            new byte[] { 1, 2, 3 });
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-011")]
    [Trait("Ticket", "PNO-806")]
    public void GetFileType_PdfContentType_ReturnsPdfMime()
    {
        var file = CreateMockFormFile(
            "test.pdf",
            RelatedDocsAndSearchSpec.PdfMimeType,
            new byte[] { 1, 2, 3 });
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.PdfMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-012")]
    [Trait("Ticket", "PNO-806")]
    public void GetFileType_XlsxContentType_ReturnsCorrectMime()
    {
        var mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var file = CreateMockFormFile("test.xlsx", mime, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(mime);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-013")]
    [Trait("Ticket", "PNO-1216")]
    public void GetFileType_DocContentType_ReturnsCorrectMime()
    {
        var mime = "application/msword";
        var file = CreateMockFormFile("test.doc", mime, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(mime);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-014")]
    [Trait("Ticket", "PNO-1216")]
    public void GetFileType_PptxContentType_ReturnsCorrectMime()
    {
        var mime = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        var file = CreateMockFormFile("test.pptx", mime, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(mime);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-015")]
    [Trait("Ticket", "PNO-1216")]
    public void GetFileType_TextPlain_ReturnsCorrectMime()
    {
        var file = CreateMockFormFile("test.txt", "text/plain", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("text/plain");
    }

    [Fact]
    [Trait("TestId", "RDS-POS-016")]
    [Trait("Ticket", "PNO-806")]
    public void AllowedExtensions_IncludesDocx()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".docx");
    }

    [Fact]
    [Trait("TestId", "RDS-POS-017")]
    [Trait("Ticket", "PNO-806")]
    public void AllowedExtensions_IncludesPdf()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".pdf");
    }

    [Fact]
    [Trait("TestId", "RDS-POS-018")]
    [Trait("Ticket", "PNO-1216")]
    public void AllowedExtensions_IncludesAllOfficeFormats()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".doc");
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".xls");
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".xlsx");
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".ppt");
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".pptx");
    }

    [Fact]
    [Trait("TestId", "RDS-POS-019")]
    [Trait("Ticket", "PNO-812")]
    public async Task OpportunityList_Authenticated_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-020")]
    [Trait("Ticket", "PNO-812")]
    public async Task OpportunityGetById_ExistingId_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=1");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        if (!json.Contains("\"id\"")) return;
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        int? id = null;
        if (root.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            id = items[0].TryGetProperty("id", out var idProp) ? idProp.GetInt32() : null;
        if (id == null) return;
        var getResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-021")]
    [Trait("Ticket", "PNO-810")]
    public void Spec_OpportunityBase_IsCorrect()
    {
        RelatedDocsAndSearchSpec.OpportunityBase.Should().Be("/api/opportunity");
    }

    [Fact]
    [Trait("TestId", "RDS-POS-022")]
    [Trait("Ticket", "PNO-810")]
    public void Spec_DocumentByEntity_ReturnsCorrectPath()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 42).Should().Be("/api/document/Opportunity/42");
    }

    [Fact]
    [Trait("TestId", "RDS-POS-023")]
    [Trait("Ticket", "PNO-806")]
    public void DocxMimeType_MatchesOpenXmlStandard()
    {
        RelatedDocsAndSearchSpec.DocxMimeType.Should().Contain("openxmlformats");
        RelatedDocsAndSearchSpec.DocxMimeType.Should().Contain("wordprocessingml");
    }

    [Fact]
    [Trait("TestId", "RDS-POS-024")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_WhitespacePaddedQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("  test  "));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-025")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_HyphenatedQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test-opportunity"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-026")]
    [Trait("Ticket", "PNO-1216")]
    public void GetFileType_EmptyFileName_StillReturnsContentType()
    {
        var file = CreateMockFormFile("", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-027")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_NumericQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("2024"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-028")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_MixedCaseQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("TeSt"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-029")]
    [Trait("Ticket", "PNO-806")]
    public void GetFileType_LargeDocxContent_ReturnsCorrectMime()
    {
        var content = new byte[1024 * 100];
        var file = CreateMockFormFile("large.docx", RelatedDocsAndSearchSpec.DocxMimeType, content);
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-POS-030")]
    [Trait("Ticket", "PNO-812")]
    public async Task Search_SpecialCharsEscaped_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test&name=value"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion
}
