namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Unified request model for generating AI opportunity proposals from multiple sources
/// Supports interactions, new document uploads, and existing document references
/// Can be used from partner tabs, interaction lists, opportunity lists, etc.
/// </summary>
public class OpportunityProposalRequest
{
    /// <summary>
    /// Proposed opportunity name (required)
    /// </summary>
    public required string OpportunityName { get; set; }

    /// <summary>
    /// Proposed opportunity description (optional)
    /// </summary>
    public string? OpportunityDescription { get; set; }

    /// <summary>
    /// Partner ID (optional - can be inferred from interactions or selected by user)
    /// </summary>
    public int? PartnerId { get; set; }

    /// <summary>
    /// Whether partner is a funding partner
    /// </summary>
    public bool IsFundingPartner { get; set; }

    /// <summary>
    /// Whether partner is a client partner
    /// </summary>
    public bool IsClientPartner { get; set; }

    /// <summary>
    /// Optional interaction IDs to analyze for opportunity data
    /// </summary>
    public List<int>? InteractionIds { get; set; }

    /// <summary>
    /// Storage paths for newly uploaded documents (already in GCS)
    /// Frontend converts Office to PDF and uploads to GCS before calling generate-proposal
    /// Example: ["gs://bucket/doc1.pdf", "gs://bucket/doc2.pdf"]
    /// </summary>
    public List<string>? NewDocumentStoragePaths { get; set; }

    /// <summary>
    /// MIME types for newly uploaded documents (matches NewDocumentStoragePaths by index)
    /// Example: ["application/pdf", "application/pdf"]
    /// </summary>
    public List<string>? NewDocumentMimeTypes { get; set; }

    /// <summary>
    /// Document type IDs for newly uploaded documents (matches NewDocumentStoragePaths by index)
    /// Used to categorize documents when creating the opportunity
    /// </summary>
    public List<int?>? NewDocumentTypeIds { get; set; }

    /// <summary>
    /// IDs of existing documents already in the system
    /// Backend will query database for their GCS storage paths
    /// </summary>
    public List<int>? ExistingDocumentIds { get; set; }
}

