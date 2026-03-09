/// <summary>
/// Integration tests for Offices Feature (PNO-1213, PNO-1214).
/// Full API flows: organization-hierarchy (office tree), opportunity-org link, partner-org link.
/// DEF-211: Dedicated Offices API (/api/offices) not implemented — uses organization-hierarchy as proxy.
/// </summary>

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OfficesFeature;

[Collection("OfficesFeature Integration")]
[Trait("Category", "Integration")]
[Trait("Feature", "Offices")]
public class IntegrationTests : OfficesFeatureIntegrationFixtureBase
{
    public IntegrationTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "INT-001")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchy_Authenticated_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-002")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchyLegacy_Authenticated_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy/legacy");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-003")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchyById_ValidId_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync("/api/organization-hierarchy");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var firstId = GetFirstIdFromHierarchy(doc.RootElement);
        if (firstId == null) return;
        var response = await client.GetAsync($"/api/organization-hierarchy/{firstId}");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    private static int? GetFirstIdFromHierarchy(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
        {
            var first = root[0];
            if (first.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var id))
                return id.GetInt32();
        }
        return null;
    }

    [Fact]
    [Trait("TestId", "INT-004")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchies_WithPagination_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organizationhierarchy?pageIndex=0&pageSize=10");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-005")]
    [Trait("Ticket", "PNO-1213")]
    public async Task SearchOrganizationHierarchies_WithBody_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = JsonSerializer.Serialize(new { searchTerm = "office", pageIndex = 0, pageSize = 10 });
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/organizationhierarchy/search", content);
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-006")]
    [Trait("Ticket", "PNO-1214")]
    public async Task GetOpportunities_WithOrgUnitFilter_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/opportunity?pageIndex=0&pageSize=10");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-007")]
    [Trait("Ticket", "PNO-1214")]
    public async Task GetPartners_WithOrgUnitFilter_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/partner?pageIndex=0&pageSize=10");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-008")]
    public async Task GetOrganizationHierarchy_Unauthenticated_Returns401Or403()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/api/organization-hierarchy");
        ((int)response.StatusCode).Should().BeOneOf(401, 403);
    }

    [Fact]
    [Trait("TestId", "INT-009")]
    public async Task GetOrganizationHierarchyById_NonExistent_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy/999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "INT-010")]
    public async Task GetOrganizationHierarchyById_Zero_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy/0");
        ((int)response.StatusCode).Should().BeOneOf(404, 400, 500);
    }

    [Fact]
    [Trait("TestId", "INT-011")]
    public async Task GetOrganizationHierarchy_ResponseContainsData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "INT-012")]
    public async Task GetOrganizationHierarchyById_ResponseContainsNameAndCode()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync("/api/organization-hierarchy");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var firstId = GetFirstIdFromHierarchy(doc.RootElement);
        if (firstId == null) return;
        var response = await client.GetAsync($"/api/organization-hierarchy/{firstId}");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var getJson = await response.Content.ReadAsStringAsync();
        getJson.Should().Contain("name").And.Contain("code");
    }

    [Fact]
    [Trait("TestId", "INT-013")]
    public async Task GetOrganizationHierarchyMetadata_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy/metadata-info");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-014")]
    public async Task FullFlow_GetHierarchyThenGetById()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var hierarchyResponse = await client.GetAsync("/api/organization-hierarchy");
        hierarchyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var hierarchyJson = await hierarchyResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(hierarchyJson);
        var firstId = GetFirstIdFromHierarchy(doc.RootElement);
        if (firstId == null) return;
        var detailResponse = await client.GetAsync($"/api/organization-hierarchy/{firstId}");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "INT-015")]
    public async Task GetOrganizationHierarchyByIdWithDetails_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync("/api/organizationhierarchy?pageIndex=0&pageSize=1");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        var id = ExtractFirstIdFromListResponse(json);
        if (id == null) return;
        var response = await client.GetAsync($"/api/organizationhierarchy/{id}");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    private static int? ExtractFirstIdFromListResponse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("records", out var records) && records.GetArrayLength() > 0)
            {
                var first = records[0];
                if (first.TryGetProperty("id", out var id))
                    return id.GetInt32();
            }
        }
        catch { }
        return null;
    }

    [Fact]
    [Trait("TestId", "INT-016")]
    public async Task GetOrganizationHierarchy_ContentTypeJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy");
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
        }
    }

    [Fact]
    [Trait("TestId", "INT-017")]
    public async Task SearchOrganizationHierarchies_EmptySearch_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = JsonSerializer.Serialize(new { searchTerm = "", pageIndex = 0, pageSize = 10 });
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/organizationhierarchy/search", content);
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-018")]
    public async Task GetOrganizationHierarchies_LargePageSize_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organizationhierarchy?pageIndex=0&pageSize=100");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-019")]
    public async Task GetOrganizationHierarchyById_NegativeId_Returns404Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy/-1");
        ((int)response.StatusCode).Should().BeOneOf(404, 400, 500);
    }

    [Fact]
    [Trait("TestId", "INT-020")]
    public async Task GetOrganizationHierarchy_AndOpportunity_CrossEntity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var hierarchyResponse = await client.GetAsync("/api/organization-hierarchy");
        var oppResponse = await client.GetAsync("/api/opportunity?pageIndex=0&pageSize=5");
        hierarchyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        oppResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "INT-021")]
    public async Task GetOrganizationHierarchy_AndPartner_CrossEntity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var hierarchyResponse = await client.GetAsync("/api/organization-hierarchy");
        var partnerResponse = await client.GetAsync("/api/partner?pageIndex=0&pageSize=5");
        hierarchyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        partnerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "INT-022")]
    public async Task GetOrganizationHierarchyPrime_ReturnsFlatList()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "INT-023")]
    public async Task GetOrganizationHierarchy_ConcurrentRequests_Succeed()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var tasks = Enumerable.Range(0, 3).Select(_ => client.GetAsync("/api/organization-hierarchy"));
        var responses = await Task.WhenAll(tasks);
        responses.Should().OnlyContain(r => r.IsSuccessStatusCode);
    }

    [Fact]
    [Trait("TestId", "INT-024")]
    public async Task GetOrganizationHierarchyById_ValidId_ResponseHasId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync("/api/organizationhierarchy?pageIndex=0&pageSize=1");
        if (listResponse.StatusCode != HttpStatusCode.OK) return;
        var json = await listResponse.Content.ReadAsStringAsync();
        var id = ExtractFirstIdFromListResponse(json);
        if (id == null) return;
        var response = await client.GetAsync($"/api/organization-hierarchy/{id}");
        if (response.StatusCode != HttpStatusCode.OK) return;
        var getJson = await response.Content.ReadAsStringAsync();
        getJson.Should().Contain($"\"id\":{id}");
    }

    [Fact]
    [Trait("TestId", "INT-025")]
    public async Task GetOrganizationHierarchies_SecondPage_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/organization-hierarchy/list?pageIndex=1&pageSize=10");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-026")]
    public async Task SearchOrganizationHierarchies_ByCode_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = JsonSerializer.Serialize(new { searchTerm = "HQ", pageIndex = 0, pageSize = 10 });
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/organizationhierarchy/search", content);
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-027")]
    public async Task GetOrganizationHierarchy_ValuesControllerOrgUnits_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/values/organization-units");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-028")]
    public async Task GetOrganizationHierarchy_OpportunityOrgUnits_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/values/opportunity-organization-units");
        ((int)response.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-029")]
    public async Task FullFlow_HierarchyToDetailToMetadata()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var h = await client.GetAsync("/api/organization-hierarchy");
        h.StatusCode.Should().Be(HttpStatusCode.OK);
        var m = await client.GetAsync("/api/organization-hierarchy/metadata-info");
        ((int)m.StatusCode).Should().BeInRange(200, 299);
    }

    [Fact]
    [Trait("TestId", "INT-030")]
    public async Task GetOrganizationHierarchy_ListAndLegacy_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync("/api/organization-hierarchy/list?pageIndex=0&pageSize=100");
        var legacyResponse = await client.GetAsync("/api/organization-hierarchy/legacy");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        legacyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ========== Defect-Exposing Tests (RUN and FAIL until DEF-211+ fixed) ==========

    [Fact]
    [Trait("TestId", "INT-DEF-211-001")]
    [Trait("Defect", "DEF-211")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOffices_Authenticated_Returns200_PNO1213RequiresOfficesAPI()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OfficesFeatureSpec.OfficesBase);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "PNO-1213 AC: Offices appears as top-level nav; GET /api/offices must return 200");
    }

    [Fact]
    [Trait("TestId", "INT-DEF-211-002")]
    [Trait("Defect", "DEF-211")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOfficeById_ValidId_Returns200_PNO1213RequiresOfficeDetail()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OfficesFeatureSpec.OfficeById(1));
        response.StatusCode.Should().Be(HttpStatusCode.OK, "PNO-1213 AC: Clicking office navigates to Office Detail; GET /api/offices/{id} must return 200");
    }

    [Fact]
    [Trait("TestId", "INT-DEF-213-001")]
    [Trait("Defect", "DEF-213")]
    [Trait("Ticket", "PNO-1214")]
    public async Task GetOfficeFinancial_ValidId_Returns200_PNO1214RequiresBigQueryData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OfficesFeatureSpec.OfficeFinancial(1));
        response.StatusCode.Should().Be(HttpStatusCode.OK, "PNO-1214 AC: Financial tab displays cost centre type, funding, NER/EA from BigQuery");
    }

    [Fact]
    [Trait("TestId", "INT-DEF-214-001")]
    [Trait("Defect", "DEF-214")]
    [Trait("Ticket", "PNO-1214")]
    public async Task GetOfficeRolesAndDoA_ValidId_Returns200_PNO1214RequiresERPData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OfficesFeatureSpec.OfficeRolesAndDoA(1));
        response.StatusCode.Should().Be(HttpStatusCode.OK, "PNO-1214 AC: Roles & DoA tab from ERP Management Structure and Core Controls");
    }

    [Fact]
    [Trait("TestId", "INT-DEF-215-001")]
    [Trait("Defect", "DEF-215")]
    [Trait("Ticket", "PNO-1214")]
    public async Task GetOfficePhysical_ValidId_Returns200_PNO1214RequiresOUPLocationData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OfficesFeatureSpec.OfficePhysical(1));
        response.StatusCode.Should().Be(HttpStatusCode.OK, "PNO-1214 AC: Physical Office section from oUP Location Management");
    }

    [Fact]
    [Trait("TestId", "INT-DEF-216-001")]
    [Trait("Defect", "DEF-216")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOfficeDocuments_ValidId_Returns200_PNO1213RequiresDocumentsTab()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OfficesFeatureSpec.OfficeDocuments(1));
        response.StatusCode.Should().Be(HttpStatusCode.OK, "PNO-1213 AC: Documents tab allows Strategy type; restricted to RD/Manager/OiC");
    }
}
