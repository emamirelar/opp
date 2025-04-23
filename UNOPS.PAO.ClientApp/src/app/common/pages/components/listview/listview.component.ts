import { Component, ContentChild, EventEmitter, HostListener, Input, OnInit, Output, TemplateRef, computed, inject, effect, OnDestroy } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TranslateModule } from '@ngx-translate/core';
import { ListViewColumn, ListViewConfig } from './listview.model';
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
    ConfirmDialog
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
  @Input() config: ListViewConfig = {
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
  @Output() searchChange = new EventEmitter<string>();
  @Output() exportClick = new EventEmitter<void>();

  // State
  selectedRecord: T | null = null;
  first = 0;
  rows: number;
  searchText = '';
  currentSortField: string | undefined;
  currentSortOrder: 'asc' | 'desc' | undefined;

  // Computed properties from data loader
  isLoading = computed(() => this.dataLoader.isLoading());
  hasError = computed(() => this.dataLoader.hasError());
  currentPageData = computed(() => this.dataLoader.currentPageData());
  totalRecordsCount = computed(() => this.dataLoader.totalRecordsCount());
  hasActionsTemplate = computed(() => !!this.actionsTemplate);

  constructor() {
    this.rows = this.config.pageSize || 50;
    this.setupSearchDebounce();
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
    this.searchSubject.next(value);
  }

  /**
   * Execute the search with the given value
   */
  private executeSearch(value: string): void {
    this.searchChange.emit(value);
    this.dataLoader.setSearchText(value);
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
    const trimmedValue = this.searchText.trim();
    this.searchText = trimmedValue; // Update the input with trimmed value
    this.executeSearch(trimmedValue);
  }

  /**
   * Clear search text
   */
  clearSearch(): void {
    this.searchText = '';
    this.executeSearch('');
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
      
      this.exportService.exportToGoogleSheet(
        entityName,
        this._dataUrl,
        this.searchText,
        this.currentSortField || this.config.defaultSortField,
        this.currentSortOrder || this.config.defaultSortOrder,
        customTransform
      ).subscribe();
    }
  }

  /**
   * Helper to determine if scrolling should be enabled
   */
  get scrollHeightValue(): string | undefined {
    if (!this.config.scrollable) return undefined;
    if (!this.currentPageData()?.length) return undefined;
    return this.config.scrollHeight || 'flex';
  }

  /**
   * Load data with current parameters
   */
  private loadData(): void {
    this.dataLoader.loadData<T>();
  }
}
