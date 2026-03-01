/**
 * @fileoverview Models for Vector Store API communication
 * @author UNOPS Opportunity+ System Development Team
 */

using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models.AI;

/// <summary>
/// Request model for vector store search
/// </summary>
public class VectorStoreSearchRequest
{
    /// <summary>
    /// Search query string
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    [JsonPropertyName("maxResults")]
    public int MaxResults { get; set; } = 10;

    /// <summary>
    /// Entity type ID filter (e.g., "partner", "contact", "interaction")
    /// </summary>
    [JsonPropertyName("entityTypeId")]
    public string EntityTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID filter for specific entity
    /// </summary>
    [JsonPropertyName("entityId")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Application ID filter
    /// </summary>
    [JsonPropertyName("applicationId")]
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>
    /// Datasource ID filter
    /// </summary>
    [JsonPropertyName("datasourceId")]
    public string DatasourceId { get; set; } = string.Empty;

    /// <summary>
    /// Datasource connector type filter (e.g., "GOOGLE_BIGQUERY")
    /// </summary>
    [JsonPropertyName("datasourceConnector")]
    public string DatasourceConnector { get; set; } = string.Empty;

    /// <summary>
    /// Primary related entity type ID filter
    /// </summary>
    [JsonPropertyName("primaryRelatedToEntityTypeId")]
    public string PrimaryRelatedToEntityTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Primary related entity ID filter
    /// </summary>
    [JsonPropertyName("primaryRelatedToEntityId")]
    public string PrimaryRelatedToEntityId { get; set; } = string.Empty;

    /// <summary>
    /// Additional filters as key-value pairs
    /// </summary>
    [JsonPropertyName("filters")]
    public Dictionary<string, string> Filters { get; set; } = new();

    /// <summary>
    /// Enable debug mode for detailed response
    /// </summary>
    [JsonPropertyName("debug")]
    public bool Debug { get; set; } = false;
}

/// <summary>
/// Response model for vector store search
/// </summary>
public class VectorStoreSearchResponse
{
    /// <summary>
    /// Search results (API returns as "documents")
    /// </summary>
    [JsonPropertyName("documents")]
    public List<VectorStoreDocument> Documents { get; set; } = new();

    /// <summary>
    /// Legacy property for backward compatibility
    /// </summary>
    [JsonIgnore]
    public List<VectorStoreDocument> Results => Documents;

    /// <summary>
    /// Status of the search operation
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Original query string
    /// </summary>
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    /// <summary>
    /// Error message if any
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Individual document from vector store search
/// </summary>
public class VectorStoreDocument
{
    /// <summary>
    /// Organization ID
    /// </summary>
    [JsonPropertyName("organizationId")]
    public string OrganizationId { get; set; } = string.Empty;

    /// <summary>
    /// Datasource ID
    /// </summary>
    [JsonPropertyName("datasourceId")]
    public string DatasourceId { get; set; } = string.Empty;

    /// <summary>
    /// Document ID
    /// </summary>
    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Document name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Document title
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Entity type ID
    /// </summary>
    [JsonPropertyName("entityTypeId")]
    public string EntityTypeId { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID
    /// </summary>
    [JsonPropertyName("entityId")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// MIME type
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Document metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    /// <summary>
    /// Datasource description
    /// </summary>
    [JsonPropertyName("datasourceDescription")]
    public string? DatasourceDescription { get; set; }

    /// <summary>
    /// Datasource comments
    /// </summary>
    [JsonPropertyName("datasourceComments")]
    public string? DatasourceComments { get; set; }

    /// <summary>
    /// Datasource connector type
    /// </summary>
    [JsonPropertyName("datasourceConnector")]
    public string? DatasourceConnector { get; set; }

    /// <summary>
    /// Chunk ID
    /// </summary>
    [JsonPropertyName("chunkId")]
    public string? ChunkId { get; set; }

    /// <summary>
    /// Page start
    /// </summary>
    [JsonPropertyName("pageStart")]
    public int? PageStart { get; set; }

    /// <summary>
    /// Page end
    /// </summary>
    [JsonPropertyName("pageEnd")]
    public int? PageEnd { get; set; }

    /// <summary>
    /// Document content
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Distance/relevance score (lower is better for distance metrics)
    /// </summary>
    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    /// <summary>
    /// Legacy score property for backward compatibility
    /// </summary>
    [JsonIgnore]
    public double Score => 1 - Distance; // Convert distance to similarity score
}

/// <summary>
/// Response model for URL conversion
/// </summary>
public class ConvertedDocument
{
    /// <summary>
    /// Status of conversion
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Converted content
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Original URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Error message if any
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Response model for Google Doc conversion
/// </summary>
public class GoogleDocResponse
{
    /// <summary>
    /// Status of conversion
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Google Doc ID
    /// </summary>
    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Google Doc URL
    /// </summary>
    [JsonPropertyName("documentUrl")]
    public string DocumentUrl { get; set; } = string.Empty;

    /// <summary>
    /// Error message if any
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Base64-encoded PDF when downloadPDF=true in request (API may return pdfBase64 or pdf_base64)
    /// </summary>
    [JsonPropertyName("pdfBase64")]
    public string? PdfBase64 { get; set; }

    [JsonPropertyName("pdf_base64")]
    public string? PdfBase64Snake { get; set; }
}

