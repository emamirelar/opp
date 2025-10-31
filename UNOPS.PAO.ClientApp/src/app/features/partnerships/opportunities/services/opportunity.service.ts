/**
 * @fileoverview Service for managing Opportunity API interactions
 * @author UNOPS Opportunity+ System Development Team
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Opportunity,
  OpportunityRequest,
  UpdateOpportunityRequest,
} from '@shared/models/opportunity.model';

/**
 * @class OpportunityService
 * @description Service for comprehensive Opportunity CRUD operations and data manipulation
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root',
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
   * Get a single opportunity by ID with all related data
   */
  getOpportunityById(id: number): Observable<Opportunity> {
    return this.http.get<Opportunity>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create a new opportunity
   */
  createOpportunity(request: OpportunityRequest): Observable<Opportunity> {
    return this.http.post<Opportunity>(this.apiUrl, request);
  }

  /**
   * Update an existing opportunity
   */
  updateOpportunity(id: number, request: Partial<Opportunity>): Observable<Opportunity> {
    return this.http.put<Opportunity>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Update WHAT section of opportunity (description, org unit, initiative type, deliverables)
   */
  updateOpportunityWhat(id: number, data: Partial<Opportunity>): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/what`, data);
  }

  /**
   * Update WHO section of opportunity (partners, stakeholders)
   */
  updateOpportunityWho(id: number, data: any): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/who`, data);
  }

  /**
   * Update WHERE section of opportunity (implementation countries)
   */
  updateOpportunityWhere(id: number, data: any): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/where`, data);
  }

  /**
   * Update WHY section of opportunity (SDGs, alignment, outcomes)
   */
  updateOpportunityWhy(id: number, data: any): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/why`, data);
  }

  /**
   * Update WHEN section of opportunity (dates, milestones)
   */
  updateOpportunityWhen(id: number, data: Partial<Opportunity>): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/when`, data);
  }

  /**
   * Delete an opportunity by ID
   */
  deleteOpportunityById(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Format currency value to USD format
   */
  formatCurrency(value: number | null | undefined): string {
    if (value == null) return '$0';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  }

  /**
   * Format date string to readable format
   */
  formatDate(dateString: string | null | undefined): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  /**
   * Get severity class for risk scores
   */
  getSeverityClass(riskScore: number | null | undefined): string {
    if (riskScore == null) return 'info';
    if (riskScore >= 7) return 'danger';
    if (riskScore >= 4) return 'warning';
    return 'info';
  }

  /**
   * Calculate total funding from funding partners
   */
  calculateTotalFunding(opportunity: Opportunity): number {
    if (!opportunity.stats) {
      return opportunity.fundingPartners.reduce((sum, partner) => {
        return sum + (partner.amount || 0);
      }, 0);
    }
    return opportunity.stats.totalFundingUSD;
  }

  /**
   * Calculate total fees from funding partners
   */
  calculateTotalFees(opportunity: Opportunity): number {
    if (!opportunity.stats) {
      return opportunity.fundingPartners.reduce((sum, partner) => {
        return sum + (partner.feeAmountUSD || 0);
      }, 0);
    }
    return opportunity.stats.totalFeeAmountUSD;
  }
}

