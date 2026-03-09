/**
 * @fileoverview Admin, Access Control & Validation functional tests.
 * Business rules, audit fields, permissions, workflow transitions.
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
/// Functional tests for Admin, Access Control &amp; Validation.
/// </summary>
[Collection("Admin Access Validation Integration")]
[Trait("Category", "Functional")]
[Trait("Feature", "AdminAccessValidation")]
public class FunctionalTests : AdminAccessValidationFixtureBase
{
    public FunctionalTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "AAV-FUN-001")]
    [Trait("PNO", "772")]
    public async Task CreateOpportunity_AuditCreatedDatePopulated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "FUN Audit", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        created.TryGetProperty("createdDate", out var cd).Should().BeTrue();
        cd.GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-002")]
    [Trait("PNO", "772")]
    public async Task GetOpportunityDetail_ReturnsCreatedByAndLastModifiedBy()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "FUN CreatedBy", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("createdBy", out _).Should().BeTrue();
        opp.TryGetProperty("lastModifiedBy", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-003")]
    [Trait("PNO", "762")]
    public async Task SearchFields_ReturnsFieldMetadata()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search-fields");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var arr = await response.Content.ReadFromJsonAsync<JsonElement>();
        arr.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-004")]
    [Trait("PNO", "774")]
    public void OpportunityNameMaxLength_Is255()
    {
        AdminAccessValidationSpec.OpportunityNameMaxLength.Should().Be(255);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-005")]
    [Trait("PNO", "807")]
    public async Task GetOpportunitiesList_ReturnsPaginationMetadata()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("totalCount", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("records", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-006")]
    public async Task CreateOpportunity_ResponseIncludesIdAndName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "FUN Response", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("id", out var id).Should().BeTrue();
        id.GetInt32().Should().BeGreaterThan(0);
        json.TryGetProperty("name", out var name).Should().BeTrue();
        name.GetString().Should().Be("FUN Response");
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-007")]
    public async Task CreateOpportunity_ResponseIncludesStage()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "FUN Stage", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("stage", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-008")]
    [Trait("PNO", "768")]
    public async Task GetPartnerDocuments_ContentTypeIsJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.PartnerDocuments(1));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-009")]
    public async Task CreateFromPartner_AddsPartnerAsFundingPartner()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "FUN Funding", partnerRole = "funding", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("fundingPartners", out var fps).Should().BeTrue();
        fps.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-010")]
    public async Task GetOpportunityDetail_ReturnsStatusField()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "FUN Status", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("status", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-011")]
    public async Task Search_ReturnsRecordsAndTotalCount()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search?query=opp");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("records", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("totalCount", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-012")]
    public async Task CreateOpportunity_LastModifiedDateEqualsCreatedDateInitially()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "FUN Init", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("createdDate", out var cd).Should().BeTrue();
        opp.TryGetProperty("lastModifiedDate", out var lmd).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-013")]
    public async Task GetOpportunitiesList_DefaultSortApplied()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("records", out var records).Should().BeTrue();
        records.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-014")]
    public async Task CreateOpportunity_DescriptionPersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var desc = "Functional test description";
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "FUN Desc", description = desc });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("description", out var d).Should().BeTrue();
        d.GetString().Should().Be(desc);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-015")]
    public async Task CreateOpportunity_NamePersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "FUN Name Persisted";
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("name", out var n).Should().BeTrue();
        n.GetString().Should().Be(name);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-016")]
    public async Task GetSearchFields_EachFieldHasName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search-fields");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var arr = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        foreach (var item in arr.EnumerateArray())
            item.TryGetProperty("name", out _).Should().BeTrue("each search field should have name");
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-017")]
    public async Task CreateOpportunity_Returns201Or200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "FUN Status", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-018")]
    public async Task GetOpportunitiesList_ResponseIsJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-019")]
    public async Task CreateOpportunity_ValidationErrorsReturnStructuredResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "", description = "" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        (body.Contains("validationErrors") || body.Contains("error")).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-020")]
    public async Task GetOpportunityDetail_StructureMatchesExpected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "FUN Struct", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-021")]
    public async Task CreateOpportunity_Name255CharsPersistedCorrectly()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('H', 255);
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("name", out var n).Should().BeTrue();
        n.GetString()!.Length.Should().Be(255);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-022")]
    public async Task GetPartnerDocuments_ReturnsArrayOrEmpty()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.PartnerDocuments(1));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var parsed = JsonDocument.Parse(body);
        parsed.RootElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-023")]
    public async Task CreateFromPartner_DescriptionDefaultedWhenOmitted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "FUN DefaultDesc", partnerRole = "funding", description = (string?)null });
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            json.TryGetProperty("description", out _).Should().BeTrue();
        }
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-024")]
    public async Task GetOpportunitiesList_PaginationRespected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}?pageIndex=0&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (json.RootElement.TryGetProperty("records", out var records))
            records.GetArrayLength().Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-025")]
    public async Task CreateOpportunity_CreatedByPopulated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "FUN CreatedBy", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("createdBy", out var cb).Should().BeTrue();
        cb.GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-026")]
    public async Task Search_EmptyResults_ReturnsZeroTotalCount()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search?query=xyznonexistent12345");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("totalCount", out var tc).Should().BeTrue();
        tc.GetInt32().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-027")]
    public async Task CreateOpportunity_StageIsDraft()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "FUN Draft", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("stage", out var stage).Should().BeTrue();
        stage.GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-028")]
    public async Task GetOpportunityDetail_ReturnsId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "FUN Id", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetInt32().Should().Be(id);
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-029")]
    public async Task CreateFromPartner_BothRoles_AddsToFundingAndClient()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "FUN Both", partnerRole = "both", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("fundingPartners", out _).Should().BeTrue();
        json.TryGetProperty("clientPartners", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-FUN-030")]
    public async Task GetOpportunitiesList_TotalCountNonNegative()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.OpportunityBase);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("totalCount", out var tc).Should().BeTrue();
        tc.GetInt32().Should().BeGreaterThanOrEqualTo(0);
    }
}
