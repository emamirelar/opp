using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Base class for integration tests
/// Uses in-memory database with real services (no mocks)
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly UNOPSAppDbContext Context;
    protected readonly UNOPSOpportunityManager Manager;
    protected readonly IMapper Mapper;
    protected readonly ClaimsPrincipal TestUser;
    protected readonly IServiceProvider ServiceProvider;

    protected IntegrationTestBase()
    {
        // Setup in-memory database with a unique name per test
        var dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"IntegrationTestDb_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        var mockUserServiceHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockUserServiceHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary();
        
        var userServiceTestUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "testuser@unops.org")
        }, "TestAuthType"));
        
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockUserServiceHttpContext.Setup(m => m.User).Returns(userServiceTestUser);
        mockUserServiceHttpContext.Setup(m => m.Request).Returns(mockRequest.Object);
        mockUserServiceHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockUserServiceHttpContext.Object);

        var userResolverService = new UserResolverService<int>(mockUserServiceHttpContextAccessor.Object, null);
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        Context = new UNOPSAppDbContext(dbContextOptions, userResolverService, mockDbSchema.Object);

        // Setup real AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            // Add all profiles from the main application
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });
        Mapper = mapperConfig.CreateMapper();

        // Setup real Configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IsUNOPSOverride"] = "true",
                ["ExchangeRate:ApiKey"] = "test-key",
                ["ExchangeRate:BaseUrl"] = "https://test-api.example.com",
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test_db;",
                ["ConnectionStrings:DbSchema"] = "public",
                ["AISettings:DisableExternalCalls"] = "true",
                ["AISettings:ModelName"] = "gemini-pro",
                ["AISettings:ProjectId"] = "test-project",
                ["AISettings:Location"] = "us-central1",
                ["GoogleCloud:ProjectId"] = "test-project",
                ["GoogleCloud:PubSubTopic"] = "test-topic"
            })
            .Build();

        // Setup test user
        TestUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
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
        services.AddDbContext<UNOPSAppDbContext>(options => 
            options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}"));
        services.AddDbContextFactory<UNOPSAppDbContext>(options => 
            options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}"));
        
        // Setup mock permission service for testing (using Moq)
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(s => s.HasPermissionAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.CanPerformActionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<object>()))
            .ReturnsAsync(true);
        mockPermissionService.Setup(s => s.ApplyAccessControlFiltersAsync<It.IsAnyType>(It.IsAny<IQueryable<It.IsAnyType>>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((IQueryable<It.IsAnyType> query, ClaimsPrincipal user, string action, string entityName) => query);
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
        
        // Add test exchange rate service for testing
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

        // Seed test data
        SeedTestData();
    }

    protected virtual void SeedTestData()
    {
        // Seed Currencies
        Context.Currencies.AddRange(new[]
        {
            new Currency { Id = 1, Code = "USD", Name = "US Dollar", IsDeleted = false },
            new Currency { Id = 2, Code = "EUR", Name = "Euro", IsDeleted = false }
        });

        // Seed Countries
        Context.Countries.AddRange(new[]
        {
            new Country { Id = 1, Name = "Bangladesh", Iso2Code = "BD" },
            new Country { Id = 2, Name = "Nepal", Iso2Code = "NP" },
            new Country { Id = 3, Name = "Myanmar", Iso2Code = "MM" }
        });

        // Seed Organization Hierarchies
        Context.OrganizationHierarchies.AddRange(new[]
        {
            new OrganizationHierarchy 
            { 
                Id = 1, 
                Name = "South Asia Hub", 
                Code = "SAH", 
                Description = "South Asia Regional Hub", 
                IsDeleted = false 
            },
            new OrganizationHierarchy 
            { 
                Id = 2, 
                Name = "Bangladesh Office", 
                Code = "BDO", 
                Description = "Bangladesh Country Office", 
                ParentId = 1, 
                IsDeleted = false 
            }
        });

        // Workflow stages are now stored as string values in Opportunity.Stage property

        // Seed Proposed Initiative Types
        Context.ProposedInitiativeTypes.AddRange(new[]
        {
            new ProposedInitiativeType { Id = 1, Name = "Project", IsDeleted = false },
            new ProposedInitiativeType { Id = 2, Name = "Programme", IsDeleted = false },
            new ProposedInitiativeType { Id = 3, Name = "Advisory", IsDeleted = false }
        });

        // Seed test user
        Context.PAOUsers.Add(new PAOUser
        {
            Id = 1,
            Email = "testuser@unops.org"
        });

        Context.SaveChanges();
    }

    public virtual void Dispose()
    {
        Context.Database.EnsureDeleted();
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
        return new UNOPSAppDbContext(_options, mockUserService.Object, mockDbSchema.Object);
    }

    public async Task<UNOPSAppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var mockUserService = new Mock<UserResolverService<int>>(MockBehavior.Loose, new object?[] { null });
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");
        return await Task.FromResult(new UNOPSAppDbContext(_options, mockUserService.Object, mockDbSchema.Object));
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
