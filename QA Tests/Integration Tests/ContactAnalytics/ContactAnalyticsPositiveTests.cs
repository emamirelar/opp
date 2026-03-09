/**
 * @fileoverview Positive integration tests for ContactAnalytics
 * Tests happy-path scenarios against actual API endpoints
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.ContactAnalytics;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "ContactAnalytics")]
[Trait("Component", "PositiveTests")]
public class ContactAnalyticsPositiveTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/contact-analytics";

    public ContactAnalyticsPositiveTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-001")]
    public async Task GetContactAnalytics_WithValidAuth_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-002")]
    public async Task GetMostActiveContacts_WithValidParams_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getMostActiveContacts?limit=10&timeframe=30d&metric=interactions");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }

    [Fact]
    [Trait("TestId", "TC-CA-POS-003")]
    public async Task GetContactEngagementTrends_WithValidParams_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/getContactEngagementTrends?period=monthly&months=6");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }
}
