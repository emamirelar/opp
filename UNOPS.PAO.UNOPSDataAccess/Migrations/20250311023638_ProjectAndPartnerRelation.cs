using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ProjectAndPartnerRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                schema: "public",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerNumber",
                schema: "public",
                table: "Partners",
                type: "text",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "PartnerNumber",
                schema: "public",
                table: "Partners");
        }
    }
}
