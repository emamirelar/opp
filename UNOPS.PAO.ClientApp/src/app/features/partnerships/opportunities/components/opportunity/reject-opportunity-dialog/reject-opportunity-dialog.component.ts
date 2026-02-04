/**
 * @fileoverview Dialog component for No-Go Decision (Reject) workflow action
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
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
import { FloatLabelModule } from 'primeng/floatlabel';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

// Services
import { OpportunityService } from '../../../services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui';

// Models
import { Opportunity, NoGoDecisionPayload } from '@shared/models/opportunity.model';

/**
 * @class RejectOpportunityDialogComponent
 * @description Dialog for confirming a No-Go decision on an opportunity.
 * Displays a warning message, requires acknowledgment of confirmation statement,
 * and a rationale for the decision.
 * @since 1.0.0
 */
@Component({
  selector: 'app-reject-opportunity-dialog',
  templateUrl: './reject-opportunity-dialog.component.html',
  styleUrl: './reject-opportunity-dialog.component.scss',
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    DialogModule,
    ButtonModule,
    CheckboxModule,
    TextareaModule,
    FloatLabelModule,
    MessageModule,
    ProgressSpinnerModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RejectOpportunityDialogComponent {
  private opportunityService = inject(OpportunityService);
  private feedbackService = inject(FeedbackDialogService);
  private translateService = inject(TranslateService);

  /**
   * @description Two-way binding for dialog visibility
   * @type {ModelSignal<boolean>}
   */
  readonly visible = model<boolean>(false);

  /**
   * @description The opportunity being rejected
   * @type {InputSignal<Opportunity>}
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Event emitted when the No-Go decision is confirmed
   * @type {OutputEmitterRef<NoGoDecisionPayload>}
   */
  readonly decisionConfirmed = output<NoGoDecisionPayload>();

  // Form state signals
  readonly confirmationAcknowledged = signal<boolean>(false);
  readonly decisionRationale = signal<string>('');
  readonly isSubmitting = signal<boolean>(false);

  /**
   * @description Static confirmation statement for No-Go decision
   * @returns {string} The confirmation statement text
   */
  readonly confirmationStatement = computed(() => {
    return this.translateService.instant('workflow.goDecision.dialog.reject.confirmationStatement');
  });

  /**
   * @description Computed flag indicating if the form can be submitted
   * @returns {boolean} True if all required fields are valid
   */
  readonly canSubmit = computed(() => {
    return this.confirmationAcknowledged() && this.decisionRationale().trim().length > 0;
  });

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
   * @description Submit the No-Go decision
   */
  onSubmit(): void {
    if (!this.canSubmit()) return;

    const payload: NoGoDecisionPayload = {
      rationale: this.decisionRationale().trim(),
      confirmationAcknowledged: true,
    };

    this.isSubmitting.set(true);

    this.opportunityService.rejectOpportunity(this.opportunity().id, payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('workflow.goDecision.message.rejectSuccess'),
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
  }
}
