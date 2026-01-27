using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityCollaboratorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpportunityCollaborators",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AddedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityCollaborators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityCollaborators_AspNetUsers_AddedBy",
                        column: x => x.AddedBy,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OpportunityCollaborators_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityCollaborators_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaborators_AddedBy",
                schema: "public",
                table: "OpportunityCollaborators",
                column: "AddedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaborators_OpportunityId",
                schema: "public",
                table: "OpportunityCollaborators",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaborators_OpportunityId_UserId",
                schema: "public",
                table: "OpportunityCollaborators",
                columns: new[] { "OpportunityId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaborators_UserId",
                schema: "public",
                table: "OpportunityCollaborators",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunityCollaborators",
                schema: "public");
        }
    }
}
