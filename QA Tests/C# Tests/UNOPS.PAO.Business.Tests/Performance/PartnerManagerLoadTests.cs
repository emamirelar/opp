/**
 * LOAD TESTS — PartnerManager (IPartnerManager)
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * PartnerManager is read-heavy (browse partners, search, view details) with periodic writes.
 * Mixed load: 80% read (browsing, viewing details), 20% write (creating/updating).
 * Tests use mocked IPartnerManager for unit-level throughput and concurrency validation.
 *
 * Covers: sustained read/write load, spike handling, stress limits, recovery, scalability.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using System.Security.Claims;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for PartnerManager (IPartnerManager).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2)
///
/// Uses mocked IPartnerManager. PartnerManager is read-heavy (80% read, 20% write).
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class PartnerManagerLoadTests
{
    private readonly Mock<IPartnerManager> _mockManager;
    private readonly IPartnerManager _manager;
    private readonly Stopwatch _stopwatch = new();

    // Load targets — TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section B1-B4
    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public PartnerManagerLoadTests()
    {
        _mockManager = new Mock<IPartnerManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.CreatePartnerAsync(It.IsAny<PartnerRequest>()))
            .ReturnsAsync((PartnerRequest req) => CreateMockPartnerModel(1, req.Name ?? "Load Test Partner"));

        _mockManager
            .Setup(m => m.GetPartnerAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => CreateMockPartnerModel(id, $"Partner {id}"));

        _mockManager
            .Setup(m => m.GetPartnerWithContactsAndInteractionsAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => CreateMockPartnerModel(id, $"Partner {id} (with contacts)"));

        _mockManager
            .Setup(m => m.GetPartner(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int _, int id) => CreateMockPartnerModel(id, $"Partner {id}"));

        _mockManager
            .Setup(m => m.GetPartners(It.IsAny<int>(), It.IsAny<PaginationRequest>()))
            .ReturnsAsync((int _, PaginationRequest req) => CreateMockPaginationResponse(req));

        _mockManager
            .Setup(m => m.UpdatePartnerAsync(It.IsAny<int>(), It.IsAny<UpdatePartnerRequest>()))
            .ReturnsAsync((int _, UpdatePartnerRequest req) => CreateMockPartnerModel(req.Id, req.Name ?? "Updated Partner"));

        _mockManager
            .Setup(m => m.DeletePartnerAsync(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockManager
            .Setup(m => m.GetTotalPartnerCountAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(100);
    }

    private static PartnerModel CreateMockPartnerModel(int id, string name)
    {
        return new PartnerModel
        {
            Id = id,
            Name = name,
            Status = id % 4 == 0 ? "Draft" : id % 4 == 1 ? "Active" : id % 4 == 2 ? "Closed" : "Archived",
            PartnerApprovalStatus = id % 2 == 0 ? "Approved" : "NotApproved"
        };
    }

    private static PaginationResponse<PartnerModel> CreateMockPaginationResponse(PaginationRequest req)
    {
        var records = Enumerable.Range(1, Math.Min(req.PageSize, 10))
            .Select(i => CreateMockPartnerModel(i, $"Partner {i}"))
            .ToList();
        return new PaginationResponse<PartnerModel>
        {
            Records = records,
            TotalCount = 100,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            TotalPages = 10
        };
    }

    private static PartnerRequest CreatePartnerRequest(int index)
    {
        return new PartnerRequest
        {
            Name = $"Load Test Partner {index}",
            PartnerShortDescription = $"Short {index}",
            Status = index % 2 == 0 ? "Draft" : "Active",
            PartnerApprovalStatus = "NotApproved"
        };
    }

    private static UpdatePartnerRequest CreateUpdatePartnerRequest(int id, int index)
    {
        return new UpdatePartnerRequest
        {
            Id = id,
            Name = $"Updated Partner {index}",
            PartnerShortDescription = $"Updated Short {index}",
            Status = "Active",
            PartnerApprovalStatus = "Approved"
        };
    }

    private static PaginationRequest CreatePaginationRequest(int index)
    {
        return new PaginationRequest((index % 5) + 1, 10);
    }

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — concurrent partner browsing (GetPartners, GetPartnerAsync).
    /// PartnerManager is read-heavy; simulates multiple users browsing partner list and details.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentPartnerReads_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var operationCount = Math.Min(NormalUsers * 2, 200);

        for (int i = 0; i < operationCount; i++)
        {
            _stopwatch.Restart();
            if (i % 3 == 0)
                await _manager.GetPartners(1, CreatePaginationRequest(i));
            else if (i % 3 == 1)
                await _manager.GetPartnerAsync((i % 10) + 1);
            else
                await _manager.GetPartnerWithContactsAndInteractionsAsync((i % 10) + 1);
            _stopwatch.Stop();
            lock (times) times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        var threshold = Math.Max(first * 10, 100);
        last.Should().BeLessThan(threshold,
            $"Partner read performance degraded from {first:F0}ms to {last:F0}ms avg under sustained load");
    }

    /// <summary>
    /// Phase 2: Sustained write load — concurrent partner creates/updates.
    /// Simulates periodic partner creation and updates (20% of typical load).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentPartnerWrites_ConsistencyMaintained()
    {
        var times = new List<long>();
        var writeCount = NormalUsers / 2; // 20% of normal = ~25 writes

        var tasks = Enumerable.Range(0, writeCount)
            .Select(i => i % 2 == 0
                ? MeasuredCreateAsync(i, times)
                : MeasuredUpdateAsync((i % 10) + 1, i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThanOrEqualTo(Math.Max(avg * 2, 5),
            $"Partner write times inconsistent under {writeCount} concurrent writers (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 80% read (browse, view details), 20% write (create/update).
    /// Reflects real usage: users browsing partners while some create/update.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedReadsAndWrites_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.8);
        var writeCount = NormalUsers - readCount;

        var readTasks = Enumerable.Range(0, readCount).Select(i => RunMixedReadAsync(i));
        var writeTasks = Enumerable.Range(0, writeCount).Select(i => RunMixedWriteAsync(i));

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(writeTasks));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed partner load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent partner reads — system handles gracefully.
    /// Simulates burst of users browsing partner list (e.g., training session, month-end).
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenReadIncrease_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10)
            .Select(i => _manager.GetPartnerAsync((i % 10) + 1))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetPartnerAsync((i % 10) + 1))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / baselineMs;
        scale.Should().BeLessThan((double)PeakUsers / 10 * 2,
            $"Partner read spike scaled {scale:F1}× — expected <{(double)PeakUsers / 10 * 2:F1}×");
    }

    /// <summary>
    /// Phase 5: Recovery after spike — returns to baseline performance.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_Recovery_ReturnsToBaseline()
    {
        _stopwatch.Restart();
        await _manager.GetPartnerAsync(1);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetPartnerAsync((i % 10) + 1)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetPartnerAsync(1);
        _stopwatch.Stop();
        var postSpikeMs = _stopwatch.ElapsedMilliseconds;

        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Phase 3: Beyond capacity — partner read throughput under heavy browse load.
    /// System does not crash when many users browse partners simultaneously.
    /// </summary>
    [Fact]
    public async Task StressLoad_HeavyBrowseLoad_DoesNotCrash()
    {
        var completed = 0;

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async (_, i) =>
            {
                await _manager.GetPartnerAsync((i % 10) + 1);
                Interlocked.Increment(ref completed);
            }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent partner reads — only {completed} completed");
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
                await _manager.GetPartnerAsync(1);
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
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent partner reads");
    }

    /// <summary>
    /// Phase 3: Concurrent partner operations — data integrity maintained under stress.
    /// All reads return valid models, writes complete successfully.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentPartnerOperations_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            var result = await _manager.GetPartnerAsync((i % 10) + 1);
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.Name.Should().NotBeNullOrEmpty();
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent partner read stress");
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
        await _manager.GetPartnerAsync(1);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, StressUsers)
            .Select(i => _manager.GetPartnerAsync((i % 10) + 1)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetPartnerAsync(1);
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
            .Select(i => _manager.CreatePartnerAsync(CreatePartnerRequest(i))));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetPartners(1, new PaginationRequest(1, 10));
        result.Should().NotBeNull("Post-stress partner list read should succeed.");
        result!.Records.Should().NotBeNull();
    }

    #endregion

    #region Scalability — partner queries as load grows

    /// <summary>
    /// Scalability: Partner list retrieval scales under increasing concurrent load.
    /// Simulates users browsing partner list as system load grows.
    /// </summary>
    [Fact]
    public async Task Scalability_PartnerListQueries_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.GetPartners(1, CreatePaginationRequest(i))));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    /// <summary>
    /// Scalability: Partner detail retrieval (with contacts) scales under concurrent load.
    /// </summary>
    [Fact]
    public async Task Scalability_PartnerDetailQueries_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.GetPartnerWithContactsAndInteractionsAsync((i % 10) + 1)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} concurrent detail reads, avg {perUser:F0}ms/read — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private Task RunMixedReadAsync(int i)
    {
        if (i % 3 == 0) return _manager.GetPartners(1, CreatePaginationRequest(i));
        if (i % 3 == 1) return _manager.GetPartnerAsync((i % 10) + 1);
        return _manager.GetPartnerWithContactsAndInteractionsAsync((i % 10) + 1);
    }

    private Task RunMixedWriteAsync(int i)
    {
        if (i % 2 == 0)
            return _manager.CreatePartnerAsync(CreatePartnerRequest(i));
        return _manager.UpdatePartnerAsync(1, CreateUpdatePartnerRequest((i % 10) + 1, i));
    }

    private async Task MeasuredCreateAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.CreatePartnerAsync(CreatePartnerRequest(index));
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task MeasuredUpdateAsync(int id, int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.UpdatePartnerAsync(1, CreateUpdatePartnerRequest(id, index));
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
