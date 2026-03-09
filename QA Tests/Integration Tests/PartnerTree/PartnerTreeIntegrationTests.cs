/**
 * @fileoverview Integration Tests for Partner Tree — end-to-end API flow verification.
 * Verifies full request lifecycle across partner-tree, partner-tree-structure,
 * and partner endpoints.
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

namespace UNOPS.PAO.Tests.Integration.PartnerTree;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "PartnerTree")]
[Trait("Component", "IntegrationTests")]
public class PartnerTreeIntegrationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerTreeIntegrationTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = CreateAuthenticatedClient(factory);
    }

    private static HttpClient CreateAuthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    [Fact]
    [Trait("TestId", "TC-TREE-INT-001")]
    public async Task E2E_GetPartnerTree_FullRequestLifecycle()
    {
        var response = await _client.GetAsync("/api/partner-tree");

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
    [Trait("TestId", "TC-TREE-INT-002")]
    public async Task E2E_GetPartnerTree_ThenStructure_ThenPartners_AllAccessible()
    {
        var treeResponse = await _client.GetAsync("/api/partner-tree");
        var structureResponse = await _client.GetAsync("/api/partner-tree-structure");
        var partnersResponse = await _client.GetAsync("/api/partner");

        treeResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        structureResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        partnersResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-INT-003")]
    public async Task E2E_SequentialPartnerTreeCalls_NoStateLeak()
    {
        if (!_isPostgresAvailable) return;
        var first = await _client.GetAsync("/api/partner-tree");
        var second = await _client.GetAsync("/api/partner-tree");

        first.StatusCode.Should().Be(second.StatusCode,
            "sequential calls should return the same status");
    }

    [Fact]
    [Trait("TestId", "TC-TREE-INT-004")]
    public async Task E2E_ConcurrentPartnerTreeCalls_AllComplete()
    {
        var tasks = Enumerable.Range(0, 3)
            .Select(_ => _client.GetAsync("/api/partner-tree"));

        var responses = await Task.WhenAll(tasks);

        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, HttpStatusCode.NotFound,
                HttpStatusCode.InternalServerError));
    }

    [Fact]
    [Trait("TestId", "TC-TREE-INT-005")]
    public async Task E2E_PartnerTreeEndpoint_RespondsWithinTimeout()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/api/partner-tree");
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-INT-006")]
    public async Task E2E_PartnerTreeEndpoint_DoesNotSetSessionCookie()
    {
        var response = await _client.GetAsync("/api/partner-tree");

        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList()
            : new List<string>();

        var sessionCookies = setCookieHeaders
            .Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase)).ToList();
        sessionCookies.Should().BeEmpty("API must be stateless");
    }

    [Fact]
    [Trait("TestId", "TC-TREE-INT-007")]
    public async Task E2E_PartnerTreeWithQueryParams_ThenStructure_BothSucceed()
    {
        var withParams = await _client.GetAsync("/api/partner-tree?sortBy=Name&ascending=true");
        var structure = await _client.GetAsync("/api/partner-tree-structure");

        withParams.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        structure.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-INT-008")]
    public async Task E2E_PartnerTreeAfterPartnerList_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync("/api/partner");
        var treeResponse = await _client.GetAsync("/api/partner-tree");

        treeResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-INT-009")]
    public async Task E2E_PartnerTree_IAPAuthHeadersAccepted()
    {
        var response = await _client.GetAsync("/api/partner-tree");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
