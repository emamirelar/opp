using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEngagementPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BaseEngagement permissions are now handled by the seeding process in EntityPermissions.sql
            // No migration action needed as seeding will ensure permissions exist
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove BaseEngagement permissions
            migrationBuilder.Sql("""
                DELETE FROM "EntityPermissions" 
                WHERE "Entity" = 'BaseEngagement';
            """);
        }
    }
}
