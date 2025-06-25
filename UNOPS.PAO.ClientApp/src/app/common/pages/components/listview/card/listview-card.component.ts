import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChild, EventEmitter, Input, OnChanges, Output, TemplateRef, computed, ElementRef, HostListener, inject, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { DatePipe, DecimalPipe, CurrencyPipe } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { AvatarModule } from 'primeng/avatar';
import { RouterModule } from '@angular/router';

import { ListViewColumn, ListViewConfig } from '../listview.model';
import { InteractionIconService } from '../../../../services/interaction-icon.service';

@Component({
  selector: 'app-listview-card',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    CardModule,
    ButtonModule,
    SkeletonModule,
    AvatarModule,
    RouterModule
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

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .animate-fadeIn {
      animation: fadeIn 0.3s ease-out;
    }
  `]
})
export class ListviewCardComponent<T = any> implements OnChanges, AfterViewInit, OnDestroy {
  // Inputs
  @Input() columns: ListViewColumn[] = [];
  @Input() config!: ListViewConfig;
  @Input() data: T[] = [];
  @Input() totalRecords: number = 0;
  @Input() loading: boolean = false;
  @Input() error: boolean = false;
  @Input() hasMoreData: boolean = true;
  @Input() isLoadingMore: boolean = false;

  // Events
  @Output() loadMore = new EventEmitter<void>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() rowSelect = new EventEmitter<T>();
  @Output() rowClick = new EventEmitter<T>();

  // Scroll detection
  private elementRef = inject(ElementRef);
  private cdr = inject(ChangeDetectorRef);
  private interactionIconService = inject(InteractionIconService);

  // Custom template references
  @ContentChild('cardActionsTemplate') actionsTemplate?: TemplateRef<any>;
  
  // ViewChild for intersection observer sentinel
  @ViewChild('loadMoreSentinel') loadMoreSentinel?: ElementRef<HTMLDivElement>;

  // Computed values
  hasActionsTemplate = computed(() => !!this.actionsTemplate);
  
  // Load more skeletons count - show a few placeholder cards
  loadMoreSkeletonsCount = computed(() => {
    // Show 2-4 skeletons based on page size, but keep it reasonable
    const pageSize = this.config?.pageSize || 20;
    return Math.min(Math.max(Math.floor(pageSize / 5), 2), 4);
  });

  // Array constructor for template access
  Array = Array;
  
  // Intersection Observer for infinite scroll
  private intersectionObserver?: IntersectionObserver;
  
  // Loading management
  private lastLoadMoreTime = 0;
  private readonly LOAD_MORE_DEBOUNCE_MS = 500; // Prevent rapid calls

  // Add computed property to check if safe to render content
  canRenderContent = computed(() => {
    const hasColumns = this.columns && this.columns.length > 0;
    const hasConfig = this.config;
    const hasData = this.data && this.data.length > 0;
    const notLoading = !this.loading;
    
    return hasColumns && hasConfig && (hasData || notLoading) && !this.error;
  });

  // Computed property to get avatar column
  avatarColumn = computed(() => {
    if (!this.columns || this.columns.length === 0) {
      return null;
    }
    return this.columns.find(col => col.type === 'avatar') || null;
  });

  // Computed property to get interaction icon column (for avatar display)
  interactionIconColumn = computed(() => {
    if (!this.columns || this.columns.length === 0) {
      return null;
    }
    return this.columns.find(col => col.type === 'interactionIcon') || null;
  });

  // Computed property to determine if we should show interaction icon in avatar position
  shouldShowInteractionAvatar = computed(() => {
    return this.interactionIconColumn() && !this.avatarColumn();
  });

  // Computed property to get ordered card fields (excluding avatar and interaction icons shown as avatar)
  orderedCardFields = computed(() => {
    if (!this.columns || this.columns.length === 0) {
      return [];
    }
    
    // Filter out avatar columns and interaction icon columns when they're shown as avatars
    return this.columns.filter(col => {
      if (col.type === 'avatar') return false;
      if (col.type === 'interactionIcon' && this.shouldShowInteractionAvatar()) return false;
      return true;
    });
  });

  // Computed property to get field 1 (position 0 - main title)
  field1 = computed(() => {
    const fields = this.orderedCardFields();
    return fields.length > 0 ? fields[0] : null;
  });

  // Computed property to get field 2 (position 1 - secondary info)
  field2 = computed(() => {
    const fields = this.orderedCardFields();
    return fields.length > 1 ? fields[1] : null;
  });

  // Computed property to get field 3 (position 2 - content)
  field3 = computed(() => {
    const fields = this.orderedCardFields();
    return fields.length > 2 ? fields[2] : null;
  });

  // Computed property to get field 4 (position 3 - top right)
  field4 = computed(() => {
    const fields = this.orderedCardFields();
    return fields.length > 3 ? fields[3] : null;
  });

  // Computed property to get field 5 (position 4 - content area)
  field5 = computed(() => {
    const fields = this.orderedCardFields();
    return fields.length > 4 ? fields[4] : null;
  });

  /**
   * Handle input changes and force change detection if needed
   */
  ngOnChanges(): void {
    // Force change detection when data or columns change
    setTimeout(() => {
      this.cdr.detectChanges();
      // Reobserve the sentinel when data changes
      this.observeLoadMoreSentinel();
    }, 0);
  }

  /**
   * AfterViewInit - Setup intersection observer
   */
  ngAfterViewInit(): void {
    this.setupIntersectionObserver();
    this.observeLoadMoreSentinel();
  }

  /**
   * OnDestroy - Cleanup intersection observer
   */
  ngOnDestroy(): void {
    if (this.intersectionObserver) {
      this.intersectionObserver.disconnect();
    }
  }

  /**
   * Handle card double click
   */
  onCardClick(item: T): void {
    this.rowClick.emit(item);
  }

  /**
   * Setup Intersection Observer for infinite scroll
   */
  private setupIntersectionObserver(): void {
    if (!('IntersectionObserver' in window)) {
      // Fallback for older browsers - keep the button
      console.warn('IntersectionObserver not supported, infinite scroll disabled');
      return;
    }

    // Create intersection observer with root margin for early triggering
    this.intersectionObserver = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          // When the sentinel becomes visible, load more data
          if (entry.isIntersecting && this.hasMoreData && !this.isLoadingMore) {
            this.onLoadMore();
          }
        });
      },
      {
        // Root margin: start loading when element is 200px away from being visible
        rootMargin: '200px',
        // Threshold: trigger when any part of the element is visible
        threshold: 0
      }
    );
  }

  /**
   * Observe the load more sentinel element
   */
  private observeLoadMoreSentinel(): void {
    if (this.intersectionObserver && this.loadMoreSentinel?.nativeElement) {
      // Unobserve previous element first
      this.intersectionObserver.disconnect();
      // Observe the new sentinel element
      this.intersectionObserver.observe(this.loadMoreSentinel.nativeElement);
    }
  }

  /**
   * Trigger load more event with improved loading management
   */
  onLoadMore(): void {
    const now = Date.now();
    
    // Multiple layers of protection
    if (!this.canLoadMore() || !this.shouldAllowLoadMore(now)) {
      return;
    }

    // Update last load time for debouncing
    this.lastLoadMoreTime = now;
    
    // Emit the load more event
    this.loadMore.emit();
  }

  /**
   * Check if we can load more data
   */
  private canLoadMore(): boolean {
    return (
      this.hasMoreData && 
      !this.isLoadingMore && 
      this.data && 
      this.data.length > 0
    );
  }

  /**
   * Check if we should allow load more based on timing
   */
  private shouldAllowLoadMore(currentTime: number): boolean {
    return (currentTime - this.lastLoadMoreTime) >= this.LOAD_MORE_DEBOUNCE_MS;
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
        return String(value);
      case 'email':
        return String(value);
      default:
        return String(value);
    }
  }

  /**
   * Get field value without formatting - with safety checks
   */
  getFieldValue(item: T, field: string): any {
    try {
      if (!item || !field) {
        return null;
      }
      return item[field as keyof T] ?? null;
    } catch (error) {
      console.warn(`Error accessing field ${field}:`, error);
      return null;
    }
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


  /**
   * Get CSS classes for field rendering based on context and column configuration
   */
  getFieldClasses(column: ListViewColumn, context: 'field1' | 'field2' | 'field3' | 'field4' | 'field5'): string {
    const baseClasses: string[] = [];

    // Add context-specific classes
    switch (context) {
      case 'field1':
        // Field 1 (main title) has its own styling in the template wrapper
        break;
      case 'field2':
        // Field 2 has its own styling in the template wrapper
        break;
      case 'field3':
        // Field 3 has its own styling in the template wrapper
        break;
      case 'field4':
        // Field 4 (content area) has its own styling in the template wrapper
        break;
      case 'field5':
        // Field 5 (top right) has its own styling in the template wrapper
        break;
    }

    // Add column-type specific classes
    switch (column.type) {
      case 'email':
      case 'link':
        baseClasses.push('text-primary', 'hover:underline', 'cursor-pointer');
        break;
      case 'template':
        // Template content can contain HTML, so minimal styling
        break;
      default:
        // Default field styling
        break;
    }

    // Add ellipsis classes if enabled
    if (column.ellipsis) {
      baseClasses.push('ellipsis-text');
    }

    return baseClasses.join(' ');
  }

  /**
   * Get router link for a column based on row data
   */
  getRouterLink(item: T, column: ListViewColumn): string | null {
    if (column.routerLink) {
      return this.replacePlaceholders(column.routerLink, item);
    }
    return null;
  }

  /**
   * Replace placeholders in a string with actual values from row data
   * Example: '/partners/{id}/details' becomes '/partners/123/details'
   */
  private replacePlaceholders(template: string, item: T): string {
    return template.replace(/\{([^}]+)\}/g, (match, fieldName) => {
      const value = this.getNestedProperty(item, fieldName.trim());
      return value !== null && value !== undefined ? String(value) : '';
    });
  }

  /**
   * Get nested property value from an object using dot notation
   */
  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((current, prop) => current?.[prop], obj);
  }

  /**
   * Get interaction icon class for interactionIcon type columns
   */
  getInteractionIcon(item: T, column: ListViewColumn): string {
    const type = this.getFieldValue(item, column.field);
    return this.interactionIconService.getInteractionIcon(String(type || ''));
  }

  /**
   * Get interaction color for interactionIcon type columns
   */
  getInteractionColor(item: T, column: ListViewColumn): string {
    const type = this.getFieldValue(item, column.field);
    return this.interactionIconService.getInteractionColor(String(type || ''));
  }

  /**
   * Get Material Design icon name for interactionIcon type columns
   */
  getInteractionMaterialIcon(item: T, column: ListViewColumn): string {
    const type = this.getFieldValue(item, column.field);
    return this.interactionIconService.getInteractionMaterialIcon(String(type || ''));
  }

  /**
   * Get Material Design filled icon name for interactionIcon type columns
   */
  getInteractionMaterialIconFilled(item: T, column: ListViewColumn): string {
    const type = this.getFieldValue(item, column.field);
    return this.interactionIconService.getInteractionMaterialIconFilled(String(type || ''));
  }

  /**
   * Get gradient background for interactionIcon type columns
   */
  getInteractionGradient(item: T, column: ListViewColumn): string {
    const type = this.getFieldValue(item, column.field);
    return this.interactionIconService.getInteractionGradient(String(type || ''));
  }

  /**
   * Get shadow color for interactionIcon type columns
   */
  getInteractionShadowColor(item: T, column: ListViewColumn): string {
    const type = this.getFieldValue(item, column.field);
    return this.interactionIconService.getInteractionShadowColor(String(type || ''));
  }
}
