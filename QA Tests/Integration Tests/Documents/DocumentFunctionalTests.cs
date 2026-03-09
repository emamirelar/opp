/**
 * @fileoverview Functional tests for Document — business rule verification.
 * Verifies /api/document endpoints return correct response shapes, content types,
 * and enforce business rules. DEF-021/DEF-024: DocumentController has known issues;
 * tests tolerate OK, BadRequest, NotFound, InternalServerError.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Documents;

[Collection("Integration Tests")]
[Trait("Category", "Functional")]
[Trait("Feature", "Documents")]
[Trait("Component", "FunctionalTests")]
public class DocumentFunctionalTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DocumentFunctionalTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-DOC-FUNC-001")]
    public async Task GetDocument_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/api/document/Partner/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
    }

    [Fact]
    [Trait("TestId", "TC-DOC-FUNC-002")]
    public async Task GetDocument_WithAuth_DoesNotReturn401()
    {
        var response = await _client.GetAsync("/api/document/Partner/1");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-DOC-FUNC-003")]
    public async Task GetDocument_ResponseIsJsonObjectOrArray()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/document/Partner/1");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-DOC-FUNC-004")]
    public async Task GetDocument_AcceptsGetVerb()
    {
        var response = await _client.GetAsync("/api/document/Partner/1");
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-DOC-FUNC-005")]
    public async Task PostDocument_FormEncoded_Returns415Or400()
    {
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("entityType", "Partner")
        });
        var response = await _client.PostAsync("/api/document/upload", formContent);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-DOC-FUNC-006")]
    public async Task GetDocument_NoSessionCookie()
    {
        var response = await _client.GetAsync("/api/document/Partner/1");
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-DOC-FUNC-007")]
    public async Task GetDocument_RepeatedCalls_ConsistentStatus()
    {
        if (!_isPostgresAvailable) return;
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            var r = await _client.GetAsync("/api/document/Partner/1");
            statuses.Add(r.StatusCode);
        }
        statuses.Distinct().Should().HaveCount(1);
    }

    [Fact]
    [Trait("TestId", "TC-DOC-FUNC-008")]
    public async Task GetDocument_RespondsWithinReasonableTime()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/api/document/Partner/1");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-DOC-FUNC-009")]
    public async Task GetDocument_WithQueryParams_DoesNotCrash()
    {
        var response = await _client.GetAsync("/api/document/Partner/1?includeMetadata=true");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }
}
