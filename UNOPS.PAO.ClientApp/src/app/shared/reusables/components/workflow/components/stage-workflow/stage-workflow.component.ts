/**
 * @fileoverview Stage workflow component for displaying workflow stages and actions
 * @author UNOPS Grants System Development Team
 */

import {
  Component,
  inject,
  input,
  Input,
  OnChanges,
  OnInit,
  output,
  signal,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { MenuItem } from 'primeng/api';
import { StepsModule } from 'primeng/steps';
import { FieldsetModule } from 'primeng/fieldset';
import { TableModule } from 'primeng/table';
import { TranslateModule } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { TagModule } from 'primeng/tag';
import { TabsModule } from 'primeng/tabs';
import { SkeletonModule } from 'primeng/skeleton';
import { DatePipe } from '@angular/common';

import { WorkflowService } from '../../services/workflow.service';
import { WorkflowComponent, IFeedbackDialogService } from '../workflow/workflow.component';
import { CustomStageChangeResult, WorkflowHistoryUserModel, WorkflowApproverModel } from '../../models/workflow.models';

/**
 * @class StageWorkflowComponent
 * @description Component for displaying complete workflow state including stages, 
 * available actions, approvers, and history.
 * @since 1.0.0
 * 
 * @example
 * ```html
 * <app-stage-workflow
 *   [entityName]="'funding-agreement'"
 *   [entityId]="recordId.toString()"
 *   [canChangeStage]="canEdit()"
 *   (onStageChangeSuccess)="handleStageChangeSuccess()"
 * />
 * ```
 */
@Component({
  selector: 'app-stage-workflow',
  imports: [
    StepsModule,
    PanelModule,
    FieldsetModule,
    TagModule,
    WorkflowComponent,
    TableModule,
    TranslateModule,
    TabsModule,
    SkeletonModule,
    DatePipe,
  ],
  templateUrl: './stage-workflow.component.html',
  styleUrl: './stage-workflow.component.scss',
})
export class StageWorkflowComponent implements OnInit, OnChanges {
  workflowService = inject(WorkflowService);

  @ViewChild('workFlowComponent') workflowComponent!: WorkflowComponent;

  entityName = input<string>('');
  entityId = input<string>('');
  canChangeStage = input<boolean>(true);
  onStageChangeSuccess = output();

  // Feedback service must be provided by consuming application
  @Input() feedbackDialogService!: IFeedbackDialogService;

  stages = signal<MenuItem[]>([]);
  currentStageName = signal<string>('');
  approvers = signal<WorkflowApproverModel[]>([]);
  currentStageIndex = -1;
  isLoading = signal(false);
  workflowData = signal<any>(null);
  stageChangeHistory = signal<any[]>([]);

  // Skeleton loading states for different sections
  stagesLoading = signal(false);
  workflowDataLoading = signal(false);
  historyLoading = signal(false);

  @Input() beforeStageChange: (nextStage: string) => Promise<boolean> = async () => true;
  @Input() customStageChangeHandler?: (
    nextStage: string,
    actionName: string
  ) => Promise<CustomStageChangeResult | undefined>;

  get scrollHeightValue() {
    return this.approvers().length > 0 ? '200px' : undefined;
  }

  get stageChangeScrollHeightValue() {
    return this.stageChangeHistory().length > 0 ? '200px' : undefined;
  }

  ngOnInit() {
    // Only load if we have both entityName and entityId
    if (this.entityName() && this.entityId()) {
      this.loadData();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    // Reload when entityId changes (but not on first change)
    if (changes['entityId'] && !changes['entityId'].isFirstChange() && this.entityName() && this.entityId()) {
      this.loadData();
    }
    // Also reload if entityName changes
    if (changes['entityName'] && !changes['entityName'].isFirstChange() && this.entityName() && this.entityId()) {
      this.loadData();
    }
  }

  reload() {
    this.loadData();
  }

  private loadData() {
    if (!this.entityId() || !this.entityName()) {
      return;
    }

    // Load stages
    this.stagesLoading.set(true);
    this.workflowService.getWorkFlowForEntity(this.entityName()).subscribe({
      next: (data: any) => {
        const stageArray: any[] = data || [];
        if (stageArray && stageArray.length > 0) {
          this.stages.set(
            stageArray.map((item) => ({
              label: item.displayName || item.DisplayName || item.stageCode || item.StageCode || '',
              name: item.stageCode || item.StageCode || item.stage || item.Stage || '',
              value: item.sequence || item.Sequence || 0,
            }))
          );
          // Update stage index if we already have current stage name
          if (this.currentStageName() !== '') {
            this.updateCurrentStageIndex();
          }
        }
        this.stagesLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading workflow stages:', error);
        this.stagesLoading.set(false);
      },
    });

    // Load workflow actions for current record
    this.workflowDataLoading.set(true);
    this.workflowService.getNextWorkFlowActionsForARecordById(this.entityName(), this.entityId()).subscribe({
      next: (data: any) => {
        // API returns: currentStage, currentStageDisplayName, isInWorkflow, pendingStage, availableActions
        const currentStage = data.currentStage || '';
        const displayName = data.currentStageDisplayName || currentStage;
        
        // Set current stage
        this.setCurrentStageName(currentStage);
        
        // Store workflow data for template
        this.workflowData.set({
          currentStage: currentStage,
          displayName: displayName,
          isInWorkflow: data.isInWorkflow || false,
          pendingStage: data.pendingStage || null,
          availableActions: data.availableActions || []
        });
        
        // If in workflow, load details first to get permissions, then pass to WorkflowComponent
        if (data.isInWorkflow) {
          this.workflowService.getWorkflowDetails(this.entityName(), this.entityId()).subscribe({
            next: (detailsData: any) => {
              // Store approvers
              if (detailsData.approvers) {
                this.approvers.set(detailsData.approvers.map((a: any) => ({
                  userId: a.userId,
                  firstName: a.userName?.split(' ')[0] || '',
                  lastName: a.userName?.split(' ').slice(1).join(' ') || '',
                  email: a.userEmail || '',
                  role: a.roleName || '',
                  name: a.userName || a.userEmail || ''
                })));
              }
              
              // Transform data for WorkflowComponent with permissions
              if (this.workflowComponent !== null && this.workflowComponent !== undefined) {
                const transformedData = {
                  stage: currentStage,
                  displayName: displayName,
                  isInWorkflow: true,
                  nextActions: (data.availableActions || []).map((action: any) => ({
                    newStage: action.targetStage || action.newStage,
                    actionName: action.displayName || action.actionName,
                    comment: action.commentRequired ? 'mandatory' : (action.commentOptional ? 'optional' : 'none'),
                    requiresApproval: action.requiresApproval || false
                  })),
                  workflow: {
                    nextStage: detailsData.pendingStage || data.pendingStage || null,
                    canRecall: detailsData.canRecall || false,
                    recallComment: 'mandatory',
                    canApprove: detailsData.canApprove || false,
                    approvalComment: 'optional',
                    canReject: detailsData.canApprove || false, // Same permission as approve
                    rejectionComment: 'mandatory',
                    approvers: detailsData.approvers || []
                  }
                };
                this.workflowComponent.loadData(transformedData);
              }
              
              this.workflowDataLoading.set(false);
            },
            error: (error) => {
              console.error('Error loading workflow details:', error);
              // Even if details fail, still pass basic data to WorkflowComponent
              if (this.workflowComponent !== null && this.workflowComponent !== undefined) {
                const transformedData = {
                  stage: currentStage,
                  displayName: displayName,
                  isInWorkflow: true,
                  nextActions: [],
                  workflow: {
                    nextStage: data.pendingStage || null,
                    canRecall: false,
                    recallComment: 'mandatory',
                    canApprove: false,
                    approvalComment: 'optional',
                    canReject: false,
                    rejectionComment: 'mandatory',
                    approvers: []
                  }
                };
                this.workflowComponent.loadData(transformedData);
              }
              this.workflowDataLoading.set(false);
            }
          });
        } else {
          // Not in workflow - pass normal data to WorkflowComponent
          this.approvers.set([]);
          if (this.workflowComponent !== null && this.workflowComponent !== undefined) {
            const transformedData = {
              stage: currentStage,
              displayName: displayName,
              isInWorkflow: false,
              nextActions: (data.availableActions || []).map((action: any) => ({
                newStage: action.targetStage || action.newStage,
                actionName: action.displayName || action.actionName,
                comment: action.commentRequired ? 'mandatory' : (action.commentOptional ? 'optional' : 'none'),
                requiresApproval: action.requiresApproval || false
              })),
              workflow: null
            };
            this.workflowComponent.loadData(transformedData);
          }
          this.workflowDataLoading.set(false);
        }
      },
      error: (error) => {
        console.error('Error loading workflow state:', error);
        this.workflowDataLoading.set(false);
      },
    });

    // Load stage change history
    this.historyLoading.set(true);
    this.workflowService.getStageChangeHistory(this.entityName(), this.entityId()).subscribe({
      next: (data: any) => {
        this.stageChangeHistory.set(data || []);
        this.historyLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading workflow history:', error);
        this.historyLoading.set(false);
      },
    });
  }

  setCurrentStageName(name: string) {
    this.currentStageName.set(name);
    this.updateCurrentStageIndex();
  }

  private updateCurrentStageIndex() {
    this.currentStageIndex = this.stages().findIndex((item: any) => item['name'] === this.currentStageName());
  }

  handleOnStageChangeSuccess(data: any) {
    this.loadData();
    this.onStageChangeSuccess.emit(data);
  }

  getUserNameToDisplay(user: WorkflowHistoryUserModel | WorkflowApproverModel | any): string {
    if (user?.name) {
      return user.name;
    } else if (user?.firstName && user?.lastName) {
      return `${user.firstName} ${user.lastName}`;
    } else if (user?.email) {
      return user.email;
    }
    return '';
  }

  getNextStage(): string {
    return this.workflowComponent?.nextStage() || '';
  }
}
