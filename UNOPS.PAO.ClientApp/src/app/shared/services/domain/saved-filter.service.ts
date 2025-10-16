import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  SavedFilter,
  CreateSavedFilterRequest,
  UpdateSavedFilterRequest,
  SavedFilterSearchRequest,
  SavedFilterSearchResponse,
  ApplySavedFilterResponse,
  FilterStatistics
} from '../../interfaces/saved-filter.interface';

@Injectable({
  providedIn: 'root'
})
export class SavedFilterService {
  private http = inject(HttpClient);
  private readonly baseUrl = '/api/savedfilter';
  
  isLoading = signal(false);

  constructor() {}

  /**
   * Create a new saved filter
   */
  createSavedFilter(request: CreateSavedFilterRequest): Observable<SavedFilter> {
    this.isLoading.set(true);
    return this.http.post<SavedFilter>(this.baseUrl, request).pipe(
      tap({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      })
    );
  }

  /**
   * Update an existing saved filter
   */
  updateSavedFilter(request: UpdateSavedFilterRequest): Observable<SavedFilter> {
    this.isLoading.set(true);
    return this.http.put<SavedFilter>(this.baseUrl, request).pipe(
      tap({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      })
    );
  }

  /**
   * Delete a saved filter
   */
  deleteSavedFilter(id: number): Observable<void> {
    this.isLoading.set(true);
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      })
    );
  }

  /**
   * Get a specific saved filter by ID
   */
  getSavedFilter(id: number): Observable<SavedFilter> {
    this.isLoading.set(true);
    return this.http.get<SavedFilter>(`${this.baseUrl}/${id}`).pipe(
      tap({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      })
    );
  }

  /**
   * Get saved filters with optional filtering and pagination
   */
  getSavedFilters(request: SavedFilterSearchRequest): Observable<SavedFilterSearchResponse> {
    this.isLoading.set(true);
    
    let params = new HttpParams()
      .set('pageIndex', request.pageIndex.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.entityType) {
      params = params.set('entityType', request.entityType);
    }
    if (request.searchText) {
      params = params.set('searchText', request.searchText);
    }
    if (request.orderBy) {
      params = params.set('orderBy', request.orderBy);
    }
    if (request.ascending !== undefined) {
      params = params.set('ascending', request.ascending.toString());
    }

    return this.http.get<SavedFilterSearchResponse>(this.baseUrl, { params }).pipe(
      tap({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      })
    );
  }

  /**
   * Apply a saved filter and get the configured filter request object
   */
  applySavedFilter(id: number, pageIndex: number = 1, pageSize: number = 10): Observable<ApplySavedFilterResponse> {
    this.isLoading.set(true);
    
    const params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApplySavedFilterResponse>(`${this.baseUrl}/${id}/apply`, { params }).pipe(
      tap({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      })
    );
  }

  /**
   * Get usage statistics for saved filters
   */
  getFilterStatistics(entityType?: string): Observable<FilterStatistics> {
    this.isLoading.set(true);
    
    let params = new HttpParams();
    if (entityType) {
      params = params.set('entityType', entityType);
    }

    return this.http.get<FilterStatistics>(`${this.baseUrl}/statistics`, { params }).pipe(
      tap({
        next: () => this.isLoading.set(false),
        error: () => this.isLoading.set(false)
      })
    );
  }

  /**
   * Helper method to get saved filters for a specific entity type
   */
  getSavedFiltersForEntity(entityType: string, pageIndex: number = 1, pageSize: number = 50): Observable<SavedFilterSearchResponse> {
    return this.getSavedFilters({
      entityType,
      pageIndex,
      pageSize
    });
  }

  /**
   * Helper method to get most used filters
   */
  getMostUsedFilters(entityType?: string): Observable<FilterStatistics> {
    return this.getFilterStatistics(entityType);
  }
} 
