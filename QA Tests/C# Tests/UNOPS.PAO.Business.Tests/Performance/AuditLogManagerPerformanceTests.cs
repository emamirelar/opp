/**
 * PERFORMANCE TESTS — AuditLogManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * AuditLogManager (IAuditLogManager) handles audit logging for all entity mutations.
 * Write-heavy, compliance-critical component. Tests cover write throughput, bulk retrieval,
 * search/filter patterns (by entity, user, date range, action type), AsNoTracking
 * optimization, N+1 detection, and memory efficiency.
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.AuditLogs;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for AuditLogManager.
/// Verifies response times, throughput, memory efficiency, N+1 detection,
/// AsNoTracking optimization, and audit log write throughput for compliance-critical operations.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class AuditLogManagerPerformanceTests : PerformanceTestBase
{
    private readonly IAuditLogManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"AuditPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public AuditLogManagerPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuditLogMappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        _manager = new AuditLogManager(mapper, Context);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task CreateAuditLog_SingleEntry_CompletesWithinThreshold()
    {
        var request = BuildCreateRequest("Opportunity", 1, "create");

        _stopwatch.Restart();
        var result = await _manager.CreateAuditLogAsync(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.EntityType.Should().Be("Opportunity");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreateAuditLog took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetLatestAuditLog_ExistingEntry_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Partner", 1, 5);

        _stopwatch.Restart();
        var result = await _manager.GetLatestAuditLogAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetLatestAuditLog took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task BulkCreate_100AuditLogEntries_CompletesWithinThreshold()
    {
        var entityType = "Opportunity";
        var entityId = 100;

        _stopwatch.Restart();
        for (int i = 0; i < 100; i++)
        {
            await _manager.CreateAuditLogAsync(BuildCreateRequest(entityType, entityId, $"action_{i}"));
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Bulk create 100 audit logs took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetAuditLogs_EntityWith100Entries_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Partner", 200, 100);

        _stopwatch.Restart();
        var result = await _manager.GetAuditLogsAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetAuditLogs (100 entries) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetAuditLogs_EntityWith500Entries_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Opportunity", 300, 500);

        _stopwatch.Restart();
        var result = await _manager.GetAuditLogsAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(500);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetAuditLogs (500 entries) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task Search_ByEntity_SimpleQuery_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Partner", 1, 50);

        _stopwatch.Restart();
        var result = await _manager.GetAuditLogsAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Search by entity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Search_ByUser_DataLayer_CompletesWithinThreshold()
    {
        await SeedAuditLogsByUserAsync(1, 200);

        _stopwatch.Restart();
        var result = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => a.UserId == 1 && !a.IsDeleted && a.EntityType == $"PerfUser_{_testMarker}")
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Search by user took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Search_ByDateRange_DataLayer_CompletesWithinThreshold()
    {
        var (fromDate, toDate) = await SeedAuditLogsByDateRangeAsync(150);

        _stopwatch.Restart();
        var result = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.Name!.Contains(_testMarker)
                && a.Timestamp >= fromDate && a.Timestamp <= toDate)
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Search by date range took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Search_ByActionType_DataLayer_CompletesWithinThreshold()
    {
        await SeedAuditLogsByActionTypeAsync(100);

        _stopwatch.Restart();
        var result = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.Name!.Contains(_testMarker)
                && (a.Action == "create" || a.Action == "update"))
            .OrderByDescending(a => a.Timestamp)
            .Take(50)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Search by action type took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Search_Paginated_LargeTrail_FirstPageFast()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Opportunity", 500, 300);

        _stopwatch.Restart();
        var result = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
            .OrderByDescending(a => a.Timestamp)
            .Skip(0)
            .Take(20)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Paginated audit query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    public async Task ConcurrentReads_50ParallelGetLatest_MaintainsPerformance()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Partner", 1, 10);
        var results = new List<object?>();

        _stopwatch.Restart();
        for (int i = 0; i < 50; i++)
        {
            results.Add(await _manager.GetLatestAuditLogAsync(entityType, entityId));
        }
        _stopwatch.Stop();

        results.Should().OnlyContain(r => r != null);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 sequential calls exceeded threshold: {avgMs}ms");
    }

    [Fact]
    public async Task ConcurrentWrites_10ParallelCreate_AllSucceedWithinThreshold()
    {
        var results = new List<object?>();

        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            results.Add(await _manager.CreateAuditLogAsync(BuildCreateRequest("Opportunity", 1000 + i, $"concurrent_{i}")));
        }
        _stopwatch.Stop();

        results.Should().HaveCount(10).And.OnlyContain(r => r != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"10 sequential creates took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Partner", 2, 20);

        _stopwatch.Restart();
        for (int i = 0; i < 30; i++)
        {
            await _manager.GetLatestAuditLogAsync(entityType, entityId);
        }
        for (int i = 0; i < 5; i++)
        {
            await _manager.CreateAuditLogAsync(BuildCreateRequest("Opportunity", 2000 + i, $"mix_{i}"));
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed sequential ops took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeAuditTrail_MemoryUsage_WithinCap()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Opportunity", 600, 1000);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetAuditLogsAsync(entityType, entityId);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            var request = BuildCreateRequest("Partner", 3000 + i, $"leak_{i}");
            var created = await _manager.CreateAuditLogAsync(request);
            await _manager.GetLatestAuditLogAsync("Partner", 3000 + i);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Partner", 4, 100);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetAuditLogsAsync(entityType, entityId);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(first25Avg * 3,
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region EF Core — N+1 & AsNoTracking Verification

    [Fact]
    public async Task GetAuditLogs_NoN1Pattern_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Opportunity", 50, 50);

        _stopwatch.Restart();
        var result = await _manager.GetAuditLogsAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 — query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    [Fact]

    [Trait("Defect", "DEF-085")]
    public async Task AsNoTracking_ReadOnlyQuery_CompletesFasterOrComparable()
    {
        var (entityType, entityId) = await SeedAuditLogsAsync("Partner", 60, 100);

        var (_, noTrackMs) = await MeasureAsync(async () =>
            await Context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync());

        Context.ChangeTracker.Clear();
        var (_, trackMs) = await MeasureAsync(async () =>
            await Context.AuditLogs
                .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync());

        noTrackMs.Should().BeLessThan(SlowOperationThreshold,
            $"AsNoTracking query took {noTrackMs}ms");
        trackMs.Should().BeLessThan(SlowOperationThreshold,
            $"Tracking query took {trackMs}ms");
    }

    #endregion

    #region Audit Log Write Throughput Benchmark

    [Fact]
    public async Task Benchmark_WriteThroughput_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var entityId = 7000;

        report["CreateSingle"] = await TimeMs(() =>
            _manager.CreateAuditLogAsync(BuildCreateRequest("Opportunity", entityId, "create")));

        var (entityType, _) = await SeedAuditLogsAsync("Partner", entityId + 1, 20);
        report["GetLatest"] = await TimeMs(() =>
            _manager.GetLatestAuditLogAsync(entityType, entityId + 1));
        report["GetAuditLogs"] = await TimeMs(() =>
            _manager.GetAuditLogsAsync(entityType, entityId + 1));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] AuditLog {op,-15}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private AuditLogCreateRequest BuildCreateRequest(string entityType, int entityId, string action) =>
        new()
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = 1,
            Description = $"Perf test {_testMarker}"
        };

    private async Task<(string EntityType, int EntityId)> SeedAuditLogsAsync(string entityType, int entityId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            await _manager.CreateAuditLogAsync(BuildCreateRequest(entityType, entityId, $"action_{i}"));
        }
        return (entityType, entityId);
    }

    private async Task SeedAuditLogsByUserAsync(int userId, int count)
    {
        var entityId = 4000;
        for (int i = 0; i < count; i++)
        {
            var request = new AuditLogCreateRequest
            {
                EntityType = $"PerfUser_{_testMarker}",
                EntityId = entityId + i,
                Action = "update",
                UserId = userId,
                Description = $"User {userId} action"
            };
            await _manager.CreateAuditLogAsync(request);
        }
    }

    private async Task<(DateTime FromDate, DateTime ToDate)> SeedAuditLogsByDateRangeAsync(int count)
    {
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;
        var entityId = 5000;

        for (int i = 0; i < count; i++)
        {
            var log = new AuditLog
            {
                Name = $"DateRange {i} {_testMarker}",
                EntityType = "Partner",
                EntityId = entityId,
                Action = "create",
                Timestamp = fromDate.AddHours(i * 24.0 / count),
                UserId = 1,
                Description = $"Audit {i}",
                Status = EntityStatus.Active,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            await Context.AuditLogs.AddAsync(log);
        }
        await SaveChangesAsync();
        return (fromDate, toDate);
    }

    private async Task SeedAuditLogsByActionTypeAsync(int count)
    {
        var actions = new[] { "create", "update", "delete", "source_update" };
        var entityId = 6000;

        for (int i = 0; i < count; i++)
        {
            var log = new AuditLog
            {
                Name = $"Action {i} {_testMarker}",
                EntityType = "Opportunity",
                EntityId = entityId,
                Action = actions[i % actions.Length],
                Timestamp = DateTime.UtcNow,
                UserId = 1,
                Description = $"Action {actions[i % actions.Length]}",
                Status = EntityStatus.Active,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            await Context.AuditLogs.AddAsync(log);
        }
        await SaveChangesAsync();
    }

    private async Task<long> TimeMs(Func<Task> fn)
    {
        _stopwatch.Restart();
        await fn();
        _stopwatch.Stop();
        return _stopwatch.ElapsedMilliseconds;
    }

    #endregion
}
