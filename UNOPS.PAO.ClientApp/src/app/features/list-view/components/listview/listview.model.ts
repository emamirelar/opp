import { SearchField } from '@shared/services/utils';

/**
 * Supported entity types for saved filters and advanced search functionality
 */
export type EntityType = 'Partner' | 'Interaction' | 'Contact' | 'PartnerCategory' | 'PartnerGroup';

export interface ListViewColumn {
  label: string;
  field: string;
  /**
   * Type of column data. This affects how the data is formatted and displayed.
   * Supported types:
   * - 'text': Display as plain text (default)
   * - 'date': Format as date using the DatePipe
   * - 'number': Format as number using the DecimalPipe
   * - 'currency': Format as currency using the CurrencyPipe
   * - 'translate': Use the translation pipe to translate the value
   * - 'avatar': Display an image URL as an avatar using p-avatar component
   * - 'email': Display as clickable email with mailto link
   * - 'url': Display as clickable URL link
   * - 'html': Render raw HTML content (sanitized)
   * - 'image': Display as an image with optional click to enlarge
   * - 'badge': Display as a colored badge/tag
   * - 'icon': Display as an icon (FontAwesome or PrimeIcons)
   * - 'multiple-avatars': Display multiple avatars from an array of objects
   * - 'template': Use a custom template function to render the column content
   * - 'link': Display as a clickable internal router link
   * - 'interactionIcon': Display interaction type with appropriate icon and styling
   */
  type: 'text' | 'date' | 'number' | 'currency' | 'translate' | 'avatar' | 'email' | 'url' | 'html' | 'image' | 'badge' | 'icon' | 'conditionalIcon' | 'multiple-avatars' | 'template' | 'link' | 'interactionIcon';
  sortable: boolean;
  width?: string;
  /**
   * Format string for the column:
   * - For 'date': Date format string (e.g., 'MM/dd/yyyy')
   * - For 'number': Decimal format (e.g., '1.2-2')
   * - For 'currency': Currency code (e.g., 'USD')
   * - For 'email': Not used
   */
  format?: 'date' | 'number' | 'currency' | 'email';
  template?: string;
  conditionFn?: (rowData: any) => boolean;
  /**
   * Whether to apply CSS ellipsis (text truncation with "...") when text overflows
   * @default false
   */
  ellipsis?: boolean;
  /**
   * Field to use as fallback for generating initials when avatar image is not available
   * Used primarily with 'multiple-avatars' type
   */
  firstLetterFallbackField?: string;
  /**
   * Custom template function for rendering column content
   * Used with 'template' type to combine multiple fields or create custom displays
   * @param rowData The row data object
   * @returns HTML string or plain text to display
   */
  templateFn?: (rowData: any) => string;
  
  /**
   * Helper text to show in column header tooltip
   * Displayed when user hovers over the help icon next to column header
   */
  helperText?: string;
  
  /**
   * Custom properties for enhanced column types
   */
  
  /** For 'url' type: Custom link text (if different from URL) */
  linkText?: string;
  
  /** For 'url' type: Whether to open in new tab */
  openInNewTab?: boolean;
  
  /** For 'image' type: Image width */
  imageWidth?: string;
  
  /** For 'image' type: Image height */
  imageHeight?: string;
  
  /** For 'image' type: Whether clicking enlarges the image */
  enlargeOnClick?: boolean;
  
  /** For 'badge' type: Badge color mapping function */
  badgeColorFn?: (value: any) => "success" | "info" | "warn" | "secondary" | "contrast" | "danger";
  
  /** For 'badge' type: Static badge color */
  badgeColor?: "success" | "info" | "warn" | "secondary" | "contrast" | "danger";
  
  /** For 'icon' type: Icon class mapping function */
  iconClassFn?: (value: any) => string;
  
  /** For 'icon' type: Static icon class */
  iconClass?: string;
  
  /** For 'icon' type: Icon color mapping function */
  iconColorFn?: (value: any) => string;
  
  /**
   * Router link pattern with placeholders for 'link' type columns
   * Example: '/partners/{id}/details'
   */
  routerLink?: string;
}

export interface ListViewConfig {
  pageSize?: number;
  pageSizeOptions?: number[];
  enableSelection?: boolean;
  enablePagination?: boolean;
  enableSorting?: boolean;
  enableSearch?: boolean;
  enableExport?: boolean;
  entityName?: string; // Used for export file naming
  defaultSortField?: string;
  defaultSortOrder?: 'asc' | 'desc';
  scrollable?: boolean;
  scrollHeight?: string;
  /**
   * Default view mode between 'table' and 'card'
   * @default 'card'
   */
  defaultViewMode?: 'table' | 'card';
  /**
   * Whether to show the view mode toggle buttons
   * @default true
   */
  showViewModeToggle?: boolean;
  /**
   * Whether to automatically switch to card view when component width is small
   * @default true
   */
  autoSwitchToCardView?: boolean;
  /**
   * Minimum width (in pixels) below which to automatically switch to card view
   * @default 768
   */
  autoSwitchMinWidth?: number;
  /**
   * Force mobile mode regardless of screen size or component width
   * When true, the component will always behave as if it's in mobile mode
   * This overrides autoSwitchToCardView and component width detection
   * @default false
   */
  forceMobileMode?: boolean;

  /**
   * Virtual scroll configuration for improved performance with large datasets
   * Uses Angular CDK Virtual Scrolling
   */
  virtualScroll?: {
    /**
     * Whether to enable virtual scrolling
     * @default true
     */
    enabled?: boolean;

    /**
     * Height of each item in pixels (used for scroll calculations)
     * Estimate the average card height including gap
     * @default 180
     */
    itemSize?: number;

    /**
     * Minimum buffer size in pixels before loading more items
     * @default 400
     */
    minBufferPx?: number;

    /**
     * Maximum buffer size in pixels
     * @default 800
     */
    maxBufferPx?: number;
  };

  /**
   * Search metadata configuration for displaying search result details
   * Used in global search and other search-enabled views
   */
  searchMetadata?: {
    /**
     * Whether to show search metadata (match details, relevance score, etc.)
     * @default false
     */
    enabled?: boolean;
    
    /**
     * Whether metadata is visible by default or requires user toggle
     * @default false
     */
    defaultVisible?: boolean;
    
    /**
     * Function to extract search metadata from a data item
     * Should return the _searchMetadata object from search results
     */
    extractMetadata?: (item: any) => any;
    
    /**
     * Current search query for highlighting in snippets
     */
    searchQuery?: string;
  };

  searchConfig?: {
    /**
     * Searchable fields to display in the advanced search dropdown
     * If not provided, a general search is performed
     */
    searchableFields?: SearchField[];
    
    /**
     * Whether to use advanced search with chips
     * Default is false (uses simple search)
     */
    useAdvancedSearch?: boolean;
    
    /**
     * Placeholder for the search input
     */
    placeholder?: string;
  };
  
  /**
   * Custom sortable fields to override the default column-based sorting
   * If provided, only these fields will be available in the sort dropdown
   */
  sortableFields?: Array<{
    field: string;
    label: string;
  }>;
  exportOptions?: {
    /**
     * Whether to show the export button (defaults to true if enableExport is true)
     */
    showButton?: boolean;
    
    /**
     * Custom label for the export button (defaults to "Export")
     */
    buttonLabel?: string;
    
    /**
     * List of field names to exclude from export
     */
    excludeFields?: string[];
    
    /**
     * Custom transformation function for export data
     * Takes an array of data objects and returns an array of objects with the format
     * that should be exported
     */
    customTransform?: (data: any[]) => Record<string, any>[];
  };
}

export interface ListViewData<T> {
  records: T[];
  totalCount: number;
}

export interface SearchCriteria {
  field: string;
  value: string;
  label: string;
  operator: string;  // The comparison operator (is, like, >, etc.)
  logicalOperator?: 'AND' | 'OR';  // The logical operator connecting this criterion with the next one
  // Support for date range filters (like "between")
  secondValue?: string;  // For "between" operator, this holds the end date
  fieldType?: 'text' | 'date' | 'number' | 'currency' | 'translate' | 'avatar' | 'email' | 'conditionalIcon' | 'multiple-avatars' | 'template' | 'interactionIcon' | 'enum';  // Field type to determine input type
}

export interface SearchParams {
  generalSearch?: string;
  fieldSearches?: SearchCriteria[];
  myOfficeOnly?: boolean;
}
