namespace UNOPS.PAO.Models;

/// <summary>
/// Model for opportunity manager - the primary person responsible for the opportunity.
/// Part of the Opportunity Development Team.
/// </summary>
public class OpportunityManagerModel
{
    /// <summary>
    /// The user ID of the opportunity manager
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Full name of the opportunity manager
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Email of the opportunity manager
    /// </summary>
    public string? UserEmail { get; set; }
    
    /// <summary>
    /// Standardized position title from the personnel record
    /// </summary>
    public string? Position { get; set; }
}
