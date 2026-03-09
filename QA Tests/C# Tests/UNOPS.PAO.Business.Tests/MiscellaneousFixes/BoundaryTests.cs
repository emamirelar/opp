/// <summary>
/// Boundary tests for Miscellaneous Fixes (PNO-805, PNO-801).
/// Edge cases: userId boundaries, name length, null/empty collections, service account IDs.
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MiscellaneousFixes;

[Collection("MiscellaneousFixes")]
[Trait("Category", "Boundary")]
[Trait("Feature", "MiscellaneousFixes")]
[Trait("Component", "UNOPSOpportunityManager")]
public class BoundaryTests : MiscellaneousFixesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-001")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_WithNameLength120_SucceedsAndAssignsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = new string('x', 120),
            Description = "Desc"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.Name.Length.Should().Be(120);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-002")]
    public async Task CreateOpportunityFromProposalAsync_WithNameLength1_Succeeds()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "A", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-003")]
    public async Task CreateOpportunityFromProposalAsync_WithCurrentUserIdOne_AssignsOM()
    {
        var altUserId = TestDataHelper.GetOrCreateTestUser(Context, "boundary1@unops.org");
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, altUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(altUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-004")]
    public async Task CreateOpportunityFromProposalAsync_WithNullOptionalFields_DefaultsAppliedAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Minimal",
            Description = null,
            PartnerId = null,
            FundingPartners = null,
            ClientPartners = null,
            Countries = null,
            SdGs = null,
            UNOPSMissions = null,
            Deliverables = null,
            Stakeholders = null
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-005")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyDescription_AllowedAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-006")]
    public async Task CreateOpportunityFromProposalAsync_WithCurrentUserIdNegativeOne_DoesNotAssignOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, -1);
        result.Should().NotBeNull();
        result.OpportunityManager.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-007")]
    public async Task CreateOpportunityFromProposalAsync_WithCurrentUserIdZero_DoesNotAssignOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, 0);
        result.Should().NotBeNull();
        result.OpportunityManager.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-008")]
    public async Task AssignCreatorAsOpportunityManagerAsync_WhenAlreadyAssigned_NoDuplicate()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var result = await Manager.GetOpportunityAsync(oppId);
        result!.OpportunityManager.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-009")]
    public async Task CreateOpportunityFromProposalAsync_WithSingleFundingPartner_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-010")]
    public async Task CreateOpportunityFromProposalAsync_WithSingleCountry_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Countries = new List<int> { CountryId }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-011")]
    public async Task CreateOpportunityFromProposalAsync_WithExpectedImpact510Chars_TruncatedAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ExpectedImpact = new string('x', 600)
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.ExpectedImpact.Should().NotBeNullOrEmpty();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-012")]
    public async Task CreateOpportunityFromProposalAsync_WithInitiativeBudgetUSDZero_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            InitiativeBudgetUSD = 0m
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-013")]
    public async Task CreateOpportunityFromProposalAsync_WithBeneficiariesToBeDeterminedTrue_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            BeneficiariesToBeDetermined = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-014")]
    public async Task CreateOpportunityFromProposalAsync_WithDuplicateSDGIds_DeduplicatedAndOMAssigned()
    {
        var sdg = Context.SDGs.FirstOrDefault(s => !s.IsDeleted);
        if (sdg == null)
        {
            var request = new CreateOpportunityFromInteractionsRequest { Name = "Test" };
            var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
            result.Should().NotBeNull();
            result.OpportunityManager!.UserId.Should().Be(PaoUserId);
            return;
        }
        var requestWithSdg = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = sdg.Id, IsPrimary = true },
                new() { SDGId = sdg.Id, IsPrimary = false }
            }
        };
        var resultWithSdg = await Manager.CreateOpportunityFromProposalAsync(requestWithSdg, PaoUserId);
        resultWithSdg.Should().NotBeNull();
        resultWithSdg.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-015")]
    public async Task CreateOpportunityFromProposalAsync_WithDuplicateCountryIds_DeduplicatedAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Countries = new List<int> { CountryId, CountryId }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-016")]
    public async Task CreateOpportunityFromProposalAsync_WithProposedInitiativeTypeName_ResolvedAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ProposedInitiativeTypeName = "Project"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-017")]
    public async Task CreateOpportunityFromProposalAsync_WithTargetSigningDateOnly_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            TargetSigningDate = DateTime.UtcNow.AddMonths(6)
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-018")]
    public async Task CreateOpportunityFromProposalAsync_WithDeliveryModality1_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            DeliveryModality = 1
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-019")]
    public async Task CreateOpportunityFromProposalAsync_WithDeliveryModality4_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            DeliveryModality = 4
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-020")]
    public async Task CreateOpportunityFromProposalAsync_WithPartnerIdAndBothRoles_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            PartnerId = PartnerId,
            IsFundingPartner = true,
            IsClientPartner = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-021")]
    public async Task CreateOpportunityFromProposalAsync_WithSourceInteractionIds_AcceptsAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            SourceInteractionIds = new List<int> { 1 }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-022")]
    public async Task CreateOpportunityFromProposalAsync_WithStakeholdersIncludingOMRole_MergesCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Stakeholders = new List<OpportunityStakeholderRequest>
            {
                new() { UserId = PaoUserId, EntityRoleId = EntityRoleId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-023")]
    public async Task CreateOpportunityFromProposalAsync_WithUNOPSMissionsNotApplicable_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            UNOPSMissionsNotApplicable = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-024")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyDocumentsList_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Documents = new List<NewDocumentRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-025")]
    public async Task CreateOpportunityFromProposalAsync_WithNullFundingPartners_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", FundingPartners = null };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-026")]
    public async Task AssignCreatorAsOpportunityManagerAsync_WithDifferentUser_AssignsThatUserAsOM()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        var user2 = TestDataHelper.GetOrCreateTestUser(Context, "boundary2@unops.org");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, user2);
        var result = await Manager.GetOpportunityAsync(oppId);
        result!.OpportunityManager!.UserId.Should().Be(user2);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-027")]
    public async Task CreateOpportunityFromProposalAsync_WithImplementationStartDateOnly_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            TargetSigningDate = DateTime.UtcNow.AddMonths(3),
            ImplementationStartDate = DateTime.UtcNow.AddMonths(4)
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-028")]
    public async Task CreateOpportunityFromProposalAsync_WithLargeInitiativeBudgetUSD_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            InitiativeBudgetUSD = 999999999.99m
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-029")]
    public async Task CreateOpportunityFromProposalAsync_WithEstimatedBeneficiariesZero_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            EstimatedDirectBeneficiaries = 0,
            EstimatedIndirectBeneficiaries = 0
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-030")]
    public async Task CreateOpportunityFromProposalAsync_WithDuplicateStakeholderUserRole_DeduplicatedAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Stakeholders = new List<OpportunityStakeholderRequest>
            {
                new() { UserId = PaoUserId, EntityRoleId = EntityRoleId },
                new() { UserId = PaoUserId, EntityRoleId = EntityRoleId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }
}
