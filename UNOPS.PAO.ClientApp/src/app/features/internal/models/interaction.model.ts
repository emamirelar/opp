import { InteractionType } from './interaction-type.enum';
import { EntityPermissionSet } from './shared-types';
import { OrganizationUnitRelationshipModel } from './organization-unit-relationship.model';
import { Contact } from './contact.model';
import { Partner } from './partner.model';

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
  userIds: number[];
  emailAddresses: string[];
  phoneNumbers: string[];
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
  users?: any[];
  
  // Gmail integration
  gmailThreadId?: string;
  gmailMessageId?: string;
  
  // Audit fields
  createdDate?: string;
  lastModifiedDate?: string;
  lastModifiedBy?: number;
  
  // Import-specific properties
  isImportEdit?: boolean;
  _updated?: boolean;
  _importRowId?: string;
}
