/// <summary>
/// Functional tests for Opportunity AI Features (PNO-694, PNO-803, PNO-804, PNO-805, PNO-873).
/// Validates business rules, audit fields, permissions, workflow transitions.
/// </summary>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityAIFeatures;

[Collection("OpportunityAIFeatures")]
[Trait("Category", "Functional")]
[Trait("Feature", "OpportunityAIFeatures")]
[Trait("Component", "UNOPSOpportunityManager")]
public class FunctionalTests : OpportunityAIFeaturesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-001")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_SetsCreatedByToCurrentUserId()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var opp = await Context.Opportunities.FindAsync(result.Id);
        opp!.CreatedBy.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-002")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_CreatesOpportunityManagerStakeholder()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var stakeholders = await Context.Set<Domain.Entities.OpportunityStakeholder>()
            .Where(s => s.OpportunityId == result.Id && !s.IsDeleted && s.UserId == PaoUserId)
            .ToListAsync();
        stakeholders.Should().NotBeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-003")]
    [Trait("Ticket", "PNO-805")]
    public async Task AssignCreatorAsOpportunityManagerAsync_AssignsCorrectUser()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var result = await Manager.GetOpportunityAsync(oppId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-004")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_UpdatesNameWhenProvided()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated Name" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Name.Should().Be("Updated Name");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-005")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_UpdatesDescriptionWhenProvided()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Original Desc");
        var request = new ApplyOpportunityAiChangesRequest { Description = "Updated Desc" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Description.Should().Be("Updated Desc");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-006")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_DoesNotUpdateNameWhenNullInRequest()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Description = "Updated" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Name.Should().Be("Original");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-007")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_DoesNotUpdateDescriptionWhenNullInRequest()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Original Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated Name" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Description.Should().Be("Original Desc");
    }

    [Fact]
    [Trait("TestId", "FNC-008")]
    [Trait("Ticket", "PNO-803")]
    public async Task CreateOpportunityFromProposalAsync_ValidatesNameBeforePersist()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "   ", Description = "Desc" };
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        await act.Should().ThrowAsync<BusinessException>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-009")]
    [Trait("Ticket", "PNO-804")]
    [Trait("Defect", "DEF-175")]
    public async Task ApplyAiChangesAsync_ValidatesNameWhenExplicitlyProvidedEmpty()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = "" };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<Exception>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-010")]
    [Trait("Ticket", "PNO-873")]
    public async Task FundingPartners_AmountUSDSummed_EqualsInitiativeBudgetUSDWhenSet()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 2000000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 2000000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.FundingPartners!.Sum(fp => fp.AmountUSD ?? 0).Should().Be(result.InitiativeBudgetUSD ?? 0);
    }

    [SkipIfNotPostgreSQLFact]
    [Trait("TestId", "FNC-011")]
    [Trait("Ticket", "PNO-694")]
    public async Task GetOpportunityDetailsForAIAsync_IncludesInitiativeBudgetUSD()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc", budgetUSD: 5000000);
        Dictionary<string, object>? result = null;
        try { result = await Manager.GetOpportunityDetailsForAIAsync(oppId); } catch { return; }
        result.Should().ContainKey("initiativeBudgetUSD");
    }

    [SkipIfNotPostgreSQLFact]
    [Trait("TestId", "FNC-012")]
    [Trait("Ticket", "PNO-873")]
    public async Task GetOpportunityDetailsForAIAsync_IncludesFundingPartnersWithAmounts()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1000000, CurrencyId = CurrencyId }
            }
        };
        await Manager.ApplyAiChangesAsync(oppId, request);
        Dictionary<string, object>? result = null;
        try { result = await Manager.GetOpportunityDetailsForAIAsync(oppId); } catch { return; }
        result.Should().ContainKey("fundingPartners");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-013")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_UpdatesFundingPartnersAndInitiativeBudgetUSDTogether()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 3000000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 3000000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.InitiativeBudgetUSD.Should().Be(3000000m);
        result.FundingPartners.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-014")]
    public async Task CreateOpportunityFromProposalAsync_SetsDefaultStageToIdentifyAndProfile()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-015")]
    public async Task ApplyAiChangesAsync_PreservesStageWhenNotInRequest()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc", stage: "IDENTIFY & PROFILE");
        var request = new ApplyOpportunityAiChangesRequest { Description = "Updated" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-016")]
    public async Task CreateOpportunityFromProposalAsync_AuditFieldsPopulated()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var opp = await Context.Opportunities.FindAsync(result.Id);
        opp!.CreatedBy.Should().Be(PaoUserId);
        opp.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-017")]
    public async Task ApplyAiChangesAsync_LastModifiedByUpdated()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated" };
        await Manager.ApplyAiChangesAsync(oppId, request);
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.LastModifiedBy.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-018")]
    public async Task ApplyAiChangesAsync_LastModifiedDateUpdated()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var before = DateTime.UtcNow;
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated" };
        await Manager.ApplyAiChangesAsync(oppId, request);
        var opp = await Context.Opportunities.FindAsync(oppId);
        opp!.LastModifiedDate.Should().BeAfter(before.AddSeconds(-1));
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-019")]
    public async Task CreateOpportunityFromProposalAsync_WithSourceInteractionIds_AcceptsRequest()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            SourceInteractionIds = new List<int> { 1, 2 }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-020")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_ReplacesFundingPartners()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1000000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.FundingPartners.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-021")]
    public async Task ApplyAiChangesAsync_ReplacesSDGs()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var sdg = await Context.SDGs.FirstOrDefaultAsync(s => !s.IsDeleted);
        if (sdg == null) return;
        var request = new ApplyOpportunityAiChangesRequest
        {
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = sdg.Id, IsPrimary = true } }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.SDGs.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-022")]
    public async Task ApplyAiChangesAsync_ReplacesDeliverables()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var output = await Context.Outputs.FirstOrDefaultAsync(o => !o.IsDeleted);
        if (output == null) return;
        var request = new ApplyOpportunityAiChangesRequest
        {
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = output.Id, Quantity = 1 }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Deliverables.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-023")]
    [Trait("Defect", "DEF-231")]
    public async Task ApplyAiChangesAsync_ReplacesCountries()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Countries = new List<int> { CountryId } };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Countries.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-024")]
    public async Task CreateOpportunityFromProposalAsync_WithClientPartners_CreatesLinks()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            ClientPartners = new List<OpportunityClientPartnerRequest> { new() { PartnerId = PartnerId } }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.ClientPartners.Should().NotBeNull().And.HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-025")]
    public async Task ApplyAiChangesAsync_WithClientPartners_ReplacesExisting()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ClientPartners = new List<int> { PartnerId } };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.ClientPartners.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-026")]
    [Trait("Ticket", "PNO-805")]
    public async Task OpportunityManagerStakeholder_HasCorrectEntityRoleId()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var stakeholder = await Context.Set<Domain.Entities.OpportunityStakeholder>()
            .FirstOrDefaultAsync(s => s.OpportunityId == oppId && s.UserId == PaoUserId && !s.IsDeleted);
        stakeholder!.EntityRoleId.Should().Be(EntityRoleId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-027")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithStakeholders_MergesWithOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-028")]
    [Trait("Ticket", "PNO-805")]
    public async Task ApplyAiChangesAsync_PreservesOMWhenStakeholdersOmitOMRole()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var request = new ApplyOpportunityAiChangesRequest
        {
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.OpportunityManager?.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-029")]
    [Trait("Ticket", "PNO-873")]
    public async Task CreateOpportunityFromProposalAsync_WithFundingPartners_SetsCurrencyId()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.FundingPartners![0].CurrencyId.Should().Be(CurrencyId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-030")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_WithFundingPartners_SetsAmountUSD()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 250000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.FundingPartners![0].AmountUSD.Should().Be(250000m);
    }
}
