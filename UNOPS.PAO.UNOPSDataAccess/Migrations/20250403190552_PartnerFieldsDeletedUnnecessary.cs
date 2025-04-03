using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PartnerFieldsDeletedUnnecessary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address2City",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address2Country",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address2PostalCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address2StateProvince",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address2Street",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address2Street2",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "ExternalReportingLevel",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "InternalReportingLevel",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "public",
                table: "Partners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address2City",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2Country",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2PostalCode",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2StateProvince",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2Street",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2Street2",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReportingLevel",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalReportingLevel",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
