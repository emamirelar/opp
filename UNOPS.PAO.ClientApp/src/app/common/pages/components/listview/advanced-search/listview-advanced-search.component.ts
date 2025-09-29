import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, computed, inject, OnInit, OnChanges, SimpleChanges, effect, ViewChild, signal } from '@angular/core';
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
import { CheckboxModule } from 'primeng/checkbox';
import { HttpClient } from '@angular/common/http';

import { ListViewConfig, SearchCriteria, SearchParams, EntityType } from '../listview.model';
import { SavedFilter } from '../../../../interfaces/saved-filter.interface';
import { AdvancedSearchSavedFilterComponent } from './saved-filter/advanced-search-saved-filter.component';

// Backend SearchFieldInfo interface to match the API response
interface SearchFieldInfo {
  field: string;
  displayName: string;
  fieldType: string;
  isNavigationProperty?: boolean;
  allowedOperators: string[];
  dropdownOptions?: DropdownOption[];
}

interface DropdownOption {
  value: string;
  label: string;
}

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
    CheckboxModule,
    AdvancedSearchSavedFilterComponent
  ],
  templateUrl: './listview-advanced-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListviewAdvancedSearchComponent implements OnInit, OnChanges {
  // ViewChild for saved filter component
  @ViewChild('savedFilterRef') savedFilterComponent?: AdvancedSearchSavedFilterComponent;

  // Inputs
  @Input() config!: ListViewConfig;
  @Input() isLoading: boolean = false;
  @Input() filters: SearchCriteria[] = [];  // Changed from searchCriteria to filters
  @Input() entityType?: EntityType; // Required for fetching search fields from API
  @Input() orderBy?: string; // For SavedFilter functionality
  @Input() ascending: boolean = true; // For SavedFilter functionality
  @Input() preselectedSavedFilterId: number | null = null; // For URL-based filter selection

  // Outputs
  @Output() search = new EventEmitter<SearchCriteria>();
  @Output() removeCriterion = new EventEmitter<number>();
  @Output() clearSearch = new EventEmitter<void>();
  @Output() applySavedFilter = new EventEmitter<SavedFilter>();
  @Output() switchToSimple = new EventEmitter<void>();
  @Output() myOfficeFilterChanged = new EventEmitter<boolean>();

  // Dependency injection
  private http = inject(HttpClient);
  private translate = inject(TranslateService);

  // Dynamic search fields from API
  searchFieldsFromAPI = signal<SearchFieldInfo[]>([]);
  isLoadingSearchFields = signal<boolean>(false);
  searchFieldsError = signal<string | null>(null);

  // UI state
  selectedSearchField: any = null;
  advancedSearchText: string = '';
  selectedComparisonOperator: string = 'like'; // Comparison operator (is, like, >, etc.)
  selectedLogicalOperator: 'AND' | 'OR' = 'AND'; // Logical operator (AND/OR)

  // My Office filter state
  myOfficeOnly: boolean = false;

  // Date-specific UI state
  selectedDate: Date | null = null;
  selectedSecondDate: Date | null = null; // For "between" operator

  // Enum-specific UI state
  selectedEnumValue: string = '';

  // Dropdown options
  logicalOperators = [
    { label: 'entityCards.logicalOperators.and', value: 'AND' },
    { label: 'entityCards.logicalOperators.or', value: 'OR' }
  ];

  // All available comparison operators by type
  private allOperators = {
    text: [
      { label: 'entityCards.operators.equals', value: 'is' },
      { label: 'entityCards.operators.notEquals', value: 'is not' },
      { label: 'entityCards.operators.contains', value: 'like' },
      { label: 'entityCards.operators.notContains', value: 'not like' }
    ],
    date: [
      { label: 'entityCards.operators.after', value: 'after' },
      { label: 'entityCards.operators.before', value: 'before' },
      { label: 'entityCards.operators.between', value: 'between' },
    ],
    number: [
      { label: 'entityCards.operators.equals', value: 'is' },
      { label: 'entityCards.operators.notEquals', value: 'is not' },
      { label: 'entityCards.operators.greaterThan', value: '>' },
      { label: 'entityCards.operators.lessThan', value: '<' },
      { label: 'entityCards.operators.greaterThanOrEqual', value: '>=' },
      { label: 'entityCards.operators.lessThanOrEqual', value: '<=' }
    ],
    enum: [
      { label: 'entityCards.operators.equals', value: 'eq' },
      { label: 'entityCards.operators.notEquals', value: 'neq' }
    ]
  };

  // Signal to track current language for reactive translations
  currentLang = signal(this.translate.currentLang || 'en');

  // Computed properties - use dynamic fields from API, fallback to config if needed
  searchableFields = computed(() => {
    // Make computed reactive to language changes
    const lang = this.currentLang();
    const apiFields = this.searchFieldsFromAPI();
    
    if (apiFields.length > 0) {
      // Convert SearchFieldInfo from API to SearchField format for compatibility
      return apiFields.map(field => {
        // Get translation or fallback to the displayName itself
        const translatedLabel = this.translate.instant(field.displayName);
        const label = translatedLabel !== field.displayName ? translatedLabel : field.displayName;
        
        return {
          field: field.field,
          label: label,
          type: this.mapFieldTypeToSearchFieldType(field.fieldType),
          operators: field.allowedOperators || ['like', 'eq', 'neq'],
          dropdownOptions: field.dropdownOptions
        };
      });
    }
    // Fallback to config-based fields if API hasn't loaded yet
    return this.config?.searchConfig?.searchableFields || [];
  });

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
   * Check if current field is an enum field with dropdown options
   */
  isDropdownField(): boolean {
    if (!this.selectedSearchField) {
      return false;
    }
    
    return this.selectedSearchField.dropdownOptions && this.selectedSearchField.dropdownOptions.length > 0;
  }

  /**
   * Get dropdown options for the current enum field
   */
  getDropdownOptions(): any[] {
    return this.selectedSearchField.dropdownOptions.map((option: DropdownOption) => ({
      label: option.label,
      value: option.value
    }));
  }

  /**
   * Check if "between" operator is selected
   */
  isBetweenOperator(): boolean {
    return this.selectedComparisonOperator === 'between';
  }

  /**
   * Map backend field types to frontend SearchField types
   */
  private mapFieldTypeToSearchFieldType(backendType: string): 'string' | 'number' | 'date' | 'boolean' | 'enum' {
    switch (backendType.toLowerCase()) {
      case 'date':
      case 'datetime':
        return 'date';
      case 'number':
      case 'int':
      case 'integer':
      case 'decimal':
        return 'number';
      case 'boolean':
      case 'bool':
        return 'boolean';
      case 'enum':
        return 'enum';
      default:
        return 'string';
    }
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
   * Get human-readable operator label
   */
  getOperatorLabel(operator: string): string {
    const operatorMap: { [key: string]: string } = {
      'is': '=',
      'is not': '≠',
      'like': '⊃',
      'not like': '⊅',
      'after': '>',
      'before': '<',
      'between': '↔',
      '>': '>',
      '<': '<',
      '>=': '≥',
      '<=': '≤'
    };

    return operatorMap[operator] || operator;
  }

  /**
   * Get available comparison operators for current field
   */
  getComparisonOperators() {
    if (!this.selectedSearchField) {
      return this.allOperators.text;
    }

    // If the field comes from API and has allowedOperators, use those
    if (this.selectedSearchField.allowedOperators && Array.isArray(this.selectedSearchField.allowedOperators)) {
      return this.selectedSearchField.allowedOperators.map((op: string) => {
        // Find the operator definition from our allOperators
        const allOpsFlat = [
          ...this.allOperators.text,
          ...this.allOperators.date, 
          ...this.allOperators.number,
          ...this.allOperators.enum
        ];
        
        const found = allOpsFlat.find(opDef => opDef.value === op);
        return found || { label: op, value: op };
      });
    }

    // Fallback to field type-based operators
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
    // Load search fields from API if entityType is available
    if (this.entityType) {
      this.loadSearchFieldsFromAPI();
    }
    this.selectFirstSearchField();

    // Subscribe to language changes to update translations reactively
    this.translate.onLangChange.subscribe(event => {
      this.currentLang.set(event.lang);
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['config'] && this.config?.searchConfig?.searchableFields) {
      this.selectFirstSearchField();
    }
    
    // Load search fields if entityType changes
    if (changes['entityType'] && this.entityType) {
      this.loadSearchFieldsFromAPI();
    }
  }

  /**
   * Load search fields from the API based on entity type
   */
  private loadSearchFieldsFromAPI(): void {
    if (!this.entityType) {
      return;
    }

    this.isLoadingSearchFields.set(true);
    this.searchFieldsError.set(null);

    // Construct the API endpoint based on entity type
    const entityTypeLower = this.entityType.toLowerCase();
    
    // For interactions, use singular form to match backend endpoint
    // For partner categories and groups, use lowercase form
    let entityPath = entityTypeLower;
    if (entityTypeLower === 'interactions') {
      entityPath = 'interaction';
    } else if (entityTypeLower === 'partnercategory') {
      entityPath = 'partnercategory';
    } else if (entityTypeLower === 'partnergroup') {
      entityPath = 'partnergroup';
    }
    const endpoint = `/api/${entityPath}/search-fields`;

    this.http.get<SearchFieldInfo[]>(endpoint).subscribe({
      next: (searchFields) => {
        // Transform API response to include translation keys for displayName
        const transformedFields = searchFields.map(field => ({
          ...field,
          displayName: this.translate.instant(field.displayName) || field.displayName
        }));
        
        this.searchFieldsFromAPI.set(transformedFields);
        this.isLoadingSearchFields.set(false);
        
        // Auto-select first field after loading
        setTimeout(() => this.selectFirstSearchField(), 0);
      },
      error: (error) => {
        console.error('Error loading search fields:', error);
        this.searchFieldsError.set('Failed to load search fields');
        this.isLoadingSearchFields.set(false);
        
        // Fallback to config fields if API fails
        this.searchFieldsFromAPI.set([]);
      }
    });
  }

  /**
   * Get field type based on the selected field
   */
  private getFieldType(field: any): 'text' | 'date' | 'number' | 'enum' {
    // First check if the field has fieldType from API
    if (field.fieldType) {
      return this.mapFieldTypeToSearchFieldType(field.fieldType) as 'text' | 'date' | 'number' | 'enum';
    }
    
    // Legacy support for field.type
    if (field.type) {
      switch (field.type) {
        case 'date':
          return 'date';
        case 'number':
        case 'currency':
          return 'number';
        case 'enum':
          return 'enum';
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
    this.selectedEnumValue = '';

    // Reset operator to appropriate default for field type
    const fieldType = this.getFieldType(field);
    
    if (fieldType === 'date') {
      this.selectedComparisonOperator = 'after';
    } else if (fieldType === 'number') {
      this.selectedComparisonOperator = 'is';
    } else if (fieldType === 'enum') {
      this.selectedComparisonOperator = 'eq';
    } else {
      this.selectedComparisonOperator = 'like';
    }
  }

  onClearSearchText(): void {
    this.advancedSearchText = '';
    this.selectedDate = null;
    this.selectedSecondDate = null;
    this.selectedEnumValue = '';
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
    const fieldType = this.getFieldType(this.selectedSearchField);
    
    if (fieldType === 'date') {
      if (this.isBetweenOperator()) {
        return this.selectedDate != null && this.selectedSecondDate != null;
      }
      return this.selectedDate != null;
    }

    if (fieldType === 'enum') {
      return !!(this.selectedEnumValue && this.selectedEnumValue.trim().length > 0);
    }

    return !!(this.advancedSearchText && this.advancedSearchText.trim().length > 0);
  }

  /**
   * Format date to simple YYYY-MM-DD format for backend compatibility
   */
  private formatDateValue(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
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
    } else if (fieldType === 'enum') {
      value = this.selectedEnumValue.trim();
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

    if (fieldType === 'enum') {
      if (criterion.operator === 'like') {
        criterion.operator = 'eq';
      } else if (criterion.operator === 'not like') {
        criterion.operator = 'neq';
      }
    }

    // Clear saved filter dropdown when criteria are modified
    this.clearSavedFilterSelection();

    // Emit the criterion to parent component
    this.search.emit(criterion);

    // Clear the input fields
    this.advancedSearchText = '';
    this.selectedDate = null;
    this.selectedSecondDate = null;
    this.selectedEnumValue = '';
    this.selectedComparisonOperator = fieldType === 'date' ? 'after' : (fieldType === 'number' ? 'is' : 'like');

    // Automatically select the first search field again for convenience
    this.selectFirstSearchField();
  }

  /**
   * Remove a search criterion
   */
  removeSearchCriterion(index: number): void {
    // Clear saved filter dropdown when criteria are modified
    this.clearSavedFilterSelection();
    
    this.removeCriterion.emit(index);
  }

  /**
   * Clear all search criteria
   */
  onClearSearch(): void {
    this.clearSearch.emit();

    // Reset My Office filter
    this.myOfficeOnly = false;
    this.myOfficeFilterChanged.emit(this.myOfficeOnly);

    // Automatically select the first search field again for convenience
    this.selectFirstSearchField();
  }


  /**
   * Switch back to simple search
   */
  switchToSimpleSearch(): void {
    this.switchToSimple.emit();
  }

  /**
   * Handle My Office filter toggle
   */
  onMyOfficeFilterChange(): void {
    // Clear saved filter dropdown when criteria are modified
    this.clearSavedFilterSelection();
    
    this.myOfficeFilterChanged.emit(this.myOfficeOnly);
  }

  /**
   * Check if My Office filter is available for the current entity type
   */
  isMyOfficeFilterAvailable(): boolean {
    return false;
    // return this.entityType === 'Partner' || this.entityType === 'Contact' || this.entityType === 'Interaction';
  }

  // ===== SavedFilter Event Handlers =====

  /**
   * Handle saved filter applied event - CLEAN IMPLEMENTATION
   */
  onSavedFilterApplied(filter: SavedFilter): void {
    // Simply emit the filter to parent - let parent handle everything
    this.applySavedFilter.emit(filter);
  }

  /**
   * Handle applying criteria from saved filter - CLEAN IMPLEMENTATION
   * This is called AFTER the parent has already updated the state
   * We just need to trigger a search with the current criteria
   */
  onApplyCriteria(criteria: SearchCriteria[]): void {
    // The parent has already updated the searchCriteria state
    // This method is called after successful filter application
    // No additional action needed - the API call is triggered by the parent
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

  /**
   * Clear the saved filter selection when criteria are modified
   */
  private clearSavedFilterSelection(): void {
    if (this.savedFilterComponent) {
      this.savedFilterComponent.clearSelectedFilter();
    }
  }
}
