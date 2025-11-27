/**
 * @fileoverview Opportunity WHEN Section Component - Manages timeline dates with edit capabilities
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  input,
  output,
  signal,
  computed,
  inject,
  OnInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { FloatLabelModule } from 'primeng/floatlabel';
import { TooltipModule } from 'primeng/tooltip';
import { ChipModule } from 'primeng/chip';
import { TimelineModule } from 'primeng/timeline';
import { Opportunity, OpportunityDeliverable } from '@shared/models/opportunity.model';
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
    FormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    DatePickerModule,
    FloatLabelModule,
    TooltipModule,
    ChipModule,
    TimelineModule
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
  
  /**
   * @description Input signal for update permission - controls visibility of edit button
   */
  readonly canUpdate = input<boolean>(false);

  // Outputs
  readonly opportunityUpdated = output<Opportunity>();

  // State
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);

  // Form controls
  targetSigningDateControl = new FormControl<Date | null>(null);
  targetDeliveryDateControl = new FormControl<Date | null>(null);
  
  // Local state for deliverable dates (to avoid signal reactivity issues)
  // Maps deliverableId -> { start: Date | null, end: Date | null }
  deliverableDates = signal<Map<number, { start: Date | null; end: Date | null }>>(new Map());
  
  // Computed deliverables for timeline
  readonly sortedDeliverables = computed(() => {
    const deliverables = this.opportunity()?.deliverables || [];
    return [...deliverables].sort((a, b) => {
      // Sort by sequence order first, then by planned start date, then by ID
      const aSeq = a.sequenceOrder ?? Number.MAX_SAFE_INTEGER;
      const bSeq = b.sequenceOrder ?? Number.MAX_SAFE_INTEGER;
      
      if (aSeq !== bSeq) {
        return aSeq - bSeq;
      }
      
      if (a.plannedStartDate && b.plannedStartDate) {
        return new Date(a.plannedStartDate).getTime() - new Date(b.plannedStartDate).getTime();
      }
      if (a.plannedStartDate) return -1;
      if (b.plannedStartDate) return 1;
      
      return (a.id || 0) - (b.id || 0);
    });
  });
  
  // Check if any deliverable requires procurement (for timeline indicators)
  readonly hasDeliverablesWithProcurement = computed(() => {
    const deliverables = this.opportunity()?.deliverables || [];
    return deliverables.some(d => 
      d.procurementComponent === true && 
      d.serviceLine?.toLowerCase() !== 'procurement'
    );
  });

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
    
    // Initialize local date state for deliverables
    const dateMap = new Map<number, { start: Date | null; end: Date | null }>();
    (opp.deliverables || []).forEach(d => {
      dateMap.set(d.id, {
        start: d.plannedStartDate ? new Date(d.plannedStartDate) : null,
        end: d.plannedEndDate ? new Date(d.plannedEndDate) : null
      });
    });
    this.deliverableDates.set(dateMap);

    this.isEditing.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Save section changes
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    // Build updated deliverables with dates from local state
    const dateMap = this.deliverableDates();
    const updatedDeliverables = (opp.deliverables || []).map(d => {
      const dates = dateMap.get(d.id);
      return {
        ...d,
        plannedStartDate: dates?.start?.toISOString() ?? d.plannedStartDate,
        plannedEndDate: dates?.end?.toISOString() ?? d.plannedEndDate
      };
    });

    const whenData = {
      targetSigningDate: this.targetSigningDateControl.value,
      targetDeliveryDate: this.targetDeliveryDateControl.value,
      deliverables: updatedDeliverables
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityWhen(opp.id, whenData).subscribe({
      next: (fullUpdatedOpportunity) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        
        // Clear local date state
        this.deliverableDates.set(new Map());
        
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
    
    // Clear local date state
    this.deliverableDates.set(new Map());
    
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
  
  /**
   * Get the start date for a deliverable from local state
   * @description Returns the Date object for display in datepicker
   */
  getDeliverableStartDate(deliverableId: number): Date | null {
    const dateMap = this.deliverableDates();
    const dates = dateMap.get(deliverableId);
    return dates?.start ?? null;
  }
  
  /**
   * Get the end date for a deliverable from local state
   * @description Returns the Date object for display in datepicker
   */
  getDeliverableEndDate(deliverableId: number): Date | null {
    const dateMap = this.deliverableDates();
    const dates = dateMap.get(deliverableId);
    return dates?.end ?? null;
  }
  
  /**
   * Update deliverable start date
   * @description Updates planned start date for a deliverable (local state only until save)
   */
  updateDeliverableStartDate(deliverable: OpportunityDeliverable, newDate: Date | null): void {
    if (!this.isEditing()) return;
    
    const currentMap = this.deliverableDates();
    const newMap = new Map(currentMap);
    const existing = newMap.get(deliverable.id) || { start: null, end: null };
    newMap.set(deliverable.id, { ...existing, start: newDate });
    this.deliverableDates.set(newMap);
    this.cdr.detectChanges();
  }
  
  /**
   * Update deliverable end date
   * @description Updates planned end date for a deliverable (local state only until save)
   */
  updateDeliverableEndDate(deliverable: OpportunityDeliverable, newDate: Date | null): void {
    if (!this.isEditing()) return;
    
    const currentMap = this.deliverableDates();
    const newMap = new Map(currentMap);
    const existing = newMap.get(deliverable.id) || { start: null, end: null };
    newMap.set(deliverable.id, { ...existing, end: newDate });
    this.deliverableDates.set(newMap);
    this.cdr.detectChanges();
  }
  
  /**
   * Check if deliverable requires procurement prerequisite
   * @description Returns true if deliverable has procurement component and is not from Procurement service line
   */
  requiresProcurementPrerequisite(deliverable: OpportunityDeliverable): boolean {
    return deliverable.procurementComponent === true && 
           deliverable.serviceLine?.toLowerCase() !== 'procurement';
  }
  
  /**
   * Navigate to WHAT section
   * @description Smooth scroll to WHAT section to add products/services
   */
  navigateToWhatSection(): void {
    const whatSection = document.querySelector('app-opportunity-what-section');
    if (whatSection) {
      whatSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}

