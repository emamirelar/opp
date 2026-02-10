using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class GoogleADKUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sessions table: allow timezone-aware datetimes from google-adk (fixes asyncpg
            // "can't subtract offset-naive and offset-aware datetimes" after ADK upgrade).
            // Existing naive timestamps are interpreted as UTC.
            migrationBuilder.Sql(@"
                ALTER TABLE sessions
                    ALTER COLUMN create_time TYPE TIMESTAMP WITH TIME ZONE USING create_time AT TIME ZONE 'UTC',
                    ALTER COLUMN update_time TYPE TIMESTAMP WITH TIME ZONE USING update_time AT TIME ZONE 'UTC';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert sessions table to TIMESTAMP WITHOUT TIME ZONE (naive).
            migrationBuilder.Sql(@"
                ALTER TABLE sessions
                    ALTER COLUMN create_time TYPE TIMESTAMP WITHOUT TIME ZONE USING create_time AT TIME ZONE 'UTC',
                    ALTER COLUMN update_time TYPE TIMESTAMP WITHOUT TIME ZONE USING update_time AT TIME ZONE 'UTC';
            ");
        }
    }
}
