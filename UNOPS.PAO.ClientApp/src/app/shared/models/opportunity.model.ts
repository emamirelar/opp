/**
 * @fileoverview TypeScript models for Opportunity entity and related child entities
 * @author UNOPS Opportunity+ System Development Team
 */

/**
 * Main Opportunity model matching backend OpportunityModel.cs
 */
export interface Opportunity {
  id: number;
  name: string;
  description: string | null;
  partnerReference: string | null;
  status: string;
  workflowStageId: number | null;
  workflowStageName: string | null;
  responsibleOrgUnitId: number | null;
  responsibleOrgUnitName: string | null;
  proposedInitiativeTypeId: number | null;
  proposedInitiativeTypeName: string | null;
  initiativeBudgetUSD: number | null;
  partnershipAgreementReference: string | null;
  targetSigningDate: string | null;
  targetDeliveryDate: string | null;
  strategicAlignment: string | null;
  resultsFocus: string | null;
  intendedImpactOutcomes: string | null;
  expectedBeneficiaries: string | null;
  fundingPartners: OpportunityFundingPartner[];
  clientPartners: OpportunityClientPartner[];
  stakeholders: OpportunityStakeholder[];
  deliverables: OpportunityDeliverable[];
  countries: OpportunityCountry[];
  sdGs: OpportunitySDG[];
  stats: OpportunityStats | null;
  dstAnalysis: DSTAnalysis | null; // DST Insights & Recommendations
  insights: OpportunityInsight[]; // Analysis insights
  suggestions: OpportunitySuggestion[]; // Analysis suggestions
  createdDate: string;
  lastModifiedDate: string;
  createdBy: number;
  createdByName: string | null;
  lastModifiedBy: number;
  lastModifiedByName: string | null;
}

/**
 * Funding Partner model
 */
export interface OpportunityFundingPartner {
  id: number;
  opportunityId: number;
  partnerId: number;
  partnerName: string;
  partnerLogoUrl?: string;
  amount: number | null;
  currencyId: number | null; // Nullable - backend will use default if not provided
  currencyCode: string;
  percentage: number | null;
  feePercentage: number | null;
  feeAmount: number | null;
  feeAmountUSD: number | null;
  isAmountBasedFee: boolean;
  partnershipAgreementReference: string | null;
  commitmentStatus: string | null;
}

/**
 * Client Partner model
 */
export interface OpportunityClientPartner {
  id: number;
  opportunityId: number;
  partnerId: number;
  partnerName: string;
  partnerLogoUrl?: string;
}

/**
 * Stakeholder model (Internal UNOPS users only)
 */
export interface OpportunityStakeholder {
  id: number;
  opportunityId: number;
  entityRoleId: number;
  entityRoleName: string;
  isInternal: boolean;
  stakeholderType: string;
  userId: number | null;
  userName: string | null;
  userEmail: string | null;
  notes: string | null;
}

/**
 * Deliverable model
 */
export interface OpportunityDeliverable {
  id: number;
  opportunityId: number;
  outputId: number | null;
  outputName: string | null;
  outputDescription: string | null;
  outputGroup: string | null;
  outputSubGroup: string | null;
  outputServiceLine: string | null;
  unitCode: string | null;
  projectCategoryCode: string | null;
  quantity: number | null;
  notes: string | null;
}

/**
 * Country model
 */
export interface OpportunityCountry {
  id: number;
  opportunityId: number;
  countryId: number;
  specificAreas: string | null;
  contextWarning: string | null;
  riskScore: number | null;
  country: {
    id: number;
    name: string;
    iso2Code: string;
    continent: string | null;
    region: string | null;
    artifacts?: Array<{
      artifactTypeCode: string;
      artifactTypeName: string;
      category: string;
      dataType: string;
      value: string;
      effectiveDate: string;
      expiryDate: string | null;
    }>;
  } | null;
}

/**
 * SDG Alignment model
 */
export interface OpportunitySDG {
  id: number;
  opportunityId: number;
  sdgId: string;
  sdgDatabaseId?: number;  // Database FK - used for saving
  sdgNumber: string;
  sdgName: string;
  isPrimary: boolean;
  notes: string | null;
}

/**
 * Computed statistics model
 */
export interface OpportunityStats {
  totalFundingUSD: number;
  totalFeeAmountUSD: number;
  fundingPartnerCount: number;
  clientPartnerCount: number;
  stakeholderCount: number;
  internalStakeholderCount: number;
  externalStakeholderCount: number;
  deliverableCount: number;
  countryCount: number;
  sdgCount: number;
  primarySDGId: number | null;
}

/**
 * Request model for creating new opportunity
 */
export interface OpportunityRequest {
  name: string;
  description: string;
  partnerReference?: string;
  workflowStageId?: number;
  responsibleOrgUnitId?: number;
  partnershipAgreementReference?: string;
  initiativeBudgetUSD?: number;
  targetSigningDate?: string;
  targetDeliveryDate?: string;
  proposedInitiativeTypeId?: number;
  fundingPartners?: OpportunityFundingPartnerRequest[];
  clientPartners?: OpportunityClientPartnerRequest[];
  stakeholders?: OpportunityStakeholderRequest[];
  deliverables?: OpportunityDeliverableRequest[];
  countries?: OpportunityCountryRequest[];
  sdGs?: OpportunitySDGRequest[];
}

/**
 * Request model for updating opportunity
 */
export interface UpdateOpportunityRequest extends OpportunityRequest {
  id: number;
}

/**
 * Child entity request models
 */
export interface OpportunityFundingPartnerRequest {
  partnerId: number;
  fundedAmount: number;
  currencyId: number;
  feePercentage?: number;
  feeAmount?: number;
  feeAmountUSD?: number;
  isAmountBasedFee?: boolean;
}

export interface OpportunityClientPartnerRequest {
  partnerId: number;
}

export interface OpportunityStakeholderRequest {
  stakeholderType: string;
  userId?: number;
  entityRoleId: number;
}

export interface OpportunityDeliverableRequest {
  outputId: number;
  quantity?: number;
  notes?: string;
}

export interface OpportunityCountryRequest {
  countryId: number;
  specificAreas?: string;
}

export interface OpportunitySDGRequest {
  sdgId: number;
  isPrimary: boolean;
  contributionLevel?: string;
  notes?: string;
}

// Related Items Models
export interface RelatedItems {
  contacts: RelatedContact[];
  partners: RelatedPartner[];
  interactions: RelatedInteraction[];
}

export interface RelatedContact {
  id: number;
  name: string;
  email?: string;
  jobTitle?: string;
  logoUrl?: string;
  organizationId?: number;
  organizationName?: string;
}

export interface RelatedPartner {
  id: number;
  name: string;
  logoUrl?: string;
  partnerType?: string;
  country?: string;
}

export interface RelatedInteraction {
  id: number;
  subject: string;
  interactionType?: string;
  interactionDate?: string;
  description?: string;
  partnerId?: number;
  partnerName?: string;
}

/**
 * DST (Digital Strategy & Transformation) Analysis Models
 * For AI-powered insights, recommendations, risks, and similar opportunities
 */
export interface DSTAnalysis {
  lastUpdated: string; // ISO date string
  risks: DSTRisk[];
  recommendations: DSTRecommendation[];
  similarOpportunities: SimilarOpportunity[];
}

export interface DSTRisk {
  id: number;
  title: string;
  description: string;
  severity: DSTSeverity; // 'High', 'Medium', 'Low'
  recommendation: string;
}

export type DSTSeverity = 'High' | 'Medium' | 'Low';

export interface DSTRecommendation {
  id: number;
  title: string;
  rationale: string;
  status?: 'pending' | 'accepted' | 'dismissed';
}

export interface SimilarOpportunity {
  id: number;
  name: string;
  relevance: number; // Percentage (0-100)
  status: string;
  budget: number;
  duration: string;
  keyLessons: string;
}

/**
 * Similar Project model - from AI-powered semantic search
 */
export interface SimilarProject {
  projectId: string;
  description: string | null;
  relevanceScore: number; // 0-100 similarity score
  startDate: string | null;
  endDate: string | null;
  partners: string | null;
  countries: string | null;
  projectManagerName: string | null;
  projectManagerEmail: string | null;
  projectUrl: string | null;
}

/**
 * Response from Similar Projects API
 */
export interface SimilarProjectsResponse {
  similarProjects: SimilarProject[];
  extractedKeywords: string[];
  totalFound: number;
  executionTimeMs: number;
}

/**
 * Similar Opportunity Model - for semantic search results
 */
export interface SimilarOpportunity {
  opportunityId: number;
  name: string;
  description: string | null;
  budget: number; // Budget in USD
  durationMonths: number | null; // Duration in months
  relevanceScore: number; // 0-100 similarity score
  workflowStage: string | null;
}

/**
 * Response model for similar opportunities search
 */
export interface SimilarOpportunitiesResponse {
  similarOpportunities: SimilarOpportunity[];
  totalFound: number;
  executionTimeMs: number;
}

/**
 * Analysis Section Models
 * For insights and suggestions in the Analysis section
 */
export interface OpportunityInsight {
  id: number;
  title: string;
  description: string;
  type: InsightType; // 'info', 'warning', 'success'
  priority: InsightPriority; // 'high', 'medium', 'low'
  createdDate: string;
}

export type InsightType = 'info' | 'warning' | 'success';
export type InsightPriority = 'high' | 'medium' | 'low';

export interface OpportunitySuggestion {
  id: number;
  title: string;
  description: string;
  actionTarget?: 'WHAT' | 'WHERE' | 'WHY' | 'WHO' | 'WHEN';
  createdDate: string;
}

/**
 * Response model for AI-generated insights and suggestions
 */
export interface OpportunityInsightsResponse {
  insights: OpportunityInsight[];
  suggestions: OpportunitySuggestion[];
}

/**
 * Risk Register Models - for DST Risks & Recommendations section
 */

/**
 * Risk model matching backend RiskModel.cs
 */
export interface Risk {
  id: number;
  entityType: string;
  entityId: number;
  title: string;
  description: string;
  recommendation: string;
  impact: number; // 1=Low, 2=Medium, 3=High
  status: string;
  identifiedDate: string | null;
  identifiedBy: string | null;
  createdDate: string;
  createdBy: string | null;
}

/**
 * Request model for creating/updating a risk
 */
export interface RiskCreateRequest {
  entityId: number;
  title: string;
  description: string;
  recommendation: string;
  impact: number; // 1=Low, 2=Medium, 3=High
}

/**
 * Response from GET dst-risks endpoint
 */
export interface DSTRisksResponse {
  risks: Risk[];
  totalCount: number;
}

/**
 * AI-generated recommendation model matching backend DSTRecommendation
 */
export interface AIRiskRecommendation {
  title: string;
  description: string;
  recommendation: string;
  relevanceScore: number;
  sourceRiskId: string | null;
}

/**
 * Response from GET dst-recommendations endpoint
 */
export interface DSTRecommendationsResponse {
  recommendations: AIRiskRecommendation[];
  extractedKeywords: string[];
  totalFound: number;
  executionTimeMs: number;
}

