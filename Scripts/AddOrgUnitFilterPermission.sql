-- Script to add OrgUnit filtering permission for Partners
-- This will filter partners based on the user's organizational unit

-- First, check if the permission already exists
DO $$
BEGIN
    -- Add permission for UNOPS_GEN_USER role to filter partners by OrgUnit
    IF NOT EXISTS (
        SELECT 1 FROM "EntityPermissions" 
        WHERE "Entity" = 'Partner' 
        AND "Role" = 'UNOPS_GEN_USER'
        AND "RowFilter" LIKE '%PartnerOffice%'
    ) THEN
        INSERT INTO "EntityPermissions" (
            "Entity", 
            "Role", 
            "CanCreate", 
            "CanRead", 
            "CanUpdate", 
            "CanDelete", 
            "RowFilter",
            "PropertyFilter",
            "IsActive"
        ) VALUES (
            'Partner',
            'UNOPS_GEN_USER',
            true,
            true,
            true,
            false,
            '{"CanRead": "PartnerOffice != null && PartnerOffice.Code == @userOrgUnit", "CanUpdate": "PartnerOffice != null && PartnerOffice.Code == @userOrgUnit", "CanCreate": "true", "CanDelete": "false"}',
            null,
            true
        );
    END IF;
END $$;

-- Note: This filter will only show partners whose PartnerOffice.Code matches the user's OrgUnit
-- If you want to include hierarchy (all child org units), you'll need a more complex filter
-- or use the OrgUnitFilterService approach