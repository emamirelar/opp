using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Tests for PNO-1210: WHEN section > Calendars — datepicker popup clipping and label overflow fix.
///
/// Requirements validated:
/// - REQ-1 to REQ-5: All datepickers have [appendTo]="'body'" to prevent clipping
/// - REQ-6 to REQ-8: SCSS label overflow prevention rules
/// - REQ-9: Datepicker popup fully visible on small screens (appendTo body)
/// - Date validation: Implementation Start >= Signing, Delivery >= Impl Start, Submission <= Signing
/// - Effective implementation start defaults to signing date when not set
/// </summary>
public class PNO1210PositiveTests
{
    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_TargetSigningDate_HasAppendToBody()
    {
        // REQ-2
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"targetSigningDate\"");
        html.Should().Contain("[appendTo]=\"'body'\"");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_ImplementationStartDate_HasAppendToBody()
    {
        // REQ-3
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"implementationStartDate\"");
        html.Should().Contain("[appendTo]=\"'body'\"");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_TargetDeliveryDate_HasAppendToBody()
    {
        // REQ-4
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"targetDeliveryDate\"");
        html.Should().Contain("[appendTo]=\"'body'\"");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_SubmissionDeadline_HasAppendToBody()
    {
        // REQ-5
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"submissionDeadline\"");
        html.Should().Contain("[appendTo]=\"'body'\"");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_AllFourDatepickers_HaveAppendToBody()
    {
        // REQ-1: Contract that all datepickers use appendTo body
        var html = ReadWhenSectionHtml();
        var appendToCount = CountOccurrences(html, "[appendTo]=\"'body'\"");
        appendToCount.Should().BeGreaterOrEqualTo(4, "Target Signing, Implementation Start, Target Delivery, Submission Deadline");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ScssRule_DatepickerLabel_HasMaxWidthOverflowEllipsis()
    {
        // REQ-6, REQ-7: Label overflow prevention
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
        // REQ-8
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("p-inputwrapper-filled");
        scss.Should().Contain("p-inputwrapper-focus");
        scss.Should().Contain("max-width: calc(100% - 3rem)");
        scss.Should().Contain("background-color: white");
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DateSpec_ValidDates_NoValidationErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 6, 15),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DateSpec_EffectiveImplementationStart_DefaultsToSigningWhenNotSet()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(new DateTime(2025, 6, 1));
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DateSpec_EffectiveImplementationStart_UsesImplStartWhenSet()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1)
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(new DateTime(2025, 7, 1));
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DateSpec_GetMinImplementationStartDate_ReturnsSigningDate()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec { TargetSigningDate = signing };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DateSpec_GetMinDeliveryDate_ReturnsImplStartOrSigning()
    {
        var implStart = new DateTime(2025, 7, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = implStart
        };
        spec.GetMinDeliveryDate().Should().Be(implStart);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DateSpec_NormalizeDateToUTCMidnight_ProducesIsoFormat()
    {
        var date = new DateTime(2025, 3, 9, 14, 30, 0);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().NotBeNull();
        result.Should().StartWith("2025-03-09");
        result.Should().EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DateSpec_NormalizeDateToUTCMidnight_NullReturnsNull()
    {
        WhenSectionDateSpec.NormalizeDateToUTCMidnight(null).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DateSpec_SameDaySigningAndImplStart_NoError()
    {
        var sameDay = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = sameDay,
            ImplementationStartDate = sameDay,
            TargetDeliveryDate = sameDay.AddMonths(12)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
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
        var scssRelative = Path.Combine("UNOPS.PAO.ClientApp", "src", "app", "features", "partnerships", "opportunities", "components", "opportunity", "view", "sections", "when", fileName);
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", scssRelative),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", scssRelative),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", scssRelative),
            Path.Combine(Directory.GetCurrentDirectory(), scssRelative),
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
