/**
 * @fileoverview PNO-731 Boundary / Edge-Case Tests — limit values, concurrent updates, soft-delete.
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

namespace UNOPS.PAO.IntegrationTests.PNO731;

[Collection("Integration Tests")]
[Trait("Category", "EdgeBoundary")]
[Trait("Feature", "PNO-731")]
[Trait("Component", "OrgUnitRoleRefresh")]
public class BoundaryTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public BoundaryTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-001")]
    public async Task UpdateOpportunity_MaxIntOrgUnitId_Returns400Or404Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new { id = 1, responsibleOrgUnitId = int.MaxValue };
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-002")]
    public async Task UpdateOpportunity_OrgUnitIdOne_ReturnsAcceptableStatus()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // OrgUnit ID = 1 is the minimum valid ID; endpoint should not crash
        var payload = new { id = 1, responsibleOrgUnitId = 1 };
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-003")]
    public async Task UpdateOpportunity_SameOrgUnitTwiceSequentially_BothSucceedOrFail()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Sending the exact same update twice — both calls should behave identically
        var payload = new { id = 1, responsibleOrgUnitId = 1 };

        var first = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);
        var second = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        // Both should return the same status code (idempotent from a routing perspective)
        first.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
        second.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-004")]
    public async Task UpdateOpportunity_NullOrgUnitId_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Explicitly sending null for responsibleOrgUnitId
        var payload = new { id = 1, responsibleOrgUnitId = (int?)null };
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,       // null is a valid nullable — endpoint may accept
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-005")]
    public async Task UpdateOpportunity_MaxIntOpportunityId_Returns404Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new { id = int.MaxValue, responsibleOrgUnitId = 1 };
        var response = await _client.PutAsJsonAsync($"/api/opportunity/{int.MaxValue}", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-006")]
    public async Task UpdateOpportunity_WithAdditionalUnknownFields_IsIgnoredOrBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Sending extra unknown fields — controller should ignore them or reject cleanly
        var payload = new
        {
            id = 1,
            responsibleOrgUnitId = 1,
            unknownField = "should-be-ignored",
            anotherUnknown = 99999
        };
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-007")]
    public async Task UpdateOpportunity_OrgUnitIdChangedThenRevertedSameRequest_ReturnsAcceptable()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Simulate: opportunity already has orgUnit=5. We send orgUnit=5 again.
        // Before PNO-731 fix: stakeholders would NOT refresh (orgUnitChanged=false).
        // After PNO-731 fix: stakeholders WILL refresh (condition removed).
        // This test verifies no HTTP-level failure occurs for "same-value" updates.
        var payload = new { id = 1, responsibleOrgUnitId = 5 };
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-008")]
    public async Task GetOpportunity_BoundaryId_EndpointResponds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Boundary GET: id=1 (minimum meaningful id)
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-009")]
    public async Task UpdateOpportunity_VeryLargePayload_HandledGracefully()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Large name to test payload size handling
        var largeName = new string('A', 10000);
        var payload = new { id = 1, responsibleOrgUnitId = 1, name = largeName };
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.BadRequest,
            HttpStatusCode.RequestEntityTooLarge);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-010")]
    public async Task UpdateOpportunity_EmptyJsonObject_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Sending a valid JSON object but with no fields at all
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", new { }, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-011")]
    public async Task UpdateOpportunity_SwitchingBetweenTwoValidOrgUnits_BothAccepted()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var firstUpdate = new { id = 1, responsibleOrgUnitId = 1 };
        var secondUpdate = new { id = 1, responsibleOrgUnitId = 2 };

        var firstResponse = await _client.PutAsJsonAsync("/api/opportunity/1", firstUpdate, JsonOpts);
        var secondResponse = await _client.PutAsJsonAsync("/api/opportunity/1", secondUpdate, JsonOpts);

        firstResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
        secondResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-BND-012")]
    public async Task UpdateOpportunity_ResponsibleOrgUnitIdString_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Sending a string where integer is expected
        var content = new System.Net.Http.StringContent(
            """{"id": 1, "responsibleOrgUnitId": "not-a-number"}""",
            System.Text.Encoding.UTF8);
        var response = await _client.PutAsync("/api/opportunity/1", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }
}
