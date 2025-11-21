using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

public class Document : ModifiableDeletableEntity, IValidatableObject
{
    public string? Link { get; set; }
    public byte[]? Blob { get; set; }
    public string? StoragePath { get; set; }
    public string? GoogleId { get; set; }
    public string? Type { get; set; }
    public bool AITranscribed { get; set; } = false;
    public virtual ICollection<DocumentRelationship> DocumentRelationships { get; set; } = new HashSet<DocumentRelationship>();
    public int? DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public int? InteractionId { get; set; }

    /// <summary>
    /// Validates that at least one of Link, Blob, or StoragePath is provided
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Link) && Blob == null && string.IsNullOrWhiteSpace(StoragePath))
        {
            yield return new ValidationResult(
                "At least one of Link, Blob, or StoragePath must be provided.",
                new[] { nameof(Link), nameof(Blob), nameof(StoragePath) }
            );
        }
    }
}