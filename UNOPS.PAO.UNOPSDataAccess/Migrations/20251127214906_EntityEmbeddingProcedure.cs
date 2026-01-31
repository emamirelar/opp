using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EntityEmbeddingProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TEMPORARY: Commented out for CI/CD compatibility (pgvector extension not available)
            // This script creates procedures that use vector(768) type
            // Uncomment when pgvector extension is installed in the database
            
            // MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            // {
            //     "InsertEntityEmbeddings.sql"
            // });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
