import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal, computed } from '@angular/core';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { ListViewData, SearchCriteria, SearchParams } from './listview.model';

@Injectable({
  providedIn: 'root'
})
export class ListviewDataLoaderService {
  private http = inject(HttpClient);
  
  // State signals
  private loadingState = signal<boolean>(false);
  private errorState = signal<boolean>(false);
  private dataState = signal<ListViewData<any>>({ records: [], totalCount: 0 });
  
  // Computed signals
  isLoading = computed(() => this.loadingState());
  hasError = computed(() => this.errorState());
  currentPageData = computed(() => this.dataState().records);
  totalRecordsCount = computed(() => this.dataState().totalCount);
  
  // Config state
  private url = '';
  private pageIndex = 1;
  private pageSize = 20;
  private sortField = '';
  private sortOrder: 'asc' | 'desc' = 'asc';
  
  // Search state
  private searchText = '';
  private searchCriteria: SearchCriteria[] = [];
  private useAdvancedSearch = false;
  
  /**
   * Set the API endpoint URL
   */
  setUrl(url: string): void {
    this.url = url;
  }
  
  /**
   * Set direct data
   */
  setData(data: any[]): void {
    this.url = '';
    this.dataState.set({ records: data, totalCount: data.length });
  }
  
  /**
   * Set pagination parameters
   */
  setPagination(pageIndex: number, pageSize: number): void {
    this.pageIndex = pageIndex + 1;
    this.pageSize = pageSize;
  }
  
  /**
   * Set sorting parameters
   */
  setSorting(field: string, order: 'asc' | 'desc'): void {
    this.sortField = field;
    this.sortOrder = order;
  }
  
  /**
   * Set search text (simple search)
   */
  setSearchText(text: string): void {
    this.searchText = text;
    // Clear advanced search criteria when using simple search
    if (!this.useAdvancedSearch) {
      this.searchCriteria = [];
    }
  }
  
  /**
   * Configure advanced search
   */
  setAdvancedSearchEnabled(enabled: boolean): void {
    this.useAdvancedSearch = enabled;
  }
  
  /**
   * Set search criteria for advanced search
   */
  setSearchCriteria(criteria: SearchCriteria[]): void {
    this.searchCriteria = criteria;
    // If using advanced search, clear simple search text
    if (this.useAdvancedSearch) {
      this.searchText = '';
    }
  }
  
  /**
   * Add a single search criterion
   */
  addSearchCriterion(criterion: SearchCriteria): void {
    this.searchCriteria = [...this.searchCriteria, criterion];
  }
  
  /**
   * Remove a search criterion by field
   */
  removeSearchCriterion(field: string): void {
    this.searchCriteria = this.searchCriteria.filter(c => c.field !== field);
  }
  
  /**
   * Clear all search criteria
   */
  clearSearchCriteria(): void {
    this.searchCriteria = [];
    this.searchText = '';
  }
  
  /**
   * Get current search criteria
   */
  getSearchParams(): SearchParams {
    if (this.useAdvancedSearch) {
      return {
        fieldSearches: this.searchCriteria
      };
    } else {
      return {
        generalSearch: this.searchText
      };
    }
  }
  
  /**
   * Load data with current parameters
   */
  loadData<T>(): void {
    this.loadServerSideData<T>();
  }
  
  /**
   * Load data from server
   */
  private loadServerSideData<T>(): void {
    this.loadingState.set(true);
    this.errorState.set(false);
    
    this.fetchData<T>().pipe(
      tap(data => {
        this.handleDataResponse(data);
        this.loadingState.set(false);
      }),
      catchError(err => {
        this.loadingState.set(false);
        this.errorState.set(true);
        console.error('Error loading data:', err);
        this.dataState.set({ records: [], totalCount: 0 });
        return of({ records: [], totalCount: 0 } as ListViewData<T>);
      })
    ).subscribe();
  }
  
  /**
   * Handle data response from server
   */
  private handleDataResponse(data: any): void {
    if (Array.isArray(data)) {
      this.dataState.set({ records: data, totalCount: data.length });
    } else if (data.records && Array.isArray(data.records)) {
      this.dataState.set({
        records: data.records,
        totalCount: data.totalCount || data.records.length
      });
    } else {
      this.dataState.set({ records: [], totalCount: 0 });
    }
  }
  
  /**
   * Fetch data from the API
   */
  private fetchData<T>(): Observable<any> {
    if (!this.url) {
      console.log('No URL set, returning current data state');
      return of(this.dataState());
    }
    
    let params = new HttpParams()
      .set('pageIndex', this.pageIndex.toString())
      .set('pageSize', this.pageSize.toString());
      
    if (this.sortField) {
      params = params.set('orderBy', this.sortField)
                    .set('ascending', this.sortOrder === 'asc');
    }
    
    // Handle search parameters based on mode
    if (this.useAdvancedSearch && this.searchCriteria.length > 0) {
      // For advanced search, we serialize field searches as JSON
      console.log('Using advanced search with criteria:', this.searchCriteria);
      params = params.set('advancedSearch', 'true');
      params = params.set('searchCriteria', JSON.stringify(this.searchCriteria));
    } else if (this.searchText) {
      // For simple search, set both SearchText and FirstName/LastName
      console.log('Using simple search with text:', this.searchText);
      const trimmedSearchText = this.searchText.trim();
      if (trimmedSearchText) {
        // Set the general search text
        params = params.set('SearchText', trimmedSearchText);
        
        // Also set FirstName and LastName to support field-specific search
        params = params.set('FirstName', trimmedSearchText)
                      .set('LastName', trimmedSearchText);
        
        console.log('Set search parameters:', params.toString());
      }
    }
    
    console.log('Making request to:', this.url);
    console.log('With parameters:', params.toString());
    
    return this.http.get(this.url, { params }).pipe(
      tap(response => console.log('Received response:', response))
    );
  }
}
