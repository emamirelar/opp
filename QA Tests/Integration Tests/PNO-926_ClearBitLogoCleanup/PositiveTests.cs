/**
 * @fileoverview PNO-926 Positive Tests — 30 happy-path tests.
 * Migration execution, non-clearbit preservation, and UI fallback display.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Positive Tests — 30 happy-path tests covering ClearBit URL cleanup migration.
/// </summary>
[Collection("Positive")]
[Trait("Category", "Positive")]
[Trait("Ticket", "PNO-926")]
public class PositiveTests : PNO926TestFixtureBase
{
    // ─── §1.1 Migration Execution (POS-001 – 010) ────────────────────────

    [Fact] [Trait("TestId", "POS-001")]
    public async Task Migration_PartnerWithClearbitUrl_LogoUrlSetToNull()
    {
        await SeedPartnerAsync(1001, "https://logo.clearbit.com/unops.org", "UNOPS");

        var affected = await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(1001);
        partner!.LogoUrl.Should().BeNull();
        affected.Should().Be(1);
    }

    [Fact] [Trait("TestId", "POS-002")]
    public async Task Migration_PartnerWithClearbitMicrosoftUrl_LogoUrlSetToNull()
    {
        await SeedPartnerAsync(1002, "https://logo.clearbit.com/microsoft.com");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1002))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "POS-003")]
    public async Task Migration_PartnerWithClearbitGoogleUrl_LogoUrlSetToNull()
    {
        await SeedPartnerAsync(1003, "https://logo.clearbit.com/google.com");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1003))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "POS-004")]
    public async Task Migration_ClearbitInPath_LogoUrlSetToNull()
    {
        await SeedPartnerAsync(1004, "https://cdn.clearbit.com/images/partner-logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1004))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "POS-005")]
    public async Task Migration_ClearbitInSubdomain_LogoUrlSetToNull()
    {
        await SeedPartnerAsync(1005, "https://clearbit.example.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1005))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "POS-006")]
    public async Task Migration_RunsSuccessfullyWithoutErrors()
    {
        await SeedPartnerAsync(1006, "https://logo.clearbit.com/example.org");

        var act = async () => await RunClearbitCleanupMigrationAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact] [Trait("TestId", "POS-007")]
    public async Task Migration_IsIdempotent_SecondRunHasNoEffect()
    {
        await SeedPartnerAsync(1007, "https://logo.clearbit.com/idempotent.org");

        await RunClearbitCleanupMigrationAsync();
        var secondRunCount = await RunClearbitCleanupMigrationAsync();

        secondRunCount.Should().Be(0, "Second run should find no clearbit URLs");
        (await DbContext.Partners.FindAsync(1007))!.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "POS-008")]
    public async Task Migration_ReturnsCorrectAffectedCount()
    {
        await SeedPartnerAsync(1008, "https://logo.clearbit.com/a.org");
        await SeedPartnerAsync(1009, "https://logo.clearbit.com/b.org");
        await SeedPartnerAsync(1010, "https://example.com/logo.png");

        var affected = await RunClearbitCleanupMigrationAsync();

        affected.Should().Be(2);
    }

    [Fact] [Trait("TestId", "POS-009")]
    public async Task Migration_NonClearbitPartner_LogoUrlUnchanged()
    {
        await SeedPartnerAsync(1011, "https://example.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1011))!.LogoUrl.Should().Be("https://example.com/logo.png");
    }

    [Fact] [Trait("TestId", "POS-010")]
    public async Task Migration_NullLogoUrl_RemainsNull()
    {
        await SeedPartnerAsync(1012, null);

        var affected = await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1012))!.LogoUrl.Should().BeNull();
        affected.Should().Be(0);
    }

    // ─── §1.2 Non-Clearbit Preservation (POS-011 – 020) ──────────────────

    [Fact] [Trait("TestId", "POS-011")]
    public async Task Migration_GcsLogoUrl_Unchanged()
    {
        await SeedPartnerAsync(1013, "https://storage.googleapis.com/bucket/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1013))!.LogoUrl.Should().Contain("storage.googleapis.com");
    }

    [Fact] [Trait("TestId", "POS-012")]
    public async Task Migration_RelativePathLogo_Unchanged()
    {
        await SeedPartnerAsync(1014, "/uploads/partner-logo.jpg");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1014))!.LogoUrl.Should().Be("/uploads/partner-logo.jpg");
    }

    [Fact] [Trait("TestId", "POS-013")]
    public async Task Migration_DataUriLogo_Unchanged()
    {
        const string dataUri = "data:image/png;base64,iVBORw0KGgo=";
        await SeedPartnerAsync(1015, dataUri);

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1015))!.LogoUrl.Should().Be(dataUri);
    }

    [Fact] [Trait("TestId", "POS-014")]
    public async Task Migration_EmptyStringLogo_Unchanged()
    {
        await SeedPartnerAsync(1016, "");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1016))!.LogoUrl.Should().Be("");
    }

    [Fact] [Trait("TestId", "POS-015")]
    public async Task Migration_CdnHostedLogo_Unchanged()
    {
        await SeedPartnerAsync(1017, "https://cdn.example.com/logos/partner.svg");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1017))!.LogoUrl.Should().Contain("cdn.example.com");
    }

    [Fact] [Trait("TestId", "POS-016")]
    public async Task Migration_HttpsNonClearbit_Unchanged()
    {
        await SeedPartnerAsync(1018, "https://partner.org/assets/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1018))!.LogoUrl.Should().Be("https://partner.org/assets/logo.png");
    }

    [Fact] [Trait("TestId", "POS-017")]
    public async Task Migration_S3Url_Unchanged()
    {
        await SeedPartnerAsync(1019, "https://mybucket.s3.amazonaws.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1019))!.LogoUrl.Should().Contain("s3.amazonaws.com");
    }

    [Fact] [Trait("TestId", "POS-018")]
    public async Task Migration_LocalhostUrl_Unchanged()
    {
        await SeedPartnerAsync(1020, "http://localhost:3000/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1020))!.LogoUrl.Should().Contain("localhost");
    }

    [Fact] [Trait("TestId", "POS-019")]
    public async Task Migration_IpAddressUrl_Unchanged()
    {
        await SeedPartnerAsync(1021, "http://192.168.1.1/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1021))!.LogoUrl.Should().Contain("192.168.1.1");
    }

    [Fact] [Trait("TestId", "POS-020")]
    public async Task Migration_FtpUrl_Unchanged()
    {
        await SeedPartnerAsync(1022, "ftp://files.example.com/logo.png");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1022))!.LogoUrl.Should().Contain("ftp://");
    }

    // ─── §1.3 UI Fallback Display (POS-021 – 030) ─────────────────────────

    [Fact] [Trait("TestId", "POS-021")]
    public async Task Fallback_NullLogoUrl_ReturnsFallbackImage()
    {
        await SeedPartnerAsync(1023, null);

        var effective = GetEffectiveLogoUrl(null);

        effective.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "POS-022")]
    public async Task Fallback_EmptyLogoUrl_ReturnsFallbackImage()
    {
        var effective = GetEffectiveLogoUrl("");

        effective.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "POS-023")]
    public async Task Fallback_AfterMigration_PartnerShowsFallbackImage()
    {
        await SeedPartnerAsync(1024, "https://logo.clearbit.com/partner.org");
        await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(1024);
        var effective = GetEffectiveLogoUrl(partner!.LogoUrl);

        effective.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "POS-024")]
    public async Task Fallback_NonNullNonEmptyLogoUrl_ReturnsLogoUrl()
    {
        const string logoUrl = "https://example.com/logo.png";
        var effective = GetEffectiveLogoUrl(logoUrl);

        effective.Should().Be(logoUrl);
    }

    [Fact] [Trait("TestId", "POS-025")]
    public async Task Fallback_FallbackImagePath_IsCorrectAssetPath()
    {
        FallbackImage.Should().Be("assets/images/Partner.png");
    }

    [Fact] [Trait("TestId", "POS-026")]
    public async Task Fallback_NonClearbitPartner_ShowsActualLogo()
    {
        await SeedPartnerAsync(1025, "https://gcs.com/partner-logo.png");
        await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(1025);
        var effective = GetEffectiveLogoUrl(partner!.LogoUrl);

        effective.Should().NotBe(FallbackImage);
        effective.Should().Contain("gcs.com");
    }

    [Fact] [Trait("TestId", "POS-027")]
    public async Task Fallback_PartnerNamePreservedAfterMigration()
    {
        await SeedPartnerAsync(1026, "https://logo.clearbit.com/mypartner.org", "My Partner Org");
        await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(1026);
        partner!.Name.Should().Be("My Partner Org");
    }

    [Fact] [Trait("TestId", "POS-028")]
    public async Task Fallback_PartnerIdPreservedAfterMigration()
    {
        await SeedPartnerAsync(1027, "https://logo.clearbit.com/preserve.org");
        await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(1027);
        partner.Should().NotBeNull();
        partner!.Id.Should().Be(1027);
    }

    [Fact] [Trait("TestId", "POS-029")]
    public async Task Fallback_PartnerStatusPreservedAfterMigration()
    {
        await SeedPartnerAsync(1028, "https://logo.clearbit.com/status-check.org");
        await RunClearbitCleanupMigrationAsync();

        var partner = await DbContext.Partners.FindAsync(1028);
        partner!.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "POS-030")]
    public async Task Fallback_MixedPartners_OnlyClearbitAffected()
    {
        await SeedPartnerAsync(1029, "https://logo.clearbit.com/affected.org", "Affected");
        await SeedPartnerAsync(1030, "https://safe.example.com/logo.png", "Safe");
        await SeedPartnerAsync(1031, null, "No Logo");

        await RunClearbitCleanupMigrationAsync();

        (await DbContext.Partners.FindAsync(1029))!.LogoUrl.Should().BeNull();
        (await DbContext.Partners.FindAsync(1030))!.LogoUrl.Should().Contain("safe.example.com");
        (await DbContext.Partners.FindAsync(1031))!.LogoUrl.Should().BeNull();
    }
}
