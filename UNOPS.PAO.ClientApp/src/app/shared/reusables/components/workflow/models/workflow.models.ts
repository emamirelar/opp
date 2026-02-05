/**
 * @fileoverview Workflow models for Angular frontend
 * @author UNOPS Grants System Development Team
 */

/**
 * User facing type for workflow visibility
 */
export type Facing = 'TwoFace' | 'Internal' | 'External';

/**
 * Workflow comment mode
 */
export type WorkflowCommentMode = 'none' | 'optional' | 'mandatory';

/**
 * Workflow stage model
 */
export interface WorkflowStageModel {
  stage: string;
  displayName: string;
  sequence: number;
}

/**
 * Workflow action model for submitting stage changes
 */
export interface WorkflowActionModel {
  entityName: string;
  entityId: number;
  newStage: string;
  comment?: string;
}

/**
 * Workflow state action model
 */
export interface WorkflowStateActionModel {
  actionName: string;
  newStage: string;
  sequence: number;
  comment: WorkflowCommentMode | string;
  requiresApproval: boolean;
}

/**
 * Workflow approver model
 */
export interface WorkflowApproverModel {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  toStage: string;
  name?: string;
}

/**
 * Workflow details model
 */
export interface WorkflowDetailsModel {
  nextStage?: string;
  canRecall: boolean;
  recallComment: string;
  canApprove: boolean;
  approvalComment: string;
  canReject: boolean;
  rejectionComment: string;
  approvers: WorkflowApproverModel[];
}

/**
 * Workflow state model
 */
export interface WorkflowStateModel {
  stage: string;
  displayName: string;
  comment: string;
  nextActions?: WorkflowStateActionModel[];
  isInWorkflow: boolean;
  workflow?: WorkflowDetailsModel;
}

/**
 * Workflow history user model (matches WorkflowUserResponse from backend)
 */
export interface WorkflowHistoryUserModel {
  userId: number;
  userName?: string;
  userEmail?: string;
  /**
   * User's standardized position/job title
   */
  positionTitle?: string;
  /**
   * User's DOA (Delegation of Authority) level (e.g., "DoA1", "DoA2", "DoA3")
   * Only populated for approvers who have a DOA role
   */
  doaLevel?: string;
}

/**
 * Workflow history model (matches WorkflowHistoryResponse from backend)
 */
export interface WorkflowHistoryModel {
  fromStage: string;
  toStage: string;
  fromStageDisplayName?: string;
  toStageDisplayName?: string;
  action: string;
  comment: string;
  performedBy?: WorkflowHistoryUserModel;
  performedOn?: Date;
  requiresApproval: boolean;
}

/**
 * Custom stage change handler result
 */
export interface CustomStageChangeResult {
  proceed: boolean;
  comment?: string;
}

/**
 * Extended workflow action model for submission with confirmation flags
 */
export interface WorkflowSubmitRequest extends WorkflowActionModel {
  /**
   * Confirmed that user is not the Opportunity Manager but wishes to proceed
   */
  confirmedNonOMSubmission?: boolean;

  /**
   * Confirmed country-org unit mismatch warning
   */
  confirmedOrgUnitWarning?: boolean;

  /**
   * User has acknowledged the submission statement
   */
  acknowledgedStatement?: boolean;

  /**
   * Optional additional remarks for the decision maker
   */
  additionalRemarks?: string;
}

/**
 * Response from workflow submission with confirmation requirements
 */
export interface WorkflowSubmitResponse {
  /**
   * Whether the submission was successful
   */
  success: boolean;

  /**
   * Whether user confirmation is required before proceeding
   */
  requiresConfirmation?: boolean;

  /**
   * Type of confirmation required
   */
  confirmationType?: ConfirmationType;

  /**
   * Message to display in confirmation dialog
   */
  confirmationMessage?: string;

  /**
   * Opportunity Manager info for Non-OM warning (name and email)
   */
  opportunityManagerInfo?: string;

  /**
   * List of countries not related to the org unit (for OrgUnitCountryMismatch)
   */
  unrelatedCountries?: string[];

  /**
   * All implementation countries with their mapping status (for OrgUnitCountryMismatch)
   */
  countryMappings?: CountryMappingInfo[];

  /**
   * Name of the responsible org unit for display in dialogs
   */
  responsibleOrgUnitName?: string;

  /**
   * Whether acknowledgment statement is required
   */
  requiresAcknowledgment?: boolean;

  /**
   * Text of the acknowledgment statement
   */
  acknowledgmentText?: string;

  /**
   * The new stage after successful submission
   */
  newStage?: string;

  /**
   * Error message if submission failed
   */
  errorMessage?: string;

  /**
   * Whether requirements validation failed.
   * Frontend should show the requirements panel with unmet items.
   */
  requirementsNotMet?: boolean;

  /**
   * List of unmet requirement messages to display.
   */
  unmetRequirements?: string[];
}

/**
 * Types of confirmation dialogs
 */
export type ConfirmationType = 'NonOMSubmitter' | 'OrgUnitCountryMismatch';

/**
 * Country mapping information for org unit mismatch dialog
 */
export interface CountryMappingInfo {
  /**
   * Country name
   */
  countryName: string;

  /**
   * Whether the country is mapped to the selected org unit
   */
  isMapped: boolean;
}

/**
 * Cancel/Reopen request model
 */
export interface WorkflowCancelReopenRequest {
  entityName: string;
  entityId: number;
  comment?: string;
}

/**
 * Model for pending workflow approvals
 * Used to display tasks in the Actions Required dashboard card
 */
export interface PendingApprovalModel {
  /**
   * Entity type (e.g., 'Opportunity')
   */
  entityName: string;

  /**
   * Entity ID
   */
  entityId: number;

  /**
   * Display name of the entity (e.g., opportunity name)
   */
  entityDisplayName: string;

  /**
   * Current workflow stage
   */
  currentStage: string;

  /**
   * Stage waiting for approval
   */
  pendingStage: string;

  /**
   * User who submitted for approval
   */
  submittedBy: string;

  /**
   * Submission timestamp
   */
  submittedOn: Date;

  /**
   * Responsible org unit name
   */
  orgUnitName: string;

  /**
   * Optional submission comment/remarks
   */
  submissionComment?: string;
}

// Re-export requirement models for convenience
export * from './requirement.models';
