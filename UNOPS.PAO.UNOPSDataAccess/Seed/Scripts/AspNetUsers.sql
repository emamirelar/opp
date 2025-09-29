-- Seed AspNetUsers required by Partners
-- These users are referenced by the Partners table via PartnerFocalPointUserId
-- Using real user data with actual emails and security stamps

-- Only insert users that don't already exist
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
    (29737, 'martina@unops.org', 'MARTINA@UNOPS.ORG', 'martina@unops.org', 'MARTINA@UNOPS.ORG', true, '', 'f06b9359-0d67-40b6-8f79-5937ca7df0af', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (40036, 'mariacarmenco@unops.org', 'MARIACARMENCO@UNOPS.ORG', 'mariacarmenco@unops.org', 'MARIACARMENCO@UNOPS.ORG', true, '', '46bb86d1-06f2-4f01-b600-86fdeb78e8b8', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (44918, 'LaetitiaK@unops.org', 'LAETITIAK@UNOPS.ORG', 'LaetitiaK@unops.org', 'LAETITIAK@UNOPS.ORG', true, '', '4faac2b9-2606-4468-b62b-7a8c787f595c', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (87714, 'LorraineA@unops.org', 'LORRAINEA@UNOPS.ORG', 'LorraineA@unops.org', 'LORRAINEA@UNOPS.ORG', true, '', 'e8056ec2-a6f2-4fd7-8737-2f0d4f9b6f56', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (146714, 'ArnaudS@unops.org', 'ARNAUDS@UNOPS.ORG', 'ArnaudS@unops.org', 'ARNAUDS@UNOPS.ORG', true, '', '27adba67-f61e-4470-a23c-b39ea191a094', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (149329, 'HalaS@unops.org', 'HALAS@UNOPS.ORG', 'HalaS@unops.org', 'HALAS@UNOPS.ORG', true, '', '0ec8c1a5-2257-4ffd-ad62-22294957a738', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (178636, 'LaurentiuM@unops.org', 'LAURENTIUM@UNOPS.ORG', 'LaurentiuM@unops.org', 'LAURENTIUM@UNOPS.ORG', true, '', 'e3b77c4d-2a56-4191-8d17-add6bf7821c6', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (203964, 'MichaelRI@unops.org', 'MICHAELRI@UNOPS.ORG', 'MichaelRI@unops.org', 'MICHAELRI@UNOPS.ORG', true, '', 'd482d59a-5577-485c-bed9-a9e63fbe6a79', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (206589, 'YukoM@unops.org', 'YUKOM@UNOPS.ORG', 'YukoM@unops.org', 'YUKOM@UNOPS.ORG', true, '', '7dce2175-7e29-43d2-afe0-b102e4f5cabb', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (214695, 'menyeamelie@yahoo.com', 'MENYEAMELIE@YAHOO.COM', 'menyeamelie@yahoo.com', 'MENYEAMELIE@YAHOO.COM', true, '', 'faef34e3-0f03-4eeb-8011-91181da5aa9e', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (215103, 'IsabelaF@unops.org', 'ISABELAF@UNOPS.ORG', 'IsabelaF@unops.org', 'ISABELAF@UNOPS.ORG', true, '', '4fd20e6a-14af-487e-a449-03e594540731', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (218090, 'KajsaH@unops.org', 'KAJSAH@UNOPS.ORG', 'KajsaH@unops.org', 'KAJSAH@UNOPS.ORG', true, '', '5e4e8605-92d0-4b26-97ae-41e257c34f5e', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (221996, 'MikaelaG@unops.org', 'MIKAELAG@UNOPS.ORG', 'MikaelaG@unops.org', 'MIKAELAG@UNOPS.ORG', true, '', '008e2e85-9397-49e5-baa4-a4102131dd98', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (226146, 'PatrickEL@unops.org', 'PATRICKEL@UNOPS.ORG', 'PatrickEL@unops.org', 'PATRICKEL@UNOPS.ORG', true, '', 'e44e2908-b0b6-44df-8144-17e13bf38fc5', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (226953, 'ChristineBO@unops.org', 'CHRISTINEBO@UNOPS.ORG', 'ChristineBO@unops.org', 'CHRISTINEBO@UNOPS.ORG', true, '', 'c7a5f4db-d769-4f09-949d-609ae362c330', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (228474, 'JoseME@unops.org', 'JOSEME@UNOPS.ORG', 'JoseME@unops.org', 'JOSEME@UNOPS.ORG', true, '', 'f38038cb-a411-4f74-a502-974c16afb1a1', '', '', false, false, NULL::timestamp with time zone, true, 0, true),
    (229674, 'asbjornb@unops.org', 'ASBJORNB@UNOPS.ORG', 'asbjornb@unops.org', 'ASBJORNB@UNOPS.ORG', true, '', '34c60c45-48e2-45ba-a8f2-f6c1797ea251', '', '', false, false, NULL::timestamp with time zone, true, 0, true)
) AS user_data(id, username, normalized_username, email, normalized_email, email_confirmed, password_hash, security_stamp, concurrency_stamp, phone_number, phone_number_confirmed, two_factor_enabled, lockout_end, lockout_enabled, access_failed_count, is_internal)
WHERE NOT EXISTS (
    SELECT 1 FROM public."AspNetUsers" WHERE "Id" = user_data.id
);

-- Display how many users were inserted
DO $$
DECLARE 
    inserted_count INTEGER;
BEGIN
    GET DIAGNOSTICS inserted_count = ROW_COUNT;
    RAISE NOTICE 'Inserted % AspNetUsers records', inserted_count;
END $$;