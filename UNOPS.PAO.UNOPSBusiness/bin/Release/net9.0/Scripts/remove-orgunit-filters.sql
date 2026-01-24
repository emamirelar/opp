-- Remove OrgUnit filters from EntityPermissions
-- This script removes the automatic OrgUnit filtering that was preventing users from seeing all partners

-- Remove OrgUnit filters from Partner permissions
UPDATE public."EntityPermissions" 
SET "RowFilter" = '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
WHERE "Entity" = 'Partner' 
  AND "RowFilter" LIKE '%PartnerOffice%@userOrgUnit%';

-- Remove OrgUnit filters from Contact permissions  
UPDATE public."EntityPermissions"
SET "RowFilter" = '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
WHERE "Entity" = 'Contact'
  AND "RowFilter" LIKE '%Partner.PartnerOffice%@userOrgUnit%';

-- Log the changes
DO $$
DECLARE
    partner_count INTEGER;
    contact_count INTEGER;
BEGIN
    -- Get the number of affected rows
    GET DIAGNOSTICS partner_count = ROW_COUNT;
    
    UPDATE public."EntityPermissions"
    SET "RowFilter" = '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
    WHERE "Entity" = 'Contact'
      AND "RowFilter" LIKE '%Partner.PartnerOffice%@userOrgUnit%';
    
    GET DIAGNOSTICS contact_count = ROW_COUNT;
    
    RAISE NOTICE 'Removed OrgUnit filters from % Partner permissions and % Contact permissions', partner_count, contact_count;
END $$;