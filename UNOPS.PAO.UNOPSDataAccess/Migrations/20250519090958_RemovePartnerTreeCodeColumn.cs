using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemovePartnerTreeCodeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if the constraint exists before trying to drop it
            migrationBuilder.Sql(
                @"DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_constraint 
                        WHERE conname = 'FK_Partners_PartnerTrees_PartnerTreeCode'
                    ) THEN
                        ALTER TABLE ""public"".""Partners"" DROP CONSTRAINT ""FK_Partners_PartnerTrees_PartnerTreeCode"";
                    END IF;
                END $$;");

            // Check if the index exists before trying to drop it
            migrationBuilder.Sql(
                @"DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE indexname = 'IX_Partners_PartnerTreeCode'
                    ) THEN
                        DROP INDEX ""public"".""IX_Partners_PartnerTreeCode"";
                    END IF;
                END $$;");

            // Check if the column exists before trying to drop it
            migrationBuilder.Sql(
                @"DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'public' AND table_name = 'Partners' AND column_name = 'PartnerTreeCode'
                    ) THEN
                        ALTER TABLE ""public"".""Partners"" DROP COLUMN ""PartnerTreeCode"";
                    END IF;
                END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartnerTreeCode",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerTreeCode",
                schema: "public",
                table: "Partners",
                column: "PartnerTreeCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerTreeCode",
                schema: "public",
                table: "Partners",
                column: "PartnerTreeCode",
                principalSchema: "public",
                principalTable: "PartnerTrees",
                principalColumn: "Code");
        }
    }
}
