using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePartnerGroupRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerGroupCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerTrees",
                schema: "public",
                table: "PartnerTrees");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerGroupCode",
                schema: "public",
                table: "Partners");

            migrationBuilder.AddColumn<int>(
                name: "PartnerGroupId",
                schema: "public",
                table: "Partners",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerTrees",
                schema: "public",
                table: "PartnerTrees",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerGroupId",
                schema: "public",
                table: "Partners",
                column: "PartnerGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerGroupId",
                schema: "public",
                table: "Partners",
                column: "PartnerGroupId",
                principalSchema: "public",
                principalTable: "PartnerTrees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_AspNetUsers_PartnerFocalPointUserId",
                schema: "public",
                table: "Partners");
    
            migrationBuilder.AddForeignKey(
                name: "FK_Partners_AspNetUsers_PartnerFocalPointUserId",
                schema: "public",
                table: "Partners",
                column: "PartnerFocalPointUserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerGroupId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerTrees",
                schema: "public",
                table: "PartnerTrees");

            migrationBuilder.DropIndex(
                name: "IX_Partners_PartnerGroupId",
                schema: "public",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "PartnerGroupId",
                schema: "public",
                table: "Partners");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerTrees",
                schema: "public",
                table: "PartnerTrees",
                column: "Code");
            

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerGroupCode",
                schema: "public",
                table: "Partners",
                column: "PartnerGroupCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Engagements_Partners_PartnerId",
                schema: "public",
                table: "Engagements",
                column: "PartnerId",
                principalSchema: "public",
                principalTable: "Partners",
                principalColumn: "ErpDimValue",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_PartnerTrees_PartnerGroupCode",
                schema: "public",
                table: "Partners",
                column: "PartnerGroupCode",
                principalSchema: "public",
                principalTable: "PartnerTrees",
                principalColumn: "Code");
            
            migrationBuilder.DropForeignKey(
                   name: "FK_Partners_AspNetUsers_PartnerFocalPointUserId",
                   schema: "public",
                   table: "Partners");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_AspNetUsers_PartnerFocalPointUserId",
                schema: "public",
                table: "Partners",
                column: "PartnerFocalPointUserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
