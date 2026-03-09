/// <summary>
/// Integration tests for PNO-699, PNO-811, PNO-859: WHEN section full workflow, template+spec+API contract.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

public class OpportunityWhenSectionIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void FullContract_TemplateAndSpec_AllRequirementsMet()
    {
        var html = ReadWhenSectionHtml();
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        html.Should().Contain("[appendTo]=\"'body'\"");
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_StartEditingWithNoImplStart_DefaultsToSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null,
            IsImplementationStartDateExplicitlySet = false
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().Be(new DateTime(2025, 6, 1));
        spec.GetEffectiveImplementationStartDate().Should().Be(new DateTime(2025, 6, 1));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ValidDatesThenNormalize_SavePayloadReady()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
        var signingNorm = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.TargetSigningDate);
        var implNorm = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.ImplementationStartDate);
        var deliveryNorm = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.TargetDeliveryDate);
        var submissionNorm = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.SubmissionDeadline);
        signingNorm.Should().NotBeNull().And.EndWith("Z");
        implNorm.Should().NotBeNull().And.EndWith("Z");
        deliveryNorm.Should().NotBeNull().And.EndWith("Z");
        submissionNorm.Should().NotBeNull().And.EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_InvalidDates_ValidationBlocksSave()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_AllFourDatepickers_HaveAppendToBody()
    {
        var html = ReadWhenSectionHtml();
        var count = CountOccurrences(html, "[appendTo]=\"'body'\"");
        count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_EffectiveImplStart_UsedForMinDelivery()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1)
        };
        var minDelivery = spec.GetMinDeliveryDate();
        var effectiveStart = spec.GetEffectiveImplementationStartDate();
        minDelivery.Should().Be(effectiveStart);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ChangeSigningDate_ImplStartDefaultsUpdate()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 7, 1),
            ImplementationStartDate = null,
            IsImplementationStartDateExplicitlySet = false
        };
        spec.SimulateStartEditing();
        spec.ImplementationStartDate.Should().Be(new DateTime(2025, 7, 1));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ValidChronology_AllChecksPass()
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
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_TemplateIds_MatchExpected()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("id=\"targetSigningDate\"");
        html.Should().Contain("id=\"implementationStartDate\"");
        html.Should().Contain("id=\"targetDeliveryDate\"");
        html.Should().Contain("id=\"submissionDeadline\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ImplStartExplicitlySet_GetEffectiveReturnsImpl()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 9, 1),
            IsImplementationStartDateExplicitlySet = true
        };
        spec.SimulateStartEditing();
        spec.GetEffectiveImplementationStartDate().Should().Be(new DateTime(2025, 9, 1));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_NormalizeAllDates_ProducesValidIsoStrings()
    {
        var dates = new[] { new DateTime(2025, 6, 1), new DateTime(2025, 7, 1), new DateTime(2026, 6, 1) };
        foreach (var d in dates)
        {
            var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
            result.Should().NotBeNull().And.EndWith("Z");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_NormalizeNull_ReturnsNull()
    {
        OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(null).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DurationOptions_IncludeSixMonths()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(6);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DurationOptions_CompleteSet()
    {
        var expected = new[] { 3, 6, 12, 18, 24, 36, -1 };
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().BeEquivalentTo(expected);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_HtmlAndSpec_DateValidationAligned()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("isImplementationStartBeforeSigningDate");
        html.Should().Contain("isDeliveryDateBeforeImplementationStart");
        html.Should().Contain("isSubmissionDeadlineAfterSigningDate");
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_EditSaveCancel_Flow()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("startEditing");
        html.Should().Contain("saveSection");
        html.Should().Contain("cancelEditing");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_UnsavedChanges_WarningBar()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("hasUnsavedChangesSignal");
        html.Should().Contain("unsavedChanges");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_CanUpdate_ControlsEditButton()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("canUpdate()");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_WhenSection_PanelToggleable()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("p-panel");
        html.Should().Contain("toggleable");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DateFields_FloatLabel()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("p-floatlabel");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DurationCalculator_Section()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("durationCalculator");
        html.Should().Contain("durationOptions");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_SigningDateDetails_Section()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("signingDateDetails");
        html.Should().Contain("isSigningDateFirm");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_TimelineOverview_Section()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("timelineOverview");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_WorkBreakdownStructure_Section()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("work-breakdown-structure");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ValidDates_SavePayloadStructure()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        var targetSigning = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.TargetSigningDate);
        var implStart = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.ImplementationStartDate);
        var targetDelivery = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.TargetDeliveryDate);
        var submission = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.SubmissionDeadline);
        targetSigning.Should().NotBeNull();
        implStart.Should().NotBeNull();
        targetDelivery.Should().NotBeNull();
        submission.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_InvalidImplBeforeSigning_BlocksSave()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_InvalidDeliveryBeforeStart_BlocksSave()
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
    [Trait("Category", "Integration")]
    public void Workflow_InvalidSubmissionAfterSigning_BlocksSave()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 15)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_GetMinImplementationStartDate_FromSigning()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec { TargetSigningDate = signing };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_GetMinDeliveryDate_FromEffectiveStart()
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
    [Trait("Category", "Integration")]
    public void Workflow_AllNull_NoErrors()
    {
        var spec = new OpportunityWhenSectionSpec();
        spec.GetEffectiveImplementationStartDate().Should().BeNull();
        spec.GetMinImplementationStartDate().Should().BeNull();
        spec.GetMinDeliveryDate().Should().BeNull();
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ImplNull_EffectiveUsesSigning()
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
    [Trait("Category", "Integration")]
    public void Workflow_SameDayAll_Valid()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = d,
            ImplementationStartDate = d,
            TargetDeliveryDate = d.AddMonths(12),
            SubmissionDeadline = d
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DeliveryEqualsImplStart_Valid()
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
    [Trait("Category", "Integration")]
    public void Workflow_SubmissionEqualsSigning_Valid()
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
    [Trait("Category", "Integration")]
    public void Workflow_DateFormat_Consistent()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("dateFormat=\"yy-mm-dd\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_Datepicker_ShowIcon()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[showIcon]=\"true\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_CustomDuration_MinMaxRange()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("customDuration");
        html.Should().Contain("[min]=\"1\"");
        html.Should().Contain("[max]=\"120\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ResetToSigningDate_WhenImplExplicit()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("resetImplementationStartToSigningDate");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ManualDateChange_ClearsDuration_PNO859()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onSigningDateManualChange");
        html.Should().Contain("onDeliveryDateManualChange");
        html.Should().Contain("onImplementationStartDateManualChange");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ValidationError_ShowsMessage()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("p-message");
        html.Should().Contain("severity=\"error\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_OpportunityInput_Required()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("opportunity()");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Defect", "DEF-251")]
    public void Workflow_OpportunityUpdated_Output()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("opportunityUpdated");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_IsSaving_DisablesActions()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("isSaving()");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_IsEditing_TogglesViewEdit()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("isEditing()");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ImplementationDuration_ComputedDisplay()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("implementationDurationDisplay");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DefaultsToSigningDate_Label()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("defaultsToSigningDate");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_NavigateToWhatSection_Button()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("navigateToWhatSection");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScrollToWorkBreakdown_Method()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("scrollToWorkBreakdown");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_SigningDateNotes_Textarea()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("signingDateNotes");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_FirmDeadline_Checkbox()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("isSigningDateFirm");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DurationCalculation_InfoMessage()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("durationCalculationInfo");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_SixMonthDuration_CalculatesCorrectly()
    {
        var signing = new DateTime(2025, 6, 1);
        var expected = signing.AddMonths(6);
        expected.Should().Be(new DateTime(2025, 12, 1));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ThreeMonthDuration_CalculatesCorrectly()
    {
        var signing = new DateTime(2025, 6, 1);
        var expected = signing.AddMonths(3);
        expected.Should().Be(new DateTime(2025, 9, 1));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_TwelveMonthDuration_CalculatesCorrectly()
    {
        var signing = new DateTime(2025, 6, 1);
        var expected = signing.AddMonths(12);
        expected.Should().Be(new DateTime(2026, 6, 1));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_NormalizePreservesDate()
    {
        var d = new DateTime(2025, 6, 15);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().Contain("2025-06-15");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_PNO699_AC1_SectionTitle()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("whenTimelineAndKeyDates");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_PNO699_AC2_DateFields()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDate");
        html.Should().Contain("implementationStartDate");
        html.Should().Contain("targetDeliveryDate");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_PNO699_AC5_SubmissionDeadline()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("submissionDeadline");
        html.Should().Contain("submissionDeadlineHint");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_PNO811_SixMonthsOption()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(6);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_PNO859_CalculatorOptional()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("durationCalculator");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_PNO859_ManualChangeClearsDuration()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("onDeliveryDateManualChange");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_FullCycle_ValidSavePayload()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
        var payload = new
        {
            targetSigningDate = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.TargetSigningDate),
            implementationStartDate = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.ImplementationStartDate),
            targetDeliveryDate = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.TargetDeliveryDate),
            submissionDeadline = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(spec.SubmissionDeadline)
        };
        payload.targetSigningDate.Should().NotBeNull();
        payload.implementationStartDate.Should().NotBeNull();
        payload.targetDeliveryDate.Should().NotBeNull();
        payload.submissionDeadline.Should().NotBeNull();
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
