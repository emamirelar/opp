namespace UNOPS.PAO.Models.Shared;

public class OutputModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? OutputGroup { get; set; }
    public string? OutputSubGroup { get; set; }
    public string? OutputName { get; set; }
    public string? Description { get; set; }
    public int? UnitId { get; set; }
    public string? UnitName { get; set; }
    public int? ProjectCategoryId { get; set; }
    public string? ProjectCategoryName { get; set; }
    public string? OutputServiceLine { get; set; }
}


