namespace UNOPS.PAO.Models.Artifacts;

/// <summary>
/// Response model for bulk entity artifact operations
/// Contains results for each row and cell processed
/// </summary>
public class BulkEntityArtifactResponse
{
    /// <summary>
    /// Entity type that was processed
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// Total number of rows processed
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Number of rows successfully processed
    /// </summary>
    public int SuccessfulRows { get; set; }

    /// <summary>
    /// Number of rows with errors
    /// </summary>
    public int FailedRows { get; set; }

    /// <summary>
    /// Results for each row
    /// </summary>
    public required List<BulkEntityArtifactRowResult> RowResults { get; set; }
}

/// <summary>
/// Result for a single row in bulk import
/// </summary>
public class BulkEntityArtifactRowResult
{
    /// <summary>
    /// Row number in the CSV
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Unique identifier from the CSV
    /// </summary>
    public required string UniqueId { get; set; }

    /// <summary>
    /// Entity ID found/resolved for this unique identifier
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Entity name for display
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Whether the row was processed successfully overall
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if row-level error occurred
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Results for each cell/column in the row
    /// </summary>
    public required List<BulkEntityArtifactCellResult> CellResults { get; set; }
}

/// <summary>
/// Result for a single cell/artifact in bulk import
/// </summary>
public class BulkEntityArtifactCellResult
{
    /// <summary>
    /// Column index
    /// </summary>
    public int ColumnIndex { get; set; }

    /// <summary>
    /// Artifact type ID
    /// </summary>
    public int ArtifactTypeId { get; set; }

    /// <summary>
    /// Artifact type name for display
    /// </summary>
    public string? ArtifactTypeName { get; set; }

    /// <summary>
    /// Whether the cell was processed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if cell processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Previous value before import (null if newly created)
    /// </summary>
    public string? PreviousValue { get; set; }

    /// <summary>
    /// Current value after import
    /// </summary>
    public string? CurrentValue { get; set; }

    /// <summary>
    /// Whether this was a new artifact (created) vs updated
    /// </summary>
    public bool IsNew { get; set; }

    /// <summary>
    /// Whether the value was skipped (because it was empty and would overwrite existing data)
    /// </summary>
    public bool Skipped { get; set; }
}

