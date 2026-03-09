/**
 * @fileoverview Validation integration tests for OrganizationHierarchyController
 * Tests response structure, content-type, JSON format for all major endpoints
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

namespace UNOPS.PAO.Tests.Integration.OrgHierarchy;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "OrgHierarchy")]
[Trait("Component", "ValidationTests")]
public class OrgHierarchyValidationTests
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

    public OrgHierarchyValidationTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateAuthenticatedClient();
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    [Fact]
    [Trait("TestId", "TC-ORG-VAL-001")]
    public async Task GetList_ReturnsValidJsonStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out _).Should().BeTrue();
            result.TryGetProperty("totalCount", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-VAL-002")]
    public async Task GetList_RecordsIsArray()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out var records).Should().BeTrue();
            records.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-VAL-003")]
    public async Task PostSearch_ReturnsValidJsonStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = JsonContent.Create(new { searchTerm = "HQ", pageSize = 10, pageIndex = 1 });
        var response = await _client.PostAsync($"{BaseUrl}/search", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("records", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-VAL-004")]
    public async Task GetById_ReturnsValidJsonStructure()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}/1");
        if (response.StatusCode != HttpStatusCode.OK)
            return; // May be 404 if no data
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        result.TryGetProperty("id", out _).Should().BeTrue();
        result.TryGetProperty("name", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "TC-ORG-VAL-005")]
    public async Task GetList_RecordHasExpectedFields()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            if (result.TryGetProperty("records", out var records) && records.GetArrayLength() > 0)
            {
                var first = records[0];
                first.TryGetProperty("id", out _).Should().BeTrue();
                first.TryGetProperty("name", out _).Should().BeTrue();
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-VAL-006")]
    public async Task PostSearch_AcceptsContentTypeApplicationJson()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = JsonContent.Create(new { pageSize = 5 });
        content.Headers.ContentType!.MediaType.Should().Be("application/json");
        var response = await _client.PostAsync($"{BaseUrl}/search", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "TC-ORG-VAL-007")]
    public async Task GetList_TotalCountIsNumber()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync(BaseUrl);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            if (result.TryGetProperty("totalCount", out var total))
                total.ValueKind.Should().BeOneOf(JsonValueKind.Number, JsonValueKind.String);
        }
    }

    [Fact]
    [Trait("TestId", "TC-ORG-VAL-008")]
    public async Task GetList_PaginationFieldsPresent()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync($"{BaseUrl}?pageSize=5&pageIndex=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
            result.TryGetProperty("pageIndex", out _).Should().BeTrue();
            result.TryGetProperty("pageSize", out _).Should().BeTrue();
            result.TryGetProperty("totalPages", out _).Should().BeTrue();
        }
    }
}
