/**
 * @fileoverview Negative integration tests for PartnerAnalyticsController
 * Tests invalid inputs and error handling against actual API: /api/partner/analytics/*
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

namespace UNOPS.PAO.Tests.Integration.PartnerAnalytics;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "PartnerAnalytics")]
[Trait("Component", "NegativeTests")]
public class AnalyticsNegativeTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/partner/analytics";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AnalyticsNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-001")]
    public async Task GetMostActive_WithLimit0_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=0&timeframe=monthly&metric=engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-002")]
    public async Task GetMostActive_WithLimitNegative1_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=-1&timeframe=monthly&metric=engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-003")]
    public async Task GetMostActive_WithLimit101_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=101&timeframe=monthly&metric=engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-004")]
    public async Task GetMostActive_WithInvalidTimeframe_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=10&timeframe=invalid&metric=engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-005")]
    public async Task GetMostActive_WithInvalidMetric_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=10&timeframe=monthly&metric=invalid");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-006")]
    public async Task GetByUser_WithNonexistentUserId_Returns200WithEmptyResults()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byUser/999999?timeframe=monthly&includeCreated=true&includeModified=true&includeFocalPoint=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("partners", out var partners).Should().BeTrue();
            partners.GetArrayLength().Should().Be(0);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-007")]
    public async Task GetEngagementTrends_WithMonths0_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/engagementTrends?period=monthly&months=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-008")]
    public async Task GetEngagementTrends_WithMonthsNegative1_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/engagementTrends?period=monthly&months=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-009")]
    public async Task GetEngagementTrends_WithMonths61_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/engagementTrends?period=monthly&months=61");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-010")]
    public async Task GetByCountry_WithLimit0_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byCountry?limit=0&minCount=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-011")]
    public async Task GetByCountry_WithLimitNegative1_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byCountry?limit=-1&minCount=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-PA-NEG-012")]
    public async Task GetByCountry_WithMinCount0_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byCountry?limit=20&minCount=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest);
    }
}
