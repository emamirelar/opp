using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Represents a comment or collaboration note on any entity in the system
/// </summary>
public class Comment : ModifiableDeletableEntity<int, int>
{
    /// <summary>
    /// The type of entity this comment is attached to (e.g., "Opportunity", "Partner", "Contact")
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// The ID of the entity this comment is attached to
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// The comment content/text
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Optional parent comment ID for threaded replies
    /// </summary>
    public int? ParentCommentId { get; set; }

    /// <summary>
    /// Navigation property for parent comment (for threaded discussions)
    /// </summary>
    public virtual Comment? ParentComment { get; set; }

    /// <summary>
    /// Navigation property for child comments (replies)
    /// </summary>
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

    /// <summary>
    /// User IDs mentioned in this comment (e.g., @mentions)
    /// Stored as comma-separated string for simplicity
    /// </summary>
    public string? MentionedUserIds { get; set; }

    /// <summary>
    /// Whether this comment has been edited after creation
    /// </summary>
    public bool IsEdited { get; set; }

    /// <summary>
    /// Whether this comment is pinned to the top
    /// </summary>
    public bool IsPinned { get; set; }
}

