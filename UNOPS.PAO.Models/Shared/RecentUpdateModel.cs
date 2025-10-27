namespace UNOPS.PAO.Models.Shared;

/// <summary>
/// Model for representing recent updates across different entity types
/// Used by the dashboard to show a unified view of recent activities
/// </summary>
public class RecentUpdateModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // Partner, Contact, Interaction
    public DateTime? LastModifiedDate { get; set; }
    public int LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
    public string Status { get; set; }
    public object? EntityData { get; set; } // Contains the full entity data for detailed views
}
