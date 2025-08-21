using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PartnerEcosystemFieldsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartnerLevelCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerLevelDescription",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerLevelShort",
                schema: "public",
                table: "Partners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartnerLevelCode",
                schema: "public",
                table: "Partners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerLevelDescription",
                schema: "public",
                table: "Partners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerLevelShort",
                schema: "public",
                table: "Partners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
