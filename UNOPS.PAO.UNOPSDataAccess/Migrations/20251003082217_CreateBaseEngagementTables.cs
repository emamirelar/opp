using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateBaseEngagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop PhoneNumbers column only if it exists
            migrationBuilder.Sql(@"
                ALTER TABLE public.""Interactions""
                DROP COLUMN IF EXISTS ""PhoneNumbers"";
            ");

            // Create BaseEngagements table (if not exists)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS public.""BaseEngagements"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Name"" text NOT NULL DEFAULT '',
                    ""Status"" integer NOT NULL DEFAULT 0,
                    ""IsDeleted"" boolean NOT NULL DEFAULT false,
                    ""EngagementNumber"" character varying(50) NOT NULL DEFAULT '',
                    ""BaseEngagement"" character varying(50),
                    ""EngagementImplementationStartDate"" timestamp with time zone,
                    ""EngagementImplementationEndDate"" timestamp with time zone,
                    ""EngagementSignedDate"" timestamp with time zone,
                    ""EngagementAmount"" numeric,
                    ""EngagementStage"" character varying(100),
                    ""EngagementStageDescription"" character varying(255),
                    ""BusinessDeveloper"" character varying(255),
                    ""BusinessDeveloperName"" character varying(255),
                    ""BusinessDeveloperEmailAddress"" character varying(255),
                    ""EngagementProjectExecutive"" character varying(255),
                    ""EngagementProjectExecutiveName"" character varying(255),
                    ""ImplementationCountriesList"" text,
                    ""OutputsList"" text,
                    ""SDGList"" text,
                    ""EngagementDescription"" text,
                    ""EngagementLongDescription"" text
                );
            ");

            // Create BaseEngagementPartners table (if not exists)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS public.""BaseEngagementPartners"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Name"" text NOT NULL DEFAULT '',
                    ""Status"" integer NOT NULL DEFAULT 0,
                    ""IsDeleted"" boolean NOT NULL DEFAULT false,
                    ""Key"" character varying(200) NOT NULL,
                    ""BaseEngagement"" character varying(50) NOT NULL,
                    ""PartnerType"" character varying(50),
                    ""Partner"" character varying(50),
                    ""PartnerDescription"" character varying(255),
                    ""PartnerId"" integer,
                    ""BaseEngagementId"" integer
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop BaseEngagement tables first
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"BaseEngagementPartners\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"BaseEngagements\";");

            // Re-add PhoneNumbers column if it doesn't exist
            migrationBuilder.Sql(@"
                ALTER TABLE public.""Interactions""
                ADD COLUMN IF NOT EXISTS ""PhoneNumbers"" text;
            ");
        }
    }
}
