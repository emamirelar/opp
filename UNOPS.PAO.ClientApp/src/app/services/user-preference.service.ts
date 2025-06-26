import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, of } from 'rxjs';

export interface DefaultOrgUnitRequest {
  orgUnitId: number;
}

export interface DefaultOrgUnitResponse {
  defaultOrgUnitId: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class UserPreferenceService {
  private apiUrl = '/api/user-preferences';
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
}