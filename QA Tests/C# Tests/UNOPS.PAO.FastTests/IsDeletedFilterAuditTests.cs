/**
 * @fileoverview Fast standalone tests for IsDeleted query filtering logic across entity types.
 * Validates the critical business rule that all queries MUST filter by IsDeleted.
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests that query filtering logic correctly handles IsDeleted flag across various entity types.
/// All types, enums, and logic are defined inline — no production assembly references.
/// </summary>
public class IsDeletedFilterAuditTests
{
    // --- Inline types ---

    private enum EntityTypeKind { Partner, Contact, Opportunity, Interaction }

    private record QueryableEntity(int Id, string Name, bool IsDeleted, int? ParentId, EntityTypeKind EntityType);

    private record ParentEntity(int Id, string Name, bool IsDeleted);

    private record ChildEntity(int Id, string Name, int ParentId, bool IsDeleted, decimal Amount);

    // --- Helper methods ---

    private static IReadOnlyList<QueryableEntity> FilterActive(IEnumerable<QueryableEntity> entities)
    {
        return entities.Where(e => !e.IsDeleted).ToList();
    }

    private static IReadOnlyList<ChildEntity> FilterActiveWithParent(
        IEnumerable<ChildEntity> children,
        IReadOnlyDictionary<int, ParentEntity> parents)
    {
        return children
            .Where(c => !c.IsDeleted && parents.TryGetValue(c.ParentId, out var p) && !p.IsDeleted)
            .ToList();
    }

    private static QueryableEntity? GetByIdWithIsDeletedCheck(
        IEnumerable<QueryableEntity> entities,
        int id)
    {
        return entities.FirstOrDefault(e => e.Id == id && !e.IsDeleted);
    }

    private static IReadOnlyList<QueryableEntity> PaginateFiltered(
        IEnumerable<QueryableEntity> entities,
        int pageNumber,
        int pageSize)
    {
        var filtered = entities.Where(e => !e.IsDeleted).ToList();
        return filtered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    // --- FilterActive excludes deleted records (2 tests) ---

    [Fact]
    public void FilterActive_ExcludesDeletedRecords_WhenMixed()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", false, null, EntityTypeKind.Partner),
            new(2, "B", true, null, EntityTypeKind.Contact),
            new(3, "C", false, null, EntityTypeKind.Opportunity)
        };
        var filtered = FilterActive(entities);
        filtered.Should().HaveCount(2);
        filtered.Should().NotContain(e => e.Id == 2);
    }

    [Fact]
    public void FilterActive_ExcludesAllDeletedRecords_WhenMultipleDeleted()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", true, null, EntityTypeKind.Partner),
            new(2, "B", true, null, EntityTypeKind.Contact),
            new(3, "C", false, null, EntityTypeKind.Opportunity)
        };
        var filtered = FilterActive(entities);
        filtered.Should().HaveCount(1);
        filtered[0].Id.Should().Be(3);
    }

    // --- FilterActive includes all non-deleted records (2 tests) ---

    [Fact]
    public void FilterActive_IncludesAllNonDeletedRecords()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", false, null, EntityTypeKind.Partner),
            new(2, "B", false, null, EntityTypeKind.Contact),
            new(3, "C", false, null, EntityTypeKind.Opportunity)
        };
        var filtered = FilterActive(entities);
        filtered.Should().HaveCount(3);
        filtered.Should().Contain(e => e.Id == 1);
        filtered.Should().Contain(e => e.Id == 2);
        filtered.Should().Contain(e => e.Id == 3);
    }

    [Fact]
    public void FilterActive_IncludesSingleActiveRecord()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "Solo", false, null, EntityTypeKind.Interaction)
        };
        var filtered = FilterActive(entities);
        filtered.Should().HaveCount(1);
        filtered[0].Name.Should().Be("Solo");
    }

    // --- Counting only active records gives correct count (2 tests) ---

    [Fact]
    public void CountActive_ReturnsCorrectCount_WhenMixed()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", false, null, EntityTypeKind.Partner),
            new(2, "B", true, null, EntityTypeKind.Contact),
            new(3, "C", false, null, EntityTypeKind.Opportunity)
        };
        var count = FilterActive(entities).Count;
        count.Should().Be(2);
    }

    [Fact]
    public void CountActive_ReturnsZero_WhenAllDeleted()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", true, null, EntityTypeKind.Partner),
            new(2, "B", true, null, EntityTypeKind.Contact)
        };
        var count = FilterActive(entities).Count;
        count.Should().Be(0);
    }

    // --- Mixed collection returns correct subset (2 tests) ---

    [Fact]
    public void MixedCollection_ReturnsCorrectSubset_WithVariousEntityTypes()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "P1", false, null, EntityTypeKind.Partner),
            new(2, "C1", true, 1, EntityTypeKind.Contact),
            new(3, "O1", false, null, EntityTypeKind.Opportunity),
            new(4, "I1", true, 3, EntityTypeKind.Interaction)
        };
        var filtered = FilterActive(entities);
        filtered.Should().HaveCount(2);
        filtered.Select(e => e.Id).Should().BeEquivalentTo(new[] { 1, 3 });
    }

    [Fact]
    public void MixedCollection_AllActive_ReturnsFullSet()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", false, null, EntityTypeKind.Partner),
            new(2, "B", false, 1, EntityTypeKind.Contact),
            new(3, "C", false, null, EntityTypeKind.Opportunity)
        };
        var filtered = FilterActive(entities);
        filtered.Should().HaveCount(3);
    }

    // --- Empty collection returns empty (1 test) ---

    [Fact]
    public void EmptyCollection_ReturnsEmpty()
    {
        var filtered = FilterActive([]);
        filtered.Should().BeEmpty();
    }

    // --- All-deleted collection returns empty (1 test) ---

    [Fact]
    public void AllDeletedCollection_ReturnsEmpty()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", true, null, EntityTypeKind.Partner),
            new(2, "B", true, null, EntityTypeKind.Contact)
        };
        var filtered = FilterActive(entities);
        filtered.Should().BeEmpty();
    }

    // --- Nested filtering: deleted parent filters out active children (2 tests) ---

    [Fact]
    public void FilterActiveWithParent_DeletedParent_FiltersOutActiveChildren()
    {
        var parents = new Dictionary<int, ParentEntity>
        {
            [100] = new(100, "DeletedParent", true),
            [200] = new(200, "ActiveParent", false)
        };
        var children = new List<ChildEntity>
        {
            new(1, "Child1", 100, false, 10m),
            new(2, "Child2", 200, false, 20m)
        };
        var filtered = FilterActiveWithParent(children, parents);
        filtered.Should().HaveCount(1);
        filtered[0].ParentId.Should().Be(200);
    }

    [Fact]
    public void FilterActiveWithParent_ActiveParentAndDeletedChild_ExcludesDeletedChild()
    {
        var parents = new Dictionary<int, ParentEntity>
        {
            [100] = new(100, "ActiveParent", false)
        };
        var children = new List<ChildEntity>
        {
            new(1, "ActiveChild", 100, false, 10m),
            new(2, "DeletedChild", 100, true, 20m)
        };
        var filtered = FilterActiveWithParent(children, parents);
        filtered.Should().HaveCount(1);
        filtered[0].Id.Should().Be(1);
    }

    // --- GetById with IsDeleted check returns null for deleted record (2 tests) ---

    [Fact]
    public void GetByIdWithIsDeletedCheck_DeletedRecord_ReturnsNull()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", false, null, EntityTypeKind.Partner),
            new(2, "B", true, null, EntityTypeKind.Contact)
        };
        var result = GetByIdWithIsDeletedCheck(entities, 2);
        result.Should().BeNull();
    }

    [Fact]
    public void GetByIdWithIsDeletedCheck_ActiveRecord_ReturnsEntity()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", false, null, EntityTypeKind.Partner),
            new(2, "B", true, null, EntityTypeKind.Contact)
        };
        var result = GetByIdWithIsDeletedCheck(entities, 1);
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    // --- Aggregations (sum, average) exclude deleted records (2 tests) ---

    [Fact]
    public void Aggregation_SumExcludesDeletedRecords()
    {
        var children = new List<ChildEntity>
        {
            new(1, "A", 100, false, 10m),
            new(2, "B", 100, true, 20m),
            new(3, "C", 100, false, 30m)
        };
        var active = children.Where(c => !c.IsDeleted);
        var sum = active.Sum(c => c.Amount);
        sum.Should().Be(40m);
    }

    [Fact]
    public void Aggregation_AverageExcludesDeletedRecords()
    {
        var children = new List<ChildEntity>
        {
            new(1, "A", 100, false, 10m),
            new(2, "B", 100, true, 100m),
            new(3, "C", 100, false, 20m)
        };
        var active = children.Where(c => !c.IsDeleted);
        var avg = active.Average(c => c.Amount);
        avg.Should().Be(15m);
    }

    // --- Pagination of filtered results is correct (2 tests) ---

    [Fact]
    public void PaginateFiltered_ReturnsCorrectPage()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", false, null, EntityTypeKind.Partner),
            new(2, "B", true, null, EntityTypeKind.Contact),
            new(3, "C", false, null, EntityTypeKind.Opportunity),
            new(4, "D", false, null, EntityTypeKind.Interaction)
        };
        var page = PaginateFiltered(entities, 1, 2);
        page.Should().HaveCount(2);
        page[0].Id.Should().Be(1);
        page[1].Id.Should().Be(3);
    }

    [Fact]
    public void PaginateFiltered_SecondPage_ReturnsRemainingItems()
    {
        var entities = new List<QueryableEntity>
        {
            new(1, "A", false, null, EntityTypeKind.Partner),
            new(2, "B", false, null, EntityTypeKind.Contact),
            new(3, "C", false, null, EntityTypeKind.Opportunity),
            new(4, "D", false, null, EntityTypeKind.Interaction)
        };
        var page = PaginateFiltered(entities, 2, 2);
        page.Should().HaveCount(2);
        page[0].Id.Should().Be(3);
        page[1].Id.Should().Be(4);
    }
}
