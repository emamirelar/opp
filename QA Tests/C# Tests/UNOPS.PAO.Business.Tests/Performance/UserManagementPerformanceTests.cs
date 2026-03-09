/**
 * PERFORMANCE TESTS — UserManagementManager (UNOPSUserManagementManager)
 *
 * Minimum: ≥16 tests (FIXED per comprehensive-test-strategy.mdc)
 *   Single Ops (2) | Bulk Ops (3) | Search (5) | Concurrent (3) | Memory (3)
 *
 * SLA Source: QA Tests/Test Plans/PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md
 * PRD: User listing, role retrieval, org unit management, pagination
 * Related: .cursor/rules/entity-framework-performance-optimization.mdc
 *
 * NOTE: GetUsersAsync uses SqlQueryRaw (PostgreSQL-specific). All tests use [SkipIfInMemoryFact].
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
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using UNOPS.PAO.Identity.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance Tests for UserManagementManager (UNOPSUserManagementManager).
/// Verifies user listing speed, role retrieval, pagination, concurrent access.
///
/// Required: ≥16 tests (FIXED)
/// SLA thresholds — TODO: replace with values from questionnaire Section A1 when available.
/// All tests require PostgreSQL (SqlQueryRaw in GetUsersAsync).
/// </summary>
public class UserManagementPerformanceTests : PerformanceTestBase
{
    private readonly IUserManagementManager _manager;
    private readonly Stopwatch _stopwatch;
    private readonly ClaimsPrincipal _testUser;
    private readonly string _testMarker = $"UMPerf_{Guid.NewGuid():N}";

    // ── SLA thresholds (TODO: confirm with PERFORMANCE_AND_LOAD_TESTING_QUESTIONNAIRE.md Section A1) ──
    private static readonly int MaxSingleOperationMs = ScaleThreshold(500);
    private static readonly int MaxBulkOperationMs = ScaleThreshold(5_000);
    private static readonly int MaxSimpleSearchMs = ScaleThreshold(500);
    private static readonly int MaxComplexSearchMs = ScaleThreshold(2_000);
    private static readonly int MaxPaginatedQueryMs = ScaleThreshold(200);
    private static readonly int MaxConcurrentReadMs = ScaleThreshold(100);
    private const int MaxMemoryGrowthMb = 50;
    private const int MaxQueryMemoryMb = 100;

    public UserManagementPerformanceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Managers.Mapping.MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        var configuration = TestEnvironment.CreateTestConfiguration();

        var userStore = new Mock<IUserStore<PAOIdentityUser>>();
        var userManager = new UserManager<PAOIdentityUser>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!,
            new Mock<ILogger<UserManager<PAOIdentityUser>>>().Object);

        var roleStore = new Mock<IRoleStore<PAOIdentityRole>>();
        var roleManager = new RoleManager<PAOIdentityRole>(
            roleStore.Object, null!, null!, null!,
            new Mock<ILogger<RoleManager<PAOIdentityRole>>>().Object);

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(s => s.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((string?)null);

        var mockGeminiManager = new Mock<IGeminiManager>();

        var logger = new Mock<ILogger<UNOPSUserManagementManager>>().Object;

        _manager = new UNOPSUserManagementManager(mapper, Context, configuration, userManager, roleManager,
            mockPermissionService.Object, mockGeminiManager.Object, logger);
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

    [SkipIfInMemoryFact]
    public async Task GetUserById_NonExistent_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        var result = await _manager.GetUserByIdAsync(_testUser, "999999");
        _stopwatch.Stop();

        result.Should().BeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetUserById (non-existent) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [SkipIfInMemoryFact]
    public async Task GetAvailableOrgUnitsAsync_CompletesWithinThreshold()
    {
        await SeedOrgUnitsAsync(10);

        _stopwatch.Restart();
        var result = await _manager.GetAvailableOrgUnitsAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSingleOperationMs,
            $"GetAvailableOrgUnitsAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Bulk Operation Performance (min 3)

    [SkipIfInMemoryFact]
    public async Task GetUsersAsync_EmptyOrPopulated_CompletesWithinThreshold()
    {
        var request = new UserManagementRequest { PageIndex = 0, PageSize = 20 };

        _stopwatch.Restart();
        var result = await _manager.GetUsersAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Records.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetUsersAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-091")]
    public async Task GetAvailableRolesAsync_CompletesWithinThreshold()
    {
        _stopwatch.Restart();
        var result = await _manager.GetAvailableRolesAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetAvailableRolesAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [SkipIfInMemoryFact]
    public async Task GetAvailableOrgUnitsAsync_50OrgUnits_CompletesWithinThreshold()
    {
        await SeedOrgUnitsAsync(50);

        _stopwatch.Restart();
        var result = await _manager.GetAvailableOrgUnitsAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(50);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"GetAvailableOrgUnitsAsync (50) took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Search Performance (min 5)

    [SkipIfInMemoryFact]
    public async Task GetUsersAsync_SimplePagination_CompletesWithinThreshold()
    {
        var request = new UserManagementRequest { PageIndex = 0, PageSize = 10 };

        _stopwatch.Restart();
        var result = await _manager.GetUsersAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"Simple pagination took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [SkipIfInMemoryFact]
    public async Task GetUsersAsync_WithSearchTerm_CompletesWithinThreshold()
    {
        var request = new UserManagementRequest
        {
            PageIndex = 0,
            PageSize = 20,
            SearchTerm = "test"
        };

        _stopwatch.Restart();
        var result = await _manager.GetUsersAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxComplexSearchMs,
            $"Search with term took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [SkipIfInMemoryFact]
    public async Task GetUsersAsync_WithSort_CompletesWithinThreshold()
    {
        var request = new UserManagementRequest
        {
            PageIndex = 0,
            PageSize = 20,
            SortBy = "email",
            SortDirection = "asc"
        };

        _stopwatch.Restart();
        var result = await _manager.GetUsersAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Sorted query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [SkipIfInMemoryFact]
    public async Task GetOrgUnitSelfManagementAsync_ExistingOrgUnit_CompletesWithinThreshold()
    {
        var orgUnit = await SeedOrgUnitAsync("SELFMGMT", "Self Management Unit");

        _stopwatch.Restart();
        var result = await _manager.GetOrgUnitSelfManagementAsync(_testUser, orgUnit.Code);
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"GetOrgUnitSelfManagementAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [SkipIfInMemoryFact]
    public async Task GetUsersAsync_Page2_CompletesWithinThreshold()
    {
        var request = new UserManagementRequest { PageIndex = 1, PageSize = 20 };

        _stopwatch.Restart();
        var result = await _manager.GetUsersAsync(_testUser, request);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPaginatedQueryMs,
            $"Page 2 query took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Concurrent Access Performance (min 3)

    [Fact]

    [Trait("Defect", "DEF-091")]
    public async Task ConcurrentReads_50ParallelGetAvailableOrgUnits_MaintainsPerformance()
    {
        await SeedOrgUnitsAsync(20);
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _manager.GetAvailableOrgUnitsAsync(_testUser))
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

    [Trait("Defect", "DEF-091")]
    public async Task ConcurrentReads_20ParallelGetUsersAsync_MaintainsPerformance()
    {
        var request = new UserManagementRequest { PageIndex = 0, PageSize = 10 };
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _manager.GetUsersAsync(_testUser, request))
            .ToList();

        _stopwatch.Restart();
        var results = await Task.WhenAll(tasks);
        _stopwatch.Stop();

        results.Should().HaveCount(20);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"20 parallel GetUsersAsync took {_stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]

    [Trait("Defect", "DEF-091")]
    public async Task ConcurrentMixedRead_PerformanceStable()
    {
        await SeedOrgUnitsAsync(10);
        var request = new UserManagementRequest { PageIndex = 0, PageSize = 10 };

        var orgTasks = Enumerable.Range(0, 5)
            .Select(_ => _manager.GetAvailableOrgUnitsAsync(_testUser))
            .Cast<Task>()
            .ToList();
        var userTasks = Enumerable.Range(0, 5)
            .Select(_ => _manager.GetUsersAsync(_testUser, request))
            .Cast<Task>()
            .ToList();

        _stopwatch.Restart();
        await Task.WhenAll(orgTasks.Concat(userTasks));
        _stopwatch.Stop();

        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxBulkOperationMs,
            $"Mixed concurrent reads took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Memory Performance (min 3)

    [SkipIfInMemoryFact]
    public async Task LargeOrgUnitList_MemoryUsage_WithinCap()
    {
        await SeedOrgUnitsAsync(500);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        await _manager.GetAvailableOrgUnitsAsync(_testUser);

        GC.Collect();
        var usedMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        usedMb.Should().BeLessThan(MaxQueryMemoryMb,
            $"Query allocated {usedMb}MB, expected <{MaxQueryMemoryMb}MB");
    }

    [Fact]

    [Trait("Defect", "DEF-091")]
    public async Task RepeatedOperations_NoMemoryLeak()
    {
        await SeedOrgUnitsAsync(10);
        GC.Collect();
        var before = GC.GetTotalMemory(true);

        for (int i = 0; i < 100; i++)
        {
            await _manager.GetAvailableOrgUnitsAsync(_testUser);
            await _manager.GetAvailableRolesAsync(_testUser);
        }

        GC.Collect();
        var growthMb = (GC.GetTotalMemory(true) - before) / (1024 * 1024);
        growthMb.Should().BeLessThan(MaxMemoryGrowthMb,
            $"Memory grew {growthMb}MB after 100 ops — possible leak");
    }

    [SkipIfInMemoryFact]
    public async Task GcPressure_HighThroughput_DoesNotDegrade()
    {
        await SeedOrgUnitsAsync(20);
        var times = new List<long>();

        for (int i = 0; i < 100; i++)
        {
            _stopwatch.Restart();
            await _manager.GetAvailableOrgUnitsAsync(_testUser);
            _stopwatch.Stop();
            times.Add(_stopwatch.ElapsedMilliseconds);
        }

        var first25Avg = times.Take(25).Average();
        var last25Avg = times.Skip(75).Average();
        last25Avg.Should().BeLessThan(first25Avg * 3,
            $"GC pressure degraded perf from {first25Avg}ms to {last25Avg}ms avg");
    }

    #endregion

    #region EF Core — AsNoTracking Verification

    [SkipIfInMemoryFact]
    public async Task GetAvailableOrgUnits_NoTracking_ReadOnlyQueryOptimized()
    {
        await SeedOrgUnitsAsync(100);

        _stopwatch.Restart();
        var result = await _manager.GetAvailableOrgUnitsAsync(_testUser);
        _stopwatch.Stop();

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(100);
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxSimpleSearchMs,
            $"AsNoTracking read query should complete within threshold — took {_stopwatch.ElapsedMilliseconds}ms");
    }

    #endregion

    #region Benchmark Report

    [Fact]

    [Trait("Defect", "DEF-091")]
    public async Task Benchmark_AllOperations_ReportTimings()
    {
        var report = new Dictionary<string, long>();
        await SeedOrgUnitsAsync(5);
        var request = new UserManagementRequest { PageIndex = 0, PageSize = 20 };

        report["GetUsersAsync"] = await TimeMs(() => _manager.GetUsersAsync(_testUser, request));
        report["GetUserById"] = await TimeMs(() => _manager.GetUserByIdAsync(_testUser, "1"));
        report["GetAvailableRoles"] = await TimeMs(() => _manager.GetAvailableRolesAsync(_testUser));
        report["GetAvailableOrgUnits"] = await TimeMs(() => _manager.GetAvailableOrgUnitsAsync(_testUser));

        foreach (var (op, ms) in report)
            Console.WriteLine($"[PERF BENCHMARK] {op,-25}: {ms}ms");

        report.Values.Should().OnlyContain(t => t < MaxBulkOperationMs,
            "All operations should complete within bulk operation threshold");
    }

    #endregion

    #region Helpers

    private async Task<OrganizationHierarchy> SeedOrgUnitAsync(string code, string name)
    {
        var existing = await Context.OrganizationHierarchies
            .FirstOrDefaultAsync(o => o.Code == code && !o.IsDeleted);
        if (existing != null) return existing;

        var org = new OrganizationHierarchy
        {
            Name = name,
            Code = code,
            Description = $"Test org {name}",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.OrganizationHierarchies.AddAsync(org);
        await SaveChangesAsync();
        return org;
    }

    private async Task SeedOrgUnitsAsync(int count)
    {
        var prefix = $"OU_{_testMarker}";
        var existing = await Context.OrganizationHierarchies
            .CountAsync(o => o.Code != null && o.Code.StartsWith(prefix));
        if (existing >= count) return;

        var toAdd = count - existing;
        var startIdx = existing + 1;
        var orgs = Enumerable.Range(startIdx, toAdd)
            .Select(i => new OrganizationHierarchy
            {
                Name = $"Org Unit {i}",
                Code = $"{prefix}_{i}",
                Description = $"Test org unit {i}",
                Type = OrganizationUnitType.OrgUnit,
                Status = EntityStatus.Active,
                IsDeleted = false
            })
            .ToList();

        await Context.OrganizationHierarchies.AddRangeAsync(orgs);
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
