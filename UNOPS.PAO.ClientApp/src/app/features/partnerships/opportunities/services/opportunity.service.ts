import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Opportunity } from '../models/opportunity.model';

@Injectable({
  providedIn: 'root'
})
export class OpportunityService {
  private http = inject(HttpClient);
  private apiUrl = `/api/opportunity`;

  /**
   * Get the base URL for opportunities API (used by listview component)
   */
  getUrl(): string {
    return this.apiUrl;
  }

  /**
   * Get a single opportunity by ID
   */
  getOpportunityById(id: number): Observable<Opportunity> {
    return this.http.get<Opportunity>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create a new opportunity
   */
  createOpportunity(opportunity: Opportunity): Observable<Opportunity> {
    return this.http.post<Opportunity>(this.apiUrl, opportunity);
  }

  /**
   * Update an existing opportunity
   */
  updateOpportunity(opportunity: Opportunity): Observable<Opportunity> {
    return this.http.put<Opportunity>(this.apiUrl, opportunity);
  }

  /**
   * Delete an opportunity by ID
   */
  deleteOpportunityById(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

