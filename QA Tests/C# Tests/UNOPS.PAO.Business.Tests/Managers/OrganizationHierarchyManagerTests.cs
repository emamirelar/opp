using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for OrganizationHierarchyManager against PostgreSQL.
/// Uses test markers and auto-generated IDs for data isolation.
/// </summary>
public class OrganizationHierarchyManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"OHM_{Guid.NewGuid():N}";

    [Fact]
    public async Task GetOrganizationById_Should_ReturnOrganization_When_Exists()
    {
        // Arrange
        var org = new OrganizationHierarchy
        {
            Code = $"ORG001_{_testMarker}",
            Name = $"Test Organization {_testMarker}",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Test Organization Description"
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {org.Id}");

        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(org.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("Test Organization");
        result.Code.Should().Contain("ORG001_");
    }

    [Fact]
    public async Task GetOrganizationById_Should_ReturnNull_When_NotExists()
    {
        // Act
        var result = await Context.OrganizationHierarchies.FindAsync(999999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetOrganizationsByType_Should_FilterByType()
    {
        // Arrange
        var orgs = new List<OrganizationHierarchy>
        {
            new() { Code = $"REG1_{_testMarker}", Name = $"Region 1 {_testMarker}", Type = OrganizationUnitType.Region, Description = "Region Description" },
            new() { Code = $"HUB1_{_testMarker}", Name = $"Hub 1 {_testMarker}", Type = OrganizationUnitType.Hub, Description = "Hub Description" },
            new() { Code = $"OU1_{_testMarker}", Name = $"Org Unit 1 {_testMarker}", Type = OrganizationUnitType.OrgUnit, Description = "OrgUnit Description" }
        };
        await Context.OrganizationHierarchies.AddRangeAsync(orgs);
        await SaveChangesAsync();
        foreach (var o in orgs) RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {o.Id}");

        // Act
        var result = await Context.OrganizationHierarchies
            .Where(o => o.Name.Contains(_testMarker) && o.Type == OrganizationUnitType.Hub)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Contain("Hub 1");
    }

    [Fact]
    public async Task GetAllOrganizations_Should_ReturnAllOrganizations()
    {
        // Arrange
        var orgs = new List<OrganizationHierarchy>
        {
            new() { Code = $"OU1_{_testMarker}", Name = $"Org Unit 1 {_testMarker}", Type = OrganizationUnitType.OrgUnit, Description = "Description 1" },
            new() { Code = $"HUB1_{_testMarker}", Name = $"Hub 1 {_testMarker}", Type = OrganizationUnitType.Hub, Description = "Description 2" },
            new() { Code = $"REG1_{_testMarker}", Name = $"Region 1 {_testMarker}", Type = OrganizationUnitType.Region, Description = "Description 3" }
        };
        await Context.OrganizationHierarchies.AddRangeAsync(orgs);
        await SaveChangesAsync();
        foreach (var o in orgs) RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {o.Id}");

        // Act
        var result = await Context.OrganizationHierarchies
            .Where(o => o.Name.Contains(_testMarker))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task OrganizationHierarchy_Should_SupportParentChildRelationships()
    {
        // Arrange
        var parent = new OrganizationHierarchy
        {
            Code = $"PARENT_{_testMarker}",
            Name = $"Parent Org {_testMarker}",
            Type = OrganizationUnitType.Region,
            Description = "Parent Organization"
        };
        await Context.OrganizationHierarchies.AddAsync(parent);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {parent.Id}");

        var child = new OrganizationHierarchy
        {
            Code = $"CHILD_{_testMarker}",
            Name = $"Child Org {_testMarker}",
            Type = OrganizationUnitType.OrgUnit,
            Description = "Child Organization",
            ParentId = parent.Id
        };
        await Context.OrganizationHierarchies.AddAsync(child);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {child.Id}");

        // Act
        var childResult = await Context.OrganizationHierarchies.FindAsync(child.Id);

        // Assert
        childResult.Should().NotBeNull();
        childResult!.ParentId.Should().Be(parent.Id);
    }

    [Fact]
    public async Task GetOrganizations_Should_ReturnEmpty_When_NoMatchingOrganizations()
    {
        // Act
        var result = await Context.OrganizationHierarchies
            .Where(o => o.Name == "NONEXISTENT_ORG_MARKER_ZZZZZ")
            .ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChildOrganizations_Should_ReturnChildren()
    {
        // Arrange
        var parent = new OrganizationHierarchy
        {
            Code = $"PARENT_{_testMarker}",
            Name = $"Parent {_testMarker}",
            Type = OrganizationUnitType.Region,
            Description = "Parent Description"
        };
        await Context.OrganizationHierarchies.AddAsync(parent);
        await SaveChangesAsync();
        RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {parent.Id}");

        var children = new List<OrganizationHierarchy>
        {
            new() { Code = $"CHILD1_{_testMarker}", Name = $"Child 1 {_testMarker}", Type = OrganizationUnitType.Hub, Description = "Child 1 Description", ParentId = parent.Id },
            new() { Code = $"CHILD2_{_testMarker}", Name = $"Child 2 {_testMarker}", Type = OrganizationUnitType.Hub, Description = "Child 2 Description", ParentId = parent.Id }
        };
        await Context.OrganizationHierarchies.AddRangeAsync(children);
        await SaveChangesAsync();
        foreach (var c in children) RegisterTableCleanup("OrganizationHierarchies", $"\"Id\" = {c.Id}");

        // Act
        var result = await Context.OrganizationHierarchies
            .Where(o => o.ParentId == parent.Id)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}
