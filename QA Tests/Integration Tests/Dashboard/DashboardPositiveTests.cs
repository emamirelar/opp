/**
 * @fileoverview Positive integration tests for Dashboard
 * Tests happy-path scenarios against actual API endpoints
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Dashboard;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Dashboard")]
[Trait("Component", "PositiveTests")]
public class DashboardPositiveTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/dashboard";

    public DashboardPositiveTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-001")]
    public async Task GetDashboard_WithValidAuth_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-002")]
    public async Task GetMyPartners_WithValidAuth_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/my-partners");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-POS-003")]
    public async Task GetMyContacts_WithValidAuth_ReturnsSuccessStatusCode()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/my-contacts");
        ((int)response.StatusCode).Should().BeInRange(200, 299, "authenticated request to valid endpoint should succeed");
    }
}
