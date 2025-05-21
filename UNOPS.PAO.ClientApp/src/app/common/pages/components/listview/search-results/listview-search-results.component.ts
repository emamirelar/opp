import { ChangeDetectionStrategy, Component, Input, OnInit, OnDestroy, ContentChild, TemplateRef, Output, EventEmitter, inject, computed, signal, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ListViewColumn, ListViewConfig, ListViewData } from '../../listview/listview.model';
import { TableModule } from 'primeng/table';
import { TranslateModule } from '@ngx-translate/core';
import { DatePipe, DecimalPipe, CurrencyPipe } from '@angular/common';
import { ListviewTableComponent } from '../../listview/table/listview-table.component';
import { ListviewCardComponent } from '../../listview/card/listview-card.component';
import { Subscription } from 'rxjs';
import { TooltipModule } from 'primeng/tooltip';
import { ButtonModule } from 'primeng/button';
import { HttpClient, HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-listview-search-results',
  templateUrl: './listview-search-results.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    DatePipe,
    DecimalPipe,
    CurrencyPipe,
    TableModule,
    ListviewTableComponent,
    ListviewCardComponent,
    TooltipModule,
    ButtonModule
  ]
})
export class ListviewSearchResultsComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private subscription = new Subscription();
  private elRef = inject(ElementRef);
  private resizeObserver: ResizeObserver | null = null;
  private userSelectedViewMode: 'table' | 'card' | null = null;
  
  // Add signal for current component width
  private componentWidth = signal<number>(0);

  // State signals
  private loadingState = signal<boolean>(false);
  private errorState = signal<boolean>(false);
  private dataState = signal<ListViewData<any>>({ records: [], totalCount: 0 });

  // Computed signals
  isLoading = computed(() => this.loadingState());
  hasError = computed(() => this.errorState());
  currentPageData = computed(() => this.dataState().records);
  totalRecordsCount = computed(() => this.dataState().totalCount);

  // Core inputs
  @Input() set dataUrl(value: string) {
    if (value) {
      this._dataUrl = value;
      
    }
  }
  private _dataUrl: string = '';

  @Input() columns: ListViewColumn[] = [];
  
  @Input() set fullTextSearch(value: string) {
    this._fullTextSearch = value;
    if (this._dataUrl) {
      this.loadData();
    }
  }
  private _fullTextSearch: string = '';
  
  // Default configuration
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

  @Input() 
  set config(value: ListViewConfig) {
    this._config = value;
  }
  
  get config(): ListViewConfig {
    return this._config;
  }

  // View mode (table or card)
  viewMode: 'table' | 'card' = 'table';

  // Events
  @Output() rowClick = new EventEmitter<any>();
  @Output() pageChange = new EventEmitter<{first: number, rows: number}>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() totalRecordsChange = new EventEmitter<number>();
  
  // Custom template references
  @ContentChild('actionsTemplate') actionsTemplate?: TemplateRef<any>;

  // State
  selectedRecord: any | null = null;
  first = 0;
  rows: number;
  currentSortField: string | undefined;
  currentSortOrder: 'asc' | 'desc' | undefined;
  
  // Config state
  private pageIndex = 1;
  private pageSize = 20;

  // Computed property
  hasActionsTemplate = computed(() => !!this.actionsTemplate);

  constructor() {
    this.rows = this.config.pageSize || 20;
    this.pageSize = this.rows;
  }

  ngOnInit(): void {
    // if (this._dataUrl) {
    //   this.loadData();
    // }
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
  
  /**
   * Handle resize events and auto-switch view mode if necessary
   */
  private handleResize(width: number): void {
    const autoSwitchEnabled = this.config.autoSwitchToCardView !== false;
    const minWidth = this.config.autoSwitchMinWidth || 768;

    if (autoSwitchEnabled) {
      if (width < minWidth) {
        this.setViewMode('card');
      } else {
        this.setViewMode('table');
      }
    }
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
   * Used to manually check width (e.g. when container is resized)
   */
  @HostListener('window:resize')
  onWindowResize(): void {
    this.checkComponentWidth();
  }

  /**
   * Set view mode (table or card)
   */
  setViewMode(mode: 'table' | 'card'): void {
    this.viewMode = mode;
    this.userSelectedViewMode = mode;
  }

  /**
   * Handle row click events
   */
  onRowClick(event: any): void {
    this.selectedRecord = event;
    this.rowClick.emit(event);
  }

  /**
   * Handle pagination events
   */
  onPageChange(event: any): void {
    this.first = event.first;
    this.rows = event.rows;
    
    this.pageIndex = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.loadData();
    
    this.pageChange.emit(event);
  }

  /**
   * Handle sort events
   */
  onSortChange(event: any): void {
    this.currentSortField = event.field;
    this.currentSortOrder = event.order;
    this.loadData();
    
    this.sortChange.emit(event);
  }

  /**
   * Load data from the server
   */
  private loadData(): void {
    if (!this._dataUrl) {
      return;
    }

    this.loadingState.set(true);
    this.errorState.set(false);

    let params = new HttpParams()
      .set('pageIndex', this.pageIndex.toString())
      .set('pageSize', this.pageSize.toString());
      
    if (this.currentSortField) {
      params = params.set('orderBy', this.currentSortField)
                    .set('ascending', this.currentSortOrder === 'asc');
    }
    
    if (this._fullTextSearch) {
      const trimmedSearchText = this._fullTextSearch.trim();
      if (trimmedSearchText) {
        params = params.set('searchText', trimmedSearchText);
      }
    }
    
    this.subscription.add(
      this.http.get(this._dataUrl, { params }).subscribe({
        next: (data: any) => {
          this.handleDataResponse(data);
          this.loadingState.set(false);
        },
        error: (err) => {
          this.loadingState.set(false);
          this.errorState.set(true);
          console.error('Error loading data:', err);
          this.dataState.set({ records: [], totalCount: 0 });
        }
      })
    );
  }

  /**
   * Handle data response from server
   */
  private handleDataResponse(data: any): void {
    let totalCount = 0;
    
    if (Array.isArray(data)) {
      totalCount = data.length;
      this.dataState.set({ records: data, totalCount });
    } else if (data.records && Array.isArray(data.records)) {
      totalCount = data.totalCount || data.records.length;
      this.dataState.set({
        records: data.records,
        totalCount
      });
    } else {
      this.dataState.set({ records: [], totalCount: 0 });
    }
    
    // Emit the total records count
    this.totalRecordsChange.emit(totalCount);
  }
}
