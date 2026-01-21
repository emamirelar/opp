using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Models;
using Microsoft.EntityFrameworkCore;

namespace UNOPS.PAO.Presentation.Controllers;

/// <summary>
/// API endpoints for workflow operations (stage transitions, approvals, history).
/// </summary>
[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class WorkflowController : BaseController
{
    private readonly IWorkflowManager _workflowManager;
    private readonly IEntityStageProvider _entityStageProvider;
    private readonly IPaoWorkflowApproverProvider _approverProvider;
    private readonly AppDbContext _context;

    public WorkflowController(
        ILogger<WorkflowController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService,
        IWorkflowManager workflowManager,
        IEntityStageProvider entityStageProvider,
        IPaoWorkflowApproverProvider approverProvider,
        AppDbContext context)
        : base(logger, authorizationService, userResolverService)
    {
        _workflowManager = workflowManager;
        _entityStageProvider = entityStageProvider;
        _approverProvider = approverProvider;
        _context = context;
    }

    /// <summary>
    /// Gets workflow stage configuration for an entity type.
    /// </summary>
    /// <param name="entityName">The entity type name (e.g., "opportunity")</param>
    /// <returns>List of workflow stages</returns>
    [HttpGet(APIDictionary.Workflow + "/{entityName}")]
    public ActionResult<IEnumerable<WorkflowStageConfigResponse>> GetWorkflowStages(string entityName)
    {
        var stateMachine = GetStateMachine(entityName);
        if (stateMachine == null)
        {
            return NotFound(new { error = $"Workflow not found for entity type '{entityName}'" });
        }

        var stages = stateMachine.States.Select(s => new WorkflowStageConfigResponse
        {
            StageCode = s.StageCode,
            DisplayName = s.DisplayName,
            Sequence = s.Sequence
        }).OrderBy(s => s.Sequence).ToList();

        return Ok(stages);
    }

    /// <summary>
    /// Gets current workflow state and available actions for an entity.
    /// </summary>
    /// <param name="entityName">The entity type name</param>
    /// <param name="id">The entity ID</param>
    /// <returns>Current workflow state with available actions</returns>
    [HttpGet(APIDictionary.Workflow + "/{entityName}/{id}")]
    public async Task<ActionResult<WorkflowStateResponse>> GetWorkflowState(string entityName, int id)
    {
        var stateMachine = GetStateMachine(entityName);
        if (stateMachine == null)
        {
            return NotFound(new { error = $"Workflow not found for entity type '{entityName}'" });
        }

        // Normalize entity name to match database format (e.g., "opportunity" -> "Opportunity")
        var normalizedEntityName = NormalizeEntityNameForWorkflow(entityName);

        // Verify entity exists
        var entityValid = await _entityStageProvider.IsEntityValidAsync(entityName, id.ToString());
        if (!entityValid)
        {
            return NotFound(new { error = $"{entityName} with ID {id} not found" });
        }

        // Get current stage
        var currentStage = await _entityStageProvider.GetCurrentStageAsync(entityName, id.ToString());
        if (string.IsNullOrEmpty(currentStage))
        {
            return BadRequest(new { error = "Entity has no workflow stage" });
        }

        // Get pending workflow task
        var pendingTask = _workflowManager.PendingTask(normalizedEntityName, id);
        var isInWorkflow = pendingTask != null;

        // Get current state object
        var currentState = _workflowManager.WorkflowStateByStage(stateMachine, currentStage, Facing.Internal);
        var stageDisplayName = stateMachine.StageNames.TryGetValue(currentStage, out var name) ? name : currentStage;

        // Get available actions (if not already in workflow)
        var availableActions = new List<UNOPS.PAO.Models.Workflow.WorkflowActionModel>();
        if (!isInWorkflow && currentState != null)
        {
            var actions = _workflowManager.NextActions(normalizedEntityName, currentState, Facing.Internal);
            foreach (var action in actions)
            {
                // Check if user can trigger this transition (not approve)
                // Use GetTriggerConfigurationAsync to check CanTrigger permissions
                var triggerConfig = await _approverProvider.GetTriggerConfigurationAsync(normalizedEntityName, id, currentStage, action.NewStage);
                var canTrigger = triggerConfig.HasValue && 
                                 triggerConfig.Value.triggers.Any(t => t.UserId == CurrentUserId);
                
                if (canTrigger)
                {
                    availableActions.Add(new UNOPS.PAO.Models.Workflow.WorkflowActionModel
                    {
                        TargetStage = action.NewStage,
                        DisplayName = action.ActionName ?? action.NewStage,
                        RequiresApproval = _workflowManager.ApprovalNeeded(normalizedEntityName, currentStage, action.NewStage),
                        CommentRequired = action.Comment?.Equals("mandatory", StringComparison.OrdinalIgnoreCase) == true,
                        CommentOptional = action.Comment?.Equals("optional", StringComparison.OrdinalIgnoreCase) == true
                    });
                }
            }
        }

        return Ok(new WorkflowStateResponse
        {
            CurrentStage = currentStage,
            CurrentStageDisplayName = stageDisplayName,
            IsInWorkflow = isInWorkflow,
            PendingStage = pendingTask?.NewStage,
            AvailableActions = availableActions
        });
    }

    /// <summary>
    /// Gets detailed workflow information for an entity including approvers.
    /// </summary>
    /// <param name="entityName">The entity type name</param>
    /// <param name="id">The entity ID</param>
    /// <returns>Detailed workflow information</returns>
    [HttpGet(APIDictionary.Workflow + "/{entityName}/{id}/details")]
    public async Task<ActionResult<WorkflowDetailsResponse>> GetWorkflowDetails(string entityName, int id)
    {
        var stateMachine = GetStateMachine(entityName);
        if (stateMachine == null)
        {
            return NotFound(new { error = $"Workflow not found for entity type '{entityName}'" });
        }

        // Normalize entity name for workflow manager consistency
        var normalizedEntityName = NormalizeEntityNameForWorkflow(entityName);

        // Verify entity exists
        var entityValid = await _entityStageProvider.IsEntityValidAsync(entityName, id.ToString());
        if (!entityValid)
        {
            return NotFound(new { error = $"{entityName} with ID {id} not found" });
        }

        // Get current stage
        var currentStage = await _entityStageProvider.GetCurrentStageAsync(entityName, id.ToString());
        if (string.IsNullOrEmpty(currentStage))
        {
            return BadRequest(new { error = "Entity has no workflow stage" });
        }

        var stageDisplayName = stateMachine.StageNames.TryGetValue(currentStage, out var name) ? name : currentStage;

        // Get pending workflow task (use normalized entity name)
        var pendingTask = _workflowManager.PendingTask(normalizedEntityName, id);
        var isInWorkflow = pendingTask != null;

        var response = new WorkflowDetailsResponse
        {
            CurrentStage = currentStage,
            CurrentStageDisplayName = stageDisplayName,
            IsInWorkflow = isInWorkflow
        };

        if (pendingTask != null)
        {
            response.PendingStage = pendingTask.NewStage;
            response.PendingStageDisplayName = stateMachine.StageNames.TryGetValue(pendingTask.NewStage ?? "", out var pendingName) ? pendingName : pendingTask.NewStage;
            response.InitiatedOn = pendingTask.CreatedDate;
            
            // Get initiator details
            if (pendingTask.UserId > 0)
            {
                var initiator = await _context.PAOUsers
                    .Include(u => u.UserProfile)
                    .FirstOrDefaultAsync(u => u.Id == pendingTask.UserId);
                
                if (initiator != null)
                {
                    response.InitiatedBy = new WorkflowUserResponse
                    {
                        UserId = initiator.Id,
                        UserName = initiator.UserProfile?.Name ?? initiator.Email,
                        UserEmail = initiator.Email
                    };
                }
            }

            // Get approvers (use normalized entity name)
            var approvers = await _approverProvider.GetApproversAsync(normalizedEntityName, id, currentStage, pendingTask.NewStage ?? "");
            response.Approvers = approvers.Select(a => new WorkflowApproverResponse
            {
                UserId = a.UserId,
                UserName = a.Name ?? $"{a.FirstName} {a.LastName}".Trim(),
                UserEmail = a.Email,
                RoleName = a.Role
            }).ToList();

            // Check if current user can approve (use normalized entity name)
            response.CanApprove = await _approverProvider.CanUserApproveAsync(normalizedEntityName, id, CurrentUserId, currentStage, pendingTask.NewStage ?? "");
            
            // Check if current user can recall (must be the initiator)
            response.CanRecall = pendingTask.UserId == CurrentUserId;
        }

        return Ok(response);
    }

    /// <summary>
    /// Submits an entity for workflow stage change.
    /// </summary>
    /// <param name="request">The submit request</param>
    /// <returns>Submit result</returns>
    [HttpPost(APIDictionary.Workflow + "/submit")]
    public async Task<ActionResult<WorkflowSubmitResponse>> Submit([FromBody] WorkflowSubmitRequest request)
    {
        // Normalize entity name for workflow manager consistency
        var normalizedEntityName = NormalizeEntityNameForWorkflow(request.EntityName);
        
        var stateMachine = GetStateMachine(normalizedEntityName);
        if (stateMachine == null)
        {
            return NotFound(new { error = $"Workflow not found for entity type '{normalizedEntityName}'" });
        }

        // Verify entity exists
        var entityValid = await _entityStageProvider.IsEntityValidAsync(normalizedEntityName, request.EntityId.ToString());
        if (!entityValid)
        {
            return NotFound(new { error = $"{normalizedEntityName} with ID {request.EntityId} not found" });
        }

        // Get current stage
        var currentStage = await _entityStageProvider.GetCurrentStageAsync(normalizedEntityName, request.EntityId.ToString());
        if (string.IsNullOrEmpty(currentStage))
        {
            return BadRequest(new { error = "Entity has no workflow stage" });
        }

        // Check if already in workflow
        var pendingTask = _workflowManager.PendingTask(normalizedEntityName, request.EntityId);
        if (pendingTask != null)
        {
            return BadRequest(new { error = "Entity is already in a workflow approval process" });
        }

        // Validate the transition is allowed
        var currentState = _workflowManager.WorkflowStateByStage(stateMachine, currentStage, Facing.Internal);
        if (currentState == null)
        {
            return BadRequest(new { error = $"Invalid current stage '{currentStage}'" });
        }

        var actions = _workflowManager.NextActions(normalizedEntityName, currentState, Facing.Internal);
        var targetAction = actions.FirstOrDefault(a => a.NewStage.Equals(request.NewStage, StringComparison.OrdinalIgnoreCase));
        if (targetAction == null)
        {
            return BadRequest(new { error = $"Transition from '{currentStage}' to '{request.NewStage}' is not allowed" });
        }

        // Check comment requirement
        var commentRequired = targetAction.Comment?.Equals("mandatory", StringComparison.OrdinalIgnoreCase) == true;
        if (commentRequired && string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(new { error = "Comment is required for this transition" });
        }

        // Check if approval is needed
        var approvalRequired = _workflowManager.ApprovalNeeded(normalizedEntityName, currentStage, request.NewStage);
        
        // Get entity display name for notifications
        var entityDisplayName = await _entityStageProvider.GetEntityDisplayNameAsync(normalizedEntityName, request.EntityId.ToString());
        var entityUrl = $"/opportunity/{request.EntityId}"; // TODO: Make dynamic based on entity type

        if (approvalRequired)
        {
            // Create pending workflow log entry
            await _workflowManager.AddLog(new WorkflowLogModel
            {
                EntityName = normalizedEntityName,
                EntityId = request.EntityId.ToString(),
                Stage = currentStage,
                NewStage = request.NewStage,
                Comment = request.Comment ?? string.Empty,
                Action = "Submit",
                Status = UNOPS.Workflow.Domain.Enums.EntityStatus.Active, // Active with CompletedOn=null indicates pending
                UserId = CurrentUserId,
                RequiresApproval = true,
                CompletedOn = null // Not completed yet
            });

            // Update entity WorkflowStatus to InWorkflow
            await UpdateEntityWorkflowStatus(normalizedEntityName, request.EntityId, isInWorkflow: true);

            // Send approval notifications
            await _workflowManager.Initiate(
                new UNOPS.Workflow.Models.WorkflowActionModel
                {
                    EntityName = normalizedEntityName,
                    Id = request.EntityId,
                    Action = "Submit",
                    NewStage = request.NewStage,
                    Comment = request.Comment ?? string.Empty
                },
                currentStage,
                entityUrl,
                entityDisplayName);

            return Ok(new WorkflowSubmitResponse
            {
                Success = true,
                Message = "Submitted for approval",
                ApprovalRequired = true,
                PendingStage = request.NewStage
            });
        }
        else
        {
            // Direct transition (no approval needed)
            var success = await _entityStageProvider.UpdateStageAsync(normalizedEntityName, request.EntityId.ToString(), request.NewStage, CurrentUserId);
            if (!success)
            {
                return StatusCode(500, new { error = "Failed to update entity stage" });
            }

            // Log the transition
            await _workflowManager.AddLog(new WorkflowLogModel
            {
                EntityName = normalizedEntityName,
                EntityId = request.EntityId.ToString(),
                Stage = currentStage,
                NewStage = request.NewStage,
                Comment = request.Comment ?? string.Empty,
                Action = "StageChanged",
                UserId = CurrentUserId,
                CompletedOn = DateTime.UtcNow
            });

            return Ok(new WorkflowSubmitResponse
            {
                Success = true,
                Message = "Stage changed successfully",
                ApprovalRequired = false,
                NewStage = request.NewStage
            });
        }
    }

    /// <summary>
    /// Approves a pending workflow.
    /// </summary>
    /// <param name="request">The approval request</param>
    /// <returns>Approval result</returns>
    [HttpPost(APIDictionary.Workflow + "/approve")]
    public async Task<ActionResult> Approve([FromBody] WorkflowActionRequest request)
    {
        // Normalize entity name for workflow manager consistency
        var normalizedEntityName = NormalizeEntityNameForWorkflow(request.EntityName);
        
        // Get pending task
        var pendingTask = _workflowManager.PendingTask(normalizedEntityName, request.EntityId);
        if (pendingTask == null)
        {
            return BadRequest(new { error = "No pending workflow found for this entity" });
        }

        // Check if user can approve
        var currentStage = await _entityStageProvider.GetCurrentStageAsync(normalizedEntityName, request.EntityId.ToString());
        var canApprove = await _approverProvider.CanUserApproveAsync(normalizedEntityName, request.EntityId, CurrentUserId, currentStage ?? "", pendingTask.NewStage ?? "");
        if (!canApprove)
        {
            return StatusCode(403, new { error = "You do not have permission to approve this workflow" });
        }

        // Get entity display name for notifications
        var entityDisplayName = await _entityStageProvider.GetEntityDisplayNameAsync(normalizedEntityName, request.EntityId.ToString());
        var entityUrl = $"/opportunity/{request.EntityId}";

        // Approve the workflow
        var newStage = await _workflowManager.Approve(
            pendingTask,
            normalizedEntityName,
            request.EntityId,
            entityDisplayName,
            request.Comment ?? "",
            entityUrl);

        if (string.IsNullOrEmpty(newStage))
        {
            return StatusCode(500, new { error = "Failed to approve workflow" });
        }

        // Update entity stage
        await _entityStageProvider.UpdateStageAsync(normalizedEntityName, request.EntityId.ToString(), newStage, CurrentUserId);

        // Update entity WorkflowStatus back to None (approval complete)
        await UpdateEntityWorkflowStatus(normalizedEntityName, request.EntityId, isInWorkflow: false);

        return Ok(new { success = true, message = "Workflow approved", newStage });
    }

    /// <summary>
    /// Rejects a pending workflow.
    /// </summary>
    /// <param name="request">The rejection request</param>
    /// <returns>Rejection result</returns>
    [HttpPost(APIDictionary.Workflow + "/reject")]
    public async Task<ActionResult> Reject([FromBody] WorkflowActionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(new { error = "Comment is required when rejecting a workflow" });
        }

        // Normalize entity name for workflow manager consistency
        var normalizedEntityName = NormalizeEntityNameForWorkflow(request.EntityName);
        
        // Get pending task
        var pendingTask = _workflowManager.PendingTask(normalizedEntityName, request.EntityId);
        if (pendingTask == null)
        {
            return BadRequest(new { error = "No pending workflow found for this entity" });
        }

        // Check if user can approve/reject
        var currentStage = await _entityStageProvider.GetCurrentStageAsync(normalizedEntityName, request.EntityId.ToString());
        var canApprove = await _approverProvider.CanUserApproveAsync(normalizedEntityName, request.EntityId, CurrentUserId, currentStage ?? "", pendingTask.NewStage ?? "");
        if (!canApprove)
        {
            return StatusCode(403, new { error = "You do not have permission to reject this workflow" });
        }

        // Get entity display name for notifications
        var entityDisplayName = await _entityStageProvider.GetEntityDisplayNameAsync(normalizedEntityName, request.EntityId.ToString());
        var entityUrl = $"/opportunity/{request.EntityId}";

        // Reject the workflow
        var success = await _workflowManager.Reject(
            pendingTask,
            normalizedEntityName,
            request.EntityId,
            entityDisplayName,
            request.Comment,
            entityUrl);

        if (!success)
        {
            return StatusCode(500, new { error = "Failed to reject workflow" });
        }

        // Update entity WorkflowStatus back to None (rejection complete)
        await UpdateEntityWorkflowStatus(normalizedEntityName, request.EntityId, isInWorkflow: false);

        return Ok(new { success = true, message = "Workflow rejected" });
    }

    /// <summary>
    /// Recalls (cancels) a pending workflow submission.
    /// </summary>
    /// <param name="request">The recall request</param>
    /// <returns>Recall result</returns>
    [HttpPost(APIDictionary.Workflow + "/recall")]
    public async Task<ActionResult> Recall([FromBody] WorkflowRecallRequest request)
    {
        // Normalize entity name for workflow manager consistency
        var normalizedEntityName = NormalizeEntityNameForWorkflow(request.EntityName);
        
        // Get pending task
        var pendingTask = _workflowManager.PendingTask(normalizedEntityName, request.EntityId);
        if (pendingTask == null)
        {
            return BadRequest(new { error = "No pending workflow found for this entity" });
        }

        // Check if user is the one who initiated
        if (pendingTask.UserId != CurrentUserId)
        {
            return StatusCode(403, new { error = "Only the user who initiated the workflow can recall it" });
        }

        // Get entity display name for notifications
        var entityDisplayName = await _entityStageProvider.GetEntityDisplayNameAsync(normalizedEntityName, request.EntityId.ToString());
        var entityUrl = $"/opportunity/{request.EntityId}";

        // Recall the workflow
        var success = await _workflowManager.Recall(
            pendingTask,
            normalizedEntityName,
            request.EntityId,
            entityDisplayName,
            request.Comment ?? "",
            entityUrl);

        if (!success)
        {
            return StatusCode(500, new { error = "Failed to recall workflow" });
        }

        // Update entity WorkflowStatus back to None (recall complete)
        await UpdateEntityWorkflowStatus(normalizedEntityName, request.EntityId, isInWorkflow: false);

        return Ok(new { success = true, message = "Workflow recalled" });
    }

    /// <summary>
    /// Gets workflow history for an entity.
    /// </summary>
    /// <param name="entityName">The entity type name</param>
    /// <param name="id">The entity ID</param>
    /// <returns>List of workflow history entries</returns>
    [HttpGet(APIDictionary.Workflow + "/{entityName}/{id}/history")]
    public async Task<ActionResult<IEnumerable<WorkflowHistoryResponse>>> GetWorkflowHistory(string entityName, int id)
    {
        var stateMachine = GetStateMachine(entityName);
        if (stateMachine == null)
        {
            return NotFound(new { error = $"Workflow not found for entity type '{entityName}'" });
        }

        // Normalize entity name for workflow manager consistency
        var normalizedEntityName = NormalizeEntityNameForWorkflow(entityName);

        // Verify entity exists
        var entityValid = await _entityStageProvider.IsEntityValidAsync(entityName, id.ToString());
        if (!entityValid)
        {
            return NotFound(new { error = $"{entityName} with ID {id} not found" });
        }

        // Get workflow history (use normalized entity name)
        var history = _workflowManager.GetWorkflowHistory(stateMachine, normalizedEntityName, id);

        // Map to response with user details
        var response = new List<WorkflowHistoryResponse>();
        foreach (var entry in history)
        {
            var historyEntry = new WorkflowHistoryResponse
            {
                FromStage = entry.FromStage,
                ToStage = entry.ToStage,
                FromStageDisplayName = !string.IsNullOrEmpty(entry.FromStage) && stateMachine.StageNames.TryGetValue(entry.FromStage, out var fromName) ? fromName : entry.FromStage,
                ToStageDisplayName = !string.IsNullOrEmpty(entry.ToStage) && stateMachine.StageNames.TryGetValue(entry.ToStage, out var toName) ? toName : entry.ToStage,
                Action = entry.Action,
                PerformedOn = entry.CompletedOn,
                Comment = entry.Comment
            };

            // Get user details from the User property or look up in database
            var userId = entry.User?.Id ?? 0;
            if (userId > 0)
            {
                var user = await _context.PAOUsers
                    .Include(u => u.UserProfile)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null)
                {
                    historyEntry.PerformedBy = new WorkflowUserResponse
                    {
                        UserId = user.Id,
                        UserName = user.UserProfile?.Name ?? user.Email,
                        UserEmail = user.Email
                    };
                }
            }

            response.Add(historyEntry);
        }

        return Ok(response);
    }

    /// <summary>
    /// Gets the state machine for an entity type.
    /// </summary>
    private StateMachine? GetStateMachine(string entityName)
    {
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => OpportunityWorkflow.StateMachine,
            _ => null
        };
    }

    /// <summary>
    /// Normalizes entity name to match database storage format.
    /// Database stores entity names with proper casing (e.g., "Opportunity" not "opportunity").
    /// </summary>
    private static string NormalizeEntityNameForWorkflow(string entityName)
    {
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => "Opportunity",
            _ => entityName
        };
    }

    /// <summary>
    /// Updates the WorkflowStatus property of an entity.
    /// </summary>
    /// <param name="entityName">The entity type name (normalized, e.g., "Opportunity")</param>
    /// <param name="entityId">The entity ID</param>
    /// <param name="isInWorkflow">True to set WorkflowStatus to InWorkflow, false for None</param>
    private async Task UpdateEntityWorkflowStatus(string entityName, int entityId, bool isInWorkflow)
    {
        switch (entityName)
        {
            case "Opportunity":
                var opportunity = await _context.Opportunities
                    .FirstOrDefaultAsync(o => o.Id == entityId && !o.IsDeleted);
                if (opportunity != null)
                {
                    opportunity.WorkflowStatus = isInWorkflow 
                        ? UNOPS.PAO.Domain.Enums.WorkflowStatus.InWorkflow 
                        : UNOPS.PAO.Domain.Enums.WorkflowStatus.None;
                    opportunity.LastModifiedBy = CurrentUserId;
                    opportunity.LastModifiedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                break;
            // Add other entity types here as needed
            default:
                throw new NotImplementedException($"WorkflowStatus update not implemented for entity type: {entityName}");
        }
    }
}
