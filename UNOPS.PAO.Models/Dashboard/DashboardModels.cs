namespace UNOPS.PAO.Models.Dashboard;

/// <summary>
/// Lightweight partner model optimized for dashboard display.
/// Contains only the essential fields needed for partner lists and cards.
/// 
/// PERFORMANCE: This model is ~95% smaller than PartnerModel by excluding:
/// - All navigation properties (Contacts, Documents, Interactions, OrganizationUnitRelationships)
/// - All category and approval fields not needed for dashboard
/// - All system-generated keys and GUIDs
/// </summary>
public class DashboardPartnerModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

/// <summary>
/// Lightweight contact model optimized for dashboard display.
/// Contains only the essential fields needed for contact lists and cards.
/// 
/// PERFORMANCE: This model is ~90% smaller than ContactModel by excluding:
/// - All address fields (MailingStreet, MailingCity, etc.)
/// - All assistant fields
/// - All navigation properties (Partner, Documents, Interactions, OrganizationUnitRelationships)
/// - Profile picture and email fields not needed for dashboard display
/// </summary>
public class DashboardContactModel
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Title { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

/// <summary>
/// Lightweight interaction model optimized for dashboard display.
/// Contains only the essential fields needed for interaction lists and cards.
/// 
/// PERFORMANCE: This model is ~85% smaller than InteractionModel by excluding:
/// - All contact, partner, and user collections
/// - All email and phone number lists
/// - All document collections
/// - All organization unit relationships
/// - Gmail integration fields
/// </summary>
public class DashboardInteractionModel
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public DateTime? Date { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

/// <summary>
/// Lightweight opportunity model optimized for dashboard display.
/// Contains only the essential fields needed for opportunity lists and cards.
/// 
/// PERFORMANCE: This model is ~90% smaller than OpportunityModel by excluding:
/// - All partner collections (FundingPartners, ClientPartners)
/// - All stakeholder collections
/// - All country, SDG, and UNCF outcome collections
/// - Banner image and thumbnail (large base64 strings)
/// - All description and result focus fields
/// - All financial and beneficiary details
/// - All deliverable information
/// </summary>
public class DashboardOpportunityModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Status { get; set; }
    public string? Stage { get; set; }
    
    /// <summary>
    /// The current user's role(s) for this opportunity (populated from stakeholder lookup)
    /// </summary>
    public string? UserRole { get; set; }
    
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

/// <summary>
/// Model for recent update entries across all entity types.
/// Used in the "Recent Activity" section of the dashboard.
/// </summary>
public class DashboardRecentUpdateModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    /// <summary>
    /// Entity type: "Partner", "Contact", "Interaction", or "Opportunity"
    /// </summary>
    public required string Type { get; set; }
    
    public DateTime? LastModifiedDate { get; set; }
    public int LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Response model for org unit recent updates.
/// Includes the org unit context information.
/// </summary>
public class DashboardOrgUnitRecentUpdatesResponse
{
    public List<DashboardRecentUpdateModel> Updates { get; set; } = new();
    public string OrgUnitName { get; set; } = "your organization unit";
    public int? OrgUnitId { get; set; }
}

/// <summary>
/// Combined response model for the dashboard endpoint.
/// Returns all dashboard data in a single request to avoid DbContext threading issues.
/// 
/// PERFORMANCE: Uses lightweight dashboard models instead of full entity models,
/// reducing response size by ~80-90% and eliminating unnecessary database joins.
/// </summary>
public class DashboardCombinedResponse
{
    // My Workspace data (non-draft)
    public List<DashboardPartnerModel> MyPartners { get; set; } = new();
    public List<DashboardContactModel> MyContacts { get; set; } = new();
    public List<DashboardInteractionModel> MyInteractions { get; set; } = new();
    public List<DashboardOpportunityModel> MyOpportunities { get; set; } = new();
    
    // Draft items requiring attention
    public List<DashboardPartnerModel> DraftPartners { get; set; } = new();
    public List<DashboardContactModel> DraftContacts { get; set; } = new();
    public List<DashboardInteractionModel> DraftInteractions { get; set; } = new();
    public List<DashboardOpportunityModel> DraftOpportunities { get; set; } = new();
    
    // Recent activity from org unit
    public List<DashboardRecentUpdateModel> OrgUnitRecentUpdates { get; set; } = new();
    public string OrgUnitName { get; set; } = "your organization unit";
    public int? OrgUnitId { get; set; }
}

