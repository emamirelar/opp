/// <summary>
/// Positive tests for PNO-701, PNO-702, PNO-788, PNO-865: Opportunity WHO section.
/// Requirements validated:
/// - PNO-701 AC1: WHO - Partners & External Stakeholders section exists
/// - PNO-701 AC5: Funding partners with amount, currency, exchange rate
/// - PNO-701: Client partners with name, role
/// - PNO-702 AC1: Team & Internal Stakeholders section
/// - PNO-702 AC4: Opportunity Manager, Collaborators, SMEs
/// - PNO-788: Single currency per funding partner amount (no duplicate $ + RWF)
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhoSection;

[Collection("OpportunityWhoSection")]
[Trait("Category", "Positive")]
public class PositiveTests
{
    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-701")]
    public void Spec_FormatFundingPartnerAmount_NonUSD_ShowsOnlySelectedCurrency()
    {
        // PNO-788: Amount with RWF should show "1,111 RWF" not "$1,111 RWF"
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(1111m, "RWF");
        result.Should().Contain("RWF");
        result.Should().NotContain("$");
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-788")]
    public void Spec_FormatFundingPartnerAmount_USD_ShowsUSD()
    {
        var result = OpportunityWhoSectionSpec.FormatFundingPartnerAmount(50000m, "USD");
        result.Should().Contain("USD");
        result.Should().Contain("50,000");
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-701")]
    public void Spec_ValidFundingPartner_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidFundingPartner(1, 100000m, 141);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-701")]
    public void Spec_ValidClientPartner_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidClientPartner(2);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-701")]
    public void Spec_DeduplicateFundingPartners_KeepsFirst()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { PartnerId = 1, Amount = 100m },
            new() { PartnerId = 1, Amount = 200m },
            new() { PartnerId = 2, Amount = 300m }
        };
        var deduped = OpportunityWhoSectionSpec.DeduplicateFundingPartners(partners);
        deduped.Should().HaveCount(2);
        deduped.First(p => p.PartnerId == 1).Amount.Should().Be(100m);
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-701")]
    public void Spec_NonPooledFundingPartner_IsEligible()
    {
        var eligible = OpportunityWhoSectionSpec.IsPooledFundingPartnerEligible(false);
        eligible.Should().BeTrue();
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-701")]
    public void Spec_CalculateTotalBudgetUSD_SumsCorrectly()
    {
        var partners = new List<WhoSpecFundingPartner>
        {
            new() { AmountUSD = 1000m },
            new() { AmountUSD = 2000m }
        };
        var total = OpportunityWhoSectionSpec.CalculateTotalBudgetUSD(partners);
        total.Should().Be(3000m);
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-702")]
    public void Spec_ValidOpportunityManager_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidOpportunityManager(10);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-702")]
    public void Spec_ValidCollaborator_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidCollaborator(5);
        valid.Should().BeTrue();
    }

    [Fact]
    [Trait("Section", "WhoSection")]
    [Trait("Ticket", "PNO-702")]
    public void Spec_ValidSMESelection_IsValid()
    {
        var valid = OpportunityWhoSectionSpec.IsValidSMESelection(7, 3);
        valid.Should().BeTrue();
    }
}
