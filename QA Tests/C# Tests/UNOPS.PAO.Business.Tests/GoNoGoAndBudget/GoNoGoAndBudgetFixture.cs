/**
 * @fileoverview Shared fixture for Go/No-Go and Budget tests.
 * Provides UNOPSOpportunityManager, UNOPSUserManagementManager, and seeded data.
 * @author UNOPS Opportunity+ QA Team
 */

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.GoNoGoAndBudget;

/// <summary>
/// Shared fixture for Go/No-Go and Budget tests.
/// Seeds opportunities, users, org units, and provides managers.
/// </summary>
public class GoNoGoAndBudgetFixture : IDisposable
{
    public UNOPSOpportunityManager OpportunityManager { get; }
    public UNOPSUserManagementManager UserManagementManager { get; }
    public UNOPSAppDbContext Context { get; }
    public int PaoUserId { get; }
    public int PaoUserId2 { get; }
    public int OpportunityId { get; }
    public int EntityRoleId { get; }
    public int OrgHierarchyId { get; }

    private readonly DbContextOptions<UNOPSAppDbContext> _options;
    private IDbContextTransaction? _transaction;
    private readonly List<int> _createdOpportunityIds = new();

    public GoNoGoAndBudgetFixture()
    {
        _options = TestEnvironment.CreateUNOPSDbContextOptions($"GoNoGo_{Guid.NewGuid()}");
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");

        var tempAccessor = CreateMockHttpContextAccessor("0");
        var tempResolver = new UserResolverService<int>(tempAccessor.Object, null);
        using (var tempCtx = TestDbContextFactory.CreateUNOPS(_options, tempResolver, mockSchema.Object))
        {
            PaoUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "gnogo1@unops.org");
            PaoUserId2 = TestDataHelper.GetOrCreateTestUser(tempCtx, "gnogo2@unops.org");
        }

        var mainAccessor = CreateMockHttpContextAccessor(PaoUserId.ToString());
        var userResolver = new UserResolverService<int>(mainAccessor.Object, null);
        Context = TestDbContextFactory.CreateUNOPS(_options, userResolver, mockSchema.Object);

        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = Context.Database.BeginTransaction();
        }

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        var mapper = mapperConfig.CreateMapper();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbSchema"] = "public",
                ["AISettings:DisableExternalCalls"] = "true",
                ["IsUNOPSOverride"] = "true",
                ["GoogleCloud:ProjectId"] = "test-project",
                ["GoogleCloud:PubSubTopic"] = "test-topic",
                ["ExchangeRate:ApiKey"] = "test-key",
                ["ExchangeRate:BaseUrl"] = "https://test.example.com"
            })
            .Build();

        var mockDbContextFactory = new Mock<IDbContextFactory<UNOPSAppDbContext>>();
        mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var fa = CreateMockHttpContextAccessor(PaoUserId.ToString());
                var fr = new UserResolverService<int>(fa.Object, null);
                return TestDbContextFactory.CreateUNOPS(_options, fr, mockSchema.Object);
            });

        var mockExchangeRate = new Mock<IExchangeRateService>();
        var mockPermission = new Mock<IPermissionService>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        OpportunityManager = new UNOPSOpportunityManager(
            mapper,
            Context,
            config,
            mockDbContextFactory.Object,
            mockExchangeRate.Object,
            mockPermission.Object,
            CreateMockHttpContextAccessor(PaoUserId.ToString()).Object,
            mockServiceProvider.Object);

        UserManagementManager = CreateUserManagementManager();

        EntityRoleId = SeedEntityRole();
        OrgHierarchyId = SeedOrgHierarchy();
        OpportunityId = CreateTestOpportunity();
    }

    private UNOPSUserManagementManager CreateUserManagementManager()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        var mapper = mapperConfig.CreateMapper();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbSchema"] = "public",
                ["IsUNOPSOverride"] = "true"
            })
            .Build();

        var userStore = new Mock<IUserStore<PAOIdentityUser>>();
        var userManager = new UserManager<PAOIdentityUser>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!,
            new Mock<ILogger<UserManager<PAOIdentityUser>>>().Object);

        var roleStore = new Mock<IRoleStore<PAOIdentityRole>>();
        var roleManager = new RoleManager<PAOIdentityRole>(
            roleStore.Object, null!, null!, null!,
            new Mock<ILogger<RoleManager<PAOIdentityRole>>>().Object);

        var permissionService = new Mock<IPermissionService>();
        var geminiManager = new Mock<UNOPS.PAO.Business.Interfaces.IGeminiManager>();
        var logger = new Mock<ILogger<UNOPSUserManagementManager>>().Object;

        return new UNOPSUserManagementManager(
            mapper,
            Context,
            config,
            userManager,
            roleManager,
            permissionService.Object,
            geminiManager.Object,
            logger);
    }

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string userId)
    {
        var mock = new Mock<IHttpContextAccessor>();
        var identity = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Test User"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@unops.org")
        }, "TestAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(principal);
        mock.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);
        return mock;
    }

    private int SeedEntityRole()
    {
        var entityRole = Context.EntityRoles.FirstOrDefault(r =>
            r.Name != null && r.Name.ToLower() == "opportunity manager" && r.EntityType == "Opportunity");
        if (entityRole != null)
            return entityRole.Id;
        var newRole = new EntityRole
        {
            Name = "Opportunity Manager",
            Code = "OpportunityManager",
            EntityType = "Opportunity",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.EntityRoles.Add(newRole);
        Context.SaveChanges();
        return newRole.Id;
    }

    private int SeedOrgHierarchy()
    {
        var org = Context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "SAH" && !o.IsDeleted);
        if (org == null)
        {
            org = new OrganizationHierarchy
            {
                Name = "South Asia Hub",
                Code = "SAH",
                Description = "South Asia Regional Hub",
                IsDeleted = false,
                Type = OrganizationUnitType.OrgUnit
            };
            Context.OrganizationHierarchies.Add(org);
            Context.SaveChanges();
        }
        return org.Id;
    }

    private int CreateTestOpportunity()
    {
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = $"GoNoGo Test Opp {Guid.NewGuid():N}",
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = PaoUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedBy = PaoUserId,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        Context.Opportunities.Add(opp);
        Context.SaveChanges();
        _createdOpportunityIds.Add(opp.Id);
        return opp.Id;
    }

    public void Dispose() => _transaction?.Rollback();
}
