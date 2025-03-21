TRUNCATE TABLE public."AiScreenMapping";
INSERT INTO public."AiScreenMapping" ("Type", "TableName", "ComparisonKey", "RelatedEntity", "RelatedEntityKey", "QueryConditions", "Order", "CreatedAt", "Name", "Status") VALUES 
('partner_interactions_summary', 'Partners', 'Id', 'Contacts', 'PartnerId', NULL, 1, NOW(), 'Partners', 1),
('partner_interactions_summary', 'Contacts', 'Id', 'Interactions', 'ContactId', NULL, 2, NOW(), 'Partners', 1),
('partner_risk_profile', 'Partners', 'Id', 'PartnerProjects', 'PartnersId', NULL, 1, NOW(), 'Partners', 1),
('partner_risk_profile', 'PartnerProjects', 'ProjectsId', 'Projects', 'Id', NULL, 2, NOW(), 'Partners', 1),
('partner_news', 'Partners', 'Id', NULL, NULL, NULL, 1, NOW(), 'Partners', 1);