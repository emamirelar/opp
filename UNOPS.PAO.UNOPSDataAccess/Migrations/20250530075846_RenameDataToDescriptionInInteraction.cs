using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameDataToDescriptionInInteraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the new Description column as text
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "Interactions",
                type: "text",
                nullable: true);

            // Convert existing Data (bytea) to Description (text) by decoding UTF8
            // Only for non-null Data values
            migrationBuilder.Sql(@"
                UPDATE ""public"".""Interactions"" 
                SET ""Description"" = convert_from(""Data"", 'UTF8')
                WHERE ""Data"" IS NOT NULL;
            ");

            // Drop the old Data column
            migrationBuilder.DropColumn(
                name: "Data",
                schema: "public",
                table: "Interactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add back the Data column as bytea
            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                schema: "public",
                table: "Interactions",
                type: "bytea",
                nullable: true);

            // Convert Description back to Data by encoding to UTF8
            // Only for non-null Description values
            migrationBuilder.Sql(@"
                UPDATE ""public"".""Interactions"" 
                SET ""Data"" = convert_to(""Description"", 'UTF8')
                WHERE ""Description"" IS NOT NULL;
            ");

            // Drop the Description column
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "Interactions");
        }
    }
}
