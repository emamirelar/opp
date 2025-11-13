namespace UNOPS.PAO.Models;

/// <summary>
/// Request model for creating a new opportunity directly from a partner record
/// </summary>
public class CreateOpportunityFromPartnerRequest
{
    /// <summary>
    /// Name of the opportunity
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Partner role selection - "funding", "client", or "both"
    /// </summary>
    public required string PartnerRole { get; set; }
    
    /// <summary>
    /// Optional initial description
    /// </summary>
    public string? Description { get; set; }
}


