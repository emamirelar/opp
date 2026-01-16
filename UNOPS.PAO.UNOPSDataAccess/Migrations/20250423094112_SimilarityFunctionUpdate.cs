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
            //migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql(@"
        DROP FUNCTION IF EXISTS public.retrieve_similarity_results(TEXT, TEXT, TEXT, INTEGER, TEXT);

        --CREATE EXTENSION IF NOT EXISTS pg_trgm;

        CREATE OR REPLACE FUNCTION public.retrieve_similarity_results(
            entity_name text,
            input_text text,
            embedding text DEFAULT NULL::text,
            similarity_threshold real DEFAULT 0.3,
            embedding_threshold real DEFAULT 0.7,
            extra_where text DEFAULT NULL::text)
        RETURNS TABLE(entityid integer, score real, search_type text)
        LANGUAGE plpgsql
        AS
        $BODY$
        DECLARE
            dynamic_sql TEXT := '';
        BEGIN
            -- If embedding is provided, do embedding search only
            IF embedding IS NOT NULL THEN
                dynamic_sql := format(
                    'SELECT ""EntityId""::INT AS EntityId,
                            (1 - (""FullEmbedding"" <=> %L::vector(768)))::REAL AS score,
                            %L AS search_type
                     FROM public.""EntityEmbeddings""
                     WHERE ""EntityName"" = %L
                       AND (1 - (""FullEmbedding"" <=> %L::vector(768)))::REAL >= %s
                     ORDER BY score DESC',
                    embedding, 'embedding', entity_name, embedding, embedding_threshold
                );
            -- Otherwise, do similarity search
            ELSE
                SELECT string_agg(
                    format(
                        'SELECT ""%s""::INT AS EntityId,
                                similarity(""%s"", %L)::REAL AS score,
                                %L AS search_type
                         FROM public.%I
                         WHERE ""%s"" %% %L%s
                           AND similarity(""%s"", %L) >= %s',
                        primary_key, searchable_column, input_text, 'similarity',
                        table_name, searchable_column, input_text,
                        CASE WHEN extra_where IS NOT NULL THEN ' AND ' || extra_where ELSE '' END,
                        searchable_column, input_text, similarity_threshold
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
                      AND c.column_name IN ('Name', 'Title', 'Details', 'Description')
                      AND c.table_name NOT LIKE '%Asp%'
                      AND c.table_name NOT LIKE '%Ai%'
                      AND c.table_name = entity_name
                ) search_tables;

                IF dynamic_sql IS NOT NULL THEN
                    dynamic_sql := dynamic_sql || ' ORDER BY score DESC';
                END IF;
            END IF;

            -- Execute the query if we have valid SQL
            IF dynamic_sql IS NOT NULL AND dynamic_sql != '' THEN
                RETURN QUERY EXECUTE dynamic_sql;
            END IF;
        END;
        $BODY$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
