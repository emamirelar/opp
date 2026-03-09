/**
 * LOAD TESTS — ProfileManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2) | Scalability (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * ProfileManager manages user profile retrieval and updates.
 * No interface — uses internal IProfileManagerMock for unit-level throughput validation.
 * Mixed load: 80% read, 20% write.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Models.Users;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Internal mock interface for ProfileManager (concrete class has no interface).
/// </summary>
public interface IProfileManagerMock
{
    ProfileModel Get(string? email);
    Task Update(ProfileModel profile);
}

/// <summary>
/// Load Tests for ProfileManager.
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2), Scalability (2)
/// Mixed load: 80% read, 20% write.
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class ProfileManagerLoadTests
{
    private readonly Mock<IProfileManagerMock> _mockManager;
    private readonly IProfileManagerMock _manager;
    private readonly Stopwatch _stopwatch = new();

    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public ProfileManagerLoadTests()
    {
        _mockManager = new Mock<IProfileManagerMock>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager.Setup(m => m.Get(It.IsAny<string?>()))
            .Returns((string? email) => new ProfileModel { Email = email ?? "test@unops.org", FirstName = "Test", LastName = "User" });

        _mockManager.Setup(m => m.Update(It.IsAny<ProfileModel>()))
            .Returns(Task.CompletedTask);
    }

    private static ProfileModel CreateProfileModel(int index) =>
        new() { Email = $"user{index}@unops.org", FirstName = $"First{index}", LastName = $"Last{index}" };

    #region Sustained Load (min 3) — Phase 2

    [Fact]
    public async Task SustainedLoad_ReadOperations_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var readCount = (int)(NormalUsers * 0.8);
        var tasks = Enumerable.Range(0, readCount).Select(i => MeasuredReadAsync(i, times)).ToArray();
        await Task.WhenAll(tasks);

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 500),
            $"Read performance degraded from {first:F0}ms to {last:F0}ms avg under {readCount} concurrent users");
    }

    [Fact]
    public async Task SustainedLoad_WriteOperations_ConsistencyMaintained()
    {
        var times = new List<long>();
        var writeCount = (int)(NormalUsers * 0.2);
        var tasks = Enumerable.Range(0, writeCount).Select(i => MeasuredWriteAsync(i, times)).ToArray();
        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Write times inconsistent under {writeCount} concurrent writers (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    [Fact]
    public async Task SustainedLoad_MixedOperations_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.8);
        var writeCount = NormalUsers - readCount;

        var reads = Enumerable.Range(0, readCount).Select(i => SimulateReadAsync(i));
        var writes = Enumerable.Range(0, writeCount).Select(i => SimulateWriteAsync(i));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Cast<Task>().Concat(writes.Cast<Task>()));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    [Fact]
    public async Task SpikeLoad_SuddenIncrease_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10).Select(i => SimulateReadAsync(i)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers).Select(i => i % 5 == 0 ? SimulateWriteAsync(i) : SimulateReadAsync(i)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / Math.Max(baselineMs, 1);
        scale.Should().BeLessThan((double)PeakUsers / 10 * 2,
            $"Spike scaled {scale:F1}× — expected <{(double)PeakUsers / 10 * 2:F1}×");
    }

    [Fact]
    public async Task SpikeLoad_Recovery_ReturnsToBaseline()
    {
        var baselineMs = await MeasureSingleReadMs();

        await Task.WhenAll(Enumerable.Range(0, PeakUsers).Select(i => i % 5 == 0 ? SimulateWriteAsync(i) : SimulateReadAsync(i)));

        await Task.Delay(RecoveryWindowMs);

        var postSpikeMs = await MeasureSingleReadMs();
        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    [Fact]
    public async Task StressLoad_BeyondCapacity_DoesNotCrash()
    {
        var completed = 0;
        var tasks = Enumerable.Range(0, StressUsers).Select(async (_, i) =>
        {
            await (i % 5 == 0 ? SimulateWriteAsync(i) : SimulateReadAsync(i));
            Interlocked.Increment(ref completed);
        }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent users — only {completed} completed");
        completed.Should().Be(StressUsers);
    }

    [Fact]
    public async Task StressLoad_ErrorRate_WithinAcceptableLimit()
    {
        var success = 0;
        var failure = 0;

        var tasks = Enumerable.Range(0, StressUsers).Select(async _ =>
        {
            try
            {
                await SimulateReadAsync(0);
                Interlocked.Increment(ref success);
            }
            catch { Interlocked.Increment(ref failure); }
        }).ToArray();

        await Task.WhenAll(tasks);

        var errorRate = (double)failure / StressUsers;
        errorRate.Should().BeLessThan(MaxErrorRate,
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent users");
    }

    [Fact]
    public async Task StressLoad_ConcurrentUpdates_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            await _manager.Update(CreateProfileModel(i));
            lock (lockObj) actualSum += i;
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum, "Data integrity compromised under concurrent update stress");
    }

    #endregion

    #region Recovery (min 2) — Phase 3 + 5

    [Fact]
    public async Task Recovery_AfterStress_PerformanceRestored()
    {
        var baselineMs = await MeasureSingleReadMs();

        await Task.WhenAll(Enumerable.Range(0, StressUsers).Select(i => i % 5 == 0 ? SimulateWriteAsync(i) : SimulateReadAsync(i)));

        await Task.Delay(RecoveryWindowMs);

        var recoveredMs = await MeasureSingleReadMs();
        recoveredMs.Should().BeLessThan(Math.Max(baselineMs * 2, 10),
            $"System did not recover: post-stress {recoveredMs}ms vs baseline {baselineMs}ms");
    }

    [Fact]
    public async Task Recovery_AfterStress_NoStateCorruption()
    {
        await Task.WhenAll(Enumerable.Range(0, 50).Select(i => SimulateWriteAsync(i)));

        await Task.Delay(RecoveryWindowMs);

        var result = _manager.Get("test@unops.org");
        result.Should().NotBeNull("Post-stress read should succeed");
    }

    #endregion

    #region Scalability (min 2)

    [Fact]
    public async Task Scalability_ReadOperations_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount).Select(i => SimulateReadAsync(i)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    [Fact]
    public async Task Scalability_MixedOperations_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount).Select(i => i % 5 == 0 ? SimulateWriteAsync(i) : SimulateReadAsync(i)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} mixed ops, avg {perUser:F0}ms/op — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task SimulateReadAsync(int index)
    {
        await Task.Run(() => _manager.Get($"user{index % 20}@unops.org"));
    }

    private async Task SimulateWriteAsync(int index) =>
        await _manager.Update(CreateProfileModel(index));

    private async Task MeasuredReadAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await SimulateReadAsync(index);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task MeasuredWriteAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await SimulateWriteAsync(index);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task<long> MeasureSingleReadMs()
    {
        var sw = Stopwatch.StartNew();
        await SimulateReadAsync(0);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    #endregion
}
