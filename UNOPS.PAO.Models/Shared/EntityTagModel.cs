namespace UNOPS.PAO.Models.Shared;

/// <summary>
/// Generic entity tag model for conditional status/condition tags
/// Can be used across all entities (Partner, Contact, Interaction, etc.)
/// </summary>
public class EntityTagModel
{
    public string Tag { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
