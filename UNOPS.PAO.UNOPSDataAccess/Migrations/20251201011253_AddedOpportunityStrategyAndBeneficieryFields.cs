using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddedOpportunityStrategyAndBeneficieryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HumanitarianFrameworkAlignment",
                schema: "public",
                table: "OpportunityCountries",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NapAlignment",
                schema: "public",
                table: "OpportunityCountries",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NdcAlignment",
                schema: "public",
                table: "OpportunityCountries",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OrgUnitStrategyAlignment",
                schema: "public",
                table: "OpportunityCountries",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrgUnitWithStrategyId",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BeneficiariesToBeDetermined",
                schema: "public",
                table: "Opportunities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDirectBeneficiaries",
                schema: "public",
                table: "Opportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedIndirectBeneficiaries",
                schema: "public",
                table: "Opportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UNOPSMissions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IconClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UNOPSMissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityUNOPSMissions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    UNOPSMissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityUNOPSMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityUNOPSMissions_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityUNOPSMissions_UNOPSMissions_UNOPSMissionId",
                        column: x => x.UNOPSMissionId,
                        principalSchema: "public",
                        principalTable: "UNOPSMissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCountries_OrgUnitWithStrategyId",
                schema: "public",
                table: "OpportunityCountries",
                column: "OrgUnitWithStrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNOPSMissions_OpportunityId",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNOPSMissions_OpportunityId_UNOPSMissionId",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                columns: new[] { "OpportunityId", "UNOPSMissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNOPSMissions_UNOPSMissionId",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                column: "UNOPSMissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UNOPSMissions_Code",
                schema: "public",
                table: "UNOPSMissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UNOPSMissions_DisplayOrder",
                schema: "public",
                table: "UNOPSMissions",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_UNOPSMissions_Name",
                schema: "public",
                table: "UNOPSMissions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_UNOPSMissions_Status",
                schema: "public",
                table: "UNOPSMissions",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityCountries_OrganizationHierarchies_OrgUnitWithStr~",
                schema: "public",
                table: "OpportunityCountries",
                column: "OrgUnitWithStrategyId",
                principalSchema: "public",
                principalTable: "OrganizationHierarchies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityCountries_OrganizationHierarchies_OrgUnitWithStr~",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropTable(
                name: "OpportunityUNOPSMissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "UNOPSMissions",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityCountries_OrgUnitWithStrategyId",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "HumanitarianFrameworkAlignment",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "NapAlignment",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "NdcAlignment",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "OrgUnitStrategyAlignment",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "OrgUnitWithStrategyId",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "BeneficiariesToBeDetermined",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "EstimatedDirectBeneficiaries",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "EstimatedIndirectBeneficiaries",
                schema: "public",
                table: "Opportunities");
        }
    }
}
