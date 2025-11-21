namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for tagging a document with related partners (Partner Results Framework)
/// </summary>
public class TagDocumentToPartnersRequest
{
    /// <summary>
    /// Document ID to be tagged
    /// </summary>
    public required int DocumentId { get; set; }
    
    /// <summary>
    /// IDs of funding partners to associate with this document
    /// </summary>
    public List<int>? FundingPartnerIds { get; set; }
    
    /// <summary>
    /// IDs of client partners to associate with this document
    /// </summary>
    public List<int>? ClientPartnerIds { get; set; }
}

