/// <summary>
/// Negative tests for Miscellaneous Fixes (PNO-805, PNO-801).
/// PNO-805: Service account (0, -1) must not become OM; invalid inputs rejected.
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MiscellaneousFixes;

[Collection("MiscellaneousFixes")]
[Trait("Category", "Negative")]
[Trait("Feature", "MiscellaneousFixes")]
[Trait("Component", "UNOPSOpportunityManager")]
public class NegativeTests : MiscellaneousFixesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-001")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithCurrentUserIdZero_DoesNotAssignServiceAccountAsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, 0);
        result.Should().NotBeNull();
        result.OpportunityManager.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-002")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithCurrentUserIdNegativeOne_DoesNotAssignServiceAccountAsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, -1);
        result.Should().NotBeNull();
        result.OpportunityManager.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-003")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyName_ThrowsBusinessException()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "", Description = "Desc" };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<UNOPS.PAO.Domain.Infrastructure.BusinessException>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-004")]
    public async Task CreateOpportunityFromProposalAsync_WithWhitespaceOnlyName_ThrowsBusinessException()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "   ", Description = "Desc" };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<UNOPS.PAO.Domain.Infrastructure.BusinessException>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-005")]
    public async Task CreateOpportunityFromProposalAsync_WithNullName_ThrowsBusinessException()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = null!, Description = "Desc" };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<UNOPS.PAO.Domain.Infrastructure.BusinessException>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-006")]
    public async Task CreateOpportunityFromProposalAsync_WithNameExceeding120Chars_Throws()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = new string('x', 121),
            Description = "Desc"
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-007")]
    public async Task AssignCreatorAsOpportunityManagerAsync_WithInvalidOpportunityId_Throws()
    {
        var act = () => Manager.AssignCreatorAsOpportunityManagerAsync(999999, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-008")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidPartnerId_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            PartnerId = 999999,
            IsFundingPartner = true
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-009")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidResponsibleOrgUnitId_Throws()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ResponsibleOrgUnitId = 999999
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-010")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidCurrencyId_Throws()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1000, CurrencyId = 999999 }
            }
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-011")]
    public async Task CreateOpportunityFromProposalAsync_WithNegativeUserId_DoesNotAssignAsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, -999);
        result.Should().NotBeNull();
        result.OpportunityManager.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-012")]
    public async Task CreateOpportunityFromProposalAsync_WithNonExistentUserId_DoesNotAssignAsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, 999999999);
        result.Should().NotBeNull();
        result.OpportunityManager.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-013")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidCountryId_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Countries = new List<int> { 999999 }
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-014")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidProposedInitiativeTypeId_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ProposedInitiativeTypeId = 999999
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-015")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidSDGId_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 999999 } }
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-016")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidEntityRoleIdInStakeholders_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Stakeholders = new List<OpportunityStakeholderRequest>
            {
                new() { UserId = PaoUserId, EntityRoleId = 999999 }
            }
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-017")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidDeliveryModality_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            DeliveryModality = 99
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-018")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidSourceInteractionIds_HandlesGracefully()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            SourceInteractionIds = new List<int> { 999999 }
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-019")]
    public async Task AssignCreatorAsOpportunityManagerAsync_WithZeroUserId_ThrowsOrHandles()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        var act = () => Manager.AssignCreatorAsOpportunityManagerAsync(oppId, 0);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-020")]
    public async Task AssignCreatorAsOpportunityManagerAsync_WithNegativeUserId_ThrowsOrHandles()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        var act = () => Manager.AssignCreatorAsOpportunityManagerAsync(oppId, -1);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-021")]
    public async Task CreateOpportunityFromProposalAsync_WithNullRequest_Throws()
    {
        var act = () => Manager.CreateOpportunityFromProposalAsync(null!, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-022")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidUNOPSMissionId_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest> { new() { UNOPSMissionId = 999999 } }
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-023")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidOutputIdInDeliverables_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = 999999 }
            }
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-024")]
    public async Task CreateOpportunityFromProposalAsync_WithNegativeInitiativeBudgetUSD_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            InitiativeBudgetUSD = -1000m
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-025")]
    public async Task CreateOpportunityFromProposalAsync_WithInvalidTargetSigningDate_ThrowsOrHandles()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            TargetSigningDate = DateTime.MinValue
        };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-026")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyFundingPartnerList_StillAssignsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            FundingPartners = new List<OpportunityFundingPartnerRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-027")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyClientPartnerList_StillAssignsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ClientPartners = new List<OpportunityClientPartnerRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-028")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyCountriesList_StillAssignsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Countries = new List<int>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-029")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyDeliverablesList_StillAssignsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Deliverables = new List<OpportunityDeliverableRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-030")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyStakeholdersList_StillAssignsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }
}
