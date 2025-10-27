namespace UNOPS.PAO.Models.Filters;

public class TypeaheadInput
{
    public required string Label { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
}