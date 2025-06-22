-- Fix PropertyFilter for GMAIL_GEN_USER Contact role
-- This script corrects the property names to match the actual Contact entity properties

UPDATE public."EntityPermissions" 
SET "PropertyFilter" = '{"CanRead": ["CreatedBy", "LastModifiedBy", "CreatedDate", "LastModifiedDate", "IsDeleted", "Status", "Partner", "PartnerId", "FirstName", "LastName", "MiddleName", "Salutation", "Suffix", "Title", "Description", "Phone", "Mobile", "Department", "Email", "Assistant", "AssistantPhone", "AssistantEmail", "MailingStreet", "MailingStreet2", "MailingCity", "MailingStateProvince", "MailingPostalCode", "MailingCountry", "ProfilePictureUrl"], "CanCreate": [], "CanUpdate": [], "CanDelete": []}'
WHERE "Entity" = 'Contact' 
  AND "Role" = 'GMAIL_GEN_USER';

-- Fix PropertyFilter for GMAIL_GEN_USER Partner role
-- This script corrects the property names to match the actual Partner entity properties

UPDATE public."EntityPermissions" 
SET "PropertyFilter" = '{"CanRead": ["Id", "Name", "PartnerCode", "Phone"], "CanCreate": [], "CanUpdate": [], "CanDelete": []}'
WHERE "Entity" = 'Partner' 
  AND "Role" = 'GMAIL_GEN_USER';