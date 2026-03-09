using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Integration tests for PNO-1210: Full workflow cycles, template+SCSS+spec contract validation.
/// </summary>
public class PNO1210IntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void FullContract_TemplateAndScssAndSpec_AllRequirementsMet()
    {
        var html = ReadWhenSectionHtml();
        var scss = ReadWhenSectionScss();
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };

        html.Should().Contain("[appendTo]=\"'body'\"");
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("max-width: calc(100% - 3.5rem)");
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_StartEditingWithNoImplStart_DefaultsToSigning()
    {
        var spec = new WhenSectionDateSpec
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
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        spec.HasDateValidationErrors().Should().BeFalse();

        var signingNorm = WhenSectionDateSpec.NormalizeDateToUTCMidnight(spec.TargetSigningDate);
        var implNorm = WhenSectionDateSpec.NormalizeDateToUTCMidnight(spec.ImplementationStartDate);
        var deliveryNorm = WhenSectionDateSpec.NormalizeDateToUTCMidnight(spec.TargetDeliveryDate);
        var submissionNorm = WhenSectionDateSpec.NormalizeDateToUTCMidnight(spec.SubmissionDeadline);

        signingNorm.Should().NotBeNull().And.EndWith("Z");
        implNorm.Should().NotBeNull().And.EndWith("Z");
        deliveryNorm.Should().NotBeNull().And.EndWith("Z");
        submissionNorm.Should().NotBeNull().And.EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_InvalidDates_ValidationBlocksSave()
    {
        var spec = new WhenSectionDateSpec
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
    public void Workflow_ScssLabelRules_CompleteSet()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("max-width: calc(100% - 3.5rem)");
        scss.Should().Contain("overflow: hidden");
        scss.Should().Contain("text-overflow: ellipsis");
        scss.Should().Contain("white-space: nowrap");
        scss.Should().Contain("max-width: calc(100% - 3rem)");
        scss.Should().Contain("background-color: white");
        scss.Should().Contain("padding: 0 0.25rem");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_EffectiveImplStart_UsedForMinDelivery()
    {
        var spec = new WhenSectionDateSpec
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
        var spec = new WhenSectionDateSpec
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
        var spec = new WhenSectionDateSpec
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
        var spec = new WhenSectionDateSpec
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
        var dates = new[]
        {
            new DateTime(2025, 6, 1),
            new DateTime(2025, 7, 1),
            new DateTime(2026, 6, 1),
            new DateTime(2025, 5, 15)
        };
        foreach (var d in dates)
        {
            var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(d);
            result.Should().NotBeNull();
            result.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
            result.Should().EndWith("Z");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_SubmissionAfterSigning_ValidationFails()
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
    [Trait("Category", "Integration")]
    public void Workflow_DeliveryBeforeImplStart_ValidationFails()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1),
            TargetDeliveryDate = new DateTime(2025, 7, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssAndTemplate_ConsistentWithPNO1210Fix()
    {
        var html = ReadWhenSectionHtml();
        var scss = ReadWhenSectionScss();
        html.Should().Contain("[appendTo]=\"'body'\"");
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("text-overflow: ellipsis");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_SameDayAllDates_Valid()
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
    [Trait("Category", "Integration")]
    public void Workflow_GetMinDeliveryDate_FallbackToSigning()
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
    [Trait("Category", "Integration")]
    public void Workflow_GetMinImplementationStartDate_FromSigning()
    {
        var signing = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec { TargetSigningDate = signing };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_NullDates_NoValidationErrors()
    {
        var spec = new WhenSectionDateSpec();
        spec.HasDateValidationErrors().Should().BeFalse();
        spec.GetEffectiveImplementationStartDate().Should().BeNull();
        spec.GetMinImplementationStartDate().Should().BeNull();
        spec.GetMinDeliveryDate().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_SimulateStartEditing_WhenImplNullSigningSet_SetsImpl()
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
    [Trait("Category", "Integration")]
    public void Workflow_SimulateStartEditing_WhenImplSet_NoChange()
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
    [Trait("Category", "Integration")]
    public void Workflow_DatepickerIds_InTemplate()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("targetSigningDate");
        html.Should().Contain("implementationStartDate");
        html.Should().Contain("targetDeliveryDate");
        html.Should().Contain("submissionDeadline");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssPDatepickerScope()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(".p-datepicker");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ValidPayload_NormalizeProducesIso()
    {
        var date = new DateTime(2025, 3, 9, 14, 30, 0);
        var result = WhenSectionDateSpec.NormalizeDateToUTCMidnight(date);
        result.Should().NotBeNull();
        result.Should().StartWith("2025-03-09");
        result.Should().EndWith("Z");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ImplStartBeforeSigning_GetMinImplStillReturnsSigning()
    {
        var signing = new DateTime(2025, 6, 15);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = signing,
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.GetMinImplementationStartDate().Should().Be(signing);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_EffectiveStart_UsedForDeliveryValidation()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 6, 15)
        };
        spec.GetEffectiveImplementationStartDate().Should().Be(new DateTime(2025, 6, 1));
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ThreeValidationErrors_AllReported()
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
    [Trait("Category", "Integration")]
    public void Workflow_HtmlDateFormat_YyMmDd()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("dateFormat=\"yy-mm-dd\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssHostNgDeep_Scoped()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain(":host");
        scss.Should().Contain("::ng-deep");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_FullValidCycle_NoErrors()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };
        spec.SimulateStartEditing();
        spec.HasDateValidationErrors().Should().BeFalse();
        var effective = spec.GetEffectiveImplementationStartDate();
        effective.Should().Be(new DateTime(2025, 7, 1));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_NormalizeNull_ReturnsNull()
    {
        WhenSectionDateSpec.NormalizeDateToUTCMidnight(null).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssFilledFocus_MaxWidthThreeRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("calc(100% - 3rem)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssDefaultLabel_MaxWidthThreePointFiveRem()
    {
        var scss = ReadWhenSectionScss();
        scss.Should().Contain("calc(100% - 3.5rem)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_AllDatepickers_ShowIconTrue()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[showIcon]=\"true\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ImplStartEqualsSigning_Valid()
    {
        var d = new DateTime(2025, 6, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = d,
            ImplementationStartDate = d,
            TargetDeliveryDate = d.AddMonths(12)
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DeliveryEqualsImplStart_Valid()
    {
        var impl = new DateTime(2025, 7, 1);
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = impl,
            TargetDeliveryDate = impl
        };
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_SubmissionEqualsSigning_Valid()
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
    [Trait("Category", "Integration")]
    public void Workflow_PNO1210_CompleteContractValidation()
    {
        var html = ReadWhenSectionHtml();
        var scss = ReadWhenSectionScss();
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1),
            SubmissionDeadline = new DateTime(2025, 5, 15)
        };

        CountOccurrences(html, "[appendTo]=\"'body'\"").Should().BeGreaterOrEqualTo(4);
        scss.Should().Contain(".p-datepicker ~ label");
        scss.Should().Contain("max-width: calc(100% - 3.5rem)");
        scss.Should().Contain("text-overflow: ellipsis");
        spec.HasDateValidationErrors().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ValidDatesThenSimulateStartEditing_ImplDefaults()
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
    [Trait("Category", "Integration")]
    public void Workflow_InvalidImplBeforeSigning_ValidationFails()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 7, 1),
            ImplementationStartDate = new DateTime(2025, 6, 1)
        };
        spec.HasDateValidationErrors().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_DeliveryBeforeEffectiveStart_ValidationFails()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = null,
            TargetDeliveryDate = new DateTime(2025, 5, 1)
        };
        spec.IsDeliveryDateBeforeImplementationStart().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_ScssAndHtml_AppendToBodyPresent()
    {
        var html = ReadWhenSectionHtml();
        html.Should().Contain("[appendTo]=\"'body'\"");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_GetMinDeliveryDate_WhenImplSet_ReturnsImpl()
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
    [Trait("Category", "Integration")]
    public void Workflow_NormalizeMultipleDates_AllValid()
    {
        var dates = new[] { new DateTime(2025, 1, 1), new DateTime(2025, 6, 15), new DateTime(2025, 12, 31) };
        foreach (var d in dates)
        {
            var r = WhenSectionDateSpec.NormalizeDateToUTCMidnight(d);
            r.Should().NotBeNull().And.EndWith("Z");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Workflow_EffectiveStart_UsedForMinDelivery()
    {
        var spec = new WhenSectionDateSpec
        {
            TargetSigningDate = new DateTime(2025, 6, 1),
            ImplementationStartDate = new DateTime(2025, 8, 1)
        };
        spec.GetMinDeliveryDate().Should().Be(spec.GetEffectiveImplementationStartDate());
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
