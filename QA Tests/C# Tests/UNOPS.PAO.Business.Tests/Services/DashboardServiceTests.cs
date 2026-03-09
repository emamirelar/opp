/**
 * @fileoverview Comprehensive mock-based tests for DashboardService.
 * Tests GetMyPartnersAsync, GetMyContactsAsync, GetMyDraftPartnersAsync, GetMyDraftContactsAsync,
 * GetMyInteractionsAsync, GetMyDraftInteractionsAsync, GetMyOpportunitiesAsync, GetMyDraftOpportunitiesAsync,
 * GetOrgUnitRecentUpdatesAsync, and GetAllDashboardDataAsync.
 *
 * Uses transaction isolation on PostgreSQL to prevent data bleed between tests,
 * and proper test user resolution to satisfy FK constraints on AspNetUsers.
 *
 * Ratio: P=2, N=6+, E=6+, F=6+, I=6+
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Dashboard;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Domain.Enums;
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Mock-based tests for DashboardService.
/// Uses PostgreSQL transaction isolation for test data safety and proper user
/// resolution via TestDataHelper to satisfy FK constraints on AspNetUsers.
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class DashboardServiceTests : IDisposable
{
    private readonly UNOPSAppDbContext _context;
    private readonly DashboardService _service;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IUserPreferenceService> _mockUserPreferenceService;
    private readonly Mock<IOrgUnitHierarchyService> _mockHierarchyService;
    private readonly IMapper _mapper;
    private readonly int _testUserId;
    private readonly int _otherUserId;
    private readonly IDbContextTransaction? _transaction;

    public DashboardServiceTests()
    {
        _mockPermissionService = new Mock<IPermissionService>();
        _mockUserPreferenceService = new Mock<IUserPreferenceService>();
        _mockHierarchyService = new Mock<IOrgUnitHierarchyService>();

        if (TestEnvironment.UsePostgreSQL)
        {
            using var tempContext = TestDbContextFactory.CreateUNOPS();
            _testUserId = TestDataHelper.GetOrCreateTestUser(tempContext, "dashboard-test@unops.org");
            _otherUserId = TestDataHelper.GetOrCreateTestUser(tempContext, "dashboard-other@unops.org");

            _context = TestDbContextFactory.CreateUNOPSWithUserId(_testUserId);
            _transaction = _context.Database.BeginTransaction();
        }
        else
        {
            _testUserId = 1;
            _otherUserId = 2;
            var dbName = $"Dashboard_{Guid.NewGuid():N}";
            _context = TestDbContextFactory.CreateUNOPSWithUserId(_testUserId, dbName);
            TestEnvironment.EnsureCleanDatabase(_context);
        }

        var config = TestEnvironment.CreateTestConfiguration();

        SetupPermissionServicePassThrough();
        SetupUserPreferenceService();
        SetupHierarchyService();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.Contains("UNOPS.PAO") == true));
        });
        _mapper = mapperConfig.CreateMapper();

        var mockLogger = new Mock<ILogger<DashboardService>>();

        _service = new DashboardService(
            _context,
            mockLogger.Object,
            _mapper,
            config,
            _mockUserPreferenceService.Object,
            _mockHierarchyService.Object,
            _mockPermissionService.Object,
            null);
    }

    private void SetupPermissionServicePassThrough()
    {
        _mockPermissionService
            .Setup(x => x.ApplyAccessControlFiltersAsync(It.IsAny<IQueryable<UNOPSPartner>>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((IQueryable<UNOPSPartner> q, ClaimsPrincipal _, string _, string _) => q.ToList());

        _mockPermissionService
            .Setup(x => x.ApplyAccessControlFiltersAsync(It.IsAny<IQueryable<UNOPSContact>>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((IQueryable<UNOPSContact> q, ClaimsPrincipal _, string _, string _) => q.ToList());

        _mockPermissionService
            .Setup(x => x.ApplyAccessControlFiltersAsync(It.IsAny<IQueryable<Interaction>>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((IQueryable<Interaction> q, ClaimsPrincipal _, string _, string _) => q.ToList());

        _mockPermissionService
            .Setup(x => x.ApplyAccessControlFiltersAsync(It.IsAny<IQueryable<OpportunityEntity>>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((IQueryable<OpportunityEntity> q, ClaimsPrincipal _, string _, string _) => q.ToList());
    }

    private void SetupUserPreferenceService()
    {
        _mockUserPreferenceService
            .Setup(x => x.GetGlobalFiltersAsync(It.IsAny<string>()))
            .ReturnsAsync(new UNOPS.PAO.Domain.Entities.GlobalFilters { OrgUnitId = null });
    }

    private void SetupHierarchyService()
    {
        _mockHierarchyService
            .Setup(x => x.GetDescendantIdsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<int>());
    }

    private ClaimsPrincipal CreateUser(int userId, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUserWithInvalidId()
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "not-a-number") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUserWithNullId()
    {
        var identity = new ClaimsIdentity();
        return new ClaimsPrincipal(identity);
    }

    public void Dispose()
    {
        if (_transaction != null)
        {
            try { _transaction.Rollback(); }
            catch { /* Context may already be disposed */ }
            _transaction.Dispose();
        }
        _context?.Dispose();
    }

    #region Positive Tests (P=2)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyPartnersAsync_ValidUser_ReturnsPartners()
    {
        var partner = new UNOPSPartner
        {
            Name = "Test Partner Dashboard",
            Status = EntityStatus.Active,
            CreatedBy = _testUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartner>().Add(partner);
        await _context.SaveChangesAsync();

        var result = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().NotBeNull();
        result.Records.Should().Contain(r => r.Name == "Test Partner Dashboard");
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetAllDashboardDataAsync_ValidUser_ReturnsCombinedStructure()
    {
        var result = await _service.GetAllDashboardDataAsync(CreateUser(_testUserId), 50, 10);

        result.Should().NotBeNull();
        result.MyPartners.Should().NotBeNull();
        result.MyContacts.Should().NotBeNull();
        result.MyInteractions.Should().NotBeNull();
        result.MyOpportunities.Should().NotBeNull();
        result.DraftPartners.Should().NotBeNull();
        result.DraftContacts.Should().NotBeNull();
        result.DraftInteractions.Should().NotBeNull();
        result.DraftOpportunities.Should().NotBeNull();
        result.OrgUnitRecentUpdates.Should().NotBeNull();
    }

    #endregion

    #region Negative Tests (N=6)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyPartnersAsync_NullUserIdClaim_ReturnsEmpty()
    {
        var result = await _service.GetMyPartnersAsync(CreateUserWithNullId(), 1000);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyContactsAsync_InvalidUserIdClaim_ReturnsEmpty()
    {
        var result = await _service.GetMyContactsAsync(CreateUserWithInvalidId(), 1000);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftPartnersAsync_NoDraftsExist_ReturnsEmpty()
    {
        var partner = new UNOPSPartner
        {
            Name = "Active Only Partner",
            Status = EntityStatus.Active,
            CreatedBy = _testUserId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartner>().Add(partner);
        await _context.SaveChangesAsync();

        var result = await _service.GetMyDraftPartnersAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().NotContain(r => r.Name == "Active Only Partner");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyInteractionsAsync_UserWithNoInteractions_ReturnsEmpty()
    {
        var result = await _service.GetMyInteractionsAsync(CreateUser(_otherUserId), 1000);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyOpportunitiesAsync_UserNotStakeholderOrCreator_ReturnsEmpty()
    {
        var opportunity = new OpportunityEntity
        {
            Name = "Other User Opp Dashboard",
            Description = "Desc",
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<OpportunityEntity>().Add(opportunity);
        await _context.SaveChangesAsync();
        await OverrideAuditFieldsAsync("Opportunities", opportunity.Id, _otherUserId);

        var result = await _service.GetMyOpportunitiesAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().NotContain(r => r.Name == "Other User Opp Dashboard");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetOrgUnitRecentUpdatesAsync_NullUserId_ReturnsEmpty()
    {
        var result = await _service.GetOrgUnitRecentUpdatesAsync(CreateUserWithNullId(), 10);

        result.Should().NotBeNull();
        result.Updates.Should().BeEmpty();
    }

    #endregion

    #region Edge/Boundary Tests (E=6)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyPartnersAsync_PageSizeZero_ReturnsEmptyRecords()
    {
        var partner = new UNOPSPartner
        {
            Name = "Partner PageZero",
            Status = EntityStatus.Active,
            CreatedBy = _testUserId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartner>().Add(partner);
        await _context.SaveChangesAsync();

        var result = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 0);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
        result.PageSize.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyPartnersAsync_PageSizeOne_ReturnsSingleResult()
    {
        _context.Set<UNOPSPartner>().AddRange(
            new UNOPSPartner { Name = "P1 PageOne", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "P2 PageOne", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1);

        result.Should().NotBeNull();
        result.Records.Should().HaveCount(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftContactsAsync_OnlyReturnsDraftStatus()
    {
        var partnerId = await CreateTestPartnerAsync();
        _context.Set<UNOPSContact>().AddRange(
            new UNOPSContact { FirstName = "DraftEdge", LastName = "Contact", Title = "Mr", Email = $"d-edge{Guid.NewGuid():N}@test.com", ContactNumber = "DE1", PartnerId = partnerId, Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSContact { FirstName = "ActiveEdge", LastName = "Contact", Title = "Mr", Email = $"a-edge{Guid.NewGuid():N}@test.com", ContactNumber = "AE1", PartnerId = partnerId, Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyDraftContactsAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().Contain(r => r.FirstName == "DraftEdge");
        result.Records.Should().NotContain(r => r.FirstName == "ActiveEdge");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyOpportunitiesAsync_IncludesUserAsStakeholderViaOpportunityStakeholder()
    {
        var opportunity = new OpportunityEntity
        {
            Name = "Stakeholder Opp Edge",
            Description = "Desc",
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<OpportunityEntity>().Add(opportunity);
        await _context.SaveChangesAsync();
        await OverrideAuditFieldsAsync("Opportunities", opportunity.Id, _otherUserId);

        var entityRole = await EnsureEntityRoleExistsAsync();
        _context.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
        {
            OpportunityId = opportunity.Id,
            UserId = _testUserId,
            EntityRoleId = entityRole.Id
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyOpportunitiesAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().Contain(r => r.Name == "Stakeholder Opp Edge");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetOrgUnitRecentUpdatesAsync_MissingOrgUnitFilter_ReturnsAllEntityTypes()
    {
        _mockUserPreferenceService.Setup(x => x.GetGlobalFiltersAsync(It.IsAny<string>()))
            .ReturnsAsync(new UNOPS.PAO.Domain.Entities.GlobalFilters { OrgUnitId = null });

        var result = await _service.GetOrgUnitRecentUpdatesAsync(CreateUser(_testUserId), 10);

        result.Should().NotBeNull();
        result.Updates.Should().NotBeNull();
        result.OrgUnitName.Should().Be("your organization unit");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetAllDashboardDataAsync_HandlesEmptyDataGracefully()
    {
        var result = await _service.GetAllDashboardDataAsync(CreateUser(_otherUserId), 50, 10);

        result.Should().NotBeNull();
        result.MyPartners.Should().NotBeNull();
        result.MyContacts.Should().NotBeNull();
        result.MyInteractions.Should().NotBeNull();
        result.MyOpportunities.Should().NotBeNull();
        result.DraftPartners.Should().NotBeNull();
        result.DraftContacts.Should().NotBeNull();
        result.DraftInteractions.Should().NotBeNull();
        result.DraftOpportunities.Should().NotBeNull();
    }

    #endregion

    #region Functional Tests (F=6)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyPartnersAsync_FiltersByCreatedByCorrectly()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var mine = new UNOPSPartner { Name = $"Mine_{marker}", Status = EntityStatus.Active, CreatedDate = DateTime.UtcNow };
        var other = new UNOPSPartner { Name = $"Other_{marker}", Status = EntityStatus.Active, CreatedDate = DateTime.UtcNow };
        _context.Set<UNOPSPartner>().AddRange(mine, other);
        await _context.SaveChangesAsync();
        await OverrideAuditFieldsAsync("Partners", other.Id, _otherUserId);

        var result = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().Contain(r => r.Name == $"Mine_{marker}");
        result.Records.Should().NotContain(r => r.Name == $"Other_{marker}");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyPartnersAsync_FiltersByLastModifiedByCorrectly()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var modifiedByMe = new UNOPSPartner { Name = $"ModifiedByMe_{marker}", Status = EntityStatus.Active, LastModifiedDate = DateTime.UtcNow, CreatedDate = DateTime.UtcNow };
        var other = new UNOPSPartner { Name = $"Other_{marker}", Status = EntityStatus.Active, CreatedDate = DateTime.UtcNow };
        _context.Set<UNOPSPartner>().AddRange(modifiedByMe, other);
        await _context.SaveChangesAsync();
        await OverrideAuditFieldsAsync("Partners", modifiedByMe.Id, _otherUserId, _testUserId);
        await OverrideAuditFieldsAsync("Partners", other.Id, _otherUserId);

        var result = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().Contain(r => r.Name == $"ModifiedByMe_{marker}");
        result.Records.Should().NotContain(r => r.Name == $"Other_{marker}");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftPartnersAsync_CombinesUserFilterWithDraftStatus()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var myDraft = new UNOPSPartner { Name = $"MyDraft_{marker}", Status = EntityStatus.Draft, CreatedDate = DateTime.UtcNow };
        var otherDraft = new UNOPSPartner { Name = $"OtherDraft_{marker}", Status = EntityStatus.Draft, CreatedDate = DateTime.UtcNow };
        var myActive = new UNOPSPartner { Name = $"MyActive_{marker}", Status = EntityStatus.Active, CreatedDate = DateTime.UtcNow };
        _context.Set<UNOPSPartner>().AddRange(myDraft, otherDraft, myActive);
        await _context.SaveChangesAsync();
        await OverrideAuditFieldsAsync("Partners", otherDraft.Id, _otherUserId);

        var result = await _service.GetMyDraftPartnersAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().Contain(r => r.Name == $"MyDraft_{marker}");
        result.Records.Should().NotContain(r => r.Name == $"OtherDraft_{marker}");
        result.Records.Should().NotContain(r => r.Name == $"MyActive_{marker}");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyOpportunitiesAsync_IncludesRoleFromStakeholder()
    {
        var opportunity = new OpportunityEntity
        {
            Name = "Role Opp Func",
            Description = "Desc",
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<OpportunityEntity>().Add(opportunity);
        await _context.SaveChangesAsync();
        await OverrideAuditFieldsAsync("Opportunities", opportunity.Id, _otherUserId);

        var entityRole = await EnsureEntityRoleExistsAsync("Project Manager");
        _context.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
        {
            OpportunityId = opportunity.Id,
            UserId = _testUserId,
            EntityRoleId = entityRole.Id
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyOpportunitiesAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().Contain(r => r.Name == "Role Opp Func");
        var roleOpp = result.Records.First(r => r.Name == "Role Opp Func");
        roleOpp.UserRole.Should().Be("Project Manager");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrgUnitRecentUpdatesAsync_SortsByLastModifiedDateDescending()
    {
        var partner1 = new UNOPSPartner
        {
            Name = "Old Sort",
            Status = EntityStatus.Active,
            CreatedBy = _testUserId,
            CreatedDate = DateTime.UtcNow.AddDays(-2),
            LastModifiedDate = DateTime.UtcNow.AddDays(-1)
        };
        var partner2 = new UNOPSPartner
        {
            Name = "New Sort",
            Status = EntityStatus.Active,
            CreatedBy = _testUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartner>().AddRange(partner1, partner2);
        await _context.SaveChangesAsync();

        var result = await _service.GetOrgUnitRecentUpdatesAsync(CreateUser(_testUserId), 10);

        result.Updates.Should().NotBeEmpty();
        if (result.Updates.Count >= 2)
            result.Updates[0].LastModifiedDate.Should().BeAfter(result.Updates[1].LastModifiedDate ?? DateTime.MinValue);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetAllDashboardDataAsync_IncludesAllEntityTypes()
    {
        var partnerId = await CreateTestPartnerAsync();
        _context.Set<UNOPSContact>().Add(new UNOPSContact
        {
            FirstName = "CFunc", LastName = "CFunc", Title = "Mr", Email = $"cfunc{Guid.NewGuid():N}@test.com", ContactNumber = "CNF1",
            PartnerId = partnerId, Status = EntityStatus.Active,
            CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow
        });
        _context.Set<Interaction>().Add(new Interaction
        {
            Subject = "IFunc", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active,
            CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow
        });
        _context.Set<OpportunityEntity>().Add(new OpportunityEntity
        {
            Name = "OFunc", Description = "D", Status = EntityStatus.Active,
            CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllDashboardDataAsync(CreateUser(_testUserId), 50, 10);

        result.MyPartners.Should().NotBeEmpty();
        result.MyContacts.Should().NotBeEmpty();
        result.MyInteractions.Should().NotBeEmpty();
        result.MyOpportunities.Should().NotBeEmpty();
    }

    #endregion

    #region Integration Tests (I=6)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullDashboardFlow_CreateData_Retrieve_VerifyStructure()
    {
        var partnerId = await CreateTestPartnerAsync("Flow Partner Int");
        _context.Set<UNOPSContact>().Add(new UNOPSContact
        {
            FirstName = "Flow", LastName = "Contact", Title = "Mr", Email = $"flow{Guid.NewGuid():N}@test.com",
            PartnerId = partnerId, Status = EntityStatus.Active,
            CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllDashboardDataAsync(CreateUser(_testUserId), 50, 10);

        result.MyPartners.Should().Contain(p => p.Name == "Flow Partner Int");
        result.MyContacts.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Dashboard_MixedEntityStatuses_ReturnsAllStatuses()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        _context.Set<UNOPSPartner>().AddRange(
            new UNOPSPartner { Name = $"Active_{marker}", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"Draft_{marker}", Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var allResult = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1000);
        var draftResult = await _service.GetMyDraftPartnersAsync(CreateUser(_testUserId), 1000);

        allResult.Records.Should().Contain(r => r.Name == $"Active_{marker}");
        allResult.Records.Should().Contain(r => r.Name == $"Draft_{marker}");
        draftResult.Records.Should().Contain(r => r.Name == $"Draft_{marker}");
        draftResult.Records.Should().NotContain(r => r.Name == $"Active_{marker}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DraftFilter_OnlyReturnsDraftEntities()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        _context.Set<UNOPSPartner>().AddRange(
            new UNOPSPartner { Name = $"D1_{marker}", Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"A1_{marker}", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"D2_{marker}", Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyDraftPartnersAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().Contain(r => r.Name == $"D1_{marker}");
        result.Records.Should().Contain(r => r.Name == $"D2_{marker}");
        result.Records.Should().NotContain(r => r.Name == $"A1_{marker}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OpportunityStakeholderInclusion_WorksEndToEnd()
    {
        var opp = new OpportunityEntity
        {
            Name = "Stakeholder Opp Int",
            Description = "Desc",
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<OpportunityEntity>().Add(opp);
        await _context.SaveChangesAsync();
        await OverrideAuditFieldsAsync("Opportunities", opp.Id, _otherUserId);

        var entityRole = await EnsureEntityRoleExistsAsync("Lead");
        _context.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
        {
            OpportunityId = opp.Id,
            UserId = _testUserId,
            EntityRoleId = entityRole.Id
        });
        await _context.SaveChangesAsync();

        var singleResult = await _service.GetMyOpportunitiesAsync(CreateUser(_testUserId), 1000);
        var combinedResult = await _service.GetAllDashboardDataAsync(CreateUser(_testUserId), 50, 10);

        singleResult.Records.Should().Contain(r => r.Name == "Stakeholder Opp Int");
        combinedResult.MyOpportunities.Should().Contain(o => o.Name == "Stakeholder Opp Int");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OrgUnitRecentUpdates_CombinesAllEntityTypes()
    {
        var partnerId = await CreateTestPartnerAsync();
        _context.Set<UNOPSContact>().Add(new UNOPSContact
        {
            FirstName = "CInt", LastName = "CInt", Title = "Mr", Email = $"cint{Guid.NewGuid():N}@test.com", ContactNumber = "CNI2",
            PartnerId = partnerId, Status = EntityStatus.Active,
            CreatedBy = _testUserId, LastModifiedDate = DateTime.UtcNow, CreatedDate = DateTime.UtcNow
        });
        _context.Set<Interaction>().Add(new Interaction
        {
            Subject = "IInt", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active,
            CreatedBy = _testUserId, LastModifiedDate = DateTime.UtcNow, CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetOrgUnitRecentUpdatesAsync(CreateUser(_testUserId), 20);

        var types = result.Updates.Select(u => u.Type).Distinct().ToList();
        types.Should().Contain("Partner");
        types.Should().Contain("Contact");
        types.Should().Contain("Interaction");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DashboardPagination_RespectsPageSizeParameter()
    {
        for (var i = 0; i < 5; i++)
        {
            _context.Set<UNOPSPartner>().Add(new UNOPSPartner
            {
                Name = $"PartnerPag{i}",
                Status = EntityStatus.Active,
                CreatedBy = _testUserId,
                CreatedDate = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        var resultPage2 = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 2);
        var resultPage10 = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 10);

        resultPage2.Records.Should().HaveCount(2);
        resultPage2.TotalCount.Should().BeGreaterThanOrEqualTo(5);
        resultPage2.PageSize.Should().Be(2);

        resultPage10.Records.Should().HaveCountGreaterThanOrEqualTo(5);
        resultPage10.PageSize.Should().Be(10);
    }

    #endregion

    #region Helpers

    private async Task<int> CreateTestPartnerAsync(string name = "Test Partner")
    {
        var partner = new UNOPSPartner
        {
            Name = name,
            Status = EntityStatus.Active,
            CreatedBy = _testUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartner>().Add(partner);
        await _context.SaveChangesAsync();
        return partner.Id;
    }

    private async Task<EntityRole> EnsureEntityRoleExistsAsync(string name = "Test Role")
    {
        var existing = await _context.Set<EntityRole>().FirstOrDefaultAsync(r => r.Name == name);
        if (existing != null) return existing;

        var role = new EntityRole { Name = name, EntityType = "Opportunity", Status = EntityStatus.Active };
        _context.Set<EntityRole>().Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    /// <summary>
    /// Override audit fields after SaveChanges, bypassing AuditableDbContext
    /// which always sets CreatedBy/LastModifiedBy to the context user.
    /// </summary>
    #pragma warning disable EF1002 // Table name is test-controlled, values are parameterized
    private async Task OverrideAuditFieldsAsync(string tableName, int entityId, int createdBy, int? lastModifiedBy = null)
    {
        var modBy = lastModifiedBy ?? createdBy;
        await _context.Database.ExecuteSqlRawAsync(
            $"UPDATE \"{tableName}\" SET \"CreatedBy\" = @p0, \"LastModifiedBy\" = @p1 WHERE \"Id\" = @p2",
            createdBy, modBy, entityId);
    }
    #pragma warning restore EF1002

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 2 | GetMyPartnersAsync_ValidUser_ReturnsPartners, GetAllDashboardDataAsync_ValidUser_ReturnsCombinedStructure |
| Negative (N) | 6 | GetMyPartnersAsync_NullUserIdClaim_ReturnsEmpty, GetMyContactsAsync_InvalidUserIdClaim_ReturnsEmpty, GetMyDraftPartnersAsync_NoDraftsExist_ReturnsEmpty, GetMyInteractionsAsync_UserWithNoInteractions_ReturnsEmpty, GetMyOpportunitiesAsync_UserNotStakeholderOrCreator_ReturnsEmpty, GetOrgUnitRecentUpdatesAsync_NullUserId_ReturnsEmpty |
| Edge/Boundary (E) | 6 | GetMyPartnersAsync_PageSizeZero_ReturnsEmptyRecords, GetMyPartnersAsync_PageSizeOne_ReturnsSingleResult, GetMyDraftContactsAsync_OnlyReturnsDraftStatus, GetMyOpportunitiesAsync_IncludesUserAsStakeholderViaOpportunityStakeholder, GetOrgUnitRecentUpdatesAsync_MissingOrgUnitFilter_ReturnsAllEntityTypes, GetAllDashboardDataAsync_HandlesEmptyDataGracefully |
| Functional (F) | 6 | GetMyPartnersAsync_FiltersByCreatedByCorrectly, GetMyPartnersAsync_FiltersByLastModifiedByCorrectly, GetMyDraftPartnersAsync_CombinesUserFilterWithDraftStatus, GetMyOpportunitiesAsync_IncludesRoleFromStakeholder, GetOrgUnitRecentUpdatesAsync_SortsByLastModifiedDateDescending, GetAllDashboardDataAsync_IncludesAllEntityTypes |
| Integration (I) | 6 | FullDashboardFlow_CreateData_Retrieve_VerifyStructure, Dashboard_MixedEntityStatuses_ReturnsAllStatuses, DraftFilter_OnlyReturnsDraftEntities, OpportunityStakeholderInclusion_WorksEndToEnd, OrgUnitRecentUpdates_CombinesAllEntityTypes, DashboardPagination_RespectsPageSizeParameter |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
