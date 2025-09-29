using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProjectEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraints first (safe operations)
            migrationBuilder.Sql(@"
                ALTER TABLE IF EXISTS public.""BudgetLines"" 
                DROP CONSTRAINT IF EXISTS ""FK_BudgetLines_Budgets_BudgetId"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE IF EXISTS public.""BudgetLines"" 
                DROP CONSTRAINT IF EXISTS ""FK_BudgetLines_Donors_DonorId"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE IF EXISTS public.""BudgetLines"" 
                DROP CONSTRAINT IF EXISTS ""FK_BudgetLines_WorkPackages_WorkPackageId"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE IF EXISTS public.""Budgets"" 
                DROP CONSTRAINT IF EXISTS ""FK_Budgets_Projects_ProjectId"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE IF EXISTS public.""WorkPackages"" 
                DROP CONSTRAINT IF EXISTS ""FK_WorkPackages_Projects_ProjectId"";
            ");

            // Drop tables in dependency order (safe operations)
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"PartnerProjects\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"BudgetLines\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"Budgets\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"WorkPackages\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"Donors\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS public.\"Projects\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate tables in reverse order (this is for rollback purposes)
            // Note: This is a simplified recreation - you may need to adjust column types and constraints
            // based on your specific database schema
            
            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BaseEngagement = table.Column<string>(type: "text", nullable: true),
                    ProjectNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    BudgetCheckingLevel = table.Column<string>(type: "text", nullable: false),
                    BudgetDuration = table.Column<string>(type: "text", nullable: false),
                    BudgetAmount = table.Column<double>(type: "double precision", nullable: true),
                    ExpenditureAmount = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Donors",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkPackages",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkPackages_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "public",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    BudgetVersion = table.Column<string>(type: "text", nullable: false),
                    BudgetNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "public",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetLines",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BudgetAccount = table.Column<string>(type: "text", nullable: false),
                    BudgetId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    DonorId = table.Column<int>(type: "integer", nullable: false),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NatureOfCost = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WorkPackageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetLines_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "public",
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetLines_Donors_DonorId",
                        column: x => x.DonorId,
                        principalSchema: "public",
                        principalTable: "Donors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetLines_WorkPackages_WorkPackageId",
                        column: x => x.WorkPackageId,
                        principalSchema: "public",
                        principalTable: "WorkPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartnerProjects",
                schema: "public",
                columns: table => new
                {
                    PartnersId = table.Column<int>(type: "integer", nullable: false),
                    ProjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerProjects", x => new { x.PartnersId, x.ProjectsId });
                    table.ForeignKey(
                        name: "FK_PartnerProjects_Partners_PartnersId",
                        column: x => x.PartnersId,
                        principalSchema: "public",
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartnerProjects_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalSchema: "public",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Recreate indexes
            migrationBuilder.CreateIndex(
                name: "IX_BudgetLines_BudgetId",
                schema: "public",
                table: "BudgetLines",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLines_DonorId",
                schema: "public",
                table: "BudgetLines",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLines_WorkPackageId",
                schema: "public",
                table: "BudgetLines",
                column: "WorkPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ProjectId",
                schema: "public",
                table: "Budgets",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerProjects_ProjectsId",
                schema: "public",
                table: "PartnerProjects",
                column: "ProjectsId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPackages_ProjectId",
                schema: "public",
                table: "WorkPackages",
                column: "ProjectId");
        }
    }
}
