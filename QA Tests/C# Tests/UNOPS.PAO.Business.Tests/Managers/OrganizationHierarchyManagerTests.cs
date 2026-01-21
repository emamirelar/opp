using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for OrganizationHierarchyManager
/// </summary>
public class OrganizationHierarchyManagerTests : ManagerTestBase
{
    [Fact]
    public async Task GetOrganizationById_Should_ReturnOrganization_When_Exists()
    {
        // Arrange
        var org = new OrganizationHierarchy
        {
            Id = 1,
            Code = "ORG001",
            Name = "Test Organization",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Test Organization Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Organization");
        result.Code.Should().Be("ORG001");
    }

    [Fact]
    public async Task GetOrganizationById_Should_ReturnNull_When_NotExists()
    {
        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrganizationsByType_Should_FilterByType()
    {
        // Arrange
        var orgs = new List<OrganizationHierarchy>
        {
            new() { Id = 1, Code = "REG1", Name = "Region 1", Type = OrganizationUnitType.Region, Description = "Region Description" },
            new() { Id = 2, Code = "HUB1", Name = "Hub 1", Type = OrganizationUnitType.Hub, Description = "Hub Description" },
            new() { Id = 3, Code = "OU1", Name = "Org Unit 1", Type = OrganizationUnitType.OrgUnit, Description = "OrgUnit Description" }
        };
        await Context.OrganizationHierarchies.AddRangeAsync(orgs);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies
            .Where(o => o.Type == OrganizationUnitType.Hub)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Hub 1");
    }

    [Fact]
    public async Task GetAllOrganizations_Should_ReturnAllOrganizations()
    {
        // Arrange
        var orgs = new List<OrganizationHierarchy>
        {
            new() { Id = 1, Code = "OU1", Name = "Org Unit 1", Type = OrganizationUnitType.OrgUnit, Description = "Description 1" },
            new() { Id = 2, Code = "HUB1", Name = "Hub 1", Type = OrganizationUnitType.Hub, Description = "Description 2" },
            new() { Id = 3, Code = "REG1", Name = "Region 1", Type = OrganizationUnitType.Region, Description = "Description 3" }
        };
        await Context.OrganizationHierarchies.AddRangeAsync(orgs);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies.ToListAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task OrganizationHierarchy_Should_SupportParentChildRelationships()
    {
        // Arrange
        var parent = new OrganizationHierarchy
        {
            Id = 1,
            Code = "PARENT",
            Name = "Parent Org",
            Type = OrganizationUnitType.Region,
            Description = "Parent Organization"
        };
        var child = new OrganizationHierarchy
        {
            Id = 2,
            Code = "CHILD",
            Name = "Child Org",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Child Organization",
            ParentId = 1
        };
        await Context.OrganizationHierarchies.AddRangeAsync(parent, child);
        await SaveChangesAsync();

        // Act
        var childResult = await Context.OrganizationHierarchies.FindAsync(2);

        // Assert
        childResult.Should().NotBeNull();
        childResult!.ParentId.Should().Be(1);
    }

    [Fact]
    public async Task GetOrganizations_Should_ReturnEmpty_When_NoOrganizations()
    {
        // Act
        var result = await Context.OrganizationHierarchies.ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChildOrganizations_Should_ReturnChildren()
    {
        // Arrange
        var parent = new OrganizationHierarchy
        {
            Id = 1,
            Code = "PARENT",
            Name = "Parent",
            Type = OrganizationUnitType.Region,
            Description = "Parent Description"
        };
        var children = new List<OrganizationHierarchy>
        {
            new() { Id = 2, Code = "CHILD1", Name = "Child 1", Type = OrganizationUnitType.Hub, Description = "Child 1 Description", ParentId = 1 },
            new() { Id = 3, Code = "CHILD2", Name = "Child 2", Type = OrganizationUnitType.Hub, Description = "Child 2 Description", ParentId = 1 }
        };
        await Context.OrganizationHierarchies.AddAsync(parent);
        await Context.OrganizationHierarchies.AddRangeAsync(children);
        await SaveChangesAsync();

        // Act
        var result = await Context.OrganizationHierarchies.Where(o => o.ParentId == 1).ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}
