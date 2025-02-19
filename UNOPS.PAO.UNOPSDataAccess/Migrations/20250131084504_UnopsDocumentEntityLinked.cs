using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UnopsDocumentEntityLinked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                schema: "public",
                table: "Documents",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "Document");

            migrationBuilder.AddColumn<bool>(
                name: "LinkedFile",
                schema: "public",
                table: "Documents",
                type: "boolean",
                defaultValue: false,
                nullable: true);
            
            migrationBuilder.Sql(@"UPDATE ""Documents"" SET ""LinkedFile"" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LinkedFile",
                schema: "public",
                table: "Documents");
        }
    }
}
