-- =============================================================================
-- Add unique constraints for EDS bulk upsert (INSERT ... ON CONFLICT DO UPDATE)
--
-- Required for configs with use_bulk_upsert: true
--   - 02-userprofile.yaml, 03-aspnetuserroles.yaml
--   - 04-countries.yaml, 05-currencies.yaml, 06-engagements.yaml
--   - 07-engagement-partners.yaml, 08-partner-agreements.yaml
--   - 09-organization-hierarchies.yaml (uses Id PK)
--   - 10-entity-user-roles-doa.yaml, 11-entity-user-roles-mgmt.yaml (use Id PK)
--
-- Prefer: dotnet ef database update (runs migration 20260213211123_AddUniqueConstraintsForTables)
-- Use this script when migrations cannot be run (e.g. manual DB updates).
-- Idempotent: safe to run multiple times.
--
-- Location: UNOPS.PAO.UNOPSDataAccess/Scripts/AddBulkUpsertUniqueConstraints.sql
-- =============================================================================

-- UserProfile: unique on (UserId, Id) for 02-userprofile.yaml
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'public'
        AND tablename = 'UserProfile'
        AND indexname = 'IX_UserProfile_UserId_Id'
    ) THEN
        CREATE UNIQUE INDEX "IX_UserProfile_UserId_Id"
        ON public."UserProfile" ("UserId", "Id");
    END IF;
END $$;

-- AspNetUserRoles: unique on (UserId, RoleId) for 03-aspnetuserroles.yaml
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'public'
        AND tablename = 'AspNetUserRoles'
        AND indexname = 'IX_AspNetUserRoles_UserId_RoleId'
    ) THEN
        CREATE UNIQUE INDEX "IX_AspNetUserRoles_UserId_RoleId"
        ON public."AspNetUserRoles" ("UserId", "RoleId");
    END IF;
END $$;

-- Countries: unique on Iso2Code for 04-countries.yaml
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'public'
        AND tablename = 'Countries'
        AND indexname = 'IX_Countries_Iso2Code'
    ) THEN
        CREATE UNIQUE INDEX "IX_Countries_Iso2Code"
        ON public."Countries" ("Iso2Code");
    END IF;
END $$;

-- Currencies: unique on Code for 05-currencies.yaml
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'public'
        AND tablename = 'Currencies'
        AND indexname = 'IX_Currencies_Code'
    ) THEN
        CREATE UNIQUE INDEX "IX_Currencies_Code"
        ON public."Currencies" ("Code");
    END IF;
END $$;

-- BaseEngagements: unique on BaseEngagement for 06-engagements.yaml
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'public'
        AND tablename = 'BaseEngagements'
        AND indexname = 'IX_BaseEngagements_BaseEngagement'
    ) THEN
        CREATE UNIQUE INDEX "IX_BaseEngagements_BaseEngagement"
        ON public."BaseEngagements" ("BaseEngagement");
    END IF;
END $$;

-- BaseEngagementPartners: unique on Key for 07-engagement-partners.yaml
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'public'
        AND tablename = 'BaseEngagementPartners'
        AND indexname = 'IX_BaseEngagementPartners_Key'
    ) THEN
        CREATE UNIQUE INDEX "IX_BaseEngagementPartners_Key"
        ON public."BaseEngagementPartners" ("Key");
    END IF;
END $$;

-- PartnerAgreements: unique on PartnerAgreementNumber for 08-partner-agreements.yaml
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'public'
        AND tablename = 'PartnerAgreements'
        AND indexname = 'IX_PartnerAgreements_PartnerAgreementNumber'
    ) THEN
        CREATE UNIQUE INDEX "IX_PartnerAgreements_PartnerAgreementNumber"
        ON public."PartnerAgreements" ("PartnerAgreementNumber");
    END IF;
END $$;
