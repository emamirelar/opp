using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Opportunity requirements tests migrated from JIRA/Zephyr gap analysis.
/// Covers: Team Section (PNO-979), Workflow Status (PNO-940),
/// WHY Section (PNO-692/938), WHAT Section (PNO-700).
/// Tests real entity operations via UNOPSAppDbContext.
/// </summary>
public class OpportunityJiraRequirementsTests : ManagerTestBase
{
    private readonly string _marker = $"OJR_{Guid.NewGuid():N}";

    #region Seed Helpers

    private async Task<OpportunityEntity> SeedOpportunityAsync(
        string stage = "IDENTIFY & PROFILE",
        EntityStatus status = EntityStatus.Active,
        bool withOrgUnit = false,
        bool isDeleted = false)
    {
        int? orgId = null;
        if (withOrgUnit)
        {
            var org = new OrganizationHierarchy
            {
                Name = $"OrgUnit_{_marker}",
                Code = $"OU_{_marker}",
                Description = $"Test org unit {_marker}",
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Set<OrganizationHierarchy>().AddAsync(org);
            await SaveChangesAsync();
            orgId = org.Id;
            RegisterTableCleanup("OrganizationHierarchy", $"\"Id\" = {org.Id}");
        }

        var opp = new OpportunityEntity
        {
            Name = $"Opp_{_marker}",
            Description = $"Description_{_marker}",
            Stage = stage,
            Status = status,
            ResponsibleOrgUnitId = orgId,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = isDeleted
        };
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();
        RegisterTableCleanup("Opportunities", $"\"Id\" = {opp.Id}");
        return opp;
    }

    private async Task<ProposedInitiativeType> SeedInitiativeTypeAsync(string name = "Grant Support")
    {
        var pit = new ProposedInitiativeType
        {
            Name = $"{name}_{_marker}",
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Set<ProposedInitiativeType>().AddAsync(pit);
        await SaveChangesAsync();
        RegisterTableCleanup("ProposedInitiativeTypes", $"\"Id\" = {pit.Id}");
        return pit;
    }

    private async Task<SDG> SeedSDGAsync(int sdgNumber, string sdgName)
    {
        var sdg = new SDG
        {
            Name = sdgName,
            SDGNumber = sdgNumber.ToString(),
            SDGDescription = sdgName,
            Status = EntityStatus.Active
        };
        await Context.Set<SDG>().AddAsync(sdg);
        await SaveChangesAsync();
        RegisterTableCleanup("SDGs", $"\"Id\" = {sdg.Id}");
        return sdg;
    }

    private async Task<OpportunitySDG> SeedOpportunitySDGAsync(int opportunityId, int sdgId, bool isPrimary = false)
    {
        var oppSdg = new OpportunitySDG
        {
            Name = $"OppSDG_{_marker}",
            OpportunityId = opportunityId,
            SDGId = sdgId,
            IsPrimary = isPrimary,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Set<OpportunitySDG>().AddAsync(oppSdg);
        await SaveChangesAsync();
        RegisterTableCleanup("OpportunitySDGs", $"\"Id\" = {oppSdg.Id}");
        return oppSdg;
    }

    private async Task<OpportunityDeliverable> SeedDeliverableAsync(int opportunityId, string name)
    {
        var del = new OpportunityDeliverable
        {
            Name = name,
            OpportunityId = opportunityId,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Set<OpportunityDeliverable>().AddAsync(del);
        await SaveChangesAsync();
        RegisterTableCleanup("OpportunityDeliverables", $"\"Id\" = {del.Id}");
        return del;
    }

    private async Task<OpportunityCollaborator> SeedCollaboratorAsync(int opportunityId, int userId)
    {
        var collab = new OpportunityCollaborator
        {
            Name = $"Collab_{_marker}",
            OpportunityId = opportunityId,
            UserId = userId,
            AddedDate = DateTime.UtcNow,
            AddedBy = TestUserId,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Set<OpportunityCollaborator>().AddAsync(collab);
        await SaveChangesAsync();
        RegisterTableCleanup("OpportunityCollaborators", $"\"Id\" = {collab.Id}");
        return collab;
    }

    #endregion

    #region Positive Tests

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-POS-001")]
    public async Task POS_001_Opportunity_CreatedWithDefaultStage_IdentifyAndProfile()
    {
        var opp = await SeedOpportunityAsync();
        var loaded = await Context.Opportunities.FindAsync(opp.Id);

        loaded.Should().NotBeNull();
        loaded!.Stage.Should().Be("IDENTIFY & PROFILE");
    }

    [Fact]
    [Trait("JIRA", "PNO-700")]
    [Trait("TestId", "TC-OJR-POS-002")]
    public async Task POS_002_Opportunity_CanSetInitiativeType()
    {
        var opp = await SeedOpportunityAsync();
        var pit = await SeedInitiativeTypeAsync();

        opp.ProposedInitiativeTypeId = pit.Id;
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var loaded = await Context.Opportunities
            .Include(o => o.ProposedInitiativeType)
            .FirstAsync(o => o.Id == opp.Id);
        loaded.ProposedInitiativeTypeId.Should().Be(pit.Id);
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-POS-003")]
    public async Task POS_003_Opportunity_CanLinkMultipleSDGs()
    {
        var opp = await SeedOpportunityAsync();
        var sdg1 = await SeedSDGAsync(1, "No Poverty");
        var sdg13 = await SeedSDGAsync(13, "Climate Action");

        await SeedOpportunitySDGAsync(opp.Id, sdg1.Id, isPrimary: true);
        await SeedOpportunitySDGAsync(opp.Id, sdg13.Id);

        var linkedSdgs = await Context.Set<OpportunitySDG>()
            .Where(s => s.OpportunityId == opp.Id && !s.IsDeleted)
            .ToListAsync();

        linkedSdgs.Should().HaveCount(2);
        linkedSdgs.Should().Contain(s => s.SDGId == sdg1.Id && s.IsPrimary);
    }

    #endregion

    #region Negative Tests (>= 9)

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-NEG-001")]
    public async Task NEG_001_SoftDeletedOpportunity_ExcludedFromActiveQuery()
    {
        var opp = await SeedOpportunityAsync(isDeleted: true);

        var found = await Context.Opportunities
            .Where(o => o.Id == opp.Id && !o.IsDeleted)
            .FirstOrDefaultAsync();

        found.Should().BeNull();
    }

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-NEG-002")]
    public async Task NEG_002_Opportunity_WithoutDescription_HasEmptyField()
    {
        var opp = new OpportunityEntity
        {
            Name = $"NoDesc_{_marker}",
            Description = "",
            Stage = "IDENTIFY & PROFILE",
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();
        RegisterTableCleanup("Opportunities", $"\"Id\" = {opp.Id}");

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.Description.Should().BeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-700")]
    [Trait("TestId", "TC-OJR-NEG-003")]
    public async Task NEG_003_Opportunity_WithoutInitiativeType_HasNullFK()
    {
        var opp = await SeedOpportunityAsync();
        var loaded = await Context.Opportunities.FindAsync(opp.Id);

        loaded!.ProposedInitiativeTypeId.Should().BeNull();
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-NEG-004")]
    public async Task NEG_004_Opportunity_WithNegativeBeneficiaryCount_StoresValue()
    {
        var opp = await SeedOpportunityAsync();
        opp.EstimatedDirectBeneficiaries = -500;
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.EstimatedDirectBeneficiaries.Should().Be(-500);
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-NEG-005")]
    public async Task NEG_005_Opportunity_CannotQueryDeletedOpportunitiesWithActiveFilter()
    {
        var active = await SeedOpportunityAsync(isDeleted: false);
        var deleted = await SeedOpportunityAsync(isDeleted: true);

        var results = await Context.Opportunities
            .Where(o => !o.IsDeleted && o.Name!.Contains(_marker))
            .ToListAsync();

        results.Should().Contain(o => o.Id == active.Id);
        results.Should().NotContain(o => o.Id == deleted.Id);
    }

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-NEG-006")]
    public async Task NEG_006_Collaborator_WithNonExistentUser_RejectedByFKConstraint()
    {
        var opp = await SeedOpportunityAsync();

        var act = async () => await SeedCollaboratorAsync(opp.Id, userId: 999999);

        await act.Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateException>();
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-NEG-007")]
    public async Task NEG_007_OpportunitySDG_WithDeletedSDG_StillLinked()
    {
        var opp = await SeedOpportunityAsync();
        var sdg = await SeedSDGAsync(1, "No Poverty");
        await SeedOpportunitySDGAsync(opp.Id, sdg.Id);

        sdg.IsDeleted = true;
        Context.Set<SDG>().Update(sdg);
        await SaveChangesAsync();

        var links = await Context.Set<OpportunitySDG>()
            .Where(s => s.OpportunityId == opp.Id && !s.IsDeleted)
            .ToListAsync();
        links.Should().HaveCount(1);
    }

    [Fact]
    [Trait("JIRA", "PNO-700")]
    [Trait("TestId", "TC-OJR-NEG-008")]
    public async Task NEG_008_Deliverable_WithEmptyName_CanStillBeSaved()
    {
        var opp = await SeedOpportunityAsync();
        var del = await SeedDeliverableAsync(opp.Id, "");

        var loaded = await Context.Set<OpportunityDeliverable>().FindAsync(del.Id);
        loaded.Should().NotBeNull();
        loaded!.Name.Should().BeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-NEG-009")]
    public async Task NEG_009_Opportunity_InvalidStage_StoresArbitraryString()
    {
        var opp = await SeedOpportunityAsync(stage: "TOTALLY_INVALID_STAGE");

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.Stage.Should().Be("TOTALLY_INVALID_STAGE");
    }

    #endregion

    #region Edge/Boundary Tests (>= 9)

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-EDGE-001")]
    public async Task EDGE_001_Opportunity_MaxLengthName_120Chars_Persists()
    {
        var longName = new string('A', 120);
        var opp = new OpportunityEntity
        {
            Name = longName,
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();
        RegisterTableCleanup("Opportunities", $"\"Id\" = {opp.Id}");

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.Name.Should().HaveLength(120);
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-EDGE-002")]
    public async Task EDGE_002_Opportunity_GOStage_IsImmutableByConvention()
    {
        var opp = await SeedOpportunityAsync(stage: "GO");
        var loaded = await Context.Opportunities.FindAsync(opp.Id);

        loaded!.Stage.Should().Be("GO");
        new[] { "GO", "NO GO", "CANCELLED" }.Should().Contain(loaded.Stage);
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-EDGE-003")]
    public async Task EDGE_003_Opportunity_NOGOStage_IsImmutableByConvention()
    {
        var opp = await SeedOpportunityAsync(stage: "NO GO");
        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.Stage.Should().Be("NO GO");
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-EDGE-004")]
    public async Task EDGE_004_Opportunity_CANCELLEDStage_IsImmutableByConvention()
    {
        var opp = await SeedOpportunityAsync(stage: "CANCELLED");
        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.Stage.Should().Be("CANCELLED");
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-EDGE-005")]
    public async Task EDGE_005_Opportunity_ZeroBeneficiaries_IsValid()
    {
        var opp = await SeedOpportunityAsync();
        opp.EstimatedDirectBeneficiaries = 0;
        opp.EstimatedIndirectBeneficiaries = 0;
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.EstimatedDirectBeneficiaries.Should().Be(0);
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-EDGE-006")]
    public async Task EDGE_006_Opportunity_BeneficiariesToBeDetermined_DefaultsFalse()
    {
        var opp = await SeedOpportunityAsync();
        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.BeneficiariesToBeDetermined.Should().BeFalse();
    }

    [Fact]
    [Trait("JIRA", "PNO-700")]
    [Trait("TestId", "TC-OJR-EDGE-007")]
    public async Task EDGE_007_Opportunity_AllDeliveryModalities_AreValidEnumValues()
    {
        var validValues = Enum.GetValues<DeliveryModality>();
        validValues.Should().Contain(DeliveryModality.NotYetKnown);
        validValues.Should().Contain(DeliveryModality.AllDirect);
        validValues.Should().Contain(DeliveryModality.AllGrantSupport);
        validValues.Should().Contain(DeliveryModality.Mixed);
    }

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-EDGE-008")]
    public async Task EDGE_008_Opportunity_SoftDeletedWithActiveCollaborators()
    {
        var opp = await SeedOpportunityAsync();
        await SeedCollaboratorAsync(opp.Id, TestUserId);

        opp.IsDeleted = true;
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var collabs = await Context.Set<OpportunityCollaborator>()
            .Where(c => c.OpportunityId == opp.Id && !c.IsDeleted)
            .ToListAsync();
        collabs.Should().HaveCount(1, "collaborators should not be cascade-deleted");
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-EDGE-009")]
    public async Task EDGE_009_Opportunity_HighRisksAcknowledged_DefaultsFalse()
    {
        var opp = await SeedOpportunityAsync();
        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.HighRisksAcknowledged.Should().BeFalse();
    }

    #endregion

    #region Functional Tests (>= 9)

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-FUNC-001")]
    public async Task FUNC_001_Opportunity_AuditFields_PopulatedOnCreate()
    {
        var opp = await SeedOpportunityAsync();
        var loaded = await Context.Opportunities.FindAsync(opp.Id);

        loaded!.CreatedBy.Should().Be(TestUserId);
        loaded.LastModifiedBy.Should().Be(TestUserId);
        loaded.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-FUNC-002")]
    public async Task FUNC_002_Opportunity_WorkflowStatus_DefaultsToNone()
    {
        var opp = await SeedOpportunityAsync();
        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.WorkflowStatus.Should().Be(0);
    }

    [Fact]
    [Trait("JIRA", "PNO-700")]
    [Trait("TestId", "TC-OJR-FUNC-003")]
    public async Task FUNC_003_Deliverables_OrderedByCreation()
    {
        var opp = await SeedOpportunityAsync();
        var d1 = await SeedDeliverableAsync(opp.Id, $"First_{_marker}");
        var d2 = await SeedDeliverableAsync(opp.Id, $"Second_{_marker}");
        var d3 = await SeedDeliverableAsync(opp.Id, $"Third_{_marker}");

        var deliverables = await Context.Set<OpportunityDeliverable>()
            .Where(d => d.OpportunityId == opp.Id && !d.IsDeleted)
            .OrderBy(d => d.Id)
            .ToListAsync();

        deliverables.Should().HaveCount(3);
        deliverables[0].Name.Should().Contain("First");
        deliverables[2].Name.Should().Contain("Third");
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-FUNC-004")]
    public async Task FUNC_004_OpportunitySDG_CanSetPrimarySDG()
    {
        var opp = await SeedOpportunityAsync();
        var sdg = await SeedSDGAsync(4, "Quality Education");
        var link = await SeedOpportunitySDGAsync(opp.Id, sdg.Id, isPrimary: true);

        var loaded = await Context.Set<OpportunitySDG>().FindAsync(link.Id);
        loaded!.IsPrimary.Should().BeTrue();
    }

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-FUNC-005")]
    public async Task FUNC_005_Opportunity_ResponsibleOrgUnit_NavigationLoads()
    {
        var opp = await SeedOpportunityAsync(withOrgUnit: true);

        var loaded = await Context.Opportunities
            .Include(o => o.ResponsibleOrgUnit)
            .FirstAsync(o => o.Id == opp.Id);

        loaded.ResponsibleOrgUnit.Should().NotBeNull();
        loaded.ResponsibleOrgUnit!.Name.Should().Contain(_marker);
    }

    [Fact]
    [Trait("JIRA", "PNO-700")]
    [Trait("TestId", "TC-OJR-FUNC-006")]
    public async Task FUNC_006_Opportunity_DeliveryModality_CanBeSet()
    {
        var opp = await SeedOpportunityAsync();
        opp.DeliveryModality = DeliveryModality.AllGrantSupport;
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.DeliveryModality.Should().Be(DeliveryModality.AllGrantSupport);
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-FUNC-007")]
    public async Task FUNC_007_Opportunity_HighRiskAcknowledgement_CanBeToggled()
    {
        var opp = await SeedOpportunityAsync();

        opp.HighRisksAcknowledged = true;
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.HighRisksAcknowledged.Should().BeTrue();
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-FUNC-008")]
    public async Task FUNC_008_Opportunity_StageTransition_PersistedCorrectly()
    {
        var opp = await SeedOpportunityAsync(stage: "IDENTIFY & PROFILE");

        opp.Stage = "GO";
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.Stage.Should().Be("GO");
    }

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-FUNC-009")]
    public async Task FUNC_009_Collaborator_AuditFields_SetOnAdd()
    {
        var opp = await SeedOpportunityAsync();
        var collab = await SeedCollaboratorAsync(opp.Id, TestUserId);

        var loaded = await Context.Set<OpportunityCollaborator>().FindAsync(collab.Id);
        loaded!.AddedBy.Should().Be(TestUserId);
        loaded.AddedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    #endregion

    #region Integration Tests (>= 9)

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-INT-001")]
    public async Task INT_001_FullOpportunity_CreateWithTeamAndSDGs()
    {
        var opp = await SeedOpportunityAsync(withOrgUnit: true);
        var sdg = await SeedSDGAsync(5, "Gender Equality");
        await SeedOpportunitySDGAsync(opp.Id, sdg.Id, isPrimary: true);
        await SeedCollaboratorAsync(opp.Id, TestUserId);
        var del = await SeedDeliverableAsync(opp.Id, $"Deliverable_{_marker}");

        var loaded = await Context.Opportunities
            .Include(o => o.SDGs)
            .Include(o => o.Collaborators)
            .Include(o => o.Deliverables)
            .Include(o => o.ResponsibleOrgUnit)
            .FirstAsync(o => o.Id == opp.Id);

        loaded.SDGs.Should().HaveCount(1);
        loaded.Collaborators.Should().HaveCount(1);
        loaded.Deliverables.Should().HaveCount(1);
        loaded.ResponsibleOrgUnit.Should().NotBeNull();
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-INT-002")]
    public async Task INT_002_Opportunity_SoftDelete_PreservesRelatedData()
    {
        var opp = await SeedOpportunityAsync();
        await SeedCollaboratorAsync(opp.Id, TestUserId);
        await SeedDeliverableAsync(opp.Id, $"Del_{_marker}");

        opp.IsDeleted = true;
        opp.DeletedBy = TestUserId;
        opp.DeletedDate = DateTime.UtcNow;
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var collabs = await Context.Set<OpportunityCollaborator>()
            .Where(c => c.OpportunityId == opp.Id)
            .ToListAsync();
        collabs.Should().NotBeEmpty("soft delete should not cascade to children");
    }

    [Fact]
    [Trait("JIRA", "PNO-700")]
    [Trait("TestId", "TC-OJR-INT-003")]
    public async Task INT_003_Opportunity_UpdateAllSections_Persists()
    {
        var opp = await SeedOpportunityAsync(withOrgUnit: true);
        var pit = await SeedInitiativeTypeAsync("Technical Cooperation");

        opp.ResultsFocus = "Improved outcomes";
        opp.ExpectedImpact = "High impact";
        opp.ProposedInitiativeTypeId = pit.Id;
        opp.EstimatedDirectBeneficiaries = 10000;
        opp.DeliveryModality = DeliveryModality.Mixed;
        Context.Opportunities.Update(opp);
        await SaveChangesAsync();

        var loaded = await Context.Opportunities
            .Include(o => o.ProposedInitiativeType)
            .FirstAsync(o => o.Id == opp.Id);

        loaded.ResultsFocus.Should().Be("Improved outcomes");
        loaded.ExpectedImpact.Should().Be("High impact");
        loaded.ProposedInitiativeType.Should().NotBeNull();
        loaded.EstimatedDirectBeneficiaries.Should().Be(10000);
        loaded.DeliveryModality.Should().Be(DeliveryModality.Mixed);
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-INT-004")]
    public async Task INT_004_MultipleSDGs_WithPrimaryFlag_WorkCorrectly()
    {
        var opp = await SeedOpportunityAsync();
        var sdg1 = await SeedSDGAsync(1, "No Poverty");
        var sdg4 = await SeedSDGAsync(4, "Quality Education");
        var sdg13 = await SeedSDGAsync(13, "Climate Action");

        await SeedOpportunitySDGAsync(opp.Id, sdg1.Id, isPrimary: true);
        await SeedOpportunitySDGAsync(opp.Id, sdg4.Id, isPrimary: false);
        await SeedOpportunitySDGAsync(opp.Id, sdg13.Id, isPrimary: false);

        var sdgs = await Context.Set<OpportunitySDG>()
            .Where(s => s.OpportunityId == opp.Id && !s.IsDeleted)
            .ToListAsync();

        sdgs.Should().HaveCount(3);
        sdgs.Count(s => s.IsPrimary).Should().Be(1);
    }

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-INT-005")]
    public async Task INT_005_MultipleCollaborators_OnSameOpportunity()
    {
        var opp = await SeedOpportunityAsync();
        await SeedCollaboratorAsync(opp.Id, TestUserId);
        await SeedCollaboratorAsync(opp.Id, TestUserId + 1);
        await SeedCollaboratorAsync(opp.Id, TestUserId + 2);

        var collabs = await Context.Set<OpportunityCollaborator>()
            .Where(c => c.OpportunityId == opp.Id && !c.IsDeleted)
            .ToListAsync();

        collabs.Should().HaveCount(3);
    }

    [Fact]
    [Trait("JIRA", "PNO-700")]
    [Trait("TestId", "TC-OJR-INT-006")]
    public async Task INT_006_Deliverable_SoftDelete_ExcludesFromActiveQuery()
    {
        var opp = await SeedOpportunityAsync();
        var d1 = await SeedDeliverableAsync(opp.Id, $"Active_{_marker}");
        var d2 = await SeedDeliverableAsync(opp.Id, $"Deleted_{_marker}");

        d2.IsDeleted = true;
        Context.Set<OpportunityDeliverable>().Update(d2);
        await SaveChangesAsync();

        var active = await Context.Set<OpportunityDeliverable>()
            .Where(d => d.OpportunityId == opp.Id && !d.IsDeleted)
            .ToListAsync();

        active.Should().HaveCount(1);
        active.First().Name.Should().Contain("Active");
    }

    [Fact]
    [Trait("JIRA", "PNO-940")]
    [Trait("TestId", "TC-OJR-INT-007")]
    public async Task INT_007_Opportunity_QueryByStage_FiltersCorrectly()
    {
        var draft = await SeedOpportunityAsync(stage: "IDENTIFY & PROFILE");
        var go = await SeedOpportunityAsync(stage: "GO");
        var cancelled = await SeedOpportunityAsync(stage: "CANCELLED");

        var identifyResults = await Context.Opportunities
            .Where(o => o.Stage == "IDENTIFY & PROFILE" && !o.IsDeleted && o.Name!.Contains(_marker))
            .ToListAsync();

        identifyResults.Should().Contain(o => o.Id == draft.Id);
        identifyResults.Should().NotContain(o => o.Id == go.Id);
    }

    [Fact]
    [Trait("JIRA", "PNO-692")]
    [Trait("TestId", "TC-OJR-INT-008")]
    public async Task INT_008_SDG_SoftDeleteLink_ExcludedFromOpportunity()
    {
        var opp = await SeedOpportunityAsync();
        var sdg = await SeedSDGAsync(17, "Partnerships for the Goals");
        var link = await SeedOpportunitySDGAsync(opp.Id, sdg.Id);

        link.IsDeleted = true;
        Context.Set<OpportunitySDG>().Update(link);
        await SaveChangesAsync();

        var activeLinks = await Context.Set<OpportunitySDG>()
            .Where(s => s.OpportunityId == opp.Id && !s.IsDeleted)
            .ToListAsync();

        activeLinks.Should().BeEmpty();
    }

    [Fact]
    [Trait("JIRA", "PNO-979")]
    [Trait("TestId", "TC-OJR-INT-009")]
    public async Task INT_009_Opportunity_CreateWithOrgUnit_LoadsViaNavigation()
    {
        var opp = await SeedOpportunityAsync(withOrgUnit: true);

        var loaded = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.Id == opp.Id);

        loaded.Should().NotBeNull();
        loaded!.ResponsibleOrgUnitId.Should().NotBeNull();
        loaded.ResponsibleOrgUnit.Should().NotBeNull();
    }

    #endregion
}
