/// <summary>
/// Positive tests for PNO-700, PNO-864: Opportunity WHAT - Products &amp; Services section.
/// Requirements validated:
/// - PNO-700 AC1: WHAT section exists on opportunity record
/// - PNO-700 AC4: Delivery modality options (NotYetKnown, AllDirect, AllGrantSupport, Mixed)
/// - PNO-864: Manual search, AI search, multi-select outputs, save/cancel
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

[Collection("OpportunityWhatSection")]
[Trait("Category", "Positive")]
[Trait("Section", "WhatSection")]
public class OpportunityWhatSectionPositiveTests
{
    #region PNO-700 AC1 — WHAT Section Exists

    [Fact]
    public void POS_001_WhatSection_HtmlTemplate_ContainsProductsServicesLabel()
    {
        var html = ReadWhatSectionHtml();
        html.Should().Contain("whatProductsServices");
    }

    [Fact]
    public void POS_002_WhatSection_HtmlTemplate_ContainsDeliverablesPanel()
    {
        var html = ReadWhatSectionHtml();
        html.Should().Contain("field-deliverables");
    }

    [Fact]
    public void POS_003_WhatSection_HtmlTemplate_ContainsDeliveryModalitySection()
    {
        var html = ReadWhatSectionHtml();
        html.Should().Contain("deliveryModality");
    }

    #endregion

    #region PNO-700 AC4 — Delivery Modality

    [Fact]
    public void POS_004_Spec_AllDeliveryModalityValues_Valid()
    {
        foreach (var value in OpportunityWhatSectionSpec.ValidDeliveryModalityValues)
        {
            var spec = new OpportunityWhatSectionSpec { DeliveryModality = value };
            spec.IsDeliveryModalityValid().Should().BeTrue($"Delivery modality {value} should be valid");
        }
    }

    [Fact]
    public void POS_005_Spec_DeliveryModalityNotYetKnown_Valid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 1 };
        spec.IsDeliveryModalityValid().Should().BeTrue();
    }

    [Fact]
    public void POS_006_Spec_DeliveryModalityAllDirect_Valid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 2 };
        spec.IsDeliveryModalityValid().Should().BeTrue();
    }

    #endregion

    #region PNO-864 — Search & Multi-Select

    [Fact]
    public void POS_007_Spec_TreeSearchQuery_WithValidLength_Valid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("ab").Should().BeTrue();
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("search").Should().BeTrue();
    }

    [Fact]
    public void POS_008_Spec_AiSearchQuery_WithValidLength_Valid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("abc").Should().BeTrue();
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("Guidance and tools").Should().BeTrue();
    }

    [Fact]
    public void POS_009_Spec_Deliverables_NoDuplicates_Valid()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1, OutputName = "Output A" },
                new() { OutputId = 2, OutputName = "Output B" }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeFalse();
    }

    [Fact]
    public void POS_010_Spec_HasDeliverables_WithItems_ReturnsTrue()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        };
        spec.HasDeliverables().Should().BeTrue();
    }

    [Fact]
    public void POS_011_Spec_Quantity_NonNegative_Valid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(0).Should().BeTrue();
        OpportunityWhatSectionSpec.IsQuantityValid(10).Should().BeTrue();
        OpportunityWhatSectionSpec.IsQuantityValid(null).Should().BeTrue();
    }

    [Fact]
    public void POS_012_WhatSection_HtmlTemplate_ContainsAddNewButton()
    {
        var html = ReadWhatSectionHtml();
        html.Should().Contain("button.addNew");
    }

    #endregion

    #region Helpers

    private static string ReadWhatSectionHtml()
    {
        var path = ResolveWhatSectionPath("opportunity-what-section.component.html");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ResolveWhatSectionPath(string fileName)
    {
        var relative = Path.Combine("UNOPS.PAO.ClientApp", "src", "app", "features", "partnerships", "opportunities", "components", "opportunity", "view", "sections", "what", fileName);
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", relative),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", relative),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", relative),
            Path.Combine(Directory.GetCurrentDirectory(), relative),
        };
        foreach (var p in candidates)
        {
            var full = Path.GetFullPath(p);
            if (File.Exists(full))
                return full;
        }
        return Path.Combine(baseDir, fileName);
    }

    #endregion
}
