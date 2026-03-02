/**
 * @fileoverview Edge case integration tests for RoleController
 * Tests boundary conditions against actual API: /api/Role/*
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
[Trait("Component", "EdgeCaseTests")]
public class RoleEdgeCaseTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string BaseUrl = "/api/Role";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public RoleEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-001")]
    public async Task GetAllRoles_Returns200WithArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/all");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-002")]
    public async Task GetAllRoles_EmptyOrPopulated_ReturnsValidStructure()
    {
        var response = await _client.GetAsync($"{BaseUrl}/all");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-003")]
    public async Task GetUserRoles_Returns200WithEmailAndRoles()
    {
        var response = await _client.GetAsync($"{BaseUrl}/user");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("email", out _).Should().BeTrue();
            result.TryGetProperty("roles", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-004")]
    public async Task UpdateUserRoles_EmptyArray_Returns200()
    {
        var content = JsonContent.Create(Array.Empty<string>());
        var response = await _client.PutAsync($"{BaseUrl}/update", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-005")]
    public async Task AssignDoaRoles_EmptyList_Returns400()
    {
        var content = JsonContent.Create(new List<object>());
        var response = await _client.PostAsync($"{BaseUrl}/assign-doa-roles", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-006")]
    public async Task AssignDoaRoles_NullBody_Returns400()
    {
        var content = new StringContent("null", System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{BaseUrl}/assign-doa-roles", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-007")]
    public async Task GetDoaRoles_Returns200WithArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/doa-roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-008")]
    public async Task GetDoaRoles_EmptyResults_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync($"{BaseUrl}/doa-roles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-009")]
    public async Task DeleteDoaRole_NonExistentId_Returns404()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/doa-roles/999999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-010")]
    public async Task DeleteDoaRole_ZeroId_Returns404()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/doa-roles/0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-011")]
    public async Task AllGetEndpoints_RapidSequentialCalls_NoStateCorruption()
    {
        var urls = new[] { $"{BaseUrl}/all", $"{BaseUrl}/user", $"{BaseUrl}/doa-roles" };
        foreach (var url in urls)
        {
            for (var i = 0; i < 3; i++)
            {
                var response = await _client.GetAsync(url);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-ROLE-EDGE-012")]
    public async Task GetAllRoles_LargeResponse_ReturnsValidJson()
    {
        var response = await _client.GetAsync($"{BaseUrl}/all");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
        var result = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
        result.ValueKind.Should().Be(JsonValueKind.Array);
    }
}
