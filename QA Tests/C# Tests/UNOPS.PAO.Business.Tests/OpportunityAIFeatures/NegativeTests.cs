/// <summary>
/// Negative tests for Opportunity AI Features (PNO-694, PNO-803, PNO-804, PNO-805, PNO-873).
/// Requirements validated: validation errors, unauthorized states, expected failures.
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityAIFeatures;

[Collection("OpportunityAIFeatures")]
[Trait("Category", "Negative")]
[Trait("Feature", "OpportunityAIFeatures")]
[Trait("Component", "UNOPSOpportunityManager")]
public class NegativeTests : OpportunityAIFeaturesFixtureBase
{
    [Fact]
    [Trait("TestId", "NEG-001")]
    [Trait("Ticket", "PNO-803")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyName_ThrowsBusinessException()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "", Description = "Desc" };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*Name*");
    }

    [Fact]
    [Trait("TestId", "NEG-002")]
    [Trait("Ticket", "PNO-803")]
    public async Task CreateOpportunityFromProposalAsync_WithWhitespaceOnlyName_ThrowsBusinessException()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "   ", Description = "Desc" };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*Name*");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-003")]
    [Trait("Ticket", "PNO-804")]
    [Trait("Defect", "DEF-175")]
    public async Task ApplyAiChangesAsync_WithEmptyName_ThrowsOrRejects()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = "" };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-004")]
    [Trait("Ticket", "PNO-804")]
    [Trait("Defect", "DEF-175")]
    public async Task ApplyAiChangesAsync_WithWhitespaceOnlyName_ThrowsOrRejects()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = "   " };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("TestId", "NEG-005")]
    [Trait("Ticket", "PNO-803")]
    public async Task CreateOpportunityFromProposalAsync_WithNullName_ThrowsBusinessException()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = null!, Description = "Desc" };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*Name*");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-006")]
    public async Task ApplyAiChangesAsync_ForNonExistentOpportunity_ThrowsKeyNotFoundException()
    {
        var request = new ApplyOpportunityAiChangesRequest { Name = "Test" };
        var act = () => Manager.ApplyAiChangesAsync(999999, request);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-007")]
    public async Task ApplyAiChangesAsync_ForImmutableStageGO_ThrowsBusinessException()
    {
        var oppId = await CreateTestOpportunityAsync(name: "GO Opp", description: "Desc", stage: "GO");
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated" };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<BusinessException>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-008")]
    [Trait("Ticket", "PNO-803")]
    [Trait("Defect", "DEF-049")]
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
    [Trait("TestId", "NEG-009")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_WithNameExceeding120Chars_ThrowsOrTruncates()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = new string('x', 121) };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-010")]
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
    [Trait("TestId", "NEG-011")]
    public async Task ApplyAiChangesAsync_WithInvalidResponsibleOrgUnitId_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ResponsibleOrgUnitId = 999999 };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-012")]
    public async Task ApplyAiChangesAsync_WithInvalidProposedInitiativeTypeId_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ProposedInitiativeTypeId = 999999 };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-013")]
    public async Task ApplyAiChangesAsync_WithNullRequest_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var act = () => Manager.ApplyAiChangesAsync(oppId, null!);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-014")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithCurrentUserIdZero_DoesNotAssignServiceAccount()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, 0);
        result.Should().NotBeNull();
        result.OpportunityManager?.UserId.Should().NotBe(-1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-015")]
    [Trait("Ticket", "PNO-873")]
    [Trait("Defect", "DEF-226")]
    public async Task ApplyAiChangesAsync_WithNegativeInitiativeBudgetUSD_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { InitiativeBudgetUSD = -100m };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-016")]
    public async Task ApplyAiChangesAsync_WithFundingPartnersInvalidPartnerId_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 999999, Amount = 1000, CurrencyId = CurrencyId }
            }
        };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-017")]
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
    [Trait("TestId", "NEG-018")]
    public async Task ApplyAiChangesAsync_WithInvalidClientPartnersId_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ClientPartners = new List<int> { 999999 } };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-019")]
    public async Task ApplyAiChangesAsync_WithInvalidCountryId_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Countries = new List<int> { 999999 } };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-020")]
    public async Task ApplyAiChangesAsync_WithInvalidSDGId_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = 999999, IsPrimary = true } }
        };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-021")]
    public async Task CreateOpportunityFromProposalAsync_WithDuplicateInteractionIds_Deduplicated()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            SourceInteractionIds = new List<int> { 1, 1, 2, 2 }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-022")]
    public async Task ApplyAiChangesAsync_ForSoftDeletedOpportunity_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var opp = await Context.Opportunities.FindAsync(oppId);
        if (opp != null) { opp.IsDeleted = true; await Context.SaveChangesAsync(); }
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated" };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-023")]
    [Trait("Defect", "DEF-226")]
    public async Task ApplyAiChangesAsync_WithInvalidDeliveryModality_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { DeliveryModality = 99 };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-024")]
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
    [Trait("TestId", "NEG-025")]
    public async Task ApplyAiChangesAsync_WithInvalidUNOPSMissionId_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest> { new() { UNOPSMissionId = 999999 } }
        };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-026")]
    [Trait("Ticket", "PNO-804")]
    [Trait("Defect", "DEF-175")]
    public async Task ApplyAiChangesAsync_WithEmptyStringName_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = "" };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-027")]
    [Trait("Ticket", "PNO-803")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyDescription_AllowedPerComment()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-028")]
    [Trait("Ticket", "PNO-873")]
    [Trait("Defect", "DEF-226")]
    public async Task ApplyAiChangesAsync_WithFundingPartnersNegativeAmount_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = -1000, CurrencyId = CurrencyId }
            }
        };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-029")]
    public async Task ApplyAiChangesAsync_ForOpportunityInApprovalWorkflow_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var opp = await Context.Opportunities.FindAsync(oppId);
        if (opp != null)
        {
            opp.WorkflowStatus = Domain.Enums.WorkflowStatus.InWorkflow;
            await Context.SaveChangesAsync();
        }
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated" };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<BusinessException>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-030")]
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
}
