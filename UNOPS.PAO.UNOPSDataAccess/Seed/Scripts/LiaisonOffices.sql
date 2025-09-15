-- Liaison Offices configuration
-- This script manages liaison office definitions

-- Clear existing data and reset
TRUNCATE TABLE public."LiaisonOffices" RESTART IDENTITY CASCADE;

-- Insert Other Partners liaison office
INSERT INTO public."LiaisonOffices" (
    "Code", "Name", "IsActive", "Status", "CreatedBy", "CreatedDate", 
    "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy"
)
VALUES 
('a0bQx000000jsXKIAY', 'Other Partners', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXLIAY', 'Northern Europe Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXMIAY', 'Washington Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXNIAY', 'Tokyo Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXOIAY', 'Manila Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXPIAY', 'Gulf Countries Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXQIAY', 'Brussels Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXRIAY', 'New York Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXSIAY', 'Geneva Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000jsXTIAY', 'Nairobi Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx000000sTEjIAM', 'Bangkok Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx00000BPmpVIAT', 'Rome Liaison Office', true, 1, 0, NOW(), 0, NOW(), false, 0),
('a0bQx00000CT3YIIA1', 'Other PLG Managed Partners', true, 1, 0, NOW(), 0, NOW(), false, 0);
