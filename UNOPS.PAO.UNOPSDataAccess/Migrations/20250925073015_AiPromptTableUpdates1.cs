using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AiPromptTableUpdates1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prompt",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "PromptFunction",
                schema: "public",
                table: "AiPrompt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromptFunction",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
