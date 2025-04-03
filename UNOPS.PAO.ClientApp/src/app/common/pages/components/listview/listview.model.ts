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
  defaultSortField?: string;
  defaultSortOrder?: 'asc' | 'desc';
  scrollable?: boolean;
  scrollHeight?: string;
}

export interface ListViewData<T> {
  records: T[];
  totalCount: number;
}
