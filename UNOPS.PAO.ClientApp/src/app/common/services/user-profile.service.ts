import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap, catchError, shareReplay, map } from 'rxjs/operators';
import { of } from 'rxjs';

export interface UserProfile {
  userInfoWithOrgSettings?: {
    orgUnit?: string;
    userId?: number;
    firstName?: string;
    lastName?: string;
    userEmail?: string;
    dutyStation?: string;
    position?: string;
    supervisorId?: number;
    [key: string]: any;
  };
  roles?: string[];
  isPartnerGlobalAdmin?: boolean;
  canManageOffice?: boolean;
  userPreferences?: any;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root'
})
export class UserProfileService {
  private http = inject(HttpClient);
  
  // Cache the current user profile data
  private currentUserProfile = signal<UserProfile | null>(null);
  private isLoading = signal<boolean>(false);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  
  // Observable for loading state
  public isLoading$ = this.loadingSubject.asObservable();
  
  /**
   * Get current user profile data with caching
   * @param email Optional email parameter (if not provided, uses current authenticated user)
   * @returns Observable of user profile data
   */
  getCurrentUserProfile(email?: string): Observable<UserProfile> {
    // Build API URL with optional email parameter
    const apiUrl = email ? `/api/user-info/current?email=${encodeURIComponent(email)}` : '/api/user-info/current';
    
    this.loadingSubject.next(true);
    this.isLoading.set(true);
    
    return this.http.get<UserProfile>(apiUrl).pipe(
      tap(profile => {
        // Cache the profile data
        this.currentUserProfile.set(profile);
        this.isLoading.set(false);
        this.loadingSubject.next(false);
      }),
      catchError(error => {
        console.error('Failed to load user profile:', error);
        this.isLoading.set(false);
        this.loadingSubject.next(false);
        // Return empty profile object on error
        return of({} as UserProfile);
      }),
      shareReplay(1) // Cache the result for subsequent subscribers
    );
  }
  
  /**
   * Get the current user's organization unit code
   * @returns Observable of organization unit code or null
   */
  getCurrentUserOrgUnit(): Observable<string | null> {
    return this.getCurrentUserProfile().pipe(
      map(profile => {
        const orgUnit = profile?.userInfoWithOrgSettings?.orgUnit;
        return orgUnit || null;
      }),
      catchError(() => of(null))
    );
  }
  
  /**
   * Get cached user profile (signal-based, synchronous)
   */
  getCachedUserProfile() {
    return this.currentUserProfile.asReadonly();
  }
  
  /**
   * Clear cached user profile data
   */
  clearCache(): void {
    this.currentUserProfile.set(null);
  }
  
  /**
   * Check if profile data is currently loading
   */
  getLoadingState() {
    return this.isLoading.asReadonly();
  }
}
