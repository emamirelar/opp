using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

public class WorkflowStageSeeder
{
    public static async Task SeedWorkflowStagesAsync(UNOPSAppDbContext context)
    {
        // Check if any WorkflowStages exist for Opportunity entity
        var existingStages = await context.WorkflowStages
            .Where(ws => ws.EntityType == "Opportunity")
            .AnyAsync();

        if (existingStages)
        {
            Console.WriteLine("WorkflowStages for Opportunity already exist. Skipping seed.");
            return;
        }

        var workflowStages = new List<WorkflowStage>
        {
            new WorkflowStage
            {
                EntityType = "Opportunity",
                Name = "IDENTIFY & PROFILE",
                Description = "Initial stage for identifying and profiling the opportunity",
                Order = 1,
                AllowsParallelProcessing = false,
                IsFinalStage = false,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            },
            new WorkflowStage
            {
                EntityType = "Opportunity",
                Name = "DECIDE",
                Description = "Decision stage - Go/No-Go determination",
                Order = 2,
                AllowsParallelProcessing = false,
                IsFinalStage = true,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = 1 // System user
            }
        };

        await context.WorkflowStages.AddRangeAsync(workflowStages);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded {workflowStages.Count} WorkflowStages for Opportunity entity.");
    }
}

