-- Clean up existing roles
DELETE FROM public."AspNetRoles";

-- Reset the sequence to start from 1
ALTER SEQUENCE public."AspNetRoles_Id_seq" RESTART WITH 1;

--select * from public."AspNetRoles";

-- Insert new roles
INSERT INTO public."AspNetRoles" ("Name", "NormalizedName", "Description")
VALUES 
    ('UNOPS_GEN_USER', 'UNOPS_GEN_USER', 'General User'),
    ('PARTNER_GLOB_ADMIN', 'PARTNER_GLOB_ADMIN', 'Partnership Global Admin'),
    ('PARTNER_USER', 'PARTNER_USER', 'Partnership User'),
    ('ORG_UNIT_ADMIN', 'ORG_UNIT_ADMIN', 'Org Unit Admin');

-- Clean up existing entity permissions
DELETE FROM public."EntityPermissions";

-- Reset the sequence to start from 1
ALTER SEQUENCE public."EntityPermissions_Id_seq" RESTART WITH 1;

-- Partner Entity Permissions

-- UNOPS General User role permissions for Partner
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Partner',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for Partner
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Partner',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "Status != 2 && Status != 4", "CanDelete": ""}'
);

-- Partnerships User role permissions for Partner
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Partner',
    'PARTNER_USER',
    true,
    true,
    true,
    false,
    '{"CanRead": [], "CanCreate": [], "CanUpdate": ["Id", "Name", "PartnerShortDescription", "PartnerLongDescription", "PartnerCategoryId", "PartnerFocalPointUserId", "PartnerLevelCode", "PartnerLevelShort", "PartnerLevelDescription"], "CanDelete": []}',
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "PartnerApprovalStatus == 0 && OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == 3)", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for Partner (NO ACCESS)
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Partner',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Contact Entity Permissions

-- UNOPS General User role permissions for Contact
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Contact',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for Contact
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Contact',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for Contact
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Contact',
    'PARTNER_USER',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "Partner != null && Partner.PartnerOffice != null && Partner.PartnerOffice.Code == @userOrgUnit", "CanUpdate": "Partner != null && Partner.PartnerOffice != null && Partner.PartnerOffice.Code == @userOrgUnit", "CanDelete": "Partner != null && Partner.PartnerOffice != null && Partner.PartnerOffice.Code == @userOrgUnit"}'
);

-- Org Unit Admin role permissions for Contact
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Contact',
    'ORG_UNIT_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "Partner != null && Partner.PartnerOffice != null && Partner.PartnerOffice.Code == @userOrgUnit", "CanUpdate": "Partner != null && Partner.PartnerOffice != null && Partner.PartnerOffice.Code == @userOrgUnit", "CanDelete": "Partner != null && Partner.PartnerOffice != null && Partner.PartnerOffice.Code == @userOrgUnit"}'
);

-- PartnerTree Entity Permissions

-- UNOPS General User role permissions for PartnerTree
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'PartnerTree',
    'UNOPS_GEN_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for PartnerTree
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'PartnerTree',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for PartnerTree
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'PartnerTree',
    'PARTNER_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for PartnerTree
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'PartnerTree',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Interaction Entity Permissions

-- UNOPS General User role permissions for Interaction
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Interaction',
    'UNOPS_GEN_USER',
    true,
    false,
    true,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || InteractionUsers.Any(iu => iu.UserId == @currentUserId)", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for Interaction
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Interaction',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for Interaction
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Interaction',
    'PARTNER_USER',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || InteractionUsers.Any(iu => iu.UserId == @currentUserId)", "CanDelete": "OrgUnit != null && OrgUnit.Code == @userOrgUnit"}'
);

-- Org Unit Admin role permissions for Interaction
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'Interaction',
    'ORG_UNIT_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || InteractionUsers.Any(iu => iu.UserId == @currentUserId)", "CanDelete": "OrgUnit != null && OrgUnit.Code == @userOrgUnit"}'
);

-- UserManagement Entity Permissions

-- Partnership Global Admin role permissions for UserManagement
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'UserManagement',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for UserManagement
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'UserManagement',
    'ORG_UNIT_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "OrgUnit == @userOrgUnit", "CanCreate": "OrgUnit == @userOrgUnit", "CanUpdate": "OrgUnit == @userOrgUnit", "CanDelete": "OrgUnit == @userOrgUnit"}'
);

-- AiPromptManagement Entity Permissions

-- UNOPS General User role permissions for AiPromptManagement
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'AiPromptManagement',
    'UNOPS_GEN_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for AiPromptManagement
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'AiPromptManagement',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for AiPromptManagement
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'AiPromptManagement',
    'PARTNER_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for AiPromptManagement
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'AiPromptManagement',
    'ORG_UNIT_ADMIN',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Assign UNOPS_GEN_USER role to all existing users
INSERT INTO public."AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM public."AspNetUsers" u
CROSS JOIN public."AspNetRoles" r
WHERE r."Name" = 'UNOPS_GEN_USER'
AND NOT EXISTS (
    SELECT 1 
    FROM public."AspNetUserRoles" ur 
    WHERE ur."UserId" = u."Id" 
    AND ur."RoleId" = r."Id"
);

-- EntityManager Entity Permissions

-- UNOPS General User role permissions for EntityManager
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'EntityManager',
    'UNOPS_GEN_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for EntityManager
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'EntityManager',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for EntityManager
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'EntityManager',
    'PARTNER_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for EntityManager
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'EntityManager',
    'ORG_UNIT_ADMIN',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- EntityFieldManager Entity Permissions

-- UNOPS General User role permissions for EntityFieldManager
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'EntityFieldManager',
    'UNOPS_GEN_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for EntityFieldManager
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'EntityFieldManager',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for EntityFieldManager
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'EntityFieldManager',
    'PARTNER_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for EntityFieldManager
INSERT INTO public."EntityPermissions" (
    "Entity", 
    "Role", 
    "CanRead", 
    "CanCreate", 
    "CanUpdate", 
    "CanDelete", 
    "PropertyFilter", 
    "RowFilter"
) VALUES (
    'EntityFieldManager',
    'ORG_UNIT_ADMIN',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);