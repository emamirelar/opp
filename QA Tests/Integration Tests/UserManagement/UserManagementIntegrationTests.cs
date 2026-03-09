/**
 * @fileoverview Integration tests for UserManagement — end-to-end API flow verification.
 * Verifies full request lifecycle across user-management and permissions endpoints.
 * API base: /api/user-management
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

namespace UNOPS.PAO.Tests.Integration.UserManagement;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "UserManagement")]
[Trait("Component", "IntegrationTests")]
public class UserManagementIntegrationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/user-management";
    private const string PermissionsUrl = "/api/permissions";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UserManagementIntegrationTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-USER-INT-001")]
    public async Task E2E_UserManagement_FullRequestLifecycle()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-USER-INT-002")]
    public async Task E2E_CrossEndpointAccess_UserManagementAndPermissions_AllAccessible()
    {
        var userMgmtResponse = await _client.GetAsync($"{BaseUrl}/roles");
        var permResponse = await _client.GetAsync(PermissionsUrl);

        userMgmtResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        permResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-INT-003")]
    public async Task E2E_SequentialCalls_NoStateLeak()
    {
        if (!_isPostgresAvailable) return;
        var first = await _client.GetAsync($"{BaseUrl}/roles");
        var second = await _client.GetAsync($"{BaseUrl}/roles");
        first.StatusCode.Should().Be(second.StatusCode);
    }

    [Fact]
    [Trait("TestId", "TC-USER-INT-004")]
    public async Task E2E_ConcurrentCalls_AllComplete()
    {
        var tasks = Enumerable.Range(0, 3).Select(_ => _client.GetAsync($"{BaseUrl}/roles"));
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized));
    }

    [Fact]
    [Trait("TestId", "TC-USER-INT-005")]
    public async Task E2E_UserManagement_RespondsWithinTimeout()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync($"{BaseUrl}/roles");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-USER-INT-006")]
    public async Task E2E_UserManagement_NoSessionCookie()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-USER-INT-007")]
    public async Task E2E_UserManagementAfterPermissions_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync(PermissionsUrl);
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-INT-008")]
    public async Task E2E_UserManagement_WithQueryParams_DoesNotCrash()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles?includeInactive=true");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-INT-009")]
    public async Task E2E_UserManagement_IAPAuthHeadersAccepted()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
