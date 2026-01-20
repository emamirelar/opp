using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Domain.Enums;

namespace UNOPS.PAO.Business.Workflow.Seeders;

/// <summary>
/// Seeds workflow stage transitions for Opportunity entities.
/// Defines 3 transitions: IDENTIFY & PROFILE → GO, IDENTIFY & PROFILE → NO GO, NO GO → IDENTIFY & PROFILE.
/// </summary>
public static class StateMachineStageChangeSeeder
{
    /// <summary>
    /// Seeds stage change transitions for Opportunity workflow.
    /// This method is idempotent - safe to run multiple times.
    /// </summary>
    public static async Task SeedStateMachineStageChangesAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WorkflowDbContext>>();

        try
        {
            var seedData = GetSeedStageChanges();

            foreach (var stageChange in seedData)
            {
                // Check if transition already exists
                var existing = await workflowContext.StateMachineStageChanges
                    .FirstOrDefaultAsync(x =>
                        x.EntityName == stageChange.EntityName &&
                        x.FromStage == stageChange.FromStage &&
                        x.ToStage == stageChange.ToStage);

                if (existing == null)
                {
                    // Create new transition
                    workflowContext.StateMachineStageChanges.Add(stageChange);
                    logger.LogInformation(
                        "Creating workflow transition: {EntityName} {FromStage} → {ToStage}",
                        stageChange.EntityName, stageChange.FromStage, stageChange.ToStage);
                }
                else
                {
                    // Update existing transition if needed
                    var needsUpdate = false;

                    if (existing.IsDeleted)
                    {
                        existing.IsDeleted = false;
                        existing.DeletedBy = 0;
                        existing.DeletedDate = null;
                        needsUpdate = true;
                    }

                    if (existing.ApprovalRequired != stageChange.ApprovalRequired ||
                        existing.CommentRequired != stageChange.CommentRequired ||
                        existing.CommentOptional != stageChange.CommentOptional ||
                        existing.Sequence != stageChange.Sequence ||
                        existing.Name != stageChange.Name ||
                        existing.Status != stageChange.Status)
                    {
                        existing.ApprovalRequired = stageChange.ApprovalRequired;
                        existing.CommentRequired = stageChange.CommentRequired;
                        existing.CommentOptional = stageChange.CommentOptional;
                        existing.Sequence = stageChange.Sequence;
                        existing.Name = stageChange.Name;
                        existing.Status = stageChange.Status;
                        needsUpdate = true;
                    }

                    if (needsUpdate)
                    {
                        logger.LogInformation(
                            "Updating workflow transition: {EntityName} {FromStage} → {ToStage}",
                            existing.EntityName, existing.FromStage, existing.ToStage);
                    }
                }
            }

            await workflowContext.SaveChangesAsync();
            logger.LogInformation("Workflow stage change seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding workflow stage changes");
            throw;
        }
    }

    /// <summary>
    /// Returns the seed data for Opportunity workflow transitions.
    /// </summary>
    private static List<StateMachineStageChange> GetSeedStageChanges()
    {
        return new List<StateMachineStageChange>
        {
            // Transition 1: IDENTIFY & PROFILE → GO (requires approval)
            new StateMachineStageChange
            {
                EntityName = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.Go,
                Sequence = 1,
                CommentRequired = true,
                CommentOptional = false,
                ApprovalRequired = true, // Requires DOA Holder approval
                Internal = true,
                External = false,
                Name = "Submit for Go",
                Status = EntityStatus.Active
            },

            // Transition 2: IDENTIFY & PROFILE → NO GO (requires approval)
            new StateMachineStageChange
            {
                EntityName = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.NoGo,
                Sequence = 2,
                CommentRequired = true,
                CommentOptional = false,
                ApprovalRequired = true, // Requires DOA Holder approval
                Internal = true,
                External = false,
                Name = "Submit for No Go",
                Status = EntityStatus.Active
            },

            // Transition 3: NO GO → IDENTIFY & PROFILE (reopen, no approval)
            new StateMachineStageChange
            {
                EntityName = OpportunityWorkflow.EntityName,
                FromStage = OpportunityWorkflow.Stages.NoGo,
                ToStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                Sequence = 1,
                CommentRequired = false,
                CommentOptional = true,
                ApprovalRequired = false, // No approval needed for reopen
                Internal = true,
                External = false,
                Name = "Reopen",
                Status = EntityStatus.Active
            }
        };
    }
}
