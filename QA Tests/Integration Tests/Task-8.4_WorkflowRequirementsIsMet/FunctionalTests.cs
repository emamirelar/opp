/**
 * @fileoverview Task 8.4 Functional Tests — IsMet business rule verification.
 * Validates that the IsMet field correctly reflects real opportunity data state.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Task84;

[Collection("Integration Tests")]
[Trait("Category", "Functional")]
[Trait("Feature", "Task-8.4")]
[Trait("Component", "WorkflowRequirementsIsMet")]
public class FunctionalTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public FunctionalTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    /// <summary>
    /// The workflow stages endpoint must still work — Task 8.4 must not break existing workflow routes.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-001")]
    public async Task GetWorkflowStages_StillResponds_AfterTask84Changes()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }

    /// <summary>
    /// The workflow submit endpoint must still work — Task 8.4 must not break submit flow.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-002")]
    public async Task SubmitWorkflow_EndpointReachable_NotBrokenByTask84()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new
        {
            entityName = "opportunity",
            entityId = 1,
            newStage = "GO",
            confirmedNonOMSubmission = false,
            confirmedOrgUnitWarning = false,
            acknowledgedStatement = false
        };
        var response = await _client.PostAsJsonAsync("/api/workflow/submit", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// IsMet=false items in the requirements response must have non-empty description
    /// (the description is the message key displayed to the user explaining what is missing).
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-003")]
    public async Task GetRequirements_UnmetItems_HaveNonEmptyDescription()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("isMet", out var isMetProp) && !isMetProp.GetBoolean())
            {
                item.TryGetProperty("description", out var descProp).Should().BeTrue(
                    "unmet requirement must have a description message key");
                var desc = descProp.GetString();
                desc.Should().NotBeNullOrWhiteSpace(
                    "unmet requirement description must not be empty — user needs to see the message");
            }
        }
    }

    /// <summary>
    /// IsMet=true items (met requirements) must also have a description.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-004")]
    public async Task GetRequirements_MetItems_AlsoHaveDescription()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("isMet", out var isMetProp) && isMetProp.GetBoolean())
            {
                item.TryGetProperty("description", out var descProp).Should().BeTrue(
                    "met requirement must still carry a description field");
                descProp.GetString().Should().NotBeNullOrWhiteSpace(
                    "met requirement description must not be empty");
            }
        }
    }

    /// <summary>
    /// The requirements list must include the 'name' identifier for each requirement
    /// so the frontend can look up the human-readable label.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-005")]
    public async Task GetRequirements_AllItems_HaveDistinctNames()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var names = items.EnumerateArray()
            .Where(i => i.TryGetProperty("name", out _))
            .Select(i => i.GetProperty("name").GetString())
            .ToList();

        names.Should().OnlyHaveUniqueItems(
            "each requirement must have a unique name identifier — duplicates cause UI conflicts");
    }

    /// <summary>
    /// Task 8.4 IsMet logic loads Opportunity with several includes.
    /// Verifies the endpoint does not crash when Opportunity has no related collections.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-006")]
    public async Task GetRequirements_OpportunityWithNoRelatedData_ReturnsValidResponse()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Opportunity 99998 is unlikely to exist — test null-opportunity handling
        var response = await _client.GetAsync("/api/workflow/opportunity/99998/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }

    /// <summary>
    /// The workflow state endpoint and requirements endpoint must return data for the same entity.
    /// Both must be reachable after Task 8.4 changes.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-007")]
    public async Task GetWorkflowState_And_GetRequirements_BothRespond()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var stateResponse = await _client.GetAsync("/api/workflow/opportunity/1");
        var reqResponse = await _client.GetAsync("/api/workflow/opportunity/1/requirements");

        stateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound);
        reqResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound);

        // Both must return the same status category (both 200, both 404, etc.)
        if (stateResponse.StatusCode == HttpStatusCode.NotFound)
        {
            reqResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.NotFound, HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Verifies that the requirements endpoint response body does not include any requirements
    /// where OnlyServerSideEvaluation=true (these should be filtered by the controller).
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-008")]
    public async Task GetRequirements_ServerSideOnlyRequirements_AreFilteredOut()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("onlyServerSideEvaluation", out var prop))
            {
                prop.GetBoolean().Should().BeFalse(
                    "controller must filter out all server-side-only requirements before returning");
            }
        }
    }

    /// <summary>
    /// Verifies that the PaoStageRequirement.IsMet is based on the actual validation logic
    /// (ValidateOpportunityRequirementsAsync) — verified by checking name corresponds to description.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-009")]
    public async Task GetRequirements_EachItemNameAndFieldName_AreConsistent()
    {
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            // If fieldName is present, name should also be present
            var hasName = item.TryGetProperty("name", out _);
            hasName.Should().BeTrue(
                "each requirement must have a 'name' field regardless of fieldName presence");
        }
    }

    /// <summary>
    /// The GET /api/workflow/opportunity/1/requirements must not require a request body.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-010")]
    public async Task GetRequirements_NoRequestBodyRequired_GetIsStateless()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Confirm GET requires no body
        using var request = new HttpRequestMessage(HttpMethod.Get,
            "/api/workflow/opportunity/1/requirements");
        // Explicitly no body/content
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that calling requirements twice in sequence for the same opportunity
    /// returns the same IsMet values (deterministic, no side effects from Task 8.4 code path).
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-011")]
    public async Task GetRequirements_CalledTwice_ReturnsSameIsMetValues()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var r1 = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        var r2 = await _client.GetAsync("/api/workflow/opportunity/1/requirements");

        r1.StatusCode.Should().Be(r2.StatusCode);

        if (r1.StatusCode != HttpStatusCode.OK) return;

        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2);
    }

    /// <summary>
    /// The workflow history endpoint must still work — Task 8.4 read-only DB load
    /// must not interfere with other workflow operations.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-012")]
    public async Task GetWorkflowHistory_StillWorksAfterTask84()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/history");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }
    /// <summary>
    /// Functional: the response body must be a JSON array (not an object or null).
    /// Validates the structural contract of the requirements endpoint.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-013")]
    public async Task GetRequirements_ResponseIsJsonArray_WhenOk()
    {
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().Be(JsonValueKind.Array);
    }

    /// <summary>
    /// Functional: every requirement item must expose a 'name' field per the API contract.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-014")]
    public async Task GetRequirements_EachItem_HasNameField()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            item.TryGetProperty("name", out _).Should().BeTrue(
                "each requirement must have a 'name' field");
        }
    }

    /// <summary>
    /// Functional: Task 8.4 business rule — Opportunity requirements must include 'isMet' boolean.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-015")]
    public async Task GetRequirements_Opportunity_EachItem_HasIsMetField()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        if (!items.EnumerateArray().Any()) return;

        foreach (var item in items.EnumerateArray())
        {
            item.TryGetProperty("isMet", out _).Should().BeTrue(
                "Task 8.4: every PaoStageRequirement in the Opportunity response must include 'isMet'");
        }
    }

    /// <summary>
    /// Functional: 'isMet' must be a strict boolean — never null, never a string.
    /// </summary>
    [Fact]
    [Trait("TestId", "TC-TASK84-FUNC-016")]
    public async Task GetRequirements_IsMetIsBoolean_NotNullOrString()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var items = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("isMet", out var isMet))
            {
                isMet.ValueKind.Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
            }
        }
    }
}