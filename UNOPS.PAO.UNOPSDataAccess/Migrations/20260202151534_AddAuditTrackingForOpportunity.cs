using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrackingForOpportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityUNOPSMissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityUNCFOutcomes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityUNCFIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityStakeholders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunitySDGTargets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunitySDGs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunitySDGs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunitySDGs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunitySDGs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunitySDGs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunitySDGs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunitySDGIndicators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityInteractions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityInteractions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityInteractions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityInteractions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityInteractions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityInteractions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityInteractions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityInteractions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityInteractions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityInteractions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityFundingPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityExternalStakeholder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityDeliverables",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityCountries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityCountries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityCountries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityCountries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityCountries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityCountries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityCollaborators",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Truncate OpportunityCollaboratorExpertises to avoid migration conflicts
            migrationBuilder.Sql(@"TRUNCATE TABLE public.""OpportunityCollaboratorExpertises"" CASCADE;");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OpportunityId",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityClientPartners",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Populate audit fields from parent Opportunity table for all child tables
            migrationBuilder.Sql(@"
                -- OpportunityUNOPSMissions
                UPDATE public.""OpportunityUNOPSMissions"" om
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'Mission-' || om.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE om.""OpportunityId"" = o.""Id"";

                -- OpportunityUNCFOutcomes
                UPDATE public.""OpportunityUNCFOutcomes"" ouo
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'UNCFOutcome-' || ouo.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE ouo.""OpportunityId"" = o.""Id"";

                -- OpportunityUNCFIndicators
                UPDATE public.""OpportunityUNCFIndicators"" oui
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'UNCFIndicator-' || oui.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE oui.""OpportunityId"" = o.""Id"";

                -- OpportunityStakeholders
                UPDATE public.""OpportunityStakeholders"" os
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'Stakeholder-' || os.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE os.""OpportunityId"" = o.""Id"";

                -- OpportunitySDGs
                UPDATE public.""OpportunitySDGs"" osdg
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'SDG-' || osdg.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE osdg.""OpportunityId"" = o.""Id"";

                -- OpportunitySDGTargets
                UPDATE public.""OpportunitySDGTargets"" ost
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'SDGTarget-' || ost.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE ost.""OpportunityId"" = o.""Id"";

                -- OpportunitySDGIndicators
                UPDATE public.""OpportunitySDGIndicators"" osi
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'SDGIndicator-' || osi.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE osi.""OpportunityId"" = o.""Id"";

                -- OpportunityInteractions
                UPDATE public.""OpportunityInteractions"" oi
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'Interaction-' || oi.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE oi.""OpportunityId"" = o.""Id"";

                -- OpportunityFundingPartners
                UPDATE public.""OpportunityFundingPartners"" ofp
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'FundingPartner-' || ofp.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE ofp.""OpportunityId"" = o.""Id"";

                -- Updating OpportunityExternalStakeholder
                UPDATE public.""OpportunityExternalStakeholder"" oes
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'ExternalStakeholder-' || oes.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE oes.""OpportunityId"" = o.""Id"";

                -- OpportunityDeliverables
                UPDATE public.""OpportunityDeliverables"" od
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'Deliverable-' || od.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE od.""OpportunityId"" = o.""Id"";

                -- OpportunityCountries
                UPDATE public.""OpportunityCountries"" oc
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'Country-' || oc.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE oc.""OpportunityId"" = o.""Id"";

                -- OpportunityCollaboratorExpertises
                UPDATE public.""OpportunityCollaboratorExpertises"" oce
                SET 
                    ""OpportunityId"" = oc.""OpportunityId"",
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'CollaboratorExpertise-' || oce.""Id""::text,
                    ""Status"" = 1
                FROM public.""OpportunityCollaborators"" oc
                INNER JOIN public.""Opportunities"" o ON oc.""OpportunityId"" = o.""Id""
                WHERE oce.""OpportunityCollaboratorId"" = oc.""Id"";

                -- OpportunityCollaborators
                UPDATE public.""OpportunityCollaborators"" oc
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'Collaborator-' || oc.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE oc.""OpportunityId"" = o.""Id"";

                -- OpportunityClientPartners
                UPDATE public.""OpportunityClientPartners"" ocp
                SET 
                    ""CreatedBy"" = o.""CreatedBy"",
                    ""CreatedDate"" = o.""CreatedDate"",
                    ""LastModifiedBy"" = COALESCE(o.""LastModifiedBy"", o.""CreatedBy""),
                    ""LastModifiedDate"" = COALESCE(o.""LastModifiedDate"", o.""CreatedDate""),
                    ""Name"" = 'ClientPartner-' || ocp.""Id""::text,
                    ""Status"" = 1
                FROM public.""Opportunities"" o
                WHERE ocp.""OpportunityId"" = o.""Id"";
            ");

            // Create index for OpportunityCollaboratorExpertises.OpportunityId
            migrationBuilder.CreateIndex(
                name: "IX_OpportunityCollaboratorExpertises_OpportunityId",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                column: "OpportunityId");

            // Add foreign key for OpportunityCollaboratorExpertises.OpportunityId
            migrationBuilder.AddForeignKey(
                name: "FK_OpportunityCollaboratorExpertises_Opportunities_Opportunity~",
                schema: "public",
                table: "OpportunityCollaboratorExpertises",
                column: "OpportunityId",
                principalSchema: "public",
                principalTable: "Opportunities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key for OpportunityCollaboratorExpertises.OpportunityId
            migrationBuilder.DropForeignKey(
                name: "FK_OpportunityCollaboratorExpertises_Opportunities_Opportunity~",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            // Drop index for OpportunityCollaboratorExpertises.OpportunityId
            migrationBuilder.DropIndex(
                name: "IX_OpportunityCollaboratorExpertises_OpportunityId",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityUNOPSMissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityUNCFOutcomes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityUNCFIndicators");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityStakeholders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunitySDGTargets");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunitySDGs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunitySDGIndicators");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityInteractions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityFundingPartners");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityExternalStakeholder");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityDeliverables");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityCountries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityCollaborators");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "OpportunityId",
                schema: "public",
                table: "OpportunityCollaboratorExpertises");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "public",
                table: "OpportunityClientPartners");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                schema: "public",
                table: "OpportunityClientPartners");
        }
    }
}
