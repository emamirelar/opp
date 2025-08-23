using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DueDiligenceExpiringEmailNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailNotificationLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipientUserId = table.Column<int>(type: "integer", nullable: true),
                    RecipientEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EmailSubject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NotificationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RelatedEntityId = table.Column<int>(type: "integer", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedEntityName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    NotificationData = table.Column<string>(type: "text", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailNotificationLogs_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalSchema: "public",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerFocalPointUserId",
                schema: "public",
                table: "Partners",
                column: "PartnerFocalPointUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_RecipientUserId",
                schema: "public",
                table: "EmailNotificationLogs",
                column: "RecipientUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_AspNetUsers_PartnerFocalPointUserId",
                schema: "public",
                table: "Partners",
                column: "PartnerFocalPointUserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_AspNetUsers_PartnerFocalPointUserId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropTable(
                name: "EmailNotificationLogs",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerFocalPointUserId",
                schema: "public",
                table: "Partners");
        }
    }
}
