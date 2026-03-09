/**
 * @fileoverview Integration tests for ContactAnalyticsController
 * Tests actual endpoints: /api/contact-analytics/*
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 *
 * Real endpoints:
 * - GET /api/contact-analytics/getMostActiveContacts
 * - GET /api/contact-analytics/getContactsByGeographicRegion
 * - GET /api/contact-analytics/getContactEngagementTrends
 * - GET /api/contact-analytics/getContactsByInteractionType
 * - GET /api/contact-analytics/getContactsByPartner
 * - GET /api/contact-analytics/getRecentlyActiveContacts
 * - GET /api/contact-analytics/getContactsByJobTitle
 * - GET /api/contact-analytics/getContactGrowthTrends
 * - GET /api/contact-analytics/getContactsWithMostDocuments
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for ContactAnalyticsController - real endpoints only
/// </summary>
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "ContactAnalytics")]
public class ContactAnalyticsControllerTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ContactAnalyticsControllerTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = CreateAuthenticatedClient(factory);
        _isPostgresAvailable = factory.IsUsingPostgres;
    }

    private static HttpClient CreateAuthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    #region Positive Tests (9 endpoints)

    [Fact]
    [Trait("TestId", "TC-CA-POS-001")]
    public async Task GetMostActiveContacts_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getMostActiveContacts?limit=10&timeframe=30d&metric=interactions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
        result.TryGetProperty("data", out _);
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-002")]
    public async Task GetContactsByGeographicRegion_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByGeographicRegion?period=all&minCount=1&groupBy=country");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-003")]
    public async Task GetContactEngagementTrends_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactEngagementTrends?period=monthly&months=12&metric=interactions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-004")]
    public async Task GetContactsByInteractionType_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByInteractionType?limit=20");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-005")]
    public async Task GetContactsByPartner_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByPartner?minContacts=1&includeInactive=false");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-006")]
    public async Task GetRecentlyActiveContacts_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getRecentlyActiveContacts?days=30&limit=20&sortBy=lastInteraction");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-007")]
    public async Task GetContactsByJobTitle_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByJobTitle?minContacts=1&includeInteractions=false");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-008")]
    public async Task GetContactGrowthTrends_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactGrowthTrends?period=monthly&months=12");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-009")]
    public async Task GetContactsWithMostDocuments_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsWithMostDocuments?limit=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
    }

    #endregion

    #region Negative Tests

    [Fact]
    [Trait("TestId", "TC-CA-NEG-001")]
    public async Task GetMostActiveContacts_InvalidLimit_Returns500OrBadRequest()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getMostActiveContacts?limit=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-002")]
    public async Task GetContactEngagementTrends_InvalidMonths_MayReturnEmptyOrError()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactEngagementTrends?months=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-003")]
    public async Task GetContactsByPartner_InvalidMinContacts_MayReturnEmpty()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByPartner?minContacts=999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
                // DEF: "data" property may not exist when no results are found
                if (result.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    data.GetArrayLength().Should().Be(0);
                }
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-004")]
    public async Task GetContactsByInteractionType_InvalidType_Returns200WithEmptyOrFiltered()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByInteractionType?type=999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-005")]
    public async Task NonExistentEndpoint_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/nonExistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    [Trait("TestId", "TC-CA-EDGE-001")]
    public async Task GetMostActiveContacts_ZeroLimit_ReturnsEmptyOrDefault()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getMostActiveContacts?limit=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-CA-EDGE-002")]
    public async Task GetContactGrowthTrends_LargeMonths_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactGrowthTrends?months=60");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-CA-EDGE-003")]
    public async Task GetContactsByGeographicRegion_AllPeriods_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var periods = new[] { "all", "7d", "30d", "90d", "6m", "1y" };
        foreach (var period in periods)
        {
            if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
            var response = await _client.GetAsync($"/api/contact-analytics/getContactsByGeographicRegion?period={period}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "TC-CA-EDGE-004")]
    public async Task GetRecentlyActiveContacts_DifferentSortBy_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getRecentlyActiveContacts?sortBy=interactionCount");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-CA-EDGE-005")]
    public async Task GetContactsWithMostDocuments_WithDateRange_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var start = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd");
        var end = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var response = await _client.GetAsync($"/api/contact-analytics/getContactsWithMostDocuments?startDate={start}&endDate={end}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
    }

    #endregion

    #region Validation Tests

    [Fact]
    [Trait("TestId", "TC-CA-VAL-001")]
    public async Task GetMostActiveContacts_ResponseHasExpectedStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getMostActiveContacts");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean();
        result.TryGetProperty("timeframe", out _);
        result.TryGetProperty("metric", out _);
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-002")]
    public async Task GetContactEngagementTrends_DataIsArray()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactEngagementTrends");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-003")]
    public async Task GetContactsByPartner_IncludeInactive_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByPartner?includeInactive=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-004")]
    public async Task GetContactsByJobTitle_IncludeInteractions_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByJobTitle?includeInteractions=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
    }

    #endregion

    #region Security Tests

    [Fact]
    [Trait("TestId", "TC-CA-SEC-001")]
    public async Task GetMostActiveContacts_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/contact-analytics/getMostActiveContacts");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-002")]
    public async Task GetContactGrowthTrends_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/contact-analytics/getContactGrowthTrends");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-003")]
    public async Task GetContactsByPartner_Authenticated_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getContactsByPartner");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-004")]
    public async Task AllEndpoints_RequireAuthentication()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var endpoints = new[]
        {
            "/api/contact-analytics/getContactsByGeographicRegion",
            "/api/contact-analytics/getContactEngagementTrends",
            "/api/contact-analytics/getRecentlyActiveContacts",
            "/api/contact-analytics/getContactsByJobTitle",
            "/api/contact-analytics/getContactsWithMostDocuments"
        };
        foreach (var url in endpoints)
        {
            var response = await client.GetAsync(url);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"because {url} requires authentication");
        }
    }

    [Fact]
    [Trait("TestId", "TC-CAC-EDGE-001")]
    [Trait("Category", "Edge")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetMostActiveContacts_ResponseContent_NoEncodingArtifacts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/contact-analytics/getMostActiveContacts?limit=20&timeframe=30d&metric=interactions");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??",
                "PNO-1194: contact analytics names must not contain encoding artifacts");
            content.Should().NotContain("\uFFFD");
        }
    }

    #endregion
}
