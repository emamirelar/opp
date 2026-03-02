/**
 * @fileoverview Security integration tests for RoleController
 * Tests unauthenticated access returns 401/403 for all endpoints
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Roles;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Roles")]
[Trait("Component", "SecurityTests")]
public class RoleSecurityTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/Role";

    public RoleSecurityTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-ROLE-SEC-001")]
    public async Task GetAllRoles_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/all");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-SEC-002")]
    public async Task GetUserRoles_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/user");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-SEC-003")]
    public async Task UpdateUserRoles_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var content = JsonContent.Create(Array.Empty<string>());
        var response = await client.PutAsync($"{BaseUrl}/update", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-SEC-004")]
    public async Task AssignDoaRoles_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var content = JsonContent.Create(new[] { new { entityId = 1, userId = 1, roleName = "DoA2", entityType = "OrganizationHierarchy" } });
        var response = await client.PostAsync($"{BaseUrl}/assign-doa-roles", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-SEC-005")]
    public async Task GetDoaRoles_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}/doa-roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-SEC-006")]
    public async Task DeleteDoaRole_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.DeleteAsync($"{BaseUrl}/doa-roles/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-SEC-007")]
    public async Task AllEndpoints_WithoutAuthHeaders_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var endpoints = new[] { $"{BaseUrl}/all", $"{BaseUrl}/user", $"{BaseUrl}/doa-roles" };
        foreach (var url in endpoints)
        {
            var response = await client.GetAsync(url);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-SEC-008")]
    public async Task MutatingEndpoints_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var putContent = JsonContent.Create(Array.Empty<string>());
        var putResponse = await client.PutAsync($"{BaseUrl}/update", putContent);
        putResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        var postContent = JsonContent.Create(new[] { new { entityId = 1, userId = 1, roleName = "DoA2", entityType = "OrganizationHierarchy" } });
        var postResponse = await client.PostAsync($"{BaseUrl}/assign-doa-roles", postContent);
        postResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        var deleteResponse = await client.DeleteAsync($"{BaseUrl}/doa-roles/1");
        deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
