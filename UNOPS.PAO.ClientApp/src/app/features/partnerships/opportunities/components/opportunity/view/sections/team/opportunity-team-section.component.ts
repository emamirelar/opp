/**
 * @fileoverview Opportunity Team Section Component - Manages UNOPS team & internal stakeholders
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
  ChangeDetectorRef,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ChipModule } from 'primeng/chip';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { AvatarModule } from 'primeng/avatar';

// Services and Models
import {
  ValuesService,
  SimpleValue,
  OrganizationUnit,
  SuggestedOrgUnitsResponse,
} from '@shared/services/api/values.service';
import { OpportunityService } from '../../../../../services/opportunity.service';
import {
  Opportunity,
  RelevantPerson,
  RelevantPeopleResponse,
} from '@shared/models/opportunity.model';
import { FeedbackDialogService } from '@shared/services/ui';

/**
 * @class OpportunityTeamSectionComponent
 * @description Manages the Team section of opportunity with independent edit/save/cancel functionality.
 * Displays UNOPS Team & Internal Stakeholders including:
 * - Responsible Organizational Unit
 * - Initiative Type
 * - People With Skills and Experience Relevant to this Opportunity
 *
 * @example
 * ```html
 * <app-opportunity-team-section
 *   [opportunity]="opportunity()"
 *   [canUpdate]="canUpdate()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 * />
 * ```
 *
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-team-section',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    SelectModule,
    ChipModule,
    TooltipModule,
    TagModule,
    AvatarModule,
  ],
  templateUrl: './opportunity-team-section.component.html',
  styleUrls: ['./opportunity-team-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunityTeamSectionComponent implements OnInit {
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
   * @description Input signal for update permission - controls visibility of edit button
   */
  readonly canUpdate = input<boolean>(false);

  /**
   * @description Output event when opportunity is updated - signals parent to refresh
   */
  readonly opportunityUpdated = output<Opportunity>();

  /**
   * @description Output event when section is saved - for cross-section refresh triggers
   */
  readonly sectionSaved = output<void>();

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
    responsibleOrgUnitId?: number;
    proposedInitiativeTypeId?: number;
  } | null = null;
  private hasUnsavedChanges = false;

  // Form controls for Team section
  orgUnitControl = new FormControl<number | null>(null);
  initiativeTypeControl = new FormControl<number | null>(null);

  // Dropdown data
  organizationUnits = signal<OrganizationUnit[]>([]);
  initiativeTypes = signal<SimpleValue[]>([]);

  // Suggested org units based on implementation countries
  suggestedOrgUnitIds = signal<number[]>([]);
  primarySuggestedOrgUnitId = signal<number | null>(null);
  suggestionReason = signal<string | null>(null);

  // Warning banner for Hub/Region/GPO org units
  showOrgUnitWarningBanner = signal<boolean>(false);

  // Relevant People signals
  private lastLoadedOpportunityId: number | null = null;
  readonly relevantPeople = signal<RelevantPerson[] | null>(null);
  readonly relevantPeopleResponse = signal<RelevantPeopleResponse | null>(null);
  readonly loadingRelevantPeople = signal<boolean>(false);
  readonly relevantPeopleError = signal<string | null>(null);

  constructor() {
    // Set up change detection on form controls
    this.orgUnitControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.initiativeTypeControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });

    // Effect to load relevant people when opportunity ID changes
    effect(() => {
      const opp = this.opportunity();

      if (opp && opp.id && opp.id !== this.lastLoadedOpportunityId) {
        this.lastLoadedOpportunityId = opp.id;
        this.loadRelevantPeople();
      }
    });
  }

  ngOnInit(): void {
    this.loadDropdownData();
  }

  /**
   * @description Load dropdown data for form fields
   */
  private loadDropdownData(): void {
    // Use opportunity-specific endpoint that includes OrgUnit, Hub, and Region types
    this.valuesService.getOpportunityOrganizationUnits().subscribe({
      next: (data) => {
        this.organizationUnits.set(data);
        // Check if current org unit requires warning banner
        this.updateOrgUnitWarningBanner();
        this.cdr.detectChanges();
      },
    });

    this.valuesService.getProposedInitiativeTypes().subscribe({
      next: (data) => {
        this.initiativeTypes.set(data);
        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Load suggested org units based on implementation countries
   * @param prepopulateIfEmpty - If true, prepopulate the org unit control with the primary suggestion if no value is set
   */
  private loadSuggestedOrgUnits(prepopulateIfEmpty: boolean = false): void {
    const opp = this.opportunity();
    if (!opp?.countries || opp.countries.length === 0) {
      this.suggestedOrgUnitIds.set([]);
      this.primarySuggestedOrgUnitId.set(null);
      this.suggestionReason.set(null);
      return;
    }

    const countryIds = opp.countries.map((c) => c.countryId);
    this.valuesService.getSuggestedOrgUnits(countryIds).subscribe({
      next: (response: SuggestedOrgUnitsResponse) => {
        this.suggestedOrgUnitIds.set(response.suggestedOrgUnitIds);
        this.primarySuggestedOrgUnitId.set(response.primarySuggestionId);
        this.suggestionReason.set(response.suggestionReason);

        // Prepopulate with primary suggestion if no value is currently set
        if (
          prepopulateIfEmpty &&
          response.primarySuggestionId &&
          !this.orgUnitControl.value
        ) {
          this.orgUnitControl.setValue(response.primarySuggestionId);
        }

        this.cdr.detectChanges();
      },
      error: () => {
        // Silently fail - suggestions are not critical
        this.suggestedOrgUnitIds.set([]);
        this.primarySuggestedOrgUnitId.set(null);
        this.suggestionReason.set(null);
      },
    });
  }

  /**
   * @description Check if an org unit is suggested based on implementation countries
   */
  isOrgUnitSuggested(orgUnitId: number): boolean {
    return this.suggestedOrgUnitIds().includes(orgUnitId);
  }

  /**
   * @description Check if an org unit is the primary suggestion
   */
  isPrimarySuggestion(orgUnitId: number): boolean {
    return this.primarySuggestedOrgUnitId() === orgUnitId;
  }

  /**
   * @description Load relevant people from corporate directory using AI-powered semantic search
   */
  loadRelevantPeople(invalidateCache: boolean = false): void {
    const opportunityId = this.opportunity().id;

    this.loadingRelevantPeople.set(true);
    this.relevantPeopleError.set(null);

    this.opportunityService
      .getRelevantPeople(opportunityId, 6, invalidateCache)
      .subscribe({
        next: (response: RelevantPeopleResponse) => {
          this.loadingRelevantPeople.set(false);
          this.relevantPeopleResponse.set(response);
          this.relevantPeople.set(response.relevantPeople);
          this.cdr.detectChanges();
        },
        error: (error: unknown) => {
          this.loadingRelevantPeople.set(false);
          const err = error as { error?: { error?: string }; message?: string };
          const errorMessage =
            err.error?.error ||
            err.message ||
            'Failed to load relevant people';
          this.relevantPeopleError.set(errorMessage);
          this.feedbackService.showErrorToast({
            summary: 'Error',
            detail: errorMessage,
          });
        },
      });
  }

  /**
   * @description Refresh relevant people - clears cache and reloads the data
   */
  refreshRelevantPeople(): void {
    this.relevantPeople.set(null);
    this.relevantPeopleResponse.set(null);
    this.loadRelevantPeople(true);
  }

  /**
   * @description Get initials from person's name for avatar
   */
  getInitials(name: string | null): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  /**
   * @description Enter edit mode for this section
   */
  startEditing(): void {
    const opp = this.opportunity();

    // Backup original data for cancel
    this.originalData = {
      responsibleOrgUnitId: opp.responsibleOrgUnitId ?? undefined,
      proposedInitiativeTypeId: opp.proposedInitiativeTypeId ?? undefined,
    };

    // Set form controls
    this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
    this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);

    // Load suggested org units and prepopulate if no value is currently set
    const shouldPrepopulate = !opp.responsibleOrgUnitId;
    this.loadSuggestedOrgUnits(shouldPrepopulate);

    this.isEditing.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Get sorted organization units with suggestions first
   */
  getSortedOrgUnits(): OrganizationUnit[] {
    const units = this.organizationUnits();
    const suggestedIds = this.suggestedOrgUnitIds();
    const primaryId = this.primarySuggestedOrgUnitId();

    if (suggestedIds.length === 0) {
      return units;
    }

    // Sort: primary suggestion first, then other suggestions, then rest alphabetically
    return [...units].sort((a, b) => {
      const aIsPrimary = a.id === primaryId;
      const bIsPrimary = b.id === primaryId;
      const aIsSuggested = suggestedIds.includes(a.id);
      const bIsSuggested = suggestedIds.includes(b.id);

      if (aIsPrimary && !bIsPrimary) return -1;
      if (!aIsPrimary && bIsPrimary) return 1;
      if (aIsSuggested && !bIsSuggested) return -1;
      if (!aIsSuggested && bIsSuggested) return 1;
      return a.name.localeCompare(b.name);
    });
  }

  /**
   * @description Mark section as having unsaved changes
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

    const teamData = {
      responsibleOrgUnitId: this.orgUnitControl.value ?? undefined,
      proposedInitiativeTypeId: this.initiativeTypeControl.value ?? undefined,
    };

    this.isSaving.set(true);
    this.opportunityService
      .updateOpportunityTeam(opp.id, teamData)
      .subscribe({
        next: (fullUpdatedOpportunity) => {
          this.isSaving.set(false);
          this.isEditing.set(false);
          this.originalData = null;
          this.hasUnsavedChanges = false;

          this.opportunityUpdated.emit(fullUpdatedOpportunity);
          this.sectionSaved.emit();
          this.changesSavedOrDiscarded.emit();

          this.feedbackService.showSuccessToast({
            detail: this.translateService.instant(
              'message.opportunity.updatedSuccessfully'
            ),
            summary: this.translateService.instant('message.success'),
          });

          // Update warning banner visibility based on the newly saved org unit
          // Use the value from the response since the input signal hasn't been updated yet
          this.updateOrgUnitWarningBanner(fullUpdatedOpportunity.responsibleOrgUnitId ?? null);

          this.cdr.detectChanges();
        },
        error: () => {
          this.isSaving.set(false);
          this.cdr.detectChanges();
        },
      });
  }

  /**
   * @description Check if the selected/saved org unit requires a warning banner
   * @returns true if the org unit is Hub, Region, or contains GPO in name
   */
  private isOrgUnitRequiringWarning(orgUnitId: number | null | undefined): boolean {
    if (!orgUnitId) return false;

    const selectedUnit = this.organizationUnits().find((u) => u.id === orgUnitId);
    if (!selectedUnit) return false;

    // Check if the type is Hub or Region (case-insensitive string comparison)
    const unitType = String(selectedUnit.type || '').toLowerCase();
    const isHubOrRegion = unitType === 'hub' || unitType === 'region';

    // Check if the name contains GPO (case-sensitive - must be uppercase)
    const unitName = selectedUnit.name || '';
    const isGpo = unitName.includes('GPO');

    return isHubOrRegion || isGpo;
  }

  /**
   * @description Update the warning banner visibility based on the current org unit
   * @param orgUnitIdOverride - Optional org unit ID to use instead of reading from opportunity
   */
  private updateOrgUnitWarningBanner(orgUnitIdOverride?: number | null): void {
    const orgUnitId = orgUnitIdOverride !== undefined 
      ? orgUnitIdOverride 
      : this.opportunity().responsibleOrgUnitId;
    this.showOrgUnitWarningBanner.set(this.isOrgUnitRequiringWarning(orgUnitId));
  }

  /**
   * @description Cancel editing and revert changes
   */
  cancelEditing(): void {
    const opp = this.opportunity();

    // Restore original data if available
    if (this.originalData) {
      this.orgUnitControl.setValue(
        this.originalData.responsibleOrgUnitId ?? null
      );
      this.initiativeTypeControl.setValue(
        this.originalData.proposedInitiativeTypeId ?? null
      );
    } else {
      this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
      this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);
    }

    this.isEditing.set(false);
    this.originalData = null;
    this.hasUnsavedChanges = false;
    this.changesSavedOrDiscarded.emit();
    this.cdr.detectChanges();
  }
}

