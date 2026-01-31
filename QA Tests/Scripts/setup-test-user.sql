-- ============================================================================
-- Setup Test User with Administrator Permissions
-- ============================================================================
-- This script creates a test user for Playwright tests with full permissions
-- Run this against your local TestDb database
-- ============================================================================

BEGIN;

-- Step 1: Ensure Administrator role exists
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
SELECT 
    1,
    'Administrator',
    'ADMINISTRATOR',
    gen_random_uuid()::text
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMINISTRATOR'
);

-- Step 2: Create test user if doesn't exist
-- Using a pre-hashed password for "TestPassword123!" (ASP.NET Identity v3 format)
INSERT INTO "AspNetUsers" (
    "Id",
    "Email",
    "NormalizedEmail",
    "UserName",
    "NormalizedUserName",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnabled",
    "AccessFailedCount",
    "IsInternal"
)
SELECT 
    1,
    'test@playwright.local',
    'TEST@PLAYWRIGHT.LOCAL',
    'test@playwright.local',
    'TEST@PLAYWRIGHT.LOCAL',
    true,
    'AQAAAAIAAYagAAAAEL8fPPgmVZF+Lqv9L5I0HQR3X7ZQ9gF5sNz8GqWd4bJc1mxK2pqN3vE4jL5wR6tA==', -- Password: TestPassword123!
    gen_random_uuid()::text,
    gen_random_uuid()::text,
    false,
    false,
    true,
    0,
    true  -- IsInternal = true for UNOPS users
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetUsers" WHERE "NormalizedEmail" = 'TEST@PLAYWRIGHT.LOCAL'
);

-- Step 3: Assign Administrator role to test user
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 1, 1
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetUserRoles" WHERE "UserId" = 1 AND "RoleId" = 1
);

-- Step 4: Verify setup
SELECT 
    u."Email" as "User Email",
    u."IsInternal" as "Is Internal",
    r."Name" as "Role",
    CASE 
        WHEN ur."UserId" IS NOT NULL THEN 'Assigned'
        ELSE 'Not Assigned'
    END as "Role Status"
FROM "AspNetUsers" u
CROSS JOIN "AspNetRoles" r
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id" AND ur."RoleId" = r."Id"
WHERE u."Email" = 'test@playwright.local'
    AND r."Name" = 'Administrator';

COMMIT;

-- ============================================================================
-- Success! Test user created with the following credentials:
-- Email: test@playwright.local
-- Password: TestPassword123!
-- Role: Administrator
-- ============================================================================

-- Optional: View all users and their roles
SELECT 
    u."Email",
    u."IsInternal",
    STRING_AGG(r."Name", ', ') as "Roles"
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "AspNetRoles" r ON r."Id" = ur."RoleId"
GROUP BY u."Id", u."Email", u."IsInternal"
ORDER BY u."Email";
