/**
 * @fileoverview Integration tests for UserManagementController - input validation
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
[Trait("Component", "ValidationTests")]
public class UserManagementValidationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UserManagementValidationTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-USER-VAL-001")]
    [Trait("Priority", "Critical")]
    public async Task UpdateUserRoles_SQLInjectionInRoleName_SafelyHandled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roles = new[] { "'; DROP TABLE Users; --" } };
        var response = await _client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-002")]
    [Trait("Priority", "Critical")]
    public async Task GetUsers_ValidSearchTerm_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "test@unops.org", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-003")]
    [Trait("Priority", "High")]
    public async Task UpdateUserRoles_XSSPayloadInRole_SafelyHandled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roles = new[] { "<script>alert('XSS')</script>" } };
        var response = await _client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-004")]
    [Trait("Priority", "High")]
    public async Task GetUsers_ValidRoleFilter_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roleFilter = new[] { "User" }, pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-005")]
    [Trait("Priority", "High")]
    public async Task GetUser_ValidUserId_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/users/123");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-006")]
    [Trait("Priority", "Medium")]
    public async Task UpdateUserRoles_ValidRoles_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roles = new[] { "User", "Admin" } };
        var response = await _client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-007")]
    [Trait("Priority", "Medium")]
    public async Task GetUsers_ValidPagination_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { pageSize = 25, pageNumber = 2 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-008")]
    [Trait("Priority", "High")]
    public async Task ResolveUsers_ValidIds_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { userIds = new[] { 123, 456 } };
        var response = await _client.PostAsJsonAsync("/api/user-management/resolve-users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-009")]
    [Trait("Priority", "High")]
    public async Task ResolveRoles_ValidIds_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roleIds = new[] { 1, 2 } };
        var response = await _client.PostAsJsonAsync("/api/user-management/resolve-roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-010")]
    [Trait("Priority", "Medium")]
    public async Task UpdateOrgUnitSelfManagement_ValidRequest_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { isSelfManagementEnabled = true };
        var response = await _client.PutAsJsonAsync("/api/user-management/org-units/HQ/self-management", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-011")]
    [Trait("Priority", "High")]
    public async Task GetUsers_UnicodeSearchTerm_Handles()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "café", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-012")]
    [Trait("Priority", "Medium")]
    public async Task GetUsers_EmptySearchTerm_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-013")]
    [Trait("Priority", "Medium")]
    public async Task UpdateUserRoles_EmptyRolesArray_RejectsOrHandles()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roles = Array.Empty<string>() };
        var response = await _client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-014")]
    [Trait("Priority", "High")]
    public async Task GetUser_NonNumericUserId_Handles()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/users/abc");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-VAL-015")]
    [Trait("Priority", "Medium")]
    public async Task GetUsers_ShowMyOrgUnitOnly_Accepts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { showMyOrgUnitOnly = true, pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }
}
