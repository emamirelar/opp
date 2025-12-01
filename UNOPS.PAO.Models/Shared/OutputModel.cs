namespace UNOPS.PAO.Models.Shared;

/// <summary>
/// DTO for UNOPS Products and Services List with hierarchical structure (Level 0-4).
/// Based on the official UNOPS Products and Services taxonomy.
/// </summary>
public class OutputModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    
    // Hierarchical structure
    public string? Level0 { get; set; }
    public string? Level1 { get; set; }
    public string? DefinitionLevel1 { get; set; }
    public string? Level2 { get; set; }
    public string? DefinitionLevel2 { get; set; }
    public string? Level3 { get; set; }
    public string? DefinitionLevel3 { get; set; }
    public string? Level4 { get; set; }
    public string? DefinitionLevel4 { get; set; }
    
    // Service Line
    public string? ServiceLine { get; set; }
}

