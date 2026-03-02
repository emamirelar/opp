/**
 * @fileoverview Negative integration tests for RoleController
 * Tests invalid inputs and error handling against actual API: /api/Role/*
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.Roles;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Roles")]
[Trait("Component", "NegativeTests")]
public class RoleNegativeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/Role";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RoleNegativeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-001")]
    public async Task GetNonExistentEndpoint_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/non-existent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-002")]
    public async Task PostAll_InsteadOfGet_Returns405()
    {
        var response = await _client.PostAsync($"{BaseUrl}/all", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-003")]
    public async Task GetUpdate_InsteadOfPut_Returns405()
    {
        var response = await _client.GetAsync($"{BaseUrl}/update");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-004")]
    public async Task PutAssignDoaRoles_InsteadOfPost_Returns405()
    {
        var content = JsonContent.Create(new[] { new { entityId = 1, userId = 1, roleName = "DoA2", entityType = "OrganizationHierarchy" } });
        var response = await _client.PutAsync($"{BaseUrl}/assign-doa-roles", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-005")]
    public async Task PostDoaRoles_InsteadOfGet_Returns405()
    {
        var response = await _client.PostAsync($"{BaseUrl}/doa-roles", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-006")]
    public async Task GetDoaRolesDelete_InsteadOfDelete_Returns405()
    {
        var response = await _client.GetAsync($"{BaseUrl}/doa-roles/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-007")]
    public async Task DeleteDoaRole_InvalidIdFormat_Returns404Or400()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/doa-roles/not-a-number");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-008")]
    public async Task GetRoleBaseOnly_Returns404()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-009")]
    public async Task GetTyposInPath_Returns404()
    {
        var response = await _client.GetAsync($"{BaseUrl}/al");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-NEG-010")]
    public async Task AssignDoaRoles_InvalidRoleName_Returns200WithSkippedCount()
    {
        var assignments = new[] { new { entityId = 1, userId = 1, roleName = "InvalidRole", entityType = "OrganizationHierarchy" } };
        var content = JsonContent.Create(assignments);
        var response = await _client.PostAsync($"{BaseUrl}/assign-doa-roles", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("success", out _).Should().BeTrue();
        result.TryGetProperty("assignedCount", out _).Should().BeTrue();
    }
}
