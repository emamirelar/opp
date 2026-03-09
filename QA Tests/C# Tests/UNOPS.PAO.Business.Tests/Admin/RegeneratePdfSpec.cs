/**
 * @fileoverview PNO-1166: QA testing code — RegenerateGoOpportunityPdfs specification.
 * Defines constants and expected behavior for the SystemAdminController endpoint.
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.Admin;

/// <summary>
/// Specification for RegenerateGoOpportunityPdfs endpoint (PNO-1166).
/// AC-3: PDF generation for GO opportunities works correctly.
/// </summary>
public static class PNO1166RegeneratePdfSpec
{
    /// <summary>
    /// Endpoint path: POST /api/system-admin/regenerate-go-opportunity-pdfs
    /// </summary>
    public const string EndpointPath = "/api/system-admin/regenerate-go-opportunity-pdfs";

    /// <summary>
    /// Required permission for the endpoint.
    /// </summary>
    public const string RequiredPermission = "CanRunSeedings";

    /// <summary>
    /// Default value for onlyMissing query parameter.
    /// </summary>
    public const bool DefaultOnlyMissing = true;

    /// <summary>
    /// Expected response property names (camelCase).
    /// </summary>
    public static class ResponseProperties
    {
        public const string Message = "message";
        public const string TotalProcessed = "totalProcessed";
        public const string SubmissionSuccess = "submissionSuccess";
        public const string SubmissionFailed = "submissionFailed";
        public const string SubmissionSkipped = "submissionSkipped";
        public const string ApprovalSuccess = "approvalSuccess";
        public const string ApprovalFailed = "approvalFailed";
        public const string ApprovalSkipped = "approvalSkipped";
        public const string Results = "results";
    }

    /// <summary>
    /// Submission PDF filename format: Opportunity_{id}_Submission_{yyyyMMdd}_{HHmm}
    /// </summary>
    public static string SubmissionFilenameFormat(int opportunityId, string dateStr, string timeStr) =>
        $"Opportunity_{opportunityId}_Submission_{dateStr}_{timeStr}";

    /// <summary>
    /// Approval PDF filename format: Opportunity_{id}_Approved_{yyyyMMdd}
    /// </summary>
    public static string ApprovalFilenameFormat(int opportunityId, string dateStr) =>
        $"Opportunity_{opportunityId}_Approved_{dateStr}";

    /// <summary>
    /// Document type filter: EntityType="Opportunity", Name="Opportunity Statement"
    /// </summary>
    public const string StatementDocTypeEntityType = "Opportunity";
    public const string StatementDocTypeName = "Opportunity Statement";

    /// <summary>
    /// Stage filter: only opportunities in Stage=GO.
    /// </summary>
    public const string GoStage = "GO";

    /// <summary>
    /// Submission document name contains this substring.
    /// </summary>
    public const string SubmissionDocNameContains = "_Submission_";

    /// <summary>
    /// Approval document name contains this substring.
    /// </summary>
    public const string ApprovalDocNameContains = "_Approved_";
}
