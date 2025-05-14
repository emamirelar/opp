-- Entity Permissions seed data

-- Clean up any existing data first
DELETE FROM public."EntityPermissions";

-- Reset the sequence to start from 1
ALTER SEQUENCE public."EntityPermissions_Id_seq" RESTART WITH 1;

-- Administrator role permissions (full access to all entities)
INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Read', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Create', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Update', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Delete', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Read', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Create', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Update', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Delete', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Read', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Create', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Update', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Delete', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Read', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Create', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Update', 'Administrator', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Delete', 'Administrator', NULL, NULL);

-- Internal role permissions
INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Read', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Create', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Update', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Read', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Create', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Update', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Read', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Create', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Update', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Read', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Create', 'Internal', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Update', 'Internal', NULL, NULL);

-- Partner role permissions (can only read most things, manage their own content)
INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Read', 'Partner', NULL, 'CreatedBy == CurrentUser');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Read', 'Partner', NULL, 'CreatedBy == CurrentUser');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Create', 'Partner', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Update', 'Partner', NULL, 'CreatedBy == CurrentUser');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Read', 'Partner', NULL, 'CreatedBy == CurrentUser');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Create', 'Partner', NULL, NULL);

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Update', 'Partner', NULL, 'CreatedBy == CurrentUser');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Read', 'Partner', NULL, 'CreatedBy == CurrentUser');

-- External role permissions (limited access)
INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Read', 'External', NULL, 'IsPublic == true');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Read', 'External', NULL, 'IsPublic == true');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Read', 'External', NULL, 'IsPublic == true');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Read', 'External', NULL, 'IsPublic == true');

-- Add User role permissions
INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Partner', 'Read', 'User', NULL, 'IsPublic == true');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Contact', 'Read', 'User', NULL, 'IsPublic == true');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Document', 'Read', 'User', NULL, 'IsPublic == true');

INSERT INTO public."EntityPermissions" ("EntityName", "Action", "RoleName", "PropertyName", "FilterExpression")
VALUES ('Project', 'Read', 'User', NULL, 'IsPublic == true');

-- Verify the data was inserted
SELECT * FROM public."EntityPermissions"; 