// PNO-1210: Specification model for WHEN section date validation and template/SCSS contract.
// Mirrors opportunity-when-section.component.ts date validation logic and validates
// template/SCSS requirements (appendTo body, label overflow prevention).

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Specification model for WHEN section date validation.
/// REQ-1 through REQ-9: Datepicker appendTo body, label overflow, date ordering constraints.
/// </summary>
public sealed class WhenSectionDateSpec
{
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? ImplementationStartDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public bool IsImplementationStartDateExplicitlySet { get; set; }

    /// <summary>
    /// REQ: Implementation Start Date must be >= Target Signing Date.
    /// Mirrors isImplementationStartBeforeSigningDate from opportunity-when-section.component.ts.
    /// </summary>
    public bool IsImplementationStartBeforeSigningDate()
    {
        if (!TargetSigningDate.HasValue || !ImplementationStartDate.HasValue)
            return false;

        var signing = TargetSigningDate.Value;
        var implStart = ImplementationStartDate.Value;
        var signingTime = new DateTime(signing.Year, signing.Month, signing.Day, 0, 0, 0, DateTimeKind.Utc);
        var implStartTime = new DateTime(implStart.Year, implStart.Month, implStart.Day, 0, 0, 0, DateTimeKind.Utc);
        return implStartTime < signingTime;
    }

    /// <summary>
    /// REQ: Target Delivery Date must be >= Implementation Start (or Signing if no Impl Start).
    /// Mirrors isDeliveryDateBeforeImplementationStart from opportunity-when-section.component.ts.
    /// </summary>
    public bool IsDeliveryDateBeforeImplementationStart()
    {
        var effectiveStart = ImplementationStartDate ?? TargetSigningDate;
        if (!effectiveStart.HasValue || !TargetDeliveryDate.HasValue)
            return false;

        var start = effectiveStart.Value;
        var delivery = TargetDeliveryDate.Value;
        var startTime = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0, DateTimeKind.Utc);
        var deliveryTime = new DateTime(delivery.Year, delivery.Month, delivery.Day, 0, 0, 0, DateTimeKind.Utc);
        return deliveryTime < startTime;
    }

    /// <summary>
    /// REQ: Submission Deadline must be <= Target Signing Date.
    /// Mirrors isSubmissionDeadlineAfterSigningDate from opportunity-when-section.component.ts.
    /// </summary>
    public bool IsSubmissionDeadlineAfterSigningDate()
    {
        if (!TargetSigningDate.HasValue || !SubmissionDeadline.HasValue)
            return false;

        var signing = TargetSigningDate.Value;
        var submission = SubmissionDeadline.Value;
        var signingTime = new DateTime(signing.Year, signing.Month, signing.Day, 0, 0, 0, DateTimeKind.Utc);
        var submissionTime = new DateTime(submission.Year, submission.Month, submission.Day, 0, 0, 0, DateTimeKind.Utc);
        return submissionTime > signingTime;
    }

    /// <summary>
    /// REQ: Effective implementation start = ImplementationStartDate ?? TargetSigningDate.
    /// Mirrors effectiveImplementationStartDate from opportunity-when-section.component.ts.
    /// </summary>
    public DateTime? GetEffectiveImplementationStartDate()
    {
        return ImplementationStartDate ?? TargetSigningDate;
    }

    /// <summary>
    /// REQ: Min implementation start date = TargetSigningDate.
    /// Mirrors getMinImplementationStartDate from opportunity-when-section.component.ts.
    /// </summary>
    public DateTime? GetMinImplementationStartDate()
    {
        return TargetSigningDate;
    }

    /// <summary>
    /// REQ: Min delivery date = ImplementationStartDate ?? TargetSigningDate.
    /// Mirrors getMinDeliveryDate from opportunity-when-section.component.ts.
    /// </summary>
    public DateTime? GetMinDeliveryDate()
    {
        return ImplementationStartDate ?? TargetSigningDate;
    }

    /// <summary>
    /// REQ: Normalize date to UTC midnight (T00:00:00Z).
    /// Mirrors normalizeDateToUTCMidnight from opportunity-when-section.component.ts.
    /// </summary>
    public static string? NormalizeDateToUTCMidnight(DateTime? date)
    {
        if (!date.HasValue)
            return null;

        var d = date.Value;
        var utcDate = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
        return utcDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    /// <summary>
    /// Returns true if any date validation error exists.
    /// </summary>
    public bool HasDateValidationErrors()
    {
        return IsImplementationStartBeforeSigningDate()
            || IsDeliveryDateBeforeImplementationStart()
            || IsSubmissionDeadlineAfterSigningDate();
    }

    /// <summary>
    /// Simulates start editing: defaults implementation start to signing if not explicitly set.
    /// </summary>
    public void SimulateStartEditing()
    {
        if (!IsImplementationStartDateExplicitlySet && TargetSigningDate.HasValue && !ImplementationStartDate.HasValue)
        {
            ImplementationStartDate = TargetSigningDate;
        }
    }
}
