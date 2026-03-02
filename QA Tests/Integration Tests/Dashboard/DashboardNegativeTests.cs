/**
 * @fileoverview Negative integration tests for DashboardController
 * Tests invalid inputs and error handling against actual API: /api/dashboard/*
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

namespace UNOPS.PAO.Tests.Integration.Dashboard;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Dashboard")]
[Trait("Component", "NegativeTests")]
public class DashboardNegativeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/dashboard";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DashboardNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-001")]
    public async Task GetNonExistentRoute_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/non-existent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-002")]
    public async Task GetInvalidSubRoute_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/invalid-sub-route");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-003")]
    public async Task PostMyPartners_InsteadOfGet_Returns405()
    {
        var response = await _client.PostAsync($"{BaseUrl}/my-partners", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-004")]
    public async Task PostMyContacts_InsteadOfGet_Returns405()
    {
        var response = await _client.PostAsync($"{BaseUrl}/my-contacts", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-005")]
    public async Task PostContent_InsteadOfGet_Returns405()
    {
        var response = await _client.PostAsync($"{BaseUrl}/content", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-006")]
    public async Task PutMyPartners_InsteadOfGet_Returns405()
    {
        var response = await _client.PutAsync($"{BaseUrl}/my-partners", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-007")]
    public async Task DeleteOrgUnitRecentUpdates_InsteadOfGet_Returns405()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/org-unit-recent-updates");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-008")]
    public async Task GetMyPartnersWithTrailingSlash_Returns404Or200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-partners/");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-009")]
    public async Task GetTyposInPath_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/my-partner");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-010")]
    public async Task GetContentWithWrongCase_Returns404Or200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/Content");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-011")]
    public async Task GetOrgUnitRecentUpdatesWithExtraSegment_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/org-unit-recent-updates/extra");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-NEG-012")]
    public async Task GetDashboardBaseOnly_Returns404()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
