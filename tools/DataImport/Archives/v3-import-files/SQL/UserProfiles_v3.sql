-- Insert UserProfile for System User
-- This script will insert the UserProfile record for the Opportunity+ system user
-- This is a special system user account used for automated operations

DO $$
DECLARE 
    inserted_count INTEGER;
BEGIN
    INSERT INTO public."UserProfile" ("Id", "UserId", "FirstName", "LastName", "Name", "UserEmail", "OrgUnit", "SupervisorId", "DutyStation", "Position", "CreatedDate", "CreatedBy", "LastModifiedBy", "LastModifiedDate", "DeletedBy", "DeletedDate", "IsDeleted", "Status")
    SELECT * FROM (VALUES
        (-1, -1, 'Opportunity+', 'System', 'Opportunity+ System', 'opportunityplus@unops.org', NULL::varchar, NULL::integer, 'System', 'Automated System User', NOW(), -1, -1, NOW(), -1, NULL::timestamp with time zone, false, 1)
    ) AS new_data("Id", "UserId", "FirstName", "LastName", "Name", "UserEmail", "OrgUnit", "SupervisorId", "DutyStation", "Position", "CreatedDate", "CreatedBy", "LastModifiedBy", "LastModifiedDate", "DeletedBy", "DeletedDate", "IsDeleted", "Status")
    WHERE NOT EXISTS (
        SELECT 1 FROM public."UserProfile" WHERE "Id" = new_data."Id"
    );
    
    GET DIAGNOSTICS inserted_count = ROW_COUNT;
    RAISE NOTICE 'Inserted % UserProfile records (System User)', inserted_count;
END $$;