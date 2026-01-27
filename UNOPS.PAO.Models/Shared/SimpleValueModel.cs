namespace UNOPS.PAO.Models.Shared;

public class SimpleValueModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Region { get; set; }  // For countries
    public string? Continent { get; set; }  // For countries
    public string? Type { get; set; }  // For entity roles - role type classification
    public string? SubType { get; set; }  // For entity roles - role subtype classification
    public string? Position { get; set; }  // For users - standardized position title from personnel record
}


