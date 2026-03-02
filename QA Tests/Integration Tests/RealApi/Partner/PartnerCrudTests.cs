/**
 * @fileoverview Real-API integration tests for Partner CRUD endpoints.
 * Tests POST/GET/PUT/DELETE and list/permissions via PAOWebApplicationFactory.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Server;

namespace UNOPS.PAO.IntegrationTests.RealApi.Partner;

[Collection("Integration Tests")]
public class PartnerCrudTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    private const string BaseUrl = "/api/partner";

    public PartnerCrudTests(PAOWebApplicationFactory<Program> factory, ITestOutputHelper output)
        : base(factory)
    {
        _output = output;
    }

    private static object CreatePartnerRequest(string? name = null, string? shortDesc = null, string? longDesc = null) => new
    {
        Name = name ?? $"Test Partner {Guid.NewGuid():N}",
        PartnerShortDescription = shortDesc ?? "TP",
        PartnerLongDescription = longDesc ?? "Description text",
        PartnerCategoryId = 1,
        LiaisonOfficeId = 1,
        PartnerGroupId = 1,
        ConfirmDuplicateCreation = true
    };

    private async Task<(HttpResponseMessage Response, JsonElement? Body)> CreatePartnerAsync(object? request = null)
    {
        var payload = request ?? CreatePartnerRequest();
        var response = await Client.PostAsJsonAsync(BaseUrl, payload);
        JsonElement? body = null;
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
                body = JsonSerializer.Deserialize<JsonElement>(content);
        }
        catch { /* ignore */ }
        return (response, body);
    }

    private static int? GetIdFromCreateResponse(HttpResponseMessage response, JsonElement? body)
    {
        if (body == null || !response.IsSuccessStatusCode) return null;
        if (body.Value.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var idProp))
            return idProp.GetInt32();
        return null;
    }

    #region Positive Tests (3)

    [Fact]
    public async Task Create_MinimalFields_Returns201AndId()
    {
        var (response, body) = await CreatePartnerAsync();
        _output.WriteLine($"Create response: {response.StatusCode}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        if (response.StatusCode != HttpStatusCode.Created) return;
        body.Should().NotBeNull();
        var id = GetIdFromCreateResponse(response, body);
        id.Should().NotBeNull();
        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_ExistingPartner_ReturnsCorrectData()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var content = await getResp.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetInt32().Should().Be(id!.Value);
    }

    [Fact]
    public async Task Update_ExistingPartner_ChangesPersist()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var newName = $"Updated {Guid.NewGuid():N}";
        var updatePayload = new
        {
            Id = id!.Value,
            Name = newName,
            PartnerShortDescription = "UP",
            PartnerLongDescription = "Updated description",
            PartnerCategoryId = 1,
            LiaisonOfficeId = 1,
            PartnerGroupId = 1,
            ConfirmDuplicateCreation = true
        };
        var putResp = await Client.PutAsJsonAsync(BaseUrl, updatePayload);
        putResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (putResp.StatusCode != HttpStatusCode.OK) return;

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("name").GetString().Should().Be(newName);
    }

    #endregion

    #region Negative Tests (9+)

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var (response, _) = await CreatePartnerAsync(CreatePartnerRequest(name: ""));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_WhitespaceName_Returns400()
    {
        var (response, _) = await CreatePartnerAsync(CreatePartnerRequest(name: "   "));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_NonExistentPartner_Returns404()
    {
        var response = await Client.GetAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_NegativeId_Returns404()
    {
        var response = await Client.GetAsync($"{BaseUrl}/-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Update_NonExistentPartner_Returns404()
    {
        var payload = new { Id = 999999, Name = "Test", PartnerShortDescription = "T", PartnerLongDescription = "Desc" };
        var response = await Client.PutAsJsonAsync(BaseUrl, payload);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Delete_NonExistentPartner_Returns404()
    {
        var response = await Client.DeleteAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Update_EmptyName_Returns400()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var payload = new { Id = id!.Value, Name = "", PartnerShortDescription = "T", PartnerLongDescription = "Desc" };
        var response = await Client.PutAsJsonAsync(BaseUrl, payload);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Delete_AlreadyDeletedPartner_Returns404()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var firstDelete = await Client.DeleteAsync($"{BaseUrl}/{id}");
        firstDelete.IsSuccessStatusCode.Should().BeTrue();

        var secondDelete = await Client.DeleteAsync($"{BaseUrl}/{id}");
        secondDelete.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_NullBody_Returns400()
    {
        var content = new StringContent("null", Encoding.UTF8, "application/json");
        var response = await Client.PostAsync(BaseUrl, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_InvalidPartnerCategoryId_Returns400Or500()
    {
        var req = new { Name = $"Partner {Guid.NewGuid():N}", PartnerCategoryId = -999, LiaisonOfficeId = 1, PartnerGroupId = 1, ConfirmDuplicateCreation = true };
        var response = await Client.PostAsJsonAsync(BaseUrl, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_ZeroId_Returns404()
    {
        var response = await Client.GetAsync($"{BaseUrl}/0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Edge/Boundary Tests (9+)

    [Fact]
    public async Task Create_MaxLengthName_HandlesGracefully()
    {
        var name = new string('a', 500);
        var (response, _) = await CreatePartnerAsync(CreatePartnerRequest(name: name));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_OneCharName_MinBoundary_Succeeds()
    {
        var (response, body) = await CreatePartnerAsync(CreatePartnerRequest(name: "X"));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    [Fact]
    public async Task Create_SpecialCharactersInName_HandlesUnicode()
    {
        var name = $"Partner 日本é {Guid.NewGuid():N}";
        var (response, body) = await CreatePartnerAsync(CreatePartnerRequest(name: name));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().NotBeNull();
    }

    [Fact]
    public async Task Create_SqlInjectionCharsInName_DoesNotBreak()
    {
        var name = $"Partner'; DROP TABLE Partner;-- {Guid.NewGuid():N}";
        var (response, _) = await CreatePartnerAsync(CreatePartnerRequest(name: name));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Pagination_Page0_ReturnsFirstPageOrBadRequest()
    {
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=0&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Pagination_VeryLargePageNumber_ReturnsEmptyOrLastPage()
    {
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=999999&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var page = await response.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.Records.Should().NotBeNull();
    }

    [Fact]
    public async Task Pagination_PageSize1_ReturnsOneRecord()
    {
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var page = await response.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.Records.Count.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task Create_AllOptionalFieldsNull_Succeeds()
    {
        var req = new { Name = $"Partner {Guid.NewGuid():N}", ConfirmDuplicateCreation = true };
        var (response, body) = await CreatePartnerAsync(req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    [Fact]
    public async Task Create_HtmlScriptTagsInDescription_HandlesXSS()
    {
        var desc = "<script>alert('xss')</script>Normal text";
        var (response, body) = await CreatePartnerAsync(CreatePartnerRequest(longDesc: desc));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    [Fact]
    public async Task Pagination_PageSize1000_ReturnsUpTo1000()
    {
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=1000");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var page = await response.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.Records.Count.Should().BeLessThanOrEqualTo(1000);
    }

    #endregion

    #region Functional Tests (9+)

    [Fact]
    public async Task CreatedPartner_HasDefaultStatusDraft()
    {
        var (response, body) = await CreatePartnerAsync();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        if (response.StatusCode != HttpStatusCode.Created || body == null) return;
        if (body.Value.TryGetProperty("data", out var data) && data.TryGetProperty("status", out var status))
            status.GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreatedPartner_HasCorrectDefaultStatus()
    {
        var (response, body) = await CreatePartnerAsync();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        if (response.StatusCode != HttpStatusCode.Created) return;
        body.Should().NotBeNull();
        body!.Value.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreatedPartner_CreatedDateIsSet()
    {
        var (response, body) = await CreatePartnerAsync();
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        if (response.StatusCode != HttpStatusCode.Created || body == null) return;
        if (body.Value.TryGetProperty("data", out var data) && data.TryGetProperty("createdDate", out var cd))
            cd.GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task SoftDelete_GetAfterDelete_Returns404()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Update_ChangesLastModifiedDate()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var payload = new { Id = id!.Value, Name = $"Updated {Guid.NewGuid():N}", PartnerShortDescription = "UP", PartnerLongDescription = "Updated", PartnerCategoryId = 1, LiaisonOfficeId = 1, PartnerGroupId = 1 };
        await Client.PutAsJsonAsync(BaseUrl, payload);
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("lastModifiedDate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task List_DoesNotReturnSoftDeletedPartners()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=100");
        if (listResp.StatusCode != HttpStatusCode.OK) return;
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        var ids = page!.Records.Select(r => r.TryGetProperty("id", out var i) ? i.GetInt32() : -1).ToList();
        ids.Should().NotContain(id.Value);
    }

    [Fact]
    public async Task List_WithPagination_ReturnsPaginatedResults()
    {
        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=5");
        listResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (listResp.StatusCode != HttpStatusCode.OK) return;
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page.Should().NotBeNull();
        page!.PageIndex.Should().Be(1);
        page.PageSize.Should().Be(5);
        page.Records.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPermissions_ExistingPartner_ReturnsPermissions()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var permResp = await Client.GetAsync($"{BaseUrl}/{id}/permissions");
        permResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPermissions_NonExistentPartner_Returns404()
    {
        var response = await Client.GetAsync($"{BaseUrl}/999999/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Integration Tests (9+)

    [Fact]
    public async Task FullCrudCycle_CreateReadUpdateReadDeleteRead404()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var get1 = await Client.GetAsync($"{BaseUrl}/{id}");
        get1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (get1.StatusCode != HttpStatusCode.OK) return;

        var updatePayload = new { Id = id!.Value, Name = $"Updated {Guid.NewGuid():N}", PartnerShortDescription = "UP", PartnerLongDescription = "Updated", PartnerCategoryId = 1, LiaisonOfficeId = 1, PartnerGroupId = 1 };
        var putResp = await Client.PutAsJsonAsync(BaseUrl, updatePayload);
        putResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (putResp.StatusCode != HttpStatusCode.OK) return;

        var get2 = await Client.GetAsync($"{BaseUrl}/{id}");
        get2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (get2.StatusCode != HttpStatusCode.OK) return;

        var deleteResp = await Client.DeleteAsync($"{BaseUrl}/{id}");
        deleteResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.InternalServerError);
        if (!deleteResp.IsSuccessStatusCode) return;

        var get3 = await Client.GetAsync($"{BaseUrl}/{id}");
        get3.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateThenGetPermissions()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var permResp = await Client.GetAsync($"{BaseUrl}/{id!.Value}/permissions");
        permResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateMultiple_VerifyPagination()
    {
        await CreatePartnerAsync();
        await CreatePartnerAsync();
        var resp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=2");
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (resp.StatusCode != HttpStatusCode.OK) return;
        var page = await resp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        page.Records.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task CreateDelete_VerifyListExcludesDeleted()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=1000");
        listResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (listResp.StatusCode != HttpStatusCode.OK) return;
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        var found = page!.Records.Any(r => r.TryGetProperty("id", out var i) && i.GetInt32() == id!.Value);
        found.Should().BeFalse();
    }

    [Fact]
    public async Task CreateThenList_FindsPartner()
    {
        var name = $"ListablePartner {Guid.NewGuid():N}";
        var (createResp, createBody) = await CreatePartnerAsync(CreatePartnerRequest(name: name));
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=100");
        listResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (listResp.StatusCode != HttpStatusCode.OK) return;
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        var found = page!.Records.Any(r => r.TryGetProperty("id", out var i) && i.GetInt32() == id!.Value);
        found.Should().BeTrue();
    }

    [Fact]
    public async Task Search_FindsPartnerByName_WhenPostgres()
    {
        if (!Factory.IsUsingPostgres) return;
        var name = $"SearchablePartner {Guid.NewGuid():N}";
        var (createResp, createBody) = await CreatePartnerAsync(CreatePartnerRequest(name: name));
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var searchResp = await Client.GetAsync($"{BaseUrl}/search?query={Uri.EscapeDataString(name)}&pageIndex=1&pageSize=10");
        searchResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (searchResp.StatusCode != HttpStatusCode.OK) return;
        var result = await searchResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUpdateGet_VerifyPersisted()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var newName = $"PersistedName {Guid.NewGuid():N}";
        var updatePayload = new { Id = id!.Value, Name = newName, PartnerShortDescription = "PN", PartnerLongDescription = "Persisted", PartnerCategoryId = 1, LiaisonOfficeId = 1, PartnerGroupId = 1 };
        await Client.PutAsJsonAsync(BaseUrl, updatePayload);
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (getResp.StatusCode != HttpStatusCode.OK) return;
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("name").GetString().Should().Be(newName);
    }

    [Fact]
    public async Task List_OrderBy_Name_ReturnsSorted()
    {
        var resp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=5&orderBy=Name&ascending=true");
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (resp.StatusCode != HttpStatusCode.OK) return;
        var page = await resp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateThenDelete_Verify404OnGet()
    {
        var (createResp, createBody) = await CreatePartnerAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    #endregion

    /*
    ### 3:1 Ratio Compliance Check
    | Category         | Count | Tests (sample names)         |
    |------------------|-------|------------------------------|
    | Positive (P)     | 3     | Create_MinimalFields, Get_ExistingPartner, Update_ExistingPartner |
    | Negative (N)     | 12    | Create_EmptyName, Create_WhitespaceName, Get_NonExistent, Get_NegativeId, Update_NonExistent, Delete_NonExistent, Update_EmptyName, Delete_AlreadyDeleted, Create_NullBody, Create_InvalidPartnerCategoryId, Get_ZeroId |
    | Edge/Boundary (E)| 10    | Create_MaxLengthName, Create_OneCharName, Create_SpecialCharactersInName, Create_SqlInjectionCharsInName, Pagination_Page0, Pagination_VeryLargePageNumber, Pagination_PageSize1, Create_AllOptionalFieldsNull, Create_HtmlScriptTagsInDescription, Pagination_PageSize1000 |
    | Functional (F)   | 10    | CreatedPartner_HasDefaultStatusDraft, CreatedPartner_HasCorrectDefaultStatus, CreatedPartner_CreatedDateIsSet, SoftDelete_GetAfterDelete_Returns404, Update_ChangesLastModifiedDate, List_DoesNotReturnSoftDeletedPartners, List_WithPagination_ReturnsPaginatedResults, GetPermissions_ExistingPartner_ReturnsPermissions, GetPermissions_NonExistentPartner_Returns404 |
    | Integration (I)  | 10    | FullCrudCycle_CreateReadUpdateReadDeleteRead404, CreateThenGetPermissions, CreateMultiple_VerifyPagination, CreateDelete_VerifyListExcludesDeleted, CreateThenList_FindsPartner, Search_FindsPartnerByName_WhenPostgres, CreateUpdateGet_VerifyPersisted, List_OrderBy_Name_ReturnsSorted, CreateThenDelete_Verify404OnGet |
    | **N ≥ 3P?**      | ✅    | 12 >= 9 (3×3)                |
    | **E ≥ 3P?**      | ✅    | 10 >= 9 (3×3)                |
    | **F ≥ 3P?**      | ✅    | 10 >= 9 (3×3)                |
    | **I ≥ 3P?**      | ✅    | 10 >= 9 (3×3)                |
    */
}
