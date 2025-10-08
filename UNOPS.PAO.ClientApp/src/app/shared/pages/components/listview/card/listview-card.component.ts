import { ChangeDetectionStrategy, Component, ContentChild, EventEmitter, Input, OnChanges, Output, TemplateRef, computed, ElementRef, inject, ViewChild, AfterViewInit, OnDestroy, SimpleChanges, input, signal, effect, ChangeDetectorRef, HostListener } from '@angular/core';
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

    :host ::ng-deep p-avatar img {
      object-fit: cover;
    }

    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .animate-fadeIn {
      animation: fadeIn 0.3s ease-out;
    }

    .search-highlight {
      background-color: #fef3c7;
      background-image: linear-gradient(120deg, #fef3c7 0%, #fde047 100%);
      padding: 2px 4px;
      border-radius: 3px;
      font-weight: 600;
      color: #854d0e;
      text-shadow: 0 1px 0 rgba(255, 255, 255, 0.5);
      box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
    }

    /* Responsive card content transitions */
    .card-content-transition {
      transition: all 0.2s ease-in-out;
    }

    /* Ensure proper text truncation in small cards */
    .truncate {
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    /* Responsive avatar sizing */
    :host ::ng-deep p-avatar.p-avatar-normal {
      width: 2.5rem;
      height: 2.5rem;
    }

    :host ::ng-deep p-avatar.p-avatar-large {
      width: 3.5rem;
      height: 3.5rem;
    }
  `]
})
export class ListviewCardComponent<T = any> implements OnChanges, AfterViewInit, OnDestroy {
  // Inputs
  columns = input<ListViewColumn[]>([]);
  config = input.required<ListViewConfig>();
  data = input<T[]>([]);
  totalRecords = input(0);
  loading = input(false);
  error = input(false);
  hasMoreData = input(true);
  isLoadingMore = input(false);

  // Events
  @Output() loadMore = new EventEmitter<void>();
  @Output() sortChange = new EventEmitter<{field: string, order: 'asc' | 'desc'}>();
  @Output() rowSelect = new EventEmitter<T>();
  @Output() rowClick = new EventEmitter<T>();

  // Scroll detection
  private elementRef = inject(ElementRef);
  private interactionIconService = inject(InteractionIconService);
  private cdr = inject(ChangeDetectorRef);

  // Custom template references
  @ContentChild('cardActionsTemplate') actionsTemplate?: TemplateRef<any>;

  // ViewChild for intersection observer sentinel
  @ViewChild('loadMoreSentinel') loadMoreSentinel?: ElementRef<HTMLDivElement>;
  
  // ViewChild reference for width tracking
  @ViewChild('widthTracker', { static: false }) widthTracker?: ElementRef;

  // Width tracking for responsive layout
  componentWidth = signal<number>(0);
  private widthTrackingInterval?: ReturnType<typeof setInterval>;
  private resizeObserver?: ResizeObserver;
  
  // Computed values
  hasActionsTemplate = computed(() => !!this.actionsTemplate);

  // Search metadata support
  showSearchMetadata = input<boolean>(false);
  searchMetadataEnabled = computed(() => this.config()?.searchMetadata?.enabled || false);
  searchMetadataDefaultVisible = computed(() => this.config()?.searchMetadata?.defaultVisible || false);
  searchQuery = computed(() => this.config()?.searchMetadata?.searchQuery || '');

  // Load more skeletons count - show a few placeholder cards
  loadMoreSkeletonsCount = computed(() => {
    // Show 2-4 skeletons based on page size, but keep it reasonable
    const pageSize = this.config()?.pageSize || 20;
    return Math.min(Math.max(Math.floor(pageSize / 5), 2), 4);
  });


  // Array constructor for template access
  Array = Array;

  // Intersection Observer for infinite scroll
  private intersectionObserver?: IntersectionObserver;
  private hasViewInitialized = false;
  private observeSentinelScheduled = false;

  // Effect to handle data changes
  private dataChangeEffect = effect(() => {
    // Watch for data changes and re-observe sentinel
    this.data();
    if (this.hasViewInitialized) {
      this.scheduleObserveSentinel();
    }
  });

  // Loading management
  private lastLoadMoreTime = 0;
  private readonly LOAD_MORE_DEBOUNCE_MS = 500; // Prevent rapid calls

  // Computed responsive grid classes based on component width
  responsiveGridClasses = computed(() => {
    const width = this.componentWidth();
    const forceMobileMode = this.config()?.forceMobileMode;
    
    // If forceMobileMode is enabled, always use single column
    if (forceMobileMode) {
      return 'grid grid-cols-1 gap-6';
    }
    
    // Determine number of columns based on component width
    if (width >= 1400) {
      return 'grid grid-cols-1 gap-6 xl:grid-cols-3'; // 3 columns on very wide screens
    } else if (width >= 1000) {
      return 'grid grid-cols-1 gap-6 lg:grid-cols-2'; // 2 columns on wide screens
    } else if (width >= 700) {
      return 'grid grid-cols-1 gap-6 md:grid-cols-2'; // 2 columns on medium screens
    } else {
      return 'grid grid-cols-1 gap-6'; // 1 column on small screens
    }
  });

  // Computed card size category for responsive content
  cardSize = computed(() => {
    const width = this.componentWidth();
    const forceMobileMode = this.config()?.forceMobileMode;
    
    if (forceMobileMode) {
      return 'medium'; // Changed from 'small' to ensure content shows
    }
    
    // Fallback for when width is not yet calculated (mobile initial load)
    if (width === 0 || width < 100) {
      return 'medium'; // Default to medium to ensure content shows
    }
    
    // Determine number of columns and card width based on grid layout
    let cardWidth: number;
    let columns: number;
    
    if (width >= 1400) {
      columns = 3;
      cardWidth = (width - 48) / 3; // 3 columns with gaps (24px * 2)
    } else if (width >= 1000) {
      columns = 2;
      cardWidth = (width - 24) / 2; // 2 columns with gaps (24px * 1)
    } else if (width >= 700) {
      columns = 2;
      cardWidth = (width - 24) / 2; // 2 columns with gaps (24px * 1)
    } else {
      columns = 1;
      cardWidth = width - 24; // 1 column with margins
    }
    
    // Categorize card size considering both card width and viewport context
    // Key principle: Larger viewports (more columns) should show more content per card,
    // even if individual cards are smaller. This prevents the jarring experience where
    // expanding the viewport results in less content being shown.
    
    // For single column layouts (mobile), be generous with content
    if (columns === 1) {
      if (cardWidth < 250) {
        return 'medium'; // Even very narrow mobile gets medium content
      } else if (cardWidth < 400) {
        return 'medium'; // Normal mobile - moderate content
      } else {
        return 'large'; // Wide mobile - full content
      }
    }
    
    // For 2-column layouts, be more generous since viewport is larger
    else if (columns === 2) {
      if (cardWidth < 280) {
        return 'small'; // Very narrow cards in 2-column
      } else if (cardWidth < 350) {
        return 'medium'; // Normal cards in 2-column - show avatars/tags
      } else {
        return 'large'; // Wide cards in 2-column - full content
      }
    }
    
    // For 3-column layouts, always show substantial content since it's a large viewport
    else { // columns === 3
      if (cardWidth < 280) {
        return 'medium'; // Even narrow cards in 3-column get medium treatment
      } else {
        return 'large'; // Most 3-column cards get full content
      }
    }
  });

  // Computed properties for responsive content visibility
  shouldShowAvatar = computed(() => {
    const size = this.cardSize();
    const width = this.componentWidth();
    const isMobile = width < 700;
    
    // On mobile, be more conservative with avatars to prioritize text content
    if (isMobile) {
      return size === 'large'; // Only show avatars on large mobile cards
    }
    
    return size === 'medium' || size === 'large'; // Show avatars on medium and large cards for desktop
  });

  shouldShowTags = computed(() => {
    const size = this.cardSize();
    const width = this.componentWidth();
    const isMobile = width < 700;
    
    // On mobile, be more conservative with tags to prioritize text content
    if (isMobile) {
      return size === 'large'; // Only show tags on large mobile cards
    }
    
    return size === 'medium' || size === 'large'; // Show tags on medium and large cards for desktop
  });

  shouldShowSecondaryFields = computed(() => {
    const size = this.cardSize();
    // On mobile (single column), show secondary fields even on small cards
    const width = this.componentWidth();
    const isMobile = width < 700;
    
    if (isMobile) {
      return true; // Always show secondary fields on mobile
    }
    
    return size === 'medium' || size === 'large'; // Show on medium and large cards for desktop
  });

  shouldShowDescriptionField = computed(() => {
    const size = this.cardSize();
    return size === 'large'; // Only show description on large cards
  });

  shouldShowMetadataField = computed(() => {
    const size = this.cardSize();
    // On mobile (single column), show metadata even on small cards
    const width = this.componentWidth();
    const isMobile = width < 700;
    
    if (isMobile) {
      return true; // Always show metadata on mobile
    }
    
    return size === 'medium' || size === 'large'; // Show metadata on medium and large cards for desktop
  });

  // Title should always be visible regardless of card size
  shouldShowTitle = computed(() => {
    return true; // Always show the title/entity name
  });

  // Computed CSS classes for responsive font sizes
  titleFontClasses = computed(() => {
    const size = this.cardSize();
    switch (size) {
      case 'small':
        return 'font-semibold text-base text-gray-900 leading-tight';
      case 'medium':
        return 'font-semibold text-lg text-gray-900 leading-tight';
      case 'large':
      default:
        return 'font-semibold text-xl text-gray-900 leading-tight';
    }
  });

  subtitleFontClasses = computed(() => {
    const size = this.cardSize();
    switch (size) {
      case 'small':
        return 'text-xs text-gray-600';
      case 'medium':
        return 'text-sm text-gray-600';
      case 'large':
      default:
        return 'text-sm text-gray-600';
    }
  });

  cardPaddingClasses = computed(() => {
    const size = this.cardSize();
    switch (size) {
      case 'small':
        return 'p-3';
      case 'medium':
        return 'p-4';
      case 'large':
      default:
        return 'p-5';
    }
  });

  contentGapClasses = computed(() => {
    const size = this.cardSize();
    switch (size) {
      case 'small':
        return 'gap-2';
      case 'medium':
        return 'gap-3';
      case 'large':
      default:
        return 'gap-4';
    }
  });
  
  // Add computed property to check if safe to render content
  canRenderContent = computed(() => {
    const hasColumns = this.columns() && this.columns().length > 0;
    const hasConfig = this.config();
    const hasData = this.data() && this.data().length > 0;
    const notLoading = !this.loading();

    return hasColumns && hasConfig && (hasData || notLoading) && !this.error();
  });

  // Computed property to get avatar column
  avatarColumn = computed(() => {
    const columns = this.columns();
    if (!columns || columns.length === 0) {
      return null;
    }
    return columns.find(col => col.type === 'avatar') || null;
  });

  // Computed property to get interaction icon column (for avatar display)
  interactionIconColumn = computed(() => {
    const columns = this.columns();
    if (!columns || columns.length === 0) {
      return null;
    }
    return columns.find(col => col.type === 'interactionIcon') || null;
  });

  // Computed property to determine if we should show interaction icon in avatar position
  shouldShowInteractionAvatar = computed(() => {
    const interactionIconColumn = this.interactionIconColumn();
    const avatarColumn = this.avatarColumn()
    return interactionIconColumn && !avatarColumn;
  });

  // Computed property to get ordered card fields (including all columns for content)
  orderedCardFields = computed(() => {
    const columns = this.columns();
    if (!columns || columns.length === 0) {
      return [];
    }

    // Include all columns - avatar and interaction icon columns can still provide title/content
    // We'll handle the display logic separately in the template
    return columns;
  });

  // Computed property to get all card fields at once
  cardFields = computed(() => {
    const fields = this.orderedCardFields();
    return {
      field1: fields.length > 0 ? fields[0] : null, // main title
      field2: fields.length > 1 ? fields[1] : null, // secondary info
      field3: fields.length > 2 ? fields[2] : null, // additional info
      field4: fields.length > 3 ? fields[3] : null, // content/description
      field5: fields.length > 4 ? fields[4] : null  // metadata/badge
    };
  });

  /**
   * Handle input changes
   */
  ngOnChanges(changes: SimpleChanges): void {
    // OnPush strategy will automatically detect input changes
    // No need to manually trigger change detection

    // Re-observe sentinel if columns change structure
    if (changes['columns'] && !changes['columns'].firstChange && this.hasViewInitialized) {
      this.scheduleObserveSentinel();
    }

    // Search metadata visibility is now controlled by parent component
  }

  /**
   * AfterViewInit - Setup intersection observer
   */
  ngAfterViewInit(): void {
    this.hasViewInitialized = true;
    this.setupIntersectionObserver();
    this.observeLoadMoreSentinel();
    
    // Initialize width tracking
    this.initializeWidthTracking();
    
    // Expose debug method globally for mobile testing
    if (typeof window !== 'undefined') {
      (window as any).debugCardComponent = () => this.debugCardState();
    }
  }

  /**
   * OnDestroy - Cleanup intersection observer
   */
  ngOnDestroy(): void {
    this.hasViewInitialized = false;
    if (this.intersectionObserver) {
      this.intersectionObserver.disconnect();
    }
    
    // Clean up ResizeObserver
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
    
    // Clean up polling interval
    if (this.widthTrackingInterval) {
      clearInterval(this.widthTrackingInterval);
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
          if (entry.isIntersecting && this.hasMoreData() && !this.isLoadingMore()) {
            this.onLoadMore();
          }
        });
      },
      {
        // Root margin: start loading when element is 600px away from being visible
        rootMargin: '800px',
        // Threshold: trigger when any part of the element is visible
        threshold: 0
      }
    );
  }

  /**
   * Schedule observation of the load more sentinel element
   * Uses requestAnimationFrame for optimal timing
   */
  private scheduleObserveSentinel(): void {
    if (!this.observeSentinelScheduled) {
      this.observeSentinelScheduled = true;
      requestAnimationFrame(() => {
        this.observeLoadMoreSentinel();
        this.observeSentinelScheduled = false;
      });
    }
  }

  /**
   * Observe the load more sentinel element
   */
  private observeLoadMoreSentinel(): void {
    if (this.intersectionObserver && this.loadMoreSentinel?.nativeElement) {
      // Unobserve previous element first
      this.intersectionObserver.disconnect();
      // Observe the sentinel element
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
      this.hasMoreData() &&
      !this.isLoadingMore() &&
      this.data() &&
      this.data().length > 0
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

      // Handle nested properties using dot notation (e.g., 'contact.profilePicture')
      if (field.includes('.')) {
        return this.getNestedProperty(item, field);
      }

      // Handle simple properties (case insensitive)
      return this.getCaseInsensitiveProperty(item, field) ?? null;
    } catch (error) {
      console.warn(`Error accessing field ${field}:`, error);
      return null;
    }
  }

  /**
   * Track by function for @for loops - uses id field or index as fallback
   * Arrow function to preserve 'this' context when used with cdkVirtualFor
   */
  trackByFn = (index: number, item: T): any => {
    if (!item) {
      return index;
    }

    // Try to get id field value
    const id = this.getFieldValue(item, 'id');

    // Use id if available, otherwise fall back to index
    return id !== null && id !== undefined ? id : index;
  };

  /**
   * Track by function for grouped data (2-column layout)
   * Tracks the first item's ID in each group
   */
  trackByGroup = (index: number, group: T[]): any => {
    if (!group || group.length === 0) {
      return index;
    }

    const firstItem = group[0];
    const id = this.getFieldValue(firstItem, 'id');

    return id !== null && id !== undefined ? id : index;
  };

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
   * Get first letter of Field 1 for avatar fallback
   */
  getField1Initial(item: T): string {
    const firstField = this.cardFields().field1;
    if (firstField) {
      const value = this.getFieldValue(item, firstField.field);
      if (value && typeof value === 'string' && value.trim()) {
        return value.trim().charAt(0).toUpperCase();
      }
    }
    return '?';
  }

  /**
   * Get tooltip text for field - shows field description or full value if ellipsis
   */
  getFieldTooltip(item: T, column: ListViewColumn): string {
    // If column has a description/label, use that
    if (column.label) {
      return column.label;
    }

    // If ellipsis is enabled, show the full value
    if (column.ellipsis) {
      const value = this.getFieldValue(item, column.field);
      return value ? String(value) : '';
    }

    // Otherwise, return empty string (no tooltip)
    return '';
  }

  /**
   * Get entity name from avatar field for title display
   */
  getEntityNameFromAvatarField(item: any, avatarColumn: ListViewColumn): string {
    if (!item || !avatarColumn) {
      return '';
    }

    // First, try the firstLetterFallbackField if it exists
    if (avatarColumn.firstLetterFallbackField) {
      const fallbackValue = this.getFieldValue(item, avatarColumn.firstLetterFallbackField);
      if (fallbackValue && typeof fallbackValue === 'string' && fallbackValue.trim()) {
        return fallbackValue.trim();
      }
    }

    // Try common entity name fields
    const commonNameFields = ['name', 'title', 'displayName', 'entityName', 'organizationName', 'companyName'];
    for (const fieldName of commonNameFields) {
      const value = this.getFieldValue(item, fieldName);
      if (value && typeof value === 'string' && value.trim() && !value.startsWith('http')) {
        return value.trim();
      }
    }

    // Try to extract from the avatar field itself if it's not a URL
    const avatarFieldValue = this.getFieldValue(item, avatarColumn.field);
    if (avatarFieldValue && typeof avatarFieldValue === 'string' && !avatarFieldValue.startsWith('http')) {
      return avatarFieldValue.trim();
    }

    // Fallback to first non-URL string field in the item
    const itemKeys = Object.keys(item);
    for (const key of itemKeys) {
      const value = item[key];
      if (value && typeof value === 'string' && value.trim() && !value.startsWith('http') && !key.toLowerCase().includes('url') && !key.toLowerCase().includes('image')) {
        return value.trim();
      }
    }

    return 'Unknown Entity';
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
   * Get property value from an object in a case insensitive way
   * Uses Object.getOwnPropertyNames to avoid conflicts with inherited properties like HTML 'title'
   */
  private getCaseInsensitiveProperty(obj: any, prop: string): any {
    if (!obj || typeof obj !== 'object') {
      return undefined;
    }

    // Try direct access first (case sensitive) using hasOwnProperty to check own properties only
    if (Object.prototype.hasOwnProperty.call(obj, prop)) {
      return obj[prop];
    }

    // Search case insensitive among own properties only (not inherited ones)
    const ownKeys = Object.getOwnPropertyNames(obj);
    const matchingKey = ownKeys.find(key =>
      key.toLowerCase() === prop.toLowerCase()
    );

    if (matchingKey) {
      return obj[matchingKey];
    }

    return undefined;
  }

  /**
   * Get nested property value from an object using dot notation (case insensitive)
   */
  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((current, prop) =>
      current ? this.getCaseInsensitiveProperty(current, prop) : undefined, obj);
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
   * Check if a field has a non-empty value for the given item
   */
  hasFieldValue(item: T, column: ListViewColumn | null): boolean {
    if (!column) return false;

    // For template type, check the actual template output
    if (column.type === 'template' && column.templateFn) {
      const templateValue = column.templateFn(item);
      // Check if template returns meaningful content
      if (!templateValue) return false;
      // Remove HTML tags and check if there's actual text
      const textContent = templateValue.replace(/<[^>]*>/g, '').trim();
      return textContent !== '';
    }

    // For other types, use formatValue
    const value = this.formatValue(item, column);
    if (value === null || value === undefined) return false;

    const stringValue = value.toString().trim();
    return stringValue !== '' && stringValue !== 'null' && stringValue !== 'undefined';
  }

  // Search metadata helper methods


  /**
   * Get search metadata for an item
   */
  getSearchMetadata(item: any): any {
    const extractFn = this.config()?.searchMetadata?.extractMetadata;
    return extractFn ? extractFn(item) : item._searchMetadata;
  }

  /**
   * Check if item has search metadata
   */
  hasSearchMetadata(item: any): boolean {
    const metadata = this.getSearchMetadata(item);
    return metadata && typeof metadata === 'object';
  }

  /**
   * Get search type from metadata
   */
  getSearchType(metadata: any): string {
    return metadata?.searchType || metadata?.type || '';
  }

  /**
   * Get match field from metadata
   */
  getMatchField(metadata: any): string {
    return metadata?.matchedField || metadata?.field || '';
  }

  /**
   * Get search snippet from metadata
   */
  getSearchSnippet(metadata: any): string {
    return metadata?.snippet || metadata?.excerpt || '';
  }

  /**
   * Get relevance score from metadata
   */
  getRelevanceScore(metadata: any): number {
    const score = metadata?.score || metadata?.relevance || 0;
    return Math.round(score * 100);
  }

  /**
   * Get search type badge classes
   */
  getSearchTypeBadgeClasses(metadata: any): string {
    const type = this.getSearchType(metadata);
    const baseClasses = 'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium';

    switch (type.toLowerCase()) {
      case 'exact':
        return `${baseClasses} bg-green-100 text-green-700`;
      case 'partial':
        return `${baseClasses} bg-blue-100 text-blue-700`;
      case 'fuzzy':
        return `${baseClasses} bg-yellow-100 text-yellow-700`;
      default:
        return `${baseClasses} bg-gray-100 text-gray-700`;
    }
  }

  /**
   * Get search type label
   */
  getSearchTypeLabel(metadata: any): string {
    const type = this.getSearchType(metadata);
    switch (type.toLowerCase()) {
      case 'exact': return 'Exact Match';
      case 'partial': return 'Partial Match';
      case 'fuzzy': return 'Fuzzy Match';
      default: return type || 'Match';
    }
  }

  /**
   * Highlight search terms in text
   */
  highlightSearchTerms(text: string, searchQuery: string): string {
    if (!text || !searchQuery) return text;
    
    // Escape special regex characters in search term
    const escapedSearchTerm = searchQuery.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    
    // Split search term into individual words for better highlighting
    const words = escapedSearchTerm.split(/\s+/).filter(word => word.length > 0);
    
    let highlightedText = text;
    
    // Highlight each word separately
    words.forEach(word => {
      if (word.length > 1) { // Only highlight words with 2+ characters
        const regex = new RegExp(`(${word})`, 'gi');
        highlightedText = highlightedText.replace(regex, '<span class="search-highlight">$1</span>');
      }
    });
    
    return highlightedText;
  }

  /**
   * Get tags from item in a type-safe way
   * This method handles the generic type T and checks for tags property
   */
  getItemTags(item: T): any[] | null {
    const itemAsAny = item as any;
    return itemAsAny?.tags && Array.isArray(itemAsAny.tags) ? itemAsAny.tags : null;
  }

  /**
   * Check if a field likely contains tag information that would duplicate the dedicated tags
   */
  isFieldLikelyTagField(column: ListViewColumn, item: T): boolean {
    if (!column) return false;
    
    // Get existing tags for comparison
    const existingTags = this.getItemTags(item);
    if (!existingTags || existingTags.length === 0) {
      return false; // No tags to duplicate
    }
    
    // Extract tag values for comparison
    const tagValues = existingTags.map(tag => {
      // Handle different tag object structures
      const tagValue = tag.tag || tag.name || tag.value || tag.label || tag;
      return tagValue.toString().toLowerCase().trim();
    }).filter(Boolean);
    
    if (tagValues.length === 0) {
      return false;
    }
    
    // Check if field name suggests it contains tags
    const fieldName = column.field.toLowerCase();
    const tagRelatedFields = [
      'tags', 'status', 'state', 'category', 'type', 'label', 'badge', 
      'active', 'approved', 'pending', 'approval', 'partnerstatus', 
      'partnerapprovalstatus', 'partnerstate'
    ];
    
    const isTagRelatedField = tagRelatedFields.some(tagField => fieldName.includes(tagField));
    
    
    // Get the field value
    const fieldValue = this.getFieldValue(item, column.field);
    if (!fieldValue) {
      return isTagRelatedField; // If no value but tag-related field name, assume it's a tag field
    }
    
    // Convert field value to string and normalize
    const fieldValueStr = fieldValue.toString().toLowerCase().trim();
    
    // Check if field value matches any tag value
    const valueMatchesTag = tagValues.some(tagValue => {
      // Exact match
      if (tagValue === fieldValueStr) {
        return true;
      }
      
      // Partial match (tag contains field value or vice versa)
      if (tagValue.includes(fieldValueStr) || fieldValueStr.includes(tagValue)) {
        return true;
      }
      
      return false;
    });
    
    // Check if field contains multiple tag values (like "Active • Approved" or "Active, Approved")
    const containsMultipleTags = tagValues.filter(tagValue => 
      fieldValueStr.includes(tagValue)
    ).length >= 2;
    
    // Check if field value is a combination of tag values with separators
    const commonSeparators = ['•', ',', ';', '|', ' - ', ' / '];
    const fieldContainsTagCombination = commonSeparators.some(separator => {
      if (fieldValueStr.includes(separator.toLowerCase())) {
        const parts = fieldValueStr.split(separator.toLowerCase()).map((p: string) => p.trim());
        return parts.length > 1 && parts.every((part: string) => 
          tagValues.some(tagValue => tagValue === part || tagValue.includes(part) || part.includes(tagValue))
        );
      }
      return false;
    });
    
    // Return true if it's a tag-related field OR if the value matches tags
    const result = isTagRelatedField || valueMatchesTag || containsMultipleTags || fieldContainsTagCombination;
    
    
    return result;
  }

  /**
   * Check if we should show a metadata field, considering tag duplication
   */
  shouldShowMetadataFieldWithoutDuplication(column: ListViewColumn | null, item: T): boolean {
    if (!column || !this.shouldShowMetadataField()) {
      return false;
    }
    
    // If we're showing dedicated tags, don't show fields that duplicate tag information
    if (this.shouldShowTags() && this.getItemTags(item) && this.isFieldLikelyTagField(column, item)) {
      return false;
    }
    
    return this.hasFieldValue(item, column);
  }

  /**
   * Check if we should show a secondary field, considering tag duplication
   */
  shouldShowSecondaryFieldWithoutDuplication(column: ListViewColumn | null, item: T): boolean {
    if (!column) {
      return false;
    }
    
    // If we're showing dedicated tags, don't show fields that duplicate tag information
    if (this.shouldShowTags() && this.getItemTags(item) && this.isFieldLikelyTagField(column, item)) {
      return false;
    }
    
    return this.hasFieldValue(item, column);
  }

  /**
   * Check if we should show a description field, considering tag duplication
   */
  shouldShowDescriptionFieldWithoutDuplication(column: ListViewColumn | null, item: T): boolean {
    if (!column || !this.shouldShowDescriptionField()) {
      return false;
    }
    
    // If we're showing dedicated tags, don't show fields that duplicate tag information
    if (this.shouldShowTags() && this.getItemTags(item) && this.isFieldLikelyTagField(column, item)) {
      return false;
    }
    
    return this.hasFieldValue(item, column);
  }

  /**
   * Window resize event handler
   */
  @HostListener('window:resize', ['$event'])
  onResize() {
    setTimeout(() => {
      this.updateComponentWidth();
    }, 100);
  }

  /**
   * Initialize width tracking
   */
  private initializeWidthTracking() {
    // Initial width measurement with multiple attempts
    this.attemptWidthMeasurement();

    // Use ResizeObserver for more efficient width tracking if available
    if (typeof ResizeObserver !== 'undefined' && this.widthTracker?.nativeElement) {
      this.resizeObserver = new ResizeObserver((entries) => {
        for (const entry of entries) {
          const width = entry.contentRect.width;
          if (width > 0) {
            const currentWidth = this.componentWidth();
            if (currentWidth !== width) {
              this.componentWidth.set(width);
              this.cdr.detectChanges();
            }
          }
        }
      });
      
      this.resizeObserver.observe(this.widthTracker.nativeElement);
    } else {
      // Fallback to polling for older browsers
      this.startWidthPolling();
    }
  }

  /**
   * Attempt width measurement with multiple retries
   */
  private attemptWidthMeasurement() {
    // Try multiple times with increasing delays to ensure element is rendered
    const attempts = [0, 50, 100, 250, 500];
    
    attempts.forEach((delay, index) => {
      setTimeout(() => {
        this.updateComponentWidth();
        
        // If we got a width, stop trying
        if (this.componentWidth() > 0 && index < attempts.length - 1) {
          return;
        }
      }, delay);
    });
  }

  /**
   * Start width polling as fallback
   */
  private startWidthPolling() {
    this.widthTrackingInterval = setInterval(() => {
      this.updateComponentWidth();
    }, 1000); // Check every second as fallback
  }

  /**
   * Update component width
   */
  private updateComponentWidth() {
    if (!this.widthTracker?.nativeElement) {
      return;
    }
    
    const element = this.widthTracker.nativeElement;
    const width = element.offsetWidth || element.clientWidth || 0;
    
    if (width > 0) {
      const currentWidth = this.componentWidth();
      if (currentWidth !== width) {
        this.componentWidth.set(width);
        // Trigger change detection
        this.cdr.detectChanges();
      }
    }
  }

  /**
   * Debug method to check current width and responsive grid (can be called from browser console)
   */
  debugCardState() {
    return this.getCurrentWidth();
  }

  /**
   * Debug method to check current width and responsive grid (can be called from browser console)
   */
  getCurrentWidth() {
    const width = this.componentWidth();
    let cardWidth: number;
    let columns: number;
    
    if (width >= 1400) {
      columns = 3;
      cardWidth = (width - 48) / 3;
    } else if (width >= 1000) {
      columns = 2;
      cardWidth = (width - 24) / 2;
    } else if (width >= 700) {
      columns = 2;
      cardWidth = (width - 24) / 2;
    } else {
      columns = 1;
      cardWidth = width - 24;
    }
    
    return {
      componentWidth: this.componentWidth(),
      columns: columns,
      calculatedCardWidth: cardWidth,
      cardSize: this.cardSize(),
      responsiveGridClasses: this.responsiveGridClasses(),
      forceMobileMode: this.config()?.forceMobileMode,
      shouldShowAvatar: this.shouldShowAvatar(),
      shouldShowTags: this.shouldShowTags(),
      shouldShowSecondaryFields: this.shouldShowSecondaryFields(),
      shouldShowDescriptionField: this.shouldShowDescriptionField(),
      shouldShowMetadataField: this.shouldShowMetadataField()
    };
  }
}
