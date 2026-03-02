-- ==============================================================================
-- Assign Roles to Test Users for Playwright E2E Testing
-- ==============================================================================

-- ==============================================================================
-- Step 1: Create TestContactAdmin Role
-- ==============================================================================

DO $$
DECLARE
    role_id INT;
BEGIN
    -- Check if role exists
    SELECT "Id" INTO role_id 
    FROM public."AspNetRoles"
    WHERE "Name" = 'TestContactAdmin';
    
    -- Create role if doesn't exist
    IF role_id IS NULL THEN
        INSERT INTO public."AspNetRoles" (
            "Name",
            "NormalizedName",
            "ConcurrencyStamp"
        )
        VALUES (
            'TestContactAdmin',
            'TESTCONTACTADMIN',
            gen_random_uuid()::text
        )
        RETURNING "Id" INTO role_id;
        
        RAISE NOTICE 'Created TestContactAdmin role with ID: %', role_id;
    ELSE
        RAISE NOTICE 'TestContactAdmin role already exists with ID: %', role_id;
    END IF;
END $$;

-- ==============================================================================
-- Step 2: Assign TestContactAdmin role to test-contact-admin@playwright.local
-- ==============================================================================

DO $$
DECLARE
    user_id INT;
    role_id INT;
BEGIN
    -- Get user ID
    SELECT "Id" INTO user_id 
    FROM public."AspNetUsers"
    WHERE "Email" = 'test-contact-admin@playwright.local';
    
    -- Get role ID
    SELECT "Id" INTO role_id 
    FROM public."AspNetRoles"
    WHERE "Name" = 'TestContactAdmin';
    
    -- Assign role to user if not already assigned
    IF user_id IS NOT NULL AND role_id IS NOT NULL THEN
        IF NOT EXISTS (
            SELECT 1 FROM public."AspNetUserRoles"
            WHERE "UserId" = user_id AND "RoleId" = role_id
        ) THEN
            INSERT INTO public."AspNetUserRoles" ("UserId", "RoleId")
            VALUES (user_id, role_id);
            
            RAISE NOTICE 'Assigned TestContactAdmin role (ID: %) to user (ID: %)', role_id, user_id;
        ELSE
            RAISE NOTICE 'User (ID: %) already has TestContactAdmin role (ID: %)', user_id, role_id;
        END IF;
    ELSE
        RAISE WARNING 'Could not assign role - User ID: %, Role ID: %', user_id, role_id;
    END IF;
END $$;

-- ==============================================================================
-- Verification Queries
-- ==============================================================================

-- Show all roles
SELECT "Id", "Name", "NormalizedName"
FROM public."AspNetRoles"
WHERE "Name" LIKE '%Test%'
ORDER BY "Name";

-- Show user-role mappings for test users
SELECT 
    u."Id" AS "UserId",
    u."Email",
    r."Id" AS "RoleId",
    r."Name" AS "RoleName"
FROM public."AspNetUsers" u
INNER JOIN public."AspNetUserRoles" ur ON u."Id" = ur."UserId"
INNER JOIN public."AspNetRoles" r ON ur."RoleId" = r."Id"
WHERE u."Email" LIKE '%@playwright.local'
ORDER BY u."Email", r."Name";

-- Show EntityPermissions for TestContactAdmin role
SELECT "Id", "Entity", "Role", "CanCreate", "CanRead", "CanUpdate", "CanDelete"
FROM public."EntityPermissions"
WHERE "Role" = 'TestContactAdmin'
ORDER BY "Entity";

-- ==============================================================================
-- SUMMARY
-- ==============================================================================

DO $$
DECLARE
    admin_user_count INT;
    admin_role_count INT;
    permission_count INT;
BEGIN
    SELECT COUNT(*) INTO admin_user_count 
    FROM public."AspNetUsers" 
    WHERE "Email" = 'test-contact-admin@playwright.local';
    
    SELECT COUNT(*) INTO admin_role_count
    FROM public."AspNetUserRoles" ur
    INNER JOIN public."AspNetUsers" u ON ur."UserId" = u."Id"
    INNER JOIN public."AspNetRoles" r ON ur."RoleId" = r."Id"
    WHERE u."Email" = 'test-contact-admin@playwright.local' 
      AND r."Name" = 'TestContactAdmin';
    
    SELECT COUNT(*) INTO permission_count
    FROM public."EntityPermissions"
    WHERE "Role" = 'TestContactAdmin' AND "Entity" = 'Contact';
    
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Test User Setup Summary:';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'User exists: % (expected: 1)', admin_user_count;
    RAISE NOTICE 'Role assigned: % (expected: 1)', admin_role_count;
    RAISE NOTICE 'Permissions configured: % (expected: 1)', permission_count;
    RAISE NOTICE '';
    
    IF admin_user_count = 1 AND admin_role_count = 1 AND permission_count = 1 THEN
        RAISE NOTICE '✅ Test user setup COMPLETE!';
        RAISE NOTICE '   Use: test-contact-admin@playwright.local (WITH permissions)';
        RAISE NOTICE '   Use: test@playwright.local (NO permissions)';
    ELSE
        RAISE WARNING '❌ Test user setup INCOMPLETE - check logs above';
    END IF;
    RAISE NOTICE '========================================';
END $$;
