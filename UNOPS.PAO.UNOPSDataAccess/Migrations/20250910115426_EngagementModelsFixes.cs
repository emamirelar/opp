using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EngagementModelsFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First drop the foreign key that depends on the alternate key constraint
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Engagements_Partners_PartnerId",
            //     schema: "public",
            //     table: "Engagements");

            // Use SQL to safely handle constraint and index dropping

            migrationBuilder.DropForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements");

            migrationBuilder.Sql(@"
                -- Drop alternate key constraint if it exists
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints 
                        WHERE table_schema = 'public' 
                        AND table_name = 'Partners' 
                        AND constraint_name = 'AK_Partners_ErpDimValue'
                    ) THEN
                        ALTER TABLE public.""Partners"" DROP CONSTRAINT ""AK_Partners_ErpDimValue"";
                    END IF;
                END
                $$;
            ");

            // Drop the existing unique index if it exists
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS public.""IX_Partners_ErpDimValue"";
            ");

            // Alter ErpDimValue to be nullable
            migrationBuilder.AlterColumn<int?>(
                name: "ErpDimValue",
                table: "Partners",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            // Create new unique index with filter for non-null values
            migrationBuilder.CreateIndex(
                name: "IX_Partners_ErpDimValue",
                table: "Partners",
                column: "ErpDimValue",
                unique: true,
                filter: "\"ErpDimValue\" IS NOT NULL");

            // Recreate the foreign key constraint
            //migrationBuilder.AddForeignKey(
            //    name: "FK_Engagements_Partners_PartnerId",
            //    schema: "public",
            //    table: "Engagements",
            //    column: "PartnerId",
            //    principalSchema: "public",
            //    principalTable: "Partners",
            //    principalColumn: "ErpDimValue",
            //    onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // First drop the foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements");

            // Drop the filtered unique index
            migrationBuilder.DropIndex(
                name: "IX_Partners_ErpDimValue",
                table: "Partners");

            // Alter ErpDimValue back to non-nullable (this might fail if there are null values)
            migrationBuilder.AlterColumn<int>(
                name: "ErpDimValue",
                table: "Partners",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int?),
                oldType: "integer",
                oldNullable: true);

            // Recreate the alternate key constraint
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Partners_ErpDimValue",
                table: "Partners",
                column: "ErpDimValue");

            // Recreate the foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "ErpDimValue",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
