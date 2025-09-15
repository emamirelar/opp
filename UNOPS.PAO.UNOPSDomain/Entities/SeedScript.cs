using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSDomain.Entities
{
    public class SeedScript : BaseBusinessEntity
    {
        public string ScriptName { get; set; } = string.Empty;
        public string ScriptType { get; set; } = string.Empty; // "sql" or "seeder"
        public string FileHash { get; set; } = string.Empty;
        public DateTime LastExecutedDate { get; set; }
        public string? Description { get; set; }
        public int ExecutionOrder { get; set; }
    }
}
