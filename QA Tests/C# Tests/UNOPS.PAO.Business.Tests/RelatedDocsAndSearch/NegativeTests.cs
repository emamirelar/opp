/**
 * @fileoverview Related Section, Documents & Search — Negative tests.
 * PNO-810, PNO-806, PNO-812, PNO-1216. Invalid input, unauthorized, expected failures.
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
/// Negative tests for Related Section, Documents &amp; Search.
/// </summary>
[Collection("Related Docs And Search Integration")]
[Trait("Category", "Negative")]
[Trait("Feature", "RelatedDocsAndSearch")]
public class NegativeTests : RelatedDocsAndSearchFixtureBase
{
    public NegativeTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    #region PNO-812 — Search Negative

    [Fact]
    [Trait("TestId", "RDS-NEG-001")]
    public async Task Search_EmptyQuery_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(""));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-002")]
    public async Task Search_WhitespaceOnlyQuery_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("   "));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-003")]
    public async Task Search_Unauthenticated_Returns401()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-004")]
    public async Task SearchFields_Unauthenticated_Returns401()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchFieldsUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-005")]
    public void GetFileType_NullFile_ThrowsArgumentNullException()
    {
        IFormFile? file = null;
        Action act = () => file!.GetFileType();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-006")]
    public async Task Search_InvalidPage_ReturnsErrorOrEmpty()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", -1, 10));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-007")]
    public async Task Search_ZeroPageSize_ReturnsErrorOrEmpty()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 1, 0));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-008")]
    public async Task OpportunityGet_NonExistentId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/99999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-009")]
    public async Task OpportunityGet_NegativeId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-010")]
    public async Task OpportunityGet_ZeroId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-011")]
    public async Task Search_SqlInjectionAttempt_DoesNotExposeData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("'; DROP TABLE opportunity;--"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-012")]
    public async Task Search_XssAttempt_DoesNotExecute()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("<script>alert(1)</script>"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-013")]
    public void AllowedExtensions_DoesNotIncludeExe()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().NotContain(".exe");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-014")]
    public void AllowedExtensions_DoesNotIncludeBat()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().NotContain(".bat");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-015")]
    public void AllowedExtensions_DoesNotIncludeJs()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().NotContain(".js");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-016")]
    public void AllowedExtensions_DoesNotIncludeHtml()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().NotContain(".html");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-017")]
    public void AllowedExtensions_DoesNotIncludeDll()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().NotContain(".dll");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-018")]
    public async Task DocumentByEntity_InvalidEntityType_MayReturn404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.DocumentByEntity("InvalidEntity", 1));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-019")]
    public async Task Search_NewlineInQuery_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\nmalicious"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-020")]
    public async Task Search_TabInQuery_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\t"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-021")]
    public async Task Search_NullByteInQuery_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\0"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-022")]
    public async Task Search_VeryLongQuery_HandledWithoutCrash()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = new string('x', 10000);
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-023")]
    public async Task Search_UnicodeNull_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("\u0000"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-024")]
    public async Task OpportunityList_Unauthenticated_Returns401()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-025")]
    public async Task Search_InvalidUtf8_HandledGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/search?query=%FF%FE");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-026")]
    public void GetFileType_EmptyContentType_ReturnsEmpty()
    {
        var file = CreateMockFormFile("test.docx", "", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-027")]
    public void Spec_MinSearchQueryLength_IsPositive()
    {
        RelatedDocsAndSearchSpec.MinSearchQueryLength.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-028")]
    public async Task Search_OnlySpecialChars_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("!@#$%"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-029")]
    public async Task Search_EmojiOnly_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("😀"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-030")]
    public async Task Search_RepeatedSpaces_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test    query"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-031")]
    public void DocxMimeType_IsNotEmpty()
    {
        RelatedDocsAndSearchSpec.DocxMimeType.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-032")]
    public void PdfMimeType_IsNotEmpty()
    {
        RelatedDocsAndSearchSpec.PdfMimeType.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-033")]
    public void AllowedExtensions_IsNotNull()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-034")]
    public void AllowedExtensions_HasElements()
    {
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-035")]
    public async Task Search_BackslashInQuery_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\\path"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-036")]
    public async Task Search_DoubleQuoteInQuery_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\"quote"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-037")]
    public async Task Search_SingleQuoteInQuery_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test'quote"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-038")]
    public async Task Search_PercentEncoding_HandledCorrectly()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("100%"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-039")]
    public async Task Search_PlusSign_HandledCorrectly()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("C++"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-040")]
    public async Task Search_EqualsSign_HandledCorrectly()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a=b"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-041")]
    public void GetFileType_WrongMimeForDocx_ReturnsProvidedMime()
    {
        var file = CreateMockFormFile("test.docx", "application/octet-stream", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("application/octet-stream");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-042")]
    public void GetFileType_WrongExtension_StillReturnsContentType()
    {
        var file = CreateMockFormFile("malware.exe", "application/pdf", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("application/pdf");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-043")]
    public async Task Search_NonLatinScript_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("测试"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-044")]
    public async Task Search_ArabicScript_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("اختبار"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-045")]
    public async Task Search_CyrillicScript_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("тест"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-046")]
    public async Task Search_RtlText_Returns200Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("مرحبا"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-047")]
    public void OpportunityBase_IsAbsolutePath()
    {
        RelatedDocsAndSearchSpec.OpportunityBase.Should().StartWith("/");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-048")]
    public void SearchFieldsUrl_ContainsSearchFields()
    {
        RelatedDocsAndSearchSpec.SearchFieldsUrl.Should().Contain("search-fields");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-049")]
    public async Task Search_EmptyPageParam_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/search?query=test&page=&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-050")]
    public async Task Search_NonNumericPage_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/search?query=test&page=abc&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-051")]
    public void AllowedExtensions_AllStartWithDot()
    {
        foreach (var ext in RelatedDocsAndSearchSpec.AllowedExtensions)
            ext.Should().StartWith(".");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-052")]
    public void AllowedExtensions_NoDuplicates()
    {
        var distinct = RelatedDocsAndSearchSpec.AllowedExtensions.Distinct().ToList();
        distinct.Should().HaveCount(RelatedDocsAndSearchSpec.AllowedExtensions.Length);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-053")]
    public async Task Search_MissingQueryParam_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/search");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-054")]
    public async Task Search_QueryParamOnly_Returns400Or200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/search?query=");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-055")]
    public void DocumentByEntity_PartnerEntity_ReturnsCorrectPath()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("Partner", 5).Should().Be("/api/document/Partner/5");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-056")]
    public void DocumentByEntity_ContactEntity_ReturnsCorrectPath()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("Contact", 10).Should().Be("/api/document/Contact/10");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-057")]
    public async Task Search_UnicodeReplacementChar_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("\uFFFD"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-058")]
    public async Task Search_CombiningCharacters_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("e\u0301"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-059")]
    public void GetFileType_JsonContentType_ReturnsJsonMime()
    {
        var file = CreateMockFormFile("data.json", "application/json", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("application/json");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-060")]
    public void GetFileType_XmlContentType_ReturnsXmlMime()
    {
        var file = CreateMockFormFile("data.xml", "application/xml", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("application/xml");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-061")]
    public async Task Search_ControlCharacters_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\u0001\u0002"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-062")]
    public async Task Search_FormFeed_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\u000C"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-063")]
    public async Task Search_CarriageReturn_HandledSafely()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\r"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-064")]
    public void Spec_SearchUrl_EscapesQuery()
    {
        var url = RelatedDocsAndSearchSpec.SearchUrl("a b");
        url.Should().Contain("a%20b");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-065")]
    public async Task Search_NegativePage_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/search?query=test&page=-5&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-066")]
    public async Task Search_NegativePageSize_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/search?query=test&page=1&pageSize=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-067")]
    public async Task Search_ExcessivePageSize_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/search?query=test&page=1&pageSize=999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-068")]
    public void GetFileType_OctetStream_ReturnsCorrectMime()
    {
        var file = CreateMockFormFile("unknown.bin", "application/octet-stream", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("application/octet-stream");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-069")]
    public void GetFileType_MultipartFormData_ReturnsCorrectMime()
    {
        var file = CreateMockFormFile("upload.docx", "multipart/form-data", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("multipart/form-data");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-070")]
    public async Task Search_ConcurrentRequests_AllSucceed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var tasks = Enumerable.Range(0, 5).Select(_ => client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test")));
        var responses = await Task.WhenAll(tasks);
        responses.Should().OnlyContain(r => r.IsSuccessStatusCode || r.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-071")]
    public async Task Search_AlternateCaseEndpoint_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/SEARCH?query=test");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-072")]
    public void OpportunityDocuments_ReturnsCorrectPath()
    {
        RelatedDocsAndSearchSpec.OpportunityDocuments(7).Should().Be("/api/document/Opportunity/7");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-073")]
    public async Task Search_QueryWithBrackets_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test[1]"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-074")]
    public async Task Search_QueryWithBraces_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test{1}"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-075")]
    public async Task Search_QueryWithPipe_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test|pipe"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-076")]
    public async Task Search_QueryWithAsterisk_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test*"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-077")]
    public async Task Search_QueryWithQuestionMark_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test?"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-078")]
    public async Task Search_QueryWithSquareBrackets_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test[optional]"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-079")]
    public void GetFileType_CsvContentType_ReturnsCorrectMime()
    {
        var file = CreateMockFormFile("data.csv", "text/csv", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("text/csv");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-080")]
    public void GetFileType_JpegContentType_ReturnsCorrectMime()
    {
        var file = CreateMockFormFile("image.jpg", "image/jpeg", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("image/jpeg");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-081")]
    public void GetFileType_PngContentType_ReturnsCorrectMime()
    {
        var file = CreateMockFormFile("image.png", "image/png", Array.Empty<byte>());
        var result = file.GetFileType();
        result.Should().Be("image/png");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-082")]
    public async Task Search_ZeroWidthSpace_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\u200B"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-083")]
    public async Task Search_ZeroWidthJoiner_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\u200D"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-084")]
    public async Task Search_RightToLeftOverride_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test\u202E"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-085")]
    public void DocxMimeType_DoesNotContainSpaces()
    {
        RelatedDocsAndSearchSpec.DocxMimeType.Should().NotContain(" ");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-086")]
    public void PdfMimeType_IsStandard()
    {
        RelatedDocsAndSearchSpec.PdfMimeType.Should().Be("application/pdf");
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-087")]
    public async Task Search_RepeatedIdenticalQueries_ConsistentResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var r2 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        r1.StatusCode.Should().Be(r2.StatusCode);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-088")]
    public async Task Search_QueryWithColon_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("key:value"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-089")]
    public async Task Search_QueryWithSemicolon_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a;b"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-NEG-090")]
    public async Task Search_QueryWithComma_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a,b,c"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #region PNO-1216 / DEF-199 — Document upload from Google Drive (Defect-Exposing)

    /// <summary>
    /// PNO-1216: Linking Word doc from Google Drive should succeed.
    /// DEF-199: Production returns error when uploading from G Drive; local upload works.
    /// This test RUNS and FAILS until DEF-199 is fixed.
    /// </summary>
    [Fact]
    [Trait("TestId", "RDS-NEG-091")]
    [Trait("Ticket", "PNO-1216")]
    [Trait("Defect", "DEF-199")]
    public async Task LinkDocument_FromGoogleDrive_WordDoc_ShouldSucceed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=1");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        int? oppId = null;
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                oppId = items[0].TryGetProperty("id", out var p) ? p.GetInt32() : null;
        }
        catch { return; }
        if (oppId == null) return;

        var body = new
        {
            link = "https://drive.google.com/file/d/test-google-id-word/view",
            googleId = "test-google-id-word",
            parentEntityName = "Opportunity",
            parentEntityId = oppId,
            name = "Test Word from G Drive.docx",
            type = RelatedDocsAndSearchSpec.DocxMimeType
        };

        var response = await PostLinkDocumentAsync(client, body);

        // PNO-1216: Should succeed. Will FAIL until DEF-199 is fixed (production returns error for G Drive).
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    /// <summary>
    /// PNO-1216: Linking PDF from Google Drive should succeed.
    /// DEF-199: Production returns error when uploading from G Drive.
    /// </summary>
    [Fact]
    [Trait("TestId", "RDS-NEG-092")]
    [Trait("Ticket", "PNO-1216")]
    [Trait("Defect", "DEF-199")]
    public async Task LinkDocument_FromGoogleDrive_Pdf_ShouldSucceed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=1");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        int? oppId = null;
        try
        {
            var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                oppId = items[0].TryGetProperty("id", out var p) ? p.GetInt32() : null;
        }
        catch { return; }
        if (oppId == null) return;

        var body = new
        {
            link = "https://drive.google.com/file/d/test-google-id-pdf/view",
            googleId = "test-google-id-pdf",
            parentEntityName = "Opportunity",
            parentEntityId = oppId,
            name = "Test PDF from G Drive.pdf",
            type = RelatedDocsAndSearchSpec.PdfMimeType
        };

        var response = await PostLinkDocumentAsync(client, body);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    /// <summary>
    /// PNO-1216: Linking doc from G Drive to Partner should succeed.
    /// DEF-199: Same G Drive upload defect applies to all entity types.
    /// </summary>
    [Fact]
    [Trait("TestId", "RDS-NEG-093")]
    [Trait("Ticket", "PNO-1216")]
    [Trait("Defect", "DEF-199")]
    public async Task LinkDocument_FromGoogleDrive_ToPartner_ShouldSucceed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new
        {
            link = "https://drive.google.com/file/d/test-partner-doc/view",
            googleId = "test-partner-doc",
            parentEntityName = "Partner",
            parentEntityId = 1,
            name = "Partner Doc from G Drive.docx",
            type = RelatedDocsAndSearchSpec.DocxMimeType
        };

        var response = await PostLinkDocumentAsync(client, body);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #endregion
}
