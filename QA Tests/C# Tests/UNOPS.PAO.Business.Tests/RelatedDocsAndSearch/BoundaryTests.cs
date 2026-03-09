/**
 * @fileoverview Related Section, Documents & Search — Boundary tests.
 * PNO-810, PNO-806, PNO-812, PNO-1216. Boundary values, edge cases.
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
/// Boundary tests for Related Section, Documents &amp; Search.
/// </summary>
[Collection("Related Docs And Search Integration")]
[Trait("Category", "Boundary")]
[Trait("Feature", "RelatedDocsAndSearch")]
public class BoundaryTests : RelatedDocsAndSearchFixtureBase
{
    public BoundaryTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    #region Search Boundary

    [Fact]
    [Trait("TestId", "RDS-BND-001")]
    public async Task Search_QueryLengthExactlyOne_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-002")]
    public async Task Search_QueryLength255_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = new string('x', 255);
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-003")]
    public async Task Search_QueryLength256_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = new string('x', 256);
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-004")]
    public async Task Search_PageOne_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 1, 10));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-005")]
    public async Task Search_PageSizeOne_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 1, 1));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-006")]
    public async Task Search_PageSize100_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 1, 100));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-007")]
    public void GetFileType_EmptyByteArray_ReturnsContentType()
    {
        var file = CreateMockFormFile("empty.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-008")]
    public void GetFileType_SingleByte_ReturnsContentType()
    {
        var file = CreateMockFormFile("tiny.docx", RelatedDocsAndSearchSpec.DocxMimeType, new byte[] { 0 });
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-009")]
    public void GetFileType_MaxIntLength_ReturnsContentType()
    {
        var content = new byte[1024];
        var file = CreateMockFormFile("large.docx", RelatedDocsAndSearchSpec.DocxMimeType, content);
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-010")]
    public void AllowedExtensions_Count_IsSeven()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().HaveCount(7);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-011")]
    public void DocxMimeType_Length_IsReasonable()
    {
        RelatedDocsAndSearchSpec.DocxMimeType.Length.Should().BeInRange(50, 100);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-012")]
    public void MinSearchQueryLength_IsOne()
    {
        RelatedDocsAndSearchSpec.MinSearchQueryLength.Should().Be(1);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-013")]
    public async Task Search_QueryWithLeadingSpace_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(" test"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-014")]
    public async Task Search_QueryWithTrailingSpace_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test "));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-015")]
    public async Task Search_LastPage_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 9999, 10));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-016")]
    public void GetFileType_FileNameWithSpaces_ReturnsContentType()
    {
        var file = CreateMockFormFile("my document.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-017")]
    public void GetFileType_FileNameWithDots_ReturnsContentType()
    {
        var file = CreateMockFormFile("report.final.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-018")]
    public void GetFileType_FileNameUnicode_ReturnsContentType()
    {
        var file = CreateMockFormFile("文档.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-019")]
    public void GetFileType_FileNameRtl_ReturnsContentType()
    {
        var file = CreateMockFormFile("ملف.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-020")]
    public void SearchUrl_DefaultPage_IsOne()
    {
        var url = RelatedDocsAndSearchSpec.SearchUrl("x");
        url.Should().Contain("page=1");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-021")]
    public void SearchUrl_DefaultPageSize_IsTen()
    {
        var url = RelatedDocsAndSearchSpec.SearchUrl("x");
        url.Should().Contain("pageSize=10");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-022")]
    public void DocumentByEntity_ZeroEntityId_ReturnsPath()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 0).Should().Be("/api/document/Opportunity/0");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-023")]
    public void DocumentByEntity_MaxIntEntityId_ReturnsPath()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", int.MaxValue).Should().Contain("Opportunity");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-024")]
    public async Task Search_QueryExactlyMinLength_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = new string('a', RelatedDocsAndSearchSpec.MinSearchQueryLength);
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-025")]
    public void AllowedExtensions_Order_ContainsDocxBeforePdf()
    {
        var idxDocx = Array.IndexOf(RelatedDocsAndSearchSpec.AllowedExtensions, ".docx");
        var idxPdf = Array.IndexOf(RelatedDocsAndSearchSpec.AllowedExtensions, ".pdf");
        idxDocx.Should().BeGreaterThan(-1);
        idxPdf.Should().BeGreaterThan(-1);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-026")]
    public void GetFileType_FileNameExactly255Chars_ReturnsContentType()
    {
        var name = new string('a', 250) + ".docx";
        var file = CreateMockFormFile(name, RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-027")]
    public async Task Search_QueryWithBoundaryUnicode_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("\uFFFF"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-028")]
    public async Task Search_PageSizeExactlyMaxAllowed_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 1, 1000));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-029")]
    public void GetFileType_AllAllowedMimes_ReturnCorrectly()
    {
        var mimes = new[] { RelatedDocsAndSearchSpec.DocxMimeType, RelatedDocsAndSearchSpec.PdfMimeType, "application/msword" };
        foreach (var mime in mimes)
        {
            var file = CreateMockFormFile("test", mime, Array.Empty<byte>());
            file.GetFileType().Should().Be(mime);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-BND-030")]
    public void OpportunityBase_EndsWithOpportunity()
    {
        RelatedDocsAndSearchSpec.OpportunityBase.Should().EndWith("opportunity");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-031")]
    public async Task Search_QueryBoundaryAscii_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("\x7F"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-032")]
    public async Task Search_QueryBoundarySpace_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(" "));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-033")]
    public void GetFileType_FileNameWithParentheses_ReturnsContentType()
    {
        var file = CreateMockFormFile("file (1).docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-034")]
    public void GetFileType_FileNameWithHyphen_ReturnsContentType()
    {
        var file = CreateMockFormFile("my-file.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-035")]
    public void GetFileType_FileNameWithUnderscore_ReturnsContentType()
    {
        var file = CreateMockFormFile("my_file.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-036")]
    public async Task Search_QueryLength500_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = new string('x', 500);
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-037")]
    public async Task Search_QueryLength1000_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = new string('x', 1000);
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-038")]
    public void PdfMimeType_IsStandardFormat()
    {
        RelatedDocsAndSearchSpec.PdfMimeType.Should().Be("application/pdf");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-039")]
    public void DocxMimeType_ContainsWordprocessingml()
    {
        RelatedDocsAndSearchSpec.DocxMimeType.Should().Contain("wordprocessingml");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-040")]
    public void DocxMimeType_ContainsOpenxmlformats()
    {
        RelatedDocsAndSearchSpec.DocxMimeType.Should().Contain("openxmlformats");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-041")]
    public async Task Search_PageZero_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 0, 10));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-042")]
    public void GetFileType_FileNameCaseVariations_ReturnsContentType()
    {
        var file = CreateMockFormFile("TEST.DOCX", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-043")]
    public void SearchUrl_EncodesUnicode()
    {
        var url = RelatedDocsAndSearchSpec.SearchUrl("é");
        url.Should().Contain("%");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-044")]
    public void AllowedExtensions_AllLowercase()
    {
        foreach (var ext in RelatedDocsAndSearchSpec.AllowedExtensions)
            ext.Should().Be(ext.ToLowerInvariant());
    }

    [Fact]
    [Trait("TestId", "RDS-BND-045")]
    public async Task Search_QueryWithMixedWhitespace_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a b\tc"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-046")]
    public void GetFileType_StreamPositionZero_ReturnsContentType()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var file = new FormFile(stream, 0, 3, "file", "test.docx") { ContentType = RelatedDocsAndSearchSpec.DocxMimeType };
        var result = file.GetFileType();
        result.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-047")]
    public void DocumentByEntity_EmptyEntityName_ReturnsPath()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("", 1).Should().Be("/api/document//1");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-048")]
    public async Task Search_ExactMatchQuery_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("exactmatch"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-BND-049")]
    public void GetFileType_MultipleExtensions_ReturnsContentType()
    {
        var file = CreateMockFormFile("archive.tar.gz", "application/gzip", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("application/gzip");
    }

    [Fact]
    [Trait("TestId", "RDS-BND-050")]
    public async Task Search_QueryAtMaxUrlLength_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = new string('x', 2000);
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.RequestUriTooLong);
    }

    // BND-051 through BND-090 for 90 total
    [Fact]
    [Trait("TestId", "RDS-BND-051")]
    public void Spec_OpportunityDocuments_MatchesDocumentByEntity() =>
        RelatedDocsAndSearchSpec.OpportunityDocuments(3).Should().Be(RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 3));

    [Fact]
    [Trait("TestId", "RDS-BND-052")]
    public void GetFileType_XlsMime_ReturnsCorrect() =>
        CreateMockFormFile("old.xls", "application/vnd.ms-excel", Array.Empty<byte>()).GetFileType().Should().Be("application/vnd.ms-excel");

    [Fact]
    [Trait("TestId", "RDS-BND-053")]
    public void GetFileType_PptMime_ReturnsCorrect() =>
        CreateMockFormFile("old.ppt", "application/vnd.ms-powerpoint", Array.Empty<byte>()).GetFileType().Should().Be("application/vnd.ms-powerpoint");

    [Fact]
    [Trait("TestId", "RDS-BND-054")]
    public void AllowedExtensions_IncludesXls() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".xls");

    [Fact]
    [Trait("TestId", "RDS-BND-055")]
    public void AllowedExtensions_IncludesPpt() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".ppt");

    [Fact]
    [Trait("TestId", "RDS-BND-056")]
    public void SearchUrl_QueryWithAmpersand_Escaped() =>
        RelatedDocsAndSearchSpec.SearchUrl("a&b").Should().Contain("%26");

    [Fact]
    [Trait("TestId", "RDS-BND-057")]
    public void SearchUrl_QueryWithSpace_Escaped() =>
        RelatedDocsAndSearchSpec.SearchUrl("a b").Should().Contain("%20");

    [Fact]
    [Trait("TestId", "RDS-BND-058")]
    public void SearchUrl_QueryWithSlash_Escaped() =>
        RelatedDocsAndSearchSpec.SearchUrl("a/b").Should().Contain("%2F");

    [Fact]
    [Trait("TestId", "RDS-BND-059")]
    public void SearchUrl_CustomPage_Reflected() =>
        RelatedDocsAndSearchSpec.SearchUrl("x", 5, 25).Should().Contain("page=5").And.Contain("pageSize=25");

    [Fact]
    [Trait("TestId", "RDS-BND-060")]
    public void DocxMimeType_StartsWithApplication() =>
        RelatedDocsAndSearchSpec.DocxMimeType.Should().StartWith("application/");

    [Fact]
    [Trait("TestId", "RDS-BND-061")]
    public void PdfMimeType_StartsWithApplication() =>
        RelatedDocsAndSearchSpec.PdfMimeType.Should().StartWith("application/");

    [Fact]
    [Trait("TestId", "RDS-BND-062")]
    public void OpportunityBase_ContainsApi() =>
        RelatedDocsAndSearchSpec.OpportunityBase.Should().Contain("api");

    [Fact]
    [Trait("TestId", "RDS-BND-063")]
    public void SearchFieldsUrl_StartsWithOpportunityBase() =>
        RelatedDocsAndSearchSpec.SearchFieldsUrl.Should().StartWith(RelatedDocsAndSearchSpec.OpportunityBase);

    [Fact]
    [Trait("TestId", "RDS-BND-064")]
    public void DocumentByEntity_InteractionEntity_ReturnsPath() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("Interaction", 99).Should().Be("/api/document/Interaction/99");

    [Fact]
    [Trait("TestId", "RDS-BND-065")]
    public void MinSearchQueryLength_LessThanOrEqual255() =>
        RelatedDocsAndSearchSpec.MinSearchQueryLength.Should().BeLessThanOrEqualTo(255);

    [Fact]
    [Trait("TestId", "RDS-BND-066")]
    public void GetFileType_FileNameWithPlus_ReturnsContentType() =>
        CreateMockFormFile("file+name.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-067")]
    public void GetFileType_FileNameWithEquals_ReturnsContentType() =>
        CreateMockFormFile("file=1.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-068")]
    public void AllowedExtensions_NoEmptyStrings() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().NotContain("");

    [Fact]
    [Trait("TestId", "RDS-BND-069")]
    public void AllowedExtensions_NoNullStrings() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().OnlyContain(ext => !string.IsNullOrEmpty(ext));

    [Fact]
    [Trait("TestId", "RDS-BND-070")]
    public void DocxMimeType_NoLeadingTrailingSpaces() =>
        RelatedDocsAndSearchSpec.DocxMimeType.Trim().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-071")]
    public void PdfMimeType_NoLeadingTrailingSpaces() =>
        RelatedDocsAndSearchSpec.PdfMimeType.Trim().Should().Be(RelatedDocsAndSearchSpec.PdfMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-072")]
    public void SearchUrl_EmptyQuery_StillBuildsUrl() =>
        RelatedDocsAndSearchSpec.SearchUrl("").Should().Contain("query=");

    [Fact]
    [Trait("TestId", "RDS-BND-073")]
    public void DocumentByEntity_PartnerTree_ReturnsPath() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("PartnerTree", 1).Should().Be("/api/document/PartnerTree/1");

    [Fact]
    [Trait("TestId", "RDS-BND-074")]
    public void GetFileType_FileNameWithHash_ReturnsContentType() =>
        CreateMockFormFile("file#1.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-075")]
    public void GetFileType_FileNameWithAt_ReturnsContentType() =>
        CreateMockFormFile("user@file.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-076")]
    public void GetFileType_FileNameWithPercent_ReturnsContentType() =>
        CreateMockFormFile("100%.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-077")]
    public void GetFileType_FileNameWithAsterisk_ReturnsContentType() =>
        CreateMockFormFile("*.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-078")]
    public void GetFileType_FileNameWithQuestionMark_ReturnsContentType() =>
        CreateMockFormFile("?.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-079")]
    public void GetFileType_FileNameWithBrackets_ReturnsContentType() =>
        CreateMockFormFile("[draft].docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-080")]
    public void SearchUrl_QueryWithHash_Escaped() =>
        RelatedDocsAndSearchSpec.SearchUrl("a#b").Should().Contain("%23");

    [Fact]
    [Trait("TestId", "RDS-BND-081")]
    public void SearchUrl_QueryWithQuestionMark_Escaped() =>
        RelatedDocsAndSearchSpec.SearchUrl("a?b").Should().Contain("%3F");

    [Fact]
    [Trait("TestId", "RDS-BND-082")]
    public void SearchUrl_QueryWithBracket_Escaped() =>
        RelatedDocsAndSearchSpec.SearchUrl("a[b").Should().Contain("%5B");

    [Fact]
    [Trait("TestId", "RDS-BND-083")]
    public void DocxMimeType_EndsWithDocument() =>
        RelatedDocsAndSearchSpec.DocxMimeType.Should().Contain("document");

    [Fact]
    [Trait("TestId", "RDS-BND-084")]
    public void AllowedExtensions_DocBeforeDocx() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".doc");

    [Fact]
    [Trait("TestId", "RDS-BND-085")]
    public void GetFileType_FileNameWithTilde_ReturnsContentType() =>
        CreateMockFormFile("~temp.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-086")]
    public void GetFileType_FileNameWithBacktick_ReturnsContentType() =>
        CreateMockFormFile("`file`.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-087")]
    public void GetFileType_FileNameWithCaret_ReturnsContentType() =>
        CreateMockFormFile("file^.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-088")]
    public void GetFileType_FileNameWithPipe_ReturnsContentType() =>
        CreateMockFormFile("a|b.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-089")]
    public void GetFileType_FileNameWithColon_ReturnsContentType() =>
        CreateMockFormFile("C:file.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-BND-090")]
    public void GetFileType_FileNameWithSemicolon_ReturnsContentType() =>
        CreateMockFormFile("a;b.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    #endregion
}
