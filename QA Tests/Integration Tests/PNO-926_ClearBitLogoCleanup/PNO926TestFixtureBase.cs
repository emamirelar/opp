/**
 * @fileoverview PNO-926 shared fixture base for ClearBit URL cleanup tests.
 * Migration: 20260216181625_ClearClearbitLogoUrlsFromPartners
 * Tests Partner.LogoUrl cleanup behavior and fallback display logic.
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

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// Shared fixture base for PNO-926: ClearBit logo URL cleanup and partner display logic.
/// Uses AppDbContext with InMemory provider. No workflow dependency.
/// </summary>
public abstract class PNO926TestFixtureBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    protected readonly DbContextOptions<AppDbContext> DbOptions;
    protected const string ClearbitBaseUrl = "https://logo.clearbit.com/";
    protected const string FallbackImage = "assets/images/Partner.png";

    protected PNO926TestFixtureBase()
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

    /// <summary>Seeds a partner with a specified LogoUrl.</summary>
    protected async Task<Partner> SeedPartnerAsync(int id, string? logoUrl, string name = null!)
    {
        var existing = await DbContext.Partners.FindAsync(id);
        if (existing != null)
        {
            existing.LogoUrl = logoUrl;
            await DbContext.SaveChangesAsync();
            return existing;
        }

        var partner = new Partner
        {
            Id = id,
            Name = name ?? $"Test Partner {id}",
            LogoUrl = logoUrl,
            IsDeleted = false,
            Status = EntityStatus.Active
        };
        DbContext.Partners.Add(partner);
        await DbContext.SaveChangesAsync();
        return partner;
    }

    /// <summary>
    /// Simulates the migration: sets LogoUrl to NULL for partners with 'clearbit' in URL (case-insensitive).
    /// Mirrors the actual migration: 20260216181625_ClearClearbitLogoUrlsFromPartners.
    /// </summary>
    protected async Task<int> RunClearbitCleanupMigrationAsync()
    {
        var affected = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.LogoUrl != null)
            .ToListAsync();

        affected = affected.Where(p => p.LogoUrl!.Contains("clearbit", StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var p in affected)
            p.LogoUrl = null;

        await DbContext.SaveChangesAsync();
        return affected.Count;
    }

    /// <summary>
    /// Determines the effective display URL (mirrors Angular component's display logic).
    /// Returns FallbackImage when URL is null, empty, whitespace-only, or still contains 'clearbit'.
    /// </summary>
    protected static string GetEffectiveLogoUrl(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            return FallbackImage;
        if (logoUrl.Contains("clearbit", StringComparison.OrdinalIgnoreCase))
            return FallbackImage;
        return logoUrl;
    }

    public virtual void Dispose()
    {
        DbContext.Dispose();
    }
}
