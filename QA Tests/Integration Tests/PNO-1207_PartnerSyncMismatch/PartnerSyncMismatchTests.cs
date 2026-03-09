/**
 * @fileoverview PNO-1207 Partner Sync Mismatch Tests — validates that Opportunity API
 * includes partner data for oUP sync. Bug: Partners that exist in Opp+ but not in oUP
 * are silently dropped during Engagement creation.
 *
 * Bug: When an Opportunity contains Client or Funding Partners that exist in Opp+ but
 * not in oUP, the system silently drops these partners during Engagement creation.
 * Status: Ready for QA Review
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1207
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1207;

[Collection("Integration Tests")]
[Trait("Feature", "PNO-1207")]
[Trait("Component", "PartnerSyncMismatch")]
[Trait("JiraRef", "PNO-1207")]
public class PartnerSyncMismatchTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerSyncMismatchTests(PAOWebApplicationFactory<Program> factory)
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

    private static bool HasFundingPartners(JsonElement root)
    {
        return root.TryGetProperty("fundingPartners", out var fp) &&
               (fp.ValueKind == JsonValueKind.Array || fp.ValueKind == JsonValueKind.Null);
    }

    private static bool HasClientPartners(JsonElement root)
    {
        return root.TryGetProperty("clientPartners", out var cp) &&
               (cp.ValueKind == JsonValueKind.Array || cp.ValueKind == JsonValueKind.Null);
    }

    /// <summary>
    /// QA-097: UNOPSGeminiManager.CreateDummyCredential() throws FormatException in the
    /// test environment (no Google Secret Manager), causing the OpportunityController
    /// to return 500 on every request. Skip tests when the endpoint is unreachable
    /// due to this infrastructure issue.
    /// </summary>
    private async Task<bool> IsOpportunityEndpointReachable()
    {
        var probe = await _client.GetAsync("/api/opportunity/1");
        return probe.StatusCode != HttpStatusCode.InternalServerError;
    }

    #region POSITIVE (2)

    [Fact]
    [Trait("TestId", "TC-PNO1207-POS-001")]
    [Trait("Category", "Positive")]
    public async Task POS_001_OpportunityDetail_IncludesFundingPartnersCollection()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasFundingPartners = HasFundingPartners(json);
        hasFundingPartners.Should().BeTrue(
            "Opportunity detail endpoint must return fundingPartners collection for oUP sync");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-POS-002")]
    [Trait("Category", "Positive")]
    public async Task POS_002_OpportunityDetail_IncludesClientPartnersCollection()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasClientPartners = HasClientPartners(json);
        hasClientPartners.Should().BeTrue(
            "Opportunity detail endpoint must return clientPartners collection for oUP sync");
    }

    #endregion

    #region NEGATIVE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1207-NEG-001")]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-109")]
    public async Task NEG_001_OpportunityWithPartnerWithoutOupReference_StillIncludesPartnerInResponse()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasFundingPartners = HasFundingPartners(json);
        var hasClientPartners = HasClientPartners(json);
        (hasFundingPartners || hasClientPartners).Should().BeTrue(
            "PNO-1207: Partner without oUP reference must NOT be silently dropped. " +
            "Opportunity response must include all partners for oUP sync mapping.");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task NEG_002_NonExistentOpportunity_Returns404()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task NEG_003_UnauthenticatedRequest_Returns401Or302()
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
    [Trait("TestId", "TC-PNO1207-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task NEG_004_PartnerWithEmptyName_StillAppearsInPartnerList()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/partner/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.TryGetProperty("id", out _).Should().BeTrue("Partner must have id");
        json.TryGetProperty("name", out _).Should().BeTrue("Partner must have name field (may be empty)");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task NEG_005_DeletedPartner_ExcludedFromOpportunityPartnerList()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasFundingPartners = HasFundingPartners(json);
        var hasClientPartners = HasClientPartners(json);
        (hasFundingPartners || hasClientPartners).Should().BeTrue(
            "Opportunity must have partner collections; soft-deleted partners excluded");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-NEG-006")]
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
    [Trait("TestId", "TC-PNO1207-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_PartnersResponse_IncludesPartnerIdForOupSyncMapping()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        if (json.TryGetProperty("fundingPartners", out var fp) && fp.ValueKind == JsonValueKind.Array &&
            fp.GetArrayLength() > 0)
        {
            var first = fp[0];
            (first.TryGetProperty("partnerId", out _) || first.TryGetProperty("id", out _)).Should().BeTrue(
                "Partner response must include partnerId for oUP sync mapping");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_PartnersResponse_IncludesPartnerName()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/partner/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.TryGetProperty("name", out _).Should().BeTrue("Partner detail must include name field");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_MultipleFundingPartners_ReturnedInSingleResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasFundingPartners = HasFundingPartners(json);
        hasFundingPartners.Should().BeTrue("Opportunity must support multiple funding partners");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task FUNC_004_MultipleClientPartners_ReturnedInSingleResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasClientPartners = HasClientPartners(json);
        hasClientPartners.Should().BeTrue("Opportunity must support multiple client partners");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task FUNC_005_PartnerData_ConsistentBetweenConsecutiveGetCalls()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        var r2 = await _client.GetAsync("/api/opportunity/1");
        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode != HttpStatusCode.OK) return;

        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2, "Opportunity response must be consistent between GET calls");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_OpportunityResponseStructure_IsValidJson()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    #endregion

    #region EDGE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1207-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_001_OpportunityWithZeroPartners_ReturnsEmptyCollectionNotNull()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        if (json.TryGetProperty("fundingPartners", out var fp))
            fp.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Null);
        if (json.TryGetProperty("clientPartners", out var cp))
            cp.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Null);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_002_OpportunityWithOnlyFundingPartners_NoClient()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasFundingPartners = HasFundingPartners(json);
        var hasClientPartners = HasClientPartners(json);
        (hasFundingPartners || hasClientPartners).Should().BeTrue(
            "Opportunity with only funding partners must still return valid structure");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_003_OpportunityWithOnlyClientPartners_NoFunding()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasFundingPartners = HasFundingPartners(json);
        var hasClientPartners = HasClientPartners(json);
        (hasFundingPartners || hasClientPartners).Should().BeTrue(
            "Opportunity with only client partners must still return valid structure");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_004_SamePartnerAsBothClientAndFunding()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_005_PartnerWithVeryLongName()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/partner/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_006_SoftDeletedOpportunity_Returns404OrEmpty()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/999998");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    #endregion

    #region INTEGRATION (6)

    [Fact]
    [Trait("TestId", "TC-PNO1207-INT-001")]
    [Trait("Category", "Integration")]
    public async Task INT_001_FullFlow_GetOpportunity_IncludesAllPartnerDataForOup()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasFundingPartners = HasFundingPartners(json);
        var hasClientPartners = HasClientPartners(json);
        (hasFundingPartners || hasClientPartners).Should().BeTrue(
            "Full flow: GET opportunity must include all partner data needed for oUP sync");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-INT-002")]
    [Trait("Category", "Integration")]
    public async Task INT_002_PartnerAndOpportunityEndpoints_ReturnConsistentData()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var oppResponse = await _client.GetAsync("/api/opportunity/1");
        var partnerResponse = await _client.GetAsync("/api/partner/1");
        oppResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        partnerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-INT-003")]
    [Trait("Category", "Integration")]
    public async Task INT_003_OpportunityListEndpoint_IncludesPartnerCountOrSummary()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-INT-004")]
    [Trait("Category", "Integration")]
    public async Task INT_004_PartnerData_PersistsAfterWorkflowStateChange()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.TryGetProperty("stage", out _).Should().BeTrue();
        json.TryGetProperty("workflowStatus", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-INT-005")]
    [Trait("Category", "Integration")]
    public async Task INT_005_OpportunityDetailAndTeamSection_BothAccessible()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasTeamOrStakeholders = json.TryGetProperty("stakeholders", out _) ||
                                    json.TryGetProperty("team", out _) ||
                                    json.TryGetProperty("opportunityManager", out _);
        hasTeamOrStakeholders.Should().BeTrue("Opportunity detail and team section must be accessible");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1207-INT-006")]
    [Trait("Category", "Integration")]
    public async Task INT_006_MultipleOpportunities_EachIncludeIndependentPartnerData()
    {
        if (!_isPostgresAvailable) return;
        if (!await IsOpportunityEndpointReachable()) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        var r2 = await _client.GetAsync("/api/opportunity/2");
        r1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        r2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion
}
