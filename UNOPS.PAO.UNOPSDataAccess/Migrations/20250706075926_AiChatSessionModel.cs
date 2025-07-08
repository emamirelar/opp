using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AiChatSessionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                schema: "public",
                table: "AiChatSession");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                schema: "public",
                table: "AiChatSession",
                newName: "LastUpdated");

            migrationBuilder.AddColumn<bool>(
                name: "AdminCanChange",
                schema: "public",
                table: "AiPrompt",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AiGenerateTitle",
                schema: "public",
                table: "AiChatSession",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminCanChange",
                schema: "public",
                table: "AiPrompt");

            migrationBuilder.DropColumn(
                name: "AiGenerateTitle",
                schema: "public",
                table: "AiChatSession");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                schema: "public",
                table: "AiChatSession",
                newName: "StartTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                schema: "public",
                table: "AiChatSession",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
