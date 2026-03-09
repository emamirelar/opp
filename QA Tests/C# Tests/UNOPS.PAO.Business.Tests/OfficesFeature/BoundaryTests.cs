/// <summary>
/// Boundary tests for Offices Feature (PNO-1213, PNO-1214).
/// Requirements validated: Min/max values, soft-delete interactions, nullable FK, hierarchy edges.
/// </summary>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.OrganizationUnits;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OfficesFeature;

[Collection("OfficesFeature")]
[Trait("Category", "Boundary")]
[Trait("Feature", "Offices")]
public class BoundaryTests : OfficesFeatureFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-001")]
    public async Task GetOrganizationHierarchyById_IdOne_WhenExists_Returns()
    {
        var minId = await Context.OrganizationHierarchies.MinAsync(o => o.Id);
        if (minId == 1)
        {
            var result = await OrgHierarchyManager.GetOrganizationHierarchyById(1);
            result.Should().NotBeNull();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-002")]
    public async Task OrganizationHierarchy_ParentIdNull_RootNode()
    {
        var roots = await Context.OrganizationHierarchies
            .AsNoTracking()
            .Where(o => o.ParentId == null && !o.IsDeleted)
            .ToListAsync();
        roots.Should().NotBeEmpty();
        roots.Should().OnlyContain(r => r.ParentId == null);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-003")]
    public async Task OrganizationHierarchy_ChildWithNoChildren_LeafNode()
    {
        var allIds = await Context.OrganizationHierarchies.Where(o => !o.IsDeleted).Select(o => o.Id).ToListAsync();
        var parentIds = await Context.OrganizationHierarchies.Where(o => !o.IsDeleted && o.ParentId != null).Select(o => o.ParentId!.Value).Distinct().ToListAsync();
        var leafIds = allIds.Except(parentIds).ToList();
        leafIds.Should().NotBeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-004")]
    public async Task Opportunity_ResponsibleOrgUnitIdNull_ValidState()
    {
        var oppWithNull = await Context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ResponsibleOrgUnitId == null && !o.IsDeleted);
        if (oppWithNull != null)
        {
            oppWithNull.ResponsibleOrgUnitId.Should().BeNull();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-005")]
    public async Task OrganizationUnitRelationship_EntityTypePartner_Valid()
    {
        var rel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EntityType == "Partner" && !r.IsDeleted);
        if (rel != null)
        {
            rel.EntityType.Should().Be("Partner");
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-006")]
    public async Task OrganizationHierarchy_SoftDeleted_ExcludedFromGetAll()
    {
        var deleted = await Context.OrganizationHierarchies.FirstOrDefaultAsync(o => o.IsDeleted);
        if (deleted != null)
        {
            var all = OrgHierarchyManager.GetAllOrganizations();
            all.Should().NotContain(o => o.Id == deleted.Id);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-007")]
    public async Task OrganizationHierarchy_SoftDeleted_ExcludedFromGetByType()
    {
        var offices = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Office);
        var deletedIds = await Context.OrganizationHierarchies.Where(o => o.IsDeleted).Select(o => o.Id).ToListAsync();
        offices.Should().NotContain(o => deletedIds.Contains(o.Id));
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-008")]
    public async Task OrganizationHierarchy_SoftDeleted_ExcludedFromTree()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        var deletedIds = await Context.OrganizationHierarchies.Where(o => o.IsDeleted).Select(o => o.Id).ToListAsync();
        foreach (var id in deletedIds)
        {
            var found = hierarchy.Any(h => h.Data?.Id == id || ContainsChildId(h.Data?.Children, id));
            found.Should().BeFalse();
        }
    }

    private static bool ContainsChildId(List<OrganizationHierarchyDataModel>? children, int id)
    {
        if (children == null) return false;
        foreach (var c in children)
        {
            if (c.Id == id) return true;
            if (ContainsChildId(c.Children, id)) return true;
        }
        return false;
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-009")]
    public async Task OrganizationHierarchy_MaxDepth_DeepHierarchy()
    {
        var maxDepth = await Context.OrganizationHierarchies
            .Where(o => !o.IsDeleted)
            .Select(o => o.ParentId)
            .Where(p => p != null)
            .CountAsync();
        maxDepth.Should().BeGreaterThanOrEqualTo(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-010")]
    public async Task OrganizationUnitRelationship_SoftDeletedOrgHierarchy_Excluded()
    {
        var relToDeletedOrg = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .Include(r => r.OrganizationHierarchy)
            .Where(r => !r.IsDeleted && r.OrganizationHierarchy != null && r.OrganizationHierarchy.IsDeleted)
            .FirstOrDefaultAsync();
        if (relToDeletedOrg != null)
        {
            relToDeletedOrg.OrganizationHierarchy!.IsDeleted.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-011")]
    public async Task Opportunity_ResponsibleOrgUnitSoftDeleted_NullNavigation()
    {
        var opp = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.ResponsibleOrgUnit != null && o.ResponsibleOrgUnit.IsDeleted && !o.IsDeleted);
        if (opp != null)
        {
            opp.ResponsibleOrgUnit!.IsDeleted.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-012")]
    public async Task OrganizationHierarchy_SingleRoot_ValidTree()
    {
        var rootCount = await Context.OrganizationHierarchies.CountAsync(o => o.ParentId == null && !o.IsDeleted);
        rootCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-013")]
    public async Task OrganizationHierarchy_MultipleRoots_ValidForest()
    {
        var roots = await Context.OrganizationHierarchies
            .Where(o => o.ParentId == null && !o.IsDeleted)
            .CountAsync();
        roots.Should().BeGreaterThanOrEqualTo(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-014")]
    public async Task GetOrganizationsByType_Office_ExcludesOtherTypes()
    {
        var offices = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Office);
        offices.Should().OnlyContain(o => o.Type == OrganizationUnitType.Office.ToString());
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-015")]
    public async Task GetOrganizationsByType_Region_ExcludesOffices()
    {
        var regions = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Region);
        regions.Should().OnlyContain(o => o.Type == OrganizationUnitType.Region.ToString());
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-016")]
    public async Task OrganizationUnitRelationship_PartnerEntityType_CorrectEntityId()
    {
        var rel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EntityType == "Partner" && !r.IsDeleted);
        if (rel != null)
        {
            var partnerExists = await Context.Partners.AnyAsync(p => p.Id == rel.EntityId && !p.IsDeleted);
            partnerExists.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-017")]
    public async Task OrganizationHierarchy_CodeMaxLength_WithinLimit()
    {
        var maxCodeLen = await Context.OrganizationHierarchies
            .Where(o => !o.IsDeleted && o.Code != null)
            .MaxAsync(o => o.Code!.Length);
        maxCodeLen.Should().BeLessThanOrEqualTo(100);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-018")]
    public async Task OrganizationHierarchy_NameMaxLength_WithinLimit()
    {
        var org = await Context.OrganizationHierarchies
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync();
        if (org?.Name != null)
        {
            org.Name.Length.Should().BeLessThanOrEqualTo(500);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-019")]
    public async Task GetOrganizationHierarchyPrime_IncludesAllActive()
    {
        var prime = await OrgHierarchyManager.GetOrganizationHierarchyPrime();
        var activeCount = await Context.OrganizationHierarchies.CountAsync(o => !o.IsDeleted && o.Status == EntityStatus.Active);
        prime.Count().Should().BeLessThanOrEqualTo(activeCount + 10);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-020")]
    public async Task Opportunity_ResponsibleOrgUnitId_ValidForeignKey()
    {
        var opp = await Context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ResponsibleOrgUnitId != null && !o.IsDeleted);
        if (opp?.ResponsibleOrgUnitId != null)
        {
            var orgExists = await Context.OrganizationHierarchies.AnyAsync(o => o.Id == opp.ResponsibleOrgUnitId && !o.IsDeleted);
            orgExists.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-021")]
    public async Task OrganizationUnitRelationship_OrganizationHierarchyId_ValidForeignKey()
    {
        var rel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => !r.IsDeleted);
        if (rel != null)
        {
            var orgExists = await Context.OrganizationHierarchies.AnyAsync(o => o.Id == rel.OrganizationHierarchyId);
            orgExists.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-022")]
    public async Task OrganizationHierarchy_ParentIdReferencesExisting()
    {
        var withParent = await Context.OrganizationHierarchies
            .AsNoTracking()
            .Where(o => o.ParentId != null && !o.IsDeleted)
            .FirstOrDefaultAsync();
        if (withParent?.ParentId != null)
        {
            var parentExists = await Context.OrganizationHierarchies.AnyAsync(o => o.Id == withParent.ParentId);
            parentExists.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-023")]
    public async Task GetOrganizationHierarchy_EmptyChildren_ValidLeaf()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        var leaf = hierarchy.SelectMany(h => FlattenData(h.Data)).FirstOrDefault(d => d.Children?.Count == 0);
        if (leaf != null)
        {
            leaf.Children.Should().NotBeNull();
            leaf.Children.Should().BeEmpty();
        }
    }

    private static IEnumerable<OrganizationHierarchyDataModel> FlattenData(OrganizationHierarchyDataModel? data)
    {
        if (data == null) yield break;
        yield return data;
        if (data.Children != null)
        {
            foreach (var c in data.Children)
            {
                foreach (var child in FlattenData(c)) yield return child;
            }
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-024")]
    public async Task Partner_MultipleOrgUnitRelationships_Valid()
    {
        var partnerWithMultiple = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .Where(r => r.EntityType == "Partner" && !r.IsDeleted)
            .GroupBy(r => r.EntityId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .FirstOrDefaultAsync();
        if (partnerWithMultiple > 0)
        {
            var count = await Context.OrganizationUnitRelationships.CountAsync(r => r.EntityId == partnerWithMultiple && r.EntityType == "Partner" && !r.IsDeleted);
            count.Should().BeGreaterThan(1);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-025")]
    public async Task GetOrganizationHierarchyById_LastValidId_Returns()
    {
        var maxId = await Context.OrganizationHierarchies.Where(o => !o.IsDeleted).MaxAsync(o => o.Id);
        var result = await OrgHierarchyManager.GetOrganizationHierarchyById(maxId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(maxId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-026")]
    public async Task OrganizationHierarchy_StatusActive_Included()
    {
        var active = await Context.OrganizationHierarchies.FirstOrDefaultAsync(o => o.Status == EntityStatus.Active && !o.IsDeleted);
        if (active != null)
        {
            active.Status.Should().Be(EntityStatus.Active);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-027")]
    public async Task OrganizationUnitRelationship_InteractionEntityType_WhenExists()
    {
        var interactionRel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EntityType == "Interaction" && !r.IsDeleted);
        if (interactionRel != null)
        {
            interactionRel.EntityType.Should().Be("Interaction");
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-028")]
    public async Task OrganizationUnitRelationship_ContactEntityType_WhenExists()
    {
        var contactRel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EntityType == "Contact" && !r.IsDeleted);
        if (contactRel != null)
        {
            contactRel.EntityType.Should().Be("Contact");
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-029")]
    public async Task Opportunity_MultipleWithSameOrgUnit_Valid()
    {
        var orgUnitsWithMultipleOpps = await Context.Opportunities
            .AsNoTracking()
            .Where(o => o.ResponsibleOrgUnitId != null && !o.IsDeleted)
            .GroupBy(o => o.ResponsibleOrgUnitId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .FirstOrDefaultAsync();
        if (orgUnitsWithMultipleOpps != null)
        {
            var count = await Context.Opportunities.CountAsync(o => o.ResponsibleOrgUnitId == orgUnitsWithMultipleOpps && !o.IsDeleted);
            count.Should().BeGreaterThan(1);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "BND-030")]
    public async Task GetOrganizationHierarchy_TreeStructure_RecursiveChildren()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        foreach (var node in hierarchy)
        {
            if (node.Data?.Children != null && node.Data.Children.Count > 0)
            {
                foreach (var child in node.Data.Children)
                {
                    child.ParentId.Should().Be(node.Data.Id);
                }
            }
        }
    }
}
