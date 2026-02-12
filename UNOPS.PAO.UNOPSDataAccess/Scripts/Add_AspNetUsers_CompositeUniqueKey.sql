-- =============================================================================
-- Add composite unique key (Id, NormalizedUserName) to AspNetUsers
--
-- Required for External Data Service sync when using composite key upsert.
-- Allows same email (NormalizedUserName) for different Resource IDs.
--
-- PREREQUISITE: Run before deploying the updated 01-aspnetusers.yaml config.
-- =============================================================================

-- Drop unique constraint on NormalizedUserName if it exists (allows duplicate emails)
-- ASP.NET Identity and EF typically use one of these index names
DROP INDEX IF EXISTS public."UserNameIndex";
DROP INDEX IF EXISTS public."IX_AspNetUsers_NormalizedUserName";

-- Create composite unique index for sync ON CONFLICT (Id, NormalizedUserName)
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AspNetUsers_Id_NormalizedUserName"
ON public."AspNetUsers" ("Id", "NormalizedUserName");
