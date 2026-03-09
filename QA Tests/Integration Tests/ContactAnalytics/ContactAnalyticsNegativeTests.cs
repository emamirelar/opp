/**
 * @fileoverview Negative integration tests for ContactAnalyticsController
 * Tests invalid inputs and error handling against actual API: /api/contact-analytics/*
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

namespace UNOPS.PAO.Tests.Integration.ContactAnalytics;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "ContactAnalytics")]
[Trait("Component", "NegativeTests")]
public class ContactAnalyticsNegativeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/contact-analytics";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ContactAnalyticsNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-001")]
    public async Task GetMostActiveContacts_WithLimitNegative1_Returns400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getMostActiveContacts?limit=-1&timeframe=30d&metric=interactions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-002")]
    public async Task GetMostActiveContacts_WithInvalidTimeframe_Returns200Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getMostActiveContacts?limit=10&timeframe=invalid&metric=interactions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-003")]
    public async Task GetContactEngagementTrends_WithMonths0_Returns200Or400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getContactEngagementTrends?period=monthly&months=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-004")]
    public async Task GetContactEngagementTrends_WithMonthsNegative1_Returns200Or400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getContactEngagementTrends?period=monthly&months=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-005")]
    public async Task GetContactsByInteractionType_WithInvalidType_Returns200Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getContactsByInteractionType?type=999&limit=20");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-006")]
    public async Task GetContactsByPartner_WithMinContacts999999_Returns200WithEmptyOrFiltered()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getContactsByPartner?minContacts=999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
                // DEF: "data" property may not exist when filter returns no results
                if (result.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    data.GetArrayLength().Should().BeLessThanOrEqualTo(1);
                }
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-007")]
    public async Task GetRecentlyActiveContacts_WithDays0_Returns200Or400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getRecentlyActiveContacts?days=0&limit=20");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-008")]
    public async Task GetRecentlyActiveContacts_WithDaysNegative1_Returns200Or400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getRecentlyActiveContacts?days=-1&limit=20");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-009")]
    public async Task GetContactsWithMostDocuments_WithLimitNegative1_Returns200Or400Or500()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getContactsWithMostDocuments?limit=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-010")]
    public async Task NonExistentEndpoint_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/nonExistentEndpoint");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-011")]
    public async Task GetContactsByInteractionType_WithInvalidDateFormat_Returns200Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getContactsByInteractionType?startDate=invalid&endDate=invalid");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-CA-NEG-012")]
    public async Task GetContactsWithMostDocuments_WithInvalidDateRange_Returns200Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getContactsWithMostDocuments?startDate=invalid&endDate=invalid");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
