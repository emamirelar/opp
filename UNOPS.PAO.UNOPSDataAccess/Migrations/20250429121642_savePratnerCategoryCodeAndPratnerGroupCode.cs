using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class savePratnerCategoryCodeAndPratnerGroupCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PartnerGroup",
                schema: "public",
                table: "PartnerTrees",
                newName: "PartnerGroupCode");

            migrationBuilder.RenameColumn(
                name: "PartnerCategory",
                schema: "public",
                table: "PartnerTrees",
                newName: "PartnerCategoryCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PartnerGroupCode",
                schema: "public",
                table: "PartnerTrees",
                newName: "PartnerGroup");

            migrationBuilder.RenameColumn(
                name: "PartnerCategoryCode",
                schema: "public",
                table: "PartnerTrees",
                newName: "PartnerCategory");
        }
    }
}
