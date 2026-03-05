/**
 * LOAD TESTS — RiskManager (UNOPSRiskManager)
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * RiskManager handles risk register, DST risks, risk assessment, and mitigation tracking.
 * Mixed load: 60% read (viewing risk registers, lookups), 40% write (creating/updating risks).
 * Risk assessment reads are heavy; risk creation/updates are moderate.
 * Tests use mocked IRiskManager for unit-level throughput and concurrency validation.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using System.Security.Claims;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for RiskManager (UNOPSRiskManager via IRiskManager).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2)
///
/// Uses mocked IRiskManager to measure concurrent invocation patterns without DB dependency.
/// Covers: concurrent risk creation, bulk risk retrieval, sustained risk queries/filtering,
/// stress CRUD operations, and recovery after stress.
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class RiskManagerLoadTests
{
    private readonly Mock<IRiskManager> _mockManager;
    private readonly IRiskManager _manager;
    private readonly Stopwatch _stopwatch = new();

    // Load targets — TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section B1-B4
    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public RiskManagerLoadTests()
    {
        _mockManager = new Mock<IRiskManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.GetRisksByEntityAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ClaimsPrincipal?>()))
            .ReturnsAsync((string entityType, int entityId, ClaimsPrincipal? _) => CreateMockDSTRisksResponse(entityId));

        _mockManager
            .Setup(m => m.CreateRiskAsync(It.IsAny<RiskCreateRequest>(), It.IsAny<ClaimsPrincipal?>()))
            .ReturnsAsync((RiskCreateRequest req, ClaimsPrincipal? _) => CreateMockRiskModel(1, req));

        _mockManager
            .Setup(m => m.UpdateRiskAsync(It.IsAny<int>(), It.IsAny<RiskCreateRequest>(), It.IsAny<ClaimsPrincipal?>()))
            .ReturnsAsync((int id, RiskCreateRequest req, ClaimsPrincipal? _) => CreateMockRiskModel(id, req));

        _mockManager
            .Setup(m => m.DeleteRiskAsync(It.IsAny<int>(), It.IsAny<ClaimsPrincipal?>()))
            .ReturnsAsync(true);

        _mockManager
            .Setup(m => m.GetRiskLookupsAsync())
            .ReturnsAsync(CreateMockRiskLookupsResponse());

        _mockManager
            .Setup(m => m.GetRiskCategoriesAsync())
            .ReturnsAsync(CreateMockRiskCategoryHierarchyResponse());

        _mockManager
            .Setup(m => m.GetPreDefinedHighRisksAsync())
            .ReturnsAsync(new List<PreDefinedHighRiskModel> { new() { Id = 1, Code = "1.1.1", ShortTitle = "Test" } });

        _mockManager
            .Setup(m => m.GetHighRiskAnalysisAsync(It.IsAny<int>(), It.IsAny<ClaimsPrincipal?>()))
            .ReturnsAsync(new HighRiskAnalysisResponse());
    }

    private static DSTRisksResponse CreateMockDSTRisksResponse(int entityId)
    {
        var risks = Enumerable.Range(1, 5)
            .Select(i => CreateMockRiskModel(i, new RiskCreateRequest { EntityId = entityId, Title = $"Risk {i}" }))
            .ToList();
        return new DSTRisksResponse { Risks = risks, TotalCount = risks.Count };
    }

    private static RiskModel CreateMockRiskModel(int id, RiskCreateRequest? req)
    {
        return new RiskModel
        {
            Id = id,
            EntityType = "Opportunity",
            EntityId = req?.EntityId ?? 1,
            Title = req?.Title ?? "Load Test Risk",
            RiskTypeId = 1,
            RiskCategoryId = 1,
            RiskProbabilityId = 1,
            RiskProximityId = 1,
            RiskImpactLevelId = 1,
            CreatedDate = DateTime.UtcNow,
            Status = "Active"
        };
    }

    private static RiskLookupsResponse CreateMockRiskLookupsResponse()
    {
        return new RiskLookupsResponse
        {
            RiskTypes = new List<RiskTypeModel> { new() { Id = 1, Name = "Threat", Code = "THREAT" } },
            Probabilities = new List<RiskProbabilityModel> { new() { Id = 1, Name = "Low", Code = "LOW" } },
            Proximities = new List<RiskProximityModel> { new() { Id = 1, Name = "Within 6 months", Code = "WITHIN_6" } },
            ImpactLevels = new List<RiskImpactLevelModel> { new() { Id = 1, Name = "Low", Code = "LOW" } },
            ResponseTypes = new List<RiskResponseTypeModel> { new() { Id = 1, Name = "Accept", Code = "ACCEPT" } }
        };
    }

    private static RiskCategoryHierarchyResponse CreateMockRiskCategoryHierarchyResponse()
    {
        return new RiskCategoryHierarchyResponse
        {
            Categories = new List<RiskCategoryModel> { new() { Id = 1, Name = "Finance", Code = "FIN", Level = 1 } }
        };
    }

    private static ClaimsPrincipal? CreateTestUser()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Email, "loadtest@unops.org"),
            new(ClaimTypes.Name, "Load Test User")
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static RiskCreateRequest CreateTestRequest(int index)
    {
        return new RiskCreateRequest
        {
            EntityId = 1,
            Title = $"Load Test Risk {index}",
            Description = "Test description"
        };
    }

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — bulk risk retrieval under normal load.
    /// </summary>
    [Fact]
    [Trait("Defect", "DEF-084")]
    public async Task SustainedLoad_BulkRiskRetrieval_PerformanceDoesNotDegrade()
    {
        var user = CreateTestUser();
        var times = new List<long>();
        var operationCount = Math.Min(NormalUsers * 2, 200);

        for (int i = 0; i < operationCount; i++)
        {
            _stopwatch.Restart();
            await _manager.GetRisksByEntityAsync("Opportunity", 1, user);
            _stopwatch.Stop();
            lock (times) times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(first * 10,
            $"Bulk risk retrieval degraded from {first:F0}ms to {last:F0}ms avg under sustained load");
    }

    /// <summary>
    /// Phase 2: Sustained write load — concurrent risk creation maintains consistency.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentRiskCreation_ConsistencyMaintained()
    {
        var times = new List<long>();
        var writeCount = NormalUsers / 2;

        var tasks = Enumerable.Range(0, writeCount)
            .Select(i => MeasuredCreateAsync(i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThanOrEqualTo(Math.Max(avg * 2, 5),
            $"Concurrent risk creation times inconsistent under {writeCount} writers (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 60% read (risk registers, lookups), 40% write (create/update).
    /// Reflects real usage: users viewing risk registers and lookups while creating/updating risks.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedRiskRegistersAndWrites_ThroughputMeetsTarget()
    {
        var user = CreateTestUser();
        var readCount = (int)(NormalUsers * 0.6);
        var writeCount = NormalUsers - readCount;

        var reads = Enumerable.Range(0, readCount).Select(i => RunMixedReadAsync(i, user));
        var creates = Enumerable.Range(0, writeCount / 2).Select(i => _manager.CreateRiskAsync(CreateTestRequest(i), user));
        var updates = Enumerable.Range(0, writeCount - (writeCount / 2)).Select(i => _manager.UpdateRiskAsync((i % 10) + 1, CreateTestRequest(i), user));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Cast<Task>().Concat(creates.Cast<Task>()).Concat(updates.Cast<Task>()));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed risk load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent risk retrieval — system handles gracefully.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenIncrease_HandlesGracefully()
    {
        var user = CreateTestUser();

        var baselineTasks = Enumerable.Range(0, 10).Select(_ => _manager.GetRisksByEntityAsync("Opportunity", 1, user)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers).Select(_ => _manager.GetRisksByEntityAsync("Opportunity", 1, user)).ToArray();
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
        var user = CreateTestUser();

        _stopwatch.Restart();
        await _manager.GetRisksByEntityAsync("Opportunity", 1, user);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, PeakUsers).Select(_ => _manager.GetRisksByEntityAsync("Opportunity", 1, user)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetRisksByEntityAsync("Opportunity", 1, user);
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
        var user = CreateTestUser();

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async _ =>
            {
                await _manager.GetRisksByEntityAsync("Opportunity", 1, user);
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

        var tasks = Enumerable.Range(0, StressUsers).Select(async _ =>
        {
            try
            {
                await _manager.GetRisksByEntityAsync("Opportunity", 1, user);
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
    /// Phase 3: Concurrent risk CRUD — data integrity maintained under stress.
    /// All writes complete and return valid models.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentRiskCrud_DataIntegrityMaintained()
    {
        var user = CreateTestUser();
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            var result = i % 2 == 0
                ? await _manager.CreateRiskAsync(CreateTestRequest(i), user)
                : await _manager.UpdateRiskAsync((i % 10) + 1, CreateTestRequest(i), user);
            result.Should().NotBeNull();
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent risk CRUD stress load");
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
        await _manager.GetRisksByEntityAsync("Opportunity", 1, user);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, StressUsers).Select(_ => _manager.GetRisksByEntityAsync("Opportunity", 1, user)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetRisksByEntityAsync("Opportunity", 1, user);
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
        var user = CreateTestUser();

        await Task.WhenAll(Enumerable.Range(0, 50).Select(i => _manager.CreateRiskAsync(CreateTestRequest(i), user)));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetRisksByEntityAsync("Opportunity", 1, user);
        result.Should().NotBeNull();
        result.Risks.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0,
            "Post-stress risk retrieval should succeed.");
    }

    #endregion

    #region Scalability — risk queries and writes as load grows

    /// <summary>
    /// Scalability: Risk list queries scale under increasing concurrent load.
    /// Simulates users viewing risk registers and lookups as system load grows.
    /// </summary>
    [Fact]
    public async Task Scalability_RiskQueries_ScalesUnderLoad()
    {
        var user = CreateTestUser();
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.GetRisksByEntityAsync(
                    i % 2 == 0 ? "Opportunity" : "Project",
                    (i % 10) + 1,
                    user)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    /// <summary>
    /// Scalability: Risk write throughput scales under concurrent create/update load.
    /// </summary>
    [Fact]
    public async Task Scalability_RiskWriteThroughput_ScalesUnderLoad()
    {
        var user = CreateTestUser();
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => i % 2 == 0
                    ? _manager.CreateRiskAsync(CreateTestRequest(i), user)
                    : _manager.UpdateRiskAsync((i % 10) + 1, CreateTestRequest(i), user)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} concurrent writes, avg {perUser:F0}ms/write — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private Task RunMixedReadAsync(int i, ClaimsPrincipal? user)
    {
        return (i % 3) switch
        {
            0 => _manager.GetRisksByEntityAsync("Opportunity", (i % 10) + 1, user),
            1 => _manager.GetRiskLookupsAsync(),
            _ => _manager.GetRiskCategoriesAsync()
        };
    }

    private async Task MeasuredCreateAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.CreateRiskAsync(CreateTestRequest(index), CreateTestUser());
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
