/// <summary>
/// Offices Feature Specification — PNO-1213, PNO-1214
///
/// Requirements validated:
/// - PNO-1213 AC: Office List (Organigram/List views), Office Detail tabs, navigation, permissions
/// - PNO-1214 AC: Data integration (BigQuery, ERP, oUP), Related Opportunities, Related Partner Accounts
///
/// Testable surface (existing code):
/// - OrganizationHierarchy / OrganizationHierarchyManager — office hierarchy (Organigram data source)
/// - Opportunity.ResponsibleOrgUnitId — office-opportunity relationship
/// - Partner OrganizationUnitRelationships — office-partner relationship
///
/// Gaps (DEF-211+): Offices API, OfficeManager, Office detail tabs, BigQuery/ERP/oUP integration
/// </summary>

[assembly: CollectionBehavior(DisableTestParallelization = false)]

namespace UNOPS.PAO.Business.Tests.OfficesFeature;

/// <summary>
/// Specification constants for Offices Feature tests (PNO-1213, PNO-1214).
/// Expected API endpoints per PNO-1213 AC — DEF-211: not yet implemented.
/// </summary>
public static class OfficesFeatureSpec
{
    /// <summary>Expected Offices API base path per PNO-1213 — DEF-211: not implemented.</summary>
    public const string OfficesBase = "/api/offices";

    /// <summary>Office list endpoint.</summary>
    public static string OfficesList(int pageIndex = 0, int pageSize = 10) =>
        $"{OfficesBase}?pageIndex={pageIndex}&pageSize={pageSize}";

    /// <summary>Office detail by ID.</summary>
    public static string OfficeById(int id) => $"{OfficesBase}/{id}";

    /// <summary>Office Financial tab (BigQuery) — DEF-213.</summary>
    public static string OfficeFinancial(int id) => $"{OfficesBase}/{id}/financial";

    /// <summary>Office Scope tab — country mapping.</summary>
    public static string OfficeScope(int id) => $"{OfficesBase}/{id}/scope";

    /// <summary>Office Roles &amp; DoA tab (ERP) — DEF-214.</summary>
    public static string OfficeRolesAndDoA(int id) => $"{OfficesBase}/{id}/roles-and-doa";

    /// <summary>Office Physical section (oUP Location) — DEF-215.</summary>
    public static string OfficePhysical(int id) => $"{OfficesBase}/{id}/physical";

    /// <summary>Office Related Opportunities tab.</summary>
    public static string OfficeRelatedOpportunities(int id) => $"{OfficesBase}/{id}/related-opportunities";

    /// <summary>Office Related Partner Accounts tab.</summary>
    public static string OfficeRelatedPartners(int id) => $"{OfficesBase}/{id}/related-partners";

    /// <summary>Office Documents tab (Strategy type only, RD/OiC upload) — DEF-216.</summary>
    public static string OfficeDocuments(int id) => $"{OfficesBase}/{id}/documents";
}
