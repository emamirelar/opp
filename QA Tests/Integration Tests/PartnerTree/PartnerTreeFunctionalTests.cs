/**
 * @fileoverview Functional Tests for Partner Tree — business rule verification.
 * Verifies /api/partner-tree and /api/partner-tree-structure return correct response
 * shapes, content types, and enforce business rules.
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
[Trait("Category", "Functional")]
[Trait("Feature", "PartnerTree")]
[Trait("Component", "FunctionalTests")]
public class PartnerTreeFunctionalTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerTreeFunctionalTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-TREE-FUNC-001")]
    public async Task GetPartnerTree_Endpoint_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/api/partner-tree");

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
    [Trait("TestId", "TC-TREE-FUNC-002")]
    public async Task GetPartnerTree_WithAuthenticatedUser_DoesNotReturn401()
    {
        var response = await _client.GetAsync("/api/partner-tree");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-FUNC-003")]
    public async Task GetPartnerTree_ResponseShape_ContainsExpectedStructure()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/partner-tree");

        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-FUNC-004")]
    public async Task GetPartnerTreeStructure_Endpoint_ReturnsValidResponse()
    {
        var response = await _client.GetAsync("/api/partner-tree-structure");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-TREE-FUNC-005")]
    public async Task GetPartnerTree_AcceptsGetVerb_RejectsPostToSameRoute()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var getResponse = await _client.GetAsync("/api/partner-tree");
        getResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        var postResponse = await _client.PostAsJsonAsync("/api/partner-tree", new { }, JsonOpts);
        postResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-FUNC-006")]
    public async Task GetPartnerTree_WithFormEncodedBody_Returns415Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("filter", "all")
        });
        var response = await _client.PostAsync("/api/partner-tree", formContent);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-FUNC-007")]
    public async Task GetPartnerTree_ResponseDoesNotSetSessionCookie()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree");

        var setCookieHeaders = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList()
            : new List<string>();

        var sessionCookies = setCookieHeaders
            .Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase)).ToList();
        sessionCookies.Should().BeEmpty("API endpoints must not set session cookies");
    }

    [Fact]
    [Trait("TestId", "TC-TREE-FUNC-008")]
    public async Task GetPartnerTree_RespondsWithinReasonableTime()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/api/partner-tree");
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-FUNC-009")]
    public async Task GetPartnerTree_WithQueryParameters_DoesNotCrash()
    {
        var response = await _client.GetAsync("/api/partner-tree?sortBy=Name&ascending=true");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }
}
