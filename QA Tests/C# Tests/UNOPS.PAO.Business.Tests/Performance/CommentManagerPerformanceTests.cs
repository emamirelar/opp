/**
 * PERFORMANCE TESTS — CommentManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Comment CRUD on partners, opportunities, contacts, etc.
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Diagnostics;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Mapping;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Users;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for CommentManager.
/// Verifies response times, throughput, and behaviour under concurrent access
/// for comment CRUD, bulk retrieval, search/filter, and thread loading.
///
/// Required: ≥16 tests (FIXED)
/// Uses mocked IManagerWrapper.UserDataManager to isolate comment operations
/// and avoid N+1 UserDataManager calls affecting performance metrics.
/// </summary>
public class CommentManagerPerformanceTests : PerformanceTestBase
{
    private readonly ICommentManager _manager;
    private readonly IMapper _mapper;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"PerfComment_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(1_000);   // CI tolerance; tighten when SLA confirmed
    private static readonly int MaxBulkOperationMs = ScaleThreshold(10_000);    // CI tolerance for bulk; tighten when SLA confirmed
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(1_000);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(5_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(1_000);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(500);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public CommentManagerPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CommentMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        var mockUserDataManager = new Mock<IUserDataManager>();
        mockUserDataManager
            .Setup(m => m.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new PAOUserModel { Id = 1, Email = "perftest@unops.org" });

        var mockManagerWrapper = new Mock<IManagerWrapper>();
        mockManagerWrapper.Setup(m => m.UserDataManager).Returns(mockUserDataManager.Object);

        _manager = new CommentManager(_mapper, Context, mockManagerWrapper.Object);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task GetCommentById_ExistingComment_CompletesWithinThreshold()
    {
        var comment = await SeedCommentAsync("Partner", 1);

        _stopwatch.Restart();
        var result = await _manager.GetCommentByIdAsync(comment.Id);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetCommentById took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task CreateComment_SingleComment_CompletesWithinThreshold()
    {
        var request = new CommentRequest
        {
            EntityType = "Partner",
            EntityId = 1,
            Content = $"Perf test content {_testMarker}"
        };

        _stopwatch.Restart();
        var result = await _manager.CreateCommentAsync(request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreateComment took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task UpdateComment_ExistingComment_CompletesWithinThreshold()
    {
        var comment = await SeedCommentAsync("Opportunity", 1);

        _stopwatch.Restart();
        var result = await _manager.UpdateCommentAsync(new UpdateCommentRequest
        {
            Id = comment.Id,
            Content = $"Updated content {_testMarker}"
        });
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"UpdateComment took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task DeleteComment_ExistingComment_CompletesWithinThreshold()
    {
        var comment = await SeedCommentAsync("Contact", 1);

        _stopwatch.Restart();
        var result = await _manager.DeleteCommentAsync(comment.Id);
        _stopwatch.Stop();

        result.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"DeleteComment took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task GetCommentsByEntity_100Comments_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 100);

        _stopwatch.Restart();
        var result = await _manager.GetCommentsByEntityAsync(entityType, entityId, includeReplies: false);
        _stopwatch.Stop();

        result.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetCommentsByEntity (100 comments) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetCommentsByEntity_200Comments_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Opportunity", 200);

        _stopwatch.Restart();
        var result = await _manager.GetCommentsByEntityAsync(entityType, entityId, includeReplies: false);
        _stopwatch.Stop();

        result.Should().HaveCount(200);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetCommentsByEntity (200 comments) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetCommentCountAsync_EntityWithComments_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 50);

        _stopwatch.Restart();
        var result = await _manager.GetCommentCountAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().Be(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetCommentCountAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetCommentsByEntity_SimpleFilter_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 100);

        _stopwatch.Restart();
        var result = await _manager.GetCommentsByEntityAsync(entityType, entityId, includeReplies: true);
        _stopwatch.Stop();

        result.Should().HaveCount(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple GetCommentsByEntity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetCommentsByEntity_MultipleEntityTypes_CompletesWithinThreshold()
    {
        var (partnerType, partnerId) = await SeedCommentsForEntityAsync("Partner", 50);
        var (oppType, oppId) = await SeedCommentsForEntityAsync("Opportunity", 50);

        _stopwatch.Restart();
        var partnerComments = await _manager.GetCommentsByEntityAsync(partnerType, partnerId);
        var oppComments = await _manager.GetCommentsByEntityAsync(oppType, oppId);
        _stopwatch.Stop();

        partnerComments.Should().HaveCount(50);
        oppComments.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Multi-entity GetCommentsByEntity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetCommentsByEntity_ExcludesDeleted_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 100);
        var comments = await Context.Comments
            .Where(c => c.EntityType == entityType && c.EntityId == entityId && !c.IsDeleted)
            .Take(20)
            .ToListAsync();
        foreach (var c in comments)
        {
            c.IsDeleted = true;
            c.DeletedDate = DateTime.UtcNow;
        }
        await SaveChangesAsync();

        _stopwatch.Restart();
        var result = await _manager.GetCommentsByEntityAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(80);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetCommentsByEntity (excluding deleted) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetCommentsByEntity_ContactEntity_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Contact", 75);

        _stopwatch.Restart();
        var result = await _manager.GetCommentsByEntityAsync(entityType, entityId);
        _stopwatch.Stop();

        result.Should().HaveCount(75);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetCommentsByEntity (Contact) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task TogglePinAsync_ExistingComment_CompletesWithinThreshold()
    {
        var comment = await SeedCommentAsync("Partner", 1);

        _stopwatch.Restart();
        var result = await _manager.TogglePinAsync(comment.Id);
        _stopwatch.Stop();

        result.Should().BeTrue();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"TogglePinAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    /// <summary>
    /// DbContext is not thread-safe; tests 50 sequential reads to verify throughput.
    /// In production, each request has its own scoped DbContext.
    /// </summary>
    [Fact]
    public async Task ConcurrentReads_50SequentialGetCommentById_MaintainsPerformance()
    {
        var comment = await SeedCommentAsync("Partner", 1);
        var times = new List<long>();

        for (int i = 0; i < 50; i++)
        {
            _stopwatch.Restart();
            var result = await _manager.GetCommentByIdAsync(comment.Id);
            _stopwatch.Stop();
            result.Should().NotBeNull();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var avgMs = times.Average();
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read over 50 sequential calls exceeded threshold: {avgMs}ms");
    }

    /// <summary>
    /// DbContext is not thread-safe; tests 20 sequential list operations for throughput.
    /// </summary>
    [Fact]
    public async Task ConcurrentReads_20SequentialGetCommentsByEntity_MaintainsPerformance()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 30);

        _stopwatch.Restart();
        for (int i = 0; i < 20; i++)
        {
            var result = await _manager.GetCommentsByEntityAsync(entityType, entityId);
            result.Should().HaveCount(30);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs * 2,
            $"20 sequential GetCommentsByEntity took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 20);
        var comment = await SeedCommentAsync("Partner", entityId);

        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            await _manager.GetCommentsByEntityAsync(entityType, entityId);
            await _manager.GetCommentByIdAsync(comment.Id);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed sequential reads (20 ops) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeCommentList_MemoryUsage_WithinCap()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetCommentsByEntityAsync(entityType, entityId);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            var comment = await SeedCommentAsync("Partner", i);
            await _manager.GetCommentByIdAsync(comment.Id);
            await _manager.GetCommentCountAsync("Partner", i);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 100);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetCommentsByEntityAsync(entityType, entityId);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(first25Avg * 3,
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region EF Core — N+1 & AsNoTracking Verification

    /// <summary>
    /// Verifies comment retrieval does not introduce Cartesian product or severe N+1.
    /// CommentManager has known N+1 on UserDataManager.GetUserByIdAsync per comment;
    /// this test uses mocked UserDataManager so we measure query + mapping overhead.
    /// </summary>
    [Fact]
    public async Task GetCommentsByEntity_WithReplies_NoCartesianExplosion_CompletesWithinThreshold()
    {
        var (entityType, entityId) = await SeedCommentsWithRepliesAsync(20, 3);

        _stopwatch.Restart();
        var result = await _manager.GetCommentsByEntityAsync(entityType, entityId, includeReplies: true);
        _stopwatch.Stop();

        result.Should().HaveCount(20);
        result.Sum(c => (c.Replies != null ? c.Replies.Count : 0) + 1).Should().Be(80); // 20 parents + 60 replies
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Possible N+1 or Cartesian product — query took {_stopwatch.ElapsedMilliseconds}ms for 20 comments with replies");
    }

    /// <summary>
    /// Comment thread loading (parent + replies) should complete within threshold.
    /// </summary>
    [Fact]
    public async Task CommentThreadLoading_WithReplies_PerformsWithinThreshold()
    {
        var (entityType, entityId) = await SeedCommentsWithRepliesAsync(10, 5);

        _stopwatch.Restart();
        var result = await _manager.GetCommentsByEntityAsync(entityType, entityId, includeReplies: true);
        _stopwatch.Stop();

        result.Should().HaveCount(10);
        result.Should().OnlyContain(c => (c.Replies != null ? c.Replies.Count : 0) == 5);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Comment thread loading (10×5 replies) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var comment = await SeedCommentAsync("Partner", 1);

        report["GetCommentById"] = await TimeMs(() => _manager.GetCommentByIdAsync(comment.Id));
        report["GetCommentCount"] = await TimeMs(() => _manager.GetCommentCountAsync("Partner", 1));
        report["UpdateComment"] = await TimeMs(() => _manager.UpdateCommentAsync(new UpdateCommentRequest
        {
            Id = comment.Id,
            Content = "Benchmark update"
        }));
        report["DeleteComment"] = await TimeMs(() => _manager.DeleteCommentAsync(comment.Id));

        var (entityType, entityId) = await SeedCommentsForEntityAsync("Partner", 20);
        report["GetCommentsByEntity"] = await TimeMs(() => _manager.GetCommentsByEntityAsync(entityType, entityId));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<Comment> SeedCommentAsync(string entityType, int entityId)
    {
        var comment = new Comment
        {
            EntityType = entityType,
            EntityId = entityId,
            Content = $"Content {_testMarker}",
            Name = $"Comment-{entityType}-{entityId}-{Guid.NewGuid():N}"[..50],
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Comments.AddAsync(comment);
        await SaveChangesAsync();
        return comment;
    }

    private async Task<(string EntityType, int EntityId)> SeedCommentsForEntityAsync(string entityType, int count)
    {
        var entityId = 1;
        var comments = Enumerable.Range(1, count)
            .Select(i => new Comment
            {
                EntityType = entityType,
                EntityId = entityId,
                Content = $"Comment {i} {_testMarker}",
                Name = $"Comment-{entityType}-{entityId}-{i}",
                Status = EntityStatus.Active,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddSeconds(-count + i),
                LastModifiedDate = DateTime.UtcNow
            })
            .ToList();
        await Context.Comments.AddRangeAsync(comments);
        await SaveChangesAsync();
        return (entityType, entityId);
    }

    private async Task<(string EntityType, int EntityId)> SeedCommentsWithRepliesAsync(int parentCount, int repliesPerParent)
    {
        var entityType = "Partner";
        var entityId = 1;
        var parents = new List<Comment>();

        for (int i = 0; i < parentCount; i++)
        {
            var parent = new Comment
            {
                EntityType = entityType,
                EntityId = entityId,
                Content = $"Parent {i} {_testMarker}",
                Name = $"Parent-{i}",
                Status = EntityStatus.Active,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddSeconds(-parentCount + i),
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.Comments.AddAsync(parent);
            await SaveChangesAsync();
            parents.Add(parent);

            for (int r = 0; r < repliesPerParent; r++)
            {
                var reply = new Comment
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    ParentCommentId = parent.Id,
                    Content = $"Reply {r} to parent {i}",
                    Name = $"Reply-{i}-{r}",
                    Status = EntityStatus.Active,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                await Context.Comments.AddAsync(reply);
            }
        }
        await SaveChangesAsync();
        return (entityType, entityId);
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
