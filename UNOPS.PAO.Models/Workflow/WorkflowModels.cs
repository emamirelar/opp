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

    /// <summary>
    /// Confirmation flag when submitter is not the Opportunity Manager.
    /// Set to true to proceed despite the warning.
    /// </summary>
    public bool ConfirmedNonOMSubmission { get; set; }

    /// <summary>
    /// Confirmation flag when opportunity countries don't match org unit relationships.
    /// Set to true to proceed despite the warning.
    /// </summary>
    public bool ConfirmedOrgUnitWarning { get; set; }

    /// <summary>
    /// Acknowledgment flag for the mandatory statement.
    /// Must be true to proceed with submission to GO stage.
    /// </summary>
    public bool AcknowledgedStatement { get; set; }

    /// <summary>
    /// Optional additional remarks to include with the submission.
    /// </summary>
    public string? AdditionalRemarks { get; set; }
}

/// <summary>
/// Request model for approving/rejecting a workflow.
/// Legacy request model - use ApproveWorkflowRequest or RejectWorkflowRequest for enhanced Go Decision flow.
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
/// Request model for approving an opportunity workflow with Go decision requirements.
/// Used for enhanced approval flow with mandatory rationale, confirmation, and Executive assignment.
/// </summary>
public class ApproveWorkflowRequest
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
    /// Decision rationale explaining the approval (required).
    /// Stored in WorkflowLog.Comment field.
    /// </summary>
    public required string Rationale { get; set; }
    
    /// <summary>
    /// Indicates the user has acknowledged the confirmation statement.
    /// Must be true to proceed with approval.
    /// </summary>
    public bool ConfirmationAcknowledged { get; set; }
    
    /// <summary>
    /// ID of the assigned Executive (required for Opportunity approvals).
    /// Stored on Opportunity.ExecutiveId upon successful Go decision.
    /// </summary>
    public int ExecutiveId { get; set; }
}

/// <summary>
/// Request model for rejecting an opportunity workflow with No-Go decision requirements.
/// Used for enhanced rejection flow with mandatory rationale and confirmation.
/// </summary>
public class RejectWorkflowRequest
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
    /// Decision rationale explaining the rejection (required).
    /// Stored in WorkflowLog.Comment field.
    /// </summary>
    public required string Rationale { get; set; }
    
    /// <summary>
    /// Indicates the user has acknowledged the rejection confirmation statement.
    /// Must be true to proceed with rejection.
    /// </summary>
    public bool ConfirmationAcknowledged { get; set; }
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
    
    /// <summary>
    /// User's standardized position/job title
    /// </summary>
    public string? PositionTitle { get; set; }
    
    /// <summary>
    /// User's DOA (Delegation of Authority) level (e.g., "DoA1", "DoA2", "DoA3")
    /// Only populated for approvers who have a DOA role
    /// </summary>
    public string? DoaLevel { get; set; }
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

    /// <summary>
    /// Whether user confirmation is required before proceeding.
    /// Frontend should display the ConfirmationMessage and re-submit with appropriate flag set.
    /// </summary>
    public bool RequiresConfirmation { get; set; }

    /// <summary>
    /// Type of confirmation required: "NonOMSubmitter", "OrgUnitCountryMismatch"
    /// </summary>
    public string? ConfirmationType { get; set; }

    /// <summary>
    /// Message to display to user when confirmation is required.
    /// </summary>
    public string? ConfirmationMessage { get; set; }

    /// <summary>
    /// Opportunity Manager info (name and email) for Non-OM warning dialog.
    /// </summary>
    public string? OpportunityManagerInfo { get; set; }

    /// <summary>
    /// List of country names that don't match org unit relationships (for OrgUnitCountryMismatch warning).
    /// </summary>
    public List<string>? UnrelatedCountries { get; set; }

    /// <summary>
    /// All implementation countries with their mapping status (for OrgUnitCountryMismatch dialog).
    /// </summary>
    public List<CountryMappingInfo>? CountryMappings { get; set; }

    /// <summary>
    /// Name of the responsible org unit for display in dialogs.
    /// </summary>
    public string? ResponsibleOrgUnitName { get; set; }

    /// <summary>
    /// Whether acknowledgment of the statement is required before proceeding.
    /// </summary>
    public bool RequiresAcknowledgment { get; set; }

    /// <summary>
    /// Text of the acknowledgment statement that must be confirmed.
    /// </summary>
    public string? AcknowledgmentText { get; set; }

    /// <summary>
    /// Whether requirements validation failed.
    /// Frontend should show the requirements panel with unmet items.
    /// </summary>
    public bool RequirementsNotMet { get; set; }

    /// <summary>
    /// List of unmet requirement messages to display.
    /// </summary>
    public List<string>? UnmetRequirements { get; set; }
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

/// <summary>
/// Request model for cancelling an opportunity.
/// Only Opportunity Manager can cancel, only from IDENTIFY & PROFILE stage.
/// </summary>
public class WorkflowCancelRequest
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
    /// Mandatory comment explaining why the opportunity is being cancelled.
    /// </summary>
    public required string Comment { get; set; }
}

/// <summary>
/// Request model for reopening an opportunity.
/// Only Opportunity Manager can reopen, only from NO GO or CANCELLED stage.
/// </summary>
public class WorkflowReopenRequest
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
    /// Comment explaining why the opportunity is being reopened.
    /// Required when reopening from CANCELLED, optional from NO GO.
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Response model for workflow action operations (cancel, reopen).
/// </summary>
public class WorkflowActionResponse
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
    /// The new stage after the action
    /// </summary>
    public string? NewStage { get; set; }
}

/// <summary>
/// Country mapping information for org unit mismatch dialog.
/// Shows which implementation countries are/aren't mapped to the selected org unit.
/// </summary>
public class CountryMappingInfo
{
    /// <summary>
    /// Name of the country
    /// </summary>
    public required string CountryName { get; set; }

    /// <summary>
    /// Whether the country is mapped to the selected org unit
    /// </summary>
    public bool IsMapped { get; set; }
}

/// <summary>
/// Response model for pending workflow approval items.
/// Used by the Actions Required card on the home dashboard.
/// </summary>
public class PendingApprovalResponse
{
    /// <summary>
    /// The entity type name (e.g., "Opportunity")
    /// </summary>
    public required string EntityName { get; set; }
    
    /// <summary>
    /// The entity ID
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// Display name for the entity (e.g., opportunity title)
    /// </summary>
    public string? EntityDisplayName { get; set; }
    
    /// <summary>
    /// Current stage of the entity
    /// </summary>
    public string? CurrentStage { get; set; }
    
    /// <summary>
    /// Display name for the current stage
    /// </summary>
    public string? CurrentStageDisplayName { get; set; }
    
    /// <summary>
    /// The stage pending approval
    /// </summary>
    public string? PendingStage { get; set; }
    
    /// <summary>
    /// Display name for the pending stage
    /// </summary>
    public string? PendingStageDisplayName { get; set; }
    
    /// <summary>
    /// Name of the user who submitted for approval
    /// </summary>
    public string? SubmittedBy { get; set; }
    
    /// <summary>
    /// User ID of the submitter
    /// </summary>
    public int? SubmittedByUserId { get; set; }
    
    /// <summary>
    /// Date/time when the workflow was submitted
    /// </summary>
    public DateTime? SubmittedOn { get; set; }
    
    /// <summary>
    /// Name of the responsible organization unit
    /// </summary>
    public string? OrgUnitName { get; set; }
    
    /// <summary>
    /// URL to navigate to the entity detail page
    /// </summary>
    public string? EntityUrl { get; set; }
}
