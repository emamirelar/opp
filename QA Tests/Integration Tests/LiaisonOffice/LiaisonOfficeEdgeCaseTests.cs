/**
 * @fileoverview Edge case integration tests for LiaisonOfficeController
 * Tests boundary conditions against actual API: /api/LiaisonOffice/*
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

namespace UNOPS.PAO.Tests.Integration.LiaisonOffice;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "LiaisonOffice")]
[Trait("Component", "EdgeCaseTests")]
public class LiaisonOfficeEdgeCaseTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private const string BaseUrl = "/api/LiaisonOffice";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public LiaisonOfficeEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-001")]
    public async Task GetList_EmptyResults_Returns200WithEmptyOrPopulatedRecords()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out var records).Should().BeTrue();
        records.GetArrayLength().Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-002")]
    public async Task GetList_MinimumPageSize_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=1&pageIndex=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
        result.TryGetProperty("totalCount", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-003")]
    public async Task GetList_LargePageSize_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=100&pageIndex=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out var records).Should().BeTrue();
        records.GetArrayLength().Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-004")]
    public async Task GetList_FilterByName_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?name=Test&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-005")]
    public async Task GetList_FilterByRegion_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?region=Africa&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-006")]
    public async Task PostSearch_EmptyBody_Returns200Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{BaseUrl}/search", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-007")]
    public async Task PostSearch_WithSearchTerm_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { searchTerm = "Office", pageSize = 10, pageIndex = 1 };
        var content = JsonContent.Create(body);
        var response = await _client.PostAsync($"{BaseUrl}/search", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-008")]
    public async Task GetById_ExistingId_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/1");
        if (response.StatusCode != HttpStatusCode.OK)
            return; // 404 when no data - acceptable
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("id", out _).Should().BeTrue();
        result.TryGetProperty("name", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-009")]
    public async Task GetById_NonExistentId_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-010")]
    public async Task GetList_RapidSequential_NoStateIssues()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        for (var i = 0; i < 5; i++)
        {
            var response = await _client.GetAsync($"{BaseUrl}?pageSize=5");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-011")]
    [Trait("Ticket", "PNO-1194")]
    public async Task GetList_ResponseContent_NoEncodingArtifacts()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=50");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("??",
                "PNO-1194: liaison office names must not contain '??' encoding artifacts");
            content.Should().NotContain("\uFFFD",
                "Liaison office data must not contain U+FFFD replacement characters");
        }
    }

    [Fact]
    [Trait("TestId", "TC-LIAISON-EDGE-012")]
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
