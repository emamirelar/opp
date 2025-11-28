using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUNCFOutcomeDateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UNCFOutcomes_UNCFOutcomeEndDate",
                schema: "public",
                table: "UNCFOutcomes");

            migrationBuilder.DropIndex(
                name: "IX_UNCFOutcomes_UNCFOutcomeStartDate",
                schema: "public",
                table: "UNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "UNCFOutcomeEndDate",
                schema: "public",
                table: "UNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "UNCFOutcomeStartDate",
                schema: "public",
                table: "UNCFOutcomes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UNCFOutcomeEndDate",
                schema: "public",
                table: "UNCFOutcomes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UNCFOutcomeStartDate",
                schema: "public",
                table: "UNCFOutcomes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UNCFOutcomes_UNCFOutcomeEndDate",
                schema: "public",
                table: "UNCFOutcomes",
                column: "UNCFOutcomeEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFOutcomes_UNCFOutcomeStartDate",
                schema: "public",
                table: "UNCFOutcomes",
                column: "UNCFOutcomeStartDate");
        }
    }
}
