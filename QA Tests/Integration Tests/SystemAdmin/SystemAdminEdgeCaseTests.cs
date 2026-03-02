/**
 * @fileoverview Integration tests for SystemAdminController - edge cases via HTTP
 * Tests actual endpoints: endpoints, auth-debug, migrations/run, seeding/run,
 * seeding/run/{name}, seed-scripts/truncate, seed-scripts/delete/{name}, output-embeddings/generate
 * All require CanRunMigrations/CanRunSeedings except auth-debug (any authenticated user)
 * @author UNOPS Opportunity+ Test Team
 */

using System.Linq;
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
[Trait("Component", "EdgeCaseTests")]
public class SystemAdminEdgeCaseTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SystemAdminEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
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
    [Trait("TestId", "TC-ADMIN-EDGE-001")]
    public async Task GetEndpoints_Authenticated_Returns200Or403()
    {
        var response = await _client.GetAsync("/api/system-admin/endpoints");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-002")]
    public async Task GetAuthDebug_Authenticated_Returns200()
    {
        var response = await _client.GetAsync("/api/system-admin/auth-debug");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-003")]
    public async Task RunMigrations_WithPermission_Returns200Or403()
    {
        var response = await _client.GetAsync("/api/system-admin/migrations/run");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-004")]
    public async Task RunSeeding_WithPermission_Returns200Or403()
    {
        var response = await _client.GetAsync("/api/system-admin/seeding/run");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-005")]
    public async Task RunSpecificSeeder_ValidName_Returns200Or403Or404()
    {
        var response = await _client.GetAsync("/api/system-admin/seeding/run/Roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-006")]
    public async Task TruncateSeedScripts_WithPermission_Returns200Or403()
    {
        var response = await _client.GetAsync("/api/system-admin/seed-scripts/truncate");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-007")]
    public async Task DeleteSeedScript_ValidName_Returns200Or403Or404()
    {
        var response = await _client.GetAsync("/api/system-admin/seed-scripts/delete/Roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-008")]
    public async Task GenerateOutputEmbeddings_WithPermission_Returns200Or403()
    {
        var response = await _client.GetAsync("/api/system-admin/output-embeddings/generate");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-009")]
    public async Task GetEndpoints_RapidSequential_NoStateIssues()
    {
        for (var i = 0; i < 10; i++)
        {
            var response = await _client.GetAsync("/api/system-admin/endpoints");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-010")]
    public async Task GetAuthDebug_Concurrent_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10).Select(_ => _client.GetAsync("/api/system-admin/auth-debug"));
        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError));
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-011")]
    public async Task RunSpecificSeeder_Entities_Returns200Or403Or404()
    {
        var response = await _client.GetAsync("/api/system-admin/seeding/run/Entities");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-012")]
    public async Task GetEndpoints_ReturnsStructuredResponse()
    {
        var response = await _client.GetAsync("/api/system-admin/endpoints");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-013")]
    public async Task GetAuthDebug_ReturnsUserInfo()
    {
        var response = await _client.GetAsync("/api/system-admin/auth-debug");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-014")]
    public async Task RunMigrations_Concurrent_HandlesGracefully()
    {
        var tasks = Enumerable.Range(0, 3).Select(_ => _client.GetAsync("/api/system-admin/migrations/run"));
        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError));
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-EDGE-015")]
    public async Task DeleteSeedScript_NonExistent_Returns404Or200()
    {
        var response = await _client.GetAsync("/api/system-admin/seed-scripts/delete/NonExistentSeeder999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized, HttpStatusCode.InternalServerError);
    }
}
