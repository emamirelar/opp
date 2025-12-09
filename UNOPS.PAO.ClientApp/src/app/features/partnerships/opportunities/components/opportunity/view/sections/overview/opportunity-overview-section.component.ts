/**
 * @fileoverview Opportunity Overview Section Component - Manages opportunity name and description with edit capabilities
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, output, signal, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { TooltipModule } from 'primeng/tooltip';

// Services and Models
import { OpportunityService } from '../../../../../services/opportunity.service';
import { Opportunity } from '@shared/models/opportunity.model';
import { FeedbackDialogService } from '@shared/services/ui';

/**
 * @class OpportunityOverviewSectionComponent
 * @description Manages the Overview section of opportunity with independent edit/save/cancel functionality.
 * Contains the opportunity name and description fields. Updates are handled via local state management
 * without requiring full component refresh.
 * 
 * @example
 * ```html
 * <app-opportunity-overview-section
 *   [opportunity]="opportunity()"
 *   [canUpdate]="canUpdate()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 *   (changesDetected)="handleChangesDetected()"
 *   (changesSavedOrDiscarded)="handleChangesSaved()"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-overview-section',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    InputText,
    InputTextarea,
    TooltipModule,
  ],
  templateUrl: './opportunity-overview-section.component.html',
  styleUrls: ['./opportunity-overview-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityOverviewSectionComponent {
  // Services
  private readonly opportunityService = inject(OpportunityService);
  private readonly translateService = inject(TranslateService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly cdr = inject(ChangeDetectorRef);

  /**
   * @description Input signal for opportunity data from parent
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Input signal for update permission - controls visibility of edit button
   */
  readonly canUpdate = input<boolean>(false);

  /**
   * @description Output event when opportunity is updated - signals parent to refresh
   */
  readonly opportunityUpdated = output<Opportunity>();

  /**
   * @description Output event when changes are detected (for unsaved changes tracking)
   */
  readonly changesDetected = output<void>();

  /**
   * @description Output event when changes are saved or discarded (clear unsaved state)
   */
  readonly changesSavedOrDiscarded = output<void>();

  // Edit mode state
  readonly isEditing = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);
  private originalData: {
    name?: string;
    description?: string;
  } | null = null;
  private hasUnsavedChanges = false;

  // Form controls for Overview section
  nameControl = new FormControl<string | null>(null);
  descriptionControl = new FormControl<string | null>(null);

  constructor() {
    // Set up change detection on form controls
    // Only mark as changed if we're in edit mode (to avoid triggering on initial setValue)
    this.nameControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.descriptionControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
  }

  /**
   * @description Enter edit mode for this section
   */
  startEditing(): void {
    const opp = this.opportunity();
    
    // Backup original data for cancel
    this.originalData = {
      name: opp.name ?? '',
      description: opp.description ?? ''
    };

    // Set form controls
    this.nameControl.setValue(opp.name ?? null);
    this.descriptionControl.setValue(opp.description ?? null);

    this.isEditing.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Mark section as having unsaved changes
   * @private
   */
  private markAsChanged(): void {
    if (!this.hasUnsavedChanges) {
      this.hasUnsavedChanges = true;
      this.changesDetected.emit();
    }
  }

  /**
   * @description Save section changes
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    // Validate opportunity name
    const name = this.nameControl.value?.trim();
    if (!name || name === '') {
      this.feedbackService.showErrorToast({
        detail: this.translateService.instant('message.validation.opportunityNameRequired'),
        summary: this.translateService.instant('message.validationError')
      });
      return;
    }

    if (name.length > 255) {
      this.feedbackService.showErrorToast({
        detail: this.translateService.instant('message.validation.opportunityNameTooLong'),
        summary: this.translateService.instant('message.validationError')
      });
      return;
    }

    const overviewData = {
      name: name,
      description: this.descriptionControl.value ?? undefined
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityOverview(opp.id, overviewData).subscribe({
      next: (fullUpdatedOpportunity) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.originalData = null;
        this.hasUnsavedChanges = false;
        
        // Emit full updated opportunity to parent
        this.opportunityUpdated.emit(fullUpdatedOpportunity);
        
        // Clear unsaved changes tracking
        this.changesSavedOrDiscarded.emit();
        
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
    // Revert form controls to original values
    const opp = this.opportunity();
    this.nameControl.setValue(opp.name ?? null);
    this.descriptionControl.setValue(opp.description ?? null);
    
    this.isEditing.set(false);
    this.originalData = null;
    this.hasUnsavedChanges = false;
    
    // Clear unsaved changes tracking
    this.changesSavedOrDiscarded.emit();
    
    this.cdr.detectChanges();
  }
}

