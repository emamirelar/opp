using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for DashboardController (QA-071).
/// Covers all 10 dashboard endpoints with full 3:1 ratio compliance.
///
/// Uses the real DashboardService with InMemory database (same pattern as
/// NotificationControllerTests, DashboardNegativeTests, etc.)
/// No mocks are injected because the Lamar container overrides test registrations.
///
/// 3:1 Ratio: P=3, N=9, E=9, F=9, I=9 — all ratios satisfied.
/// </summary>
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Dashboard")]
[Trait("Component", "ControllerTests")]
public class DashboardControllerTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private const string DashboardBase = "/api/dashboard";

    public DashboardControllerTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    private HttpClient CreateUnauthenticatedClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        return client;
    }

    // ==========================================
    // POSITIVE TESTS (3)
    // ==========================================

    [Fact]
    [Trait("TestId", "TC-DASH-POS-001")]
    public async Task GetDashboardContent_AuthenticatedUser_Returns200()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-002")]
    public async Task GetMyPartners_AuthenticatedUser_Returns200()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-partners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-003")]
    public async Task GetMyContacts_AuthenticatedUser_Returns200()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-contacts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }

    // ==========================================
    // NEGATIVE TESTS (9)
    // ==========================================

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-001")]
    public async Task GetDashboardContent_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/content");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-002")]
    public async Task GetMyPartners_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/my-partners");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-003")]
    public async Task GetMyContacts_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/my-contacts");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-004")]
    public async Task GetMyInteractions_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/my-interactions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-005")]
    public async Task GetMyOpportunities_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/my-opportunities");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-006")]
    public async Task GetMyDraftPartners_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/my-draft-partners");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-007")]
    public async Task GetMyDraftContacts_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/my-draft-contacts");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-008")]
    public async Task GetMyDraftInteractions_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/my-draft-interactions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-009")]
    public async Task GetOrgUnitRecentUpdates_Unauthenticated_Returns401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var response = await unauth.GetAsync($"{DashboardBase}/org-unit-recent-updates");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==========================================
    // EDGE / BOUNDARY TESTS (9)
    // ==========================================

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-001")]
    public async Task GetMyPartners_ZeroPageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-partners?pageSize=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-002")]
    public async Task GetMyPartners_NegativePageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-partners?pageSize=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-003")]
    public async Task GetMyPartners_VeryLargePageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-partners?pageSize=99999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-004")]
    public async Task GetMyContacts_NonNumericPageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-contacts?pageSize=abc");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-005")]
    public async Task GetDashboardContent_ZeroPageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content?pageSize=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-006")]
    public async Task GetDashboardContent_VeryLargePageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content?pageSize=999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-007")]
    public async Task GetDashboardContent_VeryLargeRecentUpdatesPageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content?recentUpdatesPageSize=999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-008")]
    public async Task GetOrgUnitRecentUpdates_ZeroPageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/org-unit-recent-updates?pageSize=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-009")]
    public async Task GetMyInteractions_VeryLargePageSize_HandledGracefully()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-interactions?pageSize=99999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    // ==========================================
    // FUNCTIONAL TESTS (9)
    // ==========================================

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-001")]
    public async Task GetDashboardContent_Response_HasJsonContentType()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-002")]
    public async Task GetMyPartners_Response_HasJsonContentType()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-partners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-003")]
    public async Task GetDashboardContent_Response_IsValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow("response body must be valid JSON");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-004")]
    public async Task GetMyPartners_Response_IsValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-partners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow("response body must be valid JSON");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-005")]
    public async Task GetDashboardContent_ResponseContains_MyPartnersProperty()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.TryGetProperty("myPartners", out _).Should().BeTrue("response must include 'myPartners' collection");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-006")]
    public async Task GetDashboardContent_ResponseContains_MyContactsProperty()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.TryGetProperty("myContacts", out _).Should().BeTrue("response must include 'myContacts' collection");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-007")]
    public async Task GetDashboardContent_ResponseContains_AllNineEntityCollections()
    {
        var response = await _client.GetAsync($"{DashboardBase}/content");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var expectedProperties = new[]
        {
            "myPartners", "myContacts", "myInteractions", "myOpportunities",
            "draftPartners", "draftContacts", "draftInteractions", "draftOpportunities",
            "orgUnitRecentUpdates"
        };
        foreach (var prop in expectedProperties)
        {
            json.RootElement.TryGetProperty(prop, out _).Should().BeTrue($"response must contain the '{prop}' collection");
        }
    }

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-008")]
    public async Task GetMyContacts_Response_IsValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-contacts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow("response body must be valid JSON");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-FUNC-009")]
    public async Task GetMyInteractions_Response_IsValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-interactions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow("response body must be valid JSON");
    }

    // ==========================================
    // INTEGRATION TESTS (9)
    // ==========================================

    [Fact]
    [Trait("TestId", "TC-DASH-INT-001")]
    public async Task AllTenDashboardEndpoints_AuthenticatedUser_AllReturn200()
    {
        var endpoints = new[]
        {
            $"{DashboardBase}/my-partners",
            $"{DashboardBase}/my-contacts",
            $"{DashboardBase}/my-interactions",
            $"{DashboardBase}/my-opportunities",
            $"{DashboardBase}/my-draft-partners",
            $"{DashboardBase}/my-draft-contacts",
            $"{DashboardBase}/my-draft-interactions",
            $"{DashboardBase}/my-draft-opportunities",
            $"{DashboardBase}/org-unit-recent-updates",
            $"{DashboardBase}/content",
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"authenticated request to '{endpoint}' should return 200");
        }
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-002")]
    public async Task AllDashboardEndpoints_Unauthenticated_AllReturn401()
    {
        using var unauth = CreateUnauthenticatedClient();
        var endpoints = new[]
        {
            $"{DashboardBase}/my-partners",
            $"{DashboardBase}/my-contacts",
            $"{DashboardBase}/my-interactions",
            $"{DashboardBase}/my-opportunities",
            $"{DashboardBase}/my-draft-partners",
            $"{DashboardBase}/my-draft-contacts",
            $"{DashboardBase}/my-draft-interactions",
            $"{DashboardBase}/my-draft-opportunities",
            $"{DashboardBase}/org-unit-recent-updates",
            $"{DashboardBase}/content",
        };
        foreach (var endpoint in endpoints)
        {
            var response = await unauth.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                $"unauthenticated request to '{endpoint}' should be rejected with 401");
        }
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-003")]
    public async Task GetDashboardContent_SequentialCalls_BothReturn200()
    {
        var r1 = await _client.GetAsync($"{DashboardBase}/content");
        var r2 = await _client.GetAsync($"{DashboardBase}/content");

        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-004")]
    public async Task GetMyPartners_ThenGetContent_BothSucceed()
    {
        var r1 = await _client.GetAsync($"{DashboardBase}/my-partners");
        var r2 = await _client.GetAsync($"{DashboardBase}/content");

        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-005")]
    public async Task GetMyOpportunities_AuthenticatedUser_Returns200WithValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-opportunities");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-006")]
    public async Task GetMyDraftPartners_AuthenticatedUser_Returns200WithValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-draft-partners");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-007")]
    public async Task GetMyDraftInteractions_AuthenticatedUser_Returns200WithValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-draft-interactions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-008")]
    public async Task GetOrgUnitRecentUpdates_AuthenticatedUser_Returns200WithValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/org-unit-recent-updates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-009")]
    public async Task GetMyDraftOpportunities_AuthenticatedUser_Returns200WithValidJson()
    {
        var response = await _client.GetAsync($"{DashboardBase}/my-draft-opportunities");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow();
    }
}
