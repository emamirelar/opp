namespace UNOPS.PAO.Models.Partners;

/// <summary>
/// Partner Agreement information for display in opportunity context
/// </summary>
public class PartnerAgreementInfo
{
    public string PartnerAgreementNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PartnerAgreementType { get; set; }
    public string? PartnerAgreementTypeDescription { get; set; }
    public string? PartnerAgreementScope { get; set; }
    public string? PartnerAgreementScopeDescription { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? SignedDate { get; set; }
    
    /// <summary>
    /// Whether this agreement covers the full opportunity delivery period
    /// </summary>
    public bool CoversOpportunityPeriod { get; set; }
    
    /// <summary>
    /// Whether the agreement expires before the opportunity end date
    /// </summary>
    public bool ExpiresBeforeOpportunityEnd { get; set; }
    
    /// <summary>
    /// Description of service lines covered by this agreement
    /// </summary>
    public string? ServiceLinesDescription { get; set; }
    
    /// <summary>
    /// Geographic restrictions (countries) if any
    /// </summary>
    public string? GeographicRestrictions { get; set; }
    
    /// <summary>
    /// Whether this agreement has geographic restrictions
    /// </summary>
    public bool HasGeographicRestrictions { get; set; }
    
    /// <summary>
    /// Warning message if any (e.g., expires before opportunity end, geographic mismatch)
    /// </summary>
    public string? WarningMessage { get; set; }
    
    /// <summary>
    /// Source of agreement: "ERP" for BigQuery synced, "Document" for manually uploaded
    /// </summary>
    public string Source { get; set; } = "ERP";
    
    /// <summary>
    /// Document ID if this is a manually uploaded Partnership Agreement document
    /// </summary>
    public int? DocumentId { get; set; }
    
    /// <summary>
    /// Document storage path for opening in new tab
    /// </summary>
    public string? DocumentStoragePath { get; set; }
}

