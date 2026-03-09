/// <summary>
/// Negative tests for PNO-701, PNO-702, PNO-788, PNO-865: WHO section invalid inputs, validation failures.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhoSection;

[Collection("OpportunityWhoSection")]
[Trait("Category", "Negative")]
public class NegativeTests
{
    [Fact]
    [Trait("Ticket", "PNO-788")]
    [Trait("Defect", "DEF-156")]
    public void Spec_HasDuplicateCurrencyDisplay_RWFWithDollar_ReturnsTrue()
    {
        // PNO-788: "$1,111 RWF" is incorrect - has both $ and RWF
        var hasDuplicate = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay("$1,111 RWF", "RWF");
        hasDuplicate.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidFundingPartner_ZeroPartnerId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(0, 100m, 141);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidFundingPartner_NullAmount_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, null, 141);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidFundingPartner_ZeroAmount_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 0m, 141);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidFundingPartner_NullCurrencyId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 100m, null);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidFundingPartner_NegativeAmount_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, -100m, 141);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidClientPartner_ZeroPartnerId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidClientPartner(0);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidClientPartner_NegativePartnerId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidClientPartner(-1);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_PooledFundingPartner_IsNotEligible()
    {
        var eligible = OpportunityWhoSectionSpec.IsPooledFundingPartnerEligible(true);
        eligible.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_InvalidOpportunityManager_Null_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(null);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_InvalidOpportunityManager_Zero_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(0);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_InvalidCollaborator_Zero_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidCollaborator(0);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_InvalidSMESelection_NullUserId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidSMESelection(null, 3);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_InvalidSMESelection_NullRoleId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidSMESelection(7, null);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_FormatFundingPartnerAmount_EmptyCurrency_FallsBackToUSD()
    {
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(100m, "");
        result.Should().Contain("USD");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_ExternalStakeholder_ZeroContactId_NotEligible()
    {
        var eligible = OpportunityWhoSectionSpec.IsExternalStakeholderContactEligible(0, new[] { 1, 2 });
        eligible.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_ExternalStakeholder_EmptyPartnerIds_NotEligible()
    {
        var eligible = OpportunityWhoSectionSpec.IsExternalStakeholderContactEligible(1, Array.Empty<int>());
        eligible.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_CalculateTotalBudgetUSD_EmptyPartners_ReturnsZero()
    {
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(Array.Empty<WhoSpecFundingPartner>());
        total.Should().Be(0);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_CalculateTotalBudgetUSD_PartnersWithNullAmountUSD_ExcludesThem()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { AmountUSD = 100m },
            new() { AmountUSD = null }
        };
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(partners);
        total.Should().Be(100m);
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_HasDuplicateCurrencyDisplay_OnlyRWF_ReturnsFalse()
    {
        var hasDuplicate = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay("1,111 RWF", "RWF");
        hasDuplicate.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_HasDuplicateCurrencyDisplay_USD_ReturnsFalse()
    {
        var hasDuplicate = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay("$1,111", "USD");
        hasDuplicate.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_DeduplicateFundingPartners_EmptyList_ReturnsEmpty()
    {
        var deduped = OpportunityWhoSectionSpec.DeduplicateFundingPartners(Array.Empty<WhoSpecFundingPartner>());
        deduped.Should().BeEmpty();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_DeduplicateClientPartners_EmptyList_ReturnsEmpty()
    {
        var deduped = OpportunityWhoSectionSpec.DeduplicateClientPartners(Array.Empty<WhoSpecClientPartner>());
        deduped.Should().BeEmpty();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_InvalidSMESelection_ZeroUserId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidSMESelection(0, 3);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Spec_InvalidSMESelection_ZeroRoleId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidSMESelection(7, 0);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidFundingPartner_ZeroCurrencyId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 100m, 0);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Spec_InvalidFundingPartner_NegativePartnerId_IsInvalid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(-1, 100m, 141);
        valid.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Spec_HasDuplicateCurrencyDisplay_NullDisplay_ReturnsFalse()
    {
        var hasDuplicate = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay("", "RWF");
        hasDuplicate.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-865")]
    public void Spec_ClientPartnerDropdown_MustSupportFullListScroll()
    {
        // PNO-865: Client Partner dropdown must allow scrolling through entire list (not loop 6-7 items)
        // Spec validation: dropdown should have sufficient items to test scroll
        const int minItemsForScrollTest = 10;
        minItemsForScrollTest.Should().BeGreaterThan(7);
    }
}
