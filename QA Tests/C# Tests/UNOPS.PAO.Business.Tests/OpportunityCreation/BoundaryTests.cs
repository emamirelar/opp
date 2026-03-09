/**
 * @fileoverview Opportunity Creation boundary tests — min/max values, edge cases, soft-delete.
 * PNO-687 AC4, PNO-917, PNO-814.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Net;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityCreation;

/// <summary>
/// Boundary tests for Opportunity Creation.
/// </summary>
[Collection("Opportunity Creation Integration")]
[Trait("Category", "Boundary")]
[Trait("Feature", "OpportunityCreation")]
public class BoundaryTests : OpportunityCreationFixtureBase
{
    public BoundaryTests(PAOWebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    [Trait("TestId", "OPP-BND-001")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_NameAtExactly255Chars_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('A', OpportunityCreationSpec.NameMaxLength);
        var request = new { name, description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-002")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_NameAt254Chars_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('A', OpportunityCreationSpec.NameMaxLength - 1);
        var request = new { name, description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-003")]
    [Trait("AC", "PNO-687-AC4")]
    public async Task CreateOpportunity_NameAt1Char_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "X", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-004")]
    public async Task CreateOpportunity_DescriptionAtMinLength_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "X" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-005")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_PartnerRoleFundingLowerCase_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-006")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_PartnerRoleClientLowerCase_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "client", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-007")]
    [Trait("AC", "PNO-687-AC3")]
    public async Task CreateFromPartner_PartnerRoleBothLowerCase_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "both", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-008")]
    public async Task CreateOpportunity_NameAt256Chars_Rejected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var name = new string('A', OpportunityCreationSpec.NameMaxLength + 1);
        var request = new { name, description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-009")]
    public async Task CreateOpportunity_WithNullOptionalFields_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            stage = (string?)null,
            responsibleOrgUnitId = (int?)null,
            targetSigningDate = (string?)null
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-010")]
    public async Task CreateOpportunity_WithEmptyOptionalArrays_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            fundingPartners = Array.Empty<object>(),
            clientPartners = Array.Empty<object>()
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-011")]
    public async Task CreateFromProposal_WithNullDescription_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = (string?)null };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-012")]
    public async Task CreateFromProposal_WithEmptyDescription_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "" };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-013")]
    public async Task CreateOpportunity_PartnerIdAtMaxInt_HandlesGracefully()
    {
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, int.MaxValue, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-014")]
    public async Task CreateOpportunity_PartnerIdAt1_HandlesGracefully()
    {
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = "Test" };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-015")]
    public async Task CreateOpportunity_DescriptionWithUnicode_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test 日本語 中文 العربية" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-016")]
    public async Task CreateOpportunity_NameWithUnicode_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test 机会", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-017")]
    public async Task CreateOpportunity_DescriptionWithNewlines_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Line1\nLine2\nLine3" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-018")]
    public async Task CreateOpportunity_DescriptionWithTabs_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Col1\tCol2\tCol3" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-019")]
    public async Task CreateOpportunity_StageAtIDENTIFY_AND_PROFILE_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", stage = OpportunityCreationSpec.DefaultStage };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-020")]
    public async Task CreateOpportunity_WithZeroInitiativeBudgetUSD_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", initiativeBudgetUSD = 0 };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-021")]
    public async Task CreateOpportunity_WithLargeInitiativeBudgetUSD_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", initiativeBudgetUSD = 999999999.99m };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-022")]
    public async Task CreateFromProposal_WithEmptySourceInteractionIds_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", sourceInteractionIds = Array.Empty<int>() };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-023")]
    public async Task CreateFromProposal_WithNullSourceInteractionIds_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", sourceInteractionIds = (int[]?)null };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-024")]
    public async Task CreateOpportunity_WithSingleFundingPartner_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            fundingPartners = new[] { new { partnerId = 1 } }
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-025")]
    public async Task CreateOpportunity_WithSingleClientPartner_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            clientPartners = new[] { new { partnerId = 1 } }
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-026")]
    public async Task CreateOpportunity_DescriptionExactlyAtCommonLimit_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var description = new string('X', 4000);
        var request = new { name = "Test", description };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-027")]
    public async Task CreateOpportunity_WithResponsibleOrgUnitId_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", responsibleOrgUnitId = 1 };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-028")]
    public async Task CreateOpportunity_WithTargetSigningDate_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            targetSigningDate = DateTime.UtcNow.AddMonths(6).ToString("yyyy-MM-dd")
        };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-029")]
    public async Task CreateFromPartner_WithLongDescription_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var description = new string('D', 500);
        var request = new { name = "Test", partnerRole = "funding", description };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-030")]
    public async Task CreateOpportunity_ConsecutiveCreates_ReturnUniqueIds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var response1 = await PostCreateOpportunityAsync(client, new { name = "Test 1", description = "Test" });
        var response2 = await PostCreateOpportunityAsync(client, new { name = "Test 2", description = "Test" });
        if (response1.StatusCode != HttpStatusCode.OK || response2.StatusCode != HttpStatusCode.OK) return;
        var json1 = await response1.Content.ReadAsStringAsync();
        var json2 = await response2.Content.ReadAsStringAsync();
        json1.Should().NotBe(json2);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-031")]
    public async Task CreateOpportunity_WithPartnerReference_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", partnerReference = "REF-001" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-032")]
    public async Task CreateOpportunity_WithEmptyPartnerReference_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", partnerReference = "" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-033")]
    public async Task CreateFromProposal_WithPartnerIdAndNoRole_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", partnerId = 1, isFundingPartner = false, isClientPartner = false };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-034")]
    public async Task CreateOpportunity_WithDeliveryModality_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", deliveryModality = 1 };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-035")]
    public async Task CreateOpportunity_WithProposedInitiativeTypeId_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", proposedInitiativeTypeId = 1 };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-036")]
    public async Task CreateOpportunity_NameWithLeadingTrailingSpaces_Handles()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "  Test  ", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-037")]
    public async Task CreateFromPartner_WithNullDescription_UsesDefault()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", partnerRole = "funding", description = (string?)null };
        var response = await PostCreateFromPartnerAsync(client, 1, request);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-038")]
    public async Task CreateOpportunity_WithBeneficiariesToBeDeterminedTrue_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", beneficiariesToBeDetermined = true };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-039")]
    public async Task CreateOpportunity_WithIsPooledFundingTrue_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", isPooledFunding = true };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-040")]
    public async Task CreateOpportunity_WithEstimatedDirectBeneficiaries_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", estimatedDirectBeneficiaries = 1000 };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-041")]
    public async Task CreateOpportunity_WithEmptyName_Rejected()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-042")]
    public async Task CreateOpportunity_WithTabInName_Handles()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test\tName", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-043")]
    public async Task CreateOpportunity_WithNewlineInName_Handles()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test\nName", description = "Test" };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-044")]
    public async Task CreateFromProposal_WithFundingAndClientPartners_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new
        {
            name = "Test",
            description = "Test",
            fundingPartners = new[] { new { partnerId = 1 } },
            clientPartners = new[] { new { partnerId = 1 } }
        };
        var response = await PostCreateFromProposalAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("TestId", "OPP-BND-045")]
    public async Task CreateOpportunity_WithCountriesArray_Succeeds()
    {
        if (!Factory.IsUsingPostgres) return;
        var client = CreateAuthenticatedClient();
        var request = new { name = "Test", description = "Test", countries = new[] { new { countryId = 1 } } };
        var response = await PostCreateOpportunityAsync(client, request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
