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
    "EnableChangeLog",
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
    ('Contact', 'Contacts', 'Individual contact persons associated with partners', true, false, 'Contact', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    ('Partner', 'Partners', 'Organizations and entities that work with UNOPS', true, false, 'Partner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    ('Interaction', 'Interactions', 'Communication and interaction records between UNOPS and partners/contacts', true, false, 'Interaction', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    ('PartnerTree', 'PartnerTrees', 'Hierarchical structure and classification of partners', true, false, 'PartnerTree', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    ('OrganizationHierarchy', 'OrganizationHierarchies', 'UNOPS organizational hierarchy and office structure', true, false, 'OrganizationHierarchy', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- CONTACT ENTITY FIELDS (25 fields)
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField", "HelperText",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    -- Primary List View Fields
    (1, 'ProfilePictureUrl', 'string', 'URL to contact profile picture', false, true, NULL, 500, 1, true, 1, NULL, 'profilePictureUrl', NULL, 'Photo', 'avatar', '8%', false, false, 'firstName', 'Contact profile photo', 'ProfilePictureUrl', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Partner', 'Partner', 'Associated partner organization', true, true, NULL, NULL, 2, true, 2, 'name', 'partner.name', NULL, 'Partner', 'text', '20%', true, true, NULL, 'Partner organization', 'Partner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'ContactName', 'string', 'Full contact name combining first, middle, and last names', false, true, NULL, NULL, 3, true, 3, NULL, 'firstName,middleName,lastName', '{firstName} {middleName} {lastName}', 'Contact Name', 'template', '25%', true, true, NULL, 'Complete name of the contact', 'ContactName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Title', 'string', 'Job title or position', false, true, NULL, 150, 4, true, 4, NULL, 'title', NULL, 'Title', 'text', '15%', true, true, NULL, 'Job title or position', 'Title', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Email', 'string', 'Primary email address', true, true, NULL, 255, 5, true, 5, NULL, 'email', NULL, 'Email', 'text', '15%', true, true, NULL, 'Primary email address', 'Email', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Phone', 'string', 'Primary phone number', false, true, NULL, 20, 6, true, 6, NULL, 'phone', NULL, 'Phone', 'text', '17%', false, true, NULL, 'Primary phone number', 'Phone', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- Core Contact Fields (Non-List View)
    (1, 'Id', 'int', 'Unique identifier for the contact', true, true, NULL, NULL, 7, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Salutation', 'string', 'Contact salutation (Mr., Ms., Dr., etc.)', false, true, NULL, 50, 8, false, NULL, NULL, 'salutation', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'Salutation', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'FirstName', 'string', 'First name of the contact', false, true, NULL, 100, 9, false, NULL, NULL, 'firstName', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'FirstName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MiddleName', 'string', 'Contact middle name', false, true, NULL, 100, 10, false, NULL, NULL, 'middleName', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'MiddleName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'LastName', 'string', 'Last name of the contact', true, true, NULL, 100, 11, false, NULL, NULL, 'lastName', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'LastName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Suffix', 'string', 'Contact suffix (Jr., Sr., III, etc.)', false, true, NULL, 50, 12, false, NULL, NULL, 'suffix', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'Suffix', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Department', 'string', 'Contact department', false, true, NULL, 200, 13, false, NULL, NULL, 'department', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'Department', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Description', 'string', 'Contact description or notes', false, true, NULL, 1000, 14, false, NULL, NULL, 'description', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Mobile', 'string', 'Contact mobile number', false, true, NULL, 50, 15, false, NULL, NULL, 'mobile', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'Mobile', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'Assistant', 'string', 'Assistant name', false, true, NULL, 200, 16, false, NULL, NULL, 'assistant', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'Assistant', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'AssistantPhone', 'string', 'Assistant phone number', false, true, NULL, 50, 17, false, NULL, NULL, 'assistantPhone', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'AssistantPhone', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'AssistantEmail', 'string', 'Assistant email address', false, true, NULL, 200, 18, false, NULL, NULL, 'assistantEmail', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'AssistantEmail', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingStreet', 'string', 'Mailing address street', false, true, NULL, 300, 19, false, NULL, NULL, 'mailingStreet', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'MailingStreet', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingStreet2', 'string', 'Mailing address street line 2', false, true, NULL, 300, 20, false, NULL, NULL, 'mailingStreet2', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'MailingStreet2', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingCity', 'string', 'Mailing address city', false, true, NULL, 100, 21, false, NULL, NULL, 'mailingCity', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'MailingCity', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingStateProvince', 'string', 'Mailing address state/province', false, true, NULL, 100, 22, false, NULL, NULL, 'mailingStateProvince', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'MailingStateProvince', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingPostalCode', 'string', 'Mailing address postal code', false, true, NULL, 20, 23, false, NULL, NULL, 'mailingPostalCode', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'MailingPostalCode', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (1, 'MailingCountry', 'string', 'Mailing address country', false, true, NULL, 100, 24, false, NULL, NULL, 'mailingCountry', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'MailingCountry', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Navigation Properties
    (1, 'PartnerId', 'int', 'Foreign key to partner', true, true, NULL, NULL, 25, false, NULL, NULL, 'partnerId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'PartnerId', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- PARTNER ENTITY FIELDS (40 fields) - Complete Partner Entity
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField", "HelperText",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    -- Primary List View Fields
    (2, 'LogoUrl', 'string', 'Partner logo image URL', false, true, NULL, 500, 1, true, 1, NULL, 'logoUrl', NULL, '', 'avatar', '8%', false, false, 'name', 'Partner logo image', 'LogoUrl', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Name', 'string', 'Partner organization name', true, true, NULL, 300, 2, true, 2, NULL, 'name', NULL, 'Name', 'text', '25%', false, true, NULL, 'Partner organization name', 'Name', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerShortDescription', 'string', 'Partner short name or abbreviation', false, true, NULL, 100, 3, true, 3, NULL, 'partnerShortDescription', NULL, 'Short Name', 'text', '15%', true, true, NULL, 'Short name/acronym', 'PartnerShortDescription', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Status', 'enum', 'Partner status (Draft, Active, Closed, Archived)', true, true, 'Draft', 50, 4, true, 4, NULL, 'status', NULL, 'Status', 'badge', '10%', false, true, NULL, 'Partner status', 'Status', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerApprovalStatus', 'enum', 'Partner approval status', false, true, 'NotApproved', NULL, 5, true, 5, NULL, 'partnerApprovalStatus', NULL, 'Approval Status', 'badge', '12%', false, true, NULL, 'Partner approval status', 'PartnerApprovalStatus', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'First5ContactsByDate', 'Contact[]', 'Collection of first 5 contacts ordered by date', false, true, NULL, NULL, 6, true, 6, NULL, 'first5ContactsByDate.profilePictureUrl', NULL, 'Team', 'multiple-avatars', '20%', false, false, 'first5ContactsByDate.firstName', 'Partner team contacts', 'First5ContactsByDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- Core Partner Fields (Non-List View)
    (2, 'Id', 'int', 'Unique identifier for the partner', true, true, NULL, NULL, 7, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerLongDescription', 'string', 'Optional long description', false, true, NULL, 4000, 8, false, NULL, NULL, 'partnerLongDescription', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'PartnerLongDescription', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- System Generated Keys
    (2, 'UniqueKey', 'guid', 'System-generated unique identifier', true, true, NULL, NULL, 9, false, NULL, NULL, 'uniqueKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'UniqueKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerKey', 'guid', 'System-generated partner key', true, true, NULL, NULL, 10, false, NULL, NULL, 'partnerKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'PartnerKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerCategoryInternalKey', 'guid', 'System-generated category internal key', true, true, NULL, NULL, 11, false, NULL, NULL, 'partnerCategoryInternalKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'PartnerCategoryInternalKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerCategoryKey', 'guid', 'System-generated category key', true, true, NULL, NULL, 12, false, NULL, NULL, 'partnerCategoryKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'PartnerCategoryKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerTypeKey', 'guid', 'System-generated type key', true, true, NULL, NULL, 13, false, NULL, NULL, 'partnerTypeKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'PartnerTypeKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Foreign Keys and Relationships
    (2, 'PartnerGroupId', 'int', 'FK to Partner Group', false, true, NULL, NULL, 14, false, NULL, NULL, 'partnerGroupId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'PartnerGroupId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerCategoryId', 'int', 'FK to Partner Category', false, true, NULL, NULL, 15, false, NULL, NULL, 'partnerCategoryId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'PartnerCategoryId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'ErpDimValue', 'int', 'ERP dimension value', false, true, NULL, NULL, 16, false, NULL, NULL, 'erpDimValue', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'ErpDimValue', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'LiaisonOfficeId', 'int', 'FK to LiaisonOffice', false, true, NULL, NULL, 17, false, NULL, NULL, 'liaisonOfficeId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'LiaisonOfficeId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerFocalPointUserId', 'int', 'Business Developer UserId', false, true, NULL, NULL, 18, false, NULL, NULL, 'partnerFocalPointUserId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'PartnerFocalPointUserId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Boolean Fields
    (2, 'UNAndStateEntity', 'boolean', 'UN & State Entity flag', false, true, 'false', NULL, 19, false, NULL, NULL, 'unAndStateEntity', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, 'UNAndStateEntity', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'KeyGlobalPartner', 'boolean', 'Key Global Partner flag (Admin only)', false, true, 'false', NULL, 20, false, NULL, NULL, 'keyGlobalPartner', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, 'KeyGlobalPartner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'UNSecretariatPartner', 'boolean', 'UN Secretariat Partner flag (Admin only)', false, true, 'false', NULL, 21, false, NULL, NULL, 'unSecretariatPartner', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, 'UNSecretariatPartner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PooledFund', 'boolean', 'Pooled Fund flag', false, true, 'false', NULL, 22, false, NULL, NULL, 'pooledFund', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, 'PooledFund', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'CanCreateNewOpportunities', 'boolean', 'Can Create New Opportunities flag', false, true, 'false', NULL, 23, false, NULL, NULL, 'canCreateNewOpportunities', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, 'CanCreateNewOpportunities', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Due Diligence Fields
    (2, 'DueDiligenceRequired', 'enum', 'Due diligence requirement status', false, true, NULL, NULL, 24, false, NULL, NULL, 'dueDiligenceRequired', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'DueDiligenceRequired', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'DueDiligenceApproval', 'enum', 'Due diligence approval status', false, true, NULL, NULL, 25, false, NULL, NULL, 'dueDiligenceApproval', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'DueDiligenceApproval', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'DueDiligenceApprovalDate', 'datetime', 'Due diligence approval date', false, true, NULL, NULL, 26, false, NULL, NULL, 'dueDiligenceApprovalDate', NULL, NULL, 'date', NULL, false, true, NULL, NULL, 'DueDiligenceApprovalDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'DueDiligenceExpiryDate', 'datetime', 'Due diligence expiry date', false, true, NULL, NULL, 27, false, NULL, NULL, 'dueDiligenceExpiryDate', NULL, NULL, 'date', NULL, false, true, NULL, NULL, 'DueDiligenceExpiryDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Partner Approval Fields
    (2, 'PartnerApprovalDate', 'datetime', 'Partner approval date', false, true, NULL, NULL, 28, false, NULL, NULL, 'partnerApprovalDate', NULL, NULL, 'date', NULL, false, true, NULL, NULL, 'PartnerApprovalDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerApprovalReference', 'string', 'Approval notes/reference', false, true, NULL, 500, 29, false, NULL, NULL, 'partnerApprovalReference', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'PartnerApprovalReference', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerApprovedBy', 'string', 'User who approved the partner', false, true, NULL, 500, 30, false, NULL, NULL, 'partnerApprovedBy', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'PartnerApprovedBy', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Levy Fields
    (2, 'PartnerLevyStatus', 'enum', 'Partner levy status', false, true, NULL, NULL, 31, false, NULL, NULL, 'partnerLevyStatus', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'PartnerLevyStatus', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'ReasonForLevy', 'string', 'Reason for levy', false, true, NULL, 500, 32, false, NULL, NULL, 'reasonForLevy', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'ReasonForLevy', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'LevyTreatment', 'string', 'Levy treatment', false, true, NULL, 500, 33, false, NULL, NULL, 'levyTreatment', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'LevyTreatment', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'ReasonForNoNewOpportunity', 'string', 'Reason for no new opportunity', false, true, NULL, 500, 34, false, NULL, NULL, 'reasonForNoNewOpportunity', NULL, NULL, 'text', NULL, true, true, NULL, NULL, 'ReasonForNoNewOpportunity', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- Navigation Properties Collections (read-only for list management)
    (2, 'Documents', 'Document[]', 'Collection of partner-related documents', false, true, NULL, NULL, 35, false, NULL, NULL, 'documents', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'Documents', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Contacts', 'Contact[]', 'Collection of partner contacts', false, true, NULL, NULL, 36, false, NULL, NULL, 'contacts', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'Contacts', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'OrganizationUnitRelationships', 'OrganizationUnitRelationship[]', 'Organization unit relationships', false, true, NULL, NULL, 37, false, NULL, NULL, 'organizationUnitRelationships', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'OrganizationUnitRelationships', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Navigation Properties (Single)
    (2, 'PartnerGroup', 'PartnerTree', 'Partner group relationship', false, true, NULL, NULL, 38, false, NULL, 'name', 'partnerGroup.name', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'PartnerGroup', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'LiaisonOffice', 'LiaisonOffice', 'Liaison office relationship', false, true, NULL, NULL, 39, false, NULL, 'name', 'liaisonOffice.name', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'LiaisonOffice', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'PartnerFocalPointUser', 'PAOUser', 'Partner focal point user', false, true, NULL, NULL, 40, false, NULL, 'name', 'partnerFocalPointUser.name', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'PartnerFocalPointUser', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- INTERACTION ENTITY FIELDS (15 fields)
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField", "HelperText",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    -- Primary List View Fields
    (3, 'Type', 'enum', 'Type of interaction (Meeting, Email, Call, etc.)', true, true, NULL, NULL, 1, true, 1, NULL, 'type', NULL, 'Type', 'badge', '10%', false, true, NULL, 'Interaction type classification', 'Type', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Date', 'datetime', 'Date and time of the interaction', true, true, NULL, NULL, 2, true, 2, NULL, 'date', NULL, 'Date', 'date', '15%', false, true, NULL, 'When the interaction occurred', 'Date', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Subject', 'string', 'Subject or title of the interaction', true, true, NULL, 300, 3, true, 3, NULL, 'subject', NULL, 'Subject', 'text', '35%', true, true, NULL, 'Interaction subject or title', 'Subject', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Description', 'string', 'Detailed description of the interaction', false, true, NULL, 2000, 4, true, 4, NULL, 'description', NULL, 'Description', 'text', '30%', true, false, NULL, 'Detailed interaction description', 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Location', 'string', 'Location where interaction took place', false, true, NULL, 200, 5, true, 5, NULL, 'location', NULL, 'Location', 'text', '10%', true, true, NULL, 'Location where interaction took place', 'Location', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- Core Interaction Fields (Non-List View)
    (3, 'Id', 'int', 'Unique identifier for the interaction', true, true, NULL, NULL, 6, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'EmailAddresses', 'string[]', 'List of email addresses associated with the interaction', false, true, NULL, NULL, 7, false, NULL, NULL, 'emailAddresses', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'EmailAddresses', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'PhoneNumbers', 'string[]', 'List of phone numbers associated with the interaction', false, true, NULL, NULL, 8, false, NULL, NULL, 'phoneNumbers', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'PhoneNumbers', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'GmailThreadId', 'string', 'Gmail thread identifier for email interactions', false, true, NULL, NULL, 9, false, NULL, NULL, 'gmailThreadId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'GmailThreadId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'GmailMessageId', 'string', 'Gmail message identifier for email interactions', false, true, NULL, 80, 10, false, NULL, NULL, 'gmailMessageId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, 'GmailMessageId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Navigation Properties (Many-to-Many Collections)
    (3, 'InteractionContacts', 'InteractionContact[]', 'Many-to-many relationship with contacts', false, true, NULL, NULL, 11, false, NULL, NULL, 'interactionContacts', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'InteractionContacts', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'InteractionPartners', 'InteractionPartner[]', 'Many-to-many relationship with partners', false, true, NULL, NULL, 12, false, NULL, NULL, 'interactionPartners', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'InteractionPartners', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'InteractionUsers', 'InteractionUser[]', 'Many-to-many relationship with users', false, true, NULL, NULL, 13, false, NULL, NULL, 'interactionUsers', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'InteractionUsers', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'OrganizationUnitRelationships', 'OrganizationUnitRelationship[]', 'Many-to-many relationship with organization units', false, true, NULL, NULL, 14, false, NULL, NULL, 'organizationUnitRelationships', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'OrganizationUnitRelationships', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Documents', 'Document[]', 'Collection of interaction-related documents', false, true, NULL, NULL, 15, false, NULL, NULL, 'documents', NULL, NULL, 'text', NULL, false, false, NULL, NULL, 'Documents', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- PARTNERTREE ENTITY FIELDS (8 fields)
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField", "HelperText",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    -- Core PartnerTree Fields (List View) - Note: Actions are hardcoded in HTML template
    (4, 'Name', 'string', 'Name of the partner tree node', true, true, NULL, 300, 1, true, 1, NULL, 'name', NULL, 'Name', 'text', '25%', true, true, NULL, 'The display name for this partner tree level or category', 'Name', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'Description', 'string', 'Description of the partner tree node', true, true, NULL, 500, 2, true, 2, NULL, 'description', NULL, 'Description', 'text', '30%', true, true, NULL, 'Detailed description of what this partner tree level represents', 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'Type', 'string', 'Type/Level of partner tree node (Level_1, Level_2, Level_3)', true, true, NULL, 100, 3, true, 3, NULL, 'type', NULL, 'Level', 'text', '10%', false, true, NULL, 'Hierarchical level in the partner tree structure', 'Type', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'PartnerCategoryName', 'string', 'Partner category display name', false, true, NULL, 200, 4, true, 4, NULL, 'partnerCategoryName', NULL, 'Partner Category', 'text', '20%', true, true, NULL, 'The category this partner tree node belongs to', 'PartnerCategoryName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'PartnerGroupName', 'string', 'Partner group display name', false, true, NULL, 200, 5, true, 5, NULL, 'partnerGroupName', NULL, 'Partner Group', 'text', '20%', true, true, NULL, 'The group classification for this partner tree node', 'PartnerGroupName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    
    -- Additional PartnerTree Fields (Non-List View)
    (4, 'Code', 'string', 'Unique code identifier for the partner tree node', true, true, NULL, 50, 6, false, NULL, NULL, 'code', NULL, NULL, 'text', NULL, false, true, NULL, 'System-generated unique identifier for this partner tree node', 'Code', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'PartnerCategoryCode', 'string', 'Partner category code', false, true, NULL, 50, 7, false, NULL, NULL, 'partnerCategoryCode', NULL, NULL, 'text', NULL, false, true, NULL, 'Internal code for the partner category', 'PartnerCategoryCode', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'PartnerGroupCode', 'string', 'Partner group code', false, true, NULL, 50, 8, false, NULL, NULL, 'partnerGroupCode', NULL, NULL, 'text', NULL, false, true, NULL, 'Internal code for the partner group', 'PartnerGroupCode', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- ================================================================
-- ORGANIZATIONHIERARCHY ENTITY FIELDS (5 fields)
-- ================================================================
INSERT INTO public."EntityFieldManagers" (
    "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField", "HelperText",
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
) VALUES 
    (5, 'Code', 'string', 'Unique organizational code', true, true, NULL, 50, 1, true, 1, NULL, 'code', NULL, 'Code', 'text', '15%', false, true, NULL, NULL, 'Code', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (5, 'Name', 'string', 'Organization unit name', true, true, NULL, 200, 2, true, 2, NULL, 'name', NULL, 'Name', 'text', '25%', false, true, NULL, NULL, 'Name', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (5, 'Type', 'string', 'Type of organizational unit', true, true, NULL, 100, 3, true, 3, NULL, 'type', NULL, 'Type', 'text', '15%', false, true, NULL, NULL, 'Type', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (5, 'Description', 'string', 'Description of the organizational unit', false, true, NULL, 500, 4, true, 4, NULL, 'description', NULL, 'Description', 'text', '25%', true, true, NULL, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (5, 'ParentId', 'int', 'Parent organization hierarchy ID', false, true, NULL, NULL, 5, true, 5, NULL, 'parentId', NULL, 'Parent ID', 'text', '15%', false, true, NULL, NULL, 'ParentId', 0, 1, NOW(), 0, NULL, false, 0, NULL);

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