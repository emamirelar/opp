/// <summary>
/// Fixture for AdvancedSearchService unit tests.
/// Provides InMemory/SQLite DbContext, mocked GlobalFilterService dependencies,
/// AutoMapper, and seeded Partner/Contact/Interaction/Opportunity data.
/// </summary>

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.Services.AdvancedSearchServiceTests;

public class AdvancedSearchServiceFixture : IDisposable
{
    private readonly string _dbName;
    protected readonly DbContextOptions<UNOPSAppDbContext> DbContextOptions;
    protected readonly UNOPSAppDbContext Context;
    protected readonly AdvancedSearchService Service;
    protected readonly System.Security.Claims.ClaimsPrincipal TestUser;

    public AdvancedSearchServiceFixture()
    {
        _dbName = $"AdvSearch_{Guid.NewGuid():N}";
        DbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions(_dbName);
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");
        var userResolver = new UserResolverService<int>(TestDbContextFactory.CreateMockHttpContextAccessor("1").Object);
        Context = TestDbContextFactory.CreateUNOPS(DbContextOptions, userResolver, mockSchema.Object);
        TestEnvironment.EnsureCleanDatabase(Context);

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        var mapper = mapperConfig.CreateMapper();

        var mockUserPreferenceService = new Mock<IUserPreferenceService>();
        mockUserPreferenceService.Setup(x => x.GetGlobalFiltersAsync(It.IsAny<string>()))
            .ReturnsAsync((UNOPS.PAO.Domain.Entities.GlobalFilters?)null);

        var mockOrgUnitService = new Mock<IOrgUnitHierarchyService>();
        var mockLoggerGfs = new Mock<Microsoft.Extensions.Logging.ILogger<GlobalFilterService>>();
        var globalFilterService = new GlobalFilterService(
            mockUserPreferenceService.Object,
            mockLoggerGfs.Object,
            Context,
            mockOrgUnitService.Object);

        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<AdvancedSearchService>>();

        Service = new AdvancedSearchService(
            Context,
            mockLogger.Object,
            mapper,
            globalFilterService,
            null);

        TestUser = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "test@unops.org"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Test User")
            }, "TestAuth"));
    }

    protected void SeedPartners(int count = 3)
    {
        for (var i = 0; i < count; i++)
        {
            Context.Partners.Add(new UNOPSPartner
            {
                Name = $"Partner {i} Searchable",
                PartnerShortDescription = $"Short desc {i}",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        Context.SaveChanges();
    }

    protected void SeedContacts(int count = 3)
    {
        var partner = Context.Partners.FirstOrDefault(p => !p.IsDeleted);
        if (partner == null)
        {
            Context.Partners.Add(new UNOPSPartner { Name = "Default Partner", Status = EntityStatus.Active, IsDeleted = false });
            Context.SaveChanges();
            partner = Context.Partners.First();
        }
        for (var i = 0; i < count; i++)
        {
            Context.Contacts.Add(new UNOPSContact
            {
                FirstName = $"First{i}",
                LastName = $"Last{i} Searchable",
                Title = "Manager",
                Email = $"contact{i}@test.com",
                PartnerId = partner.Id,
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        Context.SaveChanges();
    }

    protected void SeedInteractions(int count = 2)
    {
        for (var i = 0; i < count; i++)
        {
            Context.Interactions.Add(new UNOPSInteraction
            {
                Subject = $"Meeting {i} Searchable",
                Description = $"Description {i}",
                Type = Domain.Enums.InteractionType.VirtualMeeting,
                Date = DateTime.UtcNow,
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        Context.SaveChanges();
    }

    protected void SeedOpportunities(int count = 2)
    {
        for (var i = 0; i < count; i++)
        {
            Context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Name = $"Opportunity {i} Searchable",
                Description = $"Description {i}",
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Draft,
                IsDeleted = false
            });
        }
        Context.SaveChanges();
    }

    public void Dispose() => Context.Dispose();
}
