using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EntityManagerChangeLog1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableChangeLog",
                schema: "public",
                table: "EntityFieldManagers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Execute SQL scripts for seeding data using the utility class
            MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            {
                "seed-entities.sql",
                "seed-entity-field-managers.sql"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableChangeLog",
                schema: "public",
                table: "EntityFieldManagers");
        }
    }
}
