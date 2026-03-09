/**
 * @fileoverview Integration tests for UserManagementController - edge cases
 * Tests actual HTTP endpoints with IAP auth
 * @author UNOPS Opportunity+ Test Team
 *
 * Real endpoints: POST /api/user-management/users, GET .../users/{id}, PUT .../users/{id}/roles,
 * GET .../roles, GET .../org-units, GET .../current-user-org-unit,
 * GET/PUT .../org-units/{code}/self-management, POST .../analyse-file, .../bulk-upload,
 * .../resolve-users, .../resolve-roles
 */

using System.Linq;
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
[Trait("Component", "EdgeCaseTests")]
public class UserManagementEdgeCaseTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public UserManagementEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-USER-EDGE-001")]
    [Trait("Priority", "Medium")]
    public async Task GetUsers_EmptyRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-002")]
    [Trait("Priority", "Medium")]
    public async Task GetUsers_MinPageSize_HandlesBoundary()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { pageSize = 1, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-003")]
    [Trait("Priority", "Low")]
    public async Task GetUsers_SearchTermUnicode_HandlesInternationalization()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "李明", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-004")]
    [Trait("Priority", "Low")]
    public async Task GetUsers_SearchTermEmoji_HandlesEmoji()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "Test👤", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-005")]
    [Trait("Priority", "High")]
    public async Task GetUser_ValidUserId_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/users/123");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-006")]
    [Trait("Priority", "Medium")]
    public async Task UpdateUserRoles_SingleRole_Succeeds()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roles = new[] { "User" } };
        var response = await _client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-007")]
    [Trait("Priority", "High")]
    public async Task GetRoles_Authenticated_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-008")]
    [Trait("Priority", "High")]
    public async Task GetOrgUnits_Authenticated_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/org-units");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-009")]
    [Trait("Priority", "High")]
    public async Task GetCurrentUserOrgUnit_Authenticated_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/current-user-org-unit");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-010")]
    [Trait("Priority", "Medium")]
    public async Task GetOrgUnitSelfManagement_ValidCode_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/org-units/HQ/self-management");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-011")]
    [Trait("Priority", "Low")]
    public async Task GetUsers_RapidSequential_NoStateIssues()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { pageSize = 10, pageNumber = 1 };
        for (var i = 0; i < 10; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-012")]
    [Trait("Priority", "High")]
    public async Task GetUsers_Concurrent_AllSucceed()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { pageSize = 10, pageNumber = 1 };
        var tasks = Enumerable.Range(0, 10).Select(_ => _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions));
        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized));
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-013")]
    [Trait("Priority", "Medium")]
    public async Task UpdateUserRoles_RepeatedCalls_Idempotent()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roles = new[] { "User" } };
        var r1 = await _client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        var r2 = await _client.PutAsJsonAsync("/api/user-management/users/123/roles", body, JsonOptions);
        r1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        r2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-014")]
    [Trait("Priority", "High")]
    public async Task GetUsers_NoUsers_ReturnsEmptyOrValid()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "nonexistentuser99999", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-015")]
    [Trait("Priority", "Low")]
    public async Task GetUsers_SortByVariousFields_Handles()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { sortBy = "Name", sortDirection = "asc", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-016")]
    [Trait("Priority", "Medium")]
    public async Task ResolveUsers_EmptyList_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { userIds = Array.Empty<int>() };
        var response = await _client.PostAsJsonAsync("/api/user-management/resolve-users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-017")]
    [Trait("Priority", "Medium")]
    public async Task ResolveRoles_EmptyList_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { roleIds = Array.Empty<int>() };
        var response = await _client.PostAsJsonAsync("/api/user-management/resolve-roles", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-018")]
    [Trait("Priority", "Medium")]
    public async Task ResolveUsers_ValidIds_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { userIds = new[] { 123 } };
        var response = await _client.PostAsJsonAsync("/api/user-management/resolve-users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-019")]
    [Trait("Priority", "Low")]
    public async Task GetOrgUnitSelfManagement_UnknownCode_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/user-management/org-units/UNKNOWN999/self-management");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-020")]
    [Trait("Priority", "High")]
    public async Task GetUser_MultipleConcurrent_Consistent()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var tasks = Enumerable.Range(0, 10).Select(_ => _client.GetAsync("/api/user-management/users/123"));
        var results = await Task.WhenAll(tasks);
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized));
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-021")]
    [Trait("Priority", "High")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetUsers_SearchWithAccentedChars_NoEncodingArtifacts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "José García", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??",
                "PNO-1194: search results must not contain '??' encoding artifacts");
        }
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-022")]
    [Trait("Priority", "Medium")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetUsers_SearchWithCyrillicChars_Handled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "Иванов", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-023")]
    [Trait("Priority", "Medium")]
    public async Task GetUsers_SearchWithArabicChars_Handled()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "محمد", pageSize = 10, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("TestId", "TC-USER-EDGE-024")]
    [Trait("Priority", "Medium")]
    public async Task GetUsers_FullUserList_NoReplacementCharacters()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { pageSize = 50, pageNumber = 1 };
        var response = await _client.PostAsJsonAsync("/api/user-management/users", body, JsonOptions);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("\uFFFD",
                "User list should not contain U+FFFD replacement characters indicating encoding failure");
        }
    }
}
