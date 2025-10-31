/**
 * @fileoverview Opportunity WHY Section Component - Manages impact and alignment with edit capabilities
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, output, signal, computed, inject, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextarea } from 'primeng/inputtextarea';
import { ChipModule } from 'primeng/chip';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';

// Services and Models
import { ValuesService, SDG } from '@shared/services/api/values.service';
import { OpportunityService } from '../../../../services/opportunity.service';
import { Opportunity, OpportunitySDG } from '@shared/models/opportunity.model';
import { FeedbackDialogService } from '@shared/services/ui';

/**
 * @class OpportunityWhySectionComponent
 * @description Manages the WHY section of opportunity with independent edit/save/cancel functionality.
 * Handles strategic alignment, expected beneficiaries, expected outcomes, and SDG alignments.
 * 
 * @example
 * ```html
 * <app-opportunity-why-section
 *   [opportunity]="opportunity()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-why-section',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    InputTextarea,
    ChipModule,
    TooltipModule,
    DialogModule,
    SelectModule,
    MessageModule,
  ],
  templateUrl: './opportunity-why-section.component.html',
  styleUrls: ['./opportunity-why-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityWhySectionComponent implements OnInit {
  // Services
  private readonly valuesService = inject(ValuesService);
  private readonly opportunityService = inject(OpportunityService);
  private readonly translateService = inject(TranslateService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly cdr = inject(ChangeDetectorRef);

  /**
   * @description Input signal for opportunity data from parent
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Output event when opportunity is updated - signals parent to refresh
   */
  readonly opportunityUpdated = output<Opportunity>();

  // Edit mode state
  readonly isEditing = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);

  // Form controls for WHY section
  strategicAlignmentControl = new FormControl<string | null>(null);
  expectedBeneficiariesControl = new FormControl<string | null>(null);
  expectedOutcomesControl = new FormControl<string | null>(null);

  // SDG data
  sdgs = signal<SDG[]>([]);
  
  // SDG dialog
  showSDGDialog = signal<boolean>(false);
  sdgControl = new FormControl<SDG | null>(null);
  isPrimaryControl = new FormControl<boolean>(false);
  isEditingSDG = signal<boolean>(false);
  editingSDGIndex = signal<number | null>(null);
  showValidationError = signal<boolean>(false);

  // Computed properties
  readonly sdgCount = computed(() => this.opportunity().sdGs?.length || 0);
  readonly primarySDG = computed(() => 
    this.opportunity().sdGs?.find(sdg => sdg.isPrimary) || null
  );

  ngOnInit(): void {
    // Load SDGs on initialization
    this.loadSDGs();
  }

  /**
   * @description Load SDG data
   */
  private loadSDGs(): void {
    this.valuesService.getSDGs().subscribe({
      next: (data) => {
        this.sdgs.set(data);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Start editing the section
   */
  startEditing(): void {
    const opp = this.opportunity();
    
    // Set form controls
    this.strategicAlignmentControl.setValue(opp.strategicAlignment ?? null);
    this.expectedBeneficiariesControl.setValue(opp.expectedBeneficiaries ?? null);
    this.expectedOutcomesControl.setValue(opp.intendedImpactOutcomes ?? null);

    this.isEditing.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Save section changes
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    const whyData = {
      strategicAlignment: this.strategicAlignmentControl.value ?? undefined,
      expectedBeneficiaries: this.expectedBeneficiariesControl.value ?? undefined,
      intendedImpactOutcomes: this.expectedOutcomesControl.value ?? undefined,
      sdGs: opp.sdGs?.map(sdg => ({
        sdgId: sdg.sdgDatabaseId || 0,  // Use the integer database ID
        isPrimary: sdg.isPrimary,
        notes: sdg.notes
      }))
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityWhy(opp.id, whyData).subscribe({
      next: (fullUpdatedOpportunity: Opportunity) => {
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
    this.strategicAlignmentControl.setValue(opp.strategicAlignment ?? null);
    this.expectedBeneficiariesControl.setValue(opp.expectedBeneficiaries ?? null);
    this.expectedOutcomesControl.setValue(opp.intendedImpactOutcomes ?? null);
    
    this.cdr.detectChanges();
  }

  /**
   * @description Open SDG dialog for adding new SDG
   */
  openSDGDialog(): void {
    this.isEditingSDG.set(false);
    this.editingSDGIndex.set(null);
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.showValidationError.set(false);
    this.showSDGDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Handle SDG selection change
   */
  onSDGChange(sdg: SDG | null): void {
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel SDG dialog and reset state
   */
  cancelSDGDialog(): void {
    this.showSDGDialog.set(false);
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.showValidationError.set(false);
    this.isEditingSDG.set(false);
    this.editingSDGIndex.set(null);
    this.cdr.detectChanges();
  }

  /**
   * @description Get SDG logo URL by SDG ID
   */
  getSDGLogo(sdgId: string): string | null {
    const sdg = this.sdgs().find(s => s.sdgId === sdgId);
    return sdg?.sdgLogo || null;
  }

  /**
   * @description Add SDG to the list
   */
  addSDG(): void {
    const sdg = this.sdgControl.value;
    const isPrimary = this.isPrimaryControl.value || false;

    // Validation
    if (!sdg) {
      this.showValidationError.set(true);
      return;
    }

    const opp = this.opportunity();
    const currentSDGs = [...(opp.sdGs || [])];

    // Check if already exists
    if (currentSDGs.some(s => s.sdgId === sdg.sdgId)) {
      this.feedbackService.showErrorToast({
        detail: this.translateService.instant('message.opportunity.sdgAlreadyAdded'),
        summary: this.translateService.instant('message.error')
      });
      return;
    }

    // If setting as primary, remove primary from others
    if (isPrimary) {
      currentSDGs.forEach(s => s.isPrimary = false);
    }

    // Add new SDG
    const newSDG: OpportunitySDG = {
      id: 0,
      opportunityId: opp.id!,
      sdgId: sdg.sdgId || '',
      sdgDatabaseId: sdg.id,  // Store the integer database ID for saving
      sdgNumber: sdg.sdgNumber || '',
      sdgName: sdg.name,
      isPrimary: isPrimary,
      notes: null
    };

    currentSDGs.push(newSDG);

    // Update opportunity
    const updatedOpportunity = {
      ...opp,
      sdGs: currentSDGs
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Reset dialog state
    this.showSDGDialog.set(false);
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.showValidationError.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Edit existing SDG
   */
  editSDG(index: number): void {
    const opp = this.opportunity();
    const sdg = opp.sdGs?.[index];
    
    if (!sdg) return;

    // Find the SDG in the master list by matching sdgId string
    const masterSDG = this.sdgs().find(s => s.sdgId === sdg.sdgId);
    
    this.isEditingSDG.set(true);
    this.editingSDGIndex.set(index);
    this.sdgControl.setValue(masterSDG || null);
    this.isPrimaryControl.setValue(sdg.isPrimary);
    this.showValidationError.set(false);
    this.showSDGDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Update existing SDG
   */
  updateSDG(): void {
    const sdg = this.sdgControl.value;
    const isPrimary = this.isPrimaryControl.value || false;
    const index = this.editingSDGIndex();

    if (!sdg || index === null) return;

    const opp = this.opportunity();
    const currentSDGs = [...(opp.sdGs || [])];

    // If setting as primary, remove primary from others
    if (isPrimary) {
      currentSDGs.forEach((s, i) => {
        if (i !== index) s.isPrimary = false;
      });
    }

    // Update SDG
    currentSDGs[index] = {
      ...currentSDGs[index],
      sdgId: sdg.sdgId || '',
      sdgDatabaseId: sdg.id,  // Store the integer database ID for saving
      sdgNumber: sdg.sdgNumber || '',
      sdgName: sdg.name,
      isPrimary: isPrimary
    };

    // Update opportunity
    const updatedOpportunity = {
      ...opp,
      sdGs: currentSDGs
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Reset dialog state
    this.showSDGDialog.set(false);
    this.isEditingSDG.set(false);
    this.editingSDGIndex.set(null);
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.showValidationError.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Remove SDG from the list
   */
  removeSDG(index: number): void {
    const opp = this.opportunity();
    const currentSDGs = [...(opp.sdGs || [])];
    
    currentSDGs.splice(index, 1);

    const updatedOpportunity = {
      ...opp,
      sdGs: currentSDGs
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);
    this.cdr.detectChanges();
  }

  /**
   * @description Get chip style based on primary/secondary status
   */
  getSDGChipStyle(isPrimary: boolean): any {
    if (isPrimary) {
      return { 'background-color': '#22c55e', 'color': 'white' };
    }
    return { 'background-color': '#0ea5e9', 'color': 'white' };
  }

  /**
   * @description Get chip label for primary/secondary
   */
  getSDGChipLabel(isPrimary: boolean): string {
    return isPrimary ? 'Primary' : 'Secondary';
  }
}

