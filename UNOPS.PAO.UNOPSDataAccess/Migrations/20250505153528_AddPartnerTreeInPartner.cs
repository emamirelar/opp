using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerTreeInPartner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerTrees",
                schema: "public",
                table: "PartnerTrees");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "public",
                table: "PartnerTrees",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "PartnerTreeCode",
                schema: "public",
                table: "Partners",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerTrees",
                schema: "public",
                table: "PartnerTrees",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerTreeCode",
                schema: "public",
                table: "Partners",
                column: "PartnerTreeCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerTreeCode",
                schema: "public",
                table: "Partners",
                column: "PartnerTreeCode",
                principalSchema: "public",
                principalTable: "PartnerTrees",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerTreeCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerTrees",
                schema: "public",
                table: "PartnerTrees");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerTreeCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerTreeCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "public",
                table: "PartnerTrees",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerTrees",
                schema: "public",
                table: "PartnerTrees",
                column: "Id");
        }
    }
}
