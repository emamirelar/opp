using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Functional tests for PNO-1210: Business rules, validation logic, state transitions.
/// </summary>
public class PNO1210FunctionalTests
{
    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_ValidationRule_ImplementationStartMustBeAfterOrEqualSigning()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 5, 31)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_ValidationRule_DeliveryMustBeAfterOrEqualEffectiveStart()
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
    [Trait("Category", "Functional")]
    public void DateSpec_ValidationRule_SubmissionDeadlineMustBeBeforeOrEqualSigning()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            SubmissionDeadline = new DateTime(2025, 6, 2)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_EffectiveStartFallback_WhenImplNull_UsesSigning()
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
    [Trait("Category", "Functional")]
    public void DateSpec_EffectiveStartFallback_WhenImplSet_UsesImpl()
    {
        var impl = new DateTime(2025, 7, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_MinImplementationStartDate_EqualsSigningDate()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec { TargetSigningDate = signing };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_MinDeliveryDate_WhenImplSet_EqualsImplStart()
    {
        var impl = new DateTime(2025, 7, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl
        };
        spec.GetMinDeliveryDate().Should().Be(impl);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_MinDeliveryDate_WhenImplNull_EqualsSigningDate()
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
    [Trait("Category", "Functional")]
    public void DateSpec_NormalizeDateToUTCMidnight_StripsTimeComponent()
    {
        var date = new DateTime(2025, 3, 9, 14, 30, 45);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().NotBeNull();
        result.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T00:00:00");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_HasDateValidationErrors_AggregatesAllThreeChecks()
    {
        var spec = new WhenSectionDateSpec
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
    public void DateSpec_SimulateStartEditing_DefaultsImplToSigningWhenNotExplicit()
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
    [Trait("Category", "Functional")]
    public void DateSpec_SimulateStartEditing_DoesNotOverrideWhenExplicitlySet()
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
    [Trait("Category", "Functional")]
    public void TemplateContract_AllDatepickersHaveAppendToBody()
    {
        var html = ReadWhenSectionHtml();
        var count = CountOccurrences(html, "[appendTo]=\"'body'\"");
        count.Should().BeGreaterOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_LabelOverflowPrevention_Enforced()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("max-width");
        scss.Should().Contain("text-overflow: ellipsis");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_DateOnlyComparison_IgnoresTime()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1, 0, 0, 0),
            ImplementationStartDate = new DateTime(2025, 6, 1, 23, 59, 59)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_ValidChronology_SigningImplDelivery_NoErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_ValidChronology_SubmissionBeforeSigning_NoErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            SubmissionDeadline = new DateTime(2025, 6, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_SavePayloadStructure_EffectiveImplDefaultsToSigning()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = null
        };
        var effectiveImpl = spec.GetEffectiveImplementationStartDate();
        effectiveImpl.Should().Be(signing);
        var normalized = WhenSectionDateSpec.NormalizeDateToUTCMidnight(effectiveImpl);
        normalized.Should().NotBeNull();
        normalized.Should().EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_UtcMidnightFormat_EndsWithZ()
    {
        var date = new DateTime(2025, 3, 9);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_NullInputs_NoValidationErrors()
    {
        var spec = new WhenSectionDateSpec();
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_PartialInputs_OnlyValidatesWhenBothPresent()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_FilledAndFocusStates_HaveDistinctMaxWidth()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("3.5rem");
        scss.Should().Contain("3rem");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HtmlContract_DatepickersUseFloatLabel()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("p-floatlabel");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_ImplementationStartExplicitlySet_GetEffectiveReturnsImpl()
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
    [Trait("Category", "Functional")]
    public void DateSpec_SimulateStartEditing_WhenSigningNull_NoDefault()
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
    [Trait("Category", "Functional")]
    public void DateSpec_SimulateStartEditing_WhenImplAlreadySet_NoOverride()
    {
        var impl = new DateTime(2025, 9, 1);
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
    [Trait("Category", "Functional")]
    public void DateSpec_ValidFullPayload_NormalizesAllDates()
    {
        var signing = new DateTime(2025, 6, 1);
        var impl = new DateTime(2025, 7, 1);
        var delivery = new DateTime(2026, 6, 1);
        var submission = new DateTime(2025, 5, 15);

        var normSigning = WhenSectionDateSpec.NormalizeDateToUTCMidnight(signing);
        var normImpl = WhenSectionDateSpec.NormalizeDateToUTCMidnight(impl);
        var normDelivery = WhenSectionDateSpec.NormalizeDateToUTCMidnight(delivery);
        var normSubmission = WhenSectionDateSpec.NormalizeDateToUTCMidnight(submission);

        normSigning.Should().NotBeNull().And.EndWith("Z");
        normImpl.Should().NotBeNull().And.EndWith("Z");
        normDelivery.Should().NotBeNull().And.EndWith("Z");
        normSubmission.Should().NotBeNull().And.EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_WhiteSpaceNowrap_PreventsLabelWrap()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("white-space: nowrap");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_OverflowHidden_ClipsLongLabels()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("overflow: hidden");
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
    public void DateSpec_ChronologicalOrder_SigningBeforeImplBeforeDelivery()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_SubmissionDeadlineBeforeSigning_Valid()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 15),
            SubmissionDeadline = new DateTime(2025, 5, 1)
        };
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_NormalizeNull_ReturnsNull()
    {
        WhenSectionDateSpec.NormalizeDateToUTCMidnight(null).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_ChronologicalOrder_ImplBeforeDelivery_Valid()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2025, 8, 1)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_EffectiveStartFallback_WhenBothSet_PrefersImpl()
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
    [Trait("Category", "Functional")]
    public void DateSpec_ValidationOrder_SigningImplDelivery()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeFalse();
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_SimulateStartEditing_WhenImplSetExplicitly_NoDefault()
    {
        var impl = new DateTime(2025, 9, 1);
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
    [Trait("Category", "Functional")]
    public void DateSpec_GetMinDeliveryDate_BothNull_ReturnsNull()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = null,
            ImplementationStartDate = null
        };
        spec.GetMinDeliveryDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_GetMinImplementationStartDate_NullSigning_ReturnsNull()
    {
        var spec = new WhenSectionDateSpec { TargetSigningDate = null };
        spec.GetMinImplementationStartDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_BackgroundColorWhite_ForFilledLabel()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("background-color: white");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ScssContract_PaddingForFilledLabel()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void DateSpec_AllValidationMethods_ReturnBoolean()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 5, 1)
        };
        spec.IsImplementationStartBeforeSigningDate().Should().BeTrue();
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
        spec.IsSubmissionDeadlineAfterSigningDate().Should().BeFalse();
        spec.HasDateValidationErrors().Should().BeTrue();
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
