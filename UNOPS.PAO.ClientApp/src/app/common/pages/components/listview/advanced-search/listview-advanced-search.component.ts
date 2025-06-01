import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, computed, inject, OnInit, OnChanges, SimpleChanges, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { ChipModule } from 'primeng/chip';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { TooltipModule } from 'primeng/tooltip';
import { CalendarModule } from 'primeng/calendar';

import { ListViewConfig, SearchCriteria, SearchParams, EntityType } from '../listview.model';
import { SavedFilter } from '../../../../interfaces/saved-filter.interface';
import { AdvancedSearchSavedFilterComponent } from './saved-filter/advanced-search-saved-filter.component';

@Component({
  selector: 'app-listview-advanced-search',
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
    TooltipModule,
    CalendarModule,
    AdvancedSearchSavedFilterComponent
  ],
  templateUrl: './listview-advanced-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListviewAdvancedSearchComponent implements OnInit, OnChanges {
  // Inputs
  @Input() config!: ListViewConfig;
  @Input() isLoading: boolean = false;
  @Input() searchCriteria: SearchCriteria[] = [];
  @Input() entityType?: EntityType; // Optional for SavedFilter functionality
  @Input() orderBy?: string; // For SavedFilter functionality
  @Input() ascending: boolean = true; // For SavedFilter functionality
  
  // Outputs
  @Output() search = new EventEmitter<SearchCriteria>();
  @Output() removeCriterion = new EventEmitter<number>();
  @Output() clearSearch = new EventEmitter<void>();
  @Output() exportData = new EventEmitter<void>();
  @Output() applySavedFilter = new EventEmitter<SavedFilter>();
  @Output() switchToSimple = new EventEmitter<void>();
  
  // UI state
  selectedSearchField: any = null;
  advancedSearchText: string = '';
  selectedComparisonOperator: string = 'like'; // Comparison operator (is, like, >, etc.)
  selectedLogicalOperator: 'AND' | 'OR' = 'AND'; // Logical operator (AND/OR)
  
  // Date-specific UI state
  selectedDate: Date | null = null;
  selectedSecondDate: Date | null = null; // For "between" operator
  
  // Dropdown options
  logicalOperators = [
    { label: 'search.logicalOperators.and', value: 'AND' },
    { label: 'search.logicalOperators.or', value: 'OR' }
  ];
  
  // All available comparison operators by type
  private allOperators = {
    text: [
      { label: 'search.operators.equals', value: 'is' },
      { label: 'search.operators.notEquals', value: 'is not' },
      { label: 'search.operators.contains', value: 'like' },
      { label: 'search.operators.notContains', value: 'not like' }
    ],
    date: [
      { label: 'search.operators.equals', value: 'is' },
      { label: 'search.operators.notEquals', value: 'is not' },
      { label: 'search.operators.after', value: 'after' },
      { label: 'search.operators.before', value: 'before' },
      { label: 'search.operators.between', value: 'between' },
    ],
    number: [
      { label: 'search.operators.equals', value: 'is' },
      { label: 'search.operators.notEquals', value: 'is not' },
      { label: 'search.operators.greaterThan', value: '>' },
      { label: 'search.operators.lessThan', value: '<' },
      { label: 'search.operators.greaterThanOrEqual', value: '>=' },
      { label: 'search.operators.lessThanOrEqual', value: '<=' }
    ]
  };
  
  // Computed properties
  searchableFields = computed(() => this.config?.searchConfig?.searchableFields || []);
  
  // Dynamic comparison operators based on selected field type
  comparisonOperators = computed(() => {
    if (!this.selectedSearchField) {
      return this.allOperators.text;
    }
    
    const fieldType = this.getFieldType(this.selectedSearchField);
    return this.allOperators[fieldType] || this.allOperators.text;
  });
  
  /**
   * Check if current field is a date field
   */
  isDateField(): boolean {
    return this.selectedSearchField && this.getFieldType(this.selectedSearchField) === 'date';
  }
  
  /**
   * Check if "between" operator is selected
   */
  isBetweenOperator(): boolean {
    return this.selectedComparisonOperator === 'between';
  }

  /**
   * Check if a value is a date string
   */
  isDateValue(value: any): boolean {
    if (!value || typeof value !== 'string') {
      return false;
    }
    
    // Check if it's a valid ISO date string
    const date = new Date(value);
    return !isNaN(date.getTime()) && value.includes('T') && value.includes(':');
  }
  
  /**
   * Get available comparison operators for current field
   */
  getComparisonOperators() {
    if (!this.selectedSearchField) {
      return this.allOperators.text;
    }
    
    const fieldType = this.getFieldType(this.selectedSearchField);
    return this.allOperators[fieldType] || this.allOperators.text;
  }
  
  constructor() {
    // Effect to automatically select first field when searchable fields change
    effect(() => {
      const fields = this.searchableFields();
      if (fields && fields.length > 0) {
        // Use setTimeout to ensure this runs after the component is fully initialized
        setTimeout(() => this.selectFirstSearchField(), 0);
      }
    });
  }
  
  ngOnInit(): void {
    this.selectFirstSearchField();
  }
  
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['config'] && this.config?.searchConfig?.searchableFields) {
      this.selectFirstSearchField();
    }
  }
  
  /**
   * Get field type based on the selected field
   */
  private getFieldType(field: any): 'text' | 'date' | 'number' {
    if (field.type) {
      switch (field.type) {
        case 'date':
          return 'date';
        case 'number':
        case 'currency':
          return 'number';
        default:
          return 'text';
      }
    }
    
    // Fallback: try to infer from field name
    const fieldName = field.field.toLowerCase();
    if (fieldName.includes('date') || fieldName.includes('time') || 
        fieldName === 'fromdate' || fieldName === 'todate' || fieldName === 'createdat' || fieldName === 'updatedat') {
      return 'date';
    }
    
    return 'text';
  }
  
  /**
   * Automatically select the first available search field
   */
  private selectFirstSearchField(): void {
    const fields = this.searchableFields();
    if (fields && fields.length > 0) {
      // Check if current selected field is still valid
      const isCurrentFieldValid = this.selectedSearchField && 
        fields.some(field => field.field === this.selectedSearchField.field);
      
      // Only select first field if no field is selected or current field is invalid
      if (!this.selectedSearchField || !isCurrentFieldValid) {
        this.selectedSearchField = fields[0];
        this.onSearchFieldSelect(this.selectedSearchField);
      }
    }
  }
  
  /**
   * Handle field selection for advanced search
   */
  onSearchFieldSelect(field: any): void {
    this.selectedSearchField = field;
    
    // Reset values when field changes
    this.advancedSearchText = '';
    this.selectedDate = null;
    this.selectedSecondDate = null;
    
    // Reset operator to appropriate default for field type
    const fieldType = this.getFieldType(field);
    if (fieldType === 'date') {
      this.selectedComparisonOperator = 'is';
    } else if (fieldType === 'number') {
      this.selectedComparisonOperator = 'is';
    } else {
      this.selectedComparisonOperator = 'like';
    }
  }

  onClearSearchText(): void {
    this.advancedSearchText = '';
    this.selectedDate = null;
    this.selectedSecondDate = null;
  }
  
  /**
   * Add a new search criterion when user presses enter in advanced search
   */
  onAdvancedSearchEnter(): void {
    if (this.selectedSearchField && this.canAddCriterion()) {
      this.addSearchCriterion();
    }
  }
  
  /**
   * Check if we can add a criterion (simplified for template use)
   */
  canAddCriterion(): boolean {
    if (this.getFieldType(this.selectedSearchField) === 'date') {
      if (this.isBetweenOperator()) {
        return this.selectedDate != null && this.selectedSecondDate != null;
      }
      return this.selectedDate != null;
    }
    
    return !!(this.advancedSearchText && this.advancedSearchText.trim().length > 0);
  }
  
  /**
   * Format date to ISO string for backend compatibility
   */
  private formatDateValue(date: Date): string {
    return date.toISOString();
  }
  
  /**
   * Add the current search criterion
   */
  addSearchCriterion(): void {
    if (!this.canAddCriterion()) {
      return;
    }
    
    const fieldType = this.getFieldType(this.selectedSearchField);
    let value: string;
    let secondValue: string | undefined;
    
    if (fieldType === 'date') {
      if (this.isBetweenOperator()) {
        value = this.formatDateValue(this.selectedDate!);
        secondValue = this.formatDateValue(this.selectedSecondDate!);
      } else {
        value = this.formatDateValue(this.selectedDate!);
      }
    } else {
      value = this.advancedSearchText.trim();
    }
    
    const criterion: SearchCriteria = {
      field: this.selectedSearchField.field,
      value: value,
      label: this.selectedSearchField.label,
      operator: this.selectedComparisonOperator,
      logicalOperator: this.selectedLogicalOperator,
      fieldType: fieldType,
      secondValue: secondValue
    };
    
    // Emit the criterion to parent component
    this.search.emit(criterion);
    
    // Clear the input fields
    this.advancedSearchText = '';
    this.selectedDate = null;
    this.selectedSecondDate = null;
    this.selectedComparisonOperator = fieldType === 'date' ? 'is' : (fieldType === 'number' ? 'is' : 'like');
    
    // Automatically select the first search field again for convenience
    this.selectFirstSearchField();
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
    
    // Automatically select the first search field again for convenience
    this.selectFirstSearchField();
  }
  
  /**
   * Export data
   */
  onExportData(): void {
    this.exportData.emit();
  }

  /**
   * Switch back to simple search
   */
  switchToSimpleSearch(): void {
    this.switchToSimple.emit();
  }

  // ===== SavedFilter Event Handlers =====

  /**
   * Handle saved filter applied event
   */
  onSavedFilterApplied(filter: SavedFilter): void {
    // First, clear current search criteria
    this.clearSearch.emit();
    
    // Then emit the filter to parent for complete handling
    this.applySavedFilter.emit(filter);
  }

  /**
   * Handle applying criteria from saved filter
   */
  onApplyCriteria(criteria: SearchCriteria[]): void {
    // Apply each criterion step by step to rebuild the search
    criteria.forEach(criterion => {
      this.search.emit(criterion);
    });
  }

  /**
   * Handle saved filter events (saved, updated, deleted)
   * These can be used to show notifications or update UI state
   */
  onSavedFilterSaved(filter: SavedFilter): void {
    // Filter was saved successfully
    // Parent component can handle this if needed
  }

  onSavedFilterUpdated(filter: SavedFilter): void {
    // Filter was updated successfully
    // Parent component can handle this if needed
  }

  onSavedFilterDeleted(filterId: number): void {
    // Filter was deleted successfully
    // Parent component can handle this if needed
  }
}
