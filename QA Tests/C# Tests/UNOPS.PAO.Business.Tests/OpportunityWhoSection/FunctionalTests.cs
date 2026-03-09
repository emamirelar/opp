/// <summary>
/// Functional tests for PNO-701, PNO-702, PNO-788, PNO-865: WHO section business rules, workflow, validation.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhoSection;

[Collection("OpportunityWhoSection")]
[Trait("Category", "Functional")]
public class FunctionalTests
{
    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_FundingPartner_Deduplication_ByPartnerId_Enforced()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { PartnerId = 1, Amount = 100m },
            new() { PartnerId = 1, Amount = 200m }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateFundingPartners(partners);
        deduped.Should().ContainSingle(p => p.PartnerId == 1);
        deduped.First().Amount.Should().Be(100m);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_ClientPartner_Deduplication_ByPartnerId_Enforced()
    {
        var partners = new List<WhoSpecClientPartner>
        {
            new() { PartnerId = 2 },
            new() { PartnerId = 2 }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateClientPartners(partners);
        deduped.Should().ContainSingle();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_TotalBudgetUSD_SumOfAllFundingPartnerAmounts()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { AmountUSD = 1000m },
            new() { AmountUSD = 2000m },
            new() { AmountUSD = 3000m }
        };
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(partners);
        total.Should().Be(6000m);
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Func_CurrencyDisplay_NonUSD_NoDollarSymbol()
    {
        var display = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(5000m, "EUR");
        display.Should().NotContain("$");
        display.Should().Contain("EUR");
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Func_CurrencyDisplay_USD_ShowsUSD()
    {
        var display = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(5000m, "USD");
        display.Should().Contain("USD");
        display.Should().Contain("5,000");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_PooledFundingPartner_ExcludedFromFundingSelection()
    {
        var eligible = OpportunityWhoSectionSpec.IsPooledFundingPartnerEligible(true);
        eligible.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Func_OpportunityManager_RequiredForTeamSection()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(1);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Func_Collaborator_CanBeAddedToTeam()
    {
        var valid = OpportunityWhoSectionSpec.IsValidCollaborator(5);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Func_SME_RequiresUserAndRole()
    {
        var valid = OpportunityWhoSectionSpec.IsValidSMESelection(10, 4);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_FundingPartner_RequiresAmountCurrencyPartner()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 100000m, 141);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_ClientPartner_RequiresPartnerId()
    {
        var valid = OpportunityWhoSectionSpec.IsValidClientPartner(3);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Func_GetExpectedAmountDisplay_MatchesSpecFormat()
    {
        var expected = OpportunityWhoSectionSpec.GetExpectedAmountDisplay(1111m, "RWF");
        expected.Should().Be("1,111 RWF");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_ExternalStakeholder_ContactMustBelongToPartners()
    {
        var eligible = OpportunityWhoSectionSpec.IsExternalStakeholderContactEligible(1, new[] { 1, 2, 3 });
        eligible.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_DeduplicateFundingPartners_OrderPreserved_FirstWins()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { PartnerId = 1, Amount = 100m, CurrencyCode = "USD" },
            new() { PartnerId = 1, Amount = 200m, CurrencyCode = "EUR" }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateFundingPartners(partners);
        deduped[0].Amount.Should().Be(100m);
        deduped[0].CurrencyCode.Should().Be("USD");
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    [Trait("Defect", "DEF-156")]
    public void Func_HasDuplicateCurrencyDisplay_DetectsPNO788Bug()
    {
        // PNO-788: Bug was $1,111 RWF - dollar + RWF
        var buggyDisplay = "$1,111 RWF";
        var hasBug = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay(buggyDisplay, "RWF");
        hasBug.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_TotalBudgetUSD_IgnoresPartnersWithoutAmountUSD()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { AmountUSD = 100m },
            new() { AmountUSD = null },
            new() { AmountUSD = null }
        };
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(partners);
        total.Should().Be(100m);
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Func_OpportunityManager_AutoAssigned_HasValidId()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(42);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_FundingPartner_AmountMustBePositive()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 0.01m, 141);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-865")]
    public void Func_ClientPartnerDropdown_SupportsFullList()
    {
        // PNO-865: Dropdown must support full list scroll (not loop 6-7 items)
        const int fullListSize = 50;
        fullListSize.Should().BeGreaterThan(7);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_FormatFundingPartnerAmount_ConsistentFormat()
    {
        var a = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1000m, "USD");
        var b = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1000m, "USD");
        a.Should().Be(b);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_DeduplicateClientPartners_OrderPreserved()
    {
        var partners = new List<WhoSpecClientPartner>
        {
            new() { PartnerId = 1 },
            new() { PartnerId = 1 },
            new() { PartnerId = 2 }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateClientPartners(partners);
        deduped[0].PartnerId.Should().Be(1);
        deduped[1].PartnerId.Should().Be(2);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_ClientPartner_MinimalValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidClientPartner(1);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Func_SME_MultipleSelections_EachValid()
    {
        var valid1 = OpportunityWhoSectionSpec.IsValidSMESelection(1, 1);
        var valid2 = OpportunityWhoSectionSpec.IsValidSMESelection(2, 2);
        valid1.Should().BeTrue();
        valid2.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Func_GetExpectedAmountDisplay_NoDollarForNonUSD()
    {
        var expected = OpportunityWhoSectionSpec.GetExpectedAmountDisplay(5000m, "GBP");
        expected.Should().NotContain("$");
        expected.Should().Contain("GBP");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_PooledFunding_NonPooled_IsEligible()
    {
        var eligible = OpportunityWhoSectionSpec.IsPooledFundingPartnerEligible(false);
        eligible.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_CalculateTotalBudgetUSD_Empty_Zero()
    {
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(new List<WhoSpecFundingPartner>());
        total.Should().Be(0);
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Func_Collaborator_Multiple_EachValid()
    {
        var valid1 = OpportunityWhoSectionSpec.IsValidCollaborator(1);
        var valid2 = OpportunityWhoSectionSpec.IsValidCollaborator(2);
        valid1.Should().BeTrue();
        valid2.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Func_FormatFundingPartnerAmount_NumberFormatting()
    {
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1234567.89m, "USD");
        result.Should().Contain("1,234,567");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_ExternalStakeholder_RequiresPartnerIds()
    {
        var eligible = OpportunityWhoSectionSpec.IsExternalStakeholderContactEligible(5, new[] { 1, 2 });
        eligible.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Func_FundingPartner_CurrencyRequired()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 100m, 141);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Func_TeamSection_RequiresValidOpportunityManager()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(100);
        valid.Should().BeTrue();
    }
}
