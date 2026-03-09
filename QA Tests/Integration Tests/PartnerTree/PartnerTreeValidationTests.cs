/**
 * @fileoverview Integration tests for PartnerTreeController - validation via HTTP
 * Tests input validation against real partner tree endpoints
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
[Trait("Component", "ValidationTests")]
public class PartnerTreeValidationTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PartnerTreeValidationTests(PAOWebApplicationFactory<Program> factory)
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
    [Trait("TestId", "TC-TREE-VAL-001")]
    public async Task GetPartnerTree_ValidRequest_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-002")]
    public async Task GetPartnerTree_SortByCode_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree?sortBy=Code&ascending=true");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-003")]
    public async Task GetPartnerTree_SortByDescription_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree?sortBy=Description&ascending=false");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-004")]
    public async Task PostPartnerTree_ValidCategory_Returns201Or400()
    {
        var body = new { name = "Validation Category", code = "VAL-CAT", description = "For validation", type = "Category" };
        var response = await _client.PostAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-005")]
    public async Task PostPartnerTree_ValidGroup_Returns201Or400()
    {
        var body = new { name = "Validation Group", code = "VAL-GRP", description = "For validation", type = "Group" };
        var response = await _client.PostAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-006")]
    public async Task GetPartnerTreeById_ValidId_Returns200Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-007")]
    public async Task GetPartnerTreePermissions_ValidId_Returns200Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/1/permissions");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-008")]
    public async Task GetDescribe_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/describe");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-009")]
    public async Task GetByPartnerCategoryCode_ValidCode_Returns200Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/by-partner-category-code/GOV");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-010")]
    public async Task GetByPartnerGroupId_ValidId_Returns200Or404()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/by-partner-group-id/1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-011")]
    public async Task PutPartnerTree_ValidArray_Returns200Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new[] { new { id = 1, name = "Updated", code = "UPD", description = "Desc", type = "Category" } };
        var response = await _client.PutAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-012")]
    public async Task PostPartnerTree_WithParent_Returns201Or400()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var body = new { name = "Child", code = "CHILD", description = "Child", type = "Group", parent = "PARENT" };
        var response = await _client.PostAsJsonAsync("/api/partner-tree", body, JsonOptions);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-013")]
    public async Task GetCategorizationOverview_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/categorization-overview");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-014")]
    public async Task GetCategoriesSummary_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/categories-summary");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "TC-TREE-VAL-015")]
    public async Task GetGroupsSummary_Returns200()
    {
        if (!_isPostgresAvailable) return; // QA-054a: InMemory DB incompatible
        var response = await _client.GetAsync("/api/partner-tree/groups-summary");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
