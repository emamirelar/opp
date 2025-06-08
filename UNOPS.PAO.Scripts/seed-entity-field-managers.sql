-- ================================================================
-- Script: Seed EntityManager and EntityFieldManager tables (PostgreSQL)
-- Description: Creates entity configurations with comprehensive field definitions and enhanced list view settings
-- Date: January 2025 (Enhanced Version - Insert Only)
-- Note: Run this after database migration scripts have been executed
-- ================================================================

-- Clean up existing entity field managers
DELETE FROM public."EntityFieldManagers";
DELETE FROM public."EntityManagers";

-- Reset the sequences to start from 1
ALTER SEQUENCE public."EntityManagers_Id_seq" RESTART WITH 1;
ALTER SEQUENCE public."EntityFieldManagers_Id_seq" RESTART WITH 1;

-- Insert EntityManager records for each entity
INSERT INTO public."EntityManagers" (
    "EntityName", 
    "TableName", 
    "Description", 
    "IsActive", 
    "Name", 
    "Status", 
    "CreatedBy", 
    "CreatedDate",
    "LastModifiedBy",
    "LastModifiedDate",
    "IsDeleted",
    "DeletedBy",
    "DeletedDate"
) VALUES 
    ('Contact', 'Contacts', 'Individual contact persons associated with partners', true, 'Contact', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    ('Partner', 'Partners', 'Organizations and entities that work with UNOPS', true, 'Partner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    ('Interaction', 'Interactions', 'Communication and interaction records between UNOPS and partners/contacts', true, 'Interaction', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    ('PartnerTree', 'PartnerTrees', 'Hierarchical structure and classification of partners', true, 'PartnerTree', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- CONTACT ENTITY FIELDS (25 fields)
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    -- Core Contact Fields (List View)
    (1, 'FirstName', 'string', 'First name of the contact', true, true, NULL, 100, 1, true, 1, NULL, 'firstName', NULL, 'First Name', 'text', '15%', false, true, NULL, 'FirstName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'LastName', 'string', 'Last name of the contact', true, true, NULL, 100, 2, true, 2, NULL, 'lastName', NULL, 'Last Name', 'text', '15%', false, true, NULL, 'LastName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Email', 'string', 'Primary email address', true, true, NULL, 255, 3, true, 3, NULL, 'email', NULL, 'Email', 'text', '20%', true, true, NULL, 'Email', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Partner', 'Partner', 'Associated partner organization', true, true, NULL, NULL, 4, true, 4, 'name', 'partner.name', NULL, 'Partner', 'text', '20%', true, true, NULL, 'Partner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Title', 'string', 'Job title or position', false, true, NULL, 150, 5, true, 5, NULL, 'title', NULL, 'Position', 'text', '15%', true, true, NULL, 'Title', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Phone', 'string', 'Primary phone number', false, true, NULL, 20, 6, true, 6, NULL, 'phone', NULL, 'Phone', 'text', '15%', false, true, NULL, 'Phone', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- Additional Contact Fields (Non-List View)
    (1, 'Id', 'int', 'Unique identifier for the contact', true, true, NULL, NULL, 7, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Salutation', 'string', 'Contact salutation (Mr., Ms., Dr., etc.)', false, true, NULL, 50, 8, false, NULL, NULL, 'salutation', NULL, NULL, 'text', NULL, false, true, NULL, 'Salutation', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MiddleName', 'string', 'Contact middle name', false, true, NULL, 100, 9, false, NULL, NULL, 'middleName', NULL, NULL, 'text', NULL, false, true, NULL, 'MiddleName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Suffix', 'string', 'Contact suffix (Jr., Sr., III, etc.)', false, true, NULL, 50, 10, false, NULL, NULL, 'suffix', NULL, NULL, 'text', NULL, false, true, NULL, 'Suffix', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Department', 'string', 'Contact department', false, true, NULL, 200, 11, false, NULL, NULL, 'department', NULL, NULL, 'text', NULL, true, true, NULL, 'Department', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Description', 'string', 'Contact description or notes', false, true, NULL, 1000, 12, false, NULL, NULL, 'description', NULL, NULL, 'text', NULL, true, true, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Mobile', 'string', 'Contact mobile number', false, true, NULL, 50, 13, false, NULL, NULL, 'mobile', NULL, NULL, 'text', NULL, false, true, NULL, 'Mobile', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Assistant', 'string', 'Assistant name', false, true, NULL, 200, 14, false, NULL, NULL, 'assistant', NULL, NULL, 'text', NULL, false, true, NULL, 'Assistant', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'AssistantPhone', 'string', 'Assistant phone number', false, true, NULL, 50, 15, false, NULL, NULL, 'assistantPhone', NULL, NULL, 'text', NULL, false, true, NULL, 'AssistantPhone', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'AssistantEmail', 'string', 'Assistant email address', false, true, NULL, 200, 16, false, NULL, NULL, 'assistantEmail', NULL, NULL, 'text', NULL, true, true, NULL, 'AssistantEmail', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Status', 'string', 'Contact status', true, true, 'Active', 50, 17, false, NULL, NULL, 'status', NULL, NULL, 'text', NULL, false, true, NULL, 'Status', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingStreet', 'string', 'Mailing address street', false, true, NULL, 300, 18, false, NULL, NULL, 'mailingStreet', NULL, NULL, 'text', NULL, true, true, NULL, 'MailingStreet', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingStreet2', 'string', 'Mailing address street line 2', false, true, NULL, 300, 19, false, NULL, NULL, 'mailingStreet2', NULL, NULL, 'text', NULL, true, true, NULL, 'MailingStreet2', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingCity', 'string', 'Mailing address city', false, true, NULL, 100, 20, false, NULL, NULL, 'mailingCity', NULL, NULL, 'text', NULL, false, true, NULL, 'MailingCity', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingStateProvince', 'string', 'Mailing address state/province', false, true, NULL, 100, 21, false, NULL, NULL, 'mailingStateProvince', NULL, NULL, 'text', NULL, false, true, NULL, 'MailingStateProvince', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingPostalCode', 'string', 'Mailing address postal code', false, true, NULL, 20, 22, false, NULL, NULL, 'mailingPostalCode', NULL, NULL, 'text', NULL, false, true, NULL, 'MailingPostalCode', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingCountry', 'string', 'Mailing address country', false, true, NULL, 100, 23, false, NULL, NULL, 'mailingCountry', NULL, NULL, 'text', NULL, false, true, NULL, 'MailingCountry', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'ProfilePictureUrl', 'string', 'URL to contact profile picture', false, true, NULL, 500, 24, false, NULL, NULL, 'profilePictureUrl', NULL, NULL, 'text', NULL, true, true, NULL, 'ProfilePictureUrl', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'ContactNumber', 'string', 'UNOPS-specific contact number', false, true, NULL, 50, 25, false, NULL, NULL, 'contactNumber', NULL, NULL, 'text', NULL, false, true, NULL, 'ContactNumber', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- PARTNER ENTITY FIELDS (18 fields) - Enhanced with Navigation Properties
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    -- Core Partner Fields (List View)
    (2, 'Name', 'string', 'Partner organization name', true, true, NULL, 300, 1, true, 1, NULL, 'name', NULL, 'Name', 'text', '25%', true, true, NULL, 'Name', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'ShortName', 'string', 'Partner short name or abbreviation', false, true, NULL, 100, 2, true, 2, NULL, 'shortName', NULL, 'Short Name', 'text', '15%', false, true, NULL, 'ShortName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerCode', 'string', 'Unique partner identification code', false, true, NULL, 50, 3, true, 3, NULL, 'partnerCode', NULL, 'Code', 'text', '10%', false, true, NULL, 'PartnerCode', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Status', 'string', 'Partner status (Active, Inactive, etc.)', true, true, 'Active', 50, 4, true, 4, NULL, 'status', NULL, 'Status', 'badge', '12%', false, true, NULL, 'Status', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Website', 'string', 'Partner website URL', false, true, NULL, 500, 5, true, 5, NULL, 'website', NULL, 'Website', 'link', '15%', true, true, NULL, 'Website', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Phone', 'string', 'Partner primary phone number', false, true, NULL, 50, 6, true, 6, NULL, 'phone', NULL, 'Phone', 'text', '12%', false, true, NULL, 'Phone', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'LogoUrl', 'string', 'Partner logo image URL', false, true, NULL, 500, 7, true, 7, NULL, 'logoUrl', NULL, 'Logo', 'avatar', '8%', false, false, 'name', 'LogoUrl', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Address Information
    (2, 'Address1Street', 'string', 'Partner street address', false, true, NULL, 300, 8, false, NULL, NULL, 'address1Street', NULL, NULL, 'text', NULL, true, true, NULL, 'Address1Street', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Address1City', 'string', 'Partner city', false, true, NULL, 100, 9, false, NULL, NULL, 'address1City', NULL, NULL, 'text', NULL, false, true, NULL, 'Address1City', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Address1Country', 'string', 'Partner country', false, true, NULL, 100, 10, false, NULL, NULL, 'address1Country', NULL, NULL, 'text', NULL, false, true, NULL, 'Address1Country', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- Classification Fields
    (2, 'PartnerCategoryName', 'string', 'Partner category classification', false, true, NULL, 200, 11, false, NULL, NULL, 'partnerCategoryName', NULL, NULL, 'text', NULL, true, true, NULL, 'PartnerCategoryName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerGroupName', 'string', 'Partner group classification', false, true, NULL, 200, 12, false, NULL, NULL, 'partnerGroupName', NULL, NULL, 'text', NULL, true, true, NULL, 'PartnerGroupName', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- Navigation Properties and Complex Fields
    (2, 'PartnerOffice', 'OrganizationHierarchy', 'Organization hierarchy for partner office structure', false, true, NULL, NULL, 13, false, NULL, 'name', 'partnerOffice.name', '{partnerOffice.name}', NULL, 'template', NULL, false, true, NULL, 'PartnerOffice', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'First5ContactsByDate', 'Contact[]', 'Collection of first 5 contacts ordered by date', false, true, NULL, NULL, 14, true, 8, NULL, 'first5ContactsByDate', NULL, 'Key Contacts', 'multiple-avatars', '15%', false, false, 'firstName', 'First5ContactsByDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Documents', 'Document[]', 'Collection of partner-related documents', false, true, NULL, NULL, 15, false, NULL, NULL, 'documents', NULL, NULL, 'text', NULL, false, false, NULL, 'Documents', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Projects', 'Project[]', 'Collection of partner-related projects', false, true, NULL, NULL, 16, false, NULL, NULL, 'projects', NULL, NULL, 'text', NULL, false, false, NULL, 'Projects', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- Boolean Flags
    (2, 'GlobalKeyAccount', 'boolean', 'Indicates if partner is a global key account', false, true, 'false', NULL, 17, false, NULL, NULL, 'globalKeyAccount', NULL, NULL, 'boolean', NULL, false, true, NULL, 'GlobalKeyAccount', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'UNSecretariatEntity', 'boolean', 'Indicates if partner is a UN Secretariat entity', false, true, 'false', NULL, 18, false, NULL, NULL, 'unSecretariatEntity', NULL, NULL, 'boolean', NULL, false, true, NULL, 'UNSecretariatEntity', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- INTERACTION ENTITY FIELDS (8 fields)
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    (3, 'Id', 'int', 'Unique identifier for the interaction', true, true, NULL, NULL, 1, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Type', 'enum', 'Type of interaction (Meeting, Email, Call, etc.)', true, true, NULL, NULL, 2, true, 1, NULL, 'type', NULL, 'Type', 'text', '12%', false, true, NULL, 'Type', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Date', 'datetime', 'Date and time of the interaction', true, true, NULL, NULL, 3, true, 2, NULL, 'date', NULL, 'Date', 'text', '15%', false, true, NULL, 'Date', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Subject', 'string', 'Subject or title of the interaction', true, true, NULL, 300, 4, true, 3, NULL, 'subject', NULL, 'Subject', 'text', '25%', true, true, NULL, 'Subject', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Contact', 'Contact', 'Contact entity for managing partner contact information', true, true, NULL, NULL, 5, true, 4, 'firstname,lastname', 'contact.firstName,contact.lastName', '{firstName} {lastName}', 'Contact', 'template', '18%', true, true, NULL, 'Contact', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Description', 'string', 'Detailed description of the interaction', false, true, NULL, 2000, 6, false, NULL, NULL, 'description', NULL, NULL, 'text', NULL, true, true, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Location', 'string', 'Location where interaction took place', false, true, NULL, 200, 7, false, NULL, NULL, 'location', NULL, NULL, 'text', NULL, false, true, NULL, 'Location', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'OrgUnit', 'OrganizationHierarchy', 'Organization hierarchy for managing office structure and geographic locations', false, true, NULL, NULL, 8, false, NULL, 'name', 'orgUnit.name', NULL, NULL, 'text', NULL, false, true, NULL, 'OrgUnit', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- PARTNERTREE ENTITY FIELDS (6 fields)
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    (4, 'Code', 'string', 'Unique code identifier for the partner tree node', true, true, NULL, 50, 1, true, 1, NULL, 'code', NULL, 'Code', 'text', '15%', false, true, NULL, 'Code', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'Description', 'string', 'Description of the partner tree node', true, true, NULL, 500, 2, true, 2, NULL, 'description', NULL, 'Description', 'text', '30%', true, true, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'Type', 'string', 'Type of partner tree node', true, true, NULL, 100, 3, true, 3, NULL, 'type', NULL, 'Type', 'text', '15%', false, true, NULL, 'Type', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'Parent', 'PartnerTree', 'Partner Tree entity for managing partner hierarchy and categories', false, true, NULL, NULL, 4, true, 4, 'code,description', 'partnerTree.code,partnerTree.description', '{code} - {description}', 'Parent', 'template', '25%', true, true, NULL, 'Parent', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'PartnerCategoryCode', 'string', 'Partner category code', false, true, NULL, 50, 5, false, NULL, NULL, 'partnerCategoryCode', NULL, NULL, 'text', NULL, false, true, NULL, 'PartnerCategoryCode', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'PartnerGroupCode', 'string', 'Partner group code', false, true, NULL, 50, 6, false, NULL, NULL, 'partnerGroupCode', NULL, NULL, 'text', NULL, false, true, NULL, 'PartnerGroupCode', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- Select to verify the data
SELECT 'EntityManagers' as TableName, COUNT(*) as RecordCount FROM public."EntityManagers"
UNION ALL
SELECT 'EntityFieldManagers' as TableName, COUNT(*) as RecordCount FROM public."EntityFieldManagers"
ORDER BY TableName;

-- Show summary of list view configurations
SELECT 
    em."EntityName",
    COUNT(efm."Id") as TotalFields,
    COUNT(CASE WHEN efm."ShowInListView" = true THEN 1 END) as ListViewFields
FROM public."EntityManagers" em
JOIN public."EntityFieldManagers" efm ON em."Id" = efm."EntityManagerId"
WHERE em."IsActive" = true AND efm."IsActive" = true
GROUP BY em."EntityName"
ORDER BY em."EntityName";