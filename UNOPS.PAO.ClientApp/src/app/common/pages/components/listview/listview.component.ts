import { Component, ContentChild, ElementRef, EventEmitter, HostListener, Input, Output, TemplateRef, AfterViewInit, computed, inject, signal, ChangeDetectorRef, DestroyRef, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ListViewColumn, ListViewConfig, SearchCriteria, SearchParams, EntityType, ListViewData } from './listview.model';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { Subject, debounceTime, distinctUntilChanged, catchError, tap, of, switchMap } from 'rxjs';
import { GlobalFilterService } from '../../../../services/global-filter.service';
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
import { SearchField } from '../../../services/search-parser.service';
import { ListviewAdvancedSearchComponent } from './advanced-search/listview-advanced-search.component';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { TranslateService } from '@ngx-translate/core';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { SavedFilter } from '../../../interfaces/saved-filter.interface';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserPreferenceService, GlobalFilters } from '../../../../services/user-preference.service';
import { AuthService } from '../../../../essentials/services/auth.service';
import { GlobalFiltersDialogService } from '../../../../services/global-filters-dialog.service';

interface ListViewState<T> {
  loading: boolean;
  loadingMore: boolean;
  error: boolean;
  data: T[];
  totalCount: number;
  hasMoreData: boolean;
  pageIndex: number;
  pageSize: number;
  sortField: string;
  sortOrder: 'asc' | 'desc';
  searchText: string;
  searchCriteria: SearchCriteria[];
  isAdvancedSearchMode: boolean;
  componentWidth: number;
}

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
export class ListviewComponent<T = any> implements AfterViewInit {
  private readonly http = inject(HttpClient);
  private readonly exportService = inject(ListviewExportService);
  private readonly elRef = inject(ElementRef);
  private readonly translateService = inject(TranslateService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly globalFilterService = inject(GlobalFilterService);
  private readonly userPreferenceService = inject(UserPreferenceService);
  private readonly authService = inject(AuthService);
  private readonly globalFiltersDialogService = inject(GlobalFiltersDialogService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  // State management
  private readonly state = signal<ListViewState<T>>({
    loading: false,
    loadingMore: false,
    error: false,
    data: [],
    totalCount: 0,
    hasMoreData: true,
    pageIndex: 1,
    pageSize: 20,
    sortField: '',
    sortOrder: 'asc',
    searchText: '',
    searchCriteria: [],
    isAdvancedSearchMode: false,
    componentWidth: 0
  });

  // Computed signals
  readonly isLoading = computed(() => this.state().loading);
  readonly isLoadingMore = computed(() => this.state().loadingMore);
  readonly hasError = computed(() => this.state().error);
  readonly currentPageData = computed(() => this.state().data);
  readonly totalRecordsCount = computed(() => this.state().totalCount);
  readonly hasMoreDataAvailable = computed(() => this.state().hasMoreData);
  readonly isAdvancedSearch = computed(() => this.state().isAdvancedSearchMode);
  readonly currentSortField = computed(() => this.state().sortField);
  readonly currentSortOrder = computed(() => this.state().sortOrder);

  readonly searchPlaceholder = computed(() =>
    this.config.searchConfig?.placeholder ||
    (this.config.searchConfig?.useAdvancedSearch ? 'Search by field...' : 'Search...')
  );

  // Computed property to determine if mobile mode is active
  readonly isMobileMode = computed(() => {
    const config = this.config;
    const { componentWidth } = this.state();
    
    // Force mobile mode if configured
    if (config.forceMobileMode) {
      return true;
    }
    
    // Check auto-switch conditions
    if (config.autoSwitchToCardView && componentWidth > 0) {
      const minWidth = config.autoSwitchMinWidth || 768;
      return componentWidth < minWidth;
    }
    
    return false;
  });

  // Search handling
  private readonly searchSubject = new Subject<string>();
  private readonly loadDataSubject = new Subject<void>();
  private resizeObserver: ResizeObserver | null = null;

  // Global filter information
  isGlobalFilterActive = signal(false);
  globalFilters = signal<GlobalFilters | null>(null);
  currentUserId = signal<string>('');
  activeFilterLabels = signal<string[]>([]);
  
  // Filter toggle state - tracks whether filters are temporarily disabled
  isFilterTemporarilyDisabled = signal(false);
  
  // Record counts for display - we'll use the current totalRecordsCount for now
  // In the future, we could make separate API calls to get unfiltered totals

  // Template references
  @ContentChild('actionsTemplate') actionsTemplate?: TemplateRef<any>;

  // Inputs
  entityType = input<EntityType>();
  columns = input<ListViewColumn[]>([]);
  idField = input('id');

  @Input() set dataUrl(value: string) {
    if (value && value !== this._dataUrl) {
      this._dataUrl = value;
      setTimeout(() => this.loadData(), 0);
    }
  }
  private _dataUrl: string = '';

  @Input() set fullTextSearch(value: string) {
    if (this._dataUrl) {
      this.state.update(s => ({ ...s, searchText: value }));
      this.loadData();
    }
  }

  @Input()
  set config(value: ListViewConfig) {
    this._config = value;

    // Initialize searchable fields when config changes
    setTimeout(() => this.initializeSearchableFields(), 0);

    // Update state from config
    this.state.update(s => ({
      ...s,
      pageSize: value.pageSize || 20,
      isAdvancedSearchMode: (value.searchConfig?.useAdvancedSearch && s.searchCriteria.length > 0) || false
    }));

    // Initialize default sort
    if (value.defaultSortField && value.defaultSortOrder) {
      this.state.update(s => ({
        ...s,
        sortField: value.defaultSortField!,
        sortOrder: value.defaultSortOrder!
      }));
      this.currentSortConfig = `${value.defaultSortField}:${value.defaultSortOrder}`;
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
    autoSwitchToCardView: false,
    autoSwitchMinWidth: 768,
    defaultViewMode: 'card',
    forceMobileMode: false
  };

  @Input() set searchDebounceTime(value: number) {
    this._searchDebounceTime = value;
    this.setupSearchDebounce();
  }
  get searchDebounceTime(): number {
    return this._searchDebounceTime;
  }
  private _searchDebounceTime = 500;

  // Outputs
  @Output() rowClick = new EventEmitter<T>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() searchChange = new EventEmitter<SearchParams>();
  @Output() exportClick = new EventEmitter<void>();
  @Output() totalRecordsChange = new EventEmitter<number>();
  @Output() loadMore = new EventEmitter<void>();

  // Component state
  viewMode: 'card' = 'card';
  searchableFields: SearchField[] = [];
  searchValue: any = '';
  currentSortConfig: string = '';
  operators = [
    { label: 'AND', value: 'AND' },
    { label: 'OR', value: 'OR' }
  ];

  // Data loader mock (for compatibility)
  dataLoader = {
    setMyOfficeFilter: (enabled: boolean) => {
      console.log('My office filter:', enabled);
    },
    setPagination: (first: number, rows: number) => {
      const pageIndex = Math.floor(first / rows) + 1;
      this.state.update(s => ({ ...s, pageIndex, pageSize: rows }));
    }
  };

  constructor() {
    this.setupSearchDebounce();
    this.setupLoadDataStream();
    this.loadGlobalFilterInfo();

    // Note: Global filter display is now handled via loadGlobalFilterInfo()
    // which gets the org unit name directly from the backend

    // Subscribe to global filter changes (when filters are saved)
    this.globalFilterService.filtersChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        // Reload global filter information for UI display
        this.loadGlobalFilterInfo();
        
        // Reload the listview data when filters change
        this.refreshData();
      });

    // Cleanup resize observer on destroy
    this.destroyRef.onDestroy(() => {
      if (this.resizeObserver) {
        this.resizeObserver.disconnect();
        this.resizeObserver = null;
      }
    });
  }

  @HostListener('window:refresh-listview')
  refreshData() {
    this.loadData();
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    this.checkComponentWidth();
  }

  ngAfterViewInit(): void {
    this.initializeComponent();
  }

  private initializeComponent(): void {
    this.setupResizeObserver();
    this.checkComponentWidth();
    this.loadSearchCriteriaFromUrl();

    if (this._dataUrl) {
      this.loadData();
    }
  }

  private setupResizeObserver(): void {
    if (!window.ResizeObserver) {
      console.warn('ResizeObserver API not supported in this browser');
      return;
    }

    this.resizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        const width = entry.contentRect.width;
        this.state.update(s => ({ ...s, componentWidth: width }));
      }
    });

    this.resizeObserver.observe(this.elRef.nativeElement);
  }

  private checkComponentWidth(): void {
    setTimeout(() => {
      const width = this.elRef.nativeElement.offsetWidth;
      this.state.update(s => ({ ...s, componentWidth: width }));
    }, 0);
  }

  private setupSearchDebounce(): void {
    this.searchSubject.pipe(
      debounceTime(this.searchDebounceTime),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(searchValue => {
      this.executeSearch(searchValue);
    });
  }

  private setupLoadDataStream(): void {
    this.loadDataSubject.pipe(
      switchMap(() => {
        const { pageIndex } = this.state();
        const isInitialLoad = pageIndex === 1;
        
        if (isInitialLoad) {
          this.state.update(s => ({ ...s, loading: true }));
        }
        this.state.update(s => ({ ...s, error: false }));

        const params = this.buildHttpParams();
        const endpoint = this.getApiEndpoint();
        return this.http.get<any>(endpoint, { params }).pipe(
          tap(response => {
            this.handleDataResponse(response);
            if (isInitialLoad) {
              this.state.update(s => ({ ...s, loading: false }));
            }
            this.cdr.detectChanges();
            setTimeout(() => this.checkComponentWidth(), 100);
          }),
          catchError(err => {
            const { pageIndex } = this.state();

            if (isInitialLoad) {
              this.state.update(s => ({ ...s, loading: false }));
            }

            this.state.update(s => ({
              ...s,
              loadingMore: false,
              error: true,
              pageIndex: pageIndex > 1 ? pageIndex - 1 : pageIndex
            }));

            console.error('Error loading data:', err);

            if (pageIndex === 1) {
              this.state.update(s => ({ ...s, data: [], totalCount: 0 }));
            }

            return of({ records: [], totalCount: 0 } as ListViewData<T>);
          })
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  private loadSearchCriteriaFromUrl(): void {
    const queryParams = this.route.snapshot.queryParams;

    // Handle search criteria - automatically enable advanced search if present
    if (queryParams['searchCriteria']) {
      try {
        const criteria = JSON.parse(queryParams['searchCriteria']) as SearchCriteria[];
        if (Array.isArray(criteria) && criteria.length > 0) {
          this.state.update(s => ({
            ...s,
            searchCriteria: criteria,
            isAdvancedSearchMode: true
          }));
        } else {
          this.state.update(s => ({ ...s, isAdvancedSearchMode: false }));
          this.clearSearchCriteriaFromUrl();
        }
      } catch (error) {
        console.warn('Failed to parse search criteria from URL:', error);
        this.state.update(s => ({ ...s, isAdvancedSearchMode: false }));
        this.clearSearchCriteriaFromUrl();
      }
    } else {
      // No search criteria - default to simple search mode
      this.state.update(s => ({ ...s, isAdvancedSearchMode: false }));
    }
  }

  private syncSearchCriteriaToUrl(): void {
    const queryParams: any = { ...this.route.snapshot.queryParams };
    const { searchCriteria } = this.state();

    if (searchCriteria.length > 0) {
      queryParams.searchCriteria = JSON.stringify(searchCriteria);
      // Remove savedFilterId when using direct search criteria
      delete queryParams.savedFilterId;
      // Remove advancedSearch as it's inferred from searchCriteria presence
      delete queryParams.advancedSearch;
    } else {
      delete queryParams.searchCriteria;
      delete queryParams.advancedSearch;
    }

    this.updateUrlParams(queryParams);
  }

  private clearSearchCriteriaFromUrl(): void {
    const queryParams: any = { ...this.route.snapshot.queryParams };
    delete queryParams.searchCriteria;
    delete queryParams.advancedSearch;
    delete queryParams.savedFilterId;

    this.updateUrlParams(queryParams);
  }

  // Search methods
  onSearchInput(value: string): void {
    if (!this.config.searchConfig?.useAdvancedSearch) {
      this.searchSubject.next(value);
    }
  }

  onSearch(): void {
    const { isAdvancedSearchMode } = this.state();
    if (!isAdvancedSearchMode) {
      let searchTerm = '';
      if (typeof this.searchValue === 'string') {
        searchTerm = this.searchValue.trim();
      } else if (this.searchValue && this.searchValue.value) {
        searchTerm = this.searchValue.value.trim();
      }

      this.executeSearch(searchTerm);
    }
  }

  private executeSearch(value: string): void {
    this.state.update(s => ({
      ...s,
      searchText: value,
      pageIndex: 1,
      data: [],
      hasMoreData: true
    }));

    const searchParams = this.getSearchParams();
    this.searchChange.emit(searchParams);
    this.loadData();
  }

  onAdvancedSearch(criterion: SearchCriteria): void {
    this.state.update(s => ({
      ...s,
      searchCriteria: [...s.searchCriteria, criterion]
    }));

    this.syncSearchCriteriaToUrl();
    this.executeAdvancedSearch();
  }

  onRemoveSearchCriterion(index: number): void {
    this.state.update(s => ({
      ...s,
      searchCriteria: s.searchCriteria.filter((_, i) => i !== index)
    }));

    this.syncSearchCriteriaToUrl();
    this.executeAdvancedSearch();
  }

  executeAdvancedSearch(): void {
    this.state.update(s => ({
      ...s,
      pageIndex: 1,
      data: [],
      hasMoreData: true
    }));

    const searchParams = this.getSearchParams();
    this.searchChange.emit(searchParams);
    this.loadData();
  }

  clearSearch(): void {
    const { isAdvancedSearchMode } = this.state();

    if (isAdvancedSearchMode) {
      this.state.update(s => ({ ...s, searchCriteria: [] }));
      this.clearSearchCriteriaFromUrl();
    } else {
      this.state.update(s => ({ ...s, searchText: '' }));
      this.searchValue = '';
    }

    const searchParams = this.getSearchParams();
    this.searchChange.emit(searchParams);

    this.state.update(s => ({
      ...s,
      pageIndex: 1,
      data: [],
      hasMoreData: true,
      isAdvancedSearchMode: false
    }));
    this.loadData();
  }

  onClearAdvancedSearch(): void {
    this.state.update(s => ({
      ...s,
      searchCriteria: [],
      pageIndex: 1,
      data: [],
      hasMoreData: true,
      isAdvancedSearchMode: false
    }));

    this.clearSearchCriteriaFromUrl();

    const searchParams = this.getSearchParams();
    this.searchChange.emit(searchParams);
    this.loadData();
  }

  switchToAdvancedSearch(): void {
    this.state.update(s => ({
      ...s,
      isAdvancedSearchMode: true,
      searchText: ''
    }));
    this.searchValue = '';
  }

  switchToSimpleSearch(): void {
    this.state.update(s => ({
      ...s,
      isAdvancedSearchMode: false,
      searchCriteria: [],
      pageIndex: 1,
      data: [],
      hasMoreData: true
    }));

    this.clearSearchCriteriaFromUrl();
    this.loadData();
  }

  // Sort methods
  sortableFields(): ListViewColumn[] {
    // Use custom sortable fields if provided, otherwise use columns
    if (this.config.sortableFields && this.config.sortableFields.length > 0) {
      return this.config.sortableFields.map(field => ({
        field: field.field,
        label: field.label,
        sortable: true,
        type: 'text'
      } as ListViewColumn));
    }
    
    return this.columns().filter(col => col.sortable);
  }

  sortOptions(): Array<{ label: string, value: string }> {
    const options: Array<{ label: string, value: string }> = [];

    this.sortableFields().forEach(field => {
      options.push({
        label: `${field.label} (${this.translateService.instant('label.ascending')})`,
        value: `${field.field}:asc`
      });

      options.push({
        label: `${field.label} (${this.translateService.instant('label.descending')})`,
        value: `${field.field}:desc`
      });
    });

    return options;
  }

  onSortChange(event: any): void {
    const order = event.order === 1 ? 'asc' : 'desc';

    this.state.update(s => ({
      ...s,
      sortField: event.field,
      sortOrder: order
    }));

    this.sortChange.emit({ field: event.field, order });
    this.loadData();
  }

  onSortConfigChange(sortConfig: string): void {
    if (!sortConfig) {
      this.clearSort();
      return;
    }

    const [field, order] = sortConfig.split(':');

    this.state.update(s => ({
      ...s,
      sortField: field,
      sortOrder: order as 'asc' | 'desc'
    }));

    this.sortChange.emit({ field, order: order as 'asc' | 'desc' });
    this.loadData();
  }

  clearSort(): void {
    this.currentSortConfig = '';

    this.state.update(s => ({
      ...s,
      sortField: '',
      sortOrder: 'asc'
    }));

    this.sortChange.emit({ field: '', order: 'asc' });
    this.loadData();
  }

  // Data loading
  onRowClick(event: any): void {
    this.rowClick.emit(event);
  }

  onLoadMore(): void {
    const { hasMoreData, loadingMore, loading, data } = this.state();

    if (!hasMoreData || loadingMore || loading || !this._dataUrl || data.length === 0) {
      console.warn('LoadMore ignored: conditions not met');
      return;
    }

    try {
      this.state.update(s => ({
        ...s,
        loadingMore: true,
        error: false,
        pageIndex: s.pageIndex + 1
      }));

      this.loadData();
      this.loadMore.emit();

    } catch (error) {
      console.error('Error in onLoadMore:', error);
      this.state.update(s => ({
        ...s,
        loadingMore: false,
        error: true,
        pageIndex: s.pageIndex - 1
      }));
    }
  }

  exportData(): void {
    if (this.config.enableExport && this._dataUrl) {
      if (this.exportClick.observed) {
        this.exportClick.emit();
        return;
      }

      const entityName = this.config.entityName || 'Record';
      const { sortField, sortOrder } = this.state();

      const customTransform = this.config.exportOptions?.customTransform ||
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

      const searchParams = this.getSearchParams();

      this.exportService.exportToGoogleSheet(
        entityName,
        this.getApiEndpoint(),
        searchParams,
        sortField || this.config.defaultSortField,
        sortOrder || this.config.defaultSortOrder,
        customTransform
      ).subscribe();
    }
  }

  private getSearchParams(): SearchParams {
    const { isAdvancedSearchMode, searchCriteria, searchText } = this.state();

    if (isAdvancedSearchMode) {
      return {
        fieldSearches: searchCriteria
      };
    } else {
      return {
        generalSearch: searchText
      };
    }
  }

  private loadData(): void {
    const { loading } = this.state();

    if (!this._dataUrl || loading) {
      return;
    }

    this.loadDataSubject.next();
  }

  private buildHttpParams(): HttpParams {
    const { pageIndex, pageSize, sortField, sortOrder, isAdvancedSearchMode, searchCriteria, searchText } = this.state();

    let params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    if (sortField) {
      params = params
        .set('orderBy', sortField)
        .set('ascending', (sortOrder === 'asc').toString());
    }

    if (isAdvancedSearchMode && searchCriteria.length > 0) {
      // For advanced search, pass filters as JSON
      params = params.set('filters', JSON.stringify(searchCriteria));
    } else if (searchText?.trim()) {
      // For simple search, pass query parameter
      params = params.set('query', searchText.trim());
    }

    // Add filterActive parameter based on current filter state
    const filterActive = !this.isFilterTemporarilyDisabled();
    params = params.set('filterActive', filterActive.toString());

    // Removed automatic orgUnitId parameter addition
    // const activeOrgUnitId = this.globalFilterService.getActiveOrgUnitId();
    // if (activeOrgUnitId) {
    //   params = params.set('orgUnitId', activeOrgUnitId.toString());
    // }

    return params;
  }

  /**
   * Determines the correct API endpoint based on search type
   */
  private getApiEndpoint(): string {
    const { isAdvancedSearchMode, searchCriteria, searchText } = this.state();
    
    if (isAdvancedSearchMode && searchCriteria.length > 0) {
      // Advanced search with criteria
      return this.buildSearchUrl('advanced-search');
    } else if (searchText?.trim()) {
      // Simple text search
      return this.buildSearchUrl('search');
    } else {
      // List all (no search)
      return this._dataUrl;
    }
  }

  /**
   * Builds search URL by properly handling existing query parameters
   */
  private buildSearchUrl(searchType: 'search' | 'advanced-search'): string {
    const url = new URL(this._dataUrl, window.location.origin);
    
    // Extract the base path and add the search endpoint
    const basePath = url.pathname;
    const searchPath = `${basePath}/${searchType}`;
    
    // Preserve existing query parameters
    const searchParams = url.searchParams.toString();
    
    // Construct the final URL
    return searchParams ? `${searchPath}?${searchParams}` : searchPath;
  }

  private handleDataResponse(data: any): void {
    const { pageIndex, data: currentData } = this.state();

    let totalCount = 0;
    let newRecords: T[] = [];

    if (Array.isArray(data)) {
      totalCount = data.length;
      newRecords = data;
    } else if (data?.records && Array.isArray(data.records)) {
      totalCount = data.totalCount || data.records.length;
      newRecords = data.records;
    }

    if (pageIndex > 1 && newRecords.length === 0) {
      console.warn('Load more returned empty results, marking as no more data');
      this.state.update(s => ({ ...s, hasMoreData: false, loadingMore: false }));
      return;
    }

    let updatedData: T[];
    if (pageIndex === 1) {
      updatedData = newRecords;
    } else {
      const combinedData = [...currentData, ...newRecords];
      updatedData = this.removeDuplicateRecords(combinedData);
    }

    this.state.update(s => ({
      ...s,
      data: updatedData,
      totalCount,
      hasMoreData: updatedData.length < totalCount && newRecords.length > 0,
      loadingMore: false
    }));

    this.totalRecordsChange.emit(totalCount);

    this.cdr.detectChanges();
  }

  private removeDuplicateRecords(records: T[]): T[] {
    if (!records || records.length === 0) return records;

    const seen = new Set();
    return records.filter(record => {
      const id = record[this.idField() as keyof T];
      if (!id || seen.has(id)) {
        return false;
      }
      seen.add(id);
      return true;
    });
  }

  // Field initialization
  private initializeSearchableFields(): void {
    if (this._config.searchConfig?.searchableFields) {
      this.searchableFields = this._config.searchConfig.searchableFields.map(field => {
        const column = this.columns().find(c => c.field === field.field);
        return {
          field: field.field,
          label: field.label,
          type: column ? this.getFieldType(column) : 'string',
          operators: column ? this.getOperatorsForType(this.getFieldType(column)) : ['is', 'is not', 'like', 'not like']
        };
      });
      return;
    }

    if (this.columns() && this.columns().length > 0) {
      this.searchableFields = this.columns().map(column => ({
        field: column.field,
        label: column.label,
        type: this.getFieldType(column),
        operators: this.getOperatorsForType(this.getFieldType(column))
      }));
    }
  }

  private getFieldType(column: ListViewColumn): 'string' | 'number' | 'date' {
    switch (column.type) {
      case 'number':
      case 'currency':
        return 'number';
      case 'date':
        return 'date';
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
        return ['is', 'is not'];
    }
  }

  // Event handlers

  onMyOfficeFilterChanged(enabled: boolean): void {
    this.dataLoader.setMyOfficeFilter(enabled);
    this.executeAdvancedSearch();
  }

  onApplySavedFilter(filter: SavedFilter): void {
    if (filter.isAdvancedSearch) {
      // Apply the search criteria from the saved filter
      if (filter.searchCriteria) {
        try {
          let criteria: SearchCriteria[] = [];
          
          // Handle both string and array formats
          if (typeof filter.searchCriteria === 'string') {
            criteria = JSON.parse(filter.searchCriteria);
          } else {
            criteria = filter.searchCriteria;
          }

          // CLEAN IMPLEMENTATION: Clear and replace all criteria at once
          this.state.update(s => ({
            ...s,
            isAdvancedSearchMode: true,
            searchCriteria: [...criteria], // Replace (not append) all criteria
            searchText: '', // Clear simple search
            pageIndex: 1 // Reset to first page
          }));

          // Use the same URL structure as manual advanced search
          this.syncSearchCriteriaToUrl();
          
        } catch (error) {
          console.error('❌ Error parsing saved filter criteria:', error);
        }
      }
    } else if (filter.searchText) {
      // Apply simple search text
      this.state.update(s => ({
        ...s,
        isAdvancedSearchMode: false,
        searchText: filter.searchText || '',
        searchCriteria: [], // Clear advanced search criteria
        pageIndex: 1
      }));
      
      // Clear URL parameters for simple search
      const queryParams: any = { ...this.route.snapshot.queryParams };
      delete queryParams.searchCriteria;
      delete queryParams.advancedSearch;
      delete queryParams.savedFilterId;
      this.updateUrlParams(queryParams);
    }

    // Apply sorting if specified
    if (filter.orderBy) {
      this.state.update(s => ({
        ...s,
        sortField: filter.orderBy || '',
        sortOrder: filter.ascending ? 'asc' : 'desc'
      }));
    }

    // Reset pagination and trigger data load
    this.dataLoader.setPagination(0, this.state().pageSize);
    this.loadData();
  }

  // Getters
  get scrollHeightValue(): string | undefined {
    if (!this.config.scrollable) return undefined;
    return this.config.scrollHeight === 'flex'
      ? 'calc(100vh - 16rem)'
      : this.config.scrollHeight;
  }

  get searchCriteria(): SearchCriteria[] {
    return this.state().searchCriteria;
  }

  set searchCriteria(value: SearchCriteria[]) {
    this.state.update(s => ({ ...s, searchCriteria: value }));
  }

  private updateUrlParams(queryParams: any): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      replaceUrl: true
    }).catch(error => console.error('Navigation error:', error));
  }

  // Load global filter information
  private loadGlobalFilterInfo(): void {
    // Get current user ID
    this.authService.user().subscribe({
      next: (claims) => {
        const userIdClaim = claims.find(c => c.type === 'userId');
        if (userIdClaim) {
          this.currentUserId.set(userIdClaim.value);
          
          // Load user's global filters
          this.userPreferenceService.getGlobalFilters(userIdClaim.value)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: (filters) => {
                this.globalFilters.set(filters);
                
                // Update global filter active status
                this.isGlobalFilterActive.set(this.hasOtherActiveFilters());
                
                // Update filter labels now that we have org unit name from backend
                this.updateActiveFilterLabels();
              },
              error: (error) => {
                console.error('Error loading global filters:', error);
              }
            });
        }
      },
      error: (error) => {
        console.error('Error getting user claims:', error);
      }
    });
  }



  // Check if any global filters are active
  private hasOtherActiveFilters(): boolean {
    const filters = this.globalFilters();
    if (!filters) return false;
    
    return !!(
      filters.orgUnitId ||
      filters.relatedToMe ||
      filters.dateOn ||
      filters.dateFrom ||
      filters.dateTo
    );
  }

  // Update active filter labels for display
  private updateActiveFilterLabels(): void {
    const labels: string[] = [];
    const filters = this.globalFilters();
    
    if (filters) {
      // Add org unit filter - use orgUnitName from global filters
      if (filters.orgUnitId && filters.orgUnitName) {
        labels.push(filters.orgUnitName);
      } else if (filters.orgUnitId) {
        // Fallback to org unit ID if name is not available
        labels.push(`Org Unit ${filters.orgUnitId}`);
      }
      
      // Add related to me filter
      if (filters.relatedToMe) {
        labels.push('Related to Me');
      }
      
      // Add date filters
      if (filters.dateOn) {
        labels.push(`Date: ${new Date(filters.dateOn).toLocaleDateString()}`);
      } else if (filters.dateFrom || filters.dateTo) {
        const from = filters.dateFrom ? new Date(filters.dateFrom).toLocaleDateString() : '';
        const to = filters.dateTo ? new Date(filters.dateTo).toLocaleDateString() : '';
        if (from && to) {
          labels.push(`Date: ${from} - ${to}`);
        } else if (from) {
          labels.push(`Date: from ${from}`);
        } else if (to) {
          labels.push(`Date: until ${to}`);
        }
      }
    }
    
    this.activeFilterLabels.set(labels);
  }

  // Toggle filter functionality
  toggleGlobalFilter(): void {
    const currentlyDisabled = this.isFilterTemporarilyDisabled();
    
    if (currentlyDisabled) {
      // Re-enable filters
      this.isFilterTemporarilyDisabled.set(false);
      this.globalFilterService.setFilterEnabled(true);
    } else {
      // Temporarily disable filters
      this.isFilterTemporarilyDisabled.set(true);
      this.globalFilterService.setFilterEnabled(false);
    }
    
    // Reload data with new filter state
    this.loadData();
  }
  
  // Check if we should show filter controls
  shouldShowFilterToggle(): boolean {
    // Show toggle if there are active filters OR if filters are temporarily disabled
    return this.isGlobalFilterActive() && (this.activeFilterLabels().length > 0 || this.isFilterTemporarilyDisabled());
  }
  
  // Get display text for toggle button
  getToggleButtonText(): string {
    return this.isFilterTemporarilyDisabled() 
      ? this.translateService.instant('search.applyFilter')
      : this.translateService.instant('search.showAll');
  }
  
  // Get record count display text
  getRecordCountText(): string {
    const currentCount = this.totalRecordsCount();
    
    if (this.isFilterTemporarilyDisabled() || !this.isGlobalFilterActive()) {
      return this.translateService.instant('search.showingAllRecords', { total: currentCount });
    } else {
      // When filters are active, we show the filtered count
      // For now, we don't have the unfiltered total, so we just show current count
      return this.translateService.instant('search.showingAllRecords', { total: currentCount });
    }
  }

  // Open global filters dialog
  openGlobalFiltersDialog(): void {
    this.globalFiltersDialogService.openDialog();
  }
}
