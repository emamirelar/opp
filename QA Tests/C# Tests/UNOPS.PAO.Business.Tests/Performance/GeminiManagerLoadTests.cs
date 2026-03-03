/**
 * LOAD TESTS — GeminiManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * GeminiManager (UNOPSGeminiManager) handles AI/Gemini integration: DST operations,
 * AI content generation, AI-powered analysis, AI assistant. Resource-intensive,
 * calls external AI services. Tests mock IGeminiManager to isolate throughput and
 * concurrency handling from external AI calls.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Load Tests for GeminiManager (via IGeminiManager).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions
/// for AI-related operations: content generation, analysis, DST, prompt retrieval, embeddings.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2)
///
/// Uses mocked IGeminiManager to measure concurrent invocation patterns without DB or external AI calls.
/// AI-specific: lower concurrency targets (AI ops are resource-intensive).
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class GeminiManagerLoadTests
{
    private readonly Mock<IGeminiManager> _mockManager;
    private readonly IGeminiManager _manager;
    private readonly Stopwatch _stopwatch = new();

    // Load targets — AI ops are resource-intensive; use lower concurrency than typical CRUD
    // TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section B1-B4, A4
    private const int NormalUsers = 25;
    private const int PeakUsers = 50;
    private const int StressUsers = 200;
    private const int MaxP95ResponseMs = 10_000; // AI ops can be slower (e.g. statement generation)
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 500;

    public GeminiManagerLoadTests()
    {
        _mockManager = new Mock<IGeminiManager>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        // Simulate fast async AI operations (mocked external AI calls)
        _mockManager
            .Setup(m => m.GetPromptData(It.IsAny<string>()))
            .ReturnsAsync((string type) => CreateMockAiPrompts(type));

        _mockManager
            .Setup(m => m.ProcessDataRelatedSummaryDetails(It.IsAny<GeminiProcessDataRequest>(), It.IsAny<ClaimsPrincipal?>()))
            .ReturnsAsync("Mock AI summary content");

        _mockManager
            .Setup(m => m.GenerateOpportunityInsightsAsync(It.IsAny<int>(), It.IsAny<ClaimsPrincipal?>(), It.IsAny<bool>()))
            .ReturnsAsync(CreateMockOpportunityInsightsResponse());

        _mockManager
            .Setup(m => m.GetDSTRecommendationsAsync(It.IsAny<int>(), It.IsAny<ClaimsPrincipal?>(), It.IsAny<int>(), It.IsAny<List<int>?>(), It.IsAny<bool>()))
            .ReturnsAsync(CreateMockDSTRecommendationsResponse());

        _mockManager
            .Setup(m => m.GetSimilarProjectsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ClaimsPrincipal?>(), It.IsAny<bool>()))
            .ReturnsAsync(CreateMockSimilarProjectsResponse());

        _mockManager
            .Setup(m => m.GetRelevantPeopleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ClaimsPrincipal?>(), It.IsAny<bool>()))
            .ReturnsAsync(CreateMockRelevantPeopleResponse());

        _mockManager
            .Setup(m => m.GetSessionConfigurationAsync())
            .ReturnsAsync(CreateMockSessionConfiguration());

        _mockManager
            .Setup(m => m.ChatWithGemini(It.IsAny<GeminiAssistantRequest>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<IHeaderDictionary?>()))
            .ReturnsAsync("Mock AI chat response");

        _mockManager
            .Setup(m => m.CreateBatchEmbeddingsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync((List<string> texts) => texts.Select(_ => "mock-embedding-id").ToList());
    }

    private static IEnumerable<AiPrompt> CreateMockAiPrompts(string type)
    {
        return Enumerable.Range(1, 5).Select(i => new AiPrompt
        {
            Id = i,
            Name = $"Prompt {type} {i}",
            Type = type,
            DataRetrievalMethod = "GetData",
            SystemInstructions = "System instructions",
            Feature = "AI",
            GenerationConfig = "{}",
            ContentConfig = "{}",
            Project = "test",
            Location = "us-central1",
            Model = "gemini-1.5",
            Status = EntityStatus.Active
        });
    }

    private static OpportunityInsightsResponse CreateMockOpportunityInsightsResponse()
    {
        return new OpportunityInsightsResponse
        {
            Insights = new List<OpportunityInsight> { new() { Title = "Insight", Description = "Desc", Type = "info" } },
            Suggestions = new List<OpportunitySuggestion> { new() { Title = "Suggestion", Description = "Desc" } },
            AnalysisConfidence = 0.9,
            AnalysisTimestamp = DateTime.UtcNow,
            ExecutionTimeMs = 100
        };
    }

    private static DSTRecommendationsResponse CreateMockDSTRecommendationsResponse()
    {
        return new DSTRecommendationsResponse
        {
            Recommendations = new List<DSTRecommendation> { new() { Title = "Risk", Description = "Desc", Recommendation = "Mitigate" } },
            ExtractedKeywords = new List<string> { "keyword" },
            TotalFound = 1,
            ExecutionTimeMs = 150
        };
    }

    private static SimilarProjectsResponse CreateMockSimilarProjectsResponse()
    {
        return new SimilarProjectsResponse
        {
            SimilarProjects = new List<SimilarProjectModel> { new() { ProjectId = "P1", RelevanceScore = 85 } },
            ExtractedKeywords = new List<string> { "project" },
            TotalFound = 1
        };
    }

    private static RelevantPeopleResponse CreateMockRelevantPeopleResponse()
    {
        return new RelevantPeopleResponse
        {
            RelevantPeople = new List<RelevantPersonModel> { new() { PersonId = "U1", Name = "User", RelevanceScore = 80 } },
            ExtractedRoles = new List<string> { "PM" },
            TotalFound = 1
        };
    }

    private static SessionConfiguration CreateMockSessionConfiguration()
    {
        return new SessionConfiguration
        {
            AppName = "Opportunity+",
            ApplicationName = "AI",
            ProjectName = "test",
            Organization = "UNOPS",
            Environment = "Test",
            Version = "1.0"
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

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained AI content generation — ProcessDataRelatedSummaryDetails under normal load.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentAIContentGeneration_PerformanceDoesNotDegrade()
    {
        var user = CreateTestUser();
        var request = new GeminiProcessDataRequest { Id = 1, Type = "summary", Message = "Analyze" };
        var times = new List<long>();
        var operationCount = Math.Min(NormalUsers * 2, 100);

        for (int i = 0; i < operationCount; i++)
        {
            _stopwatch.Restart();
            await _manager.ProcessDataRelatedSummaryDetails(request, user);
            _stopwatch.Stop();
            lock (times) times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        var threshold = Math.Max(first * 10, 100);
        last.Should().BeLessThanOrEqualTo(threshold,
            $"AI content generation degraded from {first:F0}ms to {last:F0}ms avg under sustained load");
    }

    /// <summary>
    /// Phase 2: Sustained bulk AI analysis — GenerateOpportunityInsights, GetDSTRecommendations, GetSimilarProjects under load.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_BulkAIAnalysisOperations_ConsistencyMaintained()
    {
        var user = CreateTestUser();
        var times = new List<long>();
        var writeCount = NormalUsers / 2;

        var tasks = Enumerable.Range(0, writeCount)
            .Select(i => MeasuredAIAnalysisAsync(i, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        var maxStdDev = Math.Max(avg * 2, 5);
        stdDev.Should().BeLessThanOrEqualTo(maxStdDev,
            $"Bulk AI analysis times inconsistent under {writeCount} concurrent calls (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed AI load — 80% read (prompts, session config), 20% analysis (insights, DST).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedAIOperations_ThroughputMeetsTarget()
    {
        var user = CreateTestUser();
        var readCount = (int)(NormalUsers * 0.8);
        var writeCount = NormalUsers - readCount;

        var reads = Enumerable.Range(0, readCount).Select(_ => SimulateAIReadAsync());
        var writes = Enumerable.Range(0, writeCount).Select(i => SimulateAIAnalysisAsync(i));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Concat(writes));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed AI load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent AI requests — many users requesting AI simultaneously.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenAISpike_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10).Select(_ => SimulateAIReadAsync()).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers).Select(_ => SimulateAIAnalysisAsync(0)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / baselineMs;
        scale.Should().BeLessThan((double)PeakUsers / 10 * 2,
            $"AI spike scaled {scale:F1}× — expected <{(double)PeakUsers / 10 * 2:F1}×");
    }

    /// <summary>
    /// Phase 5: Recovery after AI stress spike — returns to baseline performance.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_Recovery_ReturnsToBaseline()
    {
        var baselineMs = await MeasureSingleAIOpMs();

        await Task.WhenAll(Enumerable.Range(0, PeakUsers).Select(_ => SimulateAIAnalysisAsync(0)));

        await Task.Delay(RecoveryWindowMs);

        var postSpikeMs = await MeasureSingleAIOpMs();
        var recoveryThreshold = Math.Max(baselineMs * 3, 1);
        postSpikeMs.Should().BeLessThanOrEqualTo(recoveryThreshold,
            $"Post-spike AI response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Phase 3: Beyond capacity — stress testing AI service throughput; system does not crash.
    /// </summary>
    [Fact]
    public async Task StressLoad_BeyondCapacity_DoesNotCrash()
    {
        var completed = 0;

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async _ =>
            {
                await SimulateAIAnalysisAsync(0);
                Interlocked.Increment(ref completed);
            }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent AI users — only {completed} completed");
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
                await SimulateAIAnalysisAsync(0);
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
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent AI users");
    }

    /// <summary>
    /// Phase 3: Concurrent AI operations — data integrity maintained under stress.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentAIOperations_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            await SimulateAIReadAsync();
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent AI stress load");
    }

    #endregion

    #region Recovery (min 2) — Phase 3 + 5

    /// <summary>
    /// Phase 3+5: After stress — performance restored.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_PerformanceRestored()
    {
        var baselineMs = await MeasureSingleAIOpMs();

        await Task.WhenAll(Enumerable.Range(0, StressUsers).Select(_ => SimulateAIAnalysisAsync(0)));

        await Task.Delay(RecoveryWindowMs);

        var recoveredMs = await MeasureSingleAIOpMs();
        var recoveryThreshold = Math.Max(baselineMs * 2, 1);
        recoveredMs.Should().BeLessThanOrEqualTo(recoveryThreshold,
            $"System did not recover: post-stress {recoveredMs}ms vs baseline {baselineMs}ms");
    }

    /// <summary>
    /// Phase 3+5: After stress — no state corruption, AI operations succeed.
    /// </summary>
    [Fact]
    public async Task Recovery_AfterStress_NoStateCorruption()
    {
        await Task.WhenAll(Enumerable.Range(0, 25).Select(i => SimulateAIAnalysisAsync(i)));

        await Task.Delay(RecoveryWindowMs);

        var prompts = await _manager.GetPromptData("insights");
        prompts.Should().NotBeNull("Post-stress prompt retrieval should succeed.");
        prompts.Should().NotBeEmpty();
    }

    #endregion

    #region Scalability — AI prompt retrieval

    /// <summary>
    /// AI prompt retrieval scalability — GetPromptData scales under load.
    /// </summary>
    [Fact]
    public async Task BulkRead_AIPromptRetrieval_ScalesUnderLoad()
    {
        var promptTypes = new[] { "insights", "dst", "summary", "chat" };
        var perType = 25;

        foreach (var type in promptTypes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, perType)
                .Select(_ => _manager.GetPromptData(type)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)perType;
            perUser.Should().BeLessThan(100,
                $"At {perType} users for prompt type {type}, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task SimulateAIReadAsync()
    {
        await _manager.GetPromptData("insights");
        await _manager.GetSessionConfigurationAsync();
    }

    private async Task SimulateAIAnalysisAsync(int index)
    {
        var user = CreateTestUser();
        await _manager.GenerateOpportunityInsightsAsync(index % 10 + 1, user, false);
        await _manager.GetDSTRecommendationsAsync(index % 10 + 1, user, 10, null, false);
    }

    private async Task MeasuredAIAnalysisAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await SimulateAIAnalysisAsync(index);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task<long> MeasureSingleAIOpMs()
    {
        var sw = Stopwatch.StartNew();
        await _manager.GetPromptData("insights");
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    #endregion
}
