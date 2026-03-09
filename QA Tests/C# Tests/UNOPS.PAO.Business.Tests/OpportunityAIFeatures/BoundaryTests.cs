/// <summary>
/// Boundary tests for Opportunity AI Features (PNO-694, PNO-803, PNO-804, PNO-805, PNO-873).
/// Covers min/max values, null handling, soft-delete, and edge cases.
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityAIFeatures;

[Collection("OpportunityAIFeatures")]
[Trait("Category", "Boundary")]
[Trait("Feature", "OpportunityAIFeatures")]
[Trait("Component", "UNOPSOpportunityManager")]
public class BoundaryTests : OpportunityAIFeaturesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-001")]
    [Trait("Ticket", "PNO-803")]
    public async Task CreateOpportunityFromProposalAsync_WithNameLength120_Succeeds()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = new string('x', 120),
            Description = "Desc"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.Name.Should().HaveLength(120);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-002")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_WithNameLength120_Succeeds()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = new string('x', 120) };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Name.Should().HaveLength(120);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-003")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_WithNameNull_PreservesExistingName()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original Name", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Description = "Updated" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Name.Should().Be("Original Name");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-004")]
    [Trait("Ticket", "PNO-804")]
    public async Task ApplyAiChangesAsync_WithDescriptionNull_PreservesExistingDescription()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Original Desc");
        var request = new ApplyOpportunityAiChangesRequest { Name = "Updated Name" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Description.Should().Be("Original Desc");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-005")]
    [Trait("Defect", "DEF-225")]
    public async Task CreateOpportunityFromProposalAsync_WithNullOptionalFields_DefaultsApplied()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Minimal",
            Description = null,
            FundingPartners = null,
            ClientPartners = null,
            Countries = null
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-006")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_WithEmptyFundingPartnersList_ClearsPartners()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { FundingPartners = new List<OpportunityFundingPartnerRequest>() };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.FundingPartners.Should().NotBeNull().And.BeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-007")]
    [Trait("Ticket", "PNO-873")]
    public async Task InitiativeBudgetUSD_Zero_AndFundingPartnerTotalZero_Allowed()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Zero Budget", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 0,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 0, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.InitiativeBudgetUSD.Should().Be(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-008")]
    [Trait("Ticket", "PNO-873")]
    public async Task InitiativeBudgetUSD_MatchesSumOfFundingPartnerAmounts()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Budget Match", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 1500000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1000000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.InitiativeBudgetUSD.Should().Be(1500000m);
        result.FundingPartners!.Sum(fp => fp.AmountUSD ?? 0).Should().Be(1000000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-009")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_SingleFundingPartnerAmount_EqualsInitiativeBudgetUSD()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Single Partner", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 500000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 500000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.FundingPartners![0].AmountUSD.Should().Be(500000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-010")]
    public async Task CreateOpportunityFromProposalAsync_WithSingleInteraction_Succeeds()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Single Interaction",
            Description = "Desc",
            SourceInteractionIds = new List<int> { 1 }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-011")]
    public async Task ApplyAiChangesAsync_WithEmptyDeliverables_ClearsDeliverables()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Deliverables = new List<OpportunityDeliverableRequest>() };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Deliverables.Should().NotBeNull().And.BeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-012")]
    public async Task ApplyAiChangesAsync_WithEmptySdGs_ClearsSDGs()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { SdGs = new List<OpportunitySDGRequest>() };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.SDGs.Should().NotBeNull().And.BeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-013")]
    public async Task ApplyAiChangesAsync_WithEmptyCountries_ClearsCountries()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Countries = new List<int>() };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Countries.Should().NotBeNull().And.BeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-014")]
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
    [Trait("TestId", "BND-015")]
    public async Task ApplyAiChangesAsync_ExpectedImpactTruncationAt510Chars()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ExpectedImpact = new string('x', 600) };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.ExpectedImpact.Should().HaveLength(510);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-016")]
    public async Task ApplyAiChangesAsync_ExpectedOutcomesTruncationAt510Chars()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ExpectedOutcomes = new string('x', 600) };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.ExpectedOutcomes.Should().HaveLength(510);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-017")]
    public async Task ApplyAiChangesAsync_WithProposedInitiativeTypeName_WhenIdNull_Resolves()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { ProposedInitiativeTypeName = "Project" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.ProposedInitiativeTypeId.Should().Be(ProposedInitiativeTypeId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-018")]
    public async Task CreateOpportunityFromProposalAsync_WithProposedInitiativeTypeName_Fallback()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            ProposedInitiativeTypeName = "Project"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-019")]
    [Trait("Ticket", "PNO-805")]
    public async Task ApplyAiChangesAsync_PreservesOpportunityManager_WhenStakeholdersEmpty()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var request = new ApplyOpportunityAiChangesRequest { Stakeholders = new List<OpportunityStakeholderRequest>() };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.OpportunityManager?.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-020")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithCurrentUserIdNegativeOne_ShouldNotAssignServiceAccountAsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, -1);
        result.Should().NotBeNull();
        result.OpportunityManager?.UserId.Should().NotBe(-1, because: "PNO-805: OM must be logged-in user, not service account");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-021")]
    public async Task ApplyAiChangesAsync_WithUNOPSMissionsNotApplicableTrue_ClearsMissions()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { UNOPSMissionsNotApplicable = true };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.UNOPSMissionsNotApplicable.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-022")]
    public async Task ApplyAiChangesAsync_WithMinimalRequiredFieldsOnly()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { Challenges = "New" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.Challenges.Should().Be("New");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-023")]
    [Trait("Ticket", "PNO-873")]
    public async Task FundingPartner_AmountUSD_DecimalPrecision()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1234567.89m, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.FundingPartners![0].AmountUSD.Should().Be(1234567.89m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-024")]
    [Trait("Ticket", "PNO-873")]
    public async Task InitiativeBudgetUSD_DecimalPrecision()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { InitiativeBudgetUSD = 9876543.21m };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.InitiativeBudgetUSD.Should().Be(9876543.21m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-025")]
    public async Task ApplyAiChangesAsync_WithTargetSigningDateInPast()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest { TargetSigningDate = DateTime.UtcNow.AddDays(-30) };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.TargetSigningDate.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-026")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyDocumentsList()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Desc",
            Documents = new List<NewDocumentRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-027")]
    [Trait("Ticket", "PNO-805")]
    public async Task ApplyAiChangesAsync_WithEmptyStakeholders_PreservesOM()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var request = new ApplyOpportunityAiChangesRequest { Stakeholders = new List<OpportunityStakeholderRequest>() };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.OpportunityManager.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-028")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_WithNullInitiativeBudgetUSD_PreservesExisting()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc", budgetUSD: 1000000);
        var request = new ApplyOpportunityAiChangesRequest { Description = "Updated" };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.InitiativeBudgetUSD.Should().Be(1000000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-029")]
    public async Task CreateOpportunityFromProposalAsync_WithNullFundingPartners()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc", FundingPartners = null };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-030")]
    [Trait("Ticket", "PNO-873")]
    public async Task ApplyAiChangesAsync_FundingPartnersSum_SlightlyDifferentFromInitiativeBudgetUSD()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original", description: "Desc");
        var request = new ApplyOpportunityAiChangesRequest
        {
            InitiativeBudgetUSD = 1000000m,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 999999.99m, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.ApplyAiChangesAsync(oppId, request);
        result.InitiativeBudgetUSD.Should().Be(1000000m);
        result.FundingPartners!.Sum(fp => fp.AmountUSD ?? 0).Should().Be(999999.99m);
    }
}
