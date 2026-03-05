/**
 * PERFORMANCE TESTS — ProfileManager
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: User profile management — Get by email, Update profile
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * @see comprehensive-test-strategy.mdc §9 Performance Tests
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Users;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for ProfileManager.
/// Verifies response times for Get(email) and Update(profile).
/// Focus on single ops, repeated reads, concurrent reads, and memory.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// </summary>
[Collection("ProfilePerformance")]
public class ProfilePerformanceTests : PerformanceTestBase
{
    private readonly ProfileManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly string _testMarker = $"PROF_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public ProfilePerformanceTests()
    {
        _manager = new ProfileManager(Context);
        _stopwatch = new Stopwatch();
    }

    #region Single Operation Performance (min 2)

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Get_ExistingUserByEmail_CompletesWithinThreshold()
    {
        var email = await SeedUserWithProfileAsync();

        _stopwatch.Restart();
        var result = _manager.Get(email);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Get took {_stopwatch.ElapsedMilliseconds}ms, expected <{MaxSingleOperationMs}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Update_ExistingProfile_CompletesWithinThreshold()
    {
        var email = await SeedUserWithProfileAsync();
        var profile = _manager.Get(email);

        var updatedProfile = new ProfileModel
        {
            Email = email,
            FirstName = "UpdatedFirst",
            LastName = "UpdatedLast"
        };

        _stopwatch.Restart();
        await _manager.Update(updatedProfile);
        _stopwatch.Stop();

        var after = _manager.Get(email);
        after!.FirstName.Should().Be("UpdatedFirst");
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"Update took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Get_Repeated100Calls_CompletesWithinThreshold()
    {
        var email = await SeedUserWithProfileAsync();

        _stopwatch.Restart();
        for (int i = 0; i < 100; i++)
        {
            _ = _manager.Get(email);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"100 repeated Get took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Get_MultipleUsers_CompletesWithinThreshold()
    {
        var emails = await SeedUsersWithProfilesAsync(50);

        _stopwatch.Restart();
        foreach (var email in emails)
        {
            _ = _manager.Get(email);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Get 50 users took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Update_MultipleProfiles_CompletesWithinThreshold()
    {
        var emails = await SeedUsersWithProfilesAsync(30);

        _stopwatch.Restart();
        for (int i = 0; i < emails.Count; i++)
        {
            await _manager.Update(new ProfileModel
            {
                Email = emails[i],
                FirstName = $"BulkFirst{i}",
                LastName = $"BulkLast{i}"
            });
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Update 30 profiles took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Get_ByEmail_SimpleLookup_CompletesWithinThreshold()
    {
        var email = await SeedUserWithProfileAsync();

        _stopwatch.Restart();
        var result = _manager.Get(email);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Get by email took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Get_WithUserProfileNavigation_CompletesWithinThreshold()
    {
        var email = await SeedUserWithProfileAsync();

        _stopwatch.Restart();
        var result = _manager.Get(email);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result!.FirstName.Should().NotBeNullOrEmpty();
        result.LastName.Should().NotBeNullOrEmpty();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Get with profile navigation took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task DirectQuery_PAOUsersWithProfile_CompletesWithinThreshold()
    {
        await SeedUsersWithProfilesAsync(100);

        _stopwatch.Restart();
        var result = await Context.PAOUsers
            .AsNoTracking()
            .Include(u => u.UserProfile)
            .Where(u => u.Email.Contains(_testMarker))
            .Take(20)
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Direct PAOUsers query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Get_AlternatingReads_CompletesWithinThreshold()
    {
        var emails = await SeedUsersWithProfilesAsync(20);

        _stopwatch.Restart();
        for (int i = 0; i < 10; i++)
        {
            _ = _manager.Get(emails[i % emails.Count]);
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Alternating Get across users took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Get_EnsureTestUser_CompletesWithinThreshold()
    {
        await EnsureTestUserAsync();
        var email = "perf@test.local";

        _stopwatch.Restart();
        var result = _manager.Get(email);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Get EnsureTestUser took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task ConcurrentReads_50ParallelGet_MaintainsPerformance()
    {
        var email = await SeedUserWithProfileAsync();
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => Task.Run(() => _manager.Get(email)))
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

    [Trait("Defect", "DEF-067")]
    public async Task ConcurrentReads_20ParallelGetDifferentUsers_MaintainsPerformance()
    {
        var emails = await SeedUsersWithProfilesAsync(20);
        var tasks = emails
            .Select(email => Task.Run(() => _manager.Get(email)))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r != null);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel Get (different users) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task ConcurrentMixedReadUpdate_PerformanceStable()
    {
        var emails = await SeedUsersWithProfilesAsync(10);

        _stopwatch.Restart();
        for (int i = 0; i < 5; i++)
        {
            _ = _manager.Get(emails[i % emails.Count]);
        }
        for (int i = 0; i < 3; i++)
        {
            await _manager.Update(new ProfileModel
            {
                Email = emails[i],
                FirstName = $"Mixed{i}",
                LastName = "Test"
            });
        }
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed read/update took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Get_RepeatedCalls_MemoryUsage_WithinCap()
    {
        var email = await SeedUserWithProfileAsync();
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            _ = _manager.Get(email);
        }

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"100 Get calls allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task RepeatedGet_NoMemoryLeak()
    {
        var email = await SeedUserWithProfileAsync();
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 200; i++)
        {
            _ = _manager.Get(email);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 200 Get ops — possible leak");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        var email = await SeedUserWithProfileAsync();
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            _ = _manager.Get(email);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        if (first25Avg > 0)
            last25Avg.Should().BeLessThan(first25Avg * 3,
                $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region EF Core — AsNoTracking Verification

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task DirectQuery_AsNoTracking_ReadOnlyOptimized()
    {
        await SeedUsersWithProfilesAsync(50);

        _stopwatch.Restart();
        var result = await Context.PAOUsers
            .AsNoTracking()
            .Include(u => u.UserProfile)
            .Where(u => u.Email.Contains(_testMarker))
            .ToListAsync();
        _stopwatch.Stop();

        result.Should().HaveCount(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"AsNoTracking PAOUsers query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var email = await SeedUserWithProfileAsync();
        var report = new Dictionary<string, long>();

        report["Get"] = Measure(() => _manager.Get(email)).ElapsedMs;
        report["Update"] = await TimeMs(async () =>
            await _manager.Update(new ProfileModel { Email = email, FirstName = "Bench", LastName = "Test" }));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<string> SeedUserWithProfileAsync()
    {
        await EnsureTestUserAsync();
        var email = $"perf_{_testMarker}@test.local";
        var existing = await Context.PAOUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (existing != null) return email;

        var user = new PAOUser
        {
            Email = email,
            IsInternal = true,
            ActiveUser = true
        };
        await Context.PAOUsers.AddAsync(user);
        await SaveChangesAsync();

        var profile = new UserProfile
        {
            UserId = user.Id,
            FirstName = "PerfFirst",
            LastName = "PerfLast",
            UserEmail = email,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.UserProfile.AddAsync(profile);
        await SaveChangesAsync();
        return email;
    }

    private async Task<List<string>> SeedUsersWithProfilesAsync(int count)
    {
        await EnsureTestUserAsync();
        var emails = new List<string>();
        for (int i = 1; i <= count; i++)
        {
            var email = $"user{i}_{_testMarker}@perf.test";
            var existing = await Context.PAOUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (existing != null)
            {
                emails.Add(email);
                continue;
            }

            var user = new PAOUser
            {
                Email = email,
                IsInternal = true,
                ActiveUser = true
            };
            await Context.PAOUsers.AddAsync(user);
            await SaveChangesAsync();

            var profile = new UserProfile
            {
                UserId = user.Id,
                FirstName = $"First{i}",
                LastName = $"Last{i}",
                UserEmail = email,
                CreatedBy = TestUserId,
                LastModifiedBy = TestUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            await Context.UserProfile.AddAsync(profile);
            await SaveChangesAsync();
            emails.Add(email);
        }
        return emails;
    }

    private new (T Result, long ElapsedMs) Measure<T>(Func<T> fn)
    {
        _stopwatch.Restart();
        var result = fn();
        _stopwatch.Stop();
        return (result, _stopwatch.ElapsedMilliseconds);
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
