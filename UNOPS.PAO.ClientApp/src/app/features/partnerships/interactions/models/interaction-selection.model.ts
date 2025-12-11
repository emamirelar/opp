/**
 * @fileoverview Models for creating opportunities from interactions
 * @author UNOPS Opportunity+ System Development Team
 */

/**
 * Summary model for interaction selection in dialogs
 */
export interface InteractionSummary {
  id: number;
  subject: string;
  type: string;
  date: string;
  description?: string;
  partnerNames?: string[];
  contactNames?: string[];
  selected?: boolean;
}

/**
 * Dialog configuration for creating opportunity from interactions
 */
export interface CreateOpportunityFromInteractionsConfig {
  preSelectedInteractionIds?: number[];
  currentInteractionId?: number;
  partnerId: number;
  partnerName: string;
  mode: 'list-view' | 'detail-view';
}

/**
 * Dialog state for step management
 */
export interface DialogState {
  currentStep: 'select' | 'review' | 'creating';
  selectedInteractions: InteractionSummary[];
  opportunityName: string;
  opportunityDescription: string;
  showAdditionalSelection: boolean;
  searchQuery: string;
  generating: boolean;
}

/**
 * Proposed opportunity field from AI analysis
 */
export interface ProposedField {
  fieldName: string;
  fieldLabel: string;
  proposedValue: any;
  confidence: number; // 0-100
  justification: string;
  sourceInteractionIds: number[];
  isAccepted: boolean;
  isHighlighted: boolean;
}

/**
 * Proposed deliverable from AI analysis
 */
export interface ProposedDeliverable {
  outputName: string;
  outputDescription?: string;
  outputGroup?: string;
  outputSubGroup?: string;
  outputServiceLine?: string;
  unitCode?: string;
  projectCategoryCode?: string;
  quantity?: number;
  notes?: string;
}

/**
 * Proposed country from AI analysis
 */
export interface ProposedCountry {
  countryName: string;
  iso2Code?: string;
  specificAreas?: string;
  country?: {
    id?: number;
    name: string;
    iso2Code: string;
    continent?: string;
    region?: string;
  };
}

/**
 * Proposed SDG from AI analysis
 */
export interface ProposedSDG {
  sdgNumber: string;
  sdgName: string;
  isPrimary: boolean;
}

/**
 * Proposed funding partner from AI analysis
 */
export interface ProposedFundingPartner {
  partnerId?: number;
  partnerName: string;
  partnerLogoUrl?: string;
  amount?: number;
  currencyCode?: string;
  feeAmount?: number;
  partnershipAgreementReference?: string;
}

/**
 * Proposed client partner from AI analysis
 */
export interface ProposedClientPartner {
  partnerId?: number;
  partnerName: string;
  partnerLogoUrl?: string;
}

/**
 * Proposed stakeholder from AI analysis
 */
export interface ProposedStakeholder {
  userName: string;
  entityRoleName: string;
}

/**
 * Proposed opportunity response from backend (raw format with stringified collections)
 */
export interface ProposedOpportunityResponseRaw {
  opportunity: {
    name: string;
    description: string;
    partnerReference?: string;
    responsibleOrgUnitId?: number | null;
    responsibleOrgUnitName?: string | null;
    proposedInitiativeTypeId?: number | null;
    proposedInitiativeTypeName?: string | null;
    initiativeBudgetUSD?: number | null;
    partnershipAgreementReference?: string | null;
    targetSigningDate?: string | null;
    targetDeliveryDate?: string | null;
    strategicAlignment?: string | null;
    resultsFocus?: string | null;
    expectedImpact?: string | null;
    expectedOutcomes?: string | null;
    expectedBeneficiaries?: string | null;
    // Collection fields are stringified JSON from backend
    fundingPartners?: string | null;
    clientPartners?: string | null;
    stakeholders?: string | null;
    deliverables?: string | null;
    countries?: string | null;
    sdGs?: string | null;
    dependents?: string | null;
  };
  interactionsAnalyzed: number;
  sourceInteractionIds?: number[] | null;
  documentsAnalyzed: number;
  sourceDocumentIds?: number[] | null;
  partnerId: number;
  partnerName: string;
  isFundingPartner: boolean;
  isClientPartner: boolean;
}

/**
 * Proposed opportunity response (parsed format with typed collections)
 */
export interface ProposedOpportunityResponse {
  opportunity: {
    name: string;
    description: string;
    partnerReference?: string;
    responsibleOrgUnitId?: number | null;
    responsibleOrgUnitName?: string | null;
    proposedInitiativeTypeId?: number | null;
    proposedInitiativeTypeName?: string | null;
    initiativeBudgetUSD?: number | null;
    partnershipAgreementReference?: string | null;
    targetSigningDate?: string | null;
    targetDeliveryDate?: string | null;
    strategicAlignment?: string | null;
    resultsFocus?: string | null;
    expectedImpact?: string | null;
    expectedOutcomes?: string | null;
    expectedBeneficiaries?: string | null;
    fundingPartners?: ProposedFundingPartner[] | null;
    clientPartners?: ProposedClientPartner[] | null;
    stakeholders?: ProposedStakeholder[] | null;
    deliverables?: ProposedDeliverable[] | null;
    countries?: ProposedCountry[] | null;
    sdGs?: ProposedSDG[] | null;
    dependents?: string[] | null;
  };
  interactionsAnalyzed: number;
  sourceInteractionIds?: number[] | null;
  documentsAnalyzed: number;
  sourceDocumentIds?: number[] | null;
  partnerId: number;
  partnerName: string;
  isFundingPartner: boolean;
  isClientPartner: boolean;
}

/**
 * Request to propose opportunity from interactions, documents, or both
 * Unified request for the generate-proposal endpoint
 */
export interface ProposeOpportunityRequest {
  opportunityName: string;
  opportunityDescription: string;
  partnerId?: number;
  isFundingPartner: boolean;
  isClientPartner: boolean;
  interactionIds?: number[];
  newDocumentStoragePaths?: string[]; // GCS URIs for newly uploaded documents
  newDocumentMimeTypes?: string[]; // MIME types for newly uploaded documents
  existingDocumentIds?: number[]; // IDs of existing documents in database
}

/**
 * Accepted field for final opportunity creation
 */
export interface AcceptedField {
  fieldName: string;
  proposedValue: any;
  confidence: number;
}

/**
 * Request to create opportunity from proposal
 */
export interface CreateOpportunityFromProposalRequest {
  name: string;
  description: string;
  partnerId: number;
  isFundingPartner: boolean;
  isClientPartner: boolean;
  acceptedFields: AcceptedField[];
  rejectedFields?: ProposedField[];
  sourceInteractionIds: number[];
}

