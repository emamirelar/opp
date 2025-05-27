import { InteractionType } from './interaction-type.enum';
import { EntityPermissionSet } from './shared-types';

export interface Interaction {
  id: number;
  type: InteractionType;
  date: string;
  data?: string;
  contactId: number;
  contactName?: string;
  description?: string;
  status: string;
  contactIds: number[];
  partnerIds: number[];
  userIds: number[];
  emailAddresses: string[];
  phoneNumbers: string[];
  location: string;
  subject: string;
  orgUnitId: number;
  createdBy: number;
  permissions?: EntityPermissionSet;
}
