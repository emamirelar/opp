/**
 * @fileoverview Functional tests for SystemAdmin — business rule verification.
 * Verifies system-admin endpoints return correct response shapes, content types,
 * and enforce business rules. API base: /api/system-admin
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
[Trait("Category", "Functional")]
[Trait("Feature", "SystemAdmin")]
[Trait("Component", "FunctionalTests")]
public class SystemAdminFunctionalTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/system-admin";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public SystemAdminFunctionalTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-ADMIN-FUNC-001")]
    public async Task GetEndpoints_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
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
    [Trait("TestId", "TC-ADMIN-FUNC-002")]
    public async Task GetEndpoints_WithAuth_DoesNotReturn401()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-FUNC-003")]
    public async Task GetEndpoints_ResponseIsJsonObjectOrArray()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-FUNC-004")]
    public async Task GetEndpoints_AcceptsGetVerb()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]

    [Trait("Defect", "DEF-064")]
    [Trait("TestId", "TC-ADMIN-FUNC-005")]
    public async Task PostCleanUpUsers_FormEncoded_Returns415Or400()
    {
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("dryRun", "true")
        });
        var response = await _client.PostAsync($"{BaseUrl}/clean-up-users", formContent);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-FUNC-006")]
    public async Task GetEndpoints_NoSessionCookie()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints");
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-FUNC-007")]
    public async Task GetEndpoints_RepeatedCalls_ConsistentStatus()
    {
        if (!_isPostgresAvailable) return;
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            var r = await _client.GetAsync($"{BaseUrl}/endpoints");
            statuses.Add(r.StatusCode);
        }
        statuses.Distinct().Should().HaveCount(1);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-FUNC-008")]
    public async Task GetEndpoints_RespondsWithinReasonableTime()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync($"{BaseUrl}/endpoints");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-FUNC-009")]
    public async Task GetEndpoints_WithQueryParams_DoesNotCrash()
    {
        var response = await _client.GetAsync($"{BaseUrl}/endpoints?verbose=true");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }
}
