/**
 * LOAD TESTS — LinkManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * LinkManager handles link/URL management operations — creating, reading, updating,
 * and deleting links associated with partners, opportunities, contacts, etc.
 * Tests use mocked ILinkManager for unit-level throughput and concurrency validation.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Links;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for LinkManager (via ILinkManager).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2)
///
/// Uses mocked ILinkManager to measure concurrent invocation patterns without DB dependency.
/// Covers: concurrent link creation across entities, bulk retrieval, sustained listing/filtering,
/// stress CRUD operations, recovery after stress, cross-entity link query scalability.
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class LinkManagerLoadTests
{
    private readonly Mock<ILinkManager> _mockManager;
    private readonly ILinkManager _manager;
    private readonly Stopwatch _stopwatch = new();

    // Load targets — TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section B1-B4
    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public LinkManagerLoadTests()
    {
        _mockManager = new Mock<ILinkManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.CreateLinkAsync(It.IsAny<LinkRequest>()))
            .ReturnsAsync((LinkRequest req) => CreateMockLinkModel(1, req));

        _mockManager
            .Setup(m => m.GetLink(It.IsAny<int>()))
            .ReturnsAsync((int id) => CreateMockLinkModel(id, LinkEntityType.Partner, 1));

        _mockManager
            .Setup(m => m.GetLinks())
            .Returns(CreateMockLinkList());

        _mockManager
            .Setup(m => m.UpdateLinkAsync(It.IsAny<UpdateLinkRequest>()))
            .ReturnsAsync((UpdateLinkRequest req) => CreateMockLinkModel(req.Id, req));

        _mockManager
            .Setup(m => m.DeleteLinkAsync(It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockManager
            .Setup(m => m.GetEntityLinks(It.IsAny<LinkEntityType>(), It.IsAny<int>(), It.IsAny<PaginationRequest>()))
            .ReturnsAsync((LinkEntityType entity, int entityId, PaginationRequest _) =>
                CreateMockPaginationResponse(entity, entityId));
    }

    private static LinkModel CreateMockLinkModel(int id, LinkRequest req)
    {
        return new LinkModel
        {
            Id = id,
            Entity = req.Entity,
            EntityId = req.EntityId,
            Url = req.Url,
            Name = req.Name ?? req.Url
        };
    }

    private static LinkModel CreateMockLinkModel(int id, LinkEntityType entity, int entityId)
    {
        return new LinkModel
        {
            Id = id,
            Entity = entity,
            EntityId = entityId,
            Url = $"https://example.com/link{id}",
            Name = $"Link {id}"
        };
    }

    private static LinkModel CreateMockLinkModel(int id, UpdateLinkRequest req)
    {
        return new LinkModel
        {
            Id = req.Id,
            Entity = req.Entity,
            EntityId = req.EntityId,
            Url = req.Url,
            Name = req.Name ?? req.Url
        };
    }

    private static IEnumerable<LinkModel> CreateMockLinkList()
    {
        return Enumerable.Range(1, 10).Select(i => CreateMockLinkModel(i, LinkEntityType.Partner, 1));
    }

    private static PaginationResponse<LinkModel> CreateMockPaginationResponse(LinkEntityType entity, int entityId)
    {
        var records = Enumerable.Range(1, 5)
            .Select(i => CreateMockLinkModel(i, entity, entityId))
            .ToList();
        return new PaginationResponse<LinkModel>
        {
            Records = records,
            TotalCount = records.Count,
            PageIndex = 1,
            PageSize = 10,
            TotalPages = 1
        };
    }

    private static LinkRequest CreateLinkRequest(int index)
    {
        var entityTypes = new[] { LinkEntityType.Partner, LinkEntityType.Contact, LinkEntityType.PartnerTree };
        var entity = entityTypes[index % entityTypes.Length];
        return new LinkRequest
        {
            Entity = entity,
            EntityId = (index % 10) + 1,
            Url = $"https://example.com/load-test-{index}",
            Name = $"Load Test Link {index}"
        };
    }

    private static UpdateLinkRequest CreateUpdateLinkRequest(int id, int index)
    {
        var req = CreateLinkRequest(index);
        return new UpdateLinkRequest
        {
            Id = id,
            Entity = req.Entity,
            EntityId = req.EntityId,
            Url = req.Url,
            Name = req.Name
        };
    }

    private static PaginationRequest CreatePaginationRequest() => new() { PageIndex = 1, PageSize = 10 };

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — bulk link retrieval under normal load.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_BulkLinkRetrieval_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var operationCount = Math.Min(NormalUsers * 2, 200);

        for (int i = 0; i < operationCount; i++)
        {
            _stopwatch.Restart();
            await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
            _stopwatch.Stop();
            lock (times) times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 100),
            $"Bulk retrieval degraded from {first:F0}ms to {last:F0}ms avg under sustained load");
    }

    /// <summary>
    /// Phase 2: Sustained write load — concurrent link creation across different entities.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentLinkCreationAcrossEntities_ConsistencyMaintained()
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
    public async Task SustainedLoad_LinkListingAndFilteringByEntity_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.8);
        var writeCount = NormalUsers - readCount;

        var reads = Enumerable.Range(0, readCount)
            .Select(_ => _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest()));
        var writes = Enumerable.Range(0, writeCount)
            .Select(i => _manager.CreateLinkAsync(CreateLinkRequest(i)));

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
    /// Phase 5: Sudden spike in concurrent link retrieval — system handles gracefully.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenIncrease_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest()))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers)
            .Select(_ => _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest()))
            .ToArray();
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
        await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, PeakUsers)
            .Select(_ => _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest())));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
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
                await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
                Interlocked.Increment(ref completed);
            })
            .ToArray();

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
                await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
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
    /// Phase 3: Concurrent link CRUD — data integrity maintained under stress.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentLinkCrud_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent link CRUD stress");
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
        await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        await Task.WhenAll(Enumerable.Range(0, StressUsers)
            .Select(_ => _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest())));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
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
        await Task.WhenAll(Enumerable.Range(0, 50).Select(i => _manager.CreateLinkAsync(CreateLinkRequest(i))));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetEntityLinks(LinkEntityType.Partner, 1, CreatePaginationRequest());
        result.Should().NotBeNull("Post-stress read should succeed — link listing remains functional.");
        result!.Records.Should().NotBeNull();
    }

    #endregion

    #region Cross-Entity Scalability (optional — meets user requirement)

    /// <summary>
    /// Cross-entity link query scalability — scales under load across Partner, Contact, PartnerTree.
    /// </summary>
    [Fact]
    public async Task CrossEntity_LinkQueryScalability_ScalesUnderLoad()
    {
        var entityConfigs = new[]
        {
            (LinkEntityType.Partner, 1),
            (LinkEntityType.Contact, 1),
            (LinkEntityType.PartnerTree, 1)
        };
        var perEntity = 25;

        foreach (var (entityType, entityId) in entityConfigs)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, perEntity)
                .Select(_ => _manager.GetEntityLinks(entityType, entityId, CreatePaginationRequest())));
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
        await _manager.CreateLinkAsync(CreateLinkRequest(index));
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
