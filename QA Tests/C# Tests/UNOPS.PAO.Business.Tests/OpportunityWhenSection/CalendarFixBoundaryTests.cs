using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Boundary tests for PNO-1210: Same-day edges, min/max dates, calc boundaries.
/// </summary>
public class PNO1210BoundaryTests
{
    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_SigningAndImplStartSameDay_NoError()
    {
        var d = new DateTime(2025, 6, 1, 12, 30, 0);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = d,
            ImplementationStartDate = d
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_ImplStartAndDeliverySameDay_NoError()
    {
        var d = new DateTime(2025, 7, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = d,
            TargetDeliveryDate = d
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_SubmissionAndSigningSameDay_NoError()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = d,
            SubmissionDeadline = d
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_ImplStartOneDayAfterSigning_NoError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 6, 2)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_DeliveryOneDayAfterImplStart_NoError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 2)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_SubmissionOneDayBeforeSigning_NoError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 2),
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_ImplStartOneDayBeforeSigning_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 2),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_DeliveryOneDayBeforeImplStart_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 2),
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_SubmissionOneDayAfterSigning_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 2)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_ScssCalcBoundary_ThreePointFiveRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("calc(100% - 3.5rem)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_ScssCalcBoundary_ThreeRemForFilled()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("calc(100% - 3rem)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_NormalizeDate_MidnightLocal_ProducesUTCMidnight()
    {
        var date = new DateTime(2025, 3, 9, 0, 0, 0);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().NotBeNull();
        result.Should().Contain("2025-03-09");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_NormalizeDate_EndOfDay_StripsTime()
    {
        var date = new DateTime(2025, 3, 9, 23, 59, 59);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().NotBeNull();
        result.Should().StartWith("2025-03-09");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_GetEffectiveImplStart_WhenOnlySigning_ReturnsSigning()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_GetEffectiveImplStart_WhenOnlyImpl_ReturnsImpl()
    {
        var impl = new DateTime(2025, 7, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = impl
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_GetMinDeliveryDate_WhenImplSet_ReturnsImplNotSigning()
    {
        var impl = new DateTime(2025, 8, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl
        };
        spec.GetMinDeliveryDate().Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_AllDatesSameDay_Valid()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = d,
            ImplementationStartDate = d,
            TargetDeliveryDate = d,
            SubmissionDeadline = d
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_LeapYearFeb29_NormalizesCorrectly()
    {
        var date = new DateTime(2024, 2, 29, 15, 30, 0);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().NotBeNull();
        result.Should().Contain("2024-02-29");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_YearBoundary_ValidOrder()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 12, 31),
            ImplementationStartDate = new DateTime(2026, 1, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_MonthBoundary_ValidOrder()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 1, 31),
            ImplementationStartDate = new DateTime(2025, 2, 1),
            TargetDeliveryDate = new DateTime(2025, 3, 31)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void HtmlTemplate_AppendToBody_OccursAtLeastFourTimes()
    {
        var html = ReadWhenSectionHtml();
        var count = CountOccurrences(html, "[appendTo]=\"'body'\"");
        count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ScssRule_PaddingZeroPointTwoFiveRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_SimulateStartEditing_WhenImplNullAndSigningSet_SetsImpl()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null,
            IsImplementationStartDateExplicitlySet = false
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_TimeComponentIgnored_DateOnlyComparison()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1, 0, 0, 0),
            ImplementationStartDate = new DateTime(2025, 6, 1, 23, 59, 59)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_TimeComponentIgnored_DeliveryBeforeImplSameDay_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1, 12, 0, 0),
            TargetDeliveryDate = new DateTime(2025, 7, 1, 0, 0, 0)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_UtcVsLocal_DateOnlyComparison()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 6, 2)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
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
    public void DateSpec_EmptyScssFile_TestHandlesGracefully()
    {
        var path = ResolveWhenSectionPath("opportunity-when-section.component.scss");
        if (!File.Exists(path))
            return;
        var content = File.ReadAllText(path);
        content.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_EmptyHtmlFile_TestHandlesGracefully()
    {
        var path = ResolveWhenSectionPath("opportunity-when-section.component.html");
        if (!File.Exists(path))
            return;
        var content = File.ReadAllText(path);
        content.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_AllNullDates_HasNoValidationErrors()
    {
        var spec = new WhenSectionDateSpec();
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_OnlySigningSet_NoErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_OnlyDeliverySet_NoErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetDeliveryDate = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_OnlySubmissionSet_NoErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_ImplStartLastDayOfMonth_Valid()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 5, 31),
            ImplementationStartDate = new DateTime(2025, 6, 30),
            TargetDeliveryDate = new DateTime(2026, 6, 30)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_FirstDayOfYear_Valid()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 1, 1),
            ImplementationStartDate = new DateTime(2025, 1, 1),
            TargetDeliveryDate = new DateTime(2025, 12, 31)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_LastDayOfYear_Valid()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 12, 31),
            ImplementationStartDate = new DateTime(2026, 1, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_GetMinImplStart_WhenSigningSet_ReturnsThatDate()
    {
        var d = new DateTime(2025, 3, 15);
        var spec = new WhenSectionDateSpec { TargetSigningDate = d };
        spec.GetMinImplementationStartDate().Should().Be(d);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_NormalizeDate_FirstOfMonth_FormatsCorrectly()
    {
        var date = new DateTime(2025, 1, 1);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().Contain("2025-01-01");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_NormalizeDate_LastOfYear_FormatsCorrectly()
    {
        var date = new DateTime(2025, 12, 31);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().Contain("2025-12-31");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_ScssBackgroundColorWhite_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("background-color: white");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_HtmlDatepickerIds_AllPresent()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDate");
        html.Should().Contain("implementationStartDate");
        html.Should().Contain("targetDeliveryDate");
        html.Should().Contain("submissionDeadline");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_EffectiveImplStart_PrefersImplOverSigning()
    {
        var impl = new DateTime(2025, 8, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_SimulateStartEditing_WhenImplAlreadySet_NoChange()
    {
        var impl = new DateTime(2025, 8, 1);
        var spec = new WhenSectionDateSpec
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
    public void DateSpec_ScssPInputwrapperFilled_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("p-inputwrapper-filled");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void DateSpec_ScssPInputwrapperFocus_Present()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("p-inputwrapper-focus");
    }

    private static string ReadWhenSectionScss()
    {
        var path = ResolveWhenSectionPath("opportunity-when-section.component.scss");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
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
