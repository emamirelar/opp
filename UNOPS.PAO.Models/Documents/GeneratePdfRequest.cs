namespace UNOPS.PAO.Models.Documents;

/// <summary>
/// Request model for generating a PDF from markdown.
/// When EntityName and EntityId are provided, the backend fetches the content from the entity (e.g., Opportunity statement).
/// Otherwise, Data (markdown) must be provided by the client.
/// </summary>
public class GeneratePdfRequest
{
    /// <summary>
    /// Entity type name (e.g., "Opportunity", "Partner"). When provided with EntityId, the backend fetches the content.
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Entity ID. When provided with EntityName, the backend fetches the content from the entity.
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Optional markdown content. Used when EntityName/EntityId are not provided or when entity-specific fetch returns nothing.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Filename for the generated PDF (without extension). Defaults to "Generated_Document" if not provided.
    /// </summary>
    public string? Filename { get; set; }
}
