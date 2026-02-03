/**
 * @fileoverview Dialog component for Go Decision (Approve) workflow action
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
  model,
  input,
  output,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

// Services
import { OpportunityService } from '../../../services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui';

// Models
import {
  Opportunity,
  GoDecisionPayload,
  ExecutiveOption,
} from '@shared/models/opportunity.model';

/**
 * @class ApproveOpportunityDialogComponent
 * @description Dialog for confirming a Go decision on an opportunity.
 * Displays a confirmation statement, requires rationale input, and
 * mandatory Executive selection from org unit's Director/Manager/OiC.
 * @since 1.0.0
 */
@Component({
  selector: 'app-approve-opportunity-dialog',
  templateUrl: './approve-opportunity-dialog.component.html',
  styleUrl: './approve-opportunity-dialog.component.scss',
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    DialogModule,
    ButtonModule,
    CheckboxModule,
    TextareaModule,
    SelectModule,
    FloatLabelModule,
    ProgressSpinnerModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApproveOpportunityDialogComponent implements OnInit {
  private opportunityService = inject(OpportunityService);
  private feedbackService = inject(FeedbackDialogService);
  private translateService = inject(TranslateService);

  /**
   * @description Two-way binding for dialog visibility
   * @type {ModelSignal<boolean>}
   */
  readonly visible = model<boolean>(false);

  /**
   * @description The opportunity being approved
   * @type {InputSignal<Opportunity>}
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Event emitted when the Go decision is confirmed
   * @type {OutputEmitterRef<GoDecisionPayload>}
   */
  readonly decisionConfirmed = output<GoDecisionPayload>();

  // Form state signals
  readonly confirmationAcknowledged = signal<boolean>(false);
  readonly decisionRationale = signal<string>('');
  readonly selectedExecutiveId = signal<number | null>(null);

  // Data signals
  readonly executives = signal<ExecutiveOption[]>([]);
  readonly isLoadingExecutives = signal<boolean>(false);
  readonly isSubmitting = signal<boolean>(false);

  /**
   * @description Computed confirmation statement based on opportunity data
   * @returns {string} The confirmation statement text
   */
  readonly confirmationStatement = computed(() => {
    const opp = this.opportunity();
    const orgUnitCode = opp?.responsibleOrgUnitName || '';
    const initiativeType = opp?.proposedInitiativeTypeName || '';

    return this.translateService.instant('workflow.goDecision.dialog.approve.confirmationStatement', {
      orgUnitCode: orgUnitCode,
      initiativeType: initiativeType,
    });
  });

  /**
   * @description Computed flag indicating if the form can be submitted
   * @returns {boolean} True if all required fields are valid
   */
  readonly canSubmit = computed(() => {
    return (
      this.confirmationAcknowledged() &&
      this.decisionRationale().trim().length > 0 &&
      this.selectedExecutiveId() !== null
    );
  });

  ngOnInit(): void {
    this.loadExecutives();
  }

  /**
   * @description Load executives for the opportunity's responsible org unit
   */
  loadExecutives(): void {
    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) return;

    this.isLoadingExecutives.set(true);

    this.opportunityService.getExecutivesForOpportunity(opportunityId).subscribe({
      next: (executives) => {
        this.executives.set(executives);
        this.isLoadingExecutives.set(false);

        // Pre-select the suggested executive (if any)
        const suggested = executives.find((e) => e.description === 'Suggested');
        if (suggested) {
          this.selectedExecutiveId.set(suggested.value);
        } else if (executives.length === 1) {
          // If only one executive, select it automatically
          this.selectedExecutiveId.set(executives[0].value);
        }
      },
      error: () => {
        this.isLoadingExecutives.set(false);
      },
    });
  }

  /**
   * @description Handle confirmation checkbox change
   * @param {boolean} checked - New checkbox state
   */
  onConfirmationChange(checked: boolean): void {
    this.confirmationAcknowledged.set(checked);
  }

  /**
   * @description Handle rationale input change
   * @param {string} value - New rationale value
   */
  onRationaleChange(value: string): void {
    this.decisionRationale.set(value);
  }

  /**
   * @description Handle executive selection change
   * @param {number} executiveId - Selected executive ID
   */
  onExecutiveChange(executiveId: number): void {
    this.selectedExecutiveId.set(executiveId);
  }

  /**
   * @description Submit the Go decision
   */
  onSubmit(): void {
    if (!this.canSubmit()) return;

    const payload: GoDecisionPayload = {
      rationale: this.decisionRationale().trim(),
      executiveId: this.selectedExecutiveId()!,
      confirmationAcknowledged: true,
    };

    this.isSubmitting.set(true);

    this.opportunityService.approveOpportunity(this.opportunity().id, payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('workflow.goDecision.message.approveSuccess'),
        });
        this.decisionConfirmed.emit(payload);
        this.resetForm();
        this.visible.set(false);
      },
      error: () => {
        this.isSubmitting.set(false);
      },
    });
  }

  /**
   * @description Cancel and close the dialog
   */
  onCancel(): void {
    this.resetForm();
    this.visible.set(false);
  }

  /**
   * @description Reset form to initial state
   */
  private resetForm(): void {
    this.confirmationAcknowledged.set(false);
    this.decisionRationale.set('');
    // Keep executive selection for potential re-open
  }
}
