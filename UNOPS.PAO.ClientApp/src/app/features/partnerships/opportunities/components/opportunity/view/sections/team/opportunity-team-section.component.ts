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
    this.valuesService.getOrganizationUnits().subscribe({
      next: (data) => {
        this.organizationUnits.set(data);
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

    this.isEditing.set(true);
    this.cdr.detectChanges();
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
          this.cdr.detectChanges();
        },
        error: () => {
          this.isSaving.set(false);
          this.cdr.detectChanges();
        },
      });
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

