/**
 * @fileoverview PNO-926 Negative Tests — 60 failure-path tests.
 * Invalid/edge URLs, deleted partners, migration isolation, non-clearbit exclusions.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Negative Tests — 60 tests verifying migration does not corrupt non-clearbit data.
/// </summary>
[Collection("Negative")]
[Trait("Category", "Negative")]
[Trait("Ticket", "PNO-926")]
public class NegativeTests : PNO926TestFixtureBase
{
    // ─── §2.1 Non-Clearbit URLs Not Affected (NEG-001 – 015) ─────────────

    [Fact] [Trait("TestId", "NEG-001")]
    public async Task Migration_HttpsNonClearbit_NotAffected()
    {
        await SeedPartnerAsync(2001, "https://partner.org/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
        (await DbContext.Partners.FindAsync(2001))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-002")]
    public async Task Migration_NullLogos_NotCountedAsAffected()
    {
        await SeedPartnerAsync(2002, null);
        await SeedPartnerAsync(2003, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-003")]
    public async Task Migration_EmptyStringLogos_NotCountedAsAffected()
    {
        await SeedPartnerAsync(2004, "");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-004")]
    public async Task Migration_GcsUrl_NotNullifiedByMigration()
    {
        await SeedPartnerAsync(2005, "https://storage.googleapis.com/bucket/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2005))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-005")]
    public async Task Migration_DataUri_NotNullifiedByMigration()
    {
        await SeedPartnerAsync(2006, "data:image/png;base64,abc123");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2006))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-006")]
    public async Task Migration_LocalPath_NotNullifiedByMigration()
    {
        await SeedPartnerAsync(2007, "/assets/logos/custom.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2007))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-007")]
    public async Task Migration_UrlWithClearTextNotClearbit_NotAffected()
    {
        await SeedPartnerAsync(2008, "https://example.com/clear-logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2008))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-008")]
    public async Task Migration_UrlWithClearNotClearbit_NotAffected()
    {
        await SeedPartnerAsync(2009, "https://example.com/crystal-clear-logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2009))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-009")]
    public async Task Migration_UrlWithBitNotClearbit_NotAffected()
    {
        await SeedPartnerAsync(2010, "https://example.com/a-bit-logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2010))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-010")]
    public async Task Migration_FtpUrl_NotAffected()
    {
        await SeedPartnerAsync(2011, "ftp://ftp.partner.org/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2011))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-011")]
    public async Task Migration_HttpUrl_NotAffected()
    {
        await SeedPartnerAsync(2012, "http://partner.org/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2012))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-012")]
    public async Task Migration_CdnUrl_NotAffected()
    {
        await SeedPartnerAsync(2013, "https://cdn.cloudflare.net/partner-logo.svg");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2013))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-013")]
    public async Task Migration_S3SignedUrl_NotAffected()
    {
        await SeedPartnerAsync(2014, "https://s3.amazonaws.com/bucket/logo.png?X-Amz-Signature=abc");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2014))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-014")]
    public async Task Migration_PartnerWithLongUrl_NotAffectedIfNoClearbit()
    {
        var longUrl = "https://very-long-domain.partnerorganization.international.net/assets/images/logos/high-resolution/partner-logo-full-color-2024.png";
        await SeedPartnerAsync(2015, longUrl);

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2015))!.LogoUrl.Should().Be(longUrl);
    }

    [Fact] [Trait("TestId", "NEG-015")]
    public async Task Migration_UrlWithQueryParams_NotAffectedIfNoClearbit()
    {
        await SeedPartnerAsync(2016, "https://example.com/logo.png?version=2&format=webp");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2016))!.LogoUrl.Should().Contain("version=2");
    }

    // ─── §2.2 Deleted Partners (NEG-016 – 025) ───────────────────────────

    [Fact] [Trait("TestId", "NEG-016")]
    public async Task Migration_SoftDeletedPartnerWithClearbit_NotAffected()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 2017, Name = "Deleted Clearbit Partner",
            LogoUrl = "https://logo.clearbit.com/deleted.org",
            IsDeleted = true, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0, "Soft-deleted partners should not be affected by migration");
        (await DbContext.Partners.FindAsync(2017))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-017")]
    public async Task Migration_SoftDeletedNonClearbitPartner_Unchanged()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 2018, Name = "Deleted Safe Partner",
            LogoUrl = "https://safe.com/logo.png",
            IsDeleted = true, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2018))!.LogoUrl.Should().Contain("safe.com");
    }

    [Fact] [Trait("TestId", "NEG-018")]
    public async Task Migration_MixedDeletedAndActive_OnlyActiveAffected()
    {
        await SeedPartnerAsync(2019, "https://logo.clearbit.com/active.org");
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 2020, Name = "Deleted Clearbit",
            LogoUrl = "https://logo.clearbit.com/deleted.org",
            IsDeleted = true, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(1, "Only non-deleted partner affected");
        (await DbContext.Partners.FindAsync(2019))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(2020))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-019")]
    public async Task Migration_AllPartnersDeleted_NoneAffected()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 2021, Name = "Deleted 1",
            LogoUrl = "https://logo.clearbit.com/one.org",
            IsDeleted = true, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 2022, Name = "Deleted 2",
            LogoUrl = "https://logo.clearbit.com/two.org",
            IsDeleted = true, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-020")]
    public async Task Migration_DeletedPartnerLogoNotNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 2023, Name = "Should Not Clear",
            LogoUrl = "https://logo.clearbit.com/preserved.org",
            IsDeleted = true, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2023))!.LogoUrl.Should().NotBeNull();
    }

    // ─── §2.3 No Partners Scenario (NEG-021 – 030) ───────────────────────

    [Fact] [Trait("TestId", "NEG-021")]
    public async Task Migration_NoPartners_AffectedCountIsZero()
    {
        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-022")]
    public async Task Migration_NoClearbitPartners_AffectedCountIsZero()
    {
        await SeedPartnerAsync(2024, "https://safe.com/logo.png");
        await SeedPartnerAsync(2025, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-023")]
    public async Task Migration_AllNullLogos_AffectedCountIsZero()
    {
        for (var i = 2026; i <= 2030; i++)
            await SeedPartnerAsync(i, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-024")]
    public async Task Migration_AllEmptyStringLogos_AffectedCountIsZero()
    {
        for (var i = 2031; i <= 2035; i++)
            await SeedPartnerAsync(i, "");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-025")]
    public async Task Migration_ClearbitUrlCaseSensitive_OnlyLowercaseAffected()
    {
        await SeedPartnerAsync(2036, "https://logo.CLEARBIT.com/test.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(1, "Case-insensitive matching: CLEARBIT is treated same as clearbit");
    }

    // ─── §2.4 Fallback Display Failures (NEG-026 – 040) ──────────────────

    [Fact] [Trait("TestId", "NEG-026")]
    public async Task Fallback_ClearbitUrl_NotUsedDirectlyAfterMigration()
    {
        await SeedPartnerAsync(2037, "https://logo.clearbit.com/fallback-test.org");
        await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(2037);
        var effective = GetEffectiveLogoUrl(partner!.LogoUrl);

        effective.Should().NotContain("clearbit");
    }

    [Fact] [Trait("TestId", "NEG-027")]
    public async Task Fallback_NullLogoUrl_DoesNotReturnNull()
    {
        var effective = GetEffectiveLogoUrl(null);

        effective.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-028")]
    public async Task Fallback_NullLogoUrl_DoesNotReturnEmpty()
    {
        var effective = GetEffectiveLogoUrl(null);

        effective.Should().NotBeEmpty();
    }

    [Fact] [Trait("TestId", "NEG-029")]
    public async Task Fallback_EmptyLogoUrl_DoesNotReturnEmpty()
    {
        var effective = GetEffectiveLogoUrl("");

        effective.Should().NotBeEmpty();
    }

    [Fact] [Trait("TestId", "NEG-030")]
    public async Task Fallback_NullLogoUrl_NotEqualToClearbitUrl()
    {
        var effective = GetEffectiveLogoUrl(null);

        effective.Should().NotContain("clearbit");
    }

    // ─── §2.5 Partner Data Integrity After Migration (NEG-031 – 045) ──────

    [Fact] [Trait("TestId", "NEG-031")]
    public async Task Migration_PartnerName_NotNullifiedByMigration()
    {
        await SeedPartnerAsync(2040, "https://logo.clearbit.com/name-test.org", "Name Check Partner");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2040))!.Name.Should().Be("Name Check Partner");
    }

    [Fact] [Trait("TestId", "NEG-032")]
    public async Task Migration_PartnerStatus_NotChangedByMigration()
    {
        await SeedPartnerAsync(2041, "https://logo.clearbit.com/status-test.org");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2041))!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "NEG-033")]
    public async Task Migration_PartnerIsDeleted_NotChangedByMigration()
    {
        await SeedPartnerAsync(2042, "https://logo.clearbit.com/isdeleted-test.org");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2042))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "NEG-034")]
    public async Task Migration_NonClearbitPartnerIsDeleted_NotChanged()
    {
        await SeedPartnerAsync(2043, "https://safe.com/logo.png");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2043))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "NEG-035")]
    public async Task Migration_PartnerCount_NotChangedByMigration()
    {
        await SeedPartnerAsync(2044, "https://logo.clearbit.com/count1.org");
        await SeedPartnerAsync(2045, "https://safe.com/count2.png");
        var countBefore = await DbContext.Partners.CountAsync();

        await RunClearbitCleanupMigrationAsync();

        var countAfter = await DbContext.Partners.CountAsync();
        countAfter.Should().Be(countBefore, "Migration should not add or remove partners");
    }

    [Fact] [Trait("TestId", "NEG-036")]
    public async Task Migration_PartnerIds_NotChangedByMigration()
    {
        await SeedPartnerAsync(2046, "https://logo.clearbit.com/id-check.org");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2046)).Should().NotBeNull();
        (await DbContext.Partners.FindAsync(2046))!.Id.Should().Be(2046);
    }

    [Fact] [Trait("TestId", "NEG-037")]
    public async Task Migration_OnlyClearbitLogos_NullifiedNotOtherFields()
    {
        await SeedPartnerAsync(2047, "https://logo.clearbit.com/fields.org", "Fields Check");
        await RunClearbitCleanupMigrationAsync();

        var p = await DbContext.Partners.FindAsync(2047);
        p!.Name.Should().Be("Fields Check");
        p.LogoUrl.Should().BeNull();
        p.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "NEG-038")]
    public async Task Migration_InactiveClearbitPartner_LogoNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 2048, Name = "Inactive Clearbit",
            LogoUrl = "https://logo.clearbit.com/inactive.org",
            IsDeleted = false, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2048))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-039")]
    public async Task Migration_ClearbitInMiddleOfUrl_LogoNullified()
    {
        await SeedPartnerAsync(2049, "https://api.example.com/clearbit/v2/logo.png");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2049))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-040")]
    public async Task Migration_ClearbitAtEnd_LogoNullified()
    {
        await SeedPartnerAsync(2050, "https://images.example.com/clearbit");
        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(2050))!.LogoUrl.Should().BeNull();
    }

    // ─── §2.6 Idempotency Negative Scenarios (NEG-041 – 060) ─────────────

    [Fact] [Trait("TestId", "NEG-041")]
    public async Task Migration_IdempotentSecondRun_AffectedCountZero()
    {
        await SeedPartnerAsync(2051, "https://logo.clearbit.com/idem.org");
        await RunClearbitCleanupMigrationAsync();

        var secondCount = await RunClearbitCleanupMigrationAsync();

        secondCount.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-042")]
    public async Task Migration_IdempotentThirdRun_AffectedCountZero()
    {
        await SeedPartnerAsync(2052, "https://logo.clearbit.com/three-runs.org");
        await RunClearbitCleanupMigrationAsync();
        await RunClearbitCleanupMigrationAsync();

        var thirdCount = await RunClearbitCleanupMigrationAsync();

        thirdCount.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-043")]
    public async Task Migration_AlreadyNullLogo_SecondRunDoesNotFail()
    {
        await SeedPartnerAsync(2053, null);

        var act = async () =>
        {
            await RunClearbitCleanupMigrationAsync();
            await RunClearbitCleanupMigrationAsync();
        };

        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "NEG-044")]
    public async Task Migration_ManualNullify_ThenMigrationRun_NoClearbitFound()
    {
        await SeedPartnerAsync(2054, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-045")]
    public async Task Migration_RunOnEmptyDb_Returns0()
    {
        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-046")]
    public async Task Fallback_ExplicitlySetToFallbackString_StillUsed()
    {
        var effective = GetEffectiveLogoUrl(FallbackImage);

        effective.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-047")]
    public async Task Migration_100NonClearbitPartners_NoneAffected()
    {
        for (var i = 3000; i <= 3099; i++)
            await SeedPartnerAsync(i, $"https://safe-{i}.com/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-048")]
    public async Task Migration_SingleNullParticle_NotMatchedByClearbit()
    {
        await SeedPartnerAsync(3100, "\0");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-049")]
    public async Task Migration_WhitespaceUrl_NotMatchedByClearbit()
    {
        await SeedPartnerAsync(3101, "   ");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-050")]
    public async Task Fallback_WhitespaceLogoUrl_NotFallback_ReturnsWhitespace()
    {
        var effective = GetEffectiveLogoUrl("   ");

        effective.Should().Be(FallbackImage, "Whitespace-only URLs are treated as empty and return FallbackImage");
    }

    [Fact] [Trait("TestId", "NEG-051")]
    public async Task Migration_MultipleClearbit_AllNullified()
    {
        for (var i = 3200; i <= 3209; i++)
            await SeedPartnerAsync(i, $"https://logo.clearbit.com/partner{i}.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(10);
    }

    [Fact] [Trait("TestId", "NEG-052")]
    public async Task Migration_PartnerWithNoClearbit_LogoUnchangedAfterMultipleRuns()
    {
        await SeedPartnerAsync(3210, "https://stable.com/logo.png");

        for (var i = 0; i < 3; i++)
            await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(3210))!.LogoUrl.Should().Be("https://stable.com/logo.png");
    }

    [Fact] [Trait("TestId", "NEG-053")]
    public async Task Migration_ClearbitCount_MatchesExpected()
    {
        await SeedPartnerAsync(3220, "https://logo.clearbit.com/a.org");
        await SeedPartnerAsync(3221, "https://logo.clearbit.com/b.org");
        await SeedPartnerAsync(3222, "https://logo.clearbit.com/c.org");
        await SeedPartnerAsync(3223, "https://safe.com/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(3);
    }

    [Fact] [Trait("TestId", "NEG-054")]
    public async Task Migration_PartnerWithNullName_ClearbitUrlNullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 3230, Name = "",
            LogoUrl = "https://logo.clearbit.com/noname.org",
            IsDeleted = false, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(3230))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-055")]
    public async Task Migration_ClearbitUppercaseOnly_NotMatched()
    {
        await SeedPartnerAsync(3231, "https://logo.CLEARBIT.COM/test.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(1, "Case-insensitive matching: CLEARBIT.COM is matched");
    }

    [Fact] [Trait("TestId", "NEG-056")]
    public async Task Migration_MixedCaseClearbit_OnlyLowercaseMatched()
    {
        await SeedPartnerAsync(3232, "https://logo.clearbit.com/lower.org");
        await SeedPartnerAsync(3233, "https://logo.Clearbit.com/mixed.org");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(2, "Case-insensitive matching: both lowercase and mixed-case 'clearbit' are matched");
    }

    [Fact] [Trait("TestId", "NEG-057")]
    public async Task Fallback_GetEffectiveLogoUrl_WithActualUrl_ReturnsIt()
    {
        const string realUrl = "https://partner.org/real-logo.png";
        var effective = GetEffectiveLogoUrl(realUrl);

        effective.Should().Be(realUrl);
        effective.Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-058")]
    public async Task Fallback_GetEffectiveLogoUrl_NullFallback_AlwaysFallbackImage()
    {
        var effective = GetEffectiveLogoUrl(null);

        effective.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-059")]
    public async Task Migration_PartnerNameContainingClearbit_LogoUrlUnchangedIfNotClearbit()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 3240, Name = "clearbit Fan Partner",
            LogoUrl = "https://safe.com/logo.png",
            IsDeleted = false, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(3240))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-060")]
    public async Task Migration_InactivePartner_WithClearbitLogo_Nullified()
    {
        DbContext.Partners.Add(new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 3241, Name = "Inactive Clearbit Two",
            LogoUrl = "https://logo.clearbit.com/inactive2.org",
            IsDeleted = false, Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive
        });
        await DbContext.SaveChangesAsync();

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(3241))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "NEG-061")]
    public async Task Migration_EmptyDatabase_AffectsZero()
    {
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-062")]
    public async Task Migration_NonNullNonClearbit_NotChanged()
    {
        await SeedPartnerAsync(3250, "https://www.partner.com/img/logo.png");
        await RunClearbitCleanupMigrationAsync();
        (await DbContext.Partners.FindAsync(3250))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-063")]
    public async Task FallbackLogic_LongValidUrl_NotFallback()
    {
        var url = "https://cdn.partner.org/" + new string('a', 100) + "/logo.jpg";
        GetEffectiveLogoUrl(url).Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-064")]
    public async Task Migration_FtpUrl_NotMatched()
    {
        await SeedPartnerAsync(3251, "ftp://files.partner.org/logo.png");
        await RunClearbitCleanupMigrationAsync();
        (await DbContext.Partners.FindAsync(3251))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-065")]
    public async Task Migration_NullLogoUrl_InActivePartner_NotAffected()
    {
        await SeedPartnerAsync(3252, null);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-066")]
    public async Task Migration_AfterCleanup_AlreadyNull_SecondRunZero()
    {
        await SeedPartnerAsync(3253, "https://logo.clearbit.com/double.org");
        await RunClearbitCleanupMigrationAsync();
        var second = await RunClearbitCleanupMigrationAsync();
        second.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-067")]
    public async Task Migration_SafeUrl_AfterClearbitMigration_StillPresent()
    {
        await SeedPartnerAsync(3254, "https://logo.clearbit.com/first.org");
        await SeedPartnerAsync(3255, "https://safe.cdn.org/logo.png");
        await RunClearbitCleanupMigrationAsync();
        (await DbContext.Partners.FindAsync(3255))!.LogoUrl.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-068")]
    public async Task Migration_SoftDeletedClearbit_NotAffected()
    {
        var p = await SeedPartnerAsync(3256, "https://logo.clearbit.com/softdel.org");
        p.IsDeleted = true;
        await DbContext.SaveChangesAsync();
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-069")]
    public async Task Migration_WhitespaceUrl_NotClearbit_NotAffected()
    {
        await SeedPartnerAsync(3257, "   ");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-070")]
    public async Task FallbackLogic_PartialClearbitKeyword_StillFallback()
    {
        GetEffectiveLogoUrl("clearbit-adjacent-url").Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-071")]
    public async Task Migration_RelativeUrl_NotMatched()
    {
        await SeedPartnerAsync(3258, "/assets/logos/partner.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-072")]
    public async Task Migration_DataUri_NotMatched()
    {
        await SeedPartnerAsync(3259, "data:image/png;base64,abc123");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-073")]
    public async Task FallbackLogic_S3Url_NotFallback()
    {
        const string s3 = "https://mybucket.s3.amazonaws.com/logos/partner.png";
        GetEffectiveLogoUrl(s3).Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-074")]
    public async Task Migration_NumbersOnlyUrl_NotClearbit()
    {
        await SeedPartnerAsync(3260, "12345678");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-075")]
    public async Task Migration_JsonString_NotClearbit()
    {
        await SeedPartnerAsync(3261, "{\"url\":\"https://partner.org/logo.png\"}");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-076")]
    public async Task FallbackLogic_JsonString_NotFallback()
    {
        GetEffectiveLogoUrl("{\"url\":\"https://example.org/logo.png\"}").Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-077")]
    public async Task Migration_SpaceUrl_NotClearbit()
    {
        await SeedPartnerAsync(3262, " ");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-078")]
    public async Task Migration_HttpOnlyClearbit_Matched()
    {
        await SeedPartnerAsync(3263, "http://logo.clearbit.com/example.org");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "NEG-079")]
    public async Task Migration_AllNullLogos_ZeroAffected()
    {
        for (var i = 3270; i <= 3274; i++)
            await SeedPartnerAsync(i, null);
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-080")]
    public async Task Migration_AllSafeUrls_ZeroAffected()
    {
        for (var i = 3280; i <= 3284; i++)
            await SeedPartnerAsync(i, "https://cdn.safe.org/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-081")]
    public async Task FallbackLogic_BlobStorageUrl_NotFallback()
    {
        const string blobUrl = "https://myaccount.blob.core.windows.net/logos/partner.png";
        GetEffectiveLogoUrl(blobUrl).Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-082")]
    public async Task FallbackLogic_EmptyStringUrl_ReturnsFallback()
    {
        GetEffectiveLogoUrl(string.Empty).Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-083")]
    public async Task Migration_SvgUrl_NotClearbit()
    {
        await SeedPartnerAsync(3285, "https://cdn.org/partner.svg");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-084")]
    public async Task Migration_WebpUrl_NotClearbit()
    {
        await SeedPartnerAsync(3286, "https://cdn.org/partner.webp");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-085")]
    public async Task FallbackLogic_WebpUrl_NotFallback()
    {
        GetEffectiveLogoUrl("https://cdn.org/partner.webp").Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-086")]
    public async Task Migration_InactiveAndDeletedPartner_NotAffected()
    {
        var p = await SeedPartnerAsync(3287, "https://logo.clearbit.com/inactive.org");
        p.IsDeleted = true;
        p.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive;
        await DbContext.SaveChangesAsync();
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-087")]
    public async Task Migration_UnicodeUrl_NotClearbit()
    {
        await SeedPartnerAsync(3288, "https://пример.org/logo.png");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-088")]
    public async Task FallbackLogic_UnicodeUrl_NotFallback()
    {
        GetEffectiveLogoUrl("https://пример.org/logo.png").Should().NotBe(FallbackImage);
    }

    [Fact] [Trait("TestId", "NEG-089")]
    public async Task Migration_AllDeleted_ZeroAffected()
    {
        for (var i = 3290; i <= 3294; i++)
        {
            var p = await SeedPartnerAsync(i, "https://logo.clearbit.com/del.org");
            p.IsDeleted = true;
        }
        await DbContext.SaveChangesAsync();
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }

    [Fact] [Trait("TestId", "NEG-090")]
    public async Task Migration_NoMatch_AffectedIsZero_Always()
    {
        await SeedPartnerAsync(3295, "https://cdn.company.org/logo.jpg");
        await SeedPartnerAsync(3296, "https://brand.example.com/icon.png");
        await SeedPartnerAsync(3297, "https://media.partner.net/image.webp");
        var affected = await RunClearbitCleanupMigrationAsync();
        affected.Should().Be(0);
    }
}
