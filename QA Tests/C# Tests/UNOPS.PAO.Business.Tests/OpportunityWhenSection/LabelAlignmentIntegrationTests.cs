using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Integration tests for PNO-1182: Full template+SCSS+spec contract validation.
/// </summary>
public class PNO1182IntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void FullContract_TemplateAndScssAndSpec_AllRequirementsMet()
    {
        var html = ReadWhenSectionHtml();
        var scss = ReadWhenSectionScss();
        var fieldIds = ExtractDatepickerIdsFromHtml(html);

        html.Should().Contain("p-floatlabel");
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("max-width: calc(100% - 3.5rem)");
        WhenLabelAlignmentSpec.AllFieldsUseSamePattern(fieldIds).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssLabelRules_CompleteSet()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("max-width: calc(100% - 3.5rem)");
        scss.Should().Contain("overflow: hidden");
        scss.Should().Contain("text-overflow: ellipsis");
        scss.Should().Contain("white-space: nowrap");
        scss.Should().Contain("max-width: calc(100% - 3rem)");
        scss.Should().Contain("background-color: white");
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_TemplateIds_MatchExpected()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"targetSigningDate\"");
        html.Should().Contain("id=\"implementationStartDate\"");
        html.Should().Contain("id=\"targetDeliveryDate\"");
        html.Should().Contain("id=\"submissionDeadline\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssAndHtml_ConsistentWithPNO1182Fix()
    {
        var html = ReadWhenSectionHtml();
        var scss = ReadWhenSectionScss();
        html.Should().Contain("p-floatlabel");
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("text-overflow: ellipsis");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssHostNgDeep_Scoped()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(":host");
        scss.Should().Contain("::ng-deep");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssFilledFocus_MaxWidthThreeRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("calc(100% - 3rem)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssDefaultLabel_MaxWidthThreePointFiveRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("calc(100% - 3.5rem)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_AllDatepickers_ShowIconTrue()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[showIcon]=\"true\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_AllFieldIdsInTemplate()
    {
        var html = ReadWhenSectionHtml();
        foreach (var id in WhenLabelAlignmentSpec.ExpectedDateFieldIds)
        {
            html.Should().Contain($"id=\"{id}\"");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_LongLabels_NeedTruncation()
    {
        WhenLabelAlignmentSpec.WouldNeedTruncation("implementationStartDate").Should().BeTrue();
        WhenLabelAlignmentSpec.WouldNeedTruncation("submissionDeadline").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssRequiredDefaultProperties_AllPresent()
    {
        var scss = ReadWhenSectionScss();
        foreach (var prop in WhenLabelAlignmentSpec.RequiredDefaultLabelProperties)
        {
            scss.Should().Contain(prop);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssRequiredFilledProperties_AllPresent()
    {
        var scss = ReadWhenSectionScss();
        foreach (var prop in WhenLabelAlignmentSpec.RequiredFilledLabelProperties)
        {
            scss.Should().Contain(prop);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_HtmlDateFormat_YyMmDd()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("dateFormat=\"yy-mm-dd\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DatepickerIds_InTemplate()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDate");
        html.Should().Contain("implementationStartDate");
        html.Should().Contain("targetDeliveryDate");
        html.Should().Contain("submissionDeadline");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssPDatepickerScope()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(".p-datepicker");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_PNO1182_CompleteContractValidation()
    {
        var html = ReadWhenSectionHtml();
        var scss = ReadWhenSectionScss();
        var fieldIds = ExtractDatepickerIdsFromHtml(html);

        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("max-width: calc(100% - 3.5rem)");
        scss.Should().Contain("text-overflow: ellipsis");
        WhenLabelAlignmentSpec.AllFieldsUseSamePattern(fieldIds).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_AllLabelsHaveLength()
    {
        foreach (var id in WhenLabelAlignmentSpec.ExpectedDateFieldIds)
        {
            WhenLabelAlignmentSpec.GetLabelLength(id).Should().BeGreaterThan(0);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssAndHtml_FilePathsResolvable()
    {
        var htmlPath = ResolveWhenSectionPath("opportunity-when-section.component.html");
        var scssPath = ResolveWhenSectionPath("opportunity-when-section.component.scss");
        File.Exists(htmlPath).Should().BeTrue();
        File.Exists(scssPath).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssPFloatlabelScope()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(".p-floatlabel");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_HtmlLabels_UseTranslatePipe()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("translate");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_DefaultMaxWidth_MatchesScss()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(WhenLabelAlignmentSpec.DefaultLabelMaxWidth);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_FilledMaxWidth_MatchesScss()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(WhenLabelAlignmentSpec.FilledLabelMaxWidth);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_AllFourDatepickers_HaveFloatLabel()
    {
        var html = ReadWhenSectionHtml();
        var floatLabelCount = CountOccurrences(html, "p-floatlabel");
        floatLabelCount.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssDefaultLabel_HasMaxWidthThreePointFiveRem()
    {
        var scss = ReadWhenSectionScss();
        var idx = scss.IndexOf("p-inputwrapper-filled", StringComparison.Ordinal);
        var defaultSection = idx >= 0 ? scss[..idx] : scss;
        defaultSection.Should().Contain("max-width: calc(100% - 3.5rem)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_ImplementationStartDate_25Chars()
    {
        WhenLabelAlignmentSpec.GetLabelLength("implementationStartDate").Should().Be(25);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_ProposalSubmissionDate_24Chars()
    {
        WhenLabelAlignmentSpec.GetLabelLength("submissionDeadline").Should().Be(24);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssTruncation_AllThreeProperties()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("overflow: hidden");
        scss.Should().Contain("text-overflow: ellipsis");
        scss.Should().Contain("white-space: nowrap");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssFilledLabel_BackgroundAndPadding()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("background-color: white");
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_HtmlDatepickers_ClassWFull()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("class=\"w-full\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_ExpectedIds_MatchTemplate()
    {
        var html = ReadWhenSectionHtml();
        var fieldIds = ExtractDatepickerIdsFromHtml(html);
        foreach (var id in WhenLabelAlignmentSpec.ExpectedDateFieldIds)
        {
            fieldIds.Should().Contain(id);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_FullContract_AllSevenRequirements()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("max-width: calc(100% - 3.5rem)");
        scss.Should().Contain("overflow: hidden");
        scss.Should().Contain("text-overflow: ellipsis");
        scss.Should().Contain("white-space: nowrap");
        scss.Should().Contain("max-width: calc(100% - 3rem)");
        scss.Should().Contain("background-color: white");
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_GetLongestLabel_25()
    {
        WhenLabelAlignmentSpec.GetLongestLabelLength().Should().Be(25);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_LabelSpec_GetShortestLabel_19()
    {
        WhenLabelAlignmentSpec.GetShortestLabelLength().Should().Be(19);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssCalc_UsesRemUnits()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("3.5rem");
        scss.Should().Contain("3rem");
    }

    private static string ReadWhenSectionHtml()
    {
        var path = ResolveWhenSectionPath("opportunity-when-section.component.html");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadWhenSectionScss()
    {
        var path = ResolveWhenSectionPath("opportunity-when-section.component.scss");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static IReadOnlyList<string> ExtractDatepickerIdsFromHtml(string html)
    {
        var ids = new List<string>();
        foreach (var id in WhenLabelAlignmentSpec.ExpectedDateFieldIds)
        {
            if (html.Contains($"id=\"{id}\""))
                ids.Add(id);
        }
        return ids;
    }

    private static string ResolveWhenSectionPath(string fileName)
    {
        var relative = Path.Combine("UNOPS.PAO.ClientApp", "src", "app", "features", "partnerships", "opportunities", "components", "opportunity", "view", "sections", "when", fileName);
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

    private static int CountOccurrences(string text, string substring)
    {
        var count = 0;
        var idx = 0;
        while ((idx = text.IndexOf(substring, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += substring.Length;
        }
        return count;
    }
}
