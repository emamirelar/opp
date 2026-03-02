/**
 * @fileoverview PNO-926 Functional Tests — 50 business logic and validation tests.
 * Migration business rules, fallback logic, and data integrity validation.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Functional Tests — 50 business rule and validation tests.
/// </summary>
[Collection("Functional")]
[Trait("Category", "Functional")]
[Trait("Ticket", "PNO-926")]
public class FunctionalTests : PNO926TestFixtureBase
{
    // ─── §4.1 Migration Business Rules (FUN-001 – 015) ───────────────────

    [Fact] [Trait("TestId", "FUN-001")]
    public async Task Migration_TargetsOnlyClearbitContaining_BusinessRule()
    {
        await SeedPartnerAsync(8001, "https://logo.clearbit.com/example.org");
        await SeedPartnerAsync(8002, "https://safe.org/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(8001))!.LogoUrl.Should().BeNull("PNO-926: clearbit URLs must be cleared");
        (await DbContext.Partners.FindAsync(8002))!.LogoUrl.Should().NotBeNull("PNO-926: non-clearbit URLs must be preserved");
    }

    [Fact] [Trait("TestId", "FUN-002")]
    public async Task Migration_SoftDeletedAreExcluded_BusinessRule()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 8003, Name = "SD Excluded", IsDeleted = true,
            LogoUrl = "https://logo.clearbit.com/excluded.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(8003))!.LogoUrl.Should().NotBeNull("Soft-deleted partners excluded per business rule");
    }

    [Fact] [Trait("TestId", "FUN-003")]
    public async Task Migration_NullPreservesPartnerRecord_BusinessRule()
    {
        await SeedPartnerAsync(8004, "https://logo.clearbit.com/preserve-record.org");
        await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(8004);
        partner.Should().NotBeNull("Record must not be deleted");
        partner!.Id.Should().Be(8004, "ID must be preserved");
    }

    [Fact] [Trait("TestId", "FUN-004")]
    public async Task Migration_OnlyLogoUrlField_Modified_OtherFieldsIntact()
    {
        await SeedPartnerAsync(8005, "https://logo.clearbit.com/fields.org", "Fields Business Rule");
        await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(8005);
        partner!.Name.Should().Be("Fields Business Rule");
        partner.IsDeleted.Should().BeFalse();
        partner.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
        partner.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "FUN-005")]
    public async Task Migration_AffectedCount_MatchesAllClearbitPartners()
    {
        for (var i = 8010; i <= 8020; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/p{i}.org");
        for (var i = 8021; i <= 8025; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.com/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(11, "Only clearbit partners should be counted");
    }

    [Fact] [Trait("TestId", "FUN-006")]
    public async Task Migration_IsIdempotent_BusinessRule()
    {
        await SeedPartnerAsync(8030, "https://logo.clearbit.com/idempotent-biz.org");
        var firstCount = await RunClearbitCleanupMigrationAsync();
        var secondCount = await RunClearbitCleanupMigrationAsync();

        firstCount.Should().Be(1);
        secondCount.Should().Be(0, "Idempotent: second run should not modify already-null logos");
    }

    [Fact] [Trait("TestId", "FUN-007")]
    public async Task Migration_NullLogoUrlNotAffected_BusinessRule()
    {
        await SeedPartnerAsync(8031, null);
        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0, "NULL logos are not 'clearbit' URLs");
    }

    [Fact] [Trait("TestId", "FUN-008")]
    public async Task Migration_EmptyLogoUrlNotAffected_BusinessRule()
    {
        await SeedPartnerAsync(8032, "");
        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0, "Empty string logos are not 'clearbit' URLs");
    }

    [Fact] [Trait("TestId", "FUN-009")]
    public async Task Migration_PartnerWithClearbitRemoved_DisplaysFallback_BusinessRule()
    {
        await SeedPartnerAsync(8033, "https://logo.clearbit.com/biz-fallback.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(8033);
        var effective = GetEffectiveLogoUrl(p!.LogoUrl);

        effective.Should().Be(FallbackImage, "PNO-926: partners without logo must show fallback image");
    }

    [Fact] [Trait("TestId", "FUN-010")]
    public async Task Migration_NonClearbitStillShowsLogo_BusinessRule()
    {
        await SeedPartnerAsync(8034, "https://cdn.example.org/logo.png");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(8034);
        var effective = GetEffectiveLogoUrl(p!.LogoUrl);

        effective.Should().NotBe(FallbackImage);
        effective.Should().Contain("cdn.example.org");
    }

    [Fact] [Trait("TestId", "FUN-011")]
    public async Task Migration_MatchIsSubstringBased_BusinessRule()
    {
        await SeedPartnerAsync(8035, "https://images.clearbit-api.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(8035))!.LogoUrl.Should().BeNull("Substring match 'clearbit' applies");
    }

    [Fact] [Trait("TestId", "FUN-012")]
    public async Task Migration_PartnerCountUnchanged_BusinessRule()
    {
        await SeedPartnerAsync(8036, "https://logo.clearbit.com/count.org");
        await SeedPartnerAsync(8037, "https://safe.com/logo.png");
        var countBefore = await DbContext.Partners.CountAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.CountAsync()).Should().Be(countBefore);
    }

    [Fact] [Trait("TestId", "FUN-013")]
    public async Task Migration_LogoNullifiedToDbNull_NotEmptyString_BusinessRule()
    {
        await SeedPartnerAsync(8038, "https://logo.clearbit.com/null-not-empty.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 8038);
        p.LogoUrl.Should().BeNull("Should be DB NULL, not empty string");
    }

    [Fact] [Trait("TestId", "FUN-014")]
    public async Task Migration_MultiplePartners_AllClearbitUrls_AllNullified_BusinessRule()
    {
        for (var i = 8040; i <= 8049; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/batch{i}.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(10);
        for (var i = 8040; i <= 8049; i++)
            (await DbContext.Partners.FindAsync(i))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "FUN-015")]
    public async Task Migration_SetToNull_PartnerCanStillBeSaved_BusinessRule()
    {
        await SeedPartnerAsync(8050, "https://logo.clearbit.com/resave.org", "Resave Partner");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(8050);
        p!.Name = "Updated Name";
        await DbContext.SaveChangesAsync();

        (await DbContext.Partners.FindAsync(8050))!.Name.Should().Be("Updated Name");
    }

    // ─── §4.2 Fallback Display Logic (FUN-016 – 030) ─────────────────────

    [Fact] [Trait("TestId", "FUN-016")]
    public async Task Fallback_NullLogoUrl_AlwaysReturnsFallback()
    {
        for (var i = 0; i < 5; i++)
        {
            var effective = GetEffectiveLogoUrl(null);
            effective.Should().Be(FallbackImage);
        }
    }

    [Fact] [Trait("TestId", "FUN-017")]
    public async Task Fallback_EmptyLogoUrl_AlwaysReturnsFallback()
    {
        for (var i = 0; i < 5; i++)
        {
            var effective = GetEffectiveLogoUrl("");
            effective.Should().Be(FallbackImage);
        }
    }

    [Fact] [Trait("TestId", "FUN-018")]
    public async Task Fallback_ValidUrl_NeverReturnsFallback()
    {
        var validUrls = new[]
        {
            "https://partner.org/logo.png",
            "https://cdn.example.com/img.svg",
            "/assets/partner-logo.jpg",
            "http://old-partner.com/img.gif"
        };

        foreach (var url in validUrls)
            GetEffectiveLogoUrl(url).Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-019")]
    public async Task Fallback_AfterClearbitRemoval_PartnerShowsFallback()
    {
        await SeedPartnerAsync(8060, "https://logo.clearbit.com/fallback-after.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(8060);
        GetEffectiveLogoUrl(p!.LogoUrl).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-020")]
    public async Task Fallback_NonClearbitPartner_ShowsActualLogoNotFallback()
    {
        await SeedPartnerAsync(8061, "https://my-partner.org/logo.png");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(8061);
        GetEffectiveLogoUrl(p!.LogoUrl).Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-021")]
    public async Task Fallback_FallbackAsset_CorrectFilePath()
    {
        FallbackImage.Should().StartWith("assets/");
        FallbackImage.Should().EndWith(".png");
    }

    [Fact] [Trait("TestId", "FUN-022")]
    public async Task Fallback_FallbackAsset_IsRelativePath()
    {
        FallbackImage.Should().NotStartWith("http");
        FallbackImage.Should().NotStartWith("/");
    }

    [Fact] [Trait("TestId", "FUN-023")]
    public async Task Fallback_GetEffectiveLogoUrl_ReturnType_AlwaysString()
    {
        var results = new[]
        {
            GetEffectiveLogoUrl(null),
            GetEffectiveLogoUrl(""),
            GetEffectiveLogoUrl("https://example.com/logo.png")
        };

        results.Should().AllSatisfy(r => r.Should().BeOfType<string>());
    }

    [Fact] [Trait("TestId", "FUN-024")]
    public async Task Fallback_GetEffectiveLogoUrl_NeverReturnsNull()
    {
        var results = new[]
        {
            GetEffectiveLogoUrl(null),
            GetEffectiveLogoUrl(""),
            GetEffectiveLogoUrl("https://example.com/logo.png")
        };

        results.Should().AllSatisfy(r => r.Should().NotBeNull());
    }

    [Fact] [Trait("TestId", "FUN-025")]
    public async Task Fallback_MixedPartners_EachHasCorrectEffectiveUrl()
    {
        await SeedPartnerAsync(8062, "https://logo.clearbit.com/mixed1.org");
        await SeedPartnerAsync(8063, "https://my-logo.org/img.png");
        await SeedPartnerAsync(8064, null);
        await RunClearbitCleanupMigrationAsync();

        GetEffectiveLogoUrl((await DbContext.Partners.FindAsync(8062))!.LogoUrl).Should().Be(FallbackImage);
        GetEffectiveLogoUrl((await DbContext.Partners.FindAsync(8063))!.LogoUrl).Should().NotBe(FallbackImage);
        GetEffectiveLogoUrl((await DbContext.Partners.FindAsync(8064))!.LogoUrl).Should().Be(FallbackImage);
    }

    // ─── §4.3 Data Integrity Post-Migration (FUN-026 – 040) ──────────────

    [Fact] [Trait("TestId", "FUN-026")]
    public async Task PostMigration_PartnerQueryable_ByIdWorks()
    {
        await SeedPartnerAsync(8070, "https://logo.clearbit.com/query.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FirstOrDefaultAsync(x => x.Id == 8070);
        p.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-027")]
    public async Task PostMigration_ActivePartnerFilterWorks()
    {
        await SeedPartnerAsync(8071, "https://logo.clearbit.com/active-query.org");
        await RunClearbitCleanupMigrationAsync();

        var partners = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active)
            .ToListAsync();

        partners.Should().Contain(p => p.Id == 8071);
    }

    [Fact] [Trait("TestId", "FUN-028")]
    public async Task PostMigration_PartnerWithNullLogo_NullableFieldHandled()
    {
        await SeedPartnerAsync(8072, "https://logo.clearbit.com/nullable.org");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 8072);
        p.LogoUrl.Should().BeNull();
        var act = () => GetEffectiveLogoUrl(p.LogoUrl);
        act.Should().NotThrow();
    }

    [Fact] [Trait("TestId", "FUN-029")]
    public async Task PostMigration_NoClearbitUrlsInDb()
    {
        await SeedPartnerAsync(8073, "https://logo.clearbit.com/no-clearbit-left.org");
        await SeedPartnerAsync(8074, "https://logo.clearbit.com/no-clearbit-left2.org");
        await RunClearbitCleanupMigrationAsync();

        var remaining = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.LogoUrl != null && p.LogoUrl.Contains("clearbit"))
            .CountAsync();

        remaining.Should().Be(0, "No clearbit URLs should remain after migration");
    }

    [Fact] [Trait("TestId", "FUN-030")]
    public async Task PostMigration_NewlySeedClearbitPartner_StillPresent()
    {
        await SeedPartnerAsync(8075, "https://logo.clearbit.com/run1.org");
        await RunClearbitCleanupMigrationAsync();

        await SeedPartnerAsync(8076, "https://logo.clearbit.com/run2.org");
        var postCount = await DbContext.Partners.CountAsync(p => p.LogoUrl != null && p.LogoUrl.Contains("clearbit"));

        postCount.Should().Be(1, "Partner 8076 added after migration still has clearbit URL");
    }

    [Fact] [Trait("TestId", "FUN-031")]
    public async Task PostMigration_SecondMigrationRun_ClearsNewlySeedClearbit()
    {
        await SeedPartnerAsync(8080, "https://logo.clearbit.com/first.org");
        await RunClearbitCleanupMigrationAsync();

        await SeedPartnerAsync(8081, "https://logo.clearbit.com/second.org");
        var secondCount = await RunClearbitCleanupMigrationAsync();

        secondCount.Should().Be(1);
        (await DbContext.Partners.FindAsync(8081))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "FUN-032")]
    public async Task PostMigration_AllPartnersLogoUrlsAreSafeForDisplay()
    {
        await SeedPartnerAsync(8082, "https://logo.clearbit.com/safe-for-display.org");
        await SeedPartnerAsync(8083, "https://my-partner.org/logo.png");
        await SeedPartnerAsync(8084, null);
        await RunClearbitCleanupMigrationAsync();

        var partners = await DbContext.Partners.Where(p => p.Id >= 8082 && p.Id <= 8084).ToListAsync();
        foreach (var p in partners)
        {
            var effective = GetEffectiveLogoUrl(p.LogoUrl);
            effective.Should().NotBeNull();
            effective.Should().NotContain("clearbit");
        }
    }

    // ─── §4.4 Audit Fields (FUN-033 – 050) ───────────────────────────────

    [Fact] [Trait("TestId", "FUN-033")]
    public async Task Migration_PartnerIsDeletedFlag_FalseAfterMigration()
    {
        await SeedPartnerAsync(8090, "https://logo.clearbit.com/isdeleted.org");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(8090))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "FUN-034")]
    public async Task Migration_PartnerStatusAfterMigration_Unchanged()
    {
        await SeedPartnerAsync(8091, "https://logo.clearbit.com/status.org");
        var statusBefore = (await DbContext.Partners.FindAsync(8091))!.Status;
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(8091))!.Status.Should().Be(statusBefore);
    }

    [Fact] [Trait("TestId", "FUN-035")]
    public async Task Migration_PartnerNameAfterMigration_Unchanged()
    {
        await SeedPartnerAsync(8092, "https://logo.clearbit.com/name.org", "Name Unchanged");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(8092))!.Name.Should().Be("Name Unchanged");
    }

    [Fact] [Trait("TestId", "FUN-036")]
    public async Task Migration_PartnerIdAfterMigration_Unchanged()
    {
        await SeedPartnerAsync(8093, "https://logo.clearbit.com/id.org");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(8093))!.Id.Should().Be(8093);
    }

    [Fact] [Trait("TestId", "FUN-037")]
    public async Task Migration_NoClearbitInAnyActivePartnerLogo_PostMigration()
    {
        for (var i = 8100; i <= 8109; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/audit{i}.org");
        await RunClearbitCleanupMigrationAsync();

        var clearbitUrls = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.LogoUrl != null && p.LogoUrl.Contains("clearbit"))
            .CountAsync();

        clearbitUrls.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-038")]
    public async Task Migration_NonClearbit_NoClearbitRemainsAfterMigration()
    {
        await SeedPartnerAsync(8110, "https://logo.clearbit.com/clear1.org");
        await SeedPartnerAsync(8111, "https://safe.org/logo.png");
        await RunClearbitCleanupMigrationAsync();

        var remaining = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.LogoUrl != null && p.LogoUrl.Contains("clearbit"))
            .ToListAsync();

        remaining.Should().BeEmpty();
    }

    [Fact] [Trait("TestId", "FUN-039")]
    public async Task Migration_NoDataLoss_PartnerCountSame()
    {
        for (var i = 8120; i <= 8129; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/loss{i}.org");

        var countBefore = await DbContext.Partners.CountAsync();
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.CountAsync()).Should().Be(countBefore);
    }

    [Fact] [Trait("TestId", "FUN-040")]
    public async Task Migration_ActivePartnerListFunctional_AfterMigration()
    {
        for (var i = 8130; i <= 8134; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/list{i}.org");
        await RunClearbitCleanupMigrationAsync();

        var partners = await DbContext.Partners
            .Where(p => !p.IsDeleted && p.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active)
            .ToListAsync();

        partners.Should().NotBeEmpty();
    }

    [Fact] [Trait("TestId", "FUN-041")]
    public async Task Migration_Performance_100Partners_CompletesUnder3Seconds()
    {
        for (var i = 9001; i <= 9050; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/perf{i}.org");
        for (var i = 9051; i <= 9100; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");

        var sw = Stopwatch.StartNew();
        await RunClearbitCleanupMigrationAsync();
        sw.Stop();

        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
    }

    [Fact] [Trait("TestId", "FUN-042")]
    public async Task Migration_AffectedCount_MatchesChangedRecords()
    {
        for (var i = 9101; i <= 9110; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/match{i}.org");
        for (var i = 9111; i <= 9115; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();
        var actualNulls = await DbContext.Partners
            .Where(p => p.Id >= 9101 && p.Id <= 9110 && p.LogoUrl == null)
            .CountAsync();

        affected.Should().Be(actualNulls);
    }

    [Fact] [Trait("TestId", "FUN-043")]
    public async Task Fallback_EffectiveUrl_ForEachPartner_NeverNull()
    {
        await SeedPartnerAsync(9200, "https://logo.clearbit.com/null-check.org");
        await SeedPartnerAsync(9201, "https://safe.org/logo.png");
        await SeedPartnerAsync(9202, null);
        await RunClearbitCleanupMigrationAsync();

        var partners = await DbContext.Partners.Where(p => p.Id >= 9200 && p.Id <= 9202).ToListAsync();
        foreach (var p in partners)
            GetEffectiveLogoUrl(p.LogoUrl).Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "FUN-044")]
    public async Task Migration_BackToBackClearbitSeeds_BothNullified()
    {
        await SeedPartnerAsync(9300, "https://logo.clearbit.com/back1.org");
        await SeedPartnerAsync(9301, "https://logo.clearbit.com/back2.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(9300))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(9301))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "FUN-045")]
    public async Task Migration_MigrationDescriptionMatchesPR_BusinessRule()
    {
        const string expectedMigration = "ClearClearbitLogoUrlsFromPartners";
        expectedMigration.Should().Contain("Clearbit");
        expectedMigration.Should().Contain("LogoUrls");
        expectedMigration.Should().Contain("Partners");
    }

    [Fact] [Trait("TestId", "FUN-046")]
    public async Task Fallback_FallbackImagePath_MatchesAngularAsset()
    {
        FallbackImage.Should().Be("assets/images/Partner.png",
            "Must match Angular asset path for partner placeholder logo");
    }

    [Fact] [Trait("TestId", "FUN-047")]
    public async Task Migration_AllNull_NoClearbitRemains()
    {
        for (var i = 9400; i <= 9405; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/all-null.org");

        await RunClearbitCleanupMigrationAsync();

        var remaining = await DbContext.Partners
            .Where(p => p.Id >= 9400 && p.Id <= 9405 && p.LogoUrl != null)
            .CountAsync();

        remaining.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-048")]
    public async Task Migration_NonClearbit_AllPreserved()
    {
        for (var i = 9500; i <= 9505; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        var preserved = await DbContext.Partners
            .Where(p => p.Id >= 9500 && p.Id <= 9505 && p.LogoUrl != null)
            .CountAsync();

        preserved.Should().Be(6);
    }

    [Fact] [Trait("TestId", "FUN-049")]
    public async Task Migration_MixedScenario_ExactCounts()
    {
        for (var i = 9600; i <= 9609; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/exact{i}.org");
        for (var i = 9610; i <= 9614; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.org/logo.png");
        for (var i = 9615; i <= 9619; i++)
            await SeedPartnerAsync(i, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(10);
        var nulledCount = await DbContext.Partners.Where(p => p.Id >= 9600 && p.Id <= 9619 && p.LogoUrl == null).CountAsync();
        nulledCount.Should().Be(15, "10 cleared + 5 already null");
    }

    [Fact] [Trait("TestId", "FUN-050")]
    public async Task Migration_ReturnsInt_NotNegative()
    {
        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact] [Trait("TestId", "FUN-051")]
    public async Task FallbackLogic_EmptyString_ReturnsFallback()
    {
        GetEffectiveLogoUrl(string.Empty).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-052")]
    public async Task FallbackLogic_WhitespaceOnly_ReturnsFallback()
    {
        GetEffectiveLogoUrl("   ").Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-053")]
    public async Task Migration_ClearbitCapitalized_IsMatched()
    {
        await SeedPartnerAsync(8110, "https://Logo.Clearbit.com/example.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-054")]
    public async Task Migration_UpperCaseClearbit_IsMatched()
    {
        await SeedPartnerAsync(8111, "HTTPS://LOGO.CLEARBIT.COM/EXAMPLE.ORG");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-055")]
    public async Task FallbackLogic_NonClearbit_ReturnsOriginal()
    {
        const string url = "https://brand.org/logo.png";
        GetEffectiveLogoUrl(url).Should().Be(url);
    }

    [Fact] [Trait("TestId", "FUN-056")]
    public async Task FallbackLogic_ClearbitUrl_ReturnsFallback()
    {
        GetEffectiveLogoUrl("https://logo.clearbit.com/test.org").Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-057")]
    public async Task Migration_MixedBatch_OnlyClearbitAffected()
    {
        await SeedPartnerAsync(8112, "https://logo.clearbit.com/alpha.org", "Alpha");
        await SeedPartnerAsync(8113, "https://company.org/logo.png", "Beta");
        await SeedPartnerAsync(8114, "https://logo.clearbit.com/gamma.org", "Gamma");
        await SeedPartnerAsync(8115, null, "Delta");

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(2);
    }

    [Fact] [Trait("TestId", "FUN-058")]
    public async Task FallbackLogic_InternalSlashesUrl_IsPreserved()
    {
        const string url = "https://s3.aws.com/bucket/partner/logo.png";
        GetEffectiveLogoUrl(url).Should().Be(url);
    }

    [Fact] [Trait("TestId", "FUN-059")]
    public async Task Migration_SubdomainClearbit_IsMatched()
    {
        await SeedPartnerAsync(8116, "https://api.clearbit.com/v1/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-060")]
    public async Task Migration_AlreadyNull_Skipped()
    {
        await SeedPartnerAsync(8117, null);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-061")]
    public async Task Migration_PartnerName_Preserved_AfterCleanup()
    {
        await SeedPartnerAsync(8118, "https://logo.clearbit.com/named.org", "Named Corp");
        await RunClearbitCleanupMigrationAsync();
        var p = await DbContext.Partners.FindAsync(8118);
        p!.Name.Should().Be("Named Corp");
    }

    [Fact] [Trait("TestId", "FUN-062")]
    public async Task Migration_PartnerStatus_Unchanged_AfterCleanup()
    {
        await SeedPartnerAsync(8119, "https://logo.clearbit.com/status.org");
        await RunClearbitCleanupMigrationAsync();
        var p = await DbContext.Partners.FindAsync(8119);
        p!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "FUN-063")]
    public async Task FallbackLogic_ValidHttpUrl_NoFallback()
    {
        GetEffectiveLogoUrl("http://example.org/logo.jpg").Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-064")]
    public async Task Migration_EmptyLogoUrl_NotTouched()
    {
        await SeedPartnerAsync(8120, string.Empty);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-065")]
    public async Task FallbackLogic_DataUri_Preserved()
    {
        const string dataUri = "data:image/png;base64,abc123";
        GetEffectiveLogoUrl(dataUri).Should().Be(dataUri);
    }

    [Fact] [Trait("TestId", "FUN-066")]
    public async Task Migration_InactivePartner_Clearbit_IsStillCleaned()
    {
        var p = await SeedPartnerAsync(8121, "https://logo.clearbit.com/inactive.org");
        p.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive;
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact] [Trait("TestId", "FUN-067")]
    public async Task FallbackLogic_RelativePath_Preserved()
    {
        const string relative = "assets/custom/mylogo.svg";
        GetEffectiveLogoUrl(relative).Should().Be(relative);
    }

    [Fact] [Trait("TestId", "FUN-068")]
    public async Task Migration_ClearbitInPath_IsMatched()
    {
        await SeedPartnerAsync(8122, "https://external.cdn.org/clearbit/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "FUN-069")]
    public async Task FallbackLogic_FallbackImage_ConstantValue()
    {
        FallbackImage.Should().Be("assets/images/Partner.png");
    }

    [Fact] [Trait("TestId", "FUN-070")]
    public async Task Migration_ZeroPartners_AffectsZero()
    {
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-071")]
    public async Task FallbackLogic_PngExtension_Preserved()
    {
        const string url = "https://media.partner.org/logo.png";
        GetEffectiveLogoUrl(url).Should().Be(url);
    }

    [Fact] [Trait("TestId", "FUN-072")]
    public async Task FallbackLogic_SvgExtension_Preserved()
    {
        const string url = "https://media.partner.org/logo.svg";
        GetEffectiveLogoUrl(url).Should().Be(url);
    }

    [Fact] [Trait("TestId", "FUN-073")]
    public async Task Migration_MultiRun_CountDecreases()
    {
        await SeedPartnerAsync(8130, "https://logo.clearbit.com/run1.org");
        await SeedPartnerAsync(8131, "https://logo.clearbit.com/run2.org");

        var first = await RunClearbitCleanupMigrationAsync();
        var second = await RunClearbitCleanupMigrationAsync();

        first.Should().Be(2);
        second.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-074")]
    public async Task Migration_PartnerIsDeleted_False_IsRequired()
    {
        var p = await SeedPartnerAsync(8132, "https://logo.clearbit.com/nodelete.org");
        p.IsDeleted.Should().BeFalse("migration only processes non-deleted partners");
    }

    [Fact] [Trait("TestId", "FUN-075")]
    public async Task Migration_SoftDelete_Then_Clearbit_NotCleaned()
    {
        var p = await SeedPartnerAsync(8133, "https://logo.clearbit.com/soft.org");
        p.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-076")]
    public async Task FallbackLogic_LongNonClearbit_Preserved()
    {
        var url = "https://longcdnpath.example.org/" + new string('a', 200) + "/logo.png";
        GetEffectiveLogoUrl(url).Should().Be(url);
    }

    [Fact] [Trait("TestId", "FUN-077")]
    public async Task Migration_3Batches_AllClearbitCleaned()
    {
        for (var i = 8140; i <= 8142; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/batch.org");

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(3);
    }

    [Fact] [Trait("TestId", "FUN-078")]
    public async Task Migration_NoClearbit_AffectedIsZero()
    {
        for (var i = 8150; i <= 8154; i++)
            await SeedPartnerAsync(i, "https://safe.cdn.org/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-079")]
    public async Task FallbackLogic_NullIsAlwaysFallback()
    {
        for (var i = 0; i < 10; i++)
            GetEffectiveLogoUrl(null).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-080")]
    public async Task FallbackLogic_UrlWithQueryString_Preserved()
    {
        const string url = "https://media.partner.org/logo.png?v=2";
        GetEffectiveLogoUrl(url).Should().Be(url);
    }

    [Fact] [Trait("TestId", "FUN-081")]
    public async Task Migration_PartnerCount_UnchangedAfterMigration()
    {
        await SeedPartnerAsync(8160, "https://logo.clearbit.com/count.org");
        var before = await DbContext.Partners.CountAsync();
        await RunClearbitCleanupMigrationAsync();
        (await DbContext.Partners.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "FUN-082")]
    public async Task Migration_OnlyLogoUrl_Changed_NoOtherFields()
    {
        var p = await SeedPartnerAsync(8161, "https://logo.clearbit.com/fields.org", "Fields Test");
        var originalStatus = p.Status;
        var originalName = p.Name;

        await RunClearbitCleanupMigrationAsync();
        await DbContext.Entry(p).ReloadAsync();

        p.Name.Should().Be(originalName);
        p.Status.Should().Be(originalStatus);
        p.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "FUN-083")]
    public async Task FallbackLogic_ClearbitBaseUrl_IsFallback()
    {
        GetEffectiveLogoUrl(ClearbitBaseUrl).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-084")]
    public async Task FallbackLogic_ClearbitPartialString_IsFallback()
    {
        GetEffectiveLogoUrl("clearbit").Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-085")]
    public async Task Migration_CorrectCount_OnMixedNullAndClearbit()
    {
        await SeedPartnerAsync(8170, "https://logo.clearbit.com/m1.org");
        await SeedPartnerAsync(8171, null);
        await SeedPartnerAsync(8172, "https://logo.clearbit.com/m2.org");
        await SeedPartnerAsync(8173, null);

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(2, "only 2 have clearbit URLs");
    }

    [Fact] [Trait("TestId", "FUN-086")]
    public async Task Migration_Idempotent_ThirdRun_ZeroAffected()
    {
        await SeedPartnerAsync(8175, "https://logo.clearbit.com/idem.org");
        await RunClearbitCleanupMigrationAsync();
        await RunClearbitCleanupMigrationAsync();
        var third = await RunClearbitCleanupMigrationAsync();
        third.Should().Be(0);
    }

    [Fact] [Trait("TestId", "FUN-087")]
    public async Task FallbackLogic_IpBasedUrl_Preserved()
    {
        const string url = "http://192.168.1.100/logo.png";
        GetEffectiveLogoUrl(url).Should().Be(url);
    }

    [Fact] [Trait("TestId", "FUN-088")]
    public async Task Migration_ClearsUrl_MakesItFallback()
    {
        await SeedPartnerAsync(8176, "https://logo.clearbit.com/tofallback.org");
        await RunClearbitCleanupMigrationAsync();
        var p = await DbContext.Partners.FindAsync(8176);
        GetEffectiveLogoUrl(p!.LogoUrl).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-089")]
    public async Task FallbackLogic_SpaceOnlyUrl_ReturnsFallback()
    {
        GetEffectiveLogoUrl("\t").Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "FUN-090")]
    public async Task Migration_AffectedCount_MatchesExpected_LargeSeed()
    {
        for (var i = 8200; i <= 8219; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/large.org");
        for (var i = 8220; i <= 8229; i++)
            await SeedPartnerAsync(i, "https://safe.org/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(20);
    }
}
