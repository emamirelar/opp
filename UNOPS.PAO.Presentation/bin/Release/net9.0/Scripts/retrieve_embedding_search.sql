DROP FUNCTION IF EXISTS public.retrieve_similarity_results(TEXT, TEXT, TEXT, REAL, REAL, TEXT);

-- Drop existing function if it exists
DROP FUNCTION IF EXISTS public.retrieve_embedding_search(TEXT, TEXT, REAL, TEXT);

-- Create the embedding search function (vector based) - returns single result
CREATE OR REPLACE FUNCTION public.retrieve_embedding_search(
    entity_name text,
    embedding_vector text,
    embedding_threshold real DEFAULT 0.7,
    extra_where text DEFAULT NULL
) RETURNS TABLE(entityid integer, score real, search_type text)
LANGUAGE plpgsql AS
$BODY$
DECLARE
    dynamic_sql TEXT := '';
BEGIN
    -- Build dynamic SQL for embedding search using vector similarity
    dynamic_sql := format(
        'SELECT "EntityId"::INT AS EntityId,
                (1 - ("FullEmbedding" <=> %L::vector(768)))::REAL AS score,
                %L AS search_type
         FROM public."EntityEmbeddings"
         WHERE "EntityName" = %L
           AND (1 - ("FullEmbedding" <=> %L::vector(768)))::REAL >= %s%s
         ORDER BY score DESC LIMIT 1',
        embedding_vector, 'embedding', entity_name, embedding_vector, embedding_threshold,
        CASE WHEN extra_where IS NOT NULL THEN ' AND ' || extra_where ELSE '' END
    );

    -- Execute the embedding search query
    RETURN QUERY EXECUTE dynamic_sql;
END;
$BODY$;

-- Create the multiple embedding search function (vector based) - returns multiple results
CREATE OR REPLACE FUNCTION public.retrieve_embedding_search_multiple(
    entity_name text,
    embedding_vector text,
    embedding_threshold real DEFAULT 0.7,
    result_limit integer DEFAULT 10,
    extra_where text DEFAULT NULL
) RETURNS TABLE(entityid integer, score real, search_type text)
LANGUAGE plpgsql AS
$BODY$
DECLARE
    dynamic_sql TEXT := '';
BEGIN
    -- Build dynamic SQL for embedding search using vector similarity
    dynamic_sql := format(
        'SELECT "EntityId"::INT AS EntityId,
                (1 - ("FullEmbedding" <=> %L::vector(768)))::REAL AS score,
                %L AS search_type
         FROM public."EntityEmbeddings"
         WHERE "EntityName" = %L
           AND (1 - ("FullEmbedding" <=> %L::vector(768)))::REAL >= %s%s
         ORDER BY score DESC LIMIT %s',
        embedding_vector, 'embedding', entity_name, embedding_vector, embedding_threshold,
        CASE WHEN extra_where IS NOT NULL THEN ' AND ' || extra_where ELSE '' END,
        result_limit
    );

    -- Execute the embedding search query
    RETURN QUERY EXECUTE dynamic_sql;
END;
$BODY$;
