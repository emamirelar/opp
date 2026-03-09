/**
 * @fileoverview Opportunity Creation specification — PNO-687, PNO-689, PNO-764, PNO-771, PNO-800, PNO-802, PNO-814, PNO-815, PNO-816, PNO-917.
 * Defines constants, endpoints, and expected behavior for Create Opportunity functionality.
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.OpportunityCreation;

/// <summary>
/// Specification for Opportunity Creation (PNO-687 master AC, PNO-689, PNO-764, PNO-771, PNO-800, PNO-802, PNO-814, PNO-815, PNO-816, PNO-917).
/// </summary>
public static class OpportunityCreationSpec
{
    /// <summary>
    /// POST /api/opportunity — Create from Opportunities module (PNO-689).
    /// </summary>
    public const string CreateOpportunityEndpoint = "/api/opportunity";

    /// <summary>
    /// POST /api/partner/{partnerId}/create-opportunity — Create from Partner account (PNO-687).
    /// </summary>
    public static string CreateFromPartnerEndpoint(int partnerId) => $"/api/partner/{partnerId}/create-opportunity";

    /// <summary>
    /// POST /api/opportunity/create-from-proposal — Create from Interactions via AI (PNO-815, PNO-816).
    /// </summary>
    public const string CreateFromProposalEndpoint = "/api/opportunity/create-from-proposal";

    /// <summary>
    /// PNO-687 AC4: Name max length 255 chars (matches oUP engagement_description).
    /// </summary>
    public const int NameMaxLength = 255;

    /// <summary>
    /// PNO-764/PNO-771: Only PARTNER_USER and PARTNER_GLOBAL_ADMIN can create opportunities.
    /// </summary>
    public static readonly string[] AllowedCreateRoles = { "PARTNER_USER", "PARTNER_GLOBAL_ADMIN" };

    /// <summary>
    /// PNO-771: GENERAL_USER and ORG_UNIT_ADMIN must NOT see Create Opportunity button.
    /// </summary>
    public static readonly string[] BlockedCreateRoles = { "UNOPS_GEN_USER", "ORG_UNIT_ADMIN" };

    /// <summary>
    /// PNO-917: Partner statuses that allow opportunity creation.
    /// </summary>
    public static readonly string[] AllowedPartnerStatuses = { "Active", "Draft" };

    /// <summary>
    /// PNO-687 AC2, PNO-917: Partner statuses that block opportunity creation.
    /// </summary>
    public static readonly string[] BlockedPartnerStatuses = { "Closed", "Archived" };

    /// <summary>
    /// PNO-687 AC3: Valid partner roles when creating from partner.
    /// </summary>
    public static readonly string[] ValidPartnerRoles = { "funding", "client", "both" };

    /// <summary>
    /// PNO-687 AC6: Default stage for new opportunities.
    /// </summary>
    public const string DefaultStage = "IDENTIFY & PROFILE";

    /// <summary>
    /// PNO-802: Consistent button label across 5 locations.
    /// </summary>
    public const string CreateButtonLabel = "+ New";

    /// <summary>
    /// PNO-800: Section name on Create popup (Key, not Basic).
    /// </summary>
    public const string KeySectionName = "Key";
}
