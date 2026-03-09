/**
 * @fileoverview Admin, Access Control & Validation test specification.
 * Covers PNO-762, PNO-767, PNO-768, PNO-772, PNO-774, PNO-807, PNO-960, PNO-963.
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.AdminAccessValidation;

/// <summary>
/// Specification constants for Admin, Access Control &amp; Validation tests.
/// Requirements: 403 must not occur for authorized users; translated UI text;
/// document upload works; audit fields populate; name limited to 255 chars;
/// Opportunities list loads for GENUSER; ENGREVADMIN/Programme-Portfolio (oUP).
/// </summary>
public static class AdminAccessValidationSpec
{
    /// <summary>PNO-774: Opportunity Name max 255 characters.</summary>
    public const int OpportunityNameMaxLength = 255;

    /// <summary>Opportunity API base path.</summary>
    public const string OpportunityBase = "/api/opportunity";

    /// <summary>Partner API base path.</summary>
    public const string PartnerBase = "/api/partner";

    /// <summary>Document API base path pattern: /api/document/{entityName}/{entityId}.</summary>
    public static string DocumentByEntity(string entityName, int entityId) =>
        $"/api/document/{entityName}/{entityId}";

    /// <summary>Partner documents endpoint (PNO-768) - /api/document/Partner/{id}.</summary>
    public static string PartnerDocuments(int partnerId) =>
        $"/api/document/Partner/{partnerId}";

    /// <summary>Create opportunity from partner endpoint (PNO-762).</summary>
    public static string CreateOpportunityFromPartner(int partnerId) =>
        $"{PartnerBase}/{partnerId}/create-opportunity";
}
