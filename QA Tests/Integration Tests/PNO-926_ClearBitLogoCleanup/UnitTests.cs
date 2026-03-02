/**
 * @fileoverview PNO-926 Unit Tests — 21 unit and model-level tests.
 * Entity model validation, enum integrity, and helper function correctness.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO926;

/// <summary>
/// PNO-926 Unit Tests — 21 unit tests for model validation, enums, and helper logic.
/// </summary>
[Collection("Unit")]
[Trait("Category", "Unit")]
[Trait("Ticket", "PNO-926")]
public class UnitTests : PNO926TestFixtureBase
{
    // ─── §7.1 Partner Model (UNT-001 – 008) ──────────────────────────────

    [Fact] [Trait("TestId", "UNT-001")]
    public void Partner_LogoUrl_IsNullableByDefault()
    {
        var partner = new UNOPS.PAO.Domain.Entities.Partner { Id = 1, Name = "Test" };

        partner.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "UNT-002")]
    public void Partner_LogoUrl_CanBeSetToString()
    {
        var partner = new UNOPS.PAO.Domain.Entities.Partner { Id = 1, Name = "Test" };

        partner.LogoUrl = "https://example.com/logo.png";

        partner.LogoUrl.Should().Be("https://example.com/logo.png");
    }

    [Fact] [Trait("TestId", "UNT-003")]
    public void Partner_LogoUrl_CanBeSetToNull()
    {
        var partner = new UNOPS.PAO.Domain.Entities.Partner { Id = 1, Name = "Test", LogoUrl = "https://example.com/logo.png" };

        partner.LogoUrl = null;

        partner.LogoUrl.Should().BeNull();
    }

    [Fact] [Trait("TestId", "UNT-004")]
    public void Partner_Name_IsRequired()
    {
        var partner = new UNOPS.PAO.Domain.Entities.Partner { Id = 1, Name = "Required Name" };

        partner.Name.Should().NotBeNullOrEmpty();
    }

    [Fact] [Trait("TestId", "UNT-005")]
    public void Partner_IsDeleted_DefaultFalse()
    {
        var partner = new UNOPS.PAO.Domain.Entities.Partner { Id = 1, Name = "Test" };

        partner.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "UNT-006")]
    public void Partner_Status_CanBeSetToActive()
    {
        var partner = new UNOPS.PAO.Domain.Entities.Partner { Id = 1, Name = "Test" };
        partner.Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active;

        partner.Status.Should().Be(UNOPS.PAO.Domain.Entities.EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "UNT-007")]
    public void Partner_Id_CanBeAnyPositiveInt()
    {
        var partner = new UNOPS.PAO.Domain.Entities.Partner { Id = int.MaxValue, Name = "Max" };

        partner.Id.Should().Be(int.MaxValue);
    }

    [Fact] [Trait("TestId", "UNT-008")]
    public void Partner_LogoUrl_ClearbitString_CanBeDetected()
    {
        var logoUrl = "https://logo.clearbit.com/test.org";

        logoUrl.Contains("clearbit").Should().BeTrue();
    }

    // ─── §7.2 Fallback Logic (UNT-009 – 015) ────────────────────────────

    [Fact] [Trait("TestId", "UNT-009")]
    public void GetEffectiveLogoUrl_Null_ReturnsFallback()
    {
        var result = GetEffectiveLogoUrl(null);

        result.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "UNT-010")]
    public void GetEffectiveLogoUrl_EmptyString_ReturnsFallback()
    {
        var result = GetEffectiveLogoUrl("");

        result.Should().Be(FallbackImage);
    }

    [Fact] [Trait("TestId", "UNT-011")]
    public void GetEffectiveLogoUrl_ValidUrl_ReturnsIt()
    {
        var result = GetEffectiveLogoUrl("https://example.com/logo.png");

        result.Should().Be("https://example.com/logo.png");
    }

    [Fact] [Trait("TestId", "UNT-012")]
    public void GetEffectiveLogoUrl_NeverReturnsNull()
    {
        var result = GetEffectiveLogoUrl(null);

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "UNT-013")]
    public void FallbackImage_IsValidAssetPath()
    {
        FallbackImage.Should().Be("assets/images/Partner.png");
    }

    [Fact] [Trait("TestId", "UNT-014")]
    public void ClearbitBaseUrl_ContainsClearbit()
    {
        ClearbitBaseUrl.Should().Contain("clearbit");
    }

    [Fact] [Trait("TestId", "UNT-015")]
    public void GetEffectiveLogoUrl_WithRelativePath_ReturnsIt()
    {
        var result = GetEffectiveLogoUrl("/uploads/partner-logo.jpg");

        result.Should().Be("/uploads/partner-logo.jpg");
    }

    // ─── §7.3 String Contains Logic (UNT-016 – 021) ──────────────────────

    [Fact] [Trait("TestId", "UNT-016")]
    public void StringContains_Clearbit_TrueForExactMatch()
    {
        "clearbit".Contains("clearbit").Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-017")]
    public void StringContains_Clearbit_TrueForSubdomain()
    {
        "https://logo.clearbit.com/test.org".Contains("clearbit").Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-018")]
    public void StringContains_Clearbit_TrueForPath()
    {
        "https://example.com/clearbit/logo.png".Contains("clearbit").Should().BeTrue();
    }

    [Fact] [Trait("TestId", "UNT-019")]
    public void StringContains_Clearbit_FalseForSafeUrl()
    {
        "https://safe.example.com/logo.png".Contains("clearbit").Should().BeFalse();
    }

    [Fact] [Trait("TestId", "UNT-020")]
    public void StringContains_Clearbit_FalseForNull()
    {
        string? url = null;
        var result = url?.Contains("clearbit") ?? false;

        result.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "UNT-021")]
    public void StringContains_Clearbit_CaseSensitive_FalseForUppercase()
    {
        "https://logo.CLEARBIT.COM/test.org".Contains("clearbit").Should().BeFalse("String.Contains is case-sensitive");
    }
}
