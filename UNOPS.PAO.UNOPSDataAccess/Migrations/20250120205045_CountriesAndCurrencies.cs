using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CountriesAndCurrencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "public",
                table: "FundingOpportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Iso2Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

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
                name: "IX_FundingOpportunities_CurrencyId",
                schema: "public",
                table: "FundingOpportunities",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryUNOPSFundingOpportunity_UNOPSFundingOpportunityId",
                schema: "public",
                table: "CountryUNOPSFundingOpportunity",
                column: "UNOPSFundingOpportunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingOpportunities_Currencies_CurrencyId",
                schema: "public",
                table: "FundingOpportunities",
                column: "CurrencyId",
                principalSchema: "public",
                principalTable: "Currencies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingOpportunities_Currencies_CurrencyId",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropTable(
                name: "CountryUNOPSFundingOpportunity",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Currencies",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Countries",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_FundingOpportunities_CurrencyId",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "public",
                table: "FundingOpportunities");
        }
    }
}
