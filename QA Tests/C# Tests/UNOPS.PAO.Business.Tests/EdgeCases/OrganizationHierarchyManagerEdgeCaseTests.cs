using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.EdgeCases;

/// <summary>
/// Edge case tests for OrganizationHierarchyManager against PostgreSQL.
/// Uses test markers and auto-generated IDs for data isolation.
/// </summary>
public class OrganizationHierarchyManagerEdgeCaseTests : ManagerTestBase
{
    private readonly string _testMarker = $"OHEC_{Guid.NewGuid():N}";

    [Fact]
    public async Task GetOrganizationById_WithZeroId_Should_ReturnNull()
    {
        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Organization_WithEmptyCode_Should_BeStorable()
    {
        // Arrange
        var org = new OrganizationHierarchy
        {
            Code = "",
            Name = $"Empty Code Org {_testMarker}",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Empty Code Organization Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {org.Id}");

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(org.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().BeEmpty();
    }

    [Fact]
    public async Task Organization_WithSelfReferenceParent_Should_BeStorable()
    {
        // Arrange - Note: Business logic should prevent this
        // First create org, then set self-reference
        var org = new OrganizationHierarchy
        {
            Code = $"SELF_{_testMarker}",
            Name = $"Self Reference {_testMarker}",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Self Reference Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {org.Id}");

        // Set self-reference
        org.ParentId = org.Id;
        await SaveChangesAsync();

        // Act
        Context.ChangeTracker.Clear();
        var result = await Context.OrganizationHierarchies.FindAsync(org.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ParentId.Should().Be(org.Id);
    }

    [Fact]
    public async Task Organization_WithVeryLongName_Should_BeHandled()
    {
        // Arrange
        var longName = new string('O', 200);
        var org = new OrganizationHierarchy
        {
            Code = $"LONG_{_testMarker}",
            Name = longName,
            Type = OrganizationUnitType.OrgUnit,
            Description = "Long Name Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {org.Id}");

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(org.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Length.Should().Be(200);
    }

    [Fact]
    public async Task GetOrganizationsByType_WithNoMatches_Should_ReturnEmpty()
    {
        // Arrange
        var org = new OrganizationHierarchy
        {
            Code = $"ORG_{_testMarker}",
            Name = $"Org Unit {_testMarker}",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Org Unit Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {org.Id}");

        // Act - Search for Hub type among our test data only
        var result = await Context.OrganizationHierarchies
            .Where(o => o.Name.Contains(_testMarker) && o.Type == OrganizationUnitType.Hub)
            .ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [SkipIfNotPostgreSQLFact]
    public async Task Organization_WithNonExistentParentId_ShouldBeRejectedByForeignKey()
    {
        // Arrange - PostgreSQL enforces FK constraints, so a non-existent ParentId
        // should be rejected. This is the correct referential integrity behavior.
        var org = new OrganizationHierarchy
        {
            Code = $"ORPHAN_{_testMarker}",
            Name = $"Orphan Org {_testMarker}",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Orphan Organization Description",
            ParentId = 999999 // Non-existent parent
        };
        await Context.OrganizationHierarchies.AddAsync(org);

        // Act & Assert - FK constraint should reject the orphan record
        var action = async () => await SaveChangesAsync();
        await action.Should().ThrowAsync<DbUpdateException>(
            "PostgreSQL enforces FK constraints - non-existent ParentId should be rejected");
    }

    [Fact]
    public async Task Organization_WithUnicodeName_Should_BeHandled()
    {
        // Arrange
        var org = new OrganizationHierarchy
        {
            Code = $"UNICODE_{_testMarker}",
            Name = $"組織 🏢 Организация {_testMarker}",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Unicode Organization Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {org.Id}");

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(org.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("🏢");
    }

    [Fact]
    public async Task GetAllOrganizations_EmptyDatabase_Should_ReturnFilteredEmpty()
    {
        // Act - Use a marker that won't match anything
        var result = await Context.OrganizationHierarchies
            .Where(o => o.Name == "NONEXISTENT_MARKER_ZZZZZ")
            .ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Organization_WithAllTypes_Should_BeQueryable()
    {
        // Arrange
        var orgs = new List<OrganizationHierarchy>
        {
            new() { Code = $"R1_{_testMarker}", Name = $"Region {_testMarker}", Type = OrganizationUnitType.Region, Description = "Region Description" },
            new() { Code = $"H1_{_testMarker}", Name = $"Hub {_testMarker}", Type = OrganizationUnitType.Hub, Description = "Hub Description" },
            new() { Code = $"O1_{_testMarker}", Name = $"OrgUnit {_testMarker}", Type = OrganizationUnitType.OrgUnit, Description = "OrgUnit Description" }
        };
        await Context.OrganizationHierarchies.AddRangeAsync(orgs);
        await SaveChangesAsync();
        foreach (var o in orgs) RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {o.Id}");

        // Act - Count only our test data
        var regions = await Context.OrganizationHierarchies.CountAsync(o => o.Name.Contains(_testMarker) && o.Type == OrganizationUnitType.Region);
        var hubs = await Context.OrganizationHierarchies.CountAsync(o => o.Name.Contains(_testMarker) && o.Type == OrganizationUnitType.Hub);
        var orgUnits = await Context.OrganizationHierarchies.CountAsync(o => o.Name.Contains(_testMarker) && o.Type == OrganizationUnitType.OrgUnit);

        // Assert
        regions.Should().Be(1);
        hubs.Should().Be(1);
        orgUnits.Should().Be(1);
    }
}
