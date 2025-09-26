using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedContactStatusFieldToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing text column
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "Contacts");

            // Add the new integer column
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "Contacts",
                type: "integer",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the integer column
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "Contacts");

            // Add back the text column
            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: false,
                defaultValue: "Active");
        }
    }
}
