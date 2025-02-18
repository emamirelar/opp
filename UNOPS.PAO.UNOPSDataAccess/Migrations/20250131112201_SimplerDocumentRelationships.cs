using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SimplerDocumentRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRelationships_Documents_DocumentId1",
                schema: "public",
                table: "DocumentRelationships");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRelationships_FundingOpportunities_EntityId",
                schema: "public",
                table: "DocumentRelationships");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRelationships_Proposals_EntityId",
                schema: "public",
                table: "DocumentRelationships");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRelationships_DocumentId1",
                schema: "public",
                table: "DocumentRelationships");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRelationships_EntityId",
                schema: "public",
                table: "DocumentRelationships");

            migrationBuilder.DropColumn(
                name: "DocumentId1",
                schema: "public",
                table: "DocumentRelationships");

            migrationBuilder.AddColumn<int>(
                name: "FundingOpportunityId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProposalId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                schema: "public",
                table: "DocumentRelationships",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Proposal");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FundingOpportunityId",
                schema: "public",
                table: "Documents",
                column: "FundingOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ProposalId",
                schema: "public",
                table: "Documents",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRelationships_EntityId_EntityType",
                schema: "public",
                table: "DocumentRelationships",
                columns: new[] { "EntityId", "EntityType" });

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_FundingOpportunities_FundingOpportunityId",
                schema: "public",
                table: "Documents",
                column: "FundingOpportunityId",
                principalSchema: "public",
                principalTable: "FundingOpportunities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Proposals_ProposalId",
                schema: "public",
                table: "Documents",
                column: "ProposalId",
                principalSchema: "public",
                principalTable: "Proposals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_FundingOpportunities_FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Proposals_ProposalId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_ProposalId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_DocumentRelationships_EntityId_EntityType",
                schema: "public",
                table: "DocumentRelationships");

            migrationBuilder.DropColumn(
                name: "FundingOpportunityId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ProposalId",
                schema: "public",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                schema: "public",
                table: "DocumentRelationships",
                type: "text",
                nullable: false,
                defaultValue: "Proposal",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "DocumentId1",
                schema: "public",
                table: "DocumentRelationships",
                type: "integer",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRelationships_Documents_DocumentId1",
                schema: "public",
                table: "DocumentRelationships",
                column: "DocumentId1",
                principalSchema: "public",
                principalTable: "Documents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRelationships_FundingOpportunities_EntityId",
                schema: "public",
                table: "DocumentRelationships",
                column: "EntityId",
                principalSchema: "public",
                principalTable: "FundingOpportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRelationships_Proposals_EntityId",
                schema: "public",
                table: "DocumentRelationships",
                column: "EntityId",
                principalSchema: "public",
                principalTable: "Proposals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
