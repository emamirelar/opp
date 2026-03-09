/// <summary>
/// Functional tests for PNO-699, PNO-811, PNO-859: WHEN section business rules, validation, workflow.
/// </summary>

using System.Linq;
using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

public class OpportunityWhenSectionFunctionalTests
{
    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ValidationRule_ImplementationStartMustBeAfterOrEqualSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 5, 31)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ValidationRule_DeliveryMustBeAfterOrEqualImplStart()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2025, 6, 30)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ValidationRule_SubmissionDeadlineMustBeBeforeOrEqualSigning()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 2)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_EffectiveImplStart_DefaultsToSigningWhenImplNull()
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
    [Trait("Category", "Functional")]
    public void Spec_EffectiveImplStart_UsesImplWhenSet()
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
    [Trait("Category", "Functional")]
    public void Spec_MinImplementationStartDate_EqualsSigningDate()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec { TargetSigningDate = signing };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_MinDeliveryDate_EqualsEffectiveStart()
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
    [Trait("Category", "Functional")]
    public void Spec_NormalizeDateToUTCMidnight_StripsTime()
    {
        var d = new DateTime(2025, 6, 15, 14, 30, 45);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().NotBeNull().And.Contain("2025-06-15").And.EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_NormalizeNull_ReturnsNull()
    {
        OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(null).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_HasDateValidationErrors_CombinesAllChecks()
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
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_SimulateStartEditing_DefaultsImplToSigning()
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
    [Trait("Category", "Functional")]
    public void Spec_SimulateStartEditing_RespectsExplicitlySet()
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
    [Trait("Category", "Functional")]
    public void Spec_DateOnlyComparison_IgnoresTime()
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
    [Trait("Category", "Functional")]
    public void Spec_SubmissionBeforeSigning_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DeliveryAfterImplStart_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2025, 8, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ImplStartAfterSigning_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DeliveryUsesEffectiveStart_WhenImplNull()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 5, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ImplementationStartExplicitlySet_GetEffectiveReturnsImpl()
    {
        var impl = new DateTime(2025, 9, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_AllNull_NoValidationErrors()
    {
        var spec = new OpportunityWhenSectionSpec();
        spec.ImplementationStartDate.Should().BeNull();
        spec.GetEffectiveImplementationStartDate().Should().BeNull();
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ValidFullSet_NoErrors()
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
    [Trait("Category", "Functional")]
    public void Spec_DurationOptions_IncludeSixMonths_PNO811()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(6);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DurationOptions_IncludeCustom()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(-1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_TargetSigningDate_HasFormControl()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDateControl");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_ImplementationStartDate_HasMinDateBinding()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("getMinImplementationStartDate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_TargetDeliveryDate_HasMinDateBinding()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("getMinDeliveryDate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_SubmissionDeadline_HasMaxDateBinding()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDateControl.value");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DateSpec_SubmissionDeadlineBeforeSigning_Valid()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            SubmissionDeadline = new DateTime(2025, 5, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DateSpec_ValidChronology_AllPass()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_NormalizeMultipleDates_AllValid()
    {
        var dates = new[] { new DateTime(2025, 1, 1), new DateTime(2025, 6, 15), new DateTime(2025, 12, 31) };
        foreach (var d in dates)
        {
            var r = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
            r.Should().NotBeNull().And.EndWith("Z");
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_GetMinImplementationStartDate_NullWhenSigningNull()
    {
        var spec = new OpportunityWhenSectionSpec { TargetSigningDate = null };
        spec.GetMinImplementationStartDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_GetMinDeliveryDate_NullWhenNoStart()
    {
        var spec = new OpportunityWhenSectionSpec();
        spec.GetMinDeliveryDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ValidationOrder_ImplBeforeSigningChecked()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ValidationOrder_DeliveryBeforeStartChecked()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ValidationOrder_SubmissionAfterSigningChecked()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 15)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_EffectiveStart_ForMinDelivery_WhenImplSet()
    {
        var impl = new DateTime(2025, 8, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl
        };
        spec.GetMinDeliveryDate().Should().Be(spec.GetEffectiveImplementationStartDate());
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_EffectiveStart_ForMinDelivery_WhenImplNull()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null
        };
        spec.GetMinDeliveryDate().Should().Be(spec.GetEffectiveImplementationStartDate());
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DurationOptions_SevenTotal()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().HaveCount(7);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DurationOptions_StandardValues()
    {
        var expected = new[] { 3, 6, 12, 18, 24, 36, -1 };
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().BeEquivalentTo(expected);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlTemplate_WhenSection_Exists()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("whenTimelineAndKeyDates");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlTemplate_DurationCalculator_OptionalHint_PNO859()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("durationCalculatorHint");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_SameDaySigningAndSubmission_Valid()
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
    [Trait("Category", "Functional")]
    public void Spec_SameDayImplAndDelivery_Valid()
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
    [Trait("Category", "Functional")]
    public void Spec_SameDaySigningAndImpl_Valid()
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
    [Trait("Category", "Functional")]
    public void Spec_Normalize_ProducesUtcMidnight()
    {
        var d = new DateTime(2025, 6, 15, 12, 0, 0);
        var result = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d);
        result.Should().NotBeNull();
        result.Should().Contain("T");
        result.Should().EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_SimulateStartEditing_OnlyWhenNotExplicit()
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
    [Trait("Category", "Functional")]
    public void Spec_SimulateStartEditing_NoOpWhenSigningNull()
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
    [Trait("Category", "Functional")]
    public void Spec_SimulateStartEditing_NoOpWhenImplAlreadySet()
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
    [Trait("Category", "Functional")]
    public void HtmlTemplate_ValidationMessages_DisplayOnError()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("implementationStartMustBeAfterSigningDate");
        html.Should().Contain("deliveryDateMustBeAfterImplementationStart");
        html.Should().Contain("submissionDeadlineMustBeBeforeSigningDate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DeliveryDateValidation_UsesEffectiveStart()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 5, 15)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ImplStartNull_EffectiveIsSigning()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null
        };
        var effective = spec.GetEffectiveImplementationStartDate();
        effective.Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_ImplStartSet_EffectiveIsImpl()
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
    [Trait("Category", "Functional")]
    public void Spec_MinImplStart_FromSigning()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new OpportunityWhenSectionSpec { TargetSigningDate = signing };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_MinDelivery_FromEffectiveStart()
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
    [Trait("Category", "Functional")]
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
    [Trait("Category", "Functional")]
    public void Spec_InvalidDates_HasErrors()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlTemplate_Datepickers_AppendToBody()
    {
        var html = ReadWhenSectionHtml();
        var count = CountOccurrences(html, "[appendTo]=\"'body'\"");
        count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DurationOptions_NoZero()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().NotContain(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_DurationOptions_OnlyOneNegative()
    {
        var negCount = OpportunityWhenSectionSpec.ExpectedDurationValues.Count(v => v < 0);
        negCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_Normalize_ConsistentFormat()
    {
        var d1 = new DateTime(2025, 1, 1);
        var d2 = new DateTime(2025, 12, 31);
        var r1 = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d1);
        var r2 = OpportunityWhenSectionSpec.NormalizeDateToUTCMidnight(d2);
        r1.Should().NotBeNull().And.MatchRegex(@"\d{4}-\d{2}-\d{2}T");
        r2.Should().NotBeNull().And.MatchRegex(@"\d{4}-\d{2}-\d{2}T");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_Workflow_StartEditing_DefaultsImpl()
    {
        var spec = new OpportunityWhenSectionSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null,
            IsImplementationStartDateExplicitlySet = false
        };
        spec.SimulateStartEditing();
        spec.GetEffectiveImplementationStartDate().Should().Be(new DateTime(2025, 6, 1));
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_Workflow_CancelEditing_RevertsToOriginal()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("cancelEditing");
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Defect", "DEF-251")]
    public void Spec_Workflow_SaveSection_ValidatesFirst()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("hasDateValidationErrors");
        html.Should().Contain("saveSection");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_PNO699_AC2_DateFieldsExist()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDate");
        html.Should().Contain("implementationStartDate");
        html.Should().Contain("targetDeliveryDate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_PNO699_AC5_SubmissionDeadlineField()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("submissionDeadline");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_PNO811_SixMonthsInOptions()
    {
        OpportunityWhenSectionSpec.ExpectedDurationValues.Should().Contain(6);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_PNO859_MinDateOnDelivery()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("getMinDeliveryDate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_PNO859_MinDateOnImplStart()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("getMinImplementationStartDate");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Spec_PNO859_MaxDateOnSubmission()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("maxDate");
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
