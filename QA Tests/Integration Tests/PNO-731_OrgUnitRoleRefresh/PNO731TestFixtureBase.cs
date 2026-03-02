/**
 * @fileoverview PNO-731 test fixture base — OrgUnit role refresh always triggered on update.
 * Provides an in-memory AppDbContext, seeded Opportunity + OrganizationHierarchy + EntityUserRoles,
 * and helpers for asserting stakeholder auto-population behaviour.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO731;

/// <summary>
/// Base fixture for PNO-731 tests.
/// PNO-731 fix: Remove the orgUnitChanged guard so AutoPopulateStakeholdersFromOrgUnitAsync
/// runs whenever ResponsibleOrgUnitId is present in the update request, not only when the
/// OrgUnit value actually differs from the stored value.
/// </summary>
public abstract class PNO731TestFixtureBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    private bool _disposed;

    protected PNO731TestFixtureBase()
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
        var identity = new ClaimsIdentity(claims);
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

    protected async Task<OrganizationHierarchy> SeedOrgUnitAsync(
        int id,
        string name = "Test OrgUnit",
        OrganizationUnitType type = OrganizationUnitType.OrgUnit)
    {
        var existing = await DbContext.Set<OrganizationHierarchy>().FindAsync(id);
        if (existing != null) return existing;

        var orgUnit = new OrganizationHierarchy
        {
            Id = id,
            Name = name,
            Code = $"OU{id}",
            Description = $"Org unit {id}",
            Type = type,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        DbContext.Set<OrganizationHierarchy>().Add(orgUnit);
        await DbContext.SaveChangesAsync();
        return orgUnit;
    }

    protected async Task<Opportunity> SeedOpportunityAsync(
        int id,
        int responsibleOrgUnitId,
        string stage = "IDENTIFY & PROFILE")
    {
        var existing = await DbContext.Opportunities.FindAsync(id);
        if (existing != null) return existing;

        var opportunity = new Opportunity
        {
            Id = id,
            Name = $"PNO-731 Test Opportunity {id}",
            Description = "Test opportunity for OrgUnit role refresh",
            Stage = stage,
            Status = EntityStatus.Active,
            IsDeleted = false,
            ResponsibleOrgUnitId = responsibleOrgUnitId,
            InitiativeBudgetUSD = 50000m,
            BeneficiariesToBeDetermined = true,
            UNOPSMissionsNotApplicable = true
        };
        DbContext.Opportunities.Add(opportunity);
        await DbContext.SaveChangesAsync();
        return opportunity;
    }

    protected async Task<EntityUserRole> SeedEntityUserRoleAsync(
        int id,
        int orgUnitId,
        int userId = 10,
        string roleName = "Opportunity Manager")
    {
        var existing = await DbContext.Set<EntityUserRole>().FindAsync(id);
        if (existing != null) return existing;

        // Ensure the EntityRole record exists
        if (!await DbContext.Set<EntityRole>().AnyAsync(r => r.Name == roleName))
        {
            DbContext.Set<EntityRole>().Add(new EntityRole
            {
                Id = id * 100,
                Name = roleName,
                EntityType = "Opportunity",
                IsDeleted = false,
                Status = EntityStatus.Active
            });
            await DbContext.SaveChangesAsync();
        }

        var entityRole = await DbContext.Set<EntityRole>()
            .FirstAsync(r => r.Name == roleName);

        var eur = new EntityUserRole
        {
            Id = id,
            // EntityUserRole links via EntityId/EntityType — not OrganizationHierarchyId
            EntityId = orgUnitId,
            EntityType = "OrganizationHierarchy",
            UserId = userId,
            EntityRoleId = entityRole.Id,
            IsDeleted = false,
            Status = EntityStatus.Active
        };
        DbContext.Set<EntityUserRole>().Add(eur);
        await DbContext.SaveChangesAsync();
        return eur;
    }

    protected async Task<int> GetStakeholderCountAsync(int opportunityId)
    {
        return await DbContext.Set<OpportunityStakeholder>()
            .CountAsync(s => s.OpportunityId == opportunityId && !s.IsDeleted);
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
