-- Insert Liaison Office data (one-time migration script)
-- This script populates the LiaisonOffices table with UNOPS office data

INSERT INTO public."LiaisonOffices" ("Code", "Name", "Description", "Region", "Country", "IsActive", "CreatedBy", "CreatedDate") VALUES
-- Required Liaison Offices as specified by user requirements
('LO-WDC', 'Washington Liaison Office', 'Washington Liaison Office', 'Americas', 'United States', true, 0, NOW()),
('LO-GCC', 'Gulf Countries Liaison Office', 'Gulf Countries Liaison Office', 'Middle East', 'UAE', true, 0, NOW()),
('LO-OTH', 'Other Partners', 'Other Partners', 'Global', 'Global', true, 0, NOW()),
('LO-NEU', 'Northern Europe Liaison Office', 'Northern Europe Liaison Office', 'Europe', 'Sweden', true, 0, NOW()),
('LO-ROM', 'Rome Liaison Office', 'Rome Liaison Office', 'Europe', 'Italy', true, 0, NOW()),
('LO-TOK', 'Tokyo Liaison Office', 'Tokyo Liaison Office', 'Asia-Pacific', 'Japan', true, 0, NOW()),
('LO-BRU', 'Brussels Liaison Office', 'Brussels Liaison Office', 'Europe', 'Belgium', true, 0, NOW()),
('LO-GVA', 'Geneva Liaison Office', 'Geneva Liaison Office', 'Europe', 'Switzerland', true, 0, NOW()),
('LO-NAI', 'Nairobi Liaison Office', 'Nairobi Liaison Office', 'Africa', 'Kenya', true, 0, NOW()),
('LO-NYO', 'New York Liaison Office', 'New York Liaison Office', 'Americas', 'United States', true, 0, NOW()),
('LO-PLG', 'Other PLG Managed Partners', 'Other PLG Managed Partners', 'Global', 'Global', true, 0, NOW()),
('LO-MAN', 'Manila Liaison Office', 'Manila Liaison Office', 'Asia-Pacific', 'Philippines', true, 0, NOW());
