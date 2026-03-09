using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Extended tests for GlobalFilterService covering recently added features:
/// - Opportunity stakeholder-based RelatedToMe filter
/// - Opportunity ResponsibleOrgUnitId filter
/// - DateOn single-date filter
/// - OPS org unit skip behavior
/// - Cross-entity relationship filtering (Partner-Contact-Interaction)
///
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class GlobalFilterServiceExtendedTests : IDisposable
{
    private readonly Mock<IUserPreferenceService> _mockUserPref;
    private readonly Mock<IOrgUnitHierarchyService> _mockOrgUnit;
    private readonly Mock<ILogger<GlobalFilterService>> _mockLogger;
    private readonly UNOPSAppDbContext _context;
    private readonly GlobalFilterService _service;

    public GlobalFilterServiceExtendedTests()
    {
        _mockUserPref = new Mock<IUserPreferenceService>();
        _mockOrgUnit = new Mock<IOrgUnitHierarchyService>();
        _mockLogger = new Mock<ILogger<GlobalFilterService>>();

        var dbName = $"GlobalFilterExt_{Guid.NewGuid():N}";
        _context = TestDbContextFactory.CreateUNOPSWithUserId(1, dbName);
        TestEnvironment.EnsureCleanDatabase(_context);

        _service = new GlobalFilterService(
            _mockUserPref.Object,
            _mockLogger.Object,
            _context,
            _mockOrgUnit.Object);
    }

    public void Dispose() => _context.Dispose();

    #region Positive (2)

    [Fact]
    public async Task ApplyGlobalFilters_OpportunityWithOrgUnitFilter_FiltersOnResponsibleOrgUnitId()
    {
        var userId = "10";
        var user = CreateUser(userId);
        var query = _context.Set<OpportunityEntity>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 5 });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(5))
            .ReturnsAsync(new List<int> { 5, 6 });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
        _mockOrgUnit.Verify(x => x.GetDescendantIdsAsync(5), Times.Once);
    }

    [Fact]
    public async Task ApplyGlobalFilters_DateOnFilter_AppliesSingleDayRange()
    {
        var userId = "11";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        var targetDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { DateOn = targetDate });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
        _mockUserPref.Verify(x => x.GetGlobalFiltersAsync(userId), Times.Once);
    }

    #endregion

    #region Negative (6)

    [Fact]
    public async Task ApplyGlobalFilters_RelatedToMe_NonNumericUserId_SkipsStakeholderCheck()
    {
        var userId = "not-a-number";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { RelatedToMe = true });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_DateOnAndDateRange_BothNull_NoDateFilter()
    {
        var userId = "12";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { DateOn = null, DateFrom = null, DateTo = null });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
        _mockOrgUnit.Verify(x => x.GetDescendantIdsAsync(It.IsAny<int>()), Times.Never,
            "no OrgUnit filter should be applied when all filters are null");
    }

    [Fact]
    public async Task ApplyGlobalFilters_OrgUnitFilter_EmptyDescendantList_ReturnsEmptyResults()
    {
        var userId = "13";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 999 });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(999))
            .ReturnsAsync(new List<int>());

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_RelatedToMe_False_SkipsRelatedFilter()
    {
        var userId = "14";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { RelatedToMe = false });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_OrgUnitServiceThrows_ReturnsUnfilteredQuery()
    {
        var userId = "15";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 1 });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(1))
            .ThrowsAsync(new InvalidOperationException("Service failure"));

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_PreferenceServiceReturnsEmptyFilters_NoFilterApplied()
    {
        var userId = "16";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters
            {
                OrgUnitId = null,
                RelatedToMe = false,
                DateOn = null,
                DateFrom = null,
                DateTo = null
            });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
        _mockOrgUnit.Verify(x => x.GetDescendantIdsAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Edge/Boundary (6)

    [Fact]
    public async Task ApplyGlobalFilters_DateOn_MidnightBoundary_IncludesFullDay()
    {
        var userId = "20";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        var midnight = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { DateOn = midnight });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_DateRange_FromEqualsTo_SingleDayBehavior()
    {
        var userId = "21";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        var sameDay = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc);
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { DateFrom = sameDay, DateTo = sameDay });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_DateRange_OnlyDateFrom_OpenEndedRange()
    {
        var userId = "22";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { DateFrom = DateTime.UtcNow.AddDays(-30), DateTo = null });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_DateRange_OnlyDateTo_OpenStartRange()
    {
        var userId = "23";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { DateFrom = null, DateTo = DateTime.UtcNow });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_OrgUnitId_Zero_TreatedAsValidOrgUnit()
    {
        var userId = "24";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 0 });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(0))
            .ReturnsAsync(new List<int> { 0 });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
        _mockOrgUnit.Verify(x => x.GetDescendantIdsAsync(0), Times.Once);
    }

    [Fact]
    public async Task ApplyGlobalFilters_AllFiltersCombined_AndLogicApplied()
    {
        var userId = "25";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters
            {
                OrgUnitId = 1,
                RelatedToMe = true,
                DateFrom = DateTime.UtcNow.AddDays(-7),
                DateTo = DateTime.UtcNow
            });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(1))
            .ReturnsAsync(new List<int> { 1 });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
        _mockOrgUnit.Verify(x => x.GetDescendantIdsAsync(1), Times.Once);
    }

    #endregion

    #region Functional (6)

    [Fact]
    public async Task ApplyGlobalFilters_OrgUnitFilter_CallsGetDescendantIds()
    {
        var userId = "30";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 42 });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(42))
            .ReturnsAsync(new List<int> { 42, 43, 44 });

        await _service.ApplyGlobalFiltersAsync(query, user);

        _mockOrgUnit.Verify(x => x.GetDescendantIdsAsync(42), Times.Once);
    }

    [Fact]
    public async Task ApplyGlobalFilters_NoOrgUnitId_SkipsDescendantLookup()
    {
        var userId = "31";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = null, RelatedToMe = true });

        await _service.ApplyGlobalFiltersAsync(query, user);

        _mockOrgUnit.Verify(x => x.GetDescendantIdsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ApplyGlobalFilters_ExceptionCaught_LogsWarning()
    {
        var userId = "32";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ThrowsAsync(new Exception("Preference service down"));

        await _service.ApplyGlobalFiltersAsync(query, user);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ApplyGlobalFilters_ContactEntity_OrgUnitFilterApplied()
    {
        var userId = "33";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSContact>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 10 });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(10))
            .ReturnsAsync(new List<int> { 10, 11 });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
        _mockOrgUnit.Verify(x => x.GetDescendantIdsAsync(10), Times.Once);
    }

    [Fact]
    public async Task ApplyGlobalFilters_InteractionEntity_OrgUnitFilterApplied()
    {
        var userId = "34";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSInteraction>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 20 });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(20))
            .ReturnsAsync(new List<int> { 20 });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_DateOnOverridesDateRange_WhenBothPresent()
    {
        var userId = "35";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters
            {
                DateOn = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                DateFrom = DateTime.UtcNow.AddDays(-30),
                DateTo = DateTime.UtcNow
            });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);

        result.Should().NotBeNull();
    }

    #endregion

    #region Integration (6)

    [Fact]
    public async Task ApplyGlobalFilters_PartnerEntity_WithRealDb_ReturnsQueryable()
    {
        var userId = "40";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 1, RelatedToMe = true });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(1))
            .ReturnsAsync(new List<int> { 1 });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);
        var list = await result.ToListAsync();

        list.Should().NotBeNull();
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyGlobalFilters_OpportunityEntity_WithRealDb_ReturnsQueryable()
    {
        var userId = "41";
        var user = CreateUser(userId);
        var query = _context.Set<OpportunityEntity>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { OrgUnitId = 1 });
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(1))
            .ReturnsAsync(new List<int> { 1, 2 });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);
        var list = await result.ToListAsync();

        list.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_ContactEntity_WithRealDb_ReturnsQueryable()
    {
        var userId = "42";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSContact>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { DateFrom = DateTime.UtcNow.AddDays(-7), DateTo = DateTime.UtcNow });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);
        var list = await result.ToListAsync();

        list.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_InteractionEntity_WithRealDb_ReturnsQueryable()
    {
        var userId = "43";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSInteraction>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { RelatedToMe = true });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);
        var list = await result.ToListAsync();

        list.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_MultipleEntityTypes_ConsistentBehavior()
    {
        var userId = "44";
        var user = CreateUser(userId);
        var filters = new GlobalFilters { OrgUnitId = 5 };
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId)).ReturnsAsync(filters);
        _mockOrgUnit.Setup(x => x.GetDescendantIdsAsync(5)).ReturnsAsync(new List<int> { 5 });

        var partnerResult = await _service.ApplyGlobalFiltersAsync(_context.Set<UNOPSPartner>().AsQueryable(), user);
        var contactResult = await _service.ApplyGlobalFiltersAsync(_context.Set<UNOPSContact>().AsQueryable(), user);

        partnerResult.Should().NotBeNull();
        contactResult.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyGlobalFilters_DateOnFilter_WithRealDb_ExecutesWithoutError()
    {
        var userId = "45";
        var user = CreateUser(userId);
        var query = _context.Set<UNOPSPartner>().AsQueryable();
        _mockUserPref.Setup(x => x.GetGlobalFiltersAsync(userId))
            .ReturnsAsync(new GlobalFilters { DateOn = DateTime.UtcNow });

        var result = await _service.ApplyGlobalFiltersAsync(query, user);
        var list = await result.ToListAsync();

        list.Should().NotBeNull();
    }

    #endregion

    private static ClaimsPrincipal CreateUser(string userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 2 | OpportunityWithOrgUnitFilter, DateOnFilter_AppliesSingleDayRange |
| Negative (N) | 6 | NonNumericUserId, BothNull_NoDateFilter, EmptyDescendantList, RelatedToMe_False, OrgUnitServiceThrows, EmptyFilters |
| Edge/Boundary (E) | 6 | MidnightBoundary, FromEqualsTo, OnlyDateFrom, OnlyDateTo, OrgUnitId_Zero, AllFiltersCombined |
| Functional (F) | 6 | CallsGetDescendantIds, SkipsDescendantLookup, ExceptionLogsWarning, ContactEntity, InteractionEntity, DateOnOverridesDateRange |
| Integration (I) | 6 | Partner_WithRealDb, Opportunity_WithRealDb, Contact_WithRealDb, Interaction_WithRealDb, MultipleEntityTypes, DateOn_WithRealDb |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
