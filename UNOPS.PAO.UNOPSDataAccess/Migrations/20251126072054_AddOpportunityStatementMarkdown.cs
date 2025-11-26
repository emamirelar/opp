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
            // Check if column exists before adding it
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'Opportunities' 
                        AND column_name = 'OpportunityStatementMarkdown'
                    ) THEN
                        ALTER TABLE public.""Opportunities"" 
                        ADD COLUMN ""OpportunityStatementMarkdown"" text;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Check if column exists before dropping it
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'Opportunities' 
                        AND column_name = 'OpportunityStatementMarkdown'
                    ) THEN
                        ALTER TABLE public.""Opportunities"" 
                        DROP COLUMN ""OpportunityStatementMarkdown"";
                    END IF;
                END $$;
            ");
        }
    }
}
