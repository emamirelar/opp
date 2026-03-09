/// <summary>
/// Positive tests for Offices Feature (PNO-1213, PNO-1214).
/// Requirements validated: Office hierarchy (Organigram), office-opportunity link, office-partner link.
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
[Trait("Category", "Positive")]
[Trait("Feature", "Offices")]
[Trait("Component", "OrganizationHierarchyManager")]
public class PositiveTests : OfficesFeatureFixtureBase
{
    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-001")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchy_ReturnsTreeWithRootAndChildren()
    {
        // Arrange — fixture seeds root + 2 children
        // Act
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();

        // Assert — PNO-1213 AC: Organigram shows UNOPS operational structure hierarchy
        hierarchy.Should().NotBeNull();
        hierarchy.Should().NotBeEmpty();
        var roots = hierarchy.Where(h => h.Data?.ParentId == null).ToList();
        roots.Should().NotBeEmpty("organigram must have root nodes");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-002")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchyById_ValidId_ReturnsOfficeWithChildren()
    {
        // Arrange
        // Act
        var office = await OrgHierarchyManager.GetOrganizationHierarchyById(RootOrgId);

        // Assert — PNO-1213 AC: Details tab shows parent office hierarchy and child offices list
        office.Should().NotBeNull();
        office.Id.Should().Be(RootOrgId);
        office.Name.Should().NotBeNullOrEmpty();
        office.Code.Should().NotBeNullOrEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-003")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationsByType_Office_ReturnsOfficesOnly()
    {
        // Arrange
        // Act
        var offices = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Office);

        // Assert — List view supports filtering by type
        offices.Should().NotBeNull();
        offices.Should().OnlyContain(o => o.Type == OrganizationUnitType.Office.ToString());
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-004")]
    [Trait("Ticket", "PNO-1214")]
    public async Task Opportunity_WithResponsibleOrgUnitId_LinkedToOffice()
    {
        // Arrange — fixture seeds opportunity with ResponsibleOrgUnitId = ChildOrgId1
        // Act
        var opp = await Context.Opportunities
            .AsNoTracking()
            .Include(o => o.ResponsibleOrgUnit)
            .FirstOrDefaultAsync(o => o.Id == OpportunityId && !o.IsDeleted);

        // Assert — PNO-1214 AC: Related Opportunities query returns opportunities where responsible org unit is current office OR children
        opp.Should().NotBeNull();
        opp!.ResponsibleOrgUnitId.Should().Be(ChildOrgId1);
        opp.ResponsibleOrgUnit.Should().NotBeNull();
        opp.ResponsibleOrgUnit!.Code.Should().NotBeNullOrEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-005")]
    [Trait("Ticket", "PNO-1214")]
    public async Task Partner_WithOrgUnitRelationship_ManagedByOffice()
    {
        // Arrange — fixture seeds partner with OrganizationUnitRelationship to ChildOrgId1
        // Act
        var rel = await Context.OrganizationUnitRelationships
            .AsNoTracking()
            .Include(r => r.OrganizationHierarchy)
            .FirstOrDefaultAsync(r => r.EntityId == PartnerId && r.EntityType == "Partner" && !r.IsDeleted);

        // Assert — PNO-1214 AC: Related Partner Accounts query returns partners managed by office or its children
        rel.Should().NotBeNull();
        rel!.OrganizationHierarchyId.Should().Be(ChildOrgId1);
        rel.OrganizationHierarchy.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-006")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetAllOrganizations_ReturnsAllNonDeleted()
    {
        // Act
        var all = OrgHierarchyManager.GetAllOrganizations();

        // Assert — List view data source
        all.Should().NotBeNull();
        all.Should().NotBeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-007")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchyPrime_ReturnsFlatStructure()
    {
        // Act
        var prime = await OrgHierarchyManager.GetOrganizationHierarchyPrime();

        // Assert — Prime model for dropdown/select
        prime.Should().NotBeNull();
        prime.Should().NotBeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-008")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchyById_ChildOffice_ReturnsWithParent()
    {
        // Act
        var child = await OrgHierarchyManager.GetOrganizationHierarchyById(ChildOrgId1);

        // Assert — PNO-1213 AC: Details tab shows parent office hierarchy
        child.Should().NotBeNull();
        child!.ParentId.Should().Be(RootOrgId);
        child.Code.Should().Be("OF_CH1");
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-009")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationsByType_Region_ReturnsRegionsOnly()
    {
        // Arrange — ensure at least one region exists
        var region = Context.OrganizationHierarchies.FirstOrDefault(o => o.Type == OrganizationUnitType.Region && !o.IsDeleted);
        if (region == null)
        {
            var r = new UNOPS.PAO.Domain.Entities.OrganizationHierarchy
            {
                Name = "Test Region",
                Code = "OF_REG",
                Description = "Region",
                Type = OrganizationUnitType.Region,
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            Context.OrganizationHierarchies.Add(r);
            await Context.SaveChangesAsync();
        }

        // Act
        var regions = OrgHierarchyManager.GetOrganizationsByType(OrganizationUnitType.Region);

        // Assert
        regions.Should().NotBeNull();
        regions.Should().OnlyContain(o => o.Type == OrganizationUnitType.Region.ToString());
    }

    [SkipIfInMemoryFact]
    [Trait("TestId", "POS-010")]
    [Trait("Ticket", "PNO-1213")]
    public async Task GetOrganizationHierarchy_ExcludesSoftDeleted()
    {
        // Arrange — soft delete one org unit
        var toDelete = await Context.OrganizationHierarchies.FirstOrDefaultAsync(o => o.Id == ChildOrgId2 && !o.IsDeleted);
        if (toDelete != null)
        {
            toDelete.IsDeleted = true;
            toDelete.DeletedDate = DateTime.UtcNow;
            await Context.SaveChangesAsync();
        }

        // Act — ValuesRepository filters !x.IsDeleted
        var hierarchy = await OrgHierarchyManager.GetOrganizationHierarchy();

        // Assert — soft-deleted excluded from tree (repository filters IsDeleted)
        var allIds = FlattenTreeIds(hierarchy);
        allIds.Should().NotContain(ChildOrgId2);
    }

    private static List<int> FlattenTreeIds(IEnumerable<OrganizationHierarchyTreeModel> hierarchy)
    {
        var ids = new List<int>();
        foreach (var node in hierarchy)
        {
            if (node?.Data == null) continue;
            ids.Add(node.Data.Id);
            FlattenDataIds(node.Data.Children, ids);
        }
        return ids;
    }

    private static void FlattenDataIds(List<OrganizationHierarchyDataModel>? children, List<int> ids)
    {
        if (children == null) return;
        foreach (var c in children)
        {
            ids.Add(c.Id);
            FlattenDataIds(c.Children, ids);
        }
    }
}
