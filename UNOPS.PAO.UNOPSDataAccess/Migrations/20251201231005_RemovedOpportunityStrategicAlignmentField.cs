using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovedOpportunityStrategicAlignmentField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StrategicAlignment",
                schema: "public",
                table: "Opportunities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StrategicAlignment",
                schema: "public",
                table: "Opportunities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
