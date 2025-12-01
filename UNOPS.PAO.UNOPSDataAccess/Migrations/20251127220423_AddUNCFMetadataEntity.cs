using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUNCFMetadataEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UNCFMetadatas",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UNCFMetadataId = table.Column<int>(type: "integer", nullable: true),
                    Country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    UNCFFileURL = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UNCooperationFrameworkVersionNo = table.Column<int>(type: "integer", nullable: true),
                    UNCFLastUpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UNCFFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UNCFMetadatas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UNCFMetadatas_Country",
                schema: "public",
                table: "UNCFMetadatas",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFMetadatas_Country_UNCooperationFrameworkVersionNo",
                schema: "public",
                table: "UNCFMetadatas",
                columns: new[] { "Country", "UNCooperationFrameworkVersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UNCFMetadatas_Status",
                schema: "public",
                table: "UNCFMetadatas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFMetadatas_UNCFLastUpdatedDate",
                schema: "public",
                table: "UNCFMetadatas",
                column: "UNCFLastUpdatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFMetadatas_UNCFMetadataId",
                schema: "public",
                table: "UNCFMetadatas",
                column: "UNCFMetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_UNCFMetadatas_UNCooperationFrameworkVersionNo",
                schema: "public",
                table: "UNCFMetadatas",
                column: "UNCooperationFrameworkVersionNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UNCFMetadatas",
                schema: "public");
        }
    }
}
