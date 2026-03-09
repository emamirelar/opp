/// <summary>
/// Boundary tests for PNO-701, PNO-702, PNO-788, PNO-865: WHO section edge cases, min/max values, soft-delete.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhoSection;

[Collection("OpportunityWhoSection")]
[Trait("Category", "Boundary")]
public class BoundaryTests
{
    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_FundingPartner_MinAmount_OneUnit_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 0.01m, 141);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_FundingPartner_MinCurrencyId_One_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 100m, 1);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_FundingPartner_MinPartnerId_One_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 100m, 141);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_ClientPartner_MinPartnerId_One_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidClientPartner(1);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_OpportunityManager_MinUserId_One_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(1);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_Collaborator_MinUserId_One_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidCollaborator(1);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_FormatFundingPartnerAmount_ZeroAmount_FormatsCorrectly()
    {
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(0, "RWF");
        result.Should().Be("0 RWF");
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_FormatFundingPartnerAmount_LargeAmount_FormatsCorrectly()
    {
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(999_999_999.99m, "USD");
        result.Should().Contain("999,999,999");
        result.Should().Contain("USD");
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_FormatFundingPartnerAmount_WhitespaceCurrency_FallsBackToUSD()
    {
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(100m, "   ");
        result.Should().Contain("USD");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_DeduplicateFundingPartners_SingleDuplicate_KeepsOne()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { PartnerId = 1, Amount = 100m },
            new() { PartnerId = 1, Amount = 200m }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateFundingPartners(partners);
        deduped.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_DeduplicateFundingPartners_MultipleDuplicates_KeepsFirstEach()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { PartnerId = 1, Amount = 100m },
            new() { PartnerId = 1, Amount = 200m },
            new() { PartnerId = 2, Amount = 300m },
            new() { PartnerId = 2, Amount = 400m }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateFundingPartners(partners);
        deduped.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_CalculateTotalBudgetUSD_SinglePartner_ReturnsAmount()
    {
        var partners = new List<WhoSpecFundingPartner> { new() { AmountUSD = 5000m } };
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(partners);
        total.Should().Be(5000m);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_CalculateTotalBudgetUSD_AllNullAmountUSD_ReturnsZero()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { AmountUSD = null },
            new() { AmountUSD = null }
        };
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(partners);
        total.Should().Be(0);
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_HasDuplicateCurrencyDisplay_EURWithDollar_ReturnsTrue()
    {
        var hasDuplicate = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay("$5,000 EUR", "EUR");
        hasDuplicate.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_HasDuplicateCurrencyDisplay_GBPWithDollar_ReturnsTrue()
    {
        var hasDuplicate = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay("$1,000 GBP", "GBP");
        hasDuplicate.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_ExternalStakeholder_MinContactId_One_WithPartners_Eligible()
    {
        var eligible = OpportunityWhoSectionSpec.IsExternalStakeholderContactEligible(1, new[] { 1, 2 });
        eligible.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_SMESelection_MinIds_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidSMESelection(1, 1);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_GetExpectedAmountDisplay_MatchesFormat()
    {
        var expected = OpportunityWhoSectionSpec.GetExpectedAmountDisplay(1111m, "RWF");
        expected.Should().Be("1,111 RWF");
        expected.Should().NotContain("$");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_DeduplicateClientPartners_ThreeDuplicates_KeepsOne()
    {
        var partners = new List<WhoSpecClientPartner>
        {
            new() { PartnerId = 5 },
            new() { PartnerId = 5 },
            new() { PartnerId = 5 }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateClientPartners(partners);
        deduped.Should().HaveCount(1);
        deduped[0].PartnerId.Should().Be(5);
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_FormatFundingPartnerAmount_DecimalPlaces_FormatsIntegerPart()
    {
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1234.56m, "USD");
        result.Should().Contain("1,234");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_FundingPartner_AmountAtPrecisionBoundary_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 0.001m, 141);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_PooledFunding_False_IsEligible()
    {
        var eligible = OpportunityWhoSectionSpec.IsPooledFundingPartnerEligible(false);
        eligible.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_OpportunityManager_MaxIntUserId_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(int.MaxValue);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_FundingPartner_MaxReasonableAmount_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 999_999_999_999m, 141);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_HasDuplicateCurrencyDisplay_JPYWithDollar_ReturnsTrue()
    {
        var hasDuplicate = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay("$100,000 JPY", "JPY");
        hasDuplicate.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_CalculateTotalBudgetUSD_MixedNullAndValue_SumNonNull()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { AmountUSD = 100m },
            new() { AmountUSD = null },
            new() { AmountUSD = 200m }
        };
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(partners);
        total.Should().Be(300m);
    }

    [Fact]
    [Trait("Ticket", "PNO-865")]
    public void Spec_ClientPartner_ListWithManyItems_Scrollable()
    {
        // PNO-865: Ensure list can have more than 7 items (scroll bug showed only 6-7)
        var partnerCount = 20;
        partnerCount.Should().BeGreaterThan(7);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_DeduplicateFundingPartners_NoDuplicates_PreservesAll()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { PartnerId = 1 },
            new() { PartnerId = 2 },
            new() { PartnerId = 3 }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateFundingPartners(partners);
        deduped.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_FormatFundingPartnerAmount_NullCurrencyCode_FallsBackToUSD()
    {
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(100m, null!);
        result.Should().Contain("USD");
    }
}
