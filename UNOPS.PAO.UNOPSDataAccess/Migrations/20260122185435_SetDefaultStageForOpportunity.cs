using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SetDefaultStageForOpportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update all opportunities with null or empty Stage to default value "IDENTIFY & PROFILE"
            // This is necessary after migrating from WorkflowStage navigation property to Stage string property
            migrationBuilder.Sql(@"
                UPDATE public.""Opportunities""
                SET 
                    ""Stage"" = 'IDENTIFY & PROFILE'
                WHERE ""Stage"" IS NULL OR ""Stage"" = '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
