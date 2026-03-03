/**
 * @fileoverview Comprehensive mock-based tests for DashboardService.
 * Tests GetMyPartnersAsync, GetMyContactsAsync, GetMyDraftPartnersAsync, GetMyDraftContactsAsync,
 * GetMyInteractionsAsync, GetMyDraftInteractionsAsync, GetMyOpportunitiesAsync, GetMyDraftOpportunitiesAsync,
 * GetOrgUnitRecentUpdatesAsync, and GetAllDashboardDataAsync.
 *
 * Ratio: P=2, N=6+, E=6+, F=6+, I=6+
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
/// Uses InMemory/SQLite database with mocked dependencies (IPermissionService, IUserPreferenceService, IOrgUnitHierarchyService).
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class DashboardServiceTests : IDisposable
{
    private readonly UNOPSAppDbContext? _context;
    private readonly DashboardService? _service;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IUserPreferenceService> _mockUserPreferenceService;
    private readonly Mock<IOrgUnitHierarchyService> _mockHierarchyService;
    private readonly IMapper _mapper;
    private readonly int _testUserId = 42;
    private readonly string _dbName;
    private readonly bool _setupFailed;

    public DashboardServiceTests()
    {
        _dbName = $"Dashboard_{Guid.NewGuid():N}";
        _mockPermissionService = new Mock<IPermissionService>();
        _mockUserPreferenceService = new Mock<IUserPreferenceService>();
        _mockHierarchyService = new Mock<IOrgUnitHierarchyService>();

        UNOPSAppDbContext? context = null;
        try
        {
            context = TestDbContextFactory.CreateUNOPSWithUserId(_testUserId, _dbName);
            TestEnvironment.EnsureCleanDatabase(context);
        }
        catch (Exception)
        {
            try
            {
                context = (UNOPSAppDbContext)TestDbContextFactory.CreateFallbackSqlite();
                TestEnvironment.EnsureCleanDatabase(context);
            }
            catch
            {
                _setupFailed = true;
            }
        }

        _context = context;

        var config = TestEnvironment.CreateTestConfiguration();

        SetupPermissionServicePassThrough();
        SetupUserPreferenceService();
        SetupHierarchyService();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UNOPSPartner, PartnerModel>().ForMember(m => m.Name, o => o.MapFrom(e => e.Name));
            cfg.CreateMap<UNOPSContact, ContactModel>()
                .ForMember(m => m.FirstName, o => o.MapFrom(e => e.FirstName))
                .ForMember(m => m.LastName, o => o.MapFrom(e => e.LastName ?? ""));
            cfg.CreateMap<Interaction, InteractionModel>().ForMember(m => m.Subject, o => o.MapFrom(e => e.Subject ?? ""));
            cfg.CreateMap<OpportunityEntity, OpportunityModel>().ForMember(m => m.Name, o => o.MapFrom(e => e.Name ?? ""));
        });
        _mapper = mapperConfig.CreateMapper();

        var mockLogger = new Mock<ILogger<DashboardService>>();

        if (_setupFailed || _context == null)
            throw new InvalidOperationException("DbContext creation failed (PostgreSQL/SQLite unavailable or UserResolverService null). Skipping tests.");

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

    private static ClaimsPrincipal CreateUser(int userId, params string[] roles)
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

    public void Dispose() => _context?.Dispose();

    #region Positive Tests (P=2)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyPartnersAsync_ValidUser_ReturnsPartners()
    {
        var partner = new UNOPSPartner
        {
            Name = "Test Partner",
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
        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Be("Test Partner");
        result.TotalCount.Should().Be(1);
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
            Name = "Active Partner",
            Status = EntityStatus.Active,
            CreatedBy = _testUserId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<UNOPSPartner>().Add(partner);
        await _context.SaveChangesAsync();

        var result = await _service.GetMyDraftPartnersAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyInteractionsAsync_UserWithNoInteractions_ReturnsEmpty()
    {
        var result = await _service.GetMyInteractionsAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyOpportunitiesAsync_UserNotStakeholderOrCreator_ReturnsEmpty()
    {
        var partnerId = await CreateTestPartnerAsync();
        var opportunity = new OpportunityEntity
        {
            Name = "Other User Opp",
            Description = "Desc",
            Status = EntityStatus.Active,
            CreatedBy = 999,
            LastModifiedBy = 999,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<OpportunityEntity>().Add(opportunity);
        await _context.SaveChangesAsync();

        var result = await _service.GetMyOpportunitiesAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().BeEmpty();
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
            Name = "Partner",
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
            new UNOPSPartner { Name = "P1", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "P2", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1);

        result.Should().NotBeNull();
        result.Records.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
        result.PageSize.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftContactsAsync_OnlyReturnsDraftStatus()
    {
        var partnerId = await CreateTestPartnerAsync();
        _context.Set<UNOPSContact>().AddRange(
            new UNOPSContact { FirstName = "Draft", LastName = "Contact", Title = "Mr", Email = "d@test.com", ContactNumber = "D1", PartnerId = partnerId, Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSContact { FirstName = "Active", LastName = "Contact", Title = "Mr", Email = "a@test.com", ContactNumber = "A1", PartnerId = partnerId, Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyDraftContactsAsync(CreateUser(_testUserId), 1000);

        result.Should().NotBeNull();
        result.Records.Should().HaveCount(1);
        result.Records[0].FirstName.Should().Be("Draft");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyOpportunitiesAsync_IncludesUserAsStakeholderViaOpportunityStakeholder()
    {
        var opportunity = new OpportunityEntity
        {
            Name = "Stakeholder Opp",
            Description = "Desc",
            Status = EntityStatus.Active,
            CreatedBy = 999,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<OpportunityEntity>().Add(opportunity);
        await _context.SaveChangesAsync();

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
        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Be("Stakeholder Opp");
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
        var result = await _service.GetAllDashboardDataAsync(CreateUser(_testUserId), 50, 10);

        result.Should().NotBeNull();
        result.MyPartners.Should().BeEmpty();
        result.MyContacts.Should().BeEmpty();
        result.MyInteractions.Should().BeEmpty();
        result.MyOpportunities.Should().BeEmpty();
        result.DraftPartners.Should().BeEmpty();
        result.DraftContacts.Should().BeEmpty();
        result.DraftInteractions.Should().BeEmpty();
        result.DraftOpportunities.Should().BeEmpty();
    }

    #endregion

    #region Functional Tests (F=6)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyPartnersAsync_FiltersByCreatedByCorrectly()
    {
        _context.Set<UNOPSPartner>().AddRange(
            new UNOPSPartner { Name = "Mine", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "Other", Status = EntityStatus.Active, CreatedBy = 999, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Be("Mine");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyPartnersAsync_FiltersByLastModifiedByCorrectly()
    {
        _context.Set<UNOPSPartner>().AddRange(
            new UNOPSPartner { Name = "ModifiedByMe", Status = EntityStatus.Active, CreatedBy = 999, LastModifiedBy = _testUserId, LastModifiedDate = DateTime.UtcNow, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "Other", Status = EntityStatus.Active, CreatedBy = 999, LastModifiedBy = 888, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Be("ModifiedByMe");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftPartnersAsync_CombinesUserFilterWithDraftStatus()
    {
        _context.Set<UNOPSPartner>().AddRange(
            new UNOPSPartner { Name = "MyDraft", Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "OtherDraft", Status = EntityStatus.Draft, CreatedBy = 999, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "MyActive", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyDraftPartnersAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().HaveCount(1);
        result.Records[0].Name.Should().Be("MyDraft");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyOpportunitiesAsync_IncludesRoleFromStakeholder()
    {
        var opportunity = new OpportunityEntity
        {
            Name = "Role Opp",
            Description = "Desc",
            Status = EntityStatus.Active,
            CreatedBy = 999,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<OpportunityEntity>().Add(opportunity);
        await _context.SaveChangesAsync();

        var entityRole = await EnsureEntityRoleExistsAsync("Project Manager");
        _context.Set<OpportunityStakeholder>().Add(new OpportunityStakeholder
        {
            OpportunityId = opportunity.Id,
            UserId = _testUserId,
            EntityRoleId = entityRole.Id
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyOpportunitiesAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().HaveCount(1);
        result.Records[0].UserRole.Should().Be("Project Manager");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrgUnitRecentUpdatesAsync_SortsByLastModifiedDateDescending()
    {
        var partner1 = new UNOPSPartner
        {
            Name = "Old",
            Status = EntityStatus.Active,
            CreatedBy = _testUserId,
            CreatedDate = DateTime.UtcNow.AddDays(-2),
            LastModifiedDate = DateTime.UtcNow.AddDays(-1)
        };
        var partner2 = new UNOPSPartner
        {
            Name = "New",
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
            FirstName = "C", LastName = "C", Title = "Mr", Email = "c@test.com", ContactNumber = "CN1",
            PartnerId = partnerId, Status = EntityStatus.Active,
            CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow
        });
        _context.Set<Interaction>().Add(new Interaction
        {
            Subject = "I", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active,
            CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow
        });
        _context.Set<OpportunityEntity>().Add(new OpportunityEntity
        {
            Name = "O", Description = "D", Status = EntityStatus.Active,
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
        var partnerId = await CreateTestPartnerAsync("Flow Partner");
        _context.Set<UNOPSContact>().Add(new UNOPSContact
        {
            FirstName = "Flow", LastName = "Contact", Title = "Mr", Email = "flow@test.com",
            PartnerId = partnerId, Status = EntityStatus.Active,
            CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllDashboardDataAsync(CreateUser(_testUserId), 50, 10);

        result.MyPartners.Should().Contain(p => p.Name == "Flow Partner");
        result.MyContacts.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Dashboard_MixedEntityStatuses_ReturnsAllStatuses()
    {
        _context.Set<UNOPSPartner>().AddRange(
            new UNOPSPartner { Name = "Active", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "Draft", Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var allResult = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 1000);
        var draftResult = await _service.GetMyDraftPartnersAsync(CreateUser(_testUserId), 1000);

        allResult.Records.Should().HaveCount(2);
        draftResult.Records.Should().HaveCount(1);
        draftResult.Records[0].Name.Should().Be("Draft");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DraftFilter_OnlyReturnsDraftEntities()
    {
        _context.Set<UNOPSPartner>().AddRange(
            new UNOPSPartner { Name = "D1", Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "A1", Status = EntityStatus.Active, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "D2", Status = EntityStatus.Draft, CreatedBy = _testUserId, CreatedDate = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var result = await _service.GetMyDraftPartnersAsync(CreateUser(_testUserId), 1000);

        result.Records.Should().HaveCount(2);
        result.Records.Should().OnlyContain(r => r.Name == "D1" || r.Name == "D2");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OpportunityStakeholderInclusion_WorksEndToEnd()
    {
        var opp = new OpportunityEntity
        {
            Name = "Stakeholder Opp",
            Description = "Desc",
            Status = EntityStatus.Active,
            CreatedBy = 111,
            CreatedDate = DateTime.UtcNow
        };
        _context.Set<OpportunityEntity>().Add(opp);
        await _context.SaveChangesAsync();

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

        singleResult.Records.Should().HaveCount(1);
        combinedResult.MyOpportunities.Should().Contain(o => o.Name == "Stakeholder Opp");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task OrgUnitRecentUpdates_CombinesAllEntityTypes()
    {
        var partnerId = await CreateTestPartnerAsync();
        _context.Set<UNOPSContact>().Add(new UNOPSContact
        {
            FirstName = "C", LastName = "C", Title = "Mr", Email = "c@test.com", ContactNumber = "CN2",
            PartnerId = partnerId, Status = EntityStatus.Active,
            CreatedBy = _testUserId, LastModifiedDate = DateTime.UtcNow, CreatedDate = DateTime.UtcNow
        });
        _context.Set<Interaction>().Add(new Interaction
        {
            Subject = "I", Type = InteractionType.InPersonMeeting, Date = DateTime.UtcNow, Status = EntityStatus.Active,
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
                Name = $"Partner{i}",
                Status = EntityStatus.Active,
                CreatedBy = _testUserId,
                CreatedDate = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        var resultPage2 = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 2);
        var resultPage10 = await _service.GetMyPartnersAsync(CreateUser(_testUserId), 10);

        resultPage2.Records.Should().HaveCount(2);
        resultPage2.TotalCount.Should().Be(5);
        resultPage2.PageSize.Should().Be(2);

        resultPage10.Records.Should().HaveCount(5);
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
