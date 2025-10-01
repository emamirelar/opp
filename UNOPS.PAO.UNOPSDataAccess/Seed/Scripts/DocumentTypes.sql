-- Document Types configuration
-- This script manages document type definitions for different entities

-- Clear existing data and reset
TRUNCATE TABLE public."DocumentTypes" RESTART IDENTITY CASCADE;

-- Insert document types for different entities
INSERT INTO public."DocumentTypes" (
    "EntityType", "Name", "Status", "CreatedBy", "CreatedDate",
    "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy"
)
VALUES
-- Partner document types
('Partner', 'Partner/National strategic plan', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Concept note', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Action plan', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Mission report', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Draft/working document', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Presentation', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Factsheet', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Reports produced by partner', 1, 0, NOW(), 0, NOW(), false, 0),
('Partner', 'Other', 1, 0, NOW(), 0, NOW(), false, 0),

-- Contact document types
('Contact', 'CV/Bio', 1, 0, NOW(), 0, NOW(), false, 0),
('Contact', 'Talking points', 1, 0, NOW(), 0, NOW(), false, 0),
('Contact', 'Background note', 1, 0, NOW(), 0, NOW(), false, 0),
('Contact', 'Other', 1, 0, NOW(), 0, NOW(), false, 0),

-- Interaction document types
('Interaction', 'Minutes', 1, 0, NOW(), 0, NOW(), false, 0),
('Interaction', 'Action plan/next steps', 1, 0, NOW(), 0, NOW(), false, 0),
('Interaction', 'Supporting materials', 1, 0, NOW(), 0, NOW(), false, 0),
('Interaction', 'Other', 1, 0, NOW(), 0, NOW(), false, 0),

-- PartnerTree document types
('PartnerTree', 'Other', 1, 0, NOW(), 0, NOW(), false, 0);
