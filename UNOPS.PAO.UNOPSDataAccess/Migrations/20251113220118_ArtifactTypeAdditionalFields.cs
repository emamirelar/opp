using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ArtifactTypeAdditionalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowBulkUpdate",
                schema: "public",
                table: "ArtifactTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSearchable",
                schema: "public",
                table: "ArtifactTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "public",
                table: "ArtifactTypes",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowBulkUpdate",
                schema: "public",
                table: "ArtifactTypes");

            migrationBuilder.DropColumn(
                name: "IsSearchable",
                schema: "public",
                table: "ArtifactTypes");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "public",
                table: "ArtifactTypes");
        }
    }
}
