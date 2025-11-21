using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingModels2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityStakeholders_Contacts_ContactId",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityStakeholders_ContactId",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "ContactId",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "Organization",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "ContributionLevel",
                schema: "public",
                table: "OpportunitySDGs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContributionLevel",
                schema: "public",
                table: "OpportunitySDGs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStakeholders_ContactId",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityStakeholders_Contacts_ContactId",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "ContactId",
                principalSchema: "public",
                principalTable: "Contacts",
                principalColumn: "Id");
        }
    }
}
