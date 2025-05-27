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
  styles: [`
    .ellipsis-cell {
      max-width: 0;
      overflow: hidden;
    }
    
    .ellipsis-text {
      display: block;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      width: 100%;
    }
  `]
})
export class ListviewTableComponent<T = any> {
  // Inputs
  @Input() columns: ListViewColumn[] = [];
  @Input() config!: ListViewConfig;
  @Input() data: T[] = [];
  @Input() totalRecords: number = 0;
  @Input() loading: boolean = false;
  @Input() error: boolean = false;
  @Input() first: number = 1;
  @Input() rows: number = 20;
  
  // Events
  @Output() pageChange = new EventEmitter<{first: number, rows: number}>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() rowClick = new EventEmitter<T>();
  
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
  onRowClick(event: any): void {
    this.rowClick.emit(event);
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
  getAvatarUrl(rowData: any, field: string): string | undefined {
    const value = rowData?.[field];
    if (!value || typeof value !== 'string' || value.trim() === '') {
      return undefined;
    }
    return value.trim();
  }
  
  /**
   * Get array of items for multiple avatars display
   */
  getMultipleAvatarItems(rowData: any, column: ListViewColumn): any[] {
    const fieldParts = column.field.split('.');
    let value = rowData;
    
    // Navigate to the nested property (e.g., first5ContactsByDate)
    for (let i = 0; i < fieldParts.length - 1; i++) {
      value = value?.[fieldParts[i]];
    }
    
    return Array.isArray(value) ? value : [];
  }
  
  /**
   * Get initials for avatar when no image is available
   */
  getAvatarInitials(item: any, fallbackFieldPath: string | undefined): string {
    if (!fallbackFieldPath || !item) {
      return '?';
    }
    
    // Extract the actual field name from the path (e.g., 'firstName' from 'first5ContactsByDate.firstName')
    const fieldName = fallbackFieldPath.split('.').pop();
    if (!fieldName || !item[fieldName]) {
      return '?';
    }
    
    const name = String(item[fieldName]).trim();
    return name.charAt(0).toUpperCase();
  }
  
  /**
   * Get title for avatar hover tooltip
   */
  getAvatarTitle(item: any, fallbackFieldPath: string | undefined): string {
    if (!fallbackFieldPath || !item) {
      return '';
    }
    
    // Try to create a full name from firstName and lastName if available
    const firstName = item.firstName || '';
    const lastName = item.lastName || '';
    
    if (firstName && lastName) {
      return `${firstName} ${lastName}`;
    } else if (firstName) {
      return firstName;
    } else {
      // Extract the actual field name from the path
      const fieldName = fallbackFieldPath.split('.').pop();
      if (fieldName && item[fieldName]) {
        return String(item[fieldName]);
      }
    }
    
    return '';
  }
  
  /**
   * Get template value using the templateFn function
   */
  getTemplateValue(rowData: any, column: ListViewColumn): string {
    if (column.templateFn) {
      return column.templateFn(rowData);
    }
    return rowData[column.field] || '';
  }
}
