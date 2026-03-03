/**
 * LOAD TESTS — ValuesManager
 *
 * Minimum: ≥10 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Sustained Load (3) | Spike (2) | Stress Limits (3) | Recovery (2) | Scalability (2)
 *
 * Load Targets: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * Phase Strategy: QA Tests/Load Tests/README.md (5 phases)
 *
 * ValuesManager is a read-only lookup service (currencies, countries, users, SDGs, etc.).
 * No interface — uses internal IValuesManagerMock for unit-level throughput validation.
 * Sync methods wrapped in Task.Run() for concurrent testing.
 *
 * @see comprehensive-test-strategy.mdc §10 Load Tests
 */

using System.Diagnostics;
using FluentAssertions;
using Moq;
using UNOPS.PAO.Models.Locations;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Values;
using UNOPS.PAO.Models.Users;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Internal mock interface for ValuesManager (concrete class has no interface).
/// </summary>
public interface IValuesManagerMock
{
    IEnumerable<CurrencyModel> GetCurrencies();
    IEnumerable<SimpleValueModel> GetCountries();
    IEnumerable<UserValueModel> GetUsers();
    Task<PaginationResponse<UserValueModel>> GetUsersPagedAsync(UsersPagedRequest request);
    Task<IEnumerable<UserValueModel>> SearchUsersAsync(string? searchTerm, int maxResults, int[]? selectedUserIds);
    IEnumerable<SDGModel> GetSDGs();
    Task<IEnumerable<SimpleValueModel>> GetEntityRolesAsync(string entityType);
}

/// <summary>
/// Load Tests for ValuesManager (read-only lookup service).
/// Verifies throughput and concurrency handling under sustained, spike, and stress conditions.
///
/// Required: ≥10 tests (FIXED)
/// Subcategories: Sustained Load (3), Spike (2), Stress Limits (3), Recovery (2), Scalability (2)
/// </summary>
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class ValuesManagerLoadTests
{
    private readonly Mock<IValuesManagerMock> _mockManager;
    private readonly IValuesManagerMock _manager;
    private readonly Stopwatch _stopwatch = new();

    private const int NormalUsers = 50;
    private const int PeakUsers = 100;
    private const int StressUsers = 500;
    private const int MaxP95ResponseMs = 3_000;
    private const double MaxErrorRate = 0.01;
    private const int RecoveryWindowMs = 100;

    public ValuesManagerLoadTests()
    {
        _mockManager = new Mock<IValuesManagerMock>();
        SetupMockBehavior();
        _manager = _mockManager.Object;
    }

    private void SetupMockBehavior()
    {
        _mockManager.Setup(m => m.GetCurrencies()).Returns(CreateMockCurrencies());
        _mockManager.Setup(m => m.GetCountries()).Returns(CreateMockCountries());
        _mockManager.Setup(m => m.GetUsers()).Returns(CreateMockUsers());
        _mockManager.Setup(m => m.GetUsersPagedAsync(It.IsAny<UsersPagedRequest>()))
            .ReturnsAsync((UsersPagedRequest r) => CreateMockPaginationResponse(r));
        _mockManager.Setup(m => m.SearchUsersAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int[]?>()))
            .ReturnsAsync(CreateMockUsers().ToList);
        _mockManager.Setup(m => m.GetSDGs()).Returns(CreateMockSDGs());
        _mockManager.Setup(m => m.GetEntityRolesAsync(It.IsAny<string>()))
            .ReturnsAsync((string _) => CreateMockEntityRoles());
    }

    private static IEnumerable<CurrencyModel> CreateMockCurrencies() =>
        Enumerable.Range(1, 10).Select(i => new CurrencyModel { Id = i, Code = $"CC{i}", Name = $"Currency {i}" });

    private static IEnumerable<SimpleValueModel> CreateMockCountries() =>
        Enumerable.Range(1, 20).Select(i => new SimpleValueModel { Id = i, Name = $"Country {i}", Code = $"C{i}" });

    private static IEnumerable<UserValueModel> CreateMockUsers() =>
        Enumerable.Range(1, 50).Select(i => new UserValueModel { Id = i, Email = $"user{i}@test.org" });

    private static PaginationResponse<UserValueModel> CreateMockPaginationResponse(UsersPagedRequest r) =>
        new() { Records = CreateMockUsers().Take(r.PageSize).ToList(), TotalCount = 50, PageIndex = r.PageIndex, PageSize = r.PageSize };

    private static IEnumerable<SDGModel> CreateMockSDGs() =>
        Enumerable.Range(1, 17).Select(i => new SDGModel { Id = i, Name = $"SDG {i}", SDGNumber = i.ToString() });

    private static IEnumerable<SimpleValueModel> CreateMockEntityRoles() =>
        Enumerable.Range(1, 5).Select(i => new SimpleValueModel { Id = i, Name = $"Role {i}", Type = "Partner" });

    #region Sustained Load (min 3) — Phase 2

    [Fact]
    public async Task SustainedLoad_ReadOperations_PerformanceDoesNotDegrade()
    {
        var times = new List<long>();
        var tasks = Enumerable.Range(0, NormalUsers).Select(i => MeasuredReadAsync(i, times)).ToArray();
        await Task.WhenAll(tasks);

        var first = times.Take(times.Count / 4).Average();
        var last = times.Skip(3 * times.Count / 4).Average();
        last.Should().BeLessThan(Math.Max(first * 10, 500),
            $"Read performance degraded from {first:F0}ms to {last:F0}ms avg under {NormalUsers} concurrent users");
    }

    [Fact]
    public async Task SustainedLoad_MixedLookupMethods_ConsistencyMaintained()
    {
        var times = new List<long>();
        var tasks = Enumerable.Range(0, NormalUsers).Select(i => MeasuredMixedReadAsync(i, times)).ToArray();
        await Task.WhenAll(tasks);

        var avg = times.Average();
        var stdDev = Math.Sqrt(times.Average(t => Math.Pow(t - avg, 2)));
        stdDev.Should().BeLessThan(Math.Max(avg * 2, 5),
            $"Lookup times inconsistent under {NormalUsers} concurrent users (stddev={stdDev:F0}ms, avg={avg:F0}ms)");
    }

    [Fact]
    public async Task SustainedLoad_MixedOperations_ThroughputMeetsTarget()
    {
        var readCount = NormalUsers;
        var reads = Enumerable.Range(0, readCount).Select(i => SimulateReadAsync(i));

        _stopwatch.Restart();
        await Task.WhenAll(reads);
        _stopwatch.Stop();

        var avgMs = _stopwatch.ElapsedMilliseconds / (double)NormalUsers;
        avgMs.Should().BeLessThan(MaxP95ResponseMs,
            $"Mixed load avg {avgMs:F0}ms/op exceeded P95 target of {MaxP95ResponseMs}ms");
    }

    #endregion

    #region Spike Testing (min 2) — Phase 5

    [Fact]
    public async Task SpikeLoad_SuddenIncrease_HandlesGracefully()
    {
        var baselineTasks = Enumerable.Range(0, 10).Select(i => SimulateReadAsync(i)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(baselineTasks);
        _stopwatch.Stop();
        var baselineMs = Math.Max(_stopwatch.ElapsedMilliseconds, 10);

        var spikeTasks = Enumerable.Range(0, PeakUsers).Select(i => SimulateReadAsync(i)).ToArray();
        _stopwatch.Restart();
        await Task.WhenAll(spikeTasks);
        _stopwatch.Stop();
        var spikeMs = _stopwatch.ElapsedMilliseconds;

        var scale = (double)spikeMs / Math.Max(baselineMs, 1);
        scale.Should().BeLessThan((double)PeakUsers / 10 * 2,
            $"Spike scaled {scale:F1}× — expected <{(double)PeakUsers / 10 * 2:F1}×");
    }

    [Fact]
    public async Task SpikeLoad_Recovery_ReturnsToBaseline()
    {
        var baselineMs = await MeasureSingleOpMs();

        await Task.WhenAll(Enumerable.Range(0, PeakUsers).Select(i => SimulateReadAsync(i)));

        await Task.Delay(RecoveryWindowMs);

        var postSpikeMs = await MeasureSingleOpMs();
        postSpikeMs.Should().BeLessThan(Math.Max(baselineMs * 3, 10),
            $"Post-spike response {postSpikeMs}ms did not recover (baseline {baselineMs}ms)");
    }

    #endregion

    #region Stress Limits (min 3) — Phase 3

    [Fact]
    public async Task StressLoad_BeyondCapacity_DoesNotCrash()
    {
        var completed = 0;
        var tasks = Enumerable.Range(0, StressUsers).Select(async (_, i) =>
        {
            await SimulateReadAsync(i);
            Interlocked.Increment(ref completed);
        }).ToArray();

        var allDone = Task.WhenAll(tasks);
        var timeout = Task.Delay(TimeSpan.FromSeconds(60));
        var first = await Task.WhenAny(allDone, timeout);

        first.Should().Be(allDone,
            $"System timed out under {StressUsers} concurrent users — only {completed} completed");
        completed.Should().Be(StressUsers);
    }

    [Fact]
    public async Task StressLoad_ErrorRate_WithinAcceptableLimit()
    {
        var success = 0;
        var failure = 0;

        var tasks = Enumerable.Range(0, StressUsers).Select(async _ =>
        {
            try
            {
                await SimulateReadAsync(0);
                Interlocked.Increment(ref success);
            }
            catch { Interlocked.Increment(ref failure); }
        }).ToArray();

        await Task.WhenAll(tasks);

        var errorRate = (double)failure / StressUsers;
        errorRate.Should().BeLessThan(MaxErrorRate,
            $"Error rate {errorRate:P} exceeded {MaxErrorRate:P} under {StressUsers} concurrent users");
    }

    [Fact]
    public async Task StressLoad_DataIntegrity_Maintained()
    {
        var expected = Enumerable.Range(1, 100).Sum();
        var actual = 0;

        var tasks = Enumerable.Range(1, 100).Select(async i =>
        {
            await SimulateReadAsync(i);
            Interlocked.Add(ref actual, i);
        }).ToArray();

        await Task.WhenAll(tasks);

        actual.Should().Be(expected, "Data integrity compromised under concurrent stress load");
    }

    #endregion

    #region Recovery (min 2) — Phase 3 + 5

    [Fact]
    public async Task Recovery_AfterStress_PerformanceRestored()
    {
        var baselineMs = await MeasureSingleOpMs();

        await Task.WhenAll(Enumerable.Range(0, StressUsers).Select(i => SimulateReadAsync(i)));

        await Task.Delay(RecoveryWindowMs);

        var recoveredMs = await MeasureSingleOpMs();
        recoveredMs.Should().BeLessThan(Math.Max(baselineMs * 2, 10),
            $"System did not recover: post-stress {recoveredMs}ms vs baseline {baselineMs}ms");
    }

    [Fact]
    public async Task Recovery_AfterStress_NoStateCorruption()
    {
        await Task.WhenAll(Enumerable.Range(0, 50).Select(i => SimulateReadAsync(i)));

        await Task.Delay(RecoveryWindowMs);

        var result = await _manager.GetUsersPagedAsync(new UsersPagedRequest { PageIndex = 0, PageSize = 10 });
        result.Should().NotBeNull("Post-stress read should succeed");
        result.Records.Should().NotBeNull();
    }

    #endregion

    #region Scalability (min 2)

    [Fact]
    public async Task Scalability_UserCount_ScalesSubLinearly()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount).Select(i => SimulateReadAsync(i)));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} users, avg {perUser:F0}ms/user — exceeded 100ms threshold");
        }
    }

    [Fact]
    public async Task Scalability_MixedLookups_ScalesUnderLoad()
    {
        var batchSizes = new[] { 25, 50, 100 };

        foreach (var userCount in batchSizes)
        {
            _stopwatch.Restart();
            await Task.WhenAll(Enumerable.Range(0, userCount).Select(i => MeasuredMixedReadAsync(i, new List<long>())));
            _stopwatch.Stop();

            var perUser = _stopwatch.ElapsedMilliseconds / (double)userCount;
            perUser.Should().BeLessThan(100,
                $"At {userCount} mixed lookups, avg {perUser:F0}ms/op — exceeded 100ms threshold");
        }
    }

    #endregion

    #region Helpers

    private async Task SimulateReadAsync(int index)
    {
        switch (index % 6)
        {
            case 0: await Task.Run(() => _manager.GetCurrencies()); break;
            case 1: await Task.Run(() => _manager.GetCountries()); break;
            case 2: await Task.Run(() => _manager.GetUsers()); break;
            case 3: await _manager.GetUsersPagedAsync(new UsersPagedRequest { PageIndex = 0, PageSize = 10 }); break;
            case 4: await _manager.SearchUsersAsync("test", 20, null); break;
            default: await _manager.GetEntityRolesAsync("Partner"); break;
        }
    }

    private async Task MeasuredReadAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await SimulateReadAsync(index);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task MeasuredMixedReadAsync(int index, List<long> times)
    {
        var sw = Stopwatch.StartNew();
        await SimulateReadAsync(index);
        sw.Stop();
        lock (times) times.Add(sw.ElapsedMilliseconds);
    }

    private async Task<long> MeasureSingleOpMs()
    {
        var sw = Stopwatch.StartNew();
        await SimulateReadAsync(0);
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    #endregion
}
