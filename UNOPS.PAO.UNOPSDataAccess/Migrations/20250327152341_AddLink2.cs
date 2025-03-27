using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddLink2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Entity",
                schema: "public",
                table: "Links");

            migrationBuilder.AddColumn<string>(
                name: "Entity",
                schema: "public",
                table: "Links",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                schema: "public",
                table: "Links",
                type: "text",
                nullable: false,
                defaultValue: "Link");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                schema: "public",
                table: "Links");
                
            migrationBuilder.DropColumn(
                name: "Entity",
                schema: "public",
                table: "Links");

            migrationBuilder.AddColumn<int>(
                name: "Entity",
                schema: "public",
                table: "Links",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
