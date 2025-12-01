using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedOppUNCFOutcomeAndOppUNCFIndicator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpportunityUNCFOutcomes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    OpportunityCountryId = table.Column<int>(type: "integer", nullable: false),
                    UNCFOutcomeId = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityUNCFOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityUNCFOutcomes_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityUNCFOutcomes_OpportunityCountries_OpportunityCou~",
                        column: x => x.OpportunityCountryId,
                        principalSchema: "public",
                        principalTable: "OpportunityCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityUNCFOutcomes_UNCFOutcomes_UNCFOutcomeId",
                        column: x => x.UNCFOutcomeId,
                        principalSchema: "public",
                        principalTable: "UNCFOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityUNCFIndicators",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    OpportunityUNCFOutcomeId = table.Column<int>(type: "integer", nullable: false),
                    UNCFIndicatorId = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityUNCFIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityUNCFIndicators_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityUNCFIndicators_OpportunityUNCFOutcomes_Opportuni~",
                        column: x => x.OpportunityUNCFOutcomeId,
                        principalSchema: "public",
                        principalTable: "OpportunityUNCFOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityUNCFIndicators_UNCFIndicators_UNCFIndicatorId",
                        column: x => x.UNCFIndicatorId,
                        principalSchema: "public",
                        principalTable: "UNCFIndicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNCFIndicators_OpportunityId",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNCFIndicators_OpportunityUNCFOutcomeId",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                column: "OpportunityUNCFOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNCFIndicators_UNCFIndicatorId",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                column: "UNCFIndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNCFOutcomes_OpportunityCountryId",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                column: "OpportunityCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNCFOutcomes_OpportunityId",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNCFOutcomes_UNCFOutcomeId",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                column: "UNCFOutcomeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunityUNCFIndicators",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OpportunityUNCFOutcomes",
                schema: "public");
        }
    }
}
