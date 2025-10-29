namespace UNOPS.PAO.Models;

/// <summary>
/// Model for displaying a comment
/// </summary>
public class CommentModel
{
    public int Id { get; set; }
    public required string EntityType { get; set; }
    public int EntityId { get; set; }
    public required string Content { get; set; }
    public int? ParentCommentId { get; set; }
    public List<string>? MentionedUserNames { get; set; }
    public bool IsEdited { get; set; }
    public bool IsPinned { get; set; }
    
    // Audit fields
    public DateTime CreatedDate { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
    public string? LastModifiedByName { get; set; }
    
    // Navigation properties
    public List<CommentModel>? Replies { get; set; }
}

/// <summary>
/// Request model for creating a new comment
/// </summary>
public class CommentRequest
{
    public required string EntityType { get; set; }
    public int EntityId { get; set; }
    public required string Content { get; set; }
    public int? ParentCommentId { get; set; }
    public List<int>? MentionedUserIds { get; set; }
}

/// <summary>
/// Request model for updating an existing comment
/// </summary>
public class UpdateCommentRequest
{
    public int Id { get; set; }
    public required string Content { get; set; }
    public List<int>? MentionedUserIds { get; set; }
}

