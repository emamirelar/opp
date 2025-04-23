export interface ListViewColumn {
  label: string;
  field: string;
  type: string;
  sortable: boolean;
  width?: string;
  format?: string;
  template?: string;
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
