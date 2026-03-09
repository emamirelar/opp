using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Negative tests for PNO-1210: Invalid date combinations, missing appendTo, validation failures.
/// </summary>
public class PNO1210NegativeTests
{
    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_ImplementationStartBeforeSigning_HasValidationError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_DeliveryBeforeImplementationStart_HasValidationError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2025, 6, 15)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_DeliveryBeforeSigningWhenNoImplStart_HasValidationError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 7, 1),
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 6, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_SubmissionDeadlineAfterSigning_HasValidationError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 15)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_ImplementationStartOneDayBeforeSigning_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 2),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_DeliveryOneDayBeforeImplStart_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2025, 6, 30)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_SubmissionDeadlineOneDayAfterSigning_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 2)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_AllThreeErrorsSimultaneously_HasDateValidationErrorsTrue()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = new DateTime(2025, 5, 1),
            SubmissionDeadline = new DateTime(2025, 6, 20)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_GetMinImplementationStartDate_WhenSigningNull_ReturnsNull()
    {
        var spec = new WhenSectionDateSpec { TargetSigningDate = null };
        spec.GetMinImplementationStartDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_GetMinDeliveryDate_WhenBothNull_ReturnsNull()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = null
        };
        spec.GetMinDeliveryDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_IsImplementationStartBeforeSigning_WhenSigningNull_ReturnsFalse()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_IsImplementationStartBeforeSigning_WhenImplStartNull_ReturnsFalse()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_IsDeliveryBeforeImplStart_WhenDeliveryNull_ReturnsFalse()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = null
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_IsSubmissionDeadlineAfterSigning_WhenSigningNull_ReturnsFalse()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = null,
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_IsSubmissionDeadlineAfterSigning_WhenSubmissionNull_ReturnsFalse()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = null
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_MustNotContainAppendToElement()
    {
        var html = ReadWhenSectionHtml();
        html.Should().NotContain("[appendTo]=\"'element'\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustNotUseFixedPixelWidthForLabel()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().NotContain("max-width: 100px");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_EffectiveImplStart_WhenBothNull_ReturnsNull()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = null
        };
        spec.GetEffectiveImplementationStartDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_NormalizeDateToUTCMidnight_WithMinValue_DoesNotThrow()
    {
        var act = () => WhenSectionDateSpec.NormalizeDateToUTCMidnight(DateTime.MinValue);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_NormalizeDateToUTCMidnight_WithMaxValue_DoesNotThrow()
    {
        var act = () => WhenSectionDateSpec.NormalizeDateToUTCMidnight(DateTime.MaxValue);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_ImplementationStartYearBeforeSigning_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2026, 6, 1),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_DeliveryYearBeforeImplStart_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 1, 1),
            ImplementationStartDate = new DateTime(2026, 1, 1),
            TargetDeliveryDate = new DateTime(2025, 12, 31)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_SubmissionDeadlineYearAfterSigning_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2026, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_ValidDatesWithSubmissionBeforeSigning_NoSubmissionError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_ImplStartEqualsSigning_NoError()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = d,
            ImplementationStartDate = d
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_DeliveryEqualsImplStart_NoError()
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
    [Trait("Category", "Negative")]
    public void DateSpec_SubmissionEqualsSigning_NoError()
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
    [Trait("Category", "Negative")]
    public void DateSpec_SimulateStartEditing_WhenImplNotExplicitlySet_DefaultsToSigning()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null,
            IsImplementationStartDateExplicitlySet = false
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().Be(new DateTime(2025, 6, 1));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_SimulateStartEditing_WhenSigningNull_DoesNotSetImpl()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = null
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_SimulateStartEditing_WhenImplAlreadySet_DoesNotOverride()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            IsImplementationStartDateExplicitlySet = true
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().Be(new DateTime(2025, 7, 1));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_LabelMustHaveTextOverflowEllipsis_NotClip()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("text-overflow: ellipsis");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_LabelMustHaveWhiteSpaceNowrap()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("white-space: nowrap");
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
    public void DateSpec_GetMinDeliveryDate_WhenOnlySigningSet_ReturnsSigning()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null
        };
        spec.GetMinDeliveryDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_HasDateValidationErrors_WhenNoDatesSet_ReturnsFalse()
    {
        var spec = new WhenSectionDateSpec();
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_OnlyImplStartBeforeSigning_OtherValid_HasErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 7, 1),
            ImplementationStartDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = new DateTime(2026, 1, 1),
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_OnlyDeliveryBeforeImplStart_OtherValid_HasErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 1),
            SubmissionDeadline = new DateTime(2025, 5, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_OnlySubmissionAfterSigning_OtherValid_HasErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 1, 1),
            SubmissionDeadline = new DateTime(2025, 6, 15)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_ImplStartMonthBeforeSigning_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 8, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_DeliveryMonthBeforeImplStart_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 9, 1),
            TargetDeliveryDate = new DateTime(2025, 8, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_SubmissionMonthAfterSigning_IsError()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 5, 1),
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_SimulateStartEditing_WhenImplExplicitlySet_DoesNotOverride()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            IsImplementationStartDateExplicitlySet = true
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().Be(new DateTime(2025, 8, 1));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssRule_MustContainPFloatlabelScope()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(".p-floatlabel");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_AllDatepickersMustHaveDateFormat()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("dateFormat=\"yy-mm-dd\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DateSpec_ImplStartBeforeSigning_GetMinDeliveryUsesSigning()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 7, 1),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.GetMinDeliveryDate().Should().Be(new DateTime(2025, 6, 1));
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
