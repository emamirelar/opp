using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PartnerEcosystemModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"TRUNCATE TABLE public.""Partners"" CASCADE;");
            migrationBuilder.DropColumn(
                name: "Address1City",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address1Country",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address1PostalCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address1StateProvince",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address1Street",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Address1Street2",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "DDEACDone",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "DDRequired",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "EACReference",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "LevyPotentiallyApplies",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "NewEngagement",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "ReasonForLevyNotApplying",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "ShortName",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "public",
                table: "Partners");

            migrationBuilder.RenameColumn(
                name: "UNSecretariatEntity",
                schema: "public",
                table: "Partners",
                newName: "UNSecretariatPartner");

            migrationBuilder.RenameColumn(
                name: "GlobalKeyAccount",
                schema: "public",
                table: "Partners",
                newName: "UNAndStateEntity");

            // Add a temporary column, convert values, then swap columns
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" ADD COLUMN ""Status_temp"" integer;");
            
            migrationBuilder.Sql(@"
                UPDATE public.""Partners"" 
                SET ""Status_temp"" = CASE 
                    WHEN ""Status"" = 'Active' THEN 1
                    WHEN ""Status"" = 'Inactive' THEN 0 
                    WHEN ""Status"" = 'Draft' THEN 3
                    WHEN ""Status"" = 'Closed' THEN 2
                    WHEN ""Status"" = 'Archived' THEN 4
                    ELSE 0
                END;");
                
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" DROP COLUMN ""Status"";");
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" RENAME COLUMN ""Status_temp"" TO ""Status"";");

            // Add a temporary column, convert values, then swap columns for PooledFund
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" ADD COLUMN ""PooledFund_temp"" boolean;");
            
            migrationBuilder.Sql(@"
                UPDATE public.""Partners"" 
                SET ""PooledFund_temp"" = CASE 
                    WHEN LOWER(""PooledFund"") IN ('true', 't', 'yes', 'y', '1') THEN true
                    WHEN LOWER(""PooledFund"") IN ('false', 'f', 'no', 'n', '0') THEN false
                    ELSE false
                END;");
                
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" DROP COLUMN ""PooledFund"";");
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" RENAME COLUMN ""PooledFund_temp"" TO ""PooledFund"";");

            migrationBuilder.AlterColumn<string>(
                name: "LevyTreatment",
                schema: "public",
                table: "Partners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateNewOpportunities",
                schema: "public",
                table: "Partners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DueDiligenceApproval",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDiligenceApprovalDate",
                schema: "public",
                table: "Partners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDiligenceExpiryDate",
                schema: "public",
                table: "Partners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DueDiligenceRequired",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ErpDimValue",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "KeyGlobalPartner",
                schema: "public",
                table: "Partners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LiaisonOfficeId",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PartnerApprovalDate",
                schema: "public",
                table: "Partners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerApprovalReference",
                schema: "public",
                table: "Partners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerApprovalStatus",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartnerCategoryId",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerCategoryInternalKey",
                schema: "public",
                table: "Partners",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerCategoryKey",
                schema: "public",
                table: "Partners",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PartnerDescription",
                schema: "public",
                table: "Partners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PartnerExternalReportLevel",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerInternalReportLevel",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerKey",
                schema: "public",
                table: "Partners",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PartnerLevelCode",
                schema: "public",
                table: "Partners",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerLevelDescription",
                schema: "public",
                table: "Partners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerLevelShort",
                schema: "public",
                table: "Partners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerLevyStatus",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PartnerLongDescription",
                schema: "public",
                table: "Partners",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerOrgUnitId",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerScope",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerShortDescription",
                schema: "public",
                table: "Partners",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PartnerTypeKey",
                schema: "public",
                table: "Partners",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ReasonForLevy",
                schema: "public",
                table: "Partners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonForNoNewOpportunity",
                schema: "public",
                table: "Partners",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UniqueKey",
                schema: "public",
                table: "Partners",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "LiaisonOffices",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiaisonOffices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_LiaisonOfficeId",
                schema: "public",
                table: "Partners",
                column: "LiaisonOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_LiaisonOffices_Code",
                schema: "public",
                table: "LiaisonOffices",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_LiaisonOffices_LiaisonOfficeId",
                schema: "public",
                table: "Partners",
                column: "LiaisonOfficeId",
                principalSchema: "public",
                principalTable: "LiaisonOffices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            MigrationSqlScriptExecutor.ExecuteSqlScripts(migrationBuilder, new[]
            {
                "seed-roles.sql"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_LiaisonOffices_LiaisonOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropTable(
                name: "LiaisonOffices",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Partners_LiaisonOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "CanCreateNewOpportunities",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "DueDiligenceApproval",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "DueDiligenceApprovalDate",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "DueDiligenceExpiryDate",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "DueDiligenceRequired",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "ErpDimValue",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "KeyGlobalPartner",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "LiaisonOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerApprovalDate",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerApprovalReference",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerApprovalStatus",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerCategoryId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerCategoryInternalKey",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerCategoryKey",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerDescription",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerExternalReportLevel",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerInternalReportLevel",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerKey",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerLevelCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerLevelDescription",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerLevelShort",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerLevyStatus",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerLongDescription",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerOrgUnitId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerScope",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerShortDescription",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerTypeKey",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "ReasonForLevy",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "ReasonForNoNewOpportunity",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "UniqueKey",
                schema: "public",
                table: "Partners");

            migrationBuilder.RenameColumn(
                name: "UNSecretariatPartner",
                schema: "public",
                table: "Partners",
                newName: "UNSecretariatEntity");

            migrationBuilder.RenameColumn(
                name: "UNAndStateEntity",
                schema: "public",
                table: "Partners",
                newName: "GlobalKeyAccount");

            // Convert Status back to string using temporary column approach
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" ADD COLUMN ""Status_temp"" text;");
            
            migrationBuilder.Sql(@"
                UPDATE public.""Partners"" 
                SET ""Status_temp"" = CASE 
                    WHEN ""Status"" = 1 THEN 'Active'
                    WHEN ""Status"" = 0 THEN 'Inactive'
                    WHEN ""Status"" = 3 THEN 'Draft'
                    WHEN ""Status"" = 2 THEN 'Closed'
                    WHEN ""Status"" = 4 THEN 'Archived'
                    ELSE 'Inactive'
                END;");
                
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" DROP COLUMN ""Status"";");
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" RENAME COLUMN ""Status_temp"" TO ""Status"";");

            // Convert PooledFund back to string using temporary column approach
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" ADD COLUMN ""PooledFund_temp"" text;");
            
            migrationBuilder.Sql(@"
                UPDATE public.""Partners"" 
                SET ""PooledFund_temp"" = CASE 
                    WHEN ""PooledFund"" = true THEN 'True'
                    WHEN ""PooledFund"" = false THEN 'False'
                    ELSE 'False'
                END;");
                
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" DROP COLUMN ""PooledFund"";");
            migrationBuilder.Sql(@"ALTER TABLE public.""Partners"" RENAME COLUMN ""PooledFund_temp"" TO ""PooledFund"";");

            migrationBuilder.AlterColumn<string>(
                name: "LevyTreatment",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address1City",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address1Country",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address1PostalCode",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address1StateProvince",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address1Street",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address1Street2",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DDEACDone",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DDRequired",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EACReference",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LevyPotentiallyApplies",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewEngagement",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonForLevyNotApplying",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);
        }
    }
}
