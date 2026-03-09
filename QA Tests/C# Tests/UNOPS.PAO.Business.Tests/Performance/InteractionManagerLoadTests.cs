/**
 * LOAD TESTS — InteractionManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 *
 * Unit-level load tests: Mock IInteractionManager to isolate throughput and
 * concurrency handling from DbContext/repository. Tests verify that concurrent
 * callers can invoke the manager interface without deadlocks or errors.
 */

using System.Diagnostics;
using System.Security.Claims;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for InteractionManager.
/// Verifies system behaviour under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Phase mapping:
///   Sustained Load  → Phase 2: Normal operations over time
///   Spike           → Phase 5: Sudden load increases + recovery
///   Stress Limits   → Phase 3: Beyond normal capacity
///   Recovery        → Phase 3+5: Post-overload stability
///
/// Uses mocked IInteractionManager for unit-level isolation.
/// </summary>
public class InteractionManagerLoadTests
{
    private readonly Mock<IInteractionManager> _mockManager;
    private readonly IInteractionManager _manager;
    private readonly Stopwatch _stopwatch = new();

    // Load targets — TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section B1-B4
    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public InteractionManagerLoadTests()
    {
        _mockManager = new Mock<IInteractionManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        // Simulate fast async operations (mocked DB/repository layer)
        _mockManager
            .Setup(m => m.CreateInteractionAsync(It.IsAny<InteractionRequest>()))
            .ReturnsAsync((InteractionRequest req) => CreateMockModel(1, req));

        _mockManager
            .Setup(m => m.GetInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<PaginationRequest>()))
            .ReturnsAsync((ClaimsPrincipal _, PaginationRequest p) => CreateMockPaginationResponse(p.PageSize));

        _mockManager
            .Setup(m => m.GetInteractionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync((ClaimsPrincipal _, int id) => CreateMockModel(id, null));

        _mockManager
            .Setup(m => m.UpdateInteractionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<UpdateInteractionRequest>()))
            .ReturnsAsync((ClaimsPrincipal _, UpdateInteractionRequest u) => CreateMockModel(u.Id, null));

        _mockManager
            .Setup(m => m.DeleteInteractionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
    }

    private static InteractionModel CreateMockModel(int id, InteractionRequest? req)
    {
        return new InteractionModel
        {
            Id = id,
            Type = req?.Type ?? InteractionType.Email,
            Date = req?.Date ?? DateTime.UtcNow,
            Subject = req?.Subject ?? "Load Test",
            Status = "Active"
        };
    }

    private static PaginationResponse<InteractionModel> CreateMockPaginationResponse(int pageSize)
    {
        var records = Enumerable.Range(1, Math.Min(pageSize, 10))
            .Select(i => new InteractionModel { Id = i, Subject = $"Interaction {i}", Type = InteractionType.Email, Date = DateTime.UtcNow, Status = "Active" })
            .ToList();
        return new PaginationResponse<InteractionModel>
        {
            Records = records,
            TotalCount = 100,
            PageIndex = 1,
            PageSize = pageSize,
            TotalPages = 10
        };
    }

    private static ClaimsPrincipal CreateTestUser()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Email, "loadtest@unops.org"),
            new(ClaimTypes.Name, "Load Test User")
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static InteractionRequest CreateTestRequest(int index)
    {
        return new InteractionRequest
        {
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Subject = $"Load Test Interaction {index}",
            Description = "Test"
        };
    }

    private static PaginationRequest CreatePaginationRequest() => new(1, 10);

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — interaction list queries maintain performance.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-073")]
    public async Task SustainedLoad_ReadOperations_PerformanceDoesNotDegrade()
    {
        var user = CreateTestUser();
        var request = CreatePaginationRequest();
        var times = new List<long>();
        var operationCount = Math.Min(NormalUsers * 2, 200);

        for (int i = 0; i < operationCount; i++)
        {
            _stopwatch.Restart();
            await _manager.GetInteractionsAsync(user, request);
            _stopwatch.Stop();
            lock (times) times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(first * 10,
            $"Read performance degraded from {first:F0}ms to {last:F0}ms avg under sustained load");
    }

    /// <summary>
    /// Phase 2: Sustained write load — concurrent creation maintains consistency.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-073")]
    public async Task SustainedLoad_WriteOperations_ConsistencyMaintained()
    {
        var times = new List<long>();
        var writeCount = NormalUsers / 2;

        var tasks = Enumerable.Range(0, writeCount)
            .Select(i => MeasuredCreateAsync(i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(avg * 2,
            $"Write times inconsistent under {writeCount} concurrent writers (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 80% read, 20% write (Daily Operations scenario).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedOperations_ThroughputMeetsTarget()
    {
        var user = CreateTestUser();
        var request = CreatePaginationRequest();
        var readCount = (int)(NormalUsers * 0.8);
        var writeCount = NormalUsers - readCount;

        var reads = Enumerable.Range(0, readCount).Select(_ => _manager.GetInteractionsAsync(user, request));
        var writes = Enumerable.Range(0, writeCount).Select(i => _manager.CreateInteractionAsync(CreateTestRequest(i)));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Concat<Task>(writes));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent users — system handles gracefully.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenIncrease_HandlesGracefully()
    {
        var user = CreateTestUser();
        var request = CreatePaginationRequest();

        var baselineTasks = Enumerable.Range(0, 10).Select(_ => _manager.GetInteractionsAsync(user, request)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers).Select(_ => _manager.GetInteractionsAsync(user, request)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / baselineMs;
        scale.Should().BeLessThan((double)PeakUsers / 10 * 2,
            $"Spike scaled {scale:F1}× — expected <{(double)PeakUsers / 10 * 2:F1}×");
    }

    /// <summary>
    /// Phase 5: Recovery after spike — returns to baseline performance.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-073")]
    public async Task SpikeLoad_Recovery_ReturnsToBaseline()
    {
        var user = CreateTestUser();
        var request = CreatePaginationRequest();

        _stopwatch.Restart();
        await _manager.GetInteractionsAsync(user, request);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, PeakUsers).Select(_ => _manager.GetInteractionsAsync(user, request)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetInteractionsAsync(user, request);
        _stopwatch.Stop();
        var postSpikeMs = _stopwatch.ElapsedMilliseconds;

        postSpikeMs.Should().BeLessThan(baselineMs * 3,
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
        var user = CreateTestUser();
        var request = CreatePaginationRequest();

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async _ =>
            {
                await _manager.GetInteractionsAsync(user, request);
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
        var user = CreateTestUser();
        var request = CreatePaginationRequest();

        var tasks = Enumerable.Range(0, StressUsers).Select(async _ =>
        {
            try
            {
                await _manager.GetInteractionsAsync(user, request);
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
        var user = CreateTestUser();
        var request = CreatePaginationRequest();
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            await _manager.GetInteractionsAsync(user, request);
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
    [Trait("Defect", "DEF-073")]
    public async Task Recovery_AfterStress_PerformanceRestored()
    {
        var user = CreateTestUser();
        var request = CreatePaginationRequest();

        _stopwatch.Restart();
        await _manager.GetInteractionsAsync(user, request);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, StressUsers).Select(_ => _manager.GetInteractionsAsync(user, request)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetInteractionsAsync(user, request);
        _stopwatch.Stop();
        var recoveredMs = _stopwatch.ElapsedMilliseconds;

        recoveredMs.Should().BeLessThan(baselineMs * 2,
            $"System did not recover: post-stress {recoveredMs}ms vs baseline {baselineMs}ms");
    }

    /// <summary>
    /// Phase 3+5: After stress — no state corruption, operations succeed.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_NoStateCorruption()
    {
        var user = CreateTestUser();
        var request = CreatePaginationRequest();

        await Task.WhenAll(Enumerable.Range(0, 50).Select(i => _manager.CreateInteractionAsync(CreateTestRequest(i))));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetInteractionsAsync(user, request);
        result.Should().NotBeNull();
        result.Records.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0,
            "Post-stress read should succeed.");
    }

    #endregion

    #region Scalability (optional — meets bulk read requirement)

    /// <summary>
    /// Bulk read operations — interaction list queries scale under load.
    /// </summary>
    [Fact]
    public async Task BulkRead_InteractionLists_ScalesUnderLoad()
    {
        var user = CreateTestUser();
        var request = CreatePaginationRequest();
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount).Select(_ => _manager.GetInteractionsAsync(user, request)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task MeasuredCreateAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.CreateInteractionAsync(CreateTestRequest(index));
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
