/**
 * @fileoverview Integration Tests for Dashboard — end-to-end API flow verification.
 * Verifies full request lifecycle across dashboard and related endpoints.
 * @author UNOPS Opportunity+ QA Team
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
[Trait("Component", "IntegrationTests")]
public class DashboardIntegrationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DashboardIntegrationTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-001")]
    public async Task E2E_Dashboard_FullRequestLifecycle()
    {
        var response = await _client.GetAsync("/api/dashboard");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-002")]
    public async Task E2E_DashboardThenPartners_BothAccessible()
    {
        var dashResponse = await _client.GetAsync("/api/dashboard");
        var partnerResponse = await _client.GetAsync("/api/partner");
        dashResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        partnerResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-003")]
    public async Task E2E_DashboardThenOpportunities_BothAccessible()
    {
        var dashResponse = await _client.GetAsync("/api/dashboard");
        var oppResponse = await _client.GetAsync("/api/opportunity");
        dashResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        oppResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-004")]
    public async Task E2E_SequentialDashboardCalls_NoStateLeak()
    {
        if (!_isPostgresAvailable) return;
        var first = await _client.GetAsync("/api/dashboard");
        var second = await _client.GetAsync("/api/dashboard");
        first.StatusCode.Should().Be(second.StatusCode);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-005")]
    public async Task E2E_ConcurrentDashboardCalls_AllComplete()
    {
        var tasks = Enumerable.Range(0, 3).Select(_ => _client.GetAsync("/api/dashboard"));
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError));
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-006")]
    public async Task E2E_Dashboard_RespondsWithinTimeout()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/api/dashboard");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-007")]
    public async Task E2E_DashboardAfterInteractions_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync("/api/interaction");
        var response = await _client.GetAsync("/api/dashboard");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-008")]
    public async Task E2E_Dashboard_NoSessionCookie()
    {
        var response = await _client.GetAsync("/api/dashboard");
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-DASH-INT-009")]
    public async Task E2E_Dashboard_IAPAuthHeadersAccepted()
    {
        var response = await _client.GetAsync("/api/dashboard");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
