using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.EdgeCases;

/// <summary>
/// Edge case tests for OrganizationHierarchyManager
/// </summary>
public class OrganizationHierarchyManagerEdgeCaseTests : ManagerTestBase
{
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
            Id = 1,
            Code = "",
            Name = "Empty Code Org",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Empty Code Organization Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().BeEmpty();
    }

    [Fact]
    public async Task Organization_WithSelfReferenceParent_Should_BeStorable()
    {
        // Arrange - Note: Business logic should prevent this
        var org = new OrganizationHierarchy
        {
            Id = 1,
            Code = "SELF",
            Name = "Self Reference",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Self Reference Description",
            ParentId = 1 // Self-reference
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.ParentId.Should().Be(1);
    }

    [Fact]
    public async Task Organization_WithVeryLongName_Should_BeHandled()
    {
        // Arrange
        var longName = new string('O', 200); // Name is limited to 200 chars per config
        var org = new OrganizationHierarchy
        {
            Id = 1,
            Code = "LONG",
            Name = longName,
            Type = OrganizationUnitType.OrgUnit,
            Description = "Long Name Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(1);

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
            Id = 1,
            Code = "ORG",
            Name = "Org Unit",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Org Unit Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies
            .Where(o => o.Type == OrganizationUnitType.Hub)
            .ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Organization_WithNonExistentParentId_Should_BeStorable()
    {
        // Arrange
        var org = new OrganizationHierarchy
        {
            Id = 1,
            Code = "ORPHAN",
            Name = "Orphan Org",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Orphan Organization Description",
            ParentId = 999 // Non-existent parent
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.ParentId.Should().Be(999);
    }

    [Fact]
    public async Task Organization_WithUnicodeName_Should_BeHandled()
    {
        // Arrange
        var org = new OrganizationHierarchy
        {
            Id = 1,
            Code = "UNICODE",
            Name = "組織 🏢 Организация",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Unicode Organization Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("🏢");
    }

    [Fact]
    public async Task GetAllOrganizations_EmptyDatabase_Should_ReturnEmpty()
    {
        // Act
        var result = await Context.OrganizationHierarchies.ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Organization_WithAllTypes_Should_BeQueryable()
    {
        // Arrange
        var orgs = new List<OrganizationHierarchy>
        {
            new() { Id = 1, Code = "R1", Name = "Region", Type = OrganizationUnitType.Region, Description = "Region Description" },
            new() { Id = 2, Code = "H1", Name = "Hub", Type = OrganizationUnitType.Hub, Description = "Hub Description" },
            new() { Id = 3, Code = "O1", Name = "OrgUnit", Type = OrganizationUnitType.OrgUnit, Description = "OrgUnit Description" }
        };
        await Context.OrganizationHierarchies.AddRangeAsync(orgs);
        await SaveChangesAsync();

        // Act
        var regions = await Context.OrganizationHierarchies.Where(o => o.Type == OrganizationUnitType.Region).CountAsync();
        var hubs = await Context.OrganizationHierarchies.Where(o => o.Type == OrganizationUnitType.Hub).CountAsync();
        var orgUnits = await Context.OrganizationHierarchies.Where(o => o.Type == OrganizationUnitType.OrgUnit).CountAsync();

        // Assert
        regions.Should().Be(1);
        hubs.Should().Be(1);
        orgUnits.Should().Be(1);
    }
}
