using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunityModels2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceLine",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "ServiceLineReference",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "OpportunityDeliverables",
                newName: "Notes");

            migrationBuilder.AddColumn<int>(
                name: "OutputId",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ProjectCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Unit",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_Unit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Output",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OutputGroup = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OutputSubGroup = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OutputName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UnitId = table.Column<int>(type: "integer", nullable: true),
                    ProjectCategoryId = table.Column<int>(type: "integer", nullable: true),
                    OutputServiceLine = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_Output", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Output_ProjectCategory_ProjectCategoryId",
                        column: x => x.ProjectCategoryId,
                        principalSchema: "public",
                        principalTable: "ProjectCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Output_Unit_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "public",
                        principalTable: "Unit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityDeliverables_OutputId",
                schema: "public",
                table: "OpportunityDeliverables",
                column: "OutputId");

            migrationBuilder.CreateIndex(
                name: "IX_Output_ProjectCategoryId",
                schema: "public",
                table: "Output",
                column: "ProjectCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Output_UnitId",
                schema: "public",
                table: "Output",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityDeliverables_Output_OutputId",
                schema: "public",
                table: "OpportunityDeliverables",
                column: "OutputId",
                principalSchema: "public",
                principalTable: "Output",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityDeliverables_Output_OutputId",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropTable(
                name: "Output",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ProjectCategory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Unit",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_OpportunityDeliverables_OutputId",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "OutputId",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.RenameColumn(
                name: "Notes",
                schema: "public",
                table: "OpportunityDeliverables",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "ServiceLine",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceLineReference",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
