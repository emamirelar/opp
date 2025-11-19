using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityInteractionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpportunityInteractions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    InteractionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityInteractions_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalSchema: "public",
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityInteractions_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityInteractions_InteractionId",
                schema: "public",
                table: "OpportunityInteractions",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityInteractions_OpportunityId",
                schema: "public",
                table: "OpportunityInteractions",
                column: "OpportunityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunityInteractions",
                schema: "public");
        }
    }
}
