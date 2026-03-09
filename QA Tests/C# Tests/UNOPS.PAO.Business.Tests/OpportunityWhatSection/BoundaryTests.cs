/// <summary>
/// Boundary tests for PNO-700, PNO-864: Opportunity WHAT - Products &amp; Services section.
/// Covers: Min/max search length, delivery modality boundaries, quantity zero, tree level edges.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

[Trait("Category", "Boundary")]
[Trait("Section", "WhatSection")]
public class OpportunityWhatSectionBoundaryTests
{
    #region PNO-864 — Tree Search Length Boundary

    [Fact]
    public void BND_001_Spec_TreeSearchQuery_ExactlyTwoChars_Valid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("ab").Should().BeTrue();
    }

    [Fact]
    public void BND_002_Spec_TreeSearchQuery_TwoCharsWithSpaces_Valid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("  ab  ").Should().BeTrue();
    }

    [Fact]
    public void BND_003_Spec_TreeSearchQuery_OneChar_Invalid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("a").Should().BeFalse();
    }

    [Fact]
    public void BND_004_Spec_TreeSearchQuery_UnicodeTwoChars_Valid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("工具").Should().BeTrue();
    }

    [Fact]
    public void BND_005_Spec_TreeSearchQuery_VeryLongQuery_Valid()
    {
        var longQuery = new string('x', 1000);
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid(longQuery).Should().BeTrue();
    }

    #endregion

    #region PNO-864 — AI Search Length Boundary

    [Fact]
    public void BND_006_Spec_AiSearchQuery_ExactlyThreeChars_Valid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("abc").Should().BeTrue();
    }

    [Fact]
    public void BND_007_Spec_AiSearchQuery_TwoChars_Invalid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("ab").Should().BeFalse();
    }

    [Fact]
    public void BND_008_Spec_AiSearchQuery_ThreeCharsWithLeadingSpace_Valid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid(" abc").Should().BeTrue();
    }

    [Fact]
    public void BND_009_Spec_AiSearchQuery_PNOMaxPhrase_Valid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("Guidance and tools").Should().BeTrue();
    }

    [Fact]
    public void BND_010_Spec_AiSearchQuery_ExactlyMinLength_Valid()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("xyz").Should().BeTrue();
    }

    #endregion

    #region PNO-700 AC4 — Delivery Modality Boundaries

    [Fact]
    public void BND_011_Spec_DeliveryModality_MinValue1_Valid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 1 };
        spec.IsDeliveryModalityValid().Should().BeTrue();
    }

    [Fact]
    public void BND_012_Spec_DeliveryModality_MaxValue4_Valid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 4 };
        spec.IsDeliveryModalityValid().Should().BeTrue();
    }

    [Fact]
    public void BND_013_Spec_DeliveryModality_Value5_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 5 };
        spec.IsDeliveryModalityValid().Should().BeFalse();
    }

    [Fact]
    public void BND_014_Spec_DeliveryModality_Value0_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 0 };
        spec.IsDeliveryModalityValid().Should().BeFalse();
    }

    #endregion

    #region PNO-864 — Quantity Boundary

    [Fact]
    public void BND_015_Spec_Quantity_Zero_Valid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(0).Should().BeTrue();
    }

    [Fact]
    public void BND_016_Spec_Quantity_One_Valid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(1).Should().BeTrue();
    }

    [Fact]
    public void BND_017_Spec_Quantity_NegativeOne_Invalid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(-1).Should().BeFalse();
    }

    [Fact]
    public void BND_018_Spec_Quantity_LargePositive_Valid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(999999).Should().BeTrue();
    }

    [Fact]
    public void BND_019_Spec_Quantity_IntMaxValue_Valid()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(int.MaxValue).Should().BeTrue();
    }

    #endregion

    #region Output Terminal Validation — Level Boundaries

    [Fact]
    public void BND_020_Spec_IsOutputTerminalAtLevel_Level4_AlwaysTerminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", "L2", "L3", "L4", 4).Should().BeTrue();
    }

    [Fact]
    public void BND_021_Spec_IsOutputTerminalAtLevel_Level0Only_Level0Terminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", null, null, null, null, 0).Should().BeTrue();
    }

    [Fact]
    public void BND_022_Spec_IsOutputTerminalAtLevel_Level1WithNoLevel2_Level1Terminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", null, null, null, 1).Should().BeTrue();
    }

    [Fact]
    public void BND_023_Spec_IsOutputTerminalAtLevel_Level2WithLevel3_Level2NotTerminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", "L2", "L3", null, 2).Should().BeFalse();
    }

    [Fact]
    public void BND_024_Spec_IsOutputTerminalAtLevel_Level3WithLevel4_Level3NotTerminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", "L2", "L3", "L4", 3).Should().BeFalse();
    }

    [Fact]
    public void BND_025_Spec_IsOutputTerminalAtLevel_EmptyStringAtLevel_NotTerminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "", null, null, null, 1).Should().BeFalse();
    }

    #endregion

    #region Deliverables — Collection Boundaries

    [Fact]
    public void BND_026_Spec_Deliverables_SingleItem_NoDuplicates()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        };
        spec.HasDuplicateOutputIds().Should().BeFalse();
    }

    [Fact]
    public void BND_027_Spec_Deliverables_TwoDistinct_NoDuplicates()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = 2 }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeFalse();
    }

    [Fact]
    public void BND_028_Spec_Deliverables_ExactlyTwoDuplicates_Invalid()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 42 },
                new() { OutputId = 42 }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeTrue();
    }

    [Fact]
    public void BND_029_Spec_HasDeliverables_WithOneItem_ReturnsTrue()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        };
        spec.HasDeliverables().Should().BeTrue();
    }

    [Fact]
    public void BND_030_Spec_EmptyDeliverablesList_HasDeliverablesFalse()
    {
        var spec = new OpportunityWhatSectionSpec { Deliverables = new List<OpportunityWhatDeliverableSpec>() };
        spec.HasDeliverables().Should().BeFalse();
    }

    #endregion

    #region Org Unit / Initiative Type Boundaries

    [Fact]
    public void BND_031_Spec_ResponsibleOrgUnitId_Zero_EdgeCase()
    {
        var spec = new OpportunityWhatSectionSpec { ResponsibleOrgUnitId = 0 };
        spec.ResponsibleOrgUnitId.Should().Be(0);
    }

    [Fact]
    public void BND_032_Spec_ProposedInitiativeTypeId_Zero_EdgeCase()
    {
        var spec = new OpportunityWhatSectionSpec { ProposedInitiativeTypeId = 0 };
        spec.ProposedInitiativeTypeId.Should().Be(0);
    }

    [Fact]
    public void BND_033_Spec_OutputId_One_Valid()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        };
        spec.Deliverables[0].OutputId.Should().Be(1);
    }

    [Fact]
    public void BND_034_Spec_OutputId_MaxInt_Valid()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = int.MaxValue } }
        };
        spec.HasDuplicateOutputIds().Should().BeFalse();
    }

    [Fact]
    public void BND_035_Spec_AllFourDeliveryModalities_EachValid()
    {
        for (var i = 1; i <= 4; i++)
        {
            var spec = new OpportunityWhatSectionSpec { DeliveryModality = i };
            spec.IsDeliveryModalityValid().Should().BeTrue($"Modality {i} should be valid");
        }
    }

    [Fact]
    public void BND_036_Spec_TreeSearchQuery_NewlineTrimmed_Valid()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("ab\n").Should().BeTrue();
    }

    #endregion
}
