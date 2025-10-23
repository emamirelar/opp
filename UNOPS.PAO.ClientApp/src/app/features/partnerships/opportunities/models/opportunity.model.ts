export interface Opportunity {
  id?: number;
  name?: string;
  description?: string;
  partnerReference?: string;
  status?: string;
  workflowStageId?: number;
  workflowStageName?: string;
  responsibleOrgUnitId?: number;
  responsibleOrgUnitName?: string;
  partnershipAgreementReference?: string;
  initiativeBudgetUSD?: number;
  targetSigningDate?: Date | null;
  targetDeliveryDate?: Date | null;
  proposedInitiativeTypeId?: number;
  proposedInitiativeTypeName?: string;
  createdDate?: Date;
  lastModifiedDate?: Date;
  createdBy?: number;
  createdByName?: string;
  lastModifiedBy?: number;
  lastModifiedByName?: string;
}

export interface OpportunityFilterParams {
  name?: string;
  status?: string;
  workflowStageId?: number;
  responsibleOrgUnitId?: number;
  proposedInitiativeTypeId?: number;
  minBudget?: number;
  maxBudget?: number;
  targetSigningDateFrom?: Date;
  targetSigningDateTo?: Date;
  createdDateFrom?: Date;
  createdDateTo?: Date;
}

