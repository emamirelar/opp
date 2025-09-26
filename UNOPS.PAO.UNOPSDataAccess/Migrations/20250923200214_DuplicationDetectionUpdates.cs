using Microsoft.EntityFrameworkCore.Migrations;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DuplicationDetectionUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            {
                "Detect_Duplicate_Records.sql"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
