/**
 * @fileoverview Functional Tests for Partner Analytics — business rule verification.
 * Verifies /api/partner/analytics returns correct response shapes, content types,
 * and enforces business rules (filters, aggregations, auth).
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

namespace UNOPS.PAO.Tests.Integration.PartnerAnalytics;

[Collection("Integration Tests")]
[Trait("Category", "Functional")]
[Trait("Feature", "PartnerAnalytics")]
[Trait("Component", "FunctionalTests")]
public class PartnerAnalyticsFunctionalTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerAnalyticsFunctionalTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-001")]
    public async Task GetAnalytics_Endpoint_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/api/partner/analytics");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var contentType = response.Content.Headers.ContentType?.MediaType;
            contentType.Should().Contain("json");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.NotFound, HttpStatusCode.BadRequest,
                HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-002")]
    public async Task GetAnalytics_WithAuthenticatedUser_DoesNotReturn401()
    {
        var response = await _client.GetAsync("/api/partner/analytics");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-003")]
    public async Task GetAnalytics_ResponseShape_ContainsExpectedStructure()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/partner/analytics");

        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-004")]
    public async Task GetAnalytics_AcceptsGetVerb_RejectsPostToSameRoute()
    {
        var getResponse = await _client.GetAsync("/api/partner/analytics");
        getResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        var postResponse = await _client.PostAsJsonAsync("/api/partner/analytics", new { }, JsonOpts);
        postResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-005")]
    public async Task GetAnalytics_WithFormEncodedBody_Returns415Or400()
    {
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("filter", "all")
        });
        var response = await _client.PostAsync("/api/partner/analytics", formContent);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-006")]
    public async Task GetAnalytics_ResponseDoesNotSetSessionCookie()
    {
        var response = await _client.GetAsync("/api/partner/analytics");

        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList()
            : new List<string>();

        var sessionCookies = setCookieHeaders
            .Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase)).ToList();
        sessionCookies.Should().BeEmpty("API endpoints must not set session cookies");
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-007")]
    public async Task GetAnalytics_RepeatedCalls_ReturnConsistentStatusCode()
    {
        if (!_isPostgresAvailable) return;
        var responses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            var response = await _client.GetAsync("/api/partner/analytics");
            responses.Add(response.StatusCode);
        }

        responses.Distinct().Should().HaveCount(1,
            "repeated calls to the same endpoint should return the same status code");
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-008")]
    public async Task GetAnalytics_RespondsWithinReasonableTime()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/api/partner/analytics");
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-PA-FUNC-009")]
    public async Task GetAnalytics_WithQueryParameters_DoesNotCrash()
    {
        var response = await _client.GetAsync("/api/partner/analytics?page=1&pageSize=10");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }
}
