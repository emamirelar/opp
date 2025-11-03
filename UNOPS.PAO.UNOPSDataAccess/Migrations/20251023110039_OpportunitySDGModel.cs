using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class OpportunitySDGModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpportunitySDGs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityId = table.Column<int>(type: "integer", nullable: false),
                    SDGId = table.Column<int>(type: "integer", nullable: false),
                    AlignmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AlignmentNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ContributionLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_OpportunitySDGs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunitySDGs_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalSchema: "public",
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunitySDGs_SDGs_SDGId",
                        column: x => x.SDGId,
                        principalSchema: "public",
                        principalTable: "SDGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGs_AlignmentType",
                schema: "public",
                table: "OpportunitySDGs",
                column: "AlignmentType");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGs_OpportunityId",
                schema: "public",
                table: "OpportunitySDGs",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunitySDGs_SDGId",
                schema: "public",
                table: "OpportunitySDGs",
                column: "SDGId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunitySDGs",
                schema: "public");
        }
    }
}
