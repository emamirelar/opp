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
} from '../models/workflow.models';

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
 * Provides methods for fetching workflow stages, actions, history, and executing workflow transitions.
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
   * Gets the workflow path (stages) for an entity type
   * @param entityName The entity type name
   * @returns Observable of workflow stages
   */
  getWorkFlowForEntity(entityName: string): Observable<WorkflowStageModel[]> {
    return this.http.get<WorkflowStageModel[]>(`${this.apiBaseUrl}/workflow/${entityName}`);
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
  getWorkflowDetails(entityName: string, entityId: string): Observable<any> {
    return this.http.get<any>(`${this.apiBaseUrl}/workflow/${entityName}/${entityId}/details`);
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
   * Executes a workflow stage change
   * @param requestJson The workflow action model
   * @returns Observable of updated workflow state
   */
  changeWorkflow(requestJson: WorkflowActionModel): Observable<WorkflowStateModel> {
    return this.http.post<WorkflowStateModel>(`${this.apiBaseUrl}/workflow/submit`, requestJson);
  }
}
