using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPreferences",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DefaultOrgUnitId = table.Column<int>(type: "integer", nullable: true),
                    PreferencesJson = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_OrganizationHierarchies_DefaultOrgUnitId",
                        column: x => x.DefaultOrgUnitId,
                        principalSchema: "public",
                        principalTable: "OrganizationHierarchies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPreferences_UserInfos_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "UserInfos",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_DefaultOrgUnitId",
                schema: "public",
                table: "UserPreferences",
                column: "DefaultOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                schema: "public",
                table: "UserPreferences",
                column: "UserId");
                
            // Migrate existing user preferences based on UserInfo.OrgUnit
            migrationBuilder.Sql(@"
                INSERT INTO ""UserPreferences"" (""UserId"", ""DefaultOrgUnitId"", ""Name"", ""Status"", ""CreatedBy"", ""CreatedDate"", ""LastModifiedBy"", ""IsDeleted"", ""DeletedBy"")
                SELECT 
                    ui.""UserId"",
                    oh.""Id"" as ""DefaultOrgUnitId"",
                    'Auto-generated' as ""Name"",
                    1 as ""Status"",
                    ui.""UserId"" as ""CreatedBy"",
                    CURRENT_TIMESTAMP AT TIME ZONE 'UTC' as ""CreatedDate"",
                    ui.""UserId"" as ""LastModifiedBy"",
                    false as ""IsDeleted"",
                    0 as ""DeletedBy""
                FROM ""UserInfos"" ui
                INNER JOIN ""OrganizationHierarchies"" oh ON oh.""Code"" = ui.""OrgUnit""
                WHERE ui.""OrgUnit"" IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM ""UserPreferences"" up WHERE up.""UserId"" = ui.""UserId"")
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPreferences",
                schema: "public");
        }
    }
}
