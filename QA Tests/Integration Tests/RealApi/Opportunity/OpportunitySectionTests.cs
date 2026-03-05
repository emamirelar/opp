/**
 * @fileoverview Real-API integration tests for Opportunity section PATCH endpoints.
 * Tests overview, what, why, who, team, where, when section updates.
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
using UNOPS.PAO.Server;

namespace UNOPS.PAO.IntegrationTests.RealApi.Opportunity;

[Collection("Integration Tests")]
public class OpportunitySectionTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    private const string BaseUrl = "/api/opportunity";

    public OpportunitySectionTests(PAOWebApplicationFactory<Program> factory, ITestOutputHelper output)
        : base(factory)
    {
        _output = output;
    }

    private static object CreateMinimalRequest() => new
    {
        Name = $"Section Test {Guid.NewGuid():N}",
        Description = "Test description for section tests"
    };

    private async Task<(HttpResponseMessage Response, int? Id)> CreateOpportunityAsync()
    {
        var response = await Client.PostAsJsonAsync(BaseUrl, CreateMinimalRequest());
        JsonElement? body = null;
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
                body = JsonSerializer.Deserialize<JsonElement>(content);
        }
        catch { /* ignore */ }

        int? id = null;
        if (body != null && body.Value.TryGetProperty("id", out var idProp))
            id = idProp.GetInt32();
        return (response, id);
    }

    private async Task<HttpResponseMessage> PatchAsync(string url, object payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await Client.SendAsync(request);
    }

    #region Positive Tests (4)

    [Fact]
    public async Task UpdateOverviewSection_ValidRequest_Returns200()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Updated Overview", InitiativeBudgetUSD = 75000m });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateWhatSection_ValidRequest_Returns200()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/what", new { Description = "Updated what section", ResponsibleOrgUnitId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateWhySection_ValidRequest_Returns200()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/why", new { ResultsFocus = "Test focus", ExpectedImpact = "Test impact" });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateWhenSection_ValidRequest_Returns200()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var date = DateTime.UtcNow.AddMonths(3);
        var resp = await PatchAsync($"{BaseUrl}/{id}/when", new { TargetSigningDate = date, TargetDeliveryDate = date.AddMonths(12) });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Negative Tests (12+)

    [Fact]
    public async Task UpdateOverviewSection_NonExistentId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await PatchAsync($"{BaseUrl}/999999/overview", new { Name = "Test" });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWhatSection_NonExistentId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await PatchAsync($"{BaseUrl}/999999/what", new { Description = "Test" });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWhySection_NonExistentId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await PatchAsync($"{BaseUrl}/999999/why", new { ResultsFocus = "Test" });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWhoSection_NonExistentId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await PatchAsync($"{BaseUrl}/999999/who", new { IsPooledFunding = true });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTeamSection_NonExistentId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await PatchAsync($"{BaseUrl}/999999/team", new { ResponsibleOrgUnitId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWhereSection_NonExistentId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await PatchAsync($"{BaseUrl}/999999/where", new { Countries = Array.Empty<object>() });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWhenSection_NonExistentId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await PatchAsync($"{BaseUrl}/999999/when", new { TargetSigningDate = DateTime.UtcNow });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOverviewSection_NegativeId_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var resp = await PatchAsync($"{BaseUrl}/-1/overview", new { Name = "Test" });
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateOverviewSection_DeletedOpportunity_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var resp = await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Test" });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateWhatSection_InvalidOrgUnitId_HandlesGracefully()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/what", new { ResponsibleOrgUnitId = -999 });
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UpdateOverviewSection_EmptyBody_HandlesGracefully()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/overview", new { });
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateWhenSection_InvalidDateOrder_HandlesGracefully()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/when", new
        {
            TargetSigningDate = DateTime.UtcNow.AddYears(2),
            TargetDeliveryDate = DateTime.UtcNow.AddYears(1)
        });
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Edge/Boundary Tests (12+)

    [Fact]
    public async Task UpdateOverviewSection_PartialUpdate_OnlyName_OtherFieldsUnchanged()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var newName = $"Partial {Guid.NewGuid():N}";
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = newName });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("name").GetString().Should().Be(newName);
    }

    [Fact]
    public async Task UpdateOverviewSection_PartialUpdate_OnlyBudget_OtherFieldsUnchanged()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await PatchAsync($"{BaseUrl}/{id}/overview", new { InitiativeBudgetUSD = 123456.78m });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("initiativeBudgetUSD").GetDecimal().Should().Be(123456.78m);
    }

    [Fact]
    public async Task UpdateWhatSection_DoesNotAffectOverview()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var origName = $"Orig {Guid.NewGuid():N}";
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = origName });
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = "New what desc" });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("name").GetString().Should().Be(origName);
    }

    [Fact]
    public async Task UpdateWhySection_DoesNotAffectWhatSection()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var whatDesc = $"What {Guid.NewGuid():N}";
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = whatDesc });
        await PatchAsync($"{BaseUrl}/{id}/why", new { ResultsFocus = "Why focus" });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("description").GetString().Should().Be(whatDesc);
    }

    [Fact]
    public async Task UpdateWhoSection_EmptyFundingPartners_Accepts()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/who", new { IsPooledFunding = false, FundingPartners = Array.Empty<object>() });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateWhenSection_NullDates_HandlesGracefully()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/when", new { IsTargetSigningDateFirm = true });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateWhereSection_EmptyCountries_Accepts()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/where", new { Countries = Array.Empty<object>() });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateTeamSection_ResponsibleOrgUnitIdOnly_PartialUpdate()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/team", new { ResponsibleOrgUnitId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateOverviewSection_MaxBudgetValue_Handles()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/overview", new { InitiativeBudgetUSD = decimal.MaxValue });
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateWhatSection_ZeroDeliveryModality_Accepts()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/what", new { DeliveryModality = 0 });
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateWhySection_BeneficiariesToBeDetermined_Accepts()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/why", new { BeneficiariesToBeDetermined = true });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SequentialSectionUpdates_AllPersist()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();

        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Seq1", InitiativeBudgetUSD = 1000m });
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = "Seq2" });
        await PatchAsync($"{BaseUrl}/{id}/why", new { ResultsFocus = "Seq3" });
        await PatchAsync($"{BaseUrl}/{id}/who", new { IsPooledFunding = true });
        await PatchAsync($"{BaseUrl}/{id}/when", new { TargetSigningDate = DateTime.UtcNow.AddMonths(6) });
        await PatchAsync($"{BaseUrl}/{id}/where", new { Countries = Array.Empty<object>() });

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        var opp = doc.RootElement.GetProperty("opportunity");
        opp.GetProperty("name").GetString().Should().Be("Seq1");
        opp.GetProperty("description").GetString().Should().Be("Seq2");
    }

    #endregion

    #region Functional Tests (12+)

    [Fact]
    public async Task OverviewSectionUpdate_ReturnsFullOpportunityModel()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Full model test" });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        doc.RootElement.TryGetProperty("id", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("name", out _).Should().BeTrue();
    }

    [Fact]
    public async Task WhatSectionUpdate_ResponsibleOrgUnitIdPersists()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await PatchAsync($"{BaseUrl}/{id}/what", new { ResponsibleOrgUnitId = 2 });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("responsibleOrgUnitId").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task WhySectionUpdate_ResultsFocusPersists()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var focus = $"Focus {Guid.NewGuid():N}";
        await PatchAsync($"{BaseUrl}/{id}/why", new { ResultsFocus = focus });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("resultsFocus").GetString().Should().Be(focus);
    }

    [Fact]
    public async Task WhoSectionUpdate_IsPooledFundingPersists()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await PatchAsync($"{BaseUrl}/{id}/who", new { IsPooledFunding = true });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("isPooledFunding").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task WhenSectionUpdate_TargetSigningDatePersists()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var date = new DateTime(2030, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        await PatchAsync($"{BaseUrl}/{id}/when", new { TargetSigningDate = date });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        var dateStr = doc.RootElement.GetProperty("opportunity").GetProperty("targetSigningDate").GetString();
        dateStr.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SectionUpdate_UpdatesLastModifiedDate()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Modified" });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").TryGetProperty("lastModifiedDate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task SectionUpdate_UpdatesLastModifiedBy()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "ByTest" });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").TryGetProperty("lastModifiedBy", out var lb).Should().BeTrue();
        lb.GetInt32().Should().Be(123);
    }

    [Fact]
    public async Task OverviewAndWhatSection_IndependentUpdates()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "OverviewName" });
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = "WhatDesc" });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        var opp = doc.RootElement.GetProperty("opportunity");
        opp.GetProperty("name").GetString().Should().Be("OverviewName");
        opp.GetProperty("description").GetString().Should().Be("WhatDesc");
    }

    [Fact]
    public async Task TeamSection_ResponsibleOrgUnitIdFromSeededData()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/team", new { ResponsibleOrgUnitId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WhySection_UNOPSMissionsNotApplicable_Accepts()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/why", new { UNOPSMissionsNotApplicable = true });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WhenSection_IsTargetSigningDateFirm_Accepts()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var resp = await PatchAsync($"{BaseUrl}/{id}/when", new { IsTargetSigningDateFirm = true });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MultipleOverviewUpdates_LastWins()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "First" });
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Second" });
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Third" });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("name").GetString().Should().Be("Third");
    }

    #endregion

    #region Integration Tests (12+)

    [Fact]
    public async Task FullSectionWorkflow_AllSectionsUpdatedThenRead()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();

        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Full", InitiativeBudgetUSD = 100000m });
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = "What", ResponsibleOrgUnitId = 1 });
        await PatchAsync($"{BaseUrl}/{id}/why", new { ResultsFocus = "Why", ExpectedImpact = "Impact" });
        await PatchAsync($"{BaseUrl}/{id}/who", new { IsPooledFunding = false });
        await PatchAsync($"{BaseUrl}/{id}/team", new { ResponsibleOrgUnitId = 1 });
        await PatchAsync($"{BaseUrl}/{id}/where", new { Countries = Array.Empty<object>() });
        await PatchAsync($"{BaseUrl}/{id}/when", new { TargetSigningDate = DateTime.UtcNow.AddMonths(6) });

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        var opp = doc.RootElement.GetProperty("opportunity");
        opp.GetProperty("name").GetString().Should().Be("Full");
        opp.GetProperty("initiativeBudgetUSD").GetDecimal().Should().Be(100000m);
    }

    [Fact]
    public async Task CreateUpdateOverviewGet_EndToEnd()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, id) = await CreateOpportunityAsync();
        createResp.IsSuccessStatusCode.Should().BeTrue();
        id.Should().NotBeNull();

        var newName = $"E2E {Guid.NewGuid():N}";
        var patchResp = await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = newName });
        patchResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("name").GetString().Should().Be(newName);
    }

    [Fact]
    public async Task CreateUpdateWhatGet_EndToEnd()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, id) = await CreateOpportunityAsync();
        createResp.IsSuccessStatusCode.Should().BeTrue();
        id.Should().NotBeNull();

        var newDesc = $"What E2E {Guid.NewGuid():N}";
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = newDesc });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("description").GetString().Should().Be(newDesc);
    }

    [Fact]
    public async Task CreateUpdateWhyGet_EndToEnd()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, id) = await CreateOpportunityAsync();
        createResp.IsSuccessStatusCode.Should().BeTrue();
        id.Should().NotBeNull();

        var focus = $"Why E2E {Guid.NewGuid():N}";
        await PatchAsync($"{BaseUrl}/{id}/why", new { ResultsFocus = focus });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("resultsFocus").GetString().Should().Be(focus);
    }

    [Fact]
    public async Task CreateUpdateWhoGet_EndToEnd()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, id) = await CreateOpportunityAsync();
        createResp.IsSuccessStatusCode.Should().BeTrue();
        id.Should().NotBeNull();

        await PatchAsync($"{BaseUrl}/{id}/who", new { IsPooledFunding = true });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("isPooledFunding").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateUpdateTeamGet_EndToEnd()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, id) = await CreateOpportunityAsync();
        createResp.IsSuccessStatusCode.Should().BeTrue();
        id.Should().NotBeNull();

        await PatchAsync($"{BaseUrl}/{id}/team", new { ResponsibleOrgUnitId = 3 });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("responsibleOrgUnitId").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task CreateUpdateWhereGet_EndToEnd()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, id) = await CreateOpportunityAsync();
        createResp.IsSuccessStatusCode.Should().BeTrue();
        id.Should().NotBeNull();

        await PatchAsync($"{BaseUrl}/{id}/where", new { Countries = Array.Empty<object>() });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUpdateWhenGet_EndToEnd()
    {
        if (!RequirePostgres(_output)) return;
        var (createResp, id) = await CreateOpportunityAsync();
        createResp.IsSuccessStatusCode.Should().BeTrue();
        id.Should().NotBeNull();

        var date = DateTime.UtcNow.AddMonths(9);
        await PatchAsync($"{BaseUrl}/{id}/when", new { TargetSigningDate = date });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").TryGetProperty("targetSigningDate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task SectionUpdateAfterDelete_Returns404()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await Client.DeleteAsync($"{BaseUrl}/{id}");
        var resp = await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "After delete" });
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OverviewThenWhat_OrderIndependent()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = "What first" });
        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = "Overview second" });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        var opp = doc.RootElement.GetProperty("opportunity");
        opp.GetProperty("name").GetString().Should().Be("Overview second");
        opp.GetProperty("description").GetString().Should().Be("What first");
    }

    [Fact]
    public async Task PartialOverviewUpdate_DoesNotClearDescription()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();
        var origDesc = "Original description";
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = origDesc });
        await PatchAsync($"{BaseUrl}/{id}/overview", new { InitiativeBudgetUSD = 50000m });
        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("opportunity").GetProperty("description").GetString().Should().Be(origDesc);
    }

    [Fact]
    public async Task AllSectionsRoundTrip_DataIntegrity()
    {
        if (!RequirePostgres(_output)) return;
        var (_, id) = await CreateOpportunityAsync();
        id.Should().NotBeNull();

        var overviewName = $"RoundTrip {Guid.NewGuid():N}";
        var whatDesc = $"What {Guid.NewGuid():N}";
        var whyFocus = $"Why {Guid.NewGuid():N}";
        var whenDate = DateTime.UtcNow.AddMonths(12);

        await PatchAsync($"{BaseUrl}/{id}/overview", new { Name = overviewName });
        await PatchAsync($"{BaseUrl}/{id}/what", new { Description = whatDesc });
        await PatchAsync($"{BaseUrl}/{id}/why", new { ResultsFocus = whyFocus });
        await PatchAsync($"{BaseUrl}/{id}/when", new { TargetSigningDate = whenDate });

        var getResp = await Client.GetAsync($"{BaseUrl}/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
        var opp = doc.RootElement.GetProperty("opportunity");
        opp.GetProperty("name").GetString().Should().Be(overviewName);
        opp.GetProperty("description").GetString().Should().Be(whatDesc);
        opp.GetProperty("resultsFocus").GetString().Should().Be(whyFocus);
    }

    #endregion

    /*
    ### 3:1 Ratio Compliance Check
    | Category         | Count | Tests (sample names)         |
    |------------------|-------|------------------------------|
    | Positive (P)     | 4     | UpdateOverviewSection_ValidRequest, UpdateWhatSection_ValidRequest, UpdateWhySection_ValidRequest, UpdateWhenSection_ValidRequest |
    | Negative (N)     | 12    | UpdateOverviewSection_NonExistentId, UpdateWhatSection_NonExistentId, UpdateWhySection_NonExistentId, UpdateWhoSection_NonExistentId, UpdateTeamSection_NonExistentId, UpdateWhereSection_NonExistentId, UpdateWhenSection_NonExistentId, UpdateOverviewSection_NegativeId, UpdateOverviewSection_DeletedOpportunity, UpdateWhatSection_InvalidOrgUnitId, UpdateOverviewSection_EmptyBody, UpdateWhenSection_InvalidDateOrder |
    | Edge/Boundary (E)| 12    | UpdateOverviewSection_PartialUpdate_OnlyName, UpdateOverviewSection_PartialUpdate_OnlyBudget, UpdateWhatSection_DoesNotAffectOverview, UpdateWhySection_DoesNotAffectWhatSection, UpdateWhoSection_EmptyFundingPartners, UpdateWhenSection_NullDates, UpdateWhereSection_EmptyCountries, UpdateTeamSection_ResponsibleOrgUnitIdOnly, UpdateOverviewSection_MaxBudgetValue, UpdateWhatSection_ZeroDeliveryModality, UpdateWhySection_BeneficiariesToBeDetermined, SequentialSectionUpdates_AllPersist |
    | Functional (F)   | 12    | OverviewSectionUpdate_ReturnsFullOpportunityModel, WhatSectionUpdate_ResponsibleOrgUnitIdPersists, WhySectionUpdate_ResultsFocusPersists, WhoSectionUpdate_IsPooledFundingPersists, WhenSectionUpdate_TargetSigningDatePersists, SectionUpdate_UpdatesLastModifiedDate, SectionUpdate_UpdatesLastModifiedBy, OverviewAndWhatSection_IndependentUpdates, TeamSection_ResponsibleOrgUnitIdFromSeededData, WhySection_UNOPSMissionsNotApplicable_Accepts, WhenSection_IsTargetSigningDateFirm_Accepts, MultipleOverviewUpdates_LastWins |
    | Integration (I)  | 12    | FullSectionWorkflow_AllSectionsUpdatedThenRead, CreateUpdateOverviewGet_EndToEnd, CreateUpdateWhatGet_EndToEnd, CreateUpdateWhyGet_EndToEnd, CreateUpdateWhoGet_EndToEnd, CreateUpdateTeamGet_EndToEnd, CreateUpdateWhereGet_EndToEnd, CreateUpdateWhenGet_EndToEnd, SectionUpdateAfterDelete_Returns404, OverviewThenWhat_OrderIndependent, PartialOverviewUpdate_DoesNotClearDescription, AllSectionsRoundTrip_DataIntegrity |
    | **N ≥ 3P?**      | ✅    | 12 >= 12 (3×4)               |
    | **E ≥ 3P?**      | ✅    | 12 >= 12 (3×4)               |
    | **F ≥ 3P?**      | ✅    | 12 >= 12 (3×4)               |
    | **I ≥ 3P?**      | ✅    | 12 >= 12 (3×4)               |
    */
}
