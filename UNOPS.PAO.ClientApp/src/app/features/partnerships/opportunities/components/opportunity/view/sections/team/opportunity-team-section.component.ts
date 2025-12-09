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
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { FloatLabelModule } from 'primeng/floatlabel';
import { DividerModule } from 'primeng/divider';
import { CheckboxModule } from 'primeng/checkbox';

// Services and Models
import {
  ValuesService,
  SimpleValue,
  OrganizationUnit,
  SuggestedOrgUnitsResponse,
  EntityUserRolesByOrgUnitResponse,
  EntityUserRoleGroupModel,
} from '@shared/services/api/values.service';
import { OpportunityService } from '../../../../../services/opportunity.service';
import {
  Opportunity,
  OpportunityStakeholder,
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
    DialogModule,
    MessageModule,
    FloatLabelModule,
    DividerModule,
    CheckboxModule,
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
    stakeholders?: OpportunityStakeholder[];
    smeSelections?: Map<number, { selected: boolean; userId: number | null }>;
  } | null = null;
  private hasUnsavedChanges = false;

  // Form controls for Team section
  orgUnitControl = new FormControl<number | null>(null);
  initiativeTypeControl = new FormControl<number | null>(null);

  // Stakeholder dialog state
  readonly showStakeholderDialog = signal(false);
  readonly showStakeholderValidationError = signal(false);
  readonly isEditingStakeholder = signal(false);
  readonly editingStakeholderIndex = signal(-1);
  readonly userControl = new FormControl<SimpleValue | null>(null);
  readonly roleControl = new FormControl<SimpleValue | null>(null);

  // Dropdown data
  organizationUnits = signal<OrganizationUnit[]>([]);
  initiativeTypes = signal<SimpleValue[]>([]);
  readonly entityRoles = signal<SimpleValue[]>([]);
  readonly internalUsers = signal<SimpleValue[]>([]);

  // SME (Subject Matter Expert) selection state
  // Map of entityRoleId -> { selected: boolean, userId: number | null }
  readonly smeSelections = signal<Map<number, { selected: boolean; userId: number | null }>>(new Map());

  // Computed signal for SME roles (roles with Type = "SME")
  readonly smeRoles = computed(() => {
    return this.entityRoles().filter((role) => role.type === 'SME');
  });

  // Computed signal for non-SME roles (excludes SME roles for use in Add Team Member dialog)
  readonly nonSmeRoles = computed(() => {
    return this.entityRoles().filter((role) => role.type !== 'SME');
  });

  // Computed signal for SME roles grouped by SubType
  readonly smeRolesBySubType = computed(() => {
    const roles = this.smeRoles();
    const grouped = new Map<string, SimpleValue[]>();
    
    for (const role of roles) {
      const subType = role.subType || 'Other';
      if (!grouped.has(subType)) {
        grouped.set(subType, []);
      }
      grouped.get(subType)!.push(role);
    }
    
    // Convert to array of { subType, roles } sorted with custom order: Service Line first, then Other, then others
    const subTypeOrder = ['Service Line', 'Other'];
    return Array.from(grouped.entries())
      .map(([subType, roles]) => ({ subType, roles }))
      .sort((a, b) => {
        const aIndex = subTypeOrder.indexOf(a.subType);
        const bIndex = subTypeOrder.indexOf(b.subType);
        
        // If both are in the custom order, sort by their index
        if (aIndex !== -1 && bIndex !== -1) {
          return aIndex - bIndex;
        }
        // If only a is in the custom order, it comes first
        if (aIndex !== -1) {
          return -1;
        }
        // If only b is in the custom order, it comes first
        if (bIndex !== -1) {
          return 1;
        }
        // If neither is in the custom order, sort alphabetically
        return a.subType.localeCompare(b.subType);
      });
  });

  /**
   * @description Get the display name for a SubType
   * @param subType The SubType value
   * @returns The translated display name
   */
  getSubTypeDisplayName(subType: string): string {
    if (subType === 'Service Line') {
      return this.translateService.instant('label.opportunity.smeSubType.serviceLine');
    } else if (subType === 'Other') {
      return this.translateService.instant('label.opportunity.smeSubType.other');
    }
    // For any other SubType, return as-is
    return subType;
  }

  // Computed user-added stakeholders (non-auto-populated)
  readonly userAddedStakeholders = computed(() => {
    return this.opportunity().stakeholders?.filter((s) => !s.organizationHierarchyId) || [];
  });

  // Raw auto-populated stakeholders from opportunity data (without user names)
  private readonly rawAutoPopulatedStakeholders = computed(() => {
    return this.opportunity().stakeholders?.filter((s) => !!s.organizationHierarchyId) || [];
  });

  // Enriched auto-populated stakeholders with user names (for viewing mode)
  private readonly enrichedAutoPopulatedStakeholders = signal<OpportunityStakeholder[]>([]);

  // Suggested org units based on implementation countries
  suggestedOrgUnitIds = signal<number[]>([]);
  primarySuggestedOrgUnitId = signal<number | null>(null);
  suggestionReason = signal<string | null>(null);

  // Warning banner for Hub/Region/GPO org units
  showOrgUnitWarningBanner = signal<boolean>(false);

  // Dynamically loaded auto-populated stakeholders from EntityUserRoles (when editing)
  private readonly dynamicAutoPopulatedStakeholders = signal<OpportunityStakeholder[]>([]);
  readonly loadingAutoPopulatedStakeholders = signal<boolean>(false);

  // Displayed auto-populated stakeholders: uses dynamic when editing, enriched when viewing
  readonly autoPopulatedStakeholders = computed(() => {
    if (this.isEditing()) {
      return this.dynamicAutoPopulatedStakeholders();
    }
    // Use enriched data if available, otherwise fall back to raw data
    const enriched = this.enrichedAutoPopulatedStakeholders();
    return enriched.length > 0 ? enriched : this.rawAutoPopulatedStakeholders();
  });

  // Getter for existing auto-populated stakeholders (used by startEditing)
  readonly existingAutoPopulatedStakeholders = computed(() => {
    const enriched = this.enrichedAutoPopulatedStakeholders();
    return enriched.length > 0 ? enriched : this.rawAutoPopulatedStakeholders();
  });

  // Role display order for auto-populated stakeholders
  private readonly roleDisplayOrder: string[] = [
    'Region Director',
    'Region Deputy Director',
    'Hub Director',
    'Hub Deputy Director',
    'OrgUnit Director',
    'OrgUnit Deputy Director',
    'DoA1',
    'DoA2',
    'DoA3',
    'DoA4',
  ];

  // Grouped auto-populated stakeholders by OrgUnit for compact display
  readonly groupedAutoPopulatedStakeholders = computed(() => {
    const stakeholders = this.autoPopulatedStakeholders();
    const groups = new Map<string, OpportunityStakeholder[]>();

    for (const stakeholder of stakeholders) {
      const key = stakeholder.organizationHierarchyName || 'Unknown';
      if (!groups.has(key)) {
        groups.set(key, []);
      }
      groups.get(key)!.push(stakeholder);
    }

    // Sort stakeholders within each group by role display order
    const getRoleOrder = (roleName: string): number => {
      const index = this.roleDisplayOrder.indexOf(roleName);
      return index === -1 ? 999 : index; // Unknown roles go to the end
    };

    // Convert to array of groups sorted by org unit name, with stakeholders sorted by role order
    return Array.from(groups.entries())
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([orgUnitName, groupStakeholders]) => ({
        orgUnitName,
        stakeholders: groupStakeholders.sort(
          (a, b) => getRoleOrder(a.entityRoleName) - getRoleOrder(b.entityRoleName)
        ),
      }));
  });

  // Computed stakeholder count (user-added + auto-populated)
  readonly stakeholderCount = computed(() => {
    const userAdded = this.userAddedStakeholders().length;
    const autoPopulated = this.autoPopulatedStakeholders().length;
    return userAdded + autoPopulated;
  });

  // Relevant People signals
  private lastLoadedOpportunityId: number | null = null;
  readonly relevantPeople = signal<RelevantPerson[] | null>(null);
  readonly relevantPeopleResponse = signal<RelevantPeopleResponse | null>(null);
  readonly loadingRelevantPeople = signal<boolean>(false);
  readonly relevantPeopleError = signal<string | null>(null);

  constructor() {
    // Set up change detection on form controls
    this.orgUnitControl.valueChanges.subscribe((orgUnitId) => {
      if (this.isEditing()) {
        this.markAsChanged();
        // Load auto-populated stakeholders when org unit changes
        if (orgUnitId) {
          this.loadAutoPopulatedStakeholders(orgUnitId);
        } else {
          this.dynamicAutoPopulatedStakeholders.set([]);
        }
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

    // Effect to initialize SME selections when opportunity and entity roles are available
    effect(() => {
      const opp = this.opportunity();
      const smeRoles = this.smeRoles();
      
      // Initialize SME selections when both opportunity and SME roles are loaded, and not in edit mode
      if (opp && smeRoles.length > 0 && !this.isEditing()) {
        this.initializeSmeSelectionsFromStakeholders();
      }
    });

    // Effect to enrich auto-populated stakeholders with user names when viewing
    effect(() => {
      const rawStakeholders = this.rawAutoPopulatedStakeholders();
      const isEditing = this.isEditing();

      // Only fetch user names when not editing and there are auto-populated stakeholders
      if (!isEditing && rawStakeholders.length > 0) {
        // Get unique org unit IDs
        const orgUnitIds = [...new Set(rawStakeholders.map((s) => s.organizationHierarchyId).filter((id): id is number => id !== null))];

        if (orgUnitIds.length > 0) {
          this.loadingAutoPopulatedStakeholders.set(true);
          this.valuesService.getEntityUserRolesByOrgUnits(orgUnitIds).subscribe({
            next: (responses: EntityUserRolesByOrgUnitResponse[]) => {
              this.loadingAutoPopulatedStakeholders.set(false);

              // Create a map of orgUnitId -> roleId -> user names
              const userNameMap = new Map<string, string>();
              for (const response of responses) {
                for (const group of response.roleGroups) {
                  const key = `${response.organizationHierarchyId}-${group.entityRoleId}`;
                  const userNames = group.users.map((u) => u.name).join(', ');
                  userNameMap.set(key, userNames);
                }
              }

              // Enrich stakeholders with user names
              const enriched = rawStakeholders.map((s) => {
                const key = `${s.organizationHierarchyId}-${s.entityRoleId}`;
                const userName = userNameMap.get(key) || null;
                return { ...s, userName };
              });

              this.enrichedAutoPopulatedStakeholders.set(enriched);
              this.cdr.detectChanges();
            },
            error: () => {
              this.loadingAutoPopulatedStakeholders.set(false);
              // Fall back to raw stakeholders without user names
              this.enrichedAutoPopulatedStakeholders.set([]);
              this.cdr.detectChanges();
            },
          });
        }
      } else if (rawStakeholders.length === 0) {
        // Clear enriched stakeholders when there are no raw stakeholders
        this.enrichedAutoPopulatedStakeholders.set([]);
      }
    });
  }

  ngOnInit(): void {
    this.loadDropdownData();
    this.loadEntityRoles();
    this.loadInternalUsers();
  }

  /**
   * @description Load entity roles for Opportunity
   */
  private loadEntityRoles(): void {
    this.valuesService.getEntityRoles('Opportunity').subscribe({
      next: (roles) => {
        this.entityRoles.set(roles);
        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Load internal users
   */
  private loadInternalUsers(): void {
    this.valuesService.getInternalUsers().subscribe({
      next: (users) => {
        this.internalUsers.set(users);
        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Load auto-populated stakeholders from EntityUserRoles.
   * If the org unit is a GPO (name contains "GPO"), loads stakeholders from the
   * normally responsible org units for each implementation country and their parent/grandparent.
   * Otherwise, loads stakeholders from the selected OrgUnit type directly.
   */
  private loadAutoPopulatedStakeholders(orgUnitId: number): void {
    const selectedUnit = this.organizationUnits().find((u) => u.id === orgUnitId);
    if (!selectedUnit) {
      this.dynamicAutoPopulatedStakeholders.set([]);
      return;
    }

    // Check if the selected org unit is a GPO (name contains "GPO" in uppercase)
    const isGpo = selectedUnit.name?.includes('GPO') ?? false;
    const isHubOrRegion = selectedUnit.type === 'Hub' || selectedUnit.type === 'Region';

    // Clear previous stakeholders and show loading indicator
    this.dynamicAutoPopulatedStakeholders.set([]);
    this.loadingAutoPopulatedStakeholders.set(true);

    if (isGpo) {
      // For GPO: Get stakeholders from the responsible org units for each implementation country
      this.loadAutoPopulatedStakeholdersForGpo();
    } else if (isHubOrRegion) {
      // For Hub/Region (non-GPO): Get stakeholders from child org units that relate to implementation countries
      this.loadAutoPopulatedStakeholdersForHubRegion(orgUnitId);
    } else if (selectedUnit.type === 'OrgUnit') {
      // For regular OrgUnit: Load stakeholders from the selected org unit
      this.loadAutoPopulatedStakeholdersForOrgUnit(orgUnitId);
    } else {
      // For other types: No auto-population
      this.loadingAutoPopulatedStakeholders.set(false);
      this.dynamicAutoPopulatedStakeholders.set([]);
    }
  }

  /**
   * @description Load auto-populated stakeholders for GPO - gets stakeholders from
   * the responsible org units for each implementation country and their parent/grandparent.
   */
  private loadAutoPopulatedStakeholdersForGpo(): void {
    // Get implementation country IDs from the opportunity
    const countryIds = this.opportunity().countries?.map((c) => c.countryId) ?? [];

    if (countryIds.length === 0) {
      this.loadingAutoPopulatedStakeholders.set(false);
      this.dynamicAutoPopulatedStakeholders.set([]);
      this.cdr.detectChanges();
      return;
    }

    // First, get the org unit IDs for these countries (including parent/grandparent)
    this.valuesService.getOrgUnitIdsForCountries(countryIds).subscribe({
      next: (orgUnitIds: number[]) => {
        if (!orgUnitIds || orgUnitIds.length === 0) {
          this.loadingAutoPopulatedStakeholders.set(false);
          this.dynamicAutoPopulatedStakeholders.set([]);
          this.cdr.detectChanges();
          return;
        }

        // Now get EntityUserRoles for all these org units
        this.valuesService.getEntityUserRolesByOrgUnits(orgUnitIds).subscribe({
          next: (responses: EntityUserRolesByOrgUnitResponse[]) => {
            this.loadingAutoPopulatedStakeholders.set(false);

            if (!responses || responses.length === 0) {
              this.dynamicAutoPopulatedStakeholders.set([]);
              this.cdr.detectChanges();
              return;
            }

            // Create auto-populated stakeholders for each role group from each org unit
            const autoStakeholders: OpportunityStakeholder[] = [];
            for (const response of responses) {
              if (!response.roleGroups || response.roleGroups.length === 0) continue;

              for (const group of response.roleGroups) {
                autoStakeholders.push({
                  id: 0,
                  opportunityId: this.opportunity().id!,
                  entityRoleId: group.entityRoleId,
                  entityRoleName: group.entityRoleName || '',
                  isInternal: true,
                  stakeholderType: 'Internal',
                  userId: null,
                  userName: group.users.map((u) => u.name).join(', ') || null,
                  userEmail: null,
                  organizationHierarchyId: response.organizationHierarchyId,
                  organizationHierarchyName: response.organizationHierarchyName,
                  isAutoPopulated: true,
                  notes: null,
                });
              }
            }

            this.dynamicAutoPopulatedStakeholders.set(autoStakeholders);
            this.cdr.detectChanges();
          },
          error: () => {
            this.loadingAutoPopulatedStakeholders.set(false);
            this.dynamicAutoPopulatedStakeholders.set([]);
            this.cdr.detectChanges();
          },
        });
      },
      error: () => {
        this.loadingAutoPopulatedStakeholders.set(false);
        this.dynamicAutoPopulatedStakeholders.set([]);
        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Load auto-populated stakeholders for Hub/Region - gets stakeholders from
   * child org units that relate to at least one implementation country.
   */
  private loadAutoPopulatedStakeholdersForHubRegion(parentOrgUnitId: number): void {
    // Get implementation country IDs from the opportunity
    const countryIds = this.opportunity().countries?.map((c) => c.countryId) ?? [];

    if (countryIds.length === 0) {
      this.loadingAutoPopulatedStakeholders.set(false);
      this.dynamicAutoPopulatedStakeholders.set([]);
      this.cdr.detectChanges();
      return;
    }

    // First, get the child org unit IDs that relate to these countries
    this.valuesService.getChildOrgUnitIdsForHubRegion(parentOrgUnitId, countryIds).subscribe({
      next: (orgUnitIds: number[]) => {
        if (!orgUnitIds || orgUnitIds.length === 0) {
          this.loadingAutoPopulatedStakeholders.set(false);
          this.dynamicAutoPopulatedStakeholders.set([]);
          this.cdr.detectChanges();
          return;
        }

        // Now get EntityUserRoles for all these org units
        this.valuesService.getEntityUserRolesByOrgUnits(orgUnitIds).subscribe({
          next: (responses: EntityUserRolesByOrgUnitResponse[]) => {
            this.loadingAutoPopulatedStakeholders.set(false);

            if (!responses || responses.length === 0) {
              this.dynamicAutoPopulatedStakeholders.set([]);
              this.cdr.detectChanges();
              return;
            }

            // Create auto-populated stakeholders for each role group from each org unit
            const autoStakeholders: OpportunityStakeholder[] = [];
            for (const response of responses) {
              if (!response.roleGroups || response.roleGroups.length === 0) continue;

              for (const group of response.roleGroups) {
                autoStakeholders.push({
                  id: 0,
                  opportunityId: this.opportunity().id!,
                  entityRoleId: group.entityRoleId,
                  entityRoleName: group.entityRoleName || '',
                  isInternal: true,
                  stakeholderType: 'Internal',
                  userId: null,
                  userName: group.users.map((u) => u.name).join(', ') || null,
                  userEmail: null,
                  organizationHierarchyId: response.organizationHierarchyId,
                  organizationHierarchyName: response.organizationHierarchyName,
                  isAutoPopulated: true,
                  notes: null,
                });
              }
            }

            this.dynamicAutoPopulatedStakeholders.set(autoStakeholders);
            this.cdr.detectChanges();
          },
          error: () => {
            this.loadingAutoPopulatedStakeholders.set(false);
            this.dynamicAutoPopulatedStakeholders.set([]);
            this.cdr.detectChanges();
          },
        });
      },
      error: () => {
        this.loadingAutoPopulatedStakeholders.set(false);
        this.dynamicAutoPopulatedStakeholders.set([]);
        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Load auto-populated stakeholders for a specific OrgUnit type.
   */
  private loadAutoPopulatedStakeholdersForOrgUnit(orgUnitId: number): void {
    this.valuesService.getEntityUserRolesByOrgUnits([orgUnitId]).subscribe({
      next: (responses: EntityUserRolesByOrgUnitResponse[]) => {
        this.loadingAutoPopulatedStakeholders.set(false);

        // Get the first response (since we're only querying one org unit)
        const response = responses && responses.length > 0 ? responses[0] : null;

        if (!response || !response.roleGroups || response.roleGroups.length === 0) {
          this.dynamicAutoPopulatedStakeholders.set([]);
          this.cdr.detectChanges();
          return;
        }

        // Create auto-populated stakeholders for each role group
        const autoStakeholders: OpportunityStakeholder[] = response.roleGroups.map(
          (group: EntityUserRoleGroupModel) => ({
            id: 0,
            opportunityId: this.opportunity().id!,
            entityRoleId: group.entityRoleId,
            entityRoleName: group.entityRoleName || '',
            isInternal: true,
            stakeholderType: 'Internal',
            userId: null, // No specific user - auto-populated
            userName: group.users.map((u) => u.name).join(', ') || null,
            userEmail: null,
            organizationHierarchyId: orgUnitId,
            organizationHierarchyName: response.organizationHierarchyName,
            isAutoPopulated: true,
            notes: null,
          })
        );

        this.dynamicAutoPopulatedStakeholders.set(autoStakeholders);
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingAutoPopulatedStakeholders.set(false);
        this.dynamicAutoPopulatedStakeholders.set([]);
        this.cdr.detectChanges();
      },
    });
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

    // Initialize SME selections from existing stakeholders first
    this.initializeSmeSelectionsFromStakeholders();

    // Backup original data for cancel (including SME selections)
    this.originalData = {
      responsibleOrgUnitId: opp.responsibleOrgUnitId ?? undefined,
      proposedInitiativeTypeId: opp.proposedInitiativeTypeId ?? undefined,
      stakeholders: opp.stakeholders ? [...opp.stakeholders] : [],
      smeSelections: new Map(this.smeSelections()),
    };

    // Initialize dynamic auto-populated stakeholders with existing data
    this.dynamicAutoPopulatedStakeholders.set(this.existingAutoPopulatedStakeholders());

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

    // Get SME role IDs to exclude from user-added stakeholders (SME selections are handled separately via smeSelections)
    const smeRoleIds = new Set(this.smeRoles().map((r) => r.id));

    // Get user-added stakeholders (non-auto-populated and non-SME)
    const userAddedStakeholders = (opp.stakeholders || [])
      .filter((s) => !s.isAutoPopulated && !smeRoleIds.has(s.entityRoleId))
      .map((s) => ({
        userId: s.userId!,
        entityRoleId: s.entityRoleId,
        organizationHierarchyId: s.organizationHierarchyId ?? undefined,
        notes: s.notes,
      }));

    // Get auto-populated stakeholders from the current org unit
    const autoPopulated = this.autoPopulatedStakeholders().map((s) => ({
      userId: undefined,
      entityRoleId: s.entityRoleId,
      organizationHierarchyId: s.organizationHierarchyId ?? undefined,
      notes: s.notes,
    }));

    // Combine stakeholders (excluding SME - those are in smeSelections)
    const allStakeholders = [...userAddedStakeholders, ...autoPopulated];

    // Get SME selections in the format expected by the backend
    const smeSelections = this.getSmeSelectionsForSave();

    const teamData = {
      responsibleOrgUnitId: this.orgUnitControl.value ?? undefined,
      proposedInitiativeTypeId: this.initiativeTypeControl.value ?? undefined,
      stakeholders: allStakeholders.length > 0 ? allStakeholders : undefined,
      smeSelections: smeSelections.length > 0 ? smeSelections : undefined,
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityTeam(opp.id, teamData).subscribe({
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
        this.updateOrgUnitWarningBanner(
          fullUpdatedOpportunity.responsibleOrgUnitId ?? null
        );

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

      // Restore SME selections
      if (this.originalData.smeSelections) {
        this.smeSelections.set(new Map(this.originalData.smeSelections));
      }

      // Restore stakeholders
      const updatedOpportunity = {
        ...opp,
        stakeholders: this.originalData.stakeholders
          ? [...this.originalData.stakeholders]
          : [],
      };
      this.opportunityUpdated.emit(updatedOpportunity);
    } else {
      this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
      this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);
    }

    // Clear auto-populated stakeholders and reset loading state
    this.dynamicAutoPopulatedStakeholders.set([]);
    this.loadingAutoPopulatedStakeholders.set(false);

    this.isEditing.set(false);
    this.originalData = null;
    this.hasUnsavedChanges = false;
    this.changesSavedOrDiscarded.emit();
    this.cdr.detectChanges();
  }

  // ========================================================================
  // STAKEHOLDER MANAGEMENT
  // ========================================================================

  /**
   * @description Open dialog to add stakeholder
   */
  openAddStakeholderDialog(): void {
    this.userControl.setValue(null);
    this.roleControl.setValue(null);
    this.isEditingStakeholder.set(false);
    this.editingStakeholderIndex.set(-1);
    this.showStakeholderValidationError.set(false);
    this.showStakeholderDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Edit existing stakeholder
   */
  editStakeholder(index: number): void {
    const opp = this.opportunity();
    const stakeholder = opp.stakeholders?.[index];

    if (!stakeholder) return;

    const user = this.internalUsers().find((u) => u.id === stakeholder.userId);
    const role = this.entityRoles().find(
      (r) => r.id === stakeholder.entityRoleId
    );

    this.isEditingStakeholder.set(true);
    this.editingStakeholderIndex.set(index);
    this.userControl.setValue(user || null);
    this.roleControl.setValue(role || null);
    this.showStakeholderValidationError.set(false);
    this.showStakeholderDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel stakeholder dialog
   */
  cancelStakeholderDialog(): void {
    this.showStakeholderDialog.set(false);
    this.userControl.setValue(null);
    this.roleControl.setValue(null);
    this.isEditingStakeholder.set(false);
    this.editingStakeholderIndex.set(-1);
    this.showStakeholderValidationError.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Confirm stakeholder dialog (add or update)
   */
  confirmStakeholderDialog(): void {
    const user = this.userControl.value;
    const role = this.roleControl.value;

    if (!user || !role) {
      this.showStakeholderValidationError.set(true);
      this.cdr.detectChanges();
      return;
    }

    // Check for duplicate stakeholder (both when adding and editing)
    // A stakeholder is considered duplicate if the same user-role combination exists
    const opp = this.opportunity();
    const currentEditingIndex = this.editingStakeholderIndex();
    const isDuplicate = opp.stakeholders?.some((s, index) => {
      // Skip the stakeholder we're currently editing
      if (this.isEditingStakeholder() && index === currentEditingIndex) {
        return false;
      }
      return s.userId === user.id && s.entityRoleId === role.id;
    });

    if (isDuplicate) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant(
          'message.validation.stakeholderAlreadyAdded'
        ),
      });
      return;
    }

    if (this.isEditingStakeholder()) {
      this.updateStakeholder(user, role);
    } else {
      this.addStakeholder(user, role);
    }
  }

  /**
   * @description Add new stakeholder
   */
  addStakeholder(user: SimpleValue, role: SimpleValue): void {
    const opp = this.opportunity();
    const currentStakeholders = [...(opp.stakeholders || [])];

    const newStakeholder: OpportunityStakeholder = {
      id: 0,
      opportunityId: opp.id!,
      userId: user.id,
      userName: user.name,
      userEmail: null,
      entityRoleId: role.id,
      entityRoleName: role.name,
      isInternal: true,
      stakeholderType: 'Internal',
      organizationHierarchyId: null,
      organizationHierarchyName: null,
      isAutoPopulated: false,
      notes: null,
    };

    currentStakeholders.push(newStakeholder);

    const updatedOpportunity = {
      ...opp,
      stakeholders: currentStakeholders,
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.markAsChanged();
    this.cancelStakeholderDialog();
  }

  /**
   * @description Update existing stakeholder
   */
  updateStakeholder(user: SimpleValue, role: SimpleValue): void {
    const opp = this.opportunity();
    const currentStakeholders = [...(opp.stakeholders || [])];
    const index = this.editingStakeholderIndex();

    if (index < 0 || index >= currentStakeholders.length) {
      return;
    }

    currentStakeholders[index] = {
      ...currentStakeholders[index],
      userId: user.id,
      userName: user.name,
      entityRoleId: role.id,
      entityRoleName: role.name,
      notes: null,
    };

    const updatedOpportunity = {
      ...opp,
      stakeholders: currentStakeholders,
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.markAsChanged();
    this.cancelStakeholderDialog();
  }

  /**
   * @description Remove stakeholder
   */
  removeStakeholder(index: number): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.removeStakeholder'),
        detail: this.translateService.instant(
          'message.confirmRemoveStakeholder'
        ),
      },
      () => {
        const opp = this.opportunity();
        const currentStakeholders = [...(opp.stakeholders || [])];
        currentStakeholders.splice(index, 1);

        const updatedOpportunity = {
          ...opp,
          stakeholders: currentStakeholders,
        };

        this.opportunityUpdated.emit(updatedOpportunity);
        this.markAsChanged();
        this.cdr.detectChanges();
      }
    );
  }

  // ========================================================================
  // SME (SUBJECT MATTER EXPERT) MANAGEMENT
  // ========================================================================

  /**
   * @description Initialize SME selections from backend's SMESelections data
   */
  private initializeSmeSelectionsFromStakeholders(): void {
    const opp = this.opportunity();
    const smeRoles = this.smeRoles();
    const newSelections = new Map<number, { selected: boolean; userId: number | null }>();

    // Initialize all SME roles as unselected
    for (const role of smeRoles) {
      newSelections.set(role.id, { selected: false, userId: null });
    }

    // Check backend SME selections
    const smeSelections = opp.smeSelections || [];
    for (const selection of smeSelections) {
      if (selection.isSelected) {
        newSelections.set(selection.entityRoleId, {
          selected: true,
          userId: selection.userId ?? null,
        });
      }
    }

    this.smeSelections.set(newSelections);
  }

  /**
   * @description Check if an SME role is selected
   */
  isSmeRoleSelected(roleId: number): boolean {
    const selection = this.smeSelections().get(roleId);
    return selection?.selected ?? false;
  }

  /**
   * @description Get the selected user ID for an SME role
   */
  getSmeSelectedUserId(roleId: number): number | null {
    const selection = this.smeSelections().get(roleId);
    return selection?.userId ?? null;
  }

  /**
   * @description Handle SME checkbox toggle
   */
  onSmeCheckboxChange(roleId: number, checked: boolean): void {
    const currentSelections = new Map(this.smeSelections());
    const currentSelection = currentSelections.get(roleId);

    if (checked) {
      // Enable selection, keep existing userId if any
      currentSelections.set(roleId, {
        selected: true,
        userId: currentSelection?.userId ?? null,
      });
    } else {
      // Disable selection and clear userId
      currentSelections.set(roleId, {
        selected: false,
        userId: null,
      });
    }

    this.smeSelections.set(currentSelections);
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Handle SME user selection change
   */
  onSmeUserChange(roleId: number, user: SimpleValue | null): void {
    const currentSelections = new Map(this.smeSelections());

    currentSelections.set(roleId, {
      selected: true,
      userId: user?.id ?? null,
    });

    this.smeSelections.set(currentSelections);
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Get SME selections in the format expected by the backend API
   */
  private getSmeSelectionsForSave(): { entityRoleId: number; isSelected: boolean; userId: number | null }[] {
    const smeSelectionsForSave: { entityRoleId: number; isSelected: boolean; userId: number | null }[] = [];
    const selections = this.smeSelections();

    for (const [roleId, selection] of selections) {
      smeSelectionsForSave.push({
        entityRoleId: roleId,
        isSelected: selection.selected,
        userId: selection.selected ? selection.userId : null,
      });
    }

    return smeSelectionsForSave;
  }

  /**
   * @description Get the selected user object for an SME role
   */
  getSmeSelectedUser(roleId: number): SimpleValue | null {
    const userId = this.getSmeSelectedUserId(roleId);
    if (!userId) return null;
    return this.internalUsers().find((u) => u.id === userId) ?? null;
  }
}

