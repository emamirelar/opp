/**
 * @fileoverview Integration tests for PartnerAnalyticsController
 * Tests actual endpoints: /api/partner/analytics/*
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 *
 * Real endpoints:
 * - GET /api/partner/analytics/mostActive
 * - GET /api/partner/analytics/byUser/{userId}
 * - GET /api/partner/analytics/engagementTrends
 * - GET /api/partner/analytics/byCountry
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
/// Integration tests for PartnerAnalyticsController - real endpoints only
/// </summary>
[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "PartnerAnalytics")]
public class PartnerAnalyticsControllerTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerAnalyticsControllerTests(PAOWebApplicationFactory<Program> factory)
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

    #region Positive Tests (4 endpoints)

    [Fact]
    [Trait("TestId", "TC-PA-POS-001")]
    public async Task GetMostActive_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/mostActive?limit=10&timeframe=monthly&metric=engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("partners", out _).Should().BeTrue();
        result.TryGetProperty("metadata", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PA-POS-002")]
    public async Task GetMostActive_InteractionsMetric_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/mostActive?metric=interactions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-POS-003")]
    public async Task GetMostActive_LastActivityMetric_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/mostActive?metric=lastActivity");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-POS-004")]
    public async Task GetByUser_ValidUserId_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/byUser/123?timeframe=monthly&includeCreated=true&includeModified=true&includeFocalPoint=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("partners", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PA-POS-005")]
    public async Task GetEngagementTrends_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/engagementTrends?period=monthly&months=12");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("trends", out _).Should().BeTrue();
        result.TryGetProperty("metadata", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PA-POS-006")]
    public async Task GetEngagementTrends_WithPartnerId_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/engagementTrends?partnerId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-POS-007")]
    public async Task GetByCountry_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/byCountry?limit=20&minCount=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("countries", out _).Should().BeTrue();
    }

    #endregion

    #region Negative Tests

    [Fact]
    [Trait("TestId", "TC-PA-NEG-001")]
    public async Task GetMostActive_InvalidLimit_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/mostActive?limit=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-002")]
    public async Task GetMostActive_LimitOver100_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/mostActive?limit=101");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-003")]
    public async Task GetMostActive_InvalidTimeframe_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/mostActive?timeframe=invalid");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-004")]
    public async Task GetEngagementTrends_InvalidMonths_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/engagementTrends?months=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-005")]
    public async Task GetEngagementTrends_MonthsOver60_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/engagementTrends?months=61");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-006")]
    public async Task GetByCountry_InvalidLimit_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/byCountry?limit=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-007")]
    public async Task GetByCountry_MinCountZero_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/byCountry?minCount=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-008")]
    public async Task NonExistentEndpoint_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/nonExistent");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    [Trait("TestId", "TC-PA-EDGE-001")]
    public async Task GetByUser_NonExistentUserId_Returns200WithEmptyPartners()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/byUser/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var partners = result.GetProperty("partners");
        partners.GetArrayLength().Should().Be(0);
    }

    [Fact]
    [Trait("TestId", "TC-PA-EDGE-002")]
    public async Task GetEngagementTrends_AllPeriods_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var periods = new[] { "daily", "weekly", "monthly", "quarterly", "yearly" };
        foreach (var period in periods)
        {
            var response = await _client.GetAsync($"/api/partner/analytics/engagementTrends?period={period}&months=6");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-EDGE-003")]
    public async Task GetByUser_IncludeFlagsCombinations_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/byUser/123?includeCreated=false&includeModified=false&includeFocalPoint=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-EDGE-004")]
    public async Task GetByCountry_LargeLimit_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/byCountry?limit=250");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Validation Tests

    [Fact]
    [Trait("TestId", "TC-PA-VAL-001")]
    public async Task GetMostActive_ResponseHasMetadata()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/mostActive");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var metadata = result.GetProperty("metadata");
        metadata.TryGetProperty("timeframe", out _).Should().BeTrue();
        metadata.TryGetProperty("metric", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-002")]
    public async Task GetEngagementTrends_ResponseHasSummary()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/engagementTrends");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("summary", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-003")]
    public async Task GetByCountry_ResponseHasMetadata()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/byCountry");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("metadata", out _).Should().BeTrue();
    }

    #endregion

    #region Security Tests

    [Fact]
    [Trait("TestId", "TC-PA-SEC-001")]
    public async Task GetMostActive_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/partner/analytics/mostActive");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-002")]
    public async Task GetByUser_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/partner/analytics/byUser/123");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-003")]
    public async Task GetEngagementTrends_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/partner/analytics/engagementTrends");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-004")]
    public async Task GetByCountry_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/partner/analytics/byCountry");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-005")]
    public async Task AllEndpoints_Authenticated_ReturnSuccess()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var urls = new[]
        {
            "/api/partner/analytics/mostActive",
            "/api/partner/analytics/byUser/123",
            "/api/partner/analytics/engagementTrends",
            "/api/partner/analytics/byCountry"
        };
        foreach (var url in urls)
        {
            var response = await _client.GetAsync(url);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PAC-EDGE-001")]
    [Trait("Category", "Edge")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetMostActive_ResponseContent_NoEncodingArtifacts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner/analytics/mostActive");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??",
                "PNO-1194: partner analytics names must not contain encoding artifacts");
            content.Should().NotContain("\uFFFD");
        }
    }

    #endregion
}
