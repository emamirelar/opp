/**
 * @fileoverview Workflow action component for executing workflow transitions
 * @author Opportunity+ Development Team
 */

import { Component, inject, input, Input, OnInit, output, signal, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MenuItem } from 'primeng/api';

import { SplitButton } from 'primeng/splitbutton';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SkeletonModule } from 'primeng/skeleton';

import { WorkflowService } from '../../services/workflow.service';
import {
  WorkflowStateActionModel,
  CustomStageChangeResult,
  WorkflowActionModel,
  WorkflowSubmitRequest,
  WorkflowSubmitResponse,
  ConfirmationType,
  CountryMappingInfo,
} from '../../models/workflow.models';

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
  imports: [
    SplitButton,
    DialogModule,
    ButtonModule,
    TextareaModule,
    CheckboxModule,
    SkeletonModule,
    TranslateModule,
    FormsModule,
  ],
})
export class WorkflowComponent implements OnInit {
  private changeDetectorRef = inject(ChangeDetectorRef);
  private http = inject(HttpClient);
  private translateService = inject(TranslateService);

  // Injected feedback service - must be provided by consuming application
  @Input() feedbackDialogService!: IFeedbackDialogService;

  entityName = input.required<string>();
  entityId = input.required<string>();
  autoload = input<boolean>(true);
  isReadOnly = input<boolean>(false);

  /**
   * Name of the responsible org unit (for acknowledgment dialog display)
   */
  @Input() responsibleOrgUnitName = '';

  @Input() disabled!: boolean;
  @Input() beforeStageChange: (nextStage: string) => Promise<boolean> = async () => true;
  @Input() customStageChangeHandler?: (
    nextStage: string,
    actionName: string
  ) => Promise<CustomStageChangeResult | undefined>;

  stageChangeSuccess = output();

  /**
   * Emitted when a GO submission is successful (after all confirmations).
   * Parent component can use this to trigger PDF generation.
   * Contains entityName, entityId, and newStage.
   */
  goSubmissionSuccess = output<{ entityName: string; entityId: number; newStage: string }>();

  /**
   * Emitted when a GO approval is successful.
   * Parent component can use this to trigger PDF generation.
   * Contains entityName, entityId, and approvedStage.
   */
  goApprovalSuccess = output<{ entityName: string; entityId: number; approvedStage: string }>();

  /**
   * Emitted when requirements validation fails during submission
   * Parent component can use this to scroll to the requirements panel
   */
  requirementsValidationFailed = output<string[]>();

  /**
   * Emitted when any workflow action starts or completes.
   * Parent component can use this to show/hide a loading overlay.
   * @param {boolean} inProgress - true when action starts, false when action completes
   */
  actionInProgressChange = output<boolean>();

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

  // Dialog state for confirmation flows
  showNonOMWarningDialog = signal(false);
  showOrgUnitMismatchDialog = signal(false);
  showAcknowledgmentDialog = signal(false);
  showRejectToNoGoDialog = signal(false);

  // Non-OM warning dialog state
  nonOMWarningRole = signal('');
  opportunityManagerInfo = signal('');
  nonOMWarningConfirmed = signal(false);

  // Org unit mismatch dialog state
  unrelatedCountries = signal<string[]>([]);
  countryMappings = signal<CountryMappingInfo[]>([]);
  selectedOrgUnitName = signal('');
  orgUnitMismatchConfirmed = signal(false);

  // Acknowledgment dialog state
  acknowledgmentText = signal('');
  acknowledgmentOrgUnitName = signal('');
  acknowledgmentChecked = signal(false);
  additionalRemarks = signal('');

  // Rejection to NO GO dialog state
  rejectToNoGoComment = signal('');

  // Recall dialog state
  showRecallDialog = signal(false);
  recallComment = signal('');

  // Unmet requirements dialog state (for server-side validation failures)
  showUnmetRequirementsDialog = signal(false);
  unmetRequirements = signal<string[]>([]);

  // Pending submit request (to continue after confirmation)
  pendingSubmitRequest = signal<WorkflowSubmitRequest | null>(null);

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
    let actions = workflowData?.nextActions || [];

    this.workflowInfo.set(workflowData?.workflow || {});
    this.isInWorkflow.set(workflowData?.isInWorkflow || false);

    // For Opportunities, filter out the "No Go" action since users don't submit directly for No Go
    // (No Go is only reached when a Go Decision is rejected)
    if (this.entityName().toLowerCase() === 'opportunity') {
      actions = actions.filter((action: any) => {
        const newStage = (action['newStage'] || '').toUpperCase();
        return !newStage.includes('NO GO') && !newStage.includes('NO_GO') && !newStage.includes('NOGO');
      });
    }

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

            // For opportunity Go Decision flow, use the specialized submit endpoint
            if (item?.requiresApproval === true && this.entityName().toLowerCase() === 'opportunity') {
              const request: WorkflowSubmitRequest = {
                entityName: this.entityName(),
                entityId: parseInt(this.entityId(), 10),
                newStage: item['newStage'],
              };
              this._submitForGoDecision(request);
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
    this.actionInProgressChange.emit(true);
    
    const requestJson: WorkflowActionModel = {
      entityName: this.entityName(),
      entityId: parseInt(this.entityId(), 10),
      newStage: this.nextStage(),
      comment: comment || undefined,
    };

    this.workflowService.changeWorkflow(requestJson).subscribe({
      next: (data: any) => {
        this.isActionInProgress.set(false);
        this.actionInProgressChange.emit(false);
        
        // Show success message
        this.feedbackDialogService?.showSuccessToast({
          detail: 'Stage changed successfully!',
        });
        
        // Reload workflow state to get updated actions and approval status
        if (this.autoload() === true) {
          this.load(); // Reload from server to get current state
        }

        this.stageChangeSuccess.emit();
      },
      error: () => {
        this.isActionInProgress.set(false);
        this.actionInProgressChange.emit(false);
      },
    });
  }

  async _handleOnPrimaryStageClick() {
    const canProceed = await this.beforeStageChange(this.primaryeStageName);
    if (!canProceed) {
      return;
    }

    // For opportunity Go Decision flow, use the specialized submit endpoint
    // which handles Non-OM warning, Org Unit mismatch, and Acknowledgment dialogs
    if (this.primaryStageRequiresApproval === true && this.entityName().toLowerCase() === 'opportunity') {
      const request: WorkflowSubmitRequest = {
        entityName: this.entityName(),
        entityId: parseInt(this.entityId(), 10),
        newStage: this.primaryeStageName,
      };
      this._submitForGoDecision(request);
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

  async handleOnApprove() {
    this.nextStage.set(this.workflowInfo()?.nextStage || '');
    this.nextStageActionName.set('Approve');
    this.commentMode.set(this.workflowInfo()?.approvalComment || 'optional');

    // Check for custom stage change handler first (e.g., Go Decision dialog)
    if (this.customStageChangeHandler) {
      const result = await this.customStageChangeHandler(
        this.workflowInfo()?.nextStage || '',
        'Approve'
      );

      if (result !== undefined) {
        if (result.proceed) {
          this._executeWorkflowAction('approve', result.comment);
        }
        return; // Custom handler took over
      }
      // result is undefined, fall through to default behavior
    }

    const commentMode = this.workflowInfo()?.approvalComment || 'optional';
    if (commentMode !== 'none') {
      this.showCommentDialog = true;
      this.changeDetectorRef.detectChanges();
    } else {
      this._performWorkflowAction('approve');
    }
  }

  async handleOnReject() {
    this.nextStage.set(this.workflowInfo()?.nextStage || '');
    this.nextStageActionName.set('Reject');
    this.commentMode.set(this.workflowInfo()?.rejectionComment || 'mandatory');

    // Check for custom stage change handler first (e.g., No-Go Decision dialog)
    if (this.customStageChangeHandler) {
      const result = await this.customStageChangeHandler(
        this.workflowInfo()?.nextStage || '',
        'Reject'
      );

      if (result !== undefined) {
        if (result.proceed) {
          this._executeWorkflowAction('reject', result.comment);
        }
        return; // Custom handler took over
      }
      // result is undefined, fall through to default behavior
    }

    this.showCommentDialog = true;
    this.changeDetectorRef.detectChanges();
  }

  handleOnRecall(): void {
    this.nextStage.set(this.workflowInfo()?.nextStage || '');
    this.nextStageActionName.set('Recall');
    this.recallComment.set('');
    this.showRecallDialog.set(true);
    this.changeDetectorRef.detectChanges();
  }

  private _performWorkflowAction(action: 'approve' | 'reject' | 'recall', comment?: string) {
    // For rejection, show the NO GO confirmation dialog first (only for opportunities)
    if (action === 'reject' && this.entityName().toLowerCase() === 'opportunity') {
      this.rejectToNoGoComment.set(comment || '');
      this.showRejectToNoGoDialog.set(true);
      this.changeDetectorRef.detectChanges();
      return;
    }

    this._executeWorkflowAction(action, comment);
  }

  /**
   * Execute the workflow action after any confirmations
   */
  private _executeWorkflowAction(action: 'approve' | 'reject' | 'recall', comment?: string) {
    this.isActionInProgress.set(true);
    this.actionInProgressChange.emit(true);

    const requestJson: any = {
      entityName: this.entityName(),
      entityId: parseInt(this.entityId(), 10),
      comment: comment || undefined,
    };

    const endpoint = `${action}`;

    this.http.post(`/api/workflow/${endpoint}`, requestJson).subscribe({
      next: (data: any) => {
        this.isActionInProgress.set(false);
        this.actionInProgressChange.emit(false);

        const actionPastTense = action === 'approve' ? 'approved' : action === 'reject' ? 'rejected' : 'recalled';
        this.feedbackDialogService?.showSuccessToast({
          detail: `Workflow ${actionPastTense} successfully!`,
        });

        // Reload workflow state
        if (this.autoload() === true) {
          this.load();
        }

        this.stageChangeSuccess.emit();

        // Emit GO approval success event for PDF generation (only for opportunities)
        if (action === 'approve' && this.entityName().toLowerCase() === 'opportunity') {
          this.goApprovalSuccess.emit({
            entityName: this.entityName(),
            entityId: parseInt(this.entityId(), 10),
            approvedStage: this.workflowInfo()?.nextStage || 'GO',
          });
        }
      },
      error: () => {
        this.isActionInProgress.set(false);
        this.actionInProgressChange.emit(false);
      },
    });
  }

  /**
   * Handles the response from submit for Go Decision
   * May show confirmation dialogs or acknowledgment dialog
   */
  handleSubmitResponse(response: WorkflowSubmitResponse, request: WorkflowSubmitRequest): void {
    if (response.success) {
      this.feedbackDialogService?.showSuccessToast({
        detail: this.translateService.instant('message.workflow.submitSuccess'),
      });
      if (this.autoload() === true) {
        this.load();
      }
      this.stageChangeSuccess.emit();
      
      // Emit GO submission success event for PDF generation
      // This is emitted for opportunity submissions to GO stage
      if (this.entityName().toLowerCase() === 'opportunity') {
        this.goSubmissionSuccess.emit({
          entityName: this.entityName(),
          entityId: parseInt(this.entityId(), 10),
          newStage: request.newStage,
        });
      }
      return;
    }

    // PRD Flow: Check if requirements are not met (first check in flow)
    if (response.requirementsNotMet) {
      // Store unmet requirements and show dialog
      this.unmetRequirements.set(response.unmetRequirements || []);
      this.showUnmetRequirementsDialog.set(true);
      this.changeDetectorRef.detectChanges();
      // Emit event to notify parent that requirements validation failed
      // Parent can scroll to requirements panel
      this.requirementsValidationFailed.emit(response.unmetRequirements || []);
      return;
    }

    // Store pending request for re-submission after confirmation
    this.pendingSubmitRequest.set(request);

    if (response.requiresConfirmation) {
      const confirmationType = response.confirmationType;

      if (confirmationType === 'NonOMSubmitter') {
        // Extract role from message or use generic
        const roleMatch = response.confirmationMessage?.match(/\[([^\]]+)\]/);
        this.nonOMWarningRole.set(roleMatch ? roleMatch[1] : 'stakeholder');
        this.opportunityManagerInfo.set(response.opportunityManagerInfo || '');
        this.nonOMWarningConfirmed.set(false);
        this.showNonOMWarningDialog.set(true);
        this.changeDetectorRef.detectChanges();
      } else if (confirmationType === 'OrgUnitCountryMismatch') {
        this.unrelatedCountries.set(response.unrelatedCountries || []);
        this.countryMappings.set(response.countryMappings || []);
        this.selectedOrgUnitName.set(response.responsibleOrgUnitName || this.responsibleOrgUnitName || '');
        this.orgUnitMismatchConfirmed.set(false);
        this.showOrgUnitMismatchDialog.set(true);
        this.changeDetectorRef.detectChanges();
      }
    } else if (response.requiresAcknowledgment) {
      // Format acknowledgment text with org unit name
      const orgUnitName = response.responsibleOrgUnitName || this.responsibleOrgUnitName || '';
      const text =
        response.acknowledgmentText ||
        this.translateService.instant('message.workflow.acknowledgmentStatement', {
          orgUnitName: orgUnitName,
        });
      this.acknowledgmentText.set(text);
      this.acknowledgmentOrgUnitName.set(orgUnitName);
      this.acknowledgmentChecked.set(false);
      this.additionalRemarks.set('');
      this.showAcknowledgmentDialog.set(true);
      this.changeDetectorRef.detectChanges();
    }
  }

  /**
   * Confirm non-OM submitter warning and re-submit
   */
  confirmNonOMWarning(): void {
    this.showNonOMWarningDialog.set(false);
    const request = this.pendingSubmitRequest();
    if (request) {
      request.confirmedNonOMSubmission = true;
      this._submitForGoDecision(request);
    }
  }

  /**
   * Close non-OM warning dialog
   */
  closeNonOMWarningDialog(): void {
    this.showNonOMWarningDialog.set(false);
    this.pendingSubmitRequest.set(null);
  }

  /**
   * Handle non-OM warning dialog visibility change (for X button close)
   */
  onNonOMWarningDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.closeNonOMWarningDialog();
    }
  }

  /**
   * Confirm org unit mismatch warning and re-submit
   */
  confirmOrgUnitMismatch(): void {
    this.showOrgUnitMismatchDialog.set(false);
    const request = this.pendingSubmitRequest();
    if (request) {
      request.confirmedOrgUnitWarning = true;
      this._submitForGoDecision(request);
    }
  }

  /**
   * Close org unit mismatch dialog
   */
  closeOrgUnitMismatchDialog(): void {
    this.showOrgUnitMismatchDialog.set(false);
    this.pendingSubmitRequest.set(null);
  }

  /**
   * Handle org unit mismatch dialog visibility change (for X button close)
   */
  onOrgUnitMismatchDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.closeOrgUnitMismatchDialog();
    }
  }

  /**
   * Confirm acknowledgment and submit
   */
  confirmAcknowledgment(): void {
    if (!this.acknowledgmentChecked()) {
      this.feedbackDialogService?.showInfoToast({
        detail: this.translateService.instant('message.workflow.acknowledgmentRequired'),
      });
      return;
    }

    this.showAcknowledgmentDialog.set(false);
    const request = this.pendingSubmitRequest();
    if (request) {
      request.acknowledgedStatement = true;
      // Map additionalRemarks to comment field for backend
      const remarks = this.additionalRemarks().trim();
      request.additionalRemarks = remarks || undefined;
      request.comment = remarks || undefined;
      this._submitForGoDecision(request);
    }
  }

  /**
   * Close acknowledgment dialog
   */
  closeAcknowledgmentDialog(): void {
    this.showAcknowledgmentDialog.set(false);
    this.pendingSubmitRequest.set(null);
  }

  /**
   * Handle acknowledgment dialog visibility change (for X button close)
   */
  onAcknowledgmentDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.closeAcknowledgmentDialog();
    }
  }

  /**
   * Confirm rejection to NO GO
   */
  confirmRejectToNoGo(): void {
    const comment = this.rejectToNoGoComment().trim();
    if (!comment) {
      this.feedbackDialogService?.showInfoToast({
        detail: this.translateService.instant('message.workflow.rejectReasonRequired'),
      });
      return;
    }

    this.showRejectToNoGoDialog.set(false);
    this._executeWorkflowAction('reject', comment);
  }

  /**
   * Close reject to NO GO dialog
   */
  closeRejectToNoGoDialog(): void {
    this.showRejectToNoGoDialog.set(false);
    this.rejectToNoGoComment.set('');
  }

  /**
   * Handle reject to NO GO dialog visibility change (for X button close)
   */
  onRejectToNoGoDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.closeRejectToNoGoDialog();
    }
  }

  /**
   * Confirm and execute recall action
   */
  confirmRecall(): void {
    const comment = this.recallComment().trim();
    if (!comment) {
      this.feedbackDialogService?.showInfoToast({
        detail: this.translateService.instant('message.workflow.recallReasonRequired'),
      });
      return;
    }

    this.showRecallDialog.set(false);
    this._executeWorkflowAction('recall', comment);
  }

  /**
   * Close recall dialog
   */
  closeRecallDialog(): void {
    this.showRecallDialog.set(false);
    this.recallComment.set('');
  }

  /**
   * Handle recall dialog visibility change (for X button close)
   */
  onRecallDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.closeRecallDialog();
    }
  }

  /**
   * Close unmet requirements dialog
   */
  closeUnmetRequirementsDialog(): void {
    this.showUnmetRequirementsDialog.set(false);
    this.unmetRequirements.set([]);
  }

  /**
   * Handle unmet requirements dialog visibility change (for X button close)
   */
  onUnmetRequirementsDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.closeUnmetRequirementsDialog();
    }
  }

  /**
   * Internal method to submit for Go decision
   */
  private _submitForGoDecision(request: WorkflowSubmitRequest): void {
    this.isActionInProgress.set(true);
    this.actionInProgressChange.emit(true);

    this.workflowService.submitForGoDecision(request).subscribe({
      next: (response: WorkflowSubmitResponse) => {
        this.isActionInProgress.set(false);
        this.actionInProgressChange.emit(false);
        this.handleSubmitResponse(response, request);
      },
      error: () => {
        this.isActionInProgress.set(false);
        this.actionInProgressChange.emit(false);
      },
    });
  }
}
