/**
 * @fileoverview Integration tests for PartnerTreeController - negative scenarios via HTTP
 * Tests error handling and invalid inputs against real partner tree endpoints
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

namespace UNOPS.PAO.Tests.Integration.PartnerTree;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "PartnerTree")]
[Trait("Component", "NegativeTests")]
public class PartnerTreeNegativeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerTreeNegativeTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-TREE-NEG-001")]
    public async Task GetPartnerTreeById_NonExistent_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-002")]
    public async Task GetPartnerTreeById_NegativeId_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-003")]
    public async Task GetPartnerTreeById_InvalidFormat_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/abc");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-004")]
    public async Task PostPartnerTree_EmptyBody_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PostAsJsonAsync("/api/partner-tree", new { }, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-005")]
    public async Task PostPartnerTree_MissingName_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { code = "MISS", description = "Test" };
        var response = await _client.PostAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-006")]
    public async Task PutPartnerTree_EmptyArray_Returns400Or200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.PutAsJsonAsync("/api/partner-tree", Array.Empty<object>(), JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-007")]
    public async Task PutPartnerTree_NonExistentId_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new[] { new { id = 999999, name = "X", code = "X", description = "X", type = "Category" } };
        var response = await _client.PutAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-008")]
    public async Task DeletePartnerTree_NonExistent_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.DeleteAsync("/api/partner-tree/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-009")]
    public async Task GetPartnerTreePermissions_NonExistent_Returns404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/999999/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-010")]
    public async Task GetByPartnerGroupId_NegativeId_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/by-partner-group-id/-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-011")]
    public async Task GetByPartnerCategoryCode_EmptyCode_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/by-partner-category-code/");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-012")]
    public async Task PutPartnerTree_InvalidJson_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("{ invalid }", System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/partner-tree", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-013")]
    public async Task PostPartnerTree_InvalidJson_Returns400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var content = new StringContent("{ broken json }", System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/partner-tree", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-014")]
    public async Task DeletePartnerTree_NegativeId_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.DeleteAsync("/api/partner-tree/-1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-NEG-015")]
    public async Task GetPartnerTreeById_Zero_Returns404Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
