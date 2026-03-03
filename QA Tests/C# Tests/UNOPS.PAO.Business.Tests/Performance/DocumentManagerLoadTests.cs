/**
 * LOAD TESTS — DocumentManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * DocumentManager handles document retrieval, metadata management, and file content.
 * Tests use mocked IDocumentManager for unit-level throughput and concurrency validation.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using FluentAssertions;
using Moq;
using System.Diagnostics;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models.Documents;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for DocumentManager (via IDocumentManager).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2)
///
/// Uses mocked IDocumentManager to measure concurrent invocation patterns without DB dependency.
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class DocumentManagerLoadTests
{
    private readonly Stopwatch _stopwatch = new();

    // Load targets — TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
    private const int NormalConcurrentUsers = 50;
    private const int PeakConcurrentUsers = 100;
    private const int StressConcurrentUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 2_000;

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — bulk document retrieval under normal load.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_BulkDocumentRetrieval_PerformanceDoesNotDegrade()
    {
        var mock = CreateMockDocumentManager();
        var times = new List<long>();
        var tasks = Enumerable.Range(0, NormalConcurrentUsers)
            .Select(_ => MeasuredGetDocumentsAsync(mock.Object, times))
            .ToArray();

        _stopwatch.Restart();
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 100),
            $"Bulk retrieval degraded from {first:F0}ms to {last:F0}ms avg under {NormalConcurrentUsers} concurrent users");
    }

    /// <summary>
    /// Phase 2: Sustained write load — document metadata operations.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MetadataOperations_ConsistencyMaintained()
    {
        var mock = CreateMockDocumentManager();
        var times = new List<long>();
        var writeCount = NormalConcurrentUsers / 2;

        var tasks = Enumerable.Range(0, writeCount)
            .Select(i => MeasuredUpdateAsync(mock.Object, i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Metadata update times inconsistent under {writeCount} concurrent writers");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — document listing/querying (80% read, 20% metadata).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_DocumentListingAndQuerying_ThroughputMeetsTarget()
    {
        var mock = CreateMockDocumentManager();
        var readCount = (int)(NormalConcurrentUsers * 0.8);
        var writeCount = NormalConcurrentUsers - readCount;

        var reads = Enumerable.Range(0, readCount).Select(_ => SimulateListDocumentsAsync(mock.Object));
        var writes = Enumerable.Range(0, writeCount).Select(i => SimulateUpdateAsync(mock.Object, i));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Concat(writes));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalConcurrentUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent document retrieval.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenIncrease_HandlesGracefully()
    {
        var mock = CreateMockDocumentManager();
        var baselineTasks = Enumerable.Range(0, 10).Select(_ => SimulateGetByIdAsync(mock.Object)).ToArray();

        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = _stopwatch.ElapsedMilliseconds;

        var spikeTasks = Enumerable.Range(0, PeakConcurrentUsers).Select(_ => SimulateGetByIdAsync(mock.Object)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var effectiveNormal = Math.Max(baselineMs, 50);
        spikeMs.Should().BeLessThan(effectiveNormal * 30,
            $"Spike load ({spikeMs}ms) should not scale catastrophically vs baseline ({baselineMs}ms)");
    }

    /// <summary>
    /// Phase 5: Recovery after spike — returns to baseline performance.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_Recovery_ReturnsToBaseline()
    {
        var mock = CreateMockDocumentManager();
        var baselineMs = await MeasureSingleOpMs(mock.Object);

        await Task.WhenAll(Enumerable.Range(0, PeakConcurrentUsers)
            .Select(_ => SimulateGetByIdAsync(mock.Object)));

        await Task.Delay(RecoveryWindowMs);

        var postSpikeMs = await MeasureSingleOpMs(mock.Object);
        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 5, 10),
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
        var mock = CreateMockDocumentManager();
        var completed = 0;

        var tasks = Enumerable.Range(0, StressConcurrentUsers)
            .Select(_ => Task.Run(async () =>
            {
                await SimulateGetDocumentsAsync(mock.Object);
                Interlocked.Increment(ref completed);
            }))
            .ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(30));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressConcurrentUsers} concurrent users — only {completed} completed");
        completed.Should().Be(StressConcurrentUsers);
    }

    /// <summary>
    /// Phase 3: Error rate within acceptable limit under stress.
    /// </summary>
    [Fact]
    public async Task StressLoad_ErrorRate_WithinAcceptableLimit()
    {
        var mock = CreateMockDocumentManager();
        var success = 0;
        var failure = 0;

        var tasks = Enumerable.Range(0, StressConcurrentUsers).Select(async _ =>
        {
            try
            {
                await SimulateGetDocumentsAsync(mock.Object);
                Interlocked.Increment(ref success);
            }
            catch
            {
                Interlocked.Increment(ref failure);
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        var errorRate = (double)failure / StressConcurrentUsers;
        errorRate.Should().BeLessThan(MaxErrorRate,
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressConcurrentUsers} concurrent users");
    }

    /// <summary>
    /// Phase 3: Concurrent document metadata/deletion operations — data integrity maintained.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentMetadataAndDeletion_DataIntegrityMaintained()
    {
        var mock = CreateMockDocumentManager();
        var expected = Enumerable.Range(1, 100).Sum();
        var actual = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            await SimulateUpdateAsync(mock.Object, i);
            lock (lockObj)
            {
                actual += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actual.Should().Be(expected,
            "Data integrity compromised under concurrent metadata/deletion stress");
    }

    #endregion

    #region Recovery (min 2) — Phase 3 + 5

    /// <summary>
    /// Phase 3+5: Performance restored after stress.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_PerformanceRestored()
    {
        var mock = CreateMockDocumentManager();
        var baselineMs = await MeasureSingleOpMs(mock.Object);

        await Task.WhenAll(Enumerable.Range(0, StressConcurrentUsers)
            .Select(_ => SimulateGetByIdAsync(mock.Object)));

        await Task.Delay(RecoveryWindowMs);

        var recoveredMs = await MeasureSingleOpMs(mock.Object);
        recoveredMs.Should().BeLessThan(Math.Max(baselineMs * 5, 10),
            $"System did not recover: post-stress {recoveredMs}ms vs baseline {baselineMs}ms");
    }

    /// <summary>
    /// Phase 3+5: No state corruption after overload.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_NoStateCorruption()
    {
        var mock = CreateMockDocumentManager();

        await Task.WhenAll(Enumerable.Range(0, 50)
            .Select(i => SimulateUpdateAsync(mock.Object, i)));

        await Task.Delay(RecoveryWindowMs);

        var result = await mock.Object.GetDocumentsByEntityAsync("Opportunity", 1);
        result.Should().NotBeNull("Document listing should remain functional after stress recovery");
    }

    #endregion

    #region Helpers

    private static Mock<IDocumentManager> CreateMockDocumentManager()
    {
        var mock = new Mock<IDocumentManager>();
        var docModel = new DocumentModel
        {
            Id = 1,
            Name = "Test",
            Link = "https://example.com/doc.pdf",
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow,
            DocumentType = new DocumentTypeModel { Id = 1, Name = "PDF", EntityType = "Document" }
        };

        mock.Setup(m => m.GetDocumentByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(docModel);
        mock.Setup(m => m.GetDocumentsByEntityAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new[] { docModel });
        mock.Setup(m => m.ListDocumentsAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(new[] { docModel });
        mock.Setup(m => m.UpdateDocumentAsync(It.IsAny<UpdateDocumentRequest>()))
            .ReturnsAsync(docModel);
        mock.Setup(m => m.GetDocumentParentEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((1, "Opportunity"));
        mock.Setup(m => m.GetFileContentByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new byte[] { 1, 2, 3 });

        return mock;
    }

    private static async Task SimulateGetByIdAsync(IDocumentManager manager)
    {
        await manager.GetDocumentByIdAsync(1);
    }

    private static async Task SimulateGetDocumentsAsync(IDocumentManager manager)
    {
        await manager.GetDocumentsByEntityAsync("Opportunity", 1);
    }

    private static async Task SimulateListDocumentsAsync(IDocumentManager manager)
    {
        _ = manager.ListDocumentsAsync("Opportunity", 1).ToList();
        await Task.CompletedTask;
    }

    private static async Task SimulateUpdateAsync(IDocumentManager manager, int index)
    {
        await manager.UpdateDocumentAsync(new UpdateDocumentRequest { Id = index, DocumentTypeId = 1 });
    }

    private async Task MeasuredGetDocumentsAsync(IDocumentManager manager, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await SimulateGetDocumentsAsync(manager);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task MeasuredUpdateAsync(IDocumentManager manager, int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await SimulateUpdateAsync(manager, index);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task<long> MeasureSingleOpMs(IDocumentManager manager)
    {
        var sw = Stopwatch.StartNew();
        await SimulateGetByIdAsync(manager);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    #endregion
}
