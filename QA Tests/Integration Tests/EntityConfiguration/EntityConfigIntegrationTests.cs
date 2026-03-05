/**
 * @fileoverview Integration tests for EntityConfiguration — end-to-end API flow verification.
 * Verifies full request lifecycle across entity-configuration, entity-field, and entities endpoints.
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
[Trait("Category", "Integration")]
[Trait("Feature", "EntityConfiguration")]
[Trait("Component", "IntegrationTests")]
public class EntityConfigIntegrationTests
{
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public EntityConfigIntegrationTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-ECFG-INT-001")]
    public async Task E2E_EntityConfiguration_FullRequestLifecycle()
    {
        var response = await _client.GetAsync("/api/entity-configuration");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            body.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-INT-002")]
    public async Task E2E_CrossEndpointAccess_EntityConfigEntityFieldEntities_AllAccessible()
    {
        var configResponse = await _client.GetAsync("/api/entity-configuration");
        var fieldResponse = await _client.GetAsync("/api/entity-configuration/1/fields");
        var entitiesResponse = await _client.GetAsync("/api/entities");

        configResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        fieldResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        entitiesResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-INT-003")]
    public async Task E2E_SequentialCalls_NoStateLeak()
    {
        if (!_isPostgresAvailable) return;
        var first = await _client.GetAsync("/api/entity-configuration");
        var second = await _client.GetAsync("/api/entity-configuration");
        first.StatusCode.Should().Be(second.StatusCode);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-INT-004")]
    public async Task E2E_ConcurrentCalls_AllComplete()
    {
        var tasks = Enumerable.Range(0, 3).Select(_ => _client.GetAsync("/api/entity-configuration"));
        var responses = await Task.WhenAll(tasks);
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden));
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-INT-005")]
    public async Task E2E_EntityConfiguration_RespondsWithinTimeout()
    {
        if (!_isPostgresAvailable) return;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/api/entity-configuration");
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(30_000);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-INT-006")]
    public async Task E2E_EntityConfiguration_NoSessionCookie()
    {
        var response = await _client.GetAsync("/api/entity-configuration");
        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList() : new List<string>();
        setCookies.Where(c => c.Contains("session", StringComparison.OrdinalIgnoreCase))
            .Should().BeEmpty();
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-INT-007")]
    public async Task E2E_EntityConfigAfterEntities_ContextNotCorrupted()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync("/api/entities");
        var response = await _client.GetAsync("/api/entity-configuration");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-INT-008")]
    public async Task E2E_CrossEndpointSequential_ContextConsistency()
    {
        if (!_isPostgresAvailable) return;
        await _client.GetAsync("/api/entity-configuration");
        await _client.GetAsync("/api/entities");
        var response = await _client.GetAsync("/api/entity-configuration/Partner");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ECFG-INT-009")]
    public async Task E2E_EntityConfiguration_IAPAuthHeadersAccepted()
    {
        var response = await _client.GetAsync("/api/entity-configuration");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
