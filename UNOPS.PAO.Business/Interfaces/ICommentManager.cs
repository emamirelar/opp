using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces;

/// <summary>
/// Interface for Comment business logic operations
/// </summary>
public interface ICommentManager
{
    /// <summary>
    /// Get all comments for a specific entity
    /// </summary>
    Task<IEnumerable<CommentModel>> GetCommentsByEntityAsync(string entityType, int entityId, bool includeReplies = true);

    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    Task<CommentModel?> GetCommentByIdAsync(int id);

    /// <summary>
    /// Create a new comment
    /// </summary>
    Task<CommentModel> CreateCommentAsync(CommentRequest request);

    /// <summary>
    /// Update an existing comment
    /// </summary>
    Task<CommentModel?> UpdateCommentAsync(UpdateCommentRequest request);

    /// <summary>
    /// Delete a comment
    /// </summary>
    Task<bool> DeleteCommentAsync(int id);

    /// <summary>
    /// Toggle pin status of a comment
    /// </summary>
    Task<bool> TogglePinAsync(int id);

    /// <summary>
    /// Get comment count for an entity
    /// </summary>
    Task<int> GetCommentCountAsync(string entityType, int entityId);
}
