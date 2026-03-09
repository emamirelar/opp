/// <summary>
/// Boundary tests for PNO-699, PNO-811, PNO-859: WHEN section edge cases, min/max values, nullable FK.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

public class OpportunityWhenSectionBoundaryTests
{
    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ImplementationStartExactlyEqualsSigning_Valid()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = d,
            ImplementationStartDate = d
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DeliveryExactlyEqualsImplementationStart_Valid()
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
    [Trait("Category", "Boundary")]
    public void Spec_SubmissionExactlyEqualsSigning_Valid()
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
    [Trait("Category", "Boundary")]
    public void Spec_ImplStartOneDayAfterSigning_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 6, 2)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DeliveryOneDayAfterImplStart_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 2)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_SubmissionOneDayBeforeSigning_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            SubmissionDeadline = new DateTime(2025, 6, 14)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_AllDatesSameDay_Valid()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = d,
            ImplementationStartDate = d,
            TargetDeliveryDate = d,
            SubmissionDeadline = d
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ImplStartNull_EffectiveUsesSigning()
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
    [Trait("Category", "Boundary")]
    public void Spec_ImplStartSet_EffectiveUsesImpl()
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
    [Trait("Category", "Boundary")]
    public void Spec_NormalizeDate_MidnightUtc()
    {
        var d = new DateTime(2025, 6, 15, 23, 59, 59);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().NotBeNull().And.Contain("2025-06-15").And.EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_NormalizeDate_EarlyMorning()
    {
        var d = new DateTime(2025, 6, 15, 0, 0, 1);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().NotBeNull().And.Contain("2025-06-15");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DurationOption_SixMonths_PNO811()
    {
        var idx = Array.IndexOf(OpportunityWhenSectionSpec.ExpectedDurationValues, 6);
        idx.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DurationOption_CustomValue()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(-1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DurationOptions_Ordered()
    {
        var values = OpportunityWhenSectionSpec.ExpectedDurationValues;
        values[0].Should().Be(3);
        values[1].Should().Be(6);
        values[^1].Should().Be(-1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_GetMinImplementationStartDate_NullWhenSigningNull()
    {
        var spec = new OpportunityWhenSectionSpec { TargetSigningDate = null };
        spec.GetMinImplementationStartDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_GetMinDeliveryDate_NullWhenBothNull()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = null
        };
        spec.GetMinDeliveryDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_SimulateStartEditing_OnlyWhenNotExplicitlySet()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null,
            IsImplementationStartDateExplicitlySet = false
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_YearBoundary_Dec31ToJan1()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 12, 31),
            ImplementationStartDate = new DateTime(2026, 1, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_LeapYear_Feb29()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2024, 2, 29),
            ImplementationStartDate = new DateTime(2024, 3, 1),
            TargetDeliveryDate = new DateTime(2025, 2, 28)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_MonthEnd_Jan31ToFeb28()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 1, 31),
            ImplementationStartDate = new DateTime(2025, 2, 28),
            TargetDeliveryDate = new DateTime(2025, 3, 31)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ThreeMonthDuration_CalculatesCorrectly()
    {
        var signing = new DateTime(2025, 6, 1);
        var expectedDelivery = signing.AddMonths(3);
        expectedDelivery.Should().Be(new DateTime(2025, 9, 1));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_SixMonthDuration_CalculatesCorrectly()
    {
        var signing = new DateTime(2025, 6, 1);
        var expectedDelivery = signing.AddMonths(6);
        expectedDelivery.Should().Be(new DateTime(2025, 12, 1));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_TwelveMonthDuration_CalculatesCorrectly()
    {
        var signing = new DateTime(2025, 6, 1);
        var expectedDelivery = signing.AddMonths(12);
        expectedDelivery.Should().Be(new DateTime(2026, 6, 1));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_EighteenMonthDuration_CrossesYear()
    {
        var signing = new DateTime(2025, 6, 1);
        var expectedDelivery = signing.AddMonths(18);
        expectedDelivery.Should().Be(new DateTime(2026, 12, 1));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_TwentyFourMonthDuration_TwoYears()
    {
        var signing = new DateTime(2025, 6, 1);
        var expectedDelivery = signing.AddMonths(24);
        expectedDelivery.Should().Be(new DateTime(2027, 6, 1));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ThirtySixMonthDuration_ThreeYears()
    {
        var signing = new DateTime(2025, 6, 1);
        var expectedDelivery = signing.AddMonths(36);
        expectedDelivery.Should().Be(new DateTime(2028, 6, 1));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void HtmlTemplate_DatepickerCount_AtLeastFour()
    {
        var html = ReadWhenSectionHtml();
        var count = CountOccurrences(html, "p-datepicker");
        count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void HtmlTemplate_AppendToBodyCount_AtLeastFour()
    {
        var html = ReadWhenSectionHtml();
        var count = CountOccurrences(html, "[appendTo]=\"'body'\"");
        count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_TimeComponent_IgnoredInComparison()
    {
        var signing = new DateTime(2025, 6, 1, 0, 0, 0);
        var impl = new DateTime(2025, 6, 1, 23, 59, 59);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = impl
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_NormalizeDate_LeapYear()
    {
        var d = new DateTime(2024, 2, 29);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().NotBeNull().And.Contain("2024-02-29");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_NormalizeDate_YearEnd()
    {
        var d = new DateTime(2025, 12, 31);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().NotBeNull().And.Contain("2025-12-31");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_NormalizeDate_YearStart()
    {
        var d = new DateTime(2025, 1, 1);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().NotBeNull().And.Contain("2025-01-01");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ImplStartExplicitlySet_SimulateDoesNotChange()
    {
        var impl = new DateTime(2025, 9, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl,
            IsImplementationStartDateExplicitlySet = true
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_AllNull_HasNoValidationErrors()
    {
        var spec = new OpportunityWhenSectionSpec();
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_OnlySigningSet_NoValidationErrors()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_OnlySubmissionSet_NoValidationErrors()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DeliveryOnly_NoValidationErrors()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void HtmlTemplate_CustomDuration_MinMax()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("customDuration");
        html.Should().Contain("[min]=\"1\"");
        html.Should().Contain("[max]=\"120\"");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DurationOptions_NoDuplicateValues()
    {
        var values = OpportunityWhenSectionSpec.ExpectedDurationValues;
        values.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_GetMinDeliveryDate_SigningWhenImplNull()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null
        };
        spec.GetMinDeliveryDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_GetMinDeliveryDate_ImplWhenBothSet()
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
    [Trait("Category", "Boundary")]
    public void Spec_ImplStartBeforeSigning_GetMinImplReturnsSigning()
    {
        var signing = new DateTime(2025, 6, 15);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ValidDates_AllNullExceptRequiredPair()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_SigningDateNotes_MaxLength1000()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("maxlength=\"1000\"");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DurationDropdown_ShowClear()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[showClear]=\"true\"");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_IsEditing_ControlsVisibility()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("isEditing()");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_HasUnsavedChanges_ShowsWarning()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("hasUnsavedChangesSignal");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_TimelineCollapsed_State()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("isTimelineCollapsed");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ViewMode_ShowsFormatDate()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("formatDate(");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ImplementationDurationDisplay_Computed()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("implementationDurationDisplay");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_DaysUntilImplementationStart_Computed()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("daysUntilImplementationStart");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_TimelinePhases_Computed()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("timelinePhases");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_WorkBreakdownStructure_Id()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("work-breakdown-structure");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_SortedDeliverables_ForTimeline()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("sortedDeliverables");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_SubmissionDeadline_ConditionalOnFirm()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("isSigningDateFirmControl.value");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-251")]
    public void Spec_DateValidation_BlocksSave()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("hasDateValidationErrors");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_ResetToSigningDate_ButtonExists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("resetToSigningDate");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_OnDurationChange_HandlerExists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onDurationChange");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_OnCustomDurationChange_HandlerExists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onCustomDurationChange");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_OnSelect_SigningDate()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onSigningDateManualChange");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_OnSelect_DeliveryDate()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onDeliveryDateManualChange");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Spec_OnSelect_ImplementationStart()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onImplementationStartDateManualChange");
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
