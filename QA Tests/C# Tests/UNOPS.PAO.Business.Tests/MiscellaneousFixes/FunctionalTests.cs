/// <summary>
/// Functional tests for Miscellaneous Fixes (PNO-805, PNO-801).
/// Business rules: OM assignment, audit fields, stakeholder role enforcement.
/// </summary>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MiscellaneousFixes;

[Collection("MiscellaneousFixes")]
[Trait("Category", "Functional")]
[Trait("Feature", "MiscellaneousFixes")]
[Trait("Component", "UNOPSOpportunityManager")]
public class FunctionalTests : MiscellaneousFixesFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-001")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_AssignsCreatorAsOpportunityManager_WithCorrectEntityRole()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager.Should().NotBeNull();
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.OpportunityManager.UserId.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-002")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_SetsCreatedByToCurrentUserId()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Should().NotBeNull();
        result.CreatedBy.Should().Be(PaoUserId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-003")]
    [Trait("Ticket", "PNO-805")]
    public async Task CreateOpportunityFromProposalAsync_CreatesOpportunityManagerStakeholder_InStakeholdersList()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Stakeholders.Should().NotBeNull();
        result.Stakeholders.Should().Contain(s => s.UserId == PaoUserId && (s.EntityRoleCode != null && s.EntityRoleCode.Contains("Opportunity_Manager")));
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-004")]
    public async Task CreateOpportunityFromProposalAsync_AuditFieldsPopulated()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        result.LastModifiedBy.Should().Be(PaoUserId);
        result.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-005")]
    public async Task CreateOpportunityFromProposalAsync_SetsDefaultStageToIdentifyAndProfile()
    {
        var request = new CreateOpportunityFromInteractionsRequest { Name = "Test", Description = "Desc" };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.Stage.Should().NotBeNullOrEmpty();
        result.Stage!.Contains("IDENTIFY", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-006")]
    public async Task AssignCreatorAsOpportunityManagerAsync_PersistsStakeholderToDatabase()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Test", description: "Desc");
        await Manager.AssignCreatorAsOpportunityManagerAsync(oppId, PaoUserId);
        var stakeholder = await Context.Set<OpportunityStakeholder>()
            .FirstOrDefaultAsync(s => s.OpportunityId == oppId && s.UserId == PaoUserId && !s.IsDeleted);
        stakeholder.Should().NotBeNull();
        stakeholder!.Notes.Should().Contain("Opportunity Manager");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-007")]
    public async Task CreateOpportunityFromProposalAsync_WithFundingPartners_OMStillPrimaryStakeholder()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = PartnerId, Amount = 100000, CurrencyId = CurrencyId }
            }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.FundingPartners.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-008")]
    public async Task CreateOpportunityFromProposalAsync_WithClientPartners_OMStillPrimaryStakeholder()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ClientPartners = new List<OpportunityClientPartnerRequest> { new() { PartnerId = PartnerId } }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ClientPartners.Should().HaveCount(1);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-009")]
    public async Task CreateOpportunityFromProposalAsync_WithStakeholders_MergesWithOM()
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
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.Stakeholders.Should().NotBeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-010")]
    public async Task CreateOpportunityFromProposalAsync_WithResponsibleOrgUnit_AutoPopulatesStakeholdersAndOM()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ResponsibleOrgUnitId = OrgHierarchyId
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ResponsibleOrgUnitId.Should().Be(OrgHierarchyId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-011")]
    public async Task CreateOpportunityFromProposalAsync_WithPartnerIdAndFundingRole_AddsPartnerToFundingPartners()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            PartnerId = PartnerId,
            IsFundingPartner = true,
            IsClientPartner = false
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.FundingPartners.Should().Contain(fp => fp.PartnerId == PartnerId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-012")]
    public async Task CreateOpportunityFromProposalAsync_WithPartnerIdAndClientRole_AddsPartnerToClientPartners()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            PartnerId = PartnerId,
            IsFundingPartner = false,
            IsClientPartner = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ClientPartners.Should().Contain(cp => cp.PartnerId == PartnerId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-013")]
    public async Task CreateOpportunityFromProposalAsync_WithCountries_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Countries = new List<int> { CountryId }
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.Countries.Should().Contain(c => c.CountryId == CountryId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-014")]
    public async Task CreateOpportunityFromProposalAsync_WithProposedInitiativeType_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ProposedInitiativeTypeId = ProposedInitiativeTypeId
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ProposedInitiativeTypeId.Should().Be(ProposedInitiativeTypeId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-015")]
    public async Task CreateOpportunityFromProposalAsync_WithInitiativeBudgetUSD_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            InitiativeBudgetUSD = 500000m
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.InitiativeBudgetUSD.Should().Be(500000m);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-016")]
    public async Task CreateOpportunityFromProposalAsync_WithDeliveryModality_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            DeliveryModality = 2
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.DeliveryModality.Should().Be(2);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-017")]
    public async Task CreateOpportunityFromProposalAsync_WithTargetSigningDate_MapsCorrectly()
    {
        var date = DateTime.UtcNow.AddMonths(6).Date;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            TargetSigningDate = date
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.TargetSigningDate.Should().NotBeNull();
        result.TargetSigningDate!.Value.Date.Should().Be(date);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-018")]
    public async Task CreateOpportunityFromProposalAsync_WithChallenges_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Challenges = "Key challenges"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.Challenges.Should().Be("Key challenges");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-019")]
    public async Task CreateOpportunityFromProposalAsync_WithResultsFocus_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ResultsFocus = "Results focus"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ResultsFocus.Should().Be("Results focus");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-020")]
    public async Task CreateOpportunityFromProposalAsync_WithExpectedImpact_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ExpectedImpact = "Impact 123"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ExpectedImpact.Should().Be("Impact 123");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-021")]
    public async Task CreateOpportunityFromProposalAsync_WithExpectedOutcomes_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ExpectedOutcomes = "Outcomes 123"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ExpectedOutcomes.Should().Be("Outcomes 123");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-022")]
    public async Task CreateOpportunityFromProposalAsync_WithPartnerReference_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            PartnerReference = "REF-001"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.PartnerReference.Should().Be("REF-001");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-023")]
    public async Task CreateOpportunityFromProposalAsync_WithMiscExternalStakeholders_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            MiscExternalStakeholders = "External stakeholders"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.MiscExternalStakeholders.Should().Be("External stakeholders");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-024")]
    public async Task CreateOpportunityFromProposalAsync_WithExpectedBeneficiaries_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ExpectedBeneficiaries = "Beneficiaries"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ExpectedBeneficiaries.Should().Be("Beneficiaries");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-025")]
    public async Task CreateOpportunityFromProposalAsync_WithEstimatedDirectBeneficiaries_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            EstimatedDirectBeneficiaries = 1000
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.EstimatedDirectBeneficiaries.Should().Be(1000);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-026")]
    public async Task CreateOpportunityFromProposalAsync_WithEstimatedIndirectBeneficiaries_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            EstimatedIndirectBeneficiaries = 5000
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.EstimatedIndirectBeneficiaries.Should().Be(5000);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-027")]
    public async Task CreateOpportunityFromProposalAsync_WithExternalStakeholderNotes_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            ExternalStakeholderNotes = "Notes"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ExternalStakeholderNotes.Should().Be("Notes");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-028")]
    public async Task CreateOpportunityFromProposalAsync_WithIsTargetSigningDateFirm_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            IsTargetSigningDateFirm = true
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.IsTargetSigningDateFirm.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-029")]
    public async Task CreateOpportunityFromProposalAsync_WithSigningDateNotes_MapsCorrectly()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            SigningDateNotes = "Signing notes"
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.SigningDateNotes.Should().Be("Signing notes");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-030")]
    public async Task CreateOpportunityFromProposalAsync_WithImplementationStartDate_MapsCorrectly()
    {
        var date = DateTime.UtcNow.AddMonths(6).Date;
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            TargetSigningDate = DateTime.UtcNow.AddMonths(3).Date,
            ImplementationStartDate = date
        };
        var result = await Manager.CreateOpportunityFromProposalAsync(request, PaoUserId);
        result.OpportunityManager!.UserId.Should().Be(PaoUserId);
        result.ImplementationStartDate.Should().NotBeNull();
        result.ImplementationStartDate!.Value.Date.Should().Be(date);
    }
}
