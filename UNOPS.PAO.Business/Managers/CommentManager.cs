using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Managers;

/// <summary>
/// Manager for handling comment operations
/// </summary>
public class CommentManager : ICommentManager
{
    private readonly IMapper mapper;
    private readonly AppDbContext context;
    private readonly DataRepository<Comment> repository;
    private readonly IManagerWrapper managerWrapper;

    public CommentManager(IMapper mapper, AppDbContext context, IManagerWrapper managerWrapper)
    {
        this.mapper = mapper;
        this.context = context;
        this.repository = new DataRepository<Comment>(context);
        this.managerWrapper = managerWrapper;
    }

    /// <summary>
    /// Get all comments for a specific entity
    /// </summary>
    public async Task<IEnumerable<CommentModel>> GetCommentsByEntityAsync(string entityType, int entityId, bool includeReplies = true)
    {
        IQueryable<Comment> query = context.Comments
            .Where(c => c.EntityType == entityType && c.EntityId == entityId && c.ParentCommentId == null && !c.IsDeleted);

        if (includeReplies)
        {
            query = query.Include(c => c.Replies.Where(r => !r.IsDeleted).OrderBy(r => r.CreatedDate));
        }

        var comments = await query
            .OrderByDescending(c => c.IsPinned)
            .ThenByDescending(c => c.CreatedDate)
            .ToListAsync();

        // Map to models with user names
        var models = new List<CommentModel>();
        foreach (var comment in comments)
        {
            var model = mapper.Map<CommentModel>(comment);
            
            // Get creator name
            var creator = await managerWrapper.UserDataManager.GetUserByIdAsync(comment.CreatedBy);
            model.CreatedByName = creator?.Email ?? "Unknown User";
            
            // Map replies with user names
            if (includeReplies && comment.Replies.Any())
            {
                model.Replies = new List<CommentModel>();
                foreach (var reply in comment.Replies.Where(r => !r.IsDeleted))
                {
                    var replyModel = mapper.Map<CommentModel>(reply);
                    var replyCreator = await managerWrapper.UserDataManager.GetUserByIdAsync(reply.CreatedBy);
                    replyModel.CreatedByName = replyCreator?.Email ?? "Unknown User";
                    model.Replies.Add(replyModel);
                }
            }
            
            models.Add(model);
        }

        return models;
    }

    /// <summary>
    /// Get a specific comment by ID
    /// </summary>
    public async Task<CommentModel?> GetCommentByIdAsync(int id)
    {
        var comment = await context.Comments
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (comment == null) return null;

        var model = mapper.Map<CommentModel>(comment);
        
        // Get creator name
        var creator = await managerWrapper.UserDataManager.GetUserByIdAsync(comment.CreatedBy);
        model.CreatedByName = creator?.Email ?? "Unknown User";

        return model;
    }

    /// <summary>
    /// Create a new comment
    /// </summary>
    public async Task<CommentModel> CreateCommentAsync(CommentRequest request)
    {
        var comment = new Comment
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId,
            MentionedUserIds = request.MentionedUserIds != null && request.MentionedUserIds.Any()
                ? string.Join(",", request.MentionedUserIds)
                : null,
            IsEdited = false,
            IsPinned = false,
            Name = $"{request.EntityType}-Comment-{DateTime.UtcNow.Ticks}",
            Status = EntityStatus.Active
        };

        await repository.AddAsync(comment);
        await context.SaveChangesAsync();

        // Get the created comment with creator name
        return (await GetCommentByIdAsync(comment.Id))!;
    }

    /// <summary>
    /// Update an existing comment
    /// </summary>
    public async Task<CommentModel> UpdateCommentAsync(UpdateCommentRequest request)
    {
        var comment = await repository.GetByIdAsync(request.Id);
        if (comment == null || comment.IsDeleted)
        {
            throw new KeyNotFoundException($"Comment with ID {request.Id} not found");
        }

        // Get current user
        var currentUser = await managerWrapper.UserDataManager.GetCurrentUserAsync();
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // Only the creator can edit their comment
        if (comment.CreatedBy != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You can only edit your own comments");
        }

        comment.Content = request.Content;
        comment.MentionedUserIds = request.MentionedUserIds != null && request.MentionedUserIds.Any()
            ? string.Join(",", request.MentionedUserIds)
            : null;
        comment.IsEdited = true;

        await repository.UpdateAsync(comment);
        await context.SaveChangesAsync();

        return (await GetCommentByIdAsync(comment.Id))!;
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    public async Task<bool> DeleteCommentAsync(int id)
    {
        var comment = await repository.GetByIdAsync(id);
        if (comment == null || comment.IsDeleted)
        {
            throw new KeyNotFoundException($"Comment with ID {id} not found");
        }

        // Get current user
        var currentUser = await managerWrapper.UserDataManager.GetCurrentUserAsync();
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // Only the creator can delete their comment
        if (comment.CreatedBy != currentUser.Id)
        {
            throw new UnauthorizedAccessException("You can only delete your own comments");
        }

        // Soft delete
        comment.IsDeleted = true;
        await repository.UpdateAsync(comment);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Pin/unpin a comment
    /// </summary>
    public async Task<bool> TogglePinAsync(int id)
    {
        var comment = await repository.GetByIdAsync(id);
        if (comment == null || comment.IsDeleted)
        {
            throw new KeyNotFoundException($"Comment with ID {id} not found");
        }

        comment.IsPinned = !comment.IsPinned;
        await repository.UpdateAsync(comment);
        await context.SaveChangesAsync();

        return comment.IsPinned;
    }

    /// <summary>
    /// Get comment count for an entity
    /// </summary>
    public async Task<int> GetCommentCountAsync(string entityType, int entityId)
    {
        return await context.Comments
            .Where(c => c.EntityType == entityType && c.EntityId == entityId && !c.IsDeleted)
            .CountAsync();
    }
}
