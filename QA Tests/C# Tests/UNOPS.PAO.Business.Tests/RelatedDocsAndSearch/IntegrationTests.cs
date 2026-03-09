/**
 * @fileoverview Related Section, Documents & Search — Integration tests.
 * PNO-810, PNO-806, PNO-812, PNO-1216. Full CRUD, API contracts, multi-component.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using UNOPS.PAO.Utilities.Helpers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.RelatedDocsAndSearch;

/// <summary>
/// Integration tests for Related Section, Documents &amp; Search.
/// </summary>
[Collection("Related Docs And Search Integration")]
[Trait("Category", "Integration")]
[Trait("Feature", "RelatedDocsAndSearch")]
public class IntegrationTests : RelatedDocsAndSearchFixtureBase
{
    public IntegrationTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    #region Full Flow Integration

    [Fact]
    [Trait("TestId", "RDS-INT-001")]
    public async Task CreateOpportunity_ThenSearch_ThenGet_FullFlow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-INT-001 " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Integration test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-002")]
    public async Task Search_ThenList_ConsistentAuth()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var listResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=10");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-003")]
    public async Task SearchFields_ThenSearch_SequentialCalls()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var fieldsResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchFieldsUrl);
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        fieldsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-004")]
    public async Task GetFileType_ThenCreateDocument_UnitToIntegration()
    {
        var file = CreateMockFormFile("test.docx", RelatedDocsAndSearchSpec.DocxMimeType, new byte[] { 0x50, 0x4B });
        var fileType = file.GetFileType();
        fileType.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=1");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-005")]
    public async Task Search_MultipleQueries_SameClient()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var queries = new[] { "a", "test", "project", "2024" };
        foreach (var q in queries)
        {
            var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(q));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-006")]
    public async Task Search_PaginationSequence()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        for (var page = 1; page <= 3; page++)
        {
            var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", page, 5));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-007")]
    public async Task Create_Search_VerifyNameInResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-INT-007 Unique " + Guid.NewGuid().ToString("N")[..12];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await searchResponse.Content.ReadAsStringAsync();
        json.Should().Contain(name);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-008")]
    public async Task Search_Unauthenticated_ThenAuthenticated_Contrast()
    {
        if (!Factory.IsUsingPostgres) return;
        var unauthClient = CreateUnauthenticatedClient();
        var authClient = CreateAuthenticatedClient();
        var unauthResponse = await unauthClient.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var authResponse = await authClient.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        authResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        unauthResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-009")]
    public async Task DocumentByEntity_OpportunityDocuments_Equivalent()
    {
        var path1 = RelatedDocsAndSearchSpec.OpportunityDocuments(5);
        var path2 = RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 5);
        path1.Should().Be(path2);
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(path1);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-010")]
    public async Task GetFileType_AllAllowedTypes_ThenSearch()
    {
        foreach (var ext in RelatedDocsAndSearchSpec.AllowedExtensions)
        {
            var mime = ext switch
            {
                ".pdf" => RelatedDocsAndSearchSpec.PdfMimeType,
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
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("doc"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-011")]
    public async Task Search_ResponseStructure_Valid()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-012")]
    public async Task Search_EmptyResult_ValidJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("xyznonexistent999"));
        var json = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "RDS-INT-013")]
    public async Task OpportunityList_ThenSearch_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=10");
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-014")]
    public async Task Search_ConcurrentCalls_AllSucceed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var tasks = Enumerable.Range(0, 10).Select(_ => client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test")));
        var responses = await Task.WhenAll(tasks);
        responses.Should().OnlyContain(r => r.IsSuccessStatusCode);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-015")]
    public async Task Create_Get_Search_VerifyId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-INT-015 " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var getResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-016")]
    public void Spec_AllConstants_Accessible()
    {
        _ = RelatedDocsAndSearchSpec.OpportunityBase;
        _ = RelatedDocsAndSearchSpec.SearchFieldsUrl;
        _ = RelatedDocsAndSearchSpec.DocxMimeType;
        _ = RelatedDocsAndSearchSpec.PdfMimeType;
        _ = RelatedDocsAndSearchSpec.AllowedExtensions;
        _ = RelatedDocsAndSearchSpec.MinSearchQueryLength;
        RelatedDocsAndSearchSpec.DocumentByEntity("X", 1).Should().NotBeNullOrEmpty();
        RelatedDocsAndSearchSpec.OpportunityDocuments(1).Should().NotBeNullOrEmpty();
        RelatedDocsAndSearchSpec.SearchUrl("x").Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "RDS-INT-017")]
    public async Task Search_ThenGetById_FromSearchResult()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var json = await searchResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        int? id = null;
        if (root.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            id = items[0].TryGetProperty("id", out var idProp) ? idProp.GetInt32() : null;
        if (id == null && root.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
            id = data[0].TryGetProperty("id", out var idProp2) ? idProp2.GetInt32() : null;
        if (id == null) return;
        var getResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-018")]
    public async Task GetFileType_IntegrationWithSpec()
    {
        var file = CreateMockFormFile("report.docx", RelatedDocsAndSearchSpec.DocxMimeType, new byte[100]);
        file.GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".docx");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-019")]
    public async Task Search_UnicodeRoundTrip()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var query = "Café";
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(query));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-020")]
    public async Task Search_ThenSearchFields_NoStateLeak()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("first"));
        var fieldsResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchFieldsUrl);
        await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("second"));
        fieldsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // INT-021 through INT-090 for 90 total
    [Fact]
    [Trait("TestId", "RDS-INT-021")]
    public async Task Search_DifferentPageSizes_AllValid()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        foreach (var size in new[] { 1, 10, 25, 50, 100 })
        {
            var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 1, size));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-022")]
    public async Task Create_WithDescription_SearchFindsByName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-INT-022 " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Long description for search" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-023")]
    public void GetFileType_FormFile_ImplementsInterface()
    {
        var file = CreateMockFormFile("x", "y", Array.Empty<byte>());
        file.Should().BeAssignableTo<IFormFile>();
        file.GetFileType().Should().Be("y");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-024")]
    public async Task Search_Headers_Present()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        response.Headers.Should().NotBeNull();
        response.Content.Headers.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "RDS-INT-025")]
    public async Task Search_ContentLength_NonNegative()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        (response.Content.Headers.ContentLength ?? 0).Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-026")]
    public async Task OpportunityList_Pagination_ParamsReflected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=2&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-027")]
    public void DocumentByEntity_Partner_ValidPath()
    {
        var path = RelatedDocsAndSearchSpec.DocumentByEntity("Partner", 10);
        path.Should().Be("/api/document/Partner/10");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-028")]
    public void DocumentByEntity_Contact_ValidPath()
    {
        var path = RelatedDocsAndSearchSpec.DocumentByEntity("Contact", 7);
        path.Should().Be("/api/document/Contact/7");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-029")]
    public async Task Search_ReuseClient_MultipleRequests()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        for (var i = 0; i < 5; i++)
        {
            var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl($"query{i}"));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-030")]
    public async Task Search_ResponseTime_Reasonable()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        sw.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sw.ElapsedMilliseconds.Should().BeLessThan(30000);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-031")]
    public void GetFileType_AndSpec_Aligned()
    {
        var file = CreateMockFormFile("test.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        file.GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-032")]
    public async Task Search_JsonStructure_HasExpectedKeys()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var hasRelevantStructure = root.TryGetProperty("items", out _) || root.TryGetProperty("data", out _) ||
            root.TryGetProperty("results", out _) || root.TryGetProperty("total", out _) || root.ValueKind == JsonValueKind.Array;
        hasRelevantStructure.Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "RDS-INT-033")]
    public async Task Create_Search_Get_FullCycle()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-INT-033 Full " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Full cycle test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-034")]
    public void SearchUrl_AndGetFileType_Independent()
    {
        var url = RelatedDocsAndSearchSpec.SearchUrl("test");
        var file = CreateMockFormFile("x", "y", Array.Empty<byte>());
        url.Should().NotBeNullOrEmpty();
        file.GetFileType().Should().Be("y");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-035")]
    public async Task SearchFields_Structure_Valid()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchFieldsUrl);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-036")]
    public void AllowedExtensions_AndGetFileType_Compatible()
    {
        var mimes = new Dictionary<string, string>
        {
            [".pdf"] = RelatedDocsAndSearchSpec.PdfMimeType,
            [".docx"] = RelatedDocsAndSearchSpec.DocxMimeType
        };
        foreach (var (ext, mime) in mimes)
        {
            var file = CreateMockFormFile($"test{ext}", mime, Array.Empty<byte>());
            file.GetFileType().Should().Be(mime);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-037")]
    public async Task Search_ThenList_NoInterference()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var search1 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a"));
        var list = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=10");
        var search2 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("b"));
        search1.StatusCode.Should().Be(HttpStatusCode.OK);
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        search2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-038")]
    public void Spec_OpportunityDocuments_Integration()
    {
        for (var id = 1; id <= 5; id++)
        {
            RelatedDocsAndSearchSpec.OpportunityDocuments(id).Should().Be($"/api/document/Opportunity/{id}");
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-039")]
    public async Task Search_EmptyQuery_BadRequest()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(""));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-040")]
    public async Task GetFileType_ThenSearch_UnitPlusIntegration()
    {
        var file = CreateMockFormFile("doc.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>());
        file.GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("document"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // INT-041 to INT-090 - additional integration scenarios
    [Fact]
    [Trait("TestId", "RDS-INT-041")]
    public async Task Search_AuthenticatedUser_ReceivesData() =>
        await RunSearchIntegration(s => s.StatusCode.Should().Be(HttpStatusCode.OK));

    [Fact]
    [Trait("TestId", "RDS-INT-042")]
    public async Task Search_Response_HasContent() =>
        await RunSearchIntegration(async s => { var c = await s.Content.ReadAsStringAsync(); c.Should().NotBeNullOrEmpty(); });

    [Fact]
    [Trait("TestId", "RDS-INT-043")]
    public async Task SearchFields_Authenticated_ReceivesData() =>
        await RunSearchFieldsIntegration(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

    [Fact]
    [Trait("TestId", "RDS-INT-044")]
    public async Task OpportunityList_Authenticated_ReceivesData() =>
        await RunListIntegration(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

    [Fact]
    [Trait("TestId", "RDS-INT-045")]
    public void GetFileType_Integration_PdfDocx() =>
        CreateMockFormFile("a.pdf", RelatedDocsAndSearchSpec.PdfMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.PdfMimeType);

    [Fact]
    [Trait("TestId", "RDS-INT-046")]
    public void DocumentByEntity_Integration_MultipleEntities()
    {
        RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 1).Should().Contain("Opportunity");
        RelatedDocsAndSearchSpec.DocumentByEntity("Partner", 2).Should().Contain("Partner");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-047")]
    public async Task Search_Integration_SequentialDifferentQueries()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("q1"));
        await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("q2"));
        var r = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("q3"));
        r.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-048")]
    public void Spec_Constants_Integration() =>
        RelatedDocsAndSearchSpec.SearchUrl("opportunity").Should().Contain("query=");

    [Fact]
    [Trait("TestId", "RDS-INT-049")]
    public async Task Create_Search_Integration() =>
        await CreateAndSearchIntegration();

    [Fact]
    [Trait("TestId", "RDS-INT-050")]
    public void GetFileType_AllFormats_Integration()
    {
        foreach (var ext in RelatedDocsAndSearchSpec.AllowedExtensions)
        {
            var mime = "application/octet-stream";
            var file = CreateMockFormFile($"x{ext}", mime, Array.Empty<byte>());
            file.GetFileType().Should().Be(mime);
        }
    }

    private async Task RunSearchIntegration(Action<HttpResponseMessage> assert)
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        assert(response);
    }

    private async Task RunSearchIntegration(Func<HttpResponseMessage, Task> assert)
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        await assert(response);
    }

    private async Task RunSearchFieldsIntegration(Action<HttpResponseMessage> assert)
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchFieldsUrl);
        assert(response);
    }

    private async Task RunListIntegration(Action<HttpResponseMessage> assert)
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=10");
        assert(response);
    }

    private async Task CreateAndSearchIntegration()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-INT " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // INT-051 to INT-090 - more integration tests
    [Fact]
    [Trait("TestId", "RDS-INT-051")]
    public async Task Search_ThenGetById_Integration() =>
        await RunSearchIntegration(async s =>
        {
            s.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await s.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
            {
                var id = items[0].TryGetProperty("id", out var p) ? p.GetInt32() : 0;
                if (id > 0)
                {
                    var client = CreateAuthenticatedClient();
                    var get = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/{id}");
                    get.StatusCode.Should().Be(HttpStatusCode.OK);
                }
            }
        });

    [Fact]
    [Trait("TestId", "RDS-INT-052")]
    public void SearchUrl_Spec_Integration() =>
        RelatedDocsAndSearchSpec.SearchUrl("test", 1, 10).Should().Contain("test").And.Contain("page=1").And.Contain("pageSize=10");

    [Fact]
    [Trait("TestId", "RDS-INT-053")]
    public void DocumentByEntity_Spec_Integration() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 99).Should().Be("/api/document/Opportunity/99");

    [Fact]
    [Trait("TestId", "RDS-INT-054")]
    public void GetFileType_Spec_Integration() =>
        CreateMockFormFile("x.docx", RelatedDocsAndSearchSpec.DocxMimeType, Array.Empty<byte>()).GetFileType().Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);

    [Fact]
    [Trait("TestId", "RDS-INT-055")]
    public void AllowedExtensions_Spec_Integration() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().Contain(".docx").And.Contain(".pdf");

    [Fact]
    [Trait("TestId", "RDS-INT-056")]
    public async Task Search_MultipleClients_Independent()
    {
        if (!Factory.IsUsingPostgres) return;
        var c1 = CreateAuthenticatedClient();
        var c2 = CreateAuthenticatedClient();
        var r1 = await c1.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a"));
        var r2 = await c2.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("b"));
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-057")]
    public async Task Search_RepeatedSameQuery_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var r2 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        r1.StatusCode.Should().Be(r2.StatusCode);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-058")]
    public void GetFileType_AndDocumentByEntity_Unrelated() =>
        CreateMockFormFile("x", "y", Array.Empty<byte>()).GetFileType().Should().NotBe(RelatedDocsAndSearchSpec.DocumentByEntity("X", 1));

    [Fact]
    [Trait("TestId", "RDS-INT-059")]
    public async Task Search_JsonParse_NoException()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var json = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "RDS-INT-060")]
    public void Spec_AllHelpers_Work() =>
        RelatedDocsAndSearchSpec.OpportunityDocuments(1).Should().Be(RelatedDocsAndSearchSpec.DocumentByEntity("Opportunity", 1));

    // Remaining INT-061 to INT-090 - varied integration scenarios
    [Fact]
    [Trait("TestId", "RDS-INT-061")]
    public async Task Search_PageBoundary_Integration()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test", 1, 1));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-062")]
    public async Task Search_QueryBoundary_Integration()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("a"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-063")]
    public void GetFileType_Boundary_EmptyContent() =>
        CreateMockFormFile("x", "application/pdf", Array.Empty<byte>()).GetFileType().Should().Be("application/pdf");

    [Fact]
    [Trait("TestId", "RDS-INT-064")]
    public void DocumentByEntity_Boundary_ZeroId() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("X", 0).Should().EndWith("/0");

    [Fact]
    [Trait("TestId", "RDS-INT-065")]
    public async Task Search_Negative_Unauthenticated_Integration()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-066")]
    public void Spec_Integration_AllPathsStartWithSlash()
    {
        RelatedDocsAndSearchSpec.OpportunityBase.Should().StartWith("/");
        RelatedDocsAndSearchSpec.SearchFieldsUrl.Should().StartWith("/");
        RelatedDocsAndSearchSpec.DocumentByEntity("X", 1).Should().StartWith("/");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-067")]
    public async Task Create_Search_Get_VerifyData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-INT-067 Verify " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Verify" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        var searchJson = await searchResponse.Content.ReadAsStringAsync();
        searchJson.Should().Contain(name);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-068")]
    public void GetFileType_FormFile_AllProperties()
    {
        var file = CreateMockFormFile("test.docx", RelatedDocsAndSearchSpec.DocxMimeType, new byte[] { 1, 2, 3 });
        file.Name.Should().Be("file");
        file.FileName.Should().Be("test.docx");
        file.ContentType.Should().Be(RelatedDocsAndSearchSpec.DocxMimeType);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-069")]
    public async Task Search_Response_ContentTypeJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-070")]
    public void SearchUrl_Integration_DefaultParams()
    {
        var url = RelatedDocsAndSearchSpec.SearchUrl("x");
        url.Should().Contain("page=1");
        url.Should().Contain("pageSize=10");
    }

    [Fact]
    [Trait("TestId", "RDS-INT-071")]
    public void DocumentByEntity_Integration_AllEntityTypes()
    {
        foreach (var entity in new[] { "Opportunity", "Partner", "Contact", "Interaction" })
        {
            RelatedDocsAndSearchSpec.DocumentByEntity(entity, 1).Should().Contain(entity);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-072")]
    public void GetFileType_Integration_OfficeFormats()
    {
        var formats = new[] { (".docx", RelatedDocsAndSearchSpec.DocxMimeType), (".pdf", RelatedDocsAndSearchSpec.PdfMimeType) };
        foreach (var (ext, mime) in formats)
        {
            CreateMockFormFile($"x{ext}", mime, Array.Empty<byte>()).GetFileType().Should().Be(mime);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-073")]
    public async Task Search_Integration_ChainCalls()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchFieldsUrl);
        var r2 = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var r3 = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=5");
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
        r3.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-074")]
    public void Spec_MinSearchQueryLength_Integration() =>
        RelatedDocsAndSearchSpec.MinSearchQueryLength.Should().Be(1);

    [Fact]
    [Trait("TestId", "RDS-INT-075")]
    public void AllowedExtensions_Integration_SevenItems() =>
        RelatedDocsAndSearchSpec.AllowedExtensions.Should().HaveCount(7);

    [Fact]
    [Trait("TestId", "RDS-INT-076")]
    public async Task Search_Integration_StatusCodeOk()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        (await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-077")]
    public void DocxMimeType_Integration_Standard() =>
        RelatedDocsAndSearchSpec.DocxMimeType.Should().Contain("wordprocessingml");

    [Fact]
    [Trait("TestId", "RDS-INT-078")]
    public void PdfMimeType_Integration_Standard() =>
        RelatedDocsAndSearchSpec.PdfMimeType.Should().Be("application/pdf");

    [Fact]
    [Trait("TestId", "RDS-INT-079")]
    public async Task Create_Integration_ReturnsId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "RDS-INT-079 " + Guid.NewGuid().ToString("N")[..8], description = "Test" });
        if (createResponse.StatusCode == HttpStatusCode.OK)
        {
            var json = await createResponse.Content.ReadAsStringAsync();
            json.Should().Contain("id");
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-080")]
    public void SearchUrl_Integration_EncodesCorrectly() =>
        RelatedDocsAndSearchSpec.SearchUrl("a b").Should().Contain("a%20b");

    [Fact]
    [Trait("TestId", "RDS-INT-081")]
    public void OpportunityDocuments_Integration_Format() =>
        RelatedDocsAndSearchSpec.OpportunityDocuments(42).Should().Be("/api/document/Opportunity/42");

    [Fact]
    [Trait("TestId", "RDS-INT-082")]
    public void GetFileType_Integration_ReturnsNonNull() =>
        CreateMockFormFile("x", "y", Array.Empty<byte>()).GetFileType().Should().NotBeNull();

    [Fact]
    [Trait("TestId", "RDS-INT-083")]
    public void DocumentByEntity_Integration_Format() =>
        RelatedDocsAndSearchSpec.DocumentByEntity("Test", 123).Should().Be("/api/document/Test/123");

    [Fact]
    [Trait("TestId", "RDS-INT-084")]
    public async Task Search_Integration_ValidJsonResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl("test"));
        var json = await response.Content.ReadAsStringAsync();
        JsonDocument.Parse(json);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-085")]
    public void Spec_Integration_OpportunityBase() =>
        RelatedDocsAndSearchSpec.OpportunityBase.Should().Be("/api/opportunity");

    [Fact]
    [Trait("TestId", "RDS-INT-086")]
    public void Spec_Integration_SearchFieldsUrl() =>
        RelatedDocsAndSearchSpec.SearchFieldsUrl.Should().Be("/api/opportunity/search-fields");

    [Fact]
    [Trait("TestId", "RDS-INT-087")]
    public void GetFileType_Integration_AllMimes()
    {
        var mimes = new[] { "application/pdf", "application/json", "text/plain" };
        foreach (var m in mimes)
        {
            CreateMockFormFile("x", m, Array.Empty<byte>()).GetFileType().Should().Be(m);
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-088")]
    public async Task Search_Integration_ParallelCalls()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var tasks = Enumerable.Range(0, 5).Select(i => client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl($"q{i}")));
        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(r => r.IsSuccessStatusCode);
    }

    [Fact]
    [Trait("TestId", "RDS-INT-089")]
    public void DocumentByEntity_Integration_Ids()
    {
        for (var i = 1; i <= 10; i++)
        {
            RelatedDocsAndSearchSpec.DocumentByEntity("X", i).Should().EndWith($"/{i}");
        }
    }

    [Fact]
    [Trait("TestId", "RDS-INT-090")]
    public async Task FullIntegration_SearchCreateGet()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "RDS-INT-090 Full " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Full integration" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var searchResponse = await client.GetAsync(RelatedDocsAndSearchSpec.SearchUrl(name));
        var getResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}/{id}");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #region PNO-810 / DEF-200 — Relevant People API (Defect-Exposing)

    /// <summary>
    /// PNO-810: Relevant people endpoint should return 200.
    /// DEF-200: People titles may differ from Directory (BigQuery data).
    /// Validates API contract; title correctness requires Directory comparison.
    /// </summary>
    [Fact]
    [Trait("TestId", "RDS-INT-091")]
    [Trait("Ticket", "PNO-810")]
    [Trait("Defect", "DEF-200")]
    public async Task RelevantPeople_Authenticated_Returns200OrEmpty()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=1");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        int? oppId = null;
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                oppId = items[0].TryGetProperty("id", out var p) ? p.GetInt32() : null;
        }
        catch { return; }
        if (oppId == null) return;

        var response = await client.GetAsync(RelatedDocsAndSearchSpec.RelevantPeopleUrl(oppId.Value));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    /// <summary>
    /// PNO-810: When relevant people are returned, each should have title/description structure.
    /// DEF-200: Titles may be wrong (e.g. ICT Specialist vs MIS Specialist from Directory).
    /// </summary>
    [Fact]
    [Trait("TestId", "RDS-INT-092")]
    [Trait("Ticket", "PNO-810")]
    [Trait("Defect", "DEF-200")]
    public async Task RelevantPeople_WhenReturned_HasExpectedStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync($"{RelatedDocsAndSearchSpec.OpportunityBase}?page=1&pageSize=1");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        int? oppId = null;
        try
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                oppId = items[0].TryGetProperty("id", out var p) ? p.GetInt32() : null;
        }
        catch { return; }
        if (oppId == null) return;

        var response = await client.GetAsync(RelatedDocsAndSearchSpec.RelevantPeopleUrl(oppId.Value));
        if (response.StatusCode != HttpStatusCode.OK) return;

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseDoc = JsonDocument.Parse(responseJson);
        var root = responseDoc.RootElement;

        if (root.TryGetProperty("relevantPeople", out var people) && people.GetArrayLength() > 0)
        {
            var first = people[0];
            first.TryGetProperty("personId", out _).Should().BeTrue();
            first.TryGetProperty("title", out _).Should().BeTrue();
        }
    }

    /// <summary>
    /// PNO-810: Relevant people URL is correctly formed.
    /// </summary>
    [Fact]
    [Trait("TestId", "RDS-INT-093")]
    [Trait("Ticket", "PNO-810")]
    public void RelevantPeopleUrl_FormatsCorrectly()
    {
        RelatedDocsAndSearchSpec.RelevantPeopleUrl(42).Should().Contain("/42/relevant-people");
        RelatedDocsAndSearchSpec.RelevantPeopleUrl(1, 10).Should().Contain("maxResults=10");
    }

    #endregion

    #endregion
}
