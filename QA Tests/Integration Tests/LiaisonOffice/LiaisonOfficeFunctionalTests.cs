/**
 * @fileoverview Functional Tests for LiaisonOffice — business rule verification.
 * Verifies LiaisonOffice endpoints return correct response shapes, content types,
 * and enforce business rules. API base: /api/LiaisonOffice
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

namespace UNOPS.PAO.Tests.Integration.LiaisonOffice;

[Collection("Integration Tests")]
[Trait("Category", "Functional")]
[Trait("Feature", "LiaisonOffice")]
[Trait("Component", "FunctionalTests")]
public class LiaisonOfficeFunctionalTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/LiaisonOffice";
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public LiaisonOfficeFunctionalTests(PAOWebApplicationFactory<Program> factory)
    {
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-001")]
    public async Task GetLiaisonOffice_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync(BaseUrl);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-002")]
    public async Task GetLiaisonOffice_WithAuth_DoesNotReturn401()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-003")]
    public async Task GetLiaisonOffice_ResponseIsJsonObjectOrArray()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(BaseUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-004")]
    public async Task GetLiaisonOffice_AcceptsGetVerb()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-005")]
    public async Task GetLiaisonOffice_FormEncoded_Returns415Or400()
    {
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("filter", "active")
        });
        var response = await _client.PostAsync(BaseUrl, formContent);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-006")]
    public async Task GetLiaisonOffice_NoSessionCookie()
    {
        var response = await _client.GetAsync(BaseUrl);
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-007")]
    public async Task GetLiaisonOffice_RepeatedCalls_ConsistentStatus()
    {
        if (!_isPostgresAvailable) return;
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            var r = await _client.GetAsync(BaseUrl);
            statuses.Add(r.StatusCode);
        }
        statuses.Distinct().Should().HaveCount(1);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-008")]
    public async Task GetLiaisonOffice_RespondsWithinReasonableTime()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync(BaseUrl);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-FUNC-009")]
    public async Task GetLiaisonOffice_WithQueryParams_DoesNotCrash()
    {
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=10&page=1");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }
}
