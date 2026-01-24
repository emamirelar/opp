
TRUNCATE TABLE public."EntityPermissions";

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
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "Status != 2", "CanDelete": "PartnerApprovalStatus != \"Approved\""}'
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
    true,
    '{"CanRead": [], "CanCreate": [], "CanUpdate": ["Id", "Name", "PartnerShortDescription", "PartnerLongDescription", "PartnerCategoryId", "PartnerFocalPointUserId", "PartnerLiasionOfficeId", "PartnerGroupId", "OrganizationHierarchyIds", "OrganizationUnitRelationships"], "CanDelete": []}',
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || CreatedBy == @currentUserId", "CanDelete": "Status == 3 && (OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || CreatedBy == @currentUserId)"}'
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
    true,
    true,
    true,
    '{"CanRead": [], "CanCreate": [], "CanUpdate": ["Id", "Name", "PartnerShortDescription", "PartnerLongDescription", "PartnerCategoryId", "PartnerFocalPointUserId", "PartnerLiasionOfficeId", "PartnerGroupId", "OrganizationHierarchyIds", "OrganizationUnitRelationships"], "CanDelete": []}',
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || CreatedBy == @currentUserId", "CanDelete": "Status == 3 && (OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || CreatedBy == @currentUserId)"}'
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
    '{"CanRead": "", "CanCreate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\")", "CanUpdate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || CreatedBy == @currentUserId", "CanDelete": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || CreatedBy == @currentUserId"}'
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
    '{"CanRead": "", "CanCreate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\")", "CanUpdate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || CreatedBy == @currentUserId", "CanDelete": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || CreatedBy == @currentUserId"}'
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
    true,
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


-- PartnerTreeManagement Entity Permissions (for route/page access control)

-- UNOPS General User role permissions for PartnerTreeManagement
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
    'PartnerTreeManagement',
    'UNOPS_GEN_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for PartnerTreeManagement
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
    'PartnerTreeManagement',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for PartnerTreeManagement (NO PAGE ACCESS)
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
    'PartnerTreeManagement',
    'PARTNER_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for PartnerTreeManagement (NO PAGE ACCESS)
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
    'PartnerTreeManagement',
    'ORG_UNIT_ADMIN',
    false,
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
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || InteractionUsers.Any(iu => iu.UserId == @currentUserId)", "CanDelete": ""}'
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
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || InteractionUsers.Any(iu => iu.UserId == @currentUserId) || CreatedBy == @currentUserId", "CanDelete": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || InteractionUsers.Any(iu => iu.UserId == @currentUserId) || CreatedBy == @currentUserId"}'
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
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || InteractionUsers.Any(iu => iu.UserId == @currentUserId) || CreatedBy == @currentUserId", "CanDelete": "OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == \"OrgUnit\") || InteractionUsers.Any(iu => iu.UserId == @currentUserId) || CreatedBy == @currentUserId"}'
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

-- PartnershipAgreement Entity Permissions

-- UNOPS General User role permissions for PartnershipAgreement
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
    'PartnershipAgreement',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for PartnershipAgreement
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
    'PartnershipAgreement',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for PartnershipAgreement
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
    'PartnershipAgreement',
    'PARTNER_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for PartnershipAgreement
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
    'PartnershipAgreement',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- BaseEngagement Entity Permissions (read-only, externally managed)

-- UNOPS General User role permissions for BaseEngagement
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
    'BaseEngagement',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnership Global Admin role permissions for BaseEngagement
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
    'BaseEngagement',
    'PARTNER_GLOB_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnerships User role permissions for BaseEngagement
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
    'BaseEngagement',
    'PARTNER_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Org Unit Admin role permissions for BaseEngagement
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
    'BaseEngagement',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- PartnerCategory Entity Permissions (read-only, derived from PartnerTree)

-- UNOPS General User role permissions for PartnerCategory
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
    'PartnerCategory',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnership Global Admin role permissions for PartnerCategory
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
    'PartnerCategory',
    'PARTNER_GLOB_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnerships User role permissions for PartnerCategory
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
    'PartnerCategory',
    'PARTNER_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Org Unit Admin role permissions for PartnerCategory
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
    'PartnerCategory',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- PartnerGroup Entity Permissions (read-only, derived from PartnerTree)

-- UNOPS General User role permissions for PartnerGroup
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
    'PartnerGroup',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnership Global Admin role permissions for PartnerGroup
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
    'PartnerGroup',
    'PARTNER_GLOB_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnerships User role permissions for PartnerGroup
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
    'PartnerGroup',
    'PARTNER_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Org Unit Admin role permissions for PartnerGroup
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
    'PartnerGroup',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Country Entity Permissions (read-only lookup operations)

-- UNOPS General User role permissions for Country
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
    'Country',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnership Global Admin role permissions for Country
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
    'Country',
    'PARTNER_GLOB_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnerships User role permissions for Country
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
    'Country',
    'PARTNER_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Org Unit Admin role permissions for Country
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
    'Country',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- LiaisonOffice Entity Permissions

-- UNOPS General User role permissions for LiaisonOffice
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
    'LiaisonOffice',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnership Global Admin role permissions for LiaisonOffice
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
    'LiaisonOffice',
    'PARTNER_GLOB_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnerships User role permissions for LiaisonOffice
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
    'LiaisonOffice',
    'PARTNER_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Org Unit Admin role permissions for LiaisonOffice
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
    'LiaisonOffice',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- OrganizationHierarchy Entity Permissions

-- UNOPS General User role permissions for OrganizationHierarchy
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
    'OrganizationHierarchy',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnership Global Admin role permissions for OrganizationHierarchy
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
    'OrganizationHierarchy',
    'PARTNER_GLOB_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Partnerships User role permissions for OrganizationHierarchy
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
    'OrganizationHierarchy',
    'PARTNER_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Org Unit Admin role permissions for OrganizationHierarchy
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
    'OrganizationHierarchy',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": ""}'
);

-- Translation Entity Permissions

-- UNOPS General User role permissions for Translation
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
    'Translation',
    'UNOPS_GEN_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin role permissions for Translation
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
    'Translation',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User role permissions for Translation
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
    'Translation',
    'PARTNER_USER',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin role permissions for Translation
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
    'Translation',
    'ORG_UNIT_ADMIN',
    false,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Opportunity Entity Permissions
-- Any user on the Team (OpportunityStakeholder) can edit - this is handled in code via PermissionService
-- UNOPS General User: Read only (no create/update/delete)
-- Edit access can be granted via Team membership (handled in code)
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
    'Opportunity',
    'UNOPS_GEN_USER',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnership Global Admin: Full access
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
    'Opportunity',
    'PARTNER_GLOB_ADMIN',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Partnerships User: Full access
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
    'Opportunity',
    'PARTNER_USER',
    true,
    true,
    true,
    true,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);

-- Org Unit Admin: Read only (no create/update/delete)
-- Edit access can be granted via Team membership (handled in code)
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
    'Opportunity',
    'ORG_UNIT_ADMIN',
    true,
    false,
    false,
    false,
    null,
    '{"CanRead": "", "CanCreate": "", "CanUpdate": "", "CanDelete": ""}'
);