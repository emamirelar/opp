using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFundingOpportunityDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClarificationDeadline",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "InformationSessionDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PostingDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionDueDate",
                schema: "public",
                table: "FundingOpportunities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClarificationDeadline",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "DecisionDate",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "InformationSessionDate",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "PostingDate",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "SubmissionDueDate",
                schema: "public",
                table: "FundingOpportunities");
        }
    }
}
