import { InteractionType } from './interaction-type.enum';
import { EntityPermissionSet } from '@shared/models/shared-types';
import { OrganizationUnitRelationshipModel } from '@partnerships/partners/models/organization-unit-relationship.model';
import { Contact } from '@partnerships/contacts/models/contact.model';
import { Partner } from '@partnerships/partners/models/partner.model';
import { UserValueModel } from '@shared/models/user.model';

export interface DocumentModel {
  id: number;
  name: string;
  size?: number;
  type?: string;
  url?: string;
}

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
  emailAddresses: string[];
  location: string;
  subject: string;
  // Organization Unit Relationships
  organizationUnitRelationships?: OrganizationUnitRelationshipModel[] | null;
  createdBy: number;
  permissions?: EntityPermissionSet;
  
  // Documents/Attachments
  documents?: DocumentModel[];
  
  // Full related entities (from backend)
  contacts?: Contact[];
  partners?: Partner[];
  users?: UserValueModel[];
  
  // Gmail integration
  gmailThreadId?: string;
  gmailMessageId?: string;
  
  // Audit fields
  createdDate?: string;
  lastModifiedDate?: string;
  lastModifiedBy?: number;
  
  // Resolved user names for audit fields
  createdByName?: string;
  lastModifiedByName?: string;
  
  // Import-specific properties
  isImportEdit?: boolean;
  _updated?: boolean;
  _importRowId?: string;
}
