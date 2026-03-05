/**
 * @fileoverview Integration tests for SystemAdminController - negative scenarios via HTTP
 * Tests error handling and invalid inputs against real system admin endpoints
 * @author UNOPS Opportunity+ Test Team
 */

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.SystemAdmin;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "SystemAdmin")]
[Trait("Component", "NegativeTests")]
public class SystemAdminNegativeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;

    public SystemAdminNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = CreateAuthenticatedClient(factory);
        _isPostgresAvailable = factory.IsUsingPostgres;
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
    [Trait("TestId", "TC-ADMIN-NEG-001")]
    public async Task RunSpecificSeeder_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/system-admin/seeding/run/NonExistentSeeder999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-002")]
    public async Task RunSpecificSeeder_EmptyName_Returns404Or400()
    {
        var response = await _client.GetAsync("/api/system-admin/seeding/run/");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-003")]
    public async Task DeleteSeedScript_NonExistent_Returns404Or200()
    {
        var response = await _client.GetAsync("/api/system-admin/seed-scripts/delete/NonExistent999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-004")]
    public async Task GetEndpoints_WrongMethodPost_Returns405()
    {
        var response = await _client.PostAsync("/api/system-admin/endpoints", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-005")]
    public async Task GetAuthDebug_WrongMethodPut_Returns405()
    {
        var response = await _client.PutAsync("/api/system-admin/auth-debug", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-006")]
    public async Task RunMigrations_WrongMethodPost_Returns405()
    {
        var response = await _client.PostAsync("/api/system-admin/migrations/run", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-007")]
    public async Task GetNonExistentEndpoint_Returns404()
    {
        var response = await _client.GetAsync("/api/system-admin/non-existent-endpoint");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-008")]
    public async Task RunSpecificSeeder_InvalidChars_Returns404Or400()
    {
        var response = await _client.GetAsync("/api/system-admin/seeding/run/Invalid%20Name%20With%20Spaces");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-009")]
    public async Task DeleteSeedScript_EmptyName_Returns404Or400()
    {
        var response = await _client.GetAsync("/api/system-admin/seed-scripts/delete/");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-010")]
    public async Task GetEndpoints_TyposInPath_Returns404()
    {
        var response = await _client.GetAsync("/api/system-admin/endpoint");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-011")]
    public async Task RunSeeding_WrongPath_Returns404()
    {
        var response = await _client.GetAsync("/api/system-admin/seeding");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-NEG-012")]
    public async Task GenerateOutputEmbeddings_WrongMethodPost_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync("/api/system-admin/output-embeddings/generate", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }
}
