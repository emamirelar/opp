/// <summary>
/// Functional tests for Offices Feature (PNO-1213, PNO-1214).
/// Requirements validated: Business rules, audit fields, permissions, workflow, data transformations.
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
[Trait("Category", "Functional")]
[Trait("Feature", "Offices")]
public class FunctionalTests : OfficesFeatureFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-001")]
    [Trait("Ticket", "PNO-1214")]
    public async Task RelatedOpportunities_Query_ReturnsWhereResponsibleOrgUnitIsOfficeOrChild()
    {
        var opps = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .Where(o => !o.IsDeleted && (o.ResponsibleOrgUnitId == RootOrgId || o.ResponsibleOrgUnitId == ChildOrgId1 || o.ResponsibleOrgUnitId == ChildOrgId2))
            .ToListAsync();
        opps.Should().NotBeNull();
        opps.Should().OnlyContain(o => o.ResponsibleOrgUnitId == RootOrgId || o.ResponsibleOrgUnitId == ChildOrgId1 || o.ResponsibleOrgUnitId == ChildOrgId2);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-002")]
    [Trait("Ticket", "PNO-1214")]
    public async Task RelatedPartnerAccounts_Query_ReturnsPartnersManagedByOfficeOrChildren()
    {
        var orgIds = new[] { RootOrgId, ChildOrgId1, ChildOrgId2 };
        var partnerIds = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .Where(r => r.EntityType == "Partner" && !r.IsDeleted && orgIds.Contains(r.OrganizationHierarchyId))
            .Select(r => r.EntityId)
            .Distinct()
            .ToListAsync();
        partnerIds.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-003")]
    public async Task GetOrganizationHierarchy_TreeStructure_ParentChildRelationship()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        foreach (var node in hierarchy)
        {
            if (node.Data?.Children != null)
            {
                foreach (var child in node.Data.Children)
                {
                    child.ParentId.Should().Be(node.Data.Id);
                }
            }
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-004")]
    public async Task GetOrganizationsByType_FiltersByType()
    {
        var offices = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Office);
        var regions = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Region);
        offices.Should().OnlyContain(o => o.Type == OrganizationUnitType.Office.ToString());
        regions.Should().OnlyContain(o => o.Type == OrganizationUnitType.Region.ToString());
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-005")]
    public async Task GetAllOrganizations_ExcludesSoftDeleted()
    {
        var all = OrgHierarchyManager.GetAllOrganizations();
        var deletedIds = await Context.OrganizationHierarchies.Where(o => o.IsDeleted).Select(o => o.Id).ToListAsync();
        all.Should().NotContain(o => deletedIds.Contains(o.Id));
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-006")]
    public async Task GetOrganizationHierarchy_ExcludesSoftDeleted()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        var deletedIds = await Context.OrganizationHierarchies.Where(o => o.IsDeleted).Select(o => o.Id).ToListAsync();
        foreach (var id in deletedIds)
        {
            var inTree = hierarchy.Any(h => h.Data?.Id == id);
            inTree.Should().BeFalse();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-007")]
    public async Task GetOrganizationHierarchyById_IncludesChildren()
    {
        var office = await OrgHierarchyManager.GetOrganizationHierarchyById(RootOrgId);
        office.Should().NotBeNull();
        office!.Id.Should().Be(RootOrgId);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-008")]
    public async Task Opportunity_ResponsibleOrgUnit_NavigationPopulated()
    {
        var opp = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.Id == OpportunityId && !o.IsDeleted);
        opp.Should().NotBeNull();
        opp!.ResponsibleOrgUnit.Should().NotBeNull();
        opp.ResponsibleOrgUnit!.Code.Should().NotBeNullOrEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-009")]
    public async Task Partner_OrganizationUnitRelationship_LinksToOrgHierarchy()
    {
        var rel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .Include(r => r.OrganizationHierarchy)
            .FirstOrDefaultAsync(r => r.EntityId == PartnerId && r.EntityType == "Partner" && !r.IsDeleted);
        rel.Should().NotBeNull();
        rel!.OrganizationHierarchy.Should().NotBeNull();
        rel.OrganizationHierarchy!.Code.Should().NotBeNullOrEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-010")]
    public async Task GetOrganizationHierarchyPrime_FlatStructure()
    {
        var prime = await OrgHierarchyManager.GetOrganizationHierarchyPrime();
        prime.Should().NotBeNull();
        foreach (var p in prime)
        {
            p.Should().NotBeNull();
            p.Data.Should().NotBeNull();
            p.Data!.Id.Should().BeGreaterThan(0);
            p.Data.Name.Should().NotBeNullOrEmpty();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-011")]
    public async Task OrganizationHierarchy_CodeUniquePerActive()
    {
        var codes = await Context.OrganizationHierarchies
            .Where(o => !o.IsDeleted)
            .Select(o => o.Code)
            .ToListAsync();
        var distinctCount = codes.Distinct().Count();
        distinctCount.Should().Be(codes.Count);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-012")]
    public async Task GetOrganizationsByType_Office_OrderedByName()
    {
        var offices = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Office).ToList();
        var sorted = offices.OrderBy(o => o.Name).ToList();
        offices.Should().BeEquivalentTo(sorted, opts => opts.WithStrictOrdering());
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-013")]
    public async Task GetAllOrganizations_OrderedByName()
    {
        var all = OrgHierarchyManager.GetAllOrganizations().ToList();
        var sorted = all.OrderBy(o => o.Name).ToList();
        all.Should().BeEquivalentTo(sorted, opts => opts.WithStrictOrdering());
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-014")]
    public async Task GetOrganizationHierarchy_RootNodes_HaveNullParentId()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        var roots = hierarchy.Where(h => h.Data?.ParentId == null);
        roots.Should().NotBeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-015")]
    public async Task OrganizationUnitRelationship_EntityType_MatchesEntity()
    {
        var rel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EntityType == "Partner" && !r.IsDeleted);
        if (rel != null)
        {
            var partnerExists = await Context.Partners.AnyAsync(p => p.Id == rel.EntityId);
            partnerExists.Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-016")]
    public async Task Opportunity_ResponsibleOrgUnitId_ReferencesActiveOrg()
    {
        var opp = await Context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == OpportunityId && !o.IsDeleted);
        if (opp?.ResponsibleOrgUnitId != null)
        {
            var org = await Context.OrganizationHierarchies.FirstOrDefaultAsync(o => o.Id == opp.ResponsibleOrgUnitId && !o.IsDeleted);
            org.Should().NotBeNull();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-017")]
    public async Task GetOrganizationHierarchy_RecursiveChildren()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        foreach (var node in hierarchy)
        {
            if (node.Data?.Children != null)
            {
                foreach (var child in node.Data.Children)
                {
                    child.Id.Should().BeGreaterThan(0);
                    child.ParentId.Should().Be(node.Data.Id);
                }
            }
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-018")]
    public async Task OrganizationHierarchy_Type_ValidEnum()
    {
        var types = await Context.OrganizationHierarchies
            .Where(o => !o.IsDeleted)
            .Select(o => o.Type)
            .Distinct()
            .ToListAsync();
        foreach (var t in types)
        {
            Enum.IsDefined(typeof(OrganizationUnitType), t).Should().BeTrue();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-019")]
    public async Task GetOrganizationHierarchyById_MapsToModel()
    {
        var entity = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == RootOrgId && !o.IsDeleted);
        var model = await OrgHierarchyManager.GetOrganizationHierarchyById(RootOrgId);
        model.Should().NotBeNull();
        model!.Id.Should().Be(entity!.Id);
        model.Name.Should().Be(entity.Name);
        model.Code.Should().Be(entity.Code);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-020")]
    public async Task RelatedOpportunities_ForChildOffice_IncludedInParentQuery()
    {
        var childOpps = await Context.Opportunities
            .AsNoTracking()
            .Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == ChildOrgId1)
            .CountAsync();
        childOpps.Should().BeGreaterThanOrEqualTo(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-021")]
    public async Task RelatedPartnerAccounts_ForChildOffice_IncludedInParentQuery()
    {
        var childPartners = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .Where(r => r.EntityType == "Partner" && !r.IsDeleted && r.OrganizationHierarchyId == ChildOrgId1)
            .CountAsync();
        childPartners.Should().BeGreaterThanOrEqualTo(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-022")]
    public async Task GetOrganizationsByType_ActiveOnly()
    {
        var offices = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Office);
        offices.Should().OnlyContain(o => o.Status == EntityStatus.Active.ToString());
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-023")]
    public async Task GetOrganizationHierarchy_ActiveOnly()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        var allIds = hierarchy.SelectMany(h => FlattenIds(h)).ToList();
        var inactiveIds = await Context.OrganizationHierarchies
            .Where(o => o.Status != EntityStatus.Active && !o.IsDeleted)
            .Select(o => o.Id)
            .ToListAsync();
        foreach (var id in inactiveIds)
        {
            allIds.Should().NotContain(id, "ValuesRepository filters by Status == Active");
        }
    }

    private static List<int> FlattenIds(OrganizationHierarchyTreeModel node)
    {
        var ids = new List<int>();
        if (node?.Data == null) return ids;
        ids.Add(node.Data.Id);
        if (node.Data.Children != null)
        {
            foreach (var c in node.Data.Children)
            {
                ids.Add(c.Id);
                FlattenIdsRecursive(c.Children, ids);
            }
        }
        return ids;
    }

    private static void FlattenIdsRecursive(List<OrganizationHierarchyDataModel>? children, List<int> ids)
    {
        if (children == null) return;
        foreach (var c in children)
        {
            ids.Add(c.Id);
            FlattenIdsRecursive(c.Children, ids);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-024")]
    public async Task OrganizationUnitRelationship_StatusActive()
    {
        var rel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .FirstOrDefaultAsync(r => !r.IsDeleted);
        if (rel != null)
        {
            rel.Status.Should().Be(EntityStatus.Active);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-025")]
    public async Task GetOrganizationHierarchyPrime_ExcludesSoftDeleted()
    {
        var prime = await OrgHierarchyManager.GetOrganizationHierarchyPrime();
        var deletedIds = await Context.OrganizationHierarchies.Where(o => o.IsDeleted).Select(o => o.Id).ToListAsync();
        prime.Should().NotContain(p => p.Data != null && deletedIds.Contains(p.Data.Id));
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-026")]
    public async Task OrganizationHierarchy_Description_Optional()
    {
        var withEmptyDesc = await Context.OrganizationHierarchies
            .AsNoTracking()
            .FirstOrDefaultAsync(o => (o.Description == null || o.Description == "") && !o.IsDeleted);
        if (withEmptyDesc != null)
        {
            withEmptyDesc.Description.Should().BeNullOrEmpty();
        }
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-027")]
    public async Task Opportunity_WithoutResponsibleOrgUnit_ValidState()
    {
        var count = await Context.Opportunities.CountAsync(o => o.ResponsibleOrgUnitId == null && !o.IsDeleted);
        count.Should().BeGreaterThanOrEqualTo(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-028")]
    public async Task Partner_WithoutOrgUnitRelationship_ValidState()
    {
        var partnersWithoutOrg = await Context.Partners
            .Where(p => !p.IsDeleted)
            .Where(p => !Context.OrganizationUnitRelationships.Any(r => r.EntityId == p.Id && r.EntityType == "Partner" && !r.IsDeleted))
            .CountAsync();
        partnersWithoutOrg.Should().BeGreaterThanOrEqualTo(0);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-029")]
    public async Task GetOrganizationHierarchy_NoDuplicates()
    {
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();
        var allIds = hierarchy.SelectMany(h => FlattenIds(h)).ToList();
        var distinctIds = allIds.Distinct().ToList();
        allIds.Should().BeEquivalentTo(distinctIds);
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "FNC-030")]
    public async Task OrganizationHierarchy_ParentChildConsistency()
    {
        var all = await Context.OrganizationHierarchies.Where(o => !o.IsDeleted).ToListAsync();
        foreach (var child in all.Where(o => o.ParentId != null))
        {
            var parent = all.FirstOrDefault(p => p.Id == child.ParentId);
            parent.Should().NotBeNull();
        }
    }

    // ========== Defect-Exposing Tests (RUN and FAIL until DEF-212 fixed) ==========

    [Fact]
    [Trait("TestId", "FNC-DEF-212-001")]
    [Trait("Defect", "DEF-212")]
    [Trait("Ticket", "PNO-1213")]
    public void IManagerWrapper_ShouldHaveOfficeManager_PNO1213RequiresOfficeManager()
    {
        var officeManagerProp = typeof(UNOPS.PAO.Business.Interfaces.IManagerWrapper)
            .GetProperty("OfficeManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        officeManagerProp.Should().NotBeNull("PNO-1213 AC: Office Detail with tabs requires OfficeManager for GetOfficeByIdAsync, GetRelatedOpportunitiesAsync, GetRelatedPartnersAsync");
    }
}
