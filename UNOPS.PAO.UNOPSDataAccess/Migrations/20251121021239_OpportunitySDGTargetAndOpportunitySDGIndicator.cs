using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunitySDGTargetAndOpportunitySDGIndicator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpportunitySDGTargets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    OpportunitySDGId = table.Column<int>(type: "integer", nullable: false),
                    SDGTargetId = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunitySDGTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunitySDGTargets_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunitySDGTargets_OpportunitySDGs_OpportunitySDGId",
                        column: x => x.OpportunitySDGId,
                        principalSchema: "public",
                        principalTable: "OpportunitySDGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunitySDGTargets_SDGTargets_SDGTargetId",
                        column: x => x.SDGTargetId,
                        principalSchema: "public",
                        principalTable: "SDGTargets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SDGIndicators",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SDGIndicatorId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SDGTargetId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SDGIndicatorLongDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDGIndicators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpportunitySDGIndicators",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    OpportunitySDGTargetId = table.Column<int>(type: "integer", nullable: false),
                    SDGIndicatorId = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunitySDGIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunitySDGIndicators_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunitySDGIndicators_OpportunitySDGTargets_OpportunityS~",
                        column: x => x.OpportunitySDGTargetId,
                        principalSchema: "public",
                        principalTable: "OpportunitySDGTargets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunitySDGIndicators_SDGIndicators_SDGIndicatorId",
                        column: x => x.SDGIndicatorId,
                        principalSchema: "public",
                        principalTable: "SDGIndicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGIndicators_OpportunityId",
                schema: "public",
                table: "OpportunitySDGIndicators",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGIndicators_OpportunitySDGTargetId",
                schema: "public",
                table: "OpportunitySDGIndicators",
                column: "OpportunitySDGTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGIndicators_SDGIndicatorId",
                schema: "public",
                table: "OpportunitySDGIndicators",
                column: "SDGIndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGTargets_OpportunityId",
                schema: "public",
                table: "OpportunitySDGTargets",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGTargets_OpportunitySDGId",
                schema: "public",
                table: "OpportunitySDGTargets",
                column: "OpportunitySDGId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGTargets_SDGTargetId",
                schema: "public",
                table: "OpportunitySDGTargets",
                column: "SDGTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_SDGIndicators_SDGIndicatorId",
                schema: "public",
                table: "SDGIndicators",
                column: "SDGIndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SDGIndicators_SDGTargetId",
                schema: "public",
                table: "SDGIndicators",
                column: "SDGTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_SDGIndicators_Status",
                schema: "public",
                table: "SDGIndicators",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunitySDGIndicators",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OpportunitySDGTargets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDGIndicators",
                schema: "public");
        }
    }
}
