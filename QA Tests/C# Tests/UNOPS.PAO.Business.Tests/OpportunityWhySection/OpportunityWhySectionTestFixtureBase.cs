using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Base fixture for Opportunity WHY Section tests.
/// Provides DbContext, Mapper, and helper methods for seeding SDGs, UNCF, and opportunities.
/// </summary>
public abstract class OpportunityWhySectionTestFixtureBase : IDisposable
{
    protected readonly DbContextOptions<UNOPSAppDbContext> DbContextOptions;
    protected readonly UNOPSAppDbContext Context;
    protected readonly IMapper Mapper;
    protected readonly string TestMarker = $"WHY_{Guid.NewGuid():N}";

    protected OpportunityWhySectionTestFixtureBase()
    {
        DbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"WhySection_{Guid.NewGuid():N}");
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");
        var mockAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userResolver = new UserResolverService<int>(mockAccessor.Object, null);
        Context = TestDbContextFactory.CreateUNOPS(DbContextOptions, userResolver, mockSchema.Object);
        TestEnvironment.EnsureCleanDatabase(Context);

        var config = new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        Mapper = config.CreateMapper();
    }

    protected async Task<int> SeedOpportunityAsync(string? challenges = null, string? expectedImpact = null, string? expectedOutcomes = null)
    {
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = $"Test Opp {TestMarker}",
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Draft,
            IsDeleted = false,
            Challenges = challenges,
            ExpectedImpact = expectedImpact,
            ExpectedOutcomes = expectedOutcomes
        };
        Context.Opportunities.Add(opp);
        await Context.SaveChangesAsync();
        return opp.Id;
    }

    protected async Task<int> SeedSDGAsync(string sdgId, string name)
    {
        var sdg = Context.SDGs.FirstOrDefault(s => s.SDGId == sdgId && !s.IsDeleted);
        if (sdg != null) return sdg.Id;
        sdg = new UNOPS.PAO.Domain.Entities.SDG
        {
            SDGId = sdgId,
            SDGNumber = sdgId,
            Name = name,
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active,
            IsDeleted = false
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        return sdg.Id;
    }

    public virtual void Dispose() => Context.Dispose();
}
