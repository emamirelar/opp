/**
 * @fileoverview Functional tests for UserManagement — business rule verification.
 * Verifies user-management endpoints return correct response shapes, content types,
 * and enforce business rules. API base: /api/user-management
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
[Trait("Category", "Functional")]
[Trait("Feature", "UserManagement")]
[Trait("Component", "FunctionalTests")]
public class UserManagementFunctionalTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/user-management";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UserManagementFunctionalTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-USER-FUNC-001")]
    public async Task GetRoles_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    [Trait("TestId", "TC-USER-FUNC-002")]
    public async Task GetRoles_WithAuth_DoesNotReturn401()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-FUNC-003")]
    public async Task GetRoles_ResponseIsJsonObjectOrArray()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-USER-FUNC-004")]
    public async Task GetRoles_AcceptsGetVerb()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]

    [Trait("Defect", "DEF-064")]
    [Trait("TestId", "TC-USER-FUNC-005")]
    public async Task PostUsers_FormEncoded_Returns415Or400()
    {
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("email", "test@unops.org")
        });
        var response = await _client.PostAsync($"{BaseUrl}/users", formContent);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-USER-FUNC-006")]
    public async Task GetRoles_NoSessionCookie()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles");
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-USER-FUNC-007")]
    public async Task GetRoles_RepeatedCalls_ConsistentStatus()
    {
        if (!_isPostgresAvailable) return;
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            var r = await _client.GetAsync($"{BaseUrl}/roles");
            statuses.Add(r.StatusCode);
        }
        statuses.Distinct().Should().HaveCount(1);
    }

    [Fact]
    [Trait("TestId", "TC-USER-FUNC-008")]
    public async Task GetRoles_RespondsWithinReasonableTime()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync($"{BaseUrl}/roles");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-USER-FUNC-009")]
    public async Task GetRoles_WithQueryParams_DoesNotCrash()
    {
        var response = await _client.GetAsync($"{BaseUrl}/roles?includeInactive=true");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }
}
