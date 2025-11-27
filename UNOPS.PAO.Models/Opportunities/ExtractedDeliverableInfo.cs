/**
 * @fileoverview Model for AI-extracted deliverable information from Partner Results Framework and project documents.
 * This is temporary extraction data that is NOT stored in database until user verification.
 * @author UNOPS Opportunity+ System Development Team
 */

namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Represents a product/service extracted by AI from Partner Results Framework or project documents.
/// This is TEMPORARY DATA used for display and verification before saving to database.
/// </summary>
/// <remarks>
/// AI extraction phase - data returned to frontend for user review
/// User verification phase - selected items are saved to OpportunityDeliverable table
/// </remarks>
public class ExtractedDeliverableInfo
{
    /// <summary>
    /// The exact partner language/wording from the source document.
    /// Preserved as-is without translation to UNOPS terminology.
    /// </summary>
    /// <example>"Enhanced national digital service delivery systems"</example>
    public string PartnerLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Context information about where this was found in the document.
    /// </summary>
    /// <example>"Output 2.3 in Partner Results Framework, page 12"</example>
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Name of the source document this was extracted from.
    /// </summary>
    /// <example>"UNDP Results Framework 2025-2027.pdf"</example>
    public string SourceDocumentName { get; set; } = string.Empty;

    /// <summary>
    /// ID of the source document in the Documents table.
    /// </summary>
    public int SourceDocumentId { get; set; }

    /// <summary>
    /// Indicates if this came from a tagged Partner Results Framework document (priority source).
    /// True = from tagged framework, False = from other documents (fallback source).
    /// </summary>
    public bool IsPrioritySource { get; set; }

    /// <summary>
    /// AI confidence score for this extraction (0.0 to 1.0).
    /// Higher scores indicate more explicit mentions in the source document.
    /// </summary>
    /// <example>0.95</example>
    public decimal Confidence { get; set; }

    /// <summary>
    /// AI reasoning for why this item was extracted.
    /// </summary>
    /// <example>"Explicitly listed as Output 2.3 in the results framework"</example>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// Matched Output ID from the Outputs table (if similarity match found).
    /// Null if no match found or similarity score below threshold.
    /// </summary>
    public int? MatchedOutputId { get; set; }

    /// <summary>
    /// Name of the matched output from the Outputs table.
    /// </summary>
    public string? MatchedOutputName { get; set; }

    /// <summary>
    /// Similarity score for the matched output (0.0 to 1.0).
    /// Indicates how closely the partner language matches the UNOPS output.
    /// </summary>
    public decimal? MatchScore { get; set; }

    /// <summary>
    /// Field name that was matched in the Outputs table (e.g., "Name", "Description").
    /// </summary>
    public string? MatchedField { get; set; }
}

