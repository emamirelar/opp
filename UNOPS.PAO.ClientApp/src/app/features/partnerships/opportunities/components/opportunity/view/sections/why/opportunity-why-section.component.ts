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
import { CheckboxModule } from 'primeng/checkbox';
import { AccordionModule } from 'primeng/accordion';
import { InputNumberModule } from 'primeng/inputnumber';
import { FloatLabelModule } from 'primeng/floatlabel';

// Services and Models
import { ValuesService, SDG, SDGTarget, SDGIndicator, UNCFOutcome, UNCFIndicator } from '@shared/services/api/values.service';
import { OpportunityService } from '../../../../../services/opportunity.service';
import { Opportunity, OpportunitySDG, OpportunitySDGTarget, OpportunitySDGIndicator, OpportunityUNCFOutcome, OpportunityUNCFIndicator, OpportunityCountry, UNOPSMission, OpportunityUNOPSMission } from '@shared/models/opportunity.model';
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
    CheckboxModule,
    AccordionModule,
    InputNumberModule,
    FloatLabelModule,
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
    strategicAlignment?: string | null;
    expectedBeneficiaries?: string | null;
    intendedImpactOutcomes?: string | null;
    challenges?: string | null;
    sdGs?: any[];
  } | null = null;

  // Form controls for WHY section
  strategicAlignmentControl = new FormControl<string | null>(null);
  expectedBeneficiariesControl = new FormControl<string | null>(null);
  estimatedDirectBeneficiariesControl = new FormControl<number | null>(null);
  estimatedIndirectBeneficiariesControl = new FormControl<number | null>(null);
  beneficiariesToBeDeterminedControl = new FormControl<boolean>(false);
  expectedOutcomesControl = new FormControl<string | null>(null);
  challengesControl = new FormControl<string | null>(null);
  
  // Climate and framework alignments by country (map of countryId -> alignment status)
  humanitarianFrameworkAlignments = signal<Map<number, boolean | null>>(new Map());
  ndcAlignments = signal<Map<number, boolean | null>>(new Map());
  napAlignments = signal<Map<number, boolean | null>>(new Map());
  orgUnitStrategyAlignments = signal<Map<number, boolean | null>>(new Map());

  // SDG data
  sdgs = signal<SDG[]>([]);
  availableTargets = signal<SDGTarget[]>([]);
  availableIndicators = signal<SDGIndicator[]>([]);
  loadingTargets = signal<boolean>(false);
  
  // SDG dialog
  showSDGDialog = signal<boolean>(false);
  sdgControl = new FormControl<SDG | null>(null);
  isPrimaryControl = new FormControl<boolean>(false);
  skipTargetsControl = new FormControl<boolean>(false);
  isEditingSDG = signal<boolean>(false);
  editingSDGIndex = signal<number | null>(null);
  
  // Selected targets and indicators for the current SDG being added/edited
  selectedTargets = signal<Map<number, Set<number>>>(new Map());  // Map<targetId, Set<indicatorIds>>
  
  // Track which targets are currently loading indicators
  loadingIndicatorsForTargets = signal<Set<number>>(new Set());
  showValidationError = signal<boolean>(false);
  
  // UNOPS Missions data
  unopsMissions = signal<UNOPSMission[]>([]);
  selectedUNOPSMissions = signal<Set<number>>(new Set());
  showUNOPSMissionsDialog = signal<boolean>(false);

  // Computed properties
  readonly sdgCount = computed(() => this.opportunity().sdGs?.length || 0);
  readonly primarySDG = computed(() => 
    this.opportunity().sdGs?.find(sdg => sdg.isPrimary) || null
  );

  constructor() {
    // Set up change detection on form controls
    // Only mark as changed if we're in edit mode (to avoid triggering on initial setValue)
    this.strategicAlignmentControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.expectedBeneficiariesControl.valueChanges.subscribe(() => {
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
    
    return countries.filter(c => {
      // Show country if it has active UNCF Metadata
      if (c.country?.hasActiveUNCF) {
        return true;
      }
      
      // Also show country if it has existing OpportunityUNCFOutcomes
      const hasExistingOutcomes = existingUNCFOutcomes.some(uo => uo.opportunityCountryId === c.id);
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
  selectedUNCFOutcomes = signal<Map<number, Set<number>>>(new Map());  // Map<outcomeId, Set<indicatorIds>>
  
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
    return countries.filter(c => c.hasHumanitarianFramework);
  });
  
  // Computed property for countries without humanitarian framework
  readonly countriesWithoutFramework = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter(c => !c.hasHumanitarianFramework && c.country);
  });
  
  // Computed property for countries with NDC
  readonly countriesWithNdc = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter(c => c.hasNdc);
  });
  
  // Computed property for countries without NDC
  readonly countriesWithoutNdc = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter(c => !c.hasNdc && c.country);
  });
  
  // Computed property for countries with NAP
  readonly countriesWithNap = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter(c => c.hasNap);
  });
  
  // Computed property for countries without NAP
  readonly countriesWithoutNap = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter(c => !c.hasNap && c.country);
  });
  
  // Computed property for countries with Organization Unit Strategy
  readonly countriesWithOrgUnitStrategy = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter(c => c.hasOrgUnitStrategy);
  });
  
  // Computed property for countries without Organization Unit Strategy
  readonly countriesWithoutOrgUnitStrategy = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter(c => !c.hasOrgUnitStrategy && c.country);
  });

  ngOnInit(): void {
    // Load SDGs on initialization
    this.loadSDGs();
    
    // Load UNOPS Missions
    this.loadUNOPSMissions();
    
    // Load UNCF outcomes for countries with active UNSDCF
    this.loadUNCFOutcomesForCountries();
    
    // Watch for changes to beneficiariesToBeDetermined checkbox
    this.beneficiariesToBeDeterminedControl.valueChanges.subscribe((toBeDetermined) => {
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
      this.cdr.detectChanges();
    });
    
    // Watch for changes to skipTargetsControl
    this.skipTargetsControl.valueChanges.subscribe((skipValue) => {
      // If user unchecks the skip option, load targets for the current SDG
      if (!skipValue && this.sdgControl.value) {
        const currentSDG = this.sdgControl.value;
        if (currentSDG.sdgId) {
          console.log('Skip checkbox unchecked, loading targets for SDG:', currentSDG.sdgId);
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
                    const indicatorRequests = sdg.targets.map(target => 
                      this.valuesService.getSDGIndicators(target.sdgTargetId)
                    );
                    
                    if (indicatorRequests.length > 0) {
                      import('rxjs').then(rxjs => {
                        rxjs.forkJoin(indicatorRequests).subscribe({
                          next: (allIndicators) => {
                            const flatIndicators = allIndicators.flat();
                            this.availableIndicators.set(flatIndicators);
                            
                            // Pre-select targets and indicators
                            sdg.targets!.forEach(target => {
                              const indicatorIds = new Set<number>();
                              target.indicators?.forEach(indicator => {
                                indicatorIds.add(indicator.sdgIndicatorDatabaseId);
                              });
                              selectedTargetsMap.set(target.sdgTargetDatabaseId, indicatorIds);
                            });
                            
                            this.selectedTargets.set(selectedTargetsMap);
                            this.cdr.detectChanges();
                          },
                          error: (error) => {
                            console.error('Error loading indicators:', error);
                          }
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
            }
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
      }
    });
  }

  /**
   * @description Load UNOPS Missions from values service
   */
  private loadUNOPSMissions(): void {
    this.valuesService.getUNOPSMissions().subscribe({
      next: (data) => {
        this.unopsMissions.set(data);
        
        // Initialize selected missions from opportunity
        const opp = this.opportunity();
        if (opp.unopsMissions) {
          const selectedIds = new Set(opp.unopsMissions.map(m => m.unopsMissionId));
          this.selectedUNOPSMissions.set(selectedIds);
        }
        
        this.cdr.detectChanges();
      }
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
    }
    this.selectedUNOPSMissions.set(selected);
    this.cdr.detectChanges();
  }

  /**
   * @description Check if UNOPS Mission is selected
   */
  isUNOPSMissionSelected(missionId: number): boolean {
    return this.selectedUNOPSMissions().has(missionId);
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
    
    return allMissions.filter(mission => selectedIds.has(mission.id));
  });

  /**
   * @description Toggle "Not Applicable" for UNOPS Missions
   */
  toggleUNOPSNotApplicable(checked: boolean): void {
    if (checked && this.unopsMissionCount() > 0) {
      this.selectedUNOPSMissions.set(new Set());
      this.cdr.detectChanges();
    }
  }

  /**
   * @description Open UNOPS Missions dialog
   */
  openUNOPSMissionsDialog(): void {
    this.showUNOPSMissionsDialog.set(true);
  }

  /**
   * @description Confirm UNOPS Missions dialog selections (doesn't save to database, just closes dialog)
   */
  saveUNOPSMissionsDialog(): void {
    // Selections are already tracked in selectedUNOPSMissions signal
    // Just close the dialog - actual save happens when user saves the WHY section
    this.showUNOPSMissionsDialog.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel UNOPS Missions dialog
   */
  cancelUNOPSMissionsDialog(): void {
    // Reset selections to current opportunity's missions
    const opp = this.opportunity();
    if (opp.unopsMissions) {
      const selectedIds = new Set(opp.unopsMissions.map(m => m.unopsMissionId));
      this.selectedUNOPSMissions.set(selectedIds);
    } else {
      this.selectedUNOPSMissions.set(new Set());
    }
    this.showUNOPSMissionsDialog.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Start editing the section
   */
  startEditing(): void {
    const opp = this.opportunity();
    
    // Backup original data for cancel
    this.originalData = {
      strategicAlignment: opp.strategicAlignment ?? null,
      expectedBeneficiaries: opp.expectedBeneficiaries ?? null,
      intendedImpactOutcomes: opp.intendedImpactOutcomes ?? null,
      challenges: opp.challenges ?? null,
      sdGs: opp.sdGs ? [...opp.sdGs] : []
    };
    
    // Set form controls
    this.strategicAlignmentControl.setValue(opp.strategicAlignment ?? null);
    this.expectedBeneficiariesControl.setValue(opp.expectedBeneficiaries ?? null);
    this.estimatedDirectBeneficiariesControl.setValue(opp.estimatedDirectBeneficiaries ?? null);
    this.estimatedIndirectBeneficiariesControl.setValue(opp.estimatedIndirectBeneficiaries ?? null);
    this.beneficiariesToBeDeterminedControl.setValue(opp.beneficiariesToBeDetermined ?? false);
    
    // If beneficiariesToBeDetermined is true, disable the number fields
    if (opp.beneficiariesToBeDetermined) {
      this.estimatedDirectBeneficiariesControl.disable();
      this.estimatedIndirectBeneficiariesControl.disable();
    } else {
      this.estimatedDirectBeneficiariesControl.enable();
      this.estimatedIndirectBeneficiariesControl.enable();
    }
    
    this.expectedOutcomesControl.setValue(opp.intendedImpactOutcomes ?? null);
    this.challengesControl.setValue(opp.challenges ?? null);
    
    // Initialize climate and framework alignments from countries
    const frameworkAlignments = new Map<number, boolean | null>();
    const ndcAlignments = new Map<number, boolean | null>();
    const napAlignments = new Map<number, boolean | null>();
    const orgUnitStrategyAlignments = new Map<number, boolean | null>();
    
    opp.countries?.forEach(country => {
      frameworkAlignments.set(country.countryId, country.humanitarianFrameworkAlignment ?? null);
      ndcAlignments.set(country.countryId, country.ndcAlignment ?? null);
      napAlignments.set(country.countryId, country.napAlignment ?? null);
      orgUnitStrategyAlignments.set(country.countryId, country.orgUnitStrategyAlignment ?? null);
    });
    
    this.humanitarianFrameworkAlignments.set(frameworkAlignments);
    this.ndcAlignments.set(ndcAlignments);
    this.napAlignments.set(napAlignments);
    this.orgUnitStrategyAlignments.set(orgUnitStrategyAlignments);
    
    // Initialize selected UNOPS Missions
    if (opp.unopsMissions) {
      const selectedIds = new Set(opp.unopsMissions.map(m => m.unopsMissionId));
      this.selectedUNOPSMissions.set(selectedIds);
    }

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

    const whyData = {
      strategicAlignment: this.strategicAlignmentControl.value ?? undefined,
      expectedBeneficiaries: this.expectedBeneficiariesControl.value ?? undefined,
      estimatedDirectBeneficiaries: this.estimatedDirectBeneficiariesControl.value ?? undefined,
      estimatedIndirectBeneficiaries: this.estimatedIndirectBeneficiariesControl.value ?? undefined,
      beneficiariesToBeDetermined: this.beneficiariesToBeDeterminedControl.value ?? false,
      intendedImpactOutcomes: this.expectedOutcomesControl.value ?? undefined,
      challenges: this.challengesControl.value ?? undefined,
      sdGs: opp.sdGs?.map(sdg => ({
        sdgId: sdg.sdgDatabaseId || 0,  // Use the integer database ID
        isPrimary: sdg.isPrimary,
        skipTargetsAndIndicators: sdg.skipTargetsAndIndicators,
        notes: sdg.notes,
        targets: sdg.skipTargetsAndIndicators ? [] : (sdg.targets?.map(target => ({
          sdgTargetDatabaseId: target.sdgTargetDatabaseId,  // Correct property name for backend
          notes: target.notes,
          sdgIndicatorDatabaseIds: target.indicators?.map(indicator => indicator.sdgIndicatorDatabaseId) || []  // Flat array of indicator IDs
        })) || [])
      })),
      uncfOutcomes: opp.uncfOutcomes?.map(uncfOutcome => ({
        opportunityCountryId: uncfOutcome.opportunityCountryId,
        uncfOutcomeId: uncfOutcome.uncfOutcomeId,  // Use the integer database ID
        notes: uncfOutcome.notes,
        uncfIndicatorIds: uncfOutcome.indicators?.map(indicator => indicator.uncfIndicatorId) || []  // Flat array of indicator IDs
      })),
      unopsMissions: Array.from(this.selectedUNOPSMissions()).map(missionId => ({
        unopsMissionId: missionId
      }))
    };
    
    // Prepare WHERE data with updated climate and framework alignments
    const whereData = {
      countries: opp.countries?.map(country => ({
        countryId: country.countryId,
        specificAreas: country.specificAreas,
        humanitarianFrameworkAlignment: this.humanitarianFrameworkAlignments().get(country.countryId) ?? null,
        ndcAlignment: this.ndcAlignments().get(country.countryId) ?? null,
        napAlignment: this.napAlignments().get(country.countryId) ?? null,
        orgUnitStrategyAlignment: this.orgUnitStrategyAlignments().get(country.countryId) ?? null
      })) || []
    };

    this.isSaving.set(true);
    
    // Update WHY section first, then WHERE section for humanitarian framework alignments
    this.opportunityService.updateOpportunityWhy(opp.id, whyData).subscribe({
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
    const opp = this.opportunity();
    
    // Restore original data if available
    if (this.originalData) {
      // Reset form controls to original values
      this.strategicAlignmentControl.setValue(this.originalData.strategicAlignment ?? null);
      this.expectedBeneficiariesControl.setValue(this.originalData.expectedBeneficiaries ?? null);
      this.expectedOutcomesControl.setValue(this.originalData.intendedImpactOutcomes ?? null);
      this.challengesControl.setValue(this.originalData.challenges ?? null);
      
      // Restore original SDGs (reverts any SDGs that were added but not saved)
      const updatedOpportunity = {
        ...opp,
        strategicAlignment: this.originalData.strategicAlignment ?? null,
        expectedBeneficiaries: this.originalData.expectedBeneficiaries ?? null,
        intendedImpactOutcomes: this.originalData.intendedImpactOutcomes ?? null,
        challenges: this.originalData.challenges ?? null,
        sdGs: this.originalData.sdGs ? [...this.originalData.sdGs] : []
      };
      
      // Emit the reverted opportunity to parent
      this.opportunityUpdated.emit(updatedOpportunity);
    } else {
      // Fallback: just reset form controls to current opportunity values
      this.strategicAlignmentControl.setValue(opp.strategicAlignment ?? null);
      this.expectedBeneficiariesControl.setValue(opp.expectedBeneficiaries ?? null);
      this.expectedOutcomesControl.setValue(opp.intendedImpactOutcomes ?? null);
      this.challengesControl.setValue(opp.challenges ?? null);
    }

    this.estimatedDirectBeneficiariesControl.setValue(opp.estimatedDirectBeneficiaries ?? null);
    this.estimatedIndirectBeneficiariesControl.setValue(opp.estimatedIndirectBeneficiaries ?? null);
    this.beneficiariesToBeDeterminedControl.setValue(opp.beneficiariesToBeDetermined ?? false);

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
    
    opp.countries?.forEach(country => {
      frameworkAlignments.set(country.countryId, country.humanitarianFrameworkAlignment ?? null);
      ndcAlignments.set(country.countryId, country.ndcAlignment ?? null);
      napAlignments.set(country.countryId, country.napAlignment ?? null);
      orgUnitStrategyAlignments.set(country.countryId, country.orgUnitStrategyAlignment ?? null);
    });
    
    this.humanitarianFrameworkAlignments.set(frameworkAlignments);
    this.ndcAlignments.set(ndcAlignments);
    this.napAlignments.set(napAlignments);
    this.orgUnitStrategyAlignments.set(orgUnitStrategyAlignments);
    
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
    if (sdg && sdg.sdgId) {
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
        }
      });
    } else {
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
    this.sdgControl.setValue(null);
    this.isPrimaryControl.setValue(false);
    this.skipTargetsControl.setValue(false);
    this.showValidationError.set(false);
    this.isEditingSDG.set(false);
    this.editingSDGIndex.set(null);
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
    const sdg = this.sdgs().find(s => s.sdgId === sdgId);
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
      console.log('📡 API URL:', `/api/values/sdg-indicators?targetId=${target.sdgTargetId}`);
      
      this.valuesService.getSDGIndicators(target.sdgTargetId).subscribe({
        next: (indicators) => {
          // Remove from loading set
          const loadingSet = new Set(this.loadingIndicatorsForTargets());
          loadingSet.delete(target.id);
          this.loadingIndicatorsForTargets.set(loadingSet);
          
          console.log('✅ Loaded indicators:', indicators);
          console.log('📊 Indicator count:', indicators.length, 'for target:', target.sdgTargetId);
          
          if (indicators.length === 0) {
            console.warn('⚠️ No indicators found for target:', target.sdgTargetId);
            console.warn('⚠️ This could mean:');
            console.warn('   1. The SDGIndicators table does not exist (migration not run)');
            console.warn('   2. The SDGIndicatorSeeder has not been run');
            console.warn('   3. No indicators exist for this target in the database');
          }
          
          // Store or update available indicators (append to existing)
          const current = this.availableIndicators();
          const combined = [...current, ...indicators];
          // Remove duplicates
          const unique = combined.filter((indicator, index, self) => 
            index === self.findIndex(i => i.id === indicator.id)
          );
          console.log('📈 Total available indicators after loading:', unique.length);
          this.availableIndicators.set(unique);
          this.cdr.detectChanges();
        },
        error: (error) => {
          // Remove from loading set
          const loadingSet = new Set(this.loadingIndicatorsForTargets());
          loadingSet.delete(target.id);
          this.loadingIndicatorsForTargets.set(loadingSet);
          
          console.error('❌ Error loading SDG indicators for target', target.sdgTargetId);
          console.error('Error details:', error);
          console.error('Status:', error.status);
          console.error('Message:', error.message);
          
          if (error.status === 404) {
            console.error('🔴 404 Error - API endpoint not found. Check if the backend is running.');
          } else if (error.status === 500) {
            console.error('🔴 500 Error - Server error. Check if the SDGIndicators table exists in the database.');
          } else if (error.status === 0) {
            console.error('🔴 Network Error - Cannot reach the backend. Check if the backend is running.');
          }
          
          // Show error feedback to user
          this.feedbackService.showErrorToast({
            summary: 'Error Loading Indicators',
            detail: `Failed to load indicators for target ${target.sdgTargetId}. Please check the console for details.`
          });
          
          this.cdr.detectChanges();
        }
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
    return this.availableIndicators().filter(i => i.sdgTargetId === targetId);
  }

  /**
   * @description Add SDG to the list
   */
  addSDG(): void {
    const sdg = this.sdgControl.value;
    const isPrimary = this.isPrimaryControl.value || false;
    const skipTargets = this.skipTargetsControl.value || false;

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

    // Build targets array from selected targets and indicators (only if not skipped)
    const targets: OpportunitySDGTarget[] = [];
    const selectedTargetsMap = skipTargets ? new Map() : this.selectedTargets();
    
    for (const [targetDatabaseId, indicatorIds] of selectedTargetsMap.entries()) {
      const targetInfo = this.availableTargets().find(t => t.id === targetDatabaseId);
      if (targetInfo) {
        const indicators: OpportunitySDGIndicator[] = [];
        
        // Add selected indicators for this target
        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.availableIndicators().find(i => i.id === indicatorId);
          if (indicatorInfo) {
            indicators.push({
              id: 0,
              opportunityId: opp.id!,
              opportunitySDGTargetId: 0,  // Will be set by backend
              sdgIndicatorDatabaseId: indicatorInfo.id,
              sdgIndicatorId: indicatorInfo.sdgIndicatorId,
              sdgIndicatorLongDescription: indicatorInfo.sdgIndicatorLongDescription,
              notes: null
            });
          }
        }
        
        targets.push({
          id: 0,
          opportunityId: opp.id!,
          opportunitySDGId: 0,  // Will be set by backend
          sdgTargetDatabaseId: targetInfo.id,
          sdgTargetId: targetInfo.sdgTargetId,
          targetDescription: targetInfo.targetDescription,
          targetType: targetInfo.targetType,
          notes: null,
          indicators: indicators
        });
      }
    }
    
    // Add new SDG with targets and indicators
    const newSDG: OpportunitySDG = {
      id: 0,
      opportunityId: opp.id!,
      sdgId: sdg.sdgId || '',
      sdgDatabaseId: sdg.id,  // Store the integer database ID for saving
      sdgNumber: sdg.sdgNumber || '',
      sdgName: sdg.name,
      isPrimary: isPrimary,
      skipTargetsAndIndicators: skipTargets || null,
      notes: null,
      targets: targets
    };

    currentSDGs.push(newSDG);

    // Update opportunity
    const updatedOpportunity = {
      ...opp,
      sdGs: currentSDGs
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);
    
    // Mark as changed (SDG added)
    this.markAsChanged();

    // Reset dialog state
    this.showSDGDialog.set(false);
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
            const indicatorRequests = sdg.targets.map(target => 
              this.valuesService.getSDGIndicators(target.sdgTargetId)
            );
            
            // Combine all indicator requests
            if (indicatorRequests.length > 0) {
              // Use forkJoin to load all indicators
              import('rxjs').then(rxjs => {
                rxjs.forkJoin(indicatorRequests).subscribe({
                  next: (allIndicators) => {
                    // Flatten all indicators
                    const flatIndicators = allIndicators.flat();
                    this.availableIndicators.set(flatIndicators);
                    
                    // Now pre-select the targets and indicators
                    sdg.targets!.forEach(target => {
                      const indicatorIds = new Set<number>();
                      if (target.indicators && target.indicators.length > 0) {
                        target.indicators.forEach(ind => indicatorIds.add(ind.sdgIndicatorDatabaseId));
                      }
                      selectedTargetsMap.set(target.sdgTargetDatabaseId, indicatorIds);
                    });
                    
                    this.selectedTargets.set(selectedTargetsMap);
                    this.cdr.detectChanges();
                  }
                });
              });
            } else {
              // No indicators to load, just select targets
              sdg.targets.forEach(target => {
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
        }
      });
    }
    
    this.showSDGDialog.set(true);
    this.cdr.detectChanges();
  }

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
    
    for (const [targetDatabaseId, indicatorIds] of selectedTargetsMap.entries()) {
      const targetInfo = this.availableTargets().find(t => t.id === targetDatabaseId);
      if (targetInfo) {
        const indicators: OpportunitySDGIndicator[] = [];
        
        // Add selected indicators for this target
        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.availableIndicators().find(i => i.id === indicatorId);
          if (indicatorInfo) {
            indicators.push({
              id: 0,
              opportunityId: opp.id!,
              opportunitySDGTargetId: 0,  // Will be set by backend
              sdgIndicatorDatabaseId: indicatorInfo.id,
              sdgIndicatorId: indicatorInfo.sdgIndicatorId,
              sdgIndicatorLongDescription: indicatorInfo.sdgIndicatorLongDescription,
              notes: null
            });
          }
        }
        
        targets.push({
          id: 0,
          opportunityId: opp.id!,
          opportunitySDGId: 0,  // Will be set by backend
          sdgTargetDatabaseId: targetInfo.id,
          sdgTargetId: targetInfo.sdgTargetId,
          targetDescription: targetInfo.targetDescription,
          targetType: targetInfo.targetType,
          notes: null,
          indicators: indicators
        });
      }
    }

    // Update SDG with targets and indicators
    currentSDGs[index] = {
      ...currentSDGs[index],
      sdgId: sdg.sdgId || '',
      sdgDatabaseId: sdg.id,  // Store the integer database ID for saving
      sdgNumber: sdg.sdgNumber || '',
      sdgName: sdg.name,
      isPrimary: isPrimary,
      skipTargetsAndIndicators: skipTargets || null,
      targets: targets
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
      sdGs: currentSDGs
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);
    
    // Mark as changed (SDG removed)
    this.markAsChanged();
    
    this.cdr.detectChanges();
  }

  /**
   * @description Get chip style based on primary/secondary status
   */
  getSDGChipStyle(isPrimary: boolean): any {
    if (isPrimary) {
      return { 'background-color': 'var(--p-badge-success-background)', 'color': 'var(--p-badge-success-color)', 'border-radius': '8px' };
    }
    return { 'background-color': 'var(--p-badge-info-background)', 'color': 'var(--p-badge-info-color)', 'border-radius': '8px' };
  }

  /**
   * @description Get chip label for primary/secondary
   */
  getSDGChipLabel(isPrimary: boolean): string {
    return isPrimary ? 'Primary' : 'Secondary';
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
      this.valuesService.getUNCFOutcomes(oppCountry.country.iso2Code).subscribe({
        next: (outcomes) => {
          this.availableUNCFOutcomes.set(outcomes);
          this.loadingUNCFOutcomesForDialog.set(false);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading UNCF outcomes:', error);
          this.loadingUNCFOutcomesForDialog.set(false);
          this.availableUNCFOutcomes.set([]);
        }
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
      this.valuesService.getUNCFOutcomes(oppCountry.country.iso2Code).subscribe({
        next: (outcomes) => {
          this.availableUNCFOutcomes.set(outcomes);
          this.loadingUNCFOutcomesForDialog.set(false);
          
          // Pre-select existing outcomes and indicators
          const opp = this.opportunity();
          const existingUNCFOutcomes = opp.uncfOutcomes?.filter(
            uo => uo.opportunityCountryId === oppCountry.id
          ) || [];
          
          if (existingUNCFOutcomes.length > 0) {
            const selectedMap = new Map<number, Set<number>>();
            
            // Load indicators for all existing outcomes
            const indicatorRequests = existingUNCFOutcomes.map(uo => 
              this.valuesService.getUNCFIndicators(uo.uncfOutcomeId)
            );
            
            if (indicatorRequests.length > 0) {
              import('rxjs').then(rxjs => {
                rxjs.forkJoin(indicatorRequests).subscribe({
                  next: (allIndicators) => {
                    const flatIndicators = allIndicators.flat();
                    
                    // Store indicators for each outcome
                    existingUNCFOutcomes.forEach((uo, idx) => {
                      const outcomeIndicators = allIndicators[idx];
                      const indicatorMap = new Map(this.availableUNCFIndicators());
                      indicatorMap.set(uo.uncfOutcomeId, outcomeIndicators);
                      this.availableUNCFIndicators.set(indicatorMap);
                      
                      // Pre-select indicators
                      const indicatorIds = new Set<number>();
                      uo.indicators?.forEach(ind => {
                        indicatorIds.add(ind.uncfIndicatorId);
                      });
                      selectedMap.set(uo.uncfOutcomeId, indicatorIds);
                    });
                    
                    this.selectedUNCFOutcomes.set(selectedMap);
                    this.cdr.detectChanges();
                  }
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
        }
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
          console.error('Error loading UNCF indicators for outcome:', outcome.id, error);
          const loadingSet = new Set(this.loadingIndicatorsForUNCFOutcomes());
          loadingSet.delete(outcome.id);
          this.loadingIndicatorsForUNCFOutcomes.set(loadingSet);
          this.cdr.detectChanges();
        }
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
      const outcomeInfo = this.availableUNCFOutcomes().find(o => o.id === outcomeId);
      if (outcomeInfo) {
        const indicators: OpportunityUNCFIndicator[] = [];
        
        // Add selected indicators for this outcome
        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.getIndicatorsForUNCFOutcome(outcomeId).find(i => i.id === indicatorId);
          if (indicatorInfo) {
            indicators.push({
              id: 0,
              opportunityId: opp.id!,
              opportunityUNCFOutcomeId: 0,  // Will be set by backend
              uncfIndicatorId: indicatorInfo.id,
              uncfIndicatorExternalId: indicatorInfo.uncfIndicatorExternalId,
              uncfIndicatorName: indicatorInfo.name,
              notes: null
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
          indicators: indicators
        });
      }
    }
    
    // Add new UNCF outcomes
    currentUNCFOutcomes.push(...outcomes);

    // Update opportunity
    const updatedOpportunity = {
      ...opp,
      uncfOutcomes: currentUNCFOutcomes
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

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
      uo => uo.opportunityCountryId !== selectedCountry.id
    );

    // Build new outcomes array
    const outcomes: OpportunityUNCFOutcome[] = [];
    
    for (const [outcomeId, indicatorIds] of selectedOutcomes.entries()) {
      const outcomeInfo = this.availableUNCFOutcomes().find(o => o.id === outcomeId);
      if (outcomeInfo) {
        const indicators: OpportunityUNCFIndicator[] = [];
        
        for (const indicatorId of indicatorIds) {
          const indicatorInfo = this.getIndicatorsForUNCFOutcome(outcomeId).find(i => i.id === indicatorId);
          if (indicatorInfo) {
            indicators.push({
              id: 0,
              opportunityId: opp.id!,
              opportunityUNCFOutcomeId: 0,
              uncfIndicatorId: indicatorInfo.id,
              uncfIndicatorExternalId: indicatorInfo.uncfIndicatorExternalId,
              uncfIndicatorName: indicatorInfo.name,
              notes: null
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
          indicators: indicators
        });
      }
    }
    
    // Add updated outcomes
    currentUNCFOutcomes.push(...outcomes);

    // Update opportunity
    const updatedOpportunity = {
      ...opp,
      uncfOutcomes: currentUNCFOutcomes
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Reset dialog state
    this.cancelUNCFDialog();
  }

  /**
   * @description Remove UNCF outcomes for a country
   */
  /**
   * @description Get UNCF outcomes for a specific opportunity country
   */
  getUNCFOutcomesForOpportunityCountry(oppCountryId: number): OpportunityUNCFOutcome[] {
    return this.opportunity().uncfOutcomes?.filter(
      uo => uo.opportunityCountryId === oppCountryId
    ) || [];
  }

  /**
   * @description Check if a specific country has inactive UNCF data with newer versions available
   */
  hasInactiveUNCFWithUpdatesForCountry(oppCountryId: number): boolean {
    const outcomesForCountry = this.getUNCFOutcomesForOpportunityCountry(oppCountryId);
    
    // Check if any outcome for this country is inactive with newer version
    const hasOutcomeWithUpdates = outcomesForCountry.some(outcome => 
      outcome.isInactive && outcome.hasNewerVersion
    );
    
    if (hasOutcomeWithUpdates) return true;
    
    // Check if any indicator for this country is inactive with newer version
    const hasIndicatorWithUpdates = outcomesForCountry.some(outcome =>
      outcome.indicators?.some(indicator => 
        indicator.isInactive && indicator.hasNewerVersion
      )
    );
    
    return hasIndicatorWithUpdates;
  }

  /**
   * @description Check if a specific country has inactive UNCF data without newer versions available
   */
  hasInactiveUNCFWithoutUpdatesForCountry(oppCountryId: number): boolean {
    const outcomesForCountry = this.getUNCFOutcomesForOpportunityCountry(oppCountryId);
    
    // Check if any outcome for this country is inactive without newer version
    const hasOutcomeWithoutUpdates = outcomesForCountry.some(outcome => 
      outcome.isInactive && !outcome.hasNewerVersion
    );
    
    if (hasOutcomeWithoutUpdates) return true;
    
    // Check if any indicator for this country is inactive without newer version
    const hasIndicatorWithoutUpdates = outcomesForCountry.some(outcome =>
      outcome.indicators?.some(indicator => 
        indicator.isInactive && !indicator.hasNewerVersion
      )
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

    countries.forEach(oppCountry => {
      if (!oppCountry.country?.iso2Code) return;
      
      this.valuesService.getUNCFOutcomes(oppCountry.country.iso2Code).subscribe({
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
          console.error('Error loading UNCF outcomes for country:', oppCountry.country?.name, error);
          loadedCount++;
          if (loadedCount === countries.length) {
            this.uncfOutcomesByCountry.set(outcomeMap);
            this.loadingUNCFOutcomes.set(false);
            this.cdr.detectChanges();
          }
        }
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
        console.error('Error loading UNCF indicators for outcome:', outcomeId, error);
        const loadingSet = new Set(this.loadingUNCFIndicatorsForOutcome());
        loadingSet.delete(outcomeId);
        this.loadingUNCFIndicatorsForOutcome.set(loadingSet);
        this.cdr.detectChanges();
      }
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
    const country = this.opportunity().countries?.find(c => c.country?.id === countryId);
    return country?.country?.name || 'Unknown Country';
  }
  
  /**
   * @description Set humanitarian framework alignment for a country
   */
  setFrameworkAlignment(countryId: number, value: boolean | null): void {
    const alignments = new Map(this.humanitarianFrameworkAlignments());
    alignments.set(countryId, value);
    this.humanitarianFrameworkAlignments.set(alignments);
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
    this.cdr.detectChanges();
  }
  
  /**
   * @description Get Organization Unit Strategy alignment for a country
   */
  getOrgUnitStrategyAlignment(countryId: number): boolean | null {
    return this.orgUnitStrategyAlignments().get(countryId) ?? null;
  }
}

