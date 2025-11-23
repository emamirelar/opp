using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UNCFOutcomeAndUNCFIndicatorChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "UNCFOutcomes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<DateTime>(
                name: "UNCFOutcomeLastUpdatedDate",
                schema: "public",
                table: "UNCFOutcomes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UNCFIndicators",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UNCFIndicatorId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Unit = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    UNCFIndicatorStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UNCFIndicatorEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Indicators = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Baseline = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Narrative = table.Column<string>(type: "character varying(7000)", maxLength: 7000, nullable: true),
                    UNCooperationFrameworkVersionNo = table.Column<int>(type: "integer", nullable: true),
                    UNCFOutcomeExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Country = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    UNCFIndicatorLastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UNCFIndicators", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UNCFIndicators_Country",
                schema: "public",
                table: "UNCFIndicators",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFIndicators_Status",
                schema: "public",
                table: "UNCFIndicators",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFIndicators_UNCFIndicatorEndDate",
                schema: "public",
                table: "UNCFIndicators",
                column: "UNCFIndicatorEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFIndicators_UNCFIndicatorId",
                schema: "public",
                table: "UNCFIndicators",
                column: "UNCFIndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFIndicators_UNCFIndicatorStartDate",
                schema: "public",
                table: "UNCFIndicators",
                column: "UNCFIndicatorStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFIndicators_UNCFOutcomeExternalId",
                schema: "public",
                table: "UNCFIndicators",
                column: "UNCFOutcomeExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFIndicators_UNCFOutcomeExternalId_UNCooperationFramework~",
                schema: "public",
                table: "UNCFIndicators",
                columns: new[] { "UNCFOutcomeExternalId", "UNCooperationFrameworkVersionNo" });

            migrationBuilder.CreateIndex(
                name: "IX_UNCFIndicators_UNCooperationFrameworkVersionNo",
                schema: "public",
                table: "UNCFIndicators",
                column: "UNCooperationFrameworkVersionNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UNCFIndicators",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "UNCFOutcomeLastUpdatedDate",
                schema: "public",
                table: "UNCFOutcomes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "UNCFOutcomes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);
        }
    }
}
