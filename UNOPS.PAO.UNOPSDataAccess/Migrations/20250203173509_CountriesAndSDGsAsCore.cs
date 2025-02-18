using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CountriesAndSDGsAsCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingOpportunityCountries_FundingOpportunities_UNOPSFundi~",
                schema: "public",
                table: "FundingOpportunityCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingOpportunitySDGs_FundingOpportunities_UNOPSFundingOpp~",
                schema: "public",
                table: "FundingOpportunitySDGs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FundingOpportunitySDGs",
                schema: "public",
                table: "FundingOpportunitySDGs");

            migrationBuilder.DropIndex(
                name: "IX_FundingOpportunitySDGs_UNOPSFundingOpportunityId",
                schema: "public",
                table: "FundingOpportunitySDGs");

            migrationBuilder.RenameColumn(
                name: "UNOPSFundingOpportunityId",
                schema: "public",
                table: "FundingOpportunitySDGs",
                newName: "FundingOpportunityId");

            migrationBuilder.RenameColumn(
                name: "UNOPSFundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityCountries",
                newName: "FundingOpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_FundingOpportunityCountries_UNOPSFundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityCountries",
                newName: "IX_FundingOpportunityCountries_FundingOpportunityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FundingOpportunitySDGs",
                schema: "public",
                table: "FundingOpportunitySDGs",
                columns: new[] { "FundingOpportunityId", "SDGsId" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunitySDGs_SDGsId",
                schema: "public",
                table: "FundingOpportunitySDGs",
                column: "SDGsId");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingOpportunityCountries_FundingOpportunities_FundingOpp~",
                schema: "public",
                table: "FundingOpportunityCountries",
                column: "FundingOpportunityId",
                principalSchema: "public",
                principalTable: "FundingOpportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FundingOpportunitySDGs_FundingOpportunities_FundingOpportun~",
                schema: "public",
                table: "FundingOpportunitySDGs",
                column: "FundingOpportunityId",
                principalSchema: "public",
                principalTable: "FundingOpportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingOpportunityCountries_FundingOpportunities_FundingOpp~",
                schema: "public",
                table: "FundingOpportunityCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingOpportunitySDGs_FundingOpportunities_FundingOpportun~",
                schema: "public",
                table: "FundingOpportunitySDGs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FundingOpportunitySDGs",
                schema: "public",
                table: "FundingOpportunitySDGs");

            migrationBuilder.DropIndex(
                name: "IX_FundingOpportunitySDGs_SDGsId",
                schema: "public",
                table: "FundingOpportunitySDGs");

            migrationBuilder.RenameColumn(
                name: "FundingOpportunityId",
                schema: "public",
                table: "FundingOpportunitySDGs",
                newName: "UNOPSFundingOpportunityId");

            migrationBuilder.RenameColumn(
                name: "FundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityCountries",
                newName: "UNOPSFundingOpportunityId");

            migrationBuilder.RenameIndex(
                name: "IX_FundingOpportunityCountries_FundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityCountries",
                newName: "IX_FundingOpportunityCountries_UNOPSFundingOpportunityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FundingOpportunitySDGs",
                schema: "public",
                table: "FundingOpportunitySDGs",
                columns: new[] { "SDGsId", "UNOPSFundingOpportunityId" });

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunitySDGs_UNOPSFundingOpportunityId",
                schema: "public",
                table: "FundingOpportunitySDGs",
                column: "UNOPSFundingOpportunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingOpportunityCountries_FundingOpportunities_UNOPSFundi~",
                schema: "public",
                table: "FundingOpportunityCountries",
                column: "UNOPSFundingOpportunityId",
                principalSchema: "public",
                principalTable: "FundingOpportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FundingOpportunitySDGs_FundingOpportunities_UNOPSFundingOpp~",
                schema: "public",
                table: "FundingOpportunitySDGs",
                column: "UNOPSFundingOpportunityId",
                principalSchema: "public",
                principalTable: "FundingOpportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
