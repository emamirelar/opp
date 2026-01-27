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
