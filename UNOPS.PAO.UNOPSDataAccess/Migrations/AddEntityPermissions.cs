using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    public partial class AddEntityPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntityPermissions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    PropertyName = table.Column<string>(type: "text", nullable: true),
                    FilterExpression = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityPermissions", x => x.Id);
                });

            // Add test permissions for dummy data
            migrationBuilder.InsertData(
                schema: "public",
                table: "EntityPermissions",
                columns: new[] { "EntityName", "Action", "RoleName", "PropertyName", "FilterExpression" },
                values: new object[,]
                {
                    // Administrator role permissions (full access to all entities)
                    { "Partner", "Read", "Administrator", null, null },
                    { "Partner", "Create", "Administrator", null, null },
                    { "Partner", "Update", "Administrator", null, null },
                    { "Partner", "Delete", "Administrator", null, null },
                    
                    { "Contact", "Read", "Administrator", null, null },
                    { "Contact", "Create", "Administrator", null, null },
                    { "Contact", "Update", "Administrator", null, null },
                    { "Contact", "Delete", "Administrator", null, null },
                    
                    { "Document", "Read", "Administrator", null, null },
                    { "Document", "Create", "Administrator", null, null },
                    { "Document", "Update", "Administrator", null, null },
                    { "Document", "Delete", "Administrator", null, null },
                    
                    { "Project", "Read", "Administrator", null, null },
                    { "Project", "Create", "Administrator", null, null },
                    { "Project", "Update", "Administrator", null, null },
                    { "Project", "Delete", "Administrator", null, null },
                    
                    // Internal role permissions
                    { "Partner", "Read", "Internal", null, null },
                    { "Partner", "Create", "Internal", null, null },
                    { "Partner", "Update", "Internal", null, null },
                    
                    { "Contact", "Read", "Internal", null, null },
                    { "Contact", "Create", "Internal", null, null },
                    { "Contact", "Update", "Internal", null, null },
                    
                    { "Document", "Read", "Internal", null, null },
                    { "Document", "Create", "Internal", null, null },
                    { "Document", "Update", "Internal", null, null },
                    
                    { "Project", "Read", "Internal", null, null },
                    { "Project", "Create", "Internal", null, null },
                    { "Project", "Update", "Internal", null, null },
                    
                    // Partner role permissions (can only read most things, manage their own content)
                    { "Partner", "Read", "Partner", null, "CreatedBy == CurrentUser" },
                    { "Contact", "Read", "Partner", null, "CreatedBy == CurrentUser" },
                    { "Contact", "Create", "Partner", null, null },
                    { "Contact", "Update", "Partner", null, "CreatedBy == CurrentUser" },
                    
                    { "Document", "Read", "Partner", null, "CreatedBy == CurrentUser" },
                    { "Document", "Create", "Partner", null, null },
                    { "Document", "Update", "Partner", null, "CreatedBy == CurrentUser" },
                    
                    { "Project", "Read", "Partner", null, "CreatedBy == CurrentUser" },
                    
                    // External role permissions (limited access)
                    { "Partner", "Read", "External", null, "IsPublic == true" },
                    { "Contact", "Read", "External", null, "IsPublic == true" },
                    { "Document", "Read", "External", null, "IsPublic == true" },
                    { "Project", "Read", "External", null, "IsPublic == true" }
                });

            // Create index for faster lookups
            migrationBuilder.CreateIndex(
                name: "IX_EntityPermissions_EntityName_Action_RoleName",
                schema: "public",
                table: "EntityPermissions",
                columns: new[] { "EntityName", "Action", "RoleName" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityPermissions",
                schema: "public");
        }
    }
} 