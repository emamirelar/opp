namespace UNOPS.PAO.Models;

/// <summary>
/// Model for collaborator expertise lookup data.
/// Represents the expertise types that can be assigned to collaborators.
/// </summary>
public class CollaboratorExpertiseModel
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique code for the expertise type (e.g., "GEN_OPP_DEV", "FIN_MGMT")
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name of the expertise type
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the expertise type
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Display order for sorting in dropdowns
    /// </summary>
    public int DisplayOrder { get; set; }
}
