/**
 * @fileoverview Workflow service for API communication
 * @author Opportunity+ Development Team
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  WorkflowActionModel,
  WorkflowHistoryModel,
  WorkflowStageModel,
  WorkflowStateModel,
  WorkflowSubmitRequest,
  WorkflowSubmitResponse,
  WorkflowCancelReopenRequest,
  PendingApprovalModel,
} from '../models/workflow.models';
import { StageRequirement } from '../models/requirement.models';

/**
 * Configuration interface for the workflow service
 */
export interface WorkflowServiceConfig {
  /**
   * Base URL for API calls (e.g., '/api')
   */
  apiBaseUrl: string;
}

/**
 * Injection token for workflow service configuration
 */
export const WORKFLOW_SERVICE_CONFIG = 'WORKFLOW_SERVICE_CONFIG';

/**
 * @class WorkflowService
 * @description Service for interacting with workflow API endpoints.
 * Provides methods for fetching workflow stages, actions, history, requirements, and executing workflow transitions.
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root',
})
export class WorkflowService {
  private http = inject(HttpClient);

  /**
   * Base URL for API calls - should be configured by consuming application
   */
  private apiBaseUrl = '/api';

  /**
   * Configure the service with the API base URL
   * @param config Service configuration
   */
  configure(config: WorkflowServiceConfig): void {
    this.apiBaseUrl = config.apiBaseUrl;
  }

  /**
   * Gets the workflow stages for an entity type
   * @param entityName The entity type name
   * @returns Observable of workflow stages
   */
  getWorkflowStages(entityName: string): Observable<WorkflowStageModel[]> {
    return this.http.get<WorkflowStageModel[]>(`${this.apiBaseUrl}/workflow/${entityName}`);
  }

  /**
   * Gets the workflow path (stages) for an entity type
   * @param entityName The entity type name
   * @returns Observable of workflow stages
   * @deprecated Use getWorkflowStages instead
   */
  getWorkFlowForEntity(entityName: string): Observable<WorkflowStageModel[]> {
    return this.getWorkflowStages(entityName);
  }

  /**
   * Gets the next workflow actions for a specific entity record
   * @param entityName The entity type name
   * @param entityId The entity ID
   * @returns Observable of workflow state with available actions
   */
  getNextWorkFlowActionsForARecordById(entityName: string, entityId: string): Observable<WorkflowStateModel> {
    return this.http.get<WorkflowStateModel>(`${this.apiBaseUrl}/workflow/${entityName}/${entityId}`);
  }

  /**
   * @deprecated Use getNextWorkFlowActionsForARecordById instead
   */
  getNextWorkFlowAtionsForARecordById(entityName: string, entityId: string): Observable<WorkflowStateModel> {
    return this.getNextWorkFlowActionsForARecordById(entityName, entityId);
  }

  /**
   * Gets detailed workflow information for an entity including approvers
   * @param entityName The entity type name
   * @param entityId The entity ID
   * @returns Observable of workflow details
   */
  getWorkflowDetails(entityName: string, entityId: string): Observable<unknown> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/workflow/${entityName}/${entityId}/details`);
  }

  /**
   * Gets the stage change history for an entity
   * @param entityName The entity type name
   * @param entityId The entity ID
   * @returns Observable of workflow history entries
   */
  getStageChangeHistory(entityName: string, entityId: string): Observable<WorkflowHistoryModel[]> {
    return this.http.get<WorkflowHistoryModel[]>(`${this.apiBaseUrl}/workflow/${entityName}/${entityId}/history`);
  }

  /**
   * Gets the stage requirements for a workflow stage change
   * @param entityName The entity type name
   * @param entityId The entity ID
   * @param currentStage The current stage (optional - if not provided, will be determined from entity)
   * @returns Observable of stage requirements
   */
  getRequirementsForStageChange(
    entityName: string,
    entityId: string,
    currentStage?: string
  ): Observable<StageRequirement[]> {
    const params = currentStage ? `?currentStage=${encodeURIComponent(currentStage)}` : '';
    return this.http.get<StageRequirement[]>(
      `${this.apiBaseUrl}/workflow/${entityName}/${entityId}/requirements${params}`
    );
  }

  /**
   * Executes a workflow stage change
   * @param requestJson The workflow action model
   * @returns Observable of updated workflow state
   */
  changeWorkflow(requestJson: WorkflowActionModel): Observable<WorkflowStateModel> {
    return this.http.post<WorkflowStateModel>(`${this.apiBaseUrl}/workflow/submit`, requestJson);
  }

  /**
   * Submits an opportunity for Go decision with confirmation handling
   * @param request The workflow submit request with confirmation flags
   * @returns Observable of submit response (may require confirmation)
   */
  submitForGoDecision(request: WorkflowSubmitRequest): Observable<WorkflowSubmitResponse> {
    return this.http.post<WorkflowSubmitResponse>(`${this.apiBaseUrl}/workflow/submit`, request);
  }

  /**
   * Cancels an opportunity (moves to CANCELLED stage)
   * @param entityId The opportunity ID
   * @param comment Mandatory cancellation reason
   * @returns Observable of updated workflow state
   */
  cancelOpportunity(entityId: string, comment: string): Observable<WorkflowStateModel> {
    const request: WorkflowCancelReopenRequest = {
      entityName: 'opportunity',
      entityId: parseInt(entityId, 10),
      comment,
    };
    return this.http.post<WorkflowStateModel>(`${this.apiBaseUrl}/workflow/cancel`, request);
  }

  /**
   * Reopens an opportunity from NO GO or CANCELLED stage
   * @param entityId The opportunity ID
   * @param comment Optional reason for reopening (mandatory for CANCELLED)
   * @returns Observable of updated workflow state
   */
  reopenOpportunity(entityId: string, comment?: string): Observable<WorkflowStateModel> {
    const request: WorkflowCancelReopenRequest = {
      entityName: 'opportunity',
      entityId: parseInt(entityId, 10),
      comment,
    };
    return this.http.post<WorkflowStateModel>(`${this.apiBaseUrl}/workflow/reopen`, request);
  }

  /**
   * Gets pending workflow approvals for the current user
   * Used to display tasks in the Actions Required dashboard card
   * @returns Observable of pending approval models
   */
  getPendingApprovalsForUser(): Observable<PendingApprovalModel[]> {
    return this.http.get<PendingApprovalModel[]>(`${this.apiBaseUrl}/workflow/pending-approvals`);
  }
}
