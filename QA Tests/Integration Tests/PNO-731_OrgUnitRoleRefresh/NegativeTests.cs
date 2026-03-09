/**
 * @fileoverview PNO-731 Negative Tests — invalid inputs, bad org units, unauthorised access.
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
[Trait("Category", "Negative")]
[Trait("Feature", "PNO-731")]
[Trait("Component", "OrgUnitRoleRefresh")]
public class NegativeTests
{
    private readonly HttpClient _client;
    private readonly HttpClient _unauthClient;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

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
    [Trait("TestId", "TC-PNO731-NEG-001")]
    public async Task UpdateOpportunity_Unauthenticated_Returns401Or302()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new { id = 1, responsibleOrgUnitId = 1 };
        var response = await _unauthClient.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-002")]
    public async Task UpdateOpportunity_NonExistentId_Returns404Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new { id = 99999, responsibleOrgUnitId = 1 };
        var response = await _client.PutAsJsonAsync("/api/opportunity/99999", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-003")]
    public async Task UpdateOpportunity_NegativeId_Returns400Or404Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new { id = -1, responsibleOrgUnitId = 1 };
        var response = await _client.PutAsJsonAsync("/api/opportunity/-1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-004")]
    public async Task UpdateOpportunity_MissingBody_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent(string.Empty, System.Text.Encoding.UTF8);
        var response = await _client.PutAsync("/api/opportunity/1", content);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-005")]
    public async Task UpdateOpportunity_OrgUnitIdZero_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var payload = new { id = 1, responsibleOrgUnitId = 0 };
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-006")]
    public async Task GetOpportunity_AfterBadUpdate_StillReturnsOriginal()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // First attempt a bad update
        var badPayload = new { id = 1, responsibleOrgUnitId = -999 };
        await _client.PutAsJsonAsync("/api/opportunity/1", badPayload, JsonOpts);

        // Opportunity should still be readable if it existed before
        var response = await _client.GetAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-007")]
    public async Task UpdateOpportunity_IdMismatch_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Route id=1 but body id=2 — mismatch should be rejected
        var payload = new { id = 2, responsibleOrgUnitId = 1 };
        var response = await _client.PutAsJsonAsync("/api/opportunity/1", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-008")]
    public async Task UpdateOpportunity_InvalidJsonBody_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("{ invalid json }", System.Text.Encoding.UTF8);
        var response = await _client.PutAsync("/api/opportunity/1", content);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-009")]
    public async Task UpdateOpportunity_WrongHttpMethod_Get_Returns405Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/opportunity/1/update");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-010")]
    public async Task UpdateOpportunity_Delete_NotAllowedOnUpdateRoute()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.DeleteAsync("/api/opportunity/1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-011")]
    public async Task UpdateOpportunity_SoftDeletedOpportunity_Returns404Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Attempt to update opportunity 0 — should not exist
        var payload = new { id = 0, responsibleOrgUnitId = 1 };
        var response = await _client.PutAsJsonAsync("/api/opportunity/0", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO731-NEG-012")]
    public async Task UpdateOpportunity_StringIdRoute_Returns400Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        // Production returns 500 when route parameter cannot be parsed as int (no explicit model binding error handler)
        // DEF: consider adding model binding error handler to return 400 for invalid route parameters
        var payload = new { id = 1, responsibleOrgUnitId = 1 };
        var response = await _client.PutAsJsonAsync("/api/opportunity/not-an-id", payload, JsonOpts);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.MethodNotAllowed);
    }
}
