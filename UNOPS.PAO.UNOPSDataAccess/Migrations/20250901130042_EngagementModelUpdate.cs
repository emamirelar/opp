using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EngagementModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements");

            migrationBuilder.DropIndex(
                name: "IX_Engagements_PartnerId",
                schema: "public",
                table: "Engagements");

            migrationBuilder.AddColumn<int>(
                name: "ErpDimValue",
                schema: "public",
                table: "Engagements",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engagements_ErpDimValue",
                schema: "public",
                table: "Engagements",
                column: "ErpDimValue");

            migrationBuilder.AddForeignKey(
                name: "FK_Engagements_Partners_ErpDimValue",
                schema: "public",
                table: "Engagements",
                column: "ErpDimValue",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Engagements_Partners_ErpDimValue",
                schema: "public",
                table: "Engagements");

            migrationBuilder.DropIndex(
                name: "IX_Engagements_ErpDimValue",
                schema: "public",
                table: "Engagements");

            migrationBuilder.DropColumn(
                name: "ErpDimValue",
                schema: "public",
                table: "Engagements");

            migrationBuilder.CreateIndex(
                name: "IX_Engagements_PartnerId",
                schema: "public",
                table: "Engagements",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id");
        }
    }
}
