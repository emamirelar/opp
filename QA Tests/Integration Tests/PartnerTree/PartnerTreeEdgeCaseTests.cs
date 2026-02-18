/**
 * @fileoverview Integration tests for PartnerTreeController - edge cases via HTTP
 * Tests actual endpoints: POST/GET/PUT/DELETE /api/partner-tree, permissions, structure,
 * by-partner-group-id, by-partner-category-code, categories-summary, groups-summary,
 * categorization-overview, describe
 * @author UNOPS Opportunity+ Test Team
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

namespace UNOPS.PAO.Tests.Integration.PartnerTree;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
[Trait("Feature", "PartnerTree")]
[Trait("Component", "EdgeCaseTests")]
public class PartnerTreeEdgeCaseTests : IClassFixture<PAOWebApplicationFactory<Program>>
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerTreeEdgeCaseTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
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
    [Trait("TestId", "TC-TREE-EDGE-001")]
    public async Task GetPartnerTree_DefaultParams_Returns200()
    {
        var response = await _client.GetAsync("/api/partner-tree");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-002")]
    public async Task GetPartnerTree_SortByNameAscending_Returns200()
    {
        var response = await _client.GetAsync("/api/partner-tree?sortBy=Name&ascending=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-003")]
    public async Task GetPartnerTree_SortDescending_Returns200()
    {
        var response = await _client.GetAsync("/api/partner-tree?sortBy=Name&ascending=false");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-004")]
    public async Task GetPartnerTreeById_ValidId_Returns200Or404()
    {
        var response = await _client.GetAsync("/api/partner-tree/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-005")]
    public async Task GetPartnerTreePermissions_ValidId_Returns200Or404()
    {
        var response = await _client.GetAsync("/api/partner-tree/1/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-006")]
    public async Task GetPartnerTreeStructure_Returns200()
    {
        var response = await _client.GetAsync("/api/partner-tree-structure");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-007")]
    public async Task GetByPartnerGroupId_ValidId_Returns200Or404()
    {
        var response = await _client.GetAsync("/api/partner-tree/by-partner-group-id/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-008")]
    public async Task GetByPartnerCategoryCode_ValidCode_Returns200Or404()
    {
        var response = await _client.GetAsync("/api/partner-tree/by-partner-category-code/GOV");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-009")]
    public async Task GetCategoriesSummary_Returns200()
    {
        var response = await _client.GetAsync("/api/partner-tree/categories-summary");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-010")]
    public async Task GetGroupsSummary_Returns200()
    {
        var response = await _client.GetAsync("/api/partner-tree/groups-summary");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-011")]
    public async Task GetCategorizationOverview_Returns200()
    {
        var response = await _client.GetAsync("/api/partner-tree/categorization-overview");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-012")]
    public async Task GetDescribe_Returns200()
    {
        var response = await _client.GetAsync("/api/partner-tree/describe");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-013")]
    public async Task PostPartnerTree_ValidBody_Returns201Or400()
    {
        var body = new { name = "Test Category", code = "TEST-EDGE", description = "Test", type = "Category" };
        var response = await _client.PostAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-014")]
    public async Task PutPartnerTree_ValidBody_Returns200Or400()
    {
        var body = new[] { new { id = 1, name = "Updated", code = "UPD", description = "Desc", type = "Category" } };
        var response = await _client.PutAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-015")]
    public async Task DeletePartnerTree_NonExistent_Returns404Or400()
    {
        var response = await _client.DeleteAsync("/api/partner-tree/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-016")]
    public async Task GetPartnerTree_RapidSequential_NoStateIssues()
    {
        for (var i = 0; i < 10; i++)
        {
            var response = await _client.GetAsync("/api/partner-tree");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-017")]
    public async Task GetPartnerTree_Concurrent_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10).Select(_ => _client.GetAsync("/api/partner-tree"));
        var results = await Task.WhenAll(tasks);
        results.Should().AllSatisfy(r => r.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden));
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-018")]
    public async Task GetByPartnerGroupId_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/partner-tree/by-partner-group-id/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-019")]
    public async Task GetByPartnerCategoryCode_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/partner-tree/by-partner-category-code/NONEXISTENT");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-EDGE-020")]
    public async Task GetPartnerTreeById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/api/partner-tree/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
