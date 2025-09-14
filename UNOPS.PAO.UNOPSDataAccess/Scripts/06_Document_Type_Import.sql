-- Insert DocumentTypes individually to avoid transaction rollback on single failure
INSERT INTO public."DocumentTypes"("EntityType", "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate")
VALUES ('Partner', 'Guidance', 1, 0, NOW(), 0, NOW(), false, 0, null)
--ON CONFLICT ("EntityType", "Name") DO NOTHING;

INSERT INTO public."DocumentTypes"("EntityType", "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate")
VALUES ('Partner', 'Partnership Agreement', 1, 0, NOW(), 0, NOW(), false, 0, null)
--ON CONFLICT ("EntityType", "Name") DO NOTHING;

INSERT INTO public."DocumentTypes"("EntityType", "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate")
VALUES ('Partner', 'Standard Template', 1, 0, NOW(), 0, NOW(), false, 0, null)
--ON CONFLICT ("EntityType", "Name") DO NOTHING;

INSERT INTO public."DocumentTypes"("EntityType", "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate")
VALUES ('Contact', 'Other', 1, 0, NOW(), 0, NOW(), false, 0, null)
--ON CONFLICT ("EntityType", "Name") DO NOTHING;

INSERT INTO public."DocumentTypes"("EntityType", "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate")
VALUES ('Interaction', 'Meeting Note', 1, 0, NOW(), 0, NOW(), false, 0, null)
--ON CONFLICT ("EntityType", "Name") DO NOTHING;

INSERT INTO public."DocumentTypes"("EntityType", "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate")
VALUES ('PartnerTree', 'Other', 1, 0, NOW(), 0, NOW(), false, 0, null)
--ON CONFLICT ("EntityType", "Name") DO NOTHING;