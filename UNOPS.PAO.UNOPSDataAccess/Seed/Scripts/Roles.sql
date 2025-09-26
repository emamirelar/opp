
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