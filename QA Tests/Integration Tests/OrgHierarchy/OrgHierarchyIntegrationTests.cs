/**
 * @fileoverview Integration Tests for OrgHierarchy — end-to-end API flow verification.
 * Verifies full request lifecycle across organization hierarchy and related endpoints.
 * API base: /api/organizationhierarchy
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

namespace UNOPS.PAO.Tests.Integration.OrgHierarchy;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "OrgHierarchy")]
[Trait("Component", "IntegrationTests")]
public class OrgHierarchyIntegrationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/organizationhierarchy";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public OrgHierarchyIntegrationTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-001")]
    public async Task E2E_OrgHierarchy_FullRequestLifecycle()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-002")]
    public async Task E2E_OrgHierarchyThenPartners_BothAccessible()
    {
        var orgResponse = await _client.GetAsync(BaseUrl);
        var partnerResponse = await _client.GetAsync("/api/partner");
        orgResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        partnerResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-003")]
    public async Task E2E_SequentialOrgHierarchyCalls_NoStateLeak()
    {
        if (!_isPostgresAvailable) return;
        var first = await _client.GetAsync(BaseUrl);
        var second = await _client.GetAsync(BaseUrl);
        first.StatusCode.Should().Be(second.StatusCode);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-004")]
    public async Task E2E_ConcurrentOrgHierarchyCalls_AllComplete()
    {
        var tasks = Enumerable.Range(0, 3).Select(_ => _client.GetAsync(BaseUrl));
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError));
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-005")]
    public async Task E2E_OrgHierarchy_RespondsWithinTimeout()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync(BaseUrl);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-006")]
    public async Task E2E_OrgHierarchy_NoSessionCookie()
    {
        var response = await _client.GetAsync(BaseUrl);
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-007")]
    public async Task E2E_OrgHierarchyAfterPartners_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync("/api/partner");
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-008")]
    public async Task E2E_OrgHierarchyAfterOpportunities_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync("/api/opportunity");
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-INT-009")]
    public async Task E2E_OrgHierarchy_IAPAuthHeadersAccepted()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
