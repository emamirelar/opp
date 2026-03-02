-- ==============================================================================
-- Create Test Users for Playwright E2E Testing
-- ==============================================================================
-- Purpose: Create users with different permission levels for automated testing
--   1. test-contact-admin@playwright.local - Full permissions on Contacts  
--   2. test@playwright.local - No permissions (negative tests)
-- ==============================================================================

-- ==============================================================================
-- User 1: Test Contact Admin (WITH permissions)
-- ==============================================================================

-- Insert user if not exists
DO $$
DECLARE
    admin_user_id INT;
BEGIN
    -- Check if user already exists
    SELECT "Id" INTO admin_user_id 
    FROM public."AspNetUsers" 
    WHERE "Email" = 'test-contact-admin@playwright.local';
    
    -- Create user if doesn't exist
    IF admin_user_id IS NULL THEN
        INSERT INTO public."AspNetUsers" (
            "Email", 
            "NormalizedEmail",
            "IsInternal", 
            "EmailConfirmed", 
            "PhoneNumberConfirmed", 
            "TwoFactorEnabled", 
            "LockoutEnabled", 
            "AccessFailedCount",
            "UserName",
            "NormalizedUserName",
            "SecurityStamp",
            "ConcurrencyStamp"
        )
        VALUES (
            'test-contact-admin@playwright.local',
            'TEST-CONTACT-ADMIN@PLAYWRIGHT.LOCAL',
            true,
            true,
            false,
            false,
            false,
            0,
            'test-contact-admin@playwright.local',
            'TEST-CONTACT-ADMIN@PLAYWRIGHT.LOCAL',
            gen_random_uuid()::text,
            gen_random_uuid()::text
        )
        RETURNING "Id" INTO admin_user_id;
        
        RAISE NOTICE 'Created Test Contact Admin User with ID: %', admin_user_id;
    ELSE
        RAISE NOTICE 'Test Contact Admin User already exists with ID: %', admin_user_id;
    END IF;
END $$;

-- Create EntityPermission for Contacts with FULL permissions
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public."EntityPermissions" 
        WHERE "Entity" = 'Contact' AND "Role" = 'TestContactAdmin'
    ) THEN
        INSERT INTO public."EntityPermissions" (
            "Entity", 
            "Role", 
            "CanCreate", 
            "CanRead", 
            "CanUpdate", 
            "CanDelete", 
            "PropertyFilter", 
            "RowFilter"
        )
        VALUES (
            'Contact', 
            'TestContactAdmin', 
            true, 
            true, 
            true, 
            true, 
            NULL, 
            NULL
        );
        RAISE NOTICE 'Created EntityPermission for TestContactAdmin role on Contact entity';
    ELSE
        RAISE NOTICE 'EntityPermission for TestContactAdmin already exists';
    END IF;
END $$;

-- ==============================================================================
-- User 2: Test User (NO permissions) - Update if exists
-- ==============================================================================

DO $$
DECLARE
    test_user_id INT;
BEGIN
    -- Check if user already exists
    SELECT "Id" INTO test_user_id 
    FROM public."AspNetUsers" 
    WHERE "Email" = 'test@playwright.local';
    
    -- Create user if doesn't exist
    IF test_user_id IS NULL THEN
        INSERT INTO public."AspNetUsers" (
            "Email", 
            "NormalizedEmail",
            "IsInternal", 
            "EmailConfirmed", 
            "PhoneNumberConfirmed", 
            "TwoFactorEnabled", 
            "LockoutEnabled", 
            "AccessFailedCount",
            "UserName",
            "NormalizedUserName",
            "SecurityStamp",
            "ConcurrencyStamp"
        )
        VALUES (
            'test@playwright.local',
            'TEST@PLAYWRIGHT.LOCAL',
            false,
            true,
            false,
            false,
            false,
            0,
            'test@playwright.local',
            'TEST@PLAYWRIGHT.LOCAL',
            gen_random_uuid()::text,
            gen_random_uuid()::text
        )
        RETURNING "Id" INTO test_user_id;
        
        RAISE NOTICE 'Created Test User (No Permissions) with ID: %', test_user_id;
    ELSE
        RAISE NOTICE 'Test User (No Permissions) already exists with ID: %', test_user_id;
    END IF;
END $$;

-- ==============================================================================
-- Verification Queries
-- ==============================================================================

-- Show all test users
SELECT "Id", "Email", "IsInternal", "EmailConfirmed"
FROM public."AspNetUsers" 
WHERE "Email" LIKE '%@playwright.local'
ORDER BY "Email";

-- Show all EntityPermissions
SELECT "Id", "Entity", "Role", "CanCreate", "CanRead", "CanUpdate", "CanDelete"
FROM public."EntityPermissions"
WHERE "Role" = 'TestContactAdmin'
ORDER BY "Entity";

-- ==============================================================================
-- NOTES:
-- ==============================================================================
-- 1. User-to-Role mapping needs to be configured in the application
-- 2. The permission service should map test-contact-admin@playwright.local → TestContactAdmin role
-- 3. The permission service should map test@playwright.local → No role (or default role)
-- 4. For Playwright tests, set cookies: dev-user-email=test-contact-admin@playwright.local
-- ==============================================================================
