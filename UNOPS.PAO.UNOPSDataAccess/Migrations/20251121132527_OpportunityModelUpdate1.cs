using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityModelUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPooledContribution",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPooledFunding",
                schema: "public",
                table: "Opportunities",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPooledContribution",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "IsPooledFunding",
                schema: "public",
                table: "Opportunities");
        }
    }
}
