using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAiPrompts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Execute SQL scripts for updating the entity permissions filter
            MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            {
                "AI_Assistant_Prompts.sql"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
