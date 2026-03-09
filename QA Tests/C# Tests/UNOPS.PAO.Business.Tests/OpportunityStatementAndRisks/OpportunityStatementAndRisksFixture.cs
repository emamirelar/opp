/**
 * @fileoverview Fixture for Opportunity Statement and Risk Register integration tests.
 * @author UNOPS Opportunity+ QA Team
 */

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.OpportunityStatementAndRisks;

/// <summary>
/// Base fixture for Opportunity Statement and Risk Register tests.
/// Provides DbContext, Mapper, and helper methods for seeding opportunities and statement data.
/// </summary>
public abstract class OpportunityStatementAndRisksFixture : IDisposable
{
    protected readonly DbContextOptions<UNOPSAppDbContext> DbContextOptions;
    protected readonly UNOPSAppDbContext Context;
    protected readonly IMapper Mapper;
    protected readonly string TestMarker = $"OSR_{Guid.NewGuid():N}";

    protected OpportunityStatementAndRisksFixture()
    {
        DbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OSR_{Guid.NewGuid():N}");
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");
        var mockAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userResolver = new UserResolverService<int>(mockAccessor.Object, null);
        Context = TestDbContextFactory.CreateUNOPS(DbContextOptions, userResolver, mockSchema.Object);
        TestEnvironment.EnsureCleanDatabase(Context);

        var config = new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        Mapper = config.CreateMapper();
    }

    protected async Task<int> SeedOpportunityAsync(
        string? opportunityStatementMarkdown = null,
        bool highRisksAcknowledged = false,
        string? stage = null)
    {
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = $"Test Opp {TestMarker}",
            Description = "Test",
            Stage = stage ?? OpportunityWorkflow.Stages.IdentifyAndProfile,
            Status = EntityStatus.Draft,
            IsDeleted = false,
            OpportunityStatementMarkdown = opportunityStatementMarkdown,
            HighRisksAcknowledged = highRisksAcknowledged
        };
        Context.Opportunities.Add(opp);
        await Context.SaveChangesAsync();
        return opp.Id;
    }

    public virtual void Dispose() => Context.Dispose();
}
