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
  RelatedItems,
  SimilarProjectsResponse,
  SimilarOpportunitiesResponse,
  RelevantPeopleResponse,
  DSTRisksResponse,
  DSTRecommendationsResponse,
  RiskCreateRequest,
  Risk,
  OpportunityInsightsResponse,
  FrameworkStatusResponse,
  ExtractedDeliverableInfo
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
   * Get related items for opportunity (contacts, partners, interactions)
   */
  getRelatedItems(id: number): Observable<RelatedItems> {
    return this.http.get<RelatedItems>(`${this.apiUrl}/${id}/related`);
  }

  /**
   * Get source interactions for opportunity from OpportunityInteractions table
   */
  getSourceInteractions(id: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/source-interactions`);
  }

  /**
   * Update WHY section of opportunity (SDGs, alignment, outcomes)
   */
  updateOpportunityWhy(id: number, data: any): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/why`, data);
  }

  /**
   * Update WHEN section of opportunity (timeline dates)
   */
  updateOpportunityWhen(id: number, data: any): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/when`, data);
  }

  /**
   * Apply AI-extracted changes to an opportunity across multiple sections
   * @param id - Opportunity ID
   * @param changes - Object containing the fields to update
   * @returns Observable with updated opportunity
   */
  applyAiChanges(id: number, changes: any): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/apply-ai-changes`, changes);
  }

  /**
   * Tag a document as Partner Results Framework for specific funding/client partners
   * @param opportunityId - Opportunity ID
   * @param documentId - Document ID to tag
   * @param fundingPartnerIds - Array of funding partner IDs
   * @param clientPartnerIds - Array of client partner IDs
   * @returns Observable with success message
   */
  tagDocumentToPartners(
    opportunityId: number,
    documentId: number,
    fundingPartnerIds: number[],
    clientPartnerIds: number[]
  ): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${opportunityId}/tag-related-partner-to-doc`, {
      documentId: documentId,
      fundingPartnerIds: fundingPartnerIds,
      clientPartnerIds: clientPartnerIds
    });
  }

  /**
   * Delete an opportunity by ID
   */
  deleteOpportunityById(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get the latest audit log for an opportunity
   * @param entityType - Type of entity (e.g., 'Opportunity')
   * @param entityId - ID of the entity
   * @returns Observable with audit log containing JSON data
   */
  getLatestAuditLog(entityType: string, entityId: number): Observable<any> {
    return this.http.get<any>(`/api/auditlog/latest`, {
      params: {
        entityType: entityType,
        entityId: entityId.toString()
      }
    });
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

  /**
   * Get similar projects for an opportunity using AI-powered semantic search
   * @param id - Opportunity ID
   * @param maxResults - Maximum number of similar projects to return (default: 10)
   * @returns Observable with similar projects response
   */
  getSimilarProjects(id: number, maxResults: number = 10): Observable<SimilarProjectsResponse> {
    return this.http.get<SimilarProjectsResponse>(`${this.apiUrl}/${id}/similar-projects`, {
      params: {
        maxResults: maxResults.toString()
      }
    });
  }

  /**
   * Get similar opportunities using semantic search
   */
  getSimilarOpportunities(id: number, maxResults: number = 6): Observable<SimilarOpportunitiesResponse> {
    return this.http.get<SimilarOpportunitiesResponse>(`${this.apiUrl}/${id}/similar-opportunities`, {
      params: {
        maxResults: maxResults.toString()
      }
    });
  }

  /**
   * Get relevant people from corporate directory for an opportunity using AI-powered semantic search
   * @param id - Opportunity ID
   * @param maxResults - Maximum number of relevant people to return (default: 10)
   * @returns Observable with relevant people response
   */
  getRelevantPeople(id: number, maxResults: number = 10): Observable<RelevantPeopleResponse> {
    return this.http.get<RelevantPeopleResponse>(`${this.apiUrl}/${id}/relevant-people`, {
      params: {
        maxResults: maxResults.toString()
      }
    });
  }

  /**
   * Get existing risks from the risk register for an opportunity
   * @param id - Opportunity ID
   * @returns Observable with risks response
   */
  getDSTRisks(id: number): Observable<DSTRisksResponse> {
    return this.http.get<DSTRisksResponse>(`${this.apiUrl}/${id}/dst-risks`);
  }

  /**
   * Get AI-generated risk recommendations for an opportunity
   * @param id - Opportunity ID
   * @returns Observable with recommendations response
   */
  getDSTRecommendations(id: number): Observable<DSTRecommendationsResponse> {
    return this.http.get<DSTRecommendationsResponse>(`${this.apiUrl}/${id}/dst-recommendations`);
  }

  /**
   * Get AI-generated insights and suggestions for an opportunity
   * @param id - Opportunity ID
   * @returns Observable with insights and suggestions
   */
  getInsights(id: number): Observable<OpportunityInsightsResponse> {
    return this.http.get<OpportunityInsightsResponse>(`${this.apiUrl}/${id}/insights`);
  }

  /**
   * Add a new risk to the risk register
   * @param id - Opportunity ID
   * @param request - Risk creation request
   * @returns Observable with created risk
   */
  addDSTRisk(id: number, request: RiskCreateRequest): Observable<Risk> {
    return this.http.post<Risk>(`${this.apiUrl}/${id}/dst-risks`, request);
  }

  /**
   * Update an existing risk in the risk register
   * @param id - Opportunity ID
   * @param riskId - Risk ID to update
   * @param request - Risk update request
   * @returns Observable with updated risk
   */
  updateDSTRisk(id: number, riskId: number, request: RiskCreateRequest): Observable<Risk> {
    return this.http.put<Risk>(`${this.apiUrl}/${id}/dst-risks/${riskId}`, request);
  }

  /**
   * Get Partner Results Framework status for an opportunity
   * Checks if Partner Results Framework documents are tagged to funding/client partners
   * @param id - Opportunity ID
   * @returns Observable with framework status information
   */
  getFrameworkStatus(id: number): Observable<FrameworkStatusResponse> {
    return this.http.get<FrameworkStatusResponse>(`${this.apiUrl}/${id}/what/framework-status`);
  }

  /**
   * Trigger AI extraction of products and services from documents
   * Extracts deliverables from documents, prioritizing tagged Partner Results Framework documents
   * @param id - Opportunity ID
   * @returns Observable with array of extracted deliverable information
   */
  extractProductsAndServices(id: number): Observable<ExtractedDeliverableInfo[]> {
    return this.http.post<ExtractedDeliverableInfo[]>(`${this.apiUrl}/${id}/what/extract-products-services`, {});
  }

  /**
   * Generate AI-powered opportunity statement in markdown format
   * @param id - Opportunity ID
   * @returns Observable with generated statement markdown
   */
  generateOpportunityStatement(id: number): Observable<{ opportunityId: number; statementMarkdown: string; message: string }> {
    return this.http.post<{ opportunityId: number; statementMarkdown: string; message: string }>(`${this.apiUrl}/${id}/generate-statement`, {});
  }
}

