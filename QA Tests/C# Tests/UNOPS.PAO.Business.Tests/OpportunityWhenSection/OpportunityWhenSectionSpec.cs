// PNO-699, PNO-811, PNO-859: Consolidated specification for WHEN - Timeline & Key Dates section.
// Requirements: Section exists, date fields (Target Signing, Implementation Start, Target Delivery, Submission Deadline),
// Duration options including 6 months (PNO-811), date validation (start before end, submission <= signing),
// Data persistence, date picker UX (appendTo body, floating labels).

namespace UNOPS.PAO.Business.Tests.OpportunityWhenSection;

/// <summary>
/// Specification model for Opportunity WHEN section.
/// PNO-699 AC1-AC5: Timeline & Key Dates, date fields, validation, signing date details.
/// PNO-811: 6-month duration option.
/// PNO-859: Date validation (end before start greyed out), duration calculator optional, manual date change clears duration.
/// </summary>
public sealed class OpportunityWhenSectionSpec
{
    public DateTime? TargetSigningDate { get; set; }
    public DateTime? ImplementationStartDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public bool IsImplementationStartDateExplicitlySet { get; set; }

    /// <summary>
    /// PNO-699 AC5, PNO-859: Submission Deadline must be &lt;= Target Signing Date.
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
    /// PNO-859: Delivery date must be &gt;= Implementation Start (or Signing if no Impl Start).
    /// End date cannot be before start date.
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
    /// PNO-699 AC5: Submission Deadline must be &lt;= Target Signing Date.
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
    /// PNO-699 AC2: Effective implementation start = ImplementationStartDate ?? TargetSigningDate.
    /// </summary>
    public DateTime? GetEffectiveImplementationStartDate()
    {
        return ImplementationStartDate ?? TargetSigningDate;
    }

    /// <summary>
    /// PNO-859: Min implementation start date = TargetSigningDate.
    /// </summary>
    public DateTime? GetMinImplementationStartDate()
    {
        return TargetSigningDate;
    }

    /// <summary>
    /// PNO-859: Min delivery date = ImplementationStartDate ?? TargetSigningDate.
    /// </summary>
    public DateTime? GetMinDeliveryDate()
    {
        return ImplementationStartDate ?? TargetSigningDate;
    }

    /// <summary>
    /// Normalize date to UTC midnight (T00:00:00Z) for API persistence.
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
    /// PNO-699 AC2: Simulates start editing - defaults implementation start to signing if not explicitly set.
    /// </summary>
    public void SimulateStartEditing()
    {
        if (!IsImplementationStartDateExplicitlySet && TargetSigningDate.HasValue && !ImplementationStartDate.HasValue)
        {
            ImplementationStartDate = TargetSigningDate;
        }
    }

    /// <summary>
    /// PNO-811: Duration options must include 6 months.
    /// Expected: 3, 6, 12, 18, 24, 36, Custom (-1).
    /// </summary>
    public static readonly int[] ExpectedDurationValues = { 3, 6, 12, 18, 24, 36, -1 };
}
