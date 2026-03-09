/// <summary>
/// Base fixture for Opportunity AI Features tests (PNO-694, PNO-803, PNO-804, PNO-805, PNO-873).
/// Provides UNOPSOpportunityManager, seeded reference data, and helper methods.
/// </summary>

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.OpportunityAIFeatures;

public abstract class OpportunityAIFeaturesFixtureBase : IDisposable
{
    protected readonly DbContextOptions<UNOPSAppDbContext> DbContextOptions;
    protected readonly UNOPSAppDbContext Context;
    protected IDbContextTransaction? Transaction;
    protected readonly string TestMarker = $"AIF_{Guid.NewGuid():N}";
    protected readonly List<int> CreatedOpportunityIds = new();
    protected int CurrencyId;
    protected int CountryId;
    protected int OrgHierarchyId;
    protected int ProposedInitiativeTypeId;
    protected int PaoUserId;
    protected int EntityRoleId;
    protected int PartnerId;
    protected readonly IMapper Mapper;
    protected readonly IConfiguration Configuration;
    protected readonly Mock<IPermissionService> MockPermissionService;
    protected readonly Mock<IHttpContextAccessor> MockHttpContextAccessor;
    protected readonly Mock<IDbContextFactory<UNOPSAppDbContext>> MockDbContextFactory;
    protected readonly Mock<IExchangeRateService> MockExchangeRateService;
    protected readonly Mock<IServiceProvider> MockServiceProvider;
    protected readonly UNOPSOpportunityManager Manager;

    protected OpportunityAIFeaturesFixtureBase()
    {
        DbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OpportunityAIFeatures_{Guid.NewGuid()}");
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        // Phase 1: Resolve test user
        {
            var tempAccessor = CreateMockHttpContextAccessor("0");
            var tempResolver = new UserResolverService<int>(tempAccessor.Object, null);
            using var tempCtx = TestDbContextFactory.CreateUNOPS(DbContextOptions, tempResolver, mockDbSchema.Object);
            PaoUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "aiuser@unops.org");
        }

        // Phase 2: Main context
        var mainAccessor = CreateMockHttpContextAccessor(PaoUserId.ToString());
        var userResolverService = new UserResolverService<int>(mainAccessor.Object, null);
        Context = TestDbContextFactory.CreateUNOPS(DbContextOptions, userResolverService, mockDbSchema.Object);

        if (TestEnvironment.UsePostgreSQL)
        {
            Transaction = Context.Database.BeginTransaction();
        }

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbSchema"] = "public",
                ["AISettings:DisableExternalCalls"] = "true",
                ["IsUNOPSOverride"] = "true",
                ["GoogleCloud:ProjectId"] = "test-project",
                ["ExchangeRate:ApiKey"] = "test-key",
                ["ExchangeRate:BaseUrl"] = "https://test-api.example.com"
            })
            .Build();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            cfg.ConstructServicesUsing(serviceType =>
            {
                if (serviceType == typeof(UNOPS.PAO.Business.Mapping.EntityArtifactValueResolver))
                    return Activator.CreateInstance(serviceType, Configuration)!;
                return Activator.CreateInstance(serviceType)!;
            });
        });
        Mapper = mapperConfig.CreateMapper();

        MockPermissionService = new Mock<IPermissionService>();
        MockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        MockDbContextFactory = new Mock<IDbContextFactory<UNOPSAppDbContext>>();
        MockServiceProvider = new Mock<IServiceProvider>();
        MockExchangeRateService = new Mock<IExchangeRateService>();

        MockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var factoryAccessor = CreateMockHttpContextAccessor(PaoUserId.ToString());
                var factoryResolver = new UserResolverService<int>(factoryAccessor.Object, null);
                return TestDbContextFactory.CreateUNOPS(DbContextOptions, factoryResolver, mockDbSchema.Object);
            });

        // Setup HttpContext with user claims
        var testUser = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, PaoUserId.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "AI Test User"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "aiuser@unops.org")
            }, "TestAuthType"));
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(testUser);
        MockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);

        SeedTestData();

        Manager = new UNOPSOpportunityManager(
            Mapper,
            Context,
            Configuration,
            MockDbContextFactory.Object,
            MockExchangeRateService.Object,
            MockPermissionService.Object,
            MockHttpContextAccessor.Object,
            MockServiceProvider.Object
        );
    }

    protected static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string userId = "0")
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var identity = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
        }, "Test");
        accessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal(identity) });
        return accessor;
    }

    protected void SeedTestData()
    {
        var currency = Context.Currencies.FirstOrDefault(c => c.Code == "USD");
        if (currency == null)
        {
            currency = new Currency { Code = "USD", Name = "US Dollar", IsDeleted = false };
            Context.Currencies.Add(currency);
            Context.SaveChanges();
        }
        CurrencyId = currency.Id;

        var country = Context.Countries.FirstOrDefault(c => c.Iso2Code == "BD");
        if (country == null)
        {
            country = new Country { Name = "Bangladesh", Iso2Code = "BD" };
            Context.Countries.Add(country);
            Context.SaveChanges();
        }
        CountryId = country.Id;

        var orgHierarchy = Context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "SAH" && !o.IsDeleted);
        if (orgHierarchy == null)
        {
            orgHierarchy = new OrganizationHierarchy { Name = "South Asia Hub", Code = "SAH", Description = "South Asia", IsDeleted = false };
            Context.OrganizationHierarchies.Add(orgHierarchy);
            Context.SaveChanges();
        }
        OrgHierarchyId = orgHierarchy.Id;

        var proposedInitiativeType = Context.ProposedInitiativeTypes.FirstOrDefault(p => p.Name == "Project" && !p.IsDeleted);
        if (proposedInitiativeType == null)
        {
            proposedInitiativeType = new ProposedInitiativeType { Name = "Project", IsDeleted = false };
            Context.ProposedInitiativeTypes.Add(proposedInitiativeType);
            Context.SaveChanges();
        }
        ProposedInitiativeTypeId = proposedInitiativeType.Id;

        PaoUserId = TestDataHelper.GetOrCreateTestUser(Context, "aiuser@unops.org");

        var entityRole = Context.EntityRoles.FirstOrDefault(r => r.Code == "Opportunity_Manager_Opportunity" && !r.IsDeleted);
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
            Context.EntityRoles.Add(entityRole);
            Context.SaveChanges();
        }
        EntityRoleId = entityRole.Id;

        var partner = Context.Partners.FirstOrDefault(p => !p.IsDeleted);
        if (partner == null)
        {
            partner = new UNOPSPartner
            {
                Name = $"Test Partner {TestMarker}",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            Context.Partners.Add(partner);
            Context.SaveChanges();
        }
        PartnerId = partner.Id;

        Context.ChangeTracker.Clear();
    }

    protected async Task<int> CreateTestOpportunityAsync(
        string? name = null,
        string? description = null,
        string stage = "IDENTIFY & PROFILE",
        EntityStatus status = EntityStatus.Draft,
        decimal? budgetUSD = null,
        int? responsibleOrgUnitId = null)
    {
        var opportunity = new OpportunityEntity
        {
            Name = name ?? $"Test Opportunity {TestMarker}",
            Description = description ?? "Test Description",
            Stage = stage,
            Status = status,
            CreatedBy = PaoUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedBy = PaoUserId,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = false,
            InitiativeBudgetUSD = budgetUSD,
            ResponsibleOrgUnitId = responsibleOrgUnitId
        };
        Context.Opportunities.Add(opportunity);
        await Context.SaveChangesAsync();
        CreatedOpportunityIds.Add(opportunity.Id);
        return opportunity.Id;
    }

    public virtual void Dispose() => Transaction?.Dispose();
}
