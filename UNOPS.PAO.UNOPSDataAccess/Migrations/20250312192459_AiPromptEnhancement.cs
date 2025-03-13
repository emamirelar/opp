using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AiPromptEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentConfig",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GenerationConfig",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Project",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentConfig",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "GenerationConfig",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "Model",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "Project",
                schema: "public",
                table: "AiPrompt");
        }
    }
}
