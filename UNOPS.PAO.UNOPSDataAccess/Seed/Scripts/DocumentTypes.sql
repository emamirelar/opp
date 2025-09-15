-- Document Types configuration
-- This script manages document type definitions for different entities

-- Clear existing data and reset
TRUNCATE TABLE public."DocumentTypes" RESTART IDENTITY CASCADE;

-- Insert Partner - Guidance document type
INSERT INTO public."DocumentTypes" (
    "EntityType", "Name", "Status", "CreatedBy", "CreatedDate", 
    "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy"
)
VALUES 
('Partner', 'Guidance', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Partnership Agreement', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Standard Template', 1, 0, NOW(), 0, NOW(), false, 0),
('Contact', 'Other', 1, 0, NOW(), 0, NOW(), false, 0),
('Interaction', 'Meeting Note', 1, 0, NOW(), 0, NOW(), false, 0),
('PartnerTree', 'Other', 1, 0, NOW(), 0, NOW(), false, 0);
