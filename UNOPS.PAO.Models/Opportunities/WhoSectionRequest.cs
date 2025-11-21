namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the WHO section of an opportunity
/// Includes funding partners, client partners, and stakeholders
/// </summary>
public class WhoSectionRequest
{
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
}

