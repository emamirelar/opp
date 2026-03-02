/**
 * @fileoverview Shared test fixture base for BugFix regression tests.
 * Covers Global Filters, Stakeholder, DoA Prefix, Exchange Rate, and Search Icons fixes.
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

namespace UNOPS.PAO.IntegrationTests.BugFixRegressions;

/// <summary>
/// Shared fixture base for BugFix regression tests. Provides in-memory AppDbContext
/// and seed helpers for opportunities, stakeholders, org units, and exchange rates.
/// </summary>
public abstract class BugFixRegressionTestFixtureBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    protected readonly DbContextOptions<AppDbContext> DbOptions;

    protected BugFixRegressionTestFixtureBase()
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
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockDbContextSchema = new Mock<IDbContextSchema>();
        mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        DbContext = new AppDbContext(DbOptions, userResolverService, mockDbContextSchema.Object);
    }

    /// <summary>Seeds an organization hierarchy for org unit filtering.</summary>
    protected async Task SeedOrgUnitAsync(int id = 1, string name = "UNOPS HQ", string code = "HQ")
    {
        if (!await DbContext.OrganizationHierarchies.AnyAsync(oh => oh.Id == id))
        {
            DbContext.OrganizationHierarchies.Add(new OrganizationHierarchy
            {
                Id = id,
                Name = name,
                Code = code,
                Description = $"{name} org unit",
                IsDeleted = false,
                Status = EntityStatus.Active
            });
            await DbContext.SaveChangesAsync();
        }
    }

    /// <summary>Seeds an opportunity for filter and stakeholder tests.</summary>
    protected async Task<Opportunity> SeedOpportunityAsync(
        int id = 1,
        string name = "Test Opportunity",
        int? orgUnitId = 1,
        decimal? budgetUsd = 50000m)
    {
        await SeedOrgUnitAsync(orgUnitId ?? 1);
        var existing = await DbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.Name = name;
            existing.ResponsibleOrgUnitId = orgUnitId;
            existing.InitiativeBudgetUSD = budgetUsd;
        }
        else
        {
            DbContext.Opportunities.Add(new Opportunity
            {
                Id = id,
                Name = name,
                Description = "Test opportunity description",
                ResponsibleOrgUnitId = orgUnitId,
                InitiativeBudgetUSD = budgetUsd,
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await DbContext.SaveChangesAsync();
        return (await DbContext.Opportunities.FindAsync(id))!;
    }

    /// <summary>Seeds a PAOUser for stakeholder tests.</summary>
    protected async Task SeedUserAsync(int id, string email = "user@unops.org")
    {
        if (!await DbContext.PAOUsers.AnyAsync(u => u.Id == id))
        {
            DbContext.PAOUsers.Add(new PAOUser { Id = id, Email = email });
            await DbContext.SaveChangesAsync();
        }
    }

    /// <summary>Seeds an EntityRole (e.g., DoA Level 2).</summary>
    protected async Task<EntityRole> SeedEntityRoleAsync(int id = 1, string name = "DoA Level 2", string? doaPrefix = "DoA2")
    {
        var existing = await DbContext.EntityRoles.FindAsync(id);
        if (existing != null)
        {
            existing.Name = name;
            return existing;
        }
        DbContext.EntityRoles.Add(new EntityRole
        {
            Id = id,
            Name = name,
            EntityType = "Opportunity",
            IsDeleted = false,
            Status = EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();
        return (await DbContext.EntityRoles.FindAsync(id))!;
    }

    /// <summary>Seeds an OpportunityStakeholder.</summary>
    protected async Task<OpportunityStakeholder> SeedStakeholderAsync(
        int opportunityId,
        int userId,
        int entityRoleId = 1,
        int? orgHierarchyId = null,
        bool isDeleted = false)
    {
        await SeedUserAsync(userId);
        await SeedEntityRoleAsync(entityRoleId);
        var existing = await DbContext.OpportunityStakeholders
            .FirstOrDefaultAsync(s => s.OpportunityId == opportunityId && s.UserId == userId && !s.IsDeleted);
        if (existing != null && !isDeleted)
            return existing;

        DbContext.OpportunityStakeholders.Add(new OpportunityStakeholder
        {
            OpportunityId = opportunityId,
            UserId = userId,
            EntityRoleId = entityRoleId,
            OrganizationHierarchyId = orgHierarchyId,
            IsInternal = true,
            Name = $"Stakeholder-{userId}",
            IsDeleted = isDeleted
        });
        await DbContext.SaveChangesAsync();
        return (await DbContext.OpportunityStakeholders
            .FirstOrDefaultAsync(s => s.OpportunityId == opportunityId && s.UserId == userId))!;
    }

    /// <summary>Seeds an ExchangeRate for conversion tests.</summary>
    protected async Task SeedExchangeRateAsync(int id, string currency = "EUR", decimal rate = 1.18m)
    {
        if (!await DbContext.ExchangeRates.AnyAsync(e => e.Id == id))
        {
            DbContext.ExchangeRates.Add(new ExchangeRate
            {
                Id = id,
                Currency = currency,
                Exchange_Rate = rate,
                Effective_Date = DateTime.UtcNow.Date,
                Name = $"{currency} - Rate: {rate}",
                IsDeleted = false
            });
            await DbContext.SaveChangesAsync();
        }
    }

    /// <summary>Gets DoA prefix for display (simulates fixed DoA prefix logic).</summary>
    protected static string GetDoAPrefix(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
            return string.Empty;
        return level.Trim();
    }

    public virtual void Dispose()
    {
        DbContext.Dispose();
    }
}
