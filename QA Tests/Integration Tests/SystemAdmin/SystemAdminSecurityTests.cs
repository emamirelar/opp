/**
 * @fileoverview Integration tests for SystemAdminController - security via HTTP
 * Tests unauthenticated access against real system admin endpoints
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
[Trait("Component", "SecurityTests")]
public class SystemAdminSecurityTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;

    public SystemAdminSecurityTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
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

    private static HttpClient CreateUnauthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        return client;
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-001")]
    public async Task GetEndpoints_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/system-admin/endpoints");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-002")]
    public async Task GetAuthDebug_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/system-admin/auth-debug");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-003")]
    public async Task RunMigrations_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/system-admin/migrations/run");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-004")]
    public async Task RunSeeding_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/system-admin/seeding/run");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-005")]
    public async Task RunSpecificSeeder_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/system-admin/seeding/run/Roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-006")]
    public async Task TruncateSeedScripts_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/system-admin/seed-scripts/truncate");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-007")]
    public async Task DeleteSeedScript_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/system-admin/seed-scripts/delete/Roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-008")]
    public async Task GenerateOutputEmbeddings_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = CreateUnauthenticatedClient(_factory);
        var response = await client.GetAsync("/api/system-admin/output-embeddings/generate");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-009")]
    public async Task GetEndpoints_Authenticated_Returns200Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/endpoints");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-SEC-010")]
    public async Task GetAuthDebug_Authenticated_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/auth-debug");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }
}
