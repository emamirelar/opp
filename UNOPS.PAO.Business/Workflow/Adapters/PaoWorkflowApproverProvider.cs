using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Models;

namespace UNOPS.PAO.Business.Workflow.Adapters;

/// <summary>
/// PAO implementation of IWorkflowApproverProvider.
/// Provides entity-specific workflow approvers based on stakeholders and entity roles.
/// </summary>
public class PaoWorkflowApproverProvider : IPaoWorkflowApproverProvider
{
    private readonly AppDbContext _context;
    private readonly WorkflowDbContext _workflowContext;

    public PaoWorkflowApproverProvider(
        AppDbContext context,
        WorkflowDbContext workflowContext)
    {
        _context = context;
        _workflowContext = workflowContext;
    }

    /// <summary>
    /// Gets the list of users who can approve a workflow transition.
    /// </summary>
    public async Task<List<WorkflowApproverModel>> GetApproversAsync(
        string entityName, 
        int entityId, 
        string fromStage, 
        string toStage)
    {
        var approvers = new List<WorkflowApproverModel>();

        // Get roles that can approve this transition
        var stageChangeRoles = await GetStageChangeRolesAsync(entityName, fromStage, toStage, canApprove: true);

        if (!stageChangeRoles.Any())
            return approvers;

        var roleNames = stageChangeRoles.Select(r => r.RoleName).ToList();

        // For Opportunity entities, find stakeholders with the required roles
        if (entityName.Equals("opportunity", StringComparison.OrdinalIgnoreCase))
        {
            approvers = await GetOpportunityApproversAsync(entityId, roleNames, toStage);
        }

        return approvers;
    }

    /// <summary>
    /// Gets workflow approval tasks and required roles for a stage transition.
    /// </summary>
    public async Task<(List<WorkflowTaskModel> approvals, string[] roles)?> GetApprovalConfigurationAsync(
        string entityName, 
        int entityId, 
        string fromStage, 
        string toStage)
    {
        var stageChangeRoles = await GetStageChangeRolesAsync(entityName, fromStage, toStage, canApprove: true);

        if (!stageChangeRoles.Any())
            return null;

        var roles = stageChangeRoles.Select(r => r.RoleName).Distinct().ToArray();
        var roleNames = stageChangeRoles.Select(r => r.RoleName).ToList();

        // Get approvers for the entity
        var tasks = new List<WorkflowTaskModel>();

        if (entityName.Equals("opportunity", StringComparison.OrdinalIgnoreCase))
        {
            var approvers = await GetOpportunityApproverTasksAsync(entityId, roleNames);
            tasks.AddRange(approvers);
        }

        return (tasks, roles);
    }

    /// <summary>
    /// Gets workflow trigger users and required roles for a stage transition.
    /// </summary>
    public async Task<(List<WorkflowTaskModel> triggers, string[] roles)?> GetTriggerConfigurationAsync(
        string entityName, 
        int entityId, 
        string fromStage, 
        string toStage)
    {
        var stageChangeRoles = await GetStageChangeRolesAsync(entityName, fromStage, toStage, canTrigger: true);

        if (!stageChangeRoles.Any())
            return null;

        var roles = stageChangeRoles.Select(r => r.RoleName).Distinct().ToArray();
        var roleNames = stageChangeRoles.Select(r => r.RoleName).ToList();

        // Get triggers for the entity
        var tasks = new List<WorkflowTaskModel>();

        if (entityName.Equals("opportunity", StringComparison.OrdinalIgnoreCase))
        {
            var triggers = await GetOpportunityTriggerTasksAsync(entityId, roleNames);
            tasks.AddRange(triggers);
        }

        return (tasks, roles);
    }

    /// <summary>
    /// Checks if a user can approve a workflow transition.
    /// </summary>
    public async Task<bool> CanUserApproveAsync(
        string entityName, 
        int entityId, 
        int userId, 
        string fromStage, 
        string toStage)
    {
        var approvers = await GetApproversAsync(entityName, entityId, fromStage, toStage);
        return approvers.Any(a => a.UserId == userId);
    }

    /// <summary>
    /// Gets stage change roles that match the criteria.
    /// </summary>
    private async Task<List<StageChangeRoleInfo>> GetStageChangeRolesAsync(
        string entityName, 
        string fromStage, 
        string toStage,
        bool? canApprove = null,
        bool? canTrigger = null)
    {
        var query = _workflowContext.StateMachineStageChangeRoles
            .Where(x => !x.IsDeleted &&
                        x.EntityType.ToLower() == entityName.ToLower() &&
                        x.FromStage == fromStage && 
                        x.ToStage == toStage);

        if (canApprove.HasValue)
            query = query.Where(x => x.CanApprove == canApprove.Value);

        if (canTrigger.HasValue)
            query = query.Where(x => x.CanTrigger == canTrigger.Value);

        return await query
            .Select(x => new StageChangeRoleInfo
            {
                RoleId = x.RoleId,
                RoleName = x.RoleName ?? string.Empty,
                CanApprove = x.CanApprove,
                CanTrigger = x.CanTrigger
            })
            .ToListAsync();
    }

    /// <summary>
    /// Gets approvers for an Opportunity based on stakeholders with the required roles.
    /// </summary>
    private async Task<List<WorkflowApproverModel>> GetOpportunityApproversAsync(
        int opportunityId, 
        List<string> roleNames,
        string toStage)
    {
        // Get stakeholders with the required entity roles
        var stakeholders = await _context.Set<OpportunityStakeholder>()
            .Include(s => s.EntityRole)
            .Include(s => s.User)
                .ThenInclude(u => u!.UserProfile)
            .Where(s => s.OpportunityId == opportunityId &&
                       s.UserId.HasValue &&
                       s.EntityRole != null &&
                       roleNames.Contains(s.EntityRole.Name))
            .ToListAsync();

        return stakeholders
            .Where(s => s.User != null)
            .Select(s => new WorkflowApproverModel
            {
                UserId = s.UserId!.Value,
                FirstName = s.User!.UserProfile?.Name?.Split(' ').FirstOrDefault() ?? string.Empty,
                LastName = s.User!.UserProfile?.Name?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                Name = s.User!.UserProfile?.Name ?? s.User.Email,
                Email = s.User!.Email ?? string.Empty,
                Role = s.EntityRole!.Name,
                ToStage = toStage
            })
            .ToList();
    }

    /// <summary>
    /// Gets approval tasks for an Opportunity.
    /// </summary>
    private async Task<List<WorkflowTaskModel>> GetOpportunityApproverTasksAsync(
        int opportunityId, 
        List<string> roleNames)
    {
        var stakeholders = await _context.Set<OpportunityStakeholder>()
            .Include(s => s.EntityRole)
            .Where(s => s.OpportunityId == opportunityId &&
                       s.UserId.HasValue &&
                       s.EntityRole != null &&
                       roleNames.Contains(s.EntityRole.Name))
            .ToListAsync();

        return stakeholders
            .Select(s => new WorkflowTaskModel
            {
                UserId = s.UserId!.Value,
                Role = s.EntityRole!.Name
            })
            .ToList();
    }

    /// <summary>
    /// Gets trigger tasks for an Opportunity.
    /// </summary>
    private async Task<List<WorkflowTaskModel>> GetOpportunityTriggerTasksAsync(
        int opportunityId, 
        List<string> roleNames)
    {
        var stakeholders = await _context.Set<OpportunityStakeholder>()
            .Include(s => s.EntityRole)
            .Where(s => s.OpportunityId == opportunityId &&
                       s.UserId.HasValue &&
                       s.EntityRole != null &&
                       roleNames.Contains(s.EntityRole.Name))
            .ToListAsync();

        return stakeholders
            .Select(s => new WorkflowTaskModel
            {
                UserId = s.UserId!.Value,
                Role = s.EntityRole!.Name
            })
            .ToList();
    }

    /// <summary>
    /// Internal record for stage change role information.
    /// </summary>
    private record StageChangeRoleInfo
    {
        public int RoleId { get; init; }
        public string RoleName { get; init; } = string.Empty;
        public bool CanApprove { get; init; }
        public bool CanTrigger { get; init; }
    }
}
