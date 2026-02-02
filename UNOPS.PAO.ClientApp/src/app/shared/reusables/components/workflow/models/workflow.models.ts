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
 * Workflow history user model
 */
export interface WorkflowHistoryUserModel {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  name: string;
}

/**
 * Workflow history model
 */
export interface WorkflowHistoryModel {
  fromStage: string;
  toStage: string;
  completedOn?: Date;
  action: string;
  comment: string;
  user?: WorkflowHistoryUserModel;
  role?: string;
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
   * List of countries not related to the org unit (for OrgUnitCountryMismatch)
   */
  unrelatedCountries?: string[];

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
 * Cancel/Reopen request model
 */
export interface WorkflowCancelReopenRequest {
  entityName: string;
  entityId: number;
  comment?: string;
}

// Re-export requirement models for convenience
export * from './requirement.models';
