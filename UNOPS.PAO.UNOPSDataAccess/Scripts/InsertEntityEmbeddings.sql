CREATE OR REPLACE PROCEDURE public."InsertEntityEmbedding"(entityName TEXT, entityId INT, entityData TEXT,embedding TEXT)
LANGUAGE plpgsql
AS $$
BEGIN
INSERT INTO public."EntityEmbeddings" ("EntityName", "EntityId", "EntityData", "FullEmbedding") 
VALUES (entityName, entityId, entityData, embedding::vector(768)) 
ON CONFLICT ("EntityName", "EntityId") 
DO UPDATE SET "FullEmbedding" = EXCLUDED."FullEmbedding",
    "EntityData" = entityData; 
END;
$$;
                                