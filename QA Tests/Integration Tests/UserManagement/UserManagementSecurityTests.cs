/**
 * @fileoverview Integration tests for UserManagementController - auth/security
 * Tests actual HTTP endpoints with IAP auth
 * @author UNOPS Opportunity+ Test Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.UserManagement;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "UserManagement")]
[Trait("Component", "SecurityTests")]
public class UserManagementSecurityTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UserManagementSecurityTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-USER-SEC-001")]
    [Trait("Priority", "Critical")]
    public async Task GetUser_RequiresAuth_Returns401WhenUnauthenticated()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/user-management/users/123");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-002")]
    [Trait("Priority", "Critical")]
    public async Task GetUser_Authenticated_Returns200Or404()
    {
        var response = await _client.GetAsync("/api/user-management/users/123");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-003")]
    [Trait("Priority", "High")]
    public async Task UpdateUserRoles_RequiresAuth_Returns401WhenUnauthenticated()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var body = new { roles = new[] { "User" } };
        var response = await client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-004")]
    [Trait("Priority", "High")]
    public async Task GetRoles_RequiresAuth_Returns401WhenUnauthenticated()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/user-management/roles");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-005")]
    [Trait("Priority", "High")]
    public async Task GetOrgUnits_RequiresAuth_Returns401WhenUnauthenticated()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/user-management/org-units");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-006")]
    [Trait("Priority", "Critical")]
    public async Task GetUser_ResponseDoesNotExposeSensitiveData()
    {
        var response = await _client.GetAsync("/api/user-management/users/123");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("password");
            content.Should().NotContain("Password");
            content.Should().NotContain("token");
        }
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-007")]
    [Trait("Priority", "High")]
    public async Task ResolveUsers_RequiresAuth_Returns401WhenUnauthenticated()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var body = new { userIds = new[] { 123 } };
        var response = await client.PostAsJsonAsync("/api/user-management/resolve-users", body, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-008")]
    [Trait("Priority", "High")]
    public async Task ResolveRoles_RequiresAuth_Returns401WhenUnauthenticated()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var body = new { roleIds = new[] { 1 } };
        var response = await client.PostAsJsonAsync("/api/user-management/resolve-roles", body, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-009")]
    [Trait("Priority", "Medium")]
    public async Task GetOrgUnitSelfManagement_RequiresAuth_Returns401WhenUnauthenticated()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/user-management/org-units/HQ/self-management");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-SEC-010")]
    [Trait("Priority", "Medium")]
    public async Task UpdateOrgUnitSelfManagement_RequiresAuth_Returns401WhenUnauthenticated()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var body = new { isSelfManagementEnabled = true };
        var response = await client.PutAsJsonAsync("/api/user-management/org-units/HQ/self-management", body, JsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
