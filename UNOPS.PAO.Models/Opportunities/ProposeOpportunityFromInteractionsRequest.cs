namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for proposing an opportunity from selected interactions
/// Sends interaction data to AI for analysis and opportunity proposal generation
/// </summary>
public class ProposeOpportunityFromInteractionsRequest
{
    /// <summary>
    /// List of interaction IDs to analyze
    /// </summary>
    public required List<int> InteractionIds { get; set; }
    
    /// <summary>
    /// User-provided opportunity name
    /// </summary>
    public required string OpportunityName { get; set; }
    
    /// <summary>
    /// User-provided opportunity description
    /// </summary>
    public required string OpportunityDescription { get; set; }
    
    /// <summary>
    /// Partner ID associated with the interactions
    /// </summary>
    public required int PartnerId { get; set; }
    
    /// <summary>
    /// Whether the partner should be added as a funding partner
    /// </summary>
    public required bool IsFundingPartner { get; set; }
    
    /// <summary>
    /// Whether the partner should be added as a client partner
    /// </summary>
    public required bool IsClientPartner { get; set; }
}

