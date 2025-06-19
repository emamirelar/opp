import { Component, ContentChild, ElementRef, EventEmitter, HostListener, Input, OnInit, Output, TemplateRef, ViewChild, AfterViewInit, computed, inject, effect, OnDestroy, signal, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ListViewColumn, ListViewConfig, SearchCriteria, SearchParams, EntityType, ListViewData } from './listview.model';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { Subject, debounceTime, distinctUntilChanged, Subscription, catchError, tap, of } from 'rxjs';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { ListviewExportService } from './listview-export.service';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { DropdownModule } from 'primeng/dropdown';
import { SelectModule } from 'primeng/select';
import { ChipModule } from 'primeng/chip';
import { OverlayPanelModule } from 'primeng/overlaypanel';
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
    FormsModule,
    InputTextModule,
    ButtonModule,
    IconField,
    InputIcon,
    ConfirmDialog,
    DropdownModule,
    SelectModule,
    ChipModule,
    OverlayPanelModule,
    ListviewCardComponent,
    TooltipModule,
    ListviewAdvancedSearchComponent,
    AutoCompleteModule,
    IconFieldModule,
    InputIconModule,
  ],
  providers: [ConfirmationService],
  standalone: true
})
export class ListviewComponent<T = any> implements AfterViewInit, OnDestroy {
  private http = inject(HttpClient);
  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription = Subscription.EMPTY;
  private exportService = inject(ListviewExportService);
  private elRef = inject(ElementRef);
  private resizeObserver: ResizeObserver | null = null;
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
    if (value && value !== this._dataUrl) {
      this._dataUrl = value;
      // Use setTimeout to avoid race conditions with ngAfterViewInit
      setTimeout(() => {
        if (!this.hasInitialDataLoaded) {
          this.loadData();
        }
      }, 0);
    }
  }
  private _dataUrl: string = '';

  // State signals
  private loadingState = signal<boolean>(false);
  private errorState = signal<boolean>(false);
  private dataState = signal<ListViewData<T>>({ records: [], totalCount: 0 });

  // HTTP state
  private pageIndex = 1;
  private pageSize = 20;
  private sortField = '';
  private sortOrder: 'asc' | 'desc' = 'asc';
  private searchTextValue = '';
  private useAdvancedSearch = false;
  
  // Sort state
  currentSortConfig: string = '';
  private sortableFieldsCache: ListViewColumn[] = [];

  @Input() columns: ListViewColumn[] = [];

  @Input() set fullTextSearch(value: string) {
    this._fullTextSearch = value;
    if (this._dataUrl) {
      this.searchTextValue = value;
      this.hasInitialDataLoaded = true; // Mark as loaded to prevent duplicate calls
      this.loadData();
    }
  }
  private _fullTextSearch: string = '';

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
      this.useAdvancedSearch = true;
    }

    // Update page size from config
    this.pageSize = value.pageSize || 20;

    // Initialize default sort
    this.initializeDefaultSort();
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
    autoSwitchToCardView: false,
    autoSwitchMinWidth: 768,
    defaultViewMode: 'card'
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

  // View mode (card only)
  viewMode: 'card' = 'card';

  // Events
  @Output() rowClick = new EventEmitter<T>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() searchChange = new EventEmitter<SearchParams>();
  @Output() exportClick = new EventEmitter<void>();
  @Output() totalRecordsChange = new EventEmitter<number>();
  @Output() loadMore = new EventEmitter<void>();

  // State
  selectedRecord: T | null = null;
  searchText = '';
  currentSortField: string | undefined;
  currentSortOrder: 'asc' | 'desc' | undefined;

  // Pagination properties
  first = 0;
  rows = 20;

  // Data loader (mock for now until proper implementation)
  dataLoader = {
    setAdvancedSearchEnabled: (enabled: boolean) => {
      this.useAdvancedSearch = enabled;
    },
    setMyOfficeFilter: (enabled: boolean) => {
      // Implementation for my office filter
      console.log('My office filter:', enabled);
    },
    setSorting: (field: string, order: 'asc' | 'desc') => {
      this.currentSortField = field;
      this.currentSortOrder = order;
    },
    setPagination: (first: number, rows: number) => {
      this.first = first;
      this.rows = rows;
      this.pageIndex = Math.floor(first / rows) + 1;
      this.pageSize = rows;
    }
  };

  // Infinite scroll state
  private allLoadedData = signal<T[]>([]);
  private hasMoreData = signal<boolean>(true);
  isLoadingMore = signal<boolean>(false);

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

  // Computed properties
  isLoading = computed(() => this.loadingState() || this.isLoadingMore());
  hasError = computed(() => this.errorState());
  currentPageData = computed(() => this.allLoadedData());
  totalRecordsCount = computed(() => this.dataState().totalCount);
  hasActionsTemplate = computed(() => !!this.actionsTemplate);
  hasMoreDataAvailable = computed(() => this.hasMoreData());

  // Add signal for current component width
  private componentWidth = signal<number>(0);

  // Track if initial data has been loaded to prevent double calls
  private hasInitialDataLoaded = false;

  constructor() {
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
          this.useAdvancedSearch = true;


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
    this.initializeComponent();
  }

  /**
   * Initialize component - centralized initialization logic
   */
  private initializeComponent(): void {
    // Setup resize observer to detect component width changes
    this.setupResizeObserver();

    // Check initial component width
    this.checkComponentWidth();

    // Load search criteria from URL if present
    this.loadSearchCriteriaFromUrl();

    // Only load data if not already triggered by URL criteria loading or dataUrl setter
    if (this._dataUrl && !this.hasInitialDataLoaded) {
      this.hasInitialDataLoaded = true;
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
   * Handle resize events (simplified for card-only view)
   */
  private handleResize(width: number): void {
    // No view mode switching needed - always card view
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
   * Handle advanced search criterion from child component
   */
  onAdvancedSearch(criterion: SearchCriteria): void {
    // Add to local list
    this.searchCriteria = [...this.searchCriteria, criterion];

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
    this.pageIndex = 1;
    this.allLoadedData.set([]); // Reset loaded data
    this.hasMoreData.set(true);
    this.hasInitialDataLoaded = true; // Mark as loaded to prevent duplicate calls

    const searchParams = this.getSearchParams();
    this.searchChange.emit(searchParams);

    this.loadData();
  }

  /**
   * Execute the search with the given value (simple search)
   */
  private executeSearch(value: string): void {
    this.searchTextValue = value;

    const searchParams = this.getSearchParams();
    this.searchChange.emit(searchParams);

    this.pageIndex = 1;
    this.allLoadedData.set([]); // Reset loaded data
    this.hasMoreData.set(true);
    this.hasInitialDataLoaded = true; // Mark as loaded to prevent duplicate calls
    this.loadData();
  }

  /**
   * Handle row selection
   */
  onRowClick(event: any): void {
    this.rowClick.emit(event);
  }

  /**
   * Handle load more event for infinite scroll
   */
  onLoadMore(): void {
    if (this.hasMoreData() && !this.isLoadingMore()) {
      this.isLoadingMore.set(true);
      this.pageIndex++;
      this.loadData();
      this.loadMore.emit();
    }
  }

  /**
   * Handle sort change event
   */
  onSortChange(event: any): void {
    const order = event.order === 1 ? 'asc' : 'desc';
    this.currentSortField = event.field;
    this.currentSortOrder = order;
    this.sortChange.emit({ field: event.field, order });

    this.sortField = event.field;
    this.sortOrder = order;
    this.loadData();
  }

  /**
   * Get sortable fields from columns
   */
  sortableFields(): ListViewColumn[] {
    if (this.sortableFieldsCache.length === 0) {
      this.sortableFieldsCache = this.columns.filter(col => col.sortable);
    }
    return this.sortableFieldsCache;
  }

  /**
   * Get sort options for dropdown
   */
  sortOptions(): Array<{ label: string, value: string }> {
    const options: Array<{ label: string, value: string }> = [];
    
    this.sortableFields().forEach(field => {
      // Add ascending option
      options.push({
        label: `${field.label} (${this.translateService.instant('label.ascending')})`,
        value: `${field.field}:asc`
      });
      
      // Add descending option
      options.push({
        label: `${field.label} (${this.translateService.instant('label.descending')})`,
        value: `${field.field}:desc`
      });
    });
    
    return options;
  }

  /**
   * Handle sort configuration change from dropdown
   */
  onSortConfigChange(sortConfig: string): void {
    if (!sortConfig) {
      this.clearSort();
      return;
    }

    const [field, order] = sortConfig.split(':');
    this.currentSortField = field;
    this.currentSortOrder = order as 'asc' | 'desc';
    this.sortField = field;
    this.sortOrder = order as 'asc' | 'desc';
    
    this.sortChange.emit({ field, order: order as 'asc' | 'desc' });
    this.loadData();
  }

  /**
   * Clear sort configuration
   */
  clearSort(): void {
    this.currentSortConfig = '';
    this.currentSortField = '';
    this.currentSortOrder = 'asc';
    this.sortField = '';
    this.sortOrder = 'asc';
    
    this.sortChange.emit({ field: '', order: 'asc' });
    this.loadData();
  }

  /**
   * Initialize default sort configuration from config
   */
  private initializeDefaultSort(): void {
    if (this.config.defaultSortField && this.config.defaultSortOrder) {
      this.sortField = this.config.defaultSortField;
      this.sortOrder = this.config.defaultSortOrder;
      this.currentSortField = this.config.defaultSortField;
      this.currentSortOrder = this.config.defaultSortOrder;
      this.currentSortConfig = `${this.config.defaultSortField}:${this.config.defaultSortOrder}`;
    }
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
      // Clear from URL
      this.clearSearchCriteriaFromUrl();
    } else {
      this.searchText = '';
      this.searchValue = '';
      this.searchTextValue = '';
    }

    const searchParams = this.getSearchParams();
    this.searchChange.emit(searchParams);

    this.pageIndex = 1;
    this.allLoadedData.set([]); // Reset loaded data
    this.hasMoreData.set(true);
    this.loadData();
  }

  /**
   * Clear all advanced search criteria
   */
  onClearAdvancedSearch(): void {
    this.searchCriteria = [];

    // Clear from URL
    this.clearSearchCriteriaFromUrl();

    const searchParams = this.getSearchParams();
    this.searchChange.emit(searchParams);

    this.pageIndex = 1;
    this.allLoadedData.set([]); // Reset loaded data
    this.hasMoreData.set(true);
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
      const searchParams = this.getSearchParams();

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
   * Get current search parameters
   */
  private getSearchParams(): SearchParams {
    if (this.useAdvancedSearch) {
      return {
        fieldSearches: this.searchCriteria
      };
    } else {
      return {
        generalSearch: this.searchTextValue
      };
    }
  }

  /**
   * Load data from the server
   */
  private loadData(): void {
    if (!this._dataUrl || this.loadingState()) {
      return; // Avoid concurrent calls
    }

    this.loadingState.set(true);
    this.errorState.set(false);

    const params = this.buildHttpParams();

    this.http.get<any>(this._dataUrl, { params }).pipe(
      tap(response => {
        this.handleDataResponse(response);
        this.loadingState.set(false);
        // Check component width after data loaded
        setTimeout(() => this.checkComponentWidth(), 100);
      }),
      catchError(err => {
        this.loadingState.set(false);
        this.errorState.set(true);
        console.error('Error loading data:', err);
        this.dataState.set({ records: [], totalCount: 0 });
        return of({ records: [], totalCount: 0 } as ListViewData<T>);
      })
    ).subscribe();
  }

  /**
   * Build HTTP parameters for the request
   */
  private buildHttpParams(): HttpParams {
    let params = new HttpParams()
      .set('pageIndex', this.pageIndex.toString())
      .set('pageSize', this.pageSize.toString());

    // Sorting
    if (this.sortField) {
      params = params
        .set('orderBy', this.sortField)
        .set('ascending', (this.sortOrder === 'asc').toString());
    }

    // Search parameters
    if (this.useAdvancedSearch && this.searchCriteria.length > 0) {
      params = params
        .set('advancedSearch', 'true')
        .set('searchCriteria', JSON.stringify(this.searchCriteria));
    } else if (this.searchTextValue?.trim()) {
      params = params.set('searchText', this.searchTextValue.trim());
    }

    return params;
  }

  /**
   * Handle data response from server
   */
  private handleDataResponse(data: any): void {
    let totalCount = 0;
    let newRecords: T[] = [];

    if (Array.isArray(data)) {
      totalCount = data.length;
      newRecords = data;
    } else if (data?.records && Array.isArray(data.records)) {
      totalCount = data.totalCount || data.records.length;
      newRecords = data.records;
    }

    // For infinite scroll, append new data to existing data
    if (this.pageIndex === 1) {
      // First page or reset - replace all data
      this.allLoadedData.set(newRecords);
    } else {
      // Subsequent pages - append to existing data
      const currentData = this.allLoadedData();
      this.allLoadedData.set([...currentData, ...newRecords]);
    }

    // Update state signals
    this.dataState.set({
      records: this.allLoadedData(),
      totalCount
    });

    // Check if there's more data to load
    const loadedCount = this.allLoadedData().length;
    this.hasMoreData.set(loadedCount < totalCount);
    this.isLoadingMore.set(false);

    // Emit the total records count
    this.totalRecordsChange.emit(totalCount);
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
    this.useAdvancedSearch = true;
    // Clear all simple search values
    this.searchValue = '';
    this.searchText = '';
    this.searchTextValue = '';
  }

  /**
   * Switch back to simple search mode
   */
  switchToSimpleSearch(): void {
    this.isAdvancedSearchMode.set(false);
    this.useAdvancedSearch = false;
    this.searchCriteria = [];

    // Clear search criteria from URL when switching to simple search
    this.clearSearchCriteriaFromUrl();

    this.pageIndex = 1;
    this.allLoadedData.set([]); // Reset loaded data
    this.hasMoreData.set(true);
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
