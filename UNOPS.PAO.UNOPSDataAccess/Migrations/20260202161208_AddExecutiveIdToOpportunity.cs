using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutiveIdToOpportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutiveId",
                schema: "public",
                table: "Opportunities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_ExecutiveId",
                schema: "public",
                table: "Opportunities",
                column: "ExecutiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_AspNetUsers_ExecutiveId",
                schema: "public",
                table: "Opportunities",
                column: "ExecutiveId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_AspNetUsers_ExecutiveId",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_ExecutiveId",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "ExecutiveId",
                schema: "public",
                table: "Opportunities");
        }
    }
}
