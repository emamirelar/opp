/**
 * @fileoverview Validation integration tests for ContactAnalyticsController
 * Tests response structure and parameter validation against actual API: /api/contact-analytics/*
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
[Trait("Component", "ValidationTests")]
public class ContactAnalyticsValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/contact-analytics";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ContactAnalyticsValidationTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-001")]
    public async Task GetMostActiveContacts_ResponseHasExpectedStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getMostActiveContacts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.TryGetProperty("data", out _).Should().BeTrue();
        result.TryGetProperty("timeframe", out _).Should().BeTrue();
        result.TryGetProperty("metric", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-002")]
    public async Task GetContactsByGeographicRegion_ResponseHasDataArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getContactsByGeographicRegion");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        result.TryGetProperty("period", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-003")]
    public async Task GetContactEngagementTrends_DataIsArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getContactEngagementTrends");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        result.TryGetProperty("period", out _).Should().BeTrue();
        result.TryGetProperty("months", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-004")]
    public async Task GetContactsByPartner_ResponseHasValidMetadata()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getContactsByPartner");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.TryGetProperty("minContacts", out _).Should().BeTrue();
        result.TryGetProperty("includeInactive", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-005")]
    public async Task GetRecentlyActiveContacts_ResponseHasCorrectContentType()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getRecentlyActiveContacts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("days", out _).Should().BeTrue();
        result.TryGetProperty("sortBy", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-006")]
    public async Task GetContactsByJobTitle_ResponseHasValidStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getContactsByJobTitle");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-007")]
    public async Task GetContactGrowthTrends_ResponseHasPeriodAndMonths()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getContactGrowthTrends");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("period", out var period).Should().BeTrue();
        result.TryGetProperty("months", out var months).Should().BeTrue();
        period.GetString().Should().NotBeNullOrEmpty();
        months.GetInt32().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-008")]
    public async Task GetContactsWithMostDocuments_ResponseHasDataAndTotal()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getContactsWithMostDocuments");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.TryGetProperty("data", out _).Should().BeTrue();
        result.TryGetProperty("total", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-009")]
    public async Task GetContactsByInteractionType_ResponseHasValidFields()
    {
        var response = await _client.GetAsync($"{BaseUrl}/getContactsByInteractionType");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetProperty("success").GetBoolean().Should().BeTrue();
        result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-CA-VAL-010")]
    public async Task AllEndpoints_ReturnJsonContentType()
    {
        var urls = new[]
        {
            $"{BaseUrl}/getMostActiveContacts",
            $"{BaseUrl}/getContactsByGeographicRegion",
            $"{BaseUrl}/getContactEngagementTrends",
            $"{BaseUrl}/getContactsByInteractionType",
            $"{BaseUrl}/getContactsByPartner",
            $"{BaseUrl}/getRecentlyActiveContacts",
            $"{BaseUrl}/getContactsByJobTitle",
            $"{BaseUrl}/getContactGrowthTrends",
            $"{BaseUrl}/getContactsWithMostDocuments"
        };
        foreach (var url in urls)
        {
            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json", $"because {url} should return JSON");
            }
        }
    }
}
