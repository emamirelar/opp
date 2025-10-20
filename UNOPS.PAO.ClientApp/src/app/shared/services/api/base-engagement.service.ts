import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { BaseEngagement, BaseEngagementPartner, StageSeverity } from '../../models/base-engagement.model';

@Injectable({
  providedIn: 'root'
})
export class BaseEngagementService {
  http = inject(HttpClient);

  private baseEngagementData = signal<BaseEngagement[]>([]);
  allBaseEngagements = this.baseEngagementData.asReadonly();
  isLoading = signal(false);

  public readonly apiUrl = '/api/base-engagements';

  constructor() { }

  // Read-only operations
  getBaseEngagements(partnerId?: number): Observable<BaseEngagement[]> {
    this.isLoading.set(true);
    const url = partnerId 
      ? `${this.apiUrl}?partnerId=${partnerId}`
      : this.apiUrl;
    
    return this.http.get<BaseEngagement[]>(url).pipe(tap({
      next: (data) => {
        this.baseEngagementData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading base engagements:', err);
        this.isLoading.set(false);
      }
    }));
  }

  getBaseEngagementById(id: number): Observable<BaseEngagement> {
    this.isLoading.set(true);
    return this.http.get<BaseEngagement>(`${this.apiUrl}/${id}`).pipe(tap({
      next: (event) => {
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading base engagement:', err);
        this.isLoading.set(false);
      }
    }));
  }

  getBaseEngagementsByPartnerId(partnerId: number): Observable<BaseEngagement[]> {
    this.isLoading.set(true);
    // Using the new query string format for consistency
    return this.http.get<BaseEngagement[]>(`${this.apiUrl}?partnerId=${partnerId}`).pipe(tap({
      next: (data) => {
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading partner engagements:', err);
        this.isLoading.set(false);
      }
    }));
  }

  getEngagementPartners(engagementId: number): Observable<BaseEngagementPartner[]> {
    this.isLoading.set(true);
    return this.http.get<BaseEngagementPartner[]>(`${this.apiUrl}/${engagementId}/partners`).pipe(tap({
      next: (data) => {
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading engagement partners:', err);
        this.isLoading.set(false);
      }
    }));
  }

  // Helper methods
  getStageSeverity(stage: string): StageSeverity {
    switch (stage?.toLowerCase()) {
      case 'signed': return 'success';
      case 'implementation': return 'success';
      case 'completed': return 'info';
      case 'pipeline': return 'warn';
      case 'development': return 'warn';
      case 'cancelled': return 'danger';
      case 'on hold': return 'danger';
      default: return 'info';
    }
  }

  getPartnerTypeColor(partnerType: string): string {
    switch (partnerType?.toLowerCase()) {
      case 'lead': return '#3B82F6';
      case 'implementing': return '#10B981';
      case 'funding': return '#F59E0B';
      case 'technical': return '#8B5CF6';
      case 'government': return '#EF4444';
      default: return '#6B7280';
    }
  }

  // Cache management
  clearCache(): void {
    this.baseEngagementData.set([]);
  }

  refreshEngagements(): void {
    this.clearCache();
    this.getBaseEngagements().subscribe();
  }
}
