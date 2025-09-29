using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEngagementsTableOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the Engagements table if it exists (safe operation)
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"Engagements\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: Cannot recreate the Engagements table in rollback as the entity model no longer exists
            // If rollback is needed, restore the Engagement entity first, then create a new migration
        }
    }
}
