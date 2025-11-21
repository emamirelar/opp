namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the WHO section of an opportunity
/// Includes funding partners, client partners, and stakeholders
/// </summary>
public class WhoSectionRequest
{
    /// <summary>
    /// Whether funding is pooled across multiple partners (AC8)
    /// </summary>
    public bool IsPooledFunding { get; set; }
    
    /// <summary>
    /// List of funding partners
    /// </summary>
    public List<OpportunityFundingPartnerRequest>? FundingPartners { get; set; }

    /// <summary>
    /// List of client partners
    /// </summary>
    public List<OpportunityClientPartnerRequest>? ClientPartners { get; set; }

    /// <summary>
    /// List of team members and stakeholders
    /// </summary>
    public List<OpportunityStakeholderRequest>? Stakeholders { get; set; }
    
    /// <summary>
    /// List of external stakeholders (contacts)
    /// </summary>
    public List<OpportunityExternalStakeholderRequest>? ExternalStakeholders { get; set; }
    
    /// <summary>
    /// Free-text list of external stakeholders not in contact list
    /// </summary>
    public string? MiscExternalStakeholders { get; set; }
    
    /// <summary>
    /// Additional notes about external stakeholders
    /// </summary>
    public string? ExternalStakeholderNotes { get; set; }
}

