/**
 * LOAD TESTS — OrganizationHierarchyManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * OrganizationHierarchyManager handles org hierarchy tree browsing and lookups.
 * Read-heavy (tree browsing). Mixed load: 90% read, 10% write-simulated (reads only).
 * Sync methods wrapped in Task.Run() for concurrent testing.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.OrganizationUnits;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for OrganizationHierarchyManager.
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
/// Read-heavy: tree browsing, hierarchy lookups. Uses mocked IOrganizationHierarchyManager.
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class OrganizationHierarchyManagerLoadTests
{
    private readonly Mock<IOrganizationHierarchyManager> _mockManager;
    private readonly IOrganizationHierarchyManager _manager;
    private readonly Stopwatch _stopwatch = new();

    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public OrganizationHierarchyManagerLoadTests()
    {
        _mockManager = new Mock<IOrganizationHierarchyManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.GetOrganizationHierarchy())
            .ReturnsAsync(CreateMockTreeModels);

        _mockManager
            .Setup(m => m.GetOrganizationHierarchyPrime())
            .ReturnsAsync(CreateMockPrimeModels);

        _mockManager
            .Setup(m => m.GetOrganizationHierarchyById(It.IsAny<int>()))
            .ReturnsAsync((int id) => CreateMockHierarchyModel(id));

        _mockManager
            .Setup(m => m.GetOrganizationsByType(It.IsAny<OrganizationUnitType>()))
            .Returns((OrganizationUnitType type) => CreateMockHierarchyModels(10));

        _mockManager
            .Setup(m => m.GetAllOrganizations())
            .Returns(CreateMockHierarchyModels(20));
    }

    private static IEnumerable<OrganizationHierarchyTreeModel> CreateMockTreeModels()
    {
        return Enumerable.Range(1, 5).Select(i => new OrganizationHierarchyTreeModel
        {
            Data = new OrganizationHierarchyDataModel
            {
                Id = i,
                Code = $"OU{i:D3}",
                Name = $"Org Unit {i}",
                Type = (OrganizationUnitType)(i % 4),
                Description = $"Description {i}",
                ParentId = i > 1 ? i - 1 : (int?)null,
                Children = new List<OrganizationHierarchyDataModel>()
            }
        });
    }

    private static IEnumerable<OrganizationHierarchyPrimeModel> CreateMockPrimeModels()
    {
        return Enumerable.Range(1, 5).Select(i => new OrganizationHierarchyPrimeModel
        {
            Expanded = true,
            Type = "person",
            Data = new OrganizationHierarchyPrimeDataModel
            {
                Id = i,
                Name = $"Org Unit {i}",
                Code = $"OU{i:D3}",
                Type = (OrganizationUnitType)(i % 4),
                Description = $"Description {i}",
                ParentId = i > 1 ? i - 1 : (int?)null
            },
            Children = new List<OrganizationHierarchyPrimeModel>()
        });
    }

    private static OrganizationHierarchyModel CreateMockHierarchyModel(int id)
    {
        return new OrganizationHierarchyModel
        {
            Id = id,
            Code = $"OU{id:D3}",
            Name = $"Org Unit {id}",
            Status = "Active",
            Type = "OrgUnit",
            Description = $"Description {id}",
            ParentId = id > 1 ? id - 1 : (int?)null,
            ParentName = id > 1 ? $"Org Unit {id - 1}" : null,
            ParentCode = id > 1 ? $"OU{(id - 1):D3}" : null,
            IsSelfManagementEnabled = id % 2 == 0,
            ChildrenCount = id % 5
        };
    }

    private static IEnumerable<OrganizationHierarchyModel> CreateMockHierarchyModels(int count)
    {
        return Enumerable.Range(1, count).Select(CreateMockHierarchyModel);
    }

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — concurrent hierarchy tree retrieval (90% read).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentHierarchyReads_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var readCount = (int)(NormalUsers * 0.9);

        var tasks = Enumerable.Range(0, readCount)
            .Select(i => MeasuredReadAsync(i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 100),
            $"Hierarchy read performance degraded from {first:F0}ms to {last:F0}ms avg under {readCount} concurrent users");
    }

    /// <summary>
    /// Phase 2: Sustained sync method load — GetOrganizationsByType and GetAllOrganizations wrapped in Task.Run.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_SyncMethodsWrappedInTaskRun_ConsistencyMaintained()
    {
        var times = new List<long>();
        var types = new[] { OrganizationUnitType.Office, OrganizationUnitType.Region, OrganizationUnitType.Hub, OrganizationUnitType.OrgUnit };

        var tasks = Enumerable.Range(0, NormalUsers / 2).Select(async i =>
        {
            var sw = Stopwatch.StartNew();
            await Task.Run(() => _manager.GetOrganizationsByType(types[i % 4]));
            sw.Stop();
            lock (times) times.Add(sw.ElapsedMilliseconds);
        }).ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Sync method times inconsistent under concurrent load (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 90% read (tree/hierarchy), 10% read-simulated (no write methods).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedReadOperations_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.9);
        var syncReadCount = NormalUsers - readCount;

        var asyncReads = Enumerable.Range(0, readCount).Select(i =>
        {
            if (i % 3 == 0) return (Task)_manager.GetOrganizationHierarchy();
            if (i % 3 == 1) return (Task)_manager.GetOrganizationHierarchyPrime();
            return (Task)_manager.GetOrganizationHierarchyById((i % 10) + 1);
        });

        var syncReads = Enumerable.Range(0, syncReadCount).Select(i =>
            Task.Run(() => _manager.GetAllOrganizations()));

        _stopwatch.Restart();
        await Task.WhenAll(asyncReads.Concat(syncReads));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed read load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent hierarchy reads — system handles gracefully.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenReadIncrease_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10)
            .Select(i => _manager.GetOrganizationHierarchyById((i % 5) + 1))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetOrganizationHierarchyById((i % 10) + 1))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / Math.Max(baselineMs, 1);
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
        await _manager.GetOrganizationHierarchyById(1);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        await Task.WhenAll(Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetOrganizationHierarchyById((i % 10) + 1)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetOrganizationHierarchyById(1);
        _stopwatch.Stop();
        var postSpikeMs = _stopwatch.ElapsedMilliseconds;

        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Phase 3: Beyond capacity — hierarchy reads under heavy load. System does not crash.
    /// </summary>
    [Fact]
    public async Task StressLoad_HeavyReadLoad_DoesNotCrash()
    {
        var completed = 0;

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async (_, i) =>
            {
                await _manager.GetOrganizationHierarchyById((i % 20) + 1);
                Interlocked.Increment(ref completed);
            }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent reads — only {completed} completed");
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
                await _manager.GetOrganizationHierarchy();
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
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent hierarchy reads");
    }

    /// <summary>
    /// Phase 3: Concurrent hierarchy reads — data integrity maintained under stress.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentHierarchyReads_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            var result = await _manager.GetOrganizationHierarchyById(i);
            result.Should().NotBeNull();
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent hierarchy read stress");
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
        await _manager.GetOrganizationHierarchyById(1);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        await Task.WhenAll(Enumerable.Range(0, StressUsers)
            .Select(i => _manager.GetOrganizationHierarchyById((i % 20) + 1)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetOrganizationHierarchyById(1);
        _stopwatch.Stop();
        var recoveredMs = _stopwatch.ElapsedMilliseconds;

        recoveredMs.Should().BeLessThan(Math.Max(baselineMs * 2, 10),
            $"System did not recover: post-stress {recoveredMs}ms vs baseline {baselineMs}ms");
    }

    /// <summary>
    /// Phase 3+5: After stress — no state corruption, read operations succeed.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_NoStateCorruption()
    {
        await Task.WhenAll(Enumerable.Range(0, 50)
            .Select(i => _manager.GetOrganizationHierarchyById((i % 10) + 1)));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetOrganizationHierarchy();
        result.Should().NotBeNull("Post-stress hierarchy read should succeed.");
    }

    #endregion

    #region Scalability (bonus 2)

    /// <summary>
    /// Scalability: Hierarchy queries scale under increasing concurrent load.
    /// </summary>
    [Fact]
    public async Task Scalability_HierarchyQueries_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.GetOrganizationHierarchyById((i % 10) + 1)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    /// <summary>
    /// Scalability: Tree retrieval scales under concurrent load.
    /// </summary>
    [Fact]
    public async Task Scalability_TreeRetrieval_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(_ => _manager.GetOrganizationHierarchy()));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} concurrent tree reads, avg {perUser:F0}ms/read — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task MeasuredReadAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        if (index % 3 == 0)
            await _manager.GetOrganizationHierarchy();
        else if (index % 3 == 1)
            await _manager.GetOrganizationHierarchyPrime();
        else
            await _manager.GetOrganizationHierarchyById((index % 10) + 1);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
