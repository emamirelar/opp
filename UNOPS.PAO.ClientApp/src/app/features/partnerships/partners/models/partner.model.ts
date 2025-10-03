import { PartnerTree } from "./partner-tree.model";
import { EntityPermissionSet } from '@features/shared/models/shared-types';
import { OrganizationUnitRelationshipModel } from '@partnerships/partners/models/organization-unit-relationship.model';
import { OrganizationHierarchyModel } from '@core/models/organization-hierarchy.model';
import { EntityTag, TaggedEntity } from '@shared/models/entity-tag.model';

export interface Partner extends TaggedEntity {
  id?: string | null;

  // ========== GENERAL FIELDS ==========
  partnerDescription?: string | null; // Full name (required) - was "name"
  partnerShortDescription?: string | null; // Short name/acronym (required) - was "shortName"
  partnerLongDescription?: string | null; // Optional long description
  partnerCategoryId?: number | null; // FK to Partner Category (required)
  liaisonOfficeId?: number | null; // FK to LiaisonOffice (required)
  liaisonOfficeName?: string | null; // LiaisonOffice Name
  partnerFocalPointUserId?: number | null; // FK to User (Partner Focal Point)
  partnerFocalPointUserName?: string | null; // Partner Focal Point User Email
  partnerFocalPointName?: string | null; // Partner Focal Point Display Name

  // Backward compatibility
  name?: string | null; // Maps to partnerDescription for backward compatibility
  shortName?: string | null; // Maps to partnerShortDescription for backward compatibility

  status?: string | null;
  pooledFund?: boolean | null;

  // ========== APPROVAL FIELDS ==========
  keyGlobalPartner?: boolean | null; // was "globalKeyAccount"
  unAndStateEntity?: boolean | null; // New field
  unSecretariatPartner?: boolean | null; // was "unSecretariatEntity"
  dueDiligenceRequired?: string | null; // was "ddRequired"
  dueDiligenceApproval?: string | null; // was "ddeacDone"
  dueDiligenceApprovalDate?: Date | null; // New field
  dueDiligenceExpiryDate?: Date | null; // New field
  partnerApprovalStatus?: string | null; // "NotApproved" | "Approved"
  partnerApprovalDate?: Date | null; // New field
  partnerApprovalReference?: string | null; // was "eacReference"
  partnerLevyStatus?: string | null; // was "levyPotentiallyApplies"
  reasonForLevy?: string | null; // was "reasonForLevyNotApplying"
  levyTreatment?: string | null;
  canCreateNewOpportunities?: boolean | null; // New field
  reasonForNoNewOpportunity?: string | null; // New field (Reason)

  address1Street?: string | null;
  address1Street2?: string | null;
  address1City?: string | null;
  address1StateProvince?: string | null;
  address1PostalCode?: string | null;
  address1Country?: string | null;
  discriminator?: string | null;
  createdBy?: string | null;
  createdDate?: Date | null;
  lastModifiedBy?: string | null;
  lastModifiedDate?: Date | null;
  
  // Resolved user names for audit fields
  createdByName?: string;
  lastModifiedByName?: string;
  isDeleted?: boolean | null;
  // Organization Unit Relationships
  organizationUnitRelationships?: OrganizationUnitRelationshipModel[] | null;
  partnerCategory?: string | null;
  logoUrl?: string | null;
  deletedBy?: string | null;
  deletedDate?: Date | null;
  _updated?: boolean;
  _importRowId?: string;
  partnerTree?: PartnerTree | null;
  partnerGroupId?: number | null;
  partnerGroupName?: string | null;
  partnerCategoryCode?: string | null;
  partnerCategoryName?: string | null;
  erpDimValue?: string | null;
  // RBAC permissions
  permissions?: EntityPermissionSet;
}

// Utility function to get the primary organization unit from a partner
export function getPrimaryOrganizationUnit(partner: Partner): OrganizationHierarchyModel | null {
  return partner.organizationUnitRelationships?.[0]?.organizationHierarchy || null;
}

export interface Office {
  id?: string | null;
  name?: string | null;
  code?: string | null;
  status?: number | null;
  createdBy?: number | null;
  createdDate?: string | null;
  lastModifiedBy?: number | null;
  lastModifiedDate?: string | null;
  isDeleted?: boolean | null;
  deletedBy?: number | null;
  deletedDate?: string | null;
}
