using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedScriptTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeedScripts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScriptName = table.Column<string>(type: "text", nullable: false),
                    FileHash = table.Column<string>(type: "text", nullable: false),
                    LastExecutedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedScripts", x => x.Id);
                });

                migrationBuilder.AddColumn<int>(
                    name: "ExecutionOrder",
                    schema: "public",
                    table: "SeedScripts",
                    type: "integer",
                    nullable: false,
                    defaultValue: 0);

                migrationBuilder.AddColumn<string>(
                    name: "ScriptType",
                    schema: "public",
                    table: "SeedScripts",
                    type: "text",
                    nullable: false,
                    defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeedScripts",
                schema: "public");
        }
    }
}
