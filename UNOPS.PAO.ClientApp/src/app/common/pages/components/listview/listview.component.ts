import { Component, ContentChild, ElementRef, EventEmitter, HostListener, Input, OnInit, Output, TemplateRef, ViewChild, AfterViewInit, computed, inject, effect, OnDestroy, signal, Signal } from '@angular/core';
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
import { ListviewAdvencedSearchComponent } from './advenced-search/listview-advenced-search.component';
import { ListviewTableComponent } from './table/listview-table.component';
import { ListviewCardComponent } from './card/listview-card.component';
import { TooltipModule } from 'primeng/tooltip';
import { SearchField, SearchCriterion } from '../../../services/search-parser.service';
import { AdvancedSearchComponent } from '../../../components/advanced-search/advanced-search.component';

@Component({
  selector: 'app-listview',
  templateUrl: './listview.component.html',
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
    OverlayPanelModule,
    ListviewAdvencedSearchComponent,
    ListviewTableComponent,
    ListviewCardComponent,
    TooltipModule,
    AdvancedSearchComponent
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
      console.log('Forcing signal refresh, isAdvancedSearch:', this.isAdvancedSearch());
      
      // Re-initialize searchable fields when config changes
      this.initializeSearchableFields();
    }, 0);
    
    // Initialize advanced search if enabled
    if (value.searchConfig?.useAdvancedSearch) {
      console.log('Setting advanced search enabled in data loader');
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
  
  // Computed properties
  isAdvancedSearch = computed(() => {
    console.log('Computing isAdvancedSearch, config:', this.config);
    console.log('searchConfig exists:', !!this.config.searchConfig);
    console.log('useAdvancedSearch value:', this.config.searchConfig?.useAdvancedSearch);
    
    return !!this.config.searchConfig?.useAdvancedSearch;
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
    
    // Add debugging for initialization
    console.log('ListviewComponent constructor');
    console.log('Initial config:', this.config);
    console.log('Is advanced search?', this.isAdvancedSearch());
  }

  ngAfterViewInit(): void {
    // Setup resize observer to detect component width changes
    this.setupResizeObserver();
    
    // Check initial component width
    this.checkComponentWidth();
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
    
    // Execute search with the updated criteria
    this.executeAdvancedSearch();
  }
  
  /**
   * Remove a search criterion
   */
  onRemoveSearchCriterion(index: number): void {
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
    if (!this.config.searchConfig?.useAdvancedSearch) {
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
    console.log('Initializing searchable fields...');
    console.log('Current config:', this._config);
    console.log('Current columns:', this.columns);
    
    // First try to get searchable fields from config
    if (this._config.searchConfig?.searchableFields) {
      this.searchableFields = this._config.searchConfig.searchableFields.map(field => {
        const column = this.columns.find(c => c.field === field.field);
        const result = {
          field: field.field,
          label: field.label,
          type: column ? this.getFieldType(column) : 'string',
          operators: column ? this.getOperatorsForType(this.getFieldType(column)) : ['is', 'is not', 'like', 'not like']
        };
        console.log(`Mapped field ${field.field}:`, result);
        return result;
      });
      console.log('Initialized searchable fields from config:', this.searchableFields);
      return;
    }

    // Fallback to using columns if no searchable fields in config
    this.searchableFields = this.columns.map(column => {
      const result = {
        field: column.field,
        label: column.label,
        type: this.getFieldType(column),
        operators: this.getOperatorsForType(this.getFieldType(column))
      };
      console.log(`Mapped column ${column.field}:`, result);
      return result;
    });
    console.log('Initialized searchable fields from columns:', this.searchableFields);
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
        return ['is', 'is not', '>', '<', '>=', '<='];
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
}
