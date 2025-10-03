import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, of, debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs';

export interface UserSearchResult {
  id: number;
  email: string;
  name: string;
}

export interface UserPagedRequest {
  pageIndex?: number;
  pageSize?: number;
  searchTerm?: string;
  activeOnly?: boolean;
  selectedUserIds?: number[];
}

export interface UserPagedResponse {
  records: UserSearchResult[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserSearchService {
  private http = inject(HttpClient);
  
  // Cache for search results to avoid repeated requests
  private searchCache = new Map<string, UserSearchResult[]>();
  private cacheTimeout = 5 * 60 * 1000; // 5 minutes
  private cacheTimestamps = new Map<string, number>();
  
  isSearching = signal(false);

  /**
   * Search users with debouncing and caching
   */
  searchUsers(searchTerm: string, maxResults: number = 20, selectedUserIds?: number[]): Observable<UserSearchResult[]> {
    // Return empty array for short search terms unless we have selected users
    if ((!searchTerm || searchTerm.length < 2) && (!selectedUserIds || selectedUserIds.length === 0)) {
      return of([]);
    }

    const selectedIdsKey = selectedUserIds ? selectedUserIds.sort().join(',') : '';
    const cacheKey = `${searchTerm.toLowerCase()}_${maxResults}_${selectedIdsKey}`;
    const cachedResult = this.getCachedResult(cacheKey);
    
    if (cachedResult) {
      return of(cachedResult);
    }

    this.isSearching.set(true);
    
    // Build params object
    let params: any = { maxResults: maxResults.toString() };
    
    // Only add searchTerm if it exists and is long enough
    if (searchTerm && searchTerm.length >= 2) {
      params.searchTerm = searchTerm;
    }
    
    // Add selected user IDs if provided
    if (selectedUserIds && selectedUserIds.length > 0) {
      params.selectedUserIds = selectedUserIds;
    }
    
    return this.http.get<UserSearchResult[]>('/api/values/users/search', {
      params: params
    }).pipe(
      catchError(() => {
        this.isSearching.set(false);
        return of([]);
      }),
      switchMap(results => {
        this.setCachedResult(cacheKey, results);
        this.isSearching.set(false);
        return of(results);
      })
    );
  }

  /**
   * Get paginated users
   */
  getUsersPaged(request: UserPagedRequest): Observable<UserPagedResponse> {
    const defaultRequest: UserPagedRequest = {
      pageIndex: 0,
      pageSize: 50,
      activeOnly: true,
      ...request
    };

    return this.http.post<UserPagedResponse>('/api/values/users/paged', defaultRequest);
  }

  /**
   * Create a debounced search observable for autocomplete components
   */
  createDebouncedSearch(debounceMs: number = 300, selectedUserIds?: number[]): (searchTerm: Observable<string>) => Observable<UserSearchResult[]> {
    return (searchTerm: Observable<string>) => 
      searchTerm.pipe(
        debounceTime(debounceMs),
        distinctUntilChanged(),
        switchMap(term => this.searchUsers(term, 20, selectedUserIds))
      );
  }

  /**
   * Get a small set of initial users for dropdown (first 50 active users)
   */
  getInitialUsers(): Observable<UserSearchResult[]> {
    return this.getUsersPaged({ 
      pageIndex: 0, 
      pageSize: 50, 
      activeOnly: true 
    }).pipe(
      switchMap(response => of(response.records)),
      catchError(() => of([]))
    );
  }

  private getCachedResult(key: string): UserSearchResult[] | null {
    const cached = this.searchCache.get(key);
    const timestamp = this.cacheTimestamps.get(key);
    
    if (cached && timestamp && (Date.now() - timestamp) < this.cacheTimeout) {
      return cached;
    }
    
    // Clean up expired cache entries
    this.searchCache.delete(key);
    this.cacheTimestamps.delete(key);
    return null;
  }

  private setCachedResult(key: string, result: UserSearchResult[]): void {
    this.searchCache.set(key, result);
    this.cacheTimestamps.set(key, Date.now());
  }

  /**
   * Clear the search cache
   */
  clearCache(): void {
    this.searchCache.clear();
    this.cacheTimestamps.clear();
  }
}
