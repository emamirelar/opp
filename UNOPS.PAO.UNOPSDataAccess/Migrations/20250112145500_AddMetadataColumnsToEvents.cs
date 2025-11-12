using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataColumnsToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE events
                ADD COLUMN usage_metadata JSONB,
                ADD COLUMN citation_metadata JSONB;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE events
                DROP COLUMN IF EXISTS usage_metadata,
                DROP COLUMN IF EXISTS citation_metadata;
            ");
        }
    }
}

