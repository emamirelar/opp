using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FundingOpportunitySetUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryUNOPSFundingOpportunity",
                schema: "public");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationType",
                schema: "public",
                table: "FundingOpportunities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EligibilityCriteria",
                schema: "public",
                table: "FundingOpportunities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "FundingAvailable",
                schema: "public",
                table: "FundingOpportunities",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Justification",
                schema: "public",
                table: "FundingOpportunities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SingleSubmition",
                schema: "public",
                table: "FundingOpportunities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EligibleEntities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibleEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundingOpportunityCountries",
                schema: "public",
                columns: table => new
                {
                    CountriesId = table.Column<int>(type: "integer", nullable: false),
                    UNOPSFundingOpportunityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingOpportunityCountries", x => new { x.CountriesId, x.UNOPSFundingOpportunityId });
                    table.ForeignKey(
                        name: "FK_FundingOpportunityCountries_Countries_CountriesId",
                        column: x => x.CountriesId,
                        principalSchema: "public",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingOpportunityCountries_FundingOpportunities_UNOPSFundi~",
                        column: x => x.UNOPSFundingOpportunityId,
                        principalSchema: "public",
                        principalTable: "FundingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundingOpportunityEligibleEntities",
                schema: "public",
                columns: table => new
                {
                    EligibleEntitiesId = table.Column<int>(type: "integer", nullable: false),
                    FundingOpportunityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingOpportunityEligibleEntities", x => new { x.EligibleEntitiesId, x.FundingOpportunityId });
                    table.ForeignKey(
                        name: "FK_FundingOpportunityEligibleEntities_EligibleEntities_Eligibl~",
                        column: x => x.EligibleEntitiesId,
                        principalSchema: "public",
                        principalTable: "EligibleEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingOpportunityEligibleEntities_FundingOpportunities_Fun~",
                        column: x => x.FundingOpportunityId,
                        principalSchema: "public",
                        principalTable: "FundingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunityCountries_UNOPSFundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityCountries",
                column: "UNOPSFundingOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunityEligibleEntities_FundingOpportunityId",
                schema: "public",
                table: "FundingOpportunityEligibleEntities",
                column: "FundingOpportunityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundingOpportunityCountries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FundingOpportunityEligibleEntities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EligibleEntities",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "ApplicationType",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "EligibilityCriteria",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "FundingAvailable",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "Justification",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "SingleSubmition",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.CreateTable(
                name: "CountryUNOPSFundingOpportunity",
                schema: "public",
                columns: table => new
                {
                    CountriesId = table.Column<int>(type: "integer", nullable: false),
                    UNOPSFundingOpportunityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryUNOPSFundingOpportunity", x => new { x.CountriesId, x.UNOPSFundingOpportunityId });
                    table.ForeignKey(
                        name: "FK_CountryUNOPSFundingOpportunity_Countries_CountriesId",
                        column: x => x.CountriesId,
                        principalSchema: "public",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CountryUNOPSFundingOpportunity_FundingOpportunities_UNOPSFu~",
                        column: x => x.UNOPSFundingOpportunityId,
                        principalSchema: "public",
                        principalTable: "FundingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryUNOPSFundingOpportunity_UNOPSFundingOpportunityId",
                schema: "public",
                table: "CountryUNOPSFundingOpportunity",
                column: "UNOPSFundingOpportunityId");
        }
    }
}
