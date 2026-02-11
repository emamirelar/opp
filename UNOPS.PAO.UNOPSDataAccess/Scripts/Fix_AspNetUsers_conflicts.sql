DO $$
DECLARE
    rec RECORD;
    old_id INT;
    new_id INT;
BEGIN
    FOR rec IN
        SELECT u."Id" AS old_id,
               CASE UPPER(u."NormalizedUserName")
                   WHEN 'BIBIANENB@UNOPS.ORG' THEN 214245
                   WHEN 'CHAPIOUH@UNOPS.ORG' THEN 241398
                   WHEN 'SAIDE@UNOPS.ORG' THEN 241483
                   WHEN 'PRODYUTP@UNOPS.ORG' THEN 234967
                   WHEN 'AHMETS@UNOPS.ORG' THEN 241154
                   WHEN 'CLAUDIAR@UNOPS.ORG' THEN 222886
                   ELSE NULL
               END AS new_id
        FROM public."AspNetUsers" u
        WHERE UPPER(u."NormalizedUserName") IN (
            'BIBIANENB@UNOPS.ORG', 'CHAPIOUH@UNOPS.ORG', 'SAIDE@UNOPS.ORG',
            'PRODYUTP@UNOPS.ORG', 'AHMETS@UNOPS.ORG', 'CLAUDIAR@UNOPS.ORG'
        )
        AND u."Id" NOT IN (214245, 241398, 241483, 234967, 241154, 222886)
    LOOP
        old_id := rec.old_id;
        new_id := rec.new_id;

        IF new_id IS NULL THEN
            RAISE NOTICE 'Skipping user Id % - no mapping', old_id;
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
            "LockoutEnabled", "AccessFailedCount", "IsInternal"
        )
        SELECT
            new_id,
            REPLACE("UserName", '_MIGRATE_OLD', ''),
            REPLACE("NormalizedUserName", '_MIGRATE_OLD', ''),
            "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
            "LockoutEnabled", "AccessFailedCount", "IsInternal"
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
        UPDATE public."PartnerAgreements" SET "DeletedBy" = new_id WHERE "DeletedBy" = old_id;
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
