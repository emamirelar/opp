/**
 * @fileoverview Validation integration tests for DashboardController
 * Tests response structure, content-type, JSON format for all major endpoints
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
[Trait("Component", "ValidationTests")]
public class DashboardValidationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/dashboard";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DashboardValidationTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-001")]
    public async Task GetMyPartners_ReturnsValidJsonStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-partners");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-002")]
    public async Task GetMyContacts_ReturnsValidJsonStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-contacts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-003")]
    public async Task GetMyInteractions_ReturnsValidJsonStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-interactions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-004")]
    public async Task GetMyOpportunities_ReturnsValidJsonStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-opportunities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-005")]
    public async Task GetOrgUnitRecentUpdates_ReturnsValidJsonStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/org-unit-recent-updates");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("updates", out _).Should().BeTrue();
        result.TryGetProperty("orgUnitName", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-006")]
    public async Task GetContent_ReturnsValidJsonStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/content");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
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
    [Trait("TestId", "TC-DASH-VAL-007")]
    public async Task GetContent_RecordsAreArrays()
    {
        var response = await _client.GetAsync($"{BaseUrl}/content");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var myPartners = result.GetProperty("myPartners");
        myPartners.ValueKind.Should().Be(JsonValueKind.Array);
        var myContacts = result.GetProperty("myContacts");
        myContacts.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-008")]
    public async Task GetMyPartners_RecordsArrayIsValid()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-partners?pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var records = result.GetProperty("records");
        records.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-009")]
    public async Task GetOrgUnitRecentUpdates_UpdatesArrayIsValid()
    {
        var response = await _client.GetAsync($"{BaseUrl}/org-unit-recent-updates?pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var updates = result.GetProperty("updates");
        updates.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-VAL-010")]
    public async Task AllEndpoints_ReturnApplicationJsonContentType()
    {
        var urls = new[]
        {
            $"{BaseUrl}/my-partners",
            $"{BaseUrl}/my-contacts",
            $"{BaseUrl}/my-draft-partners",
            $"{BaseUrl}/my-draft-contacts",
            $"{BaseUrl}/my-interactions",
            $"{BaseUrl}/my-draft-interactions",
            $"{BaseUrl}/my-opportunities",
            $"{BaseUrl}/my-draft-opportunities",
            $"{BaseUrl}/org-unit-recent-updates",
            $"{BaseUrl}/content"
        };
        foreach (var url in urls)
        {
            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json", $"URL: {url}");
            }
        }
    }
}
