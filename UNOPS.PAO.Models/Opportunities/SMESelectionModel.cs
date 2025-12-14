namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Model representing an SME (Subject Matter Expert) selection for an opportunity.
/// Used to display and manage SME assignments.
/// Loaded from OpportunityStakeholder table where IsInternal = true and EntityRole.Type = "SME".
/// </summary>
public class SMESelectionModel
{
    /// <summary>
    /// The entity role ID for the SME role
    /// </summary>
    public int EntityRoleId { get; set; }
    
    /// <summary>
    /// The name of the SME role (e.g., "SME - Infrastructure")
    /// </summary>
    public string? EntityRoleName { get; set; }
    
    /// <summary>
    /// Whether this SME role is selected/enabled for the opportunity
    /// </summary>
    public bool IsSelected { get; set; }
    
    /// <summary>
    /// The user ID assigned to this SME role (null if not assigned)
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// The name of the assigned user (null if not assigned)
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// The email of the assigned user (null if not assigned)
    /// </summary>
    public string? UserEmail { get; set; }
}

/// <summary>
/// Request model for saving SME selection for an opportunity.
/// </summary>
public class SMESelectionRequest
{
    /// <summary>
    /// The entity role ID for the SME role
    /// </summary>
    public int EntityRoleId { get; set; }
    
    /// <summary>
    /// Whether this SME role is selected/enabled
    /// </summary>
    public bool IsSelected { get; set; }
    
    /// <summary>
    /// The user ID assigned to this SME role (required if IsSelected is true)
    /// </summary>
    public int? UserId { get; set; }
}

