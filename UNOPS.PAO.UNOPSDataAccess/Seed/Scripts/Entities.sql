-- Entities configuration - Basic entity definitions for the system
-- This script manages the core entity table that defines what entities exist

-- Clear existing data and reset
TRUNCATE TABLE public."Entities" RESTART IDENTITY CASCADE;

-- Insert Contact entity
INSERT INTO public."Entities" (
    "EntityName", "Name", "Status", "IsActive", "CanManage", 
    "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", 
    "IsDeleted", "DeletedBy", "DeletedDate"
)
VALUES 
('Contact', 'Contact', 0, true, true, 1, NOW(), 0, NULL, false, 0, NULL),
('Partner', 'Partner', 0, true, true, 1, NOW(), 0, NULL, false, 0, NULL),
('Interaction', 'Interaction', 0, true, true, 1, NOW(), 0, NULL, false, 0, NULL),
('PartnerTree', 'PartnerTree', 0, true, true, 1, NOW(), 0, NULL, false, 0, NULL),
('OrganizationHierarchy', 'OrganizationHierarchy', 0, true, false, 1, NOW(), 0, NULL, false, 0, NULL);
