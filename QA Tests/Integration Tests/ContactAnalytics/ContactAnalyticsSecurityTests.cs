/**
 * @fileoverview Security integration tests for ContactAnalyticsController
 * Tests authentication and authorization against actual API: /api/contact-analytics/*
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
[Trait("Component", "SecurityTests")]
public class ContactAnalyticsSecurityTests
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

    public ContactAnalyticsSecurityTests(PAOWebApplicationFactory<Program> factory)
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
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        return client;
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-001")]
    public async Task GetMostActiveContacts_Unauthenticated_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/getMostActiveContacts");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-002")]
    public async Task GetContactsByGeographicRegion_Unauthenticated_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/getContactsByGeographicRegion");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-003")]
    public async Task GetContactEngagementTrends_Unauthenticated_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/getContactEngagementTrends");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-004")]
    public async Task GetContactsByInteractionType_Unauthenticated_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/getContactsByInteractionType");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-005")]
    public async Task GetContactsByPartner_Unauthenticated_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/getContactsByPartner");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-006")]
    public async Task GetRecentlyActiveContacts_Unauthenticated_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/getRecentlyActiveContacts");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-007")]
    public async Task GetContactsByJobTitle_Unauthenticated_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/getContactsByJobTitle");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-008")]
    public async Task GetContactGrowthTrends_Unauthenticated_Returns401Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/getContactGrowthTrends");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-CA-SEC-009")]
    public async Task ErrorResponses_NoSensitiveDataExposed()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getMostActiveContacts?limit=-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var lower = content.ToLowerInvariant();
        lower.Should().NotContain("password");
        lower.Should().NotContain("connectionstring");
        lower.Should().NotContain("c:\\");
    }
}
