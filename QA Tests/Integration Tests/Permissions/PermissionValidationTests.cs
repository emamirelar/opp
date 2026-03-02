/**
 * @fileoverview Validation integration tests for PermissionController
 * Tests response structure, content-type, JSON format for all endpoints
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
[Trait("Component", "ValidationTests")]
public class PermissionValidationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/permissions";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PermissionValidationTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-PERM-VAL-001")]
    public async Task GetSystemPermissionConfig_ReturnsApplicationJson()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    [Trait("TestId", "TC-PERM-VAL-002")]
    public async Task GetSystemPermissionConfig_ReturnsRolesAndTotalRoles()
    {
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("roles", out _).Should().BeTrue();
        result.TryGetProperty("totalRoles", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-VAL-003")]
    public async Task CheckRoutePermission_ReturnsHasAccessAndRoute()
    {
        var response = await _client.GetAsync($"{BaseUrl}/check/partnerships/contacts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("route", out _).Should().BeTrue();
        result.TryGetProperty("hasAccess", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-VAL-004")]
    public async Task GetEntityPermissions_Contact_ReturnsValidStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/entity-permissions/Contact");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("entityName", out _).Should().BeTrue();
        result.TryGetProperty("userPermissions", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-VAL-005")]
    public async Task GetEntityPermissions_Partner_ReturnsValidStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/entity-permissions/Partner");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("userPermissions", out var perms).Should().BeTrue();
        perms.TryGetProperty("canRead", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-PERM-VAL-006")]
    public async Task GetAvailableRoles_ReturnsApplicationJson()
    {
        var response = await _client.GetAsync($"{BaseUrl}/available-roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    [Trait("TestId", "TC-PERM-VAL-007")]
    public async Task GetAvailableRoles_ReturnsRolesArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/available-roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("roles", out var roles).Should().BeTrue();
        roles.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-PERM-VAL-008")]
    public async Task AllEndpoints_ReturnValidJson()
    {
        var urls = new[]
        {
            BaseUrl,
            $"{BaseUrl}/check/dashboard",
            $"{BaseUrl}/entity-permissions/Interaction",
            $"{BaseUrl}/user-roles",
            $"{BaseUrl}/available-roles"
        };
        foreach (var url in urls)
        {
            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
                var json = await response.Content.ReadAsStringAsync();
                json.Should().NotBeNullOrEmpty();
            }
        }
    }
}
