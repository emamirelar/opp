using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OrgUnitManyToManyRemoveInteractionOrgUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_OrganizationHierarchies_OrgUnitId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropIndex(
                name: "IX_Interactions_OrgUnitId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "OrgUnitId",
                schema: "public",
                table: "Interactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrgUnitId",
                schema: "public",
                table: "Interactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_OrgUnitId",
                schema: "public",
                table: "Interactions",
                column: "OrgUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_OrganizationHierarchies_OrgUnitId",
                schema: "public",
                table: "Interactions",
                column: "OrgUnitId",
                principalSchema: "public",
                principalTable: "OrganizationHierarchies",
                principalColumn: "Id");
        }
    }
}
