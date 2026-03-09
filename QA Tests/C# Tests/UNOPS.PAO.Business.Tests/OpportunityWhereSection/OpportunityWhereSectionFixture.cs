/**
 * @fileoverview Shared fixture for Opportunity WHERE Section tests.
 * Provides UNOPSOpportunityManager, seeded countries, and test opportunity.
 * @author UNOPS Opportunity+ QA Team
 */

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.OpportunityWhereSection;

/// <summary>
/// Shared fixture for WHERE section tests. Seeds countries, org units, and provides UNOPSOpportunityManager.
/// </summary>
public class OpportunityWhereSectionFixture : IDisposable
{
    public UNOPSOpportunityManager Manager { get; }
    public UNOPSAppDbContext Context { get; }
    public int CountryId1 { get; }
    public int CountryId2 { get; }
    public int CountryId3 { get; }
    public int OpportunityId { get; }
    public int PaoUserId { get; }

    private readonly DbContextOptions<UNOPSAppDbContext> _options;
    private IDbContextTransaction? _transaction;
    private readonly List<int> _createdOpportunityIds = new();

    public OpportunityWhereSectionFixture()
    {
        _options = TestEnvironment.CreateUNOPSDbContextOptions($"WhereSection_{Guid.NewGuid()}");
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");

        var tempAccessor = CreateMockHttpContextAccessor("0");
        var tempResolver = new UserResolverService<int>(tempAccessor.Object, null);
        using (var tempCtx = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_options, tempResolver, mockSchema.Object))
        {
            PaoUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "wheretest@unops.org");
        }

        var mainAccessor = CreateMockHttpContextAccessor(PaoUserId.ToString());
        var userResolver = new UserResolverService<int>(mainAccessor.Object, null);
        Context = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_options, userResolver, mockSchema.Object);

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
                return UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_options, fr, mockSchema.Object);
            });

        var mockExchangeRate = new Mock<IExchangeRateService>();
        var mockPermission = new Mock<IPermissionService>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        Manager = new UNOPSOpportunityManager(
            mapper,
            Context,
            config,
            mockDbContextFactory.Object,
            mockExchangeRate.Object,
            mockPermission.Object,
            CreateMockHttpContextAccessor(PaoUserId.ToString()).Object,
            mockServiceProvider.Object);

        CountryId1 = SeedCountry("BD", "Bangladesh");
        CountryId2 = SeedCountry("NP", "Nepal");
        CountryId3 = SeedCountry("IN", "India");
        OpportunityId = CreateTestOpportunity();
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

    private int SeedCountry(string iso2, string name)
    {
        var c = Context.Countries.FirstOrDefault(x => x.Iso2Code == iso2);
        if (c == null)
        {
            c = new Country { Name = name, Iso2Code = iso2 };
            Context.Countries.Add(c);
            Context.SaveChanges();
        }
        return c.Id;
    }

    private int CreateTestOpportunity()
    {
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = $"WHERE Test Opp {Guid.NewGuid():N}",
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
