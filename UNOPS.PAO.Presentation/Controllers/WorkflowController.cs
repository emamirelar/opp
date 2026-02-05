using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers.Shared;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Models;
using UNOPS.Workflow.Models.Requirements;
using Microsoft.EntityFrameworkCore;

namespace UNOPS.PAO.Presentation.Controllers;

/// <summary>
/// API endpoints for workflow operations (stage transitions, approvals, history).
/// Includes custom handling for Opportunity workflow:
/// - Non-OM submitter warning
/// - Country-org unit mismatch warning
/// - Custom rejection → NO GO stage
/// - Cancel and Reopen actions
/// - Internal stakeholder notification on Go Decision
/// </summary>
[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class WorkflowController : BaseController
{
    private readonly IWorkflowManager _workflowManager;
    private readonly IEntityStageProvider _entityStageProvider;
    private readonly IPaoWorkflowApproverProvider _approverProvider;
    private readonly IEnumerable<IStageRequirementsProvider> _requirementsProviders;
    private readonly IManagerWrapper _managerWrapper;
    private readonly AppDbContext _context;
    private readonly PaoWorkflowNotificationService _notificationService;

    public WorkflowController(
        ILogger<WorkflowController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService,
        IWorkflowManager workflowManager,
        IEntityStageProvider entityStageProvider,
        IPaoWorkflowApproverProvider approverProvider,
        IEnumerable<IStageRequirementsProvider> requirementsProviders,
        IManagerWrapper managerWrapper,
        AppDbContext context,
        PaoWorkflowNotificationService notificationService)
        : base(logger, authorizationService, userResolverService)
    {
        _workflowManager = workflowManager;
        _entityStageProvider = entityStageProvider;
        _approverProvider = approverProvider;
        _requirementsProviders = requirementsProviders;
        _managerWrapper = managerWrapper;
        _context = context;
        _notificationService = notificationService;
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
            
            // Check if current user can recall (submitter OR Opportunity Manager for Opportunities)
            var isInitiator = pendingTask.UserId == CurrentUserId;
            var isOMForRecall = normalizedEntityName == "Opportunity" 
                ? await IsUserOpportunityManagerAsync(id, CurrentUserId) 
                : false;
            response.CanRecall = isInitiator || isOMForRecall;
        }

        return Ok(response);
    }

    /// <summary>
    /// Gets stage requirements for a workflow transition.
    /// Used by frontend to display validation requirements before submission.
    /// </summary>
    /// <param name="entityName">The entity type name</param>
    /// <param name="id">The entity ID</param>
    /// <param name="nextStage">Optional target stage (defaults to next available stage)</param>
    /// <returns>List of stage requirements</returns>
    [HttpGet(APIDictionary.Workflow + "/{entityName}/{id}/requirements/{nextStage?}")]
    public async Task<ActionResult<List<StageRequirement>>> GetRequirementsForStageChange(
        string entityName, 
        int id, 
        string? nextStage = null)
    {
        var stateMachine = GetStateMachine(entityName);
        if (stateMachine == null)
        {
            return NotFound(new { error = $"Workflow not found for entity type '{entityName}'" });
        }

        // Normalize entity name
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

        // If nextStage not provided, determine from available actions
        if (string.IsNullOrEmpty(nextStage))
        {
            var currentState = _workflowManager.WorkflowStateByStage(stateMachine, currentStage, Facing.Internal);
            if (currentState != null)
            {
                var actions = _workflowManager.NextActions(normalizedEntityName, currentState, Facing.Internal);
                var firstAction = actions.FirstOrDefault();
                if (firstAction != null)
                {
                    nextStage = firstAction.NewStage;
                }
            }
        }

        if (string.IsNullOrEmpty(nextStage))
        {
            return Ok(new List<StageRequirement>());
        }

        // Find the requirements provider for this entity
        var provider = _requirementsProviders.FirstOrDefault(p => 
            p.EntityNames.Any(n => n.Equals(normalizedEntityName, StringComparison.OrdinalIgnoreCase)));
        
        if (provider == null)
        {
            return Ok(new List<StageRequirement>());
        }

        // Get requirements for the stage change
        var requirements = provider.GetRequirementsForStageChange(currentStage, nextStage);
        
        // Filter out server-side only requirements (they should not be displayed to users)
        var clientRequirements = requirements.Where(r => !r.OnlyServerSideEvaluation).ToList();
        return Ok(clientRequirements);
    }

    /// <summary>
    /// Submits an entity for workflow stage change.
    /// For Opportunity submissions to GO stage, includes:
    /// - Non-OM submitter warning
    /// - Country-org unit mismatch warning
    /// - Mandatory acknowledgment statement
    /// - Opportunity statement regeneration
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
        // NOTE: For Opportunity Go Decision flow, we skip the generic comment check here
        // because the PRD flow handles remarks differently (optional additional remarks in acknowledgment dialog)
        var commentRequired = targetAction.Comment?.Equals("mandatory", StringComparison.OrdinalIgnoreCase) == true;
        var isOpportunityGoFlow = normalizedEntityName == "Opportunity" && request.NewStage == OpportunityWorkflow.Stages.Go;
        if (commentRequired && string.IsNullOrWhiteSpace(request.Comment) && !isOpportunityGoFlow)
        {
            return BadRequest(new { error = "Comment is required for this transition" });
        }

        // === OPPORTUNITY-SPECIFIC CHECKS FOR GO TRANSITION ===
        if (normalizedEntityName == "Opportunity" && request.NewStage == OpportunityWorkflow.Stages.Go)
        {
            // Get opportunity with all related data for validation
            var opportunity = await _context.Opportunities
                .Include(o => o.ResponsibleOrgUnit)
                .Include(o => o.Countries)
                .Include(o => o.SDGs)
                .Include(o => o.FundingPartners)
                .Include(o => o.ClientPartners)
                .Include(o => o.Deliverables)
                .Include(o => o.UNOPSMissions)
                .Include(o => o.Stakeholders)
                    .ThenInclude(s => s.EntityRole)
                .FirstOrDefaultAsync(o => o.Id == request.EntityId && !o.IsDeleted);

            // PRD Flow Step 1: Check if all requirements are met (FIRST check)
            var unmetRequirements = await ValidateOpportunityRequirementsAsync(opportunity);
            if (unmetRequirements.Any())
            {
                return Ok(new WorkflowSubmitResponse
                {
                    Success = false,
                    RequirementsNotMet = true,
                    UnmetRequirements = unmetRequirements
                });
            }

            // 1. Non-OM Submitter Warning
            var isOM = await IsUserOpportunityManagerAsync(request.EntityId, CurrentUserId);
            if (!isOM && !request.ConfirmedNonOMSubmission)
            {
                var userRole = await GetUserRoleOnOpportunityAsync(request.EntityId, CurrentUserId);
                var omInfo = await GetOpportunityManagerInfoAsync(request.EntityId);
                return Ok(new WorkflowSubmitResponse
                {
                    Success = false,
                    RequiresConfirmation = true,
                    ConfirmationType = "NonOMSubmitter",
                    ConfirmationMessage = $"You currently hold a [{userRole ?? "stakeholder"}] role on this opportunity. " +
                        "The Opportunity Manager is typically responsible for submitting for Go Decision. " +
                        "Are you sure you want to proceed with this submission?",
                    OpportunityManagerInfo = omInfo
                });
            }

            // 2. Country-Org Unit Mismatch Warning
            var unrelatedCountries = await GetUnrelatedCountriesAsync(request.EntityId);
            if (unrelatedCountries.Any() && !request.ConfirmedOrgUnitWarning)
            {
                var orgUnitName = opportunity?.ResponsibleOrgUnit?.Name ?? "the selected org unit";
                var countryMappings = await GetCountryMappingsAsync(request.EntityId);
                return Ok(new WorkflowSubmitResponse
                {
                    Success = false,
                    RequiresConfirmation = true,
                    ConfirmationType = "OrgUnitCountryMismatch",
                    ConfirmationMessage = $"The org unit '{orgUnitName}' is not normally responsible for the following countries: " +
                        $"{string.Join(", ", unrelatedCountries)}. Are you sure you want to proceed?",
                    UnrelatedCountries = unrelatedCountries,
                    CountryMappings = countryMappings,
                    ResponsibleOrgUnitName = orgUnitName
                });
            }

            // 3. Mandatory Acknowledgment Statement
            if (!request.AcknowledgedStatement)
            {
                var orgUnitDisplay = opportunity?.ResponsibleOrgUnit != null 
                    ? $"{opportunity.ResponsibleOrgUnit.Code} - {opportunity.ResponsibleOrgUnit.Name}"
                    : "the responsible org unit";
                
                return Ok(new WorkflowSubmitResponse
                {
                    Success = false,
                    RequiresAcknowledgment = true,
                    ResponsibleOrgUnitName = orgUnitDisplay,
                    AcknowledgmentText = $"All known information and materials relevant to this Opportunity have been provided " +
                        $"and are summarized in the Opportunity Statement for your review. Please confirm whether UNOPS org unit " +
                        $"[{orgUnitDisplay}] is authorised to assign resources to continue development based on this information."
                });
            }

            // 4. Regenerate Opportunity Statement before submission
            try
            {
                await _managerWrapper.GeminiManager.GenerateOpportunityStatementAsync(request.EntityId, User, saveToDatabase: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to regenerate opportunity statement for {OpportunityId}", request.EntityId);
                // Don't block submission if statement generation fails
            }
        }

        // Check if approval is needed
        var approvalRequired = _workflowManager.ApprovalNeeded(normalizedEntityName, currentStage, request.NewStage);
        
        // Get entity display name for notifications
        var entityDisplayName = await _entityStageProvider.GetEntityDisplayNameAsync(normalizedEntityName, request.EntityId.ToString());
        var entityUrl = $"/opportunity/{request.EntityId}#statement"; // Include anchor to statement section

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
    /// Approves a pending workflow with enhanced Go decision requirements.
    /// For Opportunity approvals: Requires rationale, confirmation acknowledgment, and Executive assignment.
    /// </summary>
    /// <param name="request">The enhanced approval request with rationale, confirmation, and Executive</param>
    /// <returns>Approval result</returns>
    [HttpPost(APIDictionary.Workflow + "/approve")]
    public async Task<ActionResult> Approve([FromBody] ApproveWorkflowRequest request)
    {
        // Normalize entity name for workflow manager consistency
        var normalizedEntityName = NormalizeEntityNameForWorkflow(request.EntityName);

        // === ENHANCED VALIDATION FOR GO DECISION ===
        
        // Validate rationale is provided (required)
        if (string.IsNullOrWhiteSpace(request.Rationale))
        {
            return BadRequest(new { error = "Decision rationale is required" });
        }

        // Validate confirmation acknowledged (required)
        if (!request.ConfirmationAcknowledged)
        {
            return BadRequest(new { error = "Confirmation statement must be acknowledged" });
        }

        // Validate Executive is assigned for Opportunity approvals (required)
        if (normalizedEntityName == "Opportunity" && request.ExecutiveId <= 0)
        {
            return BadRequest(new { error = "Executive assignment is required for Go decision" });
        }
        
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

        // Approve the workflow (rationale stored in comment field)
        var newStage = await _workflowManager.Approve(
            pendingTask,
            normalizedEntityName,
            request.EntityId,
            entityDisplayName,
            request.Rationale,  // Decision rationale stored as comment
            entityUrl);

        if (string.IsNullOrEmpty(newStage))
        {
            return StatusCode(500, new { error = "Failed to approve workflow" });
        }

        // === ASSIGN EXECUTIVE TO OPPORTUNITY (NEW) ===
        if (normalizedEntityName == "Opportunity" && request.ExecutiveId > 0)
        {
            await _managerWrapper.OpportunityManager.AssignExecutiveAsync(request.EntityId, request.ExecutiveId);
        }

        // Update entity stage
        await _entityStageProvider.UpdateStageAsync(normalizedEntityName, request.EntityId.ToString(), newStage, CurrentUserId);

        // Update entity WorkflowStatus back to None (approval complete)
        await UpdateEntityWorkflowStatus(normalizedEntityName, request.EntityId, isInWorkflow: false);

        // === INTERNAL STAKEHOLDER NOTIFICATION (FR-11) ===
        // When an opportunity moves to GO stage, notify internal stakeholders from other org units
        if (normalizedEntityName == "Opportunity" && newStage == OpportunityWorkflow.Stages.Go)
        {
            var currentUserName = await GetCurrentUserNameAsync();
            await _notificationService.NotifyInternalStakeholdersOnGoDecisionAsync(request.EntityId, currentUserName);
        }

        // === MARK IN-SYSTEM NOTIFICATIONS AS DONE ===
        await _notificationService.MarkWorkflowNotificationsAsApprovedAsync(normalizedEntityName, request.EntityId);

        return Ok(new { success = true, message = "Workflow approved", newStage });
    }

    /// <summary>
    /// Rejects a pending workflow with enhanced No-Go decision requirements.
    /// For Opportunities: Custom behavior - rejection sets stage to NO GO (not previous stage).
    /// Requires rationale and confirmation acknowledgment.
    /// </summary>
    /// <param name="request">The enhanced rejection request with rationale and confirmation</param>
    /// <returns>Rejection result</returns>
    [HttpPost(APIDictionary.Workflow + "/reject")]
    public async Task<ActionResult> Reject([FromBody] RejectWorkflowRequest request)
    {
        // Normalize entity name for workflow manager consistency
        var normalizedEntityName = NormalizeEntityNameForWorkflow(request.EntityName);

        // === ENHANCED VALIDATION FOR NO-GO DECISION ===
        
        // Validate rationale is provided (required)
        if (string.IsNullOrWhiteSpace(request.Rationale))
        {
            return BadRequest(new { error = "Decision rationale is required" });
        }

        // Validate confirmation acknowledged (required)
        if (!request.ConfirmationAcknowledged)
        {
            return BadRequest(new { error = "Confirmation statement must be acknowledged" });
        }
        
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

        // === CUSTOM REJECTION FOR OPPORTUNITIES ===
        // Rejection sets stage to NO GO instead of returning to previous stage
        if (normalizedEntityName == "Opportunity")
        {
            var opportunity = await _context.Opportunities.FindAsync(request.EntityId);
            if (opportunity != null)
            {
                // Set stage to NO GO (custom rejection behavior)
                opportunity.Stage = OpportunityWorkflow.Stages.NoGo;
                opportunity.WorkflowStatus = WorkflowStatus.None;
                opportunity.LastModifiedBy = CurrentUserId;
                opportunity.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log the rejection with the NO GO stage (rationale stored in comment)
                await _workflowManager.AddLog(new WorkflowLogModel
                {
                    EntityName = normalizedEntityName,
                    EntityId = request.EntityId.ToString(),
                    Stage = currentStage,
                    NewStage = OpportunityWorkflow.Stages.NoGo,
                    Comment = request.Rationale,  // Decision rationale stored as comment
                    Action = "Rejected",
                    UserId = CurrentUserId,
                    CompletedOn = DateTime.UtcNow
                });

                // Complete the pending workflow task
                await _workflowManager.Reject(
                    pendingTask,
                    normalizedEntityName,
                    request.EntityId,
                    entityDisplayName,
                    request.Rationale,  // Decision rationale
                    entityUrl);

                // === MARK IN-SYSTEM NOTIFICATIONS AS DONE ===
                await _notificationService.MarkWorkflowNotificationsAsRejectedAsync(normalizedEntityName, request.EntityId);

                return Ok(new WorkflowActionResponse 
                { 
                    Success = true, 
                    Message = "Opportunity has been set to NO GO", 
                    NewStage = OpportunityWorkflow.Stages.NoGo 
                });
            }
        }

        // Standard rejection for other entity types
        var success = await _workflowManager.Reject(
            pendingTask,
            normalizedEntityName,
            request.EntityId,
            entityDisplayName,
            request.Rationale,  // Decision rationale
            entityUrl);

        if (!success)
        {
            return StatusCode(500, new { error = "Failed to reject workflow" });
        }

        // Update entity WorkflowStatus back to None (rejection complete)
        await UpdateEntityWorkflowStatus(normalizedEntityName, request.EntityId, isInWorkflow: false);

        // === MARK IN-SYSTEM NOTIFICATIONS AS DONE ===
        await _notificationService.MarkWorkflowNotificationsAsRejectedAsync(normalizedEntityName, request.EntityId);

        return Ok(new WorkflowActionResponse { Success = true, Message = "Workflow rejected" });
    }

    /// <summary>
    /// Recalls (cancels) a pending workflow submission.
    /// For Opportunities: Both the submitter AND the Opportunity Manager can recall.
    /// Requires mandatory justification comment.
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

        // Require mandatory justification comment
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(new { error = "Justification is required when recalling a workflow submission" });
        }

        // Check if user is the one who initiated OR is the Opportunity Manager (for Opportunities)
        var isInitiator = pendingTask.UserId == CurrentUserId;
        var isOM = normalizedEntityName == "Opportunity" 
            ? await IsUserOpportunityManagerAsync(request.EntityId, CurrentUserId) 
            : false;

        if (!isInitiator && !isOM)
        {
            return StatusCode(403, new { error = "Only the submitter or Opportunity Manager can recall this workflow" });
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
            request.Comment,
            entityUrl);

        if (!success)
        {
            return StatusCode(500, new { error = "Failed to recall workflow" });
        }

        // Update entity WorkflowStatus back to None (recall complete)
        await UpdateEntityWorkflowStatus(normalizedEntityName, request.EntityId, isInWorkflow: false);

        // === MARK IN-SYSTEM NOTIFICATIONS AS DONE ===
        await _notificationService.MarkWorkflowNotificationsAsRecalledAsync(normalizedEntityName, request.EntityId);

        return Ok(new WorkflowActionResponse { Success = true, Message = "Workflow recalled successfully" });
    }

    /// <summary>
    /// Cancels an opportunity. Only available to Opportunity Manager from IDENTIFY & PROFILE stage.
    /// Sets the opportunity to CANCELLED stage and marks entity as Closed.
    /// </summary>
    /// <param name="request">The cancel request</param>
    /// <returns>Cancel result</returns>
    [HttpPost(APIDictionary.Workflow + "/cancel")]
    public async Task<ActionResult<WorkflowActionResponse>> Cancel([FromBody] WorkflowCancelRequest request)
    {
        // Normalize entity name
        var normalizedEntityName = NormalizeEntityNameForWorkflow(request.EntityName);

        // Only support Opportunity cancellation
        if (normalizedEntityName != "Opportunity")
        {
            return BadRequest(new { error = "Cancel action is only supported for Opportunities" });
        }

        // Comment is required
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(new { error = "Comment is required when cancelling an opportunity" });
        }

        // Get the opportunity
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == request.EntityId && !o.IsDeleted);

        if (opportunity == null)
        {
            return NotFound(new { error = $"Opportunity with ID {request.EntityId} not found" });
        }

        // Validate: only from IDENTIFY & PROFILE stage
        if (opportunity.Stage != OpportunityWorkflow.Stages.IdentifyAndProfile)
        {
            return BadRequest(new { error = "Opportunity can only be cancelled from IDENTIFY & PROFILE stage" });
        }

        // Validate: only Opportunity Manager can cancel
        var isOM = await IsUserOpportunityManagerAsync(request.EntityId, CurrentUserId);
        if (!isOM)
        {
            return StatusCode(403, new { error = "Only the Opportunity Manager can cancel an opportunity" });
        }

        // Check if in workflow (cannot cancel while in approval process)
        var pendingTask = _workflowManager.PendingTask(normalizedEntityName, request.EntityId);
        if (pendingTask != null)
        {
            return BadRequest(new { error = "Cannot cancel opportunity while it is in a workflow approval process. Please recall the submission first." });
        }

        // Update opportunity
        var previousStage = opportunity.Stage;
        opportunity.Stage = OpportunityWorkflow.Stages.Cancelled;
        opportunity.Status = EntityStatus.Closed;
        opportunity.WorkflowStatus = WorkflowStatus.None;
        opportunity.LastModifiedBy = CurrentUserId;
        opportunity.LastModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log the action in workflow history
        await _workflowManager.AddLog(new WorkflowLogModel
        {
            EntityName = normalizedEntityName,
            EntityId = request.EntityId.ToString(),
            Stage = previousStage,
            NewStage = OpportunityWorkflow.Stages.Cancelled,
            Comment = request.Comment,
            Action = "Cancelled",
            UserId = CurrentUserId,
            CompletedOn = DateTime.UtcNow
        });

        return Ok(new WorkflowActionResponse 
        { 
            Success = true, 
            Message = "Opportunity has been cancelled", 
            NewStage = OpportunityWorkflow.Stages.Cancelled 
        });
    }

    /// <summary>
    /// Reopens an opportunity. Only available to Opportunity Manager from NO GO or CANCELLED stage.
    /// Sets the opportunity back to IDENTIFY & PROFILE stage.
    /// </summary>
    /// <param name="request">The reopen request</param>
    /// <returns>Reopen result</returns>
    [HttpPost(APIDictionary.Workflow + "/reopen")]
    public async Task<ActionResult<WorkflowActionResponse>> Reopen([FromBody] WorkflowReopenRequest request)
    {
        // Normalize entity name
        var normalizedEntityName = NormalizeEntityNameForWorkflow(request.EntityName);

        // Only support Opportunity reopening
        if (normalizedEntityName != "Opportunity")
        {
            return BadRequest(new { error = "Reopen action is only supported for Opportunities" });
        }

        // Get the opportunity
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == request.EntityId && !o.IsDeleted);

        if (opportunity == null)
        {
            return NotFound(new { error = $"Opportunity with ID {request.EntityId} not found" });
        }

        // Validate: only from NO GO or CANCELLED stage
        var isFromNoGo = opportunity.Stage == OpportunityWorkflow.Stages.NoGo;
        var isFromCancelled = opportunity.Stage == OpportunityWorkflow.Stages.Cancelled;

        if (!isFromNoGo && !isFromCancelled)
        {
            return BadRequest(new { error = "Opportunity can only be reopened from NO GO or CANCELLED stage" });
        }

        // Comment required when reopening from CANCELLED
        if (isFromCancelled && string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(new { error = "Comment is required when reopening from CANCELLED stage" });
        }

        // Validate: only Opportunity Manager can reopen
        var isOM = await IsUserOpportunityManagerAsync(request.EntityId, CurrentUserId);
        if (!isOM)
        {
            return StatusCode(403, new { error = "Only the Opportunity Manager can reopen an opportunity" });
        }

        // Update opportunity
        var previousStage = opportunity.Stage;
        opportunity.Stage = OpportunityWorkflow.Stages.IdentifyAndProfile;
        opportunity.Status = EntityStatus.Active;
        opportunity.WorkflowStatus = WorkflowStatus.None;
        opportunity.LastModifiedBy = CurrentUserId;
        opportunity.LastModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Log the action in workflow history
        await _workflowManager.AddLog(new WorkflowLogModel
        {
            EntityName = normalizedEntityName,
            EntityId = request.EntityId.ToString(),
            Stage = previousStage,
            NewStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
            Comment = request.Comment ?? string.Empty,
            Action = "Reopened",
            UserId = CurrentUserId,
            CompletedOn = DateTime.UtcNow
        });

        return Ok(new WorkflowActionResponse 
        { 
            Success = true, 
            Message = $"Opportunity has been reopened from {previousStage}", 
            NewStage = OpportunityWorkflow.Stages.IdentifyAndProfile 
        });
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
    /// Gets pending workflow approval tasks for the current user.
    /// Returns only tasks where the current user is authorized to approve.
    /// Used by the Actions Required card on the home dashboard.
    /// </summary>
    /// <returns>List of pending approval tasks</returns>
    [HttpGet(APIDictionary.Workflow + "/pending-approvals")]
    public async Task<ActionResult<IEnumerable<PendingApprovalResponse>>> GetPendingApprovals()
    {
        var pendingApprovals = new List<PendingApprovalResponse>();

        // Get all pending workflow tasks
        var allPendingTasks = await _workflowManager.GetAllPendingTasksAsync();

        foreach (var task in allPendingTasks)
        {
            // Parse entity ID
            if (!int.TryParse(task.EntityId, out int entityId))
                continue;

            // Normalize entity name
            var entityNameLower = task.EntityName.ToLowerInvariant();

            // Get current stage for the entity
            var currentStage = await _entityStageProvider.GetCurrentStageAsync(entityNameLower, task.EntityId);
            if (string.IsNullOrEmpty(currentStage))
                continue;

            // Check if current user can approve this task
            var canApprove = await _approverProvider.CanUserApproveAsync(
                task.EntityName, entityId, CurrentUserId, currentStage, task.NewStage);

            if (!canApprove)
                continue;

            // Get state machine for stage display names
            var stateMachine = GetStateMachine(entityNameLower);

            // Build approval response with entity details
            var approvalResponse = new PendingApprovalResponse
            {
                EntityName = task.EntityName,
                EntityId = entityId,
                CurrentStage = currentStage,
                CurrentStageDisplayName = stateMachine?.StageNames.TryGetValue(currentStage, out var currentName) == true 
                    ? currentName : currentStage,
                PendingStage = task.NewStage,
                PendingStageDisplayName = stateMachine?.StageNames.TryGetValue(task.NewStage, out var pendingName) == true 
                    ? pendingName : task.NewStage,
                SubmittedOn = task.CreatedDate,
                SubmittedByUserId = task.UserId
            };

            // Get entity-specific details
            if (entityNameLower == "opportunity")
            {
                var opportunity = await _context.Opportunities
                    .AsNoTracking()
                    .Include(o => o.ResponsibleOrgUnit)
                    .FirstOrDefaultAsync(o => o.Id == entityId && !o.IsDeleted);

                if (opportunity != null)
                {
                    approvalResponse.EntityDisplayName = opportunity.Name;
                    approvalResponse.OrgUnitName = opportunity.ResponsibleOrgUnit?.Name;
                    approvalResponse.EntityUrl = $"/opportunity/{entityId}";
                }
            }

            // Get submitter display name
            if (task.UserId > 0)
            {
                var submitter = await _context.PAOUsers
                    .AsNoTracking()
                    .Include(u => u.UserProfile)
                    .FirstOrDefaultAsync(u => u.Id == task.UserId);

                if (submitter != null)
                {
                    approvalResponse.SubmittedBy = submitter.UserProfile?.Name ?? submitter.Email;
                }
            }

            pendingApprovals.Add(approvalResponse);
        }

        // Sort by submitted date descending (most recent first)
        return Ok(pendingApprovals.OrderByDescending(p => p.SubmittedOn));
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

    /// <summary>
    /// Checks if a user is the Opportunity Manager for an opportunity.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="userId">The user ID to check</param>
    /// <returns>True if the user is an Opportunity Manager</returns>
    private async Task<bool> IsUserOpportunityManagerAsync(int opportunityId, int userId)
    {
        return await _context.Set<OpportunityStakeholder>()
            .Include(s => s.EntityRole)
            .AnyAsync(s => s.OpportunityId == opportunityId &&
                          s.UserId == userId &&
                          s.EntityRole != null &&
                          s.EntityRole.Name == "Opportunity Manager");
    }

    /// <summary>
    /// Gets the user's role on an opportunity (for warning messages).
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <param name="userId">The user ID</param>
    /// <returns>The user's role name, or null if not a stakeholder</returns>
    private async Task<string?> GetUserRoleOnOpportunityAsync(int opportunityId, int userId)
    {
        var stakeholder = await _context.Set<OpportunityStakeholder>()
            .Include(s => s.EntityRole)
            .FirstOrDefaultAsync(s => s.OpportunityId == opportunityId &&
                                     s.UserId == userId &&
                                     s.EntityRole != null);
        
        return stakeholder?.EntityRole?.Name;
    }

    /// <summary>
    /// Gets the Opportunity Manager's name and email for display in the Non-OM warning dialog.
    /// The Opportunity Manager is stored in OpportunityStakeholders with role code "Opportunity_Manager_Opportunity".
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <returns>Formatted string with OM name and email, or empty string if not found</returns>
    private async Task<string> GetOpportunityManagerInfoAsync(int opportunityId)
    {
        var omStakeholder = await _context.OpportunityStakeholders
            .AsNoTracking()
            .Include(s => s.EntityRole)
            .Include(s => s.User)
            .Where(s => s.OpportunityId == opportunityId
                     && !s.IsDeleted
                     && s.EntityRole != null
                     && s.EntityRole.Code == "Opportunity_Manager_Opportunity"
                     && s.User != null)
            .FirstOrDefaultAsync();

        if (omStakeholder?.User == null)
        {
            return string.Empty;
        }

        var om = omStakeholder.User;
        var name = $"{om.Name}".Trim();
        var email = om.Email ?? string.Empty;
        
        return !string.IsNullOrEmpty(email) 
            ? $"{name} ({email})" 
            : name;
    }

    /// <summary>
    /// Gets list of countries on the opportunity that are not in the org unit's normal relationships.
    /// Used for country-org unit mismatch warning.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <returns>List of country names that don't match the org unit's relationships</returns>
    private async Task<List<string>> GetUnrelatedCountriesAsync(int opportunityId)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.Countries)
                .ThenInclude(oc => oc.Country)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

        if (opportunity == null || !opportunity.ResponsibleOrgUnitId.HasValue)
        {
            return new List<string>();
        }

        // Get country IDs that the org unit is normally responsible for
        var orgUnitCountryIds = await _context.Set<OrganizationUnitRelationship>()
            .Where(r => r.OrganizationHierarchyId == opportunity.ResponsibleOrgUnitId.Value &&
                       r.EntityType == "Country" &&
                       !r.IsDeleted)
            .Select(r => r.EntityId)
            .ToListAsync();

        // Find countries on the opportunity that are not in the org unit's relationships
        var unrelatedCountries = opportunity.Countries
            .Where(oc => oc.Country != null && !orgUnitCountryIds.Contains(oc.CountryId))
            .Select(oc => oc.Country!.Name)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();

        return unrelatedCountries!;
    }

    /// <summary>
    /// Gets all implementation countries with their mapping status for the org unit mismatch dialog.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID</param>
    /// <returns>List of CountryMappingInfo with country name and mapping status</returns>
    private async Task<List<CountryMappingInfo>> GetCountryMappingsAsync(int opportunityId)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.Countries)
                .ThenInclude(oc => oc.Country)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && !o.IsDeleted);

        if (opportunity == null || !opportunity.ResponsibleOrgUnitId.HasValue)
        {
            return new List<CountryMappingInfo>();
        }

        // Get country IDs that the org unit is normally responsible for
        var orgUnitCountryIds = await _context.Set<OrganizationUnitRelationship>()
            .Where(r => r.OrganizationHierarchyId == opportunity.ResponsibleOrgUnitId.Value &&
                       r.EntityType == "Country" &&
                       !r.IsDeleted)
            .Select(r => r.EntityId)
            .ToListAsync();

        // Build mapping info for all implementation countries
        var countryMappings = opportunity.Countries
            .Where(oc => oc.Country != null && !string.IsNullOrEmpty(oc.Country.Name))
            .Select(oc => new CountryMappingInfo
            {
                CountryName = oc.Country!.Name!,
                IsMapped = orgUnitCountryIds.Contains(oc.CountryId)
            })
            .OrderBy(cm => cm.CountryName)
            .ToList();

        return countryMappings;
    }

    /// <summary>
    /// Gets the current user's display name.
    /// </summary>
    private async Task<string> GetCurrentUserNameAsync()
    {
        var user = await _context.PAOUsers
            .AsNoTracking()
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

        if (user?.UserProfile != null)
        {
            var fullName = $"{user.UserProfile.FirstName} {user.UserProfile.LastName}".Trim();
            return !string.IsNullOrEmpty(fullName) ? fullName : user.Email ?? "User";
        }

        return user?.Email ?? "User";
    }

    /// <summary>
    /// Validates opportunity requirements for GO transition.
    /// Based on PRD FR-2.1: 21 mandatory fields must be met before submission.
    /// </summary>
    /// <param name="opportunity">The opportunity entity with all related data loaded</param>
    /// <returns>List of unmet requirement message keys</returns>
    private async Task<List<string>> ValidateOpportunityRequirementsAsync(Opportunity? opportunity)
    {
        var unmetRequirements = new List<string>();

        if (opportunity == null)
        {
            unmetRequirements.Add("Opportunity not found");
            return unmetRequirements;
        }

        // === Text Fields (Required) ===
        if (string.IsNullOrWhiteSpace(opportunity.Name))
            unmetRequirements.Add("message.requirements.opportunity.nameRequired");

        if (string.IsNullOrWhiteSpace(opportunity.Description))
            unmetRequirements.Add("message.requirements.opportunity.descriptionRequired");

        if (string.IsNullOrWhiteSpace(opportunity.Challenges))
            unmetRequirements.Add("message.requirements.opportunity.challengesRequired");

        if (string.IsNullOrWhiteSpace(opportunity.ExpectedImpact))
            unmetRequirements.Add("message.requirements.opportunity.impactRequired");

        if (string.IsNullOrWhiteSpace(opportunity.ExpectedOutcomes))
            unmetRequirements.Add("message.requirements.opportunity.outcomesRequired");

        if (string.IsNullOrWhiteSpace(opportunity.OpportunityStatementMarkdown))
            unmetRequirements.Add("message.requirements.opportunity.statementRequired");

        // === Number Fields (Required) ===
        if (!opportunity.InitiativeBudgetUSD.HasValue || opportunity.InitiativeBudgetUSD <= 0)
            unmetRequirements.Add("message.requirements.opportunity.budgetRequired");

        // === Array Fields (minLength = 1) ===
        // Note: Junction tables (FundingPartners, ClientPartners, etc.) don't have IsDeleted property
        // UNOPS Missions: Either at least one mission selected OR marked as "Not Applicable"
        if (!opportunity.UNOPSMissionsNotApplicable && (opportunity.UNOPSMissions == null || !opportunity.UNOPSMissions.Any()))
            unmetRequirements.Add("message.requirements.opportunity.missionsRequired");

        if (opportunity.SDGs == null || !opportunity.SDGs.Any())
            unmetRequirements.Add("message.requirements.opportunity.sdgRequired");

        if (opportunity.FundingPartners == null || !opportunity.FundingPartners.Any())
            unmetRequirements.Add("message.requirements.opportunity.fundingPartnerRequired");

        if (opportunity.ClientPartners == null || !opportunity.ClientPartners.Any())
            unmetRequirements.Add("message.requirements.opportunity.clientPartnerRequired");

        if (opportunity.Deliverables == null || !opportunity.Deliverables.Any())
            unmetRequirements.Add("message.requirements.opportunity.productsRequired");

        if (opportunity.Countries == null || !opportunity.Countries.Any())
            unmetRequirements.Add("message.requirements.opportunity.countriesRequired");

        // === Date Fields (Required) ===
        if (!opportunity.TargetSigningDate.HasValue)
            unmetRequirements.Add("message.requirements.opportunity.signingDateRequired");

        if (!opportunity.ImplementationStartDate.HasValue)
            unmetRequirements.Add("message.requirements.opportunity.startDateRequired");

        if (!opportunity.TargetDeliveryDate.HasValue)
            unmetRequirements.Add("message.requirements.opportunity.endDateRequired");

        // === Select Fields (Required) ===
        if (!opportunity.ResponsibleOrgUnitId.HasValue || opportunity.ResponsibleOrgUnitId <= 0)
            unmetRequirements.Add("message.requirements.opportunity.orgUnitRequired");

        if (!opportunity.ProposedInitiativeTypeId.HasValue || opportunity.ProposedInitiativeTypeId <= 0)
            unmetRequirements.Add("message.requirements.opportunity.initiativeTypeRequired");

        // === Custom/Conditional Fields ===

        // Beneficiaries: Either TBD is true OR (DirectBeneficiaries > 0 AND IndirectBeneficiaries >= 0)
        var beneficiariesValid = opportunity.BeneficiariesToBeDetermined == true ||
            (opportunity.EstimatedDirectBeneficiaries > 0 && opportunity.EstimatedIndirectBeneficiaries >= 0);
        if (!beneficiariesValid)
            unmetRequirements.Add("message.requirements.opportunity.beneficiariesRequired");

        // Opportunity Manager: At least one stakeholder with "Opportunity Manager" role
        // Note: OpportunityStakeholder doesn't have IsDeleted, and uses EntityRole instead of Role
        var hasOpportunityManager = opportunity.Stakeholders != null &&
            opportunity.Stakeholders.Any(s => 
                s.EntityRole != null && 
                s.EntityRole.Name.Equals("Opportunity Manager", StringComparison.OrdinalIgnoreCase));
        if (!hasOpportunityManager)
            unmetRequirements.Add("message.requirements.opportunity.managerRequired");

        // DoA Level 2 Holder: Server-side only validation
        // EntityUserRole inherits from ModifiableDeletableEntity so it has IsDeleted
        // It uses EntityRole instead of Role
        if (opportunity.ResponsibleOrgUnitId.HasValue)
        {
            var hasDoAHolder = await _context.EntityUserRoles
                .AnyAsync(eur => 
                    eur.EntityType == "OrganizationHierarchy" &&
                    eur.EntityId == opportunity.ResponsibleOrgUnitId.Value &&
                    eur.EntityRole != null &&
                    eur.EntityRole.Code == "DoA2_OrganizationHierarchy" &&
                    !eur.IsDeleted);

            if (!hasDoAHolder)
                unmetRequirements.Add("message.requirements.opportunity.doaHolderRequired");
        }
        else
        {
            // If no org unit is selected, DoA holder check fails
            unmetRequirements.Add("message.requirements.opportunity.doaHolderRequired");
        }

        return unmetRequirements;
    }
}
