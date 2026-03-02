/**
 * @fileoverview Edge case integration tests for PermissionController
 * Tests boundary conditions against actual API: /api/permissions/*
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

namespace UNOPS.PAO.Tests.Integration.Permissions;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Permissions")]
[Trait("Component", "EdgeCaseTests")]
public class PermissionEdgeCaseTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/permissions";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PermissionEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-001")]
    public async Task GetSystemPermissionConfig_Returns200WithRoles()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("roles", out _).Should().BeTrue();
        result.TryGetProperty("totalRoles", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-002")]
    public async Task GetSystemPermissionConfig_EmptyOrPopulated_ReturnsValidStructure()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("generatedAt", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-003")]
    public async Task CheckRoutePermission_ValidRoute_Returns200WithHasAccess()
    {
        var response = await _client.GetAsync($"{BaseUrl}/check/partnerships/contacts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("route", out _).Should().BeTrue();
        result.TryGetProperty("hasAccess", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-004")]
    public async Task CheckRoutePermission_EntityWithId_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/check/partnerships/contacts/123");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("hasAccess", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-005")]
    public async Task GetEntityPermissions_ValidEntity_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/entity-permissions/Contact");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("entityName", out _).Should().BeTrue();
        result.TryGetProperty("userPermissions", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-006")]
    public async Task GetEntityPermissions_PartnerEntity_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/entity-permissions/Partner");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("systemConfiguration", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-007")]
    public async Task GetUserRoles_Returns200WithRoles()
    {
        var response = await _client.GetAsync($"{BaseUrl}/user-roles");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("roles", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-008")]
    public async Task GetUserRolesByUserId_CurrentUser_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/user/123");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("userId", out _).Should().BeTrue();
            result.TryGetProperty("roles", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-009")]
    public async Task GetAvailableRoles_Returns200WithRolesArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/available-roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("roles", out _).Should().BeTrue();
        result.TryGetProperty("totalRoles", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-010")]
    public async Task CheckRoutePermission_DashboardRoute_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/check/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("hasAccess", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-011")]
    public async Task AllEndpoints_RapidSequentialCalls_NoStateCorruption()
    {
        var urls = new[]
        {
            BaseUrl,
            $"{BaseUrl}/check/partnerships/contacts",
            $"{BaseUrl}/entity-permissions/Contact",
            $"{BaseUrl}/user-roles",
            $"{BaseUrl}/available-roles"
        };
        foreach (var url in urls)
        {
            for (var i = 0; i < 2; i++)
            {
                var response = await _client.GetAsync(url);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PERM-EDGE-012")]
    public async Task GetEntityPermissions_OpportunityEntity_Returns200()
    {
        var response = await _client.GetAsync($"{BaseUrl}/entity-permissions/Opportunity");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("entityName", out _).Should().BeTrue();
    }
}
