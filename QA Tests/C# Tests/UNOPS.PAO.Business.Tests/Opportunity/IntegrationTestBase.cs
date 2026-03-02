using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Base class for integration tests.
/// Supports both InMemory (default) and real PostgreSQL databases.
/// Set TEST_DB_CONNECTION_STRING env var to use real PostgreSQL.
/// 
/// When using PostgreSQL, Z.EntityFramework.Extensions (BulkUpdate, SingleUpdateAsync)
/// will work correctly — these operations require a relational database.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly UNOPSAppDbContext Context;
    protected readonly UNOPSOpportunityManager Manager;
    protected readonly IMapper Mapper;
    protected readonly ClaimsPrincipal TestUser;
    protected readonly IServiceProvider ServiceProvider;
    private IDbContextTransaction? _transaction;

    protected IntegrationTestBase()
    {
        var dbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions();
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        // Phase 1: Resolve the test user ID using a temporary context (outside transaction).
        // AuditableDbContext caches _currentUserId at construction, so we must know the
        // real user ID before creating the main context.
        int testUserId;
        {
            var tempAccessor = CreateMockHttpContextAccessor("0");
            var tempResolver = new UserResolverService<int>(tempAccessor.Object, null);
            using var tempCtx = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(dbContextOptions, tempResolver, mockDbSchema.Object);
            testUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "testuser@unops.org");
        }

        // Phase 2: Create the MAIN context with the ACTUAL test user ID in claims.
        var mainAccessor = CreateMockHttpContextAccessor(testUserId.ToString());
        var userResolverService = new UserResolverService<int>(mainAccessor.Object, null);
        Context = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(dbContextOptions, userResolverService, mockDbSchema.Object);

        // Begin transaction for PostgreSQL test isolation (rollback in Dispose)
        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = Context.Database.BeginTransaction();
        }

        // Seed reference data (test user already exists from Phase 1)
        SeedTestData();

        var actualUserId = SeededTestUserId.ToString();

        // Setup real AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });
        Mapper = mapperConfig.CreateMapper();

        var configuration = TestEnvironment.CreateTestConfiguration();

        TestUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, actualUserId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "testuser@unops.org"),
            new Claim(ClaimTypes.Role, "Administrator")
        }, "TestAuthType"));

        // Setup HttpContextAccessor with test user
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = TestUser
            }
        };

        // Setup DbContextFactory
        var dbContextFactory = new TestDbContextFactory(dbContextOptions);

        // Setup ServiceProvider with real services
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(Mapper);
        if (TestEnvironment.UsePostgreSQL)
        {
            if (TestEnvironment.DataSource != null)
            {
                services.AddDbContext<UNOPSAppDbContext>(options => options.UseNpgsql(TestEnvironment.DataSource));
                services.AddDbContextFactory<UNOPSAppDbContext>(options => options.UseNpgsql(TestEnvironment.DataSource));
            }
            else
            {
                var connStr = TestEnvironment.ConnectionString!;
                services.AddDbContext<UNOPSAppDbContext>(options => options.UseNpgsql(connStr));
                services.AddDbContextFactory<UNOPSAppDbContext>(options => options.UseNpgsql(connStr));
            }
        }
        else
        {
            var dbName = $"IntegrationTestDb_{Guid.NewGuid()}";
            services.AddDbContext<UNOPSAppDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddDbContextFactory<UNOPSAppDbContext>(options => options.UseInMemoryDatabase(dbName));
        }

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(s => s.HasPermissionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.CanPerformActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync("1");
        mockPermissionService.Setup(s => s.HasInstanceAccessAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.IsOpportunityTeamMemberAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.GetEffectiveRole(It.IsAny<ClaimsPrincipal>()))
            .Returns("Administrator");
        mockPermissionService.Setup(s => s.CanExport(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);
        mockPermissionService.Setup(s => s.CanImport(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);

        services.AddSingleton(mockPermissionService.Object);
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddSingleton<IExchangeRateService, TestExchangeRateService>();

        ServiceProvider = services.BuildServiceProvider();

        // Initialize manager with real dependencies
        Manager = new UNOPSOpportunityManager(
            Mapper,
            Context,
            configuration,
            dbContextFactory,
            ServiceProvider.GetRequiredService<IExchangeRateService>(),
            ServiceProvider.GetService<IPermissionService>(),
            httpContextAccessor,
            ServiceProvider
        );
    }

    /// <summary>IDs resolved during seeding, available to derived test classes.</summary>
    protected int SeededCurrencyUsdId { get; private set; }
    protected int SeededCurrencyEurId { get; private set; }
    protected int SeededCountryBdId { get; private set; }
    protected int SeededCountryNpId { get; private set; }
    protected int SeededCountryMmId { get; private set; }
    protected int SeededOrgHierarchySahId { get; private set; }
    protected int SeededOrgHierarchyBdoId { get; private set; }
    protected int SeededProposedInitProjectId { get; private set; }
    protected int SeededProposedInitProgrammeId { get; private set; }
    protected int SeededProposedInitAdvisoryId { get; private set; }
    protected int SeededTestUserId { get; private set; }

    protected virtual void SeedTestData()
    {
        // Seed Currencies (get-or-create to work with existing PostgreSQL data)
        var usd = Context.Currencies.FirstOrDefault(c => c.Code == "USD");
        if (usd == null) { usd = new Currency { Code = "USD", Name = "US Dollar", IsDeleted = false }; Context.Currencies.Add(usd); Context.SaveChanges(); }
        SeededCurrencyUsdId = usd.Id;

        var eur = Context.Currencies.FirstOrDefault(c => c.Code == "EUR");
        if (eur == null) { eur = new Currency { Code = "EUR", Name = "Euro", IsDeleted = false }; Context.Currencies.Add(eur); Context.SaveChanges(); }
        SeededCurrencyEurId = eur.Id;

        // Seed Countries
        var bd = Context.Countries.FirstOrDefault(c => c.Iso2Code == "BD");
        if (bd == null) { bd = new Country { Name = "Bangladesh", Iso2Code = "BD" }; Context.Countries.Add(bd); Context.SaveChanges(); }
        SeededCountryBdId = bd.Id;

        var np = Context.Countries.FirstOrDefault(c => c.Iso2Code == "NP");
        if (np == null) { np = new Country { Name = "Nepal", Iso2Code = "NP" }; Context.Countries.Add(np); Context.SaveChanges(); }
        SeededCountryNpId = np.Id;

        var mm = Context.Countries.FirstOrDefault(c => c.Iso2Code == "MM");
        if (mm == null) { mm = new Country { Name = "Myanmar", Iso2Code = "MM" }; Context.Countries.Add(mm); Context.SaveChanges(); }
        SeededCountryMmId = mm.Id;

        // Seed Organization Hierarchies
        var sah = Context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "SAH" && !o.IsDeleted);
        if (sah == null) { sah = new OrganizationHierarchy { Name = "South Asia Hub", Code = "SAH", Description = "South Asia Regional Hub", IsDeleted = false }; Context.OrganizationHierarchies.Add(sah); Context.SaveChanges(); }
        SeededOrgHierarchySahId = sah.Id;

        var bdo = Context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "BDO" && !o.IsDeleted);
        if (bdo == null) { bdo = new OrganizationHierarchy { Name = "Bangladesh Office", Code = "BDO", Description = "Bangladesh Country Office", ParentId = SeededOrgHierarchySahId, IsDeleted = false }; Context.OrganizationHierarchies.Add(bdo); Context.SaveChanges(); }
        SeededOrgHierarchyBdoId = bdo.Id;

        // Seed Proposed Initiative Types
        var project = Context.ProposedInitiativeTypes.FirstOrDefault(p => p.Name == "Project" && !p.IsDeleted);
        if (project == null) { project = new ProposedInitiativeType { Name = "Project", IsDeleted = false }; Context.ProposedInitiativeTypes.Add(project); Context.SaveChanges(); }
        SeededProposedInitProjectId = project.Id;

        var programme = Context.ProposedInitiativeTypes.FirstOrDefault(p => p.Name == "Programme" && !p.IsDeleted);
        if (programme == null) { programme = new ProposedInitiativeType { Name = "Programme", IsDeleted = false }; Context.ProposedInitiativeTypes.Add(programme); Context.SaveChanges(); }
        SeededProposedInitProgrammeId = programme.Id;

        var advisory = Context.ProposedInitiativeTypes.FirstOrDefault(p => p.Name == "Advisory" && !p.IsDeleted);
        if (advisory == null) { advisory = new ProposedInitiativeType { Name = "Advisory", IsDeleted = false }; Context.ProposedInitiativeTypes.Add(advisory); Context.SaveChanges(); }
        SeededProposedInitAdvisoryId = advisory.Id;

        // Seed test user (raw SQL for PostgreSQL to handle all AspNetUsers required columns)
        SeededTestUserId = TestDataHelper.GetOrCreateTestUser(Context, "testuser@unops.org");

        Context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Creates a mock IHttpContextAccessor configured with the given user ID claim.
    /// </summary>
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
            new Claim(ClaimTypes.Email, "testuser@unops.org")
        }, "TestAuthType"));
        httpContext.Setup(m => m.User).Returns(user);
        httpContext.Setup(m => m.Request).Returns(request.Object);
        accessor.Setup(m => m.HttpContext).Returns(httpContext.Object);
        return accessor;
    }

    public virtual void Dispose()
    {
        // For PostgreSQL: rollback transaction to undo all test data changes
        if (_transaction != null)
        {
            try { _transaction.Rollback(); } catch { }
            _transaction.Dispose();
            _transaction = null;
        }

        // For InMemory: cleanup is automatic. For PostgreSQL: do NOT delete the real database.
        if (TestEnvironment.UseInMemory)
        {
            try { Context.Database.EnsureDeleted(); }
            catch { /* SQLite connection may already be closed during concurrent test runs */ }
        }
        Context.Dispose();
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

/// <summary>
/// Test implementation of IDbContextFactory for integration tests
/// </summary>
public class TestDbContextFactory : IDbContextFactory<UNOPSAppDbContext>
{
    private readonly DbContextOptions<UNOPSAppDbContext> _options;

    public TestDbContextFactory(DbContextOptions<UNOPSAppDbContext> options)
    {
        _options = options;
    }

    public UNOPSAppDbContext CreateDbContext()
    {
        var mockUserService = new Mock<UserResolverService<int>>(MockBehavior.Loose, new object?[] { null });
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");
        return UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_options, mockUserService.Object, mockDbSchema.Object);
    }

    public async Task<UNOPSAppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(CreateDbContext());
    }
}

/// <summary>
/// Test implementation of IExchangeRateService for integration tests
/// Returns 1:1 exchange rate for all currencies (no external API calls)
/// </summary>
public class TestExchangeRateService : IExchangeRateService
{
    public Task<ExchangeRateResult> ConvertToUSDAsync(decimal amount, string currencyCode, DateTime? asOfDate = null)
    {
        // Return 1:1 conversion (no exchange rate applied in tests)
        return Task.FromResult(new ExchangeRateResult
        {
            AmountUSD = amount,
            ExchangeRate = 1.0m,
            ExchangeRateDate = asOfDate ?? DateTime.UtcNow,
            ExchangeRateId = 0
        });
    }

    public Task<decimal> GetExchangeRateAsync(string fromCurrency, DateTime? asOfDate = null)
    {
        // Return 1:1 exchange rate for all currencies in tests
        return Task.FromResult(1.0m);
    }
}
