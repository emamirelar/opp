-- Clean up existing entities
DELETE FROM public."Entities";

-- Reset the sequence to start from 1
ALTER SEQUENCE public."Entities_Id_seq" RESTART WITH 1;

--select * from public."Entities"

-- Insert core entities
INSERT INTO public."Entities" (
    "EntityName", 
    "Name", 
    "Status", 
    "IsActive", 
    "CanManage",
    "CreatedBy", 
    "CreatedDate",
	"LastModifiedBy",
	"LastModifiedDate",
	"IsDeleted",
	"DeletedBy",
	"DeletedDate"
) VALUES 
    ('Contact', 'Contact', 0, true, true, 1, NOW(), 0, NULL, false, 0, NULL),
    ('Partner', 'Partner', 0, true, true, 1, NOW(), 0, NULL, false, 0, NULL),
    ('Interaction', 'Interaction', 0, true, true, 1, NOW(), 0, NULL, false, 0, NULL),
    ('PartnerTree', 'PartnerTree', 0, true, true, 1, NOW(), 0, NULL, false, 0, NULL),
    ('OrganizationHierarchy', 'OrganizationHierarchy', 0, true, false, 1, NOW(), 0, NULL, false, 0, NULL);

-- Select to verify the data
SELECT * FROM public."Entities" ORDER BY "Id"; 