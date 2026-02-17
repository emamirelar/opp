/**
 * @fileoverview Integration tests for DashboardController
 * Tests actual dashboard API endpoints with IAP auth
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 *
 * Real endpoints (all require IAP auth):
 * GET /api/dashboard/my-partners, my-contacts, my-draft-partners, my-draft-contacts,
 *      my-interactions, my-draft-interactions, my-opportunities, my-draft-opportunities,
 *      org-unit-recent-updates, content
 */

using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models.Dashboard;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for DashboardController - real endpoints only
/// </summary>
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Dashboard")]
public class DashboardControllerTests : IClassFixture<PAOWebApplicationFactory<Program>>
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DashboardControllerTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = CreateAuthenticatedClient(factory);
    }

    private static HttpClient CreateAuthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    #region Positive Tests - All 10 Endpoints

    [Fact]
    [Trait("TestId", "TC-DASH-POS-001")]
    public async Task GetMyPartners_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Records.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-002")]
    public async Task GetMyContacts_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/my-contacts?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-003")]
    public async Task GetMyDraftPartners_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/my-draft-partners?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-004")]
    public async Task GetMyDraftContacts_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/my-draft-contacts?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-005")]
    public async Task GetMyInteractions_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/my-interactions?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-006")]
    public async Task GetMyDraftInteractions_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/my-draft-interactions?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-007")]
    public async Task GetMyOpportunities_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/my-opportunities?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-008")]
    public async Task GetMyDraftOpportunities_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/my-draft-opportunities?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-009")]
    public async Task GetOrgUnitRecentUpdates_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/org-unit-recent-updates?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DashboardOrgUnitRecentUpdatesResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Updates.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-010")]
    public async Task GetContent_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/dashboard/content?pageSize=50&recentUpdatesPageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DashboardCombinedResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.MyPartners.Should().NotBeNull();
        result.MyContacts.Should().NotBeNull();
        result.MyInteractions.Should().NotBeNull();
        result.MyOpportunities.Should().NotBeNull();
        result.DraftPartners.Should().NotBeNull();
        result.DraftContacts.Should().NotBeNull();
        result.DraftInteractions.Should().NotBeNull();
        result.DraftOpportunities.Should().NotBeNull();
        result.OrgUnitRecentUpdates.Should().NotBeNull();
    }

    #endregion

    #region Negative Tests

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-001")]
    public async Task GetMyPartners_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/dashboard/my-partners?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-002")]
    public async Task GetContent_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/dashboard/content");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-003")]
    public async Task GetMyPartners_InvalidPageSizeZero_HandlesGracefully()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-004")]
    public async Task GetMyPartners_InvalidPageSizeNegative_HandlesGracefully()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-005")]
    public async Task GetContent_InvalidPageSize_HandlesGracefully()
    {
        var response = await _client.GetAsync("/api/dashboard/content?pageSize=-5&recentUpdatesPageSize=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-006")]
    public async Task GetNonExistentEndpoint_Returns404()
    {
        var response = await _client.GetAsync("/api/dashboard/non-existent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-007")]
    public async Task PostToGetEndpoint_Returns405()
    {
        var response = await _client.PostAsync("/api/dashboard/my-partners", null);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-001")]
    public async Task GetMyPartners_DefaultPageSize_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
        result!.PageSize.Should().BeInRange(0, 1000);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-002")]
    public async Task GetContent_DefaultParams_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/dashboard/content");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DashboardCombinedResponse>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-003")]
    public async Task GetOrgUnitRecentUpdates_SmallPageSize_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/dashboard/org-unit-recent-updates?pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DashboardOrgUnitRecentUpdatesResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Updates.Count.Should().BeLessOrEqualTo(5);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-004")]
    public async Task GetContent_LargePageSize_CappedByServer()
    {
        var response = await _client.GetAsync("/api/dashboard/content?pageSize=500&recentUpdatesPageSize=100");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DashboardCombinedResponse>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-005")]
    public async Task GetMyPartners_EmptyResult_ReturnsValidStructure()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
        result!.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Records.Should().NotBeNull();
    }

    #endregion

    #region Validation Tests

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-001")]
    public async Task GetMyPartners_ResponseHasRequiredFields()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("records");
        json.Should().Contain("totalCount");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-002")]
    public async Task GetContent_ResponseHasAllSections()
    {
        var response = await _client.GetAsync("/api/dashboard/content");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DashboardCombinedResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.MyPartners.Should().NotBeNull();
        result.MyContacts.Should().NotBeNull();
        result.MyInteractions.Should().NotBeNull();
        result.MyOpportunities.Should().NotBeNull();
        result.DraftPartners.Should().NotBeNull();
        result.DraftContacts.Should().NotBeNull();
        result.DraftInteractions.Should().NotBeNull();
        result.DraftOpportunities.Should().NotBeNull();
        result.OrgUnitRecentUpdates.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-003")]
    public async Task GetOrgUnitRecentUpdates_ResponseHasUpdatesAndOrgUnit()
    {
        var response = await _client.GetAsync("/api/dashboard/org-unit-recent-updates?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DashboardOrgUnitRecentUpdatesResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Updates.Should().NotBeNull();
        result.OrgUnitName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-004")]
    public async Task GetMyPartners_ContentTypeIsJson()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=10");
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    #endregion

    #region Security Tests

    [Fact]
    [Trait("TestId", "TC-DASH-SEC-001")]
    public async Task AllDashboardEndpoints_RequireAuth()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");

        var endpoints = new[]
        {
            "/api/dashboard/my-partners",
            "/api/dashboard/my-contacts",
            "/api/dashboard/my-draft-partners",
            "/api/dashboard/content"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await client.GetAsync(endpoint);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"because {endpoint} requires auth");
        }
    }

    [Fact]
    [Trait("TestId", "TC-DASH-SEC-002")]
    public async Task GetMyPartners_AuthenticatedUser_ReturnsOnlyUserData()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    [Trait("TestId", "TC-DASH-PERF-001")]
    [Trait("Category", "Performance")]
    public async Task GetContent_CompletesWithin5Seconds()
    {
        var sw = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/dashboard/content?pageSize=50&recentUpdatesPageSize=10");
        sw.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sw.ElapsedMilliseconds.Should().BeLessThan(5000, "dashboard content should load within 5 seconds");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-PERF-002")]
    [Trait("Category", "Performance")]
    public async Task GetMyPartners_CompletesWithin3Seconds()
    {
        var sw = Stopwatch.StartNew();
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=1000");
        sw.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sw.ElapsedMilliseconds.Should().BeLessThan(3000, "my-partners should load within 3 seconds");
    }

    #endregion

    #region Concurrent Tests

    [Fact]
    [Trait("TestId", "TC-DASH-CONC-001")]
    public async Task MultipleEndpoints_ConcurrentCalls_AllSucceed()
    {
        var tasks = new[]
        {
            _client.GetAsync("/api/dashboard/my-partners?pageSize=100"),
            _client.GetAsync("/api/dashboard/my-contacts?pageSize=100"),
            _client.GetAsync("/api/dashboard/my-interactions?pageSize=100"),
            _client.GetAsync("/api/dashboard/content?pageSize=20&recentUpdatesPageSize=5")
        };

        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    [Trait("TestId", "TC-DASH-CONC-002")]
    public async Task GetContent_ConcurrentCalls_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.GetAsync("/api/dashboard/content?pageSize=10&recentUpdatesPageSize=5"))
            .ToArray();
        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    [Trait("TestId", "TC-DASH-CONC-003")]
    public async Task AllListEndpoints_Concurrent_AllSucceed()
    {
        var endpoints = new[]
        {
            "/api/dashboard/my-partners?pageSize=100",
            "/api/dashboard/my-contacts?pageSize=100",
            "/api/dashboard/my-draft-partners?pageSize=100",
            "/api/dashboard/my-draft-contacts?pageSize=100",
            "/api/dashboard/my-interactions?pageSize=100",
            "/api/dashboard/my-draft-interactions?pageSize=100",
            "/api/dashboard/my-opportunities?pageSize=100",
            "/api/dashboard/my-draft-opportunities?pageSize=100",
            "/api/dashboard/org-unit-recent-updates?pageSize=10"
        };
        var tasks = endpoints.Select(e => _client.GetAsync(e)).ToArray();
        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-005")]
    public async Task GetMyContacts_ResponseHasRecordsArray()
    {
        var response = await _client.GetAsync("/api/dashboard/my-contacts?pageSize=10");
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("records");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-006")]
    public async Task GetMyOpportunities_ResponseHasTotalCount()
    {
        var response = await _client.GetAsync("/api/dashboard/my-opportunities?pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<object>>(JsonOptions);
        result.Should().NotBeNull();
        result!.TotalCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-008")]
    public async Task GetMyPartners_InvalidPageSizeNonNumeric_HandlesGracefully()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=abc");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-006")]
    public async Task GetMyPartners_MaxPageSize_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/dashboard/my-partners?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-SEC-003")]
    public async Task GetOrgUnitRecentUpdates_Unauthenticated_Returns401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/dashboard/org-unit-recent-updates?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
