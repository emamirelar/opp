using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow.Adapters;

/// <summary>
/// PAO implementation of IEntityStageProvider.
/// Provides entity stage information and update capabilities for workflow operations.
/// Uses DbContextFactory to create separate context instances for each operation,
/// avoiding DbContext concurrency issues with other async workflow operations.
/// </summary>
public class PaoEntityStageProvider : IEntityStageProvider
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PaoEntityStageProvider(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Gets the current stage of an entity.
    /// </summary>
    public async Task<string?> GetCurrentStageAsync(string entityName, string entityId)
    {
        if (!int.TryParse(entityId, out var id)) 
            return null;

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await context.Opportunities
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => x.Stage)
                .FirstOrDefaultAsync(),
            _ => null
        };
    }

    /// <summary>
    /// Updates the stage of an entity after a workflow transition.
    /// </summary>
    public async Task<bool> UpdateStageAsync(string entityName, string entityId, string newStage, int userId)
    {
        if (!int.TryParse(entityId, out var id)) 
            return false;

        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await UpdateOpportunityStageAsync(id, newStage, userId),
            _ => false
        };
    }

    /// <summary>
    /// Checks if an entity exists and is eligible for workflow operations.
    /// </summary>
    public async Task<bool> IsEntityValidAsync(string entityName, string entityId)
    {
        if (!int.TryParse(entityId, out var id)) 
            return false;

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await context.Opportunities
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDeleted),
            _ => false
        };
    }

    /// <summary>
    /// Gets the display name of an entity for use in notifications.
    /// </summary>
    public async Task<string> GetEntityDisplayNameAsync(string entityName, string entityId)
    {
        if (!int.TryParse(entityId, out var id)) 
            return "Unknown";

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await context.Opportunities
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => x.Name)
                .FirstOrDefaultAsync() ?? "Unknown Opportunity",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Updates the stage of an Opportunity entity.
    /// </summary>
    private async Task<bool> UpdateOpportunityStageAsync(int id, string newStage, int userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var entity = await context.Opportunities
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        
        if (entity == null) 
            return false;

        entity.Stage = newStage;
        entity.LastModifiedBy = userId;
        entity.LastModifiedDate = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
        return true;
    }
}
