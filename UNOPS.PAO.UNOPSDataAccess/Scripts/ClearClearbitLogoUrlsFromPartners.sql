-- =============================================================================
-- Clear LogoUrl values from Partners that contain "clearbit" in the URL
--
-- Clearbit logo URLs may be deprecated or causing issues. This script sets
-- LogoUrl to NULL for any Partner whose LogoUrl contains "clearbit" (case-insensitive).
--
-- Use: Run manually or via migration 20260216181625_ClearClearbitLogoUrlsFromPartners
-- Idempotent: safe to run multiple times.
--
-- Location: UNOPS.PAO.UNOPSDataAccess/Scripts/ClearClearbitLogoUrlsFromPartners.sql
-- =============================================================================

UPDATE public."Partners"
SET "LogoUrl" = NULL
WHERE "LogoUrl" IS NOT NULL
  AND "LogoUrl" LIKE '%clearbit%';
