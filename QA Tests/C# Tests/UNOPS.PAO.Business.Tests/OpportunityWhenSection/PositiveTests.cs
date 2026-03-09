/// <summary>
/// Tests for PNO-699, PNO-811, PNO-859: Opportunity WHEN - Timeline & Key Dates.
/// Requirements validated:
/// - PNO-699 AC1: WHEN section exists with timeline and key dates
/// - PNO-699 AC2: Target Signing, Implementation Start, Target Delivery dates; duration derived
/// - PNO-699 AC3: Days until signing, timeline Gantt
/// - PNO-699 AC5: Submission deadline before signing, firm deadline notes
/// - PNO-811: 6-month duration option
/// - PNO-859: Date validation, calculator optional, manual date clears duration
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

public class OpportunityWhenSectionPositiveTests
{
    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_WhenSectionExists_ContainsTimelineKeyDates()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("whenTimelineAndKeyDates");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_TargetSigningDate_HasFormControl()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"targetSigningDate\"");
        html.Should().Contain("targetSigningDateControl");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_ImplementationStartDate_HasFormControl()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"implementationStartDate\"");
        html.Should().Contain("implementationStartDateControl");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_TargetDeliveryDate_HasFormControl()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"targetDeliveryDate\"");
        html.Should().Contain("targetDeliveryDateControl");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_SubmissionDeadline_HasFormControl()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"submissionDeadline\"");
        html.Should().Contain("submissionDeadlineControl");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_AllDatepickers_HaveAppendToBody()
    {
        var html = ReadWhenSectionHtml();
        var count = CountOccurrences(html, "[appendTo]=\"'body'\"");
        count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_ImplementationStartDate_HasMinDateBinding()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("getMinImplementationStartDate");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_TargetDeliveryDate_HasMinDateBinding()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("getMinDeliveryDate");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_SubmissionDeadline_HasMaxDateBinding()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDateControl.value");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_DurationCalculator_HasOptions()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("durationOptions");
        html.Should().Contain("implementationDuration");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_ValidDates_NoValidationErrors()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_ImplementationStartDefaultsToSigning_WhenNotSet()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null,
            IsImplementationStartDateExplicitlySet = false
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().Be(new DateTime(2025, 6, 1));
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_EffectiveImplementationStart_ReturnsImplWhenSet()
    {
        var impl = new DateTime(2025, 8, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_EffectiveImplementationStart_ReturnsSigningWhenImplNull()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_SubmissionDeadlineBeforeSigning_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            SubmissionDeadline = new DateTime(2025, 5, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_SubmissionDeadlineEqualsSigning_Valid()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = d,
            SubmissionDeadline = d
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_DeliveryEqualsImplementationStart_Valid()
    {
        var impl = new DateTime(2025, 7, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl,
            TargetDeliveryDate = impl
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_NormalizeDateToUTCMidnight_ProducesIsoString()
    {
        var d = new DateTime(2025, 6, 15);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().NotBeNull().And.EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_NormalizeNull_ReturnsNull()
    {
        OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(null).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_GetMinImplementationStartDate_ReturnsSigning()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec { TargetSigningDate = signing };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_GetMinDeliveryDate_ReturnsEffectiveStart()
    {
        var impl = new DateTime(2025, 8, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl
        };
        spec.GetMinDeliveryDate().Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DurationOptions_IncludeSixMonths_PNO811()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(6);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void DurationOptions_IncludeAllExpectedValues()
    {
        var expected = new[] { 3, 6, 12, 18, 24, 36, -1 };
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().BeEquivalentTo(expected);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_DurationCalculator_HasOptionalHint_PNO859()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("durationCalculator");
        html.Should().Contain("durationCalculatorHint");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_SigningDateDetails_SectionExists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("signingDateDetails");
        html.Should().Contain("isSigningDateFirm");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_TimelineOverview_Exists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("timelineOverview");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_WorkBreakdownStructure_Exists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("work-breakdown-structure");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_ValidChronology_AllChecksPass()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_SameDayDates_Valid()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = d,
            ImplementationStartDate = d,
            TargetDeliveryDate = d.AddMonths(12)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Spec_DateFormat_yyMmDd()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("dateFormat=\"yy-mm-dd\"");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void HtmlTemplate_FloatLabel_VariantOn()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("p-floatlabel");
        html.Should().Contain("variant=\"on\"");
    }

    private static string ReadWhenSectionHtml()
    {
        var path = ResolveWhenSectionPath("opportunity-when-section.component.html");
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
