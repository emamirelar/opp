import { ChangeDetectionStrategy, Component, ContentChild, EventEmitter, Input, Output, TemplateRef, computed, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { PaginatorModule } from 'primeng/paginator';
import { DatePipe, DecimalPipe, CurrencyPipe } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { AvatarModule } from 'primeng/avatar';

import { ListViewColumn, ListViewConfig } from '../listview.model';

@Component({
  selector: 'app-listview-card',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    CardModule,
    PaginatorModule,
    DatePipe,
    DecimalPipe,
    CurrencyPipe,
    ButtonModule,
    SkeletonModule,
    AvatarModule
  ],
  templateUrl: './listview-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    .ellipsis-text {
      display: block;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      width: 100%;
    }
    
    .ellipsis-text:hover {
      cursor: help;
    }
  `]
})
export class ListviewCardComponent<T = any> {
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
  @Output() rowClick = new EventEmitter<T>();
  
  // Custom template references
  @ContentChild('cardActionsTemplate') actionsTemplate?: TemplateRef<any>;
  
  // Computed values
  hasActionsTemplate = computed(() => !!this.actionsTemplate);
  
  // Computed property to get title column
  titleColumn = computed(() => {
    const titleField = this.config.cardConfig?.titleField;
    if (titleField) {
      // Find column that matches the title field
      return this.columns.find(col => col.field === titleField) || this.columns[0];
    }
    // Default to first column if no title field specified
    return this.columns.find(col => col.type === 'text') || this.columns[0];
  });

  // Computed property to get avatar column
  avatarColumn = computed(() => {
    return this.columns.find(col => col.type === 'avatar');
  });
  
  // Computed property to get content columns
  contentColumns = computed(() => {
    const titleCol = this.titleColumn();
    const contentFields = this.config.cardConfig?.contentFields;
    
    if (contentFields && contentFields.length > 0) {
      // Filter columns that match the specified content fields
      return this.columns.filter(col => contentFields.includes(col.field));
    }
    
    // Default to all columns except the title column, up to 4
    const otherColumns = this.columns.filter(col => col !== titleCol);
    return otherColumns.slice(0, 6);
  });
  
  // Get column sizes based on config or defaults
  columnSizes = computed(() => {
    const cardsPerRow = this.config.cardConfig?.cardsPerRow || {};
    return {
      xs: cardsPerRow.xs ? `col-${12 / cardsPerRow.xs}` : 'col-12',
      sm: cardsPerRow.sm ? `sm:col-${12 / cardsPerRow.sm}` : 'sm:col-6',
      md: cardsPerRow.md ? `md:col-${12 / cardsPerRow.md}` : 'md:col-6',
      lg: cardsPerRow.lg ? `lg:col-${12 / cardsPerRow.lg}` : 'lg:col-4',
      xl: cardsPerRow.xl ? `xl:col-${12 / cardsPerRow.xl}` : 'xl:col-3'
    };
  });
  
  // Combined column class
  columnClass = computed(() => {
    const sizes = this.columnSizes();
    return `${sizes.xs} ${sizes.sm} ${sizes.md} ${sizes.lg} ${sizes.xl} mb-3`;
  });
  
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
   * Handle card selection
   */
  onCardSelect(item: T): void {
    this.rowSelect.emit(item);
  }

  /**
   * Handle card double click
   */
  onCardClick(item: T): void {
    this.rowClick.emit(item);
  }

  /**
   * Handle page change event
   */
  onPageChange(event: any): void {
    this.pageChange.emit(event);
  }

  /**
   * Format field value based on column configuration
   */
  formatValue(item: T, column: ListViewColumn): string {
    const value = item[column.field as keyof T];
    
    if (value === null || value === undefined) {
      return '';
    }
    
    switch (column.type) {
      case 'date':
        if (value instanceof Date || typeof value === 'string' || typeof value === 'number') {
          return new DatePipe('en-US').transform(value, column.format || 'mediumDate') || '';
        }
        return String(value);
      case 'number':
        if (typeof value === 'number' || typeof value === 'string') {
          return new DecimalPipe('en-US').transform(value, column.format || '1.0-2') || '';
        }
        return String(value);
      case 'currency':
        if (typeof value === 'number' || typeof value === 'string') {
          return new CurrencyPipe('en-US').transform(value, 'USD', 'symbol', column.format || '1.2-2') || '';
        }
        return String(value);
      case 'avatar':
        // Pour le type avatar, on retourne simplement l'URL pour l'utiliser avec p-avatar
        return String(value);
      case 'email':
        // Pour le type email, on retourne simplement l'adresse
        return String(value);
      default:
        return String(value);
    }
  }
  
  /**
   * Get field value without formatting 
   */
  getFieldValue(item: T, field: string): any {
    return item[field as keyof T];
  }
  
  /**
   * Safely get the avatar image URL from the item
   */
  getAvatarUrl(item: T, field: string): string | undefined {
    const value = this.getFieldValue(item, field);
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
  getTemplateValue(item: T, column: ListViewColumn): string {
    if (column.templateFn) {
      return column.templateFn(item);
    }
    return this.getFieldValue(item, column.field) || '';
  }
}
