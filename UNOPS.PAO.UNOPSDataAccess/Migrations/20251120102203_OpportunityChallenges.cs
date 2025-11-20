using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Challenges",
                schema: "public",
                table: "Opportunities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityFundingPartners_DocumentId",
                schema: "public",
                table: "OpportunityFundingPartners",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityClientPartners_DocumentId",
                schema: "public",
                table: "OpportunityClientPartners",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityClientPartners_Documents_DocumentId",
                schema: "public",
                table: "OpportunityClientPartners",
                column: "DocumentId",
                principalSchema: "public",
                principalTable: "Documents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityFundingPartners_Documents_DocumentId",
                schema: "public",
                table: "OpportunityFundingPartners",
                column: "DocumentId",
                principalSchema: "public",
                principalTable: "Documents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityClientPartners_Documents_DocumentId",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityFundingPartners_Documents_DocumentId",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityFundingPartners_DocumentId",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityClientPartners_DocumentId",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "Challenges",
                schema: "public",
                table: "Opportunities");
        }
    }
}
