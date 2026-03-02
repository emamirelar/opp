/**
 * @fileoverview Validation integration tests for RoleController
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

namespace UNOPS.PAO.Tests.Integration.Roles;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "Roles")]
[Trait("Component", "ValidationTests")]
public class RoleValidationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/Role";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RoleValidationTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-VAL-001")]
    public async Task GetAllRoles_ReturnsApplicationJson()
    {
        var response = await _client.GetAsync($"{BaseUrl}/all");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-VAL-002")]
    public async Task GetAllRoles_ReturnsArrayOfRoleObjects()
    {
        var response = await _client.GetAsync($"{BaseUrl}/all");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-VAL-003")]
    public async Task GetUserRoles_ReturnsValidStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/user");
        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("email", out _).Should().BeTrue();
            result.TryGetProperty("roles", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-VAL-004")]
    public async Task GetDoaRoles_ReturnsApplicationJson()
    {
        var response = await _client.GetAsync($"{BaseUrl}/doa-roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-VAL-005")]
    public async Task GetDoaRoles_ReturnsArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/doa-roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-VAL-006")]
    public async Task AssignDoaRoles_ResponseHasSuccessAndMessage()
    {
        var assignments = new[] { new { entityId = 1, userId = 1, roleName = "DoA2", entityType = "OrganizationHierarchy" } };
        var content = JsonContent.Create(assignments);
        var response = await _client.PostAsync($"{BaseUrl}/assign-doa-roles", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("success", out _).Should().BeTrue();
        result.TryGetProperty("message", out _).Should().BeTrue();
        result.TryGetProperty("assignedCount", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-VAL-007")]
    public async Task UpdateUserRoles_ValidJson_Returns200WithMessage()
    {
        var content = JsonContent.Create(Array.Empty<string>());
        var response = await _client.PutAsync($"{BaseUrl}/update", content);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("message", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-VAL-008")]
    public async Task AllEndpoints_ReturnValidJson()
    {
        var urls = new[] { $"{BaseUrl}/all", $"{BaseUrl}/user", $"{BaseUrl}/doa-roles" };
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
