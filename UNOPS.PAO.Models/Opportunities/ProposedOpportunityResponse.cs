namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Response model containing AI-proposed opportunity data from multiple sources
/// Includes both the raw AI response and the processed opportunity data ready for user review
/// Can include data from interactions, documents, or other opportunities
/// </summary>
public class OpportunityProposalResponse
{
    /// <summary>
    /// The proposed opportunity data with all fields populated by AI
    /// </summary>
    public required ProposedOpportunityData Opportunity { get; set; }
    
    /// <summary>
    /// Number of interactions analyzed (if any)
    /// </summary>
    public int InteractionsAnalyzed { get; set; }
    
    /// <summary>
    /// List of interaction IDs that were analyzed (if any)
    /// </summary>
    public List<int>? SourceInteractionIds { get; set; }
    
    /// <summary>
    /// Number of documents analyzed (if any)
    /// </summary>
    public int DocumentsAnalyzed { get; set; }
    
    /// <summary>
    /// List of document IDs that were analyzed (if any)
    /// </summary>
    public List<int>? SourceDocumentIds { get; set; }
    
    /// <summary>
    /// Partner ID associated with the opportunity (may be null if not pre-selected)
    /// </summary>
    public int? PartnerId { get; set; }
    
    /// <summary>
    /// Partner name (if partner was selected)
    /// </summary>
    public string? PartnerName { get; set; }
    
    /// <summary>
    /// Whether partner is funding partner
    /// </summary>
    public bool IsFundingPartner { get; set; }
    
    /// <summary>
    /// Whether partner is client partner
    /// </summary>
    public bool IsClientPartner { get; set; }

    /// <summary>
    /// Raw response from AI (for debugging - compare with processed Opportunity data).
    /// </summary>
    public string? RawAiResponse { get; set; }
}

/// <summary>
/// Proposed opportunity data structure matching the OpportunityModel fields
/// Contains AI-extracted values from interactions with dependents for ID resolution
/// Aligned with opportunity-documents field mappings for consistent AI extraction
/// </summary>
public class ProposedOpportunityData
{
    // Basic Information
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? PartnerReference { get; set; }
    
    // Organizational & Initiative Type
    public int? ResponsibleOrgUnitId { get; set; }
    public string? ResponsibleOrgUnitName { get; set; }
    public int? ProposedInitiativeTypeId { get; set; }
    public string? ProposedInitiativeTypeName { get; set; }
    
    // Financial Information
    public decimal? InitiativeBudgetUSD { get; set; }
    public string? PartnershipAgreementReference { get; set; }
    
    // WHEN Section - Timeline Fields
    public DateTime? TargetSigningDate { get; set; }
    public bool? IsTargetSigningDateFirm { get; set; }
    public string? SigningDateNotes { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public DateTime? ImplementationStartDate { get; set; }
    public DateTime? TargetDeliveryDate { get; set; }
    
    // WHY Section - Strategic Information
    public string? Challenges { get; set; }
    public string? ResultsFocus { get; set; }
    /// <summary>
    /// Expected impact description (max 200 characters)
    /// </summary>
    public string? ExpectedImpact { get; set; }
    /// <summary>
    /// Expected outcomes description (max 200 characters)
    /// </summary>
    public string? ExpectedOutcomes { get; set; }
    public string? ExpectedBeneficiaries { get; set; }
    public int? EstimatedDirectBeneficiaries { get; set; }
    public int? EstimatedIndirectBeneficiaries { get; set; }
    public bool? BeneficiariesToBeDetermined { get; set; }
    
    // WHAT Section - Delivery & Stakeholders
    public int? DeliveryModality { get; set; }
    public string? MiscExternalStakeholders { get; set; }
    public string? ExternalStakeholderNotes { get; set; }
    
    // Related Entities (processed by GetDependentDropdownValues into structured objects)
    // Stringified JSON arrays to avoid serialization issues - frontend will parse these
    public string? FundingPartners { get; set; }
    public string? ClientPartners { get; set; }
    public string? Stakeholders { get; set; }
    public string? Deliverables { get; set; }
    public string? Countries { get; set; }
    public string? SdGs { get; set; }
    public string? UnopsMissions { get; set; }
    
    // Dependents list indicating which fields need ID resolution
    public List<string> Dependents { get; set; } = new();
}

