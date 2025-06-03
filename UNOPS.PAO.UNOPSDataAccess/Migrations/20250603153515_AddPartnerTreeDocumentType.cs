using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerTreeDocumentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "DocumentTypes",
                columns: new[] { "EntityType", "Name", "Status", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "IsDeleted", "DeletedBy", "DeletedDate" },
                values: new object[] { "PartnerTree", "Other", 1, 0, DateTime.UtcNow, 0, DateTime.UtcNow, false, 0, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "DocumentTypes",
                keyColumns: new[] { "EntityType", "Name" },
                keyValues: new object[] { "PartnerTree", "Other" });
        }
    }
} 