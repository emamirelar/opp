using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFilteredUniqueIndexesForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OpportunityUNOPSMissions_OpportunityId_UNOPSMissionId",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityCollaborators_OpportunityId_UserId",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNOPSMissions_OpportunityId_UNOPSMissionId",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                columns: new[] { "OpportunityId", "UNOPSMissionId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaborators_OpportunityId_UserId",
                schema: "public",
                table: "OpportunityCollaborators",
                columns: new[] { "OpportunityId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OpportunityUNOPSMissions_OpportunityId_UNOPSMissionId",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityCollaborators_OpportunityId_UserId",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityUNOPSMissions_OpportunityId_UNOPSMissionId",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                columns: new[] { "OpportunityId", "UNOPSMissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaborators_OpportunityId_UserId",
                schema: "public",
                table: "OpportunityCollaborators",
                columns: new[] { "OpportunityId", "UserId" },
                unique: true);
        }
    }
}
