using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityChildTableModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SDGTargets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SDGTargetId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SDGId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TargetDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TargetType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDGTargets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UNCFOutcomes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UNCooperationFrameworkVersionNo = table.Column<int>(type: "integer", nullable: true),
                    Country = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UNCFOutcomeId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UNCFOutcomeStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UNCFOutcomeEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UNCFOutcomes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SDGTargets_SDGId",
                schema: "public",
                table: "SDGTargets",
                column: "SDGId");

            migrationBuilder.CreateIndex(
                name: "IX_SDGTargets_SDGTargetId",
                schema: "public",
                table: "SDGTargets",
                column: "SDGTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_SDGTargets_Status",
                schema: "public",
                table: "SDGTargets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SDGTargets_TargetType",
                schema: "public",
                table: "SDGTargets",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFOutcomes_Country",
                schema: "public",
                table: "UNCFOutcomes",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFOutcomes_Status",
                schema: "public",
                table: "UNCFOutcomes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFOutcomes_UNCFOutcomeEndDate",
                schema: "public",
                table: "UNCFOutcomes",
                column: "UNCFOutcomeEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFOutcomes_UNCFOutcomeId",
                schema: "public",
                table: "UNCFOutcomes",
                column: "UNCFOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFOutcomes_UNCFOutcomeStartDate",
                schema: "public",
                table: "UNCFOutcomes",
                column: "UNCFOutcomeStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFOutcomes_UNCooperationFrameworkVersionNo",
                schema: "public",
                table: "UNCFOutcomes",
                column: "UNCooperationFrameworkVersionNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SDGTargets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UNCFOutcomes",
                schema: "public");
        }
    }
}
