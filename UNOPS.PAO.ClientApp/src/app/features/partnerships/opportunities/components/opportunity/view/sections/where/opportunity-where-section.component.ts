/**
 * @fileoverview WHERE section component for opportunity geographic implementation management
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  input,
  output,
  signal,
  computed,
  inject,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  OnInit
} from '@angular/core';
import { CommonModule, KeyValuePipe } from '@angular/common';
import { FormsModule, FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { BadgeModule } from 'primeng/badge';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { FloatLabelModule } from 'primeng/floatlabel';
import { TooltipModule } from 'primeng/tooltip';
import { InputTextModule } from 'primeng/inputtext';
import { TabsModule } from 'primeng/tabs';
import { AccordionModule } from 'primeng/accordion';
import { Opportunity, OpportunityCountry } from '@shared/models/opportunity.model';
import { OpportunityService } from '@features/partnerships/opportunities/services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { Router } from '@angular/router';
import { EntityTagsComponent } from '@shared/components/data-display/entity-tags/entity-tags.component';
import {
  ValuesService,
  SimpleValue,
  CountryDynamicSearchRequest,
  CountryDynamicSearchResponse,
  CountrySearchResult
} from '@shared/services/api/values.service';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';

/**
 * @class OpportunityWhereSectionComponent
 * @description Component for managing opportunity implementation countries
 * 
 * @example
 * ```html
 * <app-opportunity-where-section
 *   [opportunity]="opportunity()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-where-section',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    DividerModule,
    BadgeModule,
    DialogModule,
    SelectModule,
    MessageModule,
    FloatLabelModule,
    TooltipModule,
    InputTextModule,
    TabsModule,
    AccordionModule,
    EntityTagsComponent
  ],
  templateUrl: './opportunity-where-section.component.html',
  styleUrls: ['./opportunity-where-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityWhereSectionComponent implements OnInit {
  private readonly opportunityService = inject(OpportunityService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly translateService = inject(TranslateService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);
  private readonly valuesService = inject(ValuesService);

  // Inputs
  readonly opportunity = input.required<Opportunity>();
  readonly suggestions = input<any[]>([]);
  
  /**
   * @description Input signal for update permission - controls visibility of edit button
   */
  readonly canUpdate = input<boolean>(false);

  // Outputs
  readonly opportunityUpdated = output<Opportunity>();
  readonly changesDetected = output<void>();
  readonly changesSavedOrDiscarded = output<void>();

  // State signals
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  readonly hasUnsavedChangesSignal = signal<boolean>(false);
  private originalData: {
    countries?: any[];
  } | null = null;
  
  // Country dialog state
  readonly showCountryDialog = signal(false);
  readonly showValidationError = signal(false);

  // Available countries from API
  readonly availableCountries = signal<SimpleValue[]>([]);

  // Bulk delete state
  readonly selectedCountriesForDeletion = signal<Set<number>>(new Set());
  
  /**
   * @description Tracks countries removed during the current edit session.
   * When a user removes a country but then re-adds it before saving,
   * this allows us to restore the original OpportunityCountry (with its ID and data)
   * instead of creating a new entry with ID = 0.
   * This prevents duplicate entries and maintains referential integrity.
   * Array is cleared when: entering edit mode, saving changes, or canceling edits.
   */
  private removedCountries: OpportunityCountry[] = [];

  // Dynamic search state
  readonly searchTerm = new FormControl<string>('');
  readonly searchResults = signal<CountryDynamicSearchResponse | null>(null);
  readonly isSearching = signal(false);
  readonly selectedSearchResults = signal<Set<number>>(new Set());
  // Store full country data for selected countries to persist across searches
  readonly selectedCountriesData = signal<Map<number, SimpleValue>>(new Map());
  private searchTerms$ = new Subject<string>();

  // Computed count
  readonly countryCount = computed(() => {
    return this.opportunity().countries?.length || 0;
  });

  // Computed: Group artifact matches by artifact type
  readonly groupedArtifactMatches = computed(() => {
    const results = this.searchResults();
    if (!results?.groups.artifactMatches) return [];

    return Object.entries(results.groups.artifactMatches).map(([artifactType, matches]) => ({
      artifactType: matches[0]?.matchReasons[0]?.artifactTypeName || artifactType, // Use proper casing from match reasons
      matches,
      count: matches.length
    }));
  });

  ngOnInit(): void {
    this.loadCountries();
    this.setupDynamicSearch();
  }

  /**
   * @description Load available countries from API
   */
  loadCountries(): void {
    this.valuesService.getCountries().subscribe({
      next: (countries) => {
        this.availableCountries.set(countries);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading countries:', error);
      }
    });
  }

  /**
   * @description Setup dynamic search with debouncing
   */
  setupDynamicSearch(): void {
    this.searchTerms$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((term: string) => {
          if (!term || term.trim().length < 2) {
            return of(null);
          }
          this.isSearching.set(true);
          const request: CountryDynamicSearchRequest = {
            searchTerm: term.trim(),
            includeArtifacts: true,
            maxResults: 100,
            highlightMatches: true
          };
          return this.valuesService.dynamicSearchCountries(request);
        })
      )
      .subscribe({
        next: (results) => {
          this.searchResults.set(results);
          this.isSearching.set(false);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error performing dynamic search:', error);
          this.isSearching.set(false);
          this.cdr.detectChanges();
        }
      });
  }

  /**
   * @description Handle search term change
   */
  onSearchTermChange(term: string): void {
    this.searchTerms$.next(term);
  }

  /**
   * @description Toggle country selection in search results
   */
  toggleCountrySelection(countryId: number): void {
    const selected = this.selectedSearchResults();
    const newSelected = new Set(selected);
    const countryData = this.selectedCountriesData();
    const newCountryData = new Map(countryData);

    if (newSelected.has(countryId)) {
      newSelected.delete(countryId);
      newCountryData.delete(countryId);
    } else {
      newSelected.add(countryId);
      // Store the country data from search results
      const results = this.searchResults();
      if (results) {
        const country = results.allResults.find((r) => r.country.id === countryId);
        if (country) {
          newCountryData.set(countryId, {
            id: country.country.id,
            name: country.country.name,
            code: country.country.iso2Code,
            continent: country.country.continent,
            region: country.country.region
          });
        }
      }
    }

    this.selectedSearchResults.set(newSelected);
    this.selectedCountriesData.set(newCountryData);
  }

  /**
   * @description Select all countries from a specific artifact group
   */
  selectArtifactGroup(matches: CountrySearchResult[]): void {
    const selected = this.selectedSearchResults();
    const newSelected = new Set(selected);
    const countryData = this.selectedCountriesData();
    const newCountryData = new Map(countryData);

    matches.forEach((match) => {
      newSelected.add(match.country.id);
      // Store country data for each selected country
      newCountryData.set(match.country.id, {
        id: match.country.id,
        name: match.country.name,
        code: match.country.iso2Code,
        continent: match.country.continent,
        region: match.country.region
      });
    });

    this.selectedSearchResults.set(newSelected);
    this.selectedCountriesData.set(newCountryData);
    this.feedbackService.showInfoToast({
      summary: this.translateService.instant('message.info'),
      detail: this.translateService.instant('message.countriesSelected', { count: matches.length })
    });
  }

  /**
   * @description Clear all search selections
   */
  clearSearchSelections(): void {
    this.selectedSearchResults.set(new Set());
    this.selectedCountriesData.set(new Map());
  }

  /**
   * @description Remove a single country from the selection
   */
  removeSelectedCountry(countryId: number): void {
    const selected = this.selectedSearchResults();
    const newSelected = new Set(selected);
    const countryData = this.selectedCountriesData();
    const newCountryData = new Map(countryData);

    newSelected.delete(countryId);
    newCountryData.delete(countryId);

    this.selectedSearchResults.set(newSelected);
    this.selectedCountriesData.set(newCountryData);
  }

  // ========================================================================
  // Edit Mode Methods
  // ========================================================================

  /**
   * @description Enable edit mode
   */
  startEditing(): void {
    if(!this.isEditing()) {
      const opp = this.opportunity();
      
      // Backup original data for cancel
      this.originalData = {
        countries: opp.countries ? [...opp.countries] : []
      };
      
      // Clear any previous bulk delete selections
      this.selectedCountriesForDeletion.set(new Set());
      
      // Clear any previously tracked removed countries
      this.removedCountries = [];
      
      this.isEditing.set(true);
      this.cdr.detectChanges();
    }
    else
    {  
      this.markAsChanged();
    }
  }

  /**
   * @description Cancel edit mode
   */
  cancelEditing(): void {
    const opp = this.opportunity();
    
    // Restore original data if available
    if (this.originalData) {
      // Restore original countries (reverts any countries that were added but not saved)
      const updatedOpportunity = {
        ...opp,
        countries: this.originalData.countries ? [...this.originalData.countries] : []
      };
      
      // Emit the reverted opportunity to parent
      this.opportunityUpdated.emit(updatedOpportunity);
    }
    
    // Clear bulk delete selections
    this.selectedCountriesForDeletion.set(new Set());
    
    // Clear any tracked removed countries
    this.removedCountries = [];
    
    this.isEditing.set(false);
    this.originalData = null;
    this.hasUnsavedChangesSignal.set(false);
    this.changesSavedOrDiscarded.emit();
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
   * @description Save changes to WHERE section
   */
  saveSection(): void {
    const opportunityId = this.opportunity().id;
    if (!opportunityId) return;

    this.isSaving.set(true);

    const whereData = {
      countries: this.opportunity().countries?.map(country => ({
        countryId: country.countryId,
        specificAreas: country.specificAreas
      })) || []
    };

    this.opportunityService.updateOpportunityWhere(opportunityId, whereData).subscribe({
      next: (updated) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.hasUnsavedChangesSignal.set(false);
        this.originalData = null;
        this.selectedCountriesForDeletion.set(new Set());
        this.removedCountries = []; // Clear tracked removed countries after save
        this.opportunityUpdated.emit(updated);
        this.changesSavedOrDiscarded.emit();
        this.feedbackService.showSuccessToast({
          summary: this.translateService.instant('message.success'),
          detail: this.translateService.instant('message.opportunity.updatedSuccessfully')
        });
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isSaving.set(false);
        console.error('Error saving WHERE section:', error);
        this.cdr.detectChanges();
      }
    });
  }

  // ========================================================================
  // Country Management Methods
  // ========================================================================

  /**
   * @description Open dialog to add countries
   */
  openAddCountryDialog(): void {
    this.selectedSearchResults.set(new Set());
    this.selectedCountriesData.set(new Map());
    this.searchTerm.setValue('');
    this.searchResults.set(null);
    this.showValidationError.set(false);
    this.showCountryDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel country dialog
   */
  cancelCountryDialog(): void {
    this.showCountryDialog.set(false);
    this.selectedSearchResults.set(new Set());
    this.selectedCountriesData.set(new Map());
    this.searchTerm.setValue('');
    this.searchResults.set(null);
    this.showValidationError.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Confirm country dialog (add multiple countries)
   */
  confirmCountryDialog(): void {
    // Get selected countries from the stored data (persists across searches)
    const countryData = this.selectedCountriesData();
    const countriesToAdd: SimpleValue[] = Array.from(countryData.values());

    // Validation
    if (countriesToAdd.length === 0) {
      this.showValidationError.set(true);
      return;
    }

    // Check for duplicates
    const opp = this.opportunity();
    const existingCountryIds = new Set(
      opp.countries?.map(c => c.countryId) || []
    );

    // Filter out duplicates
    const newCountries = countriesToAdd.filter(c => !existingCountryIds.has(c.id));
    const duplicateCount = countriesToAdd.length - newCountries.length;

    if (duplicateCount > 0) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.countriesAlreadyAdded', { count: duplicateCount })
      });
    }

    if (newCountries.length === 0) {
      return;
    }

    // Add countries
    this.addMultipleCountries(newCountries);
  }

  /**
   * @description Add multiple countries
   */
  addMultipleCountries(countries: SimpleValue[]): void {
    const opp = this.opportunity();
    const currentCountries = [...(opp.countries || [])];

    const newCountries: OpportunityCountry[] = countries.map(country => {
      // Check if this country was previously removed (and has an ID > 0)
      const removedIndex = this.removedCountries.findIndex(rc => rc.countryId === country.id);
      
      if (removedIndex !== -1) {
        // Restore the previously removed country (preserves ID and other data)
        const restoredCountry = this.removedCountries[removedIndex];
        this.removedCountries.splice(removedIndex, 1); // Remove from tracking
        return restoredCountry;
      }
      
      // Create new country entry
      return {
        id: 0,
        opportunityId: opp.id!,
        countryId: country.id,
        specificAreas: null,
        contextWarning: null,
        riskScore: null,
        humanitarianFrameworkAlignment: null,
        hasHumanitarianFramework: false,
        ndcAlignment: null,
        hasNdc: false,
        napAlignment: null,
        hasNap: false,
        orgUnitStrategyAlignment: null,
        hasOrgUnitStrategy: false,
        orgUnitWithStrategyId: null,
        orgUnitWithStrategyName: null,
        orgUnitWithStrategyCode: null,
        currentOrgUnitWithStrategyId: null,
        currentOrgUnitWithStrategyName: null,
        currentOrgUnitWithStrategyCode: null,
        hasMoreLocalStrategyAvailable: false,
        country: {
          id: country.id,
          name: country.name,
          iso2Code: country.code || '',
          continent: country.continent || null,
          region: country.region || null
        }
      };
    });

    currentCountries.push(...newCountries);

    const updatedOpportunity = {
      ...opp,
      countries: currentCountries
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.markAsChanged();
    this.feedbackService.showSuccessToast({
      summary: this.translateService.instant('message.success'),
      detail: this.translateService.instant('message.countriesAdded', { count: countries.length })
    });
    this.cancelCountryDialog();
  }

  /**
   * @description Add new country (single)
   */
  addCountry(country: SimpleValue): void {
    this.addMultipleCountries([country]);
  }

  /**
   * @description Remove country
   */
  removeCountry(index: number): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.removeCountry'),
        detail: this.translateService.instant('message.confirmRemoveCountry')
      },
      () => {
        const opp = this.opportunity();
        const currentCountries = [...(opp.countries || [])];
        
        // Store removed country in case user adds it back before saving
        const removedCountry = currentCountries[index];
        if (removedCountry && removedCountry.id > 0) {
          this.removedCountries.push(removedCountry);
        }
        
        currentCountries.splice(index, 1);

        const updatedOpportunity = {
          ...opp,
          countries: currentCountries
        };

        this.opportunityUpdated.emit(updatedOpportunity);
        this.markAsChanged();
        this.cdr.detectChanges();
      }
    );
  }

  /**
   * @description Toggle country selection for bulk deletion
   */
  toggleCountryForDeletion(index: number): void {
    const selected = this.selectedCountriesForDeletion();
    const newSelected = new Set(selected);
    
    if (newSelected.has(index)) {
      newSelected.delete(index);
    } else {
      newSelected.add(index);
    }
    
    this.selectedCountriesForDeletion.set(newSelected);
  }

  /**
   * @description Select all countries for bulk deletion
   */
  selectAllCountriesForDeletion(): void {
    const opp = this.opportunity();
    const allIndices = new Set<number>();
    
    opp.countries?.forEach((_, index) => {
      allIndices.add(index);
    });
    
    this.selectedCountriesForDeletion.set(allIndices);
  }

  /**
   * @description Clear country selection for bulk deletion
   */
  clearCountrySelection(): void {
    this.selectedCountriesForDeletion.set(new Set());
  }

  /**
   * @description Remove selected countries
   */
  removeSelectedCountries(): void {
    const selectedIndices = this.selectedCountriesForDeletion();
    
    if (selectedIndices.size === 0) {
      return;
    }

    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.removeCountries'),
        detail: this.translateService.instant('message.confirmRemoveCountries', { count: selectedIndices.size })
      },
      () => {
        const opp = this.opportunity();
        const currentCountries = [...(opp.countries || [])];
        
        // Sort indices in descending order to avoid index shifting issues
        const sortedIndices = Array.from(selectedIndices).sort((a, b) => b - a);
        
        // Store removed countries in case user adds them back before saving
        sortedIndices.forEach(index => {
          const removedCountry = currentCountries[index];
          if (removedCountry && removedCountry.id > 0) {
            this.removedCountries.push(removedCountry);
          }
        });
        
        // Remove countries from highest index to lowest
        sortedIndices.forEach(index => {
          currentCountries.splice(index, 1);
        });

        const updatedOpportunity = {
          ...opp,
          countries: currentCountries
        };

        this.opportunityUpdated.emit(updatedOpportunity);
        this.markAsChanged();
        this.selectedCountriesForDeletion.set(new Set());
        this.cdr.detectChanges();

        this.feedbackService.showSuccessToast({
          summary: this.translateService.instant('message.success'),
          detail: this.translateService.instant('message.countriesRemoved', { count: selectedIndices.size })
        });
      }
    );
  }
}

