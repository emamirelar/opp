import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal, computed } from '@angular/core';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { ListViewData } from './listview.model';

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
  private pageSize = 50;
  private sortField = '';
  private sortOrder: 'asc' | 'desc' = 'asc';
  
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
    this.pageIndex = pageIndex;
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
      return of(this.dataState());
    }
    
    let params = new HttpParams()
      .set('pageIndex', this.pageIndex.toString())
      .set('pageSize', this.pageSize.toString());
      
    if (this.sortField) {
      params = params.set('orderBy', this.sortField)
                    .set('ascending', this.sortOrder === 'asc');
    }
    
    return this.http.get(this.url, { params });
  }
}
