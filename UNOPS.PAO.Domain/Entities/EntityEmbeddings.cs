using System.ComponentModel.DataAnnotations;
using NpgsqlTypes; // Required for pgvector
namespace UNOPS.PAO.Domain.Entities;

public class EntityEmbeddings
{
    public int Id { get; set; }
    public required string EntityName { get; set; }
    public int EntityId { get; set; }
    public required string EntityData { get; set; }
    public required byte[] FullEmbedding { get; set; }
    
    /// <summary>
    /// Metadata for storing additional information about the embedding
    /// For Output entities: stores level information (e.g., "Level0", "Level1", etc.)
    /// Can also store keywords, context, or other searchable metadata as JSON
    /// </summary>
    [MaxLength(2000)]
    public string? Metadata { get; set; }
    
    /// <summary>
    /// AI-generated keywords for hybrid search (semantic + keyword matching)
    /// Stored as comma-separated values for easy searching
    /// </summary>
    [MaxLength(1000)]
    public string? Keywords { get; set; }
}