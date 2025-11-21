/**
 * @fileoverview Opportunity WHEN Section Component - Manages timeline dates with edit capabilities
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  input,
  output,
  signal,
  inject,
  OnInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { TooltipModule } from 'primeng/tooltip';
import { Opportunity } from '@shared/models/opportunity.model';
import { OpportunityService } from '@features/partnerships/opportunities/services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui';

/**
 * @class OpportunityWhenSectionComponent
 * @description Manages the WHEN section of opportunity with independent edit/save/cancel functionality
 * 
 * @example
 * ```html
 * <app-opportunity-when-section
 *   [opportunity]="opportunity()!"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-when-section',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    DatePickerModule,
    FloatLabelModule,
    TooltipModule
  ],
  templateUrl: './opportunity-when-section.component.html',
  styleUrls: ['./opportunity-when-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityWhenSectionComponent implements OnInit {
  private readonly opportunityService = inject(OpportunityService);
  private readonly translateService = inject(TranslateService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly cdr = inject(ChangeDetectorRef);

  // Inputs
  readonly opportunity = input.required<Opportunity>();
  readonly suggestions = input<any[]>([]);

  // Outputs
  readonly opportunityUpdated = output<Opportunity>();

  // State
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);

  // Form controls
  targetSigningDateControl = new FormControl<Date | null>(null);
  targetDeliveryDateControl = new FormControl<Date | null>(null);

  ngOnInit(): void {
    // Initialize form controls with current values
    const opp = this.opportunity();
    if (opp.targetSigningDate) {
      this.targetSigningDateControl.setValue(new Date(opp.targetSigningDate));
    }
    if (opp.targetDeliveryDate) {
      this.targetDeliveryDateControl.setValue(new Date(opp.targetDeliveryDate));
    }
  }

  /**
   * @description Enter edit mode
   */
  startEditing(): void {
    const opp = this.opportunity();
    
    // Set form controls
    this.targetSigningDateControl.setValue(
      opp.targetSigningDate ? new Date(opp.targetSigningDate) : null
    );
    this.targetDeliveryDateControl.setValue(
      opp.targetDeliveryDate ? new Date(opp.targetDeliveryDate) : null
    );

    this.isEditing.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Save section changes
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    const whenData = {
      targetSigningDate: this.targetSigningDateControl.value,
      targetDeliveryDate: this.targetDeliveryDateControl.value
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityWhen(opp.id, whenData).subscribe({
      next: (fullUpdatedOpportunity) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        
        // Emit full updated opportunity to parent
        this.opportunityUpdated.emit(fullUpdatedOpportunity);
        
        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('message.opportunity.updatedSuccessfully'),
          summary: this.translateService.instant('message.success')
        });
        this.cdr.detectChanges();
      },
      error: () => {
        this.isSaving.set(false);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Cancel editing and revert changes
   */
  cancelEditing(): void {
    this.isEditing.set(false);
    
    // Reset form controls to original values
    const opp = this.opportunity();
    this.targetSigningDateControl.setValue(
      opp.targetSigningDate ? new Date(opp.targetSigningDate) : null
    );
    this.targetDeliveryDateControl.setValue(
      opp.targetDeliveryDate ? new Date(opp.targetDeliveryDate) : null
    );
    
    this.cdr.detectChanges();
  }

  /**
   * @description Format date for display
   */
  formatDate(dateString?: string | null): string {
    if (!dateString) return this.translateService.instant('message.notSet');
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}

