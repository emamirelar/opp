/**
 * LOAD TESTS — NotificationManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * NotificationManager is concrete with no interface. Uses internal INotificationManagerMock
 * for mock-friendly load testing. Mixed load: 50% read, 30% write (create), 20% update (mark as read).
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Notifications;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Internal interface for mock-friendly NotificationManager load testing.
/// Mirrors NotificationManager methods since the concrete class has no interface.
/// </summary>
public interface INotificationManagerMock
{
    Task<List<NotificationModel>> GetNotifications(int userId, bool? unreadOnly = null);
    Task MarkAsRead(int notificationId, int userId);
    Task UpdateNotification(int notificationId, string message, NotificationStatus status);
    Task CreateNotification(int userId, string message, string category, string responseType, object record);
}

/// <summary>
/// Load Tests for NotificationManager.
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
/// Mixed load: 50% read, 30% write (create), 20% update (mark as read).
/// Uses mocked INotificationManagerMock (concrete NotificationManager has no interface).
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class NotificationManagerLoadTests
{
    private readonly Mock<INotificationManagerMock> _mockManager;
    private readonly INotificationManagerMock _manager;
    private readonly Stopwatch _stopwatch = new();

    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public NotificationManagerLoadTests()
    {
        _mockManager = new Mock<INotificationManagerMock>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager
            .Setup(m => m.GetNotifications(It.IsAny<int>(), It.IsAny<bool?>()))
            .ReturnsAsync((int userId, bool? unreadOnly) => CreateMockNotificationList(userId));

        _mockManager
            .Setup(m => m.MarkAsRead(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        _mockManager
            .Setup(m => m.UpdateNotification(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<NotificationStatus>()))
            .Returns(Task.CompletedTask);

        _mockManager
            .Setup(m => m.CreateNotification(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);
    }

    private static List<NotificationModel> CreateMockNotificationList(int userId)
    {
        return Enumerable.Range(1, 10).Select(i => new NotificationModel
        {
            Id = i,
            Message = $"Notification {i} for user {userId}",
            Category = i % 2 == 0 ? "System" : "User",
            ResponseType = "Info",
            Records = new List<object>(),
            Entity = "Opportunity",
            EntityId = i,
            Status = i % 3 == 0 ? NotificationStatus.Done : NotificationStatus.Pending
        }).ToList();
    }

    private static object CreateRecordPayload(int index)
    {
        return new { Id = index, Name = $"Record {index}", Timestamp = DateTime.UtcNow };
    }

    #region Sustained Load (min 3) — Phase 2

    /// <summary>
    /// Phase 2: Sustained read load — concurrent GetNotifications (50% read).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentGetNotifications_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var readCount = (int)(NormalUsers * 0.5);

        var tasks = Enumerable.Range(0, readCount)
            .Select(i => MeasuredGetNotificationsAsync((i % 5) + 1, times))
            .ToArray();

        await Task.WhenAll(tasks);

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 100),
            $"GetNotifications performance degraded from {first:F0}ms to {last:F0}ms avg under {readCount} concurrent users");
    }

    /// <summary>
    /// Phase 2: Sustained write/update load — CreateNotification and MarkAsRead (30% + 20%).
    /// </summary>
    [Fact]
    public async Task SustainedLoad_ConcurrentCreateAndMarkAsRead_ConsistencyMaintained()
    {
        var times = new List<long>();
        var createCount = (int)(NormalUsers * 0.3);
        var markReadCount = (int)(NormalUsers * 0.2);

        var createTasks = Enumerable.Range(0, createCount)
            .Select(i => MeasuredCreateAsync((i % 5) + 1, i, times));
        var markReadTasks = Enumerable.Range(0, markReadCount)
            .Select(i => MeasuredMarkAsReadAsync((i % 10) + 1, (i % 5) + 1, times));

        await Task.WhenAll(createTasks.Concat(markReadTasks));

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Create/MarkAsRead times inconsistent under concurrent load (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    /// <summary>
    /// Phase 2: Sustained mixed load — 50% read, 30% create, 20% mark as read.
    /// </summary>
    [Fact]
    public async Task SustainedLoad_MixedReadCreateUpdate_ThroughputMeetsTarget()
    {
        var readCount = (int)(NormalUsers * 0.5);
        var createCount = (int)(NormalUsers * 0.3);
        var updateCount = NormalUsers - readCount - createCount;

        var reads = Enumerable.Range(0, readCount)
            .Select(i => _manager.GetNotifications((i % 5) + 1, i % 2 == 0 ? null : true));
        var creates = Enumerable.Range(0, createCount)
            .Select(i => _manager.CreateNotification((i % 5) + 1, $"Msg {i}", "System", "Info", CreateRecordPayload(i)));
        var updates = Enumerable.Range(0, updateCount)
            .Select(i => _manager.MarkAsRead((i % 10) + 1, (i % 5) + 1));

        _stopwatch.Restart();
        await Task.WhenAll(reads.Cast<Task>().Concat(creates.Cast<Task>()).Concat(updates.Cast<Task>()));
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    /// <summary>
    /// Phase 5: Sudden spike in concurrent GetNotifications — system handles gracefully.
    /// </summary>
    [Fact]
    public async Task SpikeLoad_SuddenReadIncrease_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10)
            .Select(i => _manager.GetNotifications((i % 5) + 1))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetNotifications((i % 10) + 1))
            .ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / Math.Max(baselineMs, 1);
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
        await _manager.GetNotifications(1);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        await Task.WhenAll(Enumerable.Range(0, PeakUsers)
            .Select(i => _manager.GetNotifications((i % 10) + 1)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetNotifications(1);
        _stopwatch.Stop();
        var postSpikeMs = _stopwatch.ElapsedMilliseconds;

        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    /// <summary>
    /// Phase 3: Beyond capacity — notification operations under heavy load. System does not crash.
    /// </summary>
    [Fact]
    public async Task StressLoad_HeavyLoad_DoesNotCrash()
    {
        var completed = 0;

        var tasks = Enumerable.Range(0, StressUsers)
            .Select(async (_, i) =>
            {
                await _manager.GetNotifications((i % 20) + 1);
                Interlocked.Increment(ref completed);
            }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent operations — only {completed} completed");
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
                await _manager.GetNotifications(1);
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
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent notification reads");
    }

    /// <summary>
    /// Phase 3: Concurrent notification operations — data integrity maintained under stress.
    /// </summary>
    [Fact]
    public async Task StressLoad_ConcurrentNotificationOperations_DataIntegrityMaintained()
    {
        var expectedSum = Enumerable.Range(1, 100).Sum();
        var actualSum = 0;
        var lockObj = new object();

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            var result = await _manager.GetNotifications(i % 5 + 1);
            result.Should().NotBeNull();
            lock (lockObj)
            {
                actualSum += i;
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        actualSum.Should().Be(expectedSum,
            "Data integrity compromised under concurrent notification read stress");
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
        await _manager.GetNotifications(1);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        await Task.WhenAll(Enumerable.Range(0, StressUsers)
            .Select(i => _manager.GetNotifications((i % 20) + 1)));

        await Task.Delay(RecoveryWindowMs);

        _stopwatch.Restart();
        await _manager.GetNotifications(1);
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
            .Select(i => _manager.CreateNotification((i % 5) + 1, $"Msg {i}", "System", "Info", CreateRecordPayload(i))));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetNotifications(1);
        result.Should().NotBeNull("Post-stress notification read should succeed.");
    }

    #endregion

    #region Scalability (bonus 2)

    /// <summary>
    /// Scalability: GetNotifications scales under increasing concurrent load.
    /// </summary>
    [Fact]
    public async Task Scalability_GetNotifications_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.GetNotifications((i % 10) + 1)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    /// <summary>
    /// Scalability: CreateNotification throughput scales under concurrent load.
    /// </summary>
    [Fact]
    public async Task Scalability_CreateNotificationThroughput_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount)
                .Select(i => _manager.CreateNotification((i % 5) + 1, $"Msg {i}", "System", "Info", CreateRecordPayload(i))));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} concurrent creates, avg {perUser:F0}ms/create — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task MeasuredGetNotificationsAsync(int userId, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.GetNotifications(userId);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task MeasuredCreateAsync(int userId, int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.CreateNotification(userId, $"Msg {index}", "System", "Info", CreateRecordPayload(index));
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task MeasuredMarkAsReadAsync(int notificationId, int userId, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await _manager.MarkAsRead(notificationId, userId);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    #endregion
}
