-- Insert Liaison Office data (one-time migration script)
-- This script populates the LiaisonOffices table with UNOPS office data

INSERT INTO public."LiaisonOffices" (
    "Code", "Name", "Description", "Region", "Country", "IsActive", 
    "Status", "IsDeleted", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "DeletedBy", "DeletedDate"
) VALUES
-- Required Liaison Offices as specified by user requirements
('LO-WDC', 'Washington Liaison Office', 'Washington Liaison Office', 'Americas', 'United States', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-GCC', 'Gulf Countries Liaison Office', 'Gulf Countries Liaison Office', 'Middle East', 'UAE', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-OTH', 'Other Partners', 'Other Partners', 'Global', 'Global', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-NEU', 'Northern Europe Liaison Office', 'Northern Europe Liaison Office', 'Europe', 'Sweden', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-ROM', 'Rome Liaison Office', 'Rome Liaison Office', 'Europe', 'Italy', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-TOK', 'Tokyo Liaison Office', 'Tokyo Liaison Office', 'Asia-Pacific', 'Japan', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-BRU', 'Brussels Liaison Office', 'Brussels Liaison Office', 'Europe', 'Belgium', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-GVA', 'Geneva Liaison Office', 'Geneva Liaison Office', 'Europe', 'Switzerland', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-NAI', 'Nairobi Liaison Office', 'Nairobi Liaison Office', 'Africa', 'Kenya', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-NYO', 'New York Liaison Office', 'New York Liaison Office', 'Americas', 'United States', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-PLG', 'Other PLG Managed Partners', 'Other PLG Managed Partners', 'Global', 'Global', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL),
('LO-MAN', 'Manila Liaison Office', 'Manila Liaison Office', 'Asia-Pacific', 'Philippines', true, 1, false, 0, NOW(), 0, NOW(), 0, NULL);