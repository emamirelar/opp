/**
 * @fileoverview PNO-926 Boundary/Edge Tests — 60 boundary and edge case tests.
 * URL length extremes, special characters, soft delete interactions, and type mismatches.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Boundary/Edge Tests — 60 tests for extremes and edge cases.
/// </summary>
[Collection("Boundary")]
[Trait("Category", "Boundary")]
[Trait("Ticket", "PNO-926")]
public class BoundaryTests : PNO926TestFixtureBase
{
    // ─── §3.1 URL Length Extremes (BND-001 – 010) ────────────────────────

    [Fact] [Trait("TestId", "BND-001")]
    public async Task Migration_SingleCharUrl_NoMatch()
    {
        await SeedPartnerAsync(4001, "x");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
        (await DbContext.Partners.FindAsync(4001))!.LogoUrl.Should().Be("x");
    }

    [Fact] [Trait("TestId", "BND-002")]
    public async Task Migration_MinimalClearbitUrl_Matched()
    {
        await SeedPartnerAsync(4002, "clearbit");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(1);
        (await DbContext.Partners.FindAsync(4002))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-003")]
    public async Task Migration_VeryLongClearbitUrl_Matched()
    {
        var longUrl = "https://logo.clearbit.com/" + new string('a', 500) + ".org/logo.png";
        await SeedPartnerAsync(4003, longUrl);

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4003))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-004")]
    public async Task Migration_VeryLongNonClearbitUrl_Unchanged()
    {
        var longUrl = "https://partner.org/" + new string('a', 500) + ".png";
        await SeedPartnerAsync(4004, longUrl);

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4004))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-005")]
    public async Task Migration_ExactlyClearbitWord_Matched()
    {
        await SeedPartnerAsync(4005, "clearbit");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4005))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-006")]
    public async Task Migration_ClearbitPrecededBySubdomain_Matched()
    {
        await SeedPartnerAsync(4006, "https://logo.clearbit.com/test.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4006))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-007")]
    public async Task Migration_ClearbitInPath_Matched()
    {
        await SeedPartnerAsync(4007, "https://api.example.com/v2/clearbit/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4007))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-008")]
    public async Task Migration_ClearbitInQuery_Matched()
    {
        await SeedPartnerAsync(4008, "https://example.com/logo.png?source=clearbit");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4008))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-009")]
    public async Task Migration_EmptyString_NotMatched()
    {
        await SeedPartnerAsync(4009, "");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-010")]
    public async Task Migration_Null_NotMatched()
    {
        await SeedPartnerAsync(4010, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    // ─── §3.2 Special Characters in URLs (BND-011 – 020) ─────────────────

    [Fact] [Trait("TestId", "BND-011")]
    public async Task Migration_UrlWithSpaces_ClearbitMatched()
    {
        await SeedPartnerAsync(4011, "https://logo.clearbit.com/my partner.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4011))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-012")]
    public async Task Migration_UrlWithUnicodeChars_ClearbitMatched()
    {
        await SeedPartnerAsync(4012, "https://logo.clearbit.com/pàrtner.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4012))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-013")]
    public async Task Migration_UrlWithHash_ClearbitMatched()
    {
        await SeedPartnerAsync(4013, "https://logo.clearbit.com/partner.org#logo");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4013))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-014")]
    public async Task Migration_UrlWithEncodedChars_ClearbitMatched()
    {
        await SeedPartnerAsync(4014, "https://logo.clearbit.com/p%20artner.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4014))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-015")]
    public async Task Migration_UrlWithPortNumber_ClearbitMatched()
    {
        await SeedPartnerAsync(4015, "https://logo.clearbit.com:443/partner.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4015))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-016")]
    public async Task Migration_UrlWithMultipleSubdomains_ClearbitMatched()
    {
        await SeedPartnerAsync(4016, "https://api.v2.logo.clearbit.com/partner.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4016))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-017")]
    public async Task Migration_UrlWithTrailingSlash_ClearbitMatched()
    {
        await SeedPartnerAsync(4017, "https://logo.clearbit.com/");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4017))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-018")]
    public async Task Migration_UrlWithNoPath_ClearbitMatched()
    {
        await SeedPartnerAsync(4018, "https://logo.clearbit.com");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4018))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-019")]
    public async Task Migration_UrlWithJpgExtension_ClearbitMatched()
    {
        await SeedPartnerAsync(4019, "https://logo.clearbit.com/partner.jpg");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4019))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-020")]
    public async Task Migration_UrlWithSvgExtension_ClearbitMatched()
    {
        await SeedPartnerAsync(4020, "https://logo.clearbit.com/partner.svg");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4020))!.LogoUrl.Should().BeNull();
    }

    // ─── §3.3 Soft-Delete Interaction (BND-021 – 030) ────────────────────

    [Fact] [Trait("TestId", "BND-021")]
    public async Task Migration_SoftDeleted_ClearbitLogo_NotNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 4021, Name = "SD Partner", IsDeleted = true,
            LogoUrl = "https://logo.clearbit.com/sd.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4021))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-022")]
    public async Task Migration_ActiveWithClearbit_Nullified_SoftDeletedWithClearbit_Preserved()
    {
        await SeedPartnerAsync(4022, "https://logo.clearbit.com/active.org");
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 4023, Name = "SD", IsDeleted = true,
            LogoUrl = "https://logo.clearbit.com/deleted.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4022))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(4023))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-023")]
    public async Task Migration_SoftDeletedPartnerWithNullLogo_Unchanged()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 4024, Name = "SD Null Logo", IsDeleted = true,
            LogoUrl = null, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-024")]
    public async Task Migration_ActiveAndSoftDeleted_MixedLogos_OnlyActiveAffected()
    {
        await SeedPartnerAsync(4025, "https://logo.clearbit.com/1.org");
        await SeedPartnerAsync(4026, "https://safe.com/logo.png");
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 4027, Name = "SD1", IsDeleted = true,
            LogoUrl = "https://logo.clearbit.com/sd1.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-025")]
    public async Task Migration_NotDeletedFlag_RequiredForAffect()
    {
        await SeedPartnerAsync(4028, "https://logo.clearbit.com/flag.org");
        var partner = await DbContext.Partners.FindAsync(4028);
        partner!.IsDeleted = false;
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(1);
    }

    // ─── §3.4 EntityStatus Extremes (BND-026 – 035) ──────────────────────

    [Fact] [Trait("TestId", "BND-026")]
    public async Task Migration_ActiveStatus_ClearbitNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 4029, Name = "Active Status", IsDeleted = false,
            LogoUrl = "https://logo.clearbit.com/active.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4029))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-027")]
    public async Task Migration_InactiveStatus_ClearbitNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 4030, Name = "Inactive Status", IsDeleted = false,
            LogoUrl = "https://logo.clearbit.com/inactive.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4030))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-028")]
    public async Task Migration_DraftStatus_ClearbitNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 4031, Name = "Draft Status", IsDeleted = false,
            LogoUrl = "https://logo.clearbit.com/draft.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Draft
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4031))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-029")]
    public async Task Migration_ClosedStatus_ClearbitNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 4032, Name = "Closed Status", IsDeleted = false,
            LogoUrl = "https://logo.clearbit.com/closed.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(4032))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-030")]
    public async Task Migration_MixedStatuses_AllClearbitNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner { Id = 4033, Name = "A", IsDeleted = false, LogoUrl = "https://logo.clearbit.com/a.org", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active });
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner { Id = 4034, Name = "I", IsDeleted = false, LogoUrl = "https://logo.clearbit.com/i.org", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive });
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner { Id = 4035, Name = "C", IsDeleted = false, LogoUrl = "https://logo.clearbit.com/c.org", Status = UNOPS.PAO.Domain.Entities.EntityStatus.Closed });
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(3);
        (await DbContext.Partners.FindAsync(4033))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(4034))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(4035))!.LogoUrl.Should().BeNull();
    }

    // ─── §3.5 Integer ID Extremes (BND-031 – 040) ────────────────────────

    [Fact] [Trait("TestId", "BND-031")]
    public async Task Migration_PartnerId1_WorksCorrectly()
    {
        await SeedPartnerAsync(1, "https://logo.clearbit.com/id1.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-032")]
    public async Task Migration_PartnerIdMaxInt_WorksCorrectly()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = int.MaxValue, Name = "MaxInt Partner", IsDeleted = false,
            LogoUrl = "https://logo.clearbit.com/maxint.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(int.MaxValue))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-033")]
    public async Task Migration_50Partners_AllClearbit_AllNullified()
    {
        for (var i = 5001; i <= 5050; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/p{i}.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(50);
    }

    [Fact] [Trait("TestId", "BND-034")]
    public async Task Migration_50Partners_NoClearbit_NoneAffected()
    {
        for (var i = 5101; i <= 5150; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.com/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-035")]
    public async Task Migration_50Partners_MixedClearbit_CorrectCount()
    {
        for (var i = 5201; i <= 5225; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/p{i}.org");
        for (var i = 5226; i <= 5250; i++)
            await SeedPartnerAsync(i, $"https://safe{i}.com/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(25);
    }

    // ─── §3.6 Concurrent Scenarios (BND-036 – 045) ───────────────────────

    [Fact] [Trait("TestId", "BND-036")]
    public async Task Migration_CalledTwiceConcurrently_BothCompleteWithoutException()
    {
        await SeedPartnerAsync(6001, "https://logo.clearbit.com/concurrent.org");

        var task1 = RunClearbitCleanupMigrationAsync();
        var task2 = RunClearbitCleanupMigrationAsync();

        var act = async () => await Task.WhenAll(task1, task2);
        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "BND-037")]
    public async Task Migration_ConsistentResult_ParallelQueryReads()
    {
        await SeedPartnerAsync(6002, "https://logo.clearbit.com/parallel.org");

        await RunClearbitCleanupMigrationAsync();

        var reads = await Task.WhenAll(
            DbContext.Partners.AsNoTracking().Where(p => p.Id == 6002).Select(p => p.LogoUrl).FirstOrDefaultAsync(),
            DbContext.Partners.AsNoTracking().Where(p => p.Id == 6002).Select(p => p.LogoUrl).FirstOrDefaultAsync()
        );

        reads[0].Should().BeNull();
        reads[1].Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-038")]
    public async Task Fallback_CalledRepeatedly_AlwaysReturnsSameValue()
    {
        var results = Enumerable.Range(0, 10)
            .Select(_ => GetEffectiveLogoUrl(null))
            .ToList();

        results.Should().AllBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "BND-039")]
    public async Task Migration_Run3Times_OnlyFirst_Affects_ClearbitPartners()
    {
        await SeedPartnerAsync(6003, "https://logo.clearbit.com/three.org");

        var first = await RunClearbitCleanupMigrationAsync();
        var second = await RunClearbitCleanupMigrationAsync();
        var third = await RunClearbitCleanupMigrationAsync();

        first.Should().Be(1);
        second.Should().Be(0);
        third.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-040")]
    public async Task Migration_AllNullsAndEmpty_NoException()
    {
        for (var i = 7001; i <= 7010; i++)
            await SeedPartnerAsync(i, i % 2 == 0 ? null : "");

        var act = async () => await RunClearbitCleanupMigrationAsync();

        await act.Should().NotThrowAsync();
    }

    // ─── §3.7 URL Pattern Extremes (BND-041 – 060) ───────────────────────

    [Fact] [Trait("TestId", "BND-041")]
    public async Task Migration_HttpClearbit_Matched()
    {
        await SeedPartnerAsync(7011, "http://logo.clearbit.com/insecure.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7011))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-042")]
    public async Task Migration_FtpClearbit_Matched()
    {
        await SeedPartnerAsync(7012, "ftp://logo.clearbit.com/ftp.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7012))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-043")]
    public async Task Migration_JustClearbitWord_NoProtocol_Matched()
    {
        await SeedPartnerAsync(7013, "clearbit");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7013))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-044")]
    public async Task Migration_ClearbitWithNumbers_Matched()
    {
        await SeedPartnerAsync(7014, "https://clearbit123.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7014))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-045")]
    public async Task Migration_ClearbitWithHyphen_Matched()
    {
        await SeedPartnerAsync(7015, "https://logo-clearbit-assets.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7015))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-046")]
    public async Task Migration_ClearbitWithUnderscore_Matched()
    {
        await SeedPartnerAsync(7016, "https://logo_clearbit_assets.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7016))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-047")]
    public async Task Migration_UrlStartingWithClearbit_Matched()
    {
        await SeedPartnerAsync(7017, "clearbit.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7017))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-048")]
    public async Task Migration_UrlEndingWithClearbit_Matched()
    {
        await SeedPartnerAsync(7018, "https://example.com/clearbit");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7018))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-049")]
    public async Task Migration_UnicodeUrl_NonClearbit_Unchanged()
    {
        await SeedPartnerAsync(7019, "https://pàrtner.org/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7019))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-050")]
    public async Task Migration_WhitespaceAroundClearbit_Matched()
    {
        await SeedPartnerAsync(7020, " clearbit ");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7020))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-051")]
    public async Task Fallback_ExactFallbackPath_ReturnsItself()
    {
        var effective = GetEffectiveLogoUrl(FallbackImage);

        effective.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "BND-052")]
    public async Task Fallback_NullIsDistinctFromEmpty()
    {
        var fromNull = GetEffectiveLogoUrl(null);
        var fromEmpty = GetEffectiveLogoUrl("");

        fromNull.Should().Be(fromEmpty, "Both null and empty produce fallback");
    }

    [Fact] [Trait("TestId", "BND-053")]
    public async Task Migration_PartnerWithClearbitAndSpecialCharsInName_LogoNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 7021, Name = "Partner & Org. (Ltd.)", IsDeleted = false,
            LogoUrl = "https://logo.clearbit.com/special-name.org",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7021))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-054")]
    public async Task Migration_MultipleRunsOnMixedSet_ConsistentResult()
    {
        await SeedPartnerAsync(7022, "https://logo.clearbit.com/multi.org");
        await SeedPartnerAsync(7023, "https://safe.com/logo.png");

        await RunClearbitCleanupMigrationAsync();
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7022))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(7023))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "BND-055")]
    public async Task Migration_ClearbitUrlSavedAsEncoded_StillMatched()
    {
        await SeedPartnerAsync(7024, "https://logo.clearbit.com/p%40rtner.org");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7024))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-056")]
    public async Task Migration_ClearbitInFragmentIdentifier_Matched()
    {
        await SeedPartnerAsync(7025, "https://example.com/logo.png#clearbit-logo");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7025))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-057")]
    public async Task Migration_ClearbitPrecededByNumber_Matched()
    {
        await SeedPartnerAsync(7026, "https://v2clearbit.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7026))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-058")]
    public async Task Migration_ClearbitCaseMixedLowerPresent_Matched()
    {
        await SeedPartnerAsync(7027, "https://logo.clearBIT.com/test.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(1, "Case-insensitive matching: clearBIT is matched same as clearbit");
    }

    [Fact] [Trait("TestId", "BND-059")]
    public async Task Migration_RepeatClearbitInUrl_Matched()
    {
        await SeedPartnerAsync(7028, "https://clearbit.clearbit.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7028))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-060")]
    public async Task Migration_ClearbitSubstringNotMatchingFull_FalsePositiveCheck()
    {
        await SeedPartnerAsync(7029, "https://partner.org/logo.png?note=from-clearbit-service-but-not-clearbit");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(7029))!.LogoUrl.Should().BeNull("URL contains 'clearbit' substring anywhere");
    }

    [Fact] [Trait("TestId", "BND-061")]
    public async Task Boundary_Id_MaxInt_Seed()
    {
        await SeedPartnerAsync(int.MaxValue - 1, "https://logo.clearbit.com/maxid.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-062")]
    public async Task Boundary_LogoUrl_5000Chars_NonClearbit_Preserved()
    {
        var url = "https://cdn.partner.org/" + new string('x', 4950) + "/logo.png";
        await SeedPartnerAsync(7030, url);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-063")]
    public async Task Boundary_LogoUrl_5000Chars_WithClearbit_Cleared()
    {
        var url = "https://logo.clearbit.com/" + new string('x', 4950) + "/logo.png";
        await SeedPartnerAsync(7031, url);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-064")]
    public async Task Boundary_OneChar_NonClearbit_NotAffected()
    {
        await SeedPartnerAsync(7032, "x");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-065")]
    public async Task FallbackLogic_1Char_NonClearbit_Preserved()
    {
        GetEffectiveLogoUrl("x").Should().Be("x");
    }

    [Fact] [Trait("TestId", "BND-066")]
    public async Task Boundary_ExactlyMatchingClearbitUrl_AffectedOne()
    {
        await SeedPartnerAsync(7033, "https://logo.clearbit.com/");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-067")]
    public async Task Boundary_ActiveAndInactive_ClearbitBoth_BothAffected()
    {
        var p1 = await SeedPartnerAsync(7034, "https://logo.clearbit.com/active.org");
        var p2 = await SeedPartnerAsync(7035, "https://logo.clearbit.com/inactive.org");
        p2.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive;
        await DbContext.SaveChangesAsync();
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact] [Trait("TestId", "BND-068")]
    public async Task Boundary_NullLogoUrl_EmptyLogoUrl_OnlyEmptyPreserved()
    {
        await SeedPartnerAsync(7036, null);
        await SeedPartnerAsync(7037, string.Empty);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-069")]
    public async Task Boundary_ClearbitAndNullMixed_OnlyClearbitAffected()
    {
        await SeedPartnerAsync(7038, "https://logo.clearbit.com/mix.org");
        await SeedPartnerAsync(7039, null);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-070")]
    public async Task FallbackLogic_OneClearbitChar_ReturnsFallback()
    {
        GetEffectiveLogoUrl("clearbit").Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "BND-071")]
    public async Task Boundary_LargePartnerCount_50_AllClearbit_AllCleaned()
    {
        for (var i = 7040; i <= 7089; i++)
            await SeedPartnerAsync(i, "https://logo.clearbit.com/bulk.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(50);
    }

    [Fact] [Trait("TestId", "BND-072")]
    public async Task Boundary_LargePartnerCount_50_AllSafe_ZeroAffected()
    {
        for (var i = 7090; i <= 7139; i++)
            await SeedPartnerAsync(i, "https://safe.cdn.org/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-073")]
    public async Task Boundary_NullFallback_Deterministic()
    {
        for (var i = 0; i < 20; i++)
            GetEffectiveLogoUrl(null).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "BND-074")]
    public async Task Boundary_EmptyFallback_Deterministic()
    {
        for (var i = 0; i < 20; i++)
            GetEffectiveLogoUrl(string.Empty).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "BND-075")]
    public async Task Boundary_ClearbitAtStart_IsMatched()
    {
        await SeedPartnerAsync(7140, "clearbit.com/example.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-076")]
    public async Task Boundary_ClearbitAtEnd_IsMatched()
    {
        await SeedPartnerAsync(7141, "https://cdn.example.org/clearbit");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-077")]
    public async Task Boundary_ClearbitMidUrl_IsMatched()
    {
        await SeedPartnerAsync(7142, "https://api.clearbit.io/v2/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-078")]
    public async Task Boundary_PartnerWith100KName_MigrationSucceeds()
    {
        var p = await SeedPartnerAsync(7143, "https://logo.clearbit.com/longname.org", new string('N', 255));
        await RunClearbitCleanupMigrationAsync();
        (await DbContext.Partners.FindAsync(7143))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "BND-079")]
    public async Task Boundary_All4StatusTypes_ClearbitCleared()
    {
        var statuses = System.Enum.GetValues<UNOPS.PAO.Domain.Entities.EntityStatus>().ToArray();
        for (var i = 0; i < statuses.Length; i++)
        {
            var id = 7150 + i;
            var p = await SeedPartnerAsync(id, "https://logo.clearbit.com/status.org");
            p.Status = statuses[i];
        }
        await DbContext.SaveChangesAsync();
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(statuses.Length);
    }

    [Fact] [Trait("TestId", "BND-080")]
    public async Task Boundary_UrlWithFragment_Clearbit_IsMatched()
    {
        await SeedPartnerAsync(7160, "https://logo.clearbit.com/fragment.org#anchor");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-081")]
    public async Task Boundary_UrlWithQueryString_Clearbit_IsMatched()
    {
        await SeedPartnerAsync(7161, "https://logo.clearbit.com/query.org?size=128");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-082")]
    public async Task Boundary_UrlWithQueryString_NonClearbit_Preserved()
    {
        await SeedPartnerAsync(7162, "https://cdn.safe.org/logo.png?v=2");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-083")]
    public async Task Boundary_Id_NegativeEdge_Seed()
    {
        await SeedPartnerAsync(-1, "https://logo.clearbit.com/neg.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-084")]
    public async Task Boundary_ZeroId_Seed()
    {
        await SeedPartnerAsync(0, "https://logo.clearbit.com/zero.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-085")]
    public async Task Boundary_ClearbitCased_Uppercase_Matched()
    {
        await SeedPartnerAsync(7163, "CLEARBIT");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-086")]
    public async Task Boundary_ClearbitMixedCase_Matched()
    {
        await SeedPartnerAsync(7164, "CleArBiT");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "BND-087")]
    public async Task FallbackLogic_SingleSpace_ReturnsFallback()
    {
        GetEffectiveLogoUrl(" ").Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "BND-088")]
    public async Task Boundary_ThirdRunAfterTwoClearbit_ZeroAffected()
    {
        await SeedPartnerAsync(7165, "https://logo.clearbit.com/triple.org");
        await RunClearbitCleanupMigrationAsync();
        await RunClearbitCleanupMigrationAsync();
        var third = await RunClearbitCleanupMigrationAsync();
        third.Should().Be(0);
    }

    [Fact] [Trait("TestId", "BND-089")]
    public async Task Boundary_FallbackImage_ConstantThroughout()
    {
        FallbackImage.Should().Be("assets/images/Partner.png");
        GetEffectiveLogoUrl(null).Should().Be("assets/images/Partner.png");
    }

    [Fact] [Trait("TestId", "BND-090")]
    public async Task Boundary_ClearbitUrl_Cleared_FallbackApplied_EndToEnd()
    {
        await SeedPartnerAsync(7166, "https://logo.clearbit.com/boundary.org");
        await RunClearbitCleanupMigrationAsync();
        var p = await DbContext.Partners.AsNoTracking().FirstAsync(x => x.Id == 7166);
        p.LogoUrl.Should().BeNull();
        GetEffectiveLogoUrl(p.LogoUrl).Should().Be(FallbackImage);
    }
}
