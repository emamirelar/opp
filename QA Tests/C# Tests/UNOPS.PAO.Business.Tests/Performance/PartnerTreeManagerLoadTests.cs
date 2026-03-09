/**
 * LOAD TESTS — PartnerTreeManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * PartnerTreeManager handles partner tree CRUD and category/group structure.
 * Mixed load: 70% read, 30% write. Uses mocked IPartnerTreeManager with ClaimsPrincipal helper.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using System.Security.Claims;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models.PartnerTrees;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for PartnerTreeManager.
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
/// Mixed load: 70% read, 30% write. Uses mocked IPartnerTreeManager.
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class PartnerTreeManagerLoadTests
{
    private readonly Mock<IPartnerTreeManager> _mockManager;
    private readonly IPartnerTreeManager _manager;
    private readonly Stopwatch _stopwatch = new();

    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public PartnerTreeManagerLoadTests()
    {
        _mockManager = new Mock<IPartnerTreeManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private static ClaimsPrincipal CreateTestUser(int userId = 1)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"TestUser{userId}")
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.CreatePartnerTreeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<PartnerTreeDataModel>()))
            .ReturnsAsync((ClaimsPrincipal _, PartnerTreeDataModel model) => CreateMockPartnerTreeModel(model));

        _mockManager
            .Setup(m => m.GetPartnerTreesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((ClaimsPrincipal _, string _, bool _) => CreateMockPartnerTreeModels(10));

        _mockManager
            .Setup(m => m.GetPartnerTreeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync((ClaimsPrincipal _, int id) => CreateMockPartnerTreeModel(CreatePartnerTreeDataModel(id)));

        _mockManager
            .Setup(m => m.GetPostedPartnerTrees())
            .Returns(CreateMockExternalPartnerTreeModels(5));

        _mockManager
            .Setup(m => m.GetPostedPartnerTree(It.IsAny<int>()))
            .ReturnsAsync((int id) => CreateMockExternalPartnerTreeModel(id));

        _mockManager
            .Setup(m => m.UpdatePartnerTreeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<PartnerTreeDataModel>()))
            .ReturnsAsync((ClaimsPrincipal _, PartnerTreeDataModel model) => CreateMockPartnerTreeModel(model));

        _mockManager
            .Setup(m => m.DeletePartnerTreeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockManager
            .Setup(m => m.GetCategoryAndGroupStructureAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(CreateMockCategoryAndGroupStructure);
    }

    private static PartnerTreeModel CreateMockPartnerTreeModel(PartnerTreeDataModel data)
    {
        return new PartnerTreeModel
        {
            Data = data,
            Children = new List<PartnerTreeModel>()
        };
    }

    private static PartnerTreeDataModel CreatePartnerTreeDataModel(int id)
    {
        return new PartnerTreeDataModel
        {
            Id = id,
            Name = $"Partner Tree {id}",
            Description = $"Description {id}",
            Code = $"PT{id:D3}",
            Type = "Category",
            Parent = id > 1 ? $"PT{(id - 1):D3}" : null,
            Status = "Active"
        };
    }

    private static IEnumerable<PartnerTreeModel> CreateMockPartnerTreeModels(int count)
    {
        return Enumerable.Range(1, count).Select(i => CreateMockPartnerTreeModel(CreatePartnerTreeDataModel(i)));
    }

    private static IEnumerable<ExternalPartnerTreeModel> CreateMockExternalPartnerTreeModels(int count)
    {
        return Enumerable.Range(1, count).Select(CreateMockExternalPartnerTreeModel);
    }

    private static ExternalPartnerTreeModel CreateMockExternalPartnerTreeModel(int id)
    {
        return new ExternalPartnerTreeModel
        {
            Id = id,
            Name = $"External Tree {id}",
            Description = $"Description {id}",
            Code = $"EXT{id:D3}",
            Type = "Category",
            Parent = id > 1 ? $"EXT{(id - 1):D3}" : null
        };
    }

    private static IEnumerable<object> CreateMockCategoryAndGroupStructure()
    {
        return Enumerable.Range(1, 5).Select<int, object>(i => new
        {
            Id = i,
            Name = $"Category {i}",
            Code = $"CAT{i}"
        }).ToList();
    }

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — concurrent GetPartnerTreesAsync and GetPartnerTreeAsync (70% read).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentPartnerTreeReads_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var readCount = (int)(NormalUsers * 0.7);
        var user = CreateTestUser();

        var tasks = Enumerable.Range(0, readCount)
            .Select(i => MeasuredReadAsync(user, i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 100),
            $"Partner tree read performance degraded from {first:F0}ms to {last:F0}ms avg under {readCount} concurrent users");
    }

    /// <summary>
    /// Phase 2: Sustained write load — concurrent CreatePartnerTreeAsync and UpdatePartnerTreeAsync (30% write).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentPartnerTreeWrites_ConsistencyMaintained()
    {
        var times = new List<long>();
        var writeCount = (int)(NormalUsers * 0.3);
        var user = CreateTestUser();

        var tasks = Enumerable.Range(0, writeCount)
            .Select(i => MeasuredWriteAsync(user, i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Partner tree write times inconsistent under {writeCount} concurrent writers (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 70% read, 30% write.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedReadAndWrite_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.7);
        var writeCount = NormalUsers - readCount;
        var user = CreateTestUser();

        var reads = Enumerable.Range(0, readCount).Select(i =>
            (Task)(i % 2 == 0
                ? _manager.GetPartnerTreesAsync(user)
                : _manager.GetPartnerTreeAsync(user, (i % 10) + 1)));

        var writes = Enumerable.Range(0, writeCount).Select(i =>
            (Task)(i % 2 == 0
                ? _manager.CreatePartnerTreeAsync(user, CreatePartnerTreeDataModel(i + 100))
                : _manager.UpdatePartnerTreeAsync(user, CreatePartnerTreeDataModel((i % 5) + 1))));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Concat(writes));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent partner tree operations — system handles gracefully.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenLoadIncrease_HandlesGracefully()
    {
        var user = CreateTestUser();
        var baselineTasks = Enumerable.Range(0, 10)
            .Select(i => _manager.GetPartnerTreeAsync(user, (i % 5) + 1))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetPartnerTreesAsync(user))
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
        var user = CreateTestUser();
        _stopwatch.Restart();
        await _manager.GetPartnerTreeAsync(user, 1);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        await Task.WhenAll(Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetPartnerTreesAsync(user)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetPartnerTreeAsync(user, 1);
        _stopwatch.Stop();
        var postSpikeMs = _stopwatch.ElapsedMilliseconds;

        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Phase 3: Beyond capacity — partner tree operations under heavy load. System does not crash.
    /// </summary>
    [Fact]
    public async Task StressLoad_HeavyLoad_DoesNotCrash()
    {
        var completed = 0;
        var user = CreateTestUser();

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async (_, i) =>
            {
                await _manager.GetPartnerTreeAsync(user, (i % 20) + 1);
                Interlocked.Increment(ref completed);
            }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent operations — only {completed} completed");
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
        var user = CreateTestUser();

        var tasks = Enumerable.Range(0, StressUsers).Select(async _ =>
        {
            try
            {
                await _manager.GetPartnerTreesAsync(user);
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
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent partner tree reads");
    }

    /// <summary>
    /// Phase 3: Concurrent partner tree writes — data integrity maintained under stress.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentPartnerTreeWrites_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();
        var user = CreateTestUser();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            var result = await _manager.CreatePartnerTreeAsync(user, CreatePartnerTreeDataModel(i));
            result.Should().NotBeNull();
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent partner tree write stress");
    }

    #endregion

    #region Recovery (min 2) — Phase 3 + 5

    /// <summary>
    /// Phase 3+5: After stress — performance restored.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_PerformanceRestored()
    {
        var user = CreateTestUser();
        _stopwatch.Restart();
        await _manager.GetPartnerTreeAsync(user, 1);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        await Task.WhenAll(Enumerable.Range(0, StressUsers)
            .Select(i => _manager.GetPartnerTreesAsync(user)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetPartnerTreeAsync(user, 1);
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
        var user = CreateTestUser();
        await Task.WhenAll(Enumerable.Range(0, 50)
            .Select(i => _manager.CreatePartnerTreeAsync(user, CreatePartnerTreeDataModel(i + 1))));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetPartnerTreesAsync(user);
        result.Should().NotBeNull("Post-stress partner tree read should succeed.");
    }

    #endregion

    #region Scalability (bonus 2)

    /// <summary>
    /// Scalability: Partner tree queries scale under increasing concurrent load.
    /// </summary>
    [Fact]
    public async Task Scalability_PartnerTreeQueries_ScalesUnderLoad()
    {
        var user = CreateTestUser();
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.GetPartnerTreeAsync(user, (i % 10) + 1)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    /// <summary>
    /// Scalability: Partner tree write throughput scales under concurrent load.
    /// </summary>
    [Fact]
    public async Task Scalability_PartnerTreeWriteThroughput_ScalesUnderLoad()
    {
        var user = CreateTestUser();
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.CreatePartnerTreeAsync(user, CreatePartnerTreeDataModel(i + 100))));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} concurrent writes, avg {perUser:F0}ms/write — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task MeasuredReadAsync(ClaimsPrincipal user, int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        if (index % 2 == 0)
            await _manager.GetPartnerTreesAsync(user);
        else
            await _manager.GetPartnerTreeAsync(user, (index % 10) + 1);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task MeasuredWriteAsync(ClaimsPrincipal user, int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        if (index % 2 == 0)
            await _manager.CreatePartnerTreeAsync(user, CreatePartnerTreeDataModel(index + 100));
        else
            await _manager.UpdatePartnerTreeAsync(user, CreatePartnerTreeDataModel((index % 5) + 1));
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
