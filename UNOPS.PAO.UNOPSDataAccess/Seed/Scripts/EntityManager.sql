-- Entity Managers and Field Managers Complete Configuration
-- This script manages both entity manager definitions and their field configurations

-- Clear existing data and reset
TRUNCATE TABLE public."EntityManagers" CASCADE;

-- Insert Entity Managers
INSERT INTO public."EntityManagers" (
    "Id", "EntityName", "TableName", "Description", "IsActive", "EnableChangeLog", 
    "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", 
    "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
)
VALUES 
    (1, 'Contact', 'Contacts', 'Individual contact persons associated with partners', true, false, 'Contact', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (2, 'Partner', 'Partners', 'Organizations and entities that work with UNOPS', true, false, 'Partner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (3, 'Interaction', 'Interactions', 'Communication and interaction records between UNOPS and partners/contacts', true, false, 'Interaction', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (4, 'PartnerTree', 'PartnerTrees', 'Hierarchical structure and classification of partners', true, false, 'PartnerTree', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (5, 'OrganizationHierarchy', 'OrganizationHierarchies', 'UNOPS organizational hierarchy and office structure', true, false, 'OrganizationHierarchy', 0, 1, NOW(), 0, NULL, false, 0, NULL),
    (6, 'Opportunity', 'Opportunities', 'Funding and partnership opportunities for UNOPS initiatives', true, false, 'Opportunity', 0, 1, NOW(), 0, NULL, false, 0, NULL);

-- EntityFieldManagers configuration seeding
-- This section seeds the UI field configuration for all entities

DO $$
DECLARE
    field_managers_count INTEGER;
BEGIN
    RAISE NOTICE 'EntityFieldManagers table cleared, inserting UI configuration...';

    -- Insert EntityFieldManagers data
    INSERT INTO public."EntityFieldManagers" (
        "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField", "HelperText", "ThumbnailSize", "ThumbnailShape", "ThumbnailBorder", "ThumbnailFallback",
        "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
    ) VALUES 
        -- Primary List View Fields
        (1, 'ProfilePictureUrl', 'string', 'URL to contact profile picture', false, true, NULL, 500, 1, true, 1, NULL, 'profilePictureUrl', NULL, 'Photo', 'avatar', '8%', false, false, 'firstName', 'Contact profile photo', NULL, NULL, NULL, NULL, 'ProfilePictureUrl', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Partner', 'Partner', 'Associated partner organization', true, true, NULL, NULL, 2, true, 2, 'name', 'partner.name', NULL, 'Partner', 'text', '20%', true, true, NULL, 'Partner organization', NULL, NULL, NULL, NULL, 'Partner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'ContactName', 'string', 'Full contact name combining first, middle, and last names', false, true, NULL, NULL, 3, true, 3, NULL, 'firstName,middleName,lastName', '{firstName} {middleName} {lastName}', 'Contact Name', 'template', '25%', true, true, NULL, 'Complete name of the contact', NULL, NULL, NULL, NULL, 'ContactName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Title', 'string', 'Job title or position', false, true, NULL, 150, 4, true, 4, NULL, 'title', NULL, 'Title', 'text', '15%', true, true, NULL, 'Job title or position', NULL, NULL, NULL, NULL, 'Title', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Email', 'string', 'Primary email address', true, true, NULL, 255, 5, true, 5, NULL, 'email', NULL, 'Email', 'text', '15%', true, true, NULL, 'Primary email address', NULL, NULL, NULL, NULL, 'Email', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Phone', 'string', 'Primary phone number', false, true, NULL, 20, 6, true, 6, NULL, 'phone', NULL, 'Phone', 'text', '17%', false, true, NULL, 'Primary phone number', NULL, NULL, NULL, NULL, 'Phone', 0, 1, NOW(), 0, NULL, false, 0, NULL),

        -- Core Contact Fields (Non-List View)
        (1, 'Id', 'int', 'Unique identifier for the contact', true, true, NULL, NULL, 7, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Salutation', 'string', 'Contact salutation (Mr., Ms., Dr., etc.)', false, true, NULL, 50, 8, false, NULL, NULL, 'salutation', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Salutation', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'FirstName', 'string', 'First name of the contact', false, true, NULL, 100, 9, false, NULL, NULL, 'firstName', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'FirstName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'MiddleName', 'string', 'Contact middle name', false, true, NULL, 100, 10, false, NULL, NULL, 'middleName', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'MiddleName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'LastName', 'string', 'Last name of the contact', true, true, NULL, 100, 11, false, NULL, NULL, 'lastName', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'LastName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Suffix', 'string', 'Contact suffix (Jr., Sr., III, etc.)', false, true, NULL, 50, 12, false, NULL, NULL, 'suffix', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Suffix', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Department', 'string', 'Contact department', false, true, NULL, 200, 13, false, NULL, NULL, 'department', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Department', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Description', 'string', 'Contact description or notes', false, true, NULL, 1000, 14, false, NULL, NULL, 'description', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Mobile', 'string', 'Contact mobile number', false, true, NULL, 50, 15, false, NULL, NULL, 'mobile', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Mobile', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'Assistant', 'string', 'Assistant name', false, true, NULL, 200, 16, false, NULL, NULL, 'assistant', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Assistant', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'AssistantPhone', 'string', 'Assistant phone number', false, true, NULL, 50, 17, false, NULL, NULL, 'assistantPhone', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'AssistantPhone', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'AssistantEmail', 'string', 'Assistant email address', false, true, NULL, 200, 18, false, NULL, NULL, 'assistantEmail', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'AssistantEmail', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'MailingStreet', 'string', 'Mailing address street', false, true, NULL, 300, 19, false, NULL, NULL, 'mailingStreet', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'MailingStreet', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'MailingStreet2', 'string', 'Mailing address street line 2', false, true, NULL, 300, 20, false, NULL, NULL, 'mailingStreet2', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'MailingStreet2', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'MailingCity', 'string', 'Mailing address city', false, true, NULL, 100, 21, false, NULL, NULL, 'mailingCity', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'MailingCity', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'MailingStateProvince', 'string', 'Mailing address state/province', false, true, NULL, 100, 22, false, NULL, NULL, 'mailingStateProvince', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'MailingStateProvince', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'MailingPostalCode', 'string', 'Mailing address postal code', false, true, NULL, 20, 23, false, NULL, NULL, 'mailingPostalCode', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'MailingPostalCode', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (1, 'MailingCountry', 'string', 'Mailing address country', false, true, NULL, 100, 24, false, NULL, NULL, 'mailingCountry', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'MailingCountry', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Navigation Properties
        (1, 'PartnerId', 'int', 'Foreign key to partner', true, true, NULL, NULL, 25, false, NULL, NULL, 'partnerId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerId', 0, 1, NOW(), 0, NULL, false, 0, NULL);

END $$;

-- Continue with Partner fields
DO $$
DECLARE
    field_managers_count INTEGER;
BEGIN
    -- ================================================================
    -- PARTNER ENTITY FIELDS (40 fields) - Complete Partner Entity
    -- ================================================================
    INSERT INTO public."EntityFieldManagers" (
        "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField", "HelperText", "ThumbnailSize", "ThumbnailShape", "ThumbnailBorder", "ThumbnailFallback",
        "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
    ) VALUES 
        -- Primary List View Fields
        (2, 'LogoUrl', 'string', 'Partner logo image URL', false, true, NULL, 500, 1, true, 1, NULL, 'logoUrl', NULL, '', 'avatar', '8%', false, false, 'name', 'Partner logo image', NULL, NULL, NULL, NULL, 'LogoUrl', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'Name', 'string', 'Partner organization name', true, true, NULL, 300, 2, true, 2, NULL, 'name', NULL, 'Name', 'text', '25%', false, true, NULL, 'Partner organization name', NULL, NULL, NULL, NULL, 'Name', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerShortDescription', 'string', 'Partner short name or abbreviation', false, true, NULL, 100, 3, true, 3, NULL, 'partnerShortDescription', NULL, 'Short Name', 'text', '15%', true, true, NULL, 'Short name/acronym', NULL, NULL, NULL, NULL, 'PartnerShortDescription', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'Status', 'enum', 'Partner status (Draft, Active, Closed, Archived)', true, true, 'Draft', 50, 4, true, 4, NULL, 'status', NULL, 'Status', 'badge', '10%', false, true, NULL, 'Partner status', NULL, NULL, NULL, NULL, 'Status', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerApprovalStatus', 'enum', 'Partner approval status', false, true, 'NotApproved', NULL, 5, true, 5, NULL, 'partnerApprovalStatus', NULL, 'Approval Status', 'badge', '12%', false, true, NULL, 'Partner approval status', NULL, NULL, NULL, NULL, 'PartnerApprovalStatus', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'First5ContactsByDate', 'Contact[]', 'Collection of first 5 contacts ordered by date', false, true, NULL, NULL, 6, true, 6, NULL, 'first5ContactsByDate.profilePictureUrl', NULL, 'Team', 'multiple-avatars', '20%', false, false, 'first5ContactsByDate.firstName', 'Partner team contacts', NULL, NULL, NULL, NULL, 'First5ContactsByDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerOrgUnit', 'string', 'Partner organization units', false, true, NULL, NULL, 7, true, 7, NULL, 'partnerOrgUnit', NULL, 'Partner Org Unit', 'text', '15%', true, false, NULL, 'Partner organization units', NULL, NULL, NULL, NULL, 'PartnerOrgUnit', 0, 1, NOW(), 0, NULL, false, 0, NULL),

        -- Core Partner Fields (Non-List View)
        (2, 'Id', 'int', 'Unique identifier for the partner', true, true, NULL, NULL, 8, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerLongDescription', 'string', 'Optional long description', false, true, NULL, 4000, 9, false, NULL, NULL, 'partnerLongDescription', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerLongDescription', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- System Generated Keys
        (2, 'UniqueKey', 'guid', 'System-generated unique identifier', true, true, NULL, NULL, 10, false, NULL, NULL, 'uniqueKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'UniqueKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerKey', 'guid', 'System-generated partner key', true, true, NULL, NULL, 11, false, NULL, NULL, 'partnerKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerCategoryInternalKey', 'guid', 'System-generated category internal key', true, true, NULL, NULL, 12, false, NULL, NULL, 'partnerCategoryInternalKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerCategoryInternalKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerCategoryKey', 'guid', 'System-generated category key', true, true, NULL, NULL, 13, false, NULL, NULL, 'partnerCategoryKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerCategoryKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerTypeKey', 'guid', 'System-generated type key', true, true, NULL, NULL, 14, false, NULL, NULL, 'partnerTypeKey', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerTypeKey', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Foreign Keys and Relationships
        (2, 'PartnerGroupId', 'int', 'FK to Partner Group', false, true, NULL, NULL, 15, false, NULL, NULL, 'partnerGroupId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerGroupId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerCategoryId', 'int', 'FK to Partner Category', false, true, NULL, NULL, 16, false, NULL, NULL, 'partnerCategoryId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerCategoryId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'ErpDimValue', 'int', 'ERP dimension value', false, true, NULL, NULL, 17, false, NULL, NULL, 'erpDimValue', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'ErpDimValue', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'LiaisonOfficeId', 'int', 'FK to LiaisonOffice', false, true, NULL, NULL, 18, false, NULL, NULL, 'liaisonOfficeId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'LiaisonOfficeId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerFocalPointUserId', 'int', 'Business Developer UserId', false, true, NULL, NULL, 19, false, NULL, NULL, 'partnerFocalPointUserId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerFocalPointUserId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Boolean Fields
        (2, 'UNAndStateEntity', 'boolean', 'UN & State Entity flag', false, true, 'false', NULL, 19, false, NULL, NULL, 'unAndStateEntity', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'UNAndStateEntity', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'KeyGlobalPartner', 'boolean', 'Key Global Partner flag (Admin only)', false, true, 'false', NULL, 20, false, NULL, NULL, 'keyGlobalPartner', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'KeyGlobalPartner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'UNSecretariatPartner', 'boolean', 'UN Secretariat Partner flag (Admin only)', false, true, 'false', NULL, 21, false, NULL, NULL, 'unSecretariatPartner', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'UNSecretariatPartner', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PooledFund', 'boolean', 'Pooled Fund flag', false, true, 'false', NULL, 22, false, NULL, NULL, 'pooledFund', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PooledFund', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'CanCreateNewOpportunities', 'boolean', 'Can Create New Opportunities flag', false, true, 'false', NULL, 23, false, NULL, NULL, 'canCreateNewOpportunities', NULL, NULL, 'boolean', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'CanCreateNewOpportunities', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Due Diligence Fields
        (2, 'DueDiligenceRequired', 'enum', 'Due diligence requirement status', false, true, NULL, NULL, 24, false, NULL, NULL, 'dueDiligenceRequired', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'DueDiligenceRequired', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'DueDiligenceApproval', 'enum', 'Due diligence approval status', false, true, NULL, NULL, 25, false, NULL, NULL, 'dueDiligenceApproval', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'DueDiligenceApproval', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'DueDiligenceApprovalDate', 'datetime', 'Due diligence approval date', false, true, NULL, NULL, 26, false, NULL, NULL, 'dueDiligenceApprovalDate', NULL, NULL, 'date', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'DueDiligenceApprovalDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'DueDiligenceExpiryDate', 'datetime', 'Due diligence expiry date', false, true, NULL, NULL, 27, false, NULL, NULL, 'dueDiligenceExpiryDate', NULL, NULL, 'date', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'DueDiligenceExpiryDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Partner Approval Fields
        (2, 'PartnerApprovalDate', 'datetime', 'Partner approval date', false, true, NULL, NULL, 28, false, NULL, NULL, 'partnerApprovalDate', NULL, NULL, 'date', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerApprovalDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerApprovalReference', 'string', 'Approval notes/reference', false, true, NULL, 500, 29, false, NULL, NULL, 'partnerApprovalReference', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerApprovalReference', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerApprovedBy', 'string', 'User who approved the partner', false, true, NULL, 500, 30, false, NULL, NULL, 'partnerApprovedBy', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerApprovedBy', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Levy Fields
        (2, 'PartnerLevyStatus', 'enum', 'Partner levy status', false, true, NULL, NULL, 31, false, NULL, NULL, 'partnerLevyStatus', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerLevyStatus', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'ReasonForLevy', 'string', 'Reason for levy', false, true, NULL, 500, 32, false, NULL, NULL, 'reasonForLevy', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'ReasonForLevy', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'LevyTreatment', 'string', 'Levy treatment', false, true, NULL, 500, 33, false, NULL, NULL, 'levyTreatment', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'LevyTreatment', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'ReasonForNoNewOpportunity', 'string', 'Reason for no new opportunity', false, true, NULL, 500, 34, false, NULL, NULL, 'reasonForNoNewOpportunity', NULL, NULL, 'text', NULL, true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'ReasonForNoNewOpportunity', 0, 1, NOW(), 0, NULL, false, 0, NULL),

        -- Navigation Properties Collections (read-only for list management)
        (2, 'Documents', 'Document[]', 'Collection of partner-related documents', false, true, NULL, NULL, 35, false, NULL, NULL, 'documents', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'Documents', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'Contacts', 'Contact[]', 'Collection of partner contacts', false, true, NULL, NULL, 36, false, NULL, NULL, 'contacts', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'Contacts', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'OrganizationUnitRelationships', 'OrganizationUnitRelationship[]', 'Organization unit relationships', false, true, NULL, NULL, 37, false, NULL, NULL, 'organizationUnitRelationships', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'OrganizationUnitRelationships', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Navigation Properties (Single)
        (2, 'PartnerGroup', 'PartnerTree', 'Partner group relationship', false, true, NULL, NULL, 38, false, NULL, 'name', 'partnerGroup.name', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerGroup', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'LiaisonOffice', 'LiaisonOffice', 'Liaison office relationship', false, true, NULL, NULL, 39, false, NULL, 'name', 'liaisonOffice.name', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'LiaisonOffice', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (2, 'PartnerFocalPointUser', 'PAOUser', 'Partner focal point user', false, true, NULL, NULL, 40, false, NULL, 'name', 'partnerFocalPointUser.name', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'PartnerFocalPointUser', 0, 1, NOW(), 0, NULL, false, 0, NULL);

END $$;

-- Continue with remaining entities
DO $$
DECLARE
    field_managers_count INTEGER;
BEGIN
    -- ================================================================
    -- INTERACTION ENTITY FIELDS (15 fields)
    -- ================================================================
    INSERT INTO public."EntityFieldManagers" (
        "EntityManagerId", "FieldName", "DataType", "Description", "IsRequired", "IsActive", "DefaultValue", "MaxLength", "DisplayOrder", "ShowInListView", "ListViewOrder", "RelatedDisplayProperty", "DisplayFieldPath", "DisplayTemplate", "ListViewLabel", "ListViewType", "ListViewWidth", "ListViewEllipsis", "ListViewSortable", "FirstLetterFallbackField", "HelperText", "ThumbnailSize", "ThumbnailShape", "ThumbnailBorder", "ThumbnailFallback",
        "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate"
    ) VALUES 
        -- Primary List View Fields
        (3, 'Type', 'enum', 'Type of interaction (Meeting, Email, Call, etc.)', true, true, NULL, NULL, 1, true, 1, NULL, 'type', NULL, 'Type', 'interactionIcon', '10%', false, true, NULL, 'Interaction type classification', NULL, NULL, NULL, NULL, 'Type', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'Date', 'datetime', 'Date and time of the interaction', true, true, NULL, NULL, 2, true, 2, NULL, 'date', NULL, 'Date', 'date', '15%', false, true, NULL, 'When the interaction occurred', NULL, NULL, NULL, NULL, 'Date', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'Subject', 'string', 'Subject or title of the interaction', true, true, NULL, 300, 3, true, 3, NULL, 'subject', NULL, 'Subject', 'text', '35%', true, true, NULL, 'Interaction subject or title', NULL, NULL, NULL, NULL, 'Subject', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'Description', 'string', 'Detailed description of the interaction', false, true, NULL, 2000, 4, true, 4, NULL, 'description', NULL, 'Description', 'text', '30%', true, false, NULL, 'Detailed interaction description', NULL, NULL, NULL, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'Location', 'string', 'Location where interaction took place', false, true, NULL, 200, 5, true, 5, NULL, 'location', NULL, 'Location', 'text', '10%', true, true, NULL, 'Location where interaction took place', NULL, NULL, NULL, NULL, 'Location', 0, 1, NOW(), 0, NULL, false, 0, NULL),

        -- Core Interaction Fields (Non-List View)
        (3, 'Id', 'int', 'Unique identifier for the interaction', true, true, NULL, NULL, 6, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'EmailAddresses', 'string[]', 'List of email addresses associated with the interaction', false, true, NULL, NULL, 7, false, NULL, NULL, 'emailAddresses', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'EmailAddresses', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'PhoneNumbers', 'string[]', 'List of phone numbers associated with the interaction', false, true, NULL, NULL, 8, false, NULL, NULL, 'phoneNumbers', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'PhoneNumbers', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'GmailThreadId', 'string', 'Gmail thread identifier for email interactions', false, true, NULL, NULL, 9, false, NULL, NULL, 'gmailThreadId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'GmailThreadId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'GmailMessageId', 'string', 'Gmail message identifier for email interactions', false, true, NULL, 80, 10, false, NULL, NULL, 'gmailMessageId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'GmailMessageId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Computed Properties (NotMapped)
        (3, 'InteractionContactsList', 'string', 'Comma-separated list of interaction contacts', false, true, NULL, NULL, 11, true, 6, NULL, 'interactionContactsList', NULL, 'Contacts', 'text', '15%', true, false, NULL, 'Associated contacts', NULL, NULL, NULL, NULL, 'InteractionContactsList', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'InteractionPartnersList', 'string', 'Comma-separated list of interaction partners', false, true, NULL, NULL, 12, true, 7, NULL, 'interactionPartnersList', NULL, 'Partners', 'text', '15%', true, false, NULL, 'Associated partners', NULL, NULL, NULL, NULL, 'InteractionPartnersList', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'InteractionUsersList', 'string', 'Comma-separated list of interaction users', false, true, NULL, NULL, 13, true, 8, NULL, 'interactionUsersList', NULL, 'Users', 'text', '10%', true, false, NULL, 'Associated users', NULL, NULL, NULL, NULL, 'InteractionUsersList', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'InteractionOrgUnits', 'string', 'Comma-separated list of interaction organization units', false, true, NULL, NULL, 14, true, 9, NULL, 'interactionOrgUnits', NULL, 'Org Units', 'text', '15%', true, false, NULL, 'Associated organization units', NULL, NULL, NULL, NULL, 'InteractionOrgUnits', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Navigation Properties (Many-to-Many Collections)
        (3, 'InteractionContacts', 'InteractionContact[]', 'Many-to-many relationship with contacts', false, true, NULL, NULL, 15, false, NULL, NULL, 'interactionContacts', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'InteractionContacts', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'InteractionPartners', 'InteractionPartner[]', 'Many-to-many relationship with partners', false, true, NULL, NULL, 16, false, NULL, NULL, 'interactionPartners', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'InteractionPartners', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'InteractionUsers', 'InteractionUser[]', 'Many-to-many relationship with users', false, true, NULL, NULL, 17, false, NULL, NULL, 'interactionUsers', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'InteractionUsers', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'OrganizationUnitRelationships', 'OrganizationUnitRelationship[]', 'Many-to-many relationship with organization units', false, true, NULL, NULL, 18, false, NULL, NULL, 'organizationUnitRelationships', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'OrganizationUnitRelationships', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (3, 'Documents', 'Document[]', 'Collection of interaction-related documents', false, true, NULL, NULL, 19, false, NULL, NULL, 'documents', NULL, NULL, 'text', NULL, false, false, NULL, NULL, NULL, NULL, NULL, NULL, 'Documents', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- ================================================================
    -- PARTNERTREE ENTITY FIELDS (8 fields)
    -- ================================================================
        -- Core PartnerTree Fields (List View) - Note: Actions are hardcoded in HTML template
        (4, 'Name', 'string', 'Name of the partner tree node', true, true, NULL, 300, 1, true, 1, NULL, 'name', NULL, 'Name', 'text', '25%', true, true, NULL, 'The display name for this partner tree level or category', NULL, NULL, NULL, NULL, 'Name', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (4, 'Description', 'string', 'Description of the partner tree node', true, true, NULL, 500, 2, true, 2, NULL, 'description', NULL, 'Description', 'text', '30%', true, true, NULL, 'Detailed description of what this partner tree level represents', NULL, NULL, NULL, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (4, 'Type', 'string', 'Type/Level of partner tree node (Level_1, Level_2, Level_3)', true, true, NULL, 100, 3, true, 3, NULL, 'type', NULL, 'Level', 'text', '10%', false, true, NULL, 'Hierarchical level in the partner tree structure', NULL, NULL, NULL, NULL, 'Type', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (4, 'PartnerCategoryName', 'string', 'Partner category display name', false, true, NULL, 200, 4, true, 4, NULL, 'partnerCategoryName', NULL, 'Partner Category', 'text', '20%', true, true, NULL, 'The category this partner tree node belongs to', NULL, NULL, NULL, NULL, 'PartnerCategoryName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (4, 'PartnerGroupName', 'string', 'Partner group display name', false, true, NULL, 200, 5, true, 5, NULL, 'partnerGroupName', NULL, 'Partner Group', 'text', '20%', true, true, NULL, 'The group classification for this partner tree node', NULL, NULL, NULL, NULL, 'PartnerGroupName', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Additional PartnerTree Fields (Non-List View)
        (4, 'Code', 'string', 'Unique code identifier for the partner tree node', true, true, NULL, 50, 6, false, NULL, NULL, 'code', NULL, NULL, 'text', NULL, false, true, NULL, 'System-generated unique identifier for this partner tree node', NULL, NULL, NULL, NULL, 'Code', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (4, 'PartnerCategoryCode', 'string', 'Partner category code', false, true, NULL, 50, 7, false, NULL, NULL, 'partnerCategoryCode', NULL, NULL, 'text', NULL, false, true, NULL, 'Internal code for the partner category', NULL, NULL, NULL, NULL, 'PartnerCategoryCode', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (4, 'PartnerGroupCode', 'string', 'Partner group code', false, true, NULL, 50, 8, false, NULL, NULL, 'partnerGroupCode', NULL, NULL, 'text', NULL, false, true, NULL, 'Internal code for the partner group', NULL, NULL, NULL, NULL, 'PartnerGroupCode', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- ================================================================
    -- ORGANIZATIONHIERARCHY ENTITY FIELDS (5 fields)
    -- ================================================================
        (5, 'Code', 'string', 'Unique organizational code', true, true, NULL, 50, 1, true, 1, NULL, 'code', NULL, 'Code', 'text', '15%', false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Code', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (5, 'Name', 'string', 'Organization unit name', true, true, NULL, 200, 2, true, 2, NULL, 'name', NULL, 'Name', 'text', '25%', false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Name', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (5, 'Type', 'string', 'Type of organizational unit', true, true, NULL, 100, 3, true, 3, NULL, 'type', NULL, 'Type', 'text', '15%', false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Type', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (5, 'Description', 'string', 'Description of the organizational unit', false, true, NULL, 500, 4, true, 4, NULL, 'description', NULL, 'Description', 'text', '25%', true, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (5, 'ParentId', 'int', 'Parent organization hierarchy ID', false, true, NULL, NULL, 5, true, 5, NULL, 'parentId', NULL, 'Parent ID', 'text', '15%', false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'ParentId', 0, 1, NOW(), 0, NULL, false, 0, NULL),

    -- ================================================================
    -- OPPORTUNITY ENTITY FIELDS (45+ fields)
    -- ================================================================
        -- Primary List View Fields
        (6, 'OpportunityThumbnail', 'string', 'AI-generated thumbnail image (base64 encoded)', false, true, NULL, NULL, 1, true, 1, NULL, 'opportunityThumbnail', NULL, 'Logo', 'thumbnail', '8%', false, false, 'name', 'AI-generated thumbnail logo for the opportunity', '80px', 'rounded-lg', true, NULL, 'OpportunityThumbnail', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'Name', 'string', 'Opportunity name', true, true, NULL, 500, 2, true, 2, NULL, 'name', NULL, 'Name', 'text', '30%', true, true, NULL, 'Opportunity name', NULL, NULL, NULL, NULL, 'Name', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'Status', 'enum', 'Opportunity status', true, true, 'Draft', 50, 3, true, 3, NULL, 'status', NULL, 'Status', 'badge', '10%', false, true, NULL, 'Current status of the opportunity', NULL, NULL, NULL, NULL, 'Status', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'Stage', 'string', 'Current workflow stage', false, true, NULL, NULL, 4, true, 4, NULL, 'stage', NULL, 'Stage', 'text', '15%', false, true, NULL, 'Current stage in the workflow', NULL, NULL, NULL, NULL, 'Stage', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'InitiativeBudgetUSD', 'decimal', 'Initiative budget in USD', false, true, NULL, NULL, 5, true, 5, NULL, 'initiativeBudgetUSD', NULL, 'Budget (USD)', 'currency', '12%', false, true, NULL, 'Total initiative budget in USD', NULL, NULL, NULL, NULL, 'InitiativeBudgetUSD', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ResponsibleOrgUnit', 'OrganizationHierarchy', 'Responsible organization unit', false, true, NULL, NULL, 6, true, 6, 'name', 'responsibleOrgUnit.name', NULL, 'Org Unit', 'text', '25%', true, true, NULL, 'Responsible organizational unit', NULL, NULL, NULL, NULL, 'ResponsibleOrgUnit', 0, 1, NOW(), 0, NULL, false, 0, NULL),

        -- Core Opportunity Fields (Non-List View)
        (6, 'Id', 'int', 'Unique identifier for the opportunity', true, true, NULL, NULL, 8, false, NULL, NULL, 'id', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Id', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'Description', 'string', 'Opportunity description', true, true, NULL, 2000, 9, false, NULL, NULL, 'description', NULL, NULL, 'text', NULL, true, false, NULL, 'Brief description of the opportunity', NULL, NULL, NULL, NULL, 'Description', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'PartnerReference', 'string', 'Partner reference number', false, true, NULL, 255, 10, false, NULL, NULL, 'partnerReference', NULL, NULL, 'text', NULL, false, true, NULL, 'Partner reference identifier', NULL, NULL, NULL, NULL, 'PartnerReference', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'PartnershipAgreementReference', 'string', 'Partnership agreement reference', false, true, NULL, 255, 11, false, NULL, NULL, 'partnershipAgreementReference', NULL, NULL, 'text', NULL, false, true, NULL, 'Reference to partnership agreement', NULL, NULL, NULL, NULL, 'PartnershipAgreementReference', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Image Fields
        (6, 'OpportunityBannerImage', 'string', 'AI-generated banner image (base64 encoded)', false, true, NULL, NULL, 12, false, NULL, NULL, 'opportunityBannerImage', NULL, NULL, 'text', NULL, false, false, NULL, 'AI-generated banner image for the opportunity', NULL, NULL, NULL, NULL, 'OpportunityBannerImage', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'OpportunityStatementMarkdown', 'string', 'AI-generated opportunity statement in markdown format', false, true, NULL, NULL, 13, false, NULL, NULL, 'opportunityStatementMarkdown', NULL, NULL, 'text', NULL, true, false, NULL, 'AI-generated markdown opportunity statement', NULL, NULL, NULL, NULL, 'OpportunityStatementMarkdown', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Date Fields
        (6, 'TargetSigningDate', 'datetime', 'Target signing date', false, true, NULL, NULL, 14, false, NULL, NULL, 'targetSigningDate', NULL, NULL, 'date', NULL, false, true, NULL, 'Expected date for agreement signing', NULL, NULL, NULL, NULL, 'TargetSigningDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ImplementationStartDate', 'datetime', 'Implementation start date', false, true, NULL, NULL, 15, false, NULL, NULL, 'implementationStartDate', NULL, NULL, 'date', NULL, false, true, NULL, 'Implementation start date - defaults to TargetSigningDate if not specified', NULL, NULL, NULL, NULL, 'ImplementationStartDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'TargetDeliveryDate', 'datetime', 'Target delivery date', false, true, NULL, NULL, 16, false, NULL, NULL, 'targetDeliveryDate', NULL, NULL, 'date', NULL, false, true, NULL, 'Expected delivery date', NULL, NULL, NULL, NULL, 'TargetDeliveryDate', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Strategic Alignment Fields
        (6, 'StrategicAlignment', 'string', 'Strategic alignment description', false, true, NULL, 4000, 17, false, NULL, NULL, 'strategicAlignment', NULL, NULL, 'text', NULL, true, false, NULL, 'How opportunity aligns with strategic objectives', NULL, NULL, NULL, NULL, 'StrategicAlignment', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ResultsFocus', 'string', 'Results focus description', false, true, NULL, 4000, 18, false, NULL, NULL, 'resultsFocus', NULL, NULL, 'text', NULL, true, false, NULL, 'Focus on expected results', NULL, NULL, NULL, NULL, 'ResultsFocus', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ExpectedImpact', 'string', 'Expected impact description', false, true, NULL, 200, 19, false, NULL, NULL, 'expectedImpact', NULL, NULL, 'text', NULL, true, false, NULL, 'Expected impact of the opportunity', NULL, NULL, NULL, NULL, 'ExpectedImpact', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ExpectedOutcomes', 'string', 'Expected outcomes description', false, true, NULL, 200, 20, false, NULL, NULL, 'expectedOutcomes', NULL, NULL, 'text', NULL, true, false, NULL, 'Expected outcomes of the opportunity', NULL, NULL, NULL, NULL, 'ExpectedOutcomes', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ExpectedBeneficiaries', 'string', 'Expected beneficiaries', false, true, NULL, 4000, 21, false, NULL, NULL, 'expectedBeneficiaries', NULL, NULL, 'text', NULL, true, false, NULL, 'Who will benefit from this opportunity', NULL, NULL, NULL, NULL, 'ExpectedBeneficiaries', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'Challenges', 'string', 'Challenges description', false, true, NULL, 4000, 22, false, NULL, NULL, 'challenges', NULL, NULL, 'text', NULL, true, false, NULL, 'Anticipated challenges', NULL, NULL, NULL, NULL, 'Challenges', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Boolean Flags
        (6, 'IsPooledFunding', 'boolean', 'Whether funding is pooled across multiple partners', false, true, 'false', NULL, 23, false, NULL, NULL, 'isPooledFunding', NULL, NULL, 'boolean', NULL, false, true, NULL, 'Pooled funding indicator', NULL, NULL, NULL, NULL, 'IsPooledFunding', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Enum Fields
        (6, 'DeliveryModality', 'enum', 'How UNOPS will deliver products and services', false, true, NULL, NULL, 24, false, NULL, NULL, 'deliveryModality', NULL, NULL, 'text', NULL, false, true, NULL, 'Delivery modality classification', NULL, NULL, NULL, NULL, 'DeliveryModality', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Text Fields
        (6, 'MiscExternalStakeholders', 'string', 'Free-text list of external stakeholders not in contact list', false, true, NULL, 2000, 25, false, NULL, NULL, 'miscExternalStakeholders', NULL, NULL, 'text', NULL, true, false, NULL, 'External stakeholders not in system', NULL, NULL, NULL, NULL, 'MiscExternalStakeholders', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ExternalStakeholderNotes', 'string', 'Notes about external stakeholders', false, true, NULL, 2000, 25, false, NULL, NULL, 'externalStakeholderNotes', NULL, NULL, 'text', NULL, true, false, NULL, 'Additional notes about external stakeholders (influence, capacity, role)', NULL, NULL, NULL, NULL, 'ExternalStakeholderNotes', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Navigation Properties - Foreign Keys
        (6, 'Stage', 'string', 'Workflow stage value', false, true, NULL, NULL, 26, false, NULL, NULL, 'stage', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'Stage', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ResponsibleOrgUnitId', 'int', 'FK to OrganizationHierarchy', false, true, NULL, NULL, 27, false, NULL, NULL, 'responsibleOrgUnitId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'ResponsibleOrgUnitId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ProposedInitiativeTypeId', 'int', 'FK to ProposedInitiativeType', false, true, NULL, NULL, 28, false, NULL, NULL, 'proposedInitiativeTypeId', NULL, NULL, 'text', NULL, false, true, NULL, NULL, NULL, NULL, NULL, NULL, 'ProposedInitiativeTypeId', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Navigation Properties - Objects
        (6, 'ProposedInitiativeType', 'ProposedInitiativeType', 'Type of proposed initiative', false, true, NULL, NULL, 29, false, NULL, 'name', 'proposedInitiativeType.name', NULL, NULL, 'text', NULL, false, false, NULL, 'Type of initiative (Project, Programme, Portfolio)', NULL, NULL, NULL, NULL, 'ProposedInitiativeType', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        
        -- Navigation Properties - Collections
        (6, 'FundingPartners', 'OpportunityFundingPartner[]', 'Collection of funding partner relationships', false, true, NULL, NULL, 30, false, NULL, NULL, 'fundingPartners', NULL, NULL, 'text', NULL, false, false, NULL, 'Funding partners for the opportunity', NULL, NULL, NULL, NULL, 'FundingPartners', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ClientPartners', 'OpportunityClientPartner[]', 'Collection of client partner relationships', false, true, NULL, NULL, 31, false, NULL, NULL, 'clientPartners', NULL, NULL, 'text', NULL, false, false, NULL, 'Client partners for the opportunity', NULL, NULL, NULL, NULL, 'ClientPartners', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'Stakeholders', 'OpportunityStakeholder[]', 'Collection of internal stakeholder relationships', false, true, NULL, NULL, 32, false, NULL, NULL, 'stakeholders', NULL, NULL, 'text', NULL, false, false, NULL, 'Internal UNOPS stakeholders', NULL, NULL, NULL, NULL, 'Stakeholders', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'ExternalStakeholders', 'OpportunityExternalStakeholder[]', 'Collection of external stakeholder relationships', false, true, NULL, NULL, 33, false, NULL, NULL, 'externalStakeholders', NULL, NULL, 'text', NULL, false, false, NULL, 'External stakeholders (from contact list)', NULL, NULL, NULL, NULL, 'ExternalStakeholders', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'Deliverables', 'OpportunityDeliverable[]', 'Collection of opportunity deliverables', false, true, NULL, NULL, 34, false, NULL, NULL, 'deliverables', NULL, NULL, 'text', NULL, false, false, NULL, 'Deliverables for the opportunity', NULL, NULL, NULL, NULL, 'Deliverables', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'Countries', 'OpportunityCountry[]', 'Collection of countries where opportunity is active', false, true, NULL, NULL, 35, false, NULL, NULL, 'countries', NULL, NULL, 'text', NULL, false, false, NULL, 'Geographic countries for the opportunity', NULL, NULL, NULL, NULL, 'Countries', 0, 1, NOW(), 0, NULL, false, 0, NULL),
        (6, 'SDGs', 'OpportunitySDG[]', 'Collection of related Sustainable Development Goals', false, true, NULL, NULL, 36, false, NULL, NULL, 'sdGs', NULL, NULL, 'text', NULL, false, false, NULL, 'SDGs addressed by the opportunity', NULL, NULL, NULL, NULL, 'SDGs', 0, 1, NOW(), 0, NULL, false, 0, NULL);

    SELECT COUNT(*) INTO field_managers_count FROM public."EntityFieldManagers";
    RAISE NOTICE 'EntityFieldManagers setup complete with % total records', field_managers_count;

END $$;