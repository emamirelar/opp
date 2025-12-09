using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOpportunityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Challenges",
                schema: "public",
                table: "Opportunities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_CreatedBy",
                schema: "public",
                table: "Opportunities",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_LastModifiedBy",
                schema: "public",
                table: "Opportunities",
                column: "LastModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_AspNetUsers_CreatedBy",
                schema: "public",
                table: "Opportunities",
                column: "CreatedBy",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Opportunities_AspNetUsers_LastModifiedBy",
                schema: "public",
                table: "Opportunities",
                column: "LastModifiedBy",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_AspNetUsers_CreatedBy",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Opportunities_AspNetUsers_LastModifiedBy",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_CreatedBy",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.DropIndex(
                name: "IX_Opportunities_LastModifiedBy",
                schema: "public",
                table: "Opportunities");

            migrationBuilder.AlterColumn<string>(
                name: "Challenges",
                schema: "public",
                table: "Opportunities",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
