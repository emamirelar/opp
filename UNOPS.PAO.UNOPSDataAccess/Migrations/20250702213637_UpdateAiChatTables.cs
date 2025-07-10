using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAiChatTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiChatHistory",
                schema: "public");

            migrationBuilder.RenameColumn(
                name: "TextToSpeech",
                schema: "public",
                table: "AiChatSession",
                newName: "Starred");

            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                schema: "public",
                table: "AiChatSession",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "public",
                table: "AiChatSession",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Archived",
                schema: "public",
                table: "AiChatSession");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "public",
                table: "AiChatSession");

            migrationBuilder.RenameColumn(
                name: "Starred",
                schema: "public",
                table: "AiChatSession",
                newName: "TextToSpeech");

            migrationBuilder.CreateTable(
                name: "AiChatHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    MediaType = table.Column<string>(type: "text", nullable: true),
                    MediaUrl = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    RawMessage = table.Column<string>(type: "text", nullable: true),
                    RequestType = table.Column<string>(type: "text", nullable: true),
                    Sender = table.Column<string>(type: "text", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiChatHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiChatHistory_AiChatSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "AiChatSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiChatHistory_SessionId",
                schema: "public",
                table: "AiChatHistory",
                column: "SessionId");
        }
    }
}
