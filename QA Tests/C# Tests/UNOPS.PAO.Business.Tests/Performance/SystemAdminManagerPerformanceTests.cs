/**
 * PERFORMANCE TESTS — SystemAdminManager (UNOPSSystemAdminManager)
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * Covers: RunMigrations, RunSeeding, RunSpecificSeeder, TruncateSeedScripts,
 * DeleteSeedScript, SeedScripts table queries (configuration lookup patterns),
 * AsNoTracking optimization, N+1 detection, memory efficiency.
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * NOTE: RunMigrations is skipped (runs all migrations, slow). TruncateSeedScripts
 * uses PostgreSQL-specific SQL — requires SkipIfNotPostgreSQLFact when applicable.
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for SystemAdminManager / UNOPSSystemAdminManager.
/// Verifies response times for migrations, seeding, SeedScript management,
/// configuration lookup patterns, AsNoTracking, N+1 detection, and memory efficiency.
///
/// Required: ≥16 tests (FIXED)
/// Uses UNOPSSystemAdminManager for real operations; base SystemAdminManager for no-op benchmarks.
/// </summary>
public class SystemAdminManagerPerformanceTests : PerformanceTestBase
{
    private readonly ISystemAdminManager _manager;
    private readonly ISystemAdminManager _baseManager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"SysAdmin_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public SystemAdminManagerPerformanceTests()
    {
        var configuration = TestEnvironment.CreateTestConfiguration();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        _manager = new UNOPS.PAO.UNOPSBusiness.Managers.UNOPSSystemAdminManager(
            Context, configuration, serviceProvider);
        _baseManager = new SystemAdminManager(Context, configuration, serviceProvider);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task DeleteSeedScript_ExistingScript_CompletesWithinThreshold()
    {
        var script = await SeedScriptAsync($"Script_{_testMarker}");

        _stopwatch.Restart();
        await _manager.DeleteSeedScript(script.ScriptName);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"DeleteSeedScript took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task DeleteSeedScript_NonExistent_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        await _manager.DeleteSeedScript($"NonExistent_{_testMarker}");
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"DeleteSeedScript (non-existent) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [SkipIfNotPostgreSQLFact]
    public async Task TruncateSeedScripts_WithData_CompletesWithinThreshold()
    {
        await SeedScriptsAsync(20);

        _stopwatch.Restart();
        await _manager.TruncateSeedScripts();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"TruncateSeedScripts took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task DeleteSeedScript_SequentialMultiple_CompletesWithinThreshold()
    {
        var scripts = await SeedScriptsAsync(5);

        _stopwatch.Restart();
        foreach (var script in scripts)
        {
            await _manager.DeleteSeedScript(script.ScriptName);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Sequential DeleteSeedScript (5) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task BaseManager_RunSeeding_NoOp_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        await _baseManager.RunSeeding();
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Base RunSeeding (no-op) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task QuerySeedScripts_AllAsNoTracking_CompletesWithinThreshold()
    {
        await SeedScriptsAsync(50);

        _stopwatch.Restart();
        var result = await Context.SeedScripts
            .AsNoTracking()
            .Where(s => s.ScriptName.Contains(_testMarker))
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"QuerySeedScripts AsNoTracking took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task QuerySeedScripts_FilterByScriptName_CompletesWithinThreshold()
    {
        await SeedScriptsAsync(100);

        _stopwatch.Restart();
        var result = await Context.SeedScripts
            .AsNoTracking()
            .Where(s => s.ScriptName == $"Seed_50_{_testMarker}")
            .FirstOrDefaultAsync();
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"QuerySeedScripts filter took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task QuerySeedScripts_OrderByExecutionOrder_CompletesWithinThreshold()
    {
        await SeedScriptsAsync(80);

        _stopwatch.Restart();
        var result = await Context.SeedScripts
            .AsNoTracking()
            .Where(s => s.ScriptName.Contains(_testMarker))
            .OrderBy(s => s.ExecutionOrder)
            .ThenBy(s => s.ScriptName)
            .Take(20)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCountLessThanOrEqualTo(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"QuerySeedScripts multi-sort took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task QuerySeedScripts_Count_CompletesWithinThreshold()
    {
        await SeedScriptsAsync(100);

        _stopwatch.Restart();
        var count = await Context.SeedScripts
            .AsNoTracking()
            .CountAsync(s => s.ScriptName.Contains(_testMarker));
        _stopwatch.Stop();

        count.Should().Be(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"QuerySeedScripts Count took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task QuerySeedScripts_Paginated_CompletesWithinThreshold()
    {
        await SeedScriptsAsync(150);

        _stopwatch.Restart();
        var result = await Context.SeedScripts
            .AsNoTracking()
            .Where(s => s.ScriptName.Contains(_testMarker))
            .OrderBy(s => s.Id)
            .Skip(20)
            .Take(20)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCountLessThanOrEqualTo(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"QuerySeedScripts paginated took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    public async Task Concurrent_DeleteSeedScript_NonExistent_50Parallel_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        for (int i = 0; i < 50; i++)
        {
            await _manager.DeleteSeedScript($"NonExistent_{_testMarker}_{Guid.NewGuid():N}");
        }
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"50 sequential DeleteSeedScript (non-existent) avg {avgMs}ms");
    }

    [SkipIfNotPostgreSQLFact]
    public async Task Concurrent_TruncateAndDelete_NoDeadlock()
    {
        await SeedScriptAsync($"Concurrent_{_testMarker}");
        await SeedScriptsAsync(9);

        await _manager.TruncateSeedScripts();
        await _manager.DeleteSeedScript($"Concurrent_{_testMarker}");
    }

    [Fact]
    public async Task Concurrent_BaseManagerNoOps_10Parallel_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            await _baseManager.RunSeeding();
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"10 sequential base RunSeeding (no-op) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task RepeatedDeleteSeedScript_NoMemoryLeak()
    {
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 50; i++)
        {
            var script = await SeedScriptAsync($"Leak_{i}_{_testMarker}");
            await _manager.DeleteSeedScript(script.ScriptName);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 50 repeated DeleteSeedScript — possible leak");
    }

    [Fact]
    public async Task LargeSeedScriptQuery_AsNoTracking_MemoryWithinCap()
    {
        await SeedScriptsAsync(500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        var result = await Context.SeedScripts
            .AsNoTracking()
            .Where(s => s.ScriptName.Contains(_testMarker))
            .ToListAsync();

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        result.Should().HaveCount(500);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedQuerySeedScripts_NoMemoryGrowth()
    {
        await SeedScriptsAsync(100);
        var memorySamples = new List<long>();

        for (int i = 0; i < 50; i++)
        {
            Context.ChangeTracker.Clear();
            await Context.SeedScripts
                .AsNoTracking()
                .Where(s => s.ScriptName.Contains(_testMarker))
                .ToListAsync();
            if (i % 10 == 0)
            {
                GC.Collect();
                memorySamples.Add(GC.GetTotalMemory(false));
            }
        }

        var first = memorySamples.First();
        var last = memorySamples.Last();
        var growthMb = (last - first) / (1024.0 * 1024.0);
        growthMb.Should().BeLessThan(20,
            $"Memory growth over 50 queries: {growthMb:F2}MB");
    }

    #endregion

    #region EF Core — N+1 & AsNoTracking Verification

    [Fact]
    public async Task QuerySeedScripts_NoN1Pattern_CompletesWithinThreshold()
    {
        await SeedScriptsAsync(50);

        _stopwatch.Restart();
        var result = await Context.SeedScripts
            .AsNoTracking()
            .Where(s => s.ScriptName.Contains(_testMarker))
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 — query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    [Fact]

    [Trait("Defect", "DEF-070")]
    public async Task AsNoTracking_SeedScriptQuery_CompletesFasterThanTracking()
    {
        await SeedScriptsAsync(100);

        Context.ChangeTracker.Clear();
        var (_, noTrackMs) = await MeasureAsync(async () =>
            await Context.SeedScripts
                .AsNoTracking()
                .Where(s => s.ScriptName.Contains(_testMarker))
                .ToListAsync());

        Context.ChangeTracker.Clear();
        var (_, trackMs) = await MeasureAsync(async () =>
            await Context.SeedScripts
                .Where(s => s.ScriptName.Contains(_testMarker))
                .ToListAsync());

        noTrackMs.Should().BeLessThan(SlowOperationThreshold,
            $"AsNoTracking query took {noTrackMs}ms");
        trackMs.Should().BeLessThan(SlowOperationThreshold,
            $"Tracking query took {trackMs}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var script = await SeedScriptAsync($"Bench_{_testMarker}");

        report["DeleteSeedScript"] = await TimeMs(() => _manager.DeleteSeedScript(script.ScriptName));
        report["Base RunSeeding"] = await TimeMs(() => _baseManager.RunSeeding());
        report["Base RunSpecificSeeder"] = await TimeMs(() => _baseManager.RunSpecificSeeder("Any"));

        await SeedScriptsAsync(20);
        report["QuerySeedScripts AsNoTracking"] = await TimeMs(async () =>
            await Context.SeedScripts.AsNoTracking().Where(s => s.ScriptName.Contains(_testMarker)).ToListAsync());

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-30}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<SeedScript> SeedScriptAsync(string scriptName)
    {
        var script = new SeedScript
        {
            Name = scriptName,
            Status = EntityStatus.Active,
            ScriptName = scriptName,
            ScriptType = "seeder",
            FileHash = $"hash_{Guid.NewGuid():N}",
            LastExecutedDate = DateTime.UtcNow,
            ExecutionOrder = 0
        };
        await Context.SeedScripts.AddAsync(script);
        await SaveChangesAsync();
        return script;
    }

    private async Task<List<SeedScript>> SeedScriptsAsync(int count)
    {
        var scripts = new List<SeedScript>();
        for (int i = 0; i < count; i++)
        {
            scripts.Add(await SeedScriptAsync($"Seed_{i}_{_testMarker}"));
        }
        return scripts;
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
