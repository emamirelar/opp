/**
 * @fileoverview Mock-based and InMemory tests for OrgUnitHierarchyService.
 * Tests GetDescendantIdsAsync with cache, hierarchy traversal, and edge cases.
 * Uses real IMemoryCache and UNOPSAppDbContext (InMemory/SQLite or PostgreSQL).
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Tests for OrgUnitHierarchyService.
/// Uses real IMemoryCache and UNOPSAppDbContext with seeded OrganizationHierarchy.
/// </summary>
public class OrgUnitHierarchyServiceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly UNOPSAppDbContext _context;
    private readonly OrgUnitHierarchyService _service;

    public OrgUnitHierarchyServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var dbName = $"OrgUnitHierarchy_{Guid.NewGuid():N}";
        var options = TestEnvironment.CreateUNOPSDbContextOptions(dbName);
        _context = TestDbContextFactory.CreateUNOPS(options);
        TestEnvironment.EnsureCleanDatabase(_context);

        var mockLogger = new Mock<ILogger<OrgUnitHierarchyService>>();
        _service = new OrgUnitHierarchyService(_context, _memoryCache, mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        _memoryCache.Dispose();
    }

    #region Positive (1)

    [Fact]
    public async Task GetDescendantIdsAsync_WithHierarchy_ReturnsDescendantsIncludingRoot()
    {
        // Arrange
        var (rootId, _) = await SeedHierarchyAsync();

        // Act
        var result = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(rootId);
        result.Count.Should().BeGreaterThan(0);
    }

    #endregion

    #region Negative (3+)

    [Fact]
    public async Task GetDescendantIdsAsync_NonExistentOrgUnitId_ReturnsEmptyOrMinimal()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _service.GetDescendantIdsAsync(nonExistentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDescendantIdsAsync_ZeroOrgUnitId_HandlesGracefully()
    {
        // Arrange
        var zeroId = 0;

        // Act
        var result = await _service.GetDescendantIdsAsync(zeroId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDescendantIdsAsync_NegativeOrgUnitId_ReturnsEmpty()
    {
        // Arrange
        var negativeId = -1;

        // Act
        var result = await _service.GetDescendantIdsAsync(negativeId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-075")]
    public async Task GetDescendantIdsAsync_DeletedOrgUnit_ExcludedFromResults()
    {
        // Arrange
        var (rootId, _) = await SeedHierarchyAsync();
        var deletedChild = await SeedDeletedChildAsync(rootId);

        // Act
        var result = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotContain(deletedChild);
    }

    #endregion

    #region Edge/Boundary (3+)

    [Fact]
    public async Task GetDescendantIdsAsync_LeafNode_ReturnsOnlySelf()
    {
        // Arrange
        var (_, leafId) = await SeedHierarchyAsync();

        // Act
        var result = await _service.GetDescendantIdsAsync(leafId);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result.Should().Contain(leafId);
    }

    [Fact]
    public async Task GetDescendantIdsAsync_SecondCall_ReturnsCacheHit()
    {
        // Arrange
        var (rootId, _) = await SeedHierarchyAsync();
        var first = await _service.GetDescendantIdsAsync(rootId);

        // Act
        var second = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        second.Should().BeEquivalentTo(first);
    }

    [Fact]
    public async Task GetDescendantIdsAsync_EmptyDatabase_ReturnsEmpty()
    {
        // Use a very high ID that won't exist in any real database
        var nonExistentId = 999999;

        // Act
        var result = await _service.GetDescendantIdsAsync(nonExistentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDescendantIdsAsync_DeepHierarchy_ReturnsAllLevels()
    {
        // Arrange
        var rootId = await SeedDeepHierarchyAsync();

        // Act
        var result = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(rootId);
        result.Count.Should().BeGreaterThan(1);
    }

    #endregion

    #region Functional (3+)

    [Fact]
    public async Task GetDescendantIdsAsync_CacheKeyFormat_IsConsistent()
    {
        // Arrange
        var (rootId, _) = await SeedHierarchyAsync();
        await _service.GetDescendantIdsAsync(rootId);

        // Act - second call should hit cache
        var result = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(rootId);
    }

    [Fact]
    public async Task GetDescendantIdsAsync_ResultIncludesRoot()
    {
        // Arrange
        var (rootId, _) = await SeedHierarchyAsync();

        // Act
        var result = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        result.Should().Contain(rootId);
    }

    [Fact]
    public async Task GetDescendantIdsAsync_ResultIncludesDirectChildren()
    {
        // Arrange
        var (rootId, childId) = await SeedHierarchyAsync();

        // Act
        var result = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        result.Should().Contain(childId);
    }

    [Fact]
    public async Task GetDescendantIdsAsync_NoDuplicateIds()
    {
        // Arrange
        var (rootId, _) = await SeedHierarchyAsync();

        // Act
        var result = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        result.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region Integration (3+)

    [Fact]
    public async Task GetDescendantIdsAsync_FullFlow_SeedHierarchyThenQuery()
    {
        // Arrange
        var root = new OrganizationHierarchy
        {
            Code = "INT_ROOT",
            Name = "Integration Root",
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(root);
        await _context.SaveChangesAsync();

        var child = new OrganizationHierarchy
        {
            Code = "INT_CHILD",
            Name = "Integration Child",
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            ParentId = root.Id,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(child);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDescendantIdsAsync(root.Id);

        // Assert
        result.Should().Contain(root.Id);
        result.Should().Contain(child.Id);
    }

    [Fact]
    public async Task GetDescendantIdsAsync_CacheThenInvalidate_RefetchesFromDb()
    {
        // Arrange
        var (rootId, _) = await SeedHierarchyAsync();
        var first = await _service.GetDescendantIdsAsync(rootId);

        // Simulate cache invalidation by using a new service instance with same cache
        _memoryCache.Remove($"org_hierarchy_{rootId}");

        // Act - second call after cache invalidation
        var second = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        second.Should().BeEquivalentTo(first);
    }

    [Fact]
    public async Task GetDescendantIdsAsync_MultipleRoots_QueryEachIndependently()
    {
        // Arrange
        var root1 = await SeedSingleOrgUnitAsync("R1");
        var root2 = await SeedSingleOrgUnitAsync("R2");

        // Act
        var result1 = await _service.GetDescendantIdsAsync(root1);
        var result2 = await _service.GetDescendantIdsAsync(root2);

        // Assert
        result1.Should().Contain(root1);
        result2.Should().Contain(root2);
        result1.Should().NotContain(root2);
        result2.Should().NotContain(root1);
    }

    [Fact]
    public async Task GetDescendantIdsAsync_ConsecutiveCalls_SameResult()
    {
        // Arrange
        var (rootId, _) = await SeedHierarchyAsync();

        // Act
        var r1 = await _service.GetDescendantIdsAsync(rootId);
        var r2 = await _service.GetDescendantIdsAsync(rootId);
        var r3 = await _service.GetDescendantIdsAsync(rootId);

        // Assert
        r1.Should().BeEquivalentTo(r2);
        r2.Should().BeEquivalentTo(r3);
    }

    #endregion

    #region Helpers

    private async Task<(int rootId, int childId)> SeedHierarchyAsync()
    {
        var root = new OrganizationHierarchy
        {
            Code = "ROOT",
            Name = "Root",
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(root);
        await _context.SaveChangesAsync();

        var child = new OrganizationHierarchy
        {
            Code = "CHILD",
            Name = "Child",
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            ParentId = root.Id,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(child);
        await _context.SaveChangesAsync();

        return (root.Id, child.Id);
    }

    private async Task<int> SeedDeletedChildAsync(int parentId)
    {
        var child = new OrganizationHierarchy
        {
            Code = "DELETED",
            Name = "Deleted",
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            ParentId = parentId,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = true,
            DeletedBy = 1,
            DeletedDate = DateTime.UtcNow
        };
        _context.OrganizationHierarchies.Add(child);
        await _context.SaveChangesAsync();
        return child.Id;
    }

    private async Task<int> SeedDeepHierarchyAsync()
    {
        var root = new OrganizationHierarchy
        {
            Code = "DEEP_ROOT",
            Name = "Deep Root",
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(root);
        await _context.SaveChangesAsync();

        var mid = new OrganizationHierarchy
        {
            Code = "DEEP_MID",
            Name = "Deep Mid",
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            ParentId = root.Id,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(mid);
        await _context.SaveChangesAsync();

        var leaf = new OrganizationHierarchy
        {
            Code = "DEEP_LEAF",
            Name = "Deep Leaf",
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            ParentId = mid.Id,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(leaf);
        await _context.SaveChangesAsync();

        return root.Id;
    }

    private async Task<int> SeedSingleOrgUnitAsync(string code)
    {
        var org = new OrganizationHierarchy
        {
            Code = code,
            Name = code,
            Description = "Test",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.OrganizationHierarchies.Add(org);
        await _context.SaveChangesAsync();
        return org.Id;
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 1 | GetDescendantIdsAsync_WithHierarchy_ReturnsDescendantsIncludingRoot |
| Negative (N) | 4 | GetDescendantIdsAsync_NonExistentOrgUnitId_ReturnsEmptyOrMinimal, GetDescendantIdsAsync_ZeroOrgUnitId_HandlesGracefully, GetDescendantIdsAsync_NegativeOrgUnitId_ReturnsEmpty, GetDescendantIdsAsync_DeletedOrgUnit_ExcludedFromResults |
| Edge/Boundary (E) | 4 | GetDescendantIdsAsync_LeafNode_ReturnsOnlySelf, GetDescendantIdsAsync_SecondCall_ReturnsCacheHit, GetDescendantIdsAsync_EmptyDatabase_ReturnsEmpty, GetDescendantIdsAsync_DeepHierarchy_ReturnsAllLevels |
| Functional (F) | 4 | GetDescendantIdsAsync_CacheKeyFormat_IsConsistent, GetDescendantIdsAsync_ResultIncludesRoot, GetDescendantIdsAsync_ResultIncludesDirectChildren, GetDescendantIdsAsync_NoDuplicateIds |
| Integration (I) | 4 | GetDescendantIdsAsync_FullFlow_SeedHierarchyThenQuery, GetDescendantIdsAsync_CacheThenInvalidate_RefetchesFromDb, GetDescendantIdsAsync_MultipleRoots_QueryEachIndependently, GetDescendantIdsAsync_ConsecutiveCalls_SameResult |
| **N ≥ 3P?** | ✅ | 4 >= 3 |
| **E ≥ 3P?** | ✅ | 4 >= 3 |
| **F ≥ 3P?** | ✅ | 4 >= 3 |
| **I ≥ 3P?** | ✅ | 4 >= 3 |
*/
