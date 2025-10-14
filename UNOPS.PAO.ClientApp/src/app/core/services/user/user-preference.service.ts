import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, of } from 'rxjs';

export interface DefaultOrgUnitRequest {
  orgUnitId: number;
}

export interface DefaultOrgUnitResponse {
  defaultOrgUnitId: number | null;
}

export interface GlobalFilters {
  orgUnitId?: number | null;
  orgUnitName?: string | null;
  relatedToMe?: boolean;
  dateOn?: string | null;
  dateFrom?: string | null;
  dateTo?: string | null;
  preferredLanguage?: string;
  theme?: string;
  activityTimeframe?: string;
}

export interface UserPreference {
  id?: number;
  userId: number;
  globalFilterJson?: string | null;
  globalFilters?: GlobalFilters;
  additionalSettingsJson?: string | null;
  name?: string;
  status?: number;
  createdBy?: number;
  createdDate?: string;
  lastModifiedBy?: number;
  lastModifiedDate?: string;
  isDeleted?: boolean;
  deletedBy?: number;
  deletedDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserPreferenceService {
  private apiUrl = '/api/user-preferences';
  private globalApiUrl = '/api/global';
  private defaultOrgUnitSubject = new BehaviorSubject<number | null>(null);
  public defaultOrgUnit$ = this.defaultOrgUnitSubject.asObservable();

  constructor(private http: HttpClient) {}

  getDefaultOrgUnit(): Observable<DefaultOrgUnitResponse> {
    return this.http.get<DefaultOrgUnitResponse>(`${this.apiUrl}/default-org-unit`).pipe(
      tap(response => {
        this.defaultOrgUnitSubject.next(response.defaultOrgUnitId);
      }),
      catchError(error => {
        console.error('Error fetching default org unit:', error);
        return of({ defaultOrgUnitId: null });
      })
    );
  }

  setDefaultOrgUnit(orgUnitId: number): Observable<any> {
    const request: DefaultOrgUnitRequest = { orgUnitId };
    return this.http.put(`${this.apiUrl}/default-org-unit`, request).pipe(
      tap(() => {
        this.defaultOrgUnitSubject.next(orgUnitId);
      }),
      catchError(error => {
        console.error('Error setting default org unit:', error);
        throw error;
      })
    );
  }

  getCurrentDefaultOrgUnitId(): number | null {
    return this.defaultOrgUnitSubject.value;
  }

  // New Global Filters Methods
  getGlobalFilters(userId: string): Observable<GlobalFilters> {
    return this.http.get<GlobalFilters>(`${this.globalApiUrl}/filters`, {
      params: { id: userId }
    }).pipe(
      catchError(error => {
        console.error('Error fetching global filters:', error);
        return of({
          orgUnitId: null,
          relatedToMe: false,
          dateOn: null,
          dateFrom: null,
          dateTo: null,
          preferredLanguage: 'en',
          theme: 'light'
        });
      })
    );
  }

  updateGlobalFilters(userId: string, globalFilters: GlobalFilters): Observable<any> {
    return this.http.put(`${this.globalApiUrl}/filters`, globalFilters, {
      params: { id: userId }
    }).pipe(
      catchError(error => {
        console.error('Error updating global filters:', error);
        throw error;
      })
    );
  }

  resetGlobalFilters(userId: string): Observable<any> {
    return this.http.post(`${this.globalApiUrl}/filters/reset`, {}, {
      params: { id: userId }
    }).pipe(
      catchError(error => {
        console.error('Error resetting global filters:', error);
        throw error;
      })
    );
  }

  getUserPreferences(userId: string): Observable<UserPreference> {
    return this.http.get<UserPreference>(`${this.globalApiUrl}/user-preferences`, {
      params: { id: userId }
    }).pipe(
      catchError(error => {
        console.error('Error fetching user preferences:', error);
        throw error;
      })
    );
  }

  updateUserPreferences(userId: string, userPreferences: UserPreference): Observable<any> {
    return this.http.put(`${this.globalApiUrl}/user-preferences`, userPreferences, {
      params: { id: userId }
    }).pipe(
      catchError(error => {
        console.error('Error updating user preferences:', error);
        throw error;
      })
    );
  }
}
