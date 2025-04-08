using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PartnerOfficeAndPartnerCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerCategoryId",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerOfficeId",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
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
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartnerCategories",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
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
                    table.PrimaryKey("PK_PartnerCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerCategoryId",
                schema: "public",
                table: "Partners",
                column: "PartnerCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerOfficeId",
                schema: "public",
                table: "Partners",
                column: "PartnerOfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_OrganizationUnits_PartnerOfficeId",
                schema: "public",
                table: "Partners",
                column: "PartnerOfficeId",
                principalSchema: "public",
                principalTable: "OrganizationUnits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_PartnerCategories_PartnerCategoryId",
                schema: "public",
                table: "Partners",
                column: "PartnerCategoryId",
                principalSchema: "public",
                principalTable: "PartnerCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_OrganizationUnits_PartnerOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_PartnerCategories_PartnerCategoryId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropTable(
                name: "OrganizationUnits",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PartnerCategories",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerCategoryId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerOfficeId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerCategoryId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerOfficeId",
                schema: "public",
                table: "Partners");
        }
    }
}
