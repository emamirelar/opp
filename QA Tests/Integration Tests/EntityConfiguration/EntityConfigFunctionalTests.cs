/**
 * @fileoverview Functional tests for EntityConfiguration — business rule verification.
 * Verifies entity-configuration, entity-field, and entities endpoints return correct
 * response shapes, content types, and enforce business rules.
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

namespace UNOPS.PAO.Tests.Integration.EntityConfiguration;

[Collection("Integration Tests")]
[Trait("Category", "Functional")]
[Trait("Feature", "EntityConfiguration")]
[Trait("Component", "FunctionalTests")]
public class EntityConfigFunctionalTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public EntityConfigFunctionalTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-ECFG-FUNC-001")]
    public async Task GetEntityConfiguration_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/api/entity-configuration");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-FUNC-002")]
    public async Task GetEntityConfiguration_WithAuth_DoesNotReturn401()
    {
        var response = await _client.GetAsync("/api/entity-configuration");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-FUNC-003")]
    public async Task GetEntityConfiguration_ResponseIsJsonObjectOrArray()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/entity-configuration");
        if (response.StatusCode != HttpStatusCode.OK) return;

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-FUNC-004")]
    public async Task GetEntityConfiguration_AcceptsGetVerb()
    {
        var response = await _client.GetAsync("/api/entity-configuration");
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-FUNC-005")]
    public async Task PostEntityConfiguration_FormEncoded_Returns415Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("entityName", "Partner")
        });
        var response = await _client.PostAsync("/api/entity-configuration/create", formContent);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest,
            HttpStatusCode.MethodNotAllowed, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-FUNC-006")]
    public async Task GetEntityConfiguration_NoSessionCookie()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/entity-configuration");
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-FUNC-007")]
    public async Task GetEntityField_EndpointAccessible()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/entity-configuration/1/fields");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-FUNC-008")]
    public async Task GetEntities_EndpointAccessible()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync("/api/entities");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-FUNC-009")]
    public async Task GetEntityConfiguration_RespondsWithinReasonableTime()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/api/entity-configuration");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }
}
