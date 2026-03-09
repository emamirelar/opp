/**
 * @fileoverview Opportunity UX & Layout specification — PNO-769, PNO-862, PNO-863, PNO-871, PNO-876, PNO-877, PNO-882.
 * Defines constants, endpoints, and expected behavior for Opportunity record display, header, key info, quick stats, comments, and navigation.
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.OpportunityUXAndLayout;

/// <summary>
/// Specification for Opportunity UX & Layout (PNO-769, PNO-862, PNO-863, PNO-871, PNO-876, PNO-877, PNO-882).
/// </summary>
public static class OpportunityUXAndLayoutSpec
{
    /// <summary>
    /// GET /api/opportunity/{id} — Opportunity detail (PNO-769 header, key info, quick stats).
    /// </summary>
    public static string GetOpportunityEndpoint(int id) => $"/api/opportunity/{id}";

    /// <summary>
    /// PATCH /api/opportunity/{id}/overview — Key Information section (PNO-769 AC4).
    /// </summary>
    public static string UpdateOverviewEndpoint(int id) => $"/api/opportunity/{id}/overview";

    /// <summary>
    /// GET /api/comment/{entityType}/{entityId} — Comments for opportunity (PNO-871).
    /// </summary>
    public static string GetCommentsEndpoint(string entityType, int entityId) => $"/api/comment/{entityType}/{entityId}";

    /// <summary>
    /// POST /api/comment — Create comment (PNO-871).
    /// </summary>
    public const string CreateCommentEndpoint = "/api/comment";

    /// <summary>
    /// GET /api/opportunity/{id}/dst-risks — Risks section (PNO-876: DST renamed to Risks).
    /// </summary>
    public static string GetRisksEndpoint(int id) => $"/api/opportunity/{id}/dst-risks";

    /// <summary>
    /// PNO-769 AC1: Header fields required in opportunity response.
    /// </summary>
    public static readonly string[] RequiredHeaderFields = { "id", "name", "stage", "targetSigningDate" };

    /// <summary>
    /// PNO-769 AC4: Key Information section fields.
    /// </summary>
    public static readonly string[] KeyInformationFields = { "id", "name", "description", "totalBudget" };

    /// <summary>
    /// PNO-769 AC5: Quick stats (days to signing, countries, service lines, partners, SDGs).
    /// </summary>
    public static readonly string[] QuickStatsIndicators = { "fundingPartners", "clientPartners", "countries" };

    /// <summary>
    /// PNO-871: Entity type for opportunity comments.
    /// </summary>
    public const string OpportunityEntityType = "Opportunity";

    /// <summary>
    /// PNO-876: Navigation label "Risks" (replaced "DST").
    /// </summary>
    public const string RisksSectionLabel = "Risks";

    /// <summary>
    /// PNO-862: Unsaved changes message key (frontend i18n).
    /// </summary>
    public const string UnsavedChangesMessageKey = "message.unsavedChanges";

    /// <summary>
    /// PNO-871: Max comment length (typical limit).
    /// </summary>
    public const int CommentMaxLength = 5000;
}
