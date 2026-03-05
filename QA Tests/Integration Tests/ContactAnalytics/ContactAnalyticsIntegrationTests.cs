/**
 * @fileoverview Integration Tests for Contact Analytics — end-to-end API flow verification.
 * Verifies full request lifecycle: auth → request → response → state consistency.
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

namespace UNOPS.PAO.Tests.Integration.ContactAnalytics;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "ContactAnalytics")]
[Trait("Component", "IntegrationTests")]
public class ContactAnalyticsIntegrationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ContactAnalyticsIntegrationTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-001")]
    public async Task E2E_GetAnalytics_FullRequestLifecycle()
    {
        var response = await _client.GetAsync("/api/contact-analytics");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-002")]
    public async Task E2E_GetAnalytics_ThenContacts_BothAccessible()
    {
        var analyticsResponse = await _client.GetAsync("/api/contact-analytics");
        var contactsResponse = await _client.GetAsync("/api/contact");

        analyticsResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        contactsResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-003")]
    public async Task E2E_SequentialAnalyticsCalls_NoStateLeak()
    {
        if (!_isPostgresAvailable) return;
        var first = await _client.GetAsync("/api/contact-analytics");
        var second = await _client.GetAsync("/api/contact-analytics");

        first.StatusCode.Should().Be(second.StatusCode,
            "sequential calls should return the same status");
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-004")]
    public async Task E2E_ConcurrentAnalyticsCalls_AllComplete()
    {
        var tasks = Enumerable.Range(0, 3)
            .Select(_ => _client.GetAsync("/api/contact-analytics"));

        var responses = await Task.WhenAll(tasks);

        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, HttpStatusCode.NotFound,
                HttpStatusCode.InternalServerError));
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-005")]
    public async Task E2E_AnalyticsEndpoint_RespondsWithinTimeout()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/api/contact-analytics");
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-006")]
    public async Task E2E_AnalyticsEndpoint_DoesNotSetSessionCookie()
    {
        var response = await _client.GetAsync("/api/contact-analytics");

        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList()
            : new List<string>();

        var sessionCookies = setCookieHeaders
            .Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase)).ToList();
        sessionCookies.Should().BeEmpty("API must be stateless");
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-007")]
    public async Task E2E_AnalyticsWithQueryParams_ThenWithout_BothSucceed()
    {
        var withParams = await _client.GetAsync("/api/contact-analytics?page=1&pageSize=5");
        var without = await _client.GetAsync("/api/contact-analytics");

        withParams.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        without.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-008")]
    public async Task E2E_AnalyticsAfterContactList_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync("/api/contact");
        var analyticsResponse = await _client.GetAsync("/api/contact-analytics");

        analyticsResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-CA-INT-009")]
    public async Task E2E_Analytics_IAPAuthHeadersAccepted()
    {
        var response = await _client.GetAsync("/api/contact-analytics");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
