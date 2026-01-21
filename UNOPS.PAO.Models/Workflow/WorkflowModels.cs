namespace UNOPS.PAO.Models.Workflow;

/// <summary>
/// Request model for submitting a workflow stage change.
/// </summary>
public class WorkflowSubmitRequest
{
    /// <summary>
    /// The entity type name (e.g., "Opportunity")
    /// </summary>
    public required string EntityName { get; set; }
    
    /// <summary>
    /// The entity ID
    /// </summary>
    public required int EntityId { get; set; }
    
    /// <summary>
    /// The target stage to transition to
    /// </summary>
    public required string NewStage { get; set; }
    
    /// <summary>
    /// Optional comment for the stage change
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Request model for approving/rejecting a workflow.
/// </summary>
public class WorkflowActionRequest
{
    /// <summary>
    /// The entity type name (e.g., "Opportunity")
    /// </summary>
    public required string EntityName { get; set; }
    
    /// <summary>
    /// The entity ID
    /// </summary>
    public required int EntityId { get; set; }
    
    /// <summary>
    /// Comment for the action (required for reject)
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Request model for recalling a workflow submission.
/// </summary>
public class WorkflowRecallRequest
{
    /// <summary>
    /// The entity type name (e.g., "Opportunity")
    /// </summary>
    public required string EntityName { get; set; }
    
    /// <summary>
    /// The entity ID
    /// </summary>
    public required int EntityId { get; set; }
    
    /// <summary>
    /// Optional comment for the recall
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Response model for workflow state information.
/// </summary>
public class WorkflowStateResponse
{
    /// <summary>
    /// Current workflow stage
    /// </summary>
    public required string CurrentStage { get; set; }
    
    /// <summary>
    /// Display name for the current stage
    /// </summary>
    public string? CurrentStageDisplayName { get; set; }
    
    /// <summary>
    /// Whether the entity is currently in a workflow approval process
    /// </summary>
    public bool IsInWorkflow { get; set; }
    
    /// <summary>
    /// The next stage if currently in workflow (pending approval)
    /// </summary>
    public string? PendingStage { get; set; }
    
    /// <summary>
    /// Available actions the current user can take
    /// </summary>
    public List<WorkflowActionModel> AvailableActions { get; set; } = new();
}

/// <summary>
/// Model for a workflow action available to the user.
/// </summary>
public class WorkflowActionModel
{
    /// <summary>
    /// Target stage code
    /// </summary>
    public required string TargetStage { get; set; }
    
    /// <summary>
    /// Display name for the action
    /// </summary>
    public required string DisplayName { get; set; }
    
    /// <summary>
    /// Whether approval is required for this transition
    /// </summary>
    public bool RequiresApproval { get; set; }
    
    /// <summary>
    /// Whether comment is required
    /// </summary>
    public bool CommentRequired { get; set; }
    
    /// <summary>
    /// Whether comment is optional
    /// </summary>
    public bool CommentOptional { get; set; }
}

/// <summary>
/// Response model for workflow details.
/// </summary>
public class WorkflowDetailsResponse
{
    /// <summary>
    /// Current workflow stage
    /// </summary>
    public required string CurrentStage { get; set; }
    
    /// <summary>
    /// Display name for the current stage
    /// </summary>
    public string? CurrentStageDisplayName { get; set; }
    
    /// <summary>
    /// Whether the entity is currently in a workflow approval process
    /// </summary>
    public bool IsInWorkflow { get; set; }
    
    /// <summary>
    /// The next stage if currently in workflow (pending approval)
    /// </summary>
    public string? PendingStage { get; set; }
    
    /// <summary>
    /// Display name for the pending stage
    /// </summary>
    public string? PendingStageDisplayName { get; set; }
    
    /// <summary>
    /// List of approvers for the pending workflow
    /// </summary>
    public List<WorkflowApproverResponse> Approvers { get; set; } = new();
    
    /// <summary>
    /// User who initiated the workflow
    /// </summary>
    public WorkflowUserResponse? InitiatedBy { get; set; }
    
    /// <summary>
    /// Date/time when the workflow was initiated
    /// </summary>
    public DateTime? InitiatedOn { get; set; }
    
    /// <summary>
    /// Whether the current user can approve/reject
    /// </summary>
    public bool CanApprove { get; set; }
    
    /// <summary>
    /// Whether the current user can recall (cancel) the workflow
    /// </summary>
    public bool CanRecall { get; set; }
}

/// <summary>
/// Response model for a workflow approver.
/// </summary>
public class WorkflowApproverResponse
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? RoleName { get; set; }
}

/// <summary>
/// Response model for a workflow user.
/// </summary>
public class WorkflowUserResponse
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}

/// <summary>
/// Response model for workflow history entry.
/// </summary>
public class WorkflowHistoryResponse
{
    /// <summary>
    /// Stage the entity transitioned from
    /// </summary>
    public string? FromStage { get; set; }
    
    /// <summary>
    /// Stage the entity transitioned to
    /// </summary>
    public string? ToStage { get; set; }
    
    /// <summary>
    /// Display name for the from stage
    /// </summary>
    public string? FromStageDisplayName { get; set; }
    
    /// <summary>
    /// Display name for the to stage
    /// </summary>
    public string? ToStageDisplayName { get; set; }
    
    /// <summary>
    /// Action taken (e.g., "Submitted", "Approved", "Rejected", "Recalled")
    /// </summary>
    public string? Action { get; set; }
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    public WorkflowUserResponse? PerformedBy { get; set; }
    
    /// <summary>
    /// Date/time when the action was performed
    /// </summary>
    public DateTime? PerformedOn { get; set; }
    
    /// <summary>
    /// Comment provided with the action
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Response model for workflow submit operation.
/// </summary>
public class WorkflowSubmitResponse
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Message describing the result
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Whether approval is required
    /// </summary>
    public bool ApprovalRequired { get; set; }
    
    /// <summary>
    /// New stage if transition was immediate (no approval required)
    /// </summary>
    public string? NewStage { get; set; }
    
    /// <summary>
    /// Pending stage if approval is required
    /// </summary>
    public string? PendingStage { get; set; }
}

/// <summary>
/// Response model for workflow stage configuration.
/// </summary>
public class WorkflowStageConfigResponse
{
    /// <summary>
    /// Stage code
    /// </summary>
    public required string StageCode { get; set; }
    
    /// <summary>
    /// Display name for the stage
    /// </summary>
    public required string DisplayName { get; set; }
    
    /// <summary>
    /// Sequence number for ordering
    /// </summary>
    public int Sequence { get; set; }
}
