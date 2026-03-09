using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Tests for PNO-1182: WHEN Section — Date field labels misaligned (Floating label clash).
///
/// Requirements validated:
/// - REQ-1: All datepicker floating labels must have consistent alignment
/// - REQ-2: Long labels must be constrained with max-width
/// - REQ-3: Labels must be truncated with text-overflow: ellipsis
/// - REQ-4: Default datepicker label must use max-width: calc(100% - 3.5rem)
/// - REQ-5: Focused/filled label must use max-width: calc(100% - 3rem) with white background and padding
/// - REQ-6: Labels must use overflow: hidden and white-space: nowrap
/// - REQ-7: Filled/focused label must have background-color: white and padding: 0 0.25rem
/// </summary>
public class PNO1182PositiveTests
{
    [Fact]
    [Trait("Category", "Positive")]
    public void ScssRule_DefaultDatepickerLabel_HasAllRequiredProperties()
    {
        // REQ-4, REQ-6
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("max-width: calc(100% - 3.5rem)");
        scss.Should().Contain("overflow: hidden");
        scss.Should().Contain("text-overflow: ellipsis");
        scss.Should().Contain("white-space: nowrap");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ScssRule_FilledOrFocusedLabel_HasBackgroundAndPadding()
    {
        // REQ-5, REQ-7
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("p-inputwrapper-filled");
        scss.Should().Contain("p-inputwrapper-focus");
        scss.Should().Contain("max-width: calc(100% - 3rem)");
        scss.Should().Contain("background-color: white");
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_AllFourDatepickers_UseFloatLabelPattern()
    {
        // REQ-1
        var html = ReadWhenSectionHtml();
        html.Should().Contain("p-floatlabel");
        html.Should().Contain("id=\"targetSigningDate\"");
        html.Should().Contain("id=\"implementationStartDate\"");
        html.Should().Contain("id=\"targetDeliveryDate\"");
        html.Should().Contain("id=\"submissionDeadline\"");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void LabelSpec_ImplementationStartDate_IsLongestLabel()
    {
        // REQ-2: Long labels need truncation — Implementation Start Date is the longest (25 chars)
        var len = WhenLabelAlignmentSpec.GetLabelLength("implementationStartDate");
        len.Should().Be(25);
        WhenLabelAlignmentSpec.WouldNeedTruncation("implementationStartDate").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void LabelSpec_ProposalSubmissionDate_IsLongLabel()
    {
        // REQ-2: Proposal Submission Date (24 chars) needs truncation on narrow layouts
        var len = WhenLabelAlignmentSpec.GetLabelLength("submissionDeadline");
        len.Should().Be(24);
        WhenLabelAlignmentSpec.WouldNeedTruncation("submissionDeadline").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void LabelSpec_TargetSigningDate_ShorterLabel()
    {
        // REQ-1: Shorter labels still benefit from consistent alignment rules
        var len = WhenLabelAlignmentSpec.GetLabelLength("targetSigningDate");
        len.Should().Be(19);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ScssRule_ScopedUnderHostNgDeep()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(":host");
        scss.Should().Contain("::ng-deep");
        scss.Should().Contain(".p-floatlabel");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ScssRule_DefaultLabelMaxWidth_ThreePointFiveRem()
    {
        // REQ-4
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(WhenLabelAlignmentSpec.DefaultLabelMaxWidth);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ScssRule_FilledLabelMaxWidth_ThreeRem()
    {
        // REQ-5
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(WhenLabelAlignmentSpec.FilledLabelMaxWidth);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void LabelSpec_AllFourFields_DefinedInSpec()
    {
        // REQ-1: Consistency — all fields use same pattern
        var expected = WhenLabelAlignmentSpec.ExpectedDateFieldIds;
        expected.Should().HaveCount(4);
        expected.Should().Contain("targetSigningDate");
        expected.Should().Contain("implementationStartDate");
        expected.Should().Contain("targetDeliveryDate");
        expected.Should().Contain("submissionDeadline");
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
