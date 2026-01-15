using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOpportunityFieldLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Truncate existing names longer than 120 characters before altering column
            migrationBuilder.Sql(@"
                UPDATE public.""Opportunities""
                SET ""Name"" = SUBSTRING(""Name"", 1, 120)
                WHERE LENGTH(""Name"") > 120;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Opportunities",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            // Truncate existing challenges longer than 1020 characters before altering column
            migrationBuilder.Sql(@"
                UPDATE public.""Opportunities""
                SET ""Challenges"" = SUBSTRING(""Challenges"", 1, 1020)
                WHERE ""Challenges"" IS NOT NULL AND LENGTH(""Challenges"") > 1020;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Challenges",
                schema: "public",
                table: "Opportunities",
                type: "character varying(1020)",
                maxLength: 1020,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Opportunities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Challenges",
                schema: "public",
                table: "Opportunities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1020)",
                oldMaxLength: 1020,
                oldNullable: true);
        }
    }
}
