import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputSwitchModule } from 'primeng/inputswitch';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';

import { SavedFilterService } from '../../../../../services/saved-filter.service';
import { SavedFilter, CreateSavedFilterRequest, UpdateSavedFilterRequest } from '../../../../../interfaces/saved-filter.interface';
import { SearchCriteria, EntityType } from '../../listview.model';

@Component({
  selector: 'app-advanced-search-saved-filter',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ButtonModule,
    DropdownModule,
    DialogModule,
    InputTextModule,
    InputSwitchModule,
    TooltipModule,
    ConfirmDialogModule
  ],
  templateUrl: './advanced-search-saved-filter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ConfirmationService, MessageService]
})
export class AdvancedSearchSavedFilterComponent implements OnInit {
  private savedFilterService = inject(SavedFilterService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // Inputs
  @Input() entityType!: EntityType;
  @Input() set searchCriteria(value: SearchCriteria[]) {
    this._searchCriteria = value;
    this.currentSearchCriteria.set([...value]);
  }
  get searchCriteria(): SearchCriteria[] {
    return this._searchCriteria;
  }
  private _searchCriteria: SearchCriteria[] = [];

  @Input() isLoading: boolean = false;
  @Input() orderBy?: string;
  @Input() ascending: boolean = true;
  @Input() set preselectedFilterId(value: number | null) {
    if (value && value !== this._preselectedFilterId) {
      this._preselectedFilterId = value;
      this.loadAndApplyPreselectedFilter(value);
    }
  }
  get preselectedFilterId(): number | null {
    return this._preselectedFilterId;
  }
  private _preselectedFilterId: number | null = null;

  // Outputs
  @Output() filterApplied = new EventEmitter<SavedFilter>();
  @Output() filterSaved = new EventEmitter<SavedFilter>();
  @Output() filterUpdated = new EventEmitter<SavedFilter>();
  @Output() filterDeleted = new EventEmitter<number>();
  @Output() applyCriteria = new EventEmitter<SearchCriteria[]>();

  // Component state
  savedFilters = signal<SavedFilter[]>([]);
  selectedSavedFilter = signal<SavedFilter | null>(null);
  showSaveDialog = signal(false);
  showUpdateDialog = signal(false);

  // Track unsaved changes
  private originalSearchCriteria = signal<SearchCriteria[]>([]);
  private currentSearchCriteria = signal<SearchCriteria[]>([]);
  private hasUnsavedChanges = signal(false);

  // Computed property for modifications
  hasModifications = computed(() => {
    const selected = this.selectedSavedFilter();
    if (!selected) return false;

    const currentCriteria = JSON.stringify(this.currentSearchCriteria());
    const originalCriteria = JSON.stringify(this.originalSearchCriteria());

    return currentCriteria !== originalCriteria;
  });

  // Computed property to check if we can save as new filter
  canSaveAsNewFilter = computed(() => {
    const currentCriteria = this.currentSearchCriteria();
    const selected = this.selectedSavedFilter();

    // Must have criteria to save
    if (!currentCriteria || currentCriteria.length === 0) return false;

    // If no filter selected, we can save as new
    if (!selected) return true;

    // If filter selected but criteria are different, we can save as new
    const currentCriteriaStr = JSON.stringify(currentCriteria);
    const selectedCriteriaStr = JSON.stringify(this.originalSearchCriteria());

    return currentCriteriaStr !== selectedCriteriaStr;
  });

  // Form state
  saveFilterName = '';
  updateFilterName = '';

  ngOnInit(): void {
    this.loadSavedFilters();
    // Initialize current search criteria signal
    this.updateCurrentSearchCriteria();
  }

  /**
   * Update current search criteria signal (call this when searchCriteria changes)
   */
  updateCurrentSearchCriteria(): void {
    this.currentSearchCriteria.set([...this.searchCriteria]);
  }

  /**
   * Load saved filters for the current entity type
   */
  private loadSavedFilters(): void {
    if (!this.entityType) return;

    this.savedFilterService.getSavedFiltersForEntity(this.entityType, 1, 100)
      .subscribe({
        next: (response) => {
          this.savedFilters.set(response.records);
        },
        error: (error) => {
          console.error('Error loading saved filters:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load saved filters'
          });
        }
      });
  }

  /**
   * Handle saved filter selection
   * NOTE: This only APPLIES the filter, it does NOT save any modifications
   * Modifications are only saved when explicitly clicking the Update button
   */
  onSavedFilterSelect(filter: SavedFilter | null): void {
    if (!filter) {
      this.selectedSavedFilter.set(null);
      return;
    }

    this.selectedSavedFilter.set(filter);

    // Apply the saved filter (READ-ONLY operation)
    this.savedFilterService.applySavedFilter(filter.id)
      .subscribe({
        next: (response) => {
          // First emit the filter for general handling (sorting, pagination, etc.)
          this.filterApplied.emit(filter);

          // Then emit criteria if it's an advanced search for step-by-step application
          if (response.isAdvancedSearch && response.searchCriteria) {
            try {
              let criteria: SearchCriteria[] = [];

              // Handle both string and array formats
              if (typeof response.searchCriteria === 'string') {
                // Parse JSON string
                criteria = JSON.parse(response.searchCriteria);
              } else if (Array.isArray(response.searchCriteria)) {
                // Direct array
                criteria = response.searchCriteria;
              } else {
                console.warn('Unexpected searchCriteria format:', response.searchCriteria);
                return;
              }

              // Emit criteria for the parent to apply one by one
              if (criteria && criteria.length > 0) {
                this.applyCriteria.emit(criteria);
                // Store the original criteria to track modifications
                this.storeOriginalCriteria(criteria);
              }
            } catch (error) {
              console.error('Error parsing searchCriteria:', error);
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: 'Failed to parse saved filter criteria'
              });
            }
          }
        },
        error: (error) => {
          console.error('Error applying saved filter:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to apply saved filter'
          });
        }
      });
  }

  /**
   * Open save filter dialog
   */
  openSaveDialog(): void {
    this.saveFilterName = '';
    this.showSaveDialog.set(true);
  }

  /**
   * Save current filter
   */
  saveCurrentFilter(event: Event): void {
    event.stopPropagation();
    event.preventDefault();

    const serializedCriteria = JSON.stringify(this.searchCriteria);

    const request: CreateSavedFilterRequest = {
      name: this.saveFilterName.trim(),
      entityType: this.entityType,
      isAdvancedSearch: true,
      searchCriteria: serializedCriteria, // Backend expects JSON string
      orderBy: this.orderBy,
      ascending: this.ascending
    };



    this.savedFilterService.createSavedFilter(request)
      .subscribe({
        next: (savedFilter) => {

          this.showSaveDialog.set(false);
          this.loadSavedFilters(); // Refresh the list
          this.selectedSavedFilter.set(savedFilter);
          this.filterSaved.emit(savedFilter);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Filter saved successfully'
          });
        },
        error: (error) => {
          console.error('Error saving filter:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save filter'
          });
        }
      });
  }

  /**
   * Open update filter dialog
   */
  openUpdateDialog(event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    if (!this.selectedSavedFilter()) return;

    this.updateFilterName = this.selectedSavedFilter()!.name;
    this.showUpdateDialog.set(true);
  }

  /**
   * Update selected filter
   * This method is ONLY called when user explicitly clicks the "Update" button
   * No automatic saving occurs elsewhere in the component
   */
  updateSelectedFilter(event: Event): void {
    event.stopPropagation();
    event.preventDefault();

    const request: UpdateSavedFilterRequest = {
      id: this.selectedSavedFilter()!.id,
      name: this.updateFilterName.trim(),
      entityType: this.selectedSavedFilter()!.entityType,
      isAdvancedSearch: true,
      searchCriteria: JSON.stringify(this.searchCriteria), // Backend expects JSON string
      orderBy: this.orderBy || this.selectedSavedFilter()!.orderBy,
      ascending: this.ascending
    };

    this.savedFilterService.updateSavedFilter(request)
      .subscribe({
        next: (updatedFilter) => {
          this.showUpdateDialog.set(false);
          this.loadSavedFilters(); // Refresh the list
          this.selectedSavedFilter.set(updatedFilter);

          // Reset modification tracking after successful save
          this.storeOriginalCriteria(this.searchCriteria);

          this.filterUpdated.emit(updatedFilter);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Filter updated successfully'
          });
        },
        error: (error) => {
          console.error('Error updating filter:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update filter'
          });
        }
      });
  }

  /**
   * Delete selected saved filter with confirmation
   */
  deleteSavedFilter(): void {
    if (!this.selectedSavedFilter()) return;

    this.confirmationService.confirm({
      message: `Are you sure you want to delete the filter "${this.selectedSavedFilter()!.name}"?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        if (this.selectedSavedFilter()) {
          this.savedFilterService.deleteSavedFilter(this.selectedSavedFilter()!.id)
            .subscribe({
              next: () => {
                const deletedId = this.selectedSavedFilter()!.id;
                this.selectedSavedFilter.set(null);
                this.showUpdateDialog.set(false); // Close the edit dialog
                this.loadSavedFilters(); // Refresh the list
                this.filterDeleted.emit(deletedId);
                this.messageService.add({
                  severity: 'success',
                  summary: 'Success',
                  detail: 'Filter deleted successfully'
                });
              },
              error: (error) => {
                console.error('Error deleting filter:', error);
                this.messageService.add({
                  severity: 'error',
                  summary: 'Error',
                  detail: 'Failed to delete filter'
                });
              }
            });
        }
      }
    });
  }

  /**
   * Cancel save dialog
   */
  cancelSaveDialog(): void {
    this.showSaveDialog.set(false);
    this.saveFilterName = '';
  }

  /**
   * Cancel update dialog
   */
  cancelUpdateDialog(): void {
    this.showUpdateDialog.set(false);
    this.updateFilterName = '';
  }

  /**
   * Check if we can manage the selected filter
   */
  get canManageFilter(): boolean {
    return this.selectedSavedFilter() !== null;
  }

  /**
   * Store the original criteria when a filter is selected
   */
  private storeOriginalCriteria(criteria: SearchCriteria[]): void {
    this.originalSearchCriteria.set(JSON.parse(JSON.stringify(criteria))); // Deep copy
    this.currentSearchCriteria.set(JSON.parse(JSON.stringify(criteria))); // Also set current
    this.hasUnsavedChanges.set(false);
  }

  /**
   * Load and apply a preselected filter from URL
   */
  private loadAndApplyPreselectedFilter(filterId: number): void {
    // First, load the filter details to set it as selected
    this.savedFilterService.getSavedFilter(filterId)
      .subscribe({
        next: (filter) => {
          // Set the filter as selected in the dropdown
          this.selectedSavedFilter.set(filter);

          // Then apply the filter to get its criteria and load data
          this.onSavedFilterSelect(filter);
        },
        error: (error) => {
          console.error('Error loading preselected filter:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load saved filter from URL'
          });
        }
      });
  }

  /**
   * Clear the selected saved filter
   * Called when search criteria are modified to avoid confusion
   */
  clearSelectedFilter(): void {
    this.selectedSavedFilter.set(null);
    this.originalSearchCriteria.set([]);
    this.hasUnsavedChanges.set(false);
  }
}
