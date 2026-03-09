using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Mapping;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerTests;

/// <summary>
/// Base fixture for OpportunityManager tests.
/// Uses UNOPSAppDbContext (which inherits AppDbContext) to get the full model
/// including AspNetUsers and all navigation properties required by string-based includes.
/// 
/// On PostgreSQL: Seeds a test user (Id=1) to satisfy FK constraints from AuditableDbContext.
/// On SQLite: Tests requiring complex includes should use [SkipIfInMemoryFact].
/// </summary>
public abstract class OpportunityManagerTestFixtureBase : IDisposable
{
    protected readonly DbContextOptions<UNOPSAppDbContext> DbContextOptions;
    protected readonly UNOPSAppDbContext Context;
    protected readonly IMapper Mapper;
    protected readonly OpportunityManager Manager;
    protected readonly string TestMarker = $"OppMgr_{Guid.NewGuid():N}";
    protected int TestUserId { get; private set; }

    protected OpportunityManagerTestFixtureBase()
    {
        DbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OppMgr_{Guid.NewGuid():N}");
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");

        if (TestEnvironment.UsePostgreSQL)
        {
            // Phase 1: Resolve test user with a temporary context
            using var tempCtx = TestDbContextFactory.CreateUNOPS(DbContextOptions);
            TestUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "oppmgr-test@unops.org");

            // Phase 2: Create main context with correct user ID in claims
            Context = TestDbContextFactory.CreateUNOPSWithUserId(DbContextOptions, TestUserId);
        }
        else
        {
            TestUserId = 1;
            var mockAccessor = TestDbContextFactory.CreateMockHttpContextAccessor("1");
            var userResolver = new UserResolverService<int>(mockAccessor.Object);
            Context = new UNOPSAppDbContext(DbContextOptions, userResolver, mockSchema.Object);
            TestEnvironment.EnsureCleanDatabase(Context);
        }

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OpportunityMappingProfile>());
        Mapper = config.CreateMapper();
        Manager = new OpportunityManager(Mapper, Context);
    }

    /// <summary>
    /// Seeds a minimal opportunity in IDENTIFY & PROFILE stage (mutable).
    /// </summary>
    protected async Task<int> SeedOpportunityAsync(string? stage = "IDENTIFY & PROFILE", bool isDeleted = false)
    {
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = $"Test Opp {TestMarker}",
            Description = "Test description",
            Stage = stage ?? "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            IsDeleted = isDeleted
        };
        Context.Opportunities.Add(opp);
        await Context.SaveChangesAsync();
        return opp.Id;
    }

    /// <summary>
    /// Seeds an opportunity in an immutable stage (GO, NO GO, CANCELLED).
    /// </summary>
    protected async Task<int> SeedImmutableOpportunityAsync(string stage = "GO")
    {
        return await SeedOpportunityAsync(stage, false);
    }

    /// <summary>
    /// Seeds a soft-deleted opportunity.
    /// </summary>
    protected async Task<int> SeedSoftDeletedOpportunityAsync()
    {
        return await SeedOpportunityAsync("IDENTIFY & PROFILE", true);
    }

    /// <summary>
    /// Seeds a Currency for funding partner tests.
    /// </summary>
    protected async Task<int> SeedCurrencyAsync(string code = "USD")
    {
        var existing = await Context.Set<Currency>().FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
        if (existing != null) return existing.Id;
        var currency = new Currency { Name = code, Code = code, Status = EntityStatus.Active, IsDeleted = false };
        Context.Set<Currency>().Add(currency);
        await Context.SaveChangesAsync();
        return currency.Id;
    }

    /// <summary>
    /// Seeds a Partner for funding/client partner tests.
    /// Uses Set&lt;Partner&gt;() for compatibility with both AppDbContext and UNOPSAppDbContext.
    /// </summary>
    protected async Task<int> SeedPartnerAsync()
    {
        var partner = new Partner
        {
            Name = $"Partner {TestMarker}",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Set<Partner>().Add(partner);
        await Context.SaveChangesAsync();
        return partner.Id;
    }

    /// <summary>
    /// Seeds an OrganizationHierarchy for org unit tests.
    /// </summary>
    protected async Task<int> SeedOrgUnitAsync()
    {
        var orgUnit = new OrganizationHierarchy
        {
            Name = $"OrgUnit {TestMarker}",
            Code = $"OU_{TestMarker}",
            Description = "Test org unit",
            Type = OrganizationUnitType.OrgUnit,
            IsDeleted = false
        };
        Context.OrganizationHierarchies.Add(orgUnit);
        await Context.SaveChangesAsync();
        return orgUnit.Id;
    }

    /// <summary>
    /// Seeds a ProposedInitiativeType.
    /// </summary>
    protected async Task<int> SeedInitiativeTypeAsync()
    {
        var initType = new ProposedInitiativeType
        {
            Name = $"Initiative {TestMarker}",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Set<ProposedInitiativeType>().Add(initType);
        await Context.SaveChangesAsync();
        return initType.Id;
    }

    /// <summary>
    /// Seeds an SDG for Why section tests.
    /// </summary>
    protected async Task<int> SeedSDGAsync(string sdgId = "1", string name = "No Poverty")
    {
        var existing = await Context.SDGs.FirstOrDefaultAsync(s => s.SDGId == sdgId && !s.IsDeleted);
        if (existing != null) return existing.Id;
        var sdg = new SDG
        {
            SDGId = sdgId,
            SDGNumber = sdgId,
            Name = name,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        return sdg.Id;
    }

    /// <summary>
    /// Seeds a Country for Where section tests.
    /// </summary>
    protected async Task<int> SeedCountryAsync(string iso2 = "XX", string name = "Test Country")
    {
        var existing = await Context.Countries.FirstOrDefaultAsync(c => c.Iso2Code == iso2 && !c.IsDeleted);
        if (existing != null) return existing.Id;
        var country = new Country
        {
            Name = name,
            Iso2Code = iso2,
            Iso3Code = iso2 + "X",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();
        return country.Id;
    }

    /// <summary>
    /// Seeds an Output for deliverable tests.
    /// </summary>
    protected async Task<int> SeedOutputAsync()
    {
        var output = new Output
        {
            Name = $"Output {TestMarker}",
            Level0 = "Test Level",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Set<Output>().Add(output);
        await Context.SaveChangesAsync();
        return output.Id;
    }

    public virtual void Dispose() => Context.Dispose();
}
