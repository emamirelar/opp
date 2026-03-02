using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models.AI;
public class EntityEmbeddingsModel
{
    public int Id { get; set; }
    public string EntityName { get; set; } = null!;
    public int EntityId { get; set; }
    public string EntityData { get; set; } = null!;
    public byte[] FullEmbedding { get; set; } = null!;
}