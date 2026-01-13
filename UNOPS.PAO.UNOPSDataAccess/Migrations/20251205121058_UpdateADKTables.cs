using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateADKTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // COMMENTED OUT: Google ADK creates the 'events' table with all required columns
            // These migrations were attempting to add columns to a table that may not exist yet
            // Let Google ADK handle the table creation on first use
            
            /* 
            // Add new columns required by Google ADK 1.3.0+
            // These columns are used by the DatabaseSessionService for session management
            
            // Add custom_metadata column if it doesn't exist
            migrationBuilder.Sql(@"
                ALTER TABLE events ADD COLUMN IF NOT EXISTS custom_metadata JSONB;
            ");
            
            // Add input_transcription column if it doesn't exist
            migrationBuilder.Sql(@"
                ALTER TABLE events ADD COLUMN IF NOT EXISTS input_transcription TEXT;
            ");
            
            // Add output_transcription column if it doesn't exist
            migrationBuilder.Sql(@"
                ALTER TABLE events ADD COLUMN IF NOT EXISTS output_transcription TEXT;
            ");
            
            // Fix any 'null' string values to actual NULL (ADK expects NULL, not string 'null')
            migrationBuilder.Sql(@"
                UPDATE events SET input_transcription = NULL WHERE input_transcription = 'null';
                UPDATE events SET output_transcription = NULL WHERE output_transcription = 'null';
                UPDATE events SET input_transcription = NULL WHERE input_transcription = '';
                UPDATE events SET output_transcription = NULL WHERE output_transcription = '';
                UPDATE events SET custom_metadata = NULL WHERE custom_metadata::text = 'null';
            ");
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the columns added for ADK 1.3.0+
            // Note: This will lose any data in these columns
            migrationBuilder.Sql(@"
                ALTER TABLE events DROP COLUMN IF EXISTS custom_metadata;
                ALTER TABLE events DROP COLUMN IF EXISTS input_transcription;
                ALTER TABLE events DROP COLUMN IF EXISTS output_transcription;
            ");
        }
    }
}
