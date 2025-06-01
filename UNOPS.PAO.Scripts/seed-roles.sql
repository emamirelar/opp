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
    null
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
    null
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
    false,
    true,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "PartnerOfficeId == @orgUnitId", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for Partner
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
    true,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "PartnerOfficeId == @orgUnitId", "CanDelete": ""}'
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
    null
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
    null
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
    '{"CanRead": "", "CanCreate": "PartnerOfficeId == @orgUnitId", "CanUpdate": "PartnerOfficeId == @orgUnitId", "CanDelete": "PartnerOfficeId == @orgUnitId"}'
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
    '{"CanRead": "", "CanCreate": "PartnerOfficeId == @orgUnitId", "CanUpdate": "PartnerOfficeId == @orgUnitId", "CanDelete": "PartnerOfficeId == @orgUnitId"}'
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
    null
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
    null
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
    false,
    false,
    false,
    false,
    null,
    null
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
    false,
    false,
    false,
    false,
    null,
    null
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
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "OrgUnitId == @orgUnitId", "CanDelete": ""}'
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
    null
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
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "OrgUnitId == @orgUnitId", "CanDelete": "OrgUnitId == @orgUnitId"}'
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
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "OrgUnitId == @orgUnitId", "CanDelete": "OrgUnitId == @orgUnitId"}'
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
    null
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
    '{"CanRead": "OrgUnit == @orgUnit", "CanCreate": "OrgUnit == @orgUnit", "CanUpdate": "OrgUnit == @orgUnit", "CanDelete": "OrgUnit == @orgUnit"}'
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
    null
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
    null
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
    null
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
    null
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