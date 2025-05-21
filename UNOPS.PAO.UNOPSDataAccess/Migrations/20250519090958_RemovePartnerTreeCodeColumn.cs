using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovePartnerTreeCodeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerTreeCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerTreeCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerTreeCode",
                schema: "public",
                table: "Partners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartnerTreeCode",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerTreeCode",
                schema: "public",
                table: "Partners",
                column: "PartnerTreeCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerTreeCode",
                schema: "public",
                table: "Partners",
                column: "PartnerTreeCode",
                principalSchema: "public",
                principalTable: "PartnerTrees",
                principalColumn: "Code");
        }
    }
}
