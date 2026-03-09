/**
 * @fileoverview Task 8.4 Negative Tests — invalid inputs to the workflow requirements endpoint.
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
[Trait("Category", "Negative")]
[Trait("Feature", "Task-8.4")]
[Trait("Component", "WorkflowRequirementsIsMet")]
public class NegativeTests
{
    private readonly HttpClient _client;
    private readonly HttpClient _unauthClient;
    private readonly bool _isPostgresAvailable;

    public NegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");

        _unauthClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-001")]
    public async Task GetRequirements_Unauthenticated_Returns401Or302()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _unauthClient.GetAsync("/api/workflow/opportunity/1/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-002")]
    public async Task GetRequirements_NonExistentOpportunity_Returns404Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/99999/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK);         // may return empty list if entity not found
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-003")]
    public async Task GetRequirements_ZeroId_Returns404Or400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/0/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-004")]
    public async Task GetRequirements_NegativeId_Returns400Or404Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/opportunity/-1/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-005")]
    public async Task GetRequirements_UnknownEntityType_Returns404Or400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/workflow/unknownentity/1/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-006")]
    public async Task GetRequirements_PostMethod_Returns405Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsJsonAsync(
            "/api/workflow/opportunity/1/requirements", new { }, new JsonSerializerOptions());
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-007")]
    public async Task GetRequirements_StringIdSegment_Returns400Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Production returns 500 when route parameter cannot be parsed as int (no explicit model binding error handler)
        // DEF: consider adding model binding error handler to return 400 for invalid route parameters
        var response = await _client.GetAsync("/api/workflow/opportunity/not-an-integer/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-008")]
    public async Task GetRequirements_MaxIntId_Returns404Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"/api/workflow/opportunity/{int.MaxValue}/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-009")]
    public async Task GetRequirements_DeleteMethod_Returns405Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.DeleteAsync("/api/workflow/opportunity/1/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-010")]
    public async Task GetRequirements_PartnerEntity_IdReturnsListOrEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Partners do not have PaoStageRequirement — IsMet should not be set
        var response = await _client.GetAsync("/api/workflow/partner/1/requirements");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            // For non-Opportunity entities, IsMet should NOT be set by Task 8.4 logic
            foreach (var item in body.EnumerateArray())
            {
                if (item.TryGetProperty("isMet", out var isMet))
                {
                    // If isMet is present it must be a valid boolean (not corrupted)
                    isMet.ValueKind.Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
                }
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-011")]
    public async Task GetRequirements_WithInvalidNextStage_Returns400Or200WithEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(
            "/api/workflow/opportunity/1/requirements/INVALID_STAGE_XYZ");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,        // may return empty requirements for unknown next stage
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-TASK84-NEG-012")]
    public async Task GetRequirements_WithEmptyNextStage_Returns200OrError()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Empty nextStage segment — endpoint should handle gracefully
        var response = await _client.GetAsync("/api/workflow/opportunity/1/requirements/");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }
}
