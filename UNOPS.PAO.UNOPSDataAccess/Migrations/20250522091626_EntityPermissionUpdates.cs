using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EntityPermissionUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                schema: "public",
                table: "EntityPermissions");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                schema: "public",
                table: "EntityPermissions",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "PropertyName",
                schema: "public",
                table: "EntityPermissions",
                newName: "RowFilter");

            migrationBuilder.RenameColumn(
                name: "FilterExpression",
                schema: "public",
                table: "EntityPermissions",
                newName: "PropertyFilter");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                schema: "public",
                table: "EntityPermissions",
                newName: "Entity");

            migrationBuilder.AddColumn<bool>(
                name: "CanCreate",
                schema: "public",
                table: "EntityPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDelete",
                schema: "public",
                table: "EntityPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanRead",
                schema: "public",
                table: "EntityPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanUpdate",
                schema: "public",
                table: "EntityPermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanCreate",
                schema: "public",
                table: "EntityPermissions");

            migrationBuilder.DropColumn(
                name: "CanDelete",
                schema: "public",
                table: "EntityPermissions");

            migrationBuilder.DropColumn(
                name: "CanRead",
                schema: "public",
                table: "EntityPermissions");

            migrationBuilder.DropColumn(
                name: "CanUpdate",
                schema: "public",
                table: "EntityPermissions");

            migrationBuilder.RenameColumn(
                name: "RowFilter",
                schema: "public",
                table: "EntityPermissions",
                newName: "PropertyName");

            migrationBuilder.RenameColumn(
                name: "Role",
                schema: "public",
                table: "EntityPermissions",
                newName: "RoleName");

            migrationBuilder.RenameColumn(
                name: "PropertyFilter",
                schema: "public",
                table: "EntityPermissions",
                newName: "FilterExpression");

            migrationBuilder.RenameColumn(
                name: "Entity",
                schema: "public",
                table: "EntityPermissions",
                newName: "EntityName");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                schema: "public",
                table: "EntityPermissions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
