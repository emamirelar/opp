using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SimilarityFunctionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS public.RetrieveSimilarityId(TEXT, TEXT);");
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.retrieve_similarity_results(
                    entity_name text,
                    input_text text,
                    embedding text DEFAULT NULL::text,
                    limit_count integer DEFAULT 5,
                    extra_where text DEFAULT NULL::text)
                    RETURNS TABLE(entityid integer, score real) 
                    LANGUAGE 'plpgsql'
                    COST 100
                    VOLATILE PARALLEL UNSAFE
                    ROWS 1000

                AS $BODY$
                DECLARE
                    dynamic_sql TEXT := '';
                    vector_query TEXT := '';
                    text_query TEXT := '';
                    min_score REAL := 0.5;
                BEGIN
                    -- Step 1: Add vector similarity query if embedding is provided
                    IF embedding IS NOT NULL THEN
                        vector_query := format(
                            'SELECT ""EntityId""::INT AS EntityId,
                                (1 - (""FullEmbedding"" <=> %L::vector(768)))::REAL AS score
                            FROM public.""EntityEmbeddings""
                            WHERE ""EntityName"" = %L',
                            --AND (1 - (""FullEmbedding"" <=> %L::vector(768)))::REAL >= %s,
                            embedding, entity_name, embedding, min_score
                        );
                    END IF;

                    -- Step 2: Build text similarity query on the specific table
                    SELECT string_agg(
                        format(
                            'SELECT ""%s""::INT AS EntityId,
                                    similarity(""%s"", %L)::REAL AS score
                            FROM public.%I
                            WHERE ""%s"" %% %L%s'
                            AND similarity(""%s"", %L) >= %s',
                            primary_key, searchable_column, input_text,
                            table_name, searchable_column, input_text,
                            CASE WHEN extra_where IS NOT NULL THEN ' AND ' || extra_where ELSE '' END,
                            searchable_column, input_text, min_score
                        ),
                        ' UNION ALL '
                    )
                    INTO text_query
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
                        AND c.column_name IN ('Name', 'Title', 'Details', 'Description')
                        AND c.table_name NOT LIKE '%Asp%'
                        AND c.table_name NOT LIKE '%Ai%'
                        AND c.table_name = entity_name
                    ) search_tables;

                -- Step 3: Combine queries based on input
                    IF embedding IS NOT NULL AND text_query IS NOT NULL THEN
                        dynamic_sql := vector_query || ' UNION ALL ' || text_query;
                    ELSIF embedding IS NOT NULL THEN
                        dynamic_sql := vector_query;
                    ELSIF text_query IS NOT NULL THEN
                        dynamic_sql := text_query;
                    ELSE
                        -- No input provided: Return empty result set
                        RETURN;
                    END IF;

                    -- Step 4: Add ORDER BY and LIMIT
                    dynamic_sql := dynamic_sql || format(' ORDER BY score DESC LIMIT %s;', limit_count);

                    -- Execute and return
                    RETURN QUERY EXECUTE dynamic_sql;
                END;
                $BODY$;

                ALTER FUNCTION public.retrieve_similarity_results(text, text, text, integer, text) OWNER TO postgres;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
