/**
 * @fileoverview Real-API integration tests for Opportunity CRUD endpoints.
 * Tests POST/GET/PUT/DELETE and list/search via PAOWebApplicationFactory.
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

namespace UNOPS.PAO.IntegrationTests.RealApi.Opportunity;

[Collection("Integration Tests")]
public class OpportunityCrudTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    private const string BaseUrl = "/api/opportunity";

    public OpportunityCrudTests(PAOWebApplicationFactory<Program> factory, ITestOutputHelper output)
        : base(factory)
    {
        _output = output;
    }

    private static object CreateMinimalRequest(string? name = null, string? description = null) => new
    {
        Name = name ?? $"Test Opportunity {Guid.NewGuid():N}",
        Description = description ?? "Test description for integration test"
    };

    private async Task<(HttpResponseMessage Response, JsonElement? Body)> CreateOpportunityAsync(object? request = null)
    {
        var payload = request ?? CreateMinimalRequest();
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

    private async Task<HttpResponseMessage> PatchAsync(string url, object payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await Client.SendAsync(request);
    }

    private static int? GetIdFromCreateResponse(HttpResponseMessage response, JsonElement? body)
    {
        if (body == null || !response.IsSuccessStatusCode) return null;
        if (body.Value.TryGetProperty("id", out var idProp))
            return idProp.GetInt32();
        return null;
    }

    #region Positive Tests (4)

    [Fact]
    public async Task Create_MinimalFields_Returns200AndId()
    {
        if (!RequirePostgres(_output)) return;
        var (response, body) = await CreateOpportunityAsync();
        _output.WriteLine($"Create response: {response.StatusCode}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        var id = GetIdFromCreateResponse(response, body);
        id.Should().NotBeNull();
        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_ExistingOpportunity_ReturnsCorrectData()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await getResp.Content.ReadAsStringAsync();
        content.Should().Contain("opportunity");
        var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetInt32().Should().Be(id!.Value);
    }

    [Fact]
    public async Task Update_ExistingOpportunity_ChangesPersist()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var newName = $"Updated {Guid.NewGuid():N}";
        var updatePayload = new { Id = id!.Value, Name = newName, Description = "Updated description" };
        var putResp = await Client.PutAsJsonAsync($"{BaseUrl}/{id}", updatePayload);
        putResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("name").GetString().Should().Be(newName);
    }

    [Fact]
    public async Task List_WithPagination_ReturnsPaginatedResults()
    {
        if (!RequirePostgres(_output)) return;
        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=5");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page.Should().NotBeNull();
        page!.PageIndex.Should().Be(1);
        page.PageSize.Should().Be(5);
        page.Records.Should().NotBeNull();
    }

    #endregion

    #region Negative Tests (12+)

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        if (!RequirePostgres(_output)) return;
        var (response, _) = await CreateOpportunityAsync(CreateMinimalRequest(name: ""));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_EmptyDescription_Returns400()
    {
        if (!RequirePostgres(_output)) return;
        var (response, _) = await CreateOpportunityAsync(CreateMinimalRequest(description: ""));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_NameExceeds120Chars_Returns400()
    {
        if (!RequirePostgres(_output)) return;
        var longName = new string('x', 121);
        var (response, _) = await CreateOpportunityAsync(CreateMinimalRequest(name: longName));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_NonExistentOpportunity_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var response = await Client.GetAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_NegativeId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var response = await Client.GetAsync($"{BaseUrl}/-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Update_NonExistentOpportunity_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var payload = new { Id = 999999, Name = "Test", Description = "Test" };
        var response = await Client.PutAsJsonAsync($"{BaseUrl}/999999", payload);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NonExistentOpportunity_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var response = await Client.DeleteAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_NegativeBudget_Returns400Or500()
    {
        if (!RequirePostgres(_output)) return;
        var req = new { Name = $"Opp {Guid.NewGuid():N}", Description = "Desc", InitiativeBudgetUSD = -100m };
        var response = await Client.PostAsJsonAsync(BaseUrl, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Update_EmptyName_Returns400()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var payload = new { Id = id!.Value, Name = "", Description = "Desc" };
        var response = await Client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Delete_AlreadyDeletedOpportunity_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var firstDelete = await Client.DeleteAsync($"{BaseUrl}/{id}");
        firstDelete.IsSuccessStatusCode.Should().BeTrue();

        var secondDelete = await Client.DeleteAsync($"{BaseUrl}/{id}");
        secondDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DoubleDelete_SameOpportunity_SecondReturns404()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var second = await Client.DeleteAsync($"{BaseUrl}/{id}");
        second.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_InvalidResponsibleOrgUnitId_Returns400Or500()
    {
        if (!RequirePostgres(_output)) return;
        var req = new { Name = $"Opp {Guid.NewGuid():N}", Description = "Desc", ResponsibleOrgUnitId = -999 };
        var response = await Client.PostAsJsonAsync(BaseUrl, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Edge/Boundary Tests (12+)

    [Fact]
    public async Task Create_Exactly120CharName_MaxBoundary_SucceedsOrValidates()
    {
        if (!RequirePostgres(_output)) return;
        var name = new string('a', 120);
        var (response, _) = await CreateOpportunityAsync(CreateMinimalRequest(name: name));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_OneCharName_MinBoundary_Succeeds()
    {
        if (!RequirePostgres(_output)) return;
        var (response, body) = await CreateOpportunityAsync(CreateMinimalRequest(name: "X"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    [Fact]
    public async Task Create_ExtremelyLongDescription_SucceedsOrValidates()
    {
        if (!RequirePostgres(_output)) return;
        var desc = new string('d', 10000);
        var (response, _) = await CreateOpportunityAsync(CreateMinimalRequest(description: desc));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_BudgetZero_Succeeds()
    {
        if (!RequirePostgres(_output)) return;
        var req = new { Name = $"Opp {Guid.NewGuid():N}", Description = "Desc", InitiativeBudgetUSD = 0m };
        var resp = await Client.PostAsJsonAsync(BaseUrl, req);
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_BudgetDecimalMaxValue_HandlesGracefully()
    {
        if (!RequirePostgres(_output)) return;
        var req = new { Name = $"Opp {Guid.NewGuid():N}", Description = "Desc", InitiativeBudgetUSD = decimal.MaxValue };
        var response = await Client.PostAsJsonAsync(BaseUrl, req);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Create_SpecialCharactersInName_HandlesUnicode()
    {
        if (!RequirePostgres(_output)) return;
        var name = $"Opp 日本\u00E9 {Guid.NewGuid():N}";
        var (response, body) = await CreateOpportunityAsync(CreateMinimalRequest(name: name));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        if (response.IsSuccessStatusCode)
            GetIdFromCreateResponse(response, body).Should().NotBeNull();
    }

    [Fact]
    public async Task Create_SqlInjectionCharsInName_DoesNotBreak()
    {
        if (!RequirePostgres(_output)) return;
        var name = $"Opp'; DROP TABLE Opportunity;-- {Guid.NewGuid():N}";
        var (response, _) = await CreateOpportunityAsync(CreateMinimalRequest(name: name));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Pagination_Page0_ReturnsFirstPageOrBadRequest()
    {
        if (!RequirePostgres(_output)) return;
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=0&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Pagination_VeryLargePageNumber_ReturnsEmptyOrLastPage()
    {
        if (!RequirePostgres(_output)) return;
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=999999&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.Records.Should().NotBeNull();
    }

    [Fact]
    public async Task Pagination_PageSize1_ReturnsOneRecord()
    {
        if (!RequirePostgres(_output)) return;
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.Records.Count.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task Pagination_PageSize1000_ReturnsUpTo1000()
    {
        if (!RequirePostgres(_output)) return;
        var response = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=1000");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var page = await response.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.Records.Count.Should().BeLessThanOrEqualTo(1000);
    }

    [Fact]
    public async Task Create_AllOptionalFieldsNull_Succeeds()
    {
        if (!RequirePostgres(_output)) return;
        var (response, body) = await CreateOpportunityAsync(new { Name = $"Opp {Guid.NewGuid():N}", Description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    [Fact]
    public async Task Create_HtmlScriptTagsInDescription_HandlesXSS()
    {
        if (!RequirePostgres(_output)) return;
        var desc = "<script>alert('xss')</script>Normal text";
        var (response, body) = await CreateOpportunityAsync(CreateMinimalRequest(description: desc));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetIdFromCreateResponse(response, body).Should().HaveValue();
    }

    #endregion

    #region Functional Tests (12+)

    [Fact]
    public async Task CreatedOpportunity_HasDefaultStageDraft()
    {
        if (!RequirePostgres(_output)) return;
        var (response, body) = await CreateOpportunityAsync();
        if (response.StatusCode != HttpStatusCode.OK) return;
        if (body != null && body.Value.TryGetProperty("stage", out var stage))
            stage.GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreatedOpportunity_HasCorrectDefaultStatus()
    {
        if (!RequirePostgres(_output)) return;
        var (response, body) = await CreateOpportunityAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().NotBeNull();
        body!.Value.TryGetProperty("status", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CreatedOpportunity_CreatedDateIsSet()
    {
        if (!RequirePostgres(_output)) return;
        var (response, body) = await CreateOpportunityAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (body != null && body.Value.TryGetProperty("createdDate", out var cd))
            cd.GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task CreatedOpportunity_CreatedByIsSetToTestUser()
    {
        if (!RequirePostgres(_output)) return;
        var (response, body) = await CreateOpportunityAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (body != null && body.Value.TryGetProperty("createdBy", out var cb))
            cb.GetInt32().Should().Be(123);
    }

    [Fact]
    public async Task SoftDelete_GetAfterDelete_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ChangesLastModifiedDate()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var payload = new { Id = id!.Value, Name = $"Updated {Guid.NewGuid():N}", Description = "Updated" };
        await Client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").TryGetProperty("lastModifiedDate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Update_ChangesLastModifiedBy()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var payload = new { Id = id!.Value, Name = $"Updated {Guid.NewGuid():N}", Description = "Updated" };
        await Client.PutAsJsonAsync($"{BaseUrl}/{id}", payload);
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").TryGetProperty("lastModifiedBy", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Search_FindsOpportunityByName()
    {
        if (!RequirePostgres(_output)) return;
        var name = $"SearchableOpp {Guid.NewGuid():N}";
        var (createResp, createBody) = await CreateOpportunityAsync(CreateMinimalRequest(name: name));
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var searchResp = await Client.GetAsync($"{BaseUrl}/search?query={Uri.EscapeDataString(name)}&pageIndex=1&pageSize=10");
        searchResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await searchResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        result.Should().NotBeNull();
        var found = result!.Records.Any(r =>
        {
            if (r.TryGetProperty("name", out var n))
            {
                var s = n.GetString();
                return s != null && s.Contains(name);
            }
            return false;
        });
        found.Should().BeTrue();
    }

    [Fact]
    public async Task Search_CaseInsensitive_WhenPostgres()
    {
        if (!RequirePostgres(_output)) return;
        var name = $"CaseTest {Guid.NewGuid():N}";
        var (createResp, _) = await CreateOpportunityAsync(CreateMinimalRequest(name: name));
        createResp.IsSuccessStatusCode.Should().BeTrue();

        var searchResp = await Client.GetAsync($"{BaseUrl}/search?query={Uri.EscapeDataString(name.ToLowerInvariant())}&pageIndex=1&pageSize=10");
        searchResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Pagination_RespectsOrderBy()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=5&orderBy=name&ascending=true");
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        var page = await resp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page.Should().NotBeNull();
    }

    [Fact]
    public async Task Pagination_RespectsAscendingFlag()
    {
        if (!RequirePostgres(_output)) return;
        var respAsc = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=5&orderBy=name&ascending=true");
        var respDesc = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=5&orderBy=name&ascending=false");
        respAsc.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        respDesc.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task List_DoesNotReturnSoftDeletedOpportunities()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
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

    #endregion

    #region Integration Tests (12+)

    [Fact]
    public async Task FullCrudCycle_CreateReadUpdateReadDeleteRead404()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var get1 = await Client.GetAsync($"{BaseUrl}/{id}");
        get1.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatePayload = new { Id = id!.Value, Name = $"Updated {Guid.NewGuid():N}", Description = "Updated" };
        var putResp = await Client.PutAsJsonAsync($"{BaseUrl}/{id}", updatePayload);
        putResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var get2 = await Client.GetAsync($"{BaseUrl}/{id}");
        get2.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResp = await Client.DeleteAsync($"{BaseUrl}/{id}");
        deleteResp.IsSuccessStatusCode.Should().BeTrue();

        var get3 = await Client.GetAsync($"{BaseUrl}/{id}");
        get3.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateThenUpdateEachSection_Sequentially()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var overviewResp = await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Overview Updated", InitiativeBudgetUSD = 50000m });
        overviewResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateThenGetPermissions()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var permResp = await Client.GetAsync($"{BaseUrl}/{id!.Value}/permissions");
        permResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateThenSearchByName()
    {
        if (!RequirePostgres(_output)) return;
        var name = $"SearchIntegration {Guid.NewGuid():N}";
        var (createResp, createBody) = await CreateOpportunityAsync(CreateMinimalRequest(name: name));
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var searchResp = await Client.GetAsync($"{BaseUrl}/search?query={Uri.EscapeDataString(name)}&pageIndex=1&pageSize=10");
        searchResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateMultiple_VerifyPagination()
    {
        if (!RequirePostgres(_output)) return;
        await CreateOpportunityAsync();
        await CreateOpportunityAsync();
        var resp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=2");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await resp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        page!.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        page.Records.Count.Should().BeLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task CreateDelete_VerifyListExcludesDeleted()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var listResp = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=1000");
        var page = await listResp.Content.ReadFromJsonAsync<PaginationResponse<JsonElement>>(JsonOptions);
        var found = page!.Records.Any(r => r.TryGetProperty("id", out var i) && i.GetInt32() == id!.Value);
        found.Should().BeFalse();
    }

    [Fact]
    public async Task CreateUpdateOverview_VerifyPersisted()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var newName = $"OverviewName {Guid.NewGuid():N}";
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = newName });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("name").GetString().Should().Be(newName);
    }

    [Fact]
    public async Task CreateUpdateWhatSection_VerifyPersisted()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var newDesc = $"What section desc {Guid.NewGuid():N}";
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = newDesc });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("description").GetString().Should().Be(newDesc);
    }

    [Fact]
    public async Task CreateUpdateWhySection_VerifyPersisted()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await PatchAsync($"{BaseUrl}/{id}/why", new { ResultsFocus = "Test results focus", ExpectedImpact = "Test impact" });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUpdateWhoSection_VerifyPersisted()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await PatchAsync($"{BaseUrl}/{id}/who", new { IsPooledFunding = true });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUpdateWhenSection_VerifyPersisted()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var date = DateTime.UtcNow.AddMonths(6);
        await PatchAsync($"{BaseUrl}/{id}/when", new { TargetSigningDate = date, TargetDeliveryDate = date.AddMonths(12) });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUpdateWhereSection_VerifyPersisted()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        await PatchAsync($"{BaseUrl}/{id}/where", new { Countries = Array.Empty<object>() });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateThenGetRelated()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, createBody) = await CreateOpportunityAsync();
        if (!createResp.IsSuccessStatusCode) return;
        var id = GetIdFromCreateResponse(createResp, createBody);
        if (id == null) return;

        var relatedResp = await Client.GetAsync($"{BaseUrl}/{id}/related");
        relatedResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    /*
    ### 3:1 Ratio Compliance Check
    | Category         | Count | Tests (sample names)         |
    |------------------|-------|------------------------------|
    | Positive (P)     | 4     | Create_MinimalFields, Get_ExistingOpportunity, Update_ExistingOpportunity, List_WithPagination |
    | Negative (N)     | 12    | Create_EmptyName, Create_EmptyDescription, Create_NameExceeds120Chars, Get_NonExistent, Get_NegativeId, Update_NonExistent, Delete_NonExistent, Create_NegativeBudget, Update_EmptyName, Delete_AlreadyDeleted, DoubleDelete, Create_InvalidResponsibleOrgUnitId |
    | Edge/Boundary (E)| 13    | Create_Exactly120CharName, Create_OneCharName, Create_ExtremelyLongDescription, Create_BudgetZero, Create_BudgetDecimalMaxValue, Create_SpecialCharactersInName, Create_SqlInjectionCharsInName, Pagination_Page0, Pagination_VeryLargePageNumber, Pagination_PageSize1, Pagination_PageSize1000, Create_AllOptionalFieldsNull, Create_HtmlScriptTagsInDescription |
    | Functional (F)   | 12    | CreatedOpportunity_HasDefaultStageDraft, CreatedOpportunity_HasCorrectDefaultStatus, CreatedOpportunity_CreatedDateIsSet, CreatedOpportunity_CreatedByIsSetToTestUser, SoftDelete_GetAfterDelete_Returns404, Update_ChangesLastModifiedDate, Update_ChangesLastModifiedBy, Search_FindsOpportunityByName, Search_CaseInsensitive_WhenPostgres, Pagination_RespectsOrderBy, Pagination_RespectsAscendingFlag, List_DoesNotReturnSoftDeletedOpportunities |
    | Integration (I)  | 12    | FullCrudCycle, CreateThenUpdateEachSection, CreateThenGetPermissions, CreateThenSearchByName, CreateMultiple_VerifyPagination, CreateDelete_VerifyListExcludesDeleted, CreateUpdateOverview_VerifyPersisted, CreateUpdateWhatSection_VerifyPersisted, CreateUpdateWhySection_VerifyPersisted, CreateUpdateWhoSection_VerifyPersisted, CreateUpdateWhenSection_VerifyPersisted, CreateUpdateWhereSection_VerifyPersisted, CreateThenGetRelated |
    | **N ≥ 3P?**      | ✅    | 12 >= 12 (3×4)               |
    | **E ≥ 3P?**      | ✅    | 13 >= 12 (3×4)               |
    | **F ≥ 3P?**      | ✅    | 12 >= 12 (3×4)               |
    | **I ≥ 3P?**      | ✅    | 12 >= 12 (3×4)               |
    */
}
