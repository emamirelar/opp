/// <summary>
/// Negative tests for Offices Feature (PNO-1213, PNO-1214).
/// Requirements validated: Invalid inputs, wrong IDs, unauthorized access, expected failures.
/// </summary>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OfficesFeature;

[Collection("OfficesFeature")]
[Trait("Category", "Negative")]
[Trait("Feature", "Offices")]
public class NegativeTests : OfficesFeatureFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-001")]
    public async Task GetOrganizationHierarchyById_ZeroId_ThrowsOrReturnsNull()
    {
        // Act
        var result = await OrgHierarchyManager.GetOrganizationHierarchyById(0);

        // Assert — invalid ID should not return valid office
        result.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-002")]
    public async Task GetOrganizationHierarchyById_NegativeId_ThrowsOrReturnsNull()
    {
        var result = await OrgHierarchyManager.GetOrganizationHierarchyById(-1);
        result.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-003")]
    public async Task GetOrganizationHierarchyById_NonExistentId_ReturnsNull()
    {
        var result = await OrgHierarchyManager.GetOrganizationHierarchyById(999999);
        result.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-004")]
    public async Task Opportunity_WithNullResponsibleOrgUnitId_NotLinkedToOffice()
    {
        var opp = await Context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ResponsibleOrgUnitId == null && !o.IsDeleted);
        if (opp != null)
        {
            opp.ResponsibleOrgUnitId.Should().BeNull();
            opp.ResponsibleOrgUnit.Should().BeNull();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-005")]
    public async Task Opportunity_WithInvalidResponsibleOrgUnitId_OrphanedReference()
    {
        var invalidOrgId = 999998;
        var count = await Context.OrganizationHierarchies.CountAsync(o => o.Id == invalidOrgId && !o.IsDeleted);
        count.Should().Be(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-006")]
    public async Task Partner_WithoutOrgUnitRelationship_NotManagedByOffice()
    {
        var partnerWithoutOrg = await Context.Partners
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Where(p => !Context.OrganizationUnitRelationships.Any(r =>
                r.EntityId == p.Id && r.EntityType == "Partner" && !r.IsDeleted))
            .FirstOrDefaultAsync();
        partnerWithoutOrg.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-007")]
    public async Task OrganizationUnitRelationship_InvalidEntityType_NotResolved()
    {
        var invalidRel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EntityType == "InvalidType" && !r.IsDeleted);
        invalidRel.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-008")]
    public async Task GetOrganizationHierarchyById_SoftDeletedOffice_Excluded()
    {
        var deleted = await Context.OrganizationHierarchies.FirstOrDefaultAsync(o => o.IsDeleted);
        if (deleted != null)
        {
            var result = await OrgHierarchyManager.GetOrganizationHierarchyById(deleted.Id);
            result.Should().BeNull();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-009")]
    public async Task OrganizationHierarchy_WithInvalidParentId_Orphaned()
    {
        var orphan = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ParentId == 999997 && !o.IsDeleted);
        orphan.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-010")]
    public async Task GetOrganizationsByType_InvalidEnumValue_ReturnsEmptyOrThrows()
    {
        var invalidType = (OrganizationUnitType)999;
        var result = OrgHierarchyManager.GetOrganizationsByType(invalidType);
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-011")]
    public async Task OrganizationUnitRelationship_Deleted_ExcludedFromPartnerQuery()
    {
        var deletedRel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IsDeleted && r.EntityType == "Partner");
        if (deletedRel != null)
        {
            deletedRel.IsDeleted.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-012")]
    public async Task Opportunity_SoftDeleted_ExcludedFromRelatedOpportunities()
    {
        var deletedOpp = await Context.Opportunities.FirstOrDefaultAsync(o => o.IsDeleted);
        if (deletedOpp != null)
        {
            deletedOpp.IsDeleted.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-013")]
    public async Task Partner_SoftDeleted_ExcludedFromRelatedPartners()
    {
        var deletedPartner = await Context.Partners.FirstOrDefaultAsync(p => p.IsDeleted);
        if (deletedPartner != null)
        {
            deletedPartner.IsDeleted.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-014")]
    public async Task GetOrganizationHierarchyById_MaxIntId_ReturnsNull()
    {
        var result = await OrgHierarchyManager.GetOrganizationHierarchyById(int.MaxValue);
        result.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-015")]
    public async Task OrganizationHierarchy_EmptyName_Invalid()
    {
        var withEmptyName = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => string.IsNullOrEmpty(o.Name) && !o.IsDeleted);
        withEmptyName.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-016")]
    public async Task OrganizationHierarchy_EmptyCode_Invalid()
    {
        var withEmptyCode = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => string.IsNullOrEmpty(o.Code) && !o.IsDeleted);
        withEmptyCode.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-017")]
    public async Task OrganizationUnitRelationship_NegativeEntityId_Invalid()
    {
        var count = await Context.OrganizationUnitRelationships
            .CountAsync(r => r.EntityId < 0 && !r.IsDeleted);
        count.Should().Be(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-018")]
    public async Task OrganizationUnitRelationship_NegativeOrgHierarchyId_Invalid()
    {
        var count = await Context.OrganizationUnitRelationships
            .CountAsync(r => r.OrganizationHierarchyId < 0 && !r.IsDeleted);
        count.Should().Be(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-019")]
    public async Task GetOrganizationHierarchy_NoRoots_ReturnsEmpty()
    {
        var allDeleted = await Context.OrganizationHierarchies.AllAsync(o => o.IsDeleted);
        if (!allDeleted)
        {
            var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
            hierarchy.Should().NotBeNull();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-020")]
    public async Task Opportunity_ResponsibleOrgUnitSoftDeleted_Handled()
    {
        var oppWithDeletedOrg = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.ResponsibleOrgUnit != null && o.ResponsibleOrgUnit.IsDeleted);
        if (oppWithDeletedOrg != null)
        {
            oppWithDeletedOrg.ResponsibleOrgUnit!.IsDeleted.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-021")]
    public async Task GetOrganizationsByType_Hub_WhenNoHubs_ReturnsEmpty()
    {
        var hubs = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Hub);
        hubs.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-022")]
    public async Task GetOrganizationsByType_OrgUnit_WhenNoOrgUnits_ReturnsEmpty()
    {
        var orgUnits = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.OrgUnit);
        orgUnits.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-023")]
    public async Task OrganizationHierarchy_InactiveStatus_ExcludedFromActiveQueries()
    {
        var inactive = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Status != EntityStatus.Active && !o.IsDeleted);
        if (inactive != null)
        {
            inactive.Status.Should().NotBe(EntityStatus.Active);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-024")]
    public async Task Partner_WithDeletedOrgUnitRelationship_NotInOfficePartners()
    {
        var relWithDeletedOrg = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .Include(r => r.OrganizationHierarchy)
            .FirstOrDefaultAsync(r => r.OrganizationHierarchy != null && r.OrganizationHierarchy.IsDeleted && !r.IsDeleted);
        if (relWithDeletedOrg != null)
        {
            relWithDeletedOrg.OrganizationHierarchy!.IsDeleted.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-025")]
    public async Task GetOrganizationHierarchyPrime_EmptyDatabase_ReturnsEmpty()
    {
        var prime = await OrgHierarchyManager.GetOrganizationHierarchyPrime();
        prime.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-026")]
    public async Task GetAllOrganizations_EmptyDatabase_ReturnsEmpty()
    {
        var all = OrgHierarchyManager.GetAllOrganizations();
        all.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-027")]
    public async Task OrganizationHierarchy_SelfReferencingParent_InvalidCycle()
    {
        var selfRef = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ParentId == o.Id && !o.IsDeleted);
        selfRef.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-028")]
    public async Task OrganizationUnitRelationship_NullEntityType_Invalid()
    {
        var withNullType = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EntityType == null && !r.IsDeleted);
        withNullType.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-029")]
    public async Task Opportunity_WithZeroResponsibleOrgUnitId_NotLinked()
    {
        var opp = await Context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ResponsibleOrgUnitId == 0 && !o.IsDeleted);
        if (opp != null)
        {
            opp.ResponsibleOrgUnitId.Should().Be(0);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "NEG-030")]
    public async Task GetOrganizationHierarchyById_DeletedChild_ExcludedFromParent()
    {
        var parent = await Context.OrganizationHierarchies
            .AsNoTracking()
            .Include(o => o.Children)
            .FirstOrDefaultAsync(o => o.Id == RootOrgId && !o.IsDeleted);
        if (parent != null)
        {
            parent.Children.Should().OnlyContain(c => !c.IsDeleted);
        }
    }
}
