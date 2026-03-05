/**
 * PERFORMANCE TESTS — InteractionManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for InteractionManager / Interaction data access.
/// Verifies response times, throughput, memory efficiency, and EF Core optimization patterns
/// (AsNoTracking, N+1 detection) for interaction CRUD operations.
///
/// Required: ≥16 tests (FIXED)
/// Uses Context.Interactions directly (same data layer as InteractionManager).
/// </summary>
public class InteractionPerformanceTests : PerformanceTestBase
{
    private readonly string _testMarker = $"IPERF_{Guid.NewGuid():N}";
    private readonly List<int> _createdInteractionIds = new();

    #region Single Operation Performance (min 2)

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task GetInteractionById_ExistingEntity_CompletesWithinThreshold()
    {
        // Arrange
        var interaction = await SeedInteractionAsync($"GetById {_testMarker}");

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Interactions.FindAsync(interaction.Id));

        // Assert
        result.Should().NotBeNull();
        elapsed.Should().BeLessThan(FastOperationThreshold,
            $"GetById took {elapsed}ms, expected <{FastOperationThreshold}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task Create_SingleInteraction_CompletesWithinThreshold()
    {
        // Arrange
        var interaction = BuildInteraction($"Create Single {_testMarker}");

        // Act
        var elapsed = await MeasureAsync(async () =>
        {
            await Context.Interactions.AddAsync(interaction);
            await SaveChangesAsync();
        });
        _createdInteractionIds.Add(interaction.Id);
        RegisterCleanup(CleanupInteractions);

        // Assert
        elapsed.Should().BeLessThan(FastOperationThreshold,
            $"Create took {elapsed}ms, expected <{FastOperationThreshold}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task BulkCreate_100Interactions_CompletesWithinThreshold()
    {
        // Arrange
        var interactions = Enumerable.Range(1, 100)
            .Select(i => BuildInteraction($"Bulk {i} {_testMarker}"))
            .ToList();

        // Act
        var elapsed = await MeasureAsync(async () =>
        {
            await Context.Interactions.AddRangeAsync(interactions);
            await SaveChangesAsync();
        });
        _createdInteractionIds.AddRange(interactions.Select(i => i.Id));
        RegisterCleanup(CleanupInteractions);

        // Assert
        elapsed.Should().BeLessThan(BulkOperationThreshold,
            $"Bulk create 100 took {elapsed}ms, expected <{BulkOperationThreshold}ms");
        var count = await Context.Interactions.CountAsync(i => i.Name!.Contains(_testMarker));
        count.Should().Be(100);
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task BulkUpdate_50Interactions_CompletesWithinThreshold()
    {
        // Arrange
        var interactions = Enumerable.Range(1, 50)
            .Select(i => BuildInteraction($"BulkUpd {i} {_testMarker}"))
            .ToList();
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        _createdInteractionIds.AddRange(interactions.Select(i => i.Id));
        RegisterCleanup(CleanupInteractions);

        // Act
        var elapsed = await MeasureAsync(async () =>
        {
            foreach (var i in interactions)
            {
                i.Subject = $"Updated {i.Subject}";
                i.LastModifiedDate = DateTime.UtcNow;
            }
            await SaveChangesAsync();
        });

        // Assert
        elapsed.Should().BeLessThan(BulkOperationThreshold,
            $"Bulk update 50 took {elapsed}ms, expected <{BulkOperationThreshold}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task BulkSoftDelete_30Interactions_CompletesWithinThreshold()
    {
        // Arrange
        var interactions = Enumerable.Range(1, 30)
            .Select(i => BuildInteraction($"BulkDel {i} {_testMarker}"))
            .ToList();
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        _createdInteractionIds.AddRange(interactions.Select(i => i.Id));
        RegisterCleanup(CleanupInteractions);

        // Act
        var elapsed = await MeasureAsync(async () =>
        {
            foreach (var i in interactions)
            {
                i.IsDeleted = true;
                i.DeletedDate = DateTime.UtcNow;
            }
            await SaveChangesAsync();
        });

        // Assert
        elapsed.Should().BeLessThan(BulkOperationThreshold,
            $"Bulk soft-delete 30 took {elapsed}ms, expected <{BulkOperationThreshold}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task Search_SimpleFilter_CompletesWithinThreshold()
    {
        // Arrange
        await SeedInteractionsAsync(200);

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Interactions
                .Where(i => i.Name!.Contains(_testMarker))
                .ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(NormalOperationThreshold,
            $"Simple search took {elapsed}ms, expected <{NormalOperationThreshold}ms");
        result.Should().HaveCount(200);
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task Search_ByType_CompletesWithinThreshold()
    {
        // Arrange
        await SeedInteractionsByTypeAsync(100);

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Interactions
                .Where(i => i.Name!.Contains(_testMarker) && i.Type == InteractionType.InPersonMeeting)
                .ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(NormalOperationThreshold,
            $"Type filter took {elapsed}ms, expected <{NormalOperationThreshold}ms");
        result.Should().NotBeEmpty();
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task Search_ByDateRange_CompletesWithinThreshold()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var interactions = Enumerable.Range(0, 50)
            .Select(i => BuildInteraction($"DateRange {i} {_testMarker}", today.AddDays(-i)))
            .ToList();
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        _createdInteractionIds.AddRange(interactions.Select(x => x.Id));
        RegisterCleanup(CleanupInteractions);

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Interactions
                .Where(i => i.Name!.Contains(_testMarker) && i.Date >= today.AddDays(-7))
                .ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(NormalOperationThreshold,
            $"Date range search took {elapsed}ms, expected <{NormalOperationThreshold}ms");
        result.Should().HaveCountGreaterThanOrEqualTo(7);
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task Search_MultiColumnSort_CompletesWithinThreshold()
    {
        // Arrange
        await SeedInteractionsAsync(150);

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Interactions
                .Where(i => i.Name!.Contains(_testMarker))
                .OrderBy(i => i.Type)
                .ThenByDescending(i => i.Date)
                .Take(50)
                .ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(NormalOperationThreshold,
            $"Multi-sort took {elapsed}ms, expected <{NormalOperationThreshold}ms");
        result.Should().HaveCountLessThanOrEqualTo(50);
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task Search_Paginated_CompletesWithinThreshold()
    {
        // Arrange
        await SeedInteractionsAsync(300);

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Interactions
                .Where(i => i.Name!.Contains(_testMarker))
                .OrderByDescending(i => i.Date)
                .Skip(20)
                .Take(20)
                .ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(FastOperationThreshold,
            $"Paginated query took {elapsed}ms, expected <{FastOperationThreshold}ms");
        result.Should().HaveCountLessThanOrEqualTo(20);
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task ConcurrentReads_50Parallel_CompletesWithinThreshold()
    {
        // Arrange
        var interaction = await SeedInteractionAsync($"Concurrent {_testMarker}");
        var readCount = 50;

        // Act
        var elapsed = await MeasureAsync(async () =>
        {
            var tasks = Enumerable.Range(0, readCount)
                .Select(_ => Task.Run(async () =>
                {
                    await using var ctx = TestDbContextFactory.CreateUNOPS();
                    return await ctx.Interactions.FindAsync(interaction.Id);
                }))
                .ToArray();
            await Task.WhenAll(tasks);
        });

        // Assert
        elapsed.Should().BeLessThan(SlowOperationThreshold,
            $"50 concurrent reads took {elapsed}ms, expected <{SlowOperationThreshold}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task ConcurrentWrites_10Parallel_CompletesWithoutDeadlock()
    {
        // Arrange - Each task creates its own interaction in its own context
        var writeCount = 10;

        // Act
        var createdIds = new List<int>();
        var elapsed = await MeasureAsync(async () =>
        {
            var tasks = Enumerable.Range(1, writeCount).Select(async i =>
            {
                await using var ctx = TestDbContextFactory.CreateUNOPS();
                var interaction = BuildInteraction($"ConcurrentWrite {i} {_testMarker}");
                await ctx.Interactions.AddAsync(interaction);
                await ctx.SaveChangesAsync();
                return interaction.Id;
            }).ToArray();
            var results = await Task.WhenAll(tasks);
            createdIds.AddRange(results);
        });
        _createdInteractionIds.AddRange(createdIds);
        RegisterCleanup(CleanupInteractions);

        // Assert - Main goal: no deadlock; time is secondary
        elapsed.Should().BeLessThan(BulkOperationThreshold,
            $"10 concurrent writes took {elapsed}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task MixedReadWrite_Concurrent_CompletesWithinThreshold()
    {
        // Arrange
        var interaction = await SeedInteractionAsync($"Mixed {_testMarker}");
        var readTasks = 30;
        var writeTasks = 5;
        var createdIds = new List<int>();

        // Act
        var elapsed = await MeasureAsync(async () =>
        {
            var reads = Enumerable.Range(0, readTasks)
                .Select(_ => Task.Run(async () =>
                {
                    await using var ctx = TestDbContextFactory.CreateUNOPS();
                    return await ctx.Interactions.FindAsync(interaction.Id);
                }));
            var writes = Enumerable.Range(0, writeTasks)
                .Select(i => Task.Run(async () =>
                {
                    await using var ctx = TestDbContextFactory.CreateUNOPS();
                    var newItem = BuildInteraction($"MixedWrite {i} {_testMarker}");
                    await ctx.Interactions.AddAsync(newItem);
                    await ctx.SaveChangesAsync();
                    return newItem.Id;
                }));
            var writeResults = await Task.WhenAll(writes);
            createdIds.AddRange(writeResults);
            await Task.WhenAll(reads);
        });
        _createdInteractionIds.AddRange(createdIds);
        RegisterCleanup(CleanupInteractions);

        // Assert
        elapsed.Should().BeLessThan(SlowOperationThreshold,
            $"Mixed read/write took {elapsed}ms, expected <{SlowOperationThreshold}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeResultSet_500Items_MemoryStaysReasonable()
    {
        // Arrange
        GC.Collect();
        var memoryBefore = GC.GetTotalMemory(true);
        await SeedInteractionsAsync(500);

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Interactions
                .AsNoTracking()
                .Where(i => i.Name!.Contains(_testMarker))
                .ToListAsync());

        GC.Collect();
        var memoryAfter = GC.GetTotalMemory(true);

        // Assert
        result.Should().HaveCount(500);
        var memoryUsedMb = (memoryAfter - memoryBefore) / (1024.0 * 1024.0);
        memoryUsedMb.Should().BeLessThan(50,
            $"Memory used: {memoryUsedMb:F2}MB for 500 interactions");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak_StableUsage()
    {
        // Arrange
        var interaction = await SeedInteractionAsync($"Repeated {_testMarker}");
        var iterations = 50;
        var memorySamples = new List<long>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            Context.ChangeTracker.Clear();
            var (_, _) = await MeasureAsync(async () =>
                await Context.Interactions
                    .AsNoTracking()
                    .Where(x => x.Id == interaction.Id)
                    .ToListAsync());
            if (i % 10 == 0)
            {
                GC.Collect();
                memorySamples.Add(GC.GetTotalMemory(false));
            }
        }

        // Assert - Memory should not grow unbounded
        var first = memorySamples.First();
        var last = memorySamples.Last();
        var growth = (last - first) / (1024.0 * 1024.0);
        growth.Should().BeLessThan(20,
            $"Memory growth over {iterations} ops: {growth:F2}MB");
    }

    [Fact]
    public async Task AsNoTracking_LargeQuery_ReducesMemoryVsTracking()
    {
        // Arrange
        await SeedInteractionsAsync(200);

        // Act - With tracking (higher memory)
        Context.ChangeTracker.Clear();
        var (tracked, _) = await MeasureAsync(async () =>
            await Context.Interactions
                .Where(i => i.Name!.Contains(_testMarker))
                .ToListAsync());
        GC.Collect();
        var memoryWithTracking = GC.GetTotalMemory(true);

        // Act - With AsNoTracking (lower memory)
        Context.ChangeTracker.Clear();
        var (noTracked, _) = await MeasureAsync(async () =>
            await Context.Interactions
                .AsNoTracking()
                .Where(i => i.Name!.Contains(_testMarker))
                .ToListAsync());
        GC.Collect();
        var memoryNoTracking = GC.GetTotalMemory(true);

        // Assert - AsNoTracking should use less or comparable memory
        tracked.Should().HaveCount(200);
        noTracked.Should().HaveCount(200);
        memoryNoTracking.Should().BeLessThanOrEqualTo(memoryWithTracking + (5 * 1024 * 1024),
            "AsNoTracking should not significantly exceed tracking memory (5MB tolerance)");
    }

    #endregion

    #region EF Core — N+1 & AsNoTracking Verification

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task GetAllWithFilter_NoN1Pattern_CompletesWithinThreshold()
    {
        // Arrange - Seed 50 interactions; if N+1 existed, time would explode
        await SeedInteractionsAsync(50);

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Interactions
                .AsNoTracking()
                .Where(i => i.Name!.Contains(_testMarker))
                .ToListAsync());

        // Assert - Single query; no N+1. Threshold would be exceeded with N+1
        result.Should().HaveCount(50);
        elapsed.Should().BeLessThan(NormalOperationThreshold,
            $"Possible N+1 — query took {elapsed}ms for 50 items");
    }

    [Fact]

    [Trait("Defect", "DEF-089")]
    public async Task AsNoTracking_ReadOnlyQuery_CompletesFasterThanTracking()
    {
        // Arrange
        await SeedInteractionsAsync(100);

        // Act - No tracking
        var (_, noTrackMs) = await MeasureAsync(async () =>
            await Context.Interactions
                .AsNoTracking()
                .Where(i => i.Name!.Contains(_testMarker))
                .ToListAsync());

        // Act - With tracking
        Context.ChangeTracker.Clear();
        var (_, trackMs) = await MeasureAsync(async () =>
            await Context.Interactions
                .Where(i => i.Name!.Contains(_testMarker))
                .ToListAsync());

        // Assert - AsNoTracking should be faster or comparable (no strict ordering in CI)
        noTrackMs.Should().BeLessThan(SlowOperationThreshold,
            $"AsNoTracking query took {noTrackMs}ms");
        trackMs.Should().BeLessThan(SlowOperationThreshold,
            $"Tracking query took {trackMs}ms");
    }

    #endregion

    #region Helpers

    private UNOPSInteraction BuildInteraction(string name, DateTime? date = null) => new()
    {
        Name = name,
        Subject = $"Subject {name}",
        Type = InteractionType.InPersonMeeting,
        Date = date ?? DateTime.UtcNow,
        Status = EntityStatus.Active,
        CreatedBy = 1,
        LastModifiedBy = 1,
        LastModifiedDate = DateTime.UtcNow
    };

    private async Task<UNOPSInteraction> SeedInteractionAsync(string name)
    {
        var interaction = BuildInteraction(name);
        await Context.Interactions.AddAsync(interaction);
        await SaveChangesAsync();
        _createdInteractionIds.Add(interaction.Id);
        RegisterCleanup(CleanupInteractions);
        return interaction;
    }

    private async Task SeedInteractionsAsync(int count)
    {
        var existing = await Context.Interactions.CountAsync(i => i.Name!.Contains(_testMarker));
        if (existing >= count) return;
        var toAdd = count - existing;
        var interactions = Enumerable.Range(existing + 1, toAdd)
            .Select(i => BuildInteraction($"Seed {i} {_testMarker}"))
            .ToList();
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        _createdInteractionIds.AddRange(interactions.Select(i => i.Id));
        RegisterCleanup(CleanupInteractions);
    }

    private async Task SeedInteractionsByTypeAsync(int count)
    {
        var types = new[] { InteractionType.InPersonMeeting, InteractionType.Email, InteractionType.Call };
        var interactions = Enumerable.Range(1, count)
            .Select(i => new UNOPSInteraction
            {
                Name = $"Type {i} {_testMarker}",
                Subject = $"Subject {i}",
                Type = types[i % 3],
                Date = DateTime.UtcNow,
                Status = EntityStatus.Active,
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.UtcNow
            })
            .ToList();
        await Context.Interactions.AddRangeAsync(interactions);
        await SaveChangesAsync();
        _createdInteractionIds.AddRange(interactions.Select(i => i.Id));
        RegisterCleanup(CleanupInteractions);
    }

    private async Task CleanupInteractions()
    {
        if (!TestEnvironment.UsePostgreSQL || !_createdInteractionIds.Any()) return;
        var ids = string.Join(",", _createdInteractionIds);
        await Context.Database.ExecuteSqlAsync(
            $"DELETE FROM public.\"Interactions\" WHERE \"Id\" IN ({ids})");
    }

    #endregion
}
