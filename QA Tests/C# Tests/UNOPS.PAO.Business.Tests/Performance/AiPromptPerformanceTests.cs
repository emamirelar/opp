/**
 * PERFORMANCE TESTS — AiPromptManager (UNOPSAiPromptManager)
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: AI prompt management, CRUD, search/filter, pagination
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using System.Security.Claims;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.AI;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.Identity.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for AiPromptManager (UNOPSAiPromptManager).
/// Verifies CRUD speed, search/filter, pagination, concurrent reads.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class AiPromptPerformanceTests : PerformanceTestBase
{
    private readonly IAiPromptManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly ClaimsPrincipal _testUser;
    private readonly string _testMarker = $"AiPromptPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public AiPromptPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        var configuration = TestEnvironment.CreateTestConfiguration();

        var store = new Mock<IUserStore<PAOIdentityUser>>();
        var userManager = new UserManager<PAOIdentityUser>(
            store.Object, null!, null!, null!, null!, null!, null!, null!,
            new Mock<ILogger<UserManager<PAOIdentityUser>>>().Object);

        var mockManagerWrapper = new Mock<IManagerWrapper>();

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(s => s.HasPermissionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.CanPerformActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(true);

        var mockAiPromptCacheService = new Mock<IAiPromptCacheService>();
        mockAiPromptCacheService.Setup(s => s.GetCachedResultAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        _manager = new UNOPSAiPromptManager(mapper, Context, configuration, userManager, mockManagerWrapper.Object,
            mockPermissionService.Object, mockAiPromptCacheService.Object);
        _stopwatch = new Stopwatch();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString()),
            new Claim(ClaimTypes.Email, "perf@test.local"),
            new Claim(ClaimTypes.Name, "Perf Test User")
        };
        _testUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    #region Single Operation Performance (min 2)

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task CreatePrompt_SingleEntity_CompletesWithinThreshold()
    {
        var model = BuildAiPromptModel($"Create_{_testMarker}");

        _stopwatch.Restart();
        var result = await _manager.CreatePromptAsync(_testUser, model);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Type.Should().Contain("Create_");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreatePrompt took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptById_ExistingEntity_CompletesWithinThreshold()
    {
        var prompt = await SeedAiPromptAsync("GetById");

        _stopwatch.Restart();
        var result = await _manager.GetPromptByIdAsync(_testUser, prompt.Id!.Value);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Id.Should().Be(prompt.Id);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetPromptById took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptsAsync_100Prompts_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("BULK", 100);

        var request = new AiPromptFilterRequest { PageIndex = 1, PageSize = 100 };

        _stopwatch.Restart();
        var result = await _manager.GetPromptsAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Records.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetPromptsAsync (100) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptTypesAsync_AllTypes_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("TYPES", 50);

        _stopwatch.Restart();
        var result = await _manager.GetPromptTypesAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetPromptTypesAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetModelsAsync_AllModels_CompletesWithinThreshold()
    {
        await SeedAiPromptsByModelAsync(50);

        _stopwatch.Restart();
        var result = await _manager.GetModelsAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetModelsAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptsAsync_SimpleSearch_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("SEARCH", 200);
        var typePrefix = $"SEARCH_{_testMarker}";

        var request = new AiPromptFilterRequest { SearchText = typePrefix[..20], PageIndex = 1, PageSize = 50 };

        _stopwatch.Restart();
        var result = await _manager.GetPromptsAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple search took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptsAsync_ComplexFilter_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("COMPLEX", 150);

        var request = new AiPromptFilterRequest
        {
            SearchText = "COMPLEX",
            PageIndex = 1,
            PageSize = 50,
            OrderBy = "CreatedAt",
            Ascending = false
        };

        _stopwatch.Restart();
        var result = await _manager.GetPromptsAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Complex search took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptsByTypeAsync_ByType_CompletesWithinThreshold()
    {
        var prompt = await SeedAiPromptAsync("BYTYPE");
        var type = prompt.Type!;

        _stopwatch.Restart();
        var result = await _manager.GetPromptsByTypeAsync(_testUser, type);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().Contain(p => p.Type == type);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetPromptsByTypeAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptsAsync_MultiSort_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("SORT", 100);

        var request = new AiPromptFilterRequest
        {
            PageIndex = 1,
            PageSize = 20,
            OrderBy = "Type",
            Ascending = true
        };

        _stopwatch.Restart();
        var result = await _manager.GetPromptsAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Multi-sort pagination took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptsAsync_Pagination_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("PAGE", 500);

        var request = new AiPromptFilterRequest { PageIndex = 2, PageSize = 20 };

        _stopwatch.Restart();
        var result = await _manager.GetPromptsAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Records.Should().HaveCountLessOrEqualTo(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Paginated query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task ConcurrentReads_50ParallelGetPromptById_MaintainsPerformance()
    {
        var prompt = await SeedAiPromptAsync("CONCURRENT");
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetPromptByIdAsync(_testUser, prompt.Id!.Value))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        results.Should().OnlyContain(r => r != null);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {avgMs}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task ConcurrentReads_20ParallelGetPromptTypes_MaintainsPerformance()
    {
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _manager.GetPromptTypesAsync(_testUser))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel GetPromptTypesAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var prompt = await SeedAiPromptAsync("MIXED");
        var readTasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetPromptByIdAsync(_testUser, prompt.Id!.Value))
            .Cast<Task>()
            .ToList();
        var typeTasks = Enumerable.Range(0, 5)
            .Select(_ => _manager.GetPromptTypesAsync(_testUser))
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(typeTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent reads took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task LargePromptList_MemoryUsage_WithinCap()
    {
        await SeedAiPromptsAsync("MEMORY", 500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        var request = new AiPromptFilterRequest { PageIndex = 1, PageSize = 500 };
        await _manager.GetPromptsAsync(_testUser, request);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        var prompt = await SeedAiPromptAsync("LEAK");
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await _manager.GetPromptByIdAsync(_testUser, prompt.Id!.Value);
            await _manager.GetPromptTypesAsync(_testUser);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var prompt = await SeedAiPromptAsync("GC");
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetPromptByIdAsync(_testUser, prompt.Id!.Value);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(first25Avg * 3,
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region Update and Delete Performance

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task UpdatePrompt_ExistingEntity_CompletesWithinThreshold()
    {
        var prompt = await SeedAiPromptAsync("UPDATE");
        var model = BuildAiPromptModel($"Updated_{_testMarker}");
        model.Id = prompt.Id;

        _stopwatch.Restart();
        var result = await _manager.UpdatePromptAsync(_testUser, prompt.Id!.Value, model);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"UpdatePrompt took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task DeletePrompt_ExistingEntity_CompletesWithinThreshold()
    {
        var prompt = await SeedAiPromptAsync("DELETE");

        _stopwatch.Restart();
        var result = await _manager.DeletePromptAsync(_testUser, prompt.Id!.Value);
        _stopwatch.Stop();

        result.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"DeletePrompt took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region EF Core — N+1 & AsNoTracking Verification

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task GetPromptsAsync_WithRelated_NoCartesianExplosion_CompletesWithinThreshold()
    {
        await SeedAiPromptsAsync("N1", 50);

        var request = new AiPromptFilterRequest { PageIndex = 1, PageSize = 50 };

        _stopwatch.Restart();
        var result = await _manager.GetPromptsAsync(_testUser, request);
        _stopwatch.Stop();

        result.Records.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var prompt = await SeedAiPromptAsync("BENCH");
        var model = BuildAiPromptModel($"Bench_{Guid.NewGuid():N}"[..15]);

        report["GetPromptById"] = await TimeMs(() => _manager.GetPromptByIdAsync(_testUser, prompt.Id!.Value));
        report["GetPromptsAsync"] = await TimeMs(() => _manager.GetPromptsAsync(_testUser, new AiPromptFilterRequest { PageIndex = 1, PageSize = 20 }));
        report["GetPromptTypes"] = await TimeMs(() => _manager.GetPromptTypesAsync(_testUser));
        report["GetModels"] = await TimeMs(() => _manager.GetModelsAsync(_testUser));
        report["CreatePrompt"] = await TimeMs(() => _manager.CreatePromptAsync(_testUser, BuildAiPromptModel($"C_{Guid.NewGuid():N}"[..20])));
        report["UpdatePrompt"] = await TimeMs(() => _manager.UpdatePromptAsync(_testUser, prompt.Id!.Value, model));
        report["DeletePrompt"] = await TimeMs(() => _manager.DeletePromptAsync(_testUser, prompt.Id!.Value));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-20}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private AiPromptModel BuildAiPromptModel(string typeSuffix) => new()
    {
        Type = $"{typeSuffix}_{Guid.NewGuid():N}"[..50],
        Name = $"Prompt_{typeSuffix}",
        DataRetrievalMethod = "GetDataAsync",
        SystemInstructions = "You are a helpful assistant.",
        UserPrompt = "Summarize: {{data}}",
        Feature = "Test",
        Description = "Perf test",
        GenerationConfig = "{\"temperature\":0.7}",
        ContentConfig = "{\"format\":\"text\"}",
        Project = "test-project",
        Location = "us-central1",
        Model = "gemini-1.5-pro",
        CreatedAt = DateTime.UtcNow
    };

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
        var typePrefix = $"{type}_{_testMarker}";
        var existing = await Context.AiPrompts.CountAsync(p => p.Type != null && p.Type.StartsWith(typePrefix));
        if (existing >= count) return;

        var toAdd = count - existing;
        var startIdx = existing + 1;
        var prompts = Enumerable.Range(startIdx, toAdd)
            .Select(i =>
            {
                var p = BuildAiPrompt(type);
                p.Type = $"{typePrefix}_{i}";
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
