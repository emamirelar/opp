using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Models;

namespace UNOPS.PAO.Business.Workflow.Adapters;

/// <summary>
/// PAO implementation of IWorkflowApproverProvider.
/// Provides entity-specific workflow approvers based on stakeholders and entity roles.
/// For GO transition: Uses DoA Level 2 holders from the opportunity's ResponsibleOrgUnit.
/// For other transitions: Uses stakeholder-based lookup.
/// Uses DbContextFactory to create separate context instances for each operation,
/// avoiding DbContext concurrency issues with other async workflow operations.
/// </summary>
public class PaoWorkflowApproverProvider : IPaoWorkflowApproverProvider
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly WorkflowDbContext _workflowContext;
    private readonly ILogger<PaoWorkflowApproverProvider>? _logger;

    /// <summary>
    /// DoA Level 2 role code used for approver lookup on OrganizationHierarchy.
    /// </summary>
    private const string DoA2RoleCode = "DoA2_OrganizationHierarchy";

    public PaoWorkflowApproverProvider(
        IDbContextFactory<AppDbContext> contextFactory,
        WorkflowDbContext workflowContext,
        ILogger<PaoWorkflowApproverProvider>? logger = null)
    {
        _contextFactory = contextFactory;
        _workflowContext = workflowContext;
        _logger = logger;
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
            var approvers = await GetOpportunityApproverTasksAsync(entityId, roleNames, toStage);
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
    /// Gets approvers for an Opportunity based on the target stage.
    /// For GO transition: Returns DoA Level 2 holders from the opportunity's ResponsibleOrgUnit.
    /// For other transitions: Returns stakeholders with the required entity roles.
    /// </summary>
    private async Task<List<WorkflowApproverModel>> GetOpportunityApproversAsync(
        int opportunityId, 
        List<string> roleNames,
        string toStage)
    {
        // For GO transition, use DoA Level 2 holders from ResponsibleOrgUnit
        if (toStage == OpportunityWorkflow.Stages.Go)
        {
            return await GetDoA2HoldersForOpportunityAsync(opportunityId, toStage);
        }

        // For other transitions, use stakeholder-based lookup
        return await GetStakeholderApproversAsync(opportunityId, roleNames, toStage);
    }

    /// <summary>
    /// Gets DoA Level 2 holders for an opportunity's ResponsibleOrgUnit.
    /// </summary>
    /// <param name="opportunityId">The opportunity ID.</param>
    /// <param name="toStage">The target workflow stage.</param>
    /// <returns>List of DoA Level 2 approvers.</returns>
    private async Task<List<WorkflowApproverModel>> GetDoA2HoldersForOpportunityAsync(
        int opportunityId,
        string toStage)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Get the opportunity's ResponsibleOrgUnitId
        var opportunity = await context.Set<Opportunity>()
            .AsNoTracking()
            .Where(o => o.Id == opportunityId && !o.IsDeleted)
            .Select(o => new { o.Id, o.ResponsibleOrgUnitId })
            .FirstOrDefaultAsync();

        if (opportunity == null)
        {
            _logger?.LogWarning("Opportunity {OpportunityId} not found for DoA2 lookup", opportunityId);
            return new List<WorkflowApproverModel>();
        }

        if (!opportunity.ResponsibleOrgUnitId.HasValue)
        {
            _logger?.LogWarning("Opportunity {OpportunityId} has no ResponsibleOrgUnitId set for DoA2 lookup", opportunityId);
            return new List<WorkflowApproverModel>();
        }

        return await GetDoA2HoldersForOrgUnitAsync(opportunity.ResponsibleOrgUnitId.Value, toStage);
    }

    /// <summary>
    /// Gets DoA Level 2 holders for a specific organization unit.
    /// Queries EntityUserRole for users with DoA2_OrganizationHierarchy role on the org unit.
    /// </summary>
    /// <param name="orgUnitId">The organization unit ID.</param>
    /// <param name="toStage">The target workflow stage.</param>
    /// <returns>List of DoA Level 2 approvers.</returns>
    private async Task<List<WorkflowApproverModel>> GetDoA2HoldersForOrgUnitAsync(
        int orgUnitId,
        string toStage)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var doaHolders = await context.Set<EntityUserRole>()
            .AsNoTracking()
            .Include(e => e.EntityRole)
            .Include(e => e.User)
                .ThenInclude(u => u!.UserProfile)
            .Where(e => !e.IsDeleted &&
                       e.EntityType == "OrganizationHierarchy" &&
                       e.EntityId == orgUnitId &&
                       e.EntityRole != null &&
                       e.EntityRole.Code == DoA2RoleCode)
            .ToListAsync();

        if (!doaHolders.Any())
        {
            _logger?.LogWarning(
                "No DoA Level 2 holders found for OrganizationHierarchy {OrgUnitId}", 
                orgUnitId);
        }

        return doaHolders
            .Where(e => e.User != null)
            .Select(e => new WorkflowApproverModel
            {
                UserId = e.UserId,
                FirstName = e.User!.UserProfile?.Name?.Split(' ').FirstOrDefault() ?? string.Empty,
                LastName = e.User!.UserProfile?.Name?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                Name = e.User!.UserProfile?.Name ?? e.User.Email,
                Email = e.User!.Email ?? string.Empty,
                Role = "DoA Level 2",
                ToStage = toStage
            })
            .ToList();
    }

    /// <summary>
    /// Gets approvers for an Opportunity based on stakeholders with the required roles.
    /// Used for non-GO transitions.
    /// </summary>
    private async Task<List<WorkflowApproverModel>> GetStakeholderApproversAsync(
        int opportunityId, 
        List<string> roleNames,
        string toStage)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Get stakeholders with the required entity roles
        var stakeholders = await context.Set<OpportunityStakeholder>()
            .AsNoTracking()
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
    /// For GO transition: Returns DoA Level 2 holders from the opportunity's ResponsibleOrgUnit.
    /// For other transitions: Returns stakeholders with the required entity roles.
    /// </summary>
    private async Task<List<WorkflowTaskModel>> GetOpportunityApproverTasksAsync(
        int opportunityId, 
        List<string> roleNames,
        string toStage)
    {
        // For GO transition, use DoA Level 2 holders from ResponsibleOrgUnit
        if (toStage == OpportunityWorkflow.Stages.Go)
        {
            return await GetDoA2HolderTasksForOpportunityAsync(opportunityId);
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // For other transitions, use stakeholder-based lookup
        var stakeholders = await context.Set<OpportunityStakeholder>()
            .AsNoTracking()
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
    /// Gets DoA Level 2 holder tasks for an opportunity's ResponsibleOrgUnit.
    /// </summary>
    private async Task<List<WorkflowTaskModel>> GetDoA2HolderTasksForOpportunityAsync(int opportunityId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        // Get the opportunity's ResponsibleOrgUnitId
        var opportunity = await context.Set<Opportunity>()
            .AsNoTracking()
            .Where(o => o.Id == opportunityId && !o.IsDeleted)
            .Select(o => new { o.Id, o.ResponsibleOrgUnitId })
            .FirstOrDefaultAsync();

        if (opportunity?.ResponsibleOrgUnitId == null)
        {
            return new List<WorkflowTaskModel>();
        }

        var doaHolders = await context.Set<EntityUserRole>()
            .AsNoTracking()
            .Include(e => e.EntityRole)
            .Where(e => !e.IsDeleted &&
                       e.EntityType == "OrganizationHierarchy" &&
                       e.EntityId == opportunity.ResponsibleOrgUnitId.Value &&
                       e.EntityRole != null &&
                       e.EntityRole.Code == DoA2RoleCode)
            .ToListAsync();

        return doaHolders
            .Select(e => new WorkflowTaskModel
            {
                UserId = e.UserId,
                Role = "DoA Level 2"
            })
            .ToList();
    }

    /// <summary>
    /// Gets trigger tasks for an Opportunity.
    /// Includes both:
    /// - Stakeholders with trigger roles (e.g., Opportunity Manager)
    /// - Collaborators (Opportunity Development Team members who have edit permissions)
    /// </summary>
    private async Task<List<WorkflowTaskModel>> GetOpportunityTriggerTasksAsync(
        int opportunityId, 
        List<string> roleNames)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var tasks = new List<WorkflowTaskModel>();
        
        // 1. Get stakeholders with trigger roles (e.g., Opportunity Manager)
        var stakeholders = await context.Set<OpportunityStakeholder>()
            .AsNoTracking()
            .Include(s => s.EntityRole)
            .Where(s => s.OpportunityId == opportunityId &&
                       !s.IsDeleted &&
                       s.UserId.HasValue &&
                       s.EntityRole != null &&
                       roleNames.Contains(s.EntityRole.Name))
            .ToListAsync();

        tasks.AddRange(stakeholders
            .Select(s => new WorkflowTaskModel
            {
                UserId = s.UserId!.Value,
                Role = s.EntityRole!.Name
            }));
        
        // 2. Get Collaborators (Opportunity Development Team members)
        // Collaborators have edit permissions and should be able to trigger workflows
        var collaborators = await context.Set<OpportunityCollaborator>()
            .AsNoTracking()
            .Where(c => c.OpportunityId == opportunityId && !c.IsDeleted)
            .ToListAsync();

        tasks.AddRange(collaborators
            .Select(c => new WorkflowTaskModel
            {
                UserId = c.UserId,
                Role = "Collaborator"
            }));
        
        // Remove duplicates (in case a user is both a stakeholder with trigger role and a collaborator)
        return tasks
            .GroupBy(t => t.UserId)
            .Select(g => g.First())
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
