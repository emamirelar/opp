import { ChangeDetectionStrategy, Component, ContentChild, EventEmitter, Input, Output, TemplateRef, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { DatePipe, DecimalPipe, CurrencyPipe } from '@angular/common';
import { AvatarModule } from 'primeng/avatar';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

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
    AvatarModule,
    TagModule,
    TooltipModule
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

  /**
   * Get badge color for badge type columns
   */
  getBadgeColor(rowData: any, column: ListViewColumn): "success" | "info" | "warn" | "secondary" | "contrast" | "danger" | undefined {
    if (column.badgeColorFn) {
      return column.badgeColorFn(rowData[column.field]) as "success" | "info" | "warn" | "secondary" | "contrast" | "danger" | undefined;
    }
    if (column.badgeColor) {
      return column.badgeColor as "success" | "info" | "warn" | "secondary" | "contrast" | "danger" | undefined;
    }
    
    // Default color mapping for common values
    const value = String(rowData[column.field]).toLowerCase();
    if (value.includes('active') || value.includes('success') || value.includes('approved')) {
      return 'success';
    } else if (value.includes('inactive') || value.includes('disabled') || value.includes('rejected')) {
      return 'danger';
    } else if (value.includes('pending') || value.includes('draft')) {
      return 'warn';
    } else {
      return 'info';
    }
  }

  /**
   * Get icon class for icon type columns
   */
  getIconClass(rowData: any, column: ListViewColumn): string {
    if (column.iconClassFn) {
      return column.iconClassFn(rowData[column.field]);
    }
    if (column.iconClass) {
      return column.iconClass;
    }
    
    // Default icon mapping for common values
    const value = String(rowData[column.field]).toLowerCase();
    if (value.includes('active') || value.includes('success') || value.includes('approved')) {
      return 'pi pi-check-circle';
    } else if (value.includes('inactive') || value.includes('disabled') || value.includes('rejected')) {
      return 'pi pi-times-circle';
    } else if (value.includes('pending') || value.includes('draft')) {
      return 'pi pi-clock';
    } else {
      return 'pi pi-info-circle';
    }
  }

  /**
   * Get icon color for icon type columns
   */
  getIconColor(rowData: any, column: ListViewColumn): string {
    if (column.iconColorFn) {
      return column.iconColorFn(rowData[column.field]);
    }
    
    // Default color mapping for common values
    const value = String(rowData[column.field]).toLowerCase();
    if (value.includes('active') || value.includes('success') || value.includes('approved')) {
      return '#22c55e'; // green
    } else if (value.includes('inactive') || value.includes('disabled') || value.includes('rejected')) {
      return '#ef4444'; // red
    } else if (value.includes('pending') || value.includes('draft')) {
      return '#f59e0b'; // amber
    } else {
      return '#6b7280'; // gray
    }
  }

  /**
   * Enlarge image when clicked (opens in a simple modal/dialog)
   */
  enlargeImage(imageUrl: string, event: Event): void {
    event.stopPropagation();
    
    // Create a simple modal overlay
    const overlay = document.createElement('div');
    overlay.style.cssText = `
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background: rgba(0, 0, 0, 0.8);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 10000;
      cursor: pointer;
    `;
    
    const img = document.createElement('img');
    img.src = imageUrl;
    img.style.cssText = `
      max-width: 90%;
      max-height: 90%;
      object-fit: contain;
      border-radius: 8px;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
    `;
    
    overlay.appendChild(img);
    document.body.appendChild(overlay);
    
    // Close on click
    overlay.addEventListener('click', () => {
      document.body.removeChild(overlay);
    });
    
    // Close on Escape key
    const handleKeydown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        document.body.removeChild(overlay);
        document.removeEventListener('keydown', handleKeydown);
      }
    };
    document.addEventListener('keydown', handleKeydown);
  }
}
