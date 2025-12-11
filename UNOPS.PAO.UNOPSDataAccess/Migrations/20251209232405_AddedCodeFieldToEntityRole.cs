using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedCodeFieldToEntityRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "public",
                table: "EntityRoles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityRoles_Code",
                schema: "public",
                table: "EntityRoles",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntityRoles_Code",
                schema: "public",
                table: "EntityRoles");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "public",
                table: "EntityRoles");
        }
    }
}
