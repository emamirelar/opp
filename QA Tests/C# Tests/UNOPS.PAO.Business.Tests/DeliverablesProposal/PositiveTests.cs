/// <summary>
/// Positive tests for PNO-1166: Deliverables model refactor and UI template changes.
/// Requirements validated:
/// - REQ-1: Backend BuildDeliverableObject returns all required fields
/// - REQ-2: Frontend ProposedDeliverable interface matches backend response shape
/// - REQ-4: HTML template references the new level fields (level0-level4)
/// - REQ-5: HTML template displays deliverable name as: level4 || level3 || level2 || level1 || level0 || outputName
/// - REQ-6: HTML template uses p-chip for serviceLine and level0 tags
/// - REQ-7: quantity can be null (nullable in both TS and C#)
/// </summary>

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DeliverablesProposal;

[Trait("Category", "Positive")]
[Trait("Feature", "DeliverablesProposal")]
public class DeliverablesProposalPositiveTests
{
    #region REQ-1 — Backend BuildDeliverableObject Response Shape

    [Fact]
    public void POS_001_BackendJObject_ContainsAllRequiredFields()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        foreach (var field in DeliverablesProposalSpec.RequiredBackendFields)
        {
            obj.ContainsKey(field).Should().BeTrue($"Backend response must include '{field}'");
        }
    }

    [Fact]
    public void POS_002_BackendJObject_OutputId_IsInteger()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputId: 42);
        obj["outputId"]!.Type.Should().Be(JTokenType.Integer);
        obj["outputId"]!.Value<int>().Should().Be(42);
    }

    [Fact]
    public void POS_003_BackendJObject_OutputName_IsString()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputName: "Test Output");
        obj["outputName"]!.Type.Should().Be(JTokenType.String);
        obj["outputName"]!.Value<string>().Should().Be("Test Output");
    }

    [Fact]
    public void POS_004_BackendJObject_Quantity_CanBeNull()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj["quantity"]!.Type.Should().Be(JTokenType.Null);
    }

    [Fact]
    public void POS_005_BackendJObject_LevelFields_DefaultToEmptyString()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj["level0"]!.Value<string>().Should().Be("");
        obj["level1"]!.Value<string>().Should().Be("");
        obj["serviceLine"]!.Value<string>().Should().Be("");
    }

    #endregion

    #region REQ-2 — Frontend ProposedDeliverable Interface

    [Fact]
    public void POS_006_TypeScriptModel_ContainsOutputId()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "outputId").Should().BeTrue();
    }

    [Fact]
    public void POS_007_TypeScriptModel_ContainsOutputName()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "outputName").Should().BeTrue();
    }

    [Fact]
    public void POS_008_TypeScriptModel_ContainsLevelFields()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        foreach (var level in new[] { "level0", "level1", "level2", "level3", "level4" })
        {
            DeliverablesProposalSpec.TypeScriptModelContainsField(ts, level).Should().BeTrue($"ProposedDeliverable must have {level}");
        }
    }

    [Fact]
    public void POS_009_TypeScriptModel_ContainsServiceLineAndQuantity()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "serviceLine").Should().BeTrue();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "quantity").Should().BeTrue();
    }

    #endregion

    #region REQ-4, REQ-5, REQ-6 — HTML Template

    [Fact]
    public void POS_010_HtmlTemplate_ReferencesLevelFields()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        foreach (var levelRef in DeliverablesProposalSpec.LevelFieldReferences)
        {
            html.Should().Contain(levelRef, $"Template must reference {levelRef}");
        }
    }

    [Fact]
    public void POS_011_HtmlTemplate_DisplaysNameInCorrectOrder()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.level4 || deliverable.level3 || deliverable.level2 || deliverable.level1 || deliverable.level0 || deliverable.outputName");
    }

    [Fact]
    public void POS_012_HtmlTemplate_UsesPChipForServiceLine()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("p-chip");
        html.Should().Contain("deliverable.serviceLine");
    }

    [Fact]
    public void POS_013_HtmlTemplate_UsesPChipForLevel0()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.level0");
        html.Should().Contain("p-chip");
    }

    [Fact]
    public void POS_014_HtmlTemplate_HasScrollableContainer()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("max-h-[400px]");
        html.Should().Contain("overflow-y-auto");
    }

    [Fact]
    public void POS_015_HtmlTemplate_TracksByOutputIdOrIndex()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("track deliverable.outputId || $index");
    }

    [Fact]
    public void POS_016_HtmlTemplate_HandlesNullableQuantity()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.quantity !== null && deliverable.quantity !== undefined");
    }

    #endregion
}
