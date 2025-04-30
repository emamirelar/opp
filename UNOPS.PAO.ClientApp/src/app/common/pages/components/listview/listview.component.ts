import { Component, ContentChild, EventEmitter, HostListener, Input, OnInit, Output, TemplateRef, computed, inject, effect, OnDestroy } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TranslateModule } from '@ngx-translate/core';
import { ListViewColumn, ListViewConfig, SearchCriteria, SearchParams } from './listview.model';
import { ListviewDataLoaderService } from './listview-data-loader.service';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { Subject, debounceTime, distinctUntilChanged, Subscription } from 'rxjs';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { ListviewExportService } from './listview-export.service';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { DropdownModule } from 'primeng/dropdown';
import { ChipModule } from 'primeng/chip';
import { OverlayPanelModule } from 'primeng/overlaypanel';

@Component({
  selector: 'app-listview',
  templateUrl: './listview.component.html',
  styleUrl: './listview.component.scss',
  imports: [
    CommonModule,
    TranslateModule,
    DatePipe,
    DecimalPipe,
    CurrencyPipe,
    TableModule,
    FormsModule,
    InputTextModule,
    ButtonModule,
    IconField,
    InputIcon,
    ConfirmDialog,
    DropdownModule,
    ChipModule,
    OverlayPanelModule
  ],
  providers: [ListviewDataLoaderService, ConfirmationService]
})
export class ListviewComponent<T = any> implements OnDestroy {
  private dataLoader = inject(ListviewDataLoaderService);
  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription = Subscription.EMPTY;
  private exportService = inject(ListviewExportService);

  // Listen for refresh events
  @HostListener('window:refresh-listview')
  refreshData() {
    this.loadData();
  }

  // Custom template references
  @ContentChild('actionsTemplate') actionsTemplate?: TemplateRef<any>;

  // Core inputs
  @Input() set dataUrl(value: string) {
    if (value) {
      this.dataLoader.setUrl(value);
      this._dataUrl = value;
      this.loadData();
    }
  }
  private _dataUrl: string = '';

  @Input() columns: ListViewColumn[] = [];
  
  @Input() 
  set config(value: ListViewConfig) {
    console.log('Listview config set:', value);
    console.log('Advanced search enabled:', !!value.searchConfig?.useAdvancedSearch);
    console.log('Searchable fields:', value.searchConfig?.searchableFields);
    
    this._config = value;
    
    // Force a refresh of the signals to ensure they pick up the new config
    setTimeout(() => {
      console.log('Forcing signal refresh, isAdvancedSearch:', this.isAdvancedSearch());
    }, 0);
    
    // Initialize advanced search if enabled
    if (value.searchConfig?.useAdvancedSearch) {
      console.log('Setting advanced search enabled in data loader');
      this.dataLoader.setAdvancedSearchEnabled(true);
    }
  }
  
  get config(): ListViewConfig {
    return this._config;
  }
  
  private _config: ListViewConfig = {
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    selectionMode: 'single',
    enablePagination: true,
    enableSorting: true,
    enableSearch: false,
    enableExport: false,
    scrollable: true,
    scrollHeight: 'flex'
  };
  @Input() set searchDebounceTime(value: number) {
    this._searchDebounceTime = value;
    this.setupSearchDebounce();
  }
  get searchDebounceTime(): number {
    return this._searchDebounceTime;
  }
  private _searchDebounceTime = 500; // Default debounce time in ms
  @Input() idField = 'id';

  // Events
  @Output() rowSelect = new EventEmitter<T>();
  @Output() rowDblClick = new EventEmitter<T>();
  @Output() pageChange = new EventEmitter<{first: number, rows: number}>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() searchChange = new EventEmitter<SearchParams>();
  @Output() exportClick = new EventEmitter<void>();

  // State
  selectedRecord: T | null = null;
  first = 0;
  rows: number;
  searchText = '';
  currentSortField: string | undefined;
  currentSortOrder: 'asc' | 'desc' | undefined;
  
  // Advanced search state
  searchCriteria: SearchCriteria[] = [];
  selectedSearchField: any = null;
  advancedSearchText = '';
  selectedOperator: 'AND' | 'OR' = 'AND';
  operators = [
    { label: 'AND', value: 'AND' },
    { label: 'OR', value: 'OR' }
  ];
  
  // Computed properties
  isAdvancedSearch = computed(() => {
    console.log('Computing isAdvancedSearch, config:', this.config);
    console.log('searchConfig exists:', !!this.config.searchConfig);
    console.log('useAdvancedSearch value:', this.config.searchConfig?.useAdvancedSearch);
    
    return !!this.config.searchConfig?.useAdvancedSearch;
  });
  searchableFields = computed(() => this.config.searchConfig?.searchableFields || []);
  searchPlaceholder = computed(() => 
    this.config.searchConfig?.placeholder || 
    (this.config.searchConfig?.useAdvancedSearch ? 'Search by field...' : 'Search...')
  );

  // Computed properties from data loader
  isLoading = computed(() => this.dataLoader.isLoading());
  hasError = computed(() => this.dataLoader.hasError());
  currentPageData = computed(() => this.dataLoader.currentPageData());
  totalRecordsCount = computed(() => this.dataLoader.totalRecordsCount());
  hasActionsTemplate = computed(() => !!this.actionsTemplate);

  constructor() {
    this.rows = this.config.pageSize || 50;
    this.setupSearchDebounce();
    
    // Add debugging for initialization
    console.log('ListviewComponent constructor');
    console.log('Initial config:', this.config);
    console.log('Is advanced search?', this.isAdvancedSearch());
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  /**
   * Setup search debounce
   */
  private setupSearchDebounce(): void {
    // Clean up existing subscription if it exists
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }

    // Create new subscription with current debounce time
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(this.searchDebounceTime),
      distinctUntilChanged()
    ).subscribe(searchValue => {
      this.executeSearch(searchValue);
    });
  }

  /**
   * Handle search input from the user
   */
  onSearchInput(value: string): void {
    // Only use debounce for simple search mode
    if (!this.config.searchConfig?.useAdvancedSearch) {
      this.searchSubject.next(value);
    }
  }
  
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
    const criterion: SearchCriteria = {
      field: this.selectedSearchField.field,
      value: this.advancedSearchText.trim(),
      label: this.selectedSearchField.label,
      operator: this.selectedOperator
    };
    
    // Add to local list
    this.searchCriteria = [...this.searchCriteria, criterion];
    
    // Add to data loader
    this.dataLoader.addSearchCriterion(criterion);
    
    // Clear the input fields
    this.advancedSearchText = '';
    this.selectedSearchField = null;
    this.selectedOperator = 'AND'; // Reset operator to default
    
    // Execute search with the updated criteria
    this.executeAdvancedSearch();
  }
  
  /**
   * Remove a search criterion
   */
  removeSearchCriterion(index: number): void {
    if (index >= 0 && index < this.searchCriteria.length) {
      const criterion = this.searchCriteria[index];
      
      // Remove from data loader
      this.dataLoader.removeSearchCriterion(criterion.field);
      
      // Remove from local list
      this.searchCriteria = this.searchCriteria.filter((_, i) => i !== index);
      
      // Execute search with the updated criteria
      this.executeAdvancedSearch();
    }
  }
  
  /**
   * Execute advanced search with current criteria
   */
  executeAdvancedSearch(): void {
    this.first = 0; // Reset to first page
    this.dataLoader.setPagination(0, this.rows);
    
    const searchParams = this.dataLoader.getSearchParams();
    this.searchChange.emit(searchParams);
    
    this.loadData();
  }

  /**
   * Execute the search with the given value (simple search)
   */
  private executeSearch(value: string): void {
    this.dataLoader.setSearchText(value);
    
    const searchParams = this.dataLoader.getSearchParams();
    this.searchChange.emit(searchParams);
    
    this.first = 0; // Reset to first page
    this.dataLoader.setPagination(0, this.rows);
    this.loadData();
  }

  /**
   * Handle row selection
   */
  onRowSelect(event: any): void {
    this.rowSelect.emit(event.data);
  }

  /**
   * Handle row double click
   */
  onRowDblClick(rowData: T): void {
    this.rowDblClick.emit(rowData);
  }

  /**
   * Handle page change event
   */
  onPageChange(event: any): void {
    this.first = event.first;
    this.rows = event.rows;
    this.pageChange.emit(event);

    const pageIndex = Math.floor(event.first / event.rows);
    this.dataLoader.setPagination(pageIndex, event.rows);
    this.loadData();
  }

  /**
   * Handle sort change event
   */
  onSortChange(event: any): void {
    const order = event.order === 1 ? 'asc' : 'desc';
    this.currentSortField = event.field;
    this.currentSortOrder = order;
    this.sortChange.emit({ field: event.field, order });

    this.dataLoader.setSorting(event.field, order);
    this.loadData();
  }

  /**
   * Handle search event (for backward compatibility with enter key)
   */
  onSearch(): void {
    if (this.config.searchConfig?.useAdvancedSearch) {
      if (this.selectedSearchField && this.advancedSearchText) {
        this.addSearchCriterion();
      }
    } else {
      const trimmedValue = this.searchText.trim();
      this.searchText = trimmedValue; // Update the input with trimmed value
      this.executeSearch(trimmedValue);
    }
  }

  /**
   * Clear all search criteria
   */
  clearSearch(): void {
    if (this.config.searchConfig?.useAdvancedSearch) {
      this.searchCriteria = [];
      this.dataLoader.clearSearchCriteria();
      this.advancedSearchText = '';
      this.selectedSearchField = null;
    } else {
      this.searchText = '';
      this.dataLoader.setSearchText('');
    }
    
    const searchParams = this.dataLoader.getSearchParams();
    this.searchChange.emit(searchParams);
    
    this.loadData();
  }

  /**
   * Export data to Google Sheets
   */
  exportData(): void {
    if (this.config.enableExport && this._dataUrl) {
      // If user has explicitly implemented their own handler, use that
      if (this.exportClick.observed) {
        this.exportClick.emit();
        return;
      }

      // Otherwise use our built-in export functionality
      const entityName = this.config.entityName || 'Record';
      
      // Use the customTransform function from config if provided
      const customTransform = this.config.exportOptions?.customTransform || 
        // Otherwise create a transform that excludes fields if specified
        (this.config.exportOptions?.excludeFields ? 
          (data: any[]) => {
            return data.map(item => {
              const result: Record<string, any> = {};
              const excludeFields = this.config.exportOptions?.excludeFields || [];
              
              Object.entries(item).forEach(([key, value]) => {
                if (!excludeFields.includes(key)) {
                  result[key] = value;
                }
              });
              
              return result;
            });
          } : undefined);
      
      // Get the search parameters
      const searchParams = this.dataLoader.getSearchParams();
      
      this.exportService.exportToGoogleSheet(
        entityName,
        this._dataUrl,
        searchParams,  // Pass the full search parameters object
        this.currentSortField || this.config.defaultSortField,
        this.currentSortOrder || this.config.defaultSortOrder,
        customTransform
      ).subscribe();
    }
  }

  /**
   * Get the value for the scrollHeight property
   */
  get scrollHeightValue(): string | undefined {
    if (!this.config.scrollable) return undefined;
    return this.config.scrollHeight === 'flex' 
      ? 'calc(100vh - 16rem)' // Default flexible height
      : this.config.scrollHeight;
  }

  /**
   * Load data from the server
   */
  private loadData(): void {
    this.dataLoader.loadData();
  }
}
