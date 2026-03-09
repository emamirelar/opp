using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Negative tests for PNO-1182: Missing SCSS rules, wrong values, anti-patterns.
/// </summary>
public class PNO1182NegativeTests
{
    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseFixedPixelMaxWidth()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().NotContain("max-width: 100px");
        scss.Should().NotContain("max-width: 200px");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseTextOverflowClip()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().NotContain("text-overflow: clip");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseWhiteSpaceNormal()
    {
        var scss = ReadWhenSectionScss();
        var datepickerLabelSection = ExtractDatepickerLabelSection(scss);
        datepickerLabelSection.Should().NotContain("white-space: normal");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseOverflowVisible()
    {
        var scss = ReadWhenSectionScss();
        var datepickerLabelSection = ExtractDatepickerLabelSection(scss);
        datepickerLabelSection.Should().NotContain("overflow: visible");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_DefaultLabelMustNotUseThreeRem()
    {
        var scss = ReadWhenSectionScss();
        var defaultSection = ExtractDefaultDatepickerLabelSection(scss);
        defaultSection.Should().NotContain("max-width: calc(100% - 3rem)");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseTransparentBackground()
    {
        var scss = ReadWhenSectionScss();
        var filledSection = ExtractFilledFocusedLabelSection(scss);
        filledSection.Should().NotContain("background-color: transparent");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotOmitPaddingForFilledLabel()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseWrongCalcValue()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().NotContain("max-width: calc(100% - 2rem)");
        scss.Should().NotContain("max-width: calc(100% - 4rem)");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_MustNotUseGenericLabelWithoutFor()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("for=\"targetSigningDate\"");
        html.Should().Contain("for=\"implementationStartDate\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void LabelSpec_UnknownFieldId_ReturnsZeroLength()
    {
        WhenLabelAlignmentSpec.GetLabelLength("unknownField").Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void LabelSpec_EmptyFieldId_ReturnsZeroLength()
    {
        WhenLabelAlignmentSpec.GetLabelLength("").Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotHaveDuplicatePDatepickerLabelRules()
    {
        var scss = ReadWhenSectionScss();
        var count = CountOccurrences(scss, ".p-datepicker ~ label");
        count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseInlineStylesInTemplate()
    {
        var html = ReadWhenSectionHtml();
        html.Should().NotContain("style=\"max-width:");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_FilledLabelMustNotOmitBackgroundColor()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("background-color: white");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_DefaultLabelMustNotHaveNonWhiteBackgroundColor()
    {
        var scss = ReadWhenSectionScss();
        var defaultBlock = ExtractDefaultDatepickerLabelBlock(scss);
        if (defaultBlock.Contains("background-color"))
        {
            defaultBlock.Should().Contain("background-color: white",
                "PNO-1182: Default label background must be white (variant='on' requires opaque background on border)");
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_DatepickersMustHaveShowIcon()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[showIcon]=\"true\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseWordBreakBreakAll()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("word-break: break-all");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseMinWidthForLabel()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("min-width:");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void LabelSpec_WouldNeedTruncation_ShortLabel_ReturnsFalse()
    {
        WhenLabelAlignmentSpec.WouldNeedTruncation("targetSigningDate", 25).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseFlexShrinkZero()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("flex-shrink: 0");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_MustNotHaveDatepickerWithoutFloatLabel()
    {
        var html = ReadWhenSectionHtml();
        var floatLabelCount = CountOccurrences(html, "p-floatlabel");
        var datepickerCount = CountOccurrences(html, "p-datepicker");
        floatLabelCount.Should().BeGreaterOrEqualTo(datepickerCount);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseImportantOnMaxWidth()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("max-width: calc(100% - 3.5rem) !important");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_PaddingMustBeZeroPointTwoFiveRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().NotContain("padding: 0 0.5rem");
        scss.Should().Contain("0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void LabelSpec_AllFieldsUseSamePattern_EmptyList_ReturnsFalse()
    {
        WhenLabelAlignmentSpec.AllFieldsUseSamePattern(Array.Empty<string>()).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void LabelSpec_AllFieldsUseSamePattern_MissingOne_ReturnsFalse()
    {
        var partial = new[] { "targetSigningDate", "implementationStartDate", "targetDeliveryDate" };
        WhenLabelAlignmentSpec.AllFieldsUseSamePattern(partial).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseDisplayNone()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("display: none");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseVisibilityHidden()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("visibility: hidden");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_SubmissionDeadline_MustHaveLabel()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("submissionDeadline");
        html.Should().Contain("label.submissionDeadline");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_CalcMustUseRemNotPx()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().NotContain("calc(100% - 56px)");
        scss.Should().Contain("3.5rem");
        scss.Should().Contain("3rem");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseFloatForLabel()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("float:");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUsePositionAbsoluteOnLabel()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("position: absolute");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseWidth100Percent()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("width: 100%");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseZIndexForLabel()
    {
        var scss = ReadWhenSectionScss();
        var labelSection = ExtractDatepickerLabelSection(scss);
        labelSection.Should().NotContain("z-index:");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_MustNotHaveDuplicateDatepickerIds()
    {
        var html = ReadWhenSectionHtml();
        var targetCount = CountOccurrences(html, "id=\"targetSigningDate\"");
        targetCount.Should().Be(1);
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

    private static string ExtractDatepickerLabelSection(string scss)
    {
        var idx = scss.IndexOf(".p-datepicker", StringComparison.Ordinal);
        return idx >= 0 ? scss[idx..] : string.Empty;
    }

    private static string ExtractDefaultDatepickerLabelSection(string scss)
    {
        var idx = scss.IndexOf(".p-datepicker ~ label", StringComparison.Ordinal);
        var endIdx = scss.IndexOf("&.p-inputwrapper-filled", idx, StringComparison.Ordinal);
        return idx >= 0 ? scss[idx..(endIdx > 0 ? endIdx : scss.Length)] : string.Empty;
    }

    private static string ExtractDefaultDatepickerLabelBlock(string scss)
    {
        var start = scss.IndexOf(".p-datepicker ~ label", StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        var braceStart = scss.IndexOf('{', start);
        if (braceStart < 0) return string.Empty;
        var depth = 1;
        var i = braceStart + 1;
        while (i < scss.Length && depth > 0)
        {
            if (scss[i] == '{') depth++;
            else if (scss[i] == '}') depth--;
            i++;
        }
        return scss[start..i];
    }

    private static string ExtractFilledFocusedLabelSection(string scss)
    {
        var idx = scss.IndexOf("p-inputwrapper-filled", StringComparison.Ordinal);
        return idx >= 0 ? scss[idx..] : string.Empty;
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
