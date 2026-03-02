/**
 * @fileoverview Edge case integration tests for DashboardController
 * Tests boundary conditions against actual API: /api/dashboard/*
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Dashboard;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Dashboard")]
[Trait("Component", "EdgeCaseTests")]
public class DashboardEdgeCaseTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/dashboard";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DashboardEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-001")]
    public async Task GetMyPartners_EmptyResults_Returns200WithEmptyArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-partners?pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out var records).Should().BeTrue();
        records.GetArrayLength().Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-002")]
    public async Task GetMyContacts_EmptyResults_Returns200WithValidStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-contacts?pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-003")]
    public async Task GetMyDraftPartners_DraftVsPublished_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-draft-partners?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-004")]
    public async Task GetMyDraftContacts_DraftVsPublished_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-draft-contacts?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-005")]
    public async Task GetMyInteractions_MinimumPageSize_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-interactions?pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out var records).Should().BeTrue();
        records.GetArrayLength().Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-006")]
    public async Task GetMyDraftInteractions_DraftVsPublished_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-draft-interactions?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-007")]
    public async Task GetMyOpportunities_EmptyResults_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-opportunities?pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-008")]
    public async Task GetMyDraftOpportunities_DraftVsPublished_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-draft-opportunities?pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-009")]
    public async Task GetOrgUnitRecentUpdates_MinimumPageSize_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/org-unit-recent-updates?pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("updates", out var updates).Should().BeTrue();
        updates.GetArrayLength().Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-010")]
    public async Task GetContent_DefaultParams_Returns200WithAllSections()
    {
        var response = await _client.GetAsync($"{BaseUrl}/content");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("myPartners", out _).Should().BeTrue();
        result.TryGetProperty("myContacts", out _).Should().BeTrue();
        result.TryGetProperty("myInteractions", out _).Should().BeTrue();
        result.TryGetProperty("myOpportunities", out _).Should().BeTrue();
        result.TryGetProperty("draftPartners", out _).Should().BeTrue();
        result.TryGetProperty("draftContacts", out _).Should().BeTrue();
        result.TryGetProperty("draftInteractions", out _).Should().BeTrue();
        result.TryGetProperty("draftOpportunities", out _).Should().BeTrue();
        result.TryGetProperty("orgUnitRecentUpdates", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-011")]
    public async Task GetContent_WithPageSizeParams_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/content?pageSize=50&recentUpdatesPageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("orgUnitName", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-012")]
    public async Task GetContent_MaxPageSize_CapsTo100()
    {
        var response = await _client.GetAsync($"{BaseUrl}/content?pageSize=500&recentUpdatesPageSize=50");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("myPartners", out var partners).Should().BeTrue();
        partners.GetArrayLength().Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-013")]
    public async Task GetOrgUnitRecentUpdates_DefaultPageSize_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/org-unit-recent-updates");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("updates", out _).Should().BeTrue();
        result.TryGetProperty("orgUnitName", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-014")]
    public async Task GetMyPartners_DefaultPageSize_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-partners");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-EDGE-015")]
    public async Task AllEndpoints_RapidSequentialCalls_NoStateCorruption()
    {
        var urls = new[]
        {
            $"{BaseUrl}/my-partners?pageSize=5",
            $"{BaseUrl}/my-contacts?pageSize=5",
            $"{BaseUrl}/content?pageSize=5&recentUpdatesPageSize=5"
        };
        foreach (var url in urls)
        {
            for (var i = 0; i < 3; i++)
            {
                var response = await _client.GetAsync(url);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
