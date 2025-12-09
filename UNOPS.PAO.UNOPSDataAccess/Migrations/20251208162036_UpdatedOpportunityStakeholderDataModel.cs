using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedOpportunityStakeholderDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityUserRoles_AspNetUserRoles_UserId_RoleId",
                schema: "public",
                table: "EntityUserRoles");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationHierarchyId",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                schema: "public",
                table: "EntityUserRoles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "EntityRoleId",
                schema: "public",
                table: "EntityUserRoles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityStakeholders_OrganizationHierarchyId",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "OrganizationHierarchyId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityUserRoles_EntityRoleId",
                schema: "public",
                table: "EntityUserRoles",
                column: "EntityRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntityUserRoles_AspNetUserRoles_UserId_RoleId",
                schema: "public",
                table: "EntityUserRoles",
                columns: new[] { "UserId", "RoleId" },
                principalSchema: "public",
                principalTable: "AspNetUserRoles",
                principalColumns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EntityUserRoles_AspNetUsers_UserId",
                schema: "public",
                table: "EntityUserRoles",
                column: "UserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityUserRoles_EntityRoles_EntityRoleId",
                schema: "public",
                table: "EntityUserRoles",
                column: "EntityRoleId",
                principalSchema: "public",
                principalTable: "EntityRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityStakeholders_OrganizationHierarchies_Organizatio~",
                schema: "public",
                table: "OpportunityStakeholders",
                column: "OrganizationHierarchyId",
                principalSchema: "public",
                principalTable: "OrganizationHierarchies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EntityUserRoles_AspNetUserRoles_UserId_RoleId",
                schema: "public",
                table: "EntityUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityUserRoles_AspNetUsers_UserId",
                schema: "public",
                table: "EntityUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_EntityUserRoles_EntityRoles_EntityRoleId",
                schema: "public",
                table: "EntityUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityStakeholders_OrganizationHierarchies_Organizatio~",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityStakeholders_OrganizationHierarchyId",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropIndex(
                name: "IX_EntityUserRoles_EntityRoleId",
                schema: "public",
                table: "EntityUserRoles");

            migrationBuilder.DropColumn(
                name: "OrganizationHierarchyId",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "EntityRoleId",
                schema: "public",
                table: "EntityUserRoles");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                schema: "public",
                table: "EntityUserRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EntityUserRoles_AspNetUserRoles_UserId_RoleId",
                schema: "public",
                table: "EntityUserRoles",
                columns: new[] { "UserId", "RoleId" },
                principalSchema: "public",
                principalTable: "AspNetUserRoles",
                principalColumns: new[] { "UserId", "RoleId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
