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

// Services and Models
import { ValuesService, SDG, SDGTarget, SDGIndicator, UNCFOutcome, UNCFIndicator } from '@shared/services/api/values.service';
import { OpportunityService } from '../../../../../services/opportunity.service';
import { Opportunity, OpportunitySDG, OpportunitySDGTarget, OpportunitySDGIndicator, OpportunityUNCFOutcome, OpportunityUNCFIndicator, OpportunityCountry } from '@shared/models/opportunity.model';
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
  challengesControl = new FormControl<string | null>(null);

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

  // Computed properties
  readonly sdgCount = computed(() => this.opportunity().sdGs?.length || 0);
  readonly primarySDG = computed(() => 
    this.opportunity().sdGs?.find(sdg => sdg.isPrimary) || null
  );

  // UNCF data - country-specific outcomes
  readonly countriesWithUNCF = computed(() => {
    const countries = this.opportunity().countries || [];
    return countries.filter(c => c.country?.hasActiveUNSDCF);
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

  ngOnInit(): void {
    // Load SDGs on initialization
    this.loadSDGs();
    
    // Load UNCF outcomes for countries with active UNSDCF
    this.loadUNCFOutcomesForCountries();
    
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
   * @description Start editing the section
   */
  startEditing(): void {
    const opp = this.opportunity();
    
    // Set form controls
    this.strategicAlignmentControl.setValue(opp.strategicAlignment ?? null);
    this.expectedBeneficiariesControl.setValue(opp.expectedBeneficiaries ?? null);
    this.expectedOutcomesControl.setValue(opp.intendedImpactOutcomes ?? null);
    this.challengesControl.setValue(opp.challenges ?? null);

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
    this.challengesControl.setValue(opp.challenges ?? null);
    
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
  removeUNCFForCountry(oppCountry: OpportunityCountry): void {
    const opp = this.opportunity();
    const currentUNCFOutcomes = [...(opp.uncfOutcomes || [])];

    // Remove outcomes for this country
    const updatedUNCFOutcomes = currentUNCFOutcomes.filter(
      uo => uo.opportunityCountryId !== oppCountry.id
    );

    const updatedOpportunity = {
      ...opp,
      uncfOutcomes: updatedUNCFOutcomes
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);
    this.cdr.detectChanges();
  }

  /**
   * @description Get UNCF outcomes for a specific opportunity country
   */
  getUNCFOutcomesForOpportunityCountry(oppCountryId: number): OpportunityUNCFOutcome[] {
    return this.opportunity().uncfOutcomes?.filter(
      uo => uo.opportunityCountryId === oppCountryId
    ) || [];
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
}

