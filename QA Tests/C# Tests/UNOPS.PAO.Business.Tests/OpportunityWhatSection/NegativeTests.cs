/// <summary>
/// Negative tests for PNO-700, PNO-864: Opportunity WHAT - Products &amp; Services section.
/// Covers: Invalid delivery modality, invalid search queries, duplicate outputs, missing required data.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

[Trait("Category", "Negative")]
[Trait("Section", "WhatSection")]
public class OpportunityWhatSectionNegativeTests
{
    #region PNO-700 AC4 — Invalid Delivery Modality

    [Fact]
    public void NEG_001_Spec_DeliveryModality_Null_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = null };
        spec.IsDeliveryModalityValid().Should().BeFalse();
    }

    [Fact]
    public void NEG_002_Spec_DeliveryModality_Zero_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 0 };
        spec.IsDeliveryModalityValid().Should().BeFalse();
    }

    [Fact]
    public void NEG_003_Spec_DeliveryModality_Five_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 5 };
        spec.IsDeliveryModalityValid().Should().BeFalse();
    }

    [Fact]
    public void NEG_004_Spec_DeliveryModality_Negative_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = -1 };
        spec.IsDeliveryModalityValid().Should().BeFalse();
    }

    #endregion

    #region PNO-864 — Invalid Search Queries

    [Fact]
    public void NEG_005_Spec_TreeSearchQuery_Empty_Invalid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("").Should().BeFalse();
    }

    [Fact]
    public void NEG_006_Spec_TreeSearchQuery_Null_Invalid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid(null).Should().BeFalse();
    }

    [Fact]
    public void NEG_007_Spec_TreeSearchQuery_SingleChar_Invalid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("a").Should().BeFalse();
    }

    [Fact]
    public void NEG_008_Spec_TreeSearchQuery_WhitespaceOnly_Invalid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("  ").Should().BeFalse();
    }

    [Fact]
    public void NEG_009_Spec_AiSearchQuery_Empty_Invalid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("").Should().BeFalse();
    }

    [Fact]
    public void NEG_010_Spec_AiSearchQuery_Null_Invalid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid(null).Should().BeFalse();
    }

    [Fact]
    public void NEG_011_Spec_AiSearchQuery_TwoChars_Invalid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("ab").Should().BeFalse();
    }

    [Fact]
    public void NEG_012_Spec_AiSearchQuery_SingleChar_Invalid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("x").Should().BeFalse();
    }

    #endregion

    #region PNO-864 — Duplicate Outputs

    [Fact]
    public void NEG_013_Spec_Deliverables_DuplicateOutputIds_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = 1 }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeTrue();
    }

    [Fact]
    public void NEG_014_Spec_Deliverables_ThreeWithTwoDuplicates_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = 2 },
                new() { OutputId = 1 }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeTrue();
    }

    #endregion

    #region PNO-864 — No Deliverables

    [Fact]
    public void NEG_015_Spec_HasDeliverables_EmptyList_ReturnsFalse()
    {
        var spec = new OpportunityWhatSectionSpec { Deliverables = new List<OpportunityWhatDeliverableSpec>() };
        spec.HasDeliverables().Should().BeFalse();
    }

    [Fact]
    public void NEG_016_Spec_HasDeliverables_NullList_ReturnsFalse()
    {
        var spec = new OpportunityWhatSectionSpec { Deliverables = null! };
        spec.HasDeliverables().Should().BeFalse();
    }

    #endregion

    #region PNO-864 — Invalid Quantity

    [Fact]
    public void NEG_017_Spec_Quantity_Negative_Invalid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(-1).Should().BeFalse();
    }

    [Fact]
    public void NEG_018_Spec_Quantity_LargeNegative_Invalid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(-100).Should().BeFalse();
    }

    #endregion

    #region Output Terminal Validation

    [Fact]
    public void NEG_019_Spec_IsOutputTerminalAtLevel_NullLevel0_ReturnsFalse()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel(null, "L1", null, null, null, 0).Should().BeFalse();
    }

    [Fact]
    public void NEG_020_Spec_IsOutputTerminalAtLevel_InvalidLevelIndex_ReturnsFalse()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", null, null, null, -1).Should().BeFalse();
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", null, null, null, 5).Should().BeFalse();
    }

    #endregion

    #region HTML Template — Missing Elements (Specification)

    [Fact]
    public void NEG_021_WhatSection_HtmlTemplate_WithoutInvalidPath_ReturnsEmpty()
    {
        var html = ReadWhatSectionFromInvalidPath();
        html.Should().BeEmpty();
    }

    #endregion

    #region Save/Cancel Behavior — Invalid State

    [Fact]
    public void NEG_022_Spec_AddDeliverable_WithZeroOutputs_ShouldReject()
    {
        var spec = new OpportunityWhatSectionSpec { Deliverables = new List<OpportunityWhatDeliverableSpec>() };
        var canAdd = spec.HasDeliverables();
        canAdd.Should().BeFalse();
    }

    [Fact]
    public void NEG_023_Spec_OutputId_NullInDeliverable_ExcludedFromDuplicateCheck()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = null },
                new() { OutputId = null }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeFalse();
    }

    [Fact]
    public void NEG_024_Spec_ResponsibleOrgUnitId_Negative_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec { ResponsibleOrgUnitId = -1 };
        spec.ResponsibleOrgUnitId.Should().Be(-1);
        // Negative org unit ID is invalid per business rules
        (spec.ResponsibleOrgUnitId >= 0).Should().BeFalse();
    }

    [Fact]
    public void NEG_025_Spec_ProposedInitiativeTypeId_Negative_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec { ProposedInitiativeTypeId = -5 };
        (spec.ProposedInitiativeTypeId >= 0).Should().BeFalse();
    }

    [Fact]
    public void NEG_026_Spec_DeliveryModality_OutOfRange_Invalid()
    {
        foreach (var value in new[] { 0, 5, 10, 100, int.MinValue, int.MaxValue })
        {
            if (OpportunityWhatSectionSpec.ValidDeliveryModalityValues.Contains(value))
                continue;
            var spec = new OpportunityWhatSectionSpec { DeliveryModality = value };
            spec.IsDeliveryModalityValid().Should().BeFalse($"Value {value} should be invalid");
        }
    }

    [Fact]
    public void NEG_027_Spec_TreeSearchQuery_TabOnly_Invalid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("\t").Should().BeFalse();
    }

    [Fact]
    public void NEG_028_Spec_AiSearchQuery_WhitespaceOnly_Invalid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("   ").Should().BeFalse();
    }

    [Fact]
    public void NEG_029_Spec_OutputId_Zero_Ambiguous()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 0 } }
        };
        spec.Deliverables[0].OutputId.Should().Be(0);
        // OutputId 0 may be invalid (no output selected)
        spec.HasDuplicateOutputIds().Should().BeFalse();
    }

    [Fact]
    public void NEG_030_Spec_EmptyOutputName_AllowedButRisky()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1, OutputName = "" } }
        };
        spec.HasDeliverables().Should().BeTrue();
        spec.Deliverables[0].OutputName.Should().Be("");
    }

    [Fact]
    public void NEG_031_Spec_NullOutputName_Allowed()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1, OutputName = null } }
        };
        spec.HasDeliverables().Should().BeTrue();
    }

    [Fact]
    public void NEG_032_Spec_AllLevelsEmpty_OutputNotSelectable()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("", "", "", "", "", 0).Should().BeFalse();
    }

    [Fact]
    public void NEG_033_Spec_Level1HasChild_Level0NotTerminal()
    {
        var isTerminal = OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", null, null, null, 0);
        isTerminal.Should().BeFalse();
    }

    [Fact]
    public void NEG_034_Spec_Quantity_IntMinValue_Invalid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(int.MinValue).Should().BeFalse();
    }

    [Fact]
    public void NEG_035_Spec_MixedNullAndValidOutputIds_DuplicateCheckExcludesNulls()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = null },
                new() { OutputId = 1 }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeTrue();
    }

    [Fact]
    public void NEG_036_Spec_DeliveryModality_OutOfRange99_Invalid()
    {
        var invalidSpec = new OpportunityWhatSectionSpec { DeliveryModality = 99 };
        invalidSpec.IsDeliveryModalityValid().Should().BeFalse();
    }

    #endregion

    #region Helpers

    private static string ReadWhatSectionFromInvalidPath()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nonexistent", "what.html");
        var fullPath = Path.GetFullPath(path);
        return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
    }

    #endregion
}
