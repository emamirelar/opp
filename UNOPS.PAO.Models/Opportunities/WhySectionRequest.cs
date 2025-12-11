namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the WHY section of an opportunity
/// Includes strategic alignment, expected beneficiaries, outcomes, and SDG alignments
/// </summary>
public class WhySectionRequest
{
    
    
    /// <summary>
    /// Results focus description
    /// </summary>
    public string? ResultsFocus { get; set; }

    /// <summary>
    /// Expected impact description (max 200 characters)
    /// </summary>
    public string? ExpectedImpact { get; set; }
    
    /// <summary>
    /// Expected outcomes description (max 200 characters)
    /// </summary>
    public string? ExpectedOutcomes { get; set; }

    /// <summary>
    /// Expected beneficiaries description
    /// </summary>
    public string? ExpectedBeneficiaries { get; set; }
    
    /// <summary>
    /// Estimated number of direct beneficiaries (positive integer)
    /// </summary>
    public int? EstimatedDirectBeneficiaries { get; set; }
    
    /// <summary>
    /// Estimated number of indirect beneficiaries (positive integer)
    /// </summary>
    public int? EstimatedIndirectBeneficiaries { get; set; }
    
    /// <summary>
    /// Indicates whether beneficiary numbers will be determined during development
    /// </summary>
    public bool BeneficiariesToBeDetermined { get; set; }
    
    /// <summary>
    /// Challenges that the initiative will address
    /// </summary>
    public string? Challenges { get; set; }

    /// <summary>
    /// List of SDG alignments for the opportunity
    /// </summary>
    public List<OpportunitySDGRequest>? SdGs { get; set; }

    /// <summary>
    /// List of UNCF Outcome alignments for the opportunity (country-specific)
    /// </summary>
    public List<OpportunityUNCFOutcomeRequest>? UncfOutcomes { get; set; }
    
    /// <summary>
    /// List of UNOPS Mission alignments for the opportunity
    /// </summary>
    public List<OpportunityUNOPSMissionRequest>? UNOPSMissions { get; set; }
}

