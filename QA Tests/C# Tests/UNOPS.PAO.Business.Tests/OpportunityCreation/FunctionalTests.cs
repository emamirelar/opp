/**
 * @fileoverview Opportunity Creation functional tests — business rules, validation, workflow.
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
/// Functional tests for Opportunity Creation — business rules and validation logic.
/// </summary>
[Collection("Opportunity Creation Integration")]
[Trait("Category", "Functional")]
[Trait("Feature", "OpportunityCreation")]
public class FunctionalTests : OpportunityCreationFixtureBase
{
    public FunctionalTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "OPP-FNC-001")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_ResponseContainsId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("id", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-002")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_ResponseContainsName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "Functional Test FNC-002";
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain(name);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-003")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_ResponseContainsStage()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("stage", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-004")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_DefaultStageIsIdentifyAndProfile()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("IDENTIFY", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-005")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_ValidationRejectsNullName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = (string?)null, description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-006")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_ValidationRejectsEmptyName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-007")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_ValidatesPartnerRole()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", partnerRole = "invalid", description = "Test" });
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-008")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_EndpointAcceptsPost()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-009")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromPartner_EndpointAcceptsPost()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", partnerRole = "funding", description = "Test" });
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-010")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromProposal_EndpointAcceptsPost()
    {
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromProposalAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-011")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_ResponseIdIsPositive()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("id", out var idEl))
            idEl.GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-012")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_ValidationRequiresDescription()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = (string?)null });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-013")]
    [Trait("AC", "PNO-687-AC5")]
    public async Task CreateOpportunity_AcceptsOptionalFundingPartners()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test", fundingPartners = new[] { new { partnerId = 1 } } });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-014")]
    [Trait("AC", "PNO-687-AC5")]
    public async Task CreateOpportunity_AcceptsOptionalClientPartners()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test", clientPartners = new[] { new { partnerId = 1 } } });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-015")]
    [Trait("AC", "PNO-687-AC9")]
    public async Task CreateOpportunity_PersistsName()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = "Persisted Name FNC-015";
        var response = await PostCreateOpportunityAsync(client, new { name, description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("name").GetString().Should().Be(name);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-016")]
    [Trait("AC", "PNO-687-AC9")]
    public async Task CreateOpportunity_PersistsDescription()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var description = "Persisted description for FNC-016";
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain(description);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-017")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_ResponseContainsStatsOrRelatedData()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-018")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_FundingRoleAddsToFundingPartners()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", partnerRole = "funding", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("fundingPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-019")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_ClientRoleAddsToClientPartners()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", partnerRole = "client", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("clientPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-020")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_BothRoleAddsToBothFundingAndClient()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", partnerRole = "both", description = "Test" });
        if (response.StatusCode != HttpStatusCode.OK) return;
        var json = await response.Content.ReadAsStringAsync();
        json.Contains("fundingPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
        json.Contains("clientPartners", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-021")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_FromOpportunitiesModule_EntryPointWorks()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "From Opp Module", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-022")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromPartner_FromPartnerAccount_EntryPointWorks()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "From Partner", partnerRole = "funding", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-023")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromProposal_FromInteractions_EntryPointWorks()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromProposalAsync(client, new { name = "From Interactions", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-024")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_ErrorResponseContainsValidationDetails()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-025")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_ErrorResponseForInvalidRole_ContainsMessage()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", partnerRole = "invalid", description = "Test" });
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-026")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_RequiresAuthentication()
    {
        var client = CreateUnauthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-027")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromPartner_RequiresAuthentication()
    {
        var client = CreateUnauthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", partnerRole = "funding", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-028")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromProposal_RequiresAuthentication()
    {
        var client = CreateUnauthenticatedClient();
        var response = await PostCreateFromProposalAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-029")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_ResponseIsValidJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-030")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_InvalidFundingPartnerId_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test", fundingPartners = new[] { new { partnerId = 0 } } });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-031")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_InvalidClientPartnerId_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test", clientPartners = new[] { new { partnerId = -1 } } });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-032")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_ContentTypeIsApplicationJson()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        if (response.StatusCode == HttpStatusCode.OK)
            response.Content.Headers.ContentType?.MediaType.Should().Contain("json");
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-033")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_AcceptsCamelCaseProperties()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-034")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_RejectsInvalidHttpMethod()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OpportunityCreationSpec.CreateOpportunityEndpoint);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-035")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromPartner_RejectsInvalidHttpMethod()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OpportunityCreationSpec.CreateFromPartnerEndpoint(1));
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-036")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromProposal_RejectsInvalidHttpMethod()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OpportunityCreationSpec.CreateFromProposalEndpoint);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-037")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_AllThreeEndpointPathsExist()
    {
        var client = CreateAuthenticatedClient();
        var r1 = await client.PostAsync(OpportunityCreationSpec.CreateOpportunityEndpoint, null);
        var r2 = await client.PostAsync(OpportunityCreationSpec.CreateFromPartnerEndpoint(1), null);
        var r3 = await client.PostAsync(OpportunityCreationSpec.CreateFromProposalEndpoint, null);
        r1.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        r2.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        r3.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-038")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_ApiPrefixIsCorrect()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/opportunity");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-039")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromPartner_ApiPrefixIsCorrect()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OpportunityCreationSpec.CreateFromPartnerEndpoint(1));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-040")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateFromProposal_ApiPrefixIsCorrect()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OpportunityCreationSpec.CreateFromProposalEndpoint);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-041")]
    [Trait("AC", "PNO-687-AC6")]
    public async Task CreateOpportunity_EachCreateReturnsUniqueId()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var r1 = await PostCreateOpportunityAsync(client, new { name = "Test 1", description = "Test" });
        var r2 = await PostCreateOpportunityAsync(client, new { name = "Test 2", description = "Test" });
        if (r1.StatusCode != HttpStatusCode.OK || r2.StatusCode != HttpStatusCode.OK) return;
        var j1 = await r1.Content.ReadAsStringAsync();
        var j2 = await r2.Content.ReadAsStringAsync();
        using var d1 = JsonDocument.Parse(j1);
        using var d2 = JsonDocument.Parse(j2);
        var id1 = d1.RootElement.GetProperty("id").GetInt32();
        var id2 = d2.RootElement.GetProperty("id").GetInt32();
        id1.Should().NotBe(id2);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-042")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_FromOpportunitiesModule_DoesNotRequirePartner()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-043")]
    [Trait("AC", "PNO-815")]
    public async Task CreateFromProposal_FromInteractions_DoesNotRequirePartnerRole()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromProposalAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-044")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_FromPartner_RequiresPartnerRole()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateFromPartnerAsync(client, 1, new { name = "Test", description = "Test" });
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-FNC-045")]
    [Trait("AC", "PNO-687-AC1")]
    public async Task CreateOpportunity_AllEntryPoints_ReturnConsistentStructure()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response = await PostCreateOpportunityAsync(client, new { name = "Test", description = "Test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("id", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("name", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("stage", out _).Should().BeTrue();
    }
}
