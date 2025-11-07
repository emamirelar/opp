namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Request model for bulk upserting Entity Artifacts from CSV
/// </summary>
public class BulkEntityArtifactRequest
{
    /// <summary>
    /// Entity type (e.g., "Country", "Partner", "OrganizationHierarchy")
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// CSV data rows for bulk import
    /// Each row contains: UniqueId, and values for each artifact type
    /// </summary>
    public required List<BulkEntityArtifactRowRequest> Rows { get; set; }

    /// <summary>
    /// Mapping of column index to ArtifactTypeId
    /// Key: Column index (0-based, starting from column 1 - column 0 is UniqueId)
    /// Value: ArtifactTypeId
    /// </summary>
    public required Dictionary<int, int> ColumnToArtifactTypeMapping { get; set; }
}

/// <summary>
/// Represents a single row of data for bulk import
/// </summary>
public class BulkEntityArtifactRowRequest
{
    /// <summary>
    /// Row number in the CSV (for error reporting)
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Unique identifier value for the entity (e.g., "AF" for Afghanistan, "B0047" for org unit)
    /// </summary>
    public required string UniqueId { get; set; }

    /// <summary>
    /// Dictionary of cell values
    /// Key: Column index (0-based, starting from column 1 - column 0 is UniqueId)
    /// Value: Cell value as string
    /// </summary>
    public required Dictionary<int, string> CellValues { get; set; }
}

