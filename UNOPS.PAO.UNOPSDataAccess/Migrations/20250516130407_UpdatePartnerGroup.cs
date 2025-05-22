using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePartnerGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerTreeCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.RenameColumn(
                name: "PartnerTreeCode",
                schema: "public",
                table: "Partners",
                newName: "PartnerGroupCode");

            migrationBuilder.RenameIndex(
                name: "IX_Partners_PartnerTreeCode",
                schema: "public",
                table: "Partners",
                newName: "IX_Partners_PartnerGroupCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerGroupCode",
                schema: "public",
                table: "Partners",
                column: "PartnerGroupCode",
                principalSchema: "public",
                principalTable: "PartnerTrees",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerGroupCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.RenameColumn(
                name: "PartnerGroupCode",
                schema: "public",
                table: "Partners",
                newName: "PartnerTreeCode");

            migrationBuilder.RenameIndex(
                name: "IX_Partners_PartnerGroupCode",
                schema: "public",
                table: "Partners",
                newName: "IX_Partners_PartnerTreeCode");

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
