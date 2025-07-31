import { InteractionType } from './interaction-type.enum';
import { EntityPermissionSet } from './shared-types';
import { OrganizationUnitRelationshipModel } from './organization-unit-relationship.model';

export interface Interaction {
  id: number;
  type: InteractionType;
  date: string;
  description?: string;
  contactId: number;
  contactName?: string;
  status: string;
  contactIds: number[];
  partnerIds: number[];
  userIds: number[];
  emailAddresses: string[];
  phoneNumbers: string[];
  location: string;
  subject: string;
  // Organization Unit Relationships
  organizationUnitRelationships?: OrganizationUnitRelationshipModel[] | null;
  createdBy: number;
  permissions?: EntityPermissionSet;
  
  // Import-specific properties
  isImportEdit?: boolean;
  _updated?: boolean;
  _importRowId?: string;
}
