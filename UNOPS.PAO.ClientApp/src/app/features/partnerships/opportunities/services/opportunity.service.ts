/**
 * @fileoverview Service for managing Opportunity API interactions
 * @author UNOPS Opportunity+ System Development Team
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { retry } from 'rxjs';
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
  DSTRecommendationsRequest,
  RiskCreateRequest,
  Risk,
  OpportunityInsightsResponse,
  FrameworkStatusResponse,
  ExtractedDeliverableInfo,
  OpportunityStatementValidationResponse,
  RiskLookupsResponse,
  RiskCategoryHierarchyResponse,
  PreDefinedHighRiskModel,
  HighRiskAnalysisResponse,
  GoDecisionPayload,
  NoGoDecisionPayload,
  ExecutiveOption,
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
  private riskApiUrl = `/api/risk`;

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
   * Generate AI banner and thumbnail images for an opportunity
   */
  generateOpportunityImages(id: number): Observable<Opportunity> {
    return this.http.post<Opportunity>(`${this.apiUrl}/${id}/generate-images`, {});
  }

  /**
   * Generate Opportunity Statement PDF and upload to GCS.
   * Backend fetches markdown from entity and converts via AI service.
   */
  generateStatementPdf(request: {
    entityName: string;
    entityId: number;
    filename?: string;
  }): Observable<{ gcsPath?: string; error?: string; details?: string; success: boolean }> {
    return this.http.post<{ gcsPath?: string; error?: string; details?: string; success: boolean }>(
      `${this.apiUrl}/generate-statement-pdf`,
      request
    );
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
  updateOpportunity(
    id: number,
    request: Partial<Opportunity>,
  ): Observable<Opportunity> {
    return this.http.put<Opportunity>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Update Overview section of opportunity (name, description)
   */
  updateOpportunityOverview(id: number, data: { name?: string; description?: string }): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/overview`, data);
  }

  /**
   * Update WHAT section of opportunity (org unit, initiative type, delivery modality, deliverables)
   */
  updateOpportunityWhat(
    id: number,
    data: Partial<Opportunity>,
  ): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/what`, data);
  }

  /**
   * Update WHO section of opportunity (funding partners, client partners, external stakeholders)
   */
  updateOpportunityWho(id: number, data: any): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/who`, data);
  }

  /**
   * Update Team section of opportunity (org unit, initiative type, internal stakeholders)
   */
  updateOpportunityTeam(id: number, data: any): Observable<Opportunity> {
    return this.http.patch<Opportunity>(`${this.apiUrl}/${id}/team`, data);
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
    return this.http.patch<Opportunity>(
      `${this.apiUrl}/${id}/apply-ai-changes`,
      changes,
    );
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
    clientPartnerIds: number[],
  ): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/${opportunityId}/tag-related-partner-to-doc`,
      {
        documentId: documentId,
        fundingPartnerIds: fundingPartnerIds,
        clientPartnerIds: clientPartnerIds,
      },
    );
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
        entityId: entityId.toString(),
      },
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
  getSimilarProjects(
    id: number,
    maxResults: number = 10,
    invalidateCache: boolean = false,
  ): Observable<SimilarProjectsResponse> {
    return this.http
      .get<SimilarProjectsResponse>(`${this.apiUrl}/${id}/similar-projects`, {
        params: {
          maxResults: maxResults.toString(),
          invalidateCache: invalidateCache.toString(),
        },
      })
      .pipe(retry({ count: 2, delay: 2000 }));
  }

  /**
   * Get similar opportunities using semantic search
   */
  getSimilarOpportunities(
    id: number,
    maxResults: number = 6,
  ): Observable<SimilarOpportunitiesResponse> {
    return this.http
      .get<SimilarOpportunitiesResponse>(
        `${this.apiUrl}/${id}/similar-opportunities`,
        {
          params: {
            maxResults: maxResults.toString(),
          },
        },
      )
      .pipe(retry({ count: 2, delay: 2000 }));
  }

  /**
   * Get relevant people from corporate directory for an opportunity using AI-powered semantic search
   * @param id - Opportunity ID
   * @param maxResults - Maximum number of relevant people to return (default: 10)
   * @returns Observable with relevant people response
   */
  getRelevantPeople(
    id: number,
    maxResults: number = 10,
    invalidateCache: boolean = false,
  ): Observable<RelevantPeopleResponse> {
    return this.http
      .get<RelevantPeopleResponse>(`${this.apiUrl}/${id}/relevant-people`, {
        params: {
          maxResults: maxResults.toString(),
          invalidateCache: invalidateCache.toString(),
        },
      })
      .pipe(retry({ count: 2, delay: 2000 }));
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
   * Uses POST to pass dismissed recommendation IDs for filtering
   * @param id - Opportunity ID
   * @param dismissedOupQuestionIds - List of oupQuestionIds that user has dismissed
   * @param forceRefresh - If true, bypasses cache to get fresh recommendations
   * @returns Observable with recommendations response
   */
  getDSTRecommendations(
    id: number,
    dismissedOupQuestionIds: number[] = [],
    forceRefresh: boolean = false
  ): Observable<DSTRecommendationsResponse> {
    const request: DSTRecommendationsRequest = {
      dismissedOupQuestionIds,
    };
    const url = forceRefresh
      ? `${this.apiUrl}/${id}/dst-recommendations?forceRefresh=true`
      : `${this.apiUrl}/${id}/dst-recommendations`;
    return this.http
      .post<DSTRecommendationsResponse>(url, request)
      .pipe(retry({ count: 2, delay: 2000 }));
  }

  /**
   * Get AI-generated insights and suggestions for an opportunity
   * @param id - Opportunity ID
   * @param forceRefresh - When true, bypasses AI cache for fresh Gemini response (e.g. after section save)
   * @returns Observable with insights and suggestions
   */
  getInsights(id: number, forceRefresh = false): Observable<OpportunityInsightsResponse> {
    const url = `${this.apiUrl}/${id}/insights`;
    return forceRefresh
      ? this.http.get<OpportunityInsightsResponse>(url, { params: { forceRefresh: 'true' } })
      : this.http.get<OpportunityInsightsResponse>(url);
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
  updateDSTRisk(
    id: number,
    riskId: number,
    request: RiskCreateRequest,
  ): Observable<Risk> {
    return this.http.put<Risk>(
      `${this.apiUrl}/${id}/dst-risks/${riskId}`,
      request,
    );
  }

  /**
   * Delete a risk from the risk register
   * @param id - Opportunity ID
   * @param riskId - Risk ID to delete
   * @returns Observable with void
   */
  deleteDSTRisk(id: number, riskId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/dst-risks/${riskId}`);
  }

  /**
   * Update the high risk acknowledgement status for an opportunity
   * AC1: User must acknowledge they've reviewed all applicable organizational high risks
   * @param id - Opportunity ID
   * @param acknowledged - Whether the user has acknowledged the high risks
   * @returns Observable with acknowledgement response
   */
  acknowledgeHighRisks(id: number, acknowledged: boolean): Observable<{ acknowledged: boolean }> {
    return this.http.put<{ acknowledged: boolean }>(`${this.apiUrl}/${id}/acknowledge-high-risks`, acknowledged);
  }

  /**
   * Get all risk lookup data (types, probabilities, proximities, impact levels, response types)
   * @returns Observable with all lookup data for risk forms
   */
  getRiskLookups(): Observable<RiskLookupsResponse> {
    return this.http.get<RiskLookupsResponse>(`${this.riskApiUrl}/lookups`);
  }

  /**
   * Get risk categories in hierarchical format (3 levels)
   * @returns Observable with category hierarchy
   */
  getRiskCategories(): Observable<RiskCategoryHierarchyResponse> {
    return this.http.get<RiskCategoryHierarchyResponse>(
      `${this.riskApiUrl}/categories`,
    );
  }

  /**
   * Get all predefined high risks (EAC checklist items)
   * @returns Observable with list of high risk checklist items
   */
  getHighRiskChecklist(): Observable<PreDefinedHighRiskModel[]> {
    return this.http.get<PreDefinedHighRiskModel[]>(
      `${this.riskApiUrl}/high-risk-checklist`,
    );
  }

  /**
   * Get high risk analysis for an opportunity with auto-detected recommendations
   * @param id - Opportunity ID
   * @returns Observable with high risk analysis response
   */
  getHighRiskAnalysis(id: number): Observable<HighRiskAnalysisResponse> {
    return this.http.get<HighRiskAnalysisResponse>(
      `${this.apiUrl}/${id}/high-risk-analysis`,
    );
  }

  /**
   * Get Partner Results Framework status for an opportunity
   * Checks if Partner Results Framework documents are tagged to funding/client partners
   * @param id - Opportunity ID
   * @returns Observable with framework status information
   */
  getFrameworkStatus(id: number): Observable<FrameworkStatusResponse> {
    return this.http.get<FrameworkStatusResponse>(`${this.apiUrl}/${id}/framework-status`);
  }

  /**
   * Trigger AI extraction of products and services from documents
   * Extracts deliverables from documents, prioritizing tagged Partner Results Framework documents
   * @param id - Opportunity ID
   * @returns Observable with array of extracted deliverable information
   */
  extractProductsAndServices(id: number): Observable<ExtractedDeliverableInfo[]> {
    return this.http.post<ExtractedDeliverableInfo[]>(`${this.apiUrl}/${id}/extract-deliverables`, {});
  }

  /**
   * Generate AI-powered opportunity statement in markdown format
   * @param id - Opportunity ID
   * @returns Observable with generated statement markdown
   */
  generateOpportunityStatement(
    id: number,
  ): Observable<{
    opportunityId: number;
    statementMarkdown: string;
    message: string;
  }> {
    return this.http.post<{
      opportunityId: number;
      statementMarkdown: string;
      message: string;
    }>(`${this.apiUrl}/${id}/generate-statement`, {});
  }

  /**
   * Validate whether the opportunity statement is aligned with the structured data
   * @param id - Opportunity ID
   * @returns Observable with validation result including alignment status and misalignment items
   */
  validateOpportunityStatement(
    id: number,
  ): Observable<OpportunityStatementValidationResponse> {
    return this.http.post<OpportunityStatementValidationResponse>(
      `${this.apiUrl}/${id}/validate-statement`,
      {},
    );
  }

  /**
   * Get available collaborator expertise types for selection
   * @returns Observable with array of expertise options
   */
  getCollaboratorExpertises(): Observable<
    { id: number; name: string; code: string; description: string | null; displayOrder: number }[]
  > {
    return this.http.get<
      { id: number; name: string; code: string; description: string | null; displayOrder: number }[]
    >(`${this.apiUrl}/collaborator-expertises`);
  }

  // ===== Go/No-Go Decision Methods =====

  /**
   * Get executives (Director/Manager/OiC) for an opportunity's responsible org unit.
   * Used to populate the Executive dropdown in the Go Decision approval dialog.
   * @param opportunityId - The opportunity ID
   * @returns Observable with list of executives with display label and user ID
   */
  getExecutivesForOpportunity(opportunityId: number): Observable<ExecutiveOption[]> {
    return this.http.get<ExecutiveOption[]>(`${this.apiUrl}/${opportunityId}/executives`);
  }

  /**
   * Approve an opportunity with Go decision.
   * Assigns an Executive and records the decision rationale.
   * @param entityId - The opportunity ID
   * @param payload - Go decision payload with rationale, executiveId, and confirmation
   * @returns Observable with workflow action response
   */
  approveOpportunity(entityId: number, payload: GoDecisionPayload): Observable<unknown> {
    const requestBody = {
      entityName: 'Opportunity',
      entityId: entityId,
      rationale: payload.rationale,
      executiveId: payload.executiveId,
      confirmationAcknowledged: payload.confirmationAcknowledged,
    };
    return this.http.post('/api/workflow/approve', requestBody);
  }

  /**
   * Reject an opportunity with No-Go decision.
   * Records the decision rationale.
   * @param entityId - The opportunity ID
   * @param payload - No-Go decision payload with rationale and confirmation
   * @returns Observable with workflow action response
   */
  rejectOpportunity(entityId: number, payload: NoGoDecisionPayload): Observable<unknown> {
    const requestBody = {
      entityName: 'Opportunity',
      entityId: entityId,
      rationale: payload.rationale,
      confirmationAcknowledged: payload.confirmationAcknowledged,
    };
    return this.http.post('/api/workflow/reject', requestBody);
  }
}
