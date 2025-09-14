using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PartnerEcosystemRefinement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"TRUNCATE TABLE public.""Partners"" CASCADE;");
            migrationBuilder.DropColumn(
                name: "PartnerExternalReportLevel",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerInternalReportLevel",
                schema: "public",
                table: "Partners");

            migrationBuilder.RenameColumn(
                name: "PartnerOrgUnitId",
                schema: "public",
                table: "Partners",
                newName: "PartnerFocalPointUserId");

             MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            {
                "seed-roles.sql"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PartnerFocalPointUserId",
                schema: "public",
                table: "Partners",
                newName: "PartnerOrgUnitId");

            migrationBuilder.AddColumn<int>(
                name: "PartnerExternalReportLevel",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerInternalReportLevel",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);
        }
    }
}
