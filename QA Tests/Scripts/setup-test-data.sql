-- =====================================================
-- Setup Test Data for Playwright E2E Tests  
-- =====================================================
-- Creates minimal test records with ID 1 for each entity
-- Uses only required fields and avoids problematic FKs
-- =====================================================

SET search_path TO public;

-- =====================================================
-- 1. Create Test Partner (ID 1)
-- =====================================================
INSERT INTO "Partners" (
    "Id", "Name", "Discriminator", "Status",
    "UNAndStateEntity", "UNSecretariatPartner",
    "CanCreateNewOpportunities", "KeyGlobalPartner",
    "PartnerApprovalStatus", "PartnerLevyStatus",
    "LiaisonOfficeId", "ErpDimValue", "PartnerGroupId", "PartnerFocalPointUserId",
    "PartnerCategoryInternalKey", "PartnerCategoryKey",
    "PartnerKey", "PartnerTypeKey", "UniqueKey",
    "PartnerShortDescription",
    "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate",
    "IsDeleted", "DeletedBy", "WorkflowStatus"
)
VALUES (
    1,
    'Test Partner Organization',
    'Organization',
    1, -- Active
    false,
    false,
    true,
    false,
    1, -- Approved
    0,
    NULL, -- LiaisonOfficeId (nullable)
    NULL, -- ErpDimValue (nullable, unique)
    NULL, -- PartnerGroupId (nullable)
    NULL, -- PartnerFocalPointUserId (nullable)
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    gen_random_uuid(),
    '00000000-0000-0000-0000-000000000000'::uuid,
    gen_random_uuid(),
    'Test partner',
    1,
    NOW(),
    1,
    NOW(),
    false,
    0,
    1
)
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- 2. Create Test Contact (ID 1)
-- =====================================================
INSERT INTO "Contacts" (
    "Id", "Name", "FirstName", "LastName", "Email", "Title",
    "Status", "PartnerId", "Discriminator",
    "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate",
    "IsDeleted", "DeletedBy", "WorkflowStatus"
)
VALUES (
    1,
    'John Doe',
    'John',
    'Doe',
    'john.doe@playwright.test',
    'Test Manager',
    1, -- Active
    1, -- Partner ID 1
    'Contact',
    1,
    NOW(),
    1,
    NOW(),
    false,
    0,
    1
)
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- 3. Create Test Interaction (ID 1)
-- =====================================================
INSERT INTO "Interactions" (
    "Id", "Name", "Type", "Date", "Status",
    "ContactId", "Subject", "Description", "Discriminator",
    "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate",
    "IsDeleted", "DeletedBy", "WorkflowStatus"
)
VALUES (
    1,
    'Test Meeting',
    1, -- Meeting type
    NOW(),
    1, -- Active
    1, -- Contact ID 1
    'Test Meeting Subject',
    'Test interaction for E2E tests',
    'Interaction',
    1,
    NOW(),
    1,
    NOW(),
    false,
    0,
    1
)
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- 4. Link Interaction to Partner (many-to-many)
-- =====================================================
INSERT INTO "InteractionPartners" ("InteractionId", "PartnerId")
VALUES (1, 1)
ON CONFLICT DO NOTHING;

-- =====================================================
-- 5. Create Test Opportunity (ID 1)
-- =====================================================
INSERT INTO "Opportunities" (
    "Id", "Name", "Description", "Status",
    "InitiativeBudgetUSD", "Stage",
    "IsPooledFunding", "BeneficiariesToBeDetermined",
    "IsTargetSigningDateFirm", "HighRisksAcknowledged",
    "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate",
    "IsDeleted", "DeletedBy", "WorkflowStatus"
)
VALUES (
    1,
    'Test Opportunity Project',
    'This is a test opportunity for Playwright E2E tests with real backend integration.',
    1, -- Active
    100000.00,
    'Draft',
    false,
    false,
    false,
    false,
    1,
    NOW(),
    1,
    NOW(),
    false,
    0,
    1
)
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================
-- Reset sequences
-- =====================================================
SELECT setval('"Partners_Id_seq"', GREATEST(1, (SELECT COALESCE(MAX("Id"), 1) FROM "Partners")), true);
SELECT setval('"Contacts_Id_seq"', GREATEST(1, (SELECT COALESCE(MAX("Id"), 1) FROM "Contacts")), true);
SELECT setval('"Interactions_Id_seq"', GREATEST(1, (SELECT COALESCE(MAX("Id"), 1) FROM "Interactions")), true);
SELECT setval('"Opportunities_Id_seq"', GREATEST(1, (SELECT COALESCE(MAX("Id"), 1) FROM "Opportunities")), true);

-- =====================================================
-- Verification
-- =====================================================
\echo ''
\echo '✅ Test data setup complete!'
\echo ''

SELECT 
    'Partners' as "Entity",
    COUNT(*) as "Total",
    COUNT(*) FILTER (WHERE NOT "IsDeleted") as "Active",
    STRING_AGG("Name", ', ') as "Names"
FROM "Partners"
UNION ALL
SELECT 
    'Contacts',
    COUNT(*),
    COUNT(*) FILTER (WHERE NOT "IsDeleted"),
    STRING_AGG("Name", ', ')
FROM "Contacts"
UNION ALL
SELECT 
    'Interactions',
    COUNT(*),
    COUNT(*) FILTER (WHERE NOT "IsDeleted"),
    STRING_AGG("Name", ', ')
FROM "Interactions"
UNION ALL
SELECT 
    'Opportunities',
    COUNT(*),
    COUNT(*) FILTER (WHERE NOT "IsDeleted"),
    STRING_AGG("Name", ', ')
FROM "Opportunities";
