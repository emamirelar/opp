-- =============================================================================
-- Revert AspNetUsers to GMS-style sync: NormalizedUserName unique, ActiveUser soft delete
--
-- 1. Add ActiveUser column (default TRUE) for soft delete
-- 2. Drop composite unique index (Id, NormalizedUserName)
-- 3. Create unique index on NormalizedUserName
--
-- PREREQUISITE: Run before deploying the updated 01-aspnetusers.yaml config.
--
-- NOTE: If duplicate NormalizedUserName rows exist, CREATE UNIQUE INDEX will fail.
-- Resolve duplicates manually (e.g. run Fix_AspNetUsers_conflicts.sql or similar)
-- before applying this migration.
-- =============================================================================

-- 1. Add ActiveUser column if not exists
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'AspNetUsers'
          AND column_name = 'ActiveUser'
    ) THEN
        ALTER TABLE public."AspNetUsers"
        ADD COLUMN "ActiveUser" boolean NOT NULL DEFAULT true;
    END IF;
END $$;

-- 2. Drop composite unique index
DROP INDEX IF EXISTS public."IX_AspNetUsers_Id_NormalizedUserName";

-- 3. Drop any conflicting indexes on NormalizedUserName
DROP INDEX IF EXISTS public."UserNameIndex";

-- 4. Create unique index on NormalizedUserName (one row per email)
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AspNetUsers_NormalizedUserName"
ON public."AspNetUsers" ("NormalizedUserName");
