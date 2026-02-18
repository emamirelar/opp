-- =====================================================
-- Setup Opportunity Entity Permissions
-- =====================================================

-- Insert Opportunity permissions for UNOPS_GEN_USER
INSERT INTO "EntityPermissions" ("Entity", "Role", "RowFilter", "PropertyFilter", "CanCreate", "CanDelete", "CanRead", "CanUpdate")
SELECT 
    'Opportunity',
    'UNOPS_GEN_USER',
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || OpportunityCollaborators.Any(oc => oc.UserId == @currentUserId)", "CanDelete": ""}',
    '',
    false,
    false,
    true,
    true
WHERE NOT EXISTS (
    SELECT 1 FROM "EntityPermissions" 
    WHERE "Entity" = 'Opportunity' AND "Role" = 'UNOPS_GEN_USER'
);

-- Insert Opportunity permissions for PARTNER_GLOB_ADMIN
INSERT INTO "EntityPermissions" ("Entity", "Role", "RowFilter", "PropertyFilter", "CanCreate", "CanDelete", "CanRead", "CanUpdate")
SELECT 
    'Opportunity',
    'PARTNER_GLOB_ADMIN',
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}',
    '',
    true,
    true,
    true,
    true
WHERE NOT EXISTS (
    SELECT 1 FROM "EntityPermissions" 
    WHERE "Entity" = 'Opportunity' AND "Role" = 'PARTNER_GLOB_ADMIN'
);

-- Insert Opportunity permissions for PARTNER_USER
INSERT INTO "EntityPermissions" ("Entity", "Role", "RowFilter", "PropertyFilter", "CanCreate", "CanDelete", "CanRead", "CanUpdate")
SELECT 
    'Opportunity',
    'PARTNER_USER',
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || OpportunityCollaborators.Any(oc => oc.UserId == @currentUserId)", "CanDelete": "OrgUnit != null && OrgUnit.Code == @userOrgUnit"}',
    '',
    true,
    true,
    true,
    true
WHERE NOT EXISTS (
    SELECT 1 FROM "EntityPermissions" 
    WHERE "Entity" = 'Opportunity' AND "Role" = 'PARTNER_USER'
);

-- Insert Opportunity permissions for ORG_UNIT_ADMIN
INSERT INTO "EntityPermissions" ("Entity", "Role", "RowFilter", "PropertyFilter", "CanCreate", "CanDelete", "CanRead", "CanUpdate")
SELECT 
    'Opportunity',
    'ORG_UNIT_ADMIN',
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || OpportunityCollaborators.Any(oc => oc.UserId == @currentUserId)", "CanDelete": "OrgUnit != null && OrgUnit.Code == @userOrgUnit"}',
    '',
    true,
    true,
    true,
    true
WHERE NOT EXISTS (
    SELECT 1 FROM "EntityPermissions" 
    WHERE "Entity" = 'Opportunity' AND "Role" = 'ORG_UNIT_ADMIN'
);
