using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Partners;

namespace UNOPS.PAO.Models.Shared;

/// <summary>
/// Response model for all dashboard data.
/// This combines all dashboard data into a single response to avoid
/// multiple concurrent API calls which can cause DbContext threading issues.
/// </summary>
public class DashboardResponse
{
    /// <summary>
    /// Partners created or modified by the current user (non-draft)
    /// </summary>
    public List<PartnerModel> MyPartners { get; set; } = new();

    /// <summary>
    /// Contacts created or modified by the current user (non-draft)
    /// </summary>
    public List<ContactModel> MyContacts { get; set; } = new();

    /// <summary>
    /// Interactions created or modified by the current user (non-draft)
    /// </summary>
    public List<InteractionModel> MyInteractions { get; set; } = new();

    /// <summary>
    /// Opportunities where user is stakeholder, creator, or modifier (non-draft)
    /// </summary>
    public List<OpportunityModel> MyOpportunities { get; set; } = new();

    /// <summary>
    /// Draft partners created or modified by the current user
    /// </summary>
    public List<PartnerModel> DraftPartners { get; set; } = new();

    /// <summary>
    /// Draft contacts created or modified by the current user
    /// </summary>
    public List<ContactModel> DraftContacts { get; set; } = new();

    /// <summary>
    /// Draft interactions created or modified by the current user
    /// </summary>
    public List<InteractionModel> DraftInteractions { get; set; } = new();

    /// <summary>
    /// Draft opportunities where user is stakeholder, creator, or modifier
    /// </summary>
    public List<OpportunityModel> DraftOpportunities { get; set; } = new();

    /// <summary>
    /// Recent updates from the user's org unit
    /// </summary>
    public List<RecentUpdateModel> OrgUnitRecentUpdates { get; set; } = new();

    /// <summary>
    /// Name of the org unit being filtered
    /// </summary>
    public string OrgUnitName { get; set; } = "your organization unit";

    /// <summary>
    /// ID of the org unit being filtered (null if no filter)
    /// </summary>
    public int? OrgUnitId { get; set; }
}

