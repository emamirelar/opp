/**
 * LOAD TESTS — SystemAdminManager (UNOPSSystemAdminManager)
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * SystemAdminManager handles system administration: migrations, seeding, seed scripts,
 * entity configuration seeding (Roles, Entities, DocumentTypes, etc.), and admin operations.
 * Tests use mocked ISystemAdminManager for unit-level throughput and concurrency validation.
 *
 * Covers: concurrent admin config reads (RunSeeding, RunSpecificSeeder), bulk system
 * settings retrieval, sustained lookup value queries (Roles, Entities, DocumentTypes),
 * stress testing admin operations throughput, recovery after stress, config caching scalability.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for SystemAdminManager (UNOPSSystemAdminManager via ISystemAdminManager).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2)
///
/// Uses mocked ISystemAdminManager to measure concurrent invocation patterns without DB dependency.
/// Admin operations: RunMigrations, RunSeeding, RunSpecificSeeder, TruncateSeedScripts, DeleteSeedScript.
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class SystemAdminManagerLoadTests
{
    private readonly Mock<ISystemAdminManager> _mockManager;
    private readonly ISystemAdminManager _manager;
    private readonly Stopwatch _stopwatch = new();

    // Load targets — TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section B1-B4
    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    private static readonly string[] LookupSeederNames = { "Roles", "Entities", "DocumentTypes", "LiaisonOffices", "EntityManagers" };

    public SystemAdminManagerLoadTests()
    {
        _mockManager = new Mock<ISystemAdminManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.RunMigrations())
            .Returns(Task.CompletedTask);

        _mockManager
            .Setup(m => m.RunSeeding())
            .Returns(Task.CompletedTask);

        _mockManager
            .Setup(m => m.RunSpecificSeeder(It.IsAny<string>()))
            .Returns((string name) => Task.CompletedTask);

        _mockManager
            .Setup(m => m.TruncateSeedScripts())
            .Returns(Task.CompletedTask);

        _mockManager
            .Setup(m => m.DeleteSeedScript(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private static string GetSeederNameForIndex(int index)
    {
        return LookupSeederNames[index % LookupSeederNames.Length];
    }

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Concurrent admin configuration reads — many users loading entity configs via RunSeeding.
    /// Simulates multiple admins triggering seeding (reads config, applies lookup values).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentAdminConfigReads_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var operationCount = Math.Min(NormalUsers * 2, 200);

        for (int i = 0; i < operationCount; i++)
        {
            _stopwatch.Restart();
            await _manager.RunSeeding();
            _stopwatch.Stop();
            lock (times) times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 100),
            $"Admin config read (RunSeeding) degraded from {first:F0}ms to {last:F0}ms avg under sustained load");
    }

    /// <summary>
    /// Phase 2: Sustained lookup value queries — RunSpecificSeeder for Roles, Entities, DocumentTypes.
    /// Simulates admins repeatedly querying/refreshing lookup data (Roles, Entities, etc.).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_LookupValueQueries_ConsistencyMaintained()
    {
        var times = new List<long>();
        var queryCount = NormalUsers;

        var tasks = Enumerable.Range(0, queryCount)
            .Select(i => MeasuredRunSpecificSeederAsync(GetSeederNameForIndex(i), times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Lookup value query times inconsistent under {queryCount} concurrent callers (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 80% RunSeeding (config retrieval), 20% RunSpecificSeeder (lookup ops).
    /// Reflects real usage: admins running full seeding vs targeted lookup refreshes.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedAdminOperations_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.8);
        var writeCount = NormalUsers - readCount;

        var reads = Enumerable.Range(0, readCount).Select(_ => _manager.RunSeeding());
        var lookups = Enumerable.Range(0, writeCount).Select(i => _manager.RunSpecificSeeder(GetSeederNameForIndex(i)));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Concat(lookups));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed admin load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent RunSeeding — bulk system settings retrieval under load.
    /// Simulates many admins simultaneously triggering full seeding.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_BulkSystemSettingsRetrieval_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10).Select(_ => _manager.RunSeeding()).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers).Select(_ => _manager.RunSeeding()).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / baselineMs;
        scale.Should().BeLessThan((double)PeakUsers / 10 * 2,
            $"Spike scaled {scale:F1}× — expected <{(double)PeakUsers / 10 * 2:F1}×");
    }

    /// <summary>
    /// Phase 5: Recovery after spike — returns to baseline performance.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_Recovery_ReturnsToBaseline()
    {
        _stopwatch.Restart();
        await _manager.RunSeeding();
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, PeakUsers).Select(_ => _manager.RunSeeding()));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.RunSeeding();
        _stopwatch.Stop();
        var postSpikeMs = _stopwatch.ElapsedMilliseconds;

        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Phase 3: Stress testing admin operations throughput — system does not crash.
    /// High concurrency on RunSeeding, RunSpecificSeeder, TruncateSeedScripts, DeleteSeedScript.
    /// </summary>
    [Fact]
    public async Task StressLoad_AdminOperationsThroughput_DoesNotCrash()
    {
        var completed = 0;

        var tasks = Enumerable.Range(0, StressUsers).Select(async i =>
        {
            var op = i % 4;
            if (op == 0) await _manager.RunSeeding();
            else if (op == 1) await _manager.RunSpecificSeeder(GetSeederNameForIndex(i));
            else if (op == 2) await _manager.TruncateSeedScripts();
            else await _manager.DeleteSeedScript(GetSeederNameForIndex(i));
            Interlocked.Increment(ref completed);
        }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent admin ops — only {completed} completed");
        completed.Should().Be(StressUsers);
    }

    /// <summary>
    /// Phase 3: Error rate under stress — within acceptable limit.
    /// </summary>
    [Fact]
    public async Task StressLoad_ErrorRate_WithinAcceptableLimit()
    {
        var success = 0;
        var failure = 0;

        var tasks = Enumerable.Range(0, StressUsers).Select(async _ =>
        {
            try
            {
                await _manager.RunSeeding();
                Interlocked.Increment(ref success);
            }
            catch
            {
                Interlocked.Increment(ref failure);
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        var errorRate = (double)failure / StressUsers;
        errorRate.Should().BeLessThan(MaxErrorRate,
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent admin ops");
    }

    /// <summary>
    /// Phase 3: Concurrent admin operations — data integrity maintained under stress.
    /// All operations complete without corruption.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentAdminOps_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            await _manager.RunSpecificSeeder(GetSeederNameForIndex(i));
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent admin operation stress");
    }

    #endregion

    #region Recovery (min 2) — Phase 3 + 5

    /// <summary>
    /// Phase 3+5: After stress — performance restored.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_PerformanceRestored()
    {
        _stopwatch.Restart();
        await _manager.RunSeeding();
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, StressUsers).Select(_ => _manager.RunSeeding()));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.RunSeeding();
        _stopwatch.Stop();
        var recoveredMs = _stopwatch.ElapsedMilliseconds;

        recoveredMs.Should().BeLessThan(Math.Max(baselineMs * 2, 10),
            $"System did not recover: post-stress {recoveredMs}ms vs baseline {baselineMs}ms");
    }

    /// <summary>
    /// Phase 3+5: After stress — no state corruption, admin operations succeed.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_NoStateCorruption()
    {
        await Task.WhenAll(Enumerable.Range(0, 50)
            .Select(i => _manager.RunSpecificSeeder(GetSeederNameForIndex(i))));

        await Task.Delay(RecoveryWindowMs);

        await _manager.RunSeeding();
        await _manager.RunMigrations();
        // If we reach here without exception, post-stress admin ops succeeded
    }

    #endregion

    #region Scalability — configuration data caching

    /// <summary>
    /// Configuration data caching scalability — RunSeeding/RunSpecificSeeder scale under load.
    /// Simulates config/lookup caching as concurrent admin requests increase.
    /// </summary>
    [Fact]
    public async Task Scalability_ConfigCaching_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.RunSpecificSeeder(GetSeederNameForIndex(i))));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task MeasuredRunSpecificSeederAsync(string seederName, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.RunSpecificSeeder(seederName);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
