using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrgUnitFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Execute SQL scripts for removing OrgUnit filters
            MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            {
                "remove-orgunit-filters.sql"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverting would require re-adding the filters, which should be done carefully
            // For now, we'll leave this empty as the filters should be managed through explicit configuration
        }
    }
}
