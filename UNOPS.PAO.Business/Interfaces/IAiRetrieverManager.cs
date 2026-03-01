/**
 * @fileoverview Interface for AI Retriever Manager for external API communication
 * @author UNOPS Opportunity+ System Development Team
 */

using UNOPS.PAO.Models.AI;

namespace UNOPS.PAO.Business.Interfaces;

/// <summary>
/// Interface for AI Retriever Manager
/// Provides methods for calling external AI retriever API endpoints
/// </summary>
public interface IAiRetrieverManager
{
    /// <summary>
    /// Search corporate vector store
    /// </summary>
    Task<VectorStoreSearchResponse> SearchVectorStoreAsync(
        VectorStoreSearchRequest request, 
        string? userEmail = null);

    /// <summary>
    /// Convert URL to document
    /// </summary>
    Task<ConvertedDocument> ConvertUrlAsync(
        string url, 
        string? userEmail = null);

    /// <summary>
    /// Convert markdown to Google Doc.
    /// API expects multipart/form-data with file and data fields.
    /// </summary>
    Task<GoogleDocResponse> ConvertMarkdownToGoogleDocAsync(
        string markdown,
        string? userEmail = null,
        string? fileName = null);

    // Add more endpoint methods as needed...
}

