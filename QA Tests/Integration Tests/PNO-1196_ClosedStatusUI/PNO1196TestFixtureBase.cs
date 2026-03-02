/**
 * @fileoverview PNO-1196 test fixture base — Closed status UI changes.

 * Provides an in-memory AppDbContext, seeded Opportunity helpers with configurable stage,
 * SeedClosedOpportunityAsync, and helper to change opportunity stage.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1196.ClosedStatusUI;

/// <summary>
/// Base fixture for PNO-1196 (Closed status UI changes) tests.
/// Provides in-memory AppDbContext, seed helpers for opportunities with various stages,
/// and helpers to change opportunity stage.
/// </summary>
public abstract class PNO1196TestFixtureBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    private bool _disposed;

    protected PNO1196TestFixtureBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
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
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        DbContext = new AppDbContext(options, userResolverService, mockSchema.Object);
    }

    // ──────────────────────────────────────────────────────────────
    // Seed helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds an opportunity with configurable stage and status.
    /// </summary>
    protected async Task<Opportunity> SeedOpportunityAsync(
        int id,
        string stage = "IDENTIFY & PROFILE",
        EntityStatus status = EntityStatus.Active,
        int? responsibleOrgUnitId = 1,
        string? name = null,
        string? description = null,
        decimal? initiativeBudgetUSD = 50000m)
    {
        var existing = await DbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.Stage = stage;
            existing.Status = status;
            await DbContext.SaveChangesAsync();
            return existing;
        }

        await EnsureOrgUnitExistsAsync(responsibleOrgUnitId ?? 1);

        var opportunity = new Opportunity
        {
            Id = id,
            Name = name ?? $"PNO-1196 Test Opportunity {id}",
            Description = description ?? "Test opportunity for Closed status UI",
            Stage = stage,
            Status = status,
            IsDeleted = false,
            ResponsibleOrgUnitId = responsibleOrgUnitId,
            InitiativeBudgetUSD = initiativeBudgetUSD,
            BeneficiariesToBeDetermined = true,
            UNOPSMissionsNotApplicable = true
        };
        DbContext.Opportunities.Add(opportunity);
        await DbContext.SaveChangesAsync();
        return opportunity;
    }

    /// <summary>
    /// Seeds a closed opportunity (Status = Closed, Stage = NO GO).
    /// </summary>
    protected async Task<Opportunity> SeedClosedOpportunityAsync(
        int id,
        string stage = "NO GO",
        int? responsibleOrgUnitId = 1)
    {
        return await SeedOpportunityAsync(id, stage, EntityStatus.Closed, responsibleOrgUnitId);
    }

    /// <summary>
    /// Changes the stage of an existing opportunity.
    /// </summary>
    protected async Task<Opportunity?> ChangeOpportunityStageAsync(int id, string newStage)
    {
        var opp = await DbContext.Opportunities.FindAsync(id);
        if (opp == null) return null;

        opp.Stage = newStage;
        opp.LastModifiedBy = 1;
        opp.LastModifiedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();
        return opp;
    }

    /// <summary>
    /// Changes the status of an existing opportunity.
    /// </summary>
    protected async Task<Opportunity?> ChangeOpportunityStatusAsync(int id, EntityStatus newStatus)
    {
        var opp = await DbContext.Opportunities.FindAsync(id);
        if (opp == null) return null;

        opp.Status = newStatus;
        opp.LastModifiedBy = 1;
        opp.LastModifiedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();
        return opp;
    }

    /// <summary>
    /// Transitions an opportunity to Closed status.
    /// </summary>
    protected async Task<Opportunity?> TransitionToClosedAsync(int id, string stage = "NO GO")
    {
        var opp = await DbContext.Opportunities.FindAsync(id);
        if (opp == null) return null;

        opp.Stage = stage;
        opp.Status = EntityStatus.Closed;
        opp.WorkflowStatus = WorkflowStatus.None;
        opp.LastModifiedBy = 1;
        opp.LastModifiedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();
        return opp;
    }

    private async Task EnsureOrgUnitExistsAsync(int id)
    {
        if (await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == id))
            return;

        DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
        {
            Id = id,
            Name = $"Test OrgUnit {id}",
            Code = $"OU{id}",
            Description = $"Org unit {id}",
            Type = OrganizationUnitType.OrgUnit,
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await DbContext.SaveChangesAsync();
    }

    // ──────────────────────────────────────────────────────────────
    // IDisposable
    // ──────────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DbContext.Dispose();
    }
}
