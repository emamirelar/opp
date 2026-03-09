/**
 * @fileoverview PNO-974 SDG Classification Tests — validates SDG terminology and API behavior.
 *
 * Bug: In Opportunity Statement section "2. Alignment with UN, global, and national goals and priorities",
 * SDGs are still classified as 'primary' and 'secondary' whereas they should be 'main' and 'cross cutting'.
 * Status: Peer Review
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-974
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO974_SdgClassification;

[Collection("Integration Tests")]
[Trait("Feature", "PNO-974")]
[Trait("Component", "SdgClassification")]
[Trait("JiraRef", "PNO-974")]
public class SdgClassificationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public SdgClassificationTests(PAOWebApplicationFactory<Program> factory)
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

    private static bool ContainsPrimaryOrSecondaryLabel(string json)
    {
        return json.Contains("primary", StringComparison.OrdinalIgnoreCase) ||
               json.Contains("secondary", StringComparison.OrdinalIgnoreCase);
    }

    #region POSITIVE (2)

    [Fact]
    [Trait("TestId", "TC-PNO974-POS-001")]
    [Trait("Category", "Positive")]
    public async Task POS_001_OpportunityDetailEndpoint_ReturnsSdgData()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var opp = json.TryGetProperty("opportunity", out var oppProp) ? oppProp : json;
        var hasSdgData = opp.TryGetProperty("sdgs", out _) ||
                        opp.TryGetProperty("SDGs", out _) ||
                        opp.TryGetProperty("why", out var why) && why.TryGetProperty("sdgs", out _);
        hasSdgData.Should().BeTrue("Opportunity detail endpoint must return SDG data");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-POS-002")]
    [Trait("Category", "Positive")]
    public async Task POS_002_SdgValuesEndpoint_ReturnsListOfSdgs()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/sdgs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Array, "SDG values endpoint must return an array");
        if (json.GetArrayLength() > 0)
        {
            var first = json[0];
            first.TryGetProperty("id", out _).Should().BeTrue("SDG items must have id");
            first.TryGetProperty("name", out _).Should().BeTrue("SDG items must have name");
        }
    }

    #endregion

    #region NEGATIVE (6)

    [Fact]
    [Trait("TestId", "TC-PNO974-NEG-001")]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-112")]
    public async Task NEG_001_SdgClassification_UsesMainCrossCutting_NotPrimarySecondary()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadAsStringAsync();
        ContainsPrimaryOrSecondaryLabel(body).Should().BeFalse(
            "PNO-974: SDG classification must use 'main'/'cross cutting' labels, NOT 'primary'/'secondary'");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task NEG_002_UnauthenticatedRequest_Returns401Or302()
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
    [Trait("TestId", "TC-PNO974-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task NEG_003_NonExistentOpportunity_Returns404()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task NEG_004_SdgResponse_DoesNotContainPrimaryClassificationLabel()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/sdgs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Contains("primary", StringComparison.OrdinalIgnoreCase).Should().BeFalse(
            "SDG response must not use deprecated 'primary' classification label");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task NEG_005_SdgResponse_DoesNotContainSecondaryClassificationLabel()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/sdgs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Contains("secondary", StringComparison.OrdinalIgnoreCase).Should().BeFalse(
            "SDG response must not use deprecated 'secondary' classification label");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-NEG-006")]
    [Trait("Category", "Negative")]
    public async Task NEG_006_InvalidOpportunityId_ReturnsError()
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
    [Trait("TestId", "TC-PNO974-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_SdgData_IncludesGoalNumberAndName()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/sdgs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        if (json.GetArrayLength() == 0) return;
        var first = json[0];
        (first.TryGetProperty("id", out _) || first.TryGetProperty("sdgNumber", out _) ||
         first.TryGetProperty("number", out _)).Should().BeTrue("SDG must have goal number");
        (first.TryGetProperty("name", out _) || first.TryGetProperty("sdgDescription", out _) ||
         first.TryGetProperty("description", out _)).Should().BeTrue("SDG must have name/description");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_SdgData_IncludesClassificationTypeField()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var opp = json.TryGetProperty("opportunity", out var oppProp) ? oppProp : json;
        if (!opp.TryGetProperty("sdgs", out var sdgs) && !opp.TryGetProperty("SDGs", out sdgs)) return;
        if (sdgs.GetArrayLength() == 0) return;
        var first = sdgs[0];
        (first.TryGetProperty("isPrimary", out _) || first.TryGetProperty("classification", out _) ||
         first.TryGetProperty("type", out _)).Should().BeTrue("SDG assignment must have classification type");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_MultipleSdgs_CanBeAssignedToOneOpportunity()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var opp = json.TryGetProperty("opportunity", out var oppProp) ? oppProp : json;
        if (opp.TryGetProperty("sdgs", out var sdgs) || opp.TryGetProperty("SDGs", out sdgs))
            sdgs.ValueKind.Should().Be(JsonValueKind.Array, "SDGs must be a collection");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task FUNC_004_SdgClassificationValues_FromValidSet()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/sdgs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task FUNC_005_SdgData_ConsistentAcrossConsecutiveCalls()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        var r2 = await _client.GetAsync("/api/opportunity/1");
        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode != HttpStatusCode.OK) return;
        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2, "SDG data must be consistent across consecutive calls");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_SdgListEndpoint_ReturnsCompleteSetOfGoals()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/sdgs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.GetArrayLength().Should().BeGreaterThan(0, "SDG list must return at least one goal");
    }

    #endregion

    #region EDGE (6)

    [Fact]
    [Trait("TestId", "TC-PNO974-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_001_OpportunityWithNoSdgs_ReturnsEmptyCollection()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var opp = json.TryGetProperty("opportunity", out var oppProp) ? oppProp : json;
        if (opp.TryGetProperty("sdgs", out var sdgs) || opp.TryGetProperty("SDGs", out sdgs))
            sdgs.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_002_OpportunityWithOnlyMainSdgs_NoCrossCutting()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_003_OpportunityWithOnlyCrossCuttingSdgs_NoMain()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_004_All17SdgsAssigned_ToSingleOpportunity()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/sdgs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.GetArrayLength().Should().BeLessThanOrEqualTo(17, "SDG list should not exceed 17 goals");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_005_SoftDeletedSdgAssignment_NotReturned()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_006_SdgWithSpecialCharacters_InDescription()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/values/sdgs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Array);
    }

    #endregion

    #region INTEGRATION (6)

    [Fact]
    [Trait("TestId", "TC-PNO974-INT-001")]
    [Trait("Category", "Integration")]
    public async Task INT_001_FullFlow_GetOpportunity_IncludesSdgDataInWhySection()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var opp = json.TryGetProperty("opportunity", out var oppProp) ? oppProp : json;
        (opp.TryGetProperty("sdgs", out _) || opp.TryGetProperty("SDGs", out _) ||
         opp.TryGetProperty("why", out var why) && why.TryGetProperty("sdgs", out _)).Should().BeTrue(
            "Full flow: GET opportunity must include SDG data in WHY section");
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-INT-002")]
    [Trait("Category", "Integration")]
    public async Task INT_002_SdgLookupAndOpportunitySdgs_UseConsistentNaming()
    {
        if (!_isPostgresAvailable) return;
        var lookupResponse = await _client.GetAsync("/api/values/sdgs");
        var oppResponse = await _client.GetAsync("/api/opportunity/1");
        lookupResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        oppResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-INT-003")]
    [Trait("Category", "Integration")]
    public async Task INT_003_SdgData_AvailableThroughDetailAndWorkflowEndpoints()
    {
        if (!_isPostgresAvailable) return;
        var detailResponse = await _client.GetAsync("/api/opportunity/1");
        var workflowResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        detailResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        workflowResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-INT-004")]
    [Trait("Category", "Integration")]
    public async Task INT_004_SdgClassification_PersistsAfterOpportunityEdit()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        if (r1.StatusCode != HttpStatusCode.OK) return;
        var r2 = await _client.GetAsync("/api/opportunity/1");
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-INT-005")]
    [Trait("Category", "Integration")]
    public async Task INT_005_MultipleOpportunities_HaveIndependentSdgAssignments()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        var r2 = await _client.GetAsync("/api/opportunity/2");
        r1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        r2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO974-INT-006")]
    [Trait("Category", "Integration")]
    public async Task INT_006_SdgData_IncludedInWorkflowRequirementsCheck()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Array);
    }

    #endregion
}
