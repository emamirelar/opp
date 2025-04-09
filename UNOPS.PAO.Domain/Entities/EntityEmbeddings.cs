using NpgsqlTypes; // Required for pgvector
namespace UNOPS.PAO.Domain.Entities;

public class EntityEmbeddings
{
    public int Id { get; set; }
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    public byte[] FullEmbedding { get; set; }
    public byte[]? NameEmbedding { get; set; }
}