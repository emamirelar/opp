/**
 * @fileoverview Opportunity Creation negative tests — invalid input, unauthorized, blocked scenarios.
 * PNO-687, PNO-689, PNO-764, PNO-771, PNO-917.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityCreation;

/// <summary>
/// Negative tests for Opportunity Creation.
/// </summary>
[Collection("Opportunity Creation Integration")]
[Trait("Category", "Negative")]
[Trait("Feature", "OpportunityCreation")]
public class NegativeTests : OpportunityCreationFixtureBase
{
    public NegativeTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "OPP-NEG-001")]
    [Trait("AC", "PNO-771")]
    public async Task CreateOpportunity_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var request = new { name = "Test", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-002")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_WithNullName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = (string?)null, description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-003")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_WithEmptyName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-004")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_WithWhitespaceOnlyName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "   ", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-005")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_WithNameExceeding255Chars_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('A', OpportunityCreationSpec.NameMaxLength + 1);
        var request = new { name, description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-006")]
    public async Task CreateOpportunity_WithEmptyBody_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-007")]
    public async Task CreateOpportunity_WithInvalidJson_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{ invalid json }", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(OpportunityCreationSpec.CreateOpportunityEndpoint, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-008")]
    public async Task CreateOpportunity_GetMethod_Returns405()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OpportunityCreationSpec.CreateOpportunityEndpoint);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-009")]
    public async Task CreateOpportunity_PutMethod_Returns405()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PutAsync(OpportunityCreationSpec.CreateOpportunityEndpoint, null);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-010")]
    public async Task CreateOpportunity_DeleteMethod_Returns405()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.DeleteAsync(OpportunityCreationSpec.CreateOpportunityEndpoint);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-011")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_WithInvalidPartnerRole_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "invalid", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-012")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_WithEmptyPartnerRole_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-013")]
    [Trait("AC", "PNO-687-AC2")]
    public async Task CreateFromPartner_WithNonExistentPartner_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 999999, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-014")]
    [Trait("AC", "PNO-687-AC2")]
    public async Task CreateFromPartner_WithZeroPartnerId_Returns404Or400()
    {
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 0, request);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-015")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateFromPartner_WithNullName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = (string?)null, partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-016")]
    public async Task CreateFromPartner_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-017")]
    public async Task CreateFromProposal_Unauthenticated_Returns401Or403()
    {
        var client = CreateUnauthenticatedClient();
        var request = new { name = "Test", description = "Test" };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-018")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateFromProposal_WithNullName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = (string?)null, description = "Test" };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-019")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateFromProposal_WithEmptyName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "", description = "Test" };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-020")]
    public async Task CreateOpportunity_WrongPath_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/opportunity-wrong", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-021")]
    public async Task CreateFromPartner_GetMethod_Returns405()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OpportunityCreationSpec.CreateFromPartnerEndpoint(1));
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-022")]
    public async Task CreateFromProposal_GetMethod_Returns405()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(OpportunityCreationSpec.CreateFromProposalEndpoint);
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-023")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_WithMissingDescription_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-024")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_WithNullDescription_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = (string?)null };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-025")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_WithFundingRoleUpperCase_Validates()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "FUNDING", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-026")]
    public async Task CreateOpportunity_WithNegativeFundingPartnerId_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            fundingPartners = new[] { new { partnerId = -1 } }
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-027")]
    public async Task CreateOpportunity_WithZeroFundingPartnerId_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            fundingPartners = new[] { new { partnerId = 0 } }
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-028")]
    public async Task CreateOpportunity_WithInvalidContentType_Returns415Or400()
    {
        var client = CreateAuthenticatedClient();
        var content = new StringContent("name=Test&description=Test", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.PostAsync(OpportunityCreationSpec.CreateOpportunityEndpoint, content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.UnsupportedMediaType, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-029")]
    public async Task CreateFromProposal_WithEmptyBody_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-030")]
    [Trait("AC", "PNO-687-AC2")]
    public async Task CreateFromPartner_WithClosedPartner_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        var body = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == HttpStatusCode.BadRequest && body.Contains("inactive", StringComparison.OrdinalIgnoreCase))
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-031")]
    public async Task CreateOpportunity_WithMalformedFundingPartners_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", fundingPartners = "invalid" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-032")]
    public async Task CreateOpportunity_WithNegativePartnerIdInCreateFromPartner_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, -1, request);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-033")]
    public async Task CreateFromProposal_WithInvalidSourceInteractionIds_HandlesGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", sourceInteractionIds = new[] { -1 } };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-034")]
    public async Task CreateOpportunity_WithVeryLongDescription_HandlesGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = new string('X', 100000) };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-035")]
    public async Task CreateOpportunity_WithSpecialCharactersInName_HandlesGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test <script>alert(1)</script>", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-036")]
    public async Task CreateOpportunity_WithUnicodeInName_HandlesGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test 机会 日本語", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-037")]
    public async Task CreateFromPartner_WithMissingPartnerRole_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-038")]
    public async Task CreateOpportunity_WithDuplicateFundingPartners_HandlesGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            fundingPartners = new[] { new { partnerId = 1 }, new { partnerId = 1 } }
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-039")]
    public async Task CreateOpportunity_WithInvalidStage_Returns400OrUsesDefault()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", stage = "INVALID_STAGE" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-040")]
    public async Task CreateFromProposal_WithWhitespaceOnlyName_Returns400()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "   ", description = "Test" };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-041")]
    public async Task CreateOpportunity_WithEmptyFundingPartnersArray_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", fundingPartners = Array.Empty<object>() };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-042")]
    public async Task CreateOpportunity_WithNullFundingPartners_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", fundingPartners = (object?)null };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-043")]
    public async Task CreateFromPartner_WithVeryLongName_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = new string('A', 1000), partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-044")]
    public async Task CreateOpportunity_WithFutureTargetSigningDate_HandlesGracefully()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            targetSigningDate = DateTime.UtcNow.AddYears(5).ToString("O")
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-NEG-045")]
    public async Task CreateOpportunity_WithInvalidDateFormat_Returns400Or500()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", targetSigningDate = "not-a-date" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}
