import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError, shareReplay } from 'rxjs/operators';

/**
 * @fileoverview Partner search service for advanced search partner dropdowns
 * @author UNOPS Opportunity+ System Development Team
 */

/**
 * @interface PartnerSearchResult
 * @description Result interface for partner search responses
 */
export interface PartnerSearchResult {
  id: number;
  name: string;
  partnerShortDescription?: string;
  status?: string;
}

/**
 * @class PartnerSearchService
 * @description Service for searching partners with debouncing and caching for dropdown selections.
 * Provides optimized partner search functionality for advanced search filters.
 * 
 * @example
 * ```typescript
 * // Search partners
 * this.partnerSearchService.searchPartners('UNICEF').subscribe(partners => {
 *   console.log('Found partners:', partners);
 * });
 * ```
 * 
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root'
})
export class PartnerSearchService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/partner/search';
  
  // Cache for search results
  private cache = new Map<string, Observable<PartnerSearchResult[]>>();
  
  // Subject for debounced search
  private searchSubject = new Subject<string>();

  constructor() {
    // Setup debounced search
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe();
  }

  /**
   * @description Search partners by query string with caching
   * @param {string} query - Search term to find partners
   * @param {number} pageSize - Number of results to return (default: 20)
   * @returns {Observable<PartnerSearchResult[]>} Observable of partner search results
   * @example
   * ```typescript
   * this.searchPartners('World Bank', 10).subscribe(partners => {
   *   this.availablePartners.set(partners);
   * });
   * ```
   * @since 1.0.0
   */
  searchPartners(query: string, pageSize: number = 20): Observable<PartnerSearchResult[]> {
    if (!query || query.trim().length === 0) {
      return of([]);
    }

    // Check cache first
    const cacheKey = `${query}_${pageSize}`;
    if (this.cache.has(cacheKey)) {
      return this.cache.get(cacheKey)!;
    }

    // Build params
    const params = new HttpParams()
      .set('query', query.trim())
      .set('pageSize', pageSize.toString())
      .set('pageIndex', '1')
      .set('filterActive', 'true'); // Only active partners

    // Make request and cache
    const request$ = this.http.get<any>(this.apiUrl, { params }).pipe(
      switchMap(response => {
        const partners: PartnerSearchResult[] = response.records?.map((p: any) => ({
          id: p.id,
          name: p.name,
          partnerShortDescription: p.partnerShortDescription,
          status: p.status
        })) || [];
        return of(partners);
      }),
      catchError(error => {
        console.error('Error searching partners:', error);
        return of([]);
      }),
      shareReplay(1)
    );

    this.cache.set(cacheKey, request$);
    return request$;
  }

  /**
   * @description Get initial partners for dropdown (most recent active partners)
   * @param {number} pageSize - Number of partners to retrieve (default: 20)
   * @returns {Observable<PartnerSearchResult[]>} Observable of partner results
   * @example
   * ```typescript
   * this.getInitialPartners(15).subscribe(partners => {
   *   this.availablePartners.set(partners);
   * });
   * ```
   * @since 1.0.0
   */
  getInitialPartners(pageSize: number = 20): Observable<PartnerSearchResult[]> {
    const params = new HttpParams()
      .set('pageSize', pageSize.toString())
      .set('pageIndex', '1')
      .set('filterActive', 'true')
      .set('orderBy', 'lastModifiedDate')
      .set('ascending', 'false'); // Most recent first

    return this.http.get<any>('/api/partner', { params }).pipe(
      switchMap(response => {
        const partners: PartnerSearchResult[] = response.records?.map((p: any) => ({
          id: p.id,
          name: p.name,
          partnerShortDescription: p.partnerShortDescription,
          status: p.status
        })) || [];
        return of(partners);
      }),
      catchError(error => {
        console.error('Error loading initial partners:', error);
        return of([]);
      })
    );
  }

  /**
   * @description Clear the search cache
   * @returns {void}
   * @since 1.0.0
   */
  clearCache(): void {
    this.cache.clear();
  }
}

