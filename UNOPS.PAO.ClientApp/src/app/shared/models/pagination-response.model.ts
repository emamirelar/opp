export interface PaginationResponse<T> {
  records: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
  
  // Optional search metadata (only populated for search endpoints)
  searchMetadata?: { [entityId: number]: { [key: string]: any } };
  searchQuery?: string;
  executionTimeMs?: number;
}
