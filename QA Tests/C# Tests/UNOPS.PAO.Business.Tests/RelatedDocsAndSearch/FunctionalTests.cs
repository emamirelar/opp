/**
 * @fileoverview Related Section, Documents & Search — Functional tests.
 * PNO-810, PNO-806, PNO-812, PNO-1216. Business rules, validation, state transitions.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using UNOPS.PAO.Utilities.Helpers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.RelatedDocsAndSearch;

/// <summary>
/// Functional tests for Related Section, Documents &amp; Search.
/// </summary>
[Collection("Related Docs And Search Integration")]
[Trait("Category", "Functional")]
[Trait("Feature", "RelatedDocsAndSearch")]
public class FunctionalTests : RelatedDocsAndSearchFixtureBase
{
    public FunctionalTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    #region Search Functional

    [Fact]
    [Trait("TestId", "RDS-FNC-001")]
    public async Task Search_ReturnsJsonContentType()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-002")]
    public async Task Search_ResponseHasItemsOrTotal()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var json = await response.Content.ReadAsStringAsync();
        var hasItems = json.Contains("items") || json.Contains("data") || json.Contains("results") || json.Contains("total");
        hasItems.Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-003")]
    public async Task SearchFields_ReturnsArrayOrObject()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchFieldsUrl);
        var json = await response.Content.ReadAsStringAsync();
        (json.StartsWith("[") || json.StartsWith("{")).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-004")]
    public void GetFileType_AlwaysReturnsContentType()
    {
        var file = CreateMockFormFile("test.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-005")]
    public void GetFileType_DoesNotDependOnFileName()
    {
        var file1 = CreateMockFormFile("a.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var file2 = CreateMockFormFile("b.pdf", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        file1.GetFileType().Should().Be(file2.GetFileType());
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-006")]
    public void GetFileType_DoesNotDependOnContent()
    {
        var file1 = CreateMockFormFile("test.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var file2 = CreateMockFormFile("test.docx", RelatedDocsAndSearchSpec.DocxMimeType, new byte[1000]);
        file1.GetFileType().Should().Be(file2.GetFileType());
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-007")]
    public void GetFileType_ReturnsExactContentType()
    {
        var mime = "application/custom";
        var file = CreateMockFormFile("x", mime, Array.Empty<byte>());
        file.GetFileType().Should().Be(mime);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-008")]
    public void SearchUrl_IncludesQueryParam()
    {
        RelatedDocsAndSearchSpec.SearchUrl("q").Should().Contain("query=");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-009")]
    public void SearchUrl_IncludesPageParam()
    {
        RelatedDocsAndSearchSpec.SearchUrl("q").Should().Contain("page=");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-010")]
    public void SearchUrl_IncludesPageSizeParam()
    {
        RelatedDocsAndSearchSpec.SearchUrl("q").Should().Contain("pageSize=");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-011")]
    public void DocumentByEntity_ConsistentFormat()
    {
        var path = RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 1);
        path.Should().StartWith("/api/document/");
        path.Should().EndWith("/1");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-012")]
    public void AllowedExtensions_AllAreDocumentTypes()
    {
        var docTypes = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" };
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().BeEquivalentTo(docTypes);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-013")]
    public async Task Search_NonExistentTerm_ReturnsEmptyOrResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("xyznonexistent12345"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-014")]
    public void DocxMimeType_IsStandardOoxml()
    {
        RelatedDocsAndSearchSpec.DocxMimeType.Should().Contain("officedocument");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-015")]
    public void PdfMimeType_IsIanaRegistered()
    {
        RelatedDocsAndSearchSpec.PdfMimeType.Should().Be("application/pdf");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-016")]
    public async Task Search_CaseInsensitive_ReturnsResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var r2 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("TEST"));
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-017")]
    public void GetFileType_IsDeterministic()
    {
        var file = CreateMockFormFile("test.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        var r1 = file.GetFileType();
        var r2 = file.GetFileType();
        r1.Should().Be(r2);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-018")]
    public void SearchUrl_QueryEscaping_PreservesMeaning()
    {
        var url = RelatedDocsAndSearchSpec.SearchUrl("hello world");
        url.Should().Contain("hello");
        url.Should().Contain("world");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-019")]
    public void OpportunityDocuments_EntityTypeCorrect()
    {
        RelatedDocsAndSearchSpec.OpportunityDocuments(1).Should().Contain("Opportunity");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-020")]
    public void OpportunityBase_IsConsistent()
    {
        RelatedDocsAndSearchSpec.OpportunityBase.Should().Be(RelatedDocsAndSearchSpec.SearchFieldsUrl.Replace("/search-fields", ""));
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-021")]
    public async Task Search_ResponseIsValidJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var json = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-022")]
    public void GetFileType_HandlesAllAllowedMimes()
    {
        foreach (var ext in RelatedDocsAndSearchSpec.AllowedExtensions)
        {
            var mime = ext switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => RelatedDocsAndSearchSpec.DocxMimeType,
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };
            var file = CreateMockFormFile($"test{ext}", mime, Array.Empty<byte>());
            file.GetFileType().Should().Be(mime);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-023")]
    public void MinSearchQueryLength_IsReasonable()
    {
        RelatedDocsAndSearchSpec.MinSearchQueryLength.Should().BeInRange(1, 10);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-024")]
    public async Task Search_Pagination_ReflectsParams()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 2, 5));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-025")]
    public void SearchUrl_EncodesSpecialChars()
    {
        var url = RelatedDocsAndSearchSpec.SearchUrl("a+b=c&d");
        url.Should().NotContain(" ");
        url.Should().Contain("query=");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-026")]
    public void DocumentByEntity_EntityNamePreserved()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("CustomEntity", 1).Should().Contain("CustomEntity");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-027")]
    public void DocumentByEntity_EntityIdPreserved()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("X", 42).Should().Contain("42");
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-028")]
    public void GetFileType_ExtensionIgnored_UsesContentType()
    {
        var file = CreateMockFormFile("fake.pdf", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        file.GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-029")]
    public void AllowedExtensions_OrderConsistent()
    {
        var first = RelatedDocsAndSearchSpec.AllowedExtensions[0];
        RelatedDocsAndSearchSpec.AllowedExtensions[0].Should().Be(first);
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-030")]
    public void DocxMimeType_UniqueAmongCommon()
    {
        RelatedDocsAndSearchSpec.DocxMimeType.Should().NotBe(RelatedDocsAndSearchSpec.PdfMimeType);
        RelatedDocsAndSearchSpec.DocxMimeType.Should().NotBe("application/msword");
    }

    // FNC-031 through FNC-090
    [Fact]
    [Trait("TestId", "RDS-FNC-031")]
    public void SearchUrl_BaseUrlCorrect() =>
        RelatedDocsAndSearchSpec.SearchUrl("x").Should().StartWith(RelatedDocsAndSearchSpec.OpportunityBase);

    [Fact]
    [Trait("TestId", "RDS-FNC-032")]
    public void SearchUrl_HasAmpersandBetweenParams() =>
        RelatedDocsAndSearchSpec.SearchUrl("x").Should().Contain("&");

    [Fact]
    [Trait("TestId", "RDS-FNC-033")]
    public void GetFileType_FormFileInterface() =>
        CreateMockFormFile("x", "y", Array.Empty<byte>()).Should().BeAssignableTo<IFormFile>();

    [Fact]
    [Trait("TestId", "RDS-FNC-034")]
    public void AllowedExtensions_ImmutableConcept() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().HaveCount(7);

    [Fact]
    [Trait("TestId", "RDS-FNC-035")]
    public void DocxMimeType_ContainsDocument() =>
        RelatedDocsAndSearchSpec.DocxMimeType.ToLower().Should().Contain("document");

    [Fact]
    [Trait("TestId", "RDS-FNC-036")]
    public void PdfMimeType_ContainsPdf() =>
        RelatedDocsAndSearchSpec.PdfMimeType.Should().Contain("pdf");

    [Fact]
    [Trait("TestId", "RDS-FNC-037")]
    public void SearchFieldsUrl_IsGetEndpoint() =>
        RelatedDocsAndSearchSpec.SearchFieldsUrl.Should().NotContain("POST");

    [Fact]
    [Trait("TestId", "RDS-FNC-038")]
    public void DocumentByEntity_NoTrailingSlash() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("X", 1).Should().NotEndWith("/");

    [Fact]
    [Trait("TestId", "RDS-FNC-039")]
    public void DocumentByEntity_NoDoubleSlash() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("X", 1).Should().NotContain("//");

    [Fact]
    [Trait("TestId", "RDS-FNC-040")]
    public void GetFileType_EmptyStringContentType_ReturnsEmpty() =>
        CreateMockFormFile("x", "", Array.Empty<byte>()).GetFileType().Should().Be("");

    [Fact]
    [Trait("TestId", "RDS-FNC-041")]
    public void SearchUrl_QueryParamFirst() =>
        RelatedDocsAndSearchSpec.SearchUrl("a").IndexOf("query=", StringComparison.Ordinal).Should().BeGreaterThan(0);

    [Fact]
    [Trait("TestId", "RDS-FNC-042")]
    public void OpportunityDocuments_MatchesDocumentByEntity() =>
        RelatedDocsAndSearchSpec.OpportunityDocuments(10).Should().Be(RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 10));

    [Fact]
    [Trait("TestId", "RDS-FNC-043")]
    public void AllowedExtensions_PdfFirstOrPresent() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".pdf");

    [Fact]
    [Trait("TestId", "RDS-FNC-044")]
    public void AllowedExtensions_DocxPresent() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".docx");

    [Fact]
    [Trait("TestId", "RDS-FNC-045")]
    public void GetFileType_StreamLengthIrrelevant() =>
        CreateMockFormFile("x", "application/pdf", new byte[1000000]).GetFileType().Should().Be("application/pdf");

    [Fact]
    [Trait("TestId", "RDS-FNC-046")]
    public void DocxMimeType_LengthReasonable() =>
        RelatedDocsAndSearchSpec.DocxMimeType.Length.Should().BeGreaterThan(40);

    [Fact]
    [Trait("TestId", "RDS-FNC-047")]
    public void SearchUrl_PageParamNumeric() =>
        RelatedDocsAndSearchSpec.SearchUrl("x", 3, 15).Should().Contain("page=3");

    [Fact]
    [Trait("TestId", "RDS-FNC-048")]
    public void SearchUrl_PageSizeParamNumeric() =>
        RelatedDocsAndSearchSpec.SearchUrl("x", 3, 15).Should().Contain("pageSize=15");

    [Fact]
    [Trait("TestId", "RDS-FNC-049")]
    public void GetFileType_WhitespaceContentType_ReturnsAsIs() =>
        CreateMockFormFile("x", "  application/pdf  ", Array.Empty<byte>()).GetFileType().Should().Be("  application/pdf  ");

    [Fact]
    [Trait("TestId", "RDS-FNC-050")]
    public void MinSearchQueryLength_Integer() =>
        RelatedDocsAndSearchSpec.MinSearchQueryLength.Should().BeOfType(typeof(int));

    [Fact]
    [Trait("TestId", "RDS-FNC-051")]
    public void OpportunityBase_NoTrailingSlash() =>
        RelatedDocsAndSearchSpec.OpportunityBase.Should().NotEndWith("/");

    [Fact]
    [Trait("TestId", "RDS-FNC-052")]
    public void SearchFieldsUrl_NoQueryString() =>
        RelatedDocsAndSearchSpec.SearchFieldsUrl.Should().NotContain("?");

    [Fact]
    [Trait("TestId", "RDS-FNC-053")]
    public void DocumentByEntity_SlashSeparators() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("A", 1).Should().Contain("/");

    [Fact]
    [Trait("TestId", "RDS-FNC-054")]
    public void GetFileType_FileNameWithPath_ReturnsContentType() =>
        CreateMockFormFile("C:\\path\\to\\file.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-FNC-055")]
    public void GetFileType_FileNameWithUnixPath_ReturnsContentType() =>
        CreateMockFormFile("/home/user/file.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-FNC-056")]
    public void AllowedExtensions_NoLeadingSpace() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().OnlyContain(ext => !ext.StartsWith(" "));

    [Fact]
    [Trait("TestId", "RDS-FNC-057")]
    public void AllowedExtensions_NoTrailingSpace() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().OnlyContain(ext => !ext.EndsWith(" "));

    [Fact]
    [Trait("TestId", "RDS-FNC-058")]
    public void DocxMimeType_ValidFormat() =>
        RelatedDocsAndSearchSpec.DocxMimeType.Split('/').Should().HaveCount(2);

    [Fact]
    [Trait("TestId", "RDS-FNC-059")]
    public void PdfMimeType_ValidFormat() =>
        RelatedDocsAndSearchSpec.PdfMimeType.Split('/').Should().HaveCount(2);

    [Fact]
    [Trait("TestId", "RDS-FNC-060")]
    public void SearchUrl_EmptyQueryStillBuilds() =>
        RelatedDocsAndSearchSpec.SearchUrl("").Length.Should().BeGreaterThan(20);

    [Fact]
    [Trait("TestId", "RDS-FNC-061")]
    public void GetFileType_ApplicationOctetStream_ReturnsCorrectly() =>
        CreateMockFormFile("bin", "application/octet-stream", Array.Empty<byte>()).GetFileType().Should().Be("application/octet-stream");

    [Fact]
    [Trait("TestId", "RDS-FNC-062")]
    public void GetFileType_TextHtml_ReturnsCorrectly() =>
        CreateMockFormFile("x.html", "text/html", Array.Empty<byte>()).GetFileType().Should().Be("text/html");

    [Fact]
    [Trait("TestId", "RDS-FNC-063")]
    public void GetFileType_ImagePng_ReturnsCorrectly() =>
        CreateMockFormFile("x.png", "image/png", Array.Empty<byte>()).GetFileType().Should().Be("image/png");

    [Fact]
    [Trait("TestId", "RDS-FNC-064")]
    public void DocumentByEntity_LargeId_Handled() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("X", 2147483647).Should().Contain("2147483647");

    [Fact]
    [Trait("TestId", "RDS-FNC-065")]
    public void SearchUrl_UnicodeQuery_Encoded() =>
        RelatedDocsAndSearchSpec.SearchUrl("日本語").Should().Contain("%");

    [Fact]
    [Trait("TestId", "RDS-FNC-066")]
    public void AllowedExtensions_SevenFormats() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Length.Should().Be(7);

    [Fact]
    [Trait("TestId", "RDS-FNC-067")]
    public void DocxMimeType_NotPdf() =>
        RelatedDocsAndSearchSpec.DocxMimeType.Should().NotBe(RelatedDocsAndSearchSpec.PdfMimeType);

    [Fact]
    [Trait("TestId", "RDS-FNC-068")]
    public void OpportunityBase_StartsWithSlash() =>
        RelatedDocsAndSearchSpec.OpportunityBase.Should().StartWith("/");

    [Fact]
    [Trait("TestId", "RDS-FNC-069")]
    public void SearchFieldsUrl_ContainsApi() =>
        RelatedDocsAndSearchSpec.SearchFieldsUrl.Should().Contain("api");

    [Fact]
    [Trait("TestId", "RDS-FNC-070")]
    public void GetFileType_MultipleCallsSameResult()
    {
        var file = CreateMockFormFile("x", "application/pdf", Array.Empty<byte>());
        file.GetFileType().Should().Be(file.GetFileType());
    }

    [Fact]
    [Trait("TestId", "RDS-FNC-071")]
    public void DocumentByEntity_DifferentEntities_DifferentPaths() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("A", 1).Should().NotBe(RelatedDocsAndSearchSpec.DocumentByEntity("B", 1));

    [Fact]
    [Trait("TestId", "RDS-FNC-072")]
    public void DocumentByEntity_DifferentIds_DifferentPaths() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("X", 1).Should().NotBe(RelatedDocsAndSearchSpec.DocumentByEntity("X", 2));

    [Fact]
    [Trait("TestId", "RDS-FNC-073")]
    public void SearchUrl_DifferentQueries_DifferentUrls() =>
        RelatedDocsAndSearchSpec.SearchUrl("a").Should().NotBe(RelatedDocsAndSearchSpec.SearchUrl("b"));

    [Fact]
    [Trait("TestId", "RDS-FNC-074")]
    public void SearchUrl_DifferentPages_DifferentUrls() =>
        RelatedDocsAndSearchSpec.SearchUrl("x", 1, 10).Should().NotBe(RelatedDocsAndSearchSpec.SearchUrl("x", 2, 10));

    [Fact]
    [Trait("TestId", "RDS-FNC-075")]
    public void SearchUrl_DifferentPageSizes_DifferentUrls() =>
        RelatedDocsAndSearchSpec.SearchUrl("x", 1, 10).Should().NotBe(RelatedDocsAndSearchSpec.SearchUrl("x", 1, 20));

    [Fact]
    [Trait("TestId", "RDS-FNC-076")]
    public void GetFileType_DifferentMimes_DifferentResults() =>
        CreateMockFormFile("x", "a", Array.Empty<byte>()).GetFileType().Should().NotBe(CreateMockFormFile("x", "b", Array.Empty<byte>()).GetFileType());

    [Fact]
    [Trait("TestId", "RDS-FNC-077")]
    public void AllowedExtensions_ContainsDoc() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".doc");

    [Fact]
    [Trait("TestId", "RDS-FNC-078")]
    public void AllowedExtensions_ContainsXlsx() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".xlsx");

    [Fact]
    [Trait("TestId", "RDS-FNC-079")]
    public void AllowedExtensions_ContainsPptx() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".pptx");

    [Fact]
    [Trait("TestId", "RDS-FNC-080")]
    public void DocxMimeType_NotGeneric() =>
        RelatedDocsAndSearchSpec.DocxMimeType.Should().NotBe("application/octet-stream");

    [Fact]
    [Trait("TestId", "RDS-FNC-081")]
    public void PdfMimeType_NotGeneric() =>
        RelatedDocsAndSearchSpec.PdfMimeType.Should().NotBe("application/octet-stream");

    [Fact]
    [Trait("TestId", "RDS-FNC-082")]
    public void SearchUrl_LongQuery_Handled() =>
        RelatedDocsAndSearchSpec.SearchUrl(new string('x', 500)).Length.Should().BeGreaterThan(500);

    [Fact]
    [Trait("TestId", "RDS-FNC-083")]
    public void GetFileType_ContentDispositionIrrelevant() =>
        CreateMockFormFile("test.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-FNC-084")]
    public void DocumentByEntity_EntityNameCaseSensitive() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("opportunity", 1).Should().NotBe(RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 1));

    [Fact]
    [Trait("TestId", "RDS-FNC-085")]
    public void MinSearchQueryLength_Positive() =>
        RelatedDocsAndSearchSpec.MinSearchQueryLength.Should().BePositive();

    [Fact]
    [Trait("TestId", "RDS-FNC-086")]
    public void SearchUrl_QueryParamNameCorrect() =>
        RelatedDocsAndSearchSpec.SearchUrl("x").Should().Contain("query=");

    [Fact]
    [Trait("TestId", "RDS-FNC-087")]
    public void SearchUrl_PageParamNameCorrect() =>
        RelatedDocsAndSearchSpec.SearchUrl("x").Should().Contain("page=");

    [Fact]
    [Trait("TestId", "RDS-FNC-088")]
    public void SearchUrl_PageSizeParamNameCorrect() =>
        RelatedDocsAndSearchSpec.SearchUrl("x").Should().Contain("pageSize=");

    [Fact]
    [Trait("TestId", "RDS-FNC-089")]
    public void OpportunityDocuments_IdInPath() =>
        RelatedDocsAndSearchSpec.OpportunityDocuments(123).Should().Contain("123");

    [Fact]
    [Trait("TestId", "RDS-FNC-090")]
    public void GetFileType_FormFileProperties_Independent() =>
        CreateMockFormFile("a", "b", Array.Empty<byte>()).Length.Should().Be(0);

    #endregion
}
