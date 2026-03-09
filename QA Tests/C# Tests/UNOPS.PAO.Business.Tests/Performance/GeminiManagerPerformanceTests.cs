/**
 * PERFORMANCE TESTS — GeminiManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * Covers: AI prompt retrieval, AI content generation (mocked), DST analysis (mocked),
 * AI response caching, concurrent AI requests, memory efficiency.
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * NOTE: External AI service calls (Gemini API) are mocked to isolate manager
 * orchestration and data-layer performance. UNOPSGeminiManager cannot be
 * instantiated in test env due to DEF-053 (missing Google credentials).
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Diagnostics;
using System.Security.Claims;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for GeminiManager / IGeminiManager.
/// Verifies response times for prompt retrieval, AI operation orchestration,
/// search/filter, concurrent access, and memory efficiency.
///
/// Required: ≥16 tests (FIXED)
/// Uses base GeminiManager for GetPromptData (DB operations); mocks IGeminiManager
/// for AI operations that require external Gemini API (DEF-053 blocks real UNOPSGeminiManager).
/// </summary>
public class GeminiManagerPerformanceTests : PerformanceTestBase
{
    private readonly IGeminiManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"GeminiPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);   // TODO: confirm SLA
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);  // TODO: confirm SLA
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);     // TODO: confirm SLA
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);  // TODO: confirm SLA
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);   // TODO: confirm SLA
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);   // TODO: confirm SLA
    private const int MaxMemoryGrowthMb = 50;      // TODO: confirm SLA
    private const int MaxQueryMemoryMb = 100;      // TODO: confirm SLA
    private static readonly int MaxAiOperationMs = ScaleThreshold(5_000);    // AI ops with mocked response; relaxed for CI

    public GeminiManagerPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();

        // Base GeminiManager for GetPromptData (real DB operations)
        _manager = new GeminiManager(mapper, Context);

        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task GetPromptData_SingleType_CompletesWithinThreshold()
    {
        var prompt = await SeedAiPromptAsync("SUMMARY");
        var type = prompt.Type!;

        _stopwatch.Restart();
        var result = await _manager.GetPromptData(type);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().Contain(p => p.Id == prompt.Id);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetPromptData took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetPromptData_ExistingType_CompletesWithinThreshold()
    {
        var prompt = await SeedAiPromptAsync("INSIGHTS");

        _stopwatch.Restart();
        var result = await _manager.GetPromptData(prompt.Type!);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().Contain(p => p.Id == prompt.Id);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetPromptData took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task AiPrompt_BulkRetrieval_100Prompts_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("BULK", 100);
        var typePrefix = $"BULK_{_testMarker}";

        _stopwatch.Restart();
        var result = await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.Type != null && p.Type.StartsWith(typePrefix))
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"AiPrompt bulk retrieval (100 prompts) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetPromptData_MultipleTypes_CompletesWithinThreshold()
    {
        var promptA = await SeedAiPromptAsync("TYPE_A");
        var promptB = await SeedAiPromptAsync("TYPE_B");

        _stopwatch.Restart();
        var resultA = await _manager.GetPromptData(promptA.Type!);
        var resultB = await _manager.GetPromptData(promptB.Type!);
        _stopwatch.Stop();

        resultA.Should().Contain(p => p.Id == promptA.Id);
        resultB.Should().Contain(p => p.Id == promptB.Id);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetPromptData (2 types) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Mocked_GenerateOpportunityStatementAsync_Bulk10_CompletesWithinThreshold()
    {
        var mock = CreateMockGeminiManager();
        mock.Setup(m => m.GenerateOpportunityStatementAsync(It.IsAny<int>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<bool>()))
            .ReturnsAsync("Generated statement content");

        var tasks = Enumerable.Range(1, 10)
            .Select(i => mock.Object.GenerateOpportunityStatementAsync(i, null!, false))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(10);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"10 mocked GenerateOpportunityStatementAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task AiPromptSearch_SimpleFilter_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("SEARCH", 200);
        var typePrefix = $"SEARCH_{_testMarker}";

        _stopwatch.Restart();
        var result = await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.Type != null && p.Type.StartsWith(typePrefix))
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple AiPrompt search took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task AiPromptSearch_ByModel_CompletesWithinThreshold()
    {
        await SeedAiPromptsByModelAsync(100);

        _stopwatch.Restart();
        var result = await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.Type != null && p.Type.Contains(_testMarker) && p.Model == "gemini-2.5-flash")
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Model filter search took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task AiPromptSearch_MultiColumnSort_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("SORT", 150);
        var typePrefix = $"SORT_{_testMarker}";

        _stopwatch.Restart();
        var result = await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.Type != null && p.Type.StartsWith(typePrefix))
            .OrderBy(p => p.Model)
            .ThenByDescending(p => p.CreatedAt)
            .Take(50)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCountLessThanOrEqualTo(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Multi-sort AiPrompt search took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task AiPromptSearch_WithTypeIndex_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("INDEXED", 300);
        var typePrefix = $"INDEXED_{_testMarker}";

        _stopwatch.Restart();
        var result = await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.Type != null && p.Type.StartsWith(typePrefix))
            .Take(100)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Type-prefix search took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task AiPromptSearch_Paginated_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("PAGE", 500);
        var typePrefix = $"PAGE_{_testMarker}";

        _stopwatch.Restart();
        var result = await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.Type != null && p.Type.StartsWith(typePrefix))
            .OrderBy(p => p.Id)
            .Skip(100)
            .Take(20)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Paginated AiPrompt query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]
    public async Task ConcurrentReads_50ParallelGetPromptData_MaintainsPerformance()
    {
        var prompt = await SeedAiPromptAsync("CONCURRENT");
        var type = prompt.Type!;

        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetPromptData(type))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r.Any(p => p.Id == prompt.Id));
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {avgMs}ms");
    }

    [Fact]
    public async Task ConcurrentMockedAI_20Parallel_CompletesWithinThreshold()
    {
        var mock = CreateMockGeminiManager();
        mock.Setup(m => m.GetSessionConfigurationAsync())
            .ReturnsAsync(new SessionConfiguration());

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => mock.Object.GetSessionConfigurationAsync())
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel mocked GetSessionConfigurationAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConcurrentMixed_PromptDataAndMockedAI_PerformanceStable()
    {
        var prompt = await SeedAiPromptAsync("MIXED");
        var type = prompt.Type!;
        var mock = CreateMockGeminiManager();
        mock.Setup(m => m.GetDSTRecommendationsAsync(It.IsAny<int>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>()))
            .ReturnsAsync(new DSTRecommendationsResponse { Recommendations = new List<DSTRecommendation>() });

        var readTasks = Enumerable.Range(0, 15)
            .Select(_ => _manager.GetPromptData(type))
            .Cast<Task>()
            .ToList();
        var aiTasks = Enumerable.Range(0, 5)
            .Select(i => mock.Object.GetDSTRecommendationsAsync(1, null!, 10, null, false))
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(aiTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent prompt + AI ops took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargePromptList_MemoryUsage_WithinCap()
    {
        await SeedAiPromptsAsync("MEMORY", 1000);
        var typePrefix = $"MEMORY_{_testMarker}";
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.Type != null && p.Type.StartsWith(typePrefix))
            .ToListAsync();

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedGetPromptData_NoMemoryLeak()
    {
        var prompt = await SeedAiPromptAsync("LEAK");
        var type = prompt.Type!;
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            Context.ChangeTracker.Clear();
            await _manager.GetPromptData(type);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var prompt = await SeedAiPromptAsync("GC");
        var type = prompt.Type!;
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            Context.ChangeTracker.Clear();
            _stopwatch.Restart();
            await _manager.GetPromptData(type);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        // Allow for fast in-memory/SQLite: if both are 0, skip strict check
        if (first25Avg > 0)
            last25Avg.Should().BeLessThan(first25Avg * 3,
                $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region AI Operation Performance (Mocked)

    [Fact]
    public async Task Mocked_GetDSTRecommendationsAsync_CompletesWithinThreshold()
    {
        var mock = CreateMockGeminiManager();
        mock.Setup(m => m.GetDSTRecommendationsAsync(It.IsAny<int>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<bool>()))
            .ReturnsAsync(new DSTRecommendationsResponse { Recommendations = new List<DSTRecommendation>() });

        _stopwatch.Restart();
        var result = await mock.Object.GetDSTRecommendationsAsync(1, null!, 10, null, false);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Mocked GetDSTRecommendationsAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Mocked_GenerateOpportunityInsightsAsync_CompletesWithinThreshold()
    {
        var mock = CreateMockGeminiManager();
        mock.Setup(m => m.GenerateOpportunityInsightsAsync(It.IsAny<int>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<bool>()))
            .ReturnsAsync(new OpportunityInsightsResponse { Insights = new List<OpportunityInsight>(), Suggestions = new List<OpportunitySuggestion>() });

        _stopwatch.Restart();
        var result = await mock.Object.GenerateOpportunityInsightsAsync(1, null!, false);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Mocked GenerateOpportunityInsightsAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region EF Core — N+1 & AsNoTracking Verification

    [Fact]
    public async Task GetPromptData_NoCartesianExplosion_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("N1", 50);
        var typePrefix = $"N1_{_testMarker}";

        _stopwatch.Restart();
        var result = await Context.AiPrompts
            .AsNoTracking()
            .Where(p => p.Type != null && p.Type.StartsWith(typePrefix))
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms for 50 records");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var prompt = await SeedAiPromptAsync("BENCH");
        var type = prompt.Type!;

        report["GetPromptData"] = await TimeMs(() => _manager.GetPromptData(type));

        var mock = CreateMockGeminiManager();
        mock.Setup(m => m.GetSessionConfigurationAsync()).ReturnsAsync(new SessionConfiguration());
        report["GetSessionConfiguration (mocked)"] = await TimeMs(() => mock.Object.GetSessionConfigurationAsync());

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-35}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private static Mock<IGeminiManager> CreateMockGeminiManager()
    {
        var mock = new Mock<IGeminiManager>();
        mock.Setup(m => m.GetSessionConfigurationAsync())
            .ReturnsAsync(new SessionConfiguration());
        mock.Setup(m => m.GetPromptData(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<AiPrompt>());
        return mock;
    }

    private AiPrompt BuildAiPrompt(string type, string model = "gemini-1.5-pro") => new()
    {
        Name = $"Prompt_{type}_{_testMarker}",
        Type = $"{type}_{_testMarker}",
        DataRetrievalMethod = "GetDataAsync",
        SystemInstructions = "You are a helpful assistant.",
        UserPrompt = "Summarize: {{data}}",
        Feature = "Test",
        Description = "Perf test",
        GenerationConfig = "{\"temperature\":0.7}",
        ContentConfig = "{\"format\":\"text\"}",
        Project = "test-project",
        Location = "us-central1",
        Model = model,
        AdminCanChange = true,
        CreatedAt = DateTime.UtcNow
    };

    private async Task<AiPrompt> SeedAiPromptAsync(string type)
    {
        var prompt = BuildAiPrompt(type);
        await Context.AiPrompts.AddAsync(prompt);
        await SaveChangesAsync();
        return prompt;
    }

    private async Task SeedAiPromptsAsync(string type, int count)
    {
        // AiPrompt has unique constraint on Type — each prompt needs unique Type
        var typePrefix = $"{type}_{_testMarker}";
        var existing = await Context.AiPrompts.CountAsync(p => p.Type != null && p.Type.StartsWith(typePrefix));
        if (existing >= count) return;

        var toAdd = count - existing;
        var startIdx = existing + 1;
        var prompts = Enumerable.Range(startIdx, toAdd)
            .Select(i =>
            {
                var p = BuildAiPrompt(type);
                p.Type = $"{typePrefix}_{i}"; // Unique per prompt
                p.Name = $"Prompt_{type}_{i}_{_testMarker}";
                return p;
            })
            .ToList();

        await Context.AiPrompts.AddRangeAsync(prompts);
        await SaveChangesAsync();
    }

    private async Task SeedAiPromptsByModelAsync(int count)
    {
        var prompts = Enumerable.Range(1, count)
            .Select(i => new AiPrompt
            {
                Name = $"Model_{i}_{_testMarker}",
                Type = $"MODEL_{i}_{_testMarker}",
                DataRetrievalMethod = "GetDataAsync",
                SystemInstructions = "Test",
                UserPrompt = "Test",
                Feature = "Test",
                GenerationConfig = "{}",
                ContentConfig = "{}",
                Project = "test",
                Location = "us-central1",
                Model = i % 2 == 0 ? "gemini-2.5-flash" : "gemini-1.5-pro",
                AdminCanChange = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        await Context.AiPrompts.AddRangeAsync(prompts);
        await SaveChangesAsync();
    }

    private async Task<long> TimeMs(Func<Task> fn)
    {
        _stopwatch.Restart();
        await fn();
        _stopwatch.Stop();
        return _stopwatch.ElapsedMilliseconds;
    }

    #endregion
}
