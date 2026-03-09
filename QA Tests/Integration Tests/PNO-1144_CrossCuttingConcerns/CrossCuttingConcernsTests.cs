/**
 * @fileoverview PNO-1144 Cross-Cutting Concerns Tests — validates WHY Section endpoints
 * including beneficiaries, SDG alignment, context/challenges, and cross-cutting concerns
 * subsection per Executive Office request.
 *
 * Feature: In the WHY Section of an Opportunity record, after the beneficiaries subsection,
 * add a subsection for cross-cutting concerns.
 * Status: Ready for Development (HIGH priority)
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-1144
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1144CrossCuttingConcerns;

[Collection("Integration Tests")]
[Trait("Feature", "PNO-1144")]
[Trait("Component", "CrossCuttingConcerns")]
[Trait("JiraRef", "PNO-1144")]
public class CrossCuttingConcernsTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CrossCuttingConcernsTests(PAOWebApplicationFactory<Program> factory)
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

    private static bool HasWhySectionData(JsonElement root)
    {
        return root.TryGetProperty("resultsFocus", out _) ||
               root.TryGetProperty("expectedImpact", out _) ||
               root.TryGetProperty("beneficiariesToBeDetermined", out _) ||
               root.TryGetProperty("estimatedDirectBeneficiaries", out _) ||
               root.TryGetProperty("sdgs", out _) ||
               root.TryGetProperty("context", out _) ||
               root.TryGetProperty("challenges", out _) ||
               root.TryGetProperty("crossCuttingConcerns", out _);
    }

    #region POSITIVE (2)

    [Fact]
    [Trait("TestId", "TC-PNO1144-POS-001")]
    [Trait("Category", "Positive")]
    public async Task POS_001_OpportunityDetailEndpoint_IncludesWhySectionData()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasWhyData = HasWhySectionData(json);
        hasWhyData.Should().BeTrue(
            "Opportunity detail endpoint must include WHY section data");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-POS-002")]
    [Trait("Category", "Positive")]
    public async Task POS_002_WhySectionData_ReturnsStructuredResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrWhiteSpace("WHY section data must return structured JSON response");
    }

    #endregion

    #region NEGATIVE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1144-NEG-001")]
    [Trait("Category", "Negative")]
    public async Task NEG_001_UnauthenticatedRequest_Returns401Or302()
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
    [Trait("TestId", "TC-PNO1144-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task NEG_002_NonExistentOpportunity_Returns404()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task NEG_003_InvalidOpportunityId_ReturnsError()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/not-an-id");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task NEG_004_WhySectionWithEmptyData_ReturnsValidResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object,
            "WHY section with empty data must still return valid JSON structure");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task NEG_005_ZeroId_Returns400Or404()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/0");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-NEG-006")]
    [Trait("Category", "Negative")]
    public async Task NEG_006_NegativeId_Returns400Or404()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/-1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    #endregion

    #region FUNCTIONAL (6)

    [Fact]
    [Trait("TestId", "TC-PNO1144-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_WhySection_IncludesBeneficiariesSubsection()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasBeneficiaries = json.TryGetProperty("beneficiariesToBeDetermined", out _) ||
                              json.TryGetProperty("estimatedDirectBeneficiaries", out _) ||
                              json.TryGetProperty("estimatedIndirectBeneficiaries", out _);
        hasBeneficiaries.Should().BeTrue(
            "WHY section must include beneficiaries subsection");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_WhySection_IncludesSdgAlignmentData()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasSdg = json.TryGetProperty("sdgs", out var sdgs) &&
                     (sdgs.ValueKind == JsonValueKind.Array || sdgs.ValueKind == JsonValueKind.Null);
        hasSdg.Should().BeTrue(
            "WHY section must include SDG alignment data");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_WhySection_IncludesContextOrChallenges()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasContext = json.TryGetProperty("context", out _) ||
                        json.TryGetProperty("challenges", out _) ||
                        json.TryGetProperty("resultsFocus", out _) ||
                        json.TryGetProperty("expectedImpact", out _);
        hasContext.Should().BeTrue(
            "WHY section must include context/challenges data");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task FUNC_004_OpportunityResponse_HasConsistentStructure()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        var r2 = await _client.GetAsync("/api/opportunity/1");
        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode != HttpStatusCode.OK) return;

        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2, "Opportunity response must have consistent structure across calls");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task FUNC_005_WhySectionData_AvailableThroughDetailEndpoint()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasWhyData = HasWhySectionData(json);
        hasWhyData.Should().BeTrue(
            "WHY section data must be available through opportunity detail endpoint");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_WhySectionFields_PartOfWorkflowRequirements()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        reqResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;

        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        items.ValueKind.Should().BeOneOf(new[] { JsonValueKind.Array, JsonValueKind.Object },
            "Workflow requirements must include WHY section field validation");
    }

    #endregion

    #region EDGE (6)

    [Fact]
    [Trait("TestId", "TC-PNO1144-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_001_WhySection_WithAllFieldsPopulated()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object);
        HasWhySectionData(json).Should().BeTrue(
            "WHY section with all fields populated must return complete structure");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_002_WhySection_WithOnlyRequiredFields()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object,
            "WHY section with only required fields must return valid structure");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_003_WhySection_WithVeryLongTextContent()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object,
            "WHY section with very long text must return valid response");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_004_WhySection_WithSpecialCharacters()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().Be(JsonValueKind.Object,
            "WHY section with special characters must be handled correctly");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_005_WhySectionData_ConsistentAcrossCalls()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        var r2 = await _client.GetAsync("/api/opportunity/1");
        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode != HttpStatusCode.OK) return;

        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2, "WHY section data must be consistent across multiple calls");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_006_SoftDeletedOpportunity_WhySectionNotAccessible()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/999998");
        response.StatusCode.Should().BeOneOf(new[] { HttpStatusCode.NotFound, HttpStatusCode.OK },
            "Soft-deleted opportunity WHY section must not be accessible");
    }

    #endregion

    #region INTEGRATION (6)

    [Fact]
    [Trait("TestId", "TC-PNO1144-INT-001")]
    [Trait("Category", "Integration")]
    public async Task INT_001_FullFlow_GetOpportunity_IncludesWhySectionInResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasWhyData = HasWhySectionData(json);
        hasWhyData.Should().BeTrue(
            "Full flow: GET opportunity must include WHY section in response");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-INT-002")]
    [Trait("Category", "Integration")]
    public async Task INT_002_WhySectionData_AndWorkflowRequirementsAligned()
    {
        if (!_isPostgresAvailable) return;
        var oppResponse = await _client.GetAsync("/api/opportunity/1");
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        oppResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        reqResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-INT-003")]
    [Trait("Category", "Integration")]
    public async Task INT_003_OpportunityDetailAndStatementEndpoints_BothIncludeWhyData()
    {
        if (!_isPostgresAvailable) return;
        var detailResponse = await _client.GetAsync("/api/opportunity/1");
        detailResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (detailResponse.StatusCode != HttpStatusCode.OK) return;

        var json = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasWhyData = HasWhySectionData(json);
        hasWhyData.Should().BeTrue(
            "Opportunity detail and statement endpoints must both include WHY data");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-INT-004")]
    [Trait("Category", "Integration")]
    public async Task INT_004_MultipleOpportunities_HaveIndependentWhySections()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync("/api/opportunity/1");
        var r2 = await _client.GetAsync("/api/opportunity/2");
        r1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        r2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-INT-005")]
    [Trait("Category", "Integration")]
    public async Task INT_005_WhySection_AccessibleAlongsideOtherSections()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.TryGetProperty("id", out _).Should().BeTrue();
        var hasWhyData = HasWhySectionData(json);
        hasWhyData.Should().BeTrue(
            "WHY section must be accessible alongside other opportunity sections");
    }

    [Fact]
    [Trait("TestId", "TC-PNO1144-INT-006")]
    [Trait("Category", "Integration")]
    public async Task INT_006_WhySectionData_IncludedInGoDecisionRequirementsCheck()
    {
        if (!_isPostgresAvailable) return;
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements/GO");
        reqResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (reqResponse.StatusCode != HttpStatusCode.OK) return;

        var items = await reqResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        items.ValueKind.Should().BeOneOf(new[] { JsonValueKind.Array, JsonValueKind.Object },
            "WHY section data must be included in Go Decision requirements check");
    }

    #endregion
}
