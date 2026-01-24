-- Drop existing function if it exists
DROP FUNCTION IF EXISTS public.retrieve_similarity_search(TEXT, TEXT, REAL, TEXT);

-- Ensure pg_trgm extension is available
--CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Create the similarity search function (pg_trgm based)
CREATE OR REPLACE FUNCTION public.retrieve_similarity_search(
    entity_name text,
    search_text text,
    similarity_threshold real DEFAULT 0.3,
    extra_where text DEFAULT NULL
) RETURNS TABLE(entityid integer, score real, search_type text)
LANGUAGE plpgsql AS
$BODY$
DECLARE
    dynamic_sql TEXT := '';
BEGIN
    -- Build dynamic SQL for similarity search across searchable columns
    SELECT string_agg(
        format(
            'SELECT "%s"::INT AS EntityId,
                    similarity("%s", %L)::REAL AS score,    
                    %L AS search_type
             FROM public.%I
             WHERE "%s" %% %L%s
               AND similarity("%s", %L) >= %s',
            primary_key, searchable_column, search_text, 'similarity',
            table_name, searchable_column, search_text,
            CASE WHEN extra_where IS NOT NULL THEN ' AND ' || extra_where ELSE '' END,
            searchable_column, search_text, similarity_threshold
        ),
        ' UNION ALL '
    )
    INTO dynamic_sql
    FROM (
        SELECT c.table_name, c.column_name AS searchable_column, pk.column_name AS primary_key
        FROM information_schema.columns c
        JOIN (
            SELECT tc.table_name, kc.column_name
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kc
            ON tc.constraint_name = kc.constraint_name
            WHERE tc.constraint_type = 'PRIMARY KEY'
        ) pk ON c.table_name = pk.table_name
        WHERE c.data_type IN ('text', 'character varying')
          AND c.column_name IN ('Name', 'Title', 'Details', 'Description', 'UserEmail')
          AND (c.table_name NOT LIKE '%Asp%' OR c.table_name = 'AspNetRoles')
          AND c.table_name NOT LIKE '%Ai%'
          AND c.table_name = entity_name
    ) search_tables;

    -- Execute the query if we have valid SQL
    IF dynamic_sql IS NOT NULL AND dynamic_sql != '' THEN
        dynamic_sql := dynamic_sql || ' ORDER BY score DESC LIMIT 1';
        RETURN QUERY EXECUTE dynamic_sql;
    END IF;
END;
$BODY$;
