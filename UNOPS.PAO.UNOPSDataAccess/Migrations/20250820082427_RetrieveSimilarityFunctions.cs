using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RetrieveSimilarityFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TEMPORARY: Commented out for CI/CD compatibility (pgvector extension not available)
            // These scripts create search functions that use vector(768) type
            // Uncomment when pgvector extension is installed in the database
            
            // MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            // {
            //     "retrieve_embedding_search.sql",
            //     "retrieve_similarity_search.sql"
            // });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
