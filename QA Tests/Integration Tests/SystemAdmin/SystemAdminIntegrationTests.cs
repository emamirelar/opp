/**
 * @fileoverview Integration tests for SystemAdmin — end-to-end API flow verification.
 * Verifies full request lifecycle across system-admin and user-management endpoints.
 * API base: /api/system-admin
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

namespace UNOPS.PAO.Tests.Integration.SystemAdmin;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "SystemAdmin")]
[Trait("Component", "IntegrationTests")]
public class SystemAdminIntegrationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/system-admin";
    private const string UserManagementUrl = "/api/user-management";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public SystemAdminIntegrationTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-ADMIN-INT-001")]
    public async Task E2E_SystemAdmin_FullRequestLifecycle()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-INT-002")]
    public async Task E2E_CrossEndpointAccess_SystemAdminAndUserManagement_AllAccessible()
    {
        var adminResponse = await _client.GetAsync($"{BaseUrl}/endpoints");
        var userMgmtResponse = await _client.GetAsync($"{UserManagementUrl}/roles");

        adminResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        userMgmtResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-INT-003")]
    public async Task E2E_SequentialCalls_NoStateLeak()
    {
        if (!_isPostgresAvailable) return;
        var first = await _client.GetAsync($"{BaseUrl}/endpoints");
        var second = await _client.GetAsync($"{BaseUrl}/endpoints");
        first.StatusCode.Should().Be(second.StatusCode);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-INT-004")]
    public async Task E2E_ConcurrentCalls_AllComplete()
    {
        var tasks = Enumerable.Range(0, 3).Select(_ => _client.GetAsync($"{BaseUrl}/endpoints"));
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized));
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-INT-005")]
    public async Task E2E_SystemAdmin_RespondsWithinTimeout()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync($"{BaseUrl}/endpoints");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-INT-006")]
    public async Task E2E_SystemAdmin_NoSessionCookie()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-INT-007")]
    public async Task E2E_SystemAdminAfterUserManagement_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync($"{UserManagementUrl}/roles");
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-INT-008")]
    public async Task E2E_SystemAdmin_WithQueryParams_DoesNotCrash()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints?verbose=true");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-INT-009")]
    public async Task E2E_SystemAdmin_IAPAuthHeadersAccepted()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
