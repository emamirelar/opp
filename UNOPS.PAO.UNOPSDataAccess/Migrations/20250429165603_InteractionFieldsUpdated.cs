using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InteractionFieldsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailAddresses",
                schema: "public",
                table: "Interactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "public",
                table: "Interactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrgUnitId",
                schema: "public",
                table: "Interactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumbers",
                schema: "public",
                table: "Interactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                schema: "public",
                table: "Interactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InteractionId",
                schema: "public",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InteractionContacts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InteractionId = table.Column<int>(type: "integer", nullable: false),
                    ContactId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_InteractionContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionContacts_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "public",
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractionContacts_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalSchema: "public",
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractionPartners",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InteractionId = table.Column<int>(type: "integer", nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_InteractionPartners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionPartners_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalSchema: "public",
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractionPartners_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalSchema: "public",
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractionUsers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InteractionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_InteractionUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InteractionUsers_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalSchema: "public",
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_OrgUnitId",
                schema: "public",
                table: "Interactions",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_InteractionId",
                schema: "public",
                table: "Documents",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionContacts_ContactId",
                schema: "public",
                table: "InteractionContacts",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionContacts_InteractionId",
                schema: "public",
                table: "InteractionContacts",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionPartners_InteractionId",
                schema: "public",
                table: "InteractionPartners",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionPartners_PartnerId",
                schema: "public",
                table: "InteractionPartners",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionUsers_InteractionId",
                schema: "public",
                table: "InteractionUsers",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionUsers_UserId",
                schema: "public",
                table: "InteractionUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Interactions_InteractionId",
                schema: "public",
                table: "Documents",
                column: "InteractionId",
                principalSchema: "public",
                principalTable: "Interactions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Interactions_OrganizationUnits_OrgUnitId",
                schema: "public",
                table: "Interactions",
                column: "OrgUnitId",
                principalSchema: "public",
                principalTable: "OrganizationUnits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Interactions_InteractionId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Interactions_OrganizationUnits_OrgUnitId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropTable(
                name: "InteractionContacts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InteractionPartners",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InteractionUsers",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Interactions_OrgUnitId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropIndex(
                name: "IX_Documents_InteractionId",
                schema: "public",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EmailAddresses",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "OrgUnitId",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "PhoneNumbers",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "Subject",
                schema: "public",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "InteractionId",
                schema: "public",
                table: "Documents");
        }
    }
}
