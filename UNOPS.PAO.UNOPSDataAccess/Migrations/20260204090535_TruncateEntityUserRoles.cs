using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TruncateEntityUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Truncate EntityUserRoles table to clear all DOA and management role data
            // CASCADE handles any dependent records
            migrationBuilder.Sql(@"TRUNCATE TABLE ""EntityUserRoles"" CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
