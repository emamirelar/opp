using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CollaboratorExpertiseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollaboratorExpertises",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WorkflowStatus = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CollaboratorExpertises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityCollaboratorExpertises",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OpportunityCollaboratorId = table.Column<int>(type: "integer", nullable: false),
                    CollaboratorExpertiseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityCollaboratorExpertises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityCollaboratorExpertises_CollaboratorExpertises_Co~",
                        column: x => x.CollaboratorExpertiseId,
                        principalSchema: "public",
                        principalTable: "CollaboratorExpertises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityCollaboratorExpertises_OpportunityCollaborators_~",
                        column: x => x.OpportunityCollaboratorId,
                        principalSchema: "public",
                        principalTable: "OpportunityCollaborators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollaboratorExpertises_Code",
                schema: "public",
                table: "CollaboratorExpertises",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaboratorExpertises_CollaboratorExpertiseId",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                column: "CollaboratorExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaboratorExpertises_OpportunityCollaboratorId~",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                columns: new[] { "OpportunityCollaboratorId", "CollaboratorExpertiseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunityCollaboratorExpertises",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CollaboratorExpertises",
                schema: "public");
        }
    }
}
