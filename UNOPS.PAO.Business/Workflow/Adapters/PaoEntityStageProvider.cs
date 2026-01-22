using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow.Adapters;

/// <summary>
/// PAO implementation of IEntityStageProvider.
/// Provides entity stage information and update capabilities for workflow operations.
/// </summary>
public class PaoEntityStageProvider : IEntityStageProvider
{
    private readonly AppDbContext _context;

    public PaoEntityStageProvider(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the current stage of an entity.
    /// </summary>
    public async Task<string?> GetCurrentStageAsync(string entityName, string entityId)
    {
        if (!int.TryParse(entityId, out var id)) 
            return null;

        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await _context.Opportunities
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

        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await _context.Opportunities
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

        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await _context.Opportunities
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
        var entity = await _context.Opportunities
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        
        if (entity == null) 
            return false;

        entity.Stage = newStage;
        entity.LastModifiedBy = userId;
        entity.LastModifiedDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
