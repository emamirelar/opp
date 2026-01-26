/**
 * @fileoverview TypeScript models for Opportunity entity and related child entities
 * @author UNOPS Opportunity+ System Development Team
 */

import { EntityTag } from './entity-tag.model';

/**
 * Document detail model for display purposes
 */
export interface DocumentDetail {
  id: number;
  name: string | null;
  type: string | null;
  storagePath: string | null;
  link: string | null;
}

/**
 * Partner Agreement Information model
 */
export interface PartnerAgreementInfo {
  partnerAgreementNumber: string;
  name: string;
  partnerAgreementType: string | null;
  partnerAgreementTypeDescription: string | null;
  partnerAgreementScope: string | null;
  partnerAgreementScopeDescription: string | null;
  startDate: Date | null;
  endDate: Date | null;
  signedDate: Date | null;
  coversOpportunityPeriod: boolean;
  expiresBeforeOpportunityEnd: boolean;
  serviceLinesDescription: string | null;
  geographicRestrictions: string | null;
  hasGeographicRestrictions: boolean;
  warningMessage: string | null;
  source: string; // "ERP" or "Document"
  documentId: number | null;
  documentStoragePath: string | null;
}

/**
 * UNOPS Strategic Mission model
 */
export interface UNOPSMission {
  id: number;
  code: string;
  name: string;
  description: string | null;
  iconClass: string | null;
  displayOrder: number;
  status: string;
}

/**
 * Opportunity UNOPS Mission alignment (junction model)
 */
export interface OpportunityUNOPSMission {
  id: number;
  opportunityId: number;
  unopsMissionId: number;
  unopsMission: UNOPSMission | null;
}

/**
 * SME (Subject Matter Expert) selection for an opportunity
 * Loaded from OpportunityStakeholder table where IsInternal = true and EntityRole.Type = "SME"
 */
export interface SMESelection {
  entityRoleId: number;
  entityRoleName: string | null;
  isSelected: boolean;
  userId: number | null;
  userName: string | null;
  userEmail: string | null;
}

/**
 * SME (Subject Matter Expert) selection request for saving
 */
export interface SMESelectionRequest {
  entityRoleId: number;
  isSelected: boolean;
  userId: number | null;
}

/**
 * Main Opportunity model matching backend OpportunityModel.cs
 */
export interface Opportunity {
  id: number;
  name: string;
  description: string | null;
  partnerReference: string | null;
  status: string;
  stage: string | null;
  workflowStatus: string | null;
  isInWorkflow: boolean;
  responsibleOrgUnitId: number | null;
  responsibleOrgUnitName: string | null;
  proposedInitiativeTypeId: number | null;
  proposedInitiativeTypeName: string | null;
  initiativeBudgetUSD: number | null;
  partnershipAgreementReference: string | null;
  targetSigningDate: string | null;
  implementationStartDate: string | null;
  targetDeliveryDate: string | null;
  isTargetSigningDateFirm: boolean;
  signingDateNotes: string | null;
  submissionDeadline: string | null;
  resultsFocus: string | null;
  expectedImpact: string | null;
  expectedOutcomes: string | null;
  expectedBeneficiaries: string | null;
  estimatedDirectBeneficiaries: number | null;
  estimatedIndirectBeneficiaries: number | null;
  beneficiariesToBeDetermined: boolean;
  challenges: string | null;
  opportunityStatementMarkdown: string | null;
  opportunityBannerImage: string | null;
  opportunityThumbnail: string | null;
  isPooledFunding: boolean;
  highRisksAcknowledged: boolean;
  deliveryModality: number | null;
  fundingPartners: OpportunityFundingPartner[];
  clientPartners: OpportunityClientPartner[];
  stakeholders: OpportunityStakeholder[];
  externalStakeholders: OpportunityExternalStakeholder[];
  miscExternalStakeholders: string | null;
  externalStakeholderNotes: string | null;
  deliverables: OpportunityDeliverable[];
  countries: OpportunityCountry[];
  sdGs: OpportunitySDG[];
  uncfOutcomes?: OpportunityUNCFOutcome[];
  unopsMissions?: OpportunityUNOPSMission[];
  collaborators?: OpportunityCollaborator[];
  opportunityManager?: OpportunityManager;
  smeSelections?: SMESelection[];
  stats: OpportunityStats | null;
  isNewValueRangeForOrgUnit: boolean | null;
  orgUnitHistoricalMaxValue: number | null;
  dstAnalysis: DSTAnalysis | null; // DST Insights & Recommendations
  insights: OpportunityInsight[]; // Analysis insights
  suggestions: OpportunitySuggestion[]; // Analysis suggestions
  createdDate: string;
  lastModifiedDate: string;
  createdBy: number;
  createdByName: string | null;
  lastModifiedBy: number;
  lastModifiedByName: string | null;
  tags?: EntityTag[];
  permissions?: EntityPermissions;
}

/**
 * Entity permissions model for record-level access control
 */
export interface EntityPermissions {
  canRead: boolean;
  canCreate: boolean;
  canUpdate: boolean;
  canDelete: boolean;
  canEditFields?: string[];
  canActivate?: boolean;
  canClose?: boolean;
  canArchive?: boolean;
  canApprove?: boolean;
  canUnapprove?: boolean;
  canExport?: boolean;
  canImport?: boolean;
  permissionSource?: string;
  notes?: string;
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
  documentId: number | null;
  documentName: string | null;
  associatedDocuments: DocumentDetail[] | null;
  partnerStatus: string | null;
  partnerApprovalStatus: string | null;
  ddApproval: string | null;
  ddApprovalDate: Date | null;
  ddExpiryDate: Date | null;
  ddStatus: string | null;
  ddExpiresBeforeOpportunityEnd: boolean | null;
  // Currency and USD conversion fields
  partnerPreferredCurrency: string | null;
  amountUSD: number | null;
  exchangeRate: number | null;
  exchangeRateDate: Date | null;
  exchangeRateDisplay: string | null;
  isPooledContribution: boolean;
  selectedPartnerAgreementNumber: string | null;
  availableAgreements: PartnerAgreementInfo[] | null;
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
  documentId: number | null;
  documentName: string | null;
  associatedDocuments: DocumentDetail[] | null;
  partnerStatus: string | null;
  partnerApprovalStatus: string | null;
  ddApproval: string | null;
  ddApprovalDate: Date | null;
  ddExpiryDate: Date | null;
  ddStatus: string | null;
  ddExpiresBeforeOpportunityEnd: boolean | null;
  selectedPartnerAgreementNumber: string | null;
  availableAgreements: PartnerAgreementInfo[] | null;
}

/**
 * Stakeholder model (Internal UNOPS users only)
 */
export interface OpportunityStakeholder {
  id: number;
  opportunityId: number;
  entityRoleId: number;
  entityRoleName: string;
  entityRoleCode: string | null;
  isInternal: boolean;
  stakeholderType: string;
  userId: number | null;
  userName: string | null;
  userEmail: string | null;
  /** Standardized position title from personnel record */
  position: string | null;
  /** Organization Hierarchy ID - used for auto-populated stakeholders from EntityUserRoles */
  organizationHierarchyId: number | null;
  /** Organization Hierarchy Name - the name of the org unit for auto-populated stakeholders */
  organizationHierarchyName: string | null;
  /** Indicates if this stakeholder was auto-populated from EntityUserRoles. Cannot be edited/removed. */
  isAutoPopulated: boolean;
  /** Indicates if this stakeholder is from a normally responsible org unit (different from selected responsible org unit) */
  isNormallyResponsible?: boolean;
  /** Country name for normally responsible org unit stakeholders */
  countryName?: string;
  notes: string | null;
}

/**
 * Collaborator expertise model - the specific expertise/capacity in which a collaborator is related to an opportunity
 */
export interface CollaboratorExpertise {
  id: number;
  code: string;
  name: string;
  description: string | null;
  displayOrder: number;
}

/**
 * Collaborator model - team members with edit permissions
 * Part of the Opportunity Development Team
 */
export interface OpportunityCollaborator {
  id: number;
  opportunityId: number;
  userId: number;
  userName: string | null;
  userEmail: string | null;
  /** Standardized position title from personnel record */
  position: string | null;
  addedDate: string | null;
  addedBy: number | null;
  addedByName: string | null;
  /** List of expertise areas for this collaborator */
  expertises: CollaboratorExpertise[];
}

/**
 * Opportunity Manager model - the primary person responsible for the opportunity
 * Part of the Opportunity Development Team
 */
export interface OpportunityManager {
  userId: number;
  userName: string | null;
  userEmail: string | null;
  /** Standardized position title from personnel record */
  position: string | null;
}

/**
 * External Stakeholder model (contacts)
 */
export interface OpportunityExternalStakeholder {
  id: number;
  opportunityId: number;
  contactId: number;
  contactName: string | null;
  contactEmail: string | null;
  contactOrganization: string | null;
}

/**
 * Deliverable model
 */
export interface OpportunityDeliverable {
  id: number;
  opportunityId: number;
  outputId: number | null;
  outputName: string | null;
  
  // Hierarchical Output fields from new Products and Services List
  level0: string | null;
  level1: string | null;
  definitionLevel1: string | null;
  level2: string | null;
  definitionLevel2: string | null;
  level3: string | null;
  definitionLevel3: string | null;
  level4: string | null;
  definitionLevel4: string | null;
  serviceLine: string | null;
  
  // Component flags from Output entity
  grantSupportImplementingModality: boolean | null;
  grantSupportComponent: boolean | null;
  procurementComponent: boolean | null;
  procurementInstallationComponent: boolean | null;
  infrastructureComponent: boolean | null;
  
  // Timeline and Work Breakdown Structure fields
  sequenceOrder: number | null;
  plannedStartDate: string | null;  // ISO date string
  plannedEndDate: string | null;    // ISO date string
  
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
  humanitarianFrameworkAlignment: boolean | null;
  hasHumanitarianFramework: boolean;
  ndcAlignment: boolean | null;
  hasNdc: boolean;
  napAlignment: boolean | null;
  hasNap: boolean;
  orgUnitStrategyAlignment: boolean | null;
  hasOrgUnitStrategy: boolean;
  orgUnitWithStrategyId: number | null;
  orgUnitWithStrategyName: string | null;
  orgUnitWithStrategyCode: string | null;
  currentOrgUnitWithStrategyId: number | null;
  currentOrgUnitWithStrategyName: string | null;
  currentOrgUnitWithStrategyCode: string | null;
  hasMoreLocalStrategyAvailable: boolean;
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
    tags?: Array<{
      tag: string;
      color: string;
    }>;
    hasActiveUNCF?: boolean;
    organizationUnitHierarchy?: OrganizationUnitHierarchyNode[];
  } | null;
}

/**
 * Organization unit hierarchy node
 */
export interface OrganizationUnitHierarchyNode {
  id: number;
  code: string;
  name: string;
  type: string;
  description: string | null;
  parentId: number | null;
  level: number;
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
  skipTargetsAndIndicators?: boolean | null;
  notes: string | null;
  targets?: OpportunitySDGTarget[];
}

/**
 * SDG Target model for opportunity
 */
export interface OpportunitySDGTarget {
  id: number;
  opportunityId: number;
  opportunitySDGId: number;
  sdgTargetDatabaseId: number;  // Database FK
  sdgTargetId: string;  // String identifier like "1.1", "3.3"
  targetDescription: string | null;
  targetType: string | null;
  notes: string | null;
  indicators?: OpportunitySDGIndicator[];
}

/**
 * SDG Indicator model for opportunity
 */
export interface OpportunitySDGIndicator {
  id: number;
  opportunityId: number;
  opportunitySDGTargetId: number;
  sdgIndicatorDatabaseId: number;  // Database FK
  sdgIndicatorId: string;  // String identifier like "1.1.1", "3.3.2"
  sdgIndicatorLongDescription: string | null;
  notes: string | null;
}

/**
 * UNCF Outcome model for opportunity
 * Linked through OpportunityCountry (country-specific)
 */
export interface OpportunityUNCFOutcome {
  id: number;
  opportunityId: number;
  opportunityCountryId: number;
  uncfOutcomeId: number;  // Database FK
  uncfOutcomeExternalId: string | null;  // String identifier from external system
  uncfOutcomeName: string | null;
  versionNo: number | null;
  country: string | null;  // ISO2 code
  notes: string | null;
  indicators?: OpportunityUNCFIndicator[];
  isInactive?: boolean;  // Indicates if this outcome is outside its active date range
  hasNewerVersion?: boolean;  // Indicates if a newer version is available
}

/**
 * UNCF Indicator model for opportunity
 * Child relationship of OpportunityUNCFOutcome
 */
export interface OpportunityUNCFIndicator {
  id: number;
  opportunityId: number;
  opportunityUNCFOutcomeId: number;
  uncfIndicatorId: number;  // Database FK
  uncfIndicatorExternalId: string | null;  // String identifier from external system
  uncfIndicatorName: string | null;
  notes: string | null;
  isInactive?: boolean;  // Indicates if this indicator is outside its active date range
  hasNewerVersion?: boolean;  // Indicates if a newer version is available
}

/**
 * Computed statistics model
 */
export interface OpportunityStats {
  totalFundingUSD: number;
  totalFeeAmountUSD: number;
  fundingPartnerCount: number;
  clientPartnerCount: number;
  totalPartnerCount: number;
  stakeholderCount: number;
  internalStakeholderCount: number;
  externalStakeholderCount: number;
  deliverableCount: number;
  countryCount: number;
  sdgCount: number;
  primarySDGId: number | null;
  daysToTargetSigningDate?: number | null;
  serviceLines: string[];
}

/**
 * Request model for creating new opportunity
 */
export interface OpportunityRequest {
  name: string;
  description: string;
  partnerReference?: string;
  responsibleOrgUnitId?: number;
  partnershipAgreementReference?: string;
  initiativeBudgetUSD?: number;
  targetSigningDate?: string;
  implementationStartDate?: string;
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
  documentId?: number;
  isPooledContribution?: boolean;
  selectedPartnerAgreementNumber?: string; // AC9
}

export interface OpportunityClientPartnerRequest {
  partnerId: number;
  documentId?: number;
  selectedPartnerAgreementNumber?: string; // AC9
}

export interface OpportunityStakeholderRequest {
  stakeholderType: string;
  userId?: number;
  entityRoleId: number;
}

export interface OpportunityExternalStakeholderRequest {
  contactId: number;
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
  skipTargetsAndIndicators?: boolean | null;
  contributionLevel?: string;
  notes?: string;
  targets?: OpportunitySDGTargetRequest[];
}

export interface OpportunitySDGTargetRequest {
  opportunitySDGId: number;
  sdgTargetDatabaseId: number;
  notes?: string;
  sdgIndicatorDatabaseIds?: number[];  // List of indicator database IDs
}

export interface OpportunitySDGIndicatorRequest {
  opportunitySDGTargetId: number;
  sdgIndicatorDatabaseId: number;
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
  relevanceExplanation?: string | null; // AI-generated one-line explanation of relevance (max 120 chars)
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
 * Relevant Person Model - for finding relevant people from corporate directory
 */
export interface RelevantPerson {
  personId: string; // Person ID from oneUNOPS
  name: string | null;
  title: string | null; // Job title/position
  department: string | null; // Department or organizational unit
  email: string | null;
  location: string | null; // Location/duty station
  photoUrl: string | null; // Profile photo URL from Google Workspace
  expertise: string[] | null; // Areas of expertise or skills
  relevanceScore: number; // 0-100 similarity score based on role match
  relevanceExplanation?: string | null; // AI-generated one-line explanation of relevance (max 120 chars)
  metadata: Record<string, any> | null; // Additional metadata
}

/**
 * Response from Relevant People API
 */
export interface RelevantPeopleResponse {
  relevantPeople: RelevantPerson[];
  extractedRoles: string[]; // Role keywords extracted for search
  totalFound: number;
  searchTimestamp: string;
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
 * Aligned with oUP risk management system
 */

/**
 * Risk model matching backend RiskModel.cs (oUP aligned)
 */
export interface Risk {
  id: number;
  entityType: string;
  entityId: number;
  
  // Mandatory fields
  title: string;
  riskTypeId: number;
  riskTypeName: string | null;
  riskTypeCode: string | null;
  riskCategoryId: number;
  riskCategoryName: string | null;
  riskCategoryFullPath: string | null;
  riskProbabilityId: number;
  riskProbabilityName: string | null;
  riskProximityId: number;
  riskProximityName: string | null;
  riskImpactLevelId: number;
  riskImpactLevelName: string | null;
  riskResponseTypeId: number | null; // Mandatory for Opportunity type
  riskResponseTypeName: string | null;
  
  // Optional fields
  description: string;
  recommendation: string;
  
  // Legacy fields (backward compatibility)
  impact: number; // 1=Low, 2=Medium, 3=High
  status: string;
  
  // PreDefined High Risk reference
  preDefinedHighRiskId: number | null;
  preDefinedHighRiskCode: string | null;
  preDefinedHighRiskTitle: string | null;
  
  // Audit fields
  identifiedDate: string | null;
  identifiedBy: string | null;
  createdDate: string;
  createdBy: string | null;
}

/**
 * Request model for creating/updating a risk
 * For predefined high risks: All oUP fields are mandatory
 * For manual entry: Only title is mandatory, oUP fields will get defaults
 */
export interface RiskCreateRequest {
  entityId: number;
  
  // Always mandatory
  title: string;
  
  // oUP fields - Mandatory for predefined high risks, optional with defaults for manual entry
  riskTypeId?: number; // MANDATORY if preDefinedHighRiskId is set, OPTIONAL for manual (defaults to THREAT)
  riskCategoryId?: number; // MANDATORY if preDefinedHighRiskId is set, OPTIONAL for manual (gets default category)
  riskProbabilityId?: number; // MANDATORY if preDefinedHighRiskId is set, OPTIONAL for manual (defaults to MEDIUM)
  riskProximityId?: number; // MANDATORY if preDefinedHighRiskId is set, OPTIONAL for manual (defaults to WITHIN_SIX_MONTHS)
  riskImpactLevelId?: number; // MANDATORY if preDefinedHighRiskId is set, OPTIONAL for manual (defaults to MEDIUM)
  riskResponseTypeId?: number | null; // CONDITIONAL - mandatory if preDefinedHighRiskId is set AND riskType = Opportunity
  
  // Optional fields
  description?: string;
  recommendation?: string;
  preDefinedHighRiskId?: number | null; // If set, indicates predefined mode; if null, manual mode
  
  // Legacy field (backward compatibility)
  impact?: number; // 1=Low, 2=Medium, 3=High
}

/**
 * Risk Type lookup (Threat or Opportunity)
 */
export interface RiskTypeModel {
  id: number;
  name: string;
  code: string;
  description: string | null;
  isResponseTypeMandatory: boolean;
  displayOrder: number;
}

/**
 * Risk Probability lookup
 */
export interface RiskProbabilityModel {
  id: number;
  name: string;
  code: string;
  displayLabel: string | null;
  numericValue: number;
  displayOrder: number;
}

/**
 * Risk Proximity lookup
 */
export interface RiskProximityModel {
  id: number;
  name: string;
  code: string;
  monthsValue: number | null;
  displayOrder: number;
}

/**
 * Risk Impact Level lookup
 */
export interface RiskImpactLevelModel {
  id: number;
  name: string;
  code: string;
  displayLabel: string | null;
  numericValue: number;
  displayOrder: number;
}

/**
 * Risk Response Type lookup
 */
export interface RiskResponseTypeModel {
  id: number;
  name: string;
  code: string;
  description: string | null;
  validForThreat: boolean;
  validForOpportunity: boolean;
  displayOrder: number;
}

/**
 * Combined response for all risk lookups
 */
export interface RiskLookupsResponse {
  riskTypes: RiskTypeModel[];
  probabilities: RiskProbabilityModel[];
  proximities: RiskProximityModel[];
  impactLevels: RiskImpactLevelModel[];
  responseTypes: RiskResponseTypeModel[];
}

/**
 * Risk Category model (3-level hierarchy)
 */
export interface RiskCategoryModel {
  id: number;
  code: string;
  shortCode: string;
  name: string;
  level: number; // 1, 2, or 3
  parentCategoryId: number | null;
  parentCategoryName: string | null;
  displayOrder: number;
  isSelectable: boolean; // Only Level 3 are selectable
  children: RiskCategoryModel[];
}

/**
 * Response for Risk Category hierarchy
 */
export interface RiskCategoryHierarchyResponse {
  categories: RiskCategoryModel[]; // Hierarchical (Level 1 with nested children)
  selectableCategories: RiskCategoryModel[]; // Flat list of Level 3 only
  totalLevel1: number;
  totalLevel2: number;
  totalLevel3: number;
}

/**
 * PreDefined High Risk model (EAC checklist item)
 */
export interface PreDefinedHighRiskModel {
  id: number;
  code: string;
  displayCode: string;
  name: string;
  shortTitle: string;
  description: string;
  categoryCode: string;
  level1: number;
  level2Code: string;
  isAutoDetectable: boolean;
  detectionRuleType: string | null;
  displayOrder: number;
  riskCategoryId: number | null;
  riskCategoryName: string | null;
}

/**
 * AI-detected high risk recommendation
 */
export interface HighRiskRecommendation {
  preDefinedHighRisk: PreDefinedHighRiskModel;
  confidenceLevel: number; // 0-100
  detectionReason: string;
  triggerData: string;
  isStronglyRecommended: boolean; // confidence >= 80
}

/**
 * Response for High Risk Analysis
 */
export interface HighRiskAnalysisResponse {
  availableHighRisks: PreDefinedHighRiskModel[];
  recommendations: HighRiskRecommendation[];
  alreadyAddedHighRiskIds: number[];
  totalHighRisks: number;
  stronglyRecommendedCount: number;
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
 * Enhanced to support predefined high risks from oUP EAC checklist
 */
export interface AIRiskRecommendation {
  title: string;
  description: string;
  recommendation: string;
  relevanceScore: number;
  sourceRiskId: string | null;
  /**
   * oUP Question ID if this is a predefined high risk from EAC checklist
   * Used as stable identifier for dismiss persistence and oUP mapping
   */
  oupQuestionId: number | null;
  /**
   * PreDefined High Risk entity ID (for linking when creating risk)
   */
  preDefinedHighRiskId: number | null;
  /**
   * Risk Category ID from the predefined high risk (Level 3 category)
   */
  riskCategoryId: number | null;
  /**
   * Confidence level (0-100) indicating how strongly this risk applies
   * >= 80 means strongly recommended
   */
  confidenceLevel: number;
  /**
   * Source type: "PREDEFINED_HIGH_RISK" or "SIMILAR_PROJECT"
   */
  sourceType: 'PREDEFINED_HIGH_RISK' | 'SIMILAR_PROJECT';
  /**
   * Whether this is strongly recommended (confidence >= 80)
   */
  isStronglyRecommended: boolean;
  /**
   * Unique stable identifier for dismiss persistence
   * Uses oupQuestionId for predefined risks, sourceRiskId for vector store risks
   */
  stableIdentifier: string;
}

/**
 * Request for POST dst-recommendations endpoint
 */
export interface DSTRecommendationsRequest {
  dismissedOupQuestionIds: number[];
}

/**
 * Response from POST dst-recommendations endpoint
 */
export interface DSTRecommendationsResponse {
  recommendations: AIRiskRecommendation[];
  extractedKeywords: string[];
  totalFound: number;
  executionTimeMs: number;
}

/**
 * Response model for opportunity statement validation
 * Contains information about whether the statement is aligned with structured data
 */
export interface OpportunityStatementValidationResponse {
  opportunityId: number;
  isAligned: boolean;
  misalignmentItems: string[];
  message: string;
}

/**
 * Framework Status Models
 * Response from framework status check endpoint
 */
export interface FrameworkStatusResponse {
  hasTaggedFrameworks: boolean;
  taggedFrameworks: TaggedFrameworkInfo[];
  allDocumentsCount: number;
}

/**
 * Information about a tagged Partner Results Framework document
 */
export interface TaggedFrameworkInfo {
  partnerId: number;
  partnerName: string;
  documentId: number;
  documentName: string;
  documentStoragePath: string;
  partnerType: 'Funding' | 'Client';
}

/**
 * Extracted Deliverable Models
 * Temporary model for AI-extracted products/services (not yet saved to database)
 */
export interface ExtractedDeliverableInfo {
  partnerLanguage: string; // The exact partner language/wording from the source document
  context: string; // Context information about where this was found in the document
  sourceDocumentName: string; // Name of the source document
  sourceDocumentId: number; // ID of the source document
  isPrioritySource: boolean; // True if extracted from tagged Partner Results Framework
  confidence: number; // AI confidence score (0.0 to 1.0)
  reasoning: string; // AI reasoning for why this item was extracted
  matchedOutputId?: number | null; // Matched Output ID from the Outputs table (if similarity match found)
  matchedOutputName?: string | null; // Name of the matched output from the Outputs table
  matchScore?: number | null; // Similarity score for the matched output (0.0 to 1.0)
  matchedField?: string | null; // Field name that was matched in the Outputs table
}

/**
 * Duration Option for Implementation Period
 * Used in the WHEN section duration calculator
 */
export interface DurationOption {
  label: string;
  value: number; // Duration in months (-1 for custom)
}

