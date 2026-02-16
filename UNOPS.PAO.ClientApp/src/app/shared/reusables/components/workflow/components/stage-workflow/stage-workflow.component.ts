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
  computed,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { MenuItem } from 'primeng/api';
import { StepsModule } from 'primeng/steps';
import { FieldsetModule } from 'primeng/fieldset';
import { TableModule } from 'primeng/table';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { TagModule } from 'primeng/tag';
import { TabsModule } from 'primeng/tabs';
import { SkeletonModule } from 'primeng/skeleton';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

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
    ButtonModule,
    DialogModule,
    TextareaModule,
    FormsModule,
    DatePipe,
  ],
  templateUrl: './stage-workflow.component.html',
  styleUrl: './stage-workflow.component.scss',
})
export class StageWorkflowComponent implements OnInit, OnChanges {
  workflowService = inject(WorkflowService);
  private translateService = inject(TranslateService);

  @ViewChild('workFlowComponent') workflowComponent!: WorkflowComponent;

  entityName = input<string>('');
  entityId = input<string>('');
  canChangeStage = input<boolean>(true);
  
  /**
   * Whether the panel should be initially collapsed
   * @default false
   */
  collapsed = input<boolean>(false);
  
  onStageChangeSuccess = output();

  /**
   * Emitted when a GO submission is successful (after all confirmations).
   * Parent component can use this to trigger PDF generation.
   * Contains entityName, entityId, and newStage.
   */
  onGoSubmissionSuccess = output<{ entityName: string; entityId: number; newStage: string }>();

  /**
   * Emitted when a GO approval is successful.
   * Parent component can use this to trigger PDF generation.
   * Contains entityName, entityId, and approvedStage.
   */
  onGoApprovalSuccess = output<{ entityName: string; entityId: number; approvedStage: string }>();

  /**
   * Emitted when requirements validation fails during submission.
   * Contains list of unmet requirement message keys.
   * Parent component can use this to display errors or scroll to requirements panel.
   */
  onRequirementsValidationFailed = output<string[]>();

  /**
   * Emitted when any workflow action (Submit, Approve, Reject, Recall, Cancel, Reopen) starts or completes.
   * Parent component can use this to show/hide a full-page loading overlay with blur effect.
   * @param {boolean} inProgress - true when action starts, false when action completes
   */
  onActionInProgressChange = output<boolean>();

  /**
   * Whether the current user is an Opportunity Manager for this opportunity
   * Controls visibility of Cancel/Reopen buttons
   */
  isOpportunityManager = input<boolean>(false);

  /**
   * Name of the responsible org unit (for acknowledgment dialog display)
   */
  responsibleOrgUnitName = input<string>('');

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

  // Cancel/Reopen dialog state
  showCancelDialog = signal(false);
  showReopenDialog = signal(false);
  cancelReason = signal('');
  reopenReason = signal('');
  isActionInProgress = signal(false);

  @Input() beforeStageChange: (nextStage: string) => Promise<boolean> = async () => true;
  @Input() customStageChangeHandler?: (
    nextStage: string,
    actionName: string
  ) => Promise<CustomStageChangeResult | undefined>;

  /**
   * Computed signal that filters stages for happy-path display
   * - Default (IDENTIFY & PROFILE or GO): show only IDENTIFY & PROFILE → GO
   * - NO GO: show IDENTIFY & PROFILE → NO GO
   * - CANCELLED: show IDENTIFY & PROFILE → CANCELLED
   */
  readonly displayStages = computed(() => {
    return this.getDisplayStages(this.stages(), this.currentStageName());
  });

  /**
   * Computed signal for display stage index (relative to filtered stages)
   */
  readonly displayStageIndex = computed(() => {
    const displayStages = this.displayStages();
    const currentStage = this.currentStageName();
    return displayStages.findIndex((item: MenuItem) => item['name'] === currentStage);
  });

  /**
   * Whether Cancel button should be visible
   * For users who can change stage when in IDENTIFY & PROFILE and not in workflow
   * Uses isOpportunityManager if explicitly set, otherwise falls back to canChangeStage
   */
  readonly canCancel = computed(() => {
    const hasPermission = this.isOpportunityManager() || this.canChangeStage();
    return (
      hasPermission &&
      this.currentStageName() === 'IDENTIFY & PROFILE' &&
      !this.workflowData()?.isInWorkflow
    );
  });

  /**
   * Whether Reopen button should be visible
   * For users who can change stage when in NO GO or CANCELLED
   * Uses isOpportunityManager if explicitly set, otherwise falls back to canChangeStage
   */
  readonly canReopen = computed(() => {
    const stage = this.currentStageName();
    const hasPermission = this.isOpportunityManager() || this.canChangeStage();
    return hasPermission && (stage === 'NO GO' || stage === 'CANCELLED');
  });

  /**
   * Whether reopen requires a mandatory reason (CANCELLED stage)
   */
  readonly reopenRequiresReason = computed(() => {
    return this.currentStageName() === 'CANCELLED';
  });

  /**
   * Whether the opportunity is in CANCELLED stage
   */
  readonly isCancelled = computed(() => {
    return this.currentStageName() === 'CANCELLED';
  });

  /**
   * Whether the opportunity is in NO GO stage
   */
  readonly isNoGo = computed(() => {
    return this.currentStageName() === 'NO GO';
  });

  /**
   * Get the cancellation reason from the stage change history
   * Finds the most recent transition to CANCELLED stage and returns its comment
   */
  readonly cancellationReason = computed(() => {
    if (!this.isCancelled()) return null;
    
    const history = this.stageChangeHistory();
    if (!history || history.length === 0) return null;

    // Find the most recent entry where toStage is CANCELLED
    const cancelEntry = history.find(
      (entry: any) => entry.toStage === 'CANCELLED' || entry.toStageDisplayName === 'CANCELLED'
    );
    
    return cancelEntry?.comment || null;
  });

  /**
   * Get the rejection reason from the stage change history
   * Finds the most recent transition to NO GO stage and returns its comment
   */
  readonly rejectionReason = computed(() => {
    if (!this.isNoGo()) return null;
    
    const history = this.stageChangeHistory();
    if (!history || history.length === 0) return null;

    // Find the most recent entry where toStage is NO GO
    const rejectEntry = history.find(
      (entry: any) => entry.toStage === 'NO GO' || entry.toStageDisplayName === 'NO GO'
    );
    
    return rejectEntry?.comment || null;
  });

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
            // Filter out Cancel and Reopen actions - these have dedicated buttons with custom dialogs
            const filteredActions = (data.availableActions || []).filter((action: any) => {
              const targetStage = action.targetStage || action.newStage || '';
              
              // Exclude Cancel action (→ CANCELLED) - has custom dialog
              if (targetStage === 'CANCELLED') {
                return false;
              }
              
              // Exclude Reopen action (→ IDENTIFY & PROFILE from NO GO or CANCELLED) - has custom dialog
              if (targetStage === 'IDENTIFY & PROFILE' && (currentStage === 'NO GO' || currentStage === 'CANCELLED')) {
                return false;
              }
              
              return true;
            });
            
            const transformedData = {
              stage: currentStage,
              displayName: displayName,
              isInWorkflow: false,
              nextActions: filteredActions.map((action: any) => ({
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

  /**
   * Handles requirements validation failure from workflow component.
   * Propagates the event to parent component.
   * @param unmetRequirements Array of unmet requirement message keys
   */
  handleRequirementsValidationFailed(unmetRequirements: string[]): void {
    this.onRequirementsValidationFailed.emit(unmetRequirements);
  }

  /**
   * Handles GO submission success from workflow component.
   * Propagates the event to parent component for PDF generation.
   * @param data Object containing entityName, entityId, and newStage
   */
  handleGoSubmissionSuccess(data: { entityName: string; entityId: number; newStage: string }): void {
    this.onGoSubmissionSuccess.emit(data);
  }

  /**
   * Handles GO approval success from workflow component.
   * Propagates the event to parent component for PDF generation.
   * @param data Object containing entityName, entityId, and approvedStage
   */
  handleGoApprovalSuccess(data: { entityName: string; entityId: number; approvedStage: string }): void {
    this.onGoApprovalSuccess.emit(data);
  }

  /**
   * Handles action in progress state change from workflow component.
   * Propagates the event to parent component for showing/hiding loading overlay.
   * @param inProgress Whether a workflow action is in progress
   */
  handleActionInProgressChange(inProgress: boolean): void {
    this.onActionInProgressChange.emit(inProgress);
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

  /**
   * Filters stages for happy-path display based on current stage
   * @param allStages All workflow stages
   * @param currentStage Current stage name
   * @returns Filtered stages for display
   */
  getDisplayStages(allStages: MenuItem[], currentStage: string): MenuItem[] {
    const happyPath = ['IDENTIFY & PROFILE', 'GO'];
    const noGoPath = ['IDENTIFY & PROFILE', 'NO GO'];
    const cancelledPath = ['IDENTIFY & PROFILE', 'CANCELLED'];

    let stagesToShow: string[];
    switch (currentStage) {
      case 'NO GO':
        stagesToShow = noGoPath;
        break;
      case 'CANCELLED':
        stagesToShow = cancelledPath;
        break;
      default:
        // For IDENTIFY & PROFILE and GO stages, show happy path
        stagesToShow = happyPath;
    }

    return allStages.filter((s) => stagesToShow.includes(s['name'] as string));
  }

  /**
   * Opens the cancel confirmation dialog
   */
  openCancelDialog(): void {
    this.cancelReason.set('');
    this.showCancelDialog.set(true);
  }

  /**
   * Closes the cancel dialog
   */
  closeCancelDialog(): void {
    this.showCancelDialog.set(false);
    this.cancelReason.set('');
  }

  /**
   * Handles visibility change from p-dialog (for X button close)
   */
  onCancelDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.closeCancelDialog();
    }
  }

  /**
   * Confirms and executes the cancel action
   */
  confirmCancel(): void {
    const reason = this.cancelReason().trim();
    if (!reason) {
      this.feedbackDialogService?.showInfoToast({
        detail: this.translateService.instant('message.workflow.cancelReasonRequired'),
      });
      return;
    }

    this.isActionInProgress.set(true);
    this.onActionInProgressChange.emit(true);
    this.workflowService.cancelOpportunity(this.entityId(), reason).subscribe({
      next: () => {
        this.isActionInProgress.set(false);
        this.onActionInProgressChange.emit(false);
        this.showCancelDialog.set(false);
        this.feedbackDialogService?.showSuccessToast({
          detail: this.translateService.instant('message.workflow.cancelSuccess'),
        });
        this.loadData();
        this.onStageChangeSuccess.emit();
      },
      error: () => {
        this.isActionInProgress.set(false);
        this.onActionInProgressChange.emit(false);
      },
    });
  }

  /**
   * Opens the reopen confirmation dialog
   */
  openReopenDialog(): void {
    this.reopenReason.set('');
    this.showReopenDialog.set(true);
  }

  /**
   * Closes the reopen dialog
   */
  closeReopenDialog(): void {
    this.showReopenDialog.set(false);
    this.reopenReason.set('');
  }

  /**
   * Handles visibility change from p-dialog (for X button close)
   */
  onReopenDialogVisibleChange(visible: boolean): void {
    if (!visible) {
      this.closeReopenDialog();
    }
  }

  /**
   * Confirms and executes the reopen action
   */
  confirmReopen(): void {
    const reason = this.reopenReason().trim();

    // For CANCELLED stage, reason is mandatory
    if (this.reopenRequiresReason() && !reason) {
      this.feedbackDialogService?.showInfoToast({
        detail: this.translateService.instant('message.workflow.reopenReasonRequired'),
      });
      return;
    }

    this.isActionInProgress.set(true);
    this.onActionInProgressChange.emit(true);
    this.workflowService.reopenOpportunity(this.entityId(), reason || undefined).subscribe({
      next: () => {
        this.isActionInProgress.set(false);
        this.onActionInProgressChange.emit(false);
        this.showReopenDialog.set(false);
        this.feedbackDialogService?.showSuccessToast({
          detail: this.translateService.instant('message.workflow.reopenSuccess'),
        });
        this.loadData();
        this.onStageChangeSuccess.emit();
      },
      error: () => {
        this.isActionInProgress.set(false);
        this.onActionInProgressChange.emit(false);
      },
    });
  }
}
