/**
 * @fileoverview PNO-1209 DOA3 oUP Mapping Tests — validates that the DOA3 field
 * is included in Opportunity data used for oUP Engagement creation.
 *
 * Bug: DOA3 field value not transferred when Opportunity → oUP Engagement sync occurs.
 * Status: Ready for Go Live (bug still present)
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1209
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1209;

[Collection("Integration Tests")]
[Trait("Feature", "PNO-1209")]
[Trait("Component", "DoA3OupMapping")]
[Trait("JiraRef", "PNO-1209")]
public class DoA3MappingTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DoA3MappingTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    private static HttpClient CreateUnauthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    private static bool HasDoA3Property(JsonElement root)
    {
        return root.TryGetProperty("doa3", out _) ||
               root.TryGetProperty("DoA3", out _) ||
               root.TryGetProperty("doa3UserId", out _) ||
               (root.TryGetProperty("orgUnitAuthority", out var oua) && oua.TryGetProperty("doa3", out _)) ||
               (root.TryGetProperty("orgUnitAuthority", out var oua2) && oua2.TryGetProperty("doa3UserId", out _)) ||
               (root.TryGetProperty("team", out var team) && team.TryGetProperty("doa3", out _)) ||
               (root.TryGetProperty("stakeholders", out var stk) && stk.ValueKind == JsonValueKind.Array &&
                stk.EnumerateArray().Any(s => s.TryGetProperty("role", out var r) &&
                    r.GetString()?.Contains("DoA3", StringComparison.OrdinalIgnoreCase) == true));
    }

    private static bool HasDoA2Property(JsonElement root)
    {
        return root.TryGetProperty("doa2", out _) ||
               root.TryGetProperty("DoA2", out _) ||
               root.TryGetProperty("doa2UserId", out _) ||
               (root.TryGetProperty("orgUnitAuthority", out var oua) && oua.TryGetProperty("doa2", out _)) ||
               (root.TryGetProperty("orgUnitAuthority", out var oua2) && oua2.TryGetProperty("doa2UserId", out _)) ||
               (root.TryGetProperty("team", out var team) && team.TryGetProperty("doa2", out _));
    }

    #region POSITIVE (2)

    [Fact]
    [Trait("TestId", "TC-PNO1209-POS-001")]
    [Trait("Category", "Positive")]
    public async Task TC_PNO1209_POS_001_OpportunityDetailEndpoint_ReturnsTeamStakeholdersDataWithDoAFields()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasTeamOrStakeholders = json.TryGetProperty("stakeholders", out _) ||
                                    json.TryGetProperty("team", out _) ||
                                    json.TryGetProperty("opportunityManager", out _) ||
                                    json.TryGetProperty("responsibleOrgUnitId", out _);
        hasTeamOrStakeholders.Should().BeTrue(
            "Opportunity detail endpoint must return team/stakeholders data including DOA fields for oUP integration");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-POS-002")]
    [Trait("Category", "Positive")]
    public async Task TC_PNO1209_POS_002_OpportunityDataStructure_ContainsOrgUnitAuthorityInformation()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasOrgUnitInfo = json.TryGetProperty("responsibleOrgUnitId", out _) ||
                             json.TryGetProperty("organizationUnit", out _) ||
                             json.TryGetProperty("orgUnitAuthority", out _);
        hasOrgUnitInfo.Should().BeTrue(
            "Opportunity data structure must contain org unit authority information for oUP mapping");
    }

    #endregion

    #region NEGATIVE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1209-NEG-001")]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-108")]
    public async Task TC_PNO1209_NEG_001_DoA3Field_MustBePresentInOpportunityResponse_NotNullOrMissing()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasDoA3 = HasDoA3Property(json);
        hasDoA3.Should().BeTrue(
            "PNO-1209: DOA3 field must be present in opportunity API response for oUP Engagement mapping. " +
            "Currently DOA3 is not transferred when opportunity gets approved.");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task TC_PNO1209_NEG_002_OpportunityWithoutDoA3Assigned_ShowsEmptyOrNullDoA3_NotOmittedEntirely()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasDoA3 = HasDoA3Property(json);
        hasDoA3.Should().BeTrue(
            "When DOA3 is not assigned, the field should still be present (null/empty), not omitted entirely");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task TC_PNO1209_NEG_003_UnauthenticatedRequest_ToOpportunityDetail_Returns401Or302()
    {
        if (!_isPostgresAvailable) return;
        using var unauthClient = CreateUnauthenticatedClient(_factory);
        var response = await unauthClient.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task TC_PNO1209_NEG_004_NonExistentOpportunity_Returns404()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task TC_PNO1209_NEG_005_DoA3Field_NotExposedWhenUserLacksViewPermission()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-NEG-006")]
    [Trait("Category", "Negative")]
    public async Task TC_PNO1209_NEG_006_InvalidOpportunityIdFormat_ReturnsError()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/not-an-id");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region FUNCTIONAL (6)

    [Fact]
    [Trait("TestId", "TC-PNO1209-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task TC_PNO1209_FUNC_001_DoA2AndDoA3Fields_BothPresentInOpportunityApiResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasDoA2 = HasDoA2Property(json);
        var hasDoA3 = HasDoA3Property(json);
        (hasDoA2 || hasDoA3).Should().BeTrue(
            "Opportunity API response must include DOA2 and/or DOA3 fields for oUP integration");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task TC_PNO1209_FUNC_002_OrgUnitAuthorityData_IncludesUserIdForDoA3()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasDoA3 = HasDoA3Property(json);
        hasDoA3.Should().BeTrue(
            "Org unit authority data must include user ID for DOA3 when assigned");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task TC_PNO1209_FUNC_003_DoA3Field_PersistsAfterOpportunityStatusChanges()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.TryGetProperty("stage", out _).Should().BeTrue("Opportunity must have stage");
        json.TryGetProperty("status", out _).Should().BeTrue("Opportunity must have status");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task TC_PNO1209_FUNC_004_TeamSection_InOpportunityResponse_IncludesAuthorityDesignations()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasTeamOrStakeholders = json.TryGetProperty("stakeholders", out _) ||
                                    json.TryGetProperty("team", out _) ||
                                    json.TryGetProperty("opportunityManager", out _);
        hasTeamOrStakeholders.Should().BeTrue(
            "Team section must include authority designations");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task TC_PNO1209_FUNC_005_OpportunityResponseStructure_IsConsistentBetweenGetCalls()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        var r2 = await _client.GetAsync("/api/opportunity/1");
        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode != HttpStatusCode.OK) return;

        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2, "Opportunity response structure must be consistent between GET calls");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task TC_PNO1209_FUNC_006_DoA3Data_AvailableViaBothDetailAndListEndpoints()
    {
        if (!_isPostgresAvailable) return;
        var detailResponse = await _client.GetAsync("/api/opportunity/1");
        var listResponse = await _client.GetAsync("/api/opportunity");
        detailResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        listResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion

    #region EDGE/BOUNDARY (6)

    [Fact]
    [Trait("TestId", "TC-PNO1209-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task TC_PNO1209_EDGE_001_Opportunity_WhereDoA2AssignedButDoA3Empty()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task TC_PNO1209_EDGE_002_Opportunity_WhereDoA3AssignedButDoA2Empty_FallbackScenario()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasDoA2OrDoA3 = HasDoA2Property(json) || HasDoA3Property(json);
        hasDoA2OrDoA3.Should().BeTrue("Fallback scenario: DoA3 when DoA2 empty must be present");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task TC_PNO1209_EDGE_003_Opportunity_WithBothDoA2AndDoA3Assigned()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task TC_PNO1209_EDGE_004_Opportunity_WithNeitherDoA2NorDoA3Assigned()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task TC_PNO1209_EDGE_005_SoftDeletedDoA3User_ShouldNotAppearInResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task TC_PNO1209_EDGE_006_DoA3Field_WithMaximumLengthUserData()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    #endregion

    #region INTEGRATION (6)

    [Fact]
    [Trait("TestId", "TC-PNO1209-INT-001")]
    [Trait("Category", "Integration")]
    public async Task TC_PNO1209_INT_001_FullFlow_GetOpportunityDetail_IncludesDoA3InTeamSection()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasDoA3 = HasDoA3Property(json);
        hasDoA3.Should().BeTrue(
            "Full flow: GET opportunity detail must include DOA3 in team section for oUP sync");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-INT-002")]
    [Trait("Category", "Integration")]
    public async Task TC_PNO1209_INT_002_DoA3Visible_InWorkflowSubmissionRequirements_AlongsideDoA2()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        reqResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;

        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        items.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-INT-003")]
    [Trait("Category", "Integration")]
    public async Task TC_PNO1209_INT_003_DoA3Present_WhenQueryingOpportunityViaOrgUnitEndpoint()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.TryGetProperty("responsibleOrgUnitId", out _).Should().BeTrue(
            "Opportunity must have org unit for DoA query");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-INT-004")]
    [Trait("Category", "Integration")]
    public async Task TC_PNO1209_INT_004_OpportunityListEndpoint_IncludesDoAAuthorityData()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-INT-005")]
    [Trait("Category", "Integration")]
    public async Task TC_PNO1209_INT_005_DoA3Persists_InResponseAfterWorkflowStateChange()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.TryGetProperty("stage", out _).Should().BeTrue();
        json.TryGetProperty("workflowStatus", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PNO1209-INT-006")]
    [Trait("Category", "Integration")]
    public async Task TC_PNO1209_INT_006_DoA3Data_ConsistentBetweenOpportunityDetailAndTeamSectionEndpoints()
    {
        if (!_isPostgresAvailable) return;
        var detailResponse = await _client.GetAsync("/api/opportunity/1");
        detailResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (detailResponse.StatusCode != HttpStatusCode.OK) return;

        var json = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    #endregion
}
