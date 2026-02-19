using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Truncate before adding column - ExternalDataService sync will repopulate with RoleSource
            migrationBuilder.Sql(@"TRUNCATE TABLE ""EntityUserRoles"";");

            migrationBuilder.AddColumn<string>(
                name: "RoleSource",
                schema: "public",
                table: "EntityUserRoles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleSource",
                schema: "public",
                table: "EntityUserRoles");
        }
    }
}
