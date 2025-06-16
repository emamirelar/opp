import { Component, ContentChild, ElementRef, EventEmitter, HostListener, Input, OnInit, Output, TemplateRef, ViewChild, AfterViewInit, computed, inject, effect, OnDestroy, signal, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TranslateModule } from '@ngx-translate/core';
import { ListViewColumn, ListViewConfig, SearchCriteria, SearchParams, EntityType } from './listview.model';
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
import { ListviewTableComponent } from './table/listview-table.component';
import { ListviewCardComponent } from './card/listview-card.component';
import { TooltipModule } from 'primeng/tooltip';
import { SearchField, SearchCriterion } from '../../../services/search-parser.service';
import { ListviewAdvancedSearchComponent } from './advanced-search/listview-advanced-search.component';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { TranslateService } from '@ngx-translate/core';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { SavedFilter } from '../../../interfaces/saved-filter.interface';

@Component({
  selector: 'app-listview',
  templateUrl: './listview.component.html',
  imports: [
    CommonModule,
    TranslateModule,
    TableModule,
    FormsModule,
    InputTextModule,
    ButtonModule,
    IconField,
    InputIcon,
    ConfirmDialog,
    DropdownModule,
    ChipModule,
    OverlayPanelModule,
    ListviewTableComponent,
    ListviewCardComponent,
    TooltipModule,
    ListviewAdvancedSearchComponent,
    AutoCompleteModule,
    IconFieldModule,
    InputIconModule,
  ],
  providers: [ListviewDataLoaderService, ConfirmationService],
  standalone: true
})
export class ListviewComponent<T = any> implements AfterViewInit, OnDestroy {
  private dataLoader = inject(ListviewDataLoaderService);
  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription = Subscription.EMPTY;
  private exportService = inject(ListviewExportService);
  private elRef = inject(ElementRef);
  private resizeObserver: ResizeObserver | null = null;
  private userSelectedViewMode: 'table' | 'card' | null = null;
  private translateService = inject(TranslateService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  @Input() entityType?: EntityType;

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
    this._config = value;
    
    // Force a refresh of the signals to ensure they pick up the new config
    setTimeout(() => {
      
      
      // Re-initialize searchable fields when config changes
      this.initializeSearchableFields();
    }, 0);
    
    // Initialize advanced search if enabled
    if (value.searchConfig?.useAdvancedSearch) {
      
      this.dataLoader.setAdvancedSearchEnabled(true);
    }

    // Set initial view mode from config
    if (value.defaultViewMode) {
      this.viewMode = value.defaultViewMode;
    }
  }
  
  get config(): ListViewConfig {
    return this._config;
  }
  
  private _config: ListViewConfig = {
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enablePagination: true,
    enableSorting: true,
    enableSearch: false,
    enableExport: false,
    scrollable: true,
    scrollHeight: 'flex',
    autoSwitchToCardView: true,
    autoSwitchMinWidth: 768
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

  // View mode (table or card)
  @Input() set defaultViewMode(value: 'table' | 'card') {
    this.viewMode = value;
  }
  viewMode: 'table' | 'card' = 'table';

  // Events
  @Output() rowClick = new EventEmitter<T>();
  @Output() pageChange = new EventEmitter<{first: number, rows: number}>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() searchChange = new EventEmitter<SearchParams>();
  @Output() exportClick = new EventEmitter<void>();
  @Output() viewModeChange = new EventEmitter<'table' | 'card'>();

  // State
  selectedRecord: T | null = null;
  first = 0;
  rows: number;
  searchText = '';
  currentSortField: string | undefined;
  currentSortOrder: 'asc' | 'desc' | undefined;
  isAutoSwitchedToCardView = false;
  
  // Advanced search state
  searchCriteria: SearchCriteria[] = [];
  selectedSearchField: any = null;
  advancedSearchText = '';
  selectedOperator: 'AND' | 'OR' = 'AND';
  operators = [
    { label: 'AND', value: 'AND' },
    { label: 'OR', value: 'OR' }
  ];
  
  // Saved filter state
  preselectedSavedFilterId: number | null = null;
  
  // Autocomplete and search mode state
  isAdvancedSearchMode = signal<boolean>(false);
  autocompleteSuggestions: any[] = [];
  searchValue: any = '';
  
  // Computed properties
  isAdvancedSearch = computed(() => {
    return this.isAdvancedSearchMode();
  });
  searchableFields: SearchField[] = [];
  
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

  // Add signal for current component width
  private componentWidth = signal<number>(0);

  constructor() {
    this.rows = this.config.pageSize || 50;
    this.setupSearchDebounce();
    
    // Initialize searchable fields if available
    setTimeout(() => this.initializeSearchableFields(), 0);
  }

  /**
   * Sync search criteria to URL parameters
   */
  private syncSearchCriteriaToUrl(): void {
    const queryParams: any = { ...this.route.snapshot.queryParams };
    
    if (this.searchCriteria.length > 0) {
      // Encode search criteria as JSON in URL
      queryParams.searchCriteria = JSON.stringify(this.searchCriteria);
      queryParams.advancedSearch = 'true';
    } else {
      // Remove search criteria from URL when cleared
      delete queryParams.searchCriteria;
      delete queryParams.advancedSearch;
    }
    
    // Update URL without triggering navigation
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      replaceUrl: true
    });
  }

  /**
   * Load search criteria from URL parameters
   */
  private loadSearchCriteriaFromUrl(): void {
    const queryParams = this.route.snapshot.queryParams;
    
    // Check if there's a saved filter ID in the URL
    if (queryParams['savedFilterId']) {
      const filterId = parseInt(queryParams['savedFilterId'], 10);
      if (!isNaN(filterId)) {
        console.log('Found saved filter ID in URL:', filterId);
        this.preselectedSavedFilterId = filterId;
        
        // The saved filter component will handle loading this filter
        // We just need to ensure advanced search mode is enabled if specified
        if (queryParams['advancedSearch'] === 'true') {
          this.isAdvancedSearchMode.set(true);
          this.dataLoader.setAdvancedSearchEnabled(true);
        }
      }
      return;
    }
    
    if (queryParams['advancedSearch'] === 'true' && queryParams['searchCriteria']) {
      try {
        const criteria = JSON.parse(queryParams['searchCriteria']) as SearchCriteria[];
        
        // Validate that the criteria are valid
        if (Array.isArray(criteria) && criteria.length > 0) {
          this.searchCriteria = criteria;
          
          // Set advanced search mode
          this.isAdvancedSearchMode.set(true);
          this.dataLoader.setAdvancedSearchEnabled(true);
          
          // Set criteria in data loader
          this.dataLoader.setSearchCriteria(criteria);
          
          
        }
      } catch (error) {
        console.warn('Failed to parse search criteria from URL:', error);
        // Clear invalid parameters
        this.clearSearchCriteriaFromUrl();
      }
    }
  }

  /**
   * Clear search criteria from URL parameters
   */
  private clearSearchCriteriaFromUrl(): void {
    const queryParams: any = { ...this.route.snapshot.queryParams };
    delete queryParams.searchCriteria;
    delete queryParams.advancedSearch;
    delete queryParams.savedFilterId;
    
    // Don't use queryParamsHandling: 'merge' as it prevents deletion of parameters
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      replaceUrl: true
    });
  }

  ngAfterViewInit(): void {
    // Setup resize observer to detect component width changes
    this.setupResizeObserver();
    
    // Check initial component width
    this.checkComponentWidth();
    
    // Load search criteria from URL if present
    this.loadSearchCriteriaFromUrl();
    
    // Load data after URL criteria are loaded
    if (this._dataUrl) {
      this.loadData();
    }
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }

    // Clean up resize observer
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
      this.resizeObserver = null;
    }
  }

  /**
   * Setup resize observer to detect width changes and auto-switch view mode
   */
  private setupResizeObserver(): void {
    if (!window.ResizeObserver) {
      console.warn('ResizeObserver API not supported in this browser');
      return;
    }

    this.resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        const width = entry.contentRect.width;
        this.handleResize(width);
      }
    });

    // Start observing the component's element
    this.resizeObserver.observe(this.elRef.nativeElement);
  }

  /**
   * Handle resize events and auto-switch view mode if necessary
   */
  private handleResize(width: number): void {
    const autoSwitchEnabled = this.config.autoSwitchToCardView !== false;
    const minWidth = this.config.autoSwitchMinWidth || 768;

    if (autoSwitchEnabled) {
      if (width < minWidth) {
        // Auto-switch to card view when width is below threshold
        if (this.viewMode !== 'card') {
          this.isAutoSwitchedToCardView = true;
          this.setViewMode('card', false);
        }
      } else if (this.isAutoSwitchedToCardView && !this.userSelectedViewMode) {
        // Switch back to table view when width increases,
        // but only if the user didn't manually select card view
        this.isAutoSwitchedToCardView = false;
        this.setViewMode('table', false);
      } else if (this.isAutoSwitchedToCardView && this.userSelectedViewMode === 'table') {
        // If user previously selected table view, respect that when width increases
        this.isAutoSwitchedToCardView = false;
        this.setViewMode('table', false);
      }
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
   * Set the view mode and emit change event
   * @param mode View mode to set ('table' or 'card')
   * @param userSelected Whether this change was triggered by the user
   */
  setViewMode(mode: 'table' | 'card', userSelected: boolean = true): void {
    this.viewMode = mode;
    this.viewModeChange.emit(mode);
    
    // Track user selection to handle auto-switching properly
    if (userSelected) {
      this.userSelectedViewMode = mode;
    }
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
   * Handle advanced search criterion from child component
   */
  onAdvancedSearch(criterion: SearchCriteria): void {
    // Add to local list
    this.searchCriteria = [...this.searchCriteria, criterion];
    
    // Add to data loader
    this.dataLoader.addSearchCriterion(criterion);
    
    // Sync to URL
    this.syncSearchCriteriaToUrl();
    
    // Execute search with the updated criteria
    this.executeAdvancedSearch();
  }
  
  /**
   * Remove a search criterion
   */
  onRemoveSearchCriterion(index: number): void {
    if (index >= 0 && index < this.searchCriteria.length) {
      // Remove from data loader by index
      this.dataLoader.removeSearchCriterionByIndex(index);
      
      // Remove from local list
      this.searchCriteria = this.searchCriteria.filter((_, i) => i !== index);
      
      // Sync to URL
      this.syncSearchCriteriaToUrl();
      
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
  onRowClick(event: any): void {
    this.rowClick.emit(event);
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
    if (!this.isAdvancedSearch()) {
      let searchTerm = '';
      if (typeof this.searchValue === 'string') {
        searchTerm = this.searchValue.trim();
      } else if (this.searchValue && this.searchValue.value) {
        searchTerm = this.searchValue.value.trim();
      }
      
      this.searchText = searchTerm; // Keep searchText in sync
      this.executeSearch(searchTerm);
    }
  }

  /**
   * Clear all search criteria
   */
  clearSearch(): void {
    if (this.isAdvancedSearchMode()) {
      this.searchCriteria = [];
      this.dataLoader.clearSearchCriteria();
      // Clear from URL
      this.clearSearchCriteriaFromUrl();
    } else {
      this.searchText = '';
      this.searchValue = '';
      this.dataLoader.setSearchText('');
    }
    
    const searchParams = this.dataLoader.getSearchParams();
    this.searchChange.emit(searchParams);
    
    this.loadData();
  }

  /**
   * Clear all advanced search criteria
   */
  onClearAdvancedSearch(): void {
    this.searchCriteria = [];
    this.dataLoader.clearSearchCriteria();
    
    // Clear from URL
    this.clearSearchCriteriaFromUrl();
    
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
   * Check component width and adjust view mode if necessary
   */
  private checkComponentWidth(): void {
    setTimeout(() => {
      const width = this.elRef.nativeElement.offsetWidth;
      this.componentWidth.set(width);
      this.handleResize(width);
    }, 0);
  }

  /**
   * Load data from the server
   */
  private loadData(): void {
    this.dataLoader.loadData();
    
    // Check component width after data loaded (since content may affect size)
    setTimeout(() => this.checkComponentWidth(), 100);
  }

  /**
   * Used to manually check width (e.g. when container is resized)
   */
  @HostListener('window:resize')
  onWindowResize(): void {
    this.checkComponentWidth();
  }

  private initializeSearchableFields(): void {
    if (this._config.searchConfig?.searchableFields) {
      this.searchableFields = this._config.searchConfig.searchableFields.map(field => {
        const column = this.columns.find(c => c.field === field.field);
        const result = {
          field: field.field,
          label: field.label,
          type: column ? this.getFieldType(column) : 'string',
          operators: column ? this.getOperatorsForType(this.getFieldType(column)) : ['is', 'is not', 'like', 'not like']
        };
        return result;
      });
      return;
    }

    // Fallback to using columns if no searchable fields in config
    if (this.columns && this.columns.length > 0) {
      this.searchableFields = this.columns.map(column => {
        const result = {
          field: column.field,
          label: column.label,
          type: this.getFieldType(column),
          operators: this.getOperatorsForType(this.getFieldType(column))
        };
        return result;
      });
    }
  }

  private getFieldType(column: ListViewColumn): 'string' | 'number' | 'date' {
    switch (column.type) {
      case 'number':
      case 'currency':
        return 'number';
      case 'date':
        return 'date';
      // Handle boolean data as string since 'boolean' is not a valid column type
      default:
        return 'string';
    }
  }

  private getOperatorsForType(type: 'string' | 'number' | 'date'): string[] {
    switch (type) {
      case 'string':
        return ['is', 'is not', 'like', 'not like'];
      case 'number':
        return ['is', 'is not', '>', '<', '>=', '<='];
      case 'date':
        return ['is', 'is not', 'after', 'before', 'between', '>', '<', '>=', '<='];
      default:
        // Boolean values and any other types use basic operators
        return ['is', 'is not'];
    }
  }

  onAdvancedSearchChange(event: { criteria: SearchCriterion[] }): void {
    // Convert criteria to search parameters
    const searchParams = event.criteria.map(criterion => {
      const field = this.searchableFields.find(f => f.field === criterion.field);
      return {
        field: criterion.field,
        label: field?.label || criterion.field,
        value: criterion.value,
        operator: criterion.operator,
        logicalOperator: criterion.logicalOperator
      };
    });

    // Update the search criteria
    this.searchCriteria = searchParams;
    
    // Trigger search
    this.onSearch();
  }

  /**
   * Handle autocomplete search input
   */
  onAutocompleteSearch(event: any): void {
    const query = event.query?.toLowerCase() || '';
    
    // Create suggestions based on searchable fields
    this.autocompleteSuggestions = [];
    
    if (query.length > 0) {
      // Add general search suggestion first (using translation)
      const searchEverywhere = this.translateService?.instant('search.searchEverywhere') || 'Search everywhere';
      this.autocompleteSuggestions.push({
        label: `${searchEverywhere}: "${event.query}"`,
        value: event.query,
        field: null,
        type: 'general'
      });
      
      // Add field-based suggestions
      const searchIn = this.translateService?.instant('search.searchIn') || 'Search in';
      this.searchableFields.forEach(field => {
        if (field.label.toLowerCase().includes(query) || field.field.toLowerCase().includes(query)) {
          this.autocompleteSuggestions.push({
            label: `${searchIn} ${field.label}: "${event.query}"`,
            value: event.query,
            field: field.field,
            type: 'field'
          });
        }
      });
    }
  }

  /**
   * Handle autocomplete selection
   */
  onAutocompleteSelect(event: any): void {
    // Perform the search
    this.searchText = event.value;
    this.searchValue = event.value;
    this.executeSearch(event.value);
  }

  /**
   * Switch to advanced search mode
   */
  switchToAdvancedSearch(): void {
    this.isAdvancedSearchMode.set(true);
    this.dataLoader.setAdvancedSearchEnabled(true);
    // Clear all simple search values
    this.searchValue = '';
    this.searchText = '';
    // Clear any existing simple search from data loader
    this.dataLoader.setSearchText('');
  }

  /**
   * Switch back to simple search mode
   */
  switchToSimpleSearch(): void {
    this.isAdvancedSearchMode.set(false);
    this.dataLoader.setAdvancedSearchEnabled(false);
    this.searchCriteria = [];
    this.dataLoader.clearSearchCriteria();
    
    // Clear search criteria from URL when switching to simple search
    this.clearSearchCriteriaFromUrl();
    
    this.loadData();
  }

  /**
   * Handle My Office filter change
   */
  onMyOfficeFilterChanged(enabled: boolean): void {
    // Update data loader with My Office filter state
    this.dataLoader.setMyOfficeFilter(enabled);
    
    // Execute search with updated filter
    this.executeAdvancedSearch();
  }

  /**
   * Handle saved filter applied event
   */
  onApplySavedFilter(filter: SavedFilter): void {
    // Update URL parameters with saved filter information
    const queryParams: any = { ...this.route.snapshot.queryParams };
    
    // Add saved filter ID to URL for tracking
    queryParams.savedFilterId = filter.id;
    
    // If it's an advanced search, mark it in URL
    if (filter.isAdvancedSearch) {
      queryParams.advancedSearch = 'true';
    }
    
    // Update sorting if specified in the filter
    if (filter.orderBy) {
      this.currentSortField = filter.orderBy;
      this.currentSortOrder = filter.ascending ? 'asc' : 'desc';
      this.dataLoader.setSorting(filter.orderBy, this.currentSortOrder);
    }
    
    // Update URL without triggering navigation
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      replaceUrl: true
    });
    
    // Reset pagination to first page
    this.first = 0;
    this.dataLoader.setPagination(0, this.rows);
    
    // Clear the preselected filter ID to avoid reprocessing
    this.preselectedSavedFilterId = null;
    
    // Load data with the applied filter
    this.loadData();
  }
}
