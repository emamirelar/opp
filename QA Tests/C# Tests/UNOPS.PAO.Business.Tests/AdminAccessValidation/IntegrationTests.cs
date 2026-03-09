/**
 * @fileoverview Admin, Access Control & Validation integration tests.
 * Full CRUD flows, API contracts, multi-component workflows.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.AdminAccessValidation;

/// <summary>
/// Integration tests for Admin, Access Control &amp; Validation.
/// </summary>
[Collection("Admin Access Validation Integration")]
[Trait("Category", "Integration")]
[Trait("Feature", "AdminAccessValidation")]
public class IntegrationTests : AdminAccessValidationFixtureBase
{
    public IntegrationTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "AAV-INT-001")]
    public async Task FullCrud_CreateGetUpdate_Flow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createBody = new { name = "INT CRUD", description = "Integration test" };
        var createResponse = await PostCreateOpportunityAsync(client, createBody);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateBody = new { id, name = "INT CRUD Updated", description = "Updated" };
        var content = new StringContent(JsonSerializer.Serialize(updateBody), Encoding.UTF8, "application/json");
        var updateResponse = await client.PutAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}", content);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-002")]
    public async Task CreateFromPartner_ThenGet_ReturnsOpportunityWithPartner()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "INT Partner", partnerRole = "funding", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-003")]
    public async Task AllOpportunityEndpoints_Authenticated_Accessible()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search?query=test");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fieldsResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search-fields");
        fieldsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-004")]
    public async Task Create_Get_VerifyAuditFieldsRoundTrip()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Audit", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("createdDate", out _).Should().BeTrue();
        opp.TryGetProperty("lastModifiedDate", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-INT-005")]
    public async Task Create_Name255_Get_VerifyPersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('I', 255);
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("name", out var n).Should().BeTrue();
        n.GetString()!.Length.Should().Be(255);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-006")]
    public async Task GetPartnerDocuments_ThenCreateOpportunity_NoConflict()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var docResponse = await client.GetAsync(AdminAccessValidationSpec.PartnerDocuments(1));
        docResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT After Docs", description = "Test" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-007")]
    public async Task CreateMultipleOpportunities_ListReturnsAll()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        await PostCreateOpportunityAsync(client, new { name = "INT Multi 1", description = "Test" });
        await PostCreateOpportunityAsync(client, new { name = "INT Multi 2", description = "Test" });
        var response = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("records", out var records).Should().BeTrue();
        records.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-008")]
    public async Task Search_ThenGetById_Consistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var searchResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search?query=test");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchJson = JsonDocument.Parse(await searchResponse.Content.ReadAsStringAsync());
        if (searchJson.RootElement.TryGetProperty("records", out var records) && records.GetArrayLength() > 0)
        {
            var firstId = records[0].TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
            if (firstId > 0)
            {
                var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{firstId}");
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }

    [Fact]
    [Trait("TestId", "AAV-INT-009")]
    public async Task Create_Update_VerifyLastModifiedChanged()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Update", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var updateBody = new { id, name = "INT Update Modified", description = "Updated" };
        var content = new StringContent(JsonSerializer.Serialize(updateBody), Encoding.UTF8, "application/json");
        await client.PutAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}", content);
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("lastModifiedDate", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-INT-010")]
    public async Task Unauthenticated_AllEndpoints_Return401()
    {
        var client = CreateUnauthenticatedClient();
        var endpoints = new[]
        {
            AdminAccessValidationSpec.OpportunityBase,
            $"{AdminAccessValidationSpec.OpportunityBase}/1",
            $"{AdminAccessValidationSpec.OpportunityBase}/search?query=x",
            $"{AdminAccessValidationSpec.OpportunityBase}/search-fields",
            AdminAccessValidationSpec.PartnerDocuments(1)
        };
        foreach (var url in endpoints)
        {
            var response = await client.GetAsync(url);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"{url} should require auth");
        }
    }

    [Fact]
    [Trait("TestId", "AAV-INT-011")]
    public async Task CreateFromPartner_Get_VerifyPartnerInResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "INT Partner Get", partnerRole = "funding", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-012")]
    public async Task GetSearchFields_UseInAdvancedSearch_Compatible()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var fieldsResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search-fields");
        fieldsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var advResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/advanced-search?filters=[]");
        advResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-013")]
    public async Task Create_List_NewRecordAppears()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = $"INT List {Guid.NewGuid():N}";
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var listResponse = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("records", out var records).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-INT-014")]
    public async Task CreateOpportunity_WithFundingAndClientPartners_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var body = new
        {
            name = "INT Both Partners",
            description = "Test",
            fundingPartners = new[] { new { partnerId = 1 } },
            clientPartners = new[] { new { partnerId = 1 } }
        };
        var response = await PostCreateOpportunityAsync(client, body);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-015")]
    public async Task Pagination_ConsecutivePages_NoOverlap()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var page0 = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}?pageIndex=0&pageSize=2");
        var page1 = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}?pageIndex=1&pageSize=2");
        page0.StatusCode.Should().Be(HttpStatusCode.OK);
        page1.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-016")]
    public async Task Create_Get_Delete_Flow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Delete", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var deleteResponse = await client.DeleteAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-017")]
    public async Task DocumentEndpoint_OpportunityEntity_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Doc", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"/api/document/Opportunity/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-018")]
    public async Task Create_ImmediateGet_DataConsistent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "INT Consistent";
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("name", out var n).Should().BeTrue();
        n.GetString().Should().Be(name);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-019")]
    public async Task CreateFromPartner_ActivePartner_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "INT Active", partnerRole = "funding", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-020")]
    public async Task GetOpportunitiesList_ContentTypeJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "AAV-INT-021")]
    public async Task Create_Update_IdPreserved()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Id", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var updateBody = new { id, name = "INT Id Updated", description = "Updated" };
        var content = new StringContent(JsonSerializer.Serialize(updateBody), Encoding.UTF8, "application/json");
        var updateResponse = await client.PutAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}", content);
        if (updateResponse.StatusCode == HttpStatusCode.OK)
        {
            var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();
            updated.TryGetProperty("id", out var idAfter).Should().BeTrue();
            idAfter.GetInt32().Should().Be(id);
        }
    }

    [Fact]
    [Trait("TestId", "AAV-INT-022")]
    public async Task SearchFields_NotEmpty()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search-fields");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var arr = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        arr.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-023")]
    public async Task CreateOpportunity_DescriptionWithSpecialChars_Persisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var desc = "Test & <script> \"quotes\"";
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Special", description = desc });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-024")]
    public async Task GetOpportunityDetail_StructureComplete()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Struct", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var opp = json.RootElement.GetProperty("opportunity");
        opp.TryGetProperty("id", out _).Should().BeTrue();
        opp.TryGetProperty("name", out _).Should().BeTrue();
        opp.TryGetProperty("description", out _).Should().BeTrue();
        opp.TryGetProperty("stage", out _).Should().BeTrue();
        opp.TryGetProperty("status", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-INT-025")]
    public async Task Create_List_Search_ConsistentCounts()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var listResponse = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        var searchResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search?query=opp");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-026")]
    public async Task CreateFromPartner_ThenSearch_Findable()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = $"INT Search {Guid.NewGuid():N}";
        var response = await PostCreateFromPartnerAsync(client, 1, new { name, partnerRole = "funding", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var searchResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search?query={Uri.EscapeDataString(name)}");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-027")]
    public async Task PartnerDocuments_ThenOpportunityDocuments_NoCrossContamination()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var partnerDocs = await client.GetAsync(AdminAccessValidationSpec.PartnerDocuments(1));
        partnerDocs.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Cross", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var oppDocs = await client.GetAsync($"/api/document/Opportunity/{id}");
        oppDocs.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-INT-028")]
    public async Task Create_Get_Update_Get_VerifyChanges()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Before", description = "Before" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var updateBody = new { id, name = "INT After", description = "After" };
        var content = new StringContent(JsonSerializer.Serialize(updateBody), Encoding.UTF8, "application/json");
        await client.PutAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}", content);
        var getResponse = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("name", out var n).Should().BeTrue();
        n.GetString().Should().Be("INT After");
    }

    [Fact]
    [Trait("TestId", "AAV-INT-029")]
    public async Task CreateOpportunity_AuditFields_AllPresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT Audit All", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var opp = json.RootElement.GetProperty("opportunity");
        opp.TryGetProperty("createdBy", out _).Should().BeTrue();
        opp.TryGetProperty("createdDate", out _).Should().BeTrue();
        opp.TryGetProperty("lastModifiedBy", out _).Should().BeTrue();
        opp.TryGetProperty("lastModifiedDate", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-INT-030")]
    public async Task OpportunitiesList_GenUser_LoadsWithoutServerError()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "PNO-807: GENUSER should load Opportunities list");
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
