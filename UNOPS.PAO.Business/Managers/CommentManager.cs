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

        // Reload the comment to get the CreatedBy field set by EF interceptor
        var savedComment = await repository.GetByIdAsync(comment.Id);
        if (savedComment == null)
        {
            throw new InvalidOperationException("Failed to retrieve saved comment");
        }

        // Create notifications for mentioned users (after comment is fully saved with audit fields)
        if (request.MentionedUserIds != null && request.MentionedUserIds.Any())
        {
            await CreateMentionNotificationsAsync(savedComment, request.MentionedUserIds);
        }

        // Get the created comment with creator name
        return (await GetCommentByIdAsync(savedComment.Id))!;
    }

    /// <summary>
    /// Create notifications for mentioned users
    /// </summary>
    private async Task CreateMentionNotificationsAsync(Comment comment, List<int> mentionedUserIds)
    {
        // Get current user info from the comment's CreatedBy field
        var currentUser = await context.PAOUsers
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == comment.CreatedBy);
        
        if (currentUser == null) return;

        var mentionedByName = currentUser.Email ?? "Someone";
        
        // Get the entity name for better message formatting
        var entityName = await GetEntityNameAsync(comment.EntityType, comment.EntityId);
        
        // Determine if this is a reply
        var isReply = comment.ParentCommentId.HasValue;
        var messageTemplate = isReply 
            ? $"You were tagged in a reply on {comment.EntityType} {entityName} by {mentionedByName}"
            : $"You were tagged in {comment.EntityType} {entityName} by {mentionedByName}";

        foreach (var userId in mentionedUserIds)
        {
            // Don't notify the user if they mentioned themselves
            if (userId == currentUser.Id) continue;

            var notification = new Notification
            {
                UserId = userId,
                Message = messageTemplate,
                Category = "collaboration",
                ResponseType = "Mention",
                Entity = comment.EntityType,
                EntityId = comment.EntityId,
                RecordData = string.Empty, // Not needed for mentions
                IsRead = false,
                Status = NotificationStatus.Done,
                CreatedAt = DateTime.UtcNow
            };

            context.Set<Notification>().Add(notification);
        }

        await context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Get entity name for notification message
    /// </summary>
    private async Task<string> GetEntityNameAsync(string entityType, int entityId)
    {
        try
        {
            switch (entityType.ToLower())
            {
                case "opportunity":
                    var opportunity = await context.Opportunities
                        .Where(o => o.Id == entityId && !o.IsDeleted)
                        .Select(o => o.Name)
                        .FirstOrDefaultAsync();
                    return opportunity ?? $"#{entityId}";
                    
                case "partner":
                    var partner = await context.Partners
                        .Where(p => p.Id == entityId && !p.IsDeleted)
                        .Select(p => p.Name)
                        .FirstOrDefaultAsync();
                    return partner ?? $"#{entityId}";
                    
                default:
                    return $"#{entityId}";
            }
        }
        catch
        {
            return $"#{entityId}";
        }
    }

    /// <summary>
    /// Update an existing comment
    /// </summary>
    public async Task<CommentModel?> UpdateCommentAsync(UpdateCommentRequest request)
    {
        var comment = await repository.GetByIdAsync(request.Id);
        if (comment == null || comment.IsDeleted)
        {
            throw new KeyNotFoundException($"Comment with ID {request.Id} not found");
        }

        // Note: Permission check should be done at the controller level
        // The audit fields (LastModifiedBy, LastModifiedDate) are automatically handled by EF interceptors

        comment.Content = request.Content;
        comment.MentionedUserIds = request.MentionedUserIds != null && request.MentionedUserIds.Any()
            ? string.Join(",", request.MentionedUserIds)
            : null;
        comment.IsEdited = true;

        await repository.UpdateAsync(comment);
        await context.SaveChangesAsync();

        return await GetCommentByIdAsync(comment.Id);
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

        // Note: Permission check should be done at the controller level
        // The audit fields (DeletedBy, DeletedDate) are automatically handled by EF interceptors

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
