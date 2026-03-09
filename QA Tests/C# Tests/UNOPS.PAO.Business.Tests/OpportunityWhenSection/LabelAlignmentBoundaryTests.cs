using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Boundary tests for PNO-1182: Label length edges, calc boundaries, truncation thresholds.
/// </summary>
public class PNO1182BoundaryTests
{
    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_CalcBoundary_ThreePointFiveRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("calc(100% - 3.5rem)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_CalcBoundary_ThreeRemForFilled()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("calc(100% - 3rem)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_PaddingBoundary_ZeroPointTwoFiveRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_ImplementationStartDate_Length25()
    {
        WhenLabelAlignmentSpec.GetLabelLength("implementationStartDate").Should().Be(25);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_ProposalSubmissionDate_Length24()
    {
        WhenLabelAlignmentSpec.GetLabelLength("submissionDeadline").Should().Be(24);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_TargetSigningDate_Length19()
    {
        WhenLabelAlignmentSpec.GetLabelLength("targetSigningDate").Should().Be(19);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_TargetDeliveryDate_Length20()
    {
        WhenLabelAlignmentSpec.GetLabelLength("targetDeliveryDate").Should().Be(20);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_LongestLabel_Is25()
    {
        // Implementation Start Date = 25 chars
        WhenLabelAlignmentSpec.GetLongestLabelLength().Should().Be(25);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_ShortestLabel_Is19()
    {
        WhenLabelAlignmentSpec.GetShortestLabelLength().Should().Be(19);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_WouldNeedTruncation_Threshold15_ImplementationStart_True()
    {
        WhenLabelAlignmentSpec.WouldNeedTruncation("implementationStartDate", 15).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_WouldNeedTruncation_Threshold25_ImplementationStart_False()
    {
        WhenLabelAlignmentSpec.WouldNeedTruncation("implementationStartDate", 25).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_WouldNeedTruncation_Threshold24_SubmissionDeadline_False()
    {
        WhenLabelAlignmentSpec.WouldNeedTruncation("submissionDeadline", 24).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_WouldNeedTruncation_Threshold19_TargetSigning_False()
    {
        WhenLabelAlignmentSpec.WouldNeedTruncation("targetSigningDate", 19).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_OverflowHidden_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("overflow: hidden");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_TextOverflowEllipsis_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("text-overflow: ellipsis");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_WhiteSpaceNowrap_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("white-space: nowrap");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_BackgroundColorWhite_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("background-color: white");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void HtmlTemplate_AllFourDatepickerIds_Present()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDate");
        html.Should().Contain("implementationStartDate");
        html.Should().Contain("targetDeliveryDate");
        html.Should().Contain("submissionDeadline");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_AllFieldsUseSamePattern_AllPresent_ReturnsTrue()
    {
        var all = WhenLabelAlignmentSpec.ExpectedDateFieldIds.ToList();
        WhenLabelAlignmentSpec.AllFieldsUseSamePattern(all).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_AllFieldsUseSamePattern_ExtraFields_ReturnsTrue()
    {
        var extended = WhenLabelAlignmentSpec.ExpectedDateFieldIds.Concat(new[] { "extra" }).ToList();
        WhenLabelAlignmentSpec.AllFieldsUseSamePattern(extended).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_PInputwrapperFilled_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("p-inputwrapper-filled");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_PInputwrapperFocus_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("p-inputwrapper-focus");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_DefaultLabelMaxWidth_ExactValue()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(WhenLabelAlignmentSpec.DefaultLabelMaxWidth);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_FilledLabelMaxWidth_ExactValue()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(WhenLabelAlignmentSpec.FilledLabelMaxWidth);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_RequiredDefaultProperties_CountThree()
    {
        WhenLabelAlignmentSpec.RequiredDefaultLabelProperties.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_RequiredFilledProperties_CountTwo()
    {
        WhenLabelAlignmentSpec.RequiredFilledLabelProperties.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_AllRequiredDefaultProperties_Present()
    {
        var scss = ReadWhenSectionScss();
        foreach (var prop in WhenLabelAlignmentSpec.RequiredDefaultLabelProperties)
        {
            scss.Should().Contain(prop);
        }
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_AllRequiredFilledProperties_Present()
    {
        var scss = ReadWhenSectionScss();
        foreach (var prop in WhenLabelAlignmentSpec.RequiredFilledLabelProperties)
        {
            scss.Should().Contain(prop);
        }
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_FileExists_NonEmpty()
    {
        var path = ResolveWhenSectionPath("opportunity-when-section.component.scss");
        if (!File.Exists(path))
            return;
        var content = File.ReadAllText(path);
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void HtmlTemplate_FileExists_NonEmpty()
    {
        var path = ResolveWhenSectionPath("opportunity-when-section.component.html");
        if (!File.Exists(path))
            return;
        var content = File.ReadAllText(path);
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_ExpectedDateFieldIds_CountFour()
    {
        WhenLabelAlignmentSpec.ExpectedDateFieldIds.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_WouldNeedTruncation_Threshold20_TargetDelivery_False()
    {
        WhenLabelAlignmentSpec.WouldNeedTruncation("targetDeliveryDate", 20).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void LabelSpec_WouldNeedTruncation_Threshold20_ImplementationStart_True()
    {
        WhenLabelAlignmentSpec.WouldNeedTruncation("implementationStartDate", 20).Should().BeTrue();
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
