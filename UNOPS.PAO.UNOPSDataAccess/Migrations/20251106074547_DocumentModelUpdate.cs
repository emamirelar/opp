using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DocumentModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Link",
                schema: "public",
                table: "Documents",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<byte[]>(
                name: "Blob",
                schema: "public",
                table: "Documents",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                schema: "public",
                table: "Documents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Blob",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "StoragePath",
                schema: "public",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "Link",
                schema: "public",
                table: "Documents",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
