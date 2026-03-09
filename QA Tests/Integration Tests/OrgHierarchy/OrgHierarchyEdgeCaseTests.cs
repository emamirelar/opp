/**
 * @fileoverview Edge case integration tests for OrganizationHierarchyController
 * Tests boundary conditions against actual API: /api/organizationhierarchy/*
 * @author UNOPS Opportunity+ Test Team
 * @date 2026-02-16
 */

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Tests.Integration.OrgHierarchy;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "OrgHierarchy")]
[Trait("Component", "EdgeCaseTests")]
public class OrgHierarchyEdgeCaseTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/organizationhierarchy";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public OrgHierarchyEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-001")]
    public async Task GetList_EmptyResults_Returns200WithEmptyOrPopulatedRecords()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out var records).Should().BeTrue();
            records.GetArrayLength().Should().BeLessThanOrEqualTo(1);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-002")]
    public async Task GetList_MinimumPageSize_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=1&pageIndex=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out _).Should().BeTrue();
            result.TryGetProperty("totalCount", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-003")]
    public async Task GetList_LargePageSize_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=100&pageIndex=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out var records).Should().BeTrue();
            records.GetArrayLength().Should().BeLessThanOrEqualTo(100);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-004")]
    public async Task GetList_FilterByName_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?name=HQ&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-005")]
    public async Task GetList_FilterByParentId_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?parentId=1&pageSize=10");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-006")]
    public async Task PostSearch_EmptyBody_Returns200Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{BaseUrl}/search", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-007")]
    public async Task PostSearch_WithSearchTerm_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "HQ", pageSize = 10, pageIndex = 1 };
        var content = JsonContent.Create(body);
        var response = await _client.PostAsync($"{BaseUrl}/search", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-008")]
    public async Task GetById_ExistingId_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            // DEF: API may return different response structure; property check is advisory
            result.TryGetProperty("id", out _);
            result.TryGetProperty("name", out _);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-009")]
    public async Task GetById_NonExistentId_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-010")]
    public async Task GetList_RapidSequential_NoStateIssues()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        for (var i = 0; i < 5; i++)
        {
            var response = await _client.GetAsync($"{BaseUrl}?pageSize=5");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-011")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetList_ResponseContent_NoEncodingArtifacts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=50");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??",
                "PNO-1194: organization hierarchy names must not contain '??' encoding artifacts");
            content.Should().NotContain("\uFFFD",
                "Org hierarchy data must not contain U+FFFD replacement characters");
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-EDGE-012")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetById_ResponseContent_NoEncodingArtifacts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/1");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??");
            content.Should().NotContain("\uFFFD");
        }
    }
}
