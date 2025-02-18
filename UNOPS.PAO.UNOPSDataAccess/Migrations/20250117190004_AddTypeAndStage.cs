using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeAndStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OpportunityTypeId",
                schema: "public",
                table: "FundingOpportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                schema: "public",
                table: "FundingOpportunities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "FundingOpportunityTypes",
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
                    table.PrimaryKey("PK_FundingOpportunityTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundingOpportunities_OpportunityTypeId",
                schema: "public",
                table: "FundingOpportunities",
                column: "OpportunityTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingOpportunities_FundingOpportunityTypes_OpportunityTyp~",
                schema: "public",
                table: "FundingOpportunities",
                column: "OpportunityTypeId",
                principalSchema: "public",
                principalTable: "FundingOpportunityTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingOpportunities_FundingOpportunityTypes_OpportunityTyp~",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropTable(
                name: "FundingOpportunityTypes",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_FundingOpportunities_OpportunityTypeId",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "OpportunityTypeId",
                schema: "public",
                table: "FundingOpportunities");

            migrationBuilder.DropColumn(
                name: "Stage",
                schema: "public",
                table: "FundingOpportunities");
        }
    }
}
