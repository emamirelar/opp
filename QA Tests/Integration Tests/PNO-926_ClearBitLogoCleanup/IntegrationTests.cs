/**
 * @fileoverview PNO-926 Integration Tests — 50 end-to-end flow tests.
 * Full migration pipeline, DbContext round-trips, and cross-component behavior.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Integration Tests — 50 end-to-end and cross-component tests.
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Ticket", "PNO-926")]
public class IntegrationTests : PNO926TestFixtureBase
{
    // ─── §5.1 Full Migration Round-Trip (INT-001 – 015) ──────────────────

    [Fact] [Trait("TestId", "INT-001")]
    public async Task FullFlow_SeedClearbit_Migrate_VerifyNull()
    {
        await SeedPartnerAsync(10001, "https://logo.clearbit.com/integration.org", "Integration Partner");

        var affected = await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var partner = await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 10001);
        affected.Should().Be(1);
        partner.LogoUrl.Should().BeNull();
        partner.Name.Should().Be("Integration Partner");
    }

    [Fact] [Trait("TestId", "INT-002")]
    public async Task FullFlow_SeedSafe_Migrate_VerifyUnchanged()
    {
        await SeedPartnerAsync(10002, "https://safe.org/logo.png", "Safe Partner");

        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var partner = await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 10002);
        partner.LogoUrl.Should().Be("https://safe.org/logo.png");
    }

    [Fact] [Trait("TestId", "INT-003")]
    public async Task FullFlow_MixedPartners_ExactStateAfterMigration()
    {
        await SeedPartnerAsync(10003, "https://logo.clearbit.com/a.org", "A");
        await SeedPartnerAsync(10004, "https://safe.org/b.png", "B");
        await SeedPartnerAsync(10005, null, "C");

        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 10003)).LogoUrl.Should().BeNull();
        (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 10004)).LogoUrl.Should().NotBeNull();
        (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 10005)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-004")]
    public async Task FullFlow_ClearbitPartner_FallbackDisplayLogicCorrect()
    {
        await SeedPartnerAsync(10006, "https://logo.clearbit.com/fallback-int.org");
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 10006);
        GetEffectiveLogoUrl(p.LogoUrl).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "INT-005")]
    public async Task FullFlow_SafePartner_RealDisplayLogicCorrect()
    {
        await SeedPartnerAsync(10007, "https://my-partner.org/logo.svg");
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 10007);
        GetEffectiveLogoUrl(p.LogoUrl).Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "INT-006")]
    public async Task FullFlow_Idempotency_TwoRunsSameResult()
    {
        await SeedPartnerAsync(10008, "https://logo.clearbit.com/idem-int.org");

        await RunClearbitCleanupMigrationAsync();
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 10008)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-007")]
    public async Task FullFlow_10ClearbitPartners_AllNullAfterMigration()
    {
        for (var i = 10010; i <= 10019; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/int{i}.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        affected.Should().Be(10);
        for (var i = 10010; i <= 10019; i++)
            (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == i)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-008")]
    public async Task FullFlow_5SafePartners_AllUnchangedAfterMigration()
    {
        for (var i = 10020; i <= 10024; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");

        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        for (var i = 10020; i <= 10024; i++)
            (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == i)).LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-009")]
    public async Task FullFlow_DeletedClearbitPartner_NotAffected()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 10025, Name = "Deleted INT", IsDeleted = true,
            LogoUrl = "https://logo.clearbit.com/deleted-int.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
        DbContext.ChangeTracker.Clear();
        (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 10025)).LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-010")]
    public async Task FullFlow_NullLogoPartner_NotAffected()
    {
        await SeedPartnerAsync(10026, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-011")]
    public async Task FullFlow_EmptyLogoPartner_NotAffected()
    {
        await SeedPartnerAsync(10027, "");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-012")]
    public async Task FullFlow_ClearbitInQuery_NullAfterMigration()
    {
        await SeedPartnerAsync(10028, "https://example.com/logo.png?src=clearbit");
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == 10028)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-013")]
    public async Task FullFlow_PartnerCanBeUpdatedAfterMigration()
    {
        await SeedPartnerAsync(10029, "https://logo.clearbit.com/update.org", "Before Update");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(10029);
        p!.Name = "After Update";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 10029)).Name.Should().Be("After Update");
    }

    [Fact] [Trait("TestId", "INT-014")]
    public async Task FullFlow_NoExceptionOnEmptyDb()
    {
        var act = async () => await RunClearbitCleanupMigrationAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "INT-015")]
    public async Task FullFlow_AffectedCountMatchesDbNulls()
    {
        for (var i = 10030; i <= 10034; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/count{i}.org");
        for (var i = 10035; i <= 10039; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();
        var nullCount = await DbContext.Partners
            .Where(p => p.Id >= 10030 && p.Id <= 10034 && p.LogoUrl == null)
            .CountAsync();

        affected.Should().Be(nullCount);
    }

    // ─── §5.2 Cross-Component Interactions (INT-016 – 030) ───────────────

    [Fact] [Trait("TestId", "INT-016")]
    public async Task Integration_MultipleSeeds_IndependentIsolation()
    {
        await SeedPartnerAsync(11001, "https://logo.clearbit.com/iso1.org");
        await SeedPartnerAsync(11002, "https://logo.clearbit.com/iso2.org");
        await SeedPartnerAsync(11003, "https://safe.org/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(11001))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(11002))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(11003))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-017")]
    public async Task Integration_PartnerCount_ConsistentBeforeAndAfterMigration()
    {
        for (var i = 11010; i <= 11019; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/count{i}.org");

        var before = await DbContext.Partners.CountAsync();
        await RunClearbitCleanupMigrationAsync();
        var after = await DbContext.Partners.CountAsync();

        after.Should().Be(before);
    }

    [Fact] [Trait("TestId", "INT-018")]
    public async Task Integration_Migration_WithDifferentStatuses_AllClearbitNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner { Id = 11020, Name = "A", IsDeleted = false, LogoUrl = "https://logo.clearbit.com/a.org", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active });
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner { Id = 11021, Name = "I", IsDeleted = false, LogoUrl = "https://logo.clearbit.com/i.org", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive });
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner { Id = 11022, Name = "D", IsDeleted = false, LogoUrl = "https://logo.clearbit.com/d.org", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Draft });
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(3);
    }

    [Fact] [Trait("TestId", "INT-019")]
    public async Task Integration_GetEffectiveLogoUrl_ConsistentWithDbState()
    {
        await SeedPartnerAsync(11030, "https://logo.clearbit.com/consistent.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 11030);
        var effective = GetEffectiveLogoUrl(p.LogoUrl);

        p.LogoUrl.Should().BeNull();
        effective.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "INT-020")]
    public async Task Integration_SeedUpdateMigrate_OrderMatters()
    {
        await SeedPartnerAsync(11031, "https://safe.org/logo.png");
        var p = await DbContext.Partners.FindAsync(11031);
        p!.LogoUrl = "https://logo.clearbit.com/updated.org";
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(1);
        DbContext.ChangeTracker.Clear();
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 11031)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-021")]
    public async Task Integration_SeedClearbitUpdateToSafe_MigrateNotAffected()
    {
        await SeedPartnerAsync(11032, "https://logo.clearbit.com/change-to-safe.org");
        var p = await DbContext.Partners.FindAsync(11032);
        p!.LogoUrl = "https://safe.org/logo.png";
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-022")]
    public async Task Integration_ParallelSeedAndMigrate_NoException()
    {
        await SeedPartnerAsync(11033, "https://logo.clearbit.com/parallel.org");

        var act = async () => await Task.WhenAll(
            RunClearbitCleanupMigrationAsync(),
            Task.Run(async () => await DbContext.Partners.AsNoTracking().ToListAsync())
        );

        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "INT-023")]
    public async Task Integration_ChangeTrackerClear_StillReadable()
    {
        await SeedPartnerAsync(11034, "https://logo.clearbit.com/change-tracker.org");
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Partners.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 11034);

        p.Should().NotBeNull();
        p!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-024")]
    public async Task Integration_MigrationAffectedCount_0WhenAllSafe()
    {
        for (var i = 11040; i <= 11044; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-025")]
    public async Task Integration_MigrationAffectedCount_AllWhenAllClearbit()
    {
        for (var i = 11050; i <= 11054; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/all{i}.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(5);
    }

    // ─── §5.3 Persistence and State (INT-026 – 040) ──────────────────────

    [Fact] [Trait("TestId", "INT-026")]
    public async Task Persistence_LogoUrlNull_PersistedCorrectly()
    {
        await SeedPartnerAsync(12001, "https://logo.clearbit.com/persist.org");
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Set<UNOPS.PAO.Domain.Entities.Partner>()
            .AsNoTracking().FirstAsync(x => x.Id == 12001);

        p.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-027")]
    public async Task Persistence_SafeUrl_PersistedCorrectly()
    {
        await SeedPartnerAsync(12002, "https://safe.org/logo.png");
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Set<UNOPS.PAO.Domain.Entities.Partner>()
            .AsNoTracking().FirstAsync(x => x.Id == 12002);

        p.LogoUrl.Should().Be("https://safe.org/logo.png");
    }

    [Fact] [Trait("TestId", "INT-028")]
    public async Task Persistence_NullUrlPartner_PersistedAsNull()
    {
        await SeedPartnerAsync(12003, null);
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Set<UNOPS.PAO.Domain.Entities.Partner>()
            .AsNoTracking().FirstAsync(x => x.Id == 12003);

        p.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-029")]
    public async Task Persistence_AfterMigration_CountConsistent()
    {
        for (var i = 12010; i <= 12014; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/p{i}.org");

        var before = await DbContext.Partners.CountAsync();
        await RunClearbitCleanupMigrationAsync();
        var after = await DbContext.Partners.CountAsync();

        after.Should().Be(before);
    }

    [Fact] [Trait("TestId", "INT-030")]
    public async Task Persistence_ReloadedContext_SeesNullLogoUrl()
    {
        await SeedPartnerAsync(12015, "https://logo.clearbit.com/reload.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 12015);
        p.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-031")]
    public async Task Persistence_EffectiveLogo_CorrectForAllPartnersAfterMigration()
    {
        await SeedPartnerAsync(12020, "https://logo.clearbit.com/eff1.org");
        await SeedPartnerAsync(12021, "https://safe.org/eff2.png");
        await SeedPartnerAsync(12022, null);
        await RunClearbitCleanupMigrationAsync();

        var partners = await DbContext.Partners.AsNoTracking()
            .Where(p => p.Id >= 12020 && p.Id <= 12022)
            .ToListAsync();

        foreach (var p in partners)
        {
            var eff = GetEffectiveLogoUrl(p.LogoUrl);
            eff.Should().NotBeNull();
        }
    }

    // ─── §5.4 DbContext Operations (INT-032 – 050) ───────────────────────

    [Fact] [Trait("TestId", "INT-032")]
    public async Task DbContext_FindAsync_WorksAfterMigration()
    {
        await SeedPartnerAsync(13001, "https://logo.clearbit.com/find.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(13001);

        p.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-033")]
    public async Task DbContext_FirstOrDefault_WorksAfterMigration()
    {
        await SeedPartnerAsync(13002, "https://logo.clearbit.com/first.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FirstOrDefaultAsync(x => x.Id == 13002);

        p.Should().NotBeNull();
        p!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-034")]
    public async Task DbContext_WhereFilter_WorksAfterMigration()
    {
        await SeedPartnerAsync(13003, "https://logo.clearbit.com/where.org");
        await RunClearbitCleanupMigrationAsync();

        var partners = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.LogoUrl == null)
            .ToListAsync();

        partners.Should().NotBeEmpty();
    }

    [Fact] [Trait("TestId", "INT-035")]
    public async Task DbContext_CountAsync_WorksAfterMigration()
    {
        for (var i = 13010; i <= 13014; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/cnt{i}.org");
        await RunClearbitCleanupMigrationAsync();

        var count = await DbContext.Partners.CountAsync(p => p.Id >= 13010 && p.Id <= 13014);

        count.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-036")]
    public async Task DbContext_AddAfterMigration_Works()
    {
        await SeedPartnerAsync(13020, "https://logo.clearbit.com/add.org");
        await RunClearbitCleanupMigrationAsync();

        await SeedPartnerAsync(13021, "https://new-logo.org/logo.png");

        (await DbContext.Partners.FindAsync(13021)).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-037")]
    public async Task DbContext_UpdateAfterMigration_Works()
    {
        await SeedPartnerAsync(13022, "https://logo.clearbit.com/upd.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(13022);
        p!.Name = "Updated Post Migration";
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 13022)).Name
            .Should().Be("Updated Post Migration");
    }

    [Fact] [Trait("TestId", "INT-038")]
    public async Task DbContext_AnyAsync_ClearbitUrlNotPresent_AfterMigration()
    {
        await SeedPartnerAsync(13023, "https://logo.clearbit.com/any.org");
        await RunClearbitCleanupMigrationAsync();

        var hasClearbit = await DbContext.Partners
            .AnyAsync(p => !p.IsDeleted && p.LogoUrl != null && p.LogoUrl.Contains("clearbit"));

        hasClearbit.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "INT-039")]
    public async Task DbContext_SelectProjection_WorksAfterMigration()
    {
        await SeedPartnerAsync(13024, "https://logo.clearbit.com/proj.org", "Projection Partner");
        await RunClearbitCleanupMigrationAsync();

        var names = await DbContext.Partners
            .Where(p => p.Id == 13024)
            .Select(p => p.Name)
            .ToListAsync();

        names.Should().Contain("Projection Partner");
    }

    [Fact] [Trait("TestId", "INT-040")]
    public async Task DbContext_OrderBy_WorksAfterMigration()
    {
        for (var i = 13030; i <= 13034; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/ord{i}.org", $"Partner {i}");
        await RunClearbitCleanupMigrationAsync();

        var ordered = await DbContext.Partners
            .Where(p => p.Id >= 13030 && p.Id <= 13034)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ordered.Should().BeInAscendingOrder(p => p.Name);
    }

    [Fact] [Trait("TestId", "INT-041")]
    public async Task DbContext_SkipTake_WorksAfterMigration()
    {
        for (var i = 13040; i <= 13049; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/page{i}.org");
        await RunClearbitCleanupMigrationAsync();

        var page = await DbContext.Partners
            .Where(p => p.Id >= 13040 && p.Id <= 13049)
            .OrderBy(p => p.Id)
            .Skip(2).Take(3)
            .ToListAsync();

        page.Should().HaveCount(3);
    }

    [Fact] [Trait("TestId", "INT-042")]
    public async Task DbContext_AsNoTracking_WorksAfterMigration()
    {
        await SeedPartnerAsync(13050, "https://logo.clearbit.com/notrack.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 13050);

        p.Should().NotBeNull();
        p.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-043")]
    public async Task DbContext_ToListAsync_ReturnsAll()
    {
        for (var i = 13060; i <= 13064; i++)
            await SeedPartnerAsync(i, null);
        await RunClearbitCleanupMigrationAsync();

        var list = await DbContext.Partners.Where(p => p.Id >= 13060 && p.Id <= 13064).ToListAsync();

        list.Should().HaveCount(5);
    }

    [Fact] [Trait("TestId", "INT-044")]
    public async Task DbContext_NullLogoQuery_ReturnsAffectedPartners()
    {
        for (var i = 13070; i <= 13074; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/null{i}.org");
        await RunClearbitCleanupMigrationAsync();

        var nullLogos = await DbContext.Partners
            .Where(p => p.Id >= 13070 && p.Id <= 13074 && p.LogoUrl == null)
            .ToListAsync();

        nullLogos.Should().HaveCount(5);
    }

    [Fact] [Trait("TestId", "INT-045")]
    public async Task DbContext_NonNullLogoQuery_ReturnsSafePartners()
    {
        for (var i = 13080; i <= 13084; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");
        await RunClearbitCleanupMigrationAsync();

        var nonNull = await DbContext.Partners
            .Where(p => p.Id >= 13080 && p.Id <= 13084 && p.LogoUrl != null)
            .ToListAsync();

        nonNull.Should().HaveCount(5);
    }

    [Fact] [Trait("TestId", "INT-046")]
    public async Task Integration_FullScenario_30Partners_MixedUrls()
    {
        for (var i = 14001; i <= 14010; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/s{i}.org");
        for (var i = 14011; i <= 14020; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");
        for (var i = 14021; i <= 14030; i++)
            await SeedPartnerAsync(i, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(10);
        var clearbitUrls = await DbContext.Partners
            .Where(p => p.Id >= 14001 && p.Id <= 14010 && p.LogoUrl != null)
            .CountAsync();
        clearbitUrls.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-047")]
    public async Task Integration_MigrationThenQuery_NoClearbitRemaining()
    {
        for (var i = 14031; i <= 14040; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/nc{i}.org");

        await RunClearbitCleanupMigrationAsync();

        var clearbitLeft = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.LogoUrl != null && p.LogoUrl.Contains("clearbit"))
            .ToListAsync();

        clearbitLeft.Should().BeEmpty();
    }

    [Fact] [Trait("TestId", "INT-048")]
    public async Task Integration_FullFlow_AllDisplayUrlsNeverContainClearbit()
    {
        for (var i = 14041; i <= 14045; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/disp{i}.org");
        for (var i = 14046; i <= 14050; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");

        await RunClearbitCleanupMigrationAsync();

        var partners = await DbContext.Partners
            .Where(p => p.Id >= 14041 && p.Id <= 14050)
            .ToListAsync();

        foreach (var p in partners)
        {
            var effective = GetEffectiveLogoUrl(p.LogoUrl);
            effective.Should().NotContain("clearbit");
        }
    }

    [Fact] [Trait("TestId", "INT-049")]
    public async Task Integration_PartnerSoftDeleted_AfterMigration_StillHasClearbitUrl()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 14051, Name = "SD Post", IsDeleted = true,
            LogoUrl = "https://logo.clearbit.com/sd-post.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(14051))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-050")]
    public async Task Integration_AffectedCount_ReflectsActualDbChanges()
    {
        var initial = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.LogoUrl == null)
            .CountAsync();

        for (var i = 15001; i <= 15005; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/db{i}.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        var postNull = await DbContext.Partners
            .Where(p => p.Id >= 15001 && p.Id <= 15005 && p.LogoUrl == null)
            .CountAsync();

        affected.Should().Be(5);
        postNull.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-051")]
    public async Task Integration_SeedClearbit_MigrateVerifyFallback_Full()
    {
        await SeedPartnerAsync(16001, "https://logo.clearbit.com/int51.org");
        await RunClearbitCleanupMigrationAsync();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16001);
        GetEffectiveLogoUrl(p.LogoUrl).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "INT-052")]
    public async Task Integration_NonClearbit_MigrateVerify_Unchanged()
    {
        await SeedPartnerAsync(16002, "https://cdn.corp.org/logo.png");
        var before = await RunClearbitCleanupMigrationAsync();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16002);
        before.Should().Be(0);
        p.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-053")]
    public async Task Integration_SoftDeleted_ExcludedFromMigration()
    {
        var p = await SeedPartnerAsync(16003, "https://logo.clearbit.com/int53.org");
        p.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-054")]
    public async Task Integration_BulkSeed5_MigrateAll_Verify()
    {
        for (var i = 16010; i <= 16014; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/int54.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(5);
        var nullCount = await DbContext.Partners.CountAsync(x => x.Id >= 16010 && x.Id <= 16014 && x.LogoUrl == null);
        nullCount.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-055")]
    public async Task Integration_MixedBatch_OnlyClearbitAffected()
    {
        await SeedPartnerAsync(16020, "https://logo.clearbit.com/mix1.org");
        await SeedPartnerAsync(16021, "https://safe.net/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16021)).LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-056")]
    public async Task Integration_NullLogoPartner_MigrationDoesNotAffect()
    {
        await SeedPartnerAsync(16022, null);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16022)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-057")]
    public async Task Integration_SecondMigrationRun_AffectsZero()
    {
        await SeedPartnerAsync(16023, "https://logo.clearbit.com/idem.org");
        await RunClearbitCleanupMigrationAsync();
        var second = await RunClearbitCleanupMigrationAsync();
        second.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-058")]
    public async Task Integration_PartnerName_Unchanged_After_Migration()
    {
        await SeedPartnerAsync(16024, "https://logo.clearbit.com/name.org", "Name Preserved");
        await RunClearbitCleanupMigrationAsync();
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16024)).Name.Should().Be("Name Preserved");
    }

    [Fact] [Trait("TestId", "INT-059")]
    public async Task Integration_Status_Unchanged_After_Migration()
    {
        await SeedPartnerAsync(16025, "https://logo.clearbit.com/status.org");
        await RunClearbitCleanupMigrationAsync();
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16025)).Status
            .Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "INT-060")]
    public async Task Integration_PartnerCount_Unchanged_After_Migration()
    {
        await SeedPartnerAsync(16026, "https://logo.clearbit.com/cnt.org");
        var before = await DbContext.Partners.CountAsync();
        await RunClearbitCleanupMigrationAsync();
        (await DbContext.Partners.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "INT-061")]
    public async Task Integration_ChangeThenMigrate_ResultIsNull()
    {
        var p = await SeedPartnerAsync(16027, "https://safe.org/logo.png");
        p.LogoUrl = "https://logo.clearbit.com/changed.org";
        await DbContext.SaveChangesAsync();
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16027)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-062")]
    public async Task Integration_AllNull_AlreadyMigrated_ZeroAffected()
    {
        await SeedPartnerAsync(16028, null);
        await SeedPartnerAsync(16029, null);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-063")]
    public async Task Integration_TenClearbit_TenNonClearbit_OnlyTenAffected()
    {
        for (var i = 16030; i <= 16039; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/bulk.org");
        for (var i = 16040; i <= 16049; i++)
            await SeedPartnerAsync(i, "https://cdn.safe.org/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(10);
    }

    [Fact] [Trait("TestId", "INT-064")]
    public async Task Integration_ClearbitBaseUrl_Only_Affected()
    {
        await SeedPartnerAsync(16050, ClearbitBaseUrl);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "INT-065")]
    public async Task Integration_ClearbitKeyword_InPath_Affected()
    {
        await SeedPartnerAsync(16051, "https://cdn.example.org/clearbit/icon.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "INT-066")]
    public async Task Integration_EmptyLogoUrl_Not_Migrated()
    {
        await SeedPartnerAsync(16052, string.Empty);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-067")]
    public async Task Integration_Reload_ContextAfterMigration()
    {
        await SeedPartnerAsync(16053, "https://logo.clearbit.com/reload.org");
        await RunClearbitCleanupMigrationAsync();
        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16053);
        p.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-068")]
    public async Task Integration_MultipleContexts_SameResult()
    {
        await SeedPartnerAsync(16054, "https://logo.clearbit.com/ctx.org");
        await RunClearbitCleanupMigrationAsync();

        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Set<UNOPS.PAO.Domain.Entities.Partner>().AsNoTracking().FirstAsync(x => x.Id == 16054);
        p.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-069")]
    public async Task Integration_FallbackIsCorrect_ForMigratedPartner()
    {
        await SeedPartnerAsync(16055, "https://logo.clearbit.com/fb.org");
        await RunClearbitCleanupMigrationAsync();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16055);
        GetEffectiveLogoUrl(p.LogoUrl).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "INT-070")]
    public async Task Integration_FallbackIsCorrect_ForNonMigratedPartner()
    {
        await SeedPartnerAsync(16056, "https://partner.org/logo.png");
        await RunClearbitCleanupMigrationAsync();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16056);
        GetEffectiveLogoUrl(p.LogoUrl).Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "INT-071")]
    public async Task Integration_3Runs_Idempotent_NoCumulation()
    {
        await SeedPartnerAsync(16057, "https://logo.clearbit.com/3run.org");
        var r1 = await RunClearbitCleanupMigrationAsync();
        var r2 = await RunClearbitCleanupMigrationAsync();
        var r3 = await RunClearbitCleanupMigrationAsync();
        (r1 + r2 + r3).Should().Be(1);
    }

    [Fact] [Trait("TestId", "INT-072")]
    public async Task Integration_PartnerIsDeleted_Flag_False_Default()
    {
        var p = await SeedPartnerAsync(16058, "https://logo.clearbit.com/del.org");
        p.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "INT-073")]
    public async Task Integration_SetDeletedAfterSeed_Migration_Skips()
    {
        var p = await SeedPartnerAsync(16059, "https://logo.clearbit.com/skipme.org");
        p.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-074")]
    public async Task Integration_ChangeTracker_ClearAfterMigration_ConsistentRead()
    {
        await SeedPartnerAsync(16060, "https://logo.clearbit.com/track.org");
        await RunClearbitCleanupMigrationAsync();
        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Partners.FirstOrDefaultAsync(x => x.Id == 16060);
        p.Should().NotBeNull();
        p!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-075")]
    public async Task Integration_NewPartnerAfterMigration_NotAffected()
    {
        await SeedPartnerAsync(16061, "https://logo.clearbit.com/early.org");
        await RunClearbitCleanupMigrationAsync();

        await SeedPartnerAsync(16062, "https://logo.clearbit.com/late.org");
        var second = await RunClearbitCleanupMigrationAsync();

        second.Should().Be(1);
        (await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16062)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-076")]
    public async Task Integration_DataPersists_AcrossMultipleSeeds()
    {
        for (var i = 16070; i <= 16074; i++)
            await SeedPartnerAsync(i, null);
        await SeedPartnerAsync(16075, "https://logo.clearbit.com/persist.org");

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "INT-077")]
    public async Task Integration_QueryByNullLogoUrl_ReturnsOnlyMigrated()
    {
        await SeedPartnerAsync(16080, "https://logo.clearbit.com/q1.org");
        await SeedPartnerAsync(16081, null);
        var affected = await RunClearbitCleanupMigrationAsync();
        var nullCount = await DbContext.Partners.CountAsync(p => p.Id >= 16080 && p.LogoUrl == null);
        nullCount.Should().Be(2);
    }

    [Fact] [Trait("TestId", "INT-078")]
    public async Task Integration_AllSafe_NoAffect_AllPreserved()
    {
        for (var i = 16090; i <= 16094; i++)
            await SeedPartnerAsync(i, "https://cdn.safe.org/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
        (await DbContext.Partners.CountAsync(p => p.Id >= 16090 && p.LogoUrl != null)).Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-079")]
    public async Task Integration_SaveChanges_NotNeeded_AfterMigration_UrlIsNull()
    {
        await SeedPartnerAsync(16095, "https://logo.clearbit.com/save.org");
        await RunClearbitCleanupMigrationAsync();
        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16095);
        p.LogoUrl.Should().BeNull("migration committed on its own");
    }

    [Fact] [Trait("TestId", "INT-080")]
    public async Task Integration_LargeSeed_100Clearbit_AllMigrated()
    {
        for (var i = 16100; i <= 16199; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/large.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(100);
        var nullCount = await DbContext.Partners.CountAsync(p => p.Id >= 16100 && p.Id <= 16199 && p.LogoUrl == null);
        nullCount.Should().Be(100);
    }

    [Fact] [Trait("TestId", "INT-081")]
    public async Task Integration_ClearbitAndSafe_50Each_OnlyClearbitAffected()
    {
        for (var i = 16200; i <= 16249; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/half.org");
        for (var i = 16250; i <= 16299; i++)
            await SeedPartnerAsync(i, "https://safe.cdn.org/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(50);
    }

    [Fact] [Trait("TestId", "INT-082")]
    public async Task Integration_DataIntegrity_NameFieldIntact()
    {
        await SeedPartnerAsync(16300, "https://logo.clearbit.com/integrity.org", "Integrity Corp");
        await RunClearbitCleanupMigrationAsync();
        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16300);
        p.Name.Should().Be("Integrity Corp");
        p.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-083")]
    public async Task Integration_MigrationAffectedCount_Accurate()
    {
        for (var i = 16310; i <= 16314; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/accurate.org");
        for (var i = 16315; i <= 16319; i++)
            await SeedPartnerAsync(i, null);
        for (var i = 16320; i <= 16324; i++)
            await SeedPartnerAsync(i, "https://safe.org/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(5);
    }

    [Fact] [Trait("TestId", "INT-084")]
    public async Task Integration_FullRound_SeedMigrateVerify()
    {
        await SeedPartnerAsync(16400, "https://logo.clearbit.com/round.org", "Round Trip");
        var affected = await RunClearbitCleanupMigrationAsync();
        DbContext.ChangeTracker.Clear();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16400);
        affected.Should().Be(1);
        p.LogoUrl.Should().BeNull();
        p.Name.Should().Be("Round Trip");
        GetEffectiveLogoUrl(p.LogoUrl).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "INT-085")]
    public async Task Integration_EmptyDb_MigrationRunsClean()
    {
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "INT-086")]
    public async Task Integration_DeletedMixed_WithClearbit_OnlyActiveAffected()
    {
        var active = await SeedPartnerAsync(16500, "https://logo.clearbit.com/active.org");
        var deleted = await SeedPartnerAsync(16501, "https://logo.clearbit.com/deleted.org");
        deleted.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
        (await DbContext.Partners.FindAsync(16500))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(16501))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "INT-087")]
    public async Task Integration_SequentialSeeds_EachClearbit_AreAllMigrated()
    {
        await SeedPartnerAsync(16510, "https://logo.clearbit.com/s1.org");
        await SeedPartnerAsync(16511, "https://logo.clearbit.com/s2.org");
        await SeedPartnerAsync(16512, "https://logo.clearbit.com/s3.org");

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(3);

        foreach (var id in new[] { 16510, 16511, 16512 })
            (await DbContext.Partners.AsNoTracking().FirstAsync(p => p.Id == id)).LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-088")]
    public async Task Integration_AfterMigration_DbContextClear_SeesMigrated()
    {
        await SeedPartnerAsync(16520, "https://logo.clearbit.com/clear.org");
        await RunClearbitCleanupMigrationAsync();
        DbContext.ChangeTracker.Clear();
        var all = await DbContext.Partners.AsNoTracking().Where(p => p.Id == 16520).ToListAsync();
        all.Should().HaveCount(1);
        all[0].LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "INT-089")]
    public async Task Integration_NullPartner_FallbackImage_Used()
    {
        await SeedPartnerAsync(16530, null);
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16530);
        GetEffectiveLogoUrl(p.LogoUrl).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "INT-090")]
    public async Task Integration_FullPNO926_EndToEnd()
    {
        await SeedPartnerAsync(16600, "https://logo.clearbit.com/e2e.org", "E2E Corp");
        var affected = await RunClearbitCleanupMigrationAsync();
        DbContext.ChangeTracker.Clear();
        var partner = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 16600);

        affected.Should().Be(1);
        partner.LogoUrl.Should().BeNull("PNO-926: clearbit URL must be cleared");
        GetEffectiveLogoUrl(partner.LogoUrl).Should().Be(FallbackImage, "PNO-926: fallback image must be used");
        partner.Name.Should().Be("E2E Corp", "PNO-926: partner name must not be affected");
    }
}
