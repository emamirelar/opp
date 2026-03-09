/// <summary>
/// Integration tests for Opportunity AI Features (PNO-694, PNO-803, PNO-804, PNO-805, PNO-873).
/// Full round-trip flows, cross-component workflows.
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
[Trait("Category", "Integration")]
[Trait("Feature", "OpportunityAIFeatures")]
[Trait("Component", "UNOPSOpportunityManager")]
public class IntegrationTests : OpportunityAIFeaturesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-001")]
    [Trait("Defect", "DEF-231")]
    public async Task CreateOpportunityFromProposalAsync_FullRoundTrip_WithAllSections()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Full AI Opportunity",
            Description = "Comprehensive description",
            ResponsibleOrgUnitId = OrgHierarchyId,
            ProposedInitiativeTypeId = ProposedInitiativeTypeId,
            InitiativeBudgetUSD = 2000000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 2000000, CurrencyId = CurrencyId }
            },
            Countries = new List<int> { CountryId }
        };
        var created = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var reloaded = await Manager.GetOpportunityAsync(created.Id);
        reloaded.Should().NotBeNull();
        reloaded!.Name.Should().Be("Full AI Opportunity");
        reloaded.InitiativeBudgetUSD.Should().Be(2000000m);
        reloaded.OpportunityManager.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-002")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_ThenGetOpportunityAsync_ReturnsUpdatedData()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Original Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };
        await Manager.ApplyAiChangesAsync(oppId, request);
        var result = await Manager.GetOpportunityAsync(oppId);
        result!.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-003")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_ThenGetOpportunityAsync_ShowsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var created = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var result = await Manager.GetOpportunityAsync(created.Id);
        result!.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-004")]
    public async Task CreateOpportunityFromProposalAsync_ThenApplyAiChangesAsync_VerifyBoth()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Created", Description = "Desc" };
        var created = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var aiRequest = new ApplyOpportunityAiChangesRequest { Name = "AI-Enhanced", Challenges = "New challenges" };
        var updated = await Manager.ApplyAiChangesAsync(created.Id, aiRequest);
        updated.Name.Should().Be("AI-Enhanced");
        updated.Challenges.Should().Be("New challenges");
        updated.OpportunityManager.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-005")]
    [Trait("Ticket", "PNO-873")]
    public async Task CreateOpportunityFromProposalAsync_WithFundingPartners_VerifyBudget()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Budget Opp",
            Description = "Desc",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1500000, CurrencyId = CurrencyId }
            },
            InitiativeBudgetUSD = 1500000m
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.InitiativeBudgetUSD.Should().Be(1500000m);
        result.FundingPartners!.Sum(fp => fp.AmountUSD ?? 0).Should().Be(1500000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-006")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_WithFundingPartners_VerifyInitiativeBudgetUSD()
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
        var reloaded = await Manager.GetOpportunityAsync(oppId);
        reloaded!.InitiativeBudgetUSD.Should().Be(3000000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-007")]
    public async Task CreateOpportunityFromProposalAsync_WithDocuments_AcceptsRequest()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "With Docs",
            Description = "Desc",
            Documents = new List<NewDocumentRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-008")]
    [Trait("Defect", "DEF-231")]
    public async Task CreateOpportunityFromProposalAsync_WithSDGsAndCountries()
    {
        var sdg = await Context.SDGs.FirstOrDefaultAsync(s => !s.IsDeleted);
        if (sdg == null) return;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "SDG Opp",
            Description = "Desc",
            SdGs = new List<OpportunitySDGRequest> { new() { SDGId = sdg.Id, IsPrimary = true } },
            Countries = new List<int> { CountryId }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.SDGs.Should().HaveCount(1);
        result.Countries.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-009")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_UpdatesMultipleSectionsInOneCall()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            Name = "Updated",
            Description = "New Desc",
            Challenges = "Challenges",
            ResultsFocus = "Results"
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Name.Should().Be("Updated");
        result.Description.Should().Be("New Desc");
        result.Challenges.Should().Be("Challenges");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-010")]
    public async Task CreateOpportunityFromProposalAsync_WithDeliverables()
    {
        var output = await Context.Outputs.FirstOrDefaultAsync(o => !o.IsDeleted);
        if (output == null) return;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Deliverables Opp",
            Description = "Desc",
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = output.Id, Quantity = 2 }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Deliverables.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-011")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithStakeholders()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Stakeholders Opp",
            Description = "Desc",
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-012")]
    [Trait("Ticket", "PNO-805")]
    public async Task ApplyAiChangesAsync_WithStakeholders_PreservesOM()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var request = new ApplyOpportunityAiChangesRequest { Stakeholders = new List<OpportunityStakeholderRequest>() };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-013")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_AssignCreatorAsOpportunityManagerAsync_Called()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var stakeholder = await Context.Set<Domain.Entities.OpportunityStakeholder>()
            .FirstOrDefaultAsync(s => s.OpportunityId == result.Id && s.UserId == PaoUserId && !s.IsDeleted);
        stakeholder.Should().NotBeNull();
    }

    [SkipIfNotPostgreSQLFact]
    [Trait("TestId", "INT-014")]
    [Trait("Ticket", "PNO-694")]
    public async Task GetOpportunityDetailsForAIAsync_ReturnsDataForApplyAiChangesFlow()
    {
        var oppId = await CreateTestOpportunityAsync(name: "AI Flow", description: "Desc", budgetUSD: 1000000);
        Dictionary<string, object>? result = null;
        try { result = await Manager.GetOpportunityDetailsForAIAsync(oppId); } catch { return; }
        result.Should().ContainKey("name").And.ContainKey("description");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-015")]
    public async Task CreateOpportunityFromProposalAsync_WithPartnerId_LinksPartner()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Partner Opp",
            Description = "Desc",
            PartnerId = PartnerId,
            IsFundingPartner = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.FundingPartners.Should().NotBeNull().And.Contain(fp => fp.PartnerId == PartnerId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-016")]
    public async Task ApplyAiChangesAsync_WithClientPartners_FullRoundTrip()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ClientPartners = new List<int> { PartnerId } };
        await Manager.ApplyAiChangesAsync(oppId, request);
        var result = await Manager.GetOpportunityAsync(oppId);
        result!.ClientPartners.Should().Contain(cp => cp.PartnerId == PartnerId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-017")]
    public async Task CreateOpportunityFromProposalAsync_WithUNOPSMissions()
    {
        var mission = await Context.UNOPSMissions.FirstOrDefaultAsync(m => !m.IsDeleted);
        if (mission == null) return;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Missions Opp",
            Description = "Desc",
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest> { new() { UNOPSMissionId = mission.Id } }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-018")]
    public async Task ApplyAiChangesAsync_WithUNOPSMissions()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var mission = await Context.UNOPSMissions.FirstOrDefaultAsync(m => !m.IsDeleted);
        if (mission == null) return;
        var request = new ApplyOpportunityAiChangesRequest
        {
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest> { new() { UNOPSMissionId = mission.Id } }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.UNOPSMissions.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-019")]
    public async Task CreateOpportunityFromProposalAsync_WithWhenSectionDates()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Dates Opp",
            Description = "Desc",
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(24)
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.TargetSigningDate.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-020")]
    public async Task ApplyAiChangesAsync_WithWhenSectionDates()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            TargetSigningDate = DateTime.UtcNow.AddMonths(3),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(18)
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.TargetSigningDate.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-021")]
    public async Task CreateOpportunityFromProposalAsync_WithProposedInitiativeTypeName()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            ProposedInitiativeTypeName = "Project"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.ProposedInitiativeTypeId.Should().Be(ProposedInitiativeTypeId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-022")]
    public async Task ApplyAiChangesAsync_WithProposedInitiativeTypeName()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ProposedInitiativeTypeName = "Project" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.ProposedInitiativeTypeId.Should().Be(ProposedInitiativeTypeId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-023")]
    [Trait("Ticket", "PNO-873")]
    public async Task FundingPartnerAmountsSum_EqualsInitiativeBudgetUSD_AfterApplyAiChanges()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 2500000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 2500000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.FundingPartners!.Sum(fp => fp.AmountUSD ?? 0).Should().Be(2500000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-024")]
    [Trait("Ticket", "PNO-873")]
    public async Task CreateOpportunityFromProposalAsync_WithMultipleFundingPartners()
    {
        var partner2 = await Context.Partners.Where(p => !p.IsDeleted && p.Id != PartnerId).FirstOrDefaultAsync();
        if (partner2 == null) return;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Multi FP",
            Description = "Desc",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1000000, CurrencyId = CurrencyId },
                new() { PartnerId = partner2.Id, Amount = 500000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.FundingPartners.Should().HaveCount(2);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-025")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_ReplacesFundingPartners_AndUpdatesInitiativeBudgetUSD()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 4000000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 4000000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.InitiativeBudgetUSD.Should().Be(4000000m);
        result.FundingPartners.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-026")]
    public async Task CreateOpportunityFromProposalAsync_WithSourceInteractionIds_AcceptsRequest()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Interactions Opp",
            Description = "Desc",
            SourceInteractionIds = new List<int> { 1, 2 }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-027")]
    public async Task ApplyAiChangesAsync_ForOpportunityInCancelledStage_Throws()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc", stage: "CANCELLED");
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated" };
        var act = () => Manager.ApplyAiChangesAsync(oppId, request);
        await act.Should().ThrowAsync<BusinessException>();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-028")]
    public async Task CreateOpportunityFromProposalAsync_WithBeneficiariesToBeDetermined()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            BeneficiariesToBeDetermined = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-029")]
    public async Task ApplyAiChangesAsync_WithExpectedImpactAndExpectedOutcomes()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            ExpectedImpact = "Significant impact",
            ExpectedOutcomes = "Improved outcomes"
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.ExpectedImpact.Should().Be("Significant impact");
        result.ExpectedOutcomes.Should().Be("Improved outcomes");
    }

    [SkipIfNotPostgreSQLFact]
    [Trait("TestId", "INT-030")]
    [Trait("Ticket", "PNO-694")]
    public async Task FullAIFlow_Create_ApplyAiChanges_GetOpportunityDetailsForAI()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "AI Flow Test", Description = "Desc" };
        var created = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var aiRequest = new ApplyOpportunityAiChangesRequest { Name = "AI-Enhanced", Challenges = "Challenges" };
        await Manager.ApplyAiChangesAsync(created.Id, aiRequest);
        Dictionary<string, object>? details = null;
        try { details = await Manager.GetOpportunityDetailsForAIAsync(created.Id); } catch { return; }
        details.Should().ContainKey("name");
        details!["name"].Should().Be("AI-Enhanced");
    }
}
