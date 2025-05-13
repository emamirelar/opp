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
   */
  type: 'text' | 'date' | 'number' | 'currency' | 'translate' | 'avatar' | 'email' | 'conditionalIcon';
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
}

export interface ListViewConfig {
  pageSize?: number;
  pageSizeOptions?: number[];
  enableSelection?: boolean;
  selectionMode?: 'single' | 'multiple';
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
   * @default 'table'
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
   * Configuration for card view display
   */
  cardConfig?: {
    /**
     * Field to use as the card title (defaults to first column)
     */
    titleField?: string;
    /**
     * Fields to display in card content (defaults to first 4 columns after title)
     */
    contentFields?: string[];
    /**
     * Number of cards per row on different screen sizes
     */
    cardsPerRow?: {
      xs?: number; // Extra small screens
      sm?: number; // Small screens
      md?: number; // Medium screens
      lg?: number; // Large screens
      xl?: number; // Extra large screens
    };
  };
  searchConfig?: {
    /**
     * Searchable fields to display in the advanced search dropdown
     * If not provided, a general search is performed
     */
    searchableFields?: Array<{
      field: string;
      label: string;
      placeholder?: string;
    }>;
    
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
  operator?: 'AND' | 'OR';  // Optional operator field defaulting to AND if not specified
}

export interface SearchParams {
  generalSearch?: string;
  fieldSearches?: SearchCriteria[];
}
