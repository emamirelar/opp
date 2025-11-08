namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Model for entity record dropdown options (for EntityID field)
/// </summary>
public class EntityRecordOption
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

