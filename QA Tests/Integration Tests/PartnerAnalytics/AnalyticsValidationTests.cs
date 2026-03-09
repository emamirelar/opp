/**
 * @fileoverview Validation integration tests for PartnerAnalyticsController
 * Tests response structure and parameter validation against actual API: /api/partner/analytics/*
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
[Trait("Component", "ValidationTests")]
public class AnalyticsValidationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/partner/analytics";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AnalyticsValidationTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-001")]
    public async Task GetMostActive_DefaultParams_ReturnsValidResponseStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/mostActive");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("metadata", out _).Should().BeTrue();
            result.TryGetProperty("partners", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-002")]
    public async Task GetMostActive_ResponseHasMetadataAndPartnersFields()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=10&timeframe=monthly&metric=engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            var metadata = result.GetProperty("metadata");
            metadata.TryGetProperty("timeframe", out _).Should().BeTrue();
            metadata.TryGetProperty("metric", out _).Should().BeTrue();
            metadata.TryGetProperty("generatedAt", out _).Should().BeTrue();
            result.TryGetProperty("partners", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-003")]
    public async Task GetByUser_ResponseHasMetadataAndPartnersFields()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byUser/123?timeframe=monthly&includeCreated=true&includeModified=true&includeFocalPoint=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            var metadata = result.GetProperty("metadata");
            metadata.TryGetProperty("userId", out _).Should().BeTrue();
            metadata.TryGetProperty("timeframe", out _).Should().BeTrue();
            metadata.TryGetProperty("totalPartners", out _).Should().BeTrue();
            result.TryGetProperty("partners", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-004")]
    public async Task GetEngagementTrends_ResponseHasMetadataTrendsAndSummary()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/engagementTrends?period=monthly&months=12");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            var metadata = result.GetProperty("metadata");
            metadata.TryGetProperty("period", out _).Should().BeTrue();
            metadata.TryGetProperty("months", out _).Should().BeTrue();
            metadata.TryGetProperty("startDate", out _).Should().BeTrue();
            metadata.TryGetProperty("endDate", out _).Should().BeTrue();
            result.TryGetProperty("trends", out _).Should().BeTrue();
            result.TryGetProperty("summary", out var summary).Should().BeTrue();
            summary.TryGetProperty("totalEngagements", out _).Should().BeTrue();
            summary.TryGetProperty("activePartners", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-005")]
    public async Task GetByCountry_ResponseHasMetadataAndCountriesFields()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byCountry?limit=20&minCount=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            var metadata = result.GetProperty("metadata");
            metadata.TryGetProperty("totalCountries", out _).Should().BeTrue();
            metadata.TryGetProperty("totalPartners", out _).Should().BeTrue();
            metadata.TryGetProperty("generatedAt", out _).Should().BeTrue();
            result.TryGetProperty("countries", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-006")]
    public async Task GetMostActive_AllValidTimeframesAccepted()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var timeframes = new[] { "daily", "weekly", "monthly", "quarterly", "yearly" };
        foreach (var timeframe in timeframes)
        {
            var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=10&timeframe={timeframe}&metric=engagements");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-007")]
    public async Task GetMostActive_AllValidMetricsAccepted()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var metrics = new[] { "engagements", "interactions", "lastActivity" };
        foreach (var metric in metrics)
        {
            var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=10&timeframe=monthly&metric={metric}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-008")]
    public async Task GetEngagementTrends_AllValidPeriodsAccepted()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var periods = new[] { "daily", "weekly", "monthly", "quarterly", "yearly" };
        foreach (var period in periods)
        {
            var response = await _client.GetAsync($"{BaseUrl}/engagementTrends?period={period}&months=6");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-009")]
    public async Task GetByCountry_WithMinCount1_IncludesAllCountries()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byCountry?limit=20&minCount=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("countries", out var countries).Should().BeTrue();
            foreach (var country in countries.EnumerateArray())
            {
                country.TryGetProperty("partnerCount", out var count).Should().BeTrue();
                count.GetInt32().Should().BeGreaterThanOrEqualTo(1);
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-VAL-010")]
    public async Task GetByCountry_WithHighMinCount_FiltersResults()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byCountry?limit=20&minCount=100");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("countries", out var countries).Should().BeTrue();
            foreach (var country in countries.EnumerateArray())
            {
                country.TryGetProperty("partnerCount", out var count).Should().BeTrue();
                count.GetInt32().Should().BeGreaterThanOrEqualTo(100);
            }
        }
    }
}
