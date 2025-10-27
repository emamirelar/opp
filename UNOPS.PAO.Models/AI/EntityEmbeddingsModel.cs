using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models.AI;
public class EntityEmbeddingsModel
{
    public int Id { get; set; }
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    public string EntityData { get; set; }
    public byte[] FullEmbedding { get; set; }
}