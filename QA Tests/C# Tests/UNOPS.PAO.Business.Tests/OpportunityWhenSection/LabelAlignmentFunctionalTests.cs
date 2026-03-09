using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Functional tests for PNO-1182: Business rules, consistency, alignment requirements.
/// </summary>
public class PNO1182FunctionalTests
{
    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_LabelOverflowPrevention_Enforced()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("max-width");
        scss.Should().Contain("text-overflow: ellipsis");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_DefaultAndFilledStates_HaveDistinctMaxWidth()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("3.5rem");
        scss.Should().Contain("3rem");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_WhiteSpaceNowrap_PreventsLabelWrap()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("white-space: nowrap");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_OverflowHidden_ClipsLongLabels()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("overflow: hidden");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_BackgroundColorWhite_ForFilledLabel()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("background-color: white");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_PaddingForFilledLabel()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_DatepickersUseFloatLabel()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("p-floatlabel");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_AllDatepickersHaveVariantOn()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("variant=\"on\"");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_TargetSigningDate_HasLabelFor()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("for=\"targetSigningDate\"");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_ImplementationStartDate_HasLabelFor()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("for=\"implementationStartDate\"");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_TargetDeliveryDate_HasLabelFor()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("for=\"targetDeliveryDate\"");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_SubmissionDeadline_HasLabelFor()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("for=\"submissionDeadline\"");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_Consistency_AllFieldsInDictionary()
    {
        foreach (var id in WhenLabelAlignmentSpec.ExpectedDateFieldIds)
        {
            WhenLabelAlignmentSpec.DateFieldLabels.Should().ContainKey(id);
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_Consistency_LabelLengthsNonZero()
    {
        foreach (var id in WhenLabelAlignmentSpec.ExpectedDateFieldIds)
        {
            WhenLabelAlignmentSpec.GetLabelLength(id).Should().BeGreaterThan(0);
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_ScopedUnderPFloatlabel()
    {
        var scss = ReadWhenSectionScss();
        var pFloatlabelIdx = scss.IndexOf(".p-floatlabel", StringComparison.Ordinal);
        var pDatepickerIdx = scss.IndexOf(".p-datepicker ~ label", StringComparison.Ordinal);
        pFloatlabelIdx.Should().BeLessThan(pDatepickerIdx);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_FilledAndFocus_BothTargeted()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("p-inputwrapper-filled");
        scss.Should().Contain("p-inputwrapper-focus");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_DatepickersUseDateFormat()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("dateFormat=\"yy-mm-dd\"");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_DatepickersUseWFull()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("class=\"w-full\"");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_ImplementationStartDate_LongerThanTargetSigning()
    {
        var implLen = WhenLabelAlignmentSpec.GetLabelLength("implementationStartDate");
        var signingLen = WhenLabelAlignmentSpec.GetLabelLength("targetSigningDate");
        implLen.Should().BeGreaterThan(signingLen);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_ImplementationStartDate_LongestLabel()
    {
        var implLen = WhenLabelAlignmentSpec.GetLabelLength("implementationStartDate");
        WhenLabelAlignmentSpec.GetLongestLabelLength().Should().Be(implLen);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_DefaultLabel_HasAllTruncationProperties()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("overflow: hidden");
        scss.Should().Contain("text-overflow: ellipsis");
        scss.Should().Contain("white-space: nowrap");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_FilledLabel_HasVisualSeparation()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("background-color: white");
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_GridLayout_SupportsResponsive()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("grid");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_RequiredDefaultProperties_IncludeOverflow()
    {
        WhenLabelAlignmentSpec.RequiredDefaultLabelProperties.Should().Contain("overflow: hidden");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_RequiredDefaultProperties_IncludeTextOverflow()
    {
        WhenLabelAlignmentSpec.RequiredDefaultLabelProperties.Should().Contain("text-overflow: ellipsis");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_RequiredDefaultProperties_IncludeWhiteSpace()
    {
        WhenLabelAlignmentSpec.RequiredDefaultLabelProperties.Should().Contain("white-space: nowrap");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_RequiredFilledProperties_IncludeBackground()
    {
        WhenLabelAlignmentSpec.RequiredFilledLabelProperties.Should().Contain("background-color: white");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_RequiredFilledProperties_IncludePadding()
    {
        WhenLabelAlignmentSpec.RequiredFilledLabelProperties.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_HostNgDeep_Scoped()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(":host");
        scss.Should().Contain("::ng-deep");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_LabelsUseTranslationKeys()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("label.opportunity");
        html.Should().Contain("translate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_DefaultLabelMaxWidth_MatchesSpec()
    {
        WhenLabelAlignmentSpec.DefaultLabelMaxWidth.Should().Be("calc(100% - 3.5rem)");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void LabelSpec_FilledLabelMaxWidth_MatchesSpec()
    {
        WhenLabelAlignmentSpec.FilledLabelMaxWidth.Should().Be("calc(100% - 3rem)");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_DefaultLabelMaxWidth_InFile()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(WhenLabelAlignmentSpec.DefaultLabelMaxWidth);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_FilledLabelMaxWidth_InFile()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(WhenLabelAlignmentSpec.FilledLabelMaxWidth);
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
}
