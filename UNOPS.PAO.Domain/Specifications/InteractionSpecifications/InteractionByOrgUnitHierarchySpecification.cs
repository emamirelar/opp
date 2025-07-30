namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Specification to filter interactions by organizational unit hierarchy through OrganizationUnitRelationships
/// Uses manual joins to efficiently filter at the database level without navigation properties
/// </summary>
public class InteractionByOrgUnitHierarchySpecification : BaseSpecification<Interaction>
{
    private readonly List<int> _orgUnitHierarchyIds;

    public InteractionByOrgUnitHierarchySpecification(List<int> orgUnitHierarchyIds)
        : base(BuildCriteria(orgUnitHierarchyIds))
    {
        _orgUnitHierarchyIds = orgUnitHierarchyIds ?? new List<int>();
        // Remove the include since we'll handle this manually
    }

    private static Expression<Func<Interaction, bool>> BuildCriteria(List<int> orgUnitHierarchyIds)
    {
        if (orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0)
        {
            // If no org units specified, return no results for security
            return i => false;
        }

        // Basic filtering - the actual org unit filtering will be done via manual join
        return i => true;
    }
    
    /// <summary>
    /// Apply manual join filtering to the query for efficient database-level filtering
    /// This should be called by the repository/manager when applying the specification
    /// </summary>
    public IQueryable<Interaction> ApplyOrgUnitFilter(IQueryable<Interaction> query, DbContext context)
    {
        if (_orgUnitHierarchyIds == null || _orgUnitHierarchyIds.Count == 0)
        {
            return query.Where(i => false); // No results for security
        }

        // Pre-materialize the interaction IDs that match the org unit criteria to avoid nested query issues
        var validInteractionIds = context.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Interaction" && 
                _orgUnitHierarchyIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToList(); // Materialize the IDs first

        // Now filter the interactions using the materialized IDs
        return query.Where(interaction => validInteractionIds.Contains(interaction.Id));
    }
}