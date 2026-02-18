using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.EdgeCases;

/// <summary>
/// Edge case tests for PartnerTreeManager
/// </summary>
public class PartnerTreeManagerEdgeCaseTests : ManagerTestBase
{
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
        var tree = new PartnerTree
        {
            Id = 1,
            Code = "",
            Name = "Empty Code Tree",
            Type = "Category",
            Description = "Empty Code Tree Description",
            Parent = null
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();

        // Act
        var result = await Context.PartnerTrees.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().BeEmpty();
    }

    [Fact]
    public async Task PartnerTree_WithSelfReference_Should_BeStorable()
    {
        // Arrange - Note: This tests storage capability, not validity
        var tree = new PartnerTree
        {
            Id = 1,
            Code = "SELF",
            Name = "Self Reference",
            Type = "Category",
            Description = "Self Reference Description",
            Parent = "SELF" // Self-referencing parent
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();

        // Act
        var result = await Context.PartnerTrees.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Parent.Should().Be("SELF");
    }

    [Fact]
    public async Task PartnerTree_WithNonExistentParent_Should_BeStorable()
    {
        // Arrange
        var tree = new PartnerTree
        {
            Id = 1,
            Code = "ORPHAN",
            Name = "Orphan Node",
            Type = "Category",
            Description = "Orphan Node Description",
            Parent = "NON_EXISTENT_PARENT"
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();

        // Act
        var result = await Context.PartnerTrees.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Parent.Should().Be("NON_EXISTENT_PARENT");
    }

    [Fact]
    public async Task PartnerTree_WithSpecialCharactersInCode_Should_BeHandled()
    {
        // Arrange
        var tree = new PartnerTree
        {
            Id = 1,
            Code = "CODE-WITH_SPECIAL.CHARS",
            Name = "Special Code",
            Type = "Category",
            Description = "Special Code Description",
            Parent = null
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();

        // Act
        var result = await Context.PartnerTrees.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Contain("-");
    }

    [Fact]
    public async Task PartnerTree_WithVeryLongName_Should_BeHandled()
    {
        // Arrange
        var longName = new string('A', 500);
        var tree = new PartnerTree
        {
            Id = 1,
            Code = "LONG",
            Name = longName,
            Type = "Category",
            Description = "Long Name Description",
            Parent = null
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();

        // Act
        var result = await Context.PartnerTrees.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Length.Should().Be(500);
    }

    [Fact]
    public async Task GetPartnerTrees_WithAllRoots_Should_ReturnAll()
    {
        // Arrange
        var trees = Enumerable.Range(1, 10)
            .Select(i => new PartnerTree { Id = i, Code = $"ROOT{i}", Name = $"Root {i}", Type = "Category", Description = $"Root {i} Description", Parent = null })
            .ToList();
        await Context.PartnerTrees.AddRangeAsync(trees);
        await SaveChangesAsync();

        // Act
        var result = await Context.PartnerTrees.Where(t => t.Parent == null).ToListAsync();

        // Assert
        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task PartnerTree_WithUnicodeInName_Should_BeHandled()
    {
        // Arrange
        var tree = new PartnerTree
        {
            Id = 1,
            Code = "UNICODE",
            Name = "분류 🌳 Категория",
            Type = "Category",
            Description = "Unicode Description",
            Parent = null
        };
        await Context.PartnerTrees.AddAsync(tree);
        await SaveChangesAsync();

        // Act
        var result = await Context.PartnerTrees.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("🌳");
    }

    [Fact]
    public async Task GetPartnerTrees_EmptyParentVsNullParent_Should_BothBeRoots()
    {
        // Arrange
        var trees = new List<PartnerTree>
        {
            new() { Id = 1, Code = "NULL_PARENT", Name = "Null Parent", Type = "Category", Description = "Null Parent Description", Parent = null },
            new() { Id = 2, Code = "EMPTY_PARENT", Name = "Empty Parent", Type = "Category", Description = "Empty Parent Description", Parent = "" }
        };
        await Context.PartnerTrees.AddRangeAsync(trees);
        await SaveChangesAsync();

        // Act
        var result = await Context.PartnerTrees
            .Where(t => string.IsNullOrEmpty(t.Parent))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}
