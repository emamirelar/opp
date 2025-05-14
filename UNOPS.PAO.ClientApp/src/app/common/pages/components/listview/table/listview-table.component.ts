import { ChangeDetectionStrategy, Component, ContentChild, EventEmitter, Input, Output, TemplateRef, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { DatePipe, DecimalPipe, CurrencyPipe } from '@angular/common';
import { AvatarModule } from 'primeng/avatar';

import { ListViewColumn, ListViewConfig } from '../listview.model';

@Component({
  selector: 'app-listview-table',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    TableModule,
    DatePipe,
    DecimalPipe,
    CurrencyPipe,
    AvatarModule
  ],
  templateUrl: './listview-table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListviewTableComponent<T = any> {
  // Inputs
  @Input() columns: ListViewColumn[] = [];
  @Input() config!: ListViewConfig;
  @Input() data: T[] = [];
  @Input() totalRecords: number = 0;
  @Input() loading: boolean = false;
  @Input() error: boolean = false;
  @Input() first: number = 0;
  @Input() rows: number = 20;
  
  // Events
  @Output() pageChange = new EventEmitter<{first: number, rows: number}>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() rowSelect = new EventEmitter<T>();
  @Output() rowDblClick = new EventEmitter<T>();
  
  // Custom template references
  @ContentChild('tableActionsTemplate') actionsTemplate?: TemplateRef<any>;
  
  // Computed values
  hasActionsTemplate = computed(() => !!this.actionsTemplate);
  
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
   * Handle row selection
   */
  onRowSelect(event: any): void {
    this.rowSelect.emit(event);
  }

  /**
   * Handle row double click
   */
  onRowDblClick(event: any): void {
    this.rowDblClick.emit(event);
  }

  /**
   * Handle page change event
   */
  onPageChange(event: any): void {
    this.pageChange.emit(event);
  }

  /**
   * Handle sort change event
   */
  onSortChange(event: any): void {
    const order = event.order === 1 ? 'asc' : 'desc';
    this.sortChange.emit({ field: event.field, order });
  }
  
  /**
   * Safely get the avatar image URL from the row data
   */
  getAvatarUrl(rowData: any, field: string): string {
    return rowData && rowData[field] ? String(rowData[field]) : '';
  }
}
