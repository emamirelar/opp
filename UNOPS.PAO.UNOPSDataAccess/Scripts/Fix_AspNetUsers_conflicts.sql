-- =============================================================================
-- Fix AspNetUsers conflicts before BQ sync (01-aspnetusers.yaml)
--
-- Case 1: Placeholder ID migration - users with Id <= 999 -> Resource IDs from BQ
-- Case 2: Same UserID, different email - update existing row to BQ canonical email
--         (e.g. PauloR@unops.org -> PAULINERO@UNOPS.ORG for UserID 240666)
-- =============================================================================

DROP TABLE IF EXISTS _aspnetusers_migration_map;
CREATE TEMP TABLE _aspnetusers_migration_map (
    normalized_user_name text PRIMARY KEY,
    new_id int NOT NULL
);

-- 33
INSERT INTO _aspnetusers_migration_map (normalized_user_name, new_id)
VALUES
    ('STEPHENP@UNOPS.ORG', 70499),
    ('BIBIANENB@UNOPS.ORG', 214245),
    ('CLAUDIAR@UNOPS.ORG', 222886),
    ('PETRAK@UNOPS.ORG', 231187),
    ('PRODYUTP@UNOPS.ORG', 234967),
    ('WISNELT@UNOPS.ORG', 236080),
    ('AHMETS@UNOPS.ORG', 241154),
    ('QUEIROZJ@UNOPS.ORG', 241253),
    ('MEGANFD@UNOPS.ORG', 241346),
    ('SANAAA@UNOPS.ORG', 241363),
    ('CHAPIOUH@UNOPS.ORG', 241398),
    ('JOAOM@UNOPS.ORG', 241413),
    ('SAIDE@UNOPS.ORG', 241483),
    ('YIDIDIYAG@UNOPS.ORG', 241513),
    ('STEPHENOM@UNOPS.ORG', 241550),
    ('MONICAD@UNOPS.ORG', 241624),
    ('MORUKD@UNOPS.ORG', 241713),
    ('ARIELP@UNOPS.ORG', 243960),
    ('ARWAM@UNOPS.ORG', 243973),
    ('THONGCHANK@UNOPS.ORG', 243983),
    ('MUXAMMADRIZOA@UNOPS.ORG', 243984),
    ('BEXZADY@UNOPS.ORG', 243993),
    ('MOHAMMADNABISA@UNOPS.ORG', 244028),
    ('DOAAAB@UNOPS.ORG', 244030),
    ('MOHAMEDSA@UNOPS.ORG', 244065),
    ('KARINO@UNOPS.ORG', 244082),
    ('KHALIFASA@UNOPS.ORG', 244138),
    ('ALAAF@UNOPS.ORG', 244220),
    ('BENINGODFREYL@UNOPS.ORG', 244353),
    ('ELONAW@UNOPS.ORG', 244541),
    ('ALESSIOAM@UNOPS.ORG', 244555),
    ('MAALEXANDRAM@UNOPS.ORG', 244579),
    ('LINADA@UNOPS.ORG', 244821),
    ('JASONAL@UNOPS.ORG', 237675),
    ('ARNAUDS@UNOPS.ORG', 146714),
    ('LEAJ@UNOPS.ORG', 217958);

-- =============================================================================
-- Case 2: Same UserID, different email (BQ has new canonical email)
-- Update existing row to new email so BQ upsert succeeds (avoids PK conflict).
-- Example: PauloR@unops.org (existing) -> PAULINERO@UNOPS.ORG (from BQ), UserID 240666
-- =============================================================================

DROP TABLE IF EXISTS _aspnetusers_email_update_map;
CREATE TEMP TABLE _aspnetusers_email_update_map (
    user_id int PRIMARY KEY,
    new_email text NOT NULL,
    new_normalized_email text NOT NULL
);

INSERT INTO _aspnetusers_email_update_map (user_id, new_email, new_normalized_email)
VALUES
    (240666, 'PAULINERO@UNOPS.ORG', 'PAULINERO@UNOPS.ORG');

DO $$
DECLARE
    rec RECORD;
BEGIN
    FOR rec IN
        SELECT m.user_id, m.new_email, m.new_normalized_email
        FROM _aspnetusers_email_update_map m
        INNER JOIN public."AspNetUsers" u ON u."Id" = m.user_id
        WHERE UPPER(u."NormalizedUserName") != m.new_normalized_email
          AND NOT EXISTS (
              SELECT 1 FROM public."AspNetUsers" a
              WHERE a."NormalizedUserName" = m.new_normalized_email AND a."Id" != m.user_id
          )
    LOOP
        RAISE NOTICE 'Updating user Id % email to %', rec.user_id, rec.new_email;

        UPDATE public."AspNetUsers"
        SET "UserName" = rec.new_email,
            "NormalizedUserName" = rec.new_normalized_email,
            "Email" = rec.new_email,
            "NormalizedEmail" = rec.new_normalized_email
        WHERE "Id" = rec.user_id;

        RAISE NOTICE 'Done updating user % to new email', rec.user_id;
    END LOOP;
END $$;

DROP TABLE IF EXISTS _aspnetusers_email_update_map;

-- =============================================================================
-- Case 1: Placeholder ID migration (Id <= 999 -> Resource ID)
-- =============================================================================

DO $$
DECLARE
    rec RECORD;
    old_id INT;
    new_id INT;
BEGIN
    FOR rec IN
        SELECT u."Id" AS old_id, m.new_id
        FROM public."AspNetUsers" u
        INNER JOIN _aspnetusers_migration_map m ON UPPER(u."NormalizedUserName") = m.normalized_user_name
        WHERE u."Id" <= 999
          AND u."Id" > 0
          AND u."UserName" LIKE '%unops.org%'
          AND u."Id" != m.new_id
          AND NOT EXISTS (SELECT 1 FROM public."AspNetUsers" a WHERE a."Id" = m.new_id)
    LOOP
        old_id := rec.old_id;
        new_id := rec.new_id;

        IF new_id IS NULL OR new_id <= 0 THEN
            RAISE NOTICE 'Skipping user Id % - no valid mapping', old_id;
            CONTINUE;
        END IF;

        RAISE NOTICE 'Migrating user Id % -> %', old_id, new_id;

        -- Rename old to avoid unique constraint
        UPDATE public."AspNetUsers"
        SET "NormalizedUserName" = "NormalizedUserName" || '_MIGRATE_OLD',
            "UserName" = "UserName" || '_MIGRATE_OLD'
        WHERE "Id" = old_id;

        -- Insert new row with correct Resource ID
        INSERT INTO public."AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
            "LockoutEnabled", "AccessFailedCount", "IsInternal", "ActiveUser"
        )
        SELECT
            new_id,
            REPLACE("UserName", '_MIGRATE_OLD', ''),
            REPLACE("NormalizedUserName", '_MIGRATE_OLD', ''),
            "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
            "LockoutEnabled", "AccessFailedCount", "IsInternal", COALESCE("ActiveUser", true)
        FROM public."AspNetUsers" WHERE "Id" = old_id;

        -- Update all references
        UPDATE public."AspNetUserRoles" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."Partners" SET "PartnerFocalPointUserId" = new_id WHERE "PartnerFocalPointUserId" = old_id;
        UPDATE public."UserProfile" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."UserPreferences" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."Opportunities" SET "ExecutiveId" = new_id WHERE "ExecutiveId" = old_id;
        UPDATE public."Opportunities" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."Opportunities" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."OpportunityCollaborators" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."OpportunityCollaborators" SET "AddedBy" = new_id WHERE "AddedBy" = old_id;
        UPDATE public."OpportunityStakeholders" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."InteractionUsers" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."Interactions" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."Interactions" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."Interactions" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;
        UPDATE public."EntityUserRoles" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."EntityRolePersons" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."Notifications" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."EmailNotificationLogs" SET "RecipientUserId" = new_id WHERE "RecipientUserId" = old_id;
        UPDATE public."AiChatSession" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."AuditLogs" SET "UserId" = new_id WHERE "UserId" = old_id;
        UPDATE public."Partners" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."Partners" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."Partners" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;
        UPDATE public."Contacts" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."Contacts" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."Contacts" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;

        -- Additional tables with audit columns (CreatedBy/LastModifiedBy/DeletedBy)
        UPDATE public."Documents" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."Documents" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."Documents" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;
        UPDATE public."PartnerAgreements" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."PartnerAgreements" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        --Commented out because it is not a valid column in the table
        --UPDATE public."PartnerAgreements" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;
        UPDATE public."PartnerTrees" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."PartnerTrees" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."PartnerTrees" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;
        UPDATE public."Comments" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."Comments" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."Comments" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;
        UPDATE public."ArtifactDataTypes" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."ArtifactDataTypes" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."ArtifactDataTypes" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;
        UPDATE public."ArtifactExtractionRules" SET "CreatedBy" = new_id WHERE "CreatedBy" = old_id;
        UPDATE public."ArtifactExtractionRules" SET "LastModifiedBy" = new_id WHERE "LastModifiedBy" = old_id;
        UPDATE public."ArtifactExtractionRules" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;

        -- Delete old user
        DELETE FROM public."AspNetUsers" WHERE "Id" = old_id;

        RAISE NOTICE 'Done migrating % -> %', old_id, new_id;
    END LOOP;
END $$;

DROP TABLE IF EXISTS _aspnetusers_migration_map;
