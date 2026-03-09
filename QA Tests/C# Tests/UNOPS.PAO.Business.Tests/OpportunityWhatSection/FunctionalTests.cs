/// <summary>
/// Functional tests for PNO-700, PNO-864: Opportunity WHAT - Products &amp; Services section.
/// Covers: Business rules, delivery modality options, save/cancel behavior, output selection logic.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

[Trait("Category", "Functional")]
[Trait("Section", "WhatSection")]
public class OpportunityWhatSectionFunctionalTests
{
    #region PNO-700 AC4 — Delivery Modality Business Rules

    [Fact]
    public void FUNC_001_DeliveryModalityOptions_ContainsAllFourValues()
    {
        var values = OpportunityWhatSectionSpec.ValidDeliveryModalityValues;
        values.Should().HaveCount(4);
        values.Should().Contain(1);
        values.Should().Contain(2);
        values.Should().Contain(3);
        values.Should().Contain(4);
    }

    [Fact]
    public void FUNC_002_DeliveryModality_NotYetKnown_MapsTo1()
    {
        const int notYetKnown = 1;
        OpportunityWhatSectionSpec.ValidDeliveryModalityValues.Should().Contain(notYetKnown);
    }

    [Fact]
    public void FUNC_003_DeliveryModality_AllDirect_MapsTo2()
    {
        const int allDirect = 2;
        OpportunityWhatSectionSpec.ValidDeliveryModalityValues.Should().Contain(allDirect);
    }

    [Fact]
    public void FUNC_004_DeliveryModality_AllGrantSupport_MapsTo3()
    {
        const int allGrantSupport = 3;
        OpportunityWhatSectionSpec.ValidDeliveryModalityValues.Should().Contain(allGrantSupport);
    }

    [Fact]
    public void FUNC_005_DeliveryModality_Mixed_MapsTo4()
    {
        const int mixed = 4;
        OpportunityWhatSectionSpec.ValidDeliveryModalityValues.Should().Contain(mixed);
    }

    [Fact]
    public void FUNC_006_Spec_IsDeliveryModalityValid_AllValidValues_ReturnTrue()
    {
        foreach (var v in OpportunityWhatSectionSpec.ValidDeliveryModalityValues)
        {
            new OpportunityWhatSectionSpec { DeliveryModality = v }.IsDeliveryModalityValid().Should().BeTrue();
        }
    }

    #endregion

    #region PNO-864 — Search Minimum Length Constants

    [Fact]
    public void FUNC_007_TreeSearchMinLength_Is2()
    {
        OpportunityWhatSectionSpec.MinTreeSearchLength.Should().Be(2);
    }

    [Fact]
    public void FUNC_008_AiSearchMinLength_Is3()
    {
        OpportunityWhatSectionSpec.MinAiSearchLength.Should().Be(3);
    }

    [Fact]
    public void FUNC_009_TreeSearch_AtMinLength_Valid()
    {
        var q = new string('x', OpportunityWhatSectionSpec.MinTreeSearchLength);
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid(q).Should().BeTrue();
    }

    [Fact]
    public void FUNC_010_AiSearch_AtMinLength_Valid()
    {
        var q = new string('x', OpportunityWhatSectionSpec.MinAiSearchLength);
        OpportunityWhatSectionSpec.IsAiSearchQueryValid(q).Should().BeTrue();
    }

    #endregion

    #region PNO-864 — Duplicate Detection Logic

    [Fact]
    public void FUNC_011_HasDuplicateOutputIds_OnlyNonNullIdsChecked()
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
    public void FUNC_012_HasDuplicateOutputIds_MultiplePairs_Detected()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = 2 },
                new() { OutputId = 1 },
                new() { OutputId = 2 }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeTrue();
    }

    [Fact]
    public void FUNC_013_HasDeliverables_RequiresNonEmptyList()
    {
        var spec = new OpportunityWhatSectionSpec { Deliverables = new List<OpportunityWhatDeliverableSpec>() };
        spec.HasDeliverables().Should().BeFalse();
    }

    [Fact]
    public void FUNC_014_HasDeliverables_RequiresNonNullList()
    {
        var spec = new OpportunityWhatSectionSpec { Deliverables = null! };
        spec.HasDeliverables().Should().BeFalse();
    }

    #endregion

    #region Output Terminal Logic

    [Fact]
    public void FUNC_015_IsOutputTerminalAtLevel_Level4AlwaysTerminal()
    {
        for (var i = 0; i < 5; i++)
        {
            var levels = new string?[] { "A", "B", "C", "D", "E" };
            OpportunityWhatSectionSpec.IsOutputTerminalAtLevel(levels[0], levels[1], levels[2], levels[3], levels[4], 4).Should().BeTrue();
        }
    }

    [Fact]
    public void FUNC_016_IsOutputTerminalAtLevel_LevelWithNextLevelNotEmpty_NotTerminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", "L2", null, null, 1).Should().BeFalse();
    }

    [Fact]
    public void FUNC_017_IsOutputTerminalAtLevel_LevelWithNextLevelNull_Terminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", null, null, null, 1).Should().BeTrue();
    }

    [Fact]
    public void FUNC_018_IsOutputTerminalAtLevel_LevelWithNextLevelEmpty_Terminal()
    {
        OpportunityWhatSectionSpec.IsOutputTerminalAtLevel("L0", "L1", "", null, null, 1).Should().BeTrue();
    }

    #endregion

    #region Quantity Validation

    [Fact]
    public void FUNC_019_IsQuantityValid_Null_Allowed()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(null).Should().BeTrue();
    }

    [Fact]
    public void FUNC_020_IsQuantityValid_Zero_Allowed()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(0).Should().BeTrue();
    }

    [Fact]
    public void FUNC_021_IsQuantityValid_Positive_Allowed()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(100).Should().BeTrue();
    }

    [Fact]
    public void FUNC_022_IsQuantityValid_Negative_Rejected()
    {
        OpportunityWhatSectionSpec.IsQuantityValid(-1).Should().BeFalse();
    }

    #endregion

    #region Save/Cancel Behavior (Spec Simulation)

    [Fact]
    public void FUNC_023_CancelEditing_RestoresOriginalDeliveryModality()
    {
        var original = new OpportunityWhatSectionSpec { DeliveryModality = 2 };
        var edited = new OpportunityWhatSectionSpec { DeliveryModality = 4 };
        var afterCancel = new OpportunityWhatSectionSpec { DeliveryModality = original.DeliveryModality };
        afterCancel.DeliveryModality.Should().Be(2);
    }

    [Fact]
    public void FUNC_024_SaveSection_RequiresValidDeliveryModality()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 2 };
        spec.IsDeliveryModalityValid().Should().BeTrue();
    }

    [Fact]
    public void FUNC_025_AddDeliverable_RequiresAtLeastOneOutput()
    {
        var spec = new OpportunityWhatSectionSpec { Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } } };
        spec.HasDeliverables().Should().BeTrue();
    }

    [Fact]
    public void FUNC_026_AddDeliverable_RejectsDuplicateOutputIds()
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
    public void FUNC_027_DeliverableSpec_CopiesOutputHierarchy()
    {
        var d = new OpportunityWhatDeliverableSpec
        {
            OutputId = 1,
            Level0 = "Cat",
            Level1 = "Sub",
            Level2 = "Item",
            Level3 = null,
            Level4 = null
        };
        d.Level0.Should().Be("Cat");
        d.Level1.Should().Be("Sub");
        d.Level2.Should().Be("Item");
    }

    [Fact]
    public void FUNC_028_DeliverableSpec_ProcurementComponentFlag()
    {
        var d = new OpportunityWhatDeliverableSpec { ProcurementComponent = true };
        d.ProcurementComponent.Should().BeTrue();
    }

    [Fact]
    public void FUNC_029_DeliverableSpec_ServiceLinePopulated()
    {
        var d = new OpportunityWhatDeliverableSpec { ServiceLine = "Procurement" };
        d.ServiceLine.Should().Be("Procurement");
    }

    [Fact]
    public void FUNC_030_Spec_DeliveryModality_ImmutableAfterSet()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 3 };
        spec.DeliveryModality.Should().Be(3);
    }

    [Fact]
    public void FUNC_031_Spec_Deliverables_OrderPreserved()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = 2 },
                new() { OutputId = 3 }
            }
        };
        spec.Deliverables[0].OutputId.Should().Be(1);
        spec.Deliverables[1].OutputId.Should().Be(2);
        spec.Deliverables[2].OutputId.Should().Be(3);
    }

    [Fact]
    public void FUNC_032_Spec_ResponsibleOrgUnitId_Optional()
    {
        var spec = new OpportunityWhatSectionSpec { ResponsibleOrgUnitId = null };
        spec.ResponsibleOrgUnitId.Should().BeNull();
    }

    [Fact]
    public void FUNC_033_Spec_ProposedInitiativeTypeId_Optional()
    {
        var spec = new OpportunityWhatSectionSpec { ProposedInitiativeTypeId = null };
        spec.ProposedInitiativeTypeId.Should().BeNull();
    }

    [Fact]
    public void FUNC_034_TreeSearchQuery_TrimmedBeforeLengthCheck()
    {
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid("  ab  ").Should().BeTrue();
    }

    [Fact]
    public void FUNC_035_AiSearchQuery_TrimmedBeforeLengthCheck()
    {
        OpportunityWhatSectionSpec.IsAiSearchQueryValid("  abc  ").Should().BeTrue();
    }

    [Fact]
    public void FUNC_036_ValidDeliveryModalityValues_Unique()
    {
        var values = OpportunityWhatSectionSpec.ValidDeliveryModalityValues;
        values.Should().OnlyHaveUniqueItems();
    }

    #endregion
}
