import { PartnerTree } from "./partner-tree.model";
import { EntityPermissionSet } from './shared-types';

export interface Partner {
  id?: string | null;
  partnerNumber?: string | null;
  name?: string | null;
  shortName?: string | null;
  status?: string | null;
  newEngagement?: string | null;
  phone?: string | null;
  website?: string | null;
  pooledFund?: string | null;
  ddRequired?: string | null;
  ddeacDone?: string | null;
  eacReference?: string | null;
  globalKeyAccount?: boolean | null;
  unSecretariatEntity?: boolean | null;
  levyPotentiallyApplies?: string | null;
  reasonForLevyNotApplying?: string | null;
  levyTreatment?: string | null;
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
  isDeleted?: boolean | null;
  partnerOffice?: Office | null;
  partnerOfficeId?: string | null;
  partnerCategory?: string | null;
  logoUrl?: string | null;
  deletedBy?: string | null;
  deletedDate?: Date | null;
  _updated?: boolean;
  _importRowId?: string;
  partnerTree?: PartnerTree | null;
  partnerGroupId?: number | null;
  partnerGroupCode?: string | null;
  partnerGroupName?: string | null;
  partnerCategoryId?: number | null;
  partnerCategoryCode?: string | null;
  partnerCategoryName?: string | null;
  
  // RBAC permissions
  permissions?: EntityPermissionSet;
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
