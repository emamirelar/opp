using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InteractionModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_Contacts_ContactId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.AlterColumn<int>(
                name: "ContactId",
                schema: "public",
                table: "Interactions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_Contacts_ContactId",
                schema: "public",
                table: "Interactions",
                column: "ContactId",
                principalSchema: "public",
                principalTable: "Contacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_Contacts_ContactId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.AlterColumn<int>(
                name: "ContactId",
                schema: "public",
                table: "Interactions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_Contacts_ContactId",
                schema: "public",
                table: "Interactions",
                column: "ContactId",
                principalSchema: "public",
                principalTable: "Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
