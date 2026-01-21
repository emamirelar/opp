/**
 * @fileoverview Workflow action component for executing workflow transitions
 * @author Opportunity+ Development Team
 */

import { Component, inject, input, Input, OnInit, output, signal, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MenuItem } from 'primeng/api';

import { SplitButton } from 'primeng/splitbutton';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { TranslateModule } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';

import { WorkflowService } from '../../services/workflow.service';
import { WorkflowStateActionModel, CustomStageChangeResult, WorkflowActionModel } from '../../models/workflow.models';

/**
 * Interface for feedback dialog service that consuming applications must provide
 */
export interface IFeedbackDialogService {
  showConfirmDialog(options: { detail: string }, onConfirm: () => void): void;
  showSuccessToast(options: { detail: string }): void;
  showInfoToast(options: { detail: string }): void;
}

/**
 * Injection token for feedback dialog service
 */
export const FEEDBACK_DIALOG_SERVICE = 'FEEDBACK_DIALOG_SERVICE';

/**
 * @class WorkflowComponent
 * @description Component for displaying and executing workflow actions.
 * Provides UI for stage transitions with approval workflow support.
 * @since 1.0.0
 */
@Component({
  selector: 'app-workflow',
  templateUrl: './workflow.component.html',
  styleUrl: './workflow.component.scss',
  imports: [SplitButton, DialogModule, ButtonModule, TextareaModule, SkeletonModule, TranslateModule],
})
export class WorkflowComponent implements OnInit {
  private changeDetectorRef = inject(ChangeDetectorRef);
  private http = inject(HttpClient);

  // Injected feedback service - must be provided by consuming application
  @Input() feedbackDialogService!: IFeedbackDialogService;

  entityName = input.required<string>();
  entityId = input.required<string>();
  autoload = input<boolean>(true);
  isReadOnly = input<boolean>(false);

  @Input() disabled!: boolean;
  @Input() beforeStageChange: (nextStage: string) => Promise<boolean> = async () => true;
  @Input() customStageChangeHandler?: (
    nextStage: string,
    actionName: string
  ) => Promise<CustomStageChangeResult | undefined>;

  stageChangeSuccess = output();

  workflowService = inject(WorkflowService);

  primaryeStageLabel = '';
  primaryeStageName = '';
  primaryStageCommentMode = '';
  primaryStageRequiresApproval = false;
  showCommentDialog: boolean = false;

  items: MenuItem[] = [];

  nextStage = signal('');
  nextStageActionName = signal('');
  commentMode = signal('');
  workflowInfo = signal<any>({});
  isInWorkflow = signal<boolean>(false);
  workflowActions = signal<WorkflowStateActionModel[]>([]);

  isWorkflowLoading = signal(false);
  isActionInProgress = signal(false); // Track button action loading state

  ngOnInit(): void {
    if (this.autoload() === true) {
      this.load();
    }
  }

  private load() {
    this.isWorkflowLoading.set(true);
    this.workflowService.getNextWorkFlowActionsForARecordById(this.entityName(), this.entityId()).subscribe({
      next: (data: any) => {
        // If in workflow, also load details (approvers, permissions, etc.)
        if (data?.isInWorkflow) {
          this.workflowService.getWorkflowDetails(this.entityName(), this.entityId()).subscribe({
            next: (detailsData: any) => {
              // Merge details into workflow state
              data.workflow = {
                nextStage: detailsData.pendingStage,
                canRecall: detailsData.canRecall,
                recallComment: 'mandatory',
                canApprove: detailsData.canApprove,
                approvalComment: 'optional',
                canReject: detailsData.canApprove, // Same permission as approve
                rejectionComment: 'mandatory',
                approvers: detailsData.approvers || []
              };
              this.initialiseUI(data);
              this.isWorkflowLoading.set(false);
            },
            error: () => {
              this.initialiseUI(data);
              this.isWorkflowLoading.set(false);
            }
          });
        } else {
          this.initialiseUI(data);
          this.isWorkflowLoading.set(false);
        }
      },
      error: () => {
        this.isWorkflowLoading.set(false);
      },
    });
  }

  loadData(data: any) {
    this.initialiseUI(data);
  }

  private initialiseUI(workflowData: any) {
    let itemsArray: MenuItem[] = [];
    const actions = workflowData?.nextActions || [];

    this.workflowInfo.set(workflowData?.workflow || {});
    this.isInWorkflow.set(workflowData?.isInWorkflow || false);
    this.workflowActions.set(actions);

    actions.forEach((item: any, index: number) => {
      if (index === 0) {
        this.primaryeStageName = item['newStage'];
        this.primaryeStageLabel = item['actionName'];
        this.primaryStageCommentMode = item['comment'];
        this.primaryStageRequiresApproval = item?.requiresApproval || false;
      } else {
        itemsArray.push({
          label: item['actionName'],
          command: async () => {
            const canProceed = await this.beforeStageChange(item['newStage']);
            if (!canProceed) {
              return;
            }

            if (item?.requiresApproval === true) {
              this.feedbackDialogService?.showConfirmDialog(
                {
                  detail: 'Moving to this stage needs approval. Do you want to continue?',
                },
                () => {
                  this.handleAfterApprovalOrValidation(item['newStage'], 'Submit', item['comment']);
                }
              );
            } else {
              this.handleAfterApprovalOrValidation(item['newStage'], item['actionName'], item['comment']);
            }
          },
        });
      }
    });
    this.items = [...itemsArray];
  }

  _executeStageChange(nextStage: string, actionName: string, commentMode: string) {
    this.nextStage.set(nextStage);
    this.nextStageActionName.set(actionName);
    this.commentMode.set(commentMode);

    if (commentMode != undefined && commentMode != '' && commentMode != 'none') {
      this.showCommentDialog = true;
      this.changeDetectorRef.detectChanges();
    } else {
      this._performStageChange();
    }
  }

  _performStageChange(comment?: string) {
    this.isActionInProgress.set(true);
    
    const requestJson: WorkflowActionModel = {
      entityName: this.entityName(),
      entityId: parseInt(this.entityId(), 10),
      newStage: this.nextStage(),
      comment: comment || undefined,
    };

    this.workflowService.changeWorkflow(requestJson).subscribe({
      next: (data: any) => {
        this.isActionInProgress.set(false);
        
        // Show success message
        this.feedbackDialogService?.showSuccessToast({
          detail: 'Stage changed successfully!',
        });
        
        // Reload workflow state to get updated actions and approval status
        if (this.autoload() === true) {
          this.load(); // Reload from server to get current state
        }
        
        this.stageChangeSuccess.emit(data);
      },
      error: () => {
        this.isActionInProgress.set(false);
      }
    });
  }

  async _handleOnPrimaryStageClick() {
    const canProceed = await this.beforeStageChange(this.primaryeStageName);
    if (!canProceed) {
      return;
    }

    if (this.primaryStageRequiresApproval === true) {
      this.feedbackDialogService?.showConfirmDialog(
        {
          detail: 'Moving to this stage needs approval. Do you want to continue?',
        },
        () => {
          this.handleAfterApprovalOrValidation(this.primaryeStageName, 'Submit', this.primaryStageCommentMode);
        }
      );
    } else {
      this.handleAfterApprovalOrValidation(
        this.primaryeStageName,
        this.primaryeStageLabel,
        this.primaryStageCommentMode
      );
    }
  }

  private async handleAfterApprovalOrValidation(nextStage: string, actionName: string, commentMode: string) {
    if (this.customStageChangeHandler) {
      this.nextStage.set(nextStage);
      this.nextStageActionName.set(actionName);

      const result = await this.customStageChangeHandler(nextStage, actionName);

      if (result !== undefined) {
        if (result.proceed) {
          this._performStageChange(result.comment || '');
        }
      } else {
        this._executeStageChange(nextStage, actionName, commentMode);
      }
    } else {
      this._executeStageChange(nextStage, actionName, commentMode);
    }
  }

  handleOnCommentSave(comment: string) {
    if (this.commentMode() == 'mandatory' && comment?.trim() == '') {
      this.feedbackDialogService?.showInfoToast({ detail: 'Comment must be entered.' });
      return;
    }
    
    const actionName = this.nextStageActionName().toLowerCase();
    if (actionName === 'approve' || actionName === 'reject' || actionName === 'recall') {
      this._performWorkflowAction(actionName as 'approve' | 'reject' | 'recall', comment);
    } else {
      this._performStageChange(comment);
    }
    
    this.showCommentDialog = false;
  }

  handleOnApprove() {
    this.nextStage.set(this.workflowInfo()?.nextStage || '');
    this.nextStageActionName.set('Approve');
    this.commentMode.set(this.workflowInfo()?.approvalComment || 'optional');

    const commentMode = this.workflowInfo()?.approvalComment || 'optional';
    if (commentMode !== 'none') {
      this.showCommentDialog = true;
      this.changeDetectorRef.detectChanges();
    } else {
      this._performWorkflowAction('approve');
    }
  }

  handleOnReject() {
    this.nextStage.set(this.workflowInfo()?.nextStage || '');
    this.nextStageActionName.set('Reject');
    this.commentMode.set(this.workflowInfo()?.rejectionComment || 'mandatory');
    this.showCommentDialog = true;
    this.changeDetectorRef.detectChanges();
  }

  handleOnRecall() {
    this.nextStage.set(this.workflowInfo()?.nextStage || '');
    this.nextStageActionName.set('Recall');
    this.commentMode.set(this.workflowInfo()?.recallComment || 'mandatory');
    this.showCommentDialog = true;
    this.changeDetectorRef.detectChanges();
  }

  private _performWorkflowAction(action: 'approve' | 'reject' | 'recall', comment?: string) {
    this.isActionInProgress.set(true);
    
    const requestJson: any = {
      entityName: this.entityName(),
      entityId: parseInt(this.entityId(), 10),
      comment: comment || undefined,
    };

    const endpoint = `${action}`;
    
    this.http.post(`/api/workflow/${endpoint}`, requestJson).subscribe({
      next: (data: any) => {
        this.isActionInProgress.set(false);
        
        const actionPastTense = action === 'approve' ? 'approved' : action === 'reject' ? 'rejected' : 'recalled';
        this.feedbackDialogService?.showSuccessToast({
          detail: `Workflow ${actionPastTense} successfully!`,
        });
        
        // Reload workflow state
        if (this.autoload() === true) {
          this.load();
        }
        
        this.stageChangeSuccess.emit(data);
      },
      error: () => {
        this.isActionInProgress.set(false);
      }
    });
  }
}
