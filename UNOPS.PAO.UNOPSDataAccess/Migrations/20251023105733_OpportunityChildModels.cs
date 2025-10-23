using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityChildModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Currencies",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "public",
                table: "Currencies",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "DecimalPlaces",
                schema: "public",
                table: "Currencies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Currencies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                schema: "public",
                table: "Currencies",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Countries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Iso2Code",
                schema: "public",
                table: "Countries",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "Countries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Iso3Code",
                schema: "public",
                table: "Countries",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ArtifactDataTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ArtifactDataTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Currency_Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Registered_Rate = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Exchange_Rate = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Currency_Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Effective_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Exchange_Rate_Start_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Exchange_Rate_End_Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Is_Current_Flag = table.Column<int>(type: "integer", nullable: true),
                    Exchange_Rate_Sequence_No = table.Column<int>(type: "integer", nullable: true),
                    Exchange_Rate_Line_Source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Rate_Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SDGs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SDGId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SDGNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SDGDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SDGLogo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SDGLongDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDGs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArtifactTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ArtifactTypeCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ArtifactDataTypeId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApplicableEntityTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsUsedForCalculations = table.Column<bool>(type: "boolean", nullable: false),
                    IsUsedForAI = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ArtifactTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtifactTypes_ArtifactDataTypes_ArtifactDataTypeId",
                        column: x => x.ArtifactDataTypeId,
                        principalSchema: "public",
                        principalTable: "ArtifactDataTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtifactExtractionRules",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceArtifactTypeId = table.Column<int>(type: "integer", nullable: false),
                    ExtractedArtifactTypeId = table.Column<int>(type: "integer", nullable: false),
                    RulePrompt = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "integer", nullable: false),
                    MinimumConfidenceScore = table.Column<decimal>(type: "numeric", maxLength: 3, nullable: true),
                    AutoExecute = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_ArtifactExtractionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtifactExtractionRules_ArtifactTypes_ExtractedArtifactType~",
                        column: x => x.ExtractedArtifactTypeId,
                        principalSchema: "public",
                        principalTable: "ArtifactTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArtifactExtractionRules_ArtifactTypes_SourceArtifactTypeId",
                        column: x => x.SourceArtifactTypeId,
                        principalSchema: "public",
                        principalTable: "ArtifactTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntityArtifacts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    ArtifactTypeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValueText = table.Column<string>(type: "text", nullable: true),
                    ValueNumber = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    ValueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValueJson = table.Column<string>(type: "text", nullable: true),
                    DocumentId = table.Column<int>(type: "integer", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsExtracted = table.Column<bool>(type: "boolean", nullable: false),
                    SourceArtifactId = table.Column<int>(type: "integer", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
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
                    table.PrimaryKey("PK_EntityArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityArtifacts_ArtifactTypes_ArtifactTypeId",
                        column: x => x.ArtifactTypeId,
                        principalSchema: "public",
                        principalTable: "ArtifactTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityArtifacts_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntityArtifacts_EntityArtifacts_SourceArtifactId",
                        column: x => x.SourceArtifactId,
                        principalSchema: "public",
                        principalTable: "EntityArtifacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                schema: "public",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Name",
                schema: "public",
                table: "Currencies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Status",
                schema: "public",
                table: "Currencies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Iso2Code",
                schema: "public",
                table: "Countries",
                column: "Iso2Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Iso3Code",
                schema: "public",
                table: "Countries",
                column: "Iso3Code");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                schema: "public",
                table: "Countries",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Status",
                schema: "public",
                table: "Countries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactDataTypes_Name",
                schema: "public",
                table: "ArtifactDataTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactDataTypes_Order",
                schema: "public",
                table: "ArtifactDataTypes",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactExtractionRules_ExecutionOrder",
                schema: "public",
                table: "ArtifactExtractionRules",
                column: "ExecutionOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactExtractionRules_ExtractedArtifactTypeId",
                schema: "public",
                table: "ArtifactExtractionRules",
                column: "ExtractedArtifactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactExtractionRules_IsActive",
                schema: "public",
                table: "ArtifactExtractionRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactExtractionRules_SourceArtifactTypeId",
                schema: "public",
                table: "ArtifactExtractionRules",
                column: "SourceArtifactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactTypes_ArtifactDataTypeId",
                schema: "public",
                table: "ArtifactTypes",
                column: "ArtifactDataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactTypes_ArtifactTypeCode",
                schema: "public",
                table: "ArtifactTypes",
                column: "ArtifactTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactTypes_Category",
                schema: "public",
                table: "ArtifactTypes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactTypes_Order",
                schema: "public",
                table: "ArtifactTypes",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_EntityArtifacts_ArtifactTypeId",
                schema: "public",
                table: "EntityArtifacts",
                column: "ArtifactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityArtifacts_DocumentId",
                schema: "public",
                table: "EntityArtifacts",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityArtifacts_EffectiveDate",
                schema: "public",
                table: "EntityArtifacts",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_EntityArtifacts_EntityType_EntityId",
                schema: "public",
                table: "EntityArtifacts",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EntityArtifacts_IsExtracted",
                schema: "public",
                table: "EntityArtifacts",
                column: "IsExtracted");

            migrationBuilder.CreateIndex(
                name: "IX_EntityArtifacts_SourceArtifactId",
                schema: "public",
                table: "EntityArtifacts",
                column: "SourceArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Currency_Effective_Date",
                schema: "public",
                table: "ExchangeRates",
                columns: new[] { "Currency", "Effective_Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Exchange_Rate_End_Date",
                schema: "public",
                table: "ExchangeRates",
                column: "Exchange_Rate_End_Date");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Exchange_Rate_Start_Date",
                schema: "public",
                table: "ExchangeRates",
                column: "Exchange_Rate_Start_Date");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Is_Current_Flag",
                schema: "public",
                table: "ExchangeRates",
                column: "Is_Current_Flag");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Status",
                schema: "public",
                table: "ExchangeRates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SDGs_SDGId",
                schema: "public",
                table: "SDGs",
                column: "SDGId");

            migrationBuilder.CreateIndex(
                name: "IX_SDGs_SDGNumber",
                schema: "public",
                table: "SDGs",
                column: "SDGNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SDGs_Status",
                schema: "public",
                table: "SDGs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtifactExtractionRules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EntityArtifacts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExchangeRates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SDGs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ArtifactTypes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ArtifactDataTypes",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Currencies_Code",
                schema: "public",
                table: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Currencies_Name",
                schema: "public",
                table: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Currencies_Status",
                schema: "public",
                table: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Countries_Iso2Code",
                schema: "public",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Countries_Iso3Code",
                schema: "public",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Countries_Name",
                schema: "public",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Countries_Status",
                schema: "public",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "DecimalPlaces",
                schema: "public",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "Symbol",
                schema: "public",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "Iso3Code",
                schema: "public",
                table: "Countries");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Currencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "public",
                table: "Currencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "public",
                table: "Countries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Iso2Code",
                schema: "public",
                table: "Countries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);
        }
    }
}
