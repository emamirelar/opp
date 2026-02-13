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
            // NOTE: pg_trgm and pgvector extensions required for full functionality.
            // This creates stub functions that will work without those extensions.
            // Re-run the full function creation when extensions are installed.
            migrationBuilder.Sql(@"
        DROP FUNCTION IF EXISTS public.retrieve_similarity_results(TEXT, TEXT, TEXT, INTEGER, TEXT);
        DROP FUNCTION IF EXISTS public.retrieve_similarity_results(TEXT, TEXT, TEXT, REAL, REAL, TEXT);

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
        BEGIN
            -- Stub: pgvector and pg_trgm extensions not available.
            -- Returns empty result set. Install extensions for full search functionality.
            RETURN;
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
