import { EntityPermissionSet } from '@shared/models/shared-types';

export interface PartnerTree {
  id?: number;
  description?: string;
  name?: string;
  code?: string;
  type?: string;
  parent?: string;
  partnerCategoryId?: number;
  partnerCategoryCode?: string;
  partnerCategoryName?: string;
  partnerGroupId?: number;
  partnerGroupCode?: string;
  partnerGroupName?: string;
  status?: string;
  partnerCategoryEditable?: boolean;
  partnerGroupEditable?: boolean;
  permissions?: EntityPermissionSet;
}
