using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOpportunityStatementMarkdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpportunityStatementMarkdown",
                schema: "public",
                table: "Opportunities",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpportunityStatementMarkdown",
                schema: "public",
                table: "Opportunities");
        }
    }
}

