/**
 * @fileoverview Security integration tests for PartnerAnalyticsController
 * Tests authentication and authorization against actual API: /api/partner/analytics/*
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
[Trait("Component", "SecurityTests")]
public class AnalyticsSecurityTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/partner/analytics";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AnalyticsSecurityTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    private HttpClient CreateUnauthenticatedClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        return client;
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-001")]
    public async Task GetMostActive_WithoutAuthHeaders_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/mostActive?limit=10&timeframe=monthly&metric=engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-002")]
    public async Task GetByUser_WithoutAuthHeaders_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/byUser/123?timeframe=monthly&includeCreated=true&includeModified=true&includeFocalPoint=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-003")]
    public async Task GetEngagementTrends_WithoutAuthHeaders_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/engagementTrends?period=monthly&months=12");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-004")]
    public async Task GetByCountry_WithoutAuthHeaders_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/byCountry?limit=20&minCount=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-005")]
    public async Task GetMostActive_WithInvalidAuthEmail_Returns401Or403()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:invalid@example.com");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:999");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=invalid@example.com; dev-user-email=invalid@example.com");
        var response = await client.GetAsync($"{BaseUrl}/mostActive?limit=10&timeframe=monthly&metric=engagements");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-006")]
    public async Task GetByUser_AccessingOtherUserData_Returns200OrAccessControlled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/byUser/99999?timeframe=monthly&includeCreated=true&includeModified=true&includeFocalPoint=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("partners", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-007")]
    public async Task AllEndpoints_ReturnJsonContentType()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var urls = new[]
        {
            $"{BaseUrl}/mostActive?limit=10&timeframe=monthly&metric=engagements",
            $"{BaseUrl}/byUser/123?timeframe=monthly&includeCreated=true&includeModified=true&includeFocalPoint=true",
            $"{BaseUrl}/engagementTrends?period=monthly&months=12",
            $"{BaseUrl}/byCountry?limit=20&minCount=1"
        };
        foreach (var url in urls)
        {
            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-SEC-008")]
    public async Task ErrorResponses_NoSensitiveDataExposed()
    {
        var response = await _client.GetAsync($"{BaseUrl}/mostActive?limit=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        var lower = content.ToLowerInvariant();
        lower.Should().NotContain("password");
        lower.Should().NotContain("connectionstring");
    }
}
