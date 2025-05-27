using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAiModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEmbedding",
                schema: "public",
                table: "EntityEmbeddings");

            migrationBuilder.AddColumn<string>(
                name: "EntityData",
                schema: "public",
                table: "EntityEmbeddings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityData",
                schema: "public",
                table: "EntityEmbeddings");

            migrationBuilder.AddColumn<byte[]>(
                name: "NameEmbedding",
                schema: "public",
                table: "EntityEmbeddings",
                type: "vector(768)",
                nullable: true);
        }
    }
}
