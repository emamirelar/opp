using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;

namespace UNOPS.PAO.Presentation.Controllers.Shared;

[Route(APIDictionary.APIPrefix)]
[ApiController]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly IManagerWrapper _manager;
    private readonly ILogger<CommentController> _logger;

    public CommentController(IManagerWrapper manager, ILogger<CommentController> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    /// <summary>
    /// Get all comments for a specific entity
    /// </summary>
    [HttpGet(APIDictionary.CommentsByEntity)]
    public async Task<ActionResult<IEnumerable<CommentModel>>> GetCommentsByEntity(
        string entityType,
        int entityId,
        [FromQuery] bool includeReplies = true)
    {
        try
        {
            var comments = await _manager.CommentManager.GetCommentsByEntityAsync(entityType, entityId, includeReplies);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, new { error = "Internal server error while retrieving comments", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    [HttpGet(APIDictionary.Comment + "/{id}")]
    public async Task<ActionResult<CommentModel>> GetCommentById(int id)
    {
        try
        {
            var comment = await _manager.CommentManager.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound(new { error = $"Comment with ID {id} not found" });
            }
            return Ok(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment {CommentId}", id);
            return StatusCode(500, new { error = "Internal server error while retrieving comment", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new comment
    /// </summary>
    [HttpPost(APIDictionary.Comment)]
    public async Task<ActionResult<CommentModel>> CreateComment([FromBody] CommentRequest request)
    {
        try
        {
            var comment = await _manager.CommentManager.CreateCommentAsync(request);
            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating comment for {EntityType} {EntityId}", request.EntityType, request.EntityId);
            return StatusCode(500, new { error = "Internal server error while creating comment", details = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing comment
    /// </summary>
    [HttpPut(APIDictionary.Comment + "/{id}")]
    public async Task<ActionResult<CommentModel>> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            request.Id = id;
            var comment = await _manager.CommentManager.UpdateCommentAsync(request);
            if (comment == null)
            {
                return NotFound(new { error = $"Comment with ID {id} not found" });
            }
            return Ok(comment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment {CommentId}", id);
            return StatusCode(500, new { error = "Internal server error while updating comment", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    [HttpDelete(APIDictionary.Comment + "/{id}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        try
        {
            var success = await _manager.CommentManager.DeleteCommentAsync(id);
            if (!success)
            {
                return NotFound(new { error = $"Comment with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", id);
            return StatusCode(500, new { error = "Internal server error while deleting comment", details = ex.Message });
        }
    }

    /// <summary>
    /// Toggle pin status of a comment
    /// </summary>
    [HttpPost(APIDictionary.CommentTogglePin)]
    public async Task<ActionResult> TogglePin(int id)
    {
        try
        {
            var isPinned = await _manager.CommentManager.TogglePinAsync(id);
            return Ok(new { isPinned });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling pin for comment {CommentId}", id);
            return StatusCode(500, new { error = "Internal server error while toggling pin", details = ex.Message });
        }
    }

    /// <summary>
    /// Get comment count for an entity
    /// </summary>
    [HttpGet(APIDictionary.CommentCount)]
    public async Task<ActionResult> GetCommentCount(string entityType, int entityId)
    {
        try
        {
            var count = await _manager.CommentManager.GetCommentCountAsync(entityType, entityId);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comment count for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, new { error = "Internal server error while getting comment count", details = ex.Message });
        }
    }
}

