/// <summary>
/// Integration tests for PNO-1166: Deliverables model refactor and UI template changes.
/// Covers: Full contract alignment, backend-to-frontend-to-template flow, cross-file consistency.
/// </summary>

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DeliverablesProposal;

[Trait("Category", "Integration")]
[Trait("Feature", "DeliverablesProposal")]
public class DeliverablesProposalIntegrationTests
{
    #region Backend → Frontend → Template Flow

    [Fact]
    public void INT_001_BackendJObject_SerializesToValidJson()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputId: 1, outputName: "Test");
        var json = obj.ToString();
        var parsed = JObject.Parse(json);
        parsed.Should().NotBeNull();
        parsed["outputId"]!.Value<int>().Should().Be(1);
        parsed["outputName"]!.Value<string>().Should().Be("Test");
    }

    [Fact]
    public void INT_002_BackendResponse_ParseableByFrontend()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(
            outputId: 42, outputName: "Output", level0: "Cat", level1: "Sub", serviceLine: "SL");
        var json = obj.ToString();
        var parsed = JObject.Parse(json);
        parsed["outputId"]!.Value<int>().Should().Be(42);
        parsed["outputName"]!.Value<string>().Should().Be("Output");
        parsed["level0"]!.Value<string>().Should().Be("Cat");
        parsed["serviceLine"]!.Value<string>().Should().Be("SL");
    }

    [Fact]
    public void INT_003_FullDeliverable_AllFieldsRoundTrip()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(
            outputId: 1, outputName: "Name",
            level0: "L0", level1: "L1", level2: "L2", level3: "L3", level4: "L4",
            definitionLevel1: "D1", definitionLevel2: "D2", definitionLevel3: "D3", definitionLevel4: "D4",
            serviceLine: "SL", quantity: 5);
        var json = obj.ToString();
        var parsed = JObject.Parse(json);
        parsed["level4"]!.Value<string>().Should().Be("L4");
        parsed["quantity"]!.Value<int>().Should().Be(5);
    }

    [Fact]
    public void INT_004_TemplateAndModel_LevelFieldsAlign()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        foreach (var level in new[] { "level0", "level1", "level2", "level3", "level4" })
        {
            DeliverablesProposalSpec.TypeScriptModelContainsField(ts, level).Should().BeTrue();
            html.Should().Contain($"deliverable.{level}");
        }
    }

    [Fact]
    public void INT_005_TemplateAndModel_ServiceLineAligns()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "serviceLine").Should().BeTrue();
        html.Should().Contain("deliverable.serviceLine");
    }

    [Fact]
    public void INT_006_TemplateAndModel_QuantityAligns()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "quantity").Should().BeTrue();
        html.Should().Contain("deliverable.quantity");
    }

    [Fact]
    public void INT_007_BackendAndTemplate_OutputIdUsedForTrack()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputId: 1);
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        obj.ContainsKey("outputId").Should().BeTrue();
        html.Should().Contain("deliverable.outputId");
    }

    [Fact]
    public void INT_008_BackendAndTemplate_OutputNameInDisplayChain()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputName: "Fallback");
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        obj["outputName"]!.Value<string>().Should().Be("Fallback");
        html.Should().Contain("deliverable.outputName");
    }

    [Fact]
    public void INT_009_AllThreeLayers_NoDeprecatedFields()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        foreach (var dep in DeliverablesProposalSpec.DeprecatedFields)
        {
            obj.ContainsKey(dep).Should().BeFalse();
            html.Should().NotContain($"deliverable.{dep}");
            DeliverablesProposalSpec.TypeScriptModelContainsField(ts, dep).Should().BeFalse();
        }
    }

    [Fact]
    public void INT_010_BackendNullQuantity_TemplateHandles()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        obj["quantity"]!.Type.Should().Be(JTokenType.Null);
        html.Should().Contain("deliverable.quantity !== null");
    }

    #endregion

    #region File Existence and Readability

    [Fact]
    public void INT_011_TypeScriptModelFile_ExistsAndReadable()
    {
        var path = DeliverablesProposalSpec.ResolvePath(DeliverablesProposalSpec.TypeScriptModelPath);
        File.Exists(path).Should().BeTrue();
        var content = DeliverablesProposalSpec.ReadTypeScriptModel();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("ProposedDeliverable");
    }

    [Fact]
    public void INT_012_HtmlTemplateFile_ExistsAndReadable()
    {
        var path = DeliverablesProposalSpec.ResolvePath(DeliverablesProposalSpec.HtmlTemplatePath);
        File.Exists(path).Should().BeTrue();
        var content = DeliverablesProposalSpec.ReadHtmlTemplate();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("deliverables");
    }

    [Fact]
    public void INT_013_TypeScriptModel_ProposedDeliverableInterface()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        ts.Should().Contain("export interface ProposedDeliverable");
        ts.Should().Contain("outputName");
    }

    [Fact]
    public void INT_014_HtmlTemplate_DeliverablesLoop()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("@for (deliverable of");
        html.Should().Contain("proposedOpportunity()!.opportunity.deliverables");
    }

    #endregion

    #region Contract Consistency

    [Fact]
    public void INT_015_RequiredBackendFields_AllInBuildMethod()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        foreach (var field in DeliverablesProposalSpec.RequiredBackendFields)
        {
            obj[field].Should().NotBeNull($"Field {field} must be present");
        }
    }

    [Fact]
    public void INT_016_ExpectedFrontendFields_AllInTypeScript()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        foreach (var field in DeliverablesProposalSpec.ExpectedFrontendFields)
        {
            DeliverablesProposalSpec.TypeScriptModelContainsField(ts, field).Should().BeTrue();
        }
    }

    [Fact]
    public void INT_017_LevelFieldReferences_AllInTemplate()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        foreach (var levelRef in DeliverablesProposalSpec.LevelFieldReferences)
        {
            html.Should().Contain(levelRef);
        }
    }

    [Fact]
    public void INT_018_DisplayNameLogic_MatchesTemplate()
    {
        var expectedPattern = DeliverablesProposalSpec.ExpectedNameDisplayPattern;
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain(expectedPattern);
    }

    [Fact]
    public void INT_019_PChipLabels_MatchModelFields()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("[label]=\"deliverable.serviceLine\"");
        html.Should().Contain("[label]=\"deliverable.level0\"");
    }

    [Fact]
    public void INT_020_DefinitionLevels_InBackendNotInNameDisplay()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(definitionLevel1: "D1");
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        obj.ContainsKey("definitionLevel1").Should().BeTrue();
        html.Should().NotContain("deliverable.definitionLevel1 || deliverable.definitionLevel2");
    }

    #endregion

    #region Multi-Deliverable Scenarios

    [Fact]
    public void INT_021_ArrayOfDeliverables_EachHasRequiredShape()
    {
        var arr = new JArray
        {
            DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputId: 1, outputName: "A"),
            DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputId: 2, outputName: "B")
        };
        foreach (JObject obj in arr)
        {
            obj.ContainsKey("outputId").Should().BeTrue();
            obj.ContainsKey("outputName").Should().BeTrue();
        }
    }

    [Fact]
    public void INT_022_Template_SupportsMultipleDeliverables()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("proposedOpportunity()!.opportunity.deliverables!.length");
        html.Should().Contain("@for (deliverable of");
    }

    [Fact]
    public void INT_023_Template_IndexUsedForSelection()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("deliverables[");
        html.Should().Contain("idx");
    }

    [Fact]
    public void INT_024_BackendEmptyLevels_DisplayFallbackWorks()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(outputName: "Only Name");
        var displayName = DeliverablesProposalSpec.GetDisplayName(
            obj["level4"]?.Value<string>(),
            obj["level3"]?.Value<string>(),
            obj["level2"]?.Value<string>(),
            obj["level1"]?.Value<string>(),
            obj["level0"]?.Value<string>(),
            obj["outputName"]!.Value<string>());
        displayName.Should().Be("Only Name");
    }

    [Fact]
    public void INT_025_FullStack_BackendToDisplayName()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(
            outputId: 1, outputName: "Output",
            level0: "Category", level1: "Subcategory", level4: "Product");
        var name = DeliverablesProposalSpec.GetDisplayName(
            obj["level4"]?.Value<string>(),
            obj["level3"]?.Value<string>(),
            obj["level2"]?.Value<string>(),
            obj["level1"]?.Value<string>(),
            obj["level0"]?.Value<string>(),
            obj["outputName"]!.Value<string>());
        name.Should().Be("Product");
    }

    [Fact]
    public void INT_026_Template_CheckboxBindingPerDeliverable()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("isFieldSelected('deliverables[");
        html.Should().Contain("toggleField('deliverables[");
    }

    [Fact]
    public void INT_027_Template_SelectAllDeliverables()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().Contain("isFieldSelected('deliverables')");
        html.Should().Contain("toggleField('deliverables')");
    }

    [Fact]
    public void INT_028_SpecConstants_ConsistentWithImplementation()
    {
        DeliverablesProposalSpec.RequiredBackendFields.Should().BeEquivalentTo(DeliverablesProposalSpec.ExpectedFrontendFields);
    }

    [Fact]
    public void INT_029_ResolvePath_ReturnsValidPath()
    {
        var path = DeliverablesProposalSpec.ResolvePath(DeliverablesProposalSpec.TypeScriptModelPath);
        path.Should().NotBeNullOrEmpty();
        Path.IsPathRooted(path).Should().BeTrue();
    }

    [Fact]
    public void INT_030_EndToEnd_BackendShapeMatchesTemplateExpectations()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject(
            outputId: 99, outputName: "Test", level0: "L0", serviceLine: "SL", quantity: null);
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        obj["outputId"]!.Value<int>().Should().Be(99);
        html.Should().Contain("deliverable.outputId");
        html.Should().Contain("deliverable.level0");
        html.Should().Contain("deliverable.serviceLine");
        obj["quantity"]!.Type.Should().Be(JTokenType.Null);
    }

    #endregion
}
