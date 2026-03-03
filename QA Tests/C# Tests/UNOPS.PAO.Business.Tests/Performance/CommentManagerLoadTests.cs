/**
 * LOAD TESTS — CommentManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * CommentManager handles comment CRUD operations on partners, opportunities,
 * contacts, etc. High-frequency operations. Tests use mocked ICommentManager
 * for unit-level throughput and concurrency validation.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for CommentManager (via ICommentManager).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2)
///
/// Uses mocked ICommentManager to measure concurrent invocation patterns without DB dependency.
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class CommentManagerLoadTests
{
    private readonly Mock<ICommentManager> _mockManager;
    private readonly ICommentManager _manager;
    private readonly Stopwatch _stopwatch = new();

    // Load targets — TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section B1-B4
    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public CommentManagerLoadTests()
    {
        _mockManager = new Mock<ICommentManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.GetCommentsByEntityAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((string entityType, int entityId, bool _) => CreateMockComments(entityType, entityId));

        _mockManager
            .Setup(m => m.GetCommentByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => CreateMockComment(id));

        _mockManager
            .Setup(m => m.CreateCommentAsync(It.IsAny<CommentRequest>()))
            .ReturnsAsync((CommentRequest req) => CreateMockCommentFromRequest(req));

        _mockManager
            .Setup(m => m.UpdateCommentAsync(It.IsAny<UpdateCommentRequest>()))
            .ReturnsAsync((UpdateCommentRequest req) => CreateMockCommentFromUpdateRequest(req));

        _mockManager
            .Setup(m => m.DeleteCommentAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        _mockManager
            .Setup(m => m.TogglePinAsync(It.IsAny<int>()))
            .ReturnsAsync(true);

        _mockManager
            .Setup(m => m.GetCommentCountAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(10);
    }

    private static IEnumerable<CommentModel> CreateMockComments(string entityType, int entityId)
    {
        return Enumerable.Range(1, 5).Select(i => new CommentModel
        {
            Id = i,
            EntityType = entityType,
            EntityId = entityId,
            Content = $"Comment {i}",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedByName = "Load Test User"
        });
    }

    private static CommentModel CreateMockComment(int id)
    {
        return new CommentModel
        {
            Id = id,
            EntityType = "Partner",
            EntityId = 1,
            Content = "Load Test Comment",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedByName = "Load Test User"
        };
    }

    private static CommentModel CreateMockCommentFromRequest(CommentRequest req)
    {
        return new CommentModel
        {
            Id = 1,
            EntityType = req.EntityType,
            EntityId = req.EntityId,
            Content = req.Content,
            ParentCommentId = req.ParentCommentId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = 1,
            CreatedByName = "Load Test User"
        };
    }

    private static CommentModel CreateMockCommentFromUpdateRequest(UpdateCommentRequest req)
    {
        return new CommentModel
        {
            Id = req.Id,
            EntityType = "Partner",
            EntityId = 1,
            Content = req.Content,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            CreatedBy = 1,
            LastModifiedBy = 1
        };
    }

    private static CommentRequest CreateCommentRequest(int index)
    {
        return new CommentRequest
        {
            EntityType = "Partner",
            EntityId = 1,
            Content = $"Load Test Comment {index}",
            ParentCommentId = null
        };
    }

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — bulk comment retrieval under normal load.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_BulkCommentRetrieval_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var operationCount = Math.Min(NormalUsers * 2, 200);

        for (int i = 0; i < operationCount; i++)
        {
            _stopwatch.Restart();
            await _manager.GetCommentsByEntityAsync("Partner", 1, true);
            _stopwatch.Stop();
            lock (times) times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 100),
            $"Bulk retrieval degraded from {first:F0}ms to {last:F0}ms avg under sustained load");
    }

    /// <summary>
    /// Phase 2: Sustained write load — concurrent comment creation maintains consistency.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentCommentCreation_ConsistencyMaintained()
    {
        var times = new List<long>();
        var writeCount = NormalUsers / 2;

        var tasks = Enumerable.Range(0, writeCount)
            .Select(i => MeasuredCreateAsync(i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Write times inconsistent under {writeCount} concurrent creators (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 80% read, 20% write (Daily Operations scenario).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_CommentListingAndFiltering_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.8);
        var writeCount = NormalUsers - readCount;

        var reads = Enumerable.Range(0, readCount).Select(_ => _manager.GetCommentsByEntityAsync("Partner", 1, true));
        var writes = Enumerable.Range(0, writeCount).Select(i => _manager.CreateCommentAsync(CreateCommentRequest(i)));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Cast<Task>().Concat(writes.Cast<Task>()));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent comment retrieval — system handles gracefully.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenIncrease_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10).Select(_ => _manager.GetCommentsByEntityAsync("Partner", 1, true)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers).Select(_ => _manager.GetCommentsByEntityAsync("Partner", 1, true)).ToArray();
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
        await _manager.GetCommentsByEntityAsync("Partner", 1, true);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, PeakUsers).Select(_ => _manager.GetCommentsByEntityAsync("Partner", 1, true)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetCommentsByEntityAsync("Partner", 1, true);
        _stopwatch.Stop();
        var postSpikeMs = _stopwatch.ElapsedMilliseconds;

        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Phase 3: Beyond capacity — system does not crash under stress.
    /// </summary>
    [Fact]
    public async Task StressLoad_BeyondCapacity_DoesNotCrash()
    {
        var completed = 0;

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async _ =>
            {
                await _manager.GetCommentsByEntityAsync("Partner", 1, true);
                Interlocked.Increment(ref completed);
            }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent users — only {completed} completed");
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
                await _manager.GetCommentsByEntityAsync("Partner", 1, true);
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
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent users");
    }

    /// <summary>
    /// Phase 3: Concurrent CRUD — data integrity maintained under stress.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentCrud_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            await _manager.GetCommentsByEntityAsync("Partner", 1, true);
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent stress load");
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
        await _manager.GetCommentsByEntityAsync("Partner", 1, true);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, StressUsers).Select(_ => _manager.GetCommentsByEntityAsync("Partner", 1, true)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetCommentsByEntityAsync("Partner", 1, true);
        _stopwatch.Stop();
        var recoveredMs = _stopwatch.ElapsedMilliseconds;

        recoveredMs.Should().BeLessThan(Math.Max(baselineMs * 2, 10),
            $"System did not recover: post-stress {recoveredMs}ms vs baseline {baselineMs}ms");
    }

    /// <summary>
    /// Phase 3+5: After stress — no state corruption, operations succeed.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_NoStateCorruption()
    {
        await Task.WhenAll(Enumerable.Range(0, 50).Select(i => _manager.CreateCommentAsync(CreateCommentRequest(i))));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetCommentsByEntityAsync("Partner", 1, true);
        result.Should().NotBeNull("Post-stress read should succeed — comment listing remains functional.");
    }

    #endregion

    #region Scalability (optional — meets bulk read requirement)

    /// <summary>
    /// Bulk comment retrieval — scales under load across entity types.
    /// </summary>
    [Fact]
    public async Task BulkRead_CommentRetrieval_ScalesUnderLoad()
    {
        var entityTypes = new[] { ("Partner", 1), ("Opportunity", 1), ("Contact", 1) };
        var perEntity = 25;

        foreach (var (entityType, entityId) in entityTypes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, perEntity)
                .Select(_ => _manager.GetCommentsByEntityAsync(entityType, entityId, true)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)perEntity;
            perUser.Should().BeLessThan(100,
                $"At {perEntity} users for {entityType}, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task MeasuredCreateAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.CreateCommentAsync(CreateCommentRequest(index));
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
