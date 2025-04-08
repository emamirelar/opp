using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removeContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Birthdate",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Fax",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "OtherPhone",
                schema: "public",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Pronouns",
                schema: "public",
                table: "Contacts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Birthdate",
                schema: "public",
                table: "Contacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPhone",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pronouns",
                schema: "public",
                table: "Contacts",
                type: "text",
                nullable: true);
        }
    }
}
