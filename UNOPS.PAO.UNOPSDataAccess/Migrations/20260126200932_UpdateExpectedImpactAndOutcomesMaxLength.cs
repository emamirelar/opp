using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExpectedImpactAndOutcomesMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExpectedOutcomes",
                schema: "public",
                table: "Opportunities",
                type: "character varying(510)",
                maxLength: 510,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpectedImpact",
                schema: "public",
                table: "Opportunities",
                type: "character varying(510)",
                maxLength: 510,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExpectedOutcomes",
                schema: "public",
                table: "Opportunities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(510)",
                oldMaxLength: 510,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExpectedImpact",
                schema: "public",
                table: "Opportunities",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(510)",
                oldMaxLength: 510,
                oldNullable: true);
        }
    }
}
