/**
 * PERFORMANCE TESTS — NotificationManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: Notification CRUD, delivery, status management
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Diagnostics;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Notifications;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for NotificationManager.
/// Verifies response times, throughput, and behaviour under concurrent access
/// for notification CRUD, list queries, and status updates.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
public class NotificationPerformanceTests : PerformanceTestBase
{
    private readonly NotificationManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"NotifPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public NotificationPerformanceTests()
    {
        var mockHttpContextAccessor = TestDbContextFactory.CreateMockHttpContextAccessor(TestUserId.ToString());
        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        _manager = new NotificationManager(Context, userResolverService);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]
    public async Task CreateNotification_SingleEntity_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        await _manager.CreateNotification(TestUserId, $"Perf msg {_testMarker}", "System", "Info", new { id = 1 });
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"CreateNotification took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]
    public async Task GetNotifications_ExistingUser_CompletesWithinThreshold()
    {
        await SeedNotificationsAsync(TestUserId, 10);

        _stopwatch.Restart();
        var result = await _manager.GetNotifications(TestUserId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetNotifications took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]
    public async Task GetNotifications_100Records_CompletesWithinThreshold()
    {
        await SeedNotificationsAsync(TestUserId, 100);

        _stopwatch.Restart();
        var result = await _manager.GetNotifications(TestUserId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThanOrEqualTo(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetNotifications (100 records) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CreateNotification_Bulk100_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        for (int i = 0; i < 100; i++)
        {
            await _manager.CreateNotification(TestUserId, $"Bulk {i} {_testMarker}", "System", "Info", new { id = i });
        }
        _stopwatch.Stop();

        var count = await Context.Notifications.CountAsync(n => n.UserId == TestUserId && n.Message.Contains(_testMarker));
        count.Should().Be(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Bulk create 100 notifications took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task MarkAsRead_Bulk50_CompletesWithinThreshold()
    {
        var ids = await SeedNotificationsAsync(TestUserId, 50);

        _stopwatch.Restart();
        foreach (var id in ids)
        {
            await _manager.MarkAsRead(id, TestUserId);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Bulk MarkAsRead 50 took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]
    public async Task GetNotifications_UnreadOnly_CompletesWithinThreshold()
    {
        await SeedNotificationsAsync(TestUserId, 200, isRead: false);

        _stopwatch.Restart();
        var result = await _manager.GetNotifications(TestUserId, unreadOnly: true);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetNotifications (unread only) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetNotifications_ReadOnly_CompletesWithinThreshold()
    {
        await SeedNotificationsAsync(TestUserId, 100, isRead: true);

        _stopwatch.Restart();
        var result = await _manager.GetNotifications(TestUserId, unreadOnly: false);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThanOrEqualTo(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetNotifications (read only) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetNotifications_MultipleUsers_CompletesWithinThreshold()
    {
        await SeedNotificationsAsync(TestUserId, 50);
        await SeedNotificationsAsync(2, 50);

        _stopwatch.Restart();
        var result1 = await _manager.GetNotifications(TestUserId);
        var result2 = await _manager.GetNotifications(2);
        _stopwatch.Stop();

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"GetNotifications (multi-user) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetNotifications_OrderedByCreatedAt_CompletesWithinThreshold()
    {
        await SeedNotificationsAsync(TestUserId, 100);

        _stopwatch.Restart();
        var result = await _manager.GetNotifications(TestUserId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"GetNotifications (ordered) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task GetNotifications_WithRecordData_CompletesWithinThreshold()
    {
        await SeedNotificationsWithRecordDataAsync(TestUserId, 80);

        _stopwatch.Restart();
        var result = await _manager.GetNotifications(TestUserId);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().OnlyContain(n => n.Records != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetNotifications (with RecordData) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-068")]
    public async Task ConcurrentReads_50ParallelGetNotifications_MaintainsPerformance()
    {
        await SeedNotificationsAsync(TestUserId, 30);
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetNotifications(TestUserId))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(50);
        var avgMs = _stopwatch.ElapsedMilliseconds / 50.0;
        avgMs.Should().BeLessThan(MaxConcurrentReadMs,
            $"Average read under 50 parallel calls exceeded threshold: {avgMs}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-068")]
    public async Task ConcurrentWrites_10ParallelCreate_MaintainsPerformance()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _manager.CreateNotification(TestUserId, $"Concurrent {i} {_testMarker}", "System", "Info", new { id = i }))
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(tasks);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"10 parallel CreateNotification took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-068")]
    public async Task ConcurrentMixedReadWrite_PerformanceStable()
    {
        await SeedNotificationsAsync(TestUserId, 20);
        var notificationId = (await SeedNotificationsAsync(TestUserId, 1)).First();

        var readTasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetNotifications(TestUserId))
            .Cast<Task>()
            .ToList();
        var markTasks = Enumerable.Range(0, 5)
            .Select(_ => _manager.MarkAsRead(notificationId, TestUserId))
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(readTasks.Concat(markTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent read/write took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]
    public async Task LargeNotificationList_MemoryUsage_WithinCap()
    {
        await SeedNotificationsAsync(TestUserId, 500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetNotifications(TestUserId);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        await SeedNotificationsAsync(TestUserId, 10);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await _manager.GetNotifications(TestUserId);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [Fact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        await SeedNotificationsAsync(TestUserId, 50);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetNotifications(TestUserId);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(first25Avg * 3,
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region Update Performance

    [Fact]
    public async Task UpdateNotification_ExistingEntity_CompletesWithinThreshold()
    {
        var ids = await SeedNotificationsAsync(TestUserId, 1);
        var id = ids[0];

        _stopwatch.Restart();
        await _manager.UpdateNotification(id, $"Updated {_testMarker}", NotificationStatus.Done);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"UpdateNotification took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task MarkAsRead_Single_CompletesWithinThreshold()
    {
        var ids = await SeedNotificationsAsync(TestUserId, 1);
        var id = ids[0];

        _stopwatch.Restart();
        await _manager.MarkAsRead(id, TestUserId);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"MarkAsRead took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        var ids = await SeedNotificationsAsync(TestUserId, 1);
        var id = ids[0];

        report["GetNotifications"] = await TimeMs(() => _manager.GetNotifications(TestUserId));
        report["CreateNotification"] = await TimeMs(() => _manager.CreateNotification(TestUserId, $"Bench {_testMarker}", "System", "Info", new { id = 1 }));
        report["MarkAsRead"] = await TimeMs(() => _manager.MarkAsRead(id, TestUserId));
        report["UpdateNotification"] = await TimeMs(() => _manager.UpdateNotification(id, "Updated", NotificationStatus.Done));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<List<int>> SeedNotificationsAsync(int userId, int count, bool isRead = false)
    {
        var notifications = Enumerable.Range(0, count)
            .Select(i => new Notification
            {
                UserId = userId,
                Message = $"Perf {i} {_testMarker}",
                Category = i % 2 == 0 ? "System" : "User",
                ResponseType = "Info",
                RecordData = "{}",
                IsRead = isRead,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            })
            .ToList();
        await Context.Notifications.AddRangeAsync(notifications);
        await SaveChangesAsync();
        return notifications.Select(n => n.Id).ToList();
    }

    private async Task<List<int>> SeedNotificationsWithRecordDataAsync(int userId, int count)
    {
        var notifications = Enumerable.Range(0, count)
            .Select(i => new Notification
            {
                UserId = userId,
                Message = $"Perf with record {i} {_testMarker}",
                Category = "System",
                ResponseType = "Info",
                RecordData = $"[{{\"id\":{i},\"name\":\"Record{i}\"}}]",
                IsRead = false,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            })
            .ToList();
        await Context.Notifications.AddRangeAsync(notifications);
        await SaveChangesAsync();
        return notifications.Select(n => n.Id).ToList();
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
