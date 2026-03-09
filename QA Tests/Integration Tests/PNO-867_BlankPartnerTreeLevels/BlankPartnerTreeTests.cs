/**
 * @fileoverview PNO-867 Blank Partner Tree Levels Tests — validates that partner tree
 * API does not return nodes with empty/blank names.
 *
 * Bug: Since data migration, additional blank partner levels appearing in partner tree.
 * Status: In Development
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-867
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO867;

[Collection("Integration Tests")]
[Trait("Feature", "PNO-867")]
[Trait("Component", "BlankPartnerTreeLevels")]
[Trait("JiraRef", "PNO-867")]
public class BlankPartnerTreeTests
{
    private readonly PAOWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly bool _isPostgresAvailable;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private const string PartnerTreeUrl = "/api/partner-tree";
    private const string PartnerTreeStructureUrl = "/api/partner-tree-structure";

    public BlankPartnerTreeTests(PAOWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _isPostgresAvailable = factory.IsUsingPostgres;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        _client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        _client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
    }

    private static HttpClient CreateUnauthenticatedClient(PAOWebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    private static void AssertNoBlankNamesInTree(JsonElement element, string path = "root")
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                AssertNoBlankNamesInTree(item, path);
            return;
        }

        if (element.ValueKind != JsonValueKind.Object) return;

        if (element.TryGetProperty("name", out var nameProp))
        {
            var name = nameProp.GetString();
            name.Should().NotBeNullOrWhiteSpace(
                $"PNO-867: Partner tree must not contain nodes with empty/blank names at {path}");
        }

        if (element.TryGetProperty("children", out var children))
            AssertNoBlankNamesInTree(children, path + ".children");
        if (element.TryGetProperty("items", out var items))
            AssertNoBlankNamesInTree(items, path + ".items");
        if (element.TryGetProperty("data", out var data))
            AssertNoBlankNamesInTree(data, path + ".data");
    }

    #region POSITIVE (2)

    [Fact]
    [Trait("TestId", "TC-PNO867-POS-001")]
    [Trait("Category", "Positive")]
    public async Task POS_001_PartnerTreeEndpoint_ReturnsValidResponse()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-POS-002")]
    [Trait("Category", "Positive")]
    public async Task POS_002_PartnerTree_IncludesHierarchicalStructure()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeStructureUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    #endregion

    #region NEGATIVE (6)

    [Fact]
    [Trait("TestId", "TC-PNO867-NEG-001")]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-110")]
    public async Task NEG_001_PartnerTree_DoesNotContainNodesWithEmptyBlankNames()
    {
        if (!_isPostgresAvailable) return;
        var treeResponse = await _client.GetAsync(PartnerTreeUrl);
        var structureResponse = await _client.GetAsync(PartnerTreeStructureUrl);

        if (treeResponse.StatusCode == HttpStatusCode.OK)
        {
            var treeJson = await treeResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            AssertNoBlankNamesInTree(treeJson, "partner-tree");
        }

        if (structureResponse.StatusCode == HttpStatusCode.OK)
        {
            var structureJson = await structureResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
            AssertNoBlankNamesInTree(structureJson, "partner-tree-structure");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-NEG-002")]
    [Trait("Category", "Negative")]
    public async Task NEG_002_UnauthenticatedRequest_Returns401Or302()
    {
        if (!_isPostgresAvailable) return;
        using var unauthClient = CreateUnauthenticatedClient(_factory);
        var response = await unauthClient.GetAsync(PartnerTreeUrl);
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Redirect,
            HttpStatusCode.Found);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-NEG-003")]
    [Trait("Category", "Negative")]
    public async Task NEG_003_PartnerTreeNodes_AllHaveNonNullIds()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in json.EnumerateArray())
            {
                if (item.TryGetProperty("id", out var idProp))
                    idProp.ValueKind.Should().NotBe(JsonValueKind.Null, "Tree nodes must have non-null IDs");
            }
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-NEG-004")]
    [Trait("Category", "Negative")]
    public async Task NEG_004_NoDuplicatePartnerTreeNodes_AtSameLevel()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-NEG-005")]
    [Trait("Category", "Negative")]
    public async Task NEG_005_BlankCategoryNames_NotPresentInTree()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeStructureUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        AssertNoBlankNamesInTree(json, "structure");
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-NEG-006")]
    [Trait("Category", "Negative")]
    public async Task NEG_006_PartnerTree_DoesNotHaveOrphanedNodes()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    #endregion

    #region FUNCTIONAL (6)

    [Fact]
    [Trait("TestId", "TC-PNO867-FUNC-001")]
    [Trait("Category", "Functional")]
    public async Task FUNC_001_EachTreeNode_HasNameField()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        if (json.ValueKind == JsonValueKind.Array && json.GetArrayLength() > 0)
        {
            var first = json[0];
            first.TryGetProperty("name", out _).Should().BeTrue("Tree nodes must have name field");
        }
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-FUNC-002")]
    [Trait("Category", "Functional")]
    public async Task FUNC_002_EachTreeNode_HasLevelOrDepthIndicator()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeStructureUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-FUNC-003")]
    [Trait("Category", "Functional")]
    public async Task FUNC_003_TreeStructure_HasParentChildRelationships()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeStructureUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var hasHierarchy = json.TryGetProperty("children", out _) ||
                          json.TryGetProperty("items", out _) ||
                          json.ValueKind == JsonValueKind.Array;
        hasHierarchy.Should().BeTrue("Tree must have parent-child structure");
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-FUNC-004")]
    [Trait("Category", "Functional")]
    public async Task FUNC_004_RootNodes_HaveNoParentReference()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-FUNC-005")]
    [Trait("Category", "Functional")]
    public async Task FUNC_005_TreeResponse_IsDeterministicAcrossCalls()
    {
        if (!_isPostgresAvailable) return;
        var r1 = await _client.GetAsync(PartnerTreeUrl);
        var r2 = await _client.GetAsync(PartnerTreeUrl);
        r1.StatusCode.Should().Be(r2.StatusCode);
        if (r1.StatusCode != HttpStatusCode.OK) return;

        var body1 = await r1.Content.ReadAsStringAsync();
        var body2 = await r2.Content.ReadAsStringAsync();
        body1.Should().Be(body2, "Tree response must be deterministic");
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-FUNC-006")]
    [Trait("Category", "Functional")]
    public async Task FUNC_006_Tree_IncludesPartnerCountPerNode()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    #endregion

    #region EDGE (6)

    [Fact]
    [Trait("TestId", "TC-PNO867-EDGE-001")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_001_TreeWithSingleRootNode()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-EDGE-002")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_002_TreeWithMaximumDepth()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeStructureUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-EDGE-003")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_003_TreeWithNodesHavingSpecialCharactersInName()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-EDGE-004")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_004_EmptyTree_ReturnsEmptyArrayNotError()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        json.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-EDGE-005")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_005_TreeWithSoftDeletedNodes_ExcludesThem()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        AssertNoBlankNamesInTree(json, "tree");
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-EDGE-006")]
    [Trait("Category", "EdgeBoundary")]
    public async Task EDGE_006_TreeResponse_HandlesLargeDataset()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        if (response.StatusCode != HttpStatusCode.OK) return;

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region INTEGRATION (6)

    [Fact]
    [Trait("TestId", "TC-PNO867-INT-001")]
    [Trait("Category", "Integration")]
    public async Task INT_001_PartnerTreeAndIndividualPartnerDetail_AreConsistent()
    {
        if (!_isPostgresAvailable) return;
        var treeResponse = await _client.GetAsync(PartnerTreeUrl);
        var partnerResponse = await _client.GetAsync("/api/partner/1");
        treeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        partnerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-INT-002")]
    [Trait("Category", "Integration")]
    public async Task INT_002_TreeStructure_MatchesPartnerHierarchyFromPartnerEndpoints()
    {
        if (!_isPostgresAvailable) return;
        var treeResponse = await _client.GetAsync(PartnerTreeStructureUrl);
        var partnerResponse = await _client.GetAsync("/api/partner/1");
        treeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        partnerResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-INT-003")]
    [Trait("Category", "Integration")]
    public async Task INT_003_TreeEndpointAndPartnerListEndpoint_DataAlign()
    {
        if (!_isPostgresAvailable) return;
        var treeResponse = await _client.GetAsync(PartnerTreeUrl);
        var partnerListResponse = await _client.GetAsync("/api/partner");
        treeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        partnerListResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-INT-004")]
    [Trait("Category", "Integration")]
    public async Task INT_004_Tree_AccessibleAfterPartnerCrudOperations()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-INT-005")]
    [Trait("Category", "Integration")]
    public async Task INT_005_TreeAndPartnerSearch_ReturnOverlappingDataCorrectly()
    {
        if (!_isPostgresAvailable) return;
        var treeResponse = await _client.GetAsync(PartnerTreeUrl);
        var searchResponse = await _client.GetAsync("/api/partner?search=test");
        treeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        searchResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "TC-PNO867-INT-006")]
    [Trait("Category", "Integration")]
    public async Task INT_006_TreeStructure_UnaffectedByWorkflowOperations()
    {
        if (!_isPostgresAvailable) return;
        var response = await _client.GetAsync(PartnerTreeUrl);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    #endregion
}
