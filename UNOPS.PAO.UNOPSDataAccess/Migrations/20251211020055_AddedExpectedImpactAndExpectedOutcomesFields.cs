using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedExpectedImpactAndExpectedOutcomesFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntendedImpactOutcomes",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedImpact",
                schema: "public",
                table: "Opportunities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedOutcomes",
                schema: "public",
                table: "Opportunities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedImpact",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExpectedOutcomes",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.AddColumn<string>(
                name: "IntendedImpactOutcomes",
                schema: "public",
                table: "Opportunities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
