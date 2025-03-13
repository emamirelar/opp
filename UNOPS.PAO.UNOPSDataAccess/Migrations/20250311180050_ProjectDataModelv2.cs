using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ProjectDataModelv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Partners_PartnerId",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_PartnerId",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                schema: "public",
                table: "Projects");

            migrationBuilder.AddColumn<double>(
                name: "BudgetAmount",
                schema: "public",
                table: "Projects",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ExpenditureAmount",
                schema: "public",
                table: "Projects",
                type: "double precision",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartnerProjects",
                schema: "public",
                columns: table => new
                {
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerProjects", x => new { x.PartnerId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_PartnerProjects_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "public",
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "public",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerProjects_ProjectId",
                schema: "public",
                table: "PartnerProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerProjects_PartnerId",
                schema: "public",
                table: "PartnerProjects",
                column: "PartnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerProjects",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "BudgetAmount",
                schema: "public",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ExpenditureAmount",
                schema: "public",
                table: "Projects");

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                schema: "public",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PartnerId",
                schema: "public",
                table: "Projects",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Partners_PartnerId",
                schema: "public",
                table: "Projects",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id");
        }
    }
}
