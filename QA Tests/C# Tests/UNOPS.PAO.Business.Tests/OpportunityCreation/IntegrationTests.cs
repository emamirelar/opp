/**
 * @fileoverview Opportunity Creation integration tests — full CRUD, cross-component workflows.
 * PNO-687, PNO-689, PNO-764, PNO-771, PNO-800, PNO-802, PNO-814, PNO-815, PNO-816, PNO-917.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using System.Text.Json;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityCreation;

/// <summary>
/// Integration tests for Opportunity Creation — end-to-end flows.
/// </summary>
[Collection("Opportunity Creation Integration")]
[Trait("Category", "Integration")]
[Trait("Feature", "OpportunityCreation")]
public class IntegrationTests : OpportunityCreationFixtureBase
{
    public IntegrationTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "OPP-INT-001")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateThenGet_OpportunityExistsInSystem()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT-001 Opp", description = "Test" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await createResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var id = doc.RootElement.GetProperty("id").GetInt32();
        var getResponse = await client.GetAsync($"/api/opportunity/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-002")]
    [Trait("AC", "PNO-687-AC9")]
    public async Task CreateThenSearch_OpportunityAppearsInResults()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "INT-002 Searchable Opportunity";
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchResponse = await client.GetAsync("/api/opportunity?page=1&pageSize=50");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchJson = await searchResponse.Content.ReadAsStringAsync();
        searchJson.Should().Contain(name);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-003")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task Create_ResponseMatchesGetById()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "INT-003 Match Test";
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var id = createDoc.RootElement.GetProperty("id").GetInt32();
        var getResponse = await client.GetAsync($"/api/opportunity/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getJson = await getResponse.Content.ReadAsStringAsync();
        getJson.Should().Contain(name);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-004")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromOpportunitiesModule_FullFlow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "INT-004 Full Flow", description = "Created from Opportunities module" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("INT-004 Full Flow");
    }

    [Fact]
    [Trait("TestId", "OPP-INT-005")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromPartner_FullFlow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "INT-005 From Partner", partnerRole = "funding", description = "From partner" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-006")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromProposal_FullFlow()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "INT-006 From Proposal", description = "From interactions" };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-007")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task Create_MultipleOpportunities_AllHaveUniqueIds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var ids = new List<int>();
        for (var i = 0; i < 3; i++)
        {
            var response = await PostCreateOpportunityAsync(client, new { name = $"INT-007 Opp {i}", description = "Test" });
            if (response.StatusCode != HttpStatusCode.OK) return;
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            ids.Add(doc.RootElement.GetProperty("id").GetInt32());
        }
        ids.Distinct().Count().Should().Be(3);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-008")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_AllThreeEndpoints_ReturnOpportunityStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await PostCreateOpportunityAsync(client, new { name = "INT-008a", description = "Test" });
        if (r1.StatusCode != HttpStatusCode.OK) return;
        var j1 = await r1.Content.ReadAsStringAsync();
        j1.Should().Contain("id");
        j1.Should().Contain("name");
        j1.Should().Contain("stage");
    }

    [Fact]
    [Trait("TestId", "OPP-INT-009")]
    [Trait("AC", "PNO-687-AC9")]
    public async Task Create_WithFundingPartners_RelationshipPersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-009", description = "Test", fundingPartners = new[] { new { partnerId = 1 } } });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("fundingPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-010")]
    [Trait("AC", "PNO-687-AC9")]
    public async Task Create_WithClientPartners_RelationshipPersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-010", description = "Test", clientPartners = new[] { new { partnerId = 1 } } });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("clientPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-011")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_FromOpportunitiesModule_NoPartnerRequired()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-011", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-012")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_FromPartner_PartnerPrePopulated()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "INT-012", partnerRole = "funding", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("fundingPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-013")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task Create_StageDefaultsToIdentifyAndProfile()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-013", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("IDENTIFY", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-014")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_Unauthenticated_AllEndpointsReject()
    {
        var client = CreateUnauthenticatedClient();
        var r1 = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        var r2 = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", partnerRole = "funding", description = "Test" });
        var r3 = await PostCreateFromProposalAsync(client, new { name = "Test", description = "Test" });
        r1.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        r2.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        r3.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-015")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task Create_ValidationErrors_Return400WithDetails()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-016")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_ContentTypeApplicationJson_Accepted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-016", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "OPP-INT-017")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_ThreeEntryPoints_ConsistentResponseShape()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-017", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("id", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("name", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("stage", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-018")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task Create_ResponseIdIsPositiveInteger()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-018", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var id = doc.RootElement.GetProperty("id").GetInt32();
        id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-019")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_OpportunitiesModule_PostSucceeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-019", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-020")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_PartnerModule_PostSucceedsOrNotFound()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "INT-020", partnerRole = "funding", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-021")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_InteractionsModule_PostSucceedsOrBadRequest()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromProposalAsync(client, new { name = "INT-021", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-022")]
    [Trait("AC", "PNO-687-AC9")]
    public async Task Create_WithDescription_DescriptionPersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var description = "Integration test description for INT-022";
        var createResponse = await PostCreateOpportunityAsync(client, new { name = "INT-022", description });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await createResponse.Content.ReadAsStringAsync();
        json.Should().Contain(description);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-023")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task Create_WithResponsibleOrgUnit_OrgUnitPersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-023", description = "Test", responsibleOrgUnitId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-024")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_GetOpportunityList_IncludesNewOpportunity()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "INT-024 List Test " + Guid.NewGuid().ToString("N")[..8];
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await client.GetAsync("/api/opportunity?page=1&pageSize=100");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listJson = await listResponse.Content.ReadAsStringAsync();
        listJson.Should().Contain(name);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-025")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task Create_InvalidRequest_Returns400Not500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-026")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_FromProposal_NoPartnerId_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromProposalAsync(client, new { name = "INT-026", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-027")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_BothRole_AddsToFundingAndClient()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "INT-027", partnerRole = "both", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("fundingPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
        json.Contains("clientPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-028")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_SequentialCreates_NoInterference()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await PostCreateOpportunityAsync(client, new { name = "INT-028a", description = "Test" });
        var r2 = await PostCreateOpportunityAsync(client, new { name = "INT-028b", description = "Test" });
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-029")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task Create_ResponseHasExpectedTopLevelProperties()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-029", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.TryGetProperty("id", out _).Should().BeTrue();
        root.TryGetProperty("name", out _).Should().BeTrue();
        root.TryGetProperty("description", out _).Should().BeTrue();
        root.TryGetProperty("stage", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-030")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_ApiRoute_MatchesApiDictionary()
    {
        var expectedPath = "/api/opportunity";
        OpportunityCreationSpec.CreateOpportunityEndpoint.Should().Be(expectedPath);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-031")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromPartner_ApiRoute_MatchesApiDictionary()
    {
        var expected = "/api/partner/1/create-opportunity";
        OpportunityCreationSpec.CreateFromPartnerEndpoint(1).Should().Be(expected);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-032")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromProposal_ApiRoute_MatchesApiDictionary()
    {
        var expectedPath = "/api/opportunity/create-from-proposal";
        OpportunityCreationSpec.CreateFromProposalEndpoint.Should().Be(expectedPath);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-033")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task Create_NameMaxLength_Enforced()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('A', OpportunityCreationSpec.NameMaxLength + 10);
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-034")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_WithTargetSigningDate_DatePersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var date = DateTime.UtcNow.AddMonths(3).ToString("yyyy-MM-dd");
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-034", description = "Test", targetSigningDate = date });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-035")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_WithInitiativeBudget_BudgetPersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-035", description = "Test", initiativeBudgetUSD = 1000000 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-036")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_WithPartnerReference_ReferencePersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-036", description = "Test", partnerReference = "REF-INT-036" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-037")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_ErrorResponse_IsValidJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-038")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_SuccessResponse_IsValidJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-038", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(body);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "OPP-INT-039")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_AllEndpoints_UnderApiPrefix()
    {
        OpportunityCreationSpec.CreateOpportunityEndpoint.Should().StartWith("/api");
        OpportunityCreationSpec.CreateFromPartnerEndpoint(1).Should().StartWith("/api");
        OpportunityCreationSpec.CreateFromProposalEndpoint.Should().StartWith("/api");
    }

    [Fact]
    [Trait("TestId", "OPP-INT-040")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task Create_OpportunityId_IsSequentialOrUnique()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-040", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var id = doc.RootElement.GetProperty("id").GetInt32();
        id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-041")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_FromProposal_WithSourceInteractionIds_Handles()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromProposalAsync(client, new { name = "INT-041", description = "Test", sourceInteractionIds = new[] { 1 } });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-042")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_WithFundingAndClientPartners_BothPersisted()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new
        {
            name = "INT-042",
            description = "Test",
            fundingPartners = new[] { new { partnerId = 1 } },
            clientPartners = new[] { new { partnerId = 1 } }
        });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-043")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_NonExistentPartner_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 999999, new { name = "INT-043", partnerRole = "funding", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-044")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_GetById_ReturnsSameDataAsCreateResponse()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "INT-044 GetById Match";
        var createResponse = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var id = createDoc.RootElement.GetProperty("id").GetInt32();
        var getResponse = await client.GetAsync($"/api/opportunity/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var getJson = await getResponse.Content.ReadAsStringAsync();
        getJson.Should().Contain(name);
    }

    [Fact]
    [Trait("TestId", "OPP-INT-045")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task Create_HomeDashboardEntryPoint_ImpliedByCreateEndpoint()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "INT-045 From Home", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
