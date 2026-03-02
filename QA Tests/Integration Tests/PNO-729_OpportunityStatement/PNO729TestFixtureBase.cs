/**
 * @fileoverview PNO-729 shared test fixture base.
 * Fix opportunity statement markdown and "Closed" status display.
 * Tests: OpportunityStatementMarkdown content, EntityStatus.Closed persistence,
 * and opportunity lifecycle around statement generation.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO729;

/// <summary>
/// Shared fixture base for PNO-729: Opportunity statement fix and Closed status display.
/// Provides InMemory DB, seeding helpers for Opportunity entities with statement markdown
/// and EntityStatus.Closed scenarios.
/// </summary>
public abstract class PNO729TestFixtureBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    protected readonly DbContextOptions<AppDbContext> DbOptions;
    protected const string ClosedStatusColor = "light-red";
    protected const string DefaultMarkdown = "## Opportunity Statement\n\nThis is a test opportunity.";
    protected const string EmptyMarkdown = "";

    protected PNO729TestFixtureBase()
    {
        DbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Email, "test@unops.org")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        mockHttpContextAccessor.Setup(x => x.HttpContext)
            .Returns(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        DbContext = new AppDbContext(DbOptions, userResolverService, mockDbContextSchema.Object);
    }

    /// <summary>Seeds an Opportunity with given statement markdown and status.</summary>
    protected async Task<Opportunity> SeedOpportunityAsync(
        int id,
        string? statementMarkdown,
        EntityStatus status = EntityStatus.Active,
        string stage = "GO",
        string name = null!)
    {
        var existing = await DbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.OpportunityStatementMarkdown = statementMarkdown;
            existing.Status = status;
            existing.Stage = stage;
            await DbContext.SaveChangesAsync();
            return existing;
        }

        var opp = new Opportunity
        {
            Id = id,
            Name = name ?? $"Test Opportunity {id}",
            Description = "PNO-729 test opportunity",
            Stage = stage,
            Status = status,
            IsDeleted = false,
            InitiativeBudgetUSD = 750000m,
            Challenges = "Test challenges",
            ExpectedImpact = "Test expected impact",
            ExpectedOutcomes = "Test expected outcomes",
            BeneficiariesToBeDetermined = true,
            UNOPSMissionsNotApplicable = true,
            TargetSigningDate = DateTime.UtcNow.AddMonths(3),
            ImplementationStartDate = DateTime.UtcNow.AddMonths(4),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(18),
            OpportunityStatementMarkdown = statementMarkdown,
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1
        };

        DbContext.Opportunities.Add(opp);
        await DbContext.SaveChangesAsync();
        return opp;
    }

    /// <summary>Seeds a Closed opportunity (simulating post-reject state).</summary>
    protected async Task<Opportunity> SeedClosedOpportunityAsync(
        int id,
        string? statementMarkdown = null,
        string name = null!)
    {
        return await SeedOpportunityAsync(id, statementMarkdown ?? DefaultMarkdown,
            EntityStatus.Closed, "NO GO", name);
    }

    /// <summary>
    /// Seeds a Closed opportunity with an explicitly null statement markdown.
    /// Use this when testing migration behaviour that requires null to be stored.
    /// </summary>
    protected Task<Opportunity> SeedClosedOpportunityWithNullStatementAsync(int id, string name = null!)
        => SeedOpportunityAsync(id, null, EntityStatus.Closed, "NO GO", name);

    /// <summary>
    /// Determines the CSS color class for a given EntityStatus,
    /// mirroring the Angular component's display logic.
    /// </summary>
    protected static string GetStatusColorClass(EntityStatus status) => status switch
    {
        EntityStatus.Closed => ClosedStatusColor,
        EntityStatus.Active => "green",
        EntityStatus.Inactive => "grey",
        EntityStatus.Draft => "blue",
        _ => "grey"
    };

    /// <summary>
    /// Simulates the statement fix migration: ensures statement field is properly set.
    /// Returns count of updated records.
    /// </summary>
    protected async Task<int> RunStatementFixMigrationAsync()
    {
        var oppsWithNullStatement = await DbContext.Opportunities
            .Where(o => !o.IsDeleted && o.OpportunityStatementMarkdown == null)
            .ToListAsync();

        foreach (var o in oppsWithNullStatement)
            o.OpportunityStatementMarkdown = EmptyMarkdown;

        await DbContext.SaveChangesAsync();
        return oppsWithNullStatement.Count;
    }

    public virtual void Dispose()
    {
        DbContext.Dispose();
    }
}
