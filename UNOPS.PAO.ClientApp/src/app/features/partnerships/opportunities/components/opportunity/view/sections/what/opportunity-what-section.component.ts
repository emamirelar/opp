/**
 * @fileoverview Opportunity WHAT Section Component - Manages opportunity overview with edit capabilities
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, output, signal, computed, inject, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputText } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { InputNumber } from 'primeng/inputnumber';
import { ChipModule } from 'primeng/chip';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';

// Services and Models
import { ValuesService, SimpleValue, OrganizationUnit, Output } from '@shared/services/api/values.service';
import { OpportunityService } from '../../../../../services/opportunity.service';
import { Opportunity, OpportunityDeliverable, FrameworkStatusResponse, ExtractedDeliverableInfo } from '@shared/models/opportunity.model';
import { FeedbackDialogService } from '@shared/services/ui';

/**
 * @class OpportunityWhatSectionComponent
 * @description Manages the WHAT section of opportunity with independent edit/save/cancel functionality.
 * Communicates with parent via input signals and output events.
 * 
 * @example
 * ```html
 * <app-opportunity-what-section
 *   [opportunity]="opportunity()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 *   (sectionSaved)="handleSectionSaved()"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-what-section',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    SelectModule,
    InputText,
    InputTextarea,
    InputNumber,
    ChipModule,
    TooltipModule,
    DialogModule,
  ],
  templateUrl: './opportunity-what-section.component.html',
  styleUrls: ['./opportunity-what-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityWhatSectionComponent implements OnInit {
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
  readonly suggestions = input<any[]>([]);

  /**
   * @description Output event when opportunity is updated - signals parent to refresh
   */
  readonly opportunityUpdated = output<Opportunity>();

  /**
   * @description Output event when section is saved - for cross-section refresh triggers
   */
  readonly sectionSaved = output<void>();

  // Edit mode state
  readonly isEditing = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);
  private originalData: {
    description?: string;
    responsibleOrgUnitId?: number;
    proposedInitiativeTypeId?: number;
    deliverables?: any[];
  } | null = null;

  // Form controls for WHAT section
  nameControl = new FormControl<string | null>(null);
  descriptionControl = new FormControl<string | null>(null);
  orgUnitControl = new FormControl<number | null>(null);
  initiativeTypeControl = new FormControl<number | null>(null);

  // Dropdown data
  organizationUnits = signal<OrganizationUnit[]>([]);
  initiativeTypes = signal<SimpleValue[]>([]);
  outputs = signal<Output[]>([]);

  // AC2 (WHAT Section) - Framework status and extraction
  frameworkStatus = signal<FrameworkStatusResponse | null>(null);
  isCheckingFramework = signal<boolean>(false);
  isExtracting = signal<boolean>(false);
  extractedDeliverables = signal<ExtractedDeliverableInfo[]>([]);
  showFrameworkWarning = computed(() => {
    const status = this.frameworkStatus();
    return status && !status.hasTaggedFrameworks;
  });
  showFrameworkInfo = computed(() => {
    const status = this.frameworkStatus();
    return status && status.hasTaggedFrameworks;
  });

  // Deliverables dialog
  showDeliverablesDialog = signal<boolean>(false);
  selectedOutput = signal<Output | null>(null);
  isEditingDeliverable = signal<boolean>(false);
  editingDeliverableIndex = signal<number | null>(null);

  // Cascading dropdown data
  outputGroups = signal<string[]>([]);
  outputSubGroups = signal<string[]>([]);
  filteredOutputs = signal<Output[]>([]);

  // Cascading dropdown form controls
  outputGroupControl = new FormControl<string | null>(null);
  outputSubGroupControl = new FormControl<string | null>(null);
  outputControl = new FormControl<Output | null>(null);
  quantityControl = new FormControl<number | null>(null);

  // Computed properties
  readonly deliverableCount = computed(() => this.opportunity().deliverables?.length || 0);
  readonly selectedOutputDetails = computed(() => {
    const output = this.outputControl.value;
    if (!output) return null;
    return {
      description: output.description,
      serviceLine: output.outputServiceLine,
      unitName: output.unitName,
      projectCategoryName: output.projectCategoryName
    };
  });

  ngOnInit(): void {
    // Load dropdown data on initialization
    this.loadDropdownData();
  }

  /**
   * @description Load dropdown data for form fields
   */
  private loadDropdownData(): void {
    this.valuesService.getOrganizationUnits().subscribe({
      next: (data) => {
        this.organizationUnits.set(data);
        this.cdr.detectChanges();
      }
    });

    this.valuesService.getProposedInitiativeTypes().subscribe({
      next: (data) => {
        this.initiativeTypes.set(data);
        this.cdr.detectChanges();
      }
    });

    this.valuesService.getOutputs().subscribe({
      next: (data) => {
        this.outputs.set(data);
        
        // Initialize ALL dropdowns with ALL data by default
        const groups = this.valuesService.getDistinctOutputGroups(data);
        this.outputGroups.set(groups);
        
        const allSubGroups = this.valuesService.getDistinctOutputSubGroups(data, ''); // Get all sub-groups
        this.outputSubGroups.set(allSubGroups);
        
        this.filteredOutputs.set(data); // Show all outputs by default
        
        this.cdr.detectChanges();
      }
    });

    // AC2: Check framework status on load
    this.checkFrameworkStatus();
  }

  /**
   * @description Handle output group selection - filters sub-groups and outputs
   */
  onOutputGroupChange(group: string | null): void {
    if (!group) {
      // If cleared, reset to show all
      const allOutputs = this.outputs();
      const allSubGroups = this.valuesService.getDistinctOutputSubGroups(allOutputs, '');
      this.outputSubGroups.set(allSubGroups);
      this.filteredOutputs.set(allOutputs);
      this.cdr.detectChanges();
      return;
    }

    const allOutputs = this.outputs();
    
    // Filter sub-groups for selected group
    const subGroups = this.valuesService.getDistinctOutputSubGroups(allOutputs, group);
    this.outputSubGroups.set(subGroups);
    
    // Check if current sub-group is still valid for this group
    const currentSubGroup = this.outputSubGroupControl.value;
    if (currentSubGroup && !subGroups.includes(currentSubGroup)) {
      this.outputSubGroupControl.setValue(null);
    }
    
    // Filter outputs by group (and sub-group if selected)
    const filtered = this.valuesService.getFilteredOutputs(
      allOutputs,
      group,
      this.outputSubGroupControl.value || undefined
    );
    this.filteredOutputs.set(filtered);
    
    // Check if current output is still valid
    const currentOutput = this.outputControl.value;
    if (currentOutput && !filtered.some(o => o.id === currentOutput.id)) {
      this.outputControl.setValue(null);
    }
    
    this.cdr.detectChanges();
  }

  /**
   * @description Handle output sub-group selection - filters outputs and may set group
   */
  onOutputSubGroupChange(subGroup: string | null): void {
    const allOutputs = this.outputs();
    const selectedGroup = this.outputGroupControl.value;
    
    if (!subGroup) {
      // If cleared, show outputs based on group selection only
      if (selectedGroup) {
        const filtered = this.valuesService.getFilteredOutputs(allOutputs, selectedGroup);
        this.filteredOutputs.set(filtered);
      } else {
        this.filteredOutputs.set(allOutputs);
      }
      this.cdr.detectChanges();
      return;
    }
    
    // If no group selected, auto-select the group for this sub-group
    if (!selectedGroup) {
      const outputWithSubGroup = allOutputs.find(o => o.outputSubGroup === subGroup);
      if (outputWithSubGroup && outputWithSubGroup.outputGroup) {
        this.outputGroupControl.setValue(outputWithSubGroup.outputGroup, { emitEvent: false });
        
        // Update sub-groups list for the auto-selected group
        const subGroups = this.valuesService.getDistinctOutputSubGroups(allOutputs, outputWithSubGroup.outputGroup);
        this.outputSubGroups.set(subGroups);
      }
    }
    
    // Filter outputs by group (if any) and sub-group
    const filtered = this.valuesService.getFilteredOutputs(
      allOutputs,
      this.outputGroupControl.value || undefined,
      subGroup
    );
    this.filteredOutputs.set(filtered);
    
    // Check if current output is still valid
    const currentOutput = this.outputControl.value;
    if (currentOutput && !filtered.some(o => o.id === currentOutput.id)) {
      this.outputControl.setValue(null);
    }
    
    this.cdr.detectChanges();
  }

  /**
   * @description Handle output selection - auto-fills group and sub-group
   */
  onOutputChange(output: Output | null): void {
    if (!output) {
      this.outputControl.setValue(null);
      this.cdr.detectChanges();
      return;
    }
    
    // Explicitly set the control value to ensure it's updated
    this.outputControl.setValue(output, { emitEvent: false });
    
    // Auto-fill group if not already set or different
    if (output.outputGroup && this.outputGroupControl.value !== output.outputGroup) {
      this.outputGroupControl.setValue(output.outputGroup, { emitEvent: false });
      
      // Update sub-groups for this group
      const allOutputs = this.outputs();
      const subGroups = this.valuesService.getDistinctOutputSubGroups(allOutputs, output.outputGroup);
      this.outputSubGroups.set(subGroups);
    }
    
    // Auto-fill sub-group if not already set or different
    if (output.outputSubGroup && this.outputSubGroupControl.value !== output.outputSubGroup) {
      this.outputSubGroupControl.setValue(output.outputSubGroup, { emitEvent: false });
    }
    
    // Force change detection to update the template
    this.cdr.detectChanges();
  }

  /**
   * AC2 (WHAT Section) - Check if Partner Results Framework documents are tagged
   * @description Checks the status of Partner Results Framework documents for this opportunity
   */
  checkFrameworkStatus(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    this.isCheckingFramework.set(true);
    this.opportunityService.getFrameworkStatus(opp.id).subscribe({
      next: (status) => {
        this.frameworkStatus.set(status);
        this.isCheckingFramework.set(false);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error checking framework status:', error);
        this.isCheckingFramework.set(false);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * AC2 (WHAT Section) - Extract products and services from documents
   * @description Triggers AI extraction of deliverables from documents, prioritizing tagged frameworks
   */
  extractProductsAndServices(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    this.isExtracting.set(true);
    this.feedbackService.showInfoToast({
      summary: this.translateService.instant('message.extracting'),
      detail: this.translateService.instant('message.extracting'),
      life: 3000
    });

    this.opportunityService.extractProductsAndServices(opp.id).subscribe({
      next: (extracted) => {
        this.extractedDeliverables.set(extracted);
        this.isExtracting.set(false);
        
        if (extracted && extracted.length > 0) {
          this.feedbackService.showSuccessToast({
            summary: this.translateService.instant('message.extractionComplete'),
            detail: this.translateService.instant('message.extractionComplete'),
            life: 5000
          });
        } else {
          this.feedbackService.showWarningToast({
            summary: this.translateService.instant('message.noProductsExtracted'),
            detail: this.translateService.instant('message.noProductsExtracted'),
            life: 5000
          });
        }
        
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error extracting products and services:', error);
        this.isExtracting.set(false);
        
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error.extractionFailed'),
          detail: error?.error?.detail || error?.message || this.translateService.instant('message.error.extractionFailed'),
          life: 5000
        });
        
        this.cdr.detectChanges();
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
      description: opp.description ?? '',
      responsibleOrgUnitId: opp.responsibleOrgUnitId ?? undefined,
      proposedInitiativeTypeId: opp.proposedInitiativeTypeId ?? undefined,
      deliverables: opp.deliverables ? [...opp.deliverables] : []
    };

    // Set form controls
    this.nameControl.setValue(opp.name ?? null);
    this.descriptionControl.setValue(opp.description ?? null);
    this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
    this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);

    this.isEditing.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Save section changes
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    const whatData = {
      name: this.nameControl.value ?? undefined,
      description: this.descriptionControl.value ?? undefined,
      responsibleOrgUnitId: this.orgUnitControl.value ?? undefined,
      proposedInitiativeTypeId: this.initiativeTypeControl.value ?? undefined,
      deliverables: opp.deliverables // Include modified deliverables
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityWhat(opp.id, whatData).subscribe({
      next: (fullUpdatedOpportunity) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.originalData = null;
        
        // Emit full updated opportunity to parent
        this.opportunityUpdated.emit(fullUpdatedOpportunity);
        
        // Emit that section was saved (for potential cross-section updates)
        this.sectionSaved.emit();
        
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
    // Revert is handled by parent refresh, just exit edit mode
    this.isEditing.set(false);
    this.originalData = null;
    
    // Reset form controls to original values
    const opp = this.opportunity();
    this.nameControl.setValue(opp.name ?? null);
    this.descriptionControl.setValue(opp.description ?? null);
    this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
    this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);
    
    this.cdr.detectChanges();
  }

  /**
   * @description Open deliverables dialog for adding new deliverable
   */
  openDeliverablesDialog(): void {
    // Reset edit mode
    this.isEditingDeliverable.set(false);
    this.editingDeliverableIndex.set(null);
    
    // Reset form controls
    this.outputGroupControl.setValue(null);
    this.outputSubGroupControl.setValue(null);
    this.outputControl.setValue(null);
    this.quantityControl.setValue(null);
    
    // Initialize all dropdowns with all data
    const allOutputs = this.outputs();
    const allGroups = this.valuesService.getDistinctOutputGroups(allOutputs);
    this.outputGroups.set(allGroups);
    
    const allSubGroups = this.valuesService.getDistinctOutputSubGroups(allOutputs, '');
    this.outputSubGroups.set(allSubGroups);
    
    this.filteredOutputs.set(allOutputs);
    
    this.showDeliverablesDialog.set(true);
  }

  /**
   * @description Add or update deliverable
   */
  addDeliverable(): void {
    const output = this.outputControl.value;
    const opp = this.opportunity();
    if (!output || !opp) return;

    // Check for duplicate output (both when adding and editing)
    const currentEditingIndex = this.editingDeliverableIndex();
    const isDuplicate = opp.deliverables?.some((d, index) => {
      // Skip the deliverable we're currently editing
      if (this.isEditingDeliverable() && index === currentEditingIndex) {
        return false;
      }
      return d.outputId === output.id;
    });

    if (isDuplicate) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.outputAlreadyAdded')
      });
      return;
    }

    // Create deliverable object
    const deliverableData: OpportunityDeliverable = {
      id: 0,
      opportunityId: opp.id,
      outputId: output.id ?? null,
      outputName: output.outputName ?? null,
      outputDescription: output.description ?? null,
      outputGroup: output.outputGroup ?? null,
      outputSubGroup: output.outputSubGroup ?? null,
      outputServiceLine: output.outputServiceLine ?? null,
      unitCode: output.unitName ?? null,
      projectCategoryCode: output.projectCategoryName ?? null,
      quantity: this.quantityControl.value ?? null,
      notes: null
    };

    let updatedDeliverables: OpportunityDeliverable[];
    
    if (this.isEditingDeliverable() && this.editingDeliverableIndex() !== null) {
      // Update existing deliverable
      const index = this.editingDeliverableIndex()!;
      updatedDeliverables = [...(opp.deliverables || [])];
      // Preserve the original ID if editing
      deliverableData.id = updatedDeliverables[index].id;
      updatedDeliverables[index] = deliverableData;
    } else {
      // Add new deliverable
      const currentDeliverables = opp.deliverables || [];
      updatedDeliverables = [...currentDeliverables, deliverableData];
    }

    // Update opportunity with modified deliverables
    const updatedOpportunity: Opportunity = {
      ...opp,
      deliverables: updatedDeliverables
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Reset dialog state
    this.showDeliverablesDialog.set(false);
    this.isEditingDeliverable.set(false);
    this.editingDeliverableIndex.set(null);
    this.outputGroupControl.setValue(null);
    this.outputSubGroupControl.setValue(null);
    this.outputControl.setValue(null);
    this.quantityControl.setValue(null);
    this.cdr.detectChanges();
  }

  /**
   * @description Edit existing deliverable
   */
  editDeliverable(index: number): void {
    const opp = this.opportunity();
    if (!opp || !opp.deliverables || !opp.deliverables[index]) return;

    const deliverable = opp.deliverables[index];
    const allOutputs = this.outputs();

    // Set edit mode
    this.isEditingDeliverable.set(true);
    this.editingDeliverableIndex.set(index);

    // Find the output to pre-fill the form
    const matchedOutput = allOutputs.find(o => o.id === deliverable.outputId);
    
    if (matchedOutput) {
      // Pre-fill cascading dropdowns
      if (matchedOutput.outputGroup) {
        this.outputGroupControl.setValue(matchedOutput.outputGroup);
        const subGroups = this.valuesService.getDistinctOutputSubGroups(allOutputs, matchedOutput.outputGroup);
        this.outputSubGroups.set(subGroups);
      }

      if (matchedOutput.outputSubGroup) {
        this.outputSubGroupControl.setValue(matchedOutput.outputSubGroup);
      }

      if (matchedOutput.outputGroup) {
        const filtered = this.valuesService.getFilteredOutputs(
          allOutputs,
          matchedOutput.outputGroup,
          matchedOutput.outputSubGroup
        );
        this.filteredOutputs.set(filtered);
      }

      this.outputControl.setValue(matchedOutput);
    }

    this.showDeliverablesDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Remove deliverable at index
   */
  removeDeliverable(index: number): void {
    const opp = this.opportunity();
    if (!opp || !opp.deliverables) return;

    const updatedDeliverables = opp.deliverables.filter((_, i) => i !== index);
    const updatedOpportunity = {
      ...opp,
      deliverables: updatedDeliverables
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);
    this.cdr.detectChanges();
  }
}

