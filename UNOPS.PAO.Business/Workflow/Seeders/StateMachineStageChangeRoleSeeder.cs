using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Domain.Enums;

namespace UNOPS.PAO.Business.Workflow.Seeders;

/// <summary>
/// Seeds workflow role permissions for Opportunity workflow transitions.
/// Includes role permissions for all 5 transitions: Go, No Go, Reopen from No Go, Cancel, and Reopen from Cancelled.
/// </summary>
public static class StateMachineStageChangeRoleSeeder
{
    /// <summary>
    /// Role names used in Opportunity workflow.
    /// These must match the EntityRole.Name values in the database.
    /// </summary>
    public static class RoleNames
    {
        public const string OpportunityManager = "Opportunity Manager";
        public const string PartnershipLead = "Partnership Lead"; // Approver role for testing
    }

    /// <summary>
    /// Seeds role permissions for Opportunity workflow transitions.
    /// This method is idempotent - safe to run multiple times.
    /// </summary>
    public static async Task SeedStateMachineStageChangeRolesAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WorkflowDbContext>>();

        try
        {
            // Look up EntityRole IDs from the PAO database
            var entityRoles = await appContext.EntityRoles
                .Where(r => !r.IsDeleted &&
                           r.EntityType == "Opportunity" &&
                           (r.Name == RoleNames.OpportunityManager || r.Name == RoleNames.PartnershipLead))
                .ToListAsync();

            var opportunityManagerRole = entityRoles.FirstOrDefault(r => r.Name == RoleNames.OpportunityManager);
            var partnershipLeadRole = entityRoles.FirstOrDefault(r => r.Name == RoleNames.PartnershipLead);

            if (opportunityManagerRole == null)
            {
                logger.LogWarning("EntityRole '{RoleName}' not found in database. Skipping role seeding.", RoleNames.OpportunityManager);
            }

            if (partnershipLeadRole == null)
            {
                logger.LogWarning("EntityRole '{RoleName}' not found in database. Skipping role seeding.", RoleNames.PartnershipLead);
            }

            var seedData = GetSeedStageChangeRoles(
                opportunityManagerRole?.Id ?? 0,
                partnershipLeadRole?.Id ?? 0);

            foreach (var rolePermission in seedData)
            {
                // Skip if role ID is 0 (role not found)
                if (rolePermission.RoleId == 0)
                {
                    logger.LogWarning(
                        "Skipping role permission for {RoleName} on transition {FromStage} → {ToStage} (role not found)",
                        rolePermission.RoleName, rolePermission.FromStage, rolePermission.ToStage);
                    continue;
                }

                // Check if role permission already exists
                var existing = await workflowContext.StateMachineStageChangeRoles
                    .FirstOrDefaultAsync(x =>
                        x.EntityType == rolePermission.EntityType &&
                        x.FromStage == rolePermission.FromStage &&
                        x.ToStage == rolePermission.ToStage &&
                        x.RoleId == rolePermission.RoleId);

                if (existing == null)
                {
                    // Create new role permission
                    workflowContext.StateMachineStageChangeRoles.Add(rolePermission);
                    logger.LogInformation(
                        "Creating workflow role permission: {EntityType} {FromStage} → {ToStage}, Role={RoleName}, CanTrigger={CanTrigger}, CanApprove={CanApprove}",
                        rolePermission.EntityType, rolePermission.FromStage, rolePermission.ToStage,
                        rolePermission.RoleName, rolePermission.CanTrigger, rolePermission.CanApprove);
                }
                else
                {
                    // Update existing role permission if needed
                    var needsUpdate = false;

                    if (existing.IsDeleted)
                    {
                        existing.IsDeleted = false;
                        existing.DeletedBy = 0;
                        existing.DeletedDate = null;
                        needsUpdate = true;
                    }

                    if (existing.CanTrigger != rolePermission.CanTrigger ||
                        existing.CanApprove != rolePermission.CanApprove ||
                        existing.RoleName != rolePermission.RoleName ||
                        existing.Status != rolePermission.Status)
                    {
                        existing.CanTrigger = rolePermission.CanTrigger;
                        existing.CanApprove = rolePermission.CanApprove;
                        existing.RoleName = rolePermission.RoleName;
                        existing.Status = rolePermission.Status;
                        needsUpdate = true;
                    }

                    if (needsUpdate)
                    {
                        logger.LogInformation(
                            "Updating workflow role permission: {EntityType} {FromStage} → {ToStage}, Role={RoleName}",
                            existing.EntityType, existing.FromStage, existing.ToStage, existing.RoleName);
                    }
                }
            }

            await workflowContext.SaveChangesAsync();
            logger.LogInformation("Workflow stage change role seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding workflow stage change roles");
            throw;
        }
    }

    /// <summary>
    /// Returns the seed data for Opportunity workflow role permissions.
    /// Includes permissions for all 5 transitions: Go, No Go, Reopen from No Go, Cancel, and Reopen from Cancelled.
    /// </summary>
    private static List<StateMachineStageChangeRole> GetSeedStageChangeRoles(
        int opportunityManagerRoleId,
        int partnershipLeadRoleId)
    {
        return new List<StateMachineStageChangeRole>
        {
            // ========================================
            // Transition: IDENTIFY & PROFILE → GO
            // ========================================

            // Opportunity Manager can trigger (submit for approval)
            new StateMachineStageChangeRole
            {
                EntityType = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.Go,
                RoleId = opportunityManagerRoleId,
                RoleName = RoleNames.OpportunityManager,
                CanTrigger = true,
                CanApprove = false,
                Name = "Opportunity Manager - Submit for Go",
                Status = EntityStatus.Active
            },

            // Partnership Lead can approve
            new StateMachineStageChangeRole
            {
                EntityType = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.Go,
                RoleId = partnershipLeadRoleId,
                RoleName = RoleNames.PartnershipLead,
                CanTrigger = false,
                CanApprove = true,
                Name = "Partnership Lead - Approve Go",
                Status = EntityStatus.Active
            },

            // ========================================
            // Transition: IDENTIFY & PROFILE → NO GO
            // ========================================

            // Opportunity Manager can trigger (submit for approval)
            new StateMachineStageChangeRole
            {
                EntityType = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.NoGo,
                RoleId = opportunityManagerRoleId,
                RoleName = RoleNames.OpportunityManager,
                CanTrigger = true,
                CanApprove = false,
                Name = "Opportunity Manager - Submit for No Go",
                Status = EntityStatus.Active
            },

            // Partnership Lead can approve
            new StateMachineStageChangeRole
            {
                EntityType = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.NoGo,
                RoleId = partnershipLeadRoleId,
                RoleName = RoleNames.PartnershipLead,
                CanTrigger = false,
                CanApprove = true,
                Name = "Partnership Lead - Approve No Go",
                Status = EntityStatus.Active
            },

            // ========================================
            // Transition: NO GO → IDENTIFY & PROFILE (Reopen from No Go)
            // ========================================

            // Opportunity Manager can trigger (reopen, no approval needed)
            new StateMachineStageChangeRole
            {
                EntityType = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.NoGo,
                ToStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                RoleId = opportunityManagerRoleId,
                RoleName = RoleNames.OpportunityManager,
                CanTrigger = true,
                CanApprove = false, // No approval needed for reopen
                Name = "Opportunity Manager - Reopen from No Go",
                Status = EntityStatus.Active
            },

            // ========================================
            // Transition: IDENTIFY & PROFILE → CANCELLED (Cancel)
            // ========================================

            // Opportunity Manager can trigger (cancel, no approval needed)
            new StateMachineStageChangeRole
            {
                EntityType = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.Cancelled,
                RoleId = opportunityManagerRoleId,
                RoleName = RoleNames.OpportunityManager,
                CanTrigger = true,
                CanApprove = false, // No approval needed for cancel
                Name = "Opportunity Manager - Cancel",
                Status = EntityStatus.Active
            },

            // ========================================
            // Transition: CANCELLED → IDENTIFY & PROFILE (Reopen from Cancelled)
            // ========================================

            // Opportunity Manager can trigger (reopen from cancelled, no approval needed)
            new StateMachineStageChangeRole
            {
                EntityType = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.Cancelled,
                ToStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                RoleId = opportunityManagerRoleId,
                RoleName = RoleNames.OpportunityManager,
                CanTrigger = true,
                CanApprove = false, // No approval needed for reopen
                Name = "Opportunity Manager - Reopen from Cancelled",
                Status = EntityStatus.Active
            }
        };
    }
}
