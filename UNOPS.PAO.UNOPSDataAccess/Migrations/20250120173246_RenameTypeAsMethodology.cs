using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameTypeAsMethodology : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingOpportunities_FundingOpportunityTypes_OpportunityTyp~",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropTable(
                name: "FundingOpportunityTypes",
                schema: "public");

            migrationBuilder.RenameColumn(
                name: "OpportunityTypeId",
                schema: "public",
                table: "FundingOpportunities",
                newName: "SelectionMethodologyId");

            migrationBuilder.RenameIndex(
                name: "IX_FundingOpportunities_OpportunityTypeId",
                schema: "public",
                table: "FundingOpportunities",
                newName: "IX_FundingOpportunities_SelectionMethodologyId");

            migrationBuilder.CreateTable(
                name: "SelectionMethodologies",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_SelectionMethodologies", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_FundingOpportunities_SelectionMethodologies_SelectionMethod~",
                schema: "public",
                table: "FundingOpportunities",
                column: "SelectionMethodologyId",
                principalSchema: "public",
                principalTable: "SelectionMethodologies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingOpportunities_SelectionMethodologies_SelectionMethod~",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropTable(
                name: "SelectionMethodologies",
                schema: "public");

            migrationBuilder.RenameColumn(
                name: "SelectionMethodologyId",
                schema: "public",
                table: "FundingOpportunities",
                newName: "OpportunityTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_FundingOpportunities_SelectionMethodologyId",
                schema: "public",
                table: "FundingOpportunities",
                newName: "IX_FundingOpportunities_OpportunityTypeId");

            migrationBuilder.CreateTable(
                name: "FundingOpportunityTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingOpportunityTypes", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_FundingOpportunities_FundingOpportunityTypes_OpportunityTyp~",
                schema: "public",
                table: "FundingOpportunities",
                column: "OpportunityTypeId",
                principalSchema: "public",
                principalTable: "FundingOpportunityTypes",
                principalColumn: "Id");
        }
    }
}
