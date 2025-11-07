namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Response model containing information about the unique identifier field for an entity type
/// Used to help users understand what to enter in the first column of bulk import CSV
/// </summary>
public class EntityUniqueIdExampleResponse
{
    /// <summary>
    /// Entity type
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// Name of the unique identifier field (e.g., "Iso2Code", "Code", "ErpDimValue")
    /// </summary>
    public required string UniqueIdFieldName { get; set; }

    /// <summary>
    /// User-friendly label for the field
    /// </summary>
    public required string UniqueIdFieldLabel { get; set; }

    /// <summary>
    /// Description of what this field represents
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Example value from an actual entity record
    /// </summary>
    public required string ExampleValue { get; set; }

    /// <summary>
    /// Name of the entity that has this example value
    /// </summary>
    public required string ExampleEntityName { get; set; }

    /// <summary>
    /// Full explanation with example for user guidance
    /// </summary>
    public string FullExplanation => $"{Description}. Example: \"{ExampleValue}\" for {ExampleEntityName}";
}

