import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ChipModule } from 'primeng/chip';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { TooltipModule } from 'primeng/tooltip';

import { ListViewConfig, SearchCriteria, SearchParams } from '../listview.model';

@Component({
  selector: 'app-listview-advenced-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    ButtonModule,
    ChipModule,
    DropdownModule,
    InputTextModule,
    IconField,
    InputIcon,
    TooltipModule
  ],
  templateUrl: './listview-advenced-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListviewAdvencedSearchComponent {
  // Inputs
  @Input() config!: ListViewConfig;
  @Input() isLoading: boolean = false;
  @Input() searchCriteria: SearchCriteria[] = [];
  
  // Outputs
  @Output() search = new EventEmitter<SearchCriteria>();
  @Output() removeCriterion = new EventEmitter<number>();
  @Output() clearSearch = new EventEmitter<void>();
  @Output() exportData = new EventEmitter<void>();
  
  // UI state
  selectedSearchField: any = null;
  advancedSearchText: string = '';
  selectedOperator: 'AND' | 'OR' = 'AND';
  
  // Dropdown options
  operators = [
    { label: 'AND', value: 'AND' },
    { label: 'OR', value: 'OR' }
  ];
  
  // Computed properties
  searchableFields = computed(() => this.config?.searchConfig?.searchableFields || []);
  
  /**
   * Handle field selection for advanced search
   */
  onSearchFieldSelect(field: any): void {
    this.selectedSearchField = field;
  }
  
  /**
   * Add a new search criterion when user presses enter in advanced search
   */
  onAdvancedSearchEnter(): void {
    if (this.selectedSearchField && this.advancedSearchText) {
      this.addSearchCriterion();
    }
  }
  
  /**
   * Add the current search criterion
   */
  addSearchCriterion(): void {
    if (!this.selectedSearchField || !this.advancedSearchText.trim()) {
      return;
    }
    
    const criterion: SearchCriteria = {
      field: this.selectedSearchField.field,
      value: this.advancedSearchText.trim(),
      label: this.selectedSearchField.label,
      operator: this.selectedOperator
    };
    
    // Emit the criterion to parent component
    this.search.emit(criterion);
    
    // Clear the input fields
    this.advancedSearchText = '';
    this.selectedSearchField = null;
    this.selectedOperator = 'AND'; // Reset operator to default
  }
  
  /**
   * Remove a search criterion
   */
  removeSearchCriterion(index: number): void {
    this.removeCriterion.emit(index);
  }
  
  /**
   * Clear all search criteria
   */
  onClearSearch(): void {
    this.clearSearch.emit();
  }
  
  /**
   * Export data
   */
  onExportData(): void {
    this.exportData.emit();
  }
}
