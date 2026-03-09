/// <summary>
/// Functional tests for PNO-1166: Deliverables model refactor and UI template changes.
/// Covers: Business rules, field mappings, API contract alignment, display logic.
/// </summary>

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DeliverablesProposal;

[Trait("Category", "Functional")]
[Trait("Feature", "DeliverablesProposal")]
public class DeliverablesProposalFunctionalTests
{
    #region REQ-1 — Backend Contract Completeness

    [Fact]
    public void FUNC_001_RequiredBackendFields_CountIs13()
    {
        DeliverablesProposalSpec.RequiredBackendFields.Should().HaveCount(13);
    }

    [Fact]
    public void FUNC_002_RequiredBackendFields_ContainsOutputId()
    {
        DeliverablesProposalSpec.RequiredBackendFields.Should().Contain("outputId");
    }

    [Fact]
    public void FUNC_003_RequiredBackendFields_ContainsAllLevels()
    {
        foreach (var level in new[] { "level0", "level1", "level2", "level3", "level4" })
        {
            DeliverablesProposalSpec.RequiredBackendFields.Should().Contain(level);
        }
    }

    [Fact]
    public void FUNC_004_RequiredBackendFields_ContainsAllDefinitionLevels()
    {
        foreach (var def in new[] { "definitionLevel1", "definitionLevel2", "definitionLevel3", "definitionLevel4" })
        {
            DeliverablesProposalSpec.RequiredBackendFields.Should().Contain(def);
        }
    }

    [Fact]
    public void FUNC_005_RequiredBackendFields_ContainsServiceLineAndQuantity()
    {
        DeliverablesProposalSpec.RequiredBackendFields.Should().Contain("serviceLine");
        DeliverablesProposalSpec.RequiredBackendFields.Should().Contain("quantity");
    }

    [Fact]
    public void FUNC_006_BuildExpectedDeliverableJObject_MatchesBackendContract()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        foreach (var field in DeliverablesProposalSpec.RequiredBackendFields)
        {
            obj.Should().ContainKey(field);
        }
    }

    [Fact]
    public void FUNC_007_BackendJObject_NoExtraFields()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj.Count.Should().Be(13);
    }

    #endregion

    #region REQ-2 — Frontend-Backend Alignment

    [Fact]
    public void FUNC_008_ExpectedFrontendFields_MatchRequiredBackendFields()
    {
        var frontend = DeliverablesProposalSpec.ExpectedFrontendFields.ToHashSet();
        var backend = DeliverablesProposalSpec.RequiredBackendFields.ToHashSet();
        frontend.SetEquals(backend).Should().BeTrue();
    }

    [Fact]
    public void FUNC_009_TypeScriptModel_HasAllExpectedFields()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        foreach (var field in DeliverablesProposalSpec.ExpectedFrontendFields)
        {
            DeliverablesProposalSpec.TypeScriptModelContainsField(ts, field).Should().BeTrue($"ProposedDeliverable must have {field}");
        }
    }

    [Fact]
    public void FUNC_010_BackendToFrontend_FieldNamesMatch()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        foreach (var field in DeliverablesProposalSpec.ExpectedFrontendFields)
        {
            obj.ContainsKey(field).Should().BeTrue($"Backend and frontend must both have {field}");
        }
    }

    #endregion

    #region REQ-5 — Display Name Logic

    [Fact]
    public void FUNC_011_GetDisplayName_OrderIsLevel4ToOutputName()
    {
        var order = new[] { "L4", "L3", "L2", "L1", "L0", "Out" };
        var name = DeliverablesProposalSpec.GetDisplayName(order[0], order[1], order[2], order[3], order[4], order[5]);
        name.Should().Be("L4");
    }

    [Fact]
    public void FUNC_012_GetDisplayName_FirstNonEmptyWins()
    {
        var name = DeliverablesProposalSpec.GetDisplayName(null, null, "L2", "L1", "L0", "Output");
        name.Should().Be("L2");
    }

    [Fact]
    public void FUNC_013_GetDisplayName_OutputNameIsLastFallback()
    {
        var name = DeliverablesProposalSpec.GetDisplayName(null, null, null, null, null, "Last Resort");
        name.Should().Be("Last Resort");
    }

    [Fact]
    public void FUNC_014_HtmlTemplate_NameDisplayPattern_MatchesSpec()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain(DeliverablesProposalSpec.ExpectedNameDisplayPattern);
    }

    [Fact]
    public void FUNC_015_ExpectedNameDisplayPattern_IsCorrect()
    {
        DeliverablesProposalSpec.ExpectedNameDisplayPattern.Should().Contain("level4");
        DeliverablesProposalSpec.ExpectedNameDisplayPattern.Should().Contain("outputName");
    }

    #endregion

    #region REQ-6 — p-chip Usage

    [Fact]
    public void FUNC_016_HtmlTemplate_PChipUsedForServiceLine()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("p-chip");
        html.Should().Contain("[label]=\"deliverable.serviceLine\"");
    }

    [Fact]
    public void FUNC_017_HtmlTemplate_PChipUsedForLevel0()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("[label]=\"deliverable.level0\"");
    }

    [Fact]
    public void FUNC_018_HtmlTemplate_PChipUsedForQuantity()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("'Qty: ' + deliverable.quantity.toString()");
    }

    [Fact]
    public void FUNC_019_HtmlTemplate_ServiceLineBeforeLevel0InChips()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        // Use chip-specific labels (REQ-6) - level0 appears earlier in name display pattern
        var serviceLineChipPos = html.IndexOf("[label]=\"deliverable.serviceLine\"");
        var level0ChipPos = html.IndexOf("[label]=\"deliverable.level0\"");
        serviceLineChipPos.Should().BeGreaterThan(-1, "serviceLine chip should exist");
        level0ChipPos.Should().BeGreaterThan(-1, "level0 chip should exist");
        serviceLineChipPos.Should().BeLessThan(level0ChipPos, "serviceLine chip should appear before level0 chip");
    }

    #endregion

    #region REQ-7 — Quantity Nullable

    [Fact]
    public void FUNC_020_BackendJObject_QuantityDefaultIsNull()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj["quantity"]!.Type.Should().Be(JTokenType.Null);
    }

    [Fact]
    public void FUNC_021_HtmlTemplate_QuantityConditionalDisplay()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.quantity !== null");
        html.Should().Contain("deliverable.quantity !== undefined");
    }

    [Fact]
    public void FUNC_022_BackendJObject_QuantityCanBeNumber()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(quantity: 10);
        obj["quantity"]!.Value<int>().Should().Be(10);
    }

    #endregion

    #region Deprecated Fields (REQ-3)

    [Fact]
    public void FUNC_023_DeprecatedFields_CountIs7()
    {
        DeliverablesProposalSpec.DeprecatedFields.Should().HaveCount(7);
    }

    [Fact]
    public void FUNC_024_DeprecatedFields_ContainsAllOldFields()
    {
        var expected = new[] { "outputDescription", "outputGroup", "outputSubGroup", "outputServiceLine", "unitCode", "projectCategoryCode", "notes" };
        DeliverablesProposalSpec.DeprecatedFields.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void FUNC_025_DeprecatedFields_NotInRequiredBackend()
    {
        var required = DeliverablesProposalSpec.RequiredBackendFields.ToHashSet();
        foreach (var dep in DeliverablesProposalSpec.DeprecatedFields)
        {
            required.Should().NotContain(dep);
        }
    }

    [Fact]
    public void FUNC_026_DeprecatedFields_NotInExpectedFrontend()
    {
        var expected = DeliverablesProposalSpec.ExpectedFrontendFields.ToHashSet();
        foreach (var dep in DeliverablesProposalSpec.DeprecatedFields)
        {
            expected.Should().NotContain(dep);
        }
    }

    #endregion

    #region Template Structure

    [Fact]
    public void FUNC_027_HtmlTemplate_DeliverablesSectionExists()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverables");
        html.Should().Contain("label.opportunity.deliverables");
    }

    [Fact]
    public void FUNC_028_HtmlTemplate_HierarchicalPathSection()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("Level 0:");
        html.Should().Contain("Level 1:");
        html.Should().Contain("Level 2:");
    }

    [Fact]
    public void FUNC_029_HtmlTemplate_LevelFieldReferences_AllPresent()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        foreach (var levelRef in DeliverablesProposalSpec.LevelFieldReferences)
        {
            html.Should().Contain(levelRef);
        }
    }

    [Fact]
    public void FUNC_030_HtmlTemplate_ScrollableContainer_MaxHeight400()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("max-h-[400px]");
    }

    #endregion
}
