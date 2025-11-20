-- Seed System User for Opportunity+ System
-- This is a special system user account used for automated operations
-- Uses negative ID to avoid conflicts with real UNOPS employee IDs
-- Not a real internal user, cannot be locked out, and has no password

DO $$
DECLARE 
    inserted_count INTEGER;
BEGIN
    INSERT INTO public."AspNetUsers" (
        "Id", 
        "UserName", 
        "NormalizedUserName", 
        "Email", 
        "NormalizedEmail", 
        "EmailConfirmed", 
        "PasswordHash",
        "SecurityStamp", 
        "ConcurrencyStamp",
        "PhoneNumber",
        "PhoneNumberConfirmed",
        "TwoFactorEnabled",
        "LockoutEnd",
        "LockoutEnabled",
        "AccessFailedCount",
        "IsInternal"
    )
    SELECT 
        user_data.id,
        user_data.username,
        user_data.normalized_username,
        user_data.email,
        user_data.normalized_email,
        user_data.email_confirmed,
        user_data.password_hash,
        user_data.security_stamp,
        user_data.concurrency_stamp,
        user_data.phone_number,
        user_data.phone_number_confirmed,
        user_data.two_factor_enabled,
        user_data.lockout_end,
        user_data.lockout_enabled,
        user_data.access_failed_count,
        user_data.is_internal
    FROM (VALUES
        (-1, 'opportunityplus@unops.org', 'OPPORTUNITYPLUS@UNOPS.ORG', 'opportunityplus@unops.org', 'OPPORTUNITYPLUS@UNOPS.ORG', true, '', 'SYSTEM-' || gen_random_uuid()::text, gen_random_uuid()::text, '', false, false, NULL::timestamp with time zone, false, 0, false)
    ) AS user_data(id, username, normalized_username, email, normalized_email, email_confirmed, password_hash, security_stamp, concurrency_stamp, phone_number, phone_number_confirmed, two_factor_enabled, lockout_end, lockout_enabled, access_failed_count, is_internal)
    WHERE NOT EXISTS (
        SELECT 1 FROM public."AspNetUsers" WHERE "Id" = user_data.id
    );
    
    GET DIAGNOSTICS inserted_count = ROW_COUNT;
    RAISE NOTICE 'Inserted % AspNetUsers records (System User)', inserted_count;
END $$;