using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddOpportunityBannerImageColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ DEFENSIVE - Add OpportunityBannerImage column with existence check
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'Opportunities' 
                        AND column_name = 'OpportunityBannerImage'
                    ) THEN
                        ALTER TABLE public.""Opportunities"" 
                        ADD COLUMN ""OpportunityBannerImage"" text;
                    END IF;
                END $$;
            ");

            // ✅ DEFENSIVE - Add OpportunityThumbnail column with existence check
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'Opportunities' 
                        AND column_name = 'OpportunityThumbnail'
                    ) THEN
                        ALTER TABLE public.""Opportunities"" 
                        ADD COLUMN ""OpportunityThumbnail"" text;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ DEFENSIVE - Remove OpportunityBannerImage column with existence check
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'Opportunities' 
                        AND column_name = 'OpportunityBannerImage'
                    ) THEN
                        ALTER TABLE public.""Opportunities"" 
                        DROP COLUMN ""OpportunityBannerImage"";
                    END IF;
                END $$;
            ");

            // ✅ DEFENSIVE - Remove OpportunityThumbnail column with existence check
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'Opportunities' 
                        AND column_name = 'OpportunityThumbnail'
                    ) THEN
                        ALTER TABLE public.""Opportunities"" 
                        DROP COLUMN ""OpportunityThumbnail"";
                    END IF;
                END $$;
            ");
        }
    }
}
