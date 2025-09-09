using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EngagementModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropForeignKey(
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
                table: "Engagements");*/

            migrationBuilder.AlterColumn<int>(
                name: "ErpDimValue",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Partners_ErpDimValue",
                schema: "public",
                table: "Partners",
                column: "ErpDimValue");

            /*migrationBuilder.CreateIndex(
                name: "IX_Engagements_PartnerId",
                schema: "public",
                table: "Engagements",
                column: "PartnerId");*/

            migrationBuilder.AddForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "ErpDimValue",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Partners_ErpDimValue",
                schema: "public",
                table: "Partners");

           /* migrationBuilder.DropIndex(
                name: "IX_Engagements_PartnerId",
                schema: "public",
                table: "Engagements");*/

            migrationBuilder.AlterColumn<int>(
                name: "ErpDimValue",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

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
    }
}
