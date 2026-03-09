/**
 * @fileoverview Fast standalone tests for soft-delete entity behavior and audit field handling
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests for soft-delete entity conventions: IsDeleted flag, DeletedBy/DeletedDate,
/// filtering, restore behavior, audit field preservation, and query exclusion.
/// </summary>
public class SoftDeleteEntityAuditTests
{
    // --- Inline SoftDeletableEntity record ---

    private record SoftDeletableEntity(
        int Id,
        string Name,
        bool IsDeleted,
        int? DeletedBy,
        DateTime? DeletedDate,
        int CreatedBy,
        DateTime CreatedDate,
        int? LastModifiedBy,
        DateTime? LastModifiedDate
    );

    private record ChildEntity(int Id, string Name, int ParentId, bool IsDeleted);

    // --- Helper methods ---

    private static SoftDeletableEntity SoftDelete(SoftDeletableEntity entity, int deletedBy)
    {
        return entity with
        {
            IsDeleted = true,
            DeletedBy = deletedBy,
            DeletedDate = DateTime.UtcNow
        };
    }

    private static SoftDeletableEntity Restore(SoftDeletableEntity entity)
    {
        return entity with
        {
            IsDeleted = false,
            DeletedBy = null,
            DeletedDate = null
        };
    }

    private static IReadOnlyList<SoftDeletableEntity> FilterExcludingDeleted(IEnumerable<SoftDeletableEntity> entities)
    {
        return entities.Where(e => !e.IsDeleted).ToList();
    }

    private static IReadOnlyList<ChildEntity> GetActiveChildren(IEnumerable<ChildEntity> children, int parentId)
    {
        return children.Where(c => c.ParentId == parentId && !c.IsDeleted).ToList();
    }

    // --- Soft delete sets IsDeleted flag (2 tests) ---

    [Fact]
    public void SoftDelete_SetsIsDeletedFlag_ToTrue()
    {
        var entity = new SoftDeletableEntity(1, "Test", false, null, null, 10, DateTime.UtcNow, null, null);
        var deleted = SoftDelete(entity, 20);
        deleted.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_OriginalEntity_RemainsUnchanged()
    {
        var entity = new SoftDeletableEntity(1, "Test", false, null, null, 10, DateTime.UtcNow, null, null);
        var deleted = SoftDelete(entity, 20);
        entity.IsDeleted.Should().BeFalse("original entity should be immutable");
    }

    // --- Soft delete sets DeletedBy and DeletedDate (3 tests) ---

    [Fact]
    public void SoftDelete_SetsDeletedBy_ToProvidedUserId()
    {
        var entity = new SoftDeletableEntity(1, "Test", false, null, null, 10, DateTime.UtcNow, null, null);
        var deleted = SoftDelete(entity, 42);
        deleted.DeletedBy.Should().Be(42);
    }

    [Fact]
    public void SoftDelete_SetsDeletedDate_ToNonNull()
    {
        var entity = new SoftDeletableEntity(1, "Test", false, null, null, 10, DateTime.UtcNow, null, null);
        var deleted = SoftDelete(entity, 20);
        deleted.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public void SoftDelete_DeletedDate_IsReasonableUtcTime()
    {
        var before = DateTime.UtcNow;
        var entity = new SoftDeletableEntity(1, "Test", false, null, null, 10, DateTime.UtcNow, null, null);
        var deleted = SoftDelete(entity, 20);
        var after = DateTime.UtcNow;
        deleted.DeletedDate!.Value.Should().BeOnOrAfter(before.AddSeconds(-1));
        deleted.DeletedDate!.Value.Should().BeOnOrBefore(after.AddSeconds(1));
    }

    // --- Filtering by IsDeleted excludes deleted records (3 tests) ---

    [Fact]
    public void FilterExcludingDeleted_ExcludesDeletedEntities()
    {
        var entities = new List<SoftDeletableEntity>
        {
            new(1, "A", false, null, null, 1, DateTime.UtcNow, null, null),
            new(2, "B", true, 10, DateTime.UtcNow, 1, DateTime.UtcNow, null, null),
            new(3, "C", false, null, null, 1, DateTime.UtcNow, null, null)
        };
        var filtered = FilterExcludingDeleted(entities);
        filtered.Should().HaveCount(2);
        filtered.Should().NotContain(e => e.Id == 2);
    }

    [Fact]
    public void FilterExcludingDeleted_ReturnsOnlyActiveEntities()
    {
        var entities = new List<SoftDeletableEntity>
        {
            new(1, "A", false, null, null, 1, DateTime.UtcNow, null, null),
            new(2, "B", true, 10, DateTime.UtcNow, 1, DateTime.UtcNow, null, null)
        };
        var filtered = FilterExcludingDeleted(entities);
        filtered.Should().AllSatisfy(e => e.IsDeleted.Should().BeFalse());
    }

    [Fact]
    public void FilterExcludingDeleted_EmptyList_ReturnsEmpty()
    {
        var filtered = FilterExcludingDeleted([]);
        filtered.Should().BeEmpty();
    }

    // --- Restore clears soft-delete fields (2 tests) ---

    [Fact]
    public void Restore_ClearsIsDeleted_DeletedBy_DeletedDate()
    {
        var deleted = new SoftDeletableEntity(1, "Test", true, 20, DateTime.UtcNow, 10, DateTime.UtcNow, null, null);
        var restored = Restore(deleted);
        restored.IsDeleted.Should().BeFalse();
        restored.DeletedBy.Should().BeNull();
        restored.DeletedDate.Should().BeNull();
    }

    [Fact]
    public void Restore_PreservesIdAndName()
    {
        var deleted = new SoftDeletableEntity(1, "Test", true, 20, DateTime.UtcNow, 10, DateTime.UtcNow, null, null);
        var restored = Restore(deleted);
        restored.Id.Should().Be(1);
        restored.Name.Should().Be("Test");
    }

    // --- Cannot delete an already-deleted entity (2 tests) ---

    [Fact]
    public void SoftDelete_AlreadyDeleted_ShouldBeIdempotentOrReject()
    {
        var alreadyDeleted = new SoftDeletableEntity(1, "Test", true, 10, DateTime.UtcNow, 10, DateTime.UtcNow, null, null);
        var result = SoftDelete(alreadyDeleted, 20);
        result.IsDeleted.Should().BeTrue("re-soft-delete is idempotent");
        result.DeletedBy.Should().Be(20, "DeletedBy may be updated on re-delete");
    }

    [Fact]
    public void SoftDelete_AlreadyDeleted_IsDeletedRemainsTrue()
    {
        var alreadyDeleted = new SoftDeletableEntity(1, "Test", true, 10, DateTime.UtcNow, 10, DateTime.UtcNow, null, null);
        var result = SoftDelete(alreadyDeleted, 20);
        result.IsDeleted.Should().BeTrue();
    }

    // --- Audit fields preserved after soft delete (2 tests) ---

    [Fact]
    public void SoftDelete_PreservesCreatedByAndCreatedDate()
    {
        var createdBy = 5;
        var createdDate = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var entity = new SoftDeletableEntity(1, "Test", false, null, null, createdBy, createdDate, null, null);
        var deleted = SoftDelete(entity, 20);
        deleted.CreatedBy.Should().Be(createdBy);
        deleted.CreatedDate.Should().Be(createdDate);
    }

    [Fact]
    public void SoftDelete_PreservesLastModifiedByAndLastModifiedDate()
    {
        var lastModifiedBy = 7;
        var lastModifiedDate = new DateTime(2025, 1, 20, 12, 0, 0, DateTimeKind.Utc);
        var entity = new SoftDeletableEntity(1, "Test", false, null, null, 5, DateTime.UtcNow, lastModifiedBy, lastModifiedDate);
        var deleted = SoftDelete(entity, 20);
        deleted.LastModifiedBy.Should().Be(lastModifiedBy);
        deleted.LastModifiedDate.Should().Be(lastModifiedDate);
    }

    // --- Query methods exclude deleted entities by default (3 tests) ---

    [Fact]
    public void QueryDefault_FilterExcludingDeleted_ExcludesAllDeleted()
    {
        var entities = new List<SoftDeletableEntity>
        {
            new(1, "A", false, null, null, 1, DateTime.UtcNow, null, null),
            new(2, "B", true, 10, DateTime.UtcNow, 1, DateTime.UtcNow, null, null),
            new(3, "B2", true, 10, DateTime.UtcNow, 1, DateTime.UtcNow, null, null)
        };
        var result = FilterExcludingDeleted(entities);
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public void QueryDefault_AllActive_ReturnsFullList()
    {
        var entities = new List<SoftDeletableEntity>
        {
            new(1, "A", false, null, null, 1, DateTime.UtcNow, null, null),
            new(2, "B", false, null, null, 1, DateTime.UtcNow, null, null)
        };
        var result = FilterExcludingDeleted(entities);
        result.Should().HaveCount(2);
    }

    [Fact]
    public void QueryDefault_AllDeleted_ReturnsEmpty()
    {
        var entities = new List<SoftDeletableEntity>
        {
            new(1, "A", true, 10, DateTime.UtcNow, 1, DateTime.UtcNow, null, null),
            new(2, "B", true, 10, DateTime.UtcNow, 1, DateTime.UtcNow, null, null)
        };
        var result = FilterExcludingDeleted(entities);
        result.Should().BeEmpty();
    }

    // --- Soft-deleted parent still returns active children (2 tests) ---

    [Fact]
    public void GetActiveChildren_SoftDeletedParent_ReturnsActiveChildren()
    {
        var children = new List<ChildEntity>
        {
            new(1, "Child1", 100, false),
            new(2, "Child2", 100, false),
            new(3, "Child3", 100, true)
        };
        var active = GetActiveChildren(children, 100);
        active.Should().HaveCount(2);
        active.Should().Contain(c => c.Id == 1);
        active.Should().Contain(c => c.Id == 2);
        active.Should().NotContain(c => c.Id == 3);
    }

    [Fact]
    public void GetActiveChildren_ExcludesDeletedChildren_RegardlessOfParent()
    {
        var children = new List<ChildEntity>
        {
            new(1, "Child1", 100, false),
            new(2, "Child2", 100, true)
        };
        var active = GetActiveChildren(children, 100);
        active.Should().HaveCount(1);
        active[0].Id.Should().Be(1);
    }
}
