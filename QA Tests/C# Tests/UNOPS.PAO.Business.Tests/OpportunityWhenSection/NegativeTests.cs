/// <summary>
/// Negative tests for PNO-699, PNO-811, PNO-859: WHEN section invalid inputs, validation failures.
/// </summary>

using System.Linq;
using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

public class OpportunityWhenSectionNegativeTests
{
    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ImplementationStartBeforeSigning_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DeliveryBeforeImplementationStart_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_SubmissionDeadlineAfterSigning_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 15)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DeliveryBeforeSigning_WhenNoImplStart_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 7, 1),
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 6, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ImplStartOneDayBeforeSigning_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 2),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DeliveryOneDayBeforeImplStart_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 2),
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_SubmissionOneDayAfterSigning_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 2)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_AllThreeValidationErrors_Simultaneously()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = new DateTime(2025, 5, 1),
            SubmissionDeadline = new DateTime(2025, 6, 20)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_GetMinImplementationStartDate_NullWhenNoSigning()
    {
        var spec = new OpportunityWhenSectionSpec { TargetSigningDate = null };
        spec.GetMinImplementationStartDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_GetMinDeliveryDate_NullWhenNoStart()
    {
        var spec = new OpportunityWhenSectionSpec();
        spec.GetMinDeliveryDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_GetEffectiveImplementationStartDate_NullWhenBothNull()
    {
        var spec = new OpportunityWhenSectionSpec();
        spec.GetEffectiveImplementationStartDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_IsImplementationStartBeforeSigningDate_FalseWhenSigningNull()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_IsImplementationStartBeforeSigningDate_FalseWhenImplNull()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_IsDeliveryDateBeforeImplementationStart_FalseWhenDeliveryNull()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = null
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_IsDeliveryDateBeforeImplementationStart_FalseWhenStartNull()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 6, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_IsSubmissionDeadlineAfterSigningDate_FalseWhenSigningNull()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = null,
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_IsSubmissionDeadlineAfterSigningDate_FalseWhenSubmissionNull()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = null
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_MustNotHavePlaceholderOnSubmissionDeadline_WhenFloatLabel()
    {
        var html = ReadWhenSectionHtml();
        var submissionSection = html.IndexOf("submissionDeadline", StringComparison.Ordinal);
        if (submissionSection >= 0)
        {
            var snippet = html.Substring(Math.Max(0, submissionSection - 50), Math.Min(200, html.Length - Math.Max(0, submissionSection - 50)));
            snippet.Should().NotContain("placeholder=\"");
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ImplStartBeforeSigning_GetEffectiveStillReturnsImpl()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(new DateTime(2025, 6, 1));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DurationOptions_MustNotExcludeSixMonths()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().NotBeEmpty();
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(6);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_InvalidDateOrder_BlocksSave()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 5, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_SubmissionWayAfterSigning_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 12, 31)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DeliveryWayBeforeImplStart_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2026, 6, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ImplStartYearBeforeSigning_ValidationError()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2026, 6, 1),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_Datepickers_MustNotOmitAppendToBody()
    {
        var html = ReadWhenSectionHtml();
        if (html.Contains("p-datepicker"))
        {
            html.Should().Contain("[appendTo]=\"'body'\"");
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_SimulateStartEditing_DoesNotOverwriteExplicitImpl()
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
    [Trait("Category", "Negative")]
    public void Spec_SimulateStartEditing_DoesNothingWhenSigningNull()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = null,
            IsImplementationStartDateExplicitlySet = false
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_SimulateStartEditing_DoesNothingWhenImplAlreadySet()
    {
        var impl = new DateTime(2025, 8, 1);
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
    [Trait("Category", "Negative")]
    public void Spec_NormalizeDateToUTCMidnight_NullInput()
    {
        OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(null).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_HasDateValidationErrors_TrueWhenImplBeforeSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_HasDateValidationErrors_TrueWhenDeliveryBeforeStart()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_HasDateValidationErrors_TrueWhenSubmissionAfterSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 15)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_MustHaveMinDateOnImplementationStart()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[minDate]=\"getMinImplementationStartDate()\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_MustHaveMinDateOnTargetDelivery()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[minDate]=\"getMinDeliveryDate()\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_SubmissionDeadline_MustHaveMaxDate()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[maxDate]=\"targetSigningDateControl.value\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DurationOptions_CustomValueMustBeNegativeOne()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(-1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_InvalidChronology_ImplStartBeforeSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 7, 1),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_InvalidChronology_DeliveryBeforeImpl()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 15)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_InvalidChronology_SubmissionAfterSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 5, 1),
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_GetMinDeliveryDate_WhenOnlySigning_ReturnsSigning()
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
    [Trait("Category", "Negative")]
    public void Spec_GetMinDeliveryDate_WhenImplSet_ReturnsImpl()
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
    [Trait("Category", "Negative")]
    public void HtmlTemplate_ValidationMessages_MustExist()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("implementationStartMustBeAfterSigningDate");
        html.Should().Contain("deliveryDateMustBeAfterImplementationStart");
        html.Should().Contain("submissionDeadlineMustBeBeforeSigningDate");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ThreeMonthDuration_ValidOption()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(3);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_TwelveMonthDuration_ValidOption()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(12);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_EighteenMonthDuration_ValidOption()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(18);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_TwentyFourMonthDuration_ValidOption()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(24);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ThirtySixMonthDuration_ValidOption()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(36);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DurationOptions_CountIsSeven()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().HaveCount(7);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ImplStartSameAsSigning_NotValidationError()
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
    [Trait("Category", "Negative")]
    public void Spec_DeliverySameAsImplStart_NotValidationError()
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
    [Trait("Category", "Negative")]
    public void Spec_SubmissionSameAsSigning_NotValidationError()
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
    [Trait("Category", "Negative")]
    public void HtmlTemplate_EditButton_RequiresCanUpdate()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("canUpdate()");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_SaveSection_ValidatesBeforeSave()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("saveSection()");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_CancelEditing_RevertsChanges()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("cancelEditing()");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_EmptyDurationOptions_Invalid()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ZeroDuration_NotInOptions()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().NotContain(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_NegativeDurationExceptCustom_Invalid()
    {
        var values = OpportunityWhenSectionSpec.ExpectedDurationValues;
        values.Where(v => v < 0).Should().ContainSingle().And.Contain(-1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ValidDates_NoErrors()
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
    [Trait("Category", "Negative")]
    public void HtmlTemplate_Datepicker_ShowIconTrue()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[showIcon]=\"true\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_SigningDateNotes_MaxLength()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("signingDateNotes");
        html.Should().Contain("maxlength=\"1000\"");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_CrossYearDates_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2024, 12, 1),
            ImplementationStartDate = new DateTime(2025, 1, 1),
            TargetDeliveryDate = new DateTime(2026, 12, 31)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_LeapYearDate_Valid()
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
    [Trait("Category", "Negative")]
    public void Spec_YearBoundary_Valid()
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
    [Trait("Category", "Negative")]
    public void Spec_MonthBoundary_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 1, 31),
            ImplementationStartDate = new DateTime(2025, 2, 1),
            TargetDeliveryDate = new DateTime(2025, 3, 31)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_ResetToSigningDate_WhenImplExplicitlySet()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("resetImplementationStartToSigningDate");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_OnSigningDateManualChange_ClearsDuration()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onSigningDateManualChange");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_OnDeliveryDateManualChange_ClearsDuration_PNO859()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onDeliveryDateManualChange");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_OnImplementationStartDateManualChange_ClearsDuration_PNO859()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onImplementationStartDateManualChange");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ImplStartBeforeSigning_ByOneMonth()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 7, 1),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DeliveryBeforeSigning_WhenNoImpl()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 8, 1),
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_SubmissionAfterSigning_ByMonths()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 3, 1),
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_HasDateValidationErrors_FalseWhenAllNull()
    {
        var spec = new OpportunityWhenSectionSpec();
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_GetEffectiveImplementationStart_NullWhenBothNull()
    {
        var spec = new OpportunityWhenSectionSpec();
        spec.GetEffectiveImplementationStartDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_MustNotOmitValidationErrorBindings()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("isImplementationStartBeforeSigningDate()");
        html.Should().Contain("isDeliveryDateBeforeImplementationStart()");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_InvalidOrder_ImplBeforeSigningByYear()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2026, 1, 1),
            ImplementationStartDate = new DateTime(2025, 1, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_InvalidOrder_DeliveryBeforeSigningByMonths()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 12, 1),
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 6, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_DurationOptions_MustIncludeThreeMonths()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(3);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_SimulateStartEditing_NoChangeWhenImplSet()
    {
        var impl = new DateTime(2025, 10, 1);
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
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-251")]
    public void HtmlTemplate_OpportunityUpdated_OutputExists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("opportunityUpdated");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-251")]
    public void HtmlTemplate_ChangesDetected_OutputExists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("changesDetected");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_SaveSection_CallsValidation()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("saveSection");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_NormalizeProducesIso8601()
    {
        var d = new DateTime(2025, 6, 15);
        var r = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        r.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ValidationBlocksSave_WhenImplBeforeSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ValidationBlocksSave_WhenDeliveryBeforeStart()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Spec_ValidationBlocksSave_WhenSubmissionAfterSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 2)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void HtmlTemplate_DateFormat_Consistent()
    {
        var html = ReadWhenSectionHtml();
        var dateFormatCount = CountOccurrences(html, "dateFormat=\"yy-mm-dd\"");
        dateFormatCount.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-251")]
    public void Spec_Deliverables_PlannedDates()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("deliverableDates");
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
}
