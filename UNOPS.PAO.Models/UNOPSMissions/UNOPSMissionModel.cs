namespace UNOPS.PAO.Models;

/// <summary>
/// Model representing a UNOPS Strategic Mission
/// </summary>
public class UNOPSMissionModel
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public string Status { get; set; } = "Active";
}

