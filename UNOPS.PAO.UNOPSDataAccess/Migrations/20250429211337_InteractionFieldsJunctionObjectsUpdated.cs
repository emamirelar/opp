using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InteractionFieldsJunctionObjectsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InteractionUsers",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropIndex(
                name: "IX_InteractionUsers_InteractionId",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InteractionPartners",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropIndex(
                name: "IX_InteractionPartners_InteractionId",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InteractionContacts",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropIndex(
                name: "IX_InteractionContacts_InteractionId",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InteractionUsers",
                schema: "public",
                table: "InteractionUsers",
                columns: new[] { "InteractionId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_InteractionPartners",
                schema: "public",
                table: "InteractionPartners",
                columns: new[] { "InteractionId", "PartnerId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_InteractionContacts",
                schema: "public",
                table: "InteractionContacts",
                columns: new[] { "InteractionId", "ContactId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InteractionUsers",
                schema: "public",
                table: "InteractionUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InteractionPartners",
                schema: "public",
                table: "InteractionPartners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InteractionContacts",
                schema: "public",
                table: "InteractionContacts");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "public",
                table: "InteractionUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "InteractionUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "InteractionUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "InteractionUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "InteractionUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "InteractionUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "InteractionUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "InteractionUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "InteractionUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "InteractionUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "public",
                table: "InteractionPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "InteractionPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "InteractionPartners",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "InteractionPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "InteractionPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "InteractionPartners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "InteractionPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "InteractionPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "InteractionPartners",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "InteractionPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "public",
                table: "InteractionContacts",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "InteractionContacts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "InteractionContacts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "InteractionContacts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "InteractionContacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "InteractionContacts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "InteractionContacts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "InteractionContacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "InteractionContacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "InteractionContacts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InteractionUsers",
                schema: "public",
                table: "InteractionUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InteractionPartners",
                schema: "public",
                table: "InteractionPartners",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InteractionContacts",
                schema: "public",
                table: "InteractionContacts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionUsers_InteractionId",
                schema: "public",
                table: "InteractionUsers",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionPartners_InteractionId",
                schema: "public",
                table: "InteractionPartners",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionContacts_InteractionId",
                schema: "public",
                table: "InteractionContacts",
                column: "InteractionId");
        }
    }
}
