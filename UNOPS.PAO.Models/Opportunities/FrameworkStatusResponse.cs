/**
 * @fileoverview Response model for Partner Results Framework status check.
 * @author UNOPS Opportunity+ System Development Team
 */

namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Response model indicating whether Partner Results Framework documents are tagged in WHO section.
/// Used to determine extraction priority and display appropriate UI banners.
/// </summary>
public class FrameworkStatusResponse
{
    /// <summary>
    /// Indicates if any Partner Results Framework documents are tagged to funding/client partners.
    /// </summary>
    public bool HasTaggedFrameworks { get; set; }

    /// <summary>
    /// List of tagged Partner Results Framework documents with partner information.
    /// </summary>
    public List<TaggedFrameworkInfo> TaggedFrameworks { get; set; } = new();

    /// <summary>
    /// Total count of all documents uploaded to the opportunity (for fallback extraction).
    /// </summary>
    public int AllDocumentsCount { get; set; }
}

/// <summary>
/// Information about a tagged Partner Results Framework document.
/// </summary>
public class TaggedFrameworkInfo
{
    /// <summary>
    /// Partner ID associated with this framework document.
    /// </summary>
    public int PartnerId { get; set; }

    /// <summary>
    /// Partner name.
    /// </summary>
    /// <example>"United Nations Development Programme (UNDP)"</example>
    public string PartnerName { get; set; } = string.Empty;

    /// <summary>
    /// Document ID of the tagged framework.
    /// </summary>
    public int DocumentId { get; set; }

    /// <summary>
    /// Document name.
    /// </summary>
    /// <example>"UNDP Results Framework 2025-2027.pdf"</example>
    public string DocumentName { get; set; } = string.Empty;

    /// <summary>
    /// Document storage path (GCS gs:// URI).
    /// </summary>
    public string DocumentStoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Partner type: "Funding" or "Client".
    /// </summary>
    public string PartnerType { get; set; } = string.Empty;
}

