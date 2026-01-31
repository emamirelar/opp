-- ============================================================================
-- Setup Test User with Administrator Permissions
-- ============================================================================
-- This script creates a test user for Playwright tests with full permissions
-- ============================================================================

-- Ensure Administrator role exists
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
SELECT 
    1,
    'Administrator',
    'ADMINISTRATOR',
    md5(random()::text)
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = 'ADMINISTRATOR'
);

-- Create test user if doesn't exist
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
    'AQAAAAIAAYagAAAAEL8fPPgmVZF+Lqv9L5I0HQR3X7ZQ9gF5sNz8GqWd4bJc1mxK2pqN3vE4jL5wR6tA==',
    md5(random()::text),
    md5(random()::text),
    false,
    false,
    true,
    0,
    true
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetUsers" WHERE "NormalizedEmail" = 'TEST@PLAYWRIGHT.LOCAL'
);

-- Assign Administrator role to test user
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 1, 1
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetUserRoles" WHERE "UserId" = 1 AND "RoleId" = 1
);

-- Done - Test user: test@playwright.local / TestPassword123!
