/// <summary>
/// Boundary tests for PNO-1166: Deliverables model refactor and UI template changes.
/// Covers: Empty strings, null quantity, level fallback order, edge values.
/// </summary>

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DeliverablesProposal;

[Trait("Category", "Boundary")]
[Trait("Feature", "DeliverablesProposal")]
public class DeliverablesProposalBoundaryTests
{
    #region Level Fallback Order (REQ-5)

    [Fact]
    public void BND_001_GetDisplayName_AllLevelsEmpty_FallsBackToOutputName()
    {
        var name = DeliverablesProposalSpec.GetDisplayName("", "", "", "", "", "Output Name");
        name.Should().Be("Output Name");
    }

    [Fact]
    public void BND_002_GetDisplayName_OnlyLevel4Set_ReturnsLevel4()
    {
        var name = DeliverablesProposalSpec.GetDisplayName("L4", null, null, null, null, "Output");
        name.Should().Be("L4");
    }

    [Fact]
    public void BND_003_GetDisplayName_OnlyLevel3Set_ReturnsLevel3()
    {
        var name = DeliverablesProposalSpec.GetDisplayName(null, "L3", null, null, null, "Output");
        name.Should().Be("L3");
    }

    [Fact]
    public void BND_004_GetDisplayName_OnlyLevel2Set_ReturnsLevel2()
    {
        var name = DeliverablesProposalSpec.GetDisplayName(null, null, "L2", null, null, "Output");
        name.Should().Be("L2");
    }

    [Fact]
    public void BND_005_GetDisplayName_OnlyLevel1Set_ReturnsLevel1()
    {
        var name = DeliverablesProposalSpec.GetDisplayName(null, null, null, "L1", null, "Output");
        name.Should().Be("L1");
    }

    [Fact]
    public void BND_006_GetDisplayName_OnlyLevel0Set_ReturnsLevel0()
    {
        var name = DeliverablesProposalSpec.GetDisplayName(null, null, null, null, "L0", "Output");
        name.Should().Be("L0");
    }

    [Fact]
    public void BND_007_GetDisplayName_Level4AndOutputName_Level4Wins()
    {
        var name = DeliverablesProposalSpec.GetDisplayName("L4", null, null, null, null, "Output");
        name.Should().Be("L4");
    }

    [Fact]
    public void BND_008_GetDisplayName_AllLevelsSet_Level4Wins()
    {
        var name = DeliverablesProposalSpec.GetDisplayName("L4", "L3", "L2", "L1", "L0", "Output");
        name.Should().Be("L4");
    }

    [Fact]
    public void BND_009_GetDisplayName_OutputNameEmpty_Level0Used()
    {
        var name = DeliverablesProposalSpec.GetDisplayName(null, null, null, null, "L0", "");
        name.Should().Be("L0");
    }

    [Fact]
    public void BND_010_GetDisplayName_AllNull_ReturnsEmpty()
    {
        var name = DeliverablesProposalSpec.GetDisplayName(null, null, null, null, null, null!);
        name.Should().Be("");
    }

    #endregion

    #region Quantity Nullable (REQ-7)

    [Fact]
    public void BND_011_BackendJObject_QuantityNull_Valid()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj["quantity"]!.Type.Should().Be(JTokenType.Null, "quantity default is null (REQ-7)");
    }

    [Fact]
    public void BND_012_BackendJObject_QuantityExplicitNull_Valid()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(quantity: null);
        obj["quantity"]!.Type.Should().Be(JTokenType.Null);
    }

    [Fact]
    public void BND_013_HtmlTemplate_QuantityNullCheck_Explicit()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.quantity !== null");
        html.Should().Contain("deliverable.quantity !== undefined");
    }

    #endregion

    #region Empty String Boundaries

    [Fact]
    public void BND_014_BackendJObject_Level0EmptyString_Valid()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(level0: "");
        obj["level0"]!.Value<string>().Should().Be("");
    }

    [Fact]
    public void BND_015_BackendJObject_AllLevelsEmpty_Valid()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(level0: "", level1: "", level2: "", level3: "", level4: "");
        obj["level4"]!.Value<string>().Should().Be("");
    }

    [Fact]
    public void BND_016_BackendJObject_ServiceLineEmpty_Valid()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(serviceLine: "");
        obj["serviceLine"]!.Value<string>().Should().Be("");
    }

    [Fact]
    public void BND_017_BackendJObject_OutputNameMinimal_Valid()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputName: "A");
        obj["outputName"]!.Value<string>().Should().Be("A");
    }

    [Fact]
    public void BND_018_BackendJObject_OutputIdZero_Valid()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputId: 0);
        obj["outputId"]!.Value<int>().Should().Be(0);
    }

    #endregion

    #region HTML Template Boundary Conditions

    [Fact]
    public void BND_019_HtmlTemplate_Level3AndLevel4Condition_ShowsLevel3()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.level3 && deliverable.level4");
    }

    [Fact]
    public void BND_020_HtmlTemplate_Level0OrLevel1OrLevel2_Condition()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.level0 || deliverable.level1 || deliverable.level2");
    }

    [Fact]
    public void BND_021_HtmlTemplate_QuantityChip_OnlyWhenDefined()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.quantity.toString()");
    }

    [Fact]
    public void BND_022_HtmlTemplate_ServiceLineChip_Conditional()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("@if (deliverable.serviceLine)");
    }

    [Fact]
    public void BND_023_HtmlTemplate_Level0Chip_Conditional()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("@if (deliverable.level0)");
    }

    [Fact]
    public void BND_024_HtmlTemplate_TrackFallback_OutputIdOrIndex()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverable.outputId || $index");
    }

    #endregion

    #region JObject Property Types

    [Fact]
    public void BND_025_BackendJObject_LevelFields_AreStrings()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(level0: "Cat");
        obj["level0"]!.Type.Should().Be(JTokenType.String);
    }

    [Fact]
    public void BND_026_BackendJObject_DefinitionLevels_AreStrings()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(definitionLevel1: "Def1");
        obj["definitionLevel1"]!.Type.Should().Be(JTokenType.String);
    }

    [Fact]
    public void BND_027_BackendJObject_Quantity_CanBeNumber()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(quantity: 5);
        obj["quantity"]!.Type.Should().Be(JTokenType.Integer);
        obj["quantity"]!.Value<int>().Should().Be(5);
    }

    [Fact]
    public void BND_028_BackendJObject_OutputId_MaxInt_Valid()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputId: int.MaxValue);
        obj["outputId"]!.Value<int>().Should().Be(int.MaxValue);
    }

    [Fact]
    public void BND_029_BackendJObject_LongOutputName_Valid()
    {
        var longName = new string('x', 500);
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputName: longName);
        obj["outputName"]!.Value<string>().Should().HaveLength(500);
    }

    [Fact]
    public void BND_030_HtmlTemplate_DefinitionLevels_NotInDisplayName()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        // definitionLevel1-4 are for definitions, not the main name display
        html.Should().NotContain("deliverable.definitionLevel1 ||");
    }

    #endregion
}
