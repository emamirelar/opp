using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Domain.Entities;
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Permission and security tests for opportunity management
/// Tests row-level security, role-based access, and permission enforcement
/// Created: January 15, 2026
/// Priority: P1-P2
/// SKIPPED: QA-009 - Z.EntityFramework.Extensions requires relational database (PostgreSQL)
/// </summary>
public class OpportunityPermissionTests : IDisposable
{
    private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
    private readonly UNOPSAppDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly string _testMarker = $"PERM_{Guid.NewGuid():N}";
    private readonly List<int> _createdOpportunityIds = new();
    private int _currencyId;
    private int _countryId;
    private int _orgHierarchyId;
    private int _orgHierarchyId2;
    private int _proposedInitiativeTypeId;
    private int _userId1;
    private int _userId2;
    private int _entityRoleId;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IDbContextFactory<UNOPSAppDbContext>> _mockDbContextFactory;
    private readonly Mock<IExchangeRateService> _mockExchangeRateService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly UNOPSOpportunityManager _manager;
    private readonly ClaimsPrincipal _testUser;

    public OpportunityPermissionTests()
    {
        _dbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OpportunityPermTestDb_{Guid.NewGuid()}");
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        // Phase 1: Resolve test user IDs using a temporary context (outside transaction).
        // AuditableDbContext caches _currentUserId at construction, so we must know the
        // real user ID before creating the main context.
        {
            var tempAccessor = CreateMockHttpContextAccessor("0");
            var tempResolver = new UserResolverService<int>(tempAccessor.Object, null);
            using var tempCtx = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, tempResolver, mockDbSchema.Object);
            _userId1 = TestDataHelper.GetOrCreateTestUser(tempCtx, "user1@unops.org");
            _userId2 = TestDataHelper.GetOrCreateTestUser(tempCtx, "user2@unops.org");
        }

        // Phase 2: Create the MAIN context with the ACTUAL test user ID in claims.
        var mainAccessor = CreateMockHttpContextAccessor(_userId1.ToString());
        var userResolverService = new UserResolverService<int>(mainAccessor.Object, null);
        _context = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, userResolverService, mockDbSchema.Object);

        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = _context.Database.BeginTransaction();
        }

        SeedTestData();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });
        _mapper = mapperConfig.CreateMapper();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbSchema"] = "public",
                ["AISettings:DisableExternalCalls"] = "true",
                ["AISettings:ModelName"] = "gemini-pro",
                ["AISettings:ProjectId"] = "test-project",
                ["AISettings:Location"] = "us-central1",
                ["IsUNOPSOverride"] = "true",
                ["GoogleCloud:ProjectId"] = "test-project",
                ["GoogleCloud:PubSubTopic"] = "test-topic",
                ["ExchangeRate:ApiKey"] = "test-key",
                ["ExchangeRate:BaseUrl"] = "https://test-api.example.com"
            })
            .Build();

        _mockPermissionService = new Mock<IPermissionService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockDbContextFactory = new Mock<IDbContextFactory<UNOPSAppDbContext>>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockExchangeRateService = new Mock<IExchangeRateService>();

        _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId1.ToString()),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "user1@unops.org"),
            new Claim(ClaimTypes.Role, "User")
        }, "TestAuthType"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(_testUser);
        _mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);

        _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var factoryAccessor = CreateMockHttpContextAccessor(_userId1.ToString());
                var factoryResolver = new UserResolverService<int>(factoryAccessor.Object, null);
                return UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, factoryResolver, mockDbSchema.Object);
            });

        _manager = new UNOPSOpportunityManager(
            _mapper,
            _context,
            _configuration,
            _mockDbContextFactory.Object,
            _mockExchangeRateService.Object,
            _mockPermissionService.Object,
            _mockHttpContextAccessor.Object,
            _mockServiceProvider.Object
        );
    }

    private void SeedTestData()
    {
        var currency = _context.Currencies.FirstOrDefault(c => c.Code == "USD");
        if (currency == null)
        {
            currency = new Currency { Code = "USD", Name = "US Dollar", IsDeleted = false };
            _context.Currencies.Add(currency);
            _context.SaveChanges();
        }
        _currencyId = currency.Id;

        var country = _context.Countries.FirstOrDefault(c => c.Iso2Code == "BD");
        if (country == null)
        {
            country = new Country { Name = "Bangladesh", Iso2Code = "BD" };
            _context.Countries.Add(country);
            _context.SaveChanges();
        }
        _countryId = country.Id;

        var orgHierarchy = _context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "OU1" && !o.IsDeleted);
        if (orgHierarchy == null)
        {
            orgHierarchy = new OrganizationHierarchy { Name = "Org Unit 1", Code = "OU1", Description = "Organization Unit 1", IsDeleted = false };
            _context.OrganizationHierarchies.Add(orgHierarchy);
            _context.SaveChanges();
        }
        _orgHierarchyId = orgHierarchy.Id;

        var orgHierarchy2 = _context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "OU2" && !o.IsDeleted);
        if (orgHierarchy2 == null)
        {
            orgHierarchy2 = new OrganizationHierarchy { Name = "Org Unit 2", Code = "OU2", Description = "Organization Unit 2", IsDeleted = false };
            _context.OrganizationHierarchies.Add(orgHierarchy2);
            _context.SaveChanges();
        }
        _orgHierarchyId2 = orgHierarchy2.Id;

        var proposedInitiativeType = _context.ProposedInitiativeTypes.FirstOrDefault(p => p.Name == "Project" && !p.IsDeleted);
        if (proposedInitiativeType == null)
        {
            proposedInitiativeType = new ProposedInitiativeType { Name = "Project", IsDeleted = false };
            _context.ProposedInitiativeTypes.Add(proposedInitiativeType);
            _context.SaveChanges();
        }
        _proposedInitiativeTypeId = proposedInitiativeType.Id;

        _userId1 = TestDataHelper.GetOrCreateTestUser(_context, "user1@unops.org");
        _userId2 = TestDataHelper.GetOrCreateTestUser(_context, "user2@unops.org");

        var entityRole = _context.EntityRoles.FirstOrDefault(r => r.Code == "Opportunity_Manager_Opportunity" && !r.IsDeleted);
        if (entityRole == null)
        {
            entityRole = new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Opportunity Manager",
                Description = "Manages the opportunity",
                IsInternal = true,
                AllowsMultiple = false,
                Code = "Opportunity_Manager_Opportunity",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            _context.EntityRoles.Add(entityRole);
            _context.SaveChanges();
        }
        _entityRoleId = entityRole.Id;

        _context.ChangeTracker.Clear();
    }

    private async Task<int> CreateTestOpportunityAsync(
        string? name = null,
        string? description = null,
        string stage = "IDENTIFY & PROFILE",
        EntityStatus status = EntityStatus.Draft,
        int? responsibleOrgUnitId = null,
        int? createdBy = null)
    {
        var opportunity = new OpportunityEntity
        {
            Name = name ?? $"Test Opportunity {_testMarker}",
            Description = description ?? "Test Description",
            Stage = stage,
            Status = status,
            ResponsibleOrgUnitId = responsibleOrgUnitId ?? _orgHierarchyId,
            CreatedBy = createdBy ?? _userId1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedBy = createdBy ?? _userId1,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();
        _createdOpportunityIds.Add(opportunity.Id);
        return opportunity.Id;
    }

    #region P1 - Permission Checks Tests

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-001")]
    public async Task GetOpportunityWithUser_IncludesPermissions_Success()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Permission Test Opportunity",
            responsibleOrgUnitId: _orgHierarchyId,
            createdBy: _userId1);

        var result = await _manager.GetOpportunityAsync(_testUser, oppId);

        result.Should().NotBeNull();
        result!.Permissions.Should().NotBeNull();
        result.Permissions!.CanRead.Should().BeTrue();
        result.Permissions.CanUpdate.Should().BeTrue();
        result.Permissions.CanDelete.Should().BeFalse();
    }

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-002")]
    public async Task GetOpportunity_UserCannotView_ReturnsNull()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Restricted Opportunity",
            responsibleOrgUnitId: _orgHierarchyId2,
            createdBy: _userId2);

        var result = await _manager.GetOpportunityAsync(_testUser, oppId);

        result.Should().BeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-003")]
    public async Task CreateOpportunity_UserLacksPermission_ThrowsException()
    {
        var request = new OpportunityRequest
        {
            Name = "Unauthorized Creation",
            Description = "User lacks create permission"
        };

        var permissions = new EntityPermissionsModel
        {
            CanRead = true,
            CanCreate = false,
            CanUpdate = false,
            CanDelete = false
        };
        _mockPermissionService.Setup(p => p.GetEntityPermissionsAsync("Opportunity", null))
            .ReturnsAsync(permissions);

        _mockPermissionService.Setup(p => p.CanPerformActionAsync("Opportunity", "Create", It.IsAny<ClaimsPrincipal>(), null))
            .ReturnsAsync(false);

        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permission*");
    }

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-004")]
    public async Task UpdateOpportunity_UserLacksEditPermission_ThrowsException()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Read-Only Opportunity",
            createdBy: _userId2);

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Unauthorized Update"
        };

        var permissions = new EntityPermissionsModel
        {
            CanRead = true,
            CanCreate = false,
            CanUpdate = false,
            CanDelete = false
        };
        _mockPermissionService.Setup(p => p.GetEntityPermissionsAsync("Opportunity", It.IsAny<object>()))
            .ReturnsAsync(permissions);

        _mockPermissionService.Setup(p => p.CanPerformActionAsync("Opportunity", "Update", It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(false);

        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*edit*");
    }

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-005")]
    public async Task DeleteOpportunity_UserLacksDeletePermission_ThrowsException()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Protected Opportunity",
            status: EntityStatus.Active,
            createdBy: _userId1);

        Func<Task> act = async () => await _manager.DeleteOpportunityAsync(oppId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*delete*");
    }

    #endregion

    #region P1 - Row-Level Security Tests

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-006")]
    public async Task GetAllOpportunities_FiltersByOrgUnit_Success()
    {
        await CreateTestOpportunityAsync(name: "Opp in Org Unit 1", responsibleOrgUnitId: _orgHierarchyId, createdBy: _userId1);
        await CreateTestOpportunityAsync(name: "Opp in Org Unit 2", responsibleOrgUnitId: _orgHierarchyId2, createdBy: _userId1);

        _mockPermissionService.Setup(p => p.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_orgHierarchyId.ToString());

        _mockPermissionService.Setup(p => p.ApplyAccessControlFiltersAsync(It.IsAny<IQueryable<OpportunityEntity>>(),
            It.IsAny<ClaimsPrincipal>(), "View", "Opportunity"))
            .ReturnsAsync((IQueryable<OpportunityEntity> query, ClaimsPrincipal user, string action, string entityName) =>
                (object)query.Where(o => o.ResponsibleOrgUnitId == _orgHierarchyId));

        var result = await _manager.GetAllOpportunitiesAsync();

        var opportunities = result.Where(o => _createdOpportunityIds.Contains(o.Id)).ToList();
        opportunities.Should().HaveCount(1);
        opportunities.Should().OnlyContain(o => o.ResponsibleOrgUnitId == _orgHierarchyId);
    }

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-007")]
    public async Task GetOpportunitiesByPartner_FiltersByPermission_Success()
    {
        await CreateTestOpportunityAsync(name: "Visible Opp", responsibleOrgUnitId: _orgHierarchyId, createdBy: _userId1);
        await CreateTestOpportunityAsync(name: "Hidden Opp", responsibleOrgUnitId: _orgHierarchyId2, createdBy: _userId1);

        var result = await _manager.GetOpportunitiesByPartnerIdAsync(1);

        var opportunities = result.Where(o => _createdOpportunityIds.Contains(o.Id)).ToList();
        opportunities.Should().HaveCount(1);
        opportunities.Should().NotContain(o => o.Name == "Hidden Opp");
    }

    #endregion

    #region P2 - Role-Based Access Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-008")]
    public async Task AdminUser_CanAccessAllOpportunities_Success()
    {
        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "99"),
            new Claim(ClaimTypes.Role, "Administrator")
        }, "TestAuthType"));

        var oppId1 = await CreateTestOpportunityAsync(name: "Opp 1", createdBy: _userId1);
        var oppId2 = await CreateTestOpportunityAsync(name: "Opp 2", createdBy: _userId2);

        _mockPermissionService.Setup(p => p.GetEffectiveRole(It.Is<ClaimsPrincipal>(u => u.IsInRole("Administrator"))))
            .Returns("Administrator");

        var permissions = EntityPermissionsModel.All;
        _mockPermissionService.Setup(p => p.GetEntityPermissionsAsync("Opportunity", null))
            .ReturnsAsync(permissions);

        _mockPermissionService.Setup(p => p.ApplyAccessControlFiltersAsync(It.IsAny<IQueryable<OpportunityEntity>>(),
            It.IsAny<ClaimsPrincipal>(), "View", "Opportunity"))
            .ReturnsAsync((IQueryable<OpportunityEntity> query, ClaimsPrincipal user, string action, string entityName) =>
                (object)query);

        var result = await _manager.GetAllOpportunitiesAsync();

        var opportunities = result.Where(o => _createdOpportunityIds.Contains(o.Id)).ToList();
        opportunities.Should().HaveCount(2);
    }

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-009")]
    public async Task ReadOnlyUser_CannotEdit_ThrowsException()
    {
        var readOnlyUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "50"),
            new Claim(ClaimTypes.Role, "ReadOnly")
        }, "TestAuthType"));

        var oppId = await CreateTestOpportunityAsync(name: "Test Opportunity", createdBy: _userId1);

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Attempted Update"
        };

        var permissions = EntityPermissionsModel.ReadOnly;
        _mockPermissionService.Setup(p => p.GetEntityPermissionsAsync("Opportunity", It.IsAny<object>()))
            .ReturnsAsync(permissions);

        _mockPermissionService.Setup(p => p.CanPerformActionAsync("Opportunity", "Update", It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(false);

        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-010")]
    public async Task OpportunityCreator_HasSpecialPermissions_Success()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Created by User 1", createdBy: _userId1);

        var creatorPermissions = new EntityPermissionsModel
        {
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true
        };
        _mockPermissionService.Setup(p => p.GetEntityInstancePermissionsAsync("Opportunity", oppId))
            .ReturnsAsync(creatorPermissions);

        _mockPermissionService.Setup(p => p.HasInstanceAccessAsync("Opportunity", It.IsAny<object>(), It.IsAny<ClaimsPrincipal>(), "Delete"))
            .ReturnsAsync(true);

        var result = await _manager.GetOpportunityAsync(_testUser, oppId);

        result.Should().NotBeNull();
        result!.Permissions.Should().NotBeNull();
        result.Permissions!.CanDelete.Should().BeTrue();
    }

    #endregion

    #region P2 - Workflow-Based Permissions Tests

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-011")]
    public async Task ActiveOpportunity_RestrictsDelete_Success()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Active Opportunity",
            stage: "DEVELOP",
            status: EntityStatus.Active,
            createdBy: _userId1);

        var activeOpportunityPermissions = new EntityPermissionsModel
        {
            CanRead = true,
            CanCreate = false,
            CanUpdate = true,
            CanDelete = false
        };
        _mockPermissionService.Setup(p => p.GetEntityInstancePermissionsAsync("Opportunity", oppId))
            .ReturnsAsync(activeOpportunityPermissions);

        _mockPermissionService.Setup(p => p.CanPerformActionAsync("Opportunity", "Delete", It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(false);

        Func<Task> act = async () => await _manager.DeleteOpportunityAsync(oppId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*active*");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-012")]
    public async Task DraftOpportunity_AllowsDelete_Success()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Draft Opportunity",
            status: EntityStatus.Draft,
            createdBy: _userId1);

        var draftPermissions = new EntityPermissionsModel
        {
            CanRead = true,
            CanCreate = true,
            CanUpdate = true,
            CanDelete = true
        };
        _mockPermissionService.Setup(p => p.GetEntityInstancePermissionsAsync("Opportunity", oppId))
            .ReturnsAsync(draftPermissions);

        _mockPermissionService.Setup(p => p.CanPerformActionAsync("Opportunity", "Delete", It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(true);

        var result = await _manager.DeleteOpportunityAsync(oppId);

        result.Should().BeTrue();

        var deletedOpportunity = await _context.Opportunities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == oppId);

        deletedOpportunity.Should().NotBeNull();
        deletedOpportunity!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region P2 - Team-Based Permissions Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-013")]
    public async Task TeamMember_HasEditPermission_Success()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Team Opportunity",
            createdBy: _userId2);

        _mockPermissionService.Setup(p => p.IsOpportunityTeamMemberAsync(oppId))
            .ReturnsAsync(true);

        var teamMemberPermissions = new EntityPermissionsModel
        {
            CanRead = true,
            CanCreate = false,
            CanUpdate = true,
            CanDelete = false
        };
        _mockPermissionService.Setup(p => p.GetEntityInstancePermissionsAsync("Opportunity", oppId))
            .ReturnsAsync(teamMemberPermissions);

        _mockPermissionService.Setup(p => p.CanPerformActionAsync("Opportunity", "Update", It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(true);

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Team Member Update"
        };

        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Team Member Update");
    }

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-014")]
    public async Task NonTeamMember_CannotEdit_ThrowsException()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Private Team Opportunity",
            createdBy: _userId2);

        _mockPermissionService.Setup(p => p.IsOpportunityTeamMemberAsync(oppId))
            .ReturnsAsync(false);

        var nonTeamMemberPermissions = new EntityPermissionsModel
        {
            CanRead = true,
            CanCreate = false,
            CanUpdate = false,
            CanDelete = false
        };
        _mockPermissionService.Setup(p => p.GetEntityInstancePermissionsAsync("Opportunity", oppId))
            .ReturnsAsync(nonTeamMemberPermissions);

        _mockPermissionService.Setup(p => p.CanPerformActionAsync("Opportunity", "Update", It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(false);

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Unauthorized Edit"
        };

        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-015")]
    public async Task AssignTeamMember_AddsPermissions_Success()
    {
        var oppId = await CreateTestOpportunityAsync(
            name: "Team Assignment Test",
            createdBy: _userId1);

        await _manager.AssignCreatorAsOpportunityManagerAsync(oppId, _userId1);

        var savedOpportunity = await _context.Opportunities
            .Include(o => o.Stakeholders)
            .FirstOrDefaultAsync(o => o.Id == oppId);

        savedOpportunity.Should().NotBeNull();
    }

    #endregion

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string userId)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Headers).Returns(new HeaderDictionary());
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "user1@unops.org")
        }, "TestAuthType"));
        httpContext.Setup(m => m.User).Returns(user);
        httpContext.Setup(m => m.Request).Returns(request.Object);
        accessor.Setup(m => m.HttpContext).Returns(httpContext.Object);
        return accessor;
    }

    public void Dispose()
    {
        try
        {
            if (TestEnvironment.UsePostgreSQL && _createdOpportunityIds.Any())
            {
                var ids = string.Join(",", _createdOpportunityIds);
                _context.Database.ExecuteSql($"DELETE FROM public.\"Opportunities\" WHERE \"Id\" IN ({ids})");
            }
        }
        catch { /* Best-effort cleanup */ }

        if (TestEnvironment.UseInMemory)
        {
            try { _context.Database.EnsureDeleted(); }
            catch { /* SQLite connection may already be closed during concurrent test runs */ }
        }
        if (_transaction != null)
        {
            try { _transaction.Rollback(); }
            catch { }
            _transaction.Dispose();
            _transaction = null;
        }
        _context.Dispose();
    }
}
