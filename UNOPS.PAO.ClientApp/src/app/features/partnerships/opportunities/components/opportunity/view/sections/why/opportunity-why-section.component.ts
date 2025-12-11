/**
 * @fileoverview Opportunity WHY Section Component - Manages impact and alignment with edit capabilities
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
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextarea } from 'primeng/inputtextarea';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { CheckboxModule } from 'primeng/checkbox';
import { AccordionModule } from 'primeng/accordion';
import { InputNumberModule } from 'primeng/inputnumber';
import { FloatLabelModule } from 'primeng/floatlabel';

// Services and Models
import {
  ValuesService,
  SDG,
  SDGTarget,
  SDGIndicator,
  UNCFOutcome,
  UNCFIndicator,
} from '@shared/services/api/values.service';
import { OpportunityService } from '../../../../../services/opportunity.service';
import {
  Opportunity,
  OpportunitySDG,
  OpportunitySDGTarget,
  OpportunitySDGIndicator,
  OpportunityUNCFOutcome,
  OpportunityUNCFIndicator,
  OpportunityCountry,
  UNOPSMission,
  OpportunityUNOPSMission,
} from '@shared/models/opportunity.model';
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
    TooltipModule,
    DialogModule,
    SelectModule,
    MessageModule,
    CheckboxModule,
    AccordionModule,
    InputNumberModule,
    FloatLabelModule,
  ],
  templateUrl: './opportunity-why-section.component.html',
  styleUrls: ['./opportunity-why-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
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
  readonly suggestions = input<any[]>([]);

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
  private hasUnsavedChanges = false;
  private originalData: {
    expectedBeneficiaries?: string | null;
    expectedImpact?: string | null;
    expectedOutcomes?: string | null;
    challenges?: string | null;
    sdGs?: any[];
    uncfOutcomes?: any[];
    unopsMissions?: any[];
  } | null = null;

  // Form controls for WHY section
  expectedBeneficiariesControl = new FormControl<string | null>(null);
  estimatedDirectBeneficiariesControl = new FormControl<number | null>(null);
  estimatedIndirectBeneficiariesControl = new FormControl<number | null>(null);
  beneficiariesToBeDeterminedControl = new FormControl<boolean>(false);
  expectedImpactControl = new FormControl<string | null>(null, [
    Validators.maxLength(200),
  ]);
  expectedOutcomesControl = new FormControl<string | null>(null, [
    Validators.maxLength(200),
  ]);
  challengesControl = new FormControl<string | null>(null);

  // Climate and framework alignments by country (map of countryId -> alignment status)
  humanitarianFrameworkAlignments = signal<Map<number, boolean | null>>(
    new Map(),
  );
  ndcAlignments = signal<Map<number, boolean | null>>(new Map());
  napAlignments = signal<Map<number, boolean | null>>(new Map());
  orgUnitStrategyAlignments = signal<Map<number, boolean | null>>(new Map());

  // SDG data
  sdgs = signal<SDG[]>([]);

  // Sorted SDGs by sdgId
  readonly sortedSDGs = computed(() => {
    const allSDGs = [...this.sdgs()];
    return allSDGs.sort((a, b) => {
      const aId = a.sdgId || '';
      const bId = b.sdgId || '';
      
      // Handle "N/A" - always put it at the end
      if (aId === 'N/A' && bId !== 'N/A') return 1;
      if (bId === 'N/A' && aId !== 'N/A') return -1;
      if (aId === 'N/A' && bId === 'N/A') return 0;
      
      // Try numeric comparison first (for "1", "2", "3", etc.)
      const aNum = parseInt(aId, 10);
      const bNum = parseInt(bId, 10);
      
      if (!isNaN(aNum) && !isNaN(bNum)) {
        return aNum - bNum;
      }
      
      // Fallback to string comparison
      return aId.localeCompare(bId);
    });
  });
  availableTargets = signal<SDGTarget[]>([]);
  availableIndicators = signal<SDGIndicator[]>([]);
  loadingTargets = signal<boolean>(false);

  // SDG dialog - Two-step flow
  showSDGDialog = signal<boolean>(false);
  sdgDialogStep = signal<1 | 2>(1); // Step 1: Select SDGs, Step 2: Select Targets/Indicators
  
  // Step 1: Multi-select SDGs with Main/Cross-cutting selection
  selectedSDGIds = signal<Set<string>>(new Set()); // Set of selected SDG IDs (sdgId strings)
  selectedSDGsForStep1 = signal<SDG[]>([]); // Selected SDG objects for step 1
  // Map<sdgId, { isPrimary: boolean }> - Main/Cross-cutting selection for each SDG in step 1
  sdgPrimarySecondaryInStep1 = signal<Map<string, { isPrimary: boolean | null }>>(new Map());
  
  // Step 2: Track which SDG panel is expanded (only one at a time)
  expandedSDGIdInStep2 = signal<string | null>(null);
  
  // Step 2: Targets and Indicators per SDG
  // Map<sdgId, { targets: Map<targetId, Set<indicatorIds>>, skipTargets: boolean, isPrimary: boolean }>
  sdgTargetsAndIndicators = signal<Map<string, {
    targets: Map<number, Set<number>>;
    skipTargets: boolean;
    isPrimary: boolean;
    availableTargets: SDGTarget[];
    availableIndicators: SDGIndicator[];
    loadingTargets: boolean;
    loadingIndicatorsForTargets: Set<number>;
  }>>(new Map());
  
  // Legacy fields (kept for backward compatibility during transition)
  sdgControl = new FormControl<SDG | null>(null);
  isPrimaryControl = new FormControl<boolean>(false);
  skipTargetsControl = new FormControl<boolean>(false);
  isEditingSDG = signal<boolean>(false);
  editingSDGIndex = signal<number | null>(null);

  // Convert FormControl values to signals for reactivity
  sdgControlValue = toSignal(this.sdgControl.valueChanges, {
    initialValue: null,
  });
  skipTargetsControlValue = toSignal(this.skipTargetsControl.valueChanges, {
    initialValue: false,
  });

  // Selected targets and indicators for the current SDG being added/edited (legacy - used in old flow)
  selectedTargets = signal<Map<number, Set<number>>>(new Map()); // Map<targetId, Set<indicatorIds>>

  // Track which targets are currently loading indicators (legacy)
  loadingIndicatorsForTargets = signal<Set<number>>(new Set());

  // Pending SDG selections (for batch add functionality)
  pendingSDGSelections = signal<OpportunitySDG[]>([]);

  // Track if editing from pending selections
  editingFromPending = signal<boolean>(false);
  editingPendingIndex = signal<number | null>(null);
  showValidationError = signal<boolean>(false);
  
  // Track validation errors for pending SDGs (by index)
  sdgValidationErrors = signal<Set<number>>(new Set());

  // UNOPS Missions data
  unopsMissions = signal<UNOPSMission[]>([]);
  selectedUNOPSMissions = signal<Set<number>>(new Set());
  unopsMissionsNotApplicable = signal<boolean>(false);
  showUNOPSMissionsDialog = signal<boolean>(false);
  // Store selections before dialog opens for restore on cancel
  private preDialogUNOPSMissions: Set<number> | null = null;

  // Computed properties
  readonly sdgCount = computed(() => this.opportunity().sdGs?.length || 0);

  // Sorted SDGs for view mode (sorted by sdgId)
  readonly sortedOpportunitySDGs = computed(() => {
    const sdGs = this.opportunity().sdGs || [];
    return [...sdGs].sort((a, b) => {
      const aId = a.sdgId || '';
      const bId = b.sdgId || '';
      
      // Handle "N/A" - always put it at the end
      if (aId === 'N/A' && bId !== 'N/A') return 1;
      if (bId === 'N/A' && aId !== 'N/A') return -1;
      if (aId === 'N/A' && bId === 'N/A') return 0;
      
      // Try numeric comparison first (for "1", "2", "3", etc.)
      const aNum = parseInt(aId, 10);
      const bNum = parseInt(bId, 10);
      
      if (!isNaN(aNum) && !isNaN(bNum)) {
        return aNum - bNum;
      }
      
      // Fallback to string comparison
      return aId.localeCompare(bId);
    });
  });
  readonly primarySDG = computed(
    () => this.opportunity().sdGs?.find((sdg) => sdg.isPrimary) || null,
  );
  readonly pendingSDGCount = computed(() => this.pendingSDGSelections().length);
  readonly hasPrimaryInPending = computed(() =>
    this.pendingSDGSelections().some((sdg) => sdg.isPrimary),
  );

  // Available SDGs (excluding those already in pending selections, except when editing)
  readonly availableSDGs = computed(() => {
    const allSDGs = this.sdgs();
    const pending = this.pendingSDGSelections();
    const editingIndex = this.editingPendingIndex();

    // If editing, allow the SDG being edited to appear in the dropdown
    const pendingSDGIds = new Set(
      pending.filter((_, index) => index !== editingIndex).map((s) => s.sdgId),
    );

    return allSDGs.filter((sdg) => sdg.sdgId && !pendingSDGIds.has(sdg.sdgId));
  });

  // Check if SDG configuration is complete and ready to add
  // NOTE: Changed to only check if SDG is selected - targets/opt-out validation now happens at commit time
  readonly isSDGConfigurationComplete = computed(() => {
    // Must have an SDG selected (use signal or current value)
    const currentSdg = this.sdgControlValue() || this.sdgControl.value;
    return !!currentSdg;
  });

  constructor() {
    // Set up change detection on form controls
    // Only mark as changed if we're in edit mode (to avoid triggering on initial setValue)
    this.expectedBeneficiariesControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.expectedImpactControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.expectedOutcomesControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.challengesControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
  }

  // UNCF data - country-specific outcomes
  // Include countries that either have active UNCF Metadata OR have existing OpportunityUNCFOutcomes
  readonly countriesWithUNCF = computed(() => {
    const countries = this.opportunity().countries || [];
    const opp = this.opportunity();
    const existingUNCFOutcomes = opp.uncfOutcomes || [];

    return countries.filter((c) => {
      // Show country if it has active UNCF Metadata
      if (c.country?.hasActiveUNCF) {
        return true;
      }

      // Also show country if it has existing OpportunityUNCFOutcomes
      const hasExistingOutcomes = existingUNCFOutcomes.some(
        (uo) => uo.opportunityCountryId === c.id,
      );
      return hasExistingOutcomes;
    });
  });

  // UNCF outcomes grouped by country
  uncfOutcomesByCountry = signal<Map<number, UNCFOutcome[]>>(new Map());

  // Available UNCF indicators for selected outcomes
  availableUNCFIndicators = signal<Map<number, UNCFIndicator[]>>(new Map());

  // Loading states for UNCF
  loadingUNCFOutcomes = signal<boolean>(false);
  loadingUNCFIndicatorsForOutcome = signal<Set<number>>(new Set());

  // UNCF dialog (similar to SDG dialog)
  showUNCFDialog = signal<boolean>(false);
  selectedCountryForUNCF = signal<OpportunityCountry | null>(null);
  availableUNCFOutcomes = signal<UNCFOutcome[]>([]);
  loadingUNCFOutcomesForDialog = signal<boolean>(false);
  isEditingUNCFCountry = signal<boolean>(false);
  editingUNCFCountryIndex = signal<number | null>(null);

  // Selected outcomes and indicators for the current country being added/edited
  selectedUNCFOutcomes = signal<Map<number, Set<number>>>(new Map()); // Map<outcomeId, Set<indicatorIds>>

  // Track which outcomes are currently loading indicators
  loadingIndicatorsForUNCFOutcomes = signal<Set<number>>(new Set());
  showUNCFValidationError = signal<boolean>(false);

  // Computed property for UNCF count
  readonly uncfCount = computed(() => {
    const opp = this.opportunity();
    return opp.uncfOutcomes?.length || 0;
  });

  // Computed property for countries with humanitarian framework
  readonly countriesWithFramework = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter((c) => c.hasHumanitarianFramework);
  });

  // Computed property for countries without humanitarian framework
  readonly countriesWithoutFramework = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter((c) => !c.hasHumanitarianFramework && c.country);
  });

  // Computed property for countries with NDC
  readonly countriesWithNdc = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter((c) => c.hasNdc);
  });

  // Computed property for countries without NDC
  readonly countriesWithoutNdc = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter((c) => !c.hasNdc && c.country);
  });

  // Computed property for countries with NAP
  readonly countriesWithNap = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter((c) => c.hasNap);
  });

  // Computed property for countries without NAP
  readonly countriesWithoutNap = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter((c) => !c.hasNap && c.country);
  });

  // Computed property for countries with Organization Unit Strategy
  readonly countriesWithOrgUnitStrategy = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter((c) => c.hasOrgUnitStrategy);
  });

  // Computed property for countries without Organization Unit Strategy
  readonly countriesWithoutOrgUnitStrategy = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter((c) => !c.hasOrgUnitStrategy && c.country);
  });

  ngOnInit(): void {
    // Load SDGs on initialization
    this.loadSDGs();

    // Load UNOPS Missions
    this.loadUNOPSMissions();

    // Load UNCF outcomes for countries with active UNSDCF
    this.loadUNCFOutcomesForCountries();

    // Watch for changes to beneficiariesToBeDetermined checkbox
    this.beneficiariesToBeDeterminedControl.valueChanges.subscribe(
      (toBeDetermined) => {
        if (toBeDetermined) {
          // Clear and disable the number fields when "to be determined" is checked
          this.estimatedDirectBeneficiariesControl.setValue(null);
          this.estimatedIndirectBeneficiariesControl.setValue(null);
          this.estimatedDirectBeneficiariesControl.disable();
          this.estimatedIndirectBeneficiariesControl.disable();
        } else {
          // Enable the number fields when "to be determined" is unchecked
          this.estimatedDirectBeneficiariesControl.enable();
          this.estimatedIndirectBeneficiariesControl.enable();
        }
        // Only mark as changed if we're in edit mode (prevents false positives during initialization)
        if (this.isEditing()) {
          this.markAsChanged();
        }
        this.cdr.detectChanges();
      },
    );

    // Watch for changes to beneficiary number controls
    this.estimatedDirectBeneficiariesControl.valueChanges.subscribe(() => {
      // Only mark as changed if we're in edit mode (prevents false positives during initialization)
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });

    this.estimatedIndirectBeneficiariesControl.valueChanges.subscribe(() => {
      // Only mark as changed if we're in edit mode (prevents false positives during initialization)
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });

    // Watch for changes to skipTargetsControl
    this.skipTargetsControl.valueChanges.subscribe((skipValue) => {
      // If user unchecks the skip option, load targets for the current SDG
      if (!skipValue && this.sdgControl.value) {
        const currentSDG = this.sdgControl.value;
        if (currentSDG.sdgId) {
          console.log(
            'Skip checkbox unchecked, loading targets for SDG:',
            currentSDG.sdgId,
          );
          this.loadingTargets.set(true);
          this.valuesService.getSDGTargets(currentSDG.sdgId).subscribe({
            next: (targets) => {
              console.log('Loaded targets for SDG:', currentSDG.sdgId, targets);
              this.loadingTargets.set(false);
              this.availableTargets.set(targets);

              // If editing an existing SDG with targets, pre-select them
              if (this.isEditingSDG()) {
                const index = this.editingSDGIndex();
                if (index !== null) {
                  const opp = this.opportunity();
                  const sdg = opp.sdGs?.[index];

                  if (sdg?.targets && sdg.targets.length > 0) {
                    const selectedTargetsMap = new Map<number, Set<number>>();

                    // Load all indicators for the targets
                    const indicatorRequests = sdg.targets.map((target) =>
                      this.valuesService.getSDGIndicators(target.sdgTargetId),
                    );

                    if (indicatorRequests.length > 0) {
                      import('rxjs').then((rxjs) => {
                        rxjs.forkJoin(indicatorRequests).subscribe({
                          next: (allIndicators) => {
                            const flatIndicators = allIndicators.flat();
                            this.availableIndicators.set(flatIndicators);

                            // Pre-select targets and indicators
                            sdg.targets!.forEach((target) => {
                              const indicatorIds = new Set<number>();
                              target.indicators?.forEach((indicator) => {
                                indicatorIds.add(
                                  indicator.sdgIndicatorDatabaseId,
                                );
                              });
                              selectedTargetsMap.set(
                                target.sdgTargetDatabaseId,
                                indicatorIds,
                              );
                            });

                            this.selectedTargets.set(selectedTargetsMap);
                            this.cdr.detectChanges();
                          },
                          error: (error) => {
                            console.error('Error loading indicators:', error);
                          },
                        });
                      });
                    }
                  }
                }
              }

              this.cdr.detectChanges();
            },
            error: (error) => {
              console.error('Error loading SDG targets:', error);
              this.loadingTargets.set(false);
              this.availableTargets.set([]);
            },
          });
        }
      } else if (skipValue) {
        // If user checks the skip option, clear targets and indicators
        console.log('Skip checkbox checked, clearing targets and indicators');
        this.selectedTargets.set(new Map());
        this.loadingIndicatorsForTargets.set(new Set());
      }
    });
  }

  /**
   * @description Load SDG data
   */
  private loadSDGs(): void {
    this.valuesService.getSDGs().subscribe({
      next: (data) => {
        this.sdgs.set(data);
        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Load UNOPS Missions from values service
   * Includes inactive missions to display previously selected missions that may now be inactive
   */
  private loadUNOPSMissions(): void {
    this.valuesService.getUNOPSMissions(true).subscribe({
      next: (data) => {
        this.unopsMissions.set(data);

        // Initialize selected missions from opportunity
        const opp = this.opportunity();
        if (opp.unopsMissions) {
          const selectedIds = new Set(
            opp.unopsMissions.map((m) => m.unopsMissionId),
          );
          this.selectedUNOPSMissions.set(selectedIds);
        }

        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Toggle UNOPS Mission selection
   */
  toggleUNOPSMission(missionId: number): void {
    const selected = new Set(this.selectedUNOPSMissions());
    if (selected.has(missionId)) {
      selected.delete(missionId);
    } else {
      selected.add(missionId);
      // If adding a mission, uncheck "not applicable"
      this.unopsMissionsNotApplicable.set(false);
    }
    this.selectedUNOPSMissions.set(selected);
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Check if UNOPS Mission is selected
   */
  isUNOPSMissionSelected(missionId: number): boolean {
    return this.selectedUNOPSMissions().has(missionId);
  }

  /**
   * @description Remove a UNOPS Mission from selection (used for inactive missions)
   */
  removeUNOPSMission(missionId: number): void {
    const selected = new Set(this.selectedUNOPSMissions());
    selected.delete(missionId);
    this.selectedUNOPSMissions.set(selected);
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Get count of selected UNOPS Missions
   */
  unopsMissionCount = computed(() => {
    return this.selectedUNOPSMissions().size;
  });

  /**
   * @description Get selected UNOPS Missions for display (in view and edit modes)
   */
  displayedUNOPSMissions = computed(() => {
    const selectedIds = this.selectedUNOPSMissions();
    const allMissions = this.unopsMissions();

    return allMissions.filter((mission) => selectedIds.has(mission.id));
  });

  /**
   * @description Get only active UNOPS Missions for selection in dialog
   * Excludes inactive missions to prevent users from selecting them
   */
  activeMissionsForDialog = computed(() => {
    return this.unopsMissions().filter((mission) => mission.status === 'Active');
  });

  /**
   * @description Check if UNOPS Missions dialog is valid (at least one mission selected OR not applicable checked)
   */
  isUNOPSMissionsDialogValid = computed(() => {
    return this.unopsMissionCount() > 0 || this.unopsMissionsNotApplicable();
  });

  /**
   * @description Toggle "Not Applicable" for UNOPS Missions
   */
  toggleUNOPSNotApplicable(checked: boolean): void {
    this.unopsMissionsNotApplicable.set(checked);
    if (checked) {
      // Clear all selected missions when "not applicable" is checked
      this.selectedUNOPSMissions.set(new Set());
    }
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Open UNOPS Missions dialog
   */
  openUNOPSMissionsDialog(): void {
    // Store current selections before opening dialog (for restore on cancel)
    this.preDialogUNOPSMissions = new Set(this.selectedUNOPSMissions());
    this.showUNOPSMissionsDialog.set(true);
  }

  /**
   * @description Confirm UNOPS Missions dialog selections (doesn't save to database, just closes dialog)
   */
  saveUNOPSMissionsDialog(): void {
    // Selections are already tracked in selectedUNOPSMissions signal
    // Just close the dialog - actual save happens when user saves the WHY section
    this.showUNOPSMissionsDialog.set(false);
    this.preDialogUNOPSMissions = null;
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel UNOPS Missions dialog
   */
  cancelUNOPSMissionsDialog(): void {
    // Restore selections to what they were before the dialog was opened
    if (this.preDialogUNOPSMissions !== null) {
      this.selectedUNOPSMissions.set(new Set(this.preDialogUNOPSMissions));
    }

    // Reset "not applicable" flag - don't auto-check
    this.unopsMissionsNotApplicable.set(false);

    this.showUNOPSMissionsDialog.set(false);
    this.preDialogUNOPSMissions = null;
    this.cdr.detectChanges();
  }

  /**
   * @description Start editing the section
   */
  startEditing(): void {
    const opp = this.opportunity();

    // Backup original data for cancel
    this.originalData = {
      expectedBeneficiaries: opp.expectedBeneficiaries ?? null,
      expectedImpact: opp.expectedImpact ?? null,
      expectedOutcomes: opp.expectedOutcomes ?? null,
      challenges: opp.challenges ?? null,
      sdGs: opp.sdGs ? [...opp.sdGs] : [],
      uncfOutcomes: opp.uncfOutcomes ? [...opp.uncfOutcomes] : [],
      unopsMissions: opp.unopsMissions ? [...opp.unopsMissions] : [],
    };

    // Set form controls
    this.expectedBeneficiariesControl.setValue(
      opp.expectedBeneficiaries ?? null,
    );
    this.estimatedDirectBeneficiariesControl.setValue(
      opp.estimatedDirectBeneficiaries ?? null,
    );
    this.estimatedIndirectBeneficiariesControl.setValue(
      opp.estimatedIndirectBeneficiaries ?? null,
    );
    this.beneficiariesToBeDeterminedControl.setValue(
      opp.beneficiariesToBeDetermined ?? false,
    );

    // If beneficiariesToBeDetermined is true, disable the number fields
    if (opp.beneficiariesToBeDetermined) {
      this.estimatedDirectBeneficiariesControl.disable();
      this.estimatedIndirectBeneficiariesControl.disable();
    } else {
      this.estimatedDirectBeneficiariesControl.enable();
      this.estimatedIndirectBeneficiariesControl.enable();
    }

    this.expectedImpactControl.setValue(opp.expectedImpact ?? null);
    this.expectedOutcomesControl.setValue(opp.expectedOutcomes ?? null);
    this.challengesControl.setValue(opp.challenges ?? null);

    // Initialize climate and framework alignments from countries
    const frameworkAlignments = new Map<number, boolean | null>();
    const ndcAlignments = new Map<number, boolean | null>();
    const napAlignments = new Map<number, boolean | null>();
    const orgUnitStrategyAlignments = new Map<number, boolean | null>();

    opp.countries?.forEach((country) => {
      frameworkAlignments.set(
        country.countryId,
        country.humanitarianFrameworkAlignment ?? null,
      );
      ndcAlignments.set(country.countryId, country.ndcAlignment ?? null);
      napAlignments.set(country.countryId, country.napAlignment ?? null);
      orgUnitStrategyAlignments.set(
        country.countryId,
        country.orgUnitStrategyAlignment ?? null,
      );
    });

    this.humanitarianFrameworkAlignments.set(frameworkAlignments);
    this.ndcAlignments.set(ndcAlignments);
    this.napAlignments.set(napAlignments);
    this.orgUnitStrategyAlignments.set(orgUnitStrategyAlignments);

    // Initialize selected UNOPS Missions
    if (opp.unopsMissions) {
      const selectedIds = new Set(
        opp.unopsMissions.map((m) => m.unopsMissionId),
      );
      this.selectedUNOPSMissions.set(selectedIds);
    }
    
    // Initialize "not applicable" state - always false so checkbox is not auto-selected
    this.unopsMissionsNotApplicable.set(false);

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
   * @description Save section changes (WHY data and framework alignments)
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    const whyData = {
      expectedBeneficiaries:
        this.expectedBeneficiariesControl.value ?? undefined,
      estimatedDirectBeneficiaries:
        this.estimatedDirectBeneficiariesControl.value ?? undefined,
      estimatedIndirectBeneficiaries:
        this.estimatedIndirectBeneficiariesControl.value ?? undefined,
      beneficiariesToBeDetermined:
        this.beneficiariesToBeDeterminedControl.value ?? false,
      expectedImpact: this.expectedImpactControl.value ?? undefined,
      expectedOutcomes: this.expectedOutcomesControl.value ?? undefined,
      challenges: this.challengesControl.value ?? undefined,
      sdGs: opp.sdGs?.map((sdg) => ({
        sdgId: sdg.sdgDatabaseId || 0, // Use the integer database ID
        isPrimary: sdg.isPrimary,
        skipTargetsAndIndicators: sdg.skipTargetsAndIndicators,
        notes: sdg.notes,
        targets: sdg.skipTargetsAndIndicators
          ? []
          : sdg.targets?.map((target) => ({
              sdgTargetDatabaseId: target.sdgTargetDatabaseId, // Correct property name for backend
              notes: target.notes,
              sdgIndicatorDatabaseIds:
                target.indicators?.map(
                  (indicator) => indicator.sdgIndicatorDatabaseId,
                ) || [], // Flat array of indicator IDs
            })) || [],
      })),
      uncfOutcomes: opp.uncfOutcomes?.map((uncfOutcome) => ({
        opportunityCountryId: uncfOutcome.opportunityCountryId,
        uncfOutcomeId: uncfOutcome.uncfOutcomeId, // Use the integer database ID
        notes: uncfOutcome.notes,
        uncfIndicatorIds:
          uncfOutcome.indicators?.map(
            (indicator) => indicator.uncfIndicatorId,
          ) || [], // Flat array of indicator IDs
      })),
      unopsMissions: Array.from(this.selectedUNOPSMissions()).map(
        (missionId) => ({
          unopsMissionId: missionId,
        }),
      ),
    };

    // Prepare WHERE data with updated framework alignments
    // Framework alignments are displayed in WHY section but are properties of OpportunityCountry
    const whereData = {
      countries:
        opp.countries?.map((country) => ({
          countryId: country.countryId,
          specificAreas: country.specificAreas,
          humanitarianFrameworkAlignment:
            this.humanitarianFrameworkAlignments().get(country.countryId) ??
            null,
          ndcAlignment: this.ndcAlignments().get(country.countryId) ?? null,
          napAlignment: this.napAlignments().get(country.countryId) ?? null,
          orgUnitStrategyAlignment:
            this.orgUnitStrategyAlignments().get(country.countryId) ?? null,
        })) || [],
    };

    this.isSaving.set(true);

    // Update WHY section first, then WHERE section for framework alignments
    this.opportunityService.updateOpportunityWhy(opp.id, whyData).subscribe({
      next: (updatedAfterWhy: Opportunity) => {
        // Now update WHERE section with framework alignments
        this.opportunityService
          .updateOpportunityWhere(opp.id, whereData)
          .subscribe({
            next: (fullUpdatedOpportunity: Opportunity) => {
              this.isSaving.set(false);
              this.isEditing.set(false);
              this.hasUnsavedChanges = false;
              this.originalData = null;

              // Emit full updated opportunity to parent
              this.opportunityUpdated.emit(fullUpdatedOpportunity);

              // Clear unsaved changes tracking
              this.changesSavedOrDiscarded.emit();

              this.feedbackService.showSuccessToast({
                detail: this.translateService.instant(
                  'message.opportunity.updatedSuccessfully',
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
      // Reset form controls to original values
      this.expectedBeneficiariesControl.setValue(
        this.originalData.expectedBeneficiaries ?? null,
      );
      this.expectedImpactControl.setValue(
        this.originalData.expectedImpact ?? null,
      );
      this.expectedOutcomesControl.setValue(
        this.originalData.expectedOutcomes ?? null,
      );
      this.challengesControl.setValue(this.originalData.challenges ?? null);

      // Restore original SDGs, UNCF Outcomes, and UNOPS Missions (reverts any that were added but not saved)
      const updatedOpportunity = {
        ...opp,
        expectedBeneficiaries: this.originalData.expectedBeneficiaries ?? null,
        expectedImpact: this.originalData.expectedImpact ?? null,
        expectedOutcomes: this.originalData.expectedOutcomes ?? null,
        challenges: this.originalData.challenges ?? null,
        sdGs: this.originalData.sdGs ? [...this.originalData.sdGs] : [],
        uncfOutcomes: this.originalData.uncfOutcomes
          ? [...this.originalData.uncfOutcomes]
          : [],
        unopsMissions: this.originalData.unopsMissions
          ? [...this.originalData.unopsMissions]
          : [],
      };

      // Emit the reverted opportunity to parent
      this.opportunityUpdated.emit(updatedOpportunity);
    } else {
      // Fallback: just reset form controls to current opportunity values
      this.expectedBeneficiariesControl.setValue(
        opp.expectedBeneficiaries ?? null,
      );
      this.expectedImpactControl.setValue(opp.expectedImpact ?? null);
      this.expectedOutcomesControl.setValue(opp.expectedOutcomes ?? null);
      this.challengesControl.setValue(opp.challenges ?? null);
    }

    this.estimatedDirectBeneficiariesControl.setValue(
      opp.estimatedDirectBeneficiaries ?? null,
    );
    this.estimatedIndirectBeneficiariesControl.setValue(
      opp.estimatedIndirectBeneficiaries ?? null,
    );
    this.beneficiariesToBeDeterminedControl.setValue(
      opp.beneficiariesToBeDetermined ?? false,
    );

    // Reset disabled state based on original value
    if (opp.beneficiariesToBeDetermined) {
      this.estimatedDirectBeneficiariesControl.disable();
      this.estimatedIndirectBeneficiariesControl.disable();
    } else {
      this.estimatedDirectBeneficiariesControl.enable();
      this.estimatedIndirectBeneficiariesControl.enable();
    }

    this.isEditing.set(false);
    this.originalData = null;
    this.hasUnsavedChanges = false;

    // Clear unsaved changes tracking
    this.changesSavedOrDiscarded.emit();

    // Reset climate and framework alignments to original values
    const frameworkAlignments = new Map<number, boolean | null>();
    const ndcAlignments = new Map<number, boolean | null>();
    const napAlignments = new Map<number, boolean | null>();
    const orgUnitStrategyAlignments = new Map<number, boolean | null>();

    opp.countries?.forEach((country) => {
      frameworkAlignments.set(
        country.countryId,
        country.humanitarianFrameworkAlignment ?? null,
      );
      ndcAlignments.set(country.countryId, country.ndcAlignment ?? null);
      napAlignments.set(country.countryId, country.napAlignment ?? null);
      orgUnitStrategyAlignments.set(
        country.countryId,
        country.orgUnitStrategyAlignment ?? null,
      );
    });

    this.humanitarianFrameworkAlignments.set(frameworkAlignments);
    this.ndcAlignments.set(ndcAlignments);
    this.napAlignments.set(napAlignments);
    this.orgUnitStrategyAlignments.set(orgUnitStrategyAlignments);

    // Reset UNOPS Missions to original values
    if (opp.unopsMissions) {
      const selectedIds = new Set(
        opp.unopsMissions.map((m) => m.unopsMissionId),
      );
      this.selectedUNOPSMissions.set(selectedIds);
    } else {
      this.selectedUNOPSMissions.set(new Set());
    }
    
    // Reset "not applicable" flag
    this.unopsMissionsNotApplicable.set(false);

    this.cdr.detectChanges();
  }

  /**
   * @description Open SDG dialog for adding new SDG(s)
   * Pre-loads existing SDGs for the two-step flow
   */
  openSDGDialog(): void {
    // Reset to step 1
    this.sdgDialogStep.set(1);
    
    // Pre-load existing SDGs from opportunity
    const opp = this.opportunity();
    const existingSDGs = opp.sdGs || [];
    const existingSDGIds = new Set(existingSDGs.map(s => s.sdgId));
    this.selectedSDGIds.set(existingSDGIds);
    
    // Get SDG objects for selected IDs
    const allSDGs = this.sdgs();
    const selectedSDGs = allSDGs.filter(s => existingSDGIds.has(s.sdgId || ''));
    this.selectedSDGsForStep1.set(selectedSDGs);
    
    // Initialize Main/Cross-cutting selections from existing SDGs (retain existing selections)
    const primarySecondaryMap = new Map<string, { isPrimary: boolean | null }>();
    existingSDGs.forEach(existingSDG => {
      // Retain the existing Main/Cross-cutting selection
      primarySecondaryMap.set(existingSDG.sdgId, { isPrimary: existingSDG.isPrimary });
    });
    this.sdgPrimarySecondaryInStep1.set(primarySecondaryMap);
    
    // Initialize targets/indicators data for existing SDGs (for step 2)
    const targetsMap = new Map<string, {
      targets: Map<number, Set<number>>;
      skipTargets: boolean;
      isPrimary: boolean;
      availableTargets: SDGTarget[];
      availableIndicators: SDGIndicator[];
      loadingTargets: boolean;
      loadingIndicatorsForTargets: Set<number>;
    }>();
    
    existingSDGs.forEach(existingSDG => {
      const targets = new Map<number, Set<number>>();
      if (existingSDG.targets && existingSDG.targets.length > 0) {
        existingSDG.targets.forEach(target => {
          const indicatorIds = new Set<number>();
          target.indicators?.forEach(ind => {
            indicatorIds.add(ind.sdgIndicatorDatabaseId);
          });
          targets.set(target.sdgTargetDatabaseId, indicatorIds);
        });
      }
      
      targetsMap.set(existingSDG.sdgId, {
        targets,
        skipTargets: false, // Targets and indicators are now optional, no skip option needed
        isPrimary: existingSDG.isPrimary,
        availableTargets: [],
        availableIndicators: [],
        loadingTargets: false,
        loadingIndicatorsForTargets: new Set(),
      });
    });
    
    this.sdgTargetsAndIndicators.set(targetsMap);
    
    // Legacy state reset (kept for backward compatibility)
    this.isEditingSDG.set(false);
    this.editingSDGIndex.set(null);
    this.editingFromPending.set(false);
    this.editingPendingIndex.set(null);
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.skipTargetsControl.setValue(false);
    this.showValidationError.set(false);
    this.pendingSDGSelections.set(existingSDGs);
    this.selectedTargets.set(new Map());
    this.availableTargets.set([]);
    this.availableIndicators.set([]);
    this.loadingIndicatorsForTargets.set(new Set());
    this.showSDGDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Toggle SDG selection in step 1
   */
  toggleSDGSelection(sdg: SDG): void {
    const selectedIds = new Set(this.selectedSDGIds());
    const sdgId = sdg.sdgId || '';
    const isNASDG = sdgId === 'N/A';
    
    if (selectedIds.has(sdgId)) {
      // Deselecting
      selectedIds.delete(sdgId);
      this.selectedSDGIds.set(selectedIds);
      
      // Update selected SDG objects
      const allSDGs = this.sdgs();
      const selectedSDGs = allSDGs.filter(s => selectedIds.has(s.sdgId || ''));
      this.selectedSDGsForStep1.set(selectedSDGs);
      
      // Remove from Main/Cross-cutting map
      const primarySecondaryMap = new Map(this.sdgPrimarySecondaryInStep1());
      primarySecondaryMap.delete(sdgId);
      this.sdgPrimarySecondaryInStep1.set(primarySecondaryMap);
      
      this.cdr.detectChanges();
      return;
    } else {
      // Selecting
      // Special handling for N/A SDG
      if (isNASDG) {
        // If there are other SDGs selected, show confirmation
        if (selectedIds.size > 0) {
          this.feedbackService.showConfirmDialog(
            {
              summary: this.translateService.instant('confirmation.clearAllSDGs'),
              detail: this.translateService.instant('message.opportunity.addingNASDGWillClearOthers'),
            },
            () => {
              // User confirmed - clear all and add N/A
              selectedIds.clear();
              selectedIds.add(sdgId);
              this.selectedSDGIds.set(selectedIds);
              
              const allSDGs = this.sdgs();
              const selectedSDGs = allSDGs.filter(s => selectedIds.has(s.sdgId || ''));
              this.selectedSDGsForStep1.set(selectedSDGs);
              
              // Set N/A as Main (always)
              const primarySecondaryMap = new Map<string, { isPrimary: boolean | null }>();
              primarySecondaryMap.set(sdgId, { isPrimary: true });
              this.sdgPrimarySecondaryInStep1.set(primarySecondaryMap);
              
              this.cdr.detectChanges();
            }
          );
          return;
        } else {
          // No other SDGs, just add N/A
          selectedIds.add(sdgId);
          this.selectedSDGIds.set(selectedIds);
          
          const allSDGs = this.sdgs();
          const selectedSDGs = allSDGs.filter(s => selectedIds.has(s.sdgId || ''));
          this.selectedSDGsForStep1.set(selectedSDGs);
          
          // Set N/A as Main (always)
          const primarySecondaryMap = new Map(this.sdgPrimarySecondaryInStep1());
          primarySecondaryMap.set(sdgId, { isPrimary: true });
          this.sdgPrimarySecondaryInStep1.set(primarySecondaryMap);
          
          this.cdr.detectChanges();
          return;
        }
      } else {
        // Regular SDG - check if N/A is selected and remove it
        if (selectedIds.has('N/A')) {
          this.feedbackService.showInfoToast({
            detail: this.translateService.instant('message.opportunity.naSDGRemovedWhenAddingOthers'),
            summary: this.translateService.instant('message.info'),
          });
          selectedIds.delete('N/A');
          
          // Remove N/A from primary/secondary map
          const primarySecondaryMap = new Map(this.sdgPrimarySecondaryInStep1());
          primarySecondaryMap.delete('N/A');
          this.sdgPrimarySecondaryInStep1.set(primarySecondaryMap);
        }
        
        selectedIds.add(sdgId);
        this.selectedSDGIds.set(selectedIds);
        
        const allSDGs = this.sdgs();
        const selectedSDGs = allSDGs.filter(s => selectedIds.has(s.sdgId || ''));
        this.selectedSDGsForStep1.set(selectedSDGs);
        
        // Initialize primary/secondary as null (user must select)
        const primarySecondaryMap = new Map(this.sdgPrimarySecondaryInStep1());
        primarySecondaryMap.set(sdgId, { isPrimary: null });
        this.sdgPrimarySecondaryInStep1.set(primarySecondaryMap);
      }
    }
    
    this.cdr.detectChanges();
  }

  /**
   * @description Toggle Main/Cross-cutting for an SDG in step 1
   */
  togglePrimarySecondaryInStep1(sdgId: string, isPrimary: boolean): void {
    const primarySecondaryMap = new Map(this.sdgPrimarySecondaryInStep1());
    
    // If setting as Main, unset all others
    if (isPrimary) {
      primarySecondaryMap.forEach((value, key) => {
        if (key !== sdgId && value.isPrimary === true) {
          primarySecondaryMap.set(key, { isPrimary: false });
        }
      });
    }
    
    primarySecondaryMap.set(sdgId, { isPrimary });
    this.sdgPrimarySecondaryInStep1.set(primarySecondaryMap);
    this.cdr.detectChanges();
  }

  /**
   * @description Get Primary/Secondary selection for an SDG in step 1
   */
  getPrimarySecondaryInStep1(sdgId: string): boolean | null {
    return this.sdgPrimarySecondaryInStep1().get(sdgId)?.isPrimary ?? null;
  }

  /**
   * @description Check if all selected SDGs have Main/Cross-cutting selected
   */
  readonly allSDGsHavePrimarySecondary = computed(() => {
    const selectedIds = this.selectedSDGIds();
    const primarySecondaryMap = this.sdgPrimarySecondaryInStep1();
    
    for (const sdgId of selectedIds) {
      const selection = primarySecondaryMap.get(sdgId);
      if (!selection || selection.isPrimary === null) {
        return false;
      }
    }
    return true;
  });

  /**
   * @description Check if an SDG is selected in step 1
   */
  isSDGSelected(sdgId: string): boolean {
    return this.selectedSDGIds().has(sdgId);
  }

  /**
   * @description Get count of selected SDGs in step 1
   */
  readonly selectedSDGCount = computed(() => this.selectedSDGIds().size);

  /**
   * @description Proceed to step 2 (Targets and Indicators selection)
   */
  proceedToStep2(): void {
    const selectedIds = this.selectedSDGIds();
    if (selectedIds.size === 0) {
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: this.translateService.instant('message.validation.atLeastOneSDGRequired'),
      });
      return;
    }

    // Validate that all SDGs have Main/Cross-cutting selected
    if (!this.allSDGsHavePrimarySecondary()) {
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: this.translateService.instant('message.validation.allSDGsMustHavePrimarySecondary'),
      });
      return;
    }

    // Validate that at least one SDG is marked as Main (Primary)
    const primarySecondaryMap = this.sdgPrimarySecondaryInStep1();
    let hasMainSDG = false;
    for (const sdgId of selectedIds) {
      const selection = primarySecondaryMap.get(sdgId);
      if (selection && selection.isPrimary === true) {
        hasMainSDG = true;
        break;
      }
    }
    
    if (!hasMainSDG) {
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: this.translateService.instant('message.validation.atLeastOneMainSDGRequired'),
      });
      return;
    }

    // Initialize targets/indicators data for each selected SDG
    const targetsMap = new Map<string, {
      targets: Map<number, Set<number>>;
      skipTargets: boolean;
      isPrimary: boolean;
      availableTargets: SDGTarget[];
      availableIndicators: SDGIndicator[];
      loadingTargets: boolean;
      loadingIndicatorsForTargets: Set<number>;
    }>();

    const selectedSDGs = this.selectedSDGsForStep1();
    const opp = this.opportunity();
    
    // Expand the first SDG panel by default
    if (selectedSDGs.length > 0) {
      this.expandedSDGIdInStep2.set(selectedSDGs[0].sdgId || null);
    }
    const existingSDGs = opp.sdGs || [];

    selectedSDGs.forEach((sdg) => {
      // Check if this SDG already exists in the opportunity
      const existingSDG = existingSDGs.find(s => s.sdgId === sdg.sdgId);
      const isNASDG = sdg.sdgId === 'N/A';
      
      // Get Main/Cross-cutting from step 1 selection (N/A is always Main)
      const step1Selection = primarySecondaryMap.get(sdg.sdgId || '');
      const isPrimary = isNASDG ? true : (step1Selection?.isPrimary ?? false);

      // Initialize with existing data if available
      const existingTargets = existingSDG?.targets || [];
      const targets = new Map<number, Set<number>>();
      
      if (existingTargets.length > 0) {
        existingTargets.forEach(target => {
          const indicatorIds = new Set<number>();
          target.indicators?.forEach(ind => {
            indicatorIds.add(ind.sdgIndicatorDatabaseId);
          });
          targets.set(target.sdgTargetDatabaseId, indicatorIds);
        });
      }

      targetsMap.set(sdg.sdgId || '', {
        targets,
        skipTargets: false, // Targets and indicators are now optional, no skip option needed
        isPrimary,
        availableTargets: [],
        availableIndicators: [],
        loadingTargets: false,
        loadingIndicatorsForTargets: new Set(),
      });
    });

    this.sdgTargetsAndIndicators.set(targetsMap);
    this.sdgDialogStep.set(2);
    
    // Load targets for all selected SDGs (except N/A)
    const loadPromises = selectedSDGs
      .filter(sdg => sdg.sdgId && sdg.sdgId !== 'N/A')
      .map(sdg => this.loadTargetsForSDGInStep2(sdg.sdgId || ''));
    
    // After all targets are loaded, load indicators for existing targets
    Promise.all(loadPromises).then(() => {
      selectedSDGs.forEach(sdg => {
        if (sdg.sdgId && sdg.sdgId !== 'N/A') {
          const currentData = this.sdgTargetsAndIndicators();
          const sdgData = currentData.get(sdg.sdgId || '');
          if (sdgData && sdgData.targets.size > 0) {
            // Load indicators for all existing targets
            sdgData.targets.forEach((indicatorIds, targetDatabaseId) => {
              const target = sdgData.availableTargets.find(t => t.id === targetDatabaseId);
              if (target) {
                this.loadIndicatorsForTargetInStep2(sdg.sdgId || '', target);
              }
            });
          }
        }
      });
    });
    
    this.cdr.detectChanges();
  }

  /**
   * @description Load targets for an SDG in step 2
   * @returns Promise that resolves when targets are loaded
   */
  loadTargetsForSDGInStep2(sdgId: string): Promise<void> {
    const data = this.sdgTargetsAndIndicators();
    const sdgData = data.get(sdgId);
    if (!sdgData) {
      return Promise.resolve();
    }

    // Set loading state
    const updatedData = new Map(data);
    updatedData.set(sdgId, {
      ...sdgData,
      loadingTargets: true,
    });
    this.sdgTargetsAndIndicators.set(updatedData);

    return new Promise((resolve, reject) => {
      this.valuesService.getSDGTargets(sdgId).subscribe({
        next: (targets) => {
          const currentData = this.sdgTargetsAndIndicators();
          const currentSDGData = currentData.get(sdgId);
          if (!currentSDGData) {
            resolve();
            return;
          }

          const updated = new Map(currentData);
          updated.set(sdgId, {
            ...currentSDGData,
            availableTargets: targets,
            loadingTargets: false,
          });
          this.sdgTargetsAndIndicators.set(updated);
          this.cdr.detectChanges();
          resolve();
        },
        error: (error) => {
          console.error('Error loading SDG targets:', error);
          const currentData = this.sdgTargetsAndIndicators();
          const currentSDGData = currentData.get(sdgId);
          if (!currentSDGData) {
            resolve();
            return;
          }

          const updated = new Map(currentData);
          updated.set(sdgId, {
            ...currentSDGData,
            availableTargets: [],
            loadingTargets: false,
          });
          this.sdgTargetsAndIndicators.set(updated);
          this.cdr.detectChanges();
          resolve(); // Resolve even on error to continue flow
        },
      });
    });
  }

  /**
   * @description Load indicators for a target in step 2 (helper method)
   */
  private loadIndicatorsForTargetInStep2(sdgId: string, target: SDGTarget): void {
    const data = this.sdgTargetsAndIndicators();
    const sdgData = data.get(sdgId);
    if (!sdgData) return;

    // Mark as loading
    const loadingSet = new Set(sdgData.loadingIndicatorsForTargets);
    loadingSet.add(target.id);
    
    const updated = new Map(data);
    updated.set(sdgId, {
      ...sdgData,
      loadingIndicatorsForTargets: loadingSet,
    });
    this.sdgTargetsAndIndicators.set(updated);

    // Load indicators
    this.valuesService.getSDGIndicators(target.sdgTargetId).subscribe({
      next: (indicators) => {
        const currentData = this.sdgTargetsAndIndicators();
        const currentSDGData = currentData.get(sdgId);
        if (!currentSDGData) return;

        // Add indicators to available list
        const existingIndicators = currentSDGData.availableIndicators;
        const combined = [...existingIndicators, ...indicators];
        const unique = combined.filter(
          (ind, index, self) => index === self.findIndex(i => i.id === ind.id)
        );

        // Remove from loading set
        const loadingSet = new Set(currentSDGData.loadingIndicatorsForTargets);
        loadingSet.delete(target.id);

        const updated = new Map(currentData);
        updated.set(sdgId, {
          ...currentSDGData,
          availableIndicators: unique,
          loadingIndicatorsForTargets: loadingSet,
        });
        this.sdgTargetsAndIndicators.set(updated);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading indicators:', error);
        const currentData = this.sdgTargetsAndIndicators();
        const currentSDGData = currentData.get(sdgId);
        if (!currentSDGData) return;

        const loadingSet = new Set(currentSDGData.loadingIndicatorsForTargets);
        loadingSet.delete(target.id);

        const updated = new Map(currentData);
        updated.set(sdgId, {
          ...currentSDGData,
          loadingIndicatorsForTargets: loadingSet,
        });
        this.sdgTargetsAndIndicators.set(updated);
        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Toggle target selection in step 2
   */
  toggleTargetInStep2(sdgId: string, target: SDGTarget): void {
    const data = this.sdgTargetsAndIndicators();
    const sdgData = data.get(sdgId);
    if (!sdgData) return;

    const targets = new Map(sdgData.targets);
    
    if (targets.has(target.id)) {
      // Remove target and its indicators
      targets.delete(target.id);
    } else {
      // Add target with empty indicator set
      targets.set(target.id, new Set());
      
      // Mark as loading and load indicators
      const loadingSet = new Set(sdgData.loadingIndicatorsForTargets);
      loadingSet.add(target.id);
      
      const updated = new Map(data);
      updated.set(sdgId, {
        ...sdgData,
        targets,
        loadingIndicatorsForTargets: loadingSet,
      });
      this.sdgTargetsAndIndicators.set(updated);

      // Load indicators for this target
      this.valuesService.getSDGIndicators(target.sdgTargetId).subscribe({
        next: (indicators) => {
          const currentData = this.sdgTargetsAndIndicators();
          const currentSDGData = currentData.get(sdgId);
          if (!currentSDGData) return;

          // Add indicators to available list
          const existingIndicators = currentSDGData.availableIndicators;
          const combined = [...existingIndicators, ...indicators];
          const unique = combined.filter(
            (ind, index, self) => index === self.findIndex(i => i.id === ind.id)
          );

          // Remove from loading set
          const loadingSet = new Set(currentSDGData.loadingIndicatorsForTargets);
          loadingSet.delete(target.id);

          const updated = new Map(currentData);
          updated.set(sdgId, {
            ...currentSDGData,
            availableIndicators: unique,
            loadingIndicatorsForTargets: loadingSet,
          });
          this.sdgTargetsAndIndicators.set(updated);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading indicators:', error);
          const currentData = this.sdgTargetsAndIndicators();
          const currentSDGData = currentData.get(sdgId);
          if (!currentSDGData) return;

          const loadingSet = new Set(currentSDGData.loadingIndicatorsForTargets);
          loadingSet.delete(target.id);

          const updated = new Map(currentData);
          updated.set(sdgId, {
            ...currentSDGData,
            loadingIndicatorsForTargets: loadingSet,
          });
          this.sdgTargetsAndIndicators.set(updated);
          this.cdr.detectChanges();
        },
      });
      return;
    }

    // Update without loading indicators
    const updated = new Map(data);
    updated.set(sdgId, {
      ...sdgData,
      targets,
    });
    this.sdgTargetsAndIndicators.set(updated);
    this.cdr.detectChanges();
  }

  /**
   * @description Toggle indicator selection in step 2
   */
  toggleIndicatorInStep2(sdgId: string, targetId: number, indicatorId: number): void {
    const data = this.sdgTargetsAndIndicators();
    const sdgData = data.get(sdgId);
    if (!sdgData) return;

    const targets = new Map(sdgData.targets);
    const indicatorIds = targets.get(targetId);
    
    if (indicatorIds) {
      if (indicatorIds.has(indicatorId)) {
        indicatorIds.delete(indicatorId);
      } else {
        indicatorIds.add(indicatorId);
      }
      targets.set(targetId, indicatorIds);
    }

    const updated = new Map(data);
    updated.set(sdgId, {
      ...sdgData,
      targets,
    });
    this.sdgTargetsAndIndicators.set(updated);
    this.cdr.detectChanges();
  }

  /**
   * @description Check if target is selected in step 2
   */
  isTargetSelectedInStep2(sdgId: string, targetId: number): boolean {
    const data = this.sdgTargetsAndIndicators();
    const sdgData = data.get(sdgId);
    return sdgData?.targets.has(targetId) || false;
  }

  /**
   * @description Check if indicator is selected in step 2
   */
  isIndicatorSelectedInStep2(sdgId: string, targetId: number, indicatorId: number): boolean {
    const data = this.sdgTargetsAndIndicators();
    const sdgData = data.get(sdgId);
    const indicatorIds = sdgData?.targets.get(targetId);
    return indicatorIds?.has(indicatorId) || false;
  }

  /**
   * @description Get indicators for a target in step 2
   */
  getIndicatorsForTargetInStep2(sdgId: string, targetId: string): SDGIndicator[] {
    const data = this.sdgTargetsAndIndicators();
    const sdgData = data.get(sdgId);
    if (!sdgData) return [];
    return sdgData.availableIndicators.filter(i => i.sdgTargetId === targetId);
  }


  /**
   * @description Toggle primary status for an SDG in step 2
   * @deprecated Primary/Secondary selection is now done in step 1
   */
  togglePrimaryInStep2(sdgId: string): void {
    // This method is no longer used - Primary/Secondary is selected in step 1
    // Kept for backward compatibility
  }

  /**
   * @description Go back to step 1
   */
  goBackToStep1(): void {
    this.sdgDialogStep.set(1);
    this.expandedSDGIdInStep2.set(null);
    this.cdr.detectChanges();
  }

  /**
   * @description Get the index of the expanded SDG panel for accordion
   */
  getExpandedSDGIndex(): number | null {
    const expandedId = this.expandedSDGIdInStep2();
    if (expandedId === null) {
      return null;
    }
    const selectedSDGs = this.selectedSDGsForStep1();
    const index = selectedSDGs.findIndex(sdg => sdg.sdgId === expandedId);
    return index >= 0 ? index : null;
  }

  /**
   * @description Handle accordion tab change (only one panel expanded at a time)
   */
  onAccordionTabChange(event: any): void {
    const selectedSDGs = this.selectedSDGsForStep1();
    if (event.index !== null && event.index >= 0 && event.index < selectedSDGs.length) {
      this.expandedSDGIdInStep2.set(selectedSDGs[event.index].sdgId || null);
    } else {
      this.expandedSDGIdInStep2.set(null);
    }
    this.cdr.detectChanges();
  }

  /**
   * @description Finalize and commit SDG selections from step 2 (or step 1 if skipping step 2)
   */
  finalizeSDGSelections(): void {
    const selectedSDGs = this.selectedSDGsForStep1();
    const selectedIds = this.selectedSDGIds();
    let targetsData = this.sdgTargetsAndIndicators();
    const opp = this.opportunity();
    const existingSDGs = opp.sdGs || [];
    const primarySecondaryMap = this.sdgPrimarySecondaryInStep1();

    // If called from Step 1, validate and initialize targets data
    if (this.sdgDialogStep() === 1) {
      // Validate that at least one SDG is selected
      if (selectedIds.size === 0) {
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: this.translateService.instant('message.validation.atLeastOneSDGRequired'),
        });
        return;
      }

      // Validate that all SDGs have Main/Cross-cutting selected
      if (!this.allSDGsHavePrimarySecondary()) {
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: this.translateService.instant('message.validation.allSDGsMustHavePrimarySecondary'),
        });
        return;
      }

      // Validate that at least one SDG is marked as Main
      let hasMainSDG = false;
      for (const sdgId of selectedIds) {
        const selection = primarySecondaryMap.get(sdgId);
        if (selection && selection.isPrimary === true) {
          hasMainSDG = true;
          break;
        }
      }
      
      if (!hasMainSDG) {
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: this.translateService.instant('message.validation.atLeastOneMainSDGRequired'),
        });
        return;
      }

      // Initialize targets/indicators data for each selected SDG (similar to proceedToStep2)
      const targetsMap = new Map<string, {
        targets: Map<number, Set<number>>;
        skipTargets: boolean;
        isPrimary: boolean;
        availableTargets: SDGTarget[];
        availableIndicators: SDGIndicator[];
        loadingTargets: boolean;
        loadingIndicatorsForTargets: Set<number>;
      }>();

      selectedSDGs.forEach((sdg) => {
        const existingSDG = existingSDGs.find(s => s.sdgId === sdg.sdgId);
        const isNASDG = sdg.sdgId === 'N/A';
        
        // Get Main/Cross-cutting from step 1 selection (N/A is always Main)
        const step1Selection = primarySecondaryMap.get(sdg.sdgId || '');
        const isPrimary = isNASDG ? true : (step1Selection?.isPrimary ?? false);

        // Initialize with existing data if available
        const existingTargets = existingSDG?.targets || [];
        const targets = new Map<number, Set<number>>();
        if (existingTargets.length > 0) {
          existingTargets.forEach(target => {
            const indicatorIds = new Set<number>();
            target.indicators?.forEach(ind => {
              indicatorIds.add(ind.sdgIndicatorDatabaseId);
            });
            targets.set(target.sdgTargetDatabaseId, indicatorIds);
          });
        }

        targetsMap.set(sdg.sdgId || '', {
          targets,
          skipTargets: false, // Targets and indicators are now optional, no skip option needed
          isPrimary,
          availableTargets: [],
          availableIndicators: [],
          loadingTargets: false,
          loadingIndicatorsForTargets: new Set(),
        });
      });

      this.sdgTargetsAndIndicators.set(targetsMap);
      targetsData = targetsMap;
    }

    // Build OpportunitySDG array
    const newSDGs: OpportunitySDG[] = selectedSDGs.map(sdg => {
      const sdgData = targetsData.get(sdg.sdgId || '');
      if (!sdgData) {
        // Fallback if data missing - get from primary/secondary map
        const step1Selection = primarySecondaryMap.get(sdg.sdgId || '');
        const isNASDG = sdg.sdgId === 'N/A';
        return {
          id: 0,
          opportunityId: opp.id!,
          sdgId: sdg.sdgId || '',
          sdgDatabaseId: sdg.id,
          sdgNumber: sdg.sdgNumber || '',
          sdgName: sdg.name,
          isPrimary: isNASDG ? true : (step1Selection?.isPrimary ?? false),
          skipTargetsAndIndicators: null,
          notes: null,
          targets: [],
        };
      }

      // Find existing SDG to preserve ID
      const existingSDG = existingSDGs.find(e => e.sdgId === sdg.sdgId);

      // Build targets array
      const targets: OpportunitySDGTarget[] = [];
      if (sdgData.targets.size > 0) {
        sdgData.targets.forEach((indicatorIds, targetDatabaseId) => {
          const targetInfo = sdgData.availableTargets.find(t => t.id === targetDatabaseId);
          if (targetInfo) {
            // Find existing target to preserve ID
            const existingTarget = existingSDG?.targets?.find(
              t => t.sdgTargetDatabaseId === targetDatabaseId
            );

            const indicators: OpportunitySDGIndicator[] = [];
            indicatorIds.forEach(indicatorId => {
              const indicatorInfo = sdgData.availableIndicators.find(i => i.id === indicatorId);
              if (indicatorInfo) {
                // Find existing indicator to preserve ID
                const existingIndicator = existingTarget?.indicators?.find(
                  ind => ind.sdgIndicatorDatabaseId === indicatorId
                );

                indicators.push({
                  id: existingIndicator?.id || 0,
                  opportunityId: opp.id!,
                  opportunitySDGTargetId: existingTarget?.id || 0,
                  sdgIndicatorDatabaseId: indicatorInfo.id,
                  sdgIndicatorId: indicatorInfo.sdgIndicatorId,
                  sdgIndicatorLongDescription: indicatorInfo.sdgIndicatorLongDescription,
                  notes: existingIndicator?.notes || null,
                });
              }
            });

            targets.push({
              id: existingTarget?.id || 0,
              opportunityId: opp.id!,
              opportunitySDGId: existingSDG?.id || 0,
              sdgTargetDatabaseId: targetInfo.id,
              sdgTargetId: targetInfo.sdgTargetId,
              targetDescription: targetInfo.targetDescription,
              targetType: targetInfo.targetType,
              notes: existingTarget?.notes || null,
              indicators: indicators,
            });
          }
        });
      }

      return {
        id: existingSDG?.id || 0, // Preserve existing ID
        opportunityId: opp.id!,
        sdgId: sdg.sdgId || '',
        sdgDatabaseId: sdg.id,
        sdgNumber: sdg.sdgNumber || '',
        sdgName: sdg.name,
        isPrimary: sdgData.isPrimary,
        skipTargetsAndIndicators: null, // No longer using skip option - targets are optional
        notes: existingSDG?.notes || null, // Preserve notes
        targets: targets,
      };
    });

    // Replace existing SDGs with new selections
    const updatedOpportunity = {
      ...opp,
      sdGs: newSDGs,
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Mark as changed
    this.markAsChanged();

    // Close dialog and reset
    this.showSDGDialog.set(false);
    this.sdgDialogStep.set(1);
    this.selectedSDGIds.set(new Set());
    this.selectedSDGsForStep1.set([]);
    this.sdgPrimarySecondaryInStep1.set(new Map());
    this.sdgTargetsAndIndicators.set(new Map());
    this.expandedSDGIdInStep2.set(null);

    this.feedbackService.showSuccessToast({
      summary: this.translateService.instant('message.success'),
      detail: this.translateService.instant('message.opportunity.sdgsUpdated'),
    });

    this.cdr.detectChanges();
  }

  /**
   * @description Handle SDG selection change
   */
  onSDGChange(sdg: SDG | null): void {
    if (sdg && sdg.sdgId) {
      // If "N/A" SDG is selected, automatically set as Primary and disable control
      if (sdg.sdgId === 'N/A') {
        this.isPrimaryControl.setValue(true);
        this.isPrimaryControl.disable(); // Disable the control to prevent changes
        // No need to load targets for N/A
        this.availableTargets.set([]);
        this.availableIndicators.set([]);
        this.cdr.detectChanges();
        return;
      } else {
        // For regular SDGs, ensure the control is enabled
        this.isPrimaryControl.enable();
        
        // Check if there's already a Primary SDG in pending selections
        const hasPrimary = this.hasPrimaryInPending();
        
        // If switching from N/A (which was Primary) to a regular SDG,
        // and there's already a Primary in pending, set to Secondary
        if (this.isPrimaryControl.value === true && hasPrimary) {
          // Check if the current Primary is N/A (which will be removed)
          const currentPending = this.pendingSDGSelections();
          const primarySDG = currentPending.find(s => s.isPrimary);
          
          // If the current Primary is NOT N/A, then we need to set this new SDG to Secondary
          if (primarySDG && primarySDG.sdgId !== 'N/A') {
            this.isPrimaryControl.setValue(false);
          }
        }
      }
      
      // Load available targets for the selected SDG
      this.loadingTargets.set(true);
      this.valuesService.getSDGTargets(sdg.sdgId).subscribe({
        next: (targets) => {
          this.loadingTargets.set(false);
          this.availableTargets.set(targets);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading SDG targets:', error);
          this.loadingTargets.set(false);
          this.availableTargets.set([]);
        },
      });
    } else {
      // No SDG selected, enable the control
      this.isPrimaryControl.enable();
      this.availableTargets.set([]);
      this.availableIndicators.set([]);
    }
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel SDG dialog and reset state
   */
  cancelSDGDialog(): void {
    this.showSDGDialog.set(false);
    this.sdgDialogStep.set(1);
    this.selectedSDGIds.set(new Set());
    this.selectedSDGsForStep1.set([]);
    this.sdgPrimarySecondaryInStep1.set(new Map());
    this.sdgTargetsAndIndicators.set(new Map());
    
    // Legacy state reset
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.skipTargetsControl.setValue(false);
    this.showValidationError.set(false);
    this.isEditingSDG.set(false);
    this.editingSDGIndex.set(null);
    this.editingFromPending.set(false);
    this.editingPendingIndex.set(null);
    this.pendingSDGSelections.set([]);
    this.availableTargets.set([]);
    this.availableIndicators.set([]);
    this.selectedTargets.set(new Map());
    this.loadingIndicatorsForTargets.set(new Set());
    this.cdr.detectChanges();
  }

  /**
   * @description Get SDG logo URL by SDG ID
   */
  getSDGLogo(sdgId: string): string | null {
    const sdg = this.sdgs().find((s) => s.sdgId === sdgId);
    return sdg?.sdgLogo || null;
  }

  /**
   * @description Toggle target selection and load indicators
   */
  toggleTarget(target: SDGTarget): void {
    const currentSelection = new Map(this.selectedTargets());

    if (currentSelection.has(target.id)) {
      // Remove target and its indicators
      currentSelection.delete(target.id);
      this.selectedTargets.set(currentSelection);
      this.cdr.detectChanges();
    } else {
      // Add target with empty indicator set
      currentSelection.set(target.id, new Set());
      this.selectedTargets.set(currentSelection);

      // Mark this target as loading
      const loadingSet = new Set(this.loadingIndicatorsForTargets());
      loadingSet.add(target.id);
      this.loadingIndicatorsForTargets.set(loadingSet);

      // Load indicators for this target
      console.log('🔍 Loading indicators for target:', target.sdgTargetId);
      console.log(
        '📡 API URL:',
        `/api/values/sdg-indicators?targetId=${target.sdgTargetId}`,
      );

      this.valuesService.getSDGIndicators(target.sdgTargetId).subscribe({
        next: (indicators) => {
          // Remove from loading set
          const loadingSet = new Set(this.loadingIndicatorsForTargets());
          loadingSet.delete(target.id);
          this.loadingIndicatorsForTargets.set(loadingSet);

          console.log('✅ Loaded indicators:', indicators);
          console.log(
            '📊 Indicator count:',
            indicators.length,
            'for target:',
            target.sdgTargetId,
          );

          if (indicators.length === 0) {
            console.warn(
              '⚠️ No indicators found for target:',
              target.sdgTargetId,
            );
            console.warn('⚠️ This could mean:');
            console.warn(
              '   1. The SDGIndicators table does not exist (migration not run)',
            );
            console.warn('   2. The SDGIndicatorSeeder has not been run');
            console.warn(
              '   3. No indicators exist for this target in the database',
            );
          }

          // Store or update available indicators (append to existing)
          const current = this.availableIndicators();
          const combined = [...current, ...indicators];
          // Remove duplicates
          const unique = combined.filter(
            (indicator, index, self) =>
              index === self.findIndex((i) => i.id === indicator.id),
          );
          console.log(
            '📈 Total available indicators after loading:',
            unique.length,
          );
          this.availableIndicators.set(unique);
          this.cdr.detectChanges();
        },
        error: (error) => {
          // Remove from loading set
          const loadingSet = new Set(this.loadingIndicatorsForTargets());
          loadingSet.delete(target.id);
          this.loadingIndicatorsForTargets.set(loadingSet);

          console.error(
            '❌ Error loading SDG indicators for target',
            target.sdgTargetId,
          );
          console.error('Error details:', error);
          console.error('Status:', error.status);
          console.error('Message:', error.message);

          if (error.status === 404) {
            console.error(
              '🔴 404 Error - API endpoint not found. Check if the backend is running.',
            );
          } else if (error.status === 500) {
            console.error(
              '🔴 500 Error - Server error. Check if the SDGIndicators table exists in the database.',
            );
          } else if (error.status === 0) {
            console.error(
              '🔴 Network Error - Cannot reach the backend. Check if the backend is running.',
            );
          }

          // Show error feedback to user
          this.feedbackService.showErrorToast({
            summary: 'Error Loading Indicators',
            detail: `Failed to load indicators for target ${target.sdgTargetId}. Please check the console for details.`,
          });

          this.cdr.detectChanges();
        },
      });
    }
  }

  /**
   * @description Toggle indicator selection for a target
   */
  toggleIndicator(targetId: number, indicatorId: number): void {
    const currentSelection = new Map(this.selectedTargets());

    if (currentSelection.has(targetId)) {
      const indicators = currentSelection.get(targetId)!;
      if (indicators.has(indicatorId)) {
        indicators.delete(indicatorId);
      } else {
        indicators.add(indicatorId);
      }
      currentSelection.set(targetId, indicators);
    }

    this.selectedTargets.set(currentSelection);
    this.cdr.detectChanges();
  }

  /**
   * @description Check if a target is selected
   */
  isTargetSelected(targetId: number): boolean {
    return this.selectedTargets().has(targetId);
  }

  /**
   * @description Check if indicators are loading for a target
   */
  isLoadingIndicators(targetId: number): boolean {
    return this.loadingIndicatorsForTargets().has(targetId);
  }

  /**
   * @description Check if an indicator is selected for a target
   */
  isIndicatorSelected(targetId: number, indicatorId: number): boolean {
    const target = this.selectedTargets().get(targetId);
    return target ? target.has(indicatorId) : false;
  }

  /**
   * @description Get indicators for a specific target
   */
  getIndicatorsForTarget(targetId: string): SDGIndicator[] {
    return this.availableIndicators().filter((i) => i.sdgTargetId === targetId);
  }

  /**
   * @description Add configured SDG to pending selections
   */
  addSDGToPendingSelection(): void {
    const sdg = this.sdgControl.value;
    const isPrimary = this.isPrimaryControl.value || false;
    const skipTargets = this.skipTargetsControl.value || false;

    // Validation
    if (!sdg) {
      this.showValidationError.set(true);
      return;
    }

    let currentPending = [...this.pendingSDGSelections()];
    const opp = this.opportunity();
    const isNASDG = sdg.sdgId === 'N/A';

    // Check if already in pending selections (includes both existing and newly added)
    if (currentPending.some((s) => s.sdgId === sdg.sdgId)) {
      this.feedbackService.showErrorToast({
        detail: this.translateService.instant(
          'message.opportunity.sdgAlreadyInSelection',
        ),
        summary: this.translateService.instant('message.error'),
      });
      return;
    }

    // SPECIAL HANDLING FOR "N/A" SDG
    if (isNASDG) {
      // If there are other SDGs in pending, show confirmation
      if (currentPending.length > 0) {
        this.feedbackService.showConfirmDialog(
          {
            summary: this.translateService.instant('confirmation.clearAllSDGs'),
            detail: this.translateService.instant('message.opportunity.addingNASDGWillClearOthers'),
          },
          () => {
            // User confirmed - clear all SDGs and add N/A as Primary
            this.addNASDG(sdg, opp);
          }
        );
        return;
      } else {
        // No other SDGs, just add N/A as Primary
        this.addNASDG(sdg, opp);
        return;
      }
    }

    // REGULAR SDG: Remove N/A if it exists
    const naIndex = currentPending.findIndex((s) => s.sdgId === 'N/A');
    if (naIndex !== -1) {
      currentPending.splice(naIndex, 1);
      // Show info message that N/A was removed
      this.feedbackService.showInfoToast({
        detail: this.translateService.instant('message.opportunity.naSDGRemovedWhenAddingOthers'),
        summary: this.translateService.instant('message.info'),
      });
    }

    // If setting as primary, remove primary from others in pending
    if (isPrimary) {
      currentPending.forEach((s) => (s.isPrimary = false));
    }

    // Build targets array from selected targets and indicators (only if not skipped)
    const targets: OpportunitySDGTarget[] = [];
    const selectedTargetsMap = skipTargets ? new Map() : this.selectedTargets();

    for (const [
      targetDatabaseId,
      indicatorIds,
    ] of selectedTargetsMap.entries()) {
      const targetInfo = this.availableTargets().find(
        (t) => t.id === targetDatabaseId,
      );
      if (targetInfo) {
        const indicators: OpportunitySDGIndicator[] = [];

        // Add selected indicators for this target
        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.availableIndicators().find(
            (i) => i.id === indicatorId,
          );
          if (indicatorInfo) {
            indicators.push({
              id: 0,
              opportunityId: opp.id!,
              opportunitySDGTargetId: 0,
              sdgIndicatorDatabaseId: indicatorInfo.id,
              sdgIndicatorId: indicatorInfo.sdgIndicatorId,
              sdgIndicatorLongDescription:
                indicatorInfo.sdgIndicatorLongDescription,
              notes: null,
            });
          }
        }

        targets.push({
          id: 0,
          opportunityId: opp.id!,
          opportunitySDGId: 0,
          sdgTargetDatabaseId: targetInfo.id,
          sdgTargetId: targetInfo.sdgTargetId,
          targetDescription: targetInfo.targetDescription,
          targetType: targetInfo.targetType,
          notes: null,
          indicators: indicators,
        });
      }
    }

    // Add new SDG with targets and indicators
    const newSDG: OpportunitySDG = {
      id: 0,
      opportunityId: opp.id!,
      sdgId: sdg.sdgId || '',
      sdgDatabaseId: sdg.id,
      sdgNumber: sdg.sdgNumber || '',
      sdgName: sdg.name,
      isPrimary: isPrimary,
      skipTargetsAndIndicators: skipTargets || null,
      notes: null,
      targets: targets,
    };

    currentPending.push(newSDG);
    this.pendingSDGSelections.set(currentPending);

    // Reset configuration section for next SDG
    this.resetSDGConfiguration();

    // Show success feedback
    this.feedbackService.showSuccessToast({
      detail: this.translateService.instant(
        'message.opportunity.sdgAddedToSelection',
      ),
      summary: this.translateService.instant('message.success'),
    });

    this.cdr.detectChanges();
  }

  /**
   * @description Add "N/A" SDG as Primary and clear all other SDGs
   * @param sdg - The N/A SDG object
   * @param opp - Current opportunity
   */
  private addNASDG(sdg: SDG, opp: Opportunity): void {
    // Create N/A SDG - always Primary, no targets/indicators needed
    const naSDG: OpportunitySDG = {
      id: 0,
      opportunityId: opp.id!,
      sdgId: sdg.sdgId || '',
      sdgDatabaseId: sdg.id,
      sdgNumber: sdg.sdgNumber || '',
      sdgName: sdg.name,
      isPrimary: true, // N/A is always Primary
      skipTargetsAndIndicators: true, // N/A doesn't need targets
      notes: null,
      targets: [],
    };

    // Clear all pending selections and add only N/A
    this.pendingSDGSelections.set([naSDG]);

    // Clear validation errors
    this.sdgValidationErrors.set(new Set());

    // Reset configuration section for next SDG
    this.resetSDGConfiguration();

    // Show success feedback
    this.feedbackService.showSuccessToast({
      detail: this.translateService.instant(
        'message.opportunity.naSDGAdded',
      ),
      summary: this.translateService.instant('message.success'),
    });

    this.cdr.detectChanges();
  }

  /**
   * @description Reset SDG configuration section (top part of dialog)
   */
  resetSDGConfiguration(): void {
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.isPrimaryControl.enable(); // Re-enable the control when resetting
    this.skipTargetsControl.setValue(false);
    this.showValidationError.set(false);
    this.editingFromPending.set(false);
    this.editingPendingIndex.set(null);
    this.availableTargets.set([]);
    this.availableIndicators.set([]);
    this.selectedTargets.set(new Map());
    this.loadingIndicatorsForTargets.set(new Set());
    this.cdr.detectChanges();
  }

  /**
   * @description Edit SDG from pending selections
   */
  editPendingSDG(index: number): void {
    const pending = this.pendingSDGSelections();
    const sdg = pending[index];

    if (!sdg) return;

    // Find the SDG in the master list
    const masterSDG = this.sdgs().find((s) => s.sdgId === sdg.sdgId);

    this.editingFromPending.set(true);
    this.editingPendingIndex.set(index);
    this.sdgControl.setValue(masterSDG || null);
    this.isPrimaryControl.setValue(sdg.isPrimary);
    
    // Disable alignment type control for N/A SDG, enable for others
    if (sdg.sdgId === 'N/A') {
      this.isPrimaryControl.disable();
    } else {
      this.isPrimaryControl.enable();
    }
    
    this.skipTargetsControl.setValue(sdg.skipTargetsAndIndicators || false);
    this.showValidationError.set(false);
    
    // Clear validation error for this SDG when entering edit mode
    const currentErrors = this.sdgValidationErrors();
    if (currentErrors.has(index)) {
      const newErrors = new Set(currentErrors);
      newErrors.delete(index);
      this.sdgValidationErrors.set(newErrors);
    }

    // Clear previous targets and indicators
    this.availableTargets.set([]);
    this.availableIndicators.set([]);
    this.selectedTargets.set(new Map());
    this.loadingIndicatorsForTargets.set(new Set());

    // Load targets for this SDG (only if not skipped)
    if (masterSDG && masterSDG.sdgId && !sdg.skipTargetsAndIndicators) {
      this.loadingTargets.set(true);
      this.valuesService.getSDGTargets(masterSDG.sdgId).subscribe({
        next: (targets) => {
          this.loadingTargets.set(false);
          this.availableTargets.set(targets);

          // Pre-select existing targets and indicators
          const selectedTargetsMap = new Map<number, Set<number>>();

          if (sdg.targets && sdg.targets.length > 0) {
            // Load all indicators for the targets
            const indicatorRequests = sdg.targets.map((target) =>
              this.valuesService.getSDGIndicators(target.sdgTargetId),
            );

            if (indicatorRequests.length > 0) {
              import('rxjs').then((rxjs) => {
                rxjs.forkJoin(indicatorRequests).subscribe({
                  next: (allIndicators) => {
                    const flatIndicators = allIndicators.flat();
                    this.availableIndicators.set(flatIndicators);

                    sdg.targets!.forEach((target) => {
                      const indicatorIds = new Set<number>();
                      if (target.indicators && target.indicators.length > 0) {
                        target.indicators.forEach((ind) =>
                          indicatorIds.add(ind.sdgIndicatorDatabaseId),
                        );
                      }
                      selectedTargetsMap.set(
                        target.sdgTargetDatabaseId,
                        indicatorIds,
                      );
                    });

                    this.selectedTargets.set(selectedTargetsMap);
                    this.cdr.detectChanges();
                  },
                });
              });
            } else {
              sdg.targets.forEach((target) => {
                selectedTargetsMap.set(target.sdgTargetDatabaseId, new Set());
              });
              this.selectedTargets.set(selectedTargetsMap);
            }
          }

          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading SDG targets:', error);
          this.loadingTargets.set(false);
          this.availableTargets.set([]);
        },
      });
    }

    this.cdr.detectChanges();
  }

  /**
   * @description Update SDG in pending selections after editing
   */
  updatePendingSDG(): void {
    const sdg = this.sdgControl.value;
    const isPrimary = this.isPrimaryControl.value || false;
    const skipTargets = this.skipTargetsControl.value || false;
    const index = this.editingPendingIndex();

    if (!sdg || index === null) return;

    let currentPending = [...this.pendingSDGSelections()];
    const opp = this.opportunity();
    const isNASDG = sdg.sdgId === 'N/A';

    // SPECIAL HANDLING: Changing TO "N/A" SDG
    if (isNASDG) {
      // If there are other SDGs besides the one being edited, show confirmation
      if (currentPending.length > 1) {
        this.feedbackService.showConfirmDialog(
          {
            summary: this.translateService.instant('confirmation.clearAllSDGs'),
            detail: this.translateService.instant('message.opportunity.changingToNASDGWillClearOthers'),
          },
          () => {
            // User confirmed - clear all SDGs and add only N/A as Primary
            this.addNASDG(sdg, opp);
          }
        );
        return;
      } else {
        // Only this SDG exists, replace it with N/A
        this.addNASDG(sdg, opp);
        return;
      }
    }

    // SPECIAL HANDLING: Changing FROM "N/A" to a regular SDG
    const originalSDG = currentPending[index];
    if (originalSDG.sdgId === 'N/A' && !isNASDG) {
      // Replacing N/A with a regular SDG - just show info message
      this.feedbackService.showInfoToast({
        detail: this.translateService.instant('message.opportunity.replacingNASDG'),
        summary: this.translateService.instant('message.info'),
      });
    }

    // If setting as primary, remove primary from others
    if (isPrimary) {
      currentPending.forEach((s, i) => {
        if (i !== index) s.isPrimary = false;
      });
    }

    // Build targets array - preserve existing IDs when updating
    // Note: originalSDG already declared above for N/A handling
    const targets: OpportunitySDGTarget[] = [];
    const selectedTargetsMap = skipTargets ? new Map() : this.selectedTargets();

    for (const [
      targetDatabaseId,
      indicatorIds,
    ] of selectedTargetsMap.entries()) {
      const targetInfo = this.availableTargets().find(
        (t) => t.id === targetDatabaseId,
      );
      if (targetInfo) {
        // Find existing target to preserve its ID
        const existingTarget = originalSDG.targets?.find(
          (t) => t.sdgTargetDatabaseId === targetDatabaseId,
        );

        const indicators: OpportunitySDGIndicator[] = [];

        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.availableIndicators().find(
            (i) => i.id === indicatorId,
          );
          if (indicatorInfo) {
            // Find existing indicator to preserve its ID
            const existingIndicator = existingTarget?.indicators?.find(
              (ind) => ind.sdgIndicatorDatabaseId === indicatorId,
            );

            indicators.push({
              id: existingIndicator?.id || 0, // Preserve existing ID
              opportunityId: opp.id!,
              opportunitySDGTargetId: existingTarget?.id || 0,
              sdgIndicatorDatabaseId: indicatorInfo.id,
              sdgIndicatorId: indicatorInfo.sdgIndicatorId,
              sdgIndicatorLongDescription:
                indicatorInfo.sdgIndicatorLongDescription,
              notes: existingIndicator?.notes || null,
            });
          }
        }

        targets.push({
          id: existingTarget?.id || 0, // Preserve existing ID
          opportunityId: opp.id!,
          opportunitySDGId: originalSDG.id,
          sdgTargetDatabaseId: targetInfo.id,
          sdgTargetId: targetInfo.sdgTargetId,
          targetDescription: targetInfo.targetDescription,
          targetType: targetInfo.targetType,
          notes: existingTarget?.notes || null,
          indicators: indicators,
        });
      }
    }

    // Update the SDG at the specified index
    // Preserve the original id if it exists (for existing SDGs)
    currentPending[index] = {
      id: originalSDG.id, // Preserve existing id
      opportunityId: opp.id!,
      sdgId: sdg.sdgId || '',
      sdgDatabaseId: sdg.id,
      sdgNumber: sdg.sdgNumber || '',
      sdgName: sdg.name,
      isPrimary: isPrimary,
      skipTargetsAndIndicators: skipTargets || null,
      notes: originalSDG.notes, // Preserve notes if any
      targets: targets,
    };

    this.pendingSDGSelections.set(currentPending);
    
    // Clear validation error for this SDG since it's been updated
    const currentErrors = this.sdgValidationErrors();
    if (currentErrors.has(index)) {
      const newErrors = new Set(currentErrors);
      newErrors.delete(index);
      this.sdgValidationErrors.set(newErrors);
    }

    // Reset configuration section
    this.resetSDGConfiguration();

    this.feedbackService.showSuccessToast({
      detail: this.translateService.instant('message.opportunity.sdgUpdated'),
      summary: this.translateService.instant('message.success'),
    });

    this.cdr.detectChanges();
  }

  /**
   * @description Remove SDG from pending selections
   */
  removePendingSDG(index: number): void {
    const currentPending = [...this.pendingSDGSelections()];
    currentPending.splice(index, 1);
    this.pendingSDGSelections.set(currentPending);
    
    // Clear validation errors and recalculate for remaining SDGs
    const currentErrors = this.sdgValidationErrors();
    const newErrors = new Set<number>();
    
    // Adjust indices for remaining errors
    currentErrors.forEach(errorIndex => {
      if (errorIndex < index) {
        newErrors.add(errorIndex);
      } else if (errorIndex > index) {
        newErrors.add(errorIndex - 1);
      }
      // Skip errorIndex === index (the removed SDG)
    });
    
    this.sdgValidationErrors.set(newErrors);
    this.cdr.detectChanges();
  }

  /**
   * @description Clear all pending SDG selections
   */
  clearPendingSDGs(): void {
    this.pendingSDGSelections.set([]);
    this.sdgValidationErrors.set(new Set());
    this.resetSDGConfiguration();
    this.cdr.detectChanges();
  }

  /**
   * @description Validate if an SDG has either targets selected OR opt-out is checked
   * @param sdg - The SDG to validate
   * @returns true if valid, false otherwise
   */
  private validateSDG(sdg: OpportunitySDG): boolean {
    // "N/A" SDG is always valid (doesn't need targets or opt-out)
    if (sdg.sdgId === 'N/A') {
      return true;
    }
    
    // Valid if skip targets is checked
    if (sdg.skipTargetsAndIndicators) {
      return true;
    }
    
    // Valid if at least one target is selected
    return !!(sdg.targets && sdg.targets.length > 0);
  }
  
  /**
   * @description Check if a specific SDG has a validation error
   * @param index - Index of the SDG in pending selections
   * @returns true if the SDG has a validation error
   */
  hasValidationError(index: number): boolean {
    return this.sdgValidationErrors().has(index);
  }

  /**
   * @description Validate all pending SDGs
   * @returns Array of invalid SDG indices
   */
  private validateAllPendingSDGs(): number[] {
    const pending = this.pendingSDGSelections();
    const invalidIndices: number[] = [];
    
    pending.forEach((sdg, index) => {
      if (!this.validateSDG(sdg)) {
        invalidIndices.push(index);
      }
    });
    
    return invalidIndices;
  }

  /**
   * @description Commit all pending SDGs to the opportunity
   * Replaces entire SDG list since pending contains both existing and new SDGs
   */
  commitPendingSDGs(): void {
    const pending = this.pendingSDGSelections();

    if (pending.length === 0) {
      return;
    }

    // Validate all pending SDGs
    const invalidIndices = this.validateAllPendingSDGs();
    
    if (invalidIndices.length > 0) {
      // Show validation errors for invalid SDGs
      this.sdgValidationErrors.set(new Set(invalidIndices));
      
      // Show error toast
      this.feedbackService.showErrorToast({
        detail: this.translateService.instant(
          invalidIndices.length === 1
            ? 'message.validation.sdgTargetsRequired'
            : 'message.validation.multipleSDGsTargetsRequired',
          { count: invalidIndices.length }
        ),
        summary: this.translateService.instant('message.validation.validationFailed'),
      });
      
      this.cdr.detectChanges();
      return;
    }

    const opp = this.opportunity();
    const originalSDGCount = opp.sdGs?.length || 0;

    // Count new SDGs (those with id === 0)
    const newSDGsCount = pending.filter((s) => s.id === 0).length;

    // Pending selections now contain both existing and new SDGs
    // Simply replace the entire SDG array with pending selections
    const updatedOpportunity = {
      ...opp,
      sdGs: [...pending],
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Mark as changed
    this.markAsChanged();

    // Show success message based on what was done
    if (newSDGsCount === 0) {
      // Only edits, no new SDGs
      this.feedbackService.showSuccessToast({
        detail: this.translateService.instant(
          'message.opportunity.sdgsUpdated',
        ),
        summary: this.translateService.instant('message.success'),
      });
    } else if (originalSDGCount === 0) {
      // All new SDGs
      this.feedbackService.showSuccessToast({
        detail: this.translateService.instant(
          newSDGsCount === 1
            ? 'message.opportunity.sdgAdded'
            : 'message.opportunity.sdgsAdded',
          { count: newSDGsCount },
        ),
        summary: this.translateService.instant('message.success'),
      });
    } else {
      // Mixed: some new, some existing
      this.feedbackService.showSuccessToast({
        detail: this.translateService.instant(
          'message.opportunity.sdgsAddedAndUpdated',
          {
            added: newSDGsCount,
            total: pending.length,
          },
        ),
        summary: this.translateService.instant('message.success'),
      });
    }

    // Clear validation errors and reset dialog state
    this.sdgValidationErrors.set(new Set());
    this.showSDGDialog.set(false);
    this.pendingSDGSelections.set([]);
    this.resetSDGConfiguration();
    this.cdr.detectChanges();
  }

  /**
   * @description Legacy method - kept for backward compatibility
   * @deprecated Use commitPendingSDGs instead
   */
  addSDG(): void {
    // If we're in the new mode with pending selections, use the new flow
    if (this.editingFromPending()) {
      this.updatePendingSDG();
    } else {
      this.addSDGToPendingSelection();
    }
  }

  /**
   * @description Edit existing SDG from read-only view
   * Pre-loads all existing SDGs into pending selections, then edits the specific one
   */
  editSDG(index: number): void {
    const opp = this.opportunity();
    const sdg = opp.sdGs?.[index];

    if (!sdg) return;

    // Pre-load all existing SDGs from opportunity into pending selections
    const existingSDGs = opp.sdGs ? [...opp.sdGs] : [];
    this.pendingSDGSelections.set(existingSDGs);

    // Clear validation errors
    this.sdgValidationErrors.set(new Set());

    // Now use the pending edit flow instead of direct edit
    // This ensures all SDGs are preserved in the pending selections
    this.editPendingSDG(index);

    // Show the dialog (editPendingSDG will have already set this up)
    this.showSDGDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description LEGACY: Old direct edit flow - kept for reference but should use editSDG -> editPendingSDG flow
   * @deprecated Use editSDG which loads pending selections first
   */
  editSDGLegacy(index: number): void {
    const opp = this.opportunity();
    const sdg = opp.sdGs?.[index];

    if (!sdg) return;

    // Find the SDG in the master list by matching sdgId string
    const masterSDG = this.sdgs().find((s) => s.sdgId === sdg.sdgId);

    this.isEditingSDG.set(true);
    this.editingSDGIndex.set(index);
    this.sdgControl.setValue(masterSDG || null);
    this.isPrimaryControl.setValue(sdg.isPrimary);
    this.skipTargetsControl.setValue(sdg.skipTargetsAndIndicators || false);
    this.showValidationError.set(false);

    // Clear previous targets and indicators
    this.availableTargets.set([]);
    this.availableIndicators.set([]);
    this.selectedTargets.set(new Map());
    this.loadingIndicatorsForTargets.set(new Set());

    // Load targets for this SDG (only if not skipped)
    if (masterSDG && masterSDG.sdgId && !sdg.skipTargetsAndIndicators) {
      this.loadingTargets.set(true);
      this.valuesService.getSDGTargets(masterSDG.sdgId).subscribe({
        next: (targets) => {
          this.loadingTargets.set(false);
          this.availableTargets.set(targets);

          // Pre-select existing targets and indicators
          const selectedTargetsMap = new Map<number, Set<number>>();

          if (sdg.targets && sdg.targets.length > 0) {
            // Load all indicators for the targets
            const indicatorRequests = sdg.targets.map((target) =>
              this.valuesService.getSDGIndicators(target.sdgTargetId),
            );

            // Combine all indicator requests
            if (indicatorRequests.length > 0) {
              // Use forkJoin to load all indicators
              import('rxjs').then((rxjs) => {
                rxjs.forkJoin(indicatorRequests).subscribe({
                  next: (allIndicators) => {
                    // Flatten all indicators
                    const flatIndicators = allIndicators.flat();
                    this.availableIndicators.set(flatIndicators);

                    // Now pre-select the targets and indicators
                    sdg.targets!.forEach((target) => {
                      const indicatorIds = new Set<number>();
                      if (target.indicators && target.indicators.length > 0) {
                        target.indicators.forEach((ind) =>
                          indicatorIds.add(ind.sdgIndicatorDatabaseId),
                        );
                      }
                      selectedTargetsMap.set(
                        target.sdgTargetDatabaseId,
                        indicatorIds,
                      );
                    });

                    this.selectedTargets.set(selectedTargetsMap);
                    this.cdr.detectChanges();
                  },
                });
              });
            } else {
              // No indicators to load, just select targets
              sdg.targets.forEach((target) => {
                selectedTargetsMap.set(target.sdgTargetDatabaseId, new Set());
              });
              this.selectedTargets.set(selectedTargetsMap);
            }
          }

          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading SDG targets:', error);
          this.loadingTargets.set(false);
          this.availableTargets.set([]);
        },
      });
    }

    this.showSDGDialog.set(true);
    this.cdr.detectChanges();
  }

  // Note: editSDGLegacy is kept for reference but not used. 
  // The new flow uses editSDG -> editPendingSDG which properly maintains all SDGs in pending selections.

  /**
   * @description Update existing SDG
   */
  updateSDG(): void {
    const sdg = this.sdgControl.value;
    const isPrimary = this.isPrimaryControl.value || false;
    const skipTargets = this.skipTargetsControl.value || false;
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

    // Build targets array from selected targets and indicators (only if not skipped)
    const targets: OpportunitySDGTarget[] = [];
    const selectedTargetsMap = skipTargets ? new Map() : this.selectedTargets();

    for (const [
      targetDatabaseId,
      indicatorIds,
    ] of selectedTargetsMap.entries()) {
      const targetInfo = this.availableTargets().find(
        (t) => t.id === targetDatabaseId,
      );
      if (targetInfo) {
        const indicators: OpportunitySDGIndicator[] = [];

        // Add selected indicators for this target
        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.availableIndicators().find(
            (i) => i.id === indicatorId,
          );
          if (indicatorInfo) {
            indicators.push({
              id: 0,
              opportunityId: opp.id!,
              opportunitySDGTargetId: 0, // Will be set by backend
              sdgIndicatorDatabaseId: indicatorInfo.id,
              sdgIndicatorId: indicatorInfo.sdgIndicatorId,
              sdgIndicatorLongDescription:
                indicatorInfo.sdgIndicatorLongDescription,
              notes: null,
            });
          }
        }

        targets.push({
          id: 0,
          opportunityId: opp.id!,
          opportunitySDGId: 0, // Will be set by backend
          sdgTargetDatabaseId: targetInfo.id,
          sdgTargetId: targetInfo.sdgTargetId,
          targetDescription: targetInfo.targetDescription,
          targetType: targetInfo.targetType,
          notes: null,
          indicators: indicators,
        });
      }
    }

    // Update SDG with targets and indicators
    currentSDGs[index] = {
      ...currentSDGs[index],
      sdgId: sdg.sdgId || '',
      sdgDatabaseId: sdg.id, // Store the integer database ID for saving
      sdgNumber: sdg.sdgNumber || '',
      sdgName: sdg.name,
      isPrimary: isPrimary,
      skipTargetsAndIndicators: skipTargets || null,
      targets: targets,
    };

    // Update opportunity
    const updatedOpportunity = {
      ...opp,
      sdGs: currentSDGs,
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Reset dialog state
    this.showSDGDialog.set(false);
    this.isEditingSDG.set(false);
    this.editingSDGIndex.set(null);
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.skipTargetsControl.setValue(false);
    this.showValidationError.set(false);
    this.availableTargets.set([]);
    this.availableIndicators.set([]);
    this.selectedTargets.set(new Map());
    this.loadingIndicatorsForTargets.set(new Set());
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
      sdGs: currentSDGs,
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Mark as changed (SDG removed)
    this.markAsChanged();

    this.cdr.detectChanges();
  }

  /**
   * @description Open UNCF dialog for a specific country
   */
  openUNCFDialog(oppCountry: OpportunityCountry): void {
    this.selectedCountryForUNCF.set(oppCountry);
    this.isEditingUNCFCountry.set(false);
    this.editingUNCFCountryIndex.set(null);
    this.showUNCFValidationError.set(false);

    // Load available outcomes for this country
    if (oppCountry.country?.iso2Code) {
      this.loadingUNCFOutcomesForDialog.set(true);
      this.valuesService
        .getUNCFOutcomes(oppCountry.country.iso2Code)
        .subscribe({
          next: (outcomes) => {
            this.availableUNCFOutcomes.set(outcomes);
            this.loadingUNCFOutcomesForDialog.set(false);
            this.cdr.detectChanges();
          },
          error: (error) => {
            console.error('Error loading UNCF outcomes:', error);
            this.loadingUNCFOutcomesForDialog.set(false);
            this.availableUNCFOutcomes.set([]);
          },
        });
    }

    this.selectedUNCFOutcomes.set(new Map());
    this.loadingIndicatorsForUNCFOutcomes.set(new Set());
    this.showUNCFDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Edit existing UNCF outcomes for a country
   */
  editUNCFForCountry(oppCountry: OpportunityCountry, index: number): void {
    this.selectedCountryForUNCF.set(oppCountry);
    this.isEditingUNCFCountry.set(true);
    this.editingUNCFCountryIndex.set(index);
    this.showUNCFValidationError.set(false);

    // Load available outcomes for this country
    if (oppCountry.country?.iso2Code) {
      this.loadingUNCFOutcomesForDialog.set(true);
      this.valuesService
        .getUNCFOutcomes(oppCountry.country.iso2Code)
        .subscribe({
          next: (outcomes) => {
            this.availableUNCFOutcomes.set(outcomes);
            this.loadingUNCFOutcomesForDialog.set(false);

            // Pre-select existing outcomes and indicators
            const opp = this.opportunity();
            const existingUNCFOutcomes =
              opp.uncfOutcomes?.filter(
                (uo) => uo.opportunityCountryId === oppCountry.id,
              ) || [];

            if (existingUNCFOutcomes.length > 0) {
              const selectedMap = new Map<number, Set<number>>();

              // Load indicators for all existing outcomes
              const indicatorRequests = existingUNCFOutcomes.map((uo) =>
                this.valuesService.getUNCFIndicators(uo.uncfOutcomeId),
              );

              if (indicatorRequests.length > 0) {
                import('rxjs').then((rxjs) => {
                  rxjs.forkJoin(indicatorRequests).subscribe({
                    next: (allIndicators) => {
                      const flatIndicators = allIndicators.flat();

                      // Store indicators for each outcome
                      existingUNCFOutcomes.forEach((uo, idx) => {
                        const outcomeIndicators = allIndicators[idx];
                        const indicatorMap = new Map(
                          this.availableUNCFIndicators(),
                        );
                        indicatorMap.set(uo.uncfOutcomeId, outcomeIndicators);
                        this.availableUNCFIndicators.set(indicatorMap);

                        // Pre-select indicators
                        const indicatorIds = new Set<number>();
                        uo.indicators?.forEach((ind) => {
                          indicatorIds.add(ind.uncfIndicatorId);
                        });
                        selectedMap.set(uo.uncfOutcomeId, indicatorIds);
                      });

                      this.selectedUNCFOutcomes.set(selectedMap);
                      this.cdr.detectChanges();
                    },
                  });
                });
              }
            }

            this.cdr.detectChanges();
          },
          error: (error) => {
            console.error('Error loading UNCF outcomes:', error);
            this.loadingUNCFOutcomesForDialog.set(false);
            this.availableUNCFOutcomes.set([]);
          },
        });
    }

    this.showUNCFDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel UNCF dialog and reset state
   */
  cancelUNCFDialog(): void {
    this.showUNCFDialog.set(false);
    this.selectedCountryForUNCF.set(null);
    this.availableUNCFOutcomes.set([]);
    this.showUNCFValidationError.set(false);
    this.isEditingUNCFCountry.set(false);
    this.editingUNCFCountryIndex.set(null);
    this.selectedUNCFOutcomes.set(new Map());
    this.loadingIndicatorsForUNCFOutcomes.set(new Set());
    this.cdr.detectChanges();
  }

  /**
   * @description Toggle UNCF outcome selection and load indicators
   */
  toggleUNCFOutcome(outcome: UNCFOutcome): void {
    const currentSelection = new Map(this.selectedUNCFOutcomes());

    if (currentSelection.has(outcome.id)) {
      // Remove outcome and its indicators
      currentSelection.delete(outcome.id);
      this.selectedUNCFOutcomes.set(currentSelection);
      this.cdr.detectChanges();
    } else {
      // Add outcome with empty indicator set
      currentSelection.set(outcome.id, new Set());
      this.selectedUNCFOutcomes.set(currentSelection);

      // Mark this outcome as loading
      const loadingSet = new Set(this.loadingIndicatorsForUNCFOutcomes());
      loadingSet.add(outcome.id);
      this.loadingIndicatorsForUNCFOutcomes.set(loadingSet);

      // Load indicators for this outcome
      this.valuesService.getUNCFIndicators(outcome.id).subscribe({
        next: (indicators) => {
          // Remove from loading set
          const loadingSet = new Set(this.loadingIndicatorsForUNCFOutcomes());
          loadingSet.delete(outcome.id);
          this.loadingIndicatorsForUNCFOutcomes.set(loadingSet);

          // Store available indicators
          const indicatorMap = new Map(this.availableUNCFIndicators());
          indicatorMap.set(outcome.id, indicators);
          this.availableUNCFIndicators.set(indicatorMap);

          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error(
            'Error loading UNCF indicators for outcome:',
            outcome.id,
            error,
          );
          const loadingSet = new Set(this.loadingIndicatorsForUNCFOutcomes());
          loadingSet.delete(outcome.id);
          this.loadingIndicatorsForUNCFOutcomes.set(loadingSet);
          this.cdr.detectChanges();
        },
      });
    }
  }

  /**
   * @description Toggle UNCF indicator selection for an outcome
   */
  toggleUNCFIndicator(outcomeId: number, indicatorId: number): void {
    const currentSelection = new Map(this.selectedUNCFOutcomes());

    if (currentSelection.has(outcomeId)) {
      const indicators = currentSelection.get(outcomeId)!;
      if (indicators.has(indicatorId)) {
        indicators.delete(indicatorId);
      } else {
        indicators.add(indicatorId);
      }
      currentSelection.set(outcomeId, indicators);
    }

    this.selectedUNCFOutcomes.set(currentSelection);
    this.cdr.detectChanges();
  }

  /**
   * @description Check if a UNCF outcome is selected
   */
  isUNCFOutcomeSelected(outcomeId: number): boolean {
    return this.selectedUNCFOutcomes().has(outcomeId);
  }

  /**
   * @description Check if indicators are loading for a UNCF outcome
   */
  isLoadingUNCFIndicators(outcomeId: number): boolean {
    return this.loadingIndicatorsForUNCFOutcomes().has(outcomeId);
  }

  /**
   * @description Check if a UNCF indicator is selected for an outcome
   */
  isUNCFIndicatorSelected(outcomeId: number, indicatorId: number): boolean {
    const outcome = this.selectedUNCFOutcomes().get(outcomeId);
    return outcome ? outcome.has(indicatorId) : false;
  }

  /**
   * @description Get UNCF indicators for a specific outcome
   */
  getIndicatorsForUNCFOutcome(outcomeId: number): UNCFIndicator[] {
    return this.availableUNCFIndicators().get(outcomeId) || [];
  }

  /**
   * @description Add UNCF outcomes for a country
   */
  addUNCFOutcomes(): void {
    const selectedOutcomes = this.selectedUNCFOutcomes();
    const selectedCountry = this.selectedCountryForUNCF();

    // Validation
    if (!selectedCountry || selectedOutcomes.size === 0) {
      this.showUNCFValidationError.set(true);
      return;
    }

    const opp = this.opportunity();
    const currentUNCFOutcomes = [...(opp.uncfOutcomes || [])];

    // Build outcomes array from selected outcomes and indicators
    const outcomes: OpportunityUNCFOutcome[] = [];

    for (const [outcomeId, indicatorIds] of selectedOutcomes.entries()) {
      const outcomeInfo = this.availableUNCFOutcomes().find(
        (o) => o.id === outcomeId,
      );
      if (outcomeInfo) {
        const indicators: OpportunityUNCFIndicator[] = [];

        // Add selected indicators for this outcome
        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.getIndicatorsForUNCFOutcome(
            outcomeId,
          ).find((i) => i.id === indicatorId);
          if (indicatorInfo) {
            indicators.push({
              id: 0,
              opportunityId: opp.id!,
              opportunityUNCFOutcomeId: 0, // Will be set by backend
              uncfIndicatorId: indicatorInfo.id,
              uncfIndicatorExternalId: indicatorInfo.uncfIndicatorExternalId,
              uncfIndicatorName: indicatorInfo.name,
              notes: null,
            });
          }
        }

        outcomes.push({
          id: 0,
          opportunityId: opp.id!,
          opportunityCountryId: selectedCountry.id,
          uncfOutcomeId: outcomeInfo.id,
          uncfOutcomeExternalId: outcomeInfo.uncfOutcomeExternalId,
          uncfOutcomeName: outcomeInfo.name,
          versionNo: outcomeInfo.versionNo,
          country: outcomeInfo.country,
          notes: null,
          indicators: indicators,
        });
      }
    }

    // Add new UNCF outcomes
    currentUNCFOutcomes.push(...outcomes);

    // Update opportunity
    const updatedOpportunity = {
      ...opp,
      uncfOutcomes: currentUNCFOutcomes,
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Mark as changed (UNCF outcomes added)
    this.markAsChanged();

    // Reset dialog state
    this.cancelUNCFDialog();
  }

  /**
   * @description Update UNCF outcomes for a country
   */
  updateUNCFOutcomes(): void {
    const selectedOutcomes = this.selectedUNCFOutcomes();
    const selectedCountry = this.selectedCountryForUNCF();

    if (!selectedCountry || selectedOutcomes.size === 0) {
      this.showUNCFValidationError.set(true);
      return;
    }

    const opp = this.opportunity();
    let currentUNCFOutcomes = [...(opp.uncfOutcomes || [])];

    // Remove existing outcomes for this country
    currentUNCFOutcomes = currentUNCFOutcomes.filter(
      (uo) => uo.opportunityCountryId !== selectedCountry.id,
    );

    // Build new outcomes array
    const outcomes: OpportunityUNCFOutcome[] = [];

    for (const [outcomeId, indicatorIds] of selectedOutcomes.entries()) {
      const outcomeInfo = this.availableUNCFOutcomes().find(
        (o) => o.id === outcomeId,
      );
      if (outcomeInfo) {
        const indicators: OpportunityUNCFIndicator[] = [];

        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.getIndicatorsForUNCFOutcome(
            outcomeId,
          ).find((i) => i.id === indicatorId);
          if (indicatorInfo) {
            indicators.push({
              id: 0,
              opportunityId: opp.id!,
              opportunityUNCFOutcomeId: 0,
              uncfIndicatorId: indicatorInfo.id,
              uncfIndicatorExternalId: indicatorInfo.uncfIndicatorExternalId,
              uncfIndicatorName: indicatorInfo.name,
              notes: null,
            });
          }
        }

        outcomes.push({
          id: 0,
          opportunityId: opp.id!,
          opportunityCountryId: selectedCountry.id,
          uncfOutcomeId: outcomeInfo.id,
          uncfOutcomeExternalId: outcomeInfo.uncfOutcomeExternalId,
          uncfOutcomeName: outcomeInfo.name,
          versionNo: outcomeInfo.versionNo,
          country: outcomeInfo.country,
          notes: null,
          indicators: indicators,
        });
      }
    }

    // Add updated outcomes
    currentUNCFOutcomes.push(...outcomes);

    // Update opportunity
    const updatedOpportunity = {
      ...opp,
      uncfOutcomes: currentUNCFOutcomes,
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Mark as changed (UNCF outcomes updated)
    this.markAsChanged();

    // Reset dialog state
    this.cancelUNCFDialog();
  }

  /**
   * @description Remove UNCF outcomes for a country
   */
  /**
   * @description Get UNCF outcomes for a specific opportunity country
   */
  getUNCFOutcomesForOpportunityCountry(
    oppCountryId: number,
  ): OpportunityUNCFOutcome[] {
    return (
      this.opportunity().uncfOutcomes?.filter(
        (uo) => uo.opportunityCountryId === oppCountryId,
      ) || []
    );
  }

  /**
   * @description Check if a specific country has inactive UNCF data with newer versions available
   */
  hasInactiveUNCFWithUpdatesForCountry(oppCountryId: number): boolean {
    const outcomesForCountry =
      this.getUNCFOutcomesForOpportunityCountry(oppCountryId);

    // Check if any outcome for this country is inactive with newer version
    const hasOutcomeWithUpdates = outcomesForCountry.some(
      (outcome) => outcome.isInactive && outcome.hasNewerVersion,
    );

    if (hasOutcomeWithUpdates) return true;

    // Check if any indicator for this country is inactive with newer version
    const hasIndicatorWithUpdates = outcomesForCountry.some((outcome) =>
      outcome.indicators?.some(
        (indicator) => indicator.isInactive && indicator.hasNewerVersion,
      ),
    );

    return hasIndicatorWithUpdates;
  }

  /**
   * @description Check if a specific country has inactive UNCF data without newer versions available
   */
  hasInactiveUNCFWithoutUpdatesForCountry(oppCountryId: number): boolean {
    const outcomesForCountry =
      this.getUNCFOutcomesForOpportunityCountry(oppCountryId);

    // Check if any outcome for this country is inactive without newer version
    const hasOutcomeWithoutUpdates = outcomesForCountry.some(
      (outcome) => outcome.isInactive && !outcome.hasNewerVersion,
    );

    if (hasOutcomeWithoutUpdates) return true;

    // Check if any indicator for this country is inactive without newer version
    const hasIndicatorWithoutUpdates = outcomesForCountry.some((outcome) =>
      outcome.indicators?.some(
        (indicator) => indicator.isInactive && !indicator.hasNewerVersion,
      ),
    );

    return hasIndicatorWithoutUpdates;
  }

  /**
   * @description Load UNCF outcomes for countries with active UNSDCF
   */
  loadUNCFOutcomesForCountries(): void {
    const countries = this.countriesWithUNCF();

    if (countries.length === 0) {
      return;
    }

    this.loadingUNCFOutcomes.set(true);
    const outcomeMap = new Map<number, UNCFOutcome[]>();
    let loadedCount = 0;

    countries.forEach((oppCountry) => {
      if (!oppCountry.country?.iso2Code) return;

      this.valuesService
        .getUNCFOutcomes(oppCountry.country.iso2Code)
        .subscribe({
          next: (outcomes) => {
            if (outcomes.length > 0) {
              outcomeMap.set(oppCountry.country!.id, outcomes);
            }

            loadedCount++;
            if (loadedCount === countries.length) {
              this.uncfOutcomesByCountry.set(outcomeMap);
              this.loadingUNCFOutcomes.set(false);
              this.cdr.detectChanges();
            }
          },
          error: (error) => {
            console.error(
              'Error loading UNCF outcomes for country:',
              oppCountry.country?.name,
              error,
            );
            loadedCount++;
            if (loadedCount === countries.length) {
              this.uncfOutcomesByCountry.set(outcomeMap);
              this.loadingUNCFOutcomes.set(false);
              this.cdr.detectChanges();
            }
          },
        });
    });
  }

  /**
   * @description Get UNCF outcomes for a specific country
   */
  getUNCFOutcomesForCountry(countryId: number): UNCFOutcome[] {
    return this.uncfOutcomesByCountry().get(countryId) || [];
  }

  /**
   * @description Get UNCF indicators for a specific outcome
   */
  getUNCFIndicatorsForOutcome(outcomeId: number): UNCFIndicator[] {
    return this.availableUNCFIndicators().get(outcomeId) || [];
  }

  /**
   * @description Load UNCF indicators for an outcome
   */
  loadUNCFIndicatorsForOutcome(outcomeId: number): void {
    const loadingSet = new Set(this.loadingUNCFIndicatorsForOutcome());
    loadingSet.add(outcomeId);
    this.loadingUNCFIndicatorsForOutcome.set(loadingSet);

    this.valuesService.getUNCFIndicators(outcomeId).subscribe({
      next: (indicators) => {
        const indicatorMap = new Map(this.availableUNCFIndicators());
        indicatorMap.set(outcomeId, indicators);
        this.availableUNCFIndicators.set(indicatorMap);

        const loadingSet = new Set(this.loadingUNCFIndicatorsForOutcome());
        loadingSet.delete(outcomeId);
        this.loadingUNCFIndicatorsForOutcome.set(loadingSet);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error(
          'Error loading UNCF indicators for outcome:',
          outcomeId,
          error,
        );
        const loadingSet = new Set(this.loadingUNCFIndicatorsForOutcome());
        loadingSet.delete(outcomeId);
        this.loadingUNCFIndicatorsForOutcome.set(loadingSet);
        this.cdr.detectChanges();
      },
    });
  }

  /**
   * @description Check if UNCF outcomes are being loaded
   */
  isLoadingUNCFOutcomes(): boolean {
    return this.loadingUNCFOutcomes();
  }

  /**
   * @description Check if UNCF indicators are being loaded for an outcome
   */
  isLoadingUNCFIndicatorsForOutcome(outcomeId: number): boolean {
    return this.loadingUNCFIndicatorsForOutcome().has(outcomeId);
  }

  /**
   * @description Get country name by ID
   */
  getCountryNameById(countryId: number): string {
    const country = this.opportunity().countries?.find(
      (c) => c.country?.id === countryId,
    );
    return country?.country?.name || 'Unknown Country';
  }

  /**
   * @description Set humanitarian framework alignment for a country
   */
  setFrameworkAlignment(countryId: number, value: boolean | null): void {
    const alignments = new Map(this.humanitarianFrameworkAlignments());
    alignments.set(countryId, value);
    this.humanitarianFrameworkAlignments.set(alignments);
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Get humanitarian framework alignment for a country
   */
  getFrameworkAlignment(countryId: number): boolean | null {
    return this.humanitarianFrameworkAlignments().get(countryId) ?? null;
  }

  /**
   * @description Set NDC alignment for a country
   */
  setNdcAlignment(countryId: number, value: boolean | null): void {
    const alignments = new Map(this.ndcAlignments());
    alignments.set(countryId, value);
    this.ndcAlignments.set(alignments);
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Get NDC alignment for a country
   */
  getNdcAlignment(countryId: number): boolean | null {
    return this.ndcAlignments().get(countryId) ?? null;
  }

  /**
   * @description Set NAP alignment for a country
   */
  setNapAlignment(countryId: number, value: boolean | null): void {
    const alignments = new Map(this.napAlignments());
    alignments.set(countryId, value);
    this.napAlignments.set(alignments);
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Get NAP alignment for a country
   */
  getNapAlignment(countryId: number): boolean | null {
    return this.napAlignments().get(countryId) ?? null;
  }

  /**
   * @description Set Organization Unit Strategy alignment for a country
   */
  setOrgUnitStrategyAlignment(countryId: number, value: boolean | null): void {
    const alignments = new Map(this.orgUnitStrategyAlignments());
    alignments.set(countryId, value);
    this.orgUnitStrategyAlignments.set(alignments);
    this.markAsChanged();
    this.cdr.detectChanges();
  }

  /**
   * @description Get Organization Unit Strategy alignment for a country
   */
  getOrgUnitStrategyAlignment(countryId: number): boolean | null {
    return this.orgUnitStrategyAlignments().get(countryId) ?? null;
  }
}
