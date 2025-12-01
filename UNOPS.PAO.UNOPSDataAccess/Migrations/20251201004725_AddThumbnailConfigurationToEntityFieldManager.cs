using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailConfigurationToEntityFieldManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'EntityFieldManagers' 
                        AND column_name = 'ThumbnailBorder'
                    ) THEN
                        ALTER TABLE public.""EntityFieldManagers"" 
                        ADD COLUMN ""ThumbnailBorder"" boolean NULL;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'EntityFieldManagers' 
                        AND column_name = 'ThumbnailFallback'
                    ) THEN
                        ALTER TABLE public.""EntityFieldManagers"" 
                        ADD COLUMN ""ThumbnailFallback"" character varying(500) NULL;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'EntityFieldManagers' 
                        AND column_name = 'ThumbnailShape'
                    ) THEN
                        ALTER TABLE public.""EntityFieldManagers"" 
                        ADD COLUMN ""ThumbnailShape"" character varying(20) NULL;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'EntityFieldManagers' 
                        AND column_name = 'ThumbnailSize'
                    ) THEN
                        ALTER TABLE public.""EntityFieldManagers"" 
                        ADD COLUMN ""ThumbnailSize"" character varying(20) NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'EntityFieldManagers' 
                        AND column_name = 'ThumbnailBorder'
                    ) THEN
                        ALTER TABLE public.""EntityFieldManagers"" 
                        DROP COLUMN ""ThumbnailBorder"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'EntityFieldManagers' 
                        AND column_name = 'ThumbnailFallback'
                    ) THEN
                        ALTER TABLE public.""EntityFieldManagers"" 
                        DROP COLUMN ""ThumbnailFallback"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'EntityFieldManagers' 
                        AND column_name = 'ThumbnailShape'
                    ) THEN
                        ALTER TABLE public.""EntityFieldManagers"" 
                        DROP COLUMN ""ThumbnailShape"";
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_schema = 'public' 
                        AND table_name = 'EntityFieldManagers' 
                        AND column_name = 'ThumbnailSize'
                    ) THEN
                        ALTER TABLE public.""EntityFieldManagers"" 
                        DROP COLUMN ""ThumbnailSize"";
                    END IF;
                END $$;
            ");
        }
    }
}
