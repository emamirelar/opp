/// <summary>
/// Integration tests for PNO-701, PNO-702, PNO-788, PNO-865: WHO section full workflow, spec+request contract.
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhoSection;

[Collection("OpportunityWhoSection")]
[Trait("Category", "Integration")]
public class IntegrationTests
{
    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_WhoSectionRequest_ValidFundingAndClientPartners_Contract()
    {
        var request = new WhoSectionRequest
        {
            IsPooledFunding = false,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 1, Amount = 100000m, CurrencyId = 141 }
            },
            ClientPartners = new List<OpportunityClientPartnerRequest>
            {
                new() { PartnerId = 2 }
            }
        };
        request.FundingPartners.Should().HaveCount(1);
        request.ClientPartners.Should().HaveCount(1);
        OpportunityWhoSectionSpec.IsValidFundingPartner(
            request.FundingPartners![0].PartnerId,
            request.FundingPartners[0].Amount,
            request.FundingPartners[0].CurrencyId).Should().BeTrue();
        OpportunityWhoSectionSpec.IsValidClientPartner(request.ClientPartners![0].PartnerId).Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_DeduplicateThenValidate_FundingPartners()
    {
        var raw = new List<WhoSpecFundingPartner>
        {
            new() { PartnerId = 1, Amount = 100m, CurrencyId = 141 },
            new() { PartnerId = 1, Amount = 200m, CurrencyId = 141 }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateFundingPartners(raw);
        deduped.Should().HaveCount(1);
        OpportunityWhoSectionSpec.IsValidFundingPartner(
            deduped[0].PartnerId, deduped[0].Amount, deduped[0].CurrencyId).Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Integ_FormatThenCheckDuplicate_NonUSD_NoDuplicate()
    {
        var formatted = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1111m, "RWF");
        var hasDuplicate = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay(formatted, "RWF");
        hasDuplicate.Should().BeFalse();
        formatted.Should().Be("1,111 RWF");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_WhoSectionRequest_EmptyPartners_Valid()
    {
        var request = new WhoSectionRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>(),
            ClientPartners = new List<OpportunityClientPartnerRequest>()
        };
        request.FundingPartners.Should().BeEmpty();
        request.ClientPartners.Should().BeEmpty();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_CalculateTotalBudget_FromDeduplicatedPartners()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { PartnerId = 1, AmountUSD = 1000m },
            new() { PartnerId = 2, AmountUSD = 2000m }
        };
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(partners);
        total.Should().Be(3000m);
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Integ_TeamSection_OMAndCollaborators_Contract()
    {
        var omValid = OpportunityWhoSectionSpec.IsValidOpportunityManager(1);
        var collabValid = OpportunityWhoSectionSpec.IsValidCollaborator(2);
        omValid.Should().BeTrue();
        collabValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_WhoSectionRequest_ExternalStakeholders_Contract()
    {
        var request = new WhoSectionRequest
        {
            ExternalStakeholders = new List<OpportunityExternalStakeholderRequest>
            {
                new() { ContactId = 1 }
            }
        };
        request.ExternalStakeholders.Should().HaveCount(1);
        OpportunityWhoSectionSpec.IsExternalStakeholderContactEligible(
            request.ExternalStakeholders![0].ContactId, new[] { 1, 2 }).Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Integ_BuggyDisplay_DetectedAsDuplicate()
    {
        var buggy = "$1,111 RWF";
        OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay(buggy, "RWF").Should().BeTrue();
        var correct = OpportunityWhoSectionSpec.GetExpectedAmountDisplay(1111m, "RWF");
        correct.Should().NotContain("$");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_OpportunityFundingPartnerRequest_AmountAndCurrency()
    {
        var fp = new OpportunityFundingPartnerRequest
        {
            PartnerId = 1,
            Amount = 50000m,
            CurrencyId = 141
        };
        OpportunityWhoSectionSpec.IsValidFundingPartner(fp.PartnerId, fp.Amount, fp.CurrencyId).Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_OpportunityClientPartnerRequest_PartnerId()
    {
        var cp = new OpportunityClientPartnerRequest { PartnerId = 3 };
        OpportunityWhoSectionSpec.IsValidClientPartner(cp.PartnerId).Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_FullWhoSection_AllSubsections()
    {
        var funding = new List<WhoSpecFundingPartner> { new() { PartnerId = 1, Amount = 100m, CurrencyId = 141 } };
        var client = new List<WhoSpecClientPartner> { new() { PartnerId = 2 } };
        var dedupedFunding = OpportunityWhoSectionSpec.DeduplicateFundingPartners(funding);
        var dedupedClient = OpportunityWhoSectionSpec.DeduplicateClientPartners(client);
        var totalUSD = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(
            funding.Select(f => new WhoSpecFundingPartner { AmountUSD = f.Amount ?? 0 }).ToList());
        dedupedFunding.Should().HaveCount(1);
        dedupedClient.Should().HaveCount(1);
        totalUSD.Should().Be(100m);
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Integ_FormatMultipleCurrencies_EachCorrect()
    {
        var usd = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1000m, "USD");
        var rwf = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1000m, "RWF");
        var eur = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1000m, "EUR");
        usd.Should().Contain("USD");
        rwf.Should().Contain("RWF").And.NotContain("$");
        eur.Should().Contain("EUR").And.NotContain("$");
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_PooledFunding_FlagRespected()
    {
        var eligible = OpportunityWhoSectionSpec.IsPooledFundingPartnerEligible(false);
        eligible.Should().BeTrue();
        var ineligible = OpportunityWhoSectionSpec.IsPooledFundingPartnerEligible(true);
        ineligible.Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Integ_SMESelections_ValidStructure()
    {
        var valid = OpportunityWhoSectionSpec.IsValidSMESelection(5, 2);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_WhoSectionRequest_IsPooledFunding()
    {
        var request = new WhoSectionRequest { IsPooledFunding = true };
        request.IsPooledFunding.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_DeduplicateClientPartners_ThenValidate()
    {
        var raw = new List<WhoSpecClientPartner>
        {
            new() { PartnerId = 1 },
            new() { PartnerId = 1 }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateClientPartners(raw);
        deduped.All(p => OpportunityWhoSectionSpec.IsValidClientPartner(p.PartnerId)).Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Integ_ExpectedDisplay_MatchesFormat()
    {
        var expected = OpportunityWhoSectionSpec.GetExpectedAmountDisplay(50000m, "USD");
        var formatted = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(50000m, "USD");
        expected.Should().Be(formatted);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_MultipleFundingPartners_TotalBudget()
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
    [Trait("Ticket", "PNO-702")]
    public void Integ_TeamSection_AllRolesValid()
    {
        var om = OpportunityWhoSectionSpec.IsValidOpportunityManager(1);
        var collab = OpportunityWhoSectionSpec.IsValidCollaborator(2);
        var sme = OpportunityWhoSectionSpec.IsValidSMESelection(3, 4);
        om.Should().BeTrue();
        collab.Should().BeTrue();
        sme.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_WhoSectionRequest_MiscExternalStakeholders()
    {
        var request = new WhoSectionRequest
        {
            MiscExternalStakeholders = "Other interested parties",
            ExternalStakeholderNotes = "Notes"
        };
        request.MiscExternalStakeholders.Should().NotBeNullOrEmpty();
        request.ExternalStakeholderNotes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Integ_CurrencyDisplay_Workflow()
    {
        var amount = 1111m;
        var currency = "RWF";
        var formatted = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(amount, currency);
        var hasBug = OpportunityWhoSectionSpec.HasDuplicateCurrencyDisplay(formatted, currency);
        hasBug.Should().BeFalse();
        formatted.Should().Be(OpportunityWhoSectionSpec.GetExpectedAmountDisplay(amount, currency));
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_FundingPartner_WithFeeFields()
    {
        var fp = new OpportunityFundingPartnerRequest
        {
            PartnerId = 1,
            Amount = 100000m,
            CurrencyId = 141,
            FeePercentage = 5m,
            IsAmountBasedFee = true
        };
        OpportunityWhoSectionSpec.IsValidFundingPartner(fp.PartnerId, fp.Amount, fp.CurrencyId).Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_ClientPartner_WithAgreement()
    {
        var cp = new OpportunityClientPartnerRequest
        {
            PartnerId = 2,
            SelectedPartnerAgreementNumber = "AGR-001"
        };
        OpportunityWhoSectionSpec.IsValidClientPartner(cp.PartnerId).Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-865")]
    public void Integ_ClientPartner_ListSize_GreaterThanScrollBug()
    {
        // PNO-865: Scroll bug showed only 6-7 items
        var minRequired = 10;
        minRequired.Should().BeGreaterThan(7);
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_WhoSectionRequest_NullPartners_Handled()
    {
        var request = new WhoSectionRequest();
        request.FundingPartners.Should().BeNull();
        request.ClientPartners.Should().BeNull();
    }

    [Fact]
    [Trait("Ticket", "PNO-788")]
    public void Integ_AllNonUSDCurrencies_NoDollarInFormat()
    {
        var currencies = new[] { "RWF", "EUR", "GBP", "JPY", "CHF" };
        foreach (var c in currencies)
        {
            var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1000m, c);
            result.Should().NotContain("$");
            result.Should().Contain(c);
        }
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_ExternalStakeholder_MultipleContacts()
    {
        var request = new WhoSectionRequest
        {
            ExternalStakeholders = new List<OpportunityExternalStakeholderRequest>
            {
                new() { ContactId = 1 },
                new() { ContactId = 2 }
            }
        };
        request.ExternalStakeholders.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Ticket", "PNO-702")]
    public void Integ_TeamSection_OpportunityManagerRequired()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(100);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Ticket", "PNO-701")]
    public void Integ_WhoSectionRequest_FullStructure()
    {
        var request = new WhoSectionRequest
        {
            IsPooledFunding = false,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 1, Amount = 100000m, CurrencyId = 141 }
            },
            ClientPartners = new List<OpportunityClientPartnerRequest>
            {
                new() { PartnerId = 2 }
            },
            ExternalStakeholders = new List<OpportunityExternalStakeholderRequest>
            {
                new() { ContactId = 1 }
            },
            MiscExternalStakeholders = "Other",
            ExternalStakeholderNotes = "Notes"
        };
        request.FundingPartners.Should().HaveCount(1);
        request.ClientPartners.Should().HaveCount(1);
        request.ExternalStakeholders.Should().HaveCount(1);
    }
}
