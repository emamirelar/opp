using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Defines the data types available for artifacts (string, number, document, map, array, json, date)
/// </summary>
public class ArtifactDataType : ModifiableDeletableEntity
{
    public new int Id { get; set; }

    /// <summary>
    /// Data type name (e.g., "string", "number", "document", "map", "array", "json", "date")
    /// </summary>
    [MaxLength(50)]
    public new required string Name { get; set; }

    /// <summary>
    /// Description of the data type
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int Order { get; set; }

    // Navigation property
    public virtual ICollection<ArtifactType> ArtifactTypes { get; set; } = new HashSet<ArtifactType>();
}

