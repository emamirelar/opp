using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AiPromptAddedToolsAndSafetySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SafetySettings",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToolsConfig",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SafetySettings",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "ToolsConfig",
                schema: "public",
                table: "AiPrompt");
        }
    }
}
