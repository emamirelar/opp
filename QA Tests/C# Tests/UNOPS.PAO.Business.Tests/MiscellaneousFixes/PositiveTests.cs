/// <summary>
/// Positive tests for Miscellaneous Fixes (PNO-805, PNO-801).
/// PNO-805: Opportunity Manager must be logged-in user when creating via AI.
/// PNO-801: Side panel Leads/Initiatives removed — validated via Playwright E2E.
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MiscellaneousFixes;

[Collection("MiscellaneousFixes")]
[Trait("Category", "Positive")]
[Trait("Feature", "MiscellaneousFixes")]
[Trait("Component", "UNOPSOpportunityManager")]
public class PositiveTests : MiscellaneousFixesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-001")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithLoggedInUserId_AssignsUserAsOpportunityManager()
    {
        // Arrange — PNO-805: OM must be logged-in user, not service account
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "AI Opportunity OM Test",
            Description = "Verify logged-in user becomes OM"
        };

        // Act
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);

        // Assert
        result.Should().NotBeNull();
        result.OpportunityManager.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-002")]
    [Trait("Ticket", "PNO-805")]
    public async Task AssignCreatorAsOpportunityManagerAsync_WithValidUserId_AssignsStakeholder()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Assign OM Test", description: "Desc");

        // Act
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var result = await Manager.GetOpportunityAsync(oppId);

        // Assert
        result.Should().NotBeNull();
        result!.OpportunityManager.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-003")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithValidNameAndDescription_Succeeds()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Valid AI Opportunity",
            Description = "Full description"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Valid AI Opportunity");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-004")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithFundingPartners_OMStillAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Funding OM Test",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 100000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-005")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithClientPartners_OMStillAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Client OM Test",
            ClientPartners = new List<OpportunityClientPartnerRequest> { new() { PartnerId = PartnerId } }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-006")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithResponsibleOrgUnit_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "OrgUnit OM Test",
            ResponsibleOrgUnitId = OrgHierarchyId
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-007")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithCountries_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Countries OM Test",
            Countries = new List<int> { CountryId }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-008")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithProposedInitiativeType_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "InitiativeType OM Test",
            ProposedInitiativeTypeId = ProposedInitiativeTypeId
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-009")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithOptionalDescription_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Minimal OM Test" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-010")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithPartnerIdAndRoles_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Partner OM Test",
            PartnerId = PartnerId,
            IsFundingPartner = true,
            IsClientPartner = false
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }
}
