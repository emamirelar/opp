using NpgsqlTypes; // Required for pgvector
namespace UNOPS.PAO.Domain.Entities;

public class EntityEmbeddings
{
    public int Id { get; set; }
    public required string EntityName { get; set; }
    public int EntityId { get; set; }
    public required string EntityData { get; set; }
    public required byte[] FullEmbedding { get; set; }
}