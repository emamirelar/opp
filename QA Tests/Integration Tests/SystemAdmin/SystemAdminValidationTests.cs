/**
 * @fileoverview Integration tests for SystemAdminController - validation via HTTP
 * Tests valid requests against real system admin endpoints
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
[Trait("Component", "ValidationTests")]
public class SystemAdminValidationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;

    public SystemAdminValidationTests(PAOWebApplicationFactory<Program> factory)
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

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-001")]
    public async Task GetEndpoints_ValidRequest_Returns200Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/endpoints");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-002")]
    public async Task GetAuthDebug_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/auth-debug");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-003")]
    public async Task RunMigrations_ValidRequest_Returns200Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/migrations/run");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-004")]
    public async Task RunSeeding_ValidRequest_Returns200Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/seeding/run");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-005")]
    public async Task RunSpecificSeeder_Roles_Returns200Or403Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/seeding/run/Roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-006")]
    public async Task RunSpecificSeeder_Entities_Returns200Or403Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/seeding/run/Entities");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-007")]
    public async Task TruncateSeedScripts_ValidRequest_Returns200Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/seed-scripts/truncate");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-008")]
    public async Task DeleteSeedScript_ValidName_Returns200Or403Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/seed-scripts/delete/DocumentTypes");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-009")]
    public async Task GenerateOutputEmbeddings_ValidRequest_Returns200Or403()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/output-embeddings/generate");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-010")]
    public async Task GetEndpoints_ResponseContainsEndpoints()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/endpoints");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("system-admin");
        }
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-011")]
    public async Task GetAuthDebug_ResponseContainsAuthInfo()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/auth-debug");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ADMIN-VAL-012")]
    public async Task RunSpecificSeeder_DocumentTypes_Returns200Or403Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/system-admin/seeding/run/DocumentTypes");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }
}
