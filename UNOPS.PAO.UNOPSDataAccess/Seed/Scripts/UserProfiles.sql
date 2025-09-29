-- Insert UserProfile data
-- This script will insert UserProfile records if they don't already exist

INSERT INTO public."UserProfile" ("Id", "UserId", "FirstName", "LastName", "Name", "UserEmail", "OrgUnit", "SupervisorId", "DutyStation", "Position", "CreatedDate", "CreatedBy", "LastModifiedBy", "LastModifiedDate", "DeletedBy", "DeletedDate", "IsDeleted", "Status")
SELECT * FROM (VALUES
    (696, 203964, 'Michael', 'Rixon', 'Michael Rixon', 'MichaelRI@unops.org', 'B0047', 146714, 'Copenhagen - Denmark', 'Partnerships Specialist', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (2106, 214695, 'Louise Amelie', 'MENYE MENGUENE', 'Louise Amelie MENYE MENGUENE', 'menyeamelie@yahoo.com', 'B5328', 223852, 'Douala - Cameroon', 'UNHCR', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (5340, 229674, 'Asbjorn', 'Brink', 'Asbjorn Brink', 'asbjornb@unops.org', 'B0047', 215072, 'Copenhagen - Denmark', 'Partnerships Senior Advisor - Liaison', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (11216, 146714, 'Arnaud', 'Sgambato', 'Arnaud Sgambato', 'ArnaudS@unops.org', 'B0047', 215072, 'Copenhagen - Denmark', 'Head of Unit - Partnerships Development', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (11276, 149329, 'Hala R', 'Alsharifi', 'Hala R Alsharifi', 'HalaS@unops.org', 'B5120', 215072, 'Amman - Jordan', 'Partnerships Advisor for Gulf countries', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (12365, 178636, 'Laurentiu', 'Mastacan', 'Laurentiu Mastacan', 'LaurentiuM@unops.org', 'B0047', 87714, 'Copenhagen - Denmark', 'Partnerships Senior Analyst', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (12637, 206589, 'Yuko', 'MAEKAWA', 'Yuko MAEKAWA', 'YukoM@unops.org', 'B0047', 215072, 'Tokyo - Japan', 'Partnerships Advisor - Liaison', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (12773, 215103, 'Isabela', 'FRAIZ SOMOZA', 'Isabela FRAIZ SOMOZA', 'IsabelaF@unops.org', 'B0047', 203964, 'Panama City - Panama', 'Partnerships Senior Analyst', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (12805, 218090, 'Kajsa Johanna', 'Hartman', 'Kajsa Johanna Hartman', 'KajsaH@unops.org', 'B5002', 1500, 'Vienna - Austria', 'Project Management Support - Specialist', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (12944, 221996, 'Mikaela Solfrid', 'Gerkman', 'Mikaela Solfrid Gerkman', 'MikaelaG@unops.org', 'B0047', 203964, 'Copenhagen - Denmark', 'Partnerships Senior Analyst - Analysis and Reporting', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (13054, 226146, 'Michael', 'Patrick Ellsworth', 'Michael Patrick Ellsworth', 'PatrickEL@unops.org', 'B0047', 215072, 'Washington DC - United States of America', 'Partnerships Advisor - Liaison', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (13075, 226953, 'Christine', 'BOWERS', 'Christine BOWERS', 'ChristineBO@unops.org', 'B0047', 215072, 'Washington DC - United States of America', 'Partnerships Senior Advisor - Liaison', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (13118, 228474, 'Jose Ignacio', 'Monzon Egana', 'Jose Ignacio Monzon Egana', 'JoseME@unops.org', 'B0047', 203964, 'Copenhagen - Denmark', 'Partnerships Analyst', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (13410, 29737, 'Martin Carlos Eduardo', 'AREVALO DE LEON', 'Martin Carlos Eduardo AREVALO DE LEON', 'martina@unops.org', 'B0047', 215072, 'Rome - Italy', 'Partnerships Senior Advisor - Liaison', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (13429, 40036, 'Mariacarmen', 'COLITTI', 'Mariacarmen COLITTI', 'mariacarmenco@unops.org', 'B0047', 215072, 'Brussels - Belgium', 'Partnerships Senior Advisor - Liaison', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (13438, 44918, 'Laetitia', 'Kraus', 'Laetitia Kraus', 'LaetitiaK@unops.org', 'B0047', 40036, 'Brussels - Belgium', 'Partnerships Advisor - Liaison', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1),
    (13719, 87714, 'Lorraine', 'Aweidah-Anabtawi', 'Lorraine Aweidah-Anabtawi', 'LorraineA@unops.org', 'B0047', 146714, 'Jerusalem - Israel', 'Business Technical Specialist (Human Resources)', NOW(), 1, 1, NOW(), 1, NULL::timestamp with time zone, false, 1)
) AS new_data("Id", "UserId", "FirstName", "LastName", "Name", "UserEmail", "OrgUnit", "SupervisorId", "DutyStation", "Position", "CreatedDate", "CreatedBy", "LastModifiedBy", "LastModifiedDate", "DeletedBy", "DeletedDate", "IsDeleted", "Status")
WHERE NOT EXISTS (
    SELECT 1 FROM public."UserProfile" WHERE "Id" = new_data."Id"
);