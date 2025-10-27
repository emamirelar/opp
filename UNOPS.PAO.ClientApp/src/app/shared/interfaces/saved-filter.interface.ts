export interface SavedFilter {
  id: number;
  name: string;
  entityType: string;
  isAdvancedSearch: boolean;
  searchCriteria?: any;
  searchText?: string;
  orderBy?: string;
  ascending: boolean;
  usageCount: number;
  lastUsedDate?: Date;
  createdDate: Date;
  modifiedDate: Date;
  createdBy: string;
  modifiedBy: string;
}

export interface CreateSavedFilterRequest {
  name: string;
  entityType: string;
  isAdvancedSearch: boolean;
  searchCriteria?: any;
  searchText?: string;
  orderBy?: string;
  ascending: boolean;
}

export interface UpdateSavedFilterRequest {
  id: number;
  name: string;
  entityType: string;
  isAdvancedSearch: boolean;
  searchCriteria?: any;
  searchText?: string;
  orderBy?: string;
  ascending: boolean;
}

export interface SavedFilterSearchRequest {
  entityType?: string;
  searchText?: string;
  pageIndex: number;
  pageSize: number;
  orderBy?: string;
  ascending?: boolean;
}

export interface SavedFilterSearchResponse {
  records: SavedFilter[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}

export interface ApplySavedFilterResponse {
  filterId: number;
  name: string;
  entityType: string;
  isAdvancedSearch: boolean;
  searchCriteria?: any;
  searchText?: string;
  orderBy?: string;
  ascending: boolean;
  pageIndex: number;
  pageSize: number;
}

export interface FilterStatistics {
  totalFilters: number;
  filtersByEntityType: { [key: string]: number };
  mostUsedFilters: {
    id: number;
    name: string;
    entityType: string;
    usageCount: number;
  }[];
} 
