export interface PaginationResponse<T> {
  records: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}
