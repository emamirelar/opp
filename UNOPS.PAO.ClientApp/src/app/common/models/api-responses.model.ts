import { Contact } from '../../features/internal/models/contact.model';
import { Partner } from '../../features/internal/models/partner.model';
import { Interaction } from '../../features/internal/models/interaction.model';

export interface DropdownOption {
  id: string | number;
  name: string;
}

export interface UserData {
  id?: string | number;
  name?: string;
  email?: string;
  displayName?: string;
  firstName?: string;
  lastName?: string;
  roles?: string[];
}

export interface DuplicateMatch {
  id: number;
  matchScore: number;
  matchFields: string[];
  entity: Contact | Partner | Interaction | any;
}

export interface DuplicateDetectionResponse {
  success?: boolean;
  hasDuplicates?: boolean;
  duplicates?: DuplicateMatch[];
  message?: string;
  confirmationRequired?: boolean;
  action?: string;
  data?: any;
}

export interface ApprovalRequest {
  id: string;
  partnerApprovalReference?: string;
  dueDiligenceApproval?: string;
  partnerApprovalStatus?: string;
  dueDiligenceApprovalDate?: Date | string | null;
  dueDiligenceExpiryDate?: Date | string | null;
  partnerApprovalDate?: Date | string | null;
}

export interface ContactQueryParams {
  page?: number;
  pageSize?: number;
  searchText?: string;
  sortField?: string;
  sortOrder?: 'asc' | 'desc';
  [key: string]: any;
}

export interface PartnerContactsResponse {
  records: Contact[];
  totalCount: number;
}
