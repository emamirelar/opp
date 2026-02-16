using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsForTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            {
                "AddBulkUpsertUniqueConstraints.sql"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
