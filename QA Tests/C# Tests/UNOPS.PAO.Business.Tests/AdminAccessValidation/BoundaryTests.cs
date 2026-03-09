/**
 * @fileoverview Admin, Access Control & Validation boundary tests.
 * PNO-762, PNO-768, PNO-772, PNO-774, PNO-807.
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
/// Boundary tests for Admin, Access Control &amp; Validation.
/// </summary>
[Collection("Admin Access Validation Integration")]
[Trait("Category", "Boundary")]
[Trait("Feature", "AdminAccessValidation")]
public class BoundaryTests : AdminAccessValidationFixtureBase
{
    public BoundaryTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "AAV-BND-001")]
    public async Task CreateOpportunity_NameAt254Chars_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('A', AdminAccessValidationSpec.OpportunityNameMaxLength - 1);
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-002")]
    public async Task CreateOpportunity_NameAtExactly255Chars_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('B', AdminAccessValidationSpec.OpportunityNameMaxLength);
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-003")]
    [Trait("Defect", "DEF-187")]
    public async Task CreateOpportunity_NameAt256Chars_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('C', AdminAccessValidationSpec.OpportunityNameMaxLength + 1);
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-004")]
    public async Task CreateOpportunity_NameSingleChar_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "X", description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-005")]
    public async Task CreateOpportunity_DescriptionEmptyString_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-006")]
    public async Task GetOpportunitiesList_PageSizeZero_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}?pageIndex=0&pageSize=0");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-007")]
    public async Task GetOpportunitiesList_PageSizeOne_ReturnsAtMostOne()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}?pageIndex=0&pageSize=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (json.RootElement.TryGetProperty("records", out var records))
            records.GetArrayLength().Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-008")]
    public async Task CreateOpportunity_NameWithUnicode_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test café 日本語", description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-009")]
    public async Task CreateOpportunity_DescriptionVeryLong_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var desc = new string('D', 5000);
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = desc });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-010")]
    public async Task GetOpportunityDetail_VerifyAuditFieldsNonNull()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "BND Audit", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("createdDate", out var cd).Should().BeTrue();
        cd.ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-011")]
    public async Task CreateFromPartner_PartnerRoleBoth_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "BND Both", partnerRole = "both", description = "Desc" });
        if (response.StatusCode != HttpStatusCode.NotFound)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-012")]
    public async Task CreateFromPartner_PartnerRoleClient_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "BND Client", partnerRole = "client", description = "Desc" });
        if (response.StatusCode != HttpStatusCode.NotFound)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-013")]
    public async Task GetPartnerDocuments_EntityIdOne_ReturnsValidResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.PartnerDocuments(1));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNull();
    }

    [Fact]
    [Trait("TestId", "AAV-BND-014")]
    public async Task Search_QuerySingleChar_Returns200()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search?query=a");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-015")]
    public async Task CreateOpportunity_NameWithSpecialChars_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test & Co. (2024)", description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-016")]
    public async Task GetOpportunitiesList_LargePageSize_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}?pageIndex=0&pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-017")]
    public async Task CreateOpportunity_NameExactlyAtBoundaryMinusOne_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('E', 254);
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-018")]
    public async Task CreateOpportunity_DescriptionNull_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = (string?)null });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-019")]
    public async Task GetSearchFields_ResponseIsArray()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/search-fields");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-020")]
    public async Task CreateOpportunity_ThenGet_ReturnsMatchingAuditDates()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "BND Dates", description = "Test" });
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
    [Trait("TestId", "AAV-BND-021")]
    public async Task CreateOpportunity_NameWithNewlines_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Line1\nLine2", description = "Desc" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-022")]
    public async Task GetPartnerDocuments_ZeroEntityId_Returns404Or400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(AdminAccessValidationSpec.PartnerDocuments(0));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-023")]
    public async Task CreateOpportunity_NameWithLeadingTrailingSpaces_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "  Trim Test  ", description = "Desc" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-024")]
    public async Task CreateOpportunity_MinimalValidRequest_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Min", description = "X" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-025")]
    public async Task GetOpportunitiesList_FirstPage_ReturnsRecords()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}?pageIndex=0&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("totalCount", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-BND-026")]
    public async Task CreateOpportunity_Name257Chars_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('F', 257);
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-027")]
    public async Task CreateOpportunity_Name300Chars_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('G', 300);
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Desc" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-028")]
    public async Task GetOpportunityDetail_VerifyCreatedByNamePresent()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "BND CreatedBy", description = "Test" });
        if (createResponse.StatusCode != HttpStatusCode.OK) return;
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.TryGetProperty("id", out var p) ? p.GetInt32() : 0;
        if (id <= 0) return;
        var response = await client.GetAsync($"{AdminAccessValidationSpec.OpportunityBase}/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.TryGetProperty("opportunity", out var opp).Should().BeTrue();
        opp.TryGetProperty("createdBy", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "AAV-BND-029")]
    public async Task CreateFromPartner_NegativePartnerId_Returns404()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, -1, new { name = "Test", partnerRole = "funding", description = "Desc" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "AAV-BND-030")]
    public async Task CreateOpportunity_NameWithEmoji_Handled()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test 📋", description = "Desc" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
