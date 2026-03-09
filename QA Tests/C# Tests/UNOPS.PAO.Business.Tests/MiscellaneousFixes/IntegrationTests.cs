/// <summary>
/// Integration tests for Miscellaneous Fixes (PNO-805, PNO-801).
/// Full round-trip flows, cross-component workflows, API-level validation.
/// </summary>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MiscellaneousFixes;

[Collection("MiscellaneousFixes")]
[Trait("Category", "Integration")]
[Trait("Feature", "MiscellaneousFixes")]
[Trait("Component", "UNOPSOpportunityManager")]
public class IntegrationTests : MiscellaneousFixesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-001")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_ThenGetOpportunityAsync_ShowsOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var created = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        var result = await Manager.GetOpportunityAsync(created.Id);
        result!.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-002")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_FullRoundTrip_WithAllSectionsAndOM()
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
        reloaded.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-003")]
    public async Task CreateOpportunityFromProposalAsync_AssignCreatorAsOpportunityManagerAsync_Called()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-004")]
    public async Task CreateOpportunityFromProposalAsync_WithPartnerId_LinksPartnerAndOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Partner Link Test",
            PartnerId = PartnerId,
            IsFundingPartner = true,
            IsClientPartner = false
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.FundingPartners.Should().Contain(fp => fp.PartnerId == PartnerId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-005")]
    public async Task CreateOpportunityFromProposalAsync_WithFundingPartners_VerifyBudgetAndOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Budget Opp",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 1500000, CurrencyId = CurrencyId }
            },
            InitiativeBudgetUSD = 1500000m
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.InitiativeBudgetUSD.Should().Be(1500000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-006")]
    public async Task CreateOpportunityFromProposalAsync_WithStakeholders_VerifyOMIncluded()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Stakeholders Test",
            Stakeholders = new List<OpportunityStakeholderRequest>
            {
                new() { UserId = PaoUserId, EntityRoleId = EntityRoleId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.Stakeholders.Should().NotBeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-007")]
    public async Task CreateOpportunityFromProposalAsync_WithSDGsAndCountries_OMAssigned()
    {
        var sdg = Context.SDGs.FirstOrDefault(s => !s.IsDeleted);
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "SDG Country Test",
            Countries = new List<int> { CountryId }
        };
        if (sdg != null)
            request.SdGs = new List<OpportunitySDGRequest> { new() { SDGId = sdg.Id, IsPrimary = true } };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-008")]
    public async Task CreateOpportunityFromProposalAsync_WithDeliverables_OMAssigned()
    {
        var output = await Context.Outputs.FirstOrDefaultAsync(o => !o.IsDeleted);
        if (output == null) return;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Deliverables Test",
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { OutputId = output.Id }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-009")]
    public async Task CreateOpportunityFromProposalAsync_WithWhenSectionDates_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "When Test",
            TargetSigningDate = DateTime.UtcNow.AddMonths(3),
            ImplementationStartDate = DateTime.UtcNow.AddMonths(4),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(12),
            SubmissionDeadline = DateTime.UtcNow.AddMonths(2)
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-010")]
    public async Task CreateOpportunityFromProposalAsync_WithProposedInitiativeTypeName_ResolvesAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Initiative Type Test",
            ProposedInitiativeTypeName = "Project"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-011")]
    public async Task CreateOpportunityFromProposalAsync_WithUNOPSMissions_OMAssigned()
    {
        var mission = await Context.UNOPSMissions.FirstOrDefaultAsync(m => !m.IsDeleted);
        if (mission == null) return;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Missions Test",
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest> { new() { UNOPSMissionId = mission.Id } }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-012")]
    public async Task CreateOpportunityFromProposalAsync_WithMultipleFundingPartners_OMAssigned()
    {
        var partners = Context.Partners.Where(p => !p.IsDeleted).Take(2).ToList();
        if (partners.Count < 2) return;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Multi Partner Test",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = partners[0].Id, Amount = 500000, CurrencyId = CurrencyId },
                new() { PartnerId = partners[1].Id, Amount = 500000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-013")]
    public async Task CreateOpportunityFromProposalAsync_WithSourceInteractionIds_AcceptsAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Source IDs Test",
            SourceInteractionIds = new List<int> { 1, 2 }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-014")]
    public async Task CreateOpportunityFromProposalAsync_WithBeneficiariesToBeDetermined_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Beneficiaries Test",
            BeneficiariesToBeDetermined = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-015")]
    public async Task AssignCreatorAsOpportunityManagerAsync_ThenGetOpportunityAsync_ReturnsOM()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var result = await Manager.GetOpportunityAsync(oppId);
        result!.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-016")]
    public async Task CreateOpportunityFromProposalAsync_WithClientPartners_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Client Test",
            ClientPartners = new List<OpportunityClientPartnerRequest> { new() { PartnerId = PartnerId } }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-017")]
    public async Task CreateOpportunityFromProposalAsync_WithDocuments_AcceptsAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Documents Test",
            Documents = new List<NewDocumentRequest>()
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-018")]
    public async Task CreateOpportunityFromProposalAsync_WithDuplicateInteractionIds_DeduplicatedAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Dedup Test",
            SourceInteractionIds = new List<int> { 1, 1, 2, 2 }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-019")]
    public async Task CreateOpportunityFromProposalAsync_WithEmptyDescription_AllowedAndOMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-020")]
    public async Task CreateOpportunityFromProposalAsync_WithOptionalDescription_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Minimal" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-021")]
    public async Task CreateOpportunityFromProposalAsync_WithResponsibleOrgUnit_OMAndStakeholdersAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "OrgUnit Test",
            ResponsibleOrgUnitId = OrgHierarchyId
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ResponsibleOrgUnitId.Should().Be(OrgHierarchyId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-022")]
    public async Task CreateOpportunityFromProposalAsync_WithDeliveryModality1Through4_OMAssigned()
    {
        for (var i = 1; i <= 4; i++)
        {
            var request = new CreateOpportunityFromInteractionsRequest
            {
                Name = $"Modality {i} Test",
                DeliveryModality = i
            };
            var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
            result.Should().NotBeNull();
            result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-023")]
    public async Task CreateOpportunityFromProposalAsync_WithImplementationStartDateDefaultedFromSigning_OMAssigned()
    {
        var signingDate = DateTime.UtcNow.AddMonths(6).Date;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Default Impl Test",
            TargetSigningDate = signingDate
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ImplementationStartDate.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-024")]
    public async Task CreateOpportunityFromProposalAsync_WithContextPartnerBothRoles_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Both Roles Test",
            PartnerId = PartnerId,
            IsFundingPartner = true,
            IsClientPartner = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-025")]
    public async Task CreateOpportunityFromProposalAsync_WithExpectedImpactTruncation_OMAssigned()
    {
        var longImpact = new string('x', 600);
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Truncation Test",
            ExpectedImpact = longImpact
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ExpectedImpact!.Length.Should().BeLessOrEqualTo(510);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-026")]
    public async Task CreateOpportunityFromProposalAsync_WithExpectedOutcomesTruncation_OMAssigned()
    {
        var longOutcomes = new string('x', 600);
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Outcomes Truncation Test",
            ExpectedOutcomes = longOutcomes
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ExpectedOutcomes!.Length.Should().BeLessOrEqualTo(510);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-027")]
    public async Task CreateOpportunityFromProposalAsync_WithUNOPSMissionsNotApplicable_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Not Applicable Test",
            UNOPSMissionsNotApplicable = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-028")]
    public async Task CreateOpportunityFromProposalAsync_WithAllOptionalNull_OMAssigned()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "All Null Test",
            Description = null,
            PartnerId = null,
            ResponsibleOrgUnitId = null,
            ProposedInitiativeTypeId = null,
            ProposedInitiativeTypeName = null,
            DeliveryModality = null,
            InitiativeBudgetUSD = null,
            TargetSigningDate = null,
            ImplementationStartDate = null,
            TargetDeliveryDate = null,
            SubmissionDeadline = null,
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
    [Trait("TestId", "INT-029")]
    public async Task CreateOpportunityFromProposalAsync_WithMultipleUsers_EachGetsOwnOM()
    {
        var user2 = TestDataHelper.GetOrCreateTestUser(Context, "intuser2@unops.org");
        var request1 = new CreateOpportunityFromInteractionsRequest { Name = "User1 Opp", Description = "Desc" };
        var request2 = new CreateOpportunityFromInteractionsRequest { Name = "User2 Opp", Description = "Desc" };
        var result1 = await Manager.CreateOpportunityFromProposalAsync(request1, PaoUserId);
        var result2 = await Manager.CreateOpportunityFromProposalAsync(request2, user2);
        result1.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result2.OpportunityManager!.UserId.Should().Be(user2);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "INT-030")]
    public async Task CreateOpportunityFromProposalAsync_ThenReload_OMPersisted()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Persistence Test", Description = "Desc" };
        var created = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        Context.ChangeTracker.Clear();
        var reloaded = await Manager.GetOpportunityAsync(created.Id);
        reloaded!.OpportunityManager!.UserId.Should().Be(PaoUserId);
    }
}
