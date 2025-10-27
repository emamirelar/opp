namespace UNOPS.PAO.Models.Shared;

public class PaginationResponse<T>
{
    public List<T> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Optional search metadata showing which fields matched the search query
    /// Only populated for search endpoints, null for regular list endpoints
    /// </summary>
    public Dictionary<int, Dictionary<string, object>>? SearchMetadata { get; set; }
    
    /// <summary>
    /// The original search query (only for search endpoints)
    /// </summary>
    public string? SearchQuery { get; set; }
    
    /// <summary>
    /// Time taken to execute the search in milliseconds (only for search endpoints)
    /// </summary>
    public double? ExecutionTimeMs { get; set; }
}