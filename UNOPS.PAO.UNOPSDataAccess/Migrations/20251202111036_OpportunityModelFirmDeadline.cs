using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityModelFirmDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTargetSigningDateFirm",
                schema: "public",
                table: "Opportunities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SigningDateNotes",
                schema: "public",
                table: "Opportunities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionDeadline",
                schema: "public",
                table: "Opportunities",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTargetSigningDateFirm",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SigningDateNotes",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "SubmissionDeadline",
                schema: "public",
                table: "Opportunities");
        }
    }
}
