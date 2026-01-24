using System.Collections.Generic;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Models
{
    public class SeedConfiguration
    {
        public List<SeedStep> SeedSteps { get; set; } = new List<SeedStep>();
    }

    public class SeedStep
    {
        public int Order { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "sql" or "seeder"
        public string? Path { get; set; } // For SQL files
        public string? ClassName { get; set; } // For C# seeders
        public string? MethodName { get; set; } // For C# seeders
        public string? FilePath { get; set; } // For C# seeder file hash tracking
        public string Description { get; set; } = string.Empty;
        public bool ForceExecuteIfAnyChanged { get; set; } = false; // Force execution if any previous step was executed

        public bool IsSqlScript => Type.Equals("sql", StringComparison.OrdinalIgnoreCase);
        public bool IsSeeder => Type.Equals("seeder", StringComparison.OrdinalIgnoreCase);
    }
}
