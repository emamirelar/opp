using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityExternalStakeholders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalStakeholderNotes",
                schema: "public",
                table: "Opportunities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiscExternalStakeholders",
                schema: "public",
                table: "Opportunities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OpportunityExternalStakeholder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    ContactId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityExternalStakeholder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityExternalStakeholder_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "public",
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityExternalStakeholder_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityExternalStakeholder_ContactId",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityExternalStakeholder_OpportunityId",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                column: "OpportunityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunityExternalStakeholder",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "ExternalStakeholderNotes",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "MiscExternalStakeholders",
                schema: "public",
                table: "Opportunities");
        }
    }
}
