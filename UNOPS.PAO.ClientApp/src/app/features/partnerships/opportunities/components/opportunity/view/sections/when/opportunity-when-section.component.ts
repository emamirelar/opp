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
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { CheckboxModule } from 'primeng/checkbox';
import {
  Opportunity,
  OpportunityDeliverable,
  DurationOption
} from '@shared/models/opportunity.model';
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
    TimelineModule,
    SelectModule,
    InputNumberModule,
    MessageModule,
    CheckboxModule
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

  // Expose Math for template use
  readonly Math = Math;

  // Inputs
  readonly opportunity = input.required<Opportunity>();
  readonly suggestions = input<any[]>([]);
  /** True when insights/suggestions are loading or refreshing - show loading indicator */
  readonly loadingInsightsSuggestions = input<boolean>(false);
  
  /**
   * @description Input signal for update permission - controls visibility of edit button
   */
  readonly canUpdate = input<boolean>(false);

  // Outputs
  readonly opportunityUpdated = output<Opportunity>();
  readonly changesDetected = output<void>();
  readonly changesSavedOrDiscarded = output<void>();

  // State
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  readonly isTimelineCollapsed = signal(false);
  readonly hasUnsavedChangesSignal = signal<boolean>(false);

  // Form controls
  targetSigningDateControl = new FormControl<Date | null>(null);
  implementationStartDateControl = new FormControl<Date | null>(null);
  targetDeliveryDateControl = new FormControl<Date | null>(null);
  
  // AC5: Signing date deadline notes controls
  isSigningDateFirmControl = new FormControl<boolean>(false);
  signingDateNotesControl = new FormControl<string | null>(null);
  submissionDeadlineControl = new FormControl<Date | null>(null);
  
  // Signals for reactive validation
  targetSigningDateSignal = signal<Date | null>(null);
  implementationStartDateSignal = signal<Date | null>(null);
  targetDeliveryDateSignal = signal<Date | null>(null);
  submissionDeadlineSignal = signal<Date | null>(null);

  // Track if implementation start date has been explicitly set by user
  readonly isImplementationStartDateExplicitlySet = signal<boolean>(false);

  // Duration calculator state
  readonly selectedDuration = signal<number | null>(null);
  readonly customDurationMonths = signal<number | null>(null);
  readonly showCustomDuration = signal<boolean>(false);

  /**
   * @description Predefined duration options for implementation period
   */
  readonly durationOptions: DurationOption[] = [
    { label: '3 months', value: 3 },
    { label: '6 months', value: 6 },
    { label: '12 months', value: 12 },
    { label: '18 months', value: 18 },
    { label: '24 months', value: 24 },
    { label: '36 months', value: 36 },
    { label: 'Custom', value: -1 }
  ];

  /**
   * @description Get the effective implementation start date (uses signing date if not explicitly set)
   * @returns {string | null} The effective implementation start date
   */
  readonly effectiveImplementationStartDate = computed(() => {
    const opp = this.opportunity();
    // Use implementation start date if set, otherwise fall back to signing date
    return opp?.implementationStartDate || opp?.targetSigningDate || null;
  });

  /**
   * @description Computed implementation duration in months (for view mode display)
   * Uses implementation start date (or signing date as fallback) to delivery date
   * @returns {number | null} Duration in months or null if dates not set
   */
  readonly implementationDurationMonths = computed(() => {
    const opp = this.opportunity();
    const startDate = this.effectiveImplementationStartDate();
    if (!startDate || !opp?.targetDeliveryDate) return null;

    const implStartDate = new Date(startDate);
    const deliveryDate = new Date(opp.targetDeliveryDate);
    return this.calculateMonthsDifference(implStartDate, deliveryDate);
  });

  /**
   * @description Computed implementation duration display string
   * @returns {string} Formatted duration string (e.g., "12 months" or "1 year 6 months")
   */
  readonly implementationDurationDisplay = computed(() => {
    const months = this.implementationDurationMonths();
    if (months === null) return null;
    return this.formatDuration(months);
  });

  // ===== AC3: Timeline Gantt Bar Computed Signals =====

  /**
   * @description Days until target signing date
   * @returns {number | null} Number of days (negative if past)
   */
  readonly daysUntilSigning = computed(() => {
    const opp = this.opportunity();
    if (!opp?.targetSigningDate) return null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const signing = new Date(opp.targetSigningDate);
    signing.setHours(0, 0, 0, 0);
    const diffTime = signing.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  });

  /**
   * @description Days until implementation start date (or signing date as fallback)
   * @returns {number | null} Number of days (negative if past)
   */
  readonly daysUntilImplementationStart = computed(() => {
    const startDate = this.effectiveImplementationStartDate();
    if (!startDate) return null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const implStart = new Date(startDate);
    implStart.setHours(0, 0, 0, 0);
    const diffTime = implStart.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  });

  /**
   * @description Timeline phases for Gantt visualization
   * @returns Timeline phase data including percentages for development and implementation
   */
  readonly timelinePhases = computed(() => {
    const opp = this.opportunity();
    const implStartDate = this.effectiveImplementationStartDate();
    if (!implStartDate || !opp?.targetDeliveryDate) return null;

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const implementationStart = new Date(implStartDate);
    implementationStart.setHours(0, 0, 0, 0);
    const deliveryDate = new Date(opp.targetDeliveryDate);
    deliveryDate.setHours(0, 0, 0, 0);

    // Calculate days
    const developmentDays = Math.max(
      0,
      Math.ceil((implementationStart.getTime() - today.getTime()) / (1000 * 60 * 60 * 24))
    );
    const implementationDays = Math.max(
      0,
      Math.ceil((deliveryDate.getTime() - implementationStart.getTime()) / (1000 * 60 * 60 * 24))
    );
    const totalDays = developmentDays + implementationDays;

    // Calculate percentages
    const developmentPercentage = totalDays > 0 ? (developmentDays / totalDays) * 100 : 0;
    const implementationPercentage = totalDays > 0 ? (implementationDays / totalDays) * 100 : 0;

    // Check if implementation has started (today is past implementation start)
    const isImplementationStarted = today >= implementationStart;
    const isSigningPast = opp.targetSigningDate ? today > new Date(opp.targetSigningDate) : false;

    return {
      developmentDays,
      implementationDays,
      totalDays,
      developmentPercentage,
      implementationPercentage,
      isImplementationStarted,
      isSigningPast
    };
  });

  /**
   * @description Check if timeline data is available for display
   */
  readonly hasTimelineData = computed(() => {
    const opp = this.opportunity();
    return !!(this.effectiveImplementationStartDate() && opp?.targetDeliveryDate);
  });

  // ===== Unified Gantt Chart Calculations =====

  /**
   * @description Calculate the timeline reference for the unified Gantt chart
   * Uses Today → Delivery Date as the reference (same as main phases)
   */
  readonly ganttTimelineReference = computed(() => {
    const opp = this.opportunity();
    if (!opp?.targetDeliveryDate) return null;

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const deliveryDate = new Date(opp.targetDeliveryDate);
    deliveryDate.setHours(0, 0, 0, 0);

    const totalDays = Math.max(1, (deliveryDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

    return {
      startDate: today,
      endDate: deliveryDate,
      totalDays
    };
  });

  /**
   * @description Calculate the implementation start marker position for the unified Gantt
   * @returns Percentage position of implementation start on the timeline
   */
  readonly implementationStartMarkerPosition = computed(() => {
    const timeline = this.ganttTimelineReference();
    const implStartDate = this.effectiveImplementationStartDate();
    if (!timeline || !implStartDate) return 0;

    const implStart = new Date(implStartDate);
    implStart.setHours(0, 0, 0, 0);

    const daysFromStart = (implStart.getTime() - timeline.startDate.getTime()) / (1000 * 60 * 60 * 24);
    return Math.max(0, Math.min((daysFromStart / timeline.totalDays) * 100, 100));
  });

  /**
   * @description Calculate Gantt bar positions for all deliverables relative to unified timeline (Today → Delivery)
   * @returns Map of deliverable ID to bar position data
   */
  readonly deliverableGanttBars = computed(() => {
    const timeline = this.ganttTimelineReference();
    const opp = this.opportunity();
    if (!timeline || !opp) return new Map();

    const barMap = new Map<number, { 
      leftOffset: number; 
      width: number; 
      hasProcurement: boolean; 
      durationDays: number;
      name: string;
      isBeforeToday: boolean;
    }>();

    (opp.deliverables || []).forEach(d => {
      if (!d.plannedStartDate || !d.plannedEndDate) return;

      const startDate = new Date(d.plannedStartDate);
      startDate.setHours(0, 0, 0, 0);
      const endDate = new Date(d.plannedEndDate);
      endDate.setHours(0, 0, 0, 0);

      // Calculate left offset (percentage from today)
      const daysFromStart = (startDate.getTime() - timeline.startDate.getTime()) / (1000 * 60 * 60 * 24);
      const leftOffset = (daysFromStart / timeline.totalDays) * 100;

      // Calculate width (percentage of total timeline)
      const durationDays = Math.max(1, (endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));
      const width = (durationDays / timeline.totalDays) * 100;

      // Check if requires procurement prerequisite
      const hasProcurement = d.procurementComponent === true && d.serviceLine?.toLowerCase() !== 'procurement';

      // Get deliverable name
      const name = d.level4 || d.level3 || d.level2 || d.level1 || d.level0 || 'Unnamed';

      // Check if deliverable starts before today
      const isBeforeToday = startDate < timeline.startDate;

      barMap.set(d.id, {
        leftOffset: Math.max(0, Math.min(leftOffset, 100)),
        width: Math.max(0.5, Math.min(width, 100 - Math.max(0, leftOffset))),
        hasProcurement,
        durationDays,
        name,
        isBeforeToday
      });
    });

    return barMap;
  });

  /**
   * @description Get sorted deliverables that have dates for the Gantt chart
   */
  readonly deliverablesWithDatesForGantt = computed(() => {
    return this.sortedDeliverables().filter(d => d.plannedStartDate && d.plannedEndDate);
  });

  /**
   * @description Check if any deliverable has dates set (for showing unified Gantt)
   */
  readonly hasDeliverablesWithDates = computed(() => {
    const deliverables = this.opportunity()?.deliverables || [];
    return deliverables.some(d => d.plannedStartDate && d.plannedEndDate);
  });

  /**
   * @description Get Gantt bar data for a specific deliverable
   */
  getDeliverableGanttBar(deliverableId: number): { 
    leftOffset: number; 
    width: number; 
    hasProcurement: boolean; 
    durationDays: number;
    name: string;
    isBeforeToday: boolean;
  } | null {
    return this.deliverableGanttBars().get(deliverableId) || null;
  }

  // ===== Vertical Timeline Configuration =====

  /**
   * @description Timeline events for PrimeNG Timeline component
   * Combines key project dates and deliverables into a vertical timeline
   */
  readonly timelineEvents = computed(() => {
    const opp = this.opportunity();
    if (!opp) return [];

    const events: Array<{
      id: string;
      type: 'milestone' | 'deliverable';
      title: string;
      date: Date | null;
      endDate?: Date | null;
      icon: string;
      color: string;
      description?: string;
      deliverable?: OpportunityDeliverable;
      isPast: boolean;
      isToday: boolean;
      timelineIndex?: number;
    }> = [];

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Helper to check if date is past
    const isPast = (date: Date | null) => date ? date < today : false;
    const isToday = (date: Date | null) => {
      if (!date) return false;
      const d = new Date(date);
      d.setHours(0, 0, 0, 0);
      return d.getTime() === today.getTime();
    };

    // Add "Today" marker
    events.push({
      id: 'today',
      type: 'milestone',
      title: this.translateService.instant('label.today'),
      date: today,
      icon: 'pi pi-calendar',
      color: '#0468B1', // UNOPS Blue
      isPast: false,
      isToday: true
    });

    // Add Target Signing Date
    if (opp.targetSigningDate) {
      const signingDate = new Date(opp.targetSigningDate);
      events.push({
        id: 'signing',
        type: 'milestone',
        title: this.translateService.instant('label.opportunity.targetSigningDate'),
        date: signingDate,
        icon: 'pi pi-file-edit',
        color: isPast(signingDate) ? '#9ca3af' : '#f59e0b', // Gray if past, amber otherwise
        isPast: isPast(signingDate),
        isToday: isToday(signingDate)
      });
    }

    // Add Implementation Start Date (if different from signing)
    const effectiveImplStart = this.effectiveImplementationStartDate();
    if (effectiveImplStart && effectiveImplStart !== opp.targetSigningDate) {
      const implDate = new Date(effectiveImplStart);
      events.push({
        id: 'impl-start',
        type: 'milestone',
        title: this.translateService.instant('label.opportunity.implementationStartDate'),
        date: implDate,
        icon: 'pi pi-play',
        color: isPast(implDate) ? '#9ca3af' : '#4CAF50', // Gray if past, green otherwise
        isPast: isPast(implDate),
        isToday: isToday(implDate)
      });
    }

    // Add Deliverables with dates
    (opp.deliverables || []).forEach((d, idx) => {
      if (d.plannedStartDate) {
        const startDate = new Date(d.plannedStartDate);
        const endDate = d.plannedEndDate ? new Date(d.plannedEndDate) : null;
        const name = d.level4 || d.level3 || d.level2 || d.level1 || d.level0 || 'Unnamed';
        const hasProcurement = d.procurementComponent === true && d.serviceLine?.toLowerCase() !== 'procurement';
        
        events.push({
          id: `deliverable-${d.id}`,
          type: 'deliverable',
          title: name,
          date: startDate,
          endDate: endDate,
          icon: hasProcurement ? 'pi pi-shopping-cart' : 'pi pi-box',
          color: hasProcurement ? '#f97316' : '#3b82f6', // Orange for procurement, blue otherwise
          description: d.serviceLine || undefined,
          deliverable: d,
          isPast: isPast(endDate || startDate),
          isToday: isToday(startDate)
        });
      }
    });

    // Add Target Delivery Date
    if (opp.targetDeliveryDate) {
      const deliveryDate = new Date(opp.targetDeliveryDate);
      events.push({
        id: 'delivery',
        type: 'milestone',
        title: this.translateService.instant('label.opportunity.targetDeliveryDate'),
        date: deliveryDate,
        icon: 'pi pi-flag-fill',
        color: isPast(deliveryDate) ? '#9ca3af' : '#4CAF50', // Gray if past, green otherwise
        isPast: isPast(deliveryDate),
        isToday: isToday(deliveryDate)
      });
    }

    // Sort by date
    const sortedEvents = events.sort((a, b) => {
      if (!a.date) return 1;
      if (!b.date) return -1;
      return a.date.getTime() - b.date.getTime();
    });

    // Assign timeline index to each event (for alternate layout positioning)
    sortedEvents.forEach((event, idx) => {
      event.timelineIndex = idx;
    });

    return sortedEvents;
  });

  /**
   * @description Format date for timeline display (shorter format)
   */
  formatTimelineDate(date: Date | null): string {
    if (!date) return '-';
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  /**
   * @description Calculate days from today to a date
   */
  daysFromToday(date: Date | null): number | null {
    if (!date) return null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const targetDate = new Date(date);
    targetDate.setHours(0, 0, 0, 0);
    return Math.ceil((targetDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
  }

  // Note: Signing date validation removed per requirements - date can be past or future

  /**
   * @description Computed validation for submission deadline (must be before or equal to target signing date)
   * @returns {boolean} True if submission deadline is invalid (after signing date)
   */
  readonly isSubmissionDeadlineAfterSigningDate = computed(() => {
    const signingDate = this.targetSigningDateSignal();
    const submissionDeadline = this.submissionDeadlineSignal();
    
    if (!signingDate || !submissionDeadline) return false;
    
    // Normalize both dates to midnight for date-only comparison
    const signing = new Date(signingDate);
    signing.setHours(0, 0, 0, 0);
    
    const submission = new Date(submissionDeadline);
    submission.setHours(0, 0, 0, 0);
    
    // Error if submission deadline is strictly after signing date
    return submission.getTime() > signing.getTime();
  });

  /**
   * @description Computed validation for delivery date (must be after implementation start date)
   * @returns {boolean} True if delivery date is invalid (before or equal to implementation start)
   */
  // Validation: Delivery date must be >= implementation start (or >= signing date if no impl start)
  readonly isDeliveryDateBeforeImplementationStart = computed(() => {
    const implStartDate = this.implementationStartDateSignal();
    const signingDate = this.targetSigningDateSignal();
    const deliveryDate = this.targetDeliveryDateSignal();
    // Use implementation start if set, otherwise use signing date
    const effectiveStart = implStartDate || signingDate;
    if (!effectiveStart || !deliveryDate) return false;
    
    // Ensure we're working with Date objects
    const start = effectiveStart instanceof Date ? effectiveStart : new Date(effectiveStart);
    const delivery = deliveryDate instanceof Date ? deliveryDate : new Date(deliveryDate);
    
    // Simple date-only comparison: convert to UTC midnight timestamp
    const startTime = Date.UTC(start.getFullYear(), start.getMonth(), start.getDate());
    const deliveryTime = Date.UTC(delivery.getFullYear(), delivery.getMonth(), delivery.getDate());
    
    // Error if delivery is strictly before start (delivery CAN equal start)
    return deliveryTime < startTime;
  });

  /**
   * @description Check if dates have validation errors
   */
  readonly hasDateValidationErrors = computed(() => {
    return this.isImplementationStartBeforeSigningDate() || 
           this.isDeliveryDateBeforeImplementationStart() ||
           this.isSubmissionDeadlineAfterSigningDate() ||
           this.hasDeliverableDateErrors();
  });

  /**
   * @description Check if any deliverable has date validation errors
   */
  hasDeliverableDateErrors(): boolean {
    const opp = this.opportunity();
    if (!opp?.deliverables) return false;

    const dateMap = this.deliverableDates();
    const implStartDate = this.implementationStartDateSignal();
    const signingDate = this.targetSigningDateSignal();
    const effectiveImplStart = implStartDate || signingDate;
    
    for (const deliverable of opp.deliverables) {
      const dates = dateMap.get(deliverable.id);
      const startDate = dates?.start || (deliverable.plannedStartDate ? new Date(deliverable.plannedStartDate) : null);
      const endDate = dates?.end || (deliverable.plannedEndDate ? new Date(deliverable.plannedEndDate) : null);
      
      // Check if end date is before start date
      if (startDate && endDate) {
        const startTime = Date.UTC(startDate.getFullYear(), startDate.getMonth(), startDate.getDate());
        const endTime = Date.UTC(endDate.getFullYear(), endDate.getMonth(), endDate.getDate());
        
        if (endTime < startTime) {
          return true;
        }
      }
      
      // Check if start date is before implementation start
      if (startDate && effectiveImplStart) {
        const implStartTime = Date.UTC(effectiveImplStart.getFullYear(), effectiveImplStart.getMonth(), effectiveImplStart.getDate());
        const deliverableStartTime = Date.UTC(startDate.getFullYear(), startDate.getMonth(), startDate.getDate());
        
        if (deliverableStartTime < implStartTime) {
          return true;
        }
      }
    }
    
    return false;
  }

  /**
   * @description Check if a specific deliverable has end date before start date
   */
  isDeliverableEndBeforeStart(deliverableId: number): boolean {
    const opp = this.opportunity();
    if (!opp?.deliverables) return false;

    const deliverable = opp.deliverables.find(d => d.id === deliverableId);
    if (!deliverable) return false;

    const dateMap = this.deliverableDates();
    const dates = dateMap.get(deliverableId);
    const startDate = dates?.start || (deliverable.plannedStartDate ? new Date(deliverable.plannedStartDate) : null);
    const endDate = dates?.end || (deliverable.plannedEndDate ? new Date(deliverable.plannedEndDate) : null);
    
    if (!startDate || !endDate) return false;
    
    const startTime = Date.UTC(startDate.getFullYear(), startDate.getMonth(), startDate.getDate());
    const endTime = Date.UTC(endDate.getFullYear(), endDate.getMonth(), endDate.getDate());
    
    return endTime < startTime;
  }

  /**
   * @description Check if a specific deliverable has start date before implementation start
   */
  isDeliverableStartBeforeImplementation(deliverableId: number): boolean {
    const opp = this.opportunity();
    if (!opp?.deliverables) return false;

    const deliverable = opp.deliverables.find(d => d.id === deliverableId);
    if (!deliverable) return false;

    const implStartDate = this.implementationStartDateSignal();
    const signingDate = this.targetSigningDateSignal();
    const effectiveImplStart = implStartDate || signingDate;
    
    if (!effectiveImplStart) return false;

    const dateMap = this.deliverableDates();
    const dates = dateMap.get(deliverableId);
    const deliverableStartDate = dates?.start || (deliverable.plannedStartDate ? new Date(deliverable.plannedStartDate) : null);
    
    if (!deliverableStartDate) return false;
    
    const implStartTime = Date.UTC(effectiveImplStart.getFullYear(), effectiveImplStart.getMonth(), effectiveImplStart.getDate());
    const deliverableStartTime = Date.UTC(deliverableStartDate.getFullYear(), deliverableStartDate.getMonth(), deliverableStartDate.getDate());
    
    return deliverableStartTime < implStartTime;
  }

  /**
   * @description Get minimum date for deliverable start date (cannot be before implementation start)
   */
  getMinDeliverableStartDate(): Date | null {
    const implStartDate = this.implementationStartDateControl.value;
    const signingDate = this.targetSigningDateControl.value;
    return implStartDate || signingDate;
  }

  /**
   * @description Get minimum date for deliverable end date (cannot be before planned start date)
   * @param {number} deliverableId - The ID of the deliverable
   * @returns {Date | null} The minimum allowed date for planned end (planned start date if set)
   */
  getMinDeliverableEndDate(deliverableId: number): Date | null {
    const dateMap = this.deliverableDates();
    const dates = dateMap.get(deliverableId);
    const startDate = dates?.start;
    
    // If start date is set in local state, use it
    if (startDate) {
      return startDate;
    }
    
    // Otherwise, check the original deliverable data
    const opp = this.opportunity();
    const deliverable = opp?.deliverables?.find(d => d.id === deliverableId);
    if (deliverable?.plannedStartDate) {
      return new Date(deliverable.plannedStartDate);
    }
    
    // If no start date is set, return null (no minimum restriction)
    return null;
  }

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

  // Validation: Implementation start date must be >= signing date
  readonly isImplementationStartBeforeSigningDate = computed(() => {
    const signingDate = this.targetSigningDateSignal();
    const implStartDate = this.implementationStartDateSignal();
    
    if (!signingDate || !implStartDate) return false;
    
    // Ensure we're working with Date objects
    const signing = signingDate instanceof Date ? signingDate : new Date(signingDate);
    const implStart = implStartDate instanceof Date ? implStartDate : new Date(implStartDate);
    
    // Simple date-only comparison: convert to UTC midnight timestamp
    const signingTime = Date.UTC(signing.getFullYear(), signing.getMonth(), signing.getDate());
    const implStartTime = Date.UTC(implStart.getFullYear(), implStart.getMonth(), implStart.getDate());
    
    // Error if implementation start is strictly before signing date
    return implStartTime < signingTime;
  });

  constructor() {
    // Set up change detection on form controls
    // Update signals for reactive validation AND mark as changed if in edit mode
    this.targetSigningDateControl.valueChanges.subscribe((value) => {
      this.targetSigningDateSignal.set(value);
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.implementationStartDateControl.valueChanges.subscribe((value) => {
      this.implementationStartDateSignal.set(value);
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.targetDeliveryDateControl.valueChanges.subscribe((value) => {
      this.targetDeliveryDateSignal.set(value);
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.submissionDeadlineControl.valueChanges.subscribe((value) => {
      this.submissionDeadlineSignal.set(value);
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
  }

  ngOnInit(): void {
    // Initialize form controls with current values
    const opp = this.opportunity();
    const signingDate = opp.targetSigningDate ? new Date(opp.targetSigningDate) : null;
    const implStartDate = opp.implementationStartDate ? new Date(opp.implementationStartDate) : null;
    const deliveryDate = opp.targetDeliveryDate ? new Date(opp.targetDeliveryDate) : null;
    
    if (signingDate) {
      this.targetSigningDateControl.setValue(signingDate);
      this.targetSigningDateSignal.set(signingDate);
    }
    
    // If implementation start date is not set, default to signing date
    // This implements the "Defaults to signing date if not specified" behavior
    const effectiveImplStartDate = implStartDate || signingDate;
    if (effectiveImplStartDate) {
      this.implementationStartDateControl.setValue(effectiveImplStartDate);
      this.implementationStartDateSignal.set(effectiveImplStartDate);
    }
    
    if (deliveryDate) {
      this.targetDeliveryDateControl.setValue(deliveryDate);
      this.targetDeliveryDateSignal.set(deliveryDate);
    }
    
    // AC5: Initialize signing date details
    this.isSigningDateFirmControl.setValue(opp.isTargetSigningDateFirm || false);
    this.signingDateNotesControl.setValue(opp.signingDateNotes || null);
    if (opp.submissionDeadline) {
      const submissionDate = new Date(opp.submissionDeadline);
      this.submissionDeadlineControl.setValue(submissionDate);
      this.submissionDeadlineSignal.set(submissionDate);
    }
  }

  /**
   * @description Enter edit mode
   */
  startEditing(): void {
    const opp = this.opportunity();

    // Reset duration selection
    this.resetDurationSelection();

    // Set form controls and signals
    const signingDate = opp.targetSigningDate ? new Date(opp.targetSigningDate) : null;
    const implStartDate = opp.implementationStartDate ? new Date(opp.implementationStartDate) : null;
    const deliveryDate = opp.targetDeliveryDate ? new Date(opp.targetDeliveryDate) : null;
    
    this.targetSigningDateControl.setValue(signingDate);
    this.targetSigningDateSignal.set(signingDate);
    
    // If implementation start date is not set, default to signing date
    // This implements the "Defaults to signing date if not specified" behavior
    const effectiveImplStartDate = implStartDate || signingDate;
    this.implementationStartDateControl.setValue(effectiveImplStartDate);
    this.implementationStartDateSignal.set(effectiveImplStartDate);
    
    this.targetDeliveryDateControl.setValue(deliveryDate);
    this.targetDeliveryDateSignal.set(deliveryDate);

    // AC5: Initialize signing date details
    this.isSigningDateFirmControl.setValue(opp.isTargetSigningDateFirm || false);
    this.signingDateNotesControl.setValue(opp.signingDateNotes || null);
    const submissionDate = opp.submissionDeadline ? new Date(opp.submissionDeadline) : null;
    this.submissionDeadlineControl.setValue(submissionDate);
    this.submissionDeadlineSignal.set(submissionDate);

    // Track if implementation start date was explicitly set
    this.isImplementationStartDateExplicitlySet.set(!!opp.implementationStartDate);

    // Initialize local date state for deliverables
    const dateMap = new Map<number, { start: Date | null; end: Date | null }>();
    (opp.deliverables || []).forEach((d) => {
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
   * @description Mark section as having unsaved changes
   * @private
   */
  private markAsChanged(): void {
    if (!this.hasUnsavedChangesSignal()) {
      this.hasUnsavedChangesSignal.set(true);
      this.changesDetected.emit();
    }
  }

  /**
   * @description Normalize date to UTC midnight (T00:00:00Z)
   * @param {Date | null} date - Date to normalize
   * @returns {string | null} ISO string with T00:00:00Z or null
   * @private
   */
  private normalizeDateToUTCMidnight(date: Date | null): string | null {
    if (!date) return null;
    
    // Create new date with UTC midnight using the local date values
    const year = date.getFullYear();
    const month = date.getMonth();
    const day = date.getDate();
    
    const utcDate = new Date(Date.UTC(year, month, day, 0, 0, 0, 0));
    return utcDate.toISOString();
  }

  /**
   * @description Save section changes
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    // Validate dates before saving
    if (this.hasDateValidationErrors()) {
      if (this.isImplementationStartBeforeSigningDate()) {
        this.feedbackService.showErrorToast({
          detail: this.translateService.instant('message.opportunity.implementationStartMustBeAfterSigningDate'),
          summary: this.translateService.instant('message.validation')
        });
      } else if (this.isSubmissionDeadlineAfterSigningDate()) {
        this.feedbackService.showErrorToast({
          detail: this.translateService.instant('message.opportunity.submissionDeadlineMustBeBeforeSigningDate'),
          summary: this.translateService.instant('message.validation')
        });
      } else if (this.isDeliveryDateBeforeImplementationStart()) {
        this.feedbackService.showErrorToast({
          detail: this.translateService.instant('message.opportunity.deliveryDateMustBeAfterImplementationStart'),
          summary: this.translateService.instant('message.validation')
        });
      } else if (this.hasDeliverableDateErrors()) {
        // Check which specific deliverable error occurred
        let errorFound = false;
        for (const deliverable of opp.deliverables || []) {
          if (this.isDeliverableStartBeforeImplementation(deliverable.id)) {
            this.feedbackService.showErrorToast({
              detail: this.translateService.instant('message.opportunity.deliverableStartMustBeAfterImplementation'),
              summary: this.translateService.instant('message.validation')
            });
            errorFound = true;
            break;
          }
          if (this.isDeliverableEndBeforeStart(deliverable.id)) {
            this.feedbackService.showErrorToast({
              detail: this.translateService.instant('message.opportunity.deliverableEndMustBeAfterStart'),
              summary: this.translateService.instant('message.validation')
            });
            errorFound = true;
            break;
          }
        }
        if (!errorFound) {
          this.feedbackService.showErrorToast({
            detail: this.translateService.instant('message.opportunity.deliverableDatesInvalid'),
            summary: this.translateService.instant('message.validation')
          });
        }
      }
      return;
    }

    // Build updated deliverables with dates from local state (normalized to UTC midnight)
    const dateMap = this.deliverableDates();
    const updatedDeliverables = (opp.deliverables || []).map((d) => {
      const dates = dateMap.get(d.id);
      return {
        ...d,
        plannedStartDate: this.normalizeDateToUTCMidnight(dates?.start ?? null) ?? d.plannedStartDate,
        plannedEndDate: this.normalizeDateToUTCMidnight(dates?.end ?? null) ?? d.plannedEndDate
      };
    });

    // Default implementation start date to signing date if not explicitly set
    // This implements the "Defaults to signing date if not specified" behavior shown in the UI
    const effectiveImplementationStartDate = this.implementationStartDateControl.value 
      || this.targetSigningDateControl.value;

    const whenData = {
      targetSigningDate: this.normalizeDateToUTCMidnight(this.targetSigningDateControl.value),
      implementationStartDate: this.normalizeDateToUTCMidnight(effectiveImplementationStartDate),
      targetDeliveryDate: this.normalizeDateToUTCMidnight(this.targetDeliveryDateControl.value),
      isTargetSigningDateFirm: this.isSigningDateFirmControl.value,
      signingDateNotes: this.signingDateNotesControl.value,
      submissionDeadline: this.normalizeDateToUTCMidnight(this.submissionDeadlineControl.value),
      deliverables: updatedDeliverables
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityWhen(opp.id, whenData).subscribe({
      next: (fullUpdatedOpportunity) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.hasUnsavedChangesSignal.set(false);

        // Clear local date state and reset duration selection
        this.deliverableDates.set(new Map());
        this.resetDurationSelection();

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
    this.isEditing.set(false);
    this.hasUnsavedChangesSignal.set(false);

    // Reset duration selection
    this.resetDurationSelection();
    
    // Clear unsaved changes tracking
    this.changesSavedOrDiscarded.emit();

    // Reset form controls and signals to original values
    const opp = this.opportunity();
    const signingDate = opp.targetSigningDate ? new Date(opp.targetSigningDate) : null;
    const implStartDate = opp.implementationStartDate ? new Date(opp.implementationStartDate) : null;
    const deliveryDate = opp.targetDeliveryDate ? new Date(opp.targetDeliveryDate) : null;
    
    this.targetSigningDateControl.setValue(signingDate);
    this.targetSigningDateSignal.set(signingDate);
    
    // If implementation start date is not set, default to signing date
    // This implements the "Defaults to signing date if not specified" behavior
    const effectiveImplStartDate = implStartDate || signingDate;
    this.implementationStartDateControl.setValue(effectiveImplStartDate);
    this.implementationStartDateSignal.set(effectiveImplStartDate);
    
    this.targetDeliveryDateControl.setValue(deliveryDate);
    this.targetDeliveryDateSignal.set(deliveryDate);

    // AC5: Reset signing date details
    this.isSigningDateFirmControl.setValue(opp.isTargetSigningDateFirm || false);
    this.signingDateNotesControl.setValue(opp.signingDateNotes || null);
    const submissionDate = opp.submissionDeadline ? new Date(opp.submissionDeadline) : null;
    this.submissionDeadlineControl.setValue(submissionDate);
    this.submissionDeadlineSignal.set(submissionDate);

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
    this.markAsChanged();
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
    this.markAsChanged();
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

  /**
   * Scroll to Work Breakdown Structure section
   * @description Smooth scroll to the Work Breakdown Structure section showing deliverable details
   */
  scrollToWorkBreakdown(): void {
    const wbsSection = document.getElementById('work-breakdown-structure');
    if (wbsSection) {
      wbsSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  /**
   * Handle duration selection change
   * @description Auto-calculates Target Delivery Date based on selected duration
   * @param {number} durationValue - Selected duration in months (-1 for custom)
   */
  onDurationChange(durationValue: number): void {
    this.selectedDuration.set(durationValue);

    if (durationValue === -1) {
      // Custom duration selected - show custom input
      this.showCustomDuration.set(true);
      return;
    }

    // Hide custom input if selecting a preset
    this.showCustomDuration.set(false);
    this.customDurationMonths.set(null);

    // Calculate delivery date based on signing date + duration
    this.calculateDeliveryDateFromDuration(durationValue);
  }

  /**
   * Handle custom duration input change
   * @description Auto-calculates Target Delivery Date based on custom duration
   * @param {number | null} months - Custom duration in months
   */
  onCustomDurationChange(months: number | null): void {
    this.customDurationMonths.set(months);
    if (months && months > 0) {
      this.calculateDeliveryDateFromDuration(months);
    }
  }

  /**
   * Calculate delivery date from implementation start date + duration
   * @description Sets the Target Delivery Date based on implementation start date (or signing date as fallback) and duration
   * @param {number} durationMonths - Duration in months
   */
  private calculateDeliveryDateFromDuration(durationMonths: number): void {
    // Use implementation start date if set, otherwise use signing date
    const implStartDate = this.implementationStartDateControl.value;
    const signingDate = this.targetSigningDateControl.value;
    const baseDate = implStartDate || signingDate;

    if (!baseDate || durationMonths <= 0) return;

    const deliveryDate = new Date(baseDate);
    deliveryDate.setMonth(deliveryDate.getMonth() + durationMonths);
    this.targetDeliveryDateControl.setValue(deliveryDate);
    this.cdr.detectChanges();
  }

  /**
   * Handle signing date change
   * @description Auto-populates implementation start date if not explicitly set, and recalculates delivery date if a duration is selected
   */
  onSigningDateChange(): void {
    const signingDate = this.targetSigningDateControl.value;

    // Auto-populate implementation start date if not explicitly set
    if (signingDate && !this.isImplementationStartDateExplicitlySet()) {
      this.implementationStartDateControl.setValue(signingDate);
    }

    // Recalculate delivery date if a duration is selected
    const duration = this.selectedDuration();
    const customDuration = this.customDurationMonths();

    if (duration && duration > 0) {
      this.calculateDeliveryDateFromDuration(duration);
    } else if (duration === -1 && customDuration && customDuration > 0) {
      this.calculateDeliveryDateFromDuration(customDuration);
    }

    this.cdr.detectChanges();
  }

  /**
   * Handle signing date manual change
   * @description Clears duration calculator selection when user manually changes the signing date
   */
  onSigningDateManualChange(): void {
    // Clear duration calculator selection to prevent auto-adjustments
    this.resetDurationSelection();
    // Still auto-populate implementation start date if not explicitly set
    const signingDate = this.targetSigningDateControl.value;
    if (signingDate && !this.isImplementationStartDateExplicitlySet()) {
      this.implementationStartDateControl.setValue(signingDate);
    }
    this.cdr.detectChanges();
  }

  /**
   * Handle implementation start date manual change
   * @description Marks as explicitly set and clears duration calculator selection
   */
  onImplementationStartDateManualChange(): void {
    // Mark as explicitly set when user changes it
    this.isImplementationStartDateExplicitlySet.set(true);
    // Clear duration calculator selection to prevent auto-adjustments
    this.resetDurationSelection();
    this.cdr.detectChanges();
  }

  /**
   * Handle implementation start date change
   * @description Marks the implementation start date as explicitly set and recalculates delivery date
   */
  onImplementationStartDateChange(): void {
    // Mark as explicitly set when user changes it
    this.isImplementationStartDateExplicitlySet.set(true);

    // Recalculate delivery date if a duration is selected
    const duration = this.selectedDuration();
    const customDuration = this.customDurationMonths();

    if (duration && duration > 0) {
      this.calculateDeliveryDateFromDuration(duration);
    } else if (duration === -1 && customDuration && customDuration > 0) {
      this.calculateDeliveryDateFromDuration(customDuration);
    }

    this.cdr.detectChanges();
  }

  /**
   * Handle delivery date manual change
   * @description Clears duration calculator selection when user manually changes the delivery date
   */
  onDeliveryDateManualChange(): void {
    // Clear duration calculator selection to prevent auto-adjustments
    this.resetDurationSelection();
    this.cdr.detectChanges();
  }

  /**
   * Reset implementation start date to signing date
   * @description Clears implementation start date to use signing date as default
   */
  resetImplementationStartToSigningDate(): void {
    const signingDate = this.targetSigningDateControl.value;
    this.implementationStartDateControl.setValue(signingDate);
    this.isImplementationStartDateExplicitlySet.set(false);
    this.cdr.detectChanges();
  }

  /**
   * Calculate months difference between two dates
   * @description Returns the number of months between two dates
   * @param {Date} startDate - Start date
   * @param {Date} endDate - End date
   * @returns {number} Number of months
   */
  private calculateMonthsDifference(startDate: Date, endDate: Date): number {
    const yearsDiff = endDate.getFullYear() - startDate.getFullYear();
    const monthsDiff = endDate.getMonth() - startDate.getMonth();
    const daysDiff = endDate.getDate() - startDate.getDate();

    let totalMonths = yearsDiff * 12 + monthsDiff;

    // Adjust for partial months
    if (daysDiff < 0) {
      totalMonths--;
    }

    return Math.max(0, totalMonths);
  }

  /**
   * Format duration for display
   * @description Formats months into a human-readable string
   * @param {number} months - Duration in months
   * @returns {string} Formatted duration string
   */
  private formatDuration(months: number): string {
    if (months < 12) {
      return `${months} ${months === 1 ? 'month' : 'months'}`;
    }

    const years = Math.floor(months / 12);
    const remainingMonths = months % 12;

    if (remainingMonths === 0) {
      return `${years} ${years === 1 ? 'year' : 'years'}`;
    }

    return `${years} ${years === 1 ? 'year' : 'years'} ${remainingMonths} ${remainingMonths === 1 ? 'month' : 'months'}`;
  }

  /**
   * Reset duration selection state
   * @description Clears duration-related state when editing starts or cancels
   */
  private resetDurationSelection(): void {
    this.selectedDuration.set(null);
    this.customDurationMonths.set(null);
    this.showCustomDuration.set(false);
  }

  /**
   * Get minimum date for implementation start date (must be on or after signing date)
   * @description Used as minDate for the implementation start date picker
   */
  getMinImplementationStartDate(): Date | null {
    return this.targetSigningDateControl.value;
  }

  /**
   * Get minimum date for delivery (same as implementation start, or signing date as fallback)
   * @description Returns the minimum allowed date for target delivery (can be same day as start)
   */
  getMinDeliveryDate(): Date | null {
    const implStartDate = this.implementationStartDateControl.value;
    const signingDate = this.targetSigningDateControl.value;
    const baseDate = implStartDate || signingDate;
    if (!baseDate) return null;
    return baseDate;
  }
}

