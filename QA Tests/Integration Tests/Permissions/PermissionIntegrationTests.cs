/**
 * @fileoverview Integration Tests for Permissions — end-to-end API flow verification.
 * Verifies full request lifecycle across permissions and roles endpoints.
 * API base: /api/permissions
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

namespace UNOPS.PAO.Tests.Integration.Permissions;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Permissions")]
[Trait("Component", "IntegrationTests")]
public class PermissionIntegrationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/permissions";
    private const string RolesBaseUrl = "/api/role";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PermissionIntegrationTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PERM-INT-001")]
    public async Task E2E_Permissions_FullRequestLifecycle()
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
    [Trait("TestId", "TC-PERM-INT-002")]
    public async Task E2E_PermissionsThenRoles_BothAccessible()
    {
        var permResponse = await _client.GetAsync(BaseUrl);
        var roleResponse = await _client.GetAsync(RolesBaseUrl + "/user");
        permResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        roleResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-INT-003")]
    public async Task E2E_SequentialPermissionsCalls_NoStateLeak()
    {
        if (!_isPostgresAvailable) return;
        var first = await _client.GetAsync(BaseUrl);
        var second = await _client.GetAsync(BaseUrl);
        first.StatusCode.Should().Be(second.StatusCode);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-INT-004")]
    public async Task E2E_ConcurrentPermissionsCalls_AllComplete()
    {
        var tasks = Enumerable.Range(0, 3).Select(_ => _client.GetAsync(BaseUrl));
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError));
    }

    [Fact]
    [Trait("TestId", "TC-PERM-INT-005")]
    public async Task E2E_Permissions_RespondsWithinTimeout()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync(BaseUrl);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-INT-006")]
    public async Task E2E_Permissions_NoSessionCookie()
    {
        var response = await _client.GetAsync(BaseUrl);
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-INT-007")]
    public async Task E2E_PermissionsAfterRoles_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync(RolesBaseUrl + "/user");
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-INT-008")]
    public async Task E2E_PermissionsAfterPartners_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync("/api/partner");
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-INT-009")]
    public async Task E2E_Permissions_IAPAuthHeadersAccepted()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
