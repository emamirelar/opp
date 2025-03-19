using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Identity.Client;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AiChatHistoryUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "RawMessage",
                schema: "public",
                table: "AiChatHistory",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawMessage",
                schema: "public",
                table: "AiChatHistory");
        }
    }
}
