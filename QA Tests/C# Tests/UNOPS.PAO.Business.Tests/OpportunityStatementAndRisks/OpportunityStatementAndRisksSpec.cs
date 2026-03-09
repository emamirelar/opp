/**
 * @fileoverview Opportunity Statement & Risk Register — Consolidated Specification
 * PNO-705, PNO-761, PNO-922, PNO-975
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.OpportunityStatementAndRisks;

/// <summary>
/// Specification and requirement traceability for Opportunity Statement and Risk Register.
///
/// JIRA Tickets:
/// - PNO-705: Opportunity Statement (auto-generated markdown, PDF, Go Decision workflow)
/// - PNO-761: Risk Register (AI suggestions, risk structure, scoring)
/// - PNO-922: Missing Edit option for existing Risks
/// - PNO-975: Risk popup visibility (z-index, modal behavior)
///
/// Requirements validated:
/// - PNO-705 AC1: "Opportunity Statement" tab exists; message when sections incomplete
/// - PNO-705 AC2: "Generate Opportunity Statement" option when all sections complete
/// - PNO-705 AC3: Structured data feeds template; AI generates document
/// - PNO-705 AC4: Document editable until Submit for Go decision
/// - PNO-705 AC5: Final document matched to record to flag disparities
/// - PNO-761 AC1: Indicate which Organisational High Risks apply; acknowledge all reviewed
/// - PNO-761 AC2: Create risks from scratch; fields align with oUP
/// - PNO-761 AC3: AI-recommended risks easily added to register
/// - PNO-761 AC4: Flag strength of case for high risks; user must intentionally add
/// - PNO-922: Edit option (pencil icon) for existing risks
/// - PNO-975: Add new risk popup visible (z-index above header)
/// - Navigation pane shows "Risks" (not "DST")
/// </summary>
public static class OpportunityStatementAndRisksSpec
{
    #region Opportunity Statement (PNO-705)

    /// <summary>Entity name for Opportunity in workflow/API</summary>
    public const string OpportunityEntityName = "Opportunity";

    /// <summary>Document type name for Opportunity Statement PDF</summary>
    public const string OpportunityStatementDocumentType = "Opportunity Statement";

    /// <summary>Default filename when generating statement PDF</summary>
    public const string DefaultStatementFilename = "Generated_Document";

    /// <summary>Message when sections incomplete (AC1)</summary>
    public const string IncompleteSectionsMessage = "Complete all sections of the Opportunity in order to generate the Opportunity Statement";

    #endregion

    #region Risk Register (PNO-761, PNO-922, PNO-975)

    /// <summary>Entity type for Opportunity risks</summary>
    public const string RiskEntityTypeOpportunity = "Opportunity";

    /// <summary>Navigation pane label for Risks (not DST)</summary>
    public const string RisksNavigationLabel = "Risks";

    /// <summary>Risk type: Threat</summary>
    public const string RiskTypeThreat = "Threat";

    /// <summary>Risk type: Opportunity</summary>
    public const string RiskTypeOpportunity = "Opportunity";

    /// <summary>Minimum mandatory fields for manual risk: Title</summary>
    public const int RiskTitleMinLength = 1;

    /// <summary>Risk scoring: likelihood × impact = risk score (matrix)</summary>
    public const string RiskScoringFormula = "Likelihood × Impact = Risk Score";

    /// <summary>High risk acknowledgement field on Opportunity</summary>
    public const string HighRisksAcknowledgedField = "HighRisksAcknowledged";

    #endregion

    #region API Endpoints

    /// <summary>Generate statement PDF endpoint</summary>
    public const string GenerateStatementPdfEndpoint = "/api/opportunity/generate-statement-pdf";

    /// <summary>Generate statement (AI) endpoint pattern</summary>
    public static string GenerateStatementEndpoint(int id) => $"/api/opportunity/{id}/generate-statement";

    /// <summary>Validate statement endpoint pattern</summary>
    public static string ValidateStatementEndpoint(int id) => $"/api/opportunity/{id}/validate-statement";

    /// <summary>DST risks endpoint pattern</summary>
    public static string DstRisksEndpoint(int opportunityId) => $"/api/opportunity/{opportunityId}/dst-risks";

    /// <summary>Risk update endpoint pattern</summary>
    public static string UpdateRiskEndpoint(int opportunityId, int riskId) => $"/api/opportunity/{opportunityId}/dst-risks/{riskId}";

    /// <summary>Risk delete endpoint pattern</summary>
    public static string DeleteRiskEndpoint(int opportunityId, int riskId) => $"/api/opportunity/{opportunityId}/dst-risks/{riskId}";

    /// <summary>High risk analysis endpoint pattern</summary>
    public static string HighRiskAnalysisEndpoint(int opportunityId) => $"/api/opportunity/{opportunityId}/high-risk-analysis";

    /// <summary>Acknowledge high risks endpoint pattern</summary>
    public static string AcknowledgeHighRisksEndpoint(int opportunityId) => $"/api/opportunity/{opportunityId}/acknowledge-high-risks";

    #endregion
}
