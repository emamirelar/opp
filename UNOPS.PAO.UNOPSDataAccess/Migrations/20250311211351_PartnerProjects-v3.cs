using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PartnerProjectsv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerProjects_Partners_PartnerId",
                schema: "public",
                table: "PartnerProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PartnerProjects_Projects_ProjectId",
                schema: "public",
                table: "PartnerProjects");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                schema: "public",
                table: "PartnerProjects",
                newName: "ProjectsId");

            migrationBuilder.RenameColumn(
                name: "PartnerId",
                schema: "public",
                table: "PartnerProjects",
                newName: "PartnersId");

            migrationBuilder.RenameIndex(
                name: "IX_PartnerProjects_ProjectId",
                schema: "public",
                table: "PartnerProjects",
                newName: "IX_PartnerProjects_ProjectsId");

            migrationBuilder.RenameIndex(
                name: "IX_PartnerProjects_PartnerId",
                schema: "public",
                table: "PartnerProjects",
                newName: "IX_PartnerProjects_PartnersId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerProjects_Partners_PartnersId",
                schema: "public",
                table: "PartnerProjects",
                column: "PartnersId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerProjects_Projects_ProjectsId",
                schema: "public",
                table: "PartnerProjects",
                column: "ProjectsId",
                principalSchema: "public",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerProjects_Partners_PartnersId",
                schema: "public",
                table: "PartnerProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PartnerProjects_Projects_ProjectsId",
                schema: "public",
                table: "PartnerProjects");

            migrationBuilder.RenameColumn(
                name: "ProjectsId",
                schema: "public",
                table: "PartnerProjects",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "PartnersId",
                schema: "public",
                table: "PartnerProjects",
                newName: "PartnerId");

            migrationBuilder.RenameIndex(
                name: "IX_PartnerProjects_ProjectsId",
                schema: "public",
                table: "PartnerProjects",
                newName: "IX_PartnerProjects_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_PartnerProjects_PartnersId",
                schema: "public",
                table: "PartnerProjects",
                newName: "IX_PartnerProjects_PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerProjects_Partners_PartnerId",
                schema: "public",
                table: "PartnerProjects",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerProjects_Projects_ProjectId",
                schema: "public",
                table: "PartnerProjects",
                column: "ProjectId",
                principalSchema: "public",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
