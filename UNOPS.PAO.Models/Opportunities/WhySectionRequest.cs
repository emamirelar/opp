namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the WHY section of an opportunity
/// Includes strategic alignment, expected beneficiaries, outcomes, and SDG alignments
/// </summary>
public class WhySectionRequest
{
    /// <summary>
    /// Strategic alignment description
    /// </summary>
    public string? StrategicAlignment { get; set; }
    
    /// <summary>
    /// Results focus description
    /// </summary>
    public string? ResultsFocus { get; set; }

    /// <summary>
    /// Intended impact and outcomes description
    /// </summary>
    public string? IntendedImpactOutcomes { get; set; }

    /// <summary>
    /// Expected beneficiaries description
    /// </summary>
    public string? ExpectedBeneficiaries { get; set; }
    
    /// <summary>
    /// Challenges that the initiative will address
    /// </summary>
    public string? Challenges { get; set; }

    /// <summary>
    /// List of SDG alignments for the opportunity
    /// </summary>
    public List<OpportunitySDGRequest>? SdGs { get; set; }
}

