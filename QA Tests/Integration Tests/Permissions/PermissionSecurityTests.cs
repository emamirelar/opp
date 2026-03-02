/**
 * @fileoverview Security integration tests for PermissionController
 * Tests unauthenticated access returns 401/403 for all endpoints
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Permissions;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Permissions")]
[Trait("Component", "SecurityTests")]
public class PermissionSecurityTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/permissions";

    public PermissionSecurityTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    private HttpClient CreateUnauthenticatedClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        return client;
    }

    [Fact]
    [Trait("TestId", "TC-PERM-SEC-001")]
    public async Task GetSystemPermissionConfig_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-SEC-002")]
    public async Task CheckRoutePermission_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/check/partnerships/contacts");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-SEC-003")]
    public async Task GetEntityPermissions_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/entity-permissions/Contact");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-SEC-004")]
    public async Task GetUserRoles_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/user-roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-SEC-005")]
    public async Task GetUserRolesByUserId_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/user/123");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-SEC-006")]
    public async Task GetAvailableRoles_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/available-roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-SEC-007")]
    public async Task AllEndpoints_WithoutAuthHeaders_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var endpoints = new[]
        {
            BaseUrl,
            $"{BaseUrl}/check/dashboard",
            $"{BaseUrl}/entity-permissions/Partner",
            $"{BaseUrl}/user-roles",
            $"{BaseUrl}/user/123",
            $"{BaseUrl}/available-roles"
        };
        foreach (var url in endpoints)
        {
            var response = await client.GetAsync(url);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    [Trait("TestId", "TC-PERM-SEC-008")]
    public async Task CheckRoutePermission_AdminRoute_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/check/admin/user-management");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
