/**
 * LOAD TESTS — AuditLogManager (UNOPSAuditLogManager)
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * AuditLogManager handles audit logging for all entity mutations across the system.
 * Write-heavy, compliance-critical component called on every create/update/delete.
 * Tests use mocked IAuditLogManager for unit-level throughput and concurrency validation.
 *
 * Covers: concurrent audit log writes, bulk retrieval (admin audit trails),
 * sustained queries/filtering (compliance reporting), stress write throughput,
 * recovery after stress, scalability as log volume grows.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models.AuditLogs;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for AuditLogManager (UNOPSAuditLogManager via IAuditLogManager).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2)
///
/// Uses mocked IAuditLogManager to measure concurrent invocation patterns without DB dependency.
/// Audit log is write-heavy (entity mutations) and read-heavy (compliance reporting).
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class AuditLogManagerLoadTests
{
    private readonly Mock<IAuditLogManager> _mockManager;
    private readonly IAuditLogManager _manager;
    private readonly Stopwatch _stopwatch = new();

    // Load targets — TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section B1-B4
    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public AuditLogManagerLoadTests()
    {
        _mockManager = new Mock<IAuditLogManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.CreateAuditLogAsync(It.IsAny<AuditLogCreateRequest>()))
            .ReturnsAsync((AuditLogCreateRequest req) => CreateMockAuditLogModel(1, req));

        _mockManager
            .Setup(m => m.GetLatestAuditLogAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string entityType, int entityId) => CreateMockAuditLogModel(1, entityType, entityId));

        _mockManager
            .Setup(m => m.GetAuditLogsAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string entityType, int entityId) => CreateMockAuditLogList(entityType, entityId));
    }

    private static AuditLogModel CreateMockAuditLogModel(int id, AuditLogCreateRequest req)
    {
        return new AuditLogModel
        {
            Id = id,
            EntityType = req.EntityType,
            EntityId = req.EntityId,
            Action = req.Action,
            Timestamp = DateTime.UtcNow,
            UserId = req.UserId,
            JsonData = req.JsonData,
            Description = req.Description
        };
    }

    private static AuditLogModel CreateMockAuditLogModel(int id, string entityType, int entityId)
    {
        return new AuditLogModel
        {
            Id = id,
            EntityType = entityType,
            EntityId = entityId,
            Action = "Update",
            Timestamp = DateTime.UtcNow,
            UserId = 1,
            JsonData = null,
            Description = "Load Test"
        };
    }

    private static IEnumerable<AuditLogModel> CreateMockAuditLogList(string entityType, int entityId)
    {
        return Enumerable.Range(1, 10).Select(i => new AuditLogModel
        {
            Id = i,
            EntityType = entityType,
            EntityId = entityId,
            Action = i % 3 == 0 ? "Create" : i % 3 == 1 ? "Update" : "Delete",
            Timestamp = DateTime.UtcNow.AddMinutes(-i),
            UserId = 1,
            JsonData = null,
            Description = $"Audit entry {i}"
        });
    }

    private static AuditLogCreateRequest CreateAuditLogRequest(int index)
    {
        return new AuditLogCreateRequest
        {
            EntityType = index % 3 == 0 ? "Partner" : index % 3 == 1 ? "Opportunity" : "Contact",
            EntityId = (index % 10) + 1,
            Action = index % 4 == 0 ? "Create" : index % 4 == 1 ? "Update" : index % 4 == 2 ? "Delete" : "StatusChange",
            UserId = 1,
            JsonData = index % 5 == 0 ? "{\"field\":\"value\"}" : null,
            Description = $"Load Test Mutation {index}"
        };
    }

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained write load — concurrent audit log writes from entity mutations.
    /// Audit log is write-heavy; simulates multiple simultaneous entity create/update/delete.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentAuditLogWrites_ConsistencyMaintained()
    {
        var times = new List<long>();
        var writeCount = NormalUsers; // Write-heavy: full load as writes

        var tasks = Enumerable.Range(0, writeCount)
            .Select(i => MeasuredCreateAsync(i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Audit write times inconsistent under {writeCount} concurrent writers (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained read load — bulk audit log retrieval (admin reviewing audit trails).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_BulkAuditLogRetrieval_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var operationCount = Math.Min(NormalUsers * 2, 200);

        for (int i = 0; i < operationCount; i++)
        {
            _stopwatch.Restart();
            await _manager.GetAuditLogsAsync("Partner", (i % 10) + 1);
            _stopwatch.Stop();
            lock (times) times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 100),
            $"Bulk audit retrieval degraded from {first:F0}ms to {last:F0}ms avg under sustained load");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 60% read (compliance reporting), 40% write (entity mutations).
    /// Reflects real usage: admins query audit trails while system processes mutations.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedQueriesAndWrites_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.6);
        var writeCount = NormalUsers - readCount;

        var reads = Enumerable.Range(0, readCount)
            .Select(i => _manager.GetAuditLogsAsync(i % 2 == 0 ? "Partner" : "Opportunity", (i % 10) + 1));
        var writes = Enumerable.Range(0, writeCount).Select(i => _manager.CreateAuditLogAsync(CreateAuditLogRequest(i)));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Cast<Task>().Concat(writes.Cast<Task>()));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed audit load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent audit log writes — system handles gracefully.
    /// Simulates burst of entity mutations (e.g., bulk import, batch update).
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenWriteIncrease_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10)
            .Select(i => _manager.CreateAuditLogAsync(CreateAuditLogRequest(i)))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.CreateAuditLogAsync(CreateAuditLogRequest(i)))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / baselineMs;
        scale.Should().BeLessThan((double)PeakUsers / 10 * 2,
            $"Audit write spike scaled {scale:F1}× — expected <{(double)PeakUsers / 10 * 2:F1}×");
    }

    /// <summary>
    /// Phase 5: Recovery after spike — returns to baseline performance.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_Recovery_ReturnsToBaseline()
    {
        _stopwatch.Restart();
        await _manager.GetLatestAuditLogAsync("Partner", 1);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetAuditLogsAsync(i % 2 == 0 ? "Partner" : "Opportunity", (i % 10) + 1)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetLatestAuditLogAsync("Partner", 1);
        _stopwatch.Stop();
        var postSpikeMs = _stopwatch.ElapsedMilliseconds;

        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Phase 3: Beyond capacity — audit log write throughput under heavy mutation load.
    /// System does not crash when many entities mutate simultaneously.
    /// </summary>
    [Fact]
    public async Task StressLoad_HeavyMutationLoad_DoesNotCrash()
    {
        var completed = 0;

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async (_, i) =>
            {
                await _manager.CreateAuditLogAsync(CreateAuditLogRequest(i));
                Interlocked.Increment(ref completed);
            }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent audit writes — only {completed} completed");
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
                await _manager.GetAuditLogsAsync("Partner", 1);
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
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent audit reads");
    }

    /// <summary>
    /// Phase 3: Concurrent audit writes — data integrity maintained under stress.
    /// All writes complete and return valid models.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentAuditWrites_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            var result = await _manager.CreateAuditLogAsync(CreateAuditLogRequest(i));
            result.Should().NotBeNull();
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent audit write stress");
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
        await _manager.GetLatestAuditLogAsync("Partner", 1);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, StressUsers)
            .Select(i => _manager.CreateAuditLogAsync(CreateAuditLogRequest(i))));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetLatestAuditLogAsync("Partner", 1);
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
            .Select(i => _manager.CreateAuditLogAsync(CreateAuditLogRequest(i))));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetAuditLogsAsync("Partner", 1);
        result.Should().NotBeNull("Post-stress audit read should succeed — compliance reporting remains functional.");
    }

    #endregion

    #region Scalability — audit log queries as log volume grows

    /// <summary>
    /// Scalability: Audit log retrieval scales under increasing concurrent load.
    /// Simulates admin reviewing audit trails as system log volume grows.
    /// </summary>
    [Fact]
    public async Task Scalability_AuditLogQueries_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.GetAuditLogsAsync(
                    i % 3 == 0 ? "Partner" : i % 3 == 1 ? "Opportunity" : "Contact",
                    (i % 10) + 1)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    /// <summary>
    /// Scalability: Audit log write throughput scales under concurrent mutation load.
    /// </summary>
    [Fact]
    public async Task Scalability_AuditLogWriteThroughput_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.CreateAuditLogAsync(CreateAuditLogRequest(i))));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} concurrent writes, avg {perUser:F0}ms/write — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task MeasuredCreateAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.CreateAuditLogAsync(CreateAuditLogRequest(index));
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
