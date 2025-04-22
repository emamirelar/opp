import {Partner} from './partner.model';

export interface Contact {
  id?: string | null;
  salutation?: string | null;
  firstName?: string | null;
  middleName?: string | null;
  lastName?: string | null;
  suffix?: string | null;
  title?: string | null;
  pronouns?: string | null;
  birthDate?: Date | null;

  email?: string | null;
  phone?: string | null;
  mobile?: string | null;
  otherPhone?: string | null;
  fax?: string | null;

  partner?: Partner | null;
  department?: string | null;
  description?: string | null;
  status?: string | null;
  contactNumber?: string | null;

  assistant?: string | null;
  assistantPhone?: string | null;
  assistantEmail?: string | null;

  mailingStreet?: string | null;
  mailingStreet2?: string | null;
  mailingCity?: string | null;
  mailingStateProvince?: string | null;
  mailingPostalCode?: string | null;
  mailingCountry?: string | null;

  discriminator?: string | null;
  createdBy?: string | null;
  createdDate?: Date | null;
  lastModifiedBy?: string | null;
  lastModifiedDate?: Date | null;
  isDeleted?: boolean | null;
  deletedBy?: string | null;
  deletedDate?: Date | null;
  
  // Import-specific properties
  isImportEdit?: boolean;
  _updated?: boolean;
  _importRowId?: string;
}
