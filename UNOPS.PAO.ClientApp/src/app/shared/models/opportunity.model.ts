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
  amount: number | null;
  currencyId: number;
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
}

/**
 * Stakeholder model
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
  contactId: number | null;
  contactName: string | null;
  contactEmail: string | null;
  organization: string | null;
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
  countryName: string;
  countryCode: string;
  specificAreas: string | null;
  contextWarning: string | null;
  riskScore: number | null;
}

/**
 * SDG Alignment model
 */
export interface OpportunitySDG {
  id: number;
  opportunityId: number;
  sdgId: number;
  sdgNumber: number;
  sdgName: string;
  isPrimary: boolean;
  contributionLevel: string | null;
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

