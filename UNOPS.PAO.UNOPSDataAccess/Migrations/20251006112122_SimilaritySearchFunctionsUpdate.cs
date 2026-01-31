using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SimilaritySearchFunctionsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TEMPORARY: Commented out for CI/CD compatibility (pgvector extension not available)
            // This script creates search functions that use vector(768) type
            // Uncomment when pgvector extension is installed in the database
            
            // MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            // {
            //     "Search_Records.sql"
            // });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
