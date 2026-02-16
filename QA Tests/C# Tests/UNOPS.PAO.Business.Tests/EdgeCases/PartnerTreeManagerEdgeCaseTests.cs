using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.EdgeCases;

/// <summary>
/// Edge case tests for PartnerTreeManager against PostgreSQL.
/// Uses test markers and auto-generated IDs for data isolation.
/// </summary>
public class PartnerTreeManagerEdgeCaseTests : ManagerTestBase
{
    private readonly string _testMarker = $"PTEC_{Guid.NewGuid():N}";

    [Fact]
    public async Task GetPartnerTree_WithZeroId_Should_ReturnNull()
    {
        // Act
        var result = await Context.PartnerTrees.FindAsync(0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task PartnerTree_WithEmptyCode_Should_BeHandled()
    {
        // Arrange
        var tree = new UNOPSPartnerTree
        {
            Code = "",
            Name = $"Empty Code Tree {_testMarker}",
            Type = "Category",
            Description = "Empty Code Tree Description",
            Parent = null
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();
        RegisterTableCleanup("PartnerTrees", $"\"Id\" = {tree.Id}");

        // Act
        var result = await Context.PartnerTrees.FindAsync(tree.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().BeEmpty();
    }

    [Fact]
    public async Task PartnerTree_WithSelfReference_Should_BeStorable()
    {
        // Arrange - Note: This tests storage capability, not validity
        var tree = new UNOPSPartnerTree
        {
            Code = $"SELF_{_testMarker}",
            Name = $"Self Reference {_testMarker}",
            Type = "Category",
            Description = "Self Reference Description",
            Parent = $"SELF_{_testMarker}" // Self-referencing parent by code
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();
        RegisterTableCleanup("PartnerTrees", $"\"Id\" = {tree.Id}");

        // Act
        var result = await Context.PartnerTrees.FindAsync(tree.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Parent.Should().Contain("SELF_");
    }

    [Fact]
    public async Task PartnerTree_WithNonExistentParent_Should_BeStorable()
    {
        // Arrange
        var tree = new UNOPSPartnerTree
        {
            Code = $"ORPHAN_{_testMarker}",
            Name = $"Orphan Node {_testMarker}",
            Type = "Category",
            Description = "Orphan Node Description",
            Parent = "NON_EXISTENT_PARENT_ZZZZZ"
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();
        RegisterTableCleanup("PartnerTrees", $"\"Id\" = {tree.Id}");

        // Act
        var result = await Context.PartnerTrees.FindAsync(tree.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Parent.Should().Be("NON_EXISTENT_PARENT_ZZZZZ");
    }

    [Fact]
    public async Task PartnerTree_WithSpecialCharactersInCode_Should_BeHandled()
    {
        // Arrange
        var tree = new UNOPSPartnerTree
        {
            Code = $"CODE-WITH_SPECIAL.CHARS_{_testMarker}",
            Name = $"Special Code {_testMarker}",
            Type = "Category",
            Description = "Special Code Description",
            Parent = null
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();
        RegisterTableCleanup("PartnerTrees", $"\"Id\" = {tree.Id}");

        // Act
        var result = await Context.PartnerTrees.FindAsync(tree.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Contain("-");
    }

    [Fact]
    public async Task PartnerTree_WithVeryLongName_Should_BeHandled()
    {
        // Arrange
        var longName = new string('A', 500);
        var tree = new UNOPSPartnerTree
        {
            Code = $"LONG_{_testMarker}",
            Name = longName,
            Type = "Category",
            Description = "Long Name Description",
            Parent = null
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();
        RegisterTableCleanup("PartnerTrees", $"\"Id\" = {tree.Id}");

        // Act
        var result = await Context.PartnerTrees.FindAsync(tree.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Length.Should().Be(500);
    }

    [Fact]
    public async Task GetPartnerTrees_WithAllRoots_Should_ReturnAll()
    {
        // Arrange
        var trees = Enumerable.Range(1, 10)
            .Select(i => new UNOPSPartnerTree
            {
                Code = $"ROOT{i}_{_testMarker}",
                Name = $"Root {i} {_testMarker}",
                Type = "Category",
                Description = $"Root {i} Description",
                Parent = null
            })
            .ToList();
        await Context.PartnerTrees.AddRangeAsync(trees);
        await SaveChangesAsync();
        foreach (var t in trees) RegisterTableCleanup("PartnerTrees", $"\"Id\" = {t.Id}");

        // Act
        var result = await Context.PartnerTrees
            .Where(t => t.Parent == null && t.Name.Contains(_testMarker))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task PartnerTree_WithUnicodeInName_Should_BeHandled()
    {
        // Arrange
        var tree = new UNOPSPartnerTree
        {
            Code = $"UNICODE_{_testMarker}",
            Name = $"분류 🌳 Категория {_testMarker}",
            Type = "Category",
            Description = "Unicode Description",
            Parent = null
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();
        RegisterTableCleanup("PartnerTrees", $"\"Id\" = {tree.Id}");

        // Act
        var result = await Context.PartnerTrees.FindAsync(tree.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("🌳");
    }

    [Fact]
    public async Task GetPartnerTrees_EmptyParentVsNullParent_Should_BothBeRoots()
    {
        // Arrange
        var trees = new List<UNOPSPartnerTree>
        {
            new() { Code = $"NULL_PARENT_{_testMarker}", Name = $"Null Parent {_testMarker}", Type = "Category", Description = "Null Parent Description", Parent = null },
            new() { Code = $"EMPTY_PARENT_{_testMarker}", Name = $"Empty Parent {_testMarker}", Type = "Category", Description = "Empty Parent Description", Parent = "" }
        };
        await Context.PartnerTrees.AddRangeAsync(trees);
        await SaveChangesAsync();
        foreach (var t in trees) RegisterTableCleanup("PartnerTrees", $"\"Id\" = {t.Id}");

        // Act
        var result = await Context.PartnerTrees
            .Where(t => string.IsNullOrEmpty(t.Parent) && t.Name.Contains(_testMarker))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}
