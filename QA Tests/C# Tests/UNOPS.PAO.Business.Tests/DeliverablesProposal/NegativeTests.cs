/// <summary>
/// Negative tests for PNO-1166: Deliverables model refactor and UI template changes.
/// REQ-3: Old deprecated fields (outputDescription, outputGroup, etc.) are NOT in the new interface/template.
/// </summary>

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DeliverablesProposal;

[Trait("Category", "Negative")]
[Trait("Feature", "DeliverablesProposal")]
public class DeliverablesProposalNegativeTests
{
    #region REQ-3 — Deprecated Fields NOT in TypeScript Model

    [Fact]
    public void NEG_001_TypeScriptModel_DoesNotContainOutputDescription()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "outputDescription").Should().BeFalse();
    }

    [Fact]
    public void NEG_002_TypeScriptModel_DoesNotContainOutputGroup()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "outputGroup").Should().BeFalse();
    }

    [Fact]
    public void NEG_003_TypeScriptModel_DoesNotContainOutputSubGroup()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "outputSubGroup").Should().BeFalse();
    }

    [Fact]
    public void NEG_004_TypeScriptModel_DoesNotContainOutputServiceLine()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "outputServiceLine").Should().BeFalse();
    }

    [Fact]
    public void NEG_005_TypeScriptModel_DoesNotContainUnitCode()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "unitCode").Should().BeFalse();
    }

    [Fact]
    public void NEG_006_TypeScriptModel_DoesNotContainProjectCategoryCode()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "projectCategoryCode").Should().BeFalse();
    }

    [Fact]
    public void NEG_007_TypeScriptModel_DoesNotContainNotes()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsField(ts, "notes").Should().BeFalse();
    }

    [Fact]
    public void NEG_008_TypeScriptModel_NoDeprecatedFieldsInProposedDeliverable()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        DeliverablesProposalSpec.TypeScriptModelContainsDeprecatedField(ts).Should().BeFalse();
    }

    #endregion

    #region REQ-3 — Deprecated Fields NOT in HTML Template

    [Fact]
    public void NEG_009_HtmlTemplate_DoesNotReferenceOutputDescription()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("deliverable.outputDescription");
    }

    [Fact]
    public void NEG_010_HtmlTemplate_DoesNotReferenceOutputGroup()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("deliverable.outputGroup");
    }

    [Fact]
    public void NEG_011_HtmlTemplate_DoesNotReferenceOutputSubGroup()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("deliverable.outputSubGroup");
    }

    [Fact]
    public void NEG_012_HtmlTemplate_DoesNotReferenceOutputServiceLine()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("deliverable.outputServiceLine");
    }

    [Fact]
    public void NEG_013_HtmlTemplate_DoesNotReferenceUnitCode()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("deliverable.unitCode");
    }

    [Fact]
    public void NEG_014_HtmlTemplate_DoesNotReferenceProjectCategoryCode()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("deliverable.projectCategoryCode");
    }

    [Fact]
    public void NEG_015_HtmlTemplate_DoesNotReferenceNotes()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("deliverable.notes");
    }

    #endregion

    #region Backend JObject — Must NOT Contain Deprecated Fields

    [Fact]
    public void NEG_016_BackendJObject_DoesNotContainOutputDescription()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj.ContainsKey("outputDescription").Should().BeFalse();
    }

    [Fact]
    public void NEG_017_BackendJObject_DoesNotContainOutputGroup()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj.ContainsKey("outputGroup").Should().BeFalse();
    }

    [Fact]
    public void NEG_018_BackendJObject_DoesNotContainOutputSubGroup()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj.ContainsKey("outputSubGroup").Should().BeFalse();
    }

    [Fact]
    public void NEG_019_BackendJObject_DoesNotContainUnitCode()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj.ContainsKey("unitCode").Should().BeFalse();
    }

    [Fact]
    public void NEG_020_BackendJObject_DoesNotContainProjectCategoryCode()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj.ContainsKey("projectCategoryCode").Should().BeFalse();
    }

    [Fact]
    public void NEG_021_BackendJObject_DoesNotContainNotes()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj.ContainsKey("notes").Should().BeFalse();
    }

    #endregion

    #region Wrong Name Display Order (REQ-5 Violations)

    [Fact]
    public void NEG_022_HtmlTemplate_DoesNotUseOutputGroupForNameDisplay()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("deliverable.outputGroup ||");
    }

    [Fact]
    public void NEG_023_HtmlTemplate_DoesNotUseReversedLevelOrder()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        // Wrong order: level0 || level1 || level2... would be reversed
        html.Should().NotContain("deliverable.level0 || deliverable.level1 || deliverable.level2 || deliverable.level3 || deliverable.level4");
    }

    #endregion

    #region Empty/Invalid Scenarios

    [Fact]
    public void NEG_024_TypeScriptModel_EmptyContent_DoesNotContainField()
    {
        DeliverablesProposalSpec.TypeScriptModelContainsField("", "outputId").Should().BeFalse();
    }

    [Fact]
    public void NEG_025_TypeScriptModel_UnrelatedContent_DoesNotContainDeprecated()
    {
        var unrelated = "export interface Other { id: number; }";
        DeliverablesProposalSpec.TypeScriptModelContainsField(unrelated, "outputGroup").Should().BeFalse();
    }

    [Fact]
    public void NEG_026_BackendJObject_MissingRequiredField_Invalid()
    {
        var obj = new JObject { ["outputId"] = 1, ["outputName"] = "Test" };
        obj.ContainsKey("level0").Should().BeFalse();
    }

    [Fact]
    public void NEG_027_HtmlTemplate_DoesNotUseOldOutputGroupForChip()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("[label]=\"deliverable.outputGroup\"");
    }

    [Fact]
    public void NEG_028_HtmlTemplate_DoesNotUseOldOutputSubGroupForChip()
    {
        var html = DeliverablesProposalSpec.ReadHtmlTemplate();
        html.Should().NotContain("[label]=\"deliverable.outputSubGroup\"");
    }

    [Fact]
    public void NEG_029_BackendJObject_DoesNotUseOutputServiceLineKey()
    {
        var obj = DeliverablesProposalSpec.BuildExpectedDeliverableJObject();
        obj.ContainsKey("outputServiceLine").Should().BeFalse();
    }

    [Fact]
    public void NEG_030_TypeScriptModel_DefinitionLevelsNotDeprecated()
    {
        var ts = DeliverablesProposalSpec.ReadTypeScriptModel();
        // definitionLevel1-4 are NEW fields, not in deprecated list
        DeliverablesProposalSpec.DeprecatedFields.Should().NotContain("definitionLevel1");
    }

    #endregion
}
