import { Component, ContentChild, EventEmitter, HostListener, Input, OnInit, Output, TemplateRef, computed, inject, effect } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TranslateModule } from '@ngx-translate/core';
import { ListViewColumn, ListViewConfig } from './listview.model';
import { ListviewDataLoaderService } from './listview-data-loader.service';

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
    TableModule
  ],
  providers: [ListviewDataLoaderService]
})
export class ListviewComponent<T = any> {
  private dataLoader = inject(ListviewDataLoaderService);

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
      this.loadData();
    }
  }

  @Input() columns: ListViewColumn[] = [];
  @Input() config: ListViewConfig = {
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    selectionMode: 'single',
    enablePagination: true,
    enableSorting: true,
    scrollable: true,
    scrollHeight: 'flex'
  };
  @Input() idField = 'id';

  // Events
  @Output() rowSelect = new EventEmitter<T>();
  @Output() rowDblClick = new EventEmitter<T>();
  @Output() pageChange = new EventEmitter<{first: number, rows: number}>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();

  // State
  selectedRecord: T | null = null;
  first = 0;
  rows: number;

  // Computed properties from data loader
  isLoading = computed(() => this.dataLoader.isLoading());
  hasError = computed(() => this.dataLoader.hasError());
  currentPageData = computed(() => this.dataLoader.currentPageData());
  totalRecordsCount = computed(() => this.dataLoader.totalRecordsCount());
  hasActionsTemplate = computed(() => !!this.actionsTemplate);

  constructor() {
    this.rows = this.config.pageSize || 50;
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
    this.sortChange.emit({ field: event.field, order });

    this.dataLoader.setSorting(event.field, order);
    this.loadData();
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
