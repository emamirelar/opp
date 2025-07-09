using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserPreferencesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "public",
                table: "UserInfos",
                type: "text",
                nullable: true,
                defaultValue: "en");

            migrationBuilder.AddColumn<bool>(
                name: "TextToSpeech",
                schema: "public",
                table: "UserInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                schema: "public",
                table: "UserInfos");

            migrationBuilder.DropColumn(
                name: "TextToSpeech",
                schema: "public",
                table: "UserInfos");
        }
    }
}
