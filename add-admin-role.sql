-- Script to assign Administrator role to a user

-- First, get the current user information
SELECT * FROM public."AspNetUsers" WHERE "Email" = 'anushas@unops.org';

-- Get the Administrator role ID
SELECT * FROM public."AspNetRoles" WHERE "Name" = 'Administrator';

-- Insert user role assignment (replace UserId and RoleId with actual values)
INSERT INTO public."AspNetUserRoles" ("UserId", "RoleId")
VALUES ('4', 'administrator-role-id');

-- Verify the user has the Administrator role
SELECT u."Email", r."Name" as "RoleName"
FROM public."AspNetUsers" u
JOIN public."AspNetUserRoles" ur ON u."Id" = ur."UserId"
JOIN public."AspNetRoles" r ON ur."RoleId" = r."Id"
WHERE u."Email" = 'anushas@unops.org'; 