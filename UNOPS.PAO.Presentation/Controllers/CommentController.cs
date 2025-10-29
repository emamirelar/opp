using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Presentation.Controllers;

/// <summary>
/// Controller for managing comments on entities
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly IManagerWrapper manager;
    private readonly IMapper mapper;

    public CommentController(IManagerWrapper manager, IMapper mapper)
    {
        this.manager = manager;
        this.mapper = mapper;
    }

    /// <summary>
    /// Get all comments for a specific entity
    /// </summary>
    /// <param name="entityType">The entity type (e.g., "Opportunity", "Partner")</param>
    /// <param name="entityId">The entity ID</param>
    /// <param name="includeReplies">Whether to include threaded replies</param>
    [HttpGet("{entityType}/{entityId}")]
    public async Task<IActionResult> GetCommentsByEntity(string entityType, int entityId, [FromQuery] bool includeReplies = true)
    {
        var comments = await manager.CommentManager.GetCommentsByEntityAsync(entityType, entityId, includeReplies);
        return Ok(comments);
    }

    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var comment = await manager.CommentManager.GetCommentByIdAsync(id);
        if (comment == null)
        {
            return NotFound();
        }
        return Ok(comment);
    }

    /// <summary>
    /// Create a new comment
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var comment = await manager.CommentManager.CreateCommentAsync(request);
        return CreatedAtAction(nameof(Get), new { id = comment.Id }, comment);
    }

    /// <summary>
    /// Update an existing comment
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var comment = await manager.CommentManager.UpdateCommentAsync(request);
            return Ok(comment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await manager.CommentManager.DeleteCommentAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ex.Message);
        }
    }

    /// <summary>
    /// Toggle pin status of a comment
    /// </summary>
    [HttpPost("{id}/toggle-pin")]
    public async Task<IActionResult> TogglePin(int id)
    {
        try
        {
            var isPinned = await manager.CommentManager.TogglePinAsync(id);
            return Ok(new { isPinned });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get comment count for an entity
    /// </summary>
    [HttpGet("{entityType}/{entityId}/count")]
    public async Task<IActionResult> GetCount(string entityType, int entityId)
    {
        var count = await manager.CommentManager.GetCommentCountAsync(entityType, entityId);
        return Ok(new { count });
    }
}

