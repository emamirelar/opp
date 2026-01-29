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
import { MultiSelectModule } from 'primeng/multiselect';

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
 * - Org Unit Responsible for Opportunity development
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
    MultiSelectModule,
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
   * @description Input signal for AI suggestions relevant to this section
   */
  readonly suggestions = input<any[]>([]);

  /**
   * @description Input signal for update permission - controls visibility of edit button
   */
  readonly canUpdate = input<boolean>(false);

  /**
   * @description Input signal to trigger relevant people refresh when any section saves
   * Parent should increment this value when any section saves successfully
   * @type {Signal<number>}
   * @since 2.0.0
   */
  readonly sectionSaveTrigger = input<number>(0);

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
  readonly hasUnsavedChangesSignal = signal<boolean>(false);
  private originalData: {
    responsibleOrgUnitId?: number;
    proposedInitiativeTypeId?: number;
    opportunityManagerId?: number;
    collaborators?: { userId: number; expertiseIds: number[] }[];
    stakeholders?: OpportunityStakeholder[];
  } | null = null;

  // Form controls for Team section
  orgUnitControl = new FormControl<number | null>(null);
  initiativeTypeControl = new FormControl<number | null>(null);
  opportunityManagerControl = new FormControl<SimpleValue | null>(null);
  collaboratorsControl = new FormControl<SimpleValue[]>([]);

  // Org Unit Warning State
  readonly showOrgUnitWarning = signal<boolean>(false);
  readonly orgUnitWarningAcknowledged = signal<boolean>(false);
  private pendingOrgUnitChange: number | null = null;

  // Stakeholder dialog state
  readonly showStakeholderDialog = signal(false);
  readonly showStakeholderValidationError = signal(false);
  readonly isEditingStakeholder = signal(false);
  readonly editingStakeholderIndex = signal(-1);
  readonly userControl = new FormControl<SimpleValue | null>(null);
  readonly roleControl = new FormControl<SimpleValue | null>(null);

  // Collaborator dialog state
  readonly showCollaboratorDialog = signal(false);
  readonly isEditingCollaborator = signal(false);
  readonly editingCollaboratorIndex = signal(-1);
  readonly collaboratorUserControl = new FormControl<SimpleValue | null>(null);
  readonly collaboratorExpertiseControl = new FormControl<number[]>([]);

  // Dropdown data
  organizationUnits = signal<OrganizationUnit[]>([]);
  initiativeTypes = signal<SimpleValue[]>([]);
  readonly entityRoles = signal<SimpleValue[]>([]);
  readonly internalUsers = signal<SimpleValue[]>([]);
  
  // Collaborator expertise options (loaded from API)
  readonly collaboratorExpertises = signal<{ id: number; name: string; code: string }[]>([]);

  // Computed signal for non-SME roles (excludes SME roles, Opportunity Manager, and External Stakeholder for use in Add Internal Stakeholder dialog)
  // Opportunity Manager is excluded because it has a dedicated field
  // External Stakeholder is excluded because this is for INTERNAL stakeholders only
  readonly nonSmeRoles = computed(() => {
    return this.entityRoles().filter((role) => {
      // Exclude SME roles
      if (role.type === 'SME') return false;
      // Case insensitive check for role name and code
      const roleName = (role.name || '').toLowerCase();
      const roleCode = (role.code || '').toLowerCase();
      // Exclude Opportunity Manager (has dedicated field)
      if (roleName === 'opportunity manager' || roleCode === 'opportunity_manager_opportunity') return false;
      // Exclude External Stakeholder (this dropdown is for INTERNAL stakeholders only)
      if (roleName === 'external stakeholder' || roleCode.includes('external_stakeholder')) return false;
      return true;
    });
  });

  // Computed user-added stakeholders (non-auto-populated)
  readonly userAddedStakeholders = computed(() => {
    return (
      this.opportunity().stakeholders?.filter(
        (s) => !s.organizationHierarchyId
      ) || []
    );
  });

  // Combined stakeholders: user-added only (auto-populated are shown in separate "Role Holders" section)
  // Normally responsible org units are shown with org unit badge and cannot be deleted
  // Excludes Opportunity Manager role (has dedicated field)
  readonly combinedInternalStakeholders = computed(() => {
    const userAdded = this.userAddedStakeholders();
    // NOTE: Don't include autoPopulated here - they're shown in the "Role Holders for Responsible Org Unit" section
    const normalOrgUnitIds = this.normallyResponsibleOrgUnits().map(ou => ou.id);
    
    // Filter out Opportunity Manager role (has dedicated field) - case insensitive check
    const filteredUserAdded = userAdded.filter(s => {
      const roleName = (s.entityRoleName || '').toLowerCase();
      const roleCode = (s.entityRoleCode || '').toLowerCase();
      return roleName !== 'opportunity manager' && roleCode !== 'opportunity_manager_opportunity';
    });
    
    // Mark stakeholders from normally responsible org units
    const enrichedStakeholders = filteredUserAdded.map(s => {
      // Check if this stakeholder is from a normally responsible org unit
      const isFromNormalOrgUnit = s.organizationHierarchyId && 
                                   normalOrgUnitIds.includes(s.organizationHierarchyId);
      
      if (isFromNormalOrgUnit) {
        // Find the country name for this normally responsible org unit
        const normalOrgUnit = this.normallyResponsibleOrgUnits()
          .find(ou => ou.id === s.organizationHierarchyId);
        
        return {
          ...s,
          isNormallyResponsible: true,  // Flag to show badge and prevent deletion
          countryName: normalOrgUnit?.countryName || ''
        };
      }
      
      return s;
    });
    
    return enrichedStakeholders;
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
  // Excludes DoA1, DoA2, and DoA3 roles from display
  readonly groupedAutoPopulatedStakeholders = computed(() => {
    const stakeholders = this.autoPopulatedStakeholders();
    
    // Filter out DoA roles and Opportunity Manager (has dedicated field)
    const filteredStakeholders = stakeholders.filter(stakeholder => {
      const roleName = stakeholder.entityRoleName || '';
      const roleCode = stakeholder.entityRoleCode || '';
      return !['DoA1', 'DoA2', 'DoA3', 'Opportunity Manager'].includes(roleName) &&
             roleCode !== 'Opportunity_Manager_Opportunity';
    });
    
    const groups = new Map<string, OpportunityStakeholder[]>();

    for (const stakeholder of filteredStakeholders) {
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

  // Auto-populated stakeholders from the SELECTED responsible org unit hierarchy only
  // (excludes stakeholders from normally responsible org units)
  readonly responsibleOrgUnitStakeholders = computed(() => {
    const stakeholders = this.autoPopulatedStakeholders();
    const normalOrgUnitIds = this.normallyResponsibleOrgUnits().map(ou => ou.id);
    
    // Filter to only include stakeholders NOT from normally responsible org units
    return stakeholders.filter(s => {
      if (!s.organizationHierarchyId) return false;
      return !normalOrgUnitIds.includes(s.organizationHierarchyId);
    });
  });

  // Auto-populated stakeholders from NORMALLY RESPONSIBLE org units only
  // (derived from implementation countries, different from selected responsible org unit)
  readonly normallyResponsibleOrgUnitStakeholders = computed(() => {
    const stakeholders = this.autoPopulatedStakeholders();
    const normalOrgUnitIds = this.normallyResponsibleOrgUnits().map(ou => ou.id);
    
    // Filter to only include stakeholders from normally responsible org units
    return stakeholders.filter(s => {
      if (!s.organizationHierarchyId) return false;
      return normalOrgUnitIds.includes(s.organizationHierarchyId);
    });
  });

  // Grouped stakeholders from the SELECTED responsible org unit hierarchy
  // Excludes DoA1, DoA2, DoA3 roles and Opportunity Manager (handled by dedicated field)
  readonly groupedResponsibleOrgUnitStakeholders = computed(() => {
    const stakeholders = this.responsibleOrgUnitStakeholders();
    
    // Filter out DoA roles and Opportunity Manager (has dedicated field)
    const filteredStakeholders = stakeholders.filter(stakeholder => {
      const roleName = stakeholder.entityRoleName || '';
      const roleCode = stakeholder.entityRoleCode || '';
      // Exclude DoA roles and Opportunity Manager (by name or code)
      return !['DoA1', 'DoA2', 'DoA3', 'Opportunity Manager'].includes(roleName) &&
             roleCode !== 'Opportunity_Manager_Opportunity';
    });
    
    const groups = new Map<string, OpportunityStakeholder[]>();

    for (const stakeholder of filteredStakeholders) {
      const key = stakeholder.organizationHierarchyName || 'Unknown';
      if (!groups.has(key)) {
        groups.set(key, []);
      }
      groups.get(key)!.push(stakeholder);
    }

    // Sort stakeholders within each group by role display order
    const getRoleOrder = (roleName: string): number => {
      const index = this.roleDisplayOrder.indexOf(roleName);
      return index === -1 ? 999 : index;
    };

    return Array.from(groups.entries())
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([orgUnitName, groupStakeholders]) => ({
        orgUnitName,
        stakeholders: groupStakeholders.sort(
          (a, b) => getRoleOrder(a.entityRoleName) - getRoleOrder(b.entityRoleName)
        ),
      }));
  });

  // Grouped stakeholders from NORMALLY RESPONSIBLE org units (for "Other Internal Stakeholders" section)
  // Excludes DoA1, DoA2, DoA3 roles and Opportunity Manager (handled by dedicated field)
  readonly groupedNormallyResponsibleStakeholders = computed(() => {
    const stakeholders = this.normallyResponsibleOrgUnitStakeholders();
    const normalOrgUnits = this.normallyResponsibleOrgUnits();
    
    // Filter out DoA1, DoA2, DoA3 roles and Opportunity Manager (has dedicated field)
    const filteredStakeholders = stakeholders.filter(stakeholder => {
      const roleName = stakeholder.entityRoleName || '';
      const roleCode = stakeholder.entityRoleCode || '';
      // Exclude DoA roles and Opportunity Manager (by name or code)
      return !['DoA1', 'DoA2', 'DoA3', 'Opportunity Manager'].includes(roleName) &&
             roleCode !== 'Opportunity_Manager_Opportunity';
    });
    
    const groups = new Map<string, { stakeholders: OpportunityStakeholder[]; countryName: string }>();

    for (const stakeholder of filteredStakeholders) {
      const key = stakeholder.organizationHierarchyName || 'Unknown';
      if (!groups.has(key)) {
        // Find the country name for this org unit
        const normalOrgUnit = normalOrgUnits.find(ou => ou.id === stakeholder.organizationHierarchyId);
        groups.set(key, { stakeholders: [], countryName: normalOrgUnit?.countryName || '' });
      }
      groups.get(key)!.stakeholders.push(stakeholder);
    }

    // Sort stakeholders within each group by role display order
    const getRoleOrder = (roleName: string): number => {
      const index = this.roleDisplayOrder.indexOf(roleName);
      return index === -1 ? 999 : index;
    };

    return Array.from(groups.entries())
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([orgUnitName, group]) => ({
        orgUnitName,
        countryName: group.countryName,
        stakeholders: group.stakeholders.sort(
          (a, b) => getRoleOrder(a.entityRoleName) - getRoleOrder(b.entityRoleName)
        ),
      }));
  });

  // Computed stakeholder count (user-added + auto-populated, excluding Opportunity Manager)
  readonly stakeholderCount = computed(() => {
    // Use combinedInternalStakeholders which already filters out Opportunity Manager
    const userAddedFiltered = this.combinedInternalStakeholders().length;
    const autoPopulated = this.autoPopulatedStakeholders().length;
    return userAddedFiltered + autoPopulated;
  });

  // Opportunity Development Team computed signals
  readonly opportunityManager = computed(() => this.opportunity().opportunityManager);
  readonly collaborators = computed(() => this.opportunity().collaborators || []);

  // Available users for collaborators (excludes opportunity manager and existing collaborators)
  readonly availableCollaboratorUsers = computed(() => {
    const allUsers = this.internalUsers();
    const managerId = this.opportunityManagerControl.value?.id;
    const existingCollaboratorIds = new Set(
      (this.collaboratorsControl.value || []).map(c => c.id)
    );
    
    return allUsers.filter(user => {
      // Exclude opportunity manager
      if (managerId && user.id === managerId) {
        return false;
      }
      // Exclude existing collaborators
      if (existingCollaboratorIds.has(user.id)) {
        return false;
      }
      return true;
    });
  });

  // Opportunity Decision Making Pathway: DoA2 and DoA3 holders (DoA1 excluded)
  readonly decisionMakingPathwayStakeholders = computed(() => {
    const stakeholders = this.autoPopulatedStakeholders();
    
    // Filter for DoA2 and DoA3 only
    const doaStakeholders = stakeholders.filter(stakeholder => {
      const roleName = stakeholder.entityRoleName || '';
      return ['DoA2', 'DoA3'].includes(roleName);
    });

    // Group by org unit
    const groups = new Map<string, OpportunityStakeholder[]>();
    for (const stakeholder of doaStakeholders) {
      const key = stakeholder.organizationHierarchyName || 'Unknown';
      if (!groups.has(key)) {
        groups.set(key, []);
      }
      groups.get(key)!.push(stakeholder);
    }

    // Sort by role: DoA2 first, then DoA3
    const roleOrder = { 'DoA2': 1, 'DoA3': 2 };
    
    return Array.from(groups.entries())
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([orgUnitName, groupStakeholders]) => ({
        orgUnitName,
        stakeholders: groupStakeholders.sort(
          (a, b) => (roleOrder[a.entityRoleName as keyof typeof roleOrder] || 999) - (roleOrder[b.entityRoleName as keyof typeof roleOrder] || 999)
        ),
      }));
  });

  // Relevant People signals
  private lastLoadedOpportunityId: number | null = null;
  private lastSectionSaveTrigger: number = 0;
  readonly relevantPeople = signal<RelevantPerson[] | null>(null);
  readonly relevantPeopleResponse = signal<RelevantPeopleResponse | null>(null);
  readonly loadingRelevantPeople = signal<boolean>(false);
  readonly relevantPeopleError = signal<string | null>(null);

  // Check if prerequisites for stakeholder suggestions are met
  readonly hasPrerequisitesForStakeholders = computed(() => {
    const opp = this.opportunity();
    const hasOrgUnit = !!opp.responsibleOrgUnitId;
    const hasCountries = opp.countries && opp.countries.length > 0;
    return hasOrgUnit || hasCountries;
  });

  // Get "normally responsible" org units from countries (OrgUnit type only, level 3)
  // Includes country name for display purposes
  readonly normallyResponsibleOrgUnits = computed(() => {
    const opp = this.opportunity();
    const selectedOrgUnitId = opp.responsibleOrgUnitId;
    
    if (!opp.countries || opp.countries.length === 0) {
      return [];
    }

    const normalOrgUnits: { id: number; name: string; code: string; countryName: string }[] = [];
    
    for (const country of opp.countries) {
      if (!country.country?.organizationUnitHierarchy) continue;
      
      // Find the OrgUnit (Type = "OrgUnit", level 3) in the hierarchy
      const normalOrgUnit = country.country.organizationUnitHierarchy.find(
        (ou: any) => ou.type === 'OrgUnit' && ou.level === 3
      );
      
      // Only include if it's different from the selected responsible org unit
      if (normalOrgUnit && normalOrgUnit.id !== selectedOrgUnitId) {
        // Check if we already have this org unit (avoid duplicates)
        if (!normalOrgUnits.find(ou => ou.id === normalOrgUnit.id)) {
          normalOrgUnits.push({
            id: normalOrgUnit.id,
            name: normalOrgUnit.name,
            code: normalOrgUnit.code,
            countryName: country.country.name  // Include country name
          });
        }
      }
    }
    
    return normalOrgUnits;
  });

  // Computed signal to get all normally responsible org unit IDs (for warning detection)
  readonly allNormallyResponsibleOrgUnitIds = computed(() => {
    const opp = this.opportunity();
    
    if (!opp.countries || opp.countries.length === 0) {
      return [];
    }

    const normalOrgUnitIds: number[] = [];
    
    for (const country of opp.countries) {
      if (!country.country?.organizationUnitHierarchy) continue;
      
      // Find the OrgUnit (Type = "OrgUnit", level 3) in the hierarchy
      const normalOrgUnit = country.country.organizationUnitHierarchy.find(
        (ou: any) => ou.type === 'OrgUnit' && ou.level === 3
      );
      
      if (normalOrgUnit && !normalOrgUnitIds.includes(normalOrgUnit.id)) {
        normalOrgUnitIds.push(normalOrgUnit.id);
      }
    }
    
    return normalOrgUnitIds;
  });

  // Computed signal to check if selected org unit matches normally responsible org units
  readonly orgUnitConflictsWithNormalOrgUnits = computed(() => {
    const opp = this.opportunity();
    const selectedOrgUnitId = opp.responsibleOrgUnitId;
    const normalOrgUnitIds = this.allNormallyResponsibleOrgUnitIds();
    
    // No conflict if no countries or no selected org unit
    if (!selectedOrgUnitId || normalOrgUnitIds.length === 0) {
      return { hasConflict: false, affectedCountries: [] };
    }
    
    // Check if selected org unit is in the normally responsible list
    const isNormallyResponsible = normalOrgUnitIds.includes(selectedOrgUnitId);
    
    if (isNormallyResponsible) {
      // Selected org unit IS normally responsible - no conflict
      return { hasConflict: false, affectedCountries: [] };
    }
    
    // Selected org unit is NOT normally responsible - conflict!
    // Get list of affected countries
    const affectedCountries: string[] = [];
    for (const country of opp.countries || []) {
      if (!country.country?.organizationUnitHierarchy) continue;
      
      const normalOrgUnit = country.country.organizationUnitHierarchy.find(
        (ou: any) => ou.type === 'OrgUnit' && ou.level === 3
      );
      
      if (normalOrgUnit && normalOrgUnit.id !== selectedOrgUnitId) {
        affectedCountries.push(country.country.name);
      }
    }
    
    return { hasConflict: true, affectedCountries };
  });

  constructor() {
    // Set up change detection on form controls
    this.orgUnitControl.valueChanges.subscribe((orgUnitId) => {
      if (this.isEditing()) {
        // Check if this change requires a warning
        if (orgUnitId && this.shouldShowOrgUnitWarning(orgUnitId)) {
          // Store the pending change and show confirmation
          this.pendingOrgUnitChange = orgUnitId;
          this.showOrgUnitConfirmation();
        } else {
          // No warning needed - proceed with change
          this.markAsChanged();
          if (orgUnitId) {
            this.loadAutoPopulatedStakeholders(orgUnitId);
          } else {
            this.dynamicAutoPopulatedStakeholders.set([]);
          }
          this.orgUnitWarningAcknowledged.set(false);
        }
      }
    });
    this.initiativeTypeControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.opportunityManagerControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.collaboratorsControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });

    // Effect to reset warning acknowledgement when countries change
    effect(() => {
      const opp = this.opportunity();
      const countries = opp.countries || [];
      
      // Reset warning acknowledgement when countries change
      // This ensures the warning shows again if user changes countries after acknowledging
      if (this.isEditing()) {
        this.orgUnitWarningAcknowledged.set(false);
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

    // Effect to refresh relevant people when sectionSaveTrigger changes (any section saves)
    effect(() => {
      const trigger = this.sectionSaveTrigger();

      // Only refresh if trigger has changed and this isn't the initial load
      if (trigger > 0 && trigger !== this.lastSectionSaveTrigger) {
        this.lastSectionSaveTrigger = trigger;

        console.log('🔄 Team Section: Section save detected, refreshing relevant people');

        // Use setTimeout to avoid calling during signal computation
        // Delay to prevent overwhelming the backend
        setTimeout(() => {
          this.relevantPeople.set(null);
          this.relevantPeopleResponse.set(null);
          this.loadRelevantPeople(true); // Invalidate cache
        }, 4000);
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
    this.loadCollaboratorExpertises();
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
   * @description Load collaborator expertise options from API
   */
  private loadCollaboratorExpertises(): void {
    this.opportunityService.getCollaboratorExpertises().subscribe({
      next: (expertises) => {
        this.collaboratorExpertises.set(expertises);
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
      // AND from the GPO org unit itself
      this.loadAutoPopulatedStakeholdersForGpo(orgUnitId);
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
   * the responsible org units for each implementation country and their parent/grandparent,
   * AND from the GPO org unit itself.
   * @param gpoOrgUnitId - The GPO org unit ID to include in stakeholder loading
   */
  private loadAutoPopulatedStakeholdersForGpo(gpoOrgUnitId: number): void {
    // Get implementation country IDs from the opportunity
    const countryIds = this.opportunity().countries?.map((c) => c.countryId) ?? [];

    // Start with the GPO org unit ID
    const orgUnitIdsToLoad: number[] = [gpoOrgUnitId];

    if (countryIds.length === 0) {
      // If no countries, still load stakeholders from the GPO org unit itself
      this.valuesService.getEntityUserRolesByOrgUnits(orgUnitIdsToLoad).subscribe({
        next: (responses: EntityUserRolesByOrgUnitResponse[]) => {
          this.loadingAutoPopulatedStakeholders.set(false);
          this.processStakeholderResponses(responses);
        },
        error: () => {
          this.loadingAutoPopulatedStakeholders.set(false);
          this.dynamicAutoPopulatedStakeholders.set([]);
          this.cdr.detectChanges();
        },
      });
      return;
    }

    // First, get the org unit IDs for these countries (including parent/grandparent)
    this.valuesService.getOrgUnitIdsForCountries(countryIds).subscribe({
      next: (orgUnitIds: number[]) => {
        // Combine GPO org unit ID with country-related org unit IDs
        const allOrgUnitIds = [...orgUnitIdsToLoad, ...(orgUnitIds || [])];
        // Remove duplicates
        const uniqueOrgUnitIds = [...new Set(allOrgUnitIds)];

        if (uniqueOrgUnitIds.length === 0) {
          this.loadingAutoPopulatedStakeholders.set(false);
          this.dynamicAutoPopulatedStakeholders.set([]);
          this.cdr.detectChanges();
          return;
        }

        // Now get EntityUserRoles for all these org units (including GPO)
        this.valuesService.getEntityUserRolesByOrgUnits(uniqueOrgUnitIds).subscribe({
          next: (responses: EntityUserRolesByOrgUnitResponse[]) => {
            this.loadingAutoPopulatedStakeholders.set(false);
            this.processStakeholderResponses(responses);
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
   * @description Process EntityUserRolesByOrgUnitResponse array and create auto-populated stakeholders
   * @param responses - Array of EntityUserRolesByOrgUnitResponse from the API
   */
  private processStakeholderResponses(responses: EntityUserRolesByOrgUnitResponse[]): void {
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
        // Get the first user's position if available
        const firstUser = group.users && group.users.length > 0 ? group.users[0] : null;
        
        autoStakeholders.push({
          id: 0,
          opportunityId: this.opportunity().id!,
          entityRoleId: group.entityRoleId,
          entityRoleName: group.entityRoleName || '',
          entityRoleCode: group.entityRoleCode || null,
          isInternal: true,
          stakeholderType: 'Internal',
          userId: null,
          userName: group.users.map((u) => u.name).join(', ') || null,
          userEmail: null,
          position: firstUser?.position || null,  // Use first user's position
          organizationHierarchyId: response.organizationHierarchyId,
          organizationHierarchyName: response.organizationHierarchyName,
          isAutoPopulated: true,
          notes: null,
        });
      }
    }

    this.dynamicAutoPopulatedStakeholders.set(autoStakeholders);
    this.cdr.detectChanges();
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
                // Get the first user's position if available
                const firstUser = group.users && group.users.length > 0 ? group.users[0] : null;
                
                autoStakeholders.push({
                  id: 0,
                  opportunityId: this.opportunity().id!,
                  entityRoleId: group.entityRoleId,
                  entityRoleName: group.entityRoleName || '',
                  entityRoleCode: group.entityRoleCode || null,
                  isInternal: true,
                  stakeholderType: 'Internal',
                  userId: null,
                  userName: group.users.map((u) => u.name).join(', ') || null,
                  userEmail: null,
                  position: firstUser?.position || null,  // Use first user's position
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
          (group: EntityUserRoleGroupModel) => {
            // Get the first user's position if available
            const firstUser = group.users && group.users.length > 0 ? group.users[0] : null;
            
            return {
              id: 0,
              opportunityId: this.opportunity().id!,
              entityRoleId: group.entityRoleId,
              entityRoleName: group.entityRoleName || '',
              entityRoleCode: group.entityRoleCode || null,
              isInternal: true,
              stakeholderType: 'Internal',
              userId: null, // No specific user - auto-populated
              userName: group.users.map((u) => u.name).join(', ') || null,
              userEmail: null,
              position: firstUser?.position || null,  // Use first user's position
              organizationHierarchyId: orgUnitId,
              organizationHierarchyName: response.organizationHierarchyName,
              isAutoPopulated: true,
              notes: null,
            };
          }
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

    // Reset org unit warning acknowledgement when entering edit mode
    this.orgUnitWarningAcknowledged.set(false);
    this.pendingOrgUnitChange = null;

    // Backup original data for cancel
    const manager = opp.opportunityManager;
    const collaborators = opp.collaborators || [];
    
    this.originalData = {
      responsibleOrgUnitId: opp.responsibleOrgUnitId ?? undefined,
      proposedInitiativeTypeId: opp.proposedInitiativeTypeId ?? undefined,
      opportunityManagerId: manager?.userId ?? undefined,
      collaborators: collaborators.map(c => ({
        userId: c.userId,
        expertiseIds: c.expertises?.map(e => e.id) || []
      })),
      stakeholders: opp.stakeholders ? [...opp.stakeholders] : [],
    };

    // Initialize dynamic auto-populated stakeholders with existing data
    this.dynamicAutoPopulatedStakeholders.set(this.existingAutoPopulatedStakeholders());

    // Set form controls
    this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
    this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);
    
    // Set Opportunity Manager control
    if (manager) {
      const managerUser = this.internalUsers().find(u => u.id === manager.userId);
      this.opportunityManagerControl.setValue(managerUser || null);
    } else {
      this.opportunityManagerControl.setValue(null);
    }
    
    // Set Collaborators control - preserve expertise IDs from original data
    const collaboratorUsers = collaborators
      .map(c => {
        const user = this.internalUsers().find(u => u.id === c.userId);
        if (!user) return undefined;
        // Preserve expertise IDs from the original collaborator data
        return {
          ...user,
          expertiseIds: c.expertises?.map(e => e.id) || []
        };
      })
      .filter((u): u is SimpleValue & { expertiseIds: number[] } => u !== undefined);
    this.collaboratorsControl.setValue(collaboratorUsers);

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
    if (!this.hasUnsavedChangesSignal()) {
      this.hasUnsavedChangesSignal.set(true);
      this.changesDetected.emit();
    }
  }

  /**
   * @description Save section changes
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    // Get user-added stakeholders (non-auto-populated)
    const userAddedStakeholders = (opp.stakeholders || [])
      .filter((s) => !s.isAutoPopulated)
      .map((s) => ({
        userId: s.userId!,
        entityRoleId: s.entityRoleId,
        organizationHierarchyId: s.organizationHierarchyId ?? undefined,
        notes: s.notes,
      }));

    // Get auto-populated stakeholders from the current org unit
    // Include userId if available (resolved from EntityUserRoles)
    const autoPopulated = this.autoPopulatedStakeholders().map((s) => ({
      userId: s.userId ?? undefined,  // Include userId if available from resolved EntityUserRoles
      entityRoleId: s.entityRoleId,
      organizationHierarchyId: s.organizationHierarchyId ?? undefined,
      notes: s.notes,
    }));

    // Combine stakeholders
    const allStakeholders = [...userAddedStakeholders, ...autoPopulated];

    // Get collaborators with their expertise IDs
    const collaborators = this.collaboratorsControl.value?.map(c => ({
      userId: c.id,
      expertiseIds: (c as any).expertiseIds || []
    })) || [];
    
    const teamData = {
      responsibleOrgUnitId: this.orgUnitControl.value ?? undefined,
      proposedInitiativeTypeId: this.initiativeTypeControl.value ?? undefined,
      opportunityManagerId: this.opportunityManagerControl.value?.id ?? undefined,
      collaborators: collaborators.length > 0 ? collaborators : undefined,
      stakeholders: allStakeholders.length > 0 ? allStakeholders : undefined,
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityTeam(opp.id, teamData).subscribe({
      next: (fullUpdatedOpportunity) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.originalData = null;
        this.hasUnsavedChangesSignal.set(false);

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
      const original = this.originalData; // Type narrowing helper
      this.orgUnitControl.setValue(
        original.responsibleOrgUnitId ?? null
      );
      this.initiativeTypeControl.setValue(
        original.proposedInitiativeTypeId ?? null
      );

      // Restore Opportunity Manager
      if (original.opportunityManagerId) {
        const managerUser = this.internalUsers().find(u => u.id === original.opportunityManagerId);
        this.opportunityManagerControl.setValue(managerUser || null);
      } else {
        this.opportunityManagerControl.setValue(null);
      }
      
      // Restore Collaborators with their expertise IDs
      if (original.collaborators && original.collaborators.length > 0) {
        const collaboratorUsers = original.collaborators
          .map(c => {
            const user = this.internalUsers().find(u => u.id === c.userId);
            if (!user) return undefined;
            return {
              ...user,
              expertiseIds: c.expertiseIds || []
            };
          })
          .filter((u): u is SimpleValue & { expertiseIds: number[] } => u !== undefined);
        this.collaboratorsControl.setValue(collaboratorUsers);
      } else {
        this.collaboratorsControl.setValue([]);
      }

      // Restore stakeholders
      const updatedOpportunity = {
        ...opp,
        stakeholders: original.stakeholders
          ? [...original.stakeholders]
          : [],
      };
      this.opportunityUpdated.emit(updatedOpportunity);
    } else {
      this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
      this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);
      this.opportunityManagerControl.setValue(null);
      this.collaboratorsControl.setValue([]);
    }

    // Clear auto-populated stakeholders and reset loading state
    this.dynamicAutoPopulatedStakeholders.set([]);
    this.loadingAutoPopulatedStakeholders.set(false);

    this.isEditing.set(false);
    this.originalData = null;
    this.hasUnsavedChangesSignal.set(false);
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
  /**
   * @description Edit existing stakeholder
   * @param index - Index in the combinedInternalStakeholders array
   */
  editStakeholder(index: number): void {
    // Get the stakeholder from combined list
    const stakeholderToEdit = this.combinedInternalStakeholders()[index];
    
    // Don't allow editing of normally responsible stakeholders
    if (stakeholderToEdit.isNormallyResponsible) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.cannotEditNormallyResponsible'),
      });
      return;
    }
    
    const opp = this.opportunity();
    
    // Find the actual index in opportunity.stakeholders
    const actualIndex = opp.stakeholders?.findIndex(
      s => s.id === stakeholderToEdit.id && 
           s.entityRoleId === stakeholderToEdit.entityRoleId &&
           s.userId === stakeholderToEdit.userId
    ) ?? -1;
    
    if (actualIndex === -1) {
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: this.translateService.instant('message.stakeholderNotFound'),
      });
      return;
    }
    
    const stakeholder = opp.stakeholders![actualIndex];

    const user = this.internalUsers().find((u) => u.id === stakeholder.userId);
    const role = this.entityRoles().find(
      (r) => r.id === stakeholder.entityRoleId
    );

    this.isEditingStakeholder.set(true);
    this.editingStakeholderIndex.set(actualIndex);  // Store the actual index
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
      position: user.position || null, // Include position from user if available
      entityRoleId: role.id,
      entityRoleName: role.name,
      entityRoleCode: null, // Will be populated from backend when saved
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
  /**
   * @description Remove stakeholder from opportunity
   * @param index - Index in the combinedInternalStakeholders array
   */
  removeStakeholder(index: number): void {
    // Get the stakeholder from combined list
    const stakeholderToRemove = this.combinedInternalStakeholders()[index];
    
    // Don't allow removal of normally responsible stakeholders
    if (stakeholderToRemove.isNormallyResponsible) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.cannotRemoveNormallyResponsible'),
      });
      return;
    }
    
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
        
        // Find the actual index in opportunity.stakeholders
        const actualIndex = currentStakeholders.findIndex(
          s => s.id === stakeholderToRemove.id && 
               s.entityRoleId === stakeholderToRemove.entityRoleId &&
               s.userId === stakeholderToRemove.userId
        );
        
        if (actualIndex === -1) {
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('message.error'),
            detail: this.translateService.instant('message.stakeholderNotFound'),
          });
          return;
        }
        
        currentStakeholders.splice(actualIndex, 1);

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
  // COLLABORATOR MANAGEMENT (Opportunity Development Team)
  // ========================================================================

  /**
   * @description Open dialog to add collaborator
   */
  openAddCollaboratorDialog(): void {
    this.isEditingCollaborator.set(false);
    this.editingCollaboratorIndex.set(-1);
    this.collaboratorUserControl.setValue(null);
    this.collaboratorExpertiseControl.setValue([]);
    this.showCollaboratorDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Open dialog to edit existing collaborator
   */
  editCollaborator(index: number): void {
    const collaborators = this.collaboratorsControl.value || [];
    const collaborator = collaborators[index] as SimpleValue & { expertiseIds?: number[] };
    if (!collaborator) return;

    this.isEditingCollaborator.set(true);
    this.editingCollaboratorIndex.set(index);
    
    // Set the user (find the full SimpleValue object)
    const userValue = this.internalUsers().find(u => u.id === collaborator.id);
    this.collaboratorUserControl.setValue(userValue || collaborator);
    
    // Set the expertise IDs
    this.collaboratorExpertiseControl.setValue(collaborator.expertiseIds || []);
    
    this.showCollaboratorDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Get expertise name by ID
   */
  getExpertiseName(expertiseId: number): string {
    const expertise = this.collaboratorExpertises().find(e => e.id === expertiseId);
    return expertise?.name || `Expertise ${expertiseId}`;
  }

  /**
   * @description Cancel collaborator dialog
   */
  cancelCollaboratorDialog(): void {
    this.showCollaboratorDialog.set(false);
    this.isEditingCollaborator.set(false);
    this.editingCollaboratorIndex.set(-1);
    this.collaboratorUserControl.setValue(null);
    this.collaboratorExpertiseControl.setValue([]);
    this.cdr.detectChanges();
  }

  /**
   * @description Confirm collaborator dialog (add or update collaborator)
   * If adding a user that already exists as collaborator, merges the expertise areas
   */
  confirmCollaboratorDialog(): void {
    const user = this.collaboratorUserControl.value;
    const expertiseIds = this.collaboratorExpertiseControl.value || [];
    
    if (!user) {
      return;
    }

    // Validate expertise selection
    if (expertiseIds.length === 0) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant(
          'message.validation.expertiseRequired'
        ),
      });
      return;
    }

    const currentCollaborators = this.collaboratorsControl.value || [];
    const isEditing = this.isEditingCollaborator();
    const editingIndex = this.editingCollaboratorIndex();

    // Check if user is already the Opportunity Manager
    const manager = this.opportunityManagerControl.value;
    if (manager && manager.id === user.id) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant(
          'message.validation.collaboratorIsManager'
        ),
      });
      return;
    }

    // Check for existing collaborator (to merge expertise if found)
    const existingIndex = currentCollaborators.findIndex((c, i) => c.id === user.id && i !== editingIndex);
    const hasExistingCollaborator = existingIndex !== -1;

    if (hasExistingCollaborator && !isEditing) {
      // Merge expertise with existing collaborator
      const existingCollaborator = currentCollaborators[existingIndex] as SimpleValue & { expertiseIds?: number[] };
      const existingExpertiseIds = existingCollaborator.expertiseIds || [];
      
      // Combine and deduplicate expertise IDs
      const mergedExpertiseIds = [...new Set([...existingExpertiseIds, ...expertiseIds])];
      
      // Update the existing collaborator with merged expertise
      const updatedCollaborators = [...currentCollaborators] as Array<SimpleValue & { expertiseIds?: number[] }>;
      updatedCollaborators[existingIndex] = {
        ...existingCollaborator,
        expertiseIds: mergedExpertiseIds
      };
      this.collaboratorsControl.setValue(updatedCollaborators);
      
      // Show info toast about merging
      this.feedbackService.showSuccessToast({
        summary: this.translateService.instant('message.success'),
        detail: this.translateService.instant(
          'message.collaboratorExpertiseMerged'
        ),
      });
    } else if (isEditing && editingIndex >= 0) {
      // Update existing collaborator (standard edit)
      const collaboratorWithExpertise = {
        ...user,
        expertiseIds: expertiseIds
      };
      const updatedCollaborators = [...currentCollaborators];
      updatedCollaborators[editingIndex] = collaboratorWithExpertise;
      this.collaboratorsControl.setValue(updatedCollaborators);
    } else {
      // Add new collaborator
      const collaboratorWithExpertise = {
        ...user,
        expertiseIds: expertiseIds
      };
      const updatedCollaborators = [...currentCollaborators, collaboratorWithExpertise];
      this.collaboratorsControl.setValue(updatedCollaborators);
    }
    
    this.markAsChanged();
    this.cancelCollaboratorDialog();
  }

  /**
   * @description Remove collaborator
   */
  removeCollaborator(index: number): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.removeCollaborator'),
        detail: this.translateService.instant(
          'message.confirmRemoveCollaborator'
        ),
      },
      () => {
        const currentCollaborators = [...(this.collaboratorsControl.value || [])];
        currentCollaborators.splice(index, 1);
        this.collaboratorsControl.setValue(currentCollaborators);
        this.markAsChanged();
        this.cdr.detectChanges();
      }
    );
  }

  /**
   * @description Check if org unit change should trigger a warning
   * @param newOrgUnitId - The newly selected org unit ID
   * @returns True if warning should be shown
   */
  private shouldShowOrgUnitWarning(newOrgUnitId: number): boolean {
    const opp = this.opportunity();
    
    // No warning if no countries selected yet
    if (!opp.countries || opp.countries.length === 0) {
      return false;
    }

    // No warning if already acknowledged for this change
    if (this.orgUnitWarningAcknowledged()) {
      return false;
    }

    // Get all normally responsible org unit IDs
    const normalOrgUnitIds = this.allNormallyResponsibleOrgUnitIds();
    
    // No warning if no normally responsible org units found
    if (normalOrgUnitIds.length === 0) {
      return false;
    }

    // Show warning if selected org unit is NOT in the normally responsible list
    return !normalOrgUnitIds.includes(newOrgUnitId);
  }

  /**
   * @description Show confirmation dialog for org unit change
   */
  private showOrgUnitConfirmation(): void {
    const conflict = this.orgUnitConflictsWithNormalOrgUnits();
    
    if (!conflict.hasConflict || conflict.affectedCountries.length === 0) {
      // No conflict - just apply the change directly
      const orgUnitId = this.pendingOrgUnitChange;
      this.pendingOrgUnitChange = null;
      
      if (orgUnitId) {
        this.markAsChanged();
        this.loadAutoPopulatedStakeholders(orgUnitId);
      } else {
        this.dynamicAutoPopulatedStakeholders.set([]);
      }
      return;
    }

    // Immediately revert the selection in the UI
    // If user confirms, we'll reapply it
    const opp = this.opportunity();
    this.orgUnitControl.setValue(opp.responsibleOrgUnitId || null, { emitEvent: false });

    const countryList = conflict.affectedCountries.join(', ');
    const isMultiple = conflict.affectedCountries.length > 1;
    
    const message = isMultiple
      ? this.translateService.instant('message.orgUnitNotNormallyResponsibleMultiple', { countries: countryList })
      : this.translateService.instant('message.orgUnitNotNormallyResponsibleSingle', { country: countryList });

    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('title.confirmOrgUnitSelection'),
        detail: message,
      },
      () => {
        // User confirmed - apply the pending change
        this.orgUnitWarningAcknowledged.set(true);
        const orgUnitId = this.pendingOrgUnitChange;
        this.pendingOrgUnitChange = null;
        
        // Reapply the selection
        this.orgUnitControl.setValue(orgUnitId, { emitEvent: false });
        
        // Load stakeholders
        if (orgUnitId) {
          this.markAsChanged();
          this.loadAutoPopulatedStakeholders(orgUnitId);
        } else {
          this.dynamicAutoPopulatedStakeholders.set([]);
        }
      }
    );
    // Note: If user cancels, pendingOrgUnitChange stays set but the UI is already reverted.
    // It will be overwritten on the next org unit selection attempt.
  }

  /**
   * @description Get translation key for entity role based on code
   * @param entityRoleCode - The code of the entity role (e.g., "DoA1_OrganizationHierarchy")
   * @returns Translation key (e.g., "role.DoA1_OrganizationHierarchy") or null if code is not available
   */
  getRoleTranslationKey(entityRoleCode: string | null | undefined): string | null {
    if (!entityRoleCode) return null;
    return `role.${entityRoleCode}`;
  }

  /**
   * @description Get translated role name, falling back to entityRoleName if translation is not available
   * @param stakeholder - The stakeholder object
   * @returns Translated role name or the original entityRoleName
   */
  getTranslatedRoleName(stakeholder: OpportunityStakeholder): string {
    const translationKey = this.getRoleTranslationKey(stakeholder.entityRoleCode);
    if (translationKey) {
      const translated = this.translateService.instant(translationKey);
      // If translation exists and is different from the key, use it
      if (translated && translated !== translationKey) {
        return translated;
      }
    }
    // Fallback to entityRoleName
    return stakeholder.entityRoleName;
  }

}


