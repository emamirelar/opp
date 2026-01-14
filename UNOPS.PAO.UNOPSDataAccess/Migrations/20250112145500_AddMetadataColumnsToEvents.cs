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
           /* migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'events' AND column_name = 'usage_metadata'
                    ) THEN
                        ALTER TABLE events ADD COLUMN usage_metadata JSONB;
                    END IF;
                    
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'events' AND column_name = 'citation_metadata'
                    ) THEN
                        ALTER TABLE events ADD COLUMN citation_metadata JSONB;
                    END IF;
                END $$;
            "); */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'events' AND column_name = 'usage_metadata'
                    ) THEN
                        ALTER TABLE events DROP COLUMN usage_metadata;
                    END IF;
                    
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'events' AND column_name = 'citation_metadata'
                    ) THEN
                        ALTER TABLE events DROP COLUMN citation_metadata;
                    END IF;
                END $$;
            ");
        }
    }
}

