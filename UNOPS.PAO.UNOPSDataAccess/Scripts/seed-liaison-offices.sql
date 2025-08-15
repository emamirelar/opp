-- Insert Liaison Office data (one-time migration script)
-- This script populates the LiaisonOffices table with UNOPS office data

INSERT INTO public."LiaisonOffices" ("Code", "Name", "Description", "Region", "Country", "IsActive", "CreatedBy", "CreatedDate") VALUES
('HQ-NYO', 'UNOPS New York Office', 'Headquarters office in New York', 'Americas', 'United States', true, 0, NOW()),
('HQ-CPH', 'UNOPS Copenhagen Office', 'Headquarters office in Copenhagen', 'Europe', 'Denmark', true, 0, NOW()),
('RO-BKK', 'Regional Office Bangkok', 'Asia Regional Office', 'Asia-Pacific', 'Thailand', true, 0, NOW()),
('RO-PAN', 'Regional Office Panama', 'Americas Regional Office', 'Americas', 'Panama', true, 0, NOW()),
('RO-DAK', 'Regional Office Dakar', 'Africa Regional Office', 'Africa', 'Senegal', true, 0, NOW()),
('RO-CAI', 'Regional Office Cairo', 'Middle East Regional Office', 'Middle East', 'Egypt', true, 0, NOW()),
('RO-KYV', 'Regional Office Kyiv', 'Europe Regional Office', 'Europe', 'Ukraine', true, 0, NOW()),
('LO-GVA', 'Liaison Office Geneva', 'Geneva Liaison Office', 'Europe', 'Switzerland', true, 0, NOW()),
('LO-BRU', 'Liaison Office Brussels', 'Brussels Liaison Office', 'Europe', 'Belgium', true, 0, NOW()),
('LO-WDC', 'Liaison Office Washington DC', 'Washington DC Liaison Office', 'Americas', 'United States', true, 0, NOW()),
('LO-TOK', 'Liaison Office Tokyo', 'Tokyo Liaison Office', 'Asia-Pacific', 'Japan', true, 0, NOW()),
('LO-LON', 'Liaison Office London', 'London Liaison Office', 'Europe', 'United Kingdom', true, 0, NOW()),
('LO-BON', 'Liaison Office Bonn', 'Bonn Liaison Office', 'Europe', 'Germany', true, 0, NOW()),
('LO-ROM', 'Liaison Office Rome', 'Rome Liaison Office', 'Europe', 'Italy', true, 0, NOW());
