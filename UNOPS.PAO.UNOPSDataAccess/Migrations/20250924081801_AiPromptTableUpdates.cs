using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AiPromptTableUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"TRUNCATE TABLE public.""AiPrompt"" RESTART IDENTITY CASCADE;");

            migrationBuilder.AddColumn<int>(
                name: "CacheInvalidationMinutes",
                schema: "public",
                table: "AiPrompt",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DataRetrievalMethod",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Feature",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SystemInstructions",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UseCache",
                schema: "public",
                table: "AiPrompt",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserPrompt",
                schema: "public",
                table: "AiPrompt",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CacheInvalidationMinutes",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "DataRetrievalMethod",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "Feature",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "SystemInstructions",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "UseCache",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "UserPrompt",
                schema: "public",
                table: "AiPrompt");
        }
    }
}
