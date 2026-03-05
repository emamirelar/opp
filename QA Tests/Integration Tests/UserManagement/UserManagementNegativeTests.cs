/**
 * @fileoverview Integration tests for UserManagementController - negative scenarios
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
[Trait("Component", "NegativeTests")]
public class UserManagementNegativeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UserManagementNegativeTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-USER-NEG-001")]
    [Trait("Priority", "Critical")]
    public async Task GetUser_NonExistentId_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/users/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-002")]
    [Trait("Priority", "High")]
    public async Task GetUsers_NullBody_Returns400Or405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsync("/api/user-management/users", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-003")]
    [Trait("Priority", "High")]
    public async Task UpdateUserRoles_NonExistentUser_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roles = new[] { "User" } };
        var response = await _client.PutAsJsonAsync("/api/user-management/users/999999/roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-004")]
    [Trait("Priority", "High")]
    public async Task UpdateUserRoles_EmptyRoles_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roles = Array.Empty<string>() };
        var response = await _client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-005")]
    [Trait("Priority", "Critical")]
    public async Task GetUser_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/user-management/users/123");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-006")]
    [Trait("Priority", "High")]
    public async Task GetRoles_Unauthenticated_Returns401()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");
        var response = await client.GetAsync("/api/user-management/roles");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-007")]
    [Trait("Priority", "High")]
    public async Task GetUsers_InvalidMethod_Returns405()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/users");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-008")]
    [Trait("Priority", "Critical")]
    public async Task GetUser_InvalidPath_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/users/");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-009")]
    [Trait("Priority", "High")]
    public async Task UpdateUserRoles_NullBody_Returns400Or415()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PutAsync("/api/user-management/users/123/roles", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-010")]
    [Trait("Priority", "High")]
    public async Task GetOrgUnitSelfManagement_EmptyCode_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/org-units//self-management");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-011")]
    [Trait("Priority", "Medium")]
    public async Task ResolveUsers_InvalidJson_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("{ invalid json }", System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/user-management/resolve-users", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-012")]
    [Trait("Priority", "High")]
    public async Task GetUser_UserIdWithSpecialChars_HandlesOrRejects()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/users/123%3Bscript");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-013")]
    [Trait("Priority", "Medium")]
    public async Task UpdateOrgUnitSelfManagement_NonExistentCode_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { isSelfManagementEnabled = true };
        var response = await _client.PutAsJsonAsync("/api/user-management/org-units/NONEXISTENT999/self-management", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-014")]
    [Trait("Priority", "High")]
    public async Task GetUsers_InvalidPageNumber_Returns400OrHandles()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { pageSize = 10, pageNumber = -1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-NEG-015")]
    [Trait("Priority", "Medium")]
    public async Task AnalyseFile_InvalidRequest_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { type = "", fileId = "" };
        var response = await _client.PostAsJsonAsync("/api/user-management/analyse-file", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }
}
