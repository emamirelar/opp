using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RiskModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "public",
                table: "Risks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<int>(
                name: "PreDefinedHighRiskId",
                schema: "public",
                table: "Risks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskCategoryId",
                schema: "public",
                table: "Risks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskImpactLevelId",
                schema: "public",
                table: "Risks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskProbabilityId",
                schema: "public",
                table: "Risks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskProximityId",
                schema: "public",
                table: "Risks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RiskResponseTypeId",
                schema: "public",
                table: "Risks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskTypeId",
                schema: "public",
                table: "Risks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RiskCategories",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ParentCategoryId = table.Column<int>(type: "integer", nullable: true),
                    ParentShortCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_RiskCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskCategories_RiskCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalSchema: "public",
                        principalTable: "RiskCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiskImpactLevels",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumericValue = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskImpactLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskProbabilities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NumericValue = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskProbabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskProximities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MonthsValue = table.Column<int>(type: "integer", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskProximities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskResponseTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValidForThreat = table.Column<bool>(type: "boolean", nullable: false),
                    ValidForOpportunity = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskResponseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsResponseTypeMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreDefinedHighRisks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Level1 = table.Column<int>(type: "integer", nullable: false),
                    Level2Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OupQuestionId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ShortTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsAutoDetectable = table.Column<bool>(type: "boolean", nullable: false),
                    DetectionRuleType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    RiskCategoryId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                    table.PrimaryKey("PK_PreDefinedHighRisks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreDefinedHighRisks_RiskCategories_RiskCategoryId",
                        column: x => x.RiskCategoryId,
                        principalSchema: "public",
                        principalTable: "RiskCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Risks_PreDefinedHighRiskId",
                schema: "public",
                table: "Risks",
                column: "PreDefinedHighRiskId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_RiskCategoryId",
                schema: "public",
                table: "Risks",
                column: "RiskCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_RiskImpactLevelId",
                schema: "public",
                table: "Risks",
                column: "RiskImpactLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_RiskProbabilityId",
                schema: "public",
                table: "Risks",
                column: "RiskProbabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_RiskProximityId",
                schema: "public",
                table: "Risks",
                column: "RiskProximityId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_RiskResponseTypeId",
                schema: "public",
                table: "Risks",
                column: "RiskResponseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_RiskTypeId",
                schema: "public",
                table: "Risks",
                column: "RiskTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDefinedHighRisks_CategoryCode",
                schema: "public",
                table: "PreDefinedHighRisks",
                column: "CategoryCode");

            migrationBuilder.CreateIndex(
                name: "IX_PreDefinedHighRisks_Code",
                schema: "public",
                table: "PreDefinedHighRisks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreDefinedHighRisks_IsAutoDetectable",
                schema: "public",
                table: "PreDefinedHighRisks",
                column: "IsAutoDetectable");

            migrationBuilder.CreateIndex(
                name: "IX_PreDefinedHighRisks_OupQuestionId",
                schema: "public",
                table: "PreDefinedHighRisks",
                column: "OupQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDefinedHighRisks_RiskCategoryId",
                schema: "public",
                table: "PreDefinedHighRisks",
                column: "RiskCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskCategories_Code",
                schema: "public",
                table: "RiskCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskCategories_Level",
                schema: "public",
                table: "RiskCategories",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_RiskCategories_ParentCategoryId",
                schema: "public",
                table: "RiskCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskCategories_ShortCode",
                schema: "public",
                table: "RiskCategories",
                column: "ShortCode");

            migrationBuilder.CreateIndex(
                name: "IX_RiskImpactLevels_Code",
                schema: "public",
                table: "RiskImpactLevels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskProbabilities_Code",
                schema: "public",
                table: "RiskProbabilities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskProximities_Code",
                schema: "public",
                table: "RiskProximities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskResponseTypes_Code",
                schema: "public",
                table: "RiskResponseTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskTypes_Code",
                schema: "public",
                table: "RiskTypes",
                column: "Code",
                unique: true);

            // Truncate existing Risks to avoid FK constraint violations
            // Existing risks don't have the new required FK values
            migrationBuilder.Sql("TRUNCATE TABLE public.\"Risks\" CASCADE;");

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_PreDefinedHighRisks_PreDefinedHighRiskId",
                schema: "public",
                table: "Risks",
                column: "PreDefinedHighRiskId",
                principalSchema: "public",
                principalTable: "PreDefinedHighRisks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_RiskCategories_RiskCategoryId",
                schema: "public",
                table: "Risks",
                column: "RiskCategoryId",
                principalSchema: "public",
                principalTable: "RiskCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_RiskImpactLevels_RiskImpactLevelId",
                schema: "public",
                table: "Risks",
                column: "RiskImpactLevelId",
                principalSchema: "public",
                principalTable: "RiskImpactLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_RiskProbabilities_RiskProbabilityId",
                schema: "public",
                table: "Risks",
                column: "RiskProbabilityId",
                principalSchema: "public",
                principalTable: "RiskProbabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_RiskProximities_RiskProximityId",
                schema: "public",
                table: "Risks",
                column: "RiskProximityId",
                principalSchema: "public",
                principalTable: "RiskProximities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_RiskResponseTypes_RiskResponseTypeId",
                schema: "public",
                table: "Risks",
                column: "RiskResponseTypeId",
                principalSchema: "public",
                principalTable: "RiskResponseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_RiskTypes_RiskTypeId",
                schema: "public",
                table: "Risks",
                column: "RiskTypeId",
                principalSchema: "public",
                principalTable: "RiskTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Risks_PreDefinedHighRisks_PreDefinedHighRiskId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_RiskCategories_RiskCategoryId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_RiskImpactLevels_RiskImpactLevelId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_RiskProbabilities_RiskProbabilityId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_RiskProximities_RiskProximityId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_RiskResponseTypes_RiskResponseTypeId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_RiskTypes_RiskTypeId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropTable(
                name: "PreDefinedHighRisks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RiskImpactLevels",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RiskProbabilities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RiskProximities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RiskResponseTypes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RiskTypes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RiskCategories",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Risks_PreDefinedHighRiskId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Risks_RiskCategoryId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Risks_RiskImpactLevelId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Risks_RiskProbabilityId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Risks_RiskProximityId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Risks_RiskResponseTypeId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_Risks_RiskTypeId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "PreDefinedHighRiskId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskCategoryId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskImpactLevelId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskProbabilityId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskProximityId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskResponseTypeId",
                schema: "public",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "RiskTypeId",
                schema: "public",
                table: "Risks");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "public",
                table: "Risks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
