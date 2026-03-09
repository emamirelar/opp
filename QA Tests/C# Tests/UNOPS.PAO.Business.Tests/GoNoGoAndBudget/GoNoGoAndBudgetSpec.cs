/**
 * @fileoverview Specification for Go/No-Go Workflow and Budget-related bugs.
 * Consolidates requirements from PNO-1193, PNO-1203, PNO-1204, PNO-1205, PNO-1206.
 * @author UNOPS Opportunity+ QA Team
 */

namespace UNOPS.PAO.Business.Tests.GoNoGoAndBudget;

/// <summary>
/// Requirements specification for Go/No-Go workflow and budget bugs.
///
/// Requirements validated:
/// - PNO-1193: When OM is reassigned in Go/No-Go, original OM must be demoted to Collaborator
/// - PNO-1203: Users must be findable/provisioned in test environment (search, typeahead)
/// - PNO-1204: Exchange rate date must default appropriately for funding partners
/// - PNO-1205: AI-created opportunities must have valid Implementation Start Date
/// - PNO-1206: Org Unit Directors/Managers must populate in Team section when Org Unit selected
///
/// Defects logged:
/// - DEF-193: [If any code differs from requirement]
/// </summary>
public static class GoNoGoAndBudgetSpec
{
    /// <summary>PNO-1193: OM reassignment demotes original OM to Collaborator</summary>
    public const string PNO1193_OM_DEMOTED_TO_COLLABORATOR = "PNO-1193";

    /// <summary>PNO-1203: User search finds/provisions users in test env</summary>
    public const string PNO1203_USER_SEARCH_FINDS_USERS = "PNO-1203";

    /// <summary>PNO-1204: Exchange rate date defaults for funding partners</summary>
    public const string PNO1204_EXCHANGE_RATE_DATE_DEFAULT = "PNO-1204";

    /// <summary>PNO-1205: AI Implementation Start Date validation</summary>
    public const string PNO1205_AI_IMPL_START_DATE_VALID = "PNO-1205";

    /// <summary>PNO-1206: Org Unit Directors/Managers populate in Team section</summary>
    public const string PNO1206_ORG_UNIT_DIRECTORS_POPULATE = "PNO-1206";
}
