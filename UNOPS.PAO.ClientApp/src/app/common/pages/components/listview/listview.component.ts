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
  preselectedSavedFilterId: number | null = null;
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

    // Subscribe to global filter changes
    this.globalFilterService.activeOrgUnitId$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this._dataUrl) {
          this.state.update(s => ({ ...s, pageIndex: 1, data: [], hasMoreData: true }));
          this.loadData();
        }
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
        return this.http.get<any>(this._dataUrl, { params }).pipe(
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

    if (queryParams['savedFilterId']) {
      const filterId = parseInt(queryParams['savedFilterId'], 10);
      if (!isNaN(filterId)) {
        this.preselectedSavedFilterId = filterId;
        if (queryParams['advancedSearch'] === 'true') {
          this.state.update(s => ({ ...s, isAdvancedSearchMode: true }));
        }
      }
      return;
    }

    if (queryParams['advancedSearch'] === 'true' && queryParams['searchCriteria']) {
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
      this.state.update(s => ({ ...s, isAdvancedSearchMode: false }));
    }
  }

  private syncSearchCriteriaToUrl(): void {
    const queryParams: any = { ...this.route.snapshot.queryParams };
    const { searchCriteria } = this.state();

    if (searchCriteria.length > 0) {
      queryParams.searchCriteria = JSON.stringify(searchCriteria);
      queryParams.advancedSearch = 'true';
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
        this._dataUrl,
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
      params = params
        .set('advancedSearch', 'true')
        .set('searchCriteria', JSON.stringify(searchCriteria));
    } else if (searchText?.trim()) {
      params = params.set('searchText', searchText.trim());
    }

    const activeOrgUnitId = this.globalFilterService.getActiveOrgUnitId();
    if (activeOrgUnitId) {
      params = params.set('orgUnitId', activeOrgUnitId.toString());
    }

    return params;
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

    if (pageIndex > 1) {
      console.log(`Load more successful: page ${pageIndex}, loaded ${newRecords.length} new records, total: ${updatedData.length}/${totalCount}`);
    }

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
    const queryParams: any = { ...this.route.snapshot.queryParams };

    queryParams.savedFilterId = filter.id;

    if (filter.isAdvancedSearch) {
      queryParams.advancedSearch = 'true';
    }

    if (filter.orderBy) {
      this.state.update(s => ({
        ...s,
        sortField: filter.orderBy || '',
        sortOrder: filter.ascending ? 'asc' : 'desc'
      }));
    }

    this.updateUrlParams(queryParams);

    this.dataLoader.setPagination(0, this.state().pageSize);
    this.preselectedSavedFilterId = null;
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
}
