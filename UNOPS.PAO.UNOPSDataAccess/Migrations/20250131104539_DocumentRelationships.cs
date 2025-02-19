using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DocumentRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_FundingOpportunities_FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.CreateTable(
                name: "DocumentRelationships",
                schema: "public",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false, defaultValue: "Proposal"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DocumentId1 = table.Column<int>(type: "integer", nullable: true),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRelationships", x => new { x.DocumentId, x.EntityId, x.EntityType });
                    table.ForeignKey(
                        name: "FK_DocumentRelationships_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentRelationships_Documents_DocumentId1",
                        column: x => x.DocumentId1,
                        principalSchema: "public",
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentRelationships_FundingOpportunities_EntityId",
                        column: x => x.EntityId,
                        principalSchema: "public",
                        principalTable: "FundingOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentRelationships_Proposals_EntityId",
                        column: x => x.EntityId,
                        principalSchema: "public",
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRelationships_DocumentId1",
                schema: "public",
                table: "DocumentRelationships",
                column: "DocumentId1");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRelationships_EntityId",
                schema: "public",
                table: "DocumentRelationships",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentRelationships",
                schema: "public");

            migrationBuilder.AddColumn<int>(
                name: "FundingOpportunityId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FundingOpportunityId",
                schema: "public",
                table: "Documents",
                column: "FundingOpportunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_FundingOpportunities_FundingOpportunityId",
                schema: "public",
                table: "Documents",
                column: "FundingOpportunityId",
                principalSchema: "public",
                principalTable: "FundingOpportunities",
                principalColumn: "Id");
        }
    }
}
